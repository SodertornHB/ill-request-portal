 # IllRequestPortal

 IllRequestPortal is a lightweight intake interface for interlibrary loan (ILL) requests.

 The application is designed to sit in front of existing library systems and capture request information before forwarding it to an external ILL workflow.

 The system does not manage the ILL lifecycle itself.

 Instead it handles:

 - capturing the request
 - validating bibliographic information
 - checking whether the item already exists in the local catalog
 - storing requests in a database
 - exporting requests to an external ILL processing system

 The application is intentionally simple and designed to integrate with systems such as Koha, Libris, and discovery systems like Primo.

 ---

 # Key features

 ## Bibliographic lookup

 When a user enters an ISBN or ISSN, the system performs a bibliographic lookup.

 The lookup process works as follows:

 1. The system first checks the local Koha catalog.
 2. If the title exists locally, the user is informed and shown a link to borrow the item in the discovery system.
 3. If the title does not exist locally, the system queries Libris and retrieves bibliographic metadata.

 The request form can then automatically populate fields such as:

 - Title
 - Author
 - Publication year
 - Edition
 - Material type

 This reduces manual input and improves data quality.

 ---

 ## Local catalog validation

 The system checks whether a requested title already exists in the local Koha catalog.

 If the item exists locally, the user receives a link to the discovery system instead of submitting an ILL request.

 This helps avoid unnecessary interlibrary loan requests for materials already available locally.

 ---

 ## Patron lookup

 Users are identified using their library card number.

 When the card number is entered:

 - the system queries Koha
 - the patron's name and email are automatically populated

 This ensures accurate contact information and reduces manual entry.

 ---

 # Database setup

 The application requires a SQL Server database.

 A migration script is included in the repository that creates the required tables and indexes.

 The script is idempotent, meaning it can be executed multiple times safely.

 ---

 # Database tables

 ## IllRequest

 Stores incoming interlibrary loan requests.

 Each row represents a request submitted by a user.

 Key fields include:

 - Title
 - Author
 - PublicationYear
 - Edition
 - Isbn / Issn / IsbnIssn
 - MaterialType
 - RequestType
 - RequesterName
 - RequesterEmail
 - CardNumber
 - Status
 - ExternalRequestId
 - ExportedOn
 - ExportError
 - CreatedOn
 - UpdatedOn

 Patron information is stored directly in the request record.

 The authoritative user data resides in Koha, so the request stores a snapshot of the relevant user information.

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
 - AppliedOn

 This allows the application to verify that the database schema matches the running version of the application.

 ---

 # Creating the database

 1. Create an empty SQL Server database.

 Example:

 sql  CREATE DATABASE IllRequests; 

 2. Run the migration script included in the repository.

 The script will create the required tables and indexes if they do not already exist.

 ---

 # Configuration

 ## Connection string

 The database connection string is configured in appsettings.json.

 Example:

 json  {  "ConnectionStrings": {  "Default": "Server=localhost;Database=IllRequests;Trusted_Connection=True;TrustServerCertificate=True"  }  } 

 ---

 # Localization

 The user interface supports localization using .resx resource files.

 Text used in:

 - Razor views
 - JavaScript messages
 - form labels

 is stored in resource files and accessed through SharedLocalizer.

 This allows the interface to support multiple languages.

 ---

 # Logging

 Application logging is handled through NLog.

 Logs are written to the Log table and include:

 - log level
 - message
 - call site (origin)
 - exception message
 - stack trace

 Logging configuration is defined in:

  NLog.config 

 ---

 # Administrative interface

 The application includes a minimal administrative interface.

 This interface allows administrators to:

 - view submitted requests
 - monitor request flow
 - inspect export errors

 Administrative routes are available under:

  /admin 

 ---

 # System architecture

  User  ↓  Request form  ↓  Bibliographic lookup  (Koha → Libris)  ↓  Database (IllRequest)  ↓  Export to external ILL processing system 

 The database acts primarily as a queue for outgoing requests.

 ---

 # Design principles

 The system follows a deliberately simple architecture:

 - minimal data model
 - no dependency on the Koha database schema
 - no direct coupling to the external ILL processing system
 - requests are self-contained

 This makes the application easy to deploy and integrate with existing library systems.

 ---

 # Future development

 Possible future enhancements include:

 - automated export to the external ILL processing system
 - request status updates
 - improved administrative dashboards
 - request monitoring and reporting
 - additional catalog integrations