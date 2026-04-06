# Finance Dashboard System (Backend API) — Project Documentation

**Project**: FinanceDashboardSystemSolution  
**Type**: ASP.NET Core 8 Web API  
**Purpose**: Role-based financial records management (transactions, categories, dashboards) with phone-based OTP login and JWT authorization.

---

## 1) Quick start

### Prerequisites
- .NET SDK 8.x
- SQL Server (local install or container)

### Configuration
- **Connection string**: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- **JWT settings**: `appsettings.json` → `Jwt:{ Key, Issuer, Audience, ExpiryDays }`

### Run (local)

```bash
dotnet ef database update --project FinanceDashboardSystem.csproj
dotnet run --project FinanceDashboardSystem.csproj
```

Swagger UI is available in development mode (see `Program.cs`).

---

## 2) High-level architecture

This project follows a straightforward layered design:

- **Controllers**: HTTP endpoints + authorization attributes.
- **Services**: business logic and orchestration (dashboard aggregation, transaction rules).
- **Repositories**: data-access wrappers for EF Core queries and persistence.
- **DbContext**: EF Core model configuration + Identity integration.
- **DTOs**: request/response payload types.
- **Models**: domain entities (`User`, `Transaction`, `Category`) and enums.
- **Middleware**: centralized exception handling (`GlobalExceptionMiddleware`).

### Dependency flow

`Controller` → `Service` → `Repository` → `FinanceDbContext` → SQL Server  
`Controller` → `IOtpService` (OTP generation/validation)  

---

## 3) Project structure

```text
FinanceDashboardSystemSolution/
├── Controllers/
│   ├── AuthController.cs
│   ├── ViewerController.cs
│   ├── AnalystController.cs
│   └── AdminController.cs
├── DbContext/
│   └── FinanceDbContext.cs
├── DTOs/
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Migrations/
├── Models/
│   ├── User.cs
│   ├── Transaction.cs
│   └── Category.cs
├── Repositories/
│   ├── UserRepo/
│   ├── TransactionRepo/
│   └── CategoryRepo/
├── Services/
│   ├── UserService/
│   ├── TransactionService/
│   ├── DashboardService/
│   └── OtpService/
├── Program.cs
├── appsettings.json
└── FinanceDashboardSystem.csproj
```

---

## 4) Runtime pipeline & configuration

### Application startup (`Program.cs`)
Key registrations:
- **EF Core**: SQL Server provider using `DefaultConnection`
- **Identity**: `User` + `IdentityRole` with relaxed password policy (OTP-style login)
- **JWT Bearer auth**: validates issuer/audience/signature and role claims
- **DI**: repositories + services added as scoped dependencies
- **Swagger/OpenAPI**: enabled in development with JWT security scheme
- **Global exceptions**: `GlobalExceptionMiddleware` is first in the pipeline

### Important config keys (`appsettings.json`)
- `ConnectionStrings:DefaultConnection`
- `Jwt:Key` (**must be kept secret**)
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiryDays`

---

## 5) Authentication & authorization

### Auth mechanism
Authentication is **OTP + JWT**:

1) **Send OTP**  
`POST /api/auth/send-otp`  
Request: phone number (+ optional role for first-time registration)  
Response: includes `referenceId`, `isExistingUser`, and OTP (OTP is returned for development/test).

2) **Verify OTP**  
`POST /api/auth/verify`  
On success returns **JWT** and the user role.

### Roles
Role enforcement is done via controller attributes:
- **ViewerController**: `[Authorize(Roles = "Viewer,Analyst,Admin")]`
- **AnalystController**: `[Authorize(Roles = "Analyst,Admin")]`
- **AdminController**: `[Authorize(Roles = "Admin")]`

**Roles** (from code): `Admin`, `Analyst`, `Viewer`

---

## 6) API endpoints (by controller)

### Auth (`/api/auth`)
- `POST /send-otp`: generate OTP + `referenceId`
- `POST /verify`: validate OTP, auto-create user if needed, return JWT

### Viewer (`/api/viewer`) — roles: Viewer, Analyst, Admin
- `GET /transactions`: own transactions (filters: `categoryId`, `type`)
- `GET /dashboard`: own dashboard summary

### Analyst (`/api/analyst`) — roles: Analyst, Admin
- `GET /transactions`: own transactions with date range support (filters: `categoryId`, `type`, `startDate`, `endDate`)
- `GET /insights`: extended dashboard summary for own data

### Admin (`/api/admin`) — role: Admin
- **Users**
  - `GET /users`: list all users
  - `POST /add-users`: create user (admin-only)
  - `PUT /users/{id}`: update user role / active status
  - `DELETE /users/{id}`: deactivate user
- **Transactions**
  - `GET /transactions`: list all transactions (all users) + filters
  - `POST /transactions`: create transaction (assigned to calling admin)
  - `PUT /transactions/{id}`: update transaction
  - `DELETE /transactions/{id}`: soft-delete transaction
- **Dashboard**
  - `GET /dashboard`: system-wide dashboard summary
- **Categories**
  - `GET /categories`
  - `POST /categories` (rejects duplicates by name)
  - `PUT /categories/{id}`
  - `DELETE /categories/{id}`

---

## 7) Data model (EF Core + Identity)

### Identity user
`FinanceDbContext` inherits `IdentityDbContext<User>`, so standard Identity tables exist (`AspNetUsers`, roles, claims, tokens, etc.).

### Domain entities
- **Category**
  - `Id` (int identity), `Name` (unique), `Description?`
- **Transaction**
  - `Id` (Guid), `Amount` (decimal(18,2)), `Type` (enum stored as int)
  - `CategoryId` → `Category.Id`
  - `UserId` (string) → `AspNetUsers.Id`
  - `IsDeleted` soft-delete flag (globally filtered out)
- **User**
  - Extends Identity user
  - Unique index on `(PhoneNumber, ReferenceId)`

### Notable EF Core rules
- **Soft delete**: global query filter on `Transaction` (`!IsDeleted`)
- **Delete behavior**: restrictive FK deletes to prevent accidental cascades
- **Uniqueness**: categories unique by name; users unique by phone+reference

---

## 8) Operational notes

### OTP storage
OTP is generated/validated by `IOtpService` and stored in memory (`IMemoryCache`).  
For multi-instance deployments, replace this with a shared store (e.g., Redis).

### Swagger authorization
Swagger is configured with a Bearer scheme; provide the JWT token when authorizing requests.

### Common troubleshooting
- **JWT Key missing**: app throws at startup if `Jwt:Key` is blank.
- **Database connection issues**: verify SQL Server is running and the `DefaultConnection` string is correct.
- **Unexpected “missing data”**: `Transaction.IsDeleted` is query-filtered globally; deleted rows won’t appear in EF queries.

