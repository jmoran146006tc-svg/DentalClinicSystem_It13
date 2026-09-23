# DENTAL CLINIC APPOINTMENT AND TREATMENT MANAGEMENT SYSTEM

A desktop application for a dental clinic front desk and dentists to manage patient records, dentist records, appointment scheduling, and treatment history.

Team Rabenda
* Llano
* Moran
* Valdez
* Villacin

---
**You don't need to run anything in Workbench.** `Program.cs` calls
`DatabaseInitializer.EnsureDatabaseReadyAsync()` on startup, which creates the
`dentalclinicdb` database, all six tables, and every stored procedure below if they
don't exist yet, then seeds starter data (including a demo `admin` / `admin123`
login) only if the tables are still empty.

The SQL queries themselves are located in `DBContent/` if you want to see them directly.

`Schema.sql` and `StoredProcedures.sql` are the same statements in plain `.sql` form,
kept here for manual reference, e.g. if you want to open them in MySQL Workbench to
show the actual stored procedures during a check-in, or run them by hand
against a database the app didn't create. 
