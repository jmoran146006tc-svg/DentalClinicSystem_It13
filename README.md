# DENTAL CLINIC APPOINTMENT AND TREATMENT MANAGEMENT SYSTEM

A desktop application for a dental clinic front desk and dentists to manage patient records, dentist records, appointment scheduling, and treatment history.

Team Rabenda
* Llano
* Moran
* Valdez
* Villacin

---
Database setup is manual. The application only connects to an existing database and calls stored procedures.

1. Start MySQL Server and connect in MySQL Workbench.
2. Execute `Database/01_Schema.sql`, then `Database/02_StoredProcedures.sql`, then `Database/03_SeedData.sql`.
3. Check the connection constants in `DentalClinicSystem/DBContent/DbConnectionHelper.cs` for your local server.
4. Optionally enable `SET GLOBAL event_scheduler = ON` with DBA permission for automatic patient inactivity. The procedure can also be run manually with `CALL sp_Patient_DeactivateStale();`.

The scripts are re-runnable but do not migrate existing tables. **`Database/00_ResetDatabase.sql` destroys all data.** Back up anything needed before using it to rebuild an old schema.

DEMO ONLY: `admin / admin123`, `reception / reception123`, `drsantos / dentist123`.

This revision stops after Phase 4 at the user's request. See `docs/REVISION_NOTES.md` for completed work, verification, runtime checks and deferred phases. The themed pages, dashboards, reports UI and xUnit project from later phases are not implemented.
