using FinanceDashboardSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboardSystem.DbContext;

public class FinanceDbContext : IdentityDbContext<User>
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Required for Identity tables

        // ── User ──────────────────────────────────────────────────
        modelBuilder.Entity<User>(u =>
        {
            u.HasIndex(x => new { x.PhoneNumber, x.ReferenceId }).IsUnique();
            u.Property(x => x.ReferenceId).IsRequired().HasMaxLength(100);
            u.Property(x => x.FirstName).HasMaxLength(100);
            u.Property(x => x.LastName).HasMaxLength(100);
        });

        // ── Category ───────────────────────────────────────────────
        modelBuilder.Entity<Category>(c =>
        {
            c.HasKey(x => x.Id);
            c.HasIndex(x => x.Name).IsUnique();
            c.Property(x => x.Name).IsRequired().HasMaxLength(100);
            c.Property(x => x.Description).HasMaxLength(255);
        });

        // ── Transaction ────────────────────────────────────────────
        modelBuilder.Entity<Transaction>(t =>
        {
            t.HasKey(x => x.Id);

            t.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            t.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            t.Property(x => x.Notes).HasMaxLength(500);

            // Soft-delete global query filter — auto-excludes deleted records
            t.HasQueryFilter(x => !x.IsDeleted);

            // FK → Category
            t.HasOne(x => x.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK → User (Identity string PK)
            t.HasOne(x => x.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}