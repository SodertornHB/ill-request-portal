# IllRequestPortal

IllRequestPortal is a lightweight intake interface for interlibrary loan (ILL) requests.

The application is designed to sit in front of existing library systems and capture request information before forwarding it to an external ILL workflow.

The system does not manage the ILL lifecycle itself.

Instead it handles:

- capturing the request
- validating bibliographic information
- checking whether the item already exists in the local catalog
- retrieving bibliographic metadata from external systems
- storing requests in a database

The application is intentionally simple and designed to integrate with systems such as Koha, Libris, and discovery systems such as Primo.

---

# Key features

## Bibliographic lookup

When a user enters an ISBN or ISSN, the system performs a bibliographic lookup and attempts to populate the request form automatically.

The lookup pipeline works as follows:

1. The system first queries the local Koha catalog.
1. If a matching bibliographic record is found locally, the user is informed that the item already exists in the local collection.
1. A link to the discovery system is displayed so the user can access or borrow the item directly.
1. If the item does not exist locally, the system queries Libris and retrieves bibliographic metadata.

The metadata returned from the lookup is used to populate fields in the request form.

Typical fields that can be auto-populated include:

- Title
- Author
- Publication year
- Edition
- Volume / Issue (for journal articles)
- Material type

This significantly reduces manual input and improves data quality.

The client-side lookup logic is implemented in JavaScript and triggers when ISBN or ISSN fields change.

---

## Material types and metadata model

The request form supports three different types of requested material:

- Book
- Article
- Chapter

The Razor view dynamically shows or hides fields depending on the selected material type.

CSS classes controlling visibility include:

- .field-book
- .field-article
- .field-chapter

---

## Local catalog validation

The system checks whether a requested title already exists in the local Koha catalog.

If the item exists locally:

- the user is notified immediately
- a link to the discovery system is provided
- the user can access the item directly instead of submitting an ILL request

This prevents unnecessary interlibrary loan requests for materials that are already available locally.

---

## Patron lookup

Users are identified using their library card number.

When the card number is entered:

- the system queries Koha via the Koha REST API
- the patron's name and email address are retrieved
- the form fields are automatically populated

This ensures correct user information and reduces manual input errors.

---

# Database setup

The application requires a SQL Server database.

A migration script included in the repository creates the required tables and indexes.

The script is idempotent and can be executed multiple times safely.

---

# Database tables

## IllRequest

Stores incoming interlibrary loan requests.

Each row represents a request submitted through the portal.

Key fields include:

- MainTitle
- MainAuthor
- ContainerTitle
- PublicationYear
- Isbn
- Issn
- Volume
- Issue
- Pages
- MaterialType
- RequestType
- RequesterName
- RequesterEmail
- CardNumber
- Status
- CreatedOn
- UpdatedOn

Patron information is stored directly in the request record.

The authoritative user data resides in Koha, so the request stores a snapshot of the relevant user information at the time the request is created.

---

## Log

Stores application logs written via NLog.

Columns include:

- Origin
- Message
- LogLevel
- Exception
- Trace
- CreatedOn

This table is used for diagnostics and operational monitoring.

---

## Migration

Tracks which database versions have been applied.

Columns include:

- ClientVersion
- DatabaseVersion

This allows the application to verify that the database schema matches the running version of the application.

---

# Creating the database

1. Create an empty SQL Server database.

2. Run the migration script included in the repository.

The script will create the required tables and indexes if they do not already exist.

---

# Configuration

The application is configured using appsettings.json.

For security reasons, the repository contains a template file named appsettings.template.json.

During installation:

1. Copy the template
2. Rename it to appsettings.json
3. Update the values for your environment

Example:

```
bash cp appsettings.template.json appsettings.json 
```
---

## Database connection

ConnectionStrings.Default defines the SQL Server connection used by the application.

Example:

```
json "ConnectionStrings": { "Default": "Server=localhost\\sqlexpress;Database=IllRequests;Trusted_Connection=True;TrustServerCertificate=True;" } 
```
---

## API authentication

Authentication.BearerToken protects internal API endpoints used by the application.

Set this to a strong random string.

---

## Application settings

Application.Name defines the name of the installation.

Application.KeepLogsInDays controls how long logs are retained.

Application.KeysFolder defines where ASP.NET Data Protection keys are stored.

Example:

```
json "Application": { "Name": "Library IllRequest Portal", "KeepLogsInDays": 30, "KeysFolder": "C:\\IllRequestPortal\\keys" } 
```
---

## Koha API configuration

The application communicates with Koha using the Koha REST API.

Example configuration:

```
json "KohaApiSettings": { "BaseUrl": "https://koha.example.org/api/v1", "AuthenticationHeaderValue": "username:password" } 
```
The integration account must have permission to read:

- patron information
- bibliographic records

---

## Libris API configuration

LibrisApiSettings.BaseUrl defines the base URL used when querying Libris for bibliographic data.

Example:

```
json "LibrisApiSettings": { "BaseUrl": "https://libris.kb.se" } 
```
The application queries Libris using the /find endpoint and parses the returned JSON-LD data.

ISBN and ISSN lookups are handled by separate parsers because Libris responses differ depending on identifier type.

---

## Discovery system integration

DiscoverySettings.RecordUrlTemplate defines how links to local records are generated when a title already exists in Koha.

The placeholder {biblioId} is replaced with the bibliographic identifier returned by Koha.

Example (Primo):

```
json "DiscoverySettings": { "RecordUrlTemplate": "https://library.example.org/primo-explore/fulldisplay?docid=LOCAL_KOHA{biblioId}&vid=MAIN" } 
```
Example (Koha OPAC):

```
json "DiscoverySettings": { "RecordUrlTemplate": "https://library.example.org/cgi-bin/koha/opac-detail.pl?biblionumber={biblioId}" } 
```
---

If you do not wish to use the discorvery link functionality, you can leave the value of the `RecordUrlTemplate` setting blank. 

```
json "DiscoverySettings": { "RecordUrlTemplate": "" } 
```
---

# Localization

The user interface supports localization using .resx resource files.

Text used in:

- Razor views
- JavaScript messages
- form labels

is stored in resource files and accessed via SharedLocalizer.

This allows the interface to support multiple languages.

---

# Logging

Application logging is handled via NLog.

Logs are written to the Log table and include:

- log level
- message
- call site
- exception message
- stack trace

Logging configuration is defined in NLog.config.

---

# Administrative interface

The application includes a minimal administrative interface.

Administrators can:

- view submitted requests
- edit and delete submitted requests
- monitor request flow

Administrative routes are available under:

/admin

---

# System architecture

```
User
↓
Request form
↓
Bibliographic lookup in Koha
↓
Bibliographic lookup in Libris
↓
Patron lookup in Koha
↓
Database (IllRequest)
↓
External ILL processing system (manual step)
```

The database acts primarily as a queue for outgoing requests.

---

# Future development

Potential future enhancements include:

- integration with discovery systems such as Primo for improved bibliographic lookup
- automated export to external ILL processing systems
- request status updates
- improved administrative dashboards
- reporting and monitoring tools