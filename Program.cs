using System.Text;
using FinanceDashboardSystem.DbContext;
using FinanceDashboardSystem.Middleware;
using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Repositories.CategoryRepo;
using FinanceDashboardSystem.Repositories.TransactionRepo;
using FinanceDashboardSystem.Repositories.UserRepo;
using FinanceDashboardSystem.Services;
using FinanceDashboardSystem.Services.DashboardService;
using FinanceDashboardSystem.Services.OtpService;
using FinanceDashboardSystem.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FinanceDashboardSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Database ───────────────────────────────────────────────
        builder.Services.AddDbContext<FinanceDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        // ── ASP.NET Core Identity ──────────────────────────────────
        builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            // OTP-based auth — relax password policy (password is never user-facing)
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<FinanceDbContext>()
        .AddDefaultTokenProviders();

        // ── JWT Bearer Auth ────────────────────────────────────────
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSettings["Key"];

        if (string.IsNullOrEmpty(jwtKey))
            throw new Exception("JWT Key is missing in configuration.");

        var key = Encoding.UTF8.GetBytes(jwtKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        builder.Services.AddAuthorization();

        // ── Repositories ───────────────────────────────────────────
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

        // ── Services ───────────────────────────────────────────────
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IOtpService, OtpService>();

        builder.Services.AddMemoryCache();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // ── Swagger with JWT support ───────────────────────────────
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Finance Dashboard API",
                Version = "v1",
                Description = "Role-based financial records management API"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token (without 'Bearer ' prefix)."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // ── Build ──────────────────────────────────────────────────
        var app = builder.Build();

        // Global exception handler (must be first in pipeline)
        app.UseMiddleware<GlobalExceptionMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}