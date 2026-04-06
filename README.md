# Finance Dashboard System – Backend API

A role-based financial records management backend built with **ASP.NET Core 8**, **Entity Framework Core**, **ASP.NET Core Identity**, and **SQL Server**.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| ORM | Entity Framework Core 8 |
| Auth & Identity | ASP.NET Core Identity + JWT Bearer |
| Database | SQL Server (configurable via connection string) |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Caching | IMemoryCache (for OTP storage) |

---

## Project Structure

```
FinanceDashboardSystem/
├── Controllers/           # AdminController, AnalystController, ViewerController, AuthController
├── DbContext/             # FinanceDbContext (IdentityDbContext<User>)
├── DTOs/                  # Request / Response data transfer objects
├── Middleware/            # GlobalExceptionMiddleware
├── Migrations/            # EF Core migrations
├── Models/                # User, Transaction, Category
├── Repositories/          # UserRepo, TransactionRepo, CategoryRepo
└── Services/              # UserService, TransactionService, DashboardService, OtpService
```

---

## Database Schema (Normalized)

```
AspNetUsers (Identity)       Category
─────────────────────        ────────────────
Id (string PK)               Id (int PK, identity)
PhoneNumber (unique)         Name (unique, required)
ReferenceId                  Description?
FirstName / LastName
Role (enum: 1=Viewer, 2=Analyst, 3=Admin)
IsActive / CreatedAt
...Identity columns...

Transaction
──────────────────────────────────────────
Id (Guid PK)
Amount (decimal 18,2)
Type (int: 1=Income, 2=Expense)
CategoryId → Category.Id (FK, restricted)
Date
Notes?
IsDeleted (soft delete, query-filtered)
CreatedAt / UpdatedAt
UserId → AspNetUsers.Id (FK, restricted)
```

**Normalization decisions:**
- `Category` is a separate table (1NF/2NF) rather than a free-text field on `Transaction`.
- `User` extends `IdentityUser` so all authentication/security fields live in the standard Identity schema.
- `IsDeleted` query filter is registered globally — deleted records are invisible to all EF queries automatically.

---

## Setup & Running

### Prerequisites
- .NET 8 SDK
- SQL Server (local or Docker — see below)

### 1. Configure Connection String

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=FinanceDb;User Id=SA;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
}
```

**Docker SQL Server (quick start):**
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configure JWT Key

Edit `appsettings.json` — replace the placeholder key with a strong secret (≥32 chars):

```json
"Jwt": {
  "Key": "YOUR_STRONG_SECRET_KEY_AT_LEAST_32_CHARS",
  "Issuer": "FinanceDashboard",
  "Audience": "FinanceDashboard",
  "ExpiryDays": "1"
}
```

> ⚠️ **Never commit your real JWT key to source control.**

### 3. Apply Migrations

```bash
cd FinanceDashboardSystemSolution
dotnet ef database update --project FinanceDashboardSystem.csproj
```

### 4. Run the API

```bash
dotnet run --project FinanceDashboardSystem.csproj
```

Swagger UI opens at: **`http://localhost:5000`**

---

## Authentication Flow

This system uses **phone-based OTP authentication** (no passwords for end users).

```
POST /api/auth/send-otp     { phoneNumber, referenceId }
  → Generate 6-digit OTP (returned in response for dev; use SMS in production)

POST /api/auth/verify       { phoneNumber, referenceId, otp }
  → Returns JWT token + role
  → New users are auto-registered as Viewer
```

**Authorize in Swagger:** Click the 🔒 button, enter the JWT token (without `Bearer `).

---

## Roles & Access Control

| Endpoint Group | Viewer | Analyst | Admin |
|---|:---:|:---:|:---:|
| `GET /api/viewer/transactions` | ✅ | ✅ | ✅ |
| `GET /api/viewer/dashboard` | ✅ | ✅ | ✅ |
| `GET /api/analyst/transactions` (date filter) | ❌ | ✅ | ✅ |
| `GET /api/analyst/insights` | ❌ | ✅ | ✅ |
| `POST /api/admin/transactions` | ❌ | ❌ | ✅ |
| `PUT/DELETE /api/admin/transactions/{id}` | ❌ | ❌ | ✅ |
| `GET/POST/PUT/DELETE /api/admin/users` | ❌ | ❌ | ✅ |
| `GET/POST/PUT/DELETE /api/admin/categories` | ❌ | ❌ | ✅ |
| `GET /api/admin/dashboard` (all users) | ❌ | ❌ | ✅ |

