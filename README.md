# IllRequestPortal

A lightweight intake portal for interlibrary loan (ILL) requests. It sits in front of existing library systems, captures request information, validates it against the local catalog, and stores requests for downstream ILL processing.

The system does **not** manage the ILL lifecycle. It handles:
- Capturing and storing the request
- Validating bibliographic information (ISBN/ISSN checksums)
- Checking whether the item already exists in the local Koha catalog
- Retrieving bibliographic metadata from Koha and Libris
- Looking up patron details by library card number

Designed to integrate with Koha, Libris, and discovery systems such as Primo.

---

## Table of Contents

1. [Project Structure](#project-structure)
2. [Architecture Overview](#architecture-overview)
3. [Getting Started (Local Development)](#getting-started-local-development)
4. [Organizational-Specific Customization](#organizational-specific-customization)
5. [Authentication](#authentication)
6. [Key Features](#key-features)
   - [Bibliographic Lookup](#bibliographic-lookup)
   - [Material Types](#material-types)
   - [Local Catalog Validation](#local-catalog-validation)
   - [Patron Lookup](#patron-lookup)
7. [API Endpoints](#api-endpoints)
8. [Configuration Reference](#configuration-reference)
9. [Database](#database)
10. [Localization](#localization)
11. [Logging](#logging)
12. [Administrative Interface](#administrative-interface)
13. [Publishing & Deployment](#publishing--deployment)

---

## Project Structure

```
ill-requests/
├── IllRequestPortal.Web/           # ASP.NET Core MVC web application
│   ├── ApiController/              # REST API endpoints (for JS + external consumers)
│   │   ├── BibliographicRecordsController.cs  # ISBN/ISSN lookup pipeline
│   │   ├── PatronApiController.cs             # Card number → patron lookup
│   │   └── IllRequestApiController.cs         # CRUD + status update
│   ├── Controller/                 # MVC controllers (Razor views)
│   │   ├── HomeController.cs       # Index, language toggle, error pages
│   │   ├── IllRequestController.cs # Create, list, edit, delete requests
│   │   └── LogController.cs        # Log viewer
│   ├── ViewModel/                  # View models (form data, display data)
│   ├── Views/                      # Razor views (.cshtml)
│   ├── wwwroot/                    # Static assets (CSS, JS, images)
│   │   ├── css/custom-site.css
│   │   └── js/custom-site.js
│   ├── Resources/                  # Localization (.resx files, sv/en)
│   ├── Migration/                  # SQL migration scripts
│   ├── Startup.cs                  # Base DI and middleware configuration
│   ├── StartupExtended.cs          # Org-specific overrides (copied from organizational-specific/)
│   └── appsettings.json            # Base config (copied from organizational-specific/)
│
├── IllRequestPortal.Logic/         # Business logic and data access
│   ├── DataAccess/                 # Dapper-based SQL data access
│   ├── Http/                       # HTTP client services (Koha, Libris)
│   ├── Model/                      # Domain models (IllRequest, Log, etc.)
│   ├── Service/                    # Business logic services
│   ├── Settings/                   # Configuration POCOs
│   └── Util/                       # Utilities (ISBN/ISSN validation)
│
├── IllRequestPortal.Test/          # Unit tests
│
└── organizational-specific/        # Organization-specific code and config
    └── web/                        # Files that override the base web project at build time
        ├── Controller/             # Org-specific controller overrides
        ├── StartupExtended.cs      # Org-specific DI and middleware
        ├── appsettings.json        # Org-specific base config
        ├── appsettings.Development.json
        ├── appsettings.Staging.json
        ├── appsettings.production.json
        └── Web.csproj              # Also overrides the project file
```

---

## Architecture Overview

```
Browser
  │
  ▼
IllRequestPortal.Web (ASP.NET Core MVC)
  │
  ├── MVC Views (Create form, Admin list)
  │     └── custom-site.js (dynamic field visibility, real-time lookups)
  │
  ├── REST API (called by JS + external integrations)
  │     ├── GET /api/v1/bibliographic-records/lookup?standardNumber=&queryField=
  │     ├── GET /api/v1/patrons?cardNumber=
  │     └── GET|POST|PUT|DELETE /api/v1/illrequests
  │
  └── IllRequestPortal.Logic
        ├── LibrisService → Libris API (libris.kb.se)
        ├── KohaPatronGetHttpService → Koha REST API (patrons)
        ├── KohaBiblioGetHttpService → Koha REST API (bibliographic records)
        └── IllRequestDataAccess → SQL Server (Dapper)
```

**Authentication** is handled by `Sh.Library.Authentication` (NuGet) which redirects unauthenticated users to a central AuthService. The patron-facing **Create** page is public (`[NoLibraryAuth]`). The **admin** pages require authentication.

---

## Getting Started (Local Development)

### Prerequisites

- .NET 8 SDK
- SQL Server (local instance)
- Access to the NuGet feed: `https://shbibl.pkgs.visualstudio.com/_packaging/LibraryAuth/nuget/v3/index.json`

### Setup

1. **Clone the repository.**

2. **Create a local database** named e.g. `dev-IllRequests`.

3. **Run the migration script** against the new database:
   ```
   IllRequestPortal.Web/Migration/Migrations.sql
   ```

4. **Create a local appsettings file.** The repository contains `appsettings.template.json` as a starting point. Copy it and fill in your local values:
   ```bash
   cp IllRequestPortal.Web/appsettings.template.json IllRequestPortal.Web/appsettings.Development.json
   ```
   Minimum required settings:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=localhost\\sqlexpress;Database=dev-IllRequests;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Authentication": {
       "Host": "https://localhost:44351",
       "BearerToken": "any-guid-for-local-dev"
     },
     "KohaApiSettings": {
       "BaseUrl": "https://your-koha.example.org/api/v1",
       "AuthenticationHeaderValue": "username:password"
     },
     "LibrisApiSettings": {
       "BaseUrl": "https://libris.kb.se"
     },
     "DiscoverySettings": {
       "RecordUrlTemplate": ""
     }
   }
   ```

5. **Restore packages:**
   ```
   dotnet restore --no-cache
   ```

6. **Run tests:** `Ctrl+R, A` in Visual Studio (or `dotnet test`)

7. **Start the application:** `F5` in Visual Studio

> **Note on the organizational-specific folder:** When you build, files from `organizational-specific/web/` are automatically copied into `IllRequestPortal.Web/` before compilation. See the [Organizational-Specific Customization](#organizational-specific-customization) section for details.

---

## Organizational-Specific Customization

The project uses a **build-time copy mechanism** to support multiple library deployments from one codebase. The base project (`IllRequestPortal.Web`) contains generic code; the `organizational-specific/web/` folder contains files that override it for a specific organization.

### How it works

Before each build, MSBuild copies files from `organizational-specific/web/` into the `IllRequestPortal.Web/` project directory, overwriting any matching files:

```xml
<!-- In Web.csproj -->
<Target Name="CopyOrgSpecificFiles" BeforeTargets="Build">
  <Copy SourceFiles="@(OrgSpecificFiles)"
        DestinationFiles="@(OrgSpecificFiles->'$(ProjectDir)%(RecursiveDir)%(Filename)%(Extension)')"
        OverwriteReadOnlyFiles="true" />
</Target>
```

File types copied: `*.cs`, `*.cshtml`, `*.css`, `*.js`, `*.png`, `*.json`, `*.resx`, `*.xml`, `*.csproj`, `*.ps1`

### What is typically customized

| File | What it overrides |
|------|------------------|
| `appsettings.json` / `appsettings.*.json` | Connection strings, API credentials, auth host |
| `StartupExtended.cs` | Additional services, middleware, AutoMapper config, language settings |
| `Controller/HomeController.cs` | Org-specific routing or landing page behavior |
| `Controller/IllRequestController.cs` | Org-specific request handling |
| `Resources/*.resx` | Translated UI strings |
| `wwwroot/css/*.css` | Org branding |

### Important

- **Do not edit auto-overwritten files directly** in `IllRequestPortal.Web/` if they originate from `organizational-specific/web/` — your changes will be lost on the next build.
- The files that are auto-generated or auto-copied have a comment at the top: `// This is an organization specific file`
- Make changes in `organizational-specific/web/` instead.

---

## Authentication

Authentication is provided by `Sh.Library.Authentication` (NuGet v1.2.12+), which connects to a central AuthService.

### How it works

1. A user hits a protected page.
2. The middleware checks the `BiblAppsSession` cookie.
3. If the cookie is missing or invalid, the user is redirected to `Authentication:Host` (the AuthService URL) to log in.
4. After login, AuthService redirects back and the middleware validates the session via `POST /api/v1/authentication`.
5. The cookie is set and the user can access the page.

### Configuration

In `appsettings.json`:
```json
"Authentication": {
  "Host": "https://bibl-app.sh.se/auth-service"
}
```

In `StartupExtended.cs`:
```csharp
// ConfigureServices:
services.AddLibraryAuthentication(authenticationHost: Configuration["Authentication:Host"]);

// Configure (after UseRouting):
app.UseLibraryAuthentication();
app.UseLibraryApiAuthentication();
```

### Public vs. protected routes

| Route | Access |
|-------|--------|
| `GET /illrequest/create` | Public (`[NoLibraryAuth]`) |
| `POST /illrequest/create` | Public (`[NoLibraryAuth]`) |
| `GET /illrequest` (admin list) | Requires authentication |
| `GET /illrequest/edit/{id}` | Requires authentication |
| `GET /api/v1/bibliographic-records/lookup` | Public |
| `GET /api/v1/patrons` | Public |
| `GET /api/v1/illrequests` | Requires bearer token |

### Reading user info (in authenticated pages)

```csharp
using Sh.Library.Authentication;

var cookie = HttpContext.Request.Cookies[AuthenticationLibraryTools.SessionCookieName];
var userName = AuthenticationLibraryTools.GetUserName(cookie);
var name     = AuthenticationLibraryTools.GetName(cookie);
var email    = AuthenticationLibraryTools.GetEmail(cookie);
```

---

## Key Features

### Bibliographic Lookup

When a user enters an ISBN or ISSN, the form calls `GET /api/v1/bibliographic-records/lookup` which runs the following pipeline:

```
1. Normalize the standard number (remove spaces/dashes, validate checksum)
       │
       ▼
2. Query Koha for a local bibliographic record
   ├─ Found → status = FoundInKoha
   │           Show "Item exists locally" + link to discovery system
   │           Still populate form fields
   └─ Not found
           │
           ▼
3. Query Libris (libris.kb.se/find?q={identifier})
   ├─ Found → status = FoundInLibris
   │           Populate title, author, publication year from Libris JSON-LD
   └─ Not found → status = NotFound
```

The JavaScript in `custom-site.js` handles the response and populates the form fields automatically.

### Material Types

The form supports three material types with different field sets:

| Type | Fields shown |
|------|-------------|
| **Book** | Title, Author, Publication Year, ISBN |
| **Article** | Article Title, Article Author, Journal Title, Publication Year, Volume, Issue, Pages, ISSN |
| **Chapter** | Chapter Title, Chapter Author, Book Title, Book Author, Publication Year, Pages, ISBN |

Fields are shown/hidden dynamically via CSS classes (`.field-book`, `.field-article`, `.field-chapter`) controlled by JavaScript when the material type selector changes.

### Local Catalog Validation

When Koha returns a match for a standard number, the user sees:
- A notification that the item already exists in the local collection
- A link to the discovery system (URL built from `DiscoverySettings.RecordUrlTemplate` with the Koha biblio ID substituted)

This prevents unnecessary ILL requests for locally available items.

### Patron Lookup

When a library card number is entered (10-digit field), the form calls `GET /api/v1/patrons?cardNumber={number}` which queries the Koha REST API and auto-fills the requester's name and email. This reduces input errors and ensures the stored patron data matches Koha.

---

## API Endpoints

### Bibliographic Records

```
GET /api/v1/bibliographic-records/lookup
    ?standardNumber={isbn-or-issn}
    &queryField={isbn|issn}

Response:
{
  "status": "FoundInKoha" | "FoundInLibris" | "NotFound" | "Error",
  "message": "...",
  "biblio_id": 123,
  "title": "...",
  "author": "...",
  "publication_year": "...",
  "edition": "...",
  "volume": "...",
  "koha_url": "...",
  "libris_url": "..."
}
```

### Patrons

```
GET /api/v1/patrons?cardNumber={cardNumber}

Response: Patron object with name and email (from Koha)
```

### ILL Requests (requires bearer token)

```
GET    /api/v1/illrequests               — Get all requests
GET    /api/v1/illrequests/{id}          — Get single request
GET    /api/v1/illrequests/search?...    — Filter by properties
GET    /api/v1/illrequests/since/{id}    — Get requests with ID > {id}
POST   /api/v1/illrequests               — Create request
PUT    /api/v1/illrequests/{id}          — Update request
DELETE /api/v1/illrequests/{id}          — Delete request
POST   /api/v1/illrequests/{id}/status   — Update status only
```

API responses use **snake_case** JSON keys (configured via `SnakeCaseNamingStrategy` in `StartupExtended.cs`).

> **Note:** The snake_case global JSON serialization setting only applies to the Newtonsoft.Json serializer used for API responses. The `Sh.Library.Authentication` library (v1.2.12+) uses explicit `JsonSerializerSettings` and is not affected by this global setting.

---

## Configuration Reference

The repository ships with `appsettings.template.json`. Copy this to create your environment-specific config files.

```json
{
  "ConnectionStrings": {
    "Default": "Server=YOUR_SQL_SERVER;Database=IllRequests;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Authentication": {
    "Host": "https://bibl-app.sh.se/auth-service",
    "BearerToken": "SET_A_SECRET_TOKEN_FOR_API_AUTHENTICATION"
  },
  "Application": {
    "Name": "SET_APPLICATION_NAME",
    "KeepLogsInDays": 30,
    "KeysFolder": "SET_PATH_TO_FOLDER_FOR_DATA_PROTECTION_KEYS"
  },
  "KohaApiSettings": {
    "BaseUrl": "https://YOUR_KOHA_DOMAIN/api/v1",
    "AuthenticationHeaderValue": "username:password"
  },
  "LibrisApiSettings": {
    "BaseUrl": "https://libris.kb.se"
  },
  "DiscoverySettings": {
    "RecordUrlTemplate": "https://library.example.org/primo/fulldisplay?docid=LOCAL_KOHA{biblioId}&vid=MAIN"
  }
}
```

### Key settings

| Setting | Description |
|---------|-------------|
| `ConnectionStrings.Default` | SQL Server connection string |
| `Authentication.Host` | URL of the AuthService instance |
| `Authentication.BearerToken` | Secret token protecting the ILL request API endpoints |
| `Application.KeepLogsInDays` | How many days of logs to retain (cleaned up by the CleanUpService) |
| `Application.KeysFolder` | Path for ASP.NET Data Protection keys (used for cookie encryption) |
| `KohaApiSettings.BaseUrl` | Koha REST API base URL (e.g. `https://koha.example.org/api/v1`) |
| `KohaApiSettings.AuthenticationHeaderValue` | Koha credentials as `username:password` (used as HTTP Basic auth) |
| `LibrisApiSettings.BaseUrl` | Libris API base URL (usually `https://libris.kb.se`) |
| `DiscoverySettings.RecordUrlTemplate` | URL template for discovery system links; `{biblioId}` is replaced with the Koha biblio ID. Leave empty to disable. |

### Koha API account

The Koha account used by `KohaApiSettings.AuthenticationHeaderValue` needs read access to:
- `GET /api/v1/patrons` (patron lookup by card number)
- `GET /api/v1/biblios` (bibliographic record lookup)

---

## Database

### Creating the database

1. Create an empty SQL Server database.
2. Run the migration script:
   ```
   IllRequestPortal.Web/Migration/Migrations.sql
   ```
   The script is idempotent and safe to run multiple times.

### Tables

#### `IllRequest`

Stores incoming ILL requests. Each row is one submitted request.

| Column | Type | Notes |
|--------|------|-------|
| `Id` | INT IDENTITY | Primary key |
| `Title` | NVARCHAR | Main title (book title, journal name, or book containing chapter) |
| `Author` | NVARCHAR | Main author |
| `ArticleTitle` | NVARCHAR | Article or chapter title (when material type is Article or Chapter) |
| `ArticleAuthor` | NVARCHAR | Article or chapter author |
| `PublicationYear` | NVARCHAR | |
| `Edition` | NVARCHAR | |
| `Isbn` | NVARCHAR | |
| `Issn` | NVARCHAR | |
| `Volume`, `Issue`, `Pages` | NVARCHAR | |
| `MaterialType` | NVARCHAR | `Book`, `Article`, or `Chapter` |
| `RequesterName` | NVARCHAR | Patron name (snapshot from Koha at time of request) |
| `RequesterEmail` | NVARCHAR | |
| `CardNumber` | NVARCHAR | Library card number |
| `Status` | NVARCHAR | Set to `Created` on insert; updated via admin or API |
| `KohaUrl` | NVARCHAR | Link to Koha record (if found locally) |
| `LibrisUrl` | NVARCHAR | Link to Libris record (if found in Libris) |
| `PurchaseFormatPreference` | NVARCHAR | |
| `CreatedOn` | DATETIME2 | Set automatically on insert |
| `UpdatedOn` | DATETIME2 | Set automatically on update |
| `AddedInLibrisOn` | DATETIME2 | Nullable; set when processed in Libris |

Indexes: `Status`, `CardNumber`, `CreatedOn`

#### `Log`

Application logs written via NLog.

| Column | Description |
|--------|-------------|
| `Origin` | Logger name / call site |
| `Message` | Log message |
| `LogLevel` | Trace, Debug, Info, Warn, Error, Fatal |
| `Exception` | Exception message |
| `Trace` | Stack trace |
| `CreatedOn` | Timestamp |

Old log entries are automatically deleted by `CleanUpServiceExtended` based on `Application.KeepLogsInDays`.

#### `Migration`

Tracks applied database schema versions.

| Column | Description |
|--------|-------------|
| `ClientVersion` | Application version at time of migration |
| `DatabaseVersion` | Database schema version applied |

---

## Localization

The UI supports Swedish (`sv-se`) and English (`en-gb`). Swedish is the default.

Resource files are in `IllRequestPortal.Web/Resources/`:
- `SharedResource.sv.resx` — Swedish strings
- `SharedResource.en.resx` — English strings

Text used in Razor views is accessed via `SharedLocalizer`. Text used in JavaScript is injected as a `window.illTexts` JSON object in the layout, avoiding hardcoded strings in JS.

The user can toggle language via `POST /` with `returnUrl` parameter. The selected culture is stored in a culture cookie.

To add or modify translated strings, edit the `.resx` files in `organizational-specific/web/Resources/` (not directly in `IllRequestPortal.Web/Resources/` — they will be overwritten on the next build).

---

## Logging

Logging is handled via NLog. Configuration files:

| File | Environment |
|------|-------------|
| `nlog.config` | Base / production |
| `nlog.Development.config` | Development overrides |

Logs are written to the `Log` database table and optionally to files in `logs/`.

`CleanUpServiceExtended` automatically deletes log entries older than `Application.KeepLogsInDays` days. It runs via the `/clean-up` endpoint exposed by the `CleanUpMiddleware`.

---

## Administrative Interface

The admin interface is protected by authentication (redirects to AuthService login).

| Route | Purpose |
|-------|---------|
| `/admin` | Redirects to the request list |
| `/illrequest` | List all submitted requests (DataTables, with copy and status-change) |
| `/illrequest/edit/{id}` | Edit a request |
| `/illrequest/remove/{id}` | Delete a request |
| `/log` | View application logs |
| `/migration` | View database migration history |
| `/version` | Returns JSON with assembly and database versions |

The request list uses **DataTables** for sorting, searching, and pagination. The status of a request can be updated inline via a dropdown that calls `POST /api/v1/illrequests/{id}/status`.

---

## Publishing & Deployment

1. Ensure all org-specific config files in `organizational-specific/web/` are correct for the target environment.

2. Build in Release mode — the `CopyOrgSpecificFiles` MSBuild target will copy all org-specific overrides before compilation:
   ```
   dotnet publish -c Release
   ```

3. The output directory will contain the fully merged application (base code + org-specific overrides).

4. Ensure the `Application.KeysFolder` path exists on the server and is writable by the application process. ASP.NET Data Protection keys are stored here.

5. Run the migration script against the production database if the schema has changed.
