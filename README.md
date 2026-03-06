# IllRequestPortal

This application provides a simple intake interface for interlibrary loan (ILL) requests.
It collects request information from users and stores it in a database before the requests are forwarded to an external processing system that handles the actual interlibrary loan workflow.

The system does not manage the ILL lifecycle itself. It only handles:

- capturing the request
- storing it in the database
- exporting it to an external system

---

# Database setup

The application requires a SQL Server database.

A migration script is included in the repository that creates the required tables and indexes.

The script is idempotent, meaning it can be executed multiple times without breaking existing installations.

## Tables created

### Log

Stores application logs written via NLog.

Columns include:

- Origin
- Message
- LogLevel
- CreatedOn
- Exception
- Trace

This table is used for operational diagnostics and troubleshooting.

---

### IllRequest

Stores incoming interlibrary loan requests.

Each row represents one request submitted by a user.

Key fields include:

- Title
- Author
- PublicationYear
- Edition
- Isbn
- Issn
- MaterialType
- RequestType
- RequesterName
- RequesterEmail
- CardNumber
- Status
- ExternalRequestId

User information is stored directly in this table instead of referencing a user table.
The authoritative user data lives in Koha, so duplicating a snapshot here simplifies the model.

---

### Migrations

Tracks database versions that have been applied.

Columns:

- ClientVersion
- DatabaseVersion
- CreatedOn

This table is used by the application to ensure the database schema is compatible with the running version.

---

# Creating the database

1. Create an empty SQL Server database.

Example:

sql CREATE DATABASE IllRequests; 

2. Run the migration script included in the repository.

This script will create the required tables and indexes if they do not already exist.

---

# Connection string

The application reads the database connection string from configuration.

Example appsettings.json:

json {  "ConnectionStrings": {  "Default": "Server=localhost;Database=IllRequests;Trusted_Connection=True;TrustServerCertificate=True"  } } 

---

# Logging

Application logging is handled through NLog and stored in the Log table.

The configuration is defined in NLog.config.

Logs include:

- log level
- message
- origin (callsite)
- exception message
- stack trace

---

# Development notes

The database model is intentionally simple:

- No foreign keys to user tables
- Each request is self-contained
- The database acts primarily as a request queue before export to the external ILL processing system

This keeps the application lightweight and avoids coupling to external library systems such as Koha.

---

# Next steps

Future development will include:

- exporting requests to the external ILL processing system
- status updates after export
- administrative views for monitoring request flow