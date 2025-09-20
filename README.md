# CarDealerShip API

A minimal ASP.NET Core Web API for a car dealership with:

* JWT authentication + role-based authorization (Admin/Customer)
* OTP verification for Register, Login, Update Vehicle, and Purchase
* EF Core with seeded data (10 cars + Admin)

---

## 1) How to run

### Prereqs

* .NET 8 SDK
* SQL Server LocalDB (ships with Visual Studio)
  *or* switch to SQLite (see **Database** below)
* Swagger: auto-enabled at runtime

### Install dependencies

```bash
dotnet restore
```

### Database

The project is configured for **SQL Server LocalDB** by default:

`appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(LocalDb)\\MSSQLLocalDB;Database=CarDealerShipDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
},
"Jwt": {
  "Key": "supersecret_dev_key_change_in_prod"
}
```

> Prefer Docker-friendly? Use SQLite instead:
>
> ```json
> "ConnectionStrings": { "DefaultConnection": "Data Source=dealership.db" }
> ```
>
> and in `Program.cs` switch to `UseSqlite(...)`.

### Create/Update DB

```bash
dotnet ef database update
```

### Run

```bash
dotnet run
```

Open Swagger at:

```
https://localhost:7117/swagger
```

### Seeded accounts

* **Admin**: `admin@local / Admin#12345`
* OTP codes are printed to your **server console** after calling `POST /api/AppUsers/otp/start`.

---

## 2) Available endpoints & usage

### Conventions

* **Auth**: Click **Authorize** in Swagger and paste `Bearer <token>`.
* **OTP**: Generate via `POST /api/AppUsers/otp/start` and read the 6-digit code from the server console.
* **OTP Purposes** (must match exactly):

  * `register:<email>`
  * `login:<email>`
  * `updateVehicle:<carId>`
  * `purchase:<userId>:<carId>`

---

### AppUsers

#### POST `/api/AppUsers/otp/start`  *(public)*

Start OTP for a purpose.

```json
{ "purpose": "register:alice@example.com" }
```

#### POST `/api/AppUsers/register`  *(public + OTP)*

Registers a Customer and returns a JWT.

```json
{ "email": "alice@example.com", "password": "P@ssw0rd!", "otp": "123456" }
```

Response:

```json
{ "token": "<JWT>", "id": "...", "email": "alice@example.com", "role": "Customer" }
```

#### POST `/api/AppUsers/login`  *(public + OTP)*

Login and get a JWT.

```json
{ "email": "alice@example.com", "password": "P@ssw0rd!", "otp": "123456" }
```

#### GET `/api/AppUsers`  *(open)*

List users. Optional filter: `?role=Admin` or `?role=Customer`.

#### GET `/api/AppUsers/{id}`  *(open)*

Get a user.

#### PUT `/api/AppUsers/{id}`  *(open in demo)*

Update role/password (demo only; no hashing in this version).

#### DELETE `/api/AppUsers/{id}`  *(open in demo)*

Delete a user.

---

### Cars

#### GET `/api/Cars`  *(public)*

List cars. Optional: `?year=2021&make=Toyota`.

#### GET `/api/Cars/{id}`  *(public)*

Get a car.

#### POST `/api/Cars`  *(Admin)*

Create a car.

```json
{ "make":"Mazda", "model":"3", "year": 2021, "price": 17000, "isAvailable": true }
```

#### PUT `/api/Cars/{id}`  *(Admin + OTP)*

Update a car.
Headers:

```
X-OTP-Purpose: updateVehicle:<carId>
X-OTP-Code: 123456
```

Body: full car object including `id`.

#### DELETE `/api/Cars/{id}`  *(Admin)*

Delete a car.

---

### Purchases

#### GET `/api/Purchases`  *(Admin)*

All purchases.

#### GET `/api/Purchases/{id}`  *(Admin)*

Purchase by id.

#### GET `/api/Purchases/by-customer/{customerId}`  *(Admin)*

Purchases for a given user.

#### GET `/api/Purchases/me`  *(Customer)*

Purchases for the authenticated customer.

#### POST `/api/Purchases`  *(Customer + OTP)*

Create a purchase.
First start OTP:

```json
{ "purpose": "purchase:<yourUserIdFromToken>:<carGuid>" }
```

Then call:

* Header: `X-OTP-Code: 123456`
* Body:

```json
{ "carId": "<carGuid>" }
```

Result: `201 Created`, and the car becomes `isAvailable=false`.

---

## 3) Assumptions & design decisions

* **Security (demo scope)**

  * Passwords are stored as plain text for simplicity. For production, replace with **ASP.NET Identity** (hashing, lockout, password policies).
  * JWT auth is configured with a single symmetric key from `appsettings.json`. Rotate & secure via secrets manager in real deployments.
  * OTPs are stored **in-memory** (per-process) and logged to console for demo. Replace with a persistent store + email/SMS provider for real use.

* **Authorization**

  * **Admin** can CRUD cars and view all purchases.
  * **Customer** can browse cars, purchase with OTP, and view own purchases.
  * Public endpoints are intentionally open for demo (read-only), but can be restricted with `[Authorize]` if needed.

* **OTP gating**

  * Required for **Register**, **Login**, **Update Vehicle (Admin)**, and **Purchase (Customer)** to match the challenge requirements.
  * The exact **purpose string** must match at generation and validation time.

* **Database**

  * EF Core + migrations.
  * Seed data: **10 cars**, **1 admin**.
  * `decimal(18,2)` for price; fixed GUIDs for seeded cars to keep migrations stable.
  * `PurchaseDate` is set by DB default (`GETUTCDATE()` for SQL Server). Use `CURRENT_TIMESTAMP` if on SQLite.

* **Tech stack**

  * .NET 8, ASP.NET Core Web API
  * EF Core (SQL Server LocalDB by default; SQLite is a drop-in option)
  * Swagger (OpenAPI)
  * JWT Bearer Auth

* **Simplicity over completeness**

  * No soft deletes, refunds, or inventory reservations.
  * Purchases are append-only; deleting cars after purchase is allowed in this demo but typically you’d restrict it.

---

## Demo “happy path”

**Customer**

1. Start OTP `register:<email>` → Register → Authorize with returned token
2. Browse cars → Start OTP `purchase:<userId>:<carId>` → Purchase → `GET /purchases/me`

**Admin**

1. Start OTP `login:admin@local` → Login → Authorize
2. `POST /cars` (create) → Start OTP `updateVehicle:<carId>` → `PUT /cars/{id}` (update)
3. `GET /purchases` / `by-customer/{customerId}`

---

## Troubleshooting

* **401 Unauthorized**: you didn’t click **Authorize** with a valid token, or the token expired.
* **403 Forbidden**: wrong role for the endpoint.
* **400 OTP invalid or expired**: regenerate with correct purpose; use within 3 minutes; pass headers.
* **Car not available**: someone already purchased it; pick another car.
* **Email already registered**: login instead or register a different email.

---


