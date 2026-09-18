# ChurchPortal

A church reporting / decision-support system. It replaces manual attendance,
home-fellowship, and inventory tracking with a role-based web system that
validates data, enforces an approval workflow, keeps an audit trail, and
(eventually) turns raw numbers into reports leadership can act on.

This document explains **what the project is, how it's built, and how to run
and test it** - read it top to bottom once, then use it as a reference.

---

## 1. What problem is this solving?

A church tracks three streams of data:

- **Attendance** - how many people came to each service (Sunday, Wednesday,
  special), broken down by men/women/children, converts, first timers.
- **Home Fellowship** - small group meetings across multiple centers/zones,
  each with a leader, tracked the same way as attendance.
- **Inventory** - physical church assets (sound equipment, furniture, media
  gear), their condition, value, and who's responsible for them.

Right now that's manual (spreadsheets, paper). The goal is a system where:

1. Different people log in with different **roles**, and only see/do what
   their role allows.
2. Data they enter is **validated automatically** (totals can't be typed in
   wrong - they're calculated by the server).
3. An **Admin approves** submitted records, after which they get **locked**
   (can't be edited/manipulated later).
4. Every change is **logged** (who did what, when).
5. Eventually: dashboards and reports turn all this into answers to
   questions like *"are we growing?"*, *"which fellowship centers are
   declining?"*, *"what assets need replacing?"*

That whole design came from a leadership blueprint document, not from a
generic "CRUD app" template - every technical decision below (roles, approval
workflow, audit log, locked records) traces back to a specific requirement in
that blueprint.

---

## 2. Tech stack

| Layer | Choice |
|---|---|
| Backend | ASP.NET Core 8 Web API (C#) |
| Database | PostgreSQL, via Entity Framework Core (Npgsql provider) |
| Auth | JWT bearer tokens, passwords hashed with BCrypt |
| API docs / dev testing | Swagger UI (auto-generated) |
| Frontend (final) | Plain HTML / CSS / JS (no framework) - still placeholder shells right now |
| Frontend (dev testing) | A separate "test console" page - see §7 |

The backend architecture deliberately mirrors a known-good pattern from a
sibling example project (`../E-commerce`), so if you've looked at that
project before, this one will look structurally identical - same layering,
same naming conventions, same request/response shape.

---

## 3. Architecture - the layers, and why they exist

Every feature (Attendance, InventoryItem, etc.) flows through the same four
layers. This is **not extra complexity for no reason** - each layer has one
job, which is exactly the kind of thing you want to be able to explain in a
defense:

```
HTTP request
    ↓
Controller       - reads the URL/route, checks [Authorize(Roles="...")],
                    calls a Service, returns an HTTP response (200/400/404)
    ↓
Service           - the actual business logic: validates input, computes
                    totals, checks for duplicates, writes audit logs,
                    enforces "is this record locked?" rules
    ↓
Repository         - talks to the database via Entity Framework Core.
                    Knows nothing about business rules, just CRUD + queries.
    ↓
UnitOfWork / DbContext - the EF Core object that actually executes SQL
                    against PostgreSQL and commits the transaction
```

**Why split it this way instead of one big Controller doing everything?**
Because a Controller shouldn't know *how* data is stored, and a Repository
shouldn't know *whether* a user is allowed to approve a record. Each layer
can be tested, reasoned about, and changed independently. This is the
standard "layered architecture" / "repository pattern" you'll see in any
real ASP.NET Core system

### Folder structure

```
ChurchPortal/
├── Controllers/            → one controller per entity, HTTP endpoints
├── Services/
│   ├── Interfaces/          → contracts (IServiceService, etc.) + Validator.cs
│   └── Implementations/     → business logic, one class per entity
├── Repositories/
│   ├── Interfaces/          → contracts (IServiceRepository, etc.)
│   └── Implementattions/    → EF Core queries, one class per entity
│                              (yes, "Implementattions" - a typo carried over
│                               from the example project, kept for consistency)
├── Core/
│   ├── Entities/             → the actual database tables, as C# classes
│   └── Dtos/                 → shapes sent over the API (never expose
│                                entities directly - DTOs are the contract)
├── DataContext/              → ChurchPortalDbContext.cs - EF Core's map of
│                                the database + seed data (roles, admin user)
├── AuthService/               → JWT token generation + password verification
├── Migrations/                → EF Core's generated "create these tables" scripts
├── Front-End/
│   ├── HTML/CSS/JS            → the (currently placeholder) real frontend
├── Program.cs                 → wires everything together (DI, auth, CORS, Swagger)
└── appsettings.json            → configuration (connection string, JWT settings)
```

---

## 4. The domain model (what data actually looks like)

Every entity below extends a shared `BaseEntity`:
```csharp
public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsDeleted { get; set; }          // soft-delete flag
    public DateTime CreatedDate { get; set; }
}
```
Deletes are **soft deletes** - a row is marked `IsDeleted = true`, never
actually removed. This preserves history/audit trail.

### Service
One church service (a specific Sunday, Wednesday, or special event).
```
Date, Day, ServiceType (Sunday/Wednesday/Special),
Theme, ScriptureText, Preacher, OnlineAttendance
```
Rule enforced in code: **you cannot create two Services with the same
Date + ServiceType** (checked in `ServiceService.CreateAsync`).

### Attendance
Attendance numbers for one Service.
```
ServiceId (→ Service), Men, Women, Children, SundaySchool,
NewConverts, FirstTimers, Total (SERVER-CALCULATED, never trust client input),
IsApproved, IsLocked, ApprovedByUserId, ApprovedDate
```
`Total` is computed server-side as `Men + Women + Children` - it's not even
accepted as user input on create. This directly implements the blueprint's
"totals must auto-calculate, no manual entry" requirement, and it's also a
basic security principle: **never trust a total the client sends you**,
always recompute it.

### FellowshipCenter
A home fellowship group.
```
CenterName, Zone, LeaderName, Location
```

### FellowshipAttendance
Same idea as Attendance, but for a FellowshipCenter meeting instead of a
Service.
```
FellowshipCenterId (→ FellowshipCenter), Date, Men, Women, Children,
NewConverts, Total (server-calculated), IsApproved, IsLocked,
ApprovedByUserId, ApprovedDate
```

### InventoryItem
A physical church asset.
```
ItemName, Category, Quantity, Location,
Condition (Good/NeedsRepair/Replace), PurchaseDate, Value,
Custodian, LastVerifiedDate
```

### AuditLog
Every Create/Update/Delete/Approve action on the entities above writes one
of these:
```
EntityName, EntityId, Action, UserId, Timestamp, Details
```
This is the system's answer to "who edited what and when?" - a requirement
straight from the blueprint's governance section.

### User / Role / UserRole
Standard many-to-many: a `User` can have multiple `Role`s, joined through
`UserRole`. Five roles are seeded automatically:

| Role | Can do |
|---|---|
| **Admin** | Everything, including approving records |
| **AttendanceOfficer** | Create/edit Service + Attendance |
| **FellowshipLeader** | Create/edit FellowshipCenter + FellowshipAttendance |
| **InventoryOfficer** | Create/edit InventoryItem |
| **Viewer** | Read-only, everywhere |

Everyone (any logged-in user) can **read** (`GET`) everything - role
restrictions only apply to write operations (`POST`/`PUT`/`DELETE`), which
matches "Viewer sees reports only" from the blueprint's access table.

---

## 5. Authentication & Authorization - how login actually works

1. `POST /api/User/login` with `{ email, password }`
2. `UserService` looks up the user, verifies the password against the
   stored **BCrypt hash** (`BCrypt.Net.BCrypt.Verify(...)`)
3. If valid, `AuthService.GenerateToken(...)` builds a **JWT** - a signed
   token containing the user's Id, Email, and Role(s) as "claims"
4. The token is returned to the client, which stores it (in this project:
   `localStorage`) and sends it back on every future request as:
   ```
   Authorization: Bearer <token>
   ```
5. Each Controller has `[Authorize]` (must be logged in) or
   `[Authorize(Roles = "Admin,AttendanceOfficer")]` (must be logged in
   **and** have one of those roles) on individual actions. ASP.NET Core
   reads the role claim out of the token automatically - no manual role
   checking code needed in the Controller body.

**Why JWT instead of, say, sessions/cookies?** JWT is stateless - the
server doesn't need to remember who's logged in, the token itself proves
it (it's cryptographically signed with a secret key in `appsettings.json`,
so it can't be forged without that key). This is the standard approach for
an API that's meant to be called from a separate frontend (or eventually a
mobile app).

**Password security:** passwords are never stored in plain text. BCrypt is
a hashing algorithm specifically designed to be slow (to resist brute-force
attacks) and includes a random "salt" automatically, so two users with the
same password get different hashes in the database.

---

## 6. The approval workflow (why records have `IsApproved` / `IsLocked`)

This directly implements the blueprint's Step 5 ("Data Entry Controls - to
avoid manipulation") and Step 6 (governance):

1. An Officer (e.g. AttendanceOfficer) creates a record → `IsApproved =
   false`, `IsLocked = false`
2. Admin reviews it and calls `POST /api/Attendance/{id}/approve`
   → sets `IsApproved = true`, `ApprovedByUserId`, `ApprovedDate`, and
   **writes an AuditLog entry**
3. Once a record `IsLocked = true` (a manual step, not automatic - locking
   is a separate concern from approving), the `UpdateAsync` method
   **rejects any further edits** - this is what "lock previous months
   after approval" means in practice.

This is the part of the system that makes it a "decision-making system"
rather than "just a form" - it's what stops someone from quietly editing
last month's attendance numbers after the fact.

---

## 7. How to run and test this project

There are **three separate things** here - don't confuse them:

| Tool | What it's for | Who uses it |
|---|---|---|
| **The API itself** (`dotnet run`) | The actual backend server | Nobody directly - it's the engine |
| **Swagger UI** (auto-opens at `/swagger`) | Poke any endpoint manually, see request/response shapes | You, while developing |
| **Test Console** (`Front-End/HTML/test-console.html`) | A simple test UI with real forms/buttons for every module | You, for a more visual test than Swagger |
| **The real frontend** (`login.html`, `admin.html`, etc.) | What an actual church user would eventually use | Not wired up to the API yet - still placeholder |

### Step 1 - Start the database
Make sure PostgreSQL is running locally. Connection details are in
`appsettings.json` (placeholder password there - the real one lives in
**User Secrets**, not committed to git, so it survives a fresh clone
without leaking your password).

### Step 2 - Run the API
In Visual Studio 2026: open `ChurchPortal.sln`, press **F5**.
From the command line:
```
cd ChurchPortal
dotnet run
```
Watch the console output - it prints which URLs it's listening on, e.g.:
```
Now listening on: https://localhost:7080
Now listening on: http://localhost:5080
```

### Step 3 - Test it
**Do not open `test-console.html` by double-clicking it** (i.e. as a
`file://` URL) - some browsers restrict what JavaScript can do on local
files, and it can fail silently. Instead serve it over a real (even
tiny) local web server, for example:
```
cd Front-End/HTML
python -m http.server 8000
```
then open `http://localhost:8000/test-console.html` in your browser.

In the test console:
1. Set the **API base URL** field to match whatever port your API printed
   (`http://localhost:5080` by default)
2. Log in with the seeded admin account:
   `admin@churchportal.local` / `Admin@123`
3. Use each tab to create/view/approve/delete records - every request and
   response is shown in the log panel at the bottom, which is the fastest
   way to actually see what the API is doing

### Seeded login credentials (by role)

All users are created by `UserSeeder` on startup (idempotent). Roles are
enforced by `[Authorize(Roles = ...)]` on the API controllers, and the
frontend hides actions you don't have permission for.

```
Role: Admin (full access, including approvals)
Email:    admin@churchportal.local
Password: Admin@123

Role: AttendanceOfficer (manages Services and Attendance)
Email:    attendance.officer@churchportal.local
Password: Attendance@123

Role: FellowshipLeader (manages Fellowship Centers and Fellowship Attendance)
Email:    fellowship.leader@churchportal.local
Password: Fellowship@123

Role: InventoryOfficer (manages Inventory Items)
Email:    inventory.officer@churchportal.local
Password: Inventory@123

Role: Viewer (read-only)
Email:    viewer@churchportal.local
Password: Viewer@123
```

---

## 8. A real bug we hit (worth knowing for your defense)

When the project was switched from MySQL to PostgreSQL, `Create` calls for
Service/FellowshipAttendance/InventoryItem started failing with:
```
Cannot write DateTime with Kind=Unspecified to PostgreSQL type
'timestamp with time zone', only UTC is supported.
```
**Why:** PostgreSQL's `timestamptz` columns require a `DateTime` value
explicitly tagged as UTC. A date typed into an HTML `<input type="date">`
(or parsed without a timezone) comes into C# as `DateTimeKind.Unspecified`
- MySQL didn't care, PostgreSQL does.

**Fix:** a shared helper, `Validator.AsUtc(DateTime)`, is applied at every
point a raw date from a request DTO touches an entity or gets used in a
database query:
```csharp
public static DateTime AsUtc(DateTime value) => value.Kind switch
{
    DateTimeKind.Utc => value,
    DateTimeKind.Local => value.ToUniversalTime(),
    _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
};
```
This is a good example of a **database-provider-specific gotcha** that
only shows up when you actually run the thing end-to-end - which is
exactly why the test console (and testing in general) matters, not just
getting the code to compile.

---

## 9. What's NOT built yet

Be upfront about this if asked in a defense - it's deliberate scoping, not
an oversight:

- **Report/analytics endpoints** - monthly growth %, demographic
  breakdowns, center rankings, expansion alerts, asset valuation reports.
  This is the actual "turn data into decisions" part of the blueprint and
  is the next major piece of work.
- **Real data-entry frontend** - the role dashboards
  (`admin.html`, `attendance-officer.html`, etc.) are empty shells with
  just a logout button. They don't call the API yet.
- **Admin approval UI** - the backend `Approve` endpoints exist and work,
  but there's no screen for an Admin to browse pending records and click
  approve (currently only testable via the test console/Swagger).
- **Deployment hardening** - the JWT signing key is still a hardcoded
  placeholder in `appsettings.json`; before any real deployment it needs
  to move to an environment variable or secret store, same as the DB
  password already does locally via User Secrets.

---