Access control is enforced at the controller level via `[Authorize(Roles = "...")]`.

---

## API Reference

### Auth
| Method | Path | Description |
|---|---|---|
| POST | `/api/auth/send-otp` | Generate OTP |
| POST | `/api/auth/verify` | Verify OTP → JWT |

### Viewer (Roles: Viewer, Analyst, Admin)
| Method | Path | Query Params | Description |
|---|---|---|---|
| GET | `/api/viewer/transactions` | `categoryId`, `type` | Own transactions |
| GET | `/api/viewer/dashboard` | — | Own summary |

### Analyst (Roles: Analyst, Admin)
| Method | Path | Query Params | Description |
|---|---|---|---|
| GET | `/api/analyst/transactions` | `categoryId`, `type`, `startDate`, `endDate` | Filtered transactions |
| GET | `/api/analyst/insights` | — | Full dashboard breakdown |

### Admin (Role: Admin only)
| Method | Path | Description |
|---|---|---|
| GET | `/api/admin/users` | All users |
| POST | `/api/admin/users` | Create user |
| PUT | `/api/admin/users/{id}` | Update role / status |
| DELETE | `/api/admin/users/{id}` | Deactivate user |
| GET | `/api/admin/transactions` | All transactions (all users) |
| POST | `/api/admin/transactions` | Create transaction |
| PUT | `/api/admin/transactions/{id}` | Update any transaction |
| DELETE | `/api/admin/transactions/{id}` | Soft-delete transaction |
| GET | `/api/admin/dashboard` | System-wide summary |
| GET | `/api/admin/categories` | List categories |
| POST | `/api/admin/categories` | Create category |
| PUT | `/api/admin/categories/{id}` | Update category |
| DELETE | `/api/admin/categories/{id}` | Delete category |

---

## Dashboard Summary Response

```json
{
  "summary": { "totalIncome": 5000, "totalExpense": 1200, "netBalance": 3800 },
  "categoryBreakdown": [
    { "categoryId": 1, "category": "Salary", "income": 5000, "expense": 0, "net": 5000 }
  ],
  "recentActivity": [ ... ],
  "monthlyTrends": [
    { "period": "2026-03", "income": 5000, "expense": 1200, "net": 3800 }
  ],
  "weeklyTrends": [ { "week": 14, "income": 1000, "expense": 300 } ]
}
```

---

## Error Handling

All errors return a consistent JSON envelope:

```json
{ "status": 404, "message": "Transaction abc123 not found." }
```

| Exception | HTTP Status |
|---|---|
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 403 Forbidden |
| `InvalidOperationException` | 400 Bad Request |
| `ArgumentException` | 400 Bad Request |
| All others | 500 Internal Server Error |

---

## Assumptions & Design Decisions

1. **OTP in response (dev mode):** In production, the OTP should be delivered via an SMS gateway (Twilio etc.). It is returned in the API response here for testability.
2. **OTP expiry:** 5 minutes, stored in `IMemoryCache`. A distributed cache (Redis) would be needed for multi-instance deployments.
3. **Admin user bootstrap:** The first admin must be created directly in the database or via a seeding script, since the admin endpoint itself requires an admin JWT.
4. **Soft delete:** Deleted transactions are flagged `IsDeleted = true` and hidden by a global EF query filter. Hard deletion is not supported.
5. **Ownership on update/delete:** Transactions can only be updated/deleted by their owner (or an admin via the `/api/admin/` routes).
6. **Category normalization:** Categories must be pre-created by an admin before they can be referenced in transactions. This prevents orphan or typo category names.
7. **Password:** Identity requires a password even for OTP-only users. A random GUID-based password is generated and never exposed — the user always authenticates via OTP.
8. **BaseEntity.cs:** The existing `BaseEntity` class was retained in the codebase but `Transaction` does not inherit it — fields are declared directly to allow the `UserId` FK type override (string vs Guid).
