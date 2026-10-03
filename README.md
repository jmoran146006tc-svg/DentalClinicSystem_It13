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

The sidebar follows each account's role permissions, highlights the active page, and keeps the welcome header and clock visible during navigation. Admin accounts can open reports with date filters and read-only tables. Phase 6 adds a shared design system, runtime legacy styling and a DEBUG review window. The new page layouts, expanded dashboards, calendar, charts and automated test project remain future work.

In Debug, press **Ctrl+Shift+F12** on the dashboard to open the style guide. To review it without a database, run `dotnet run --project DentalClinicSystem/DentalClinicSystem.csproj -- --style-guide`. The guide is not included in Release.

Existing Designer layouts are retained; runtime styling (and later runtime reparenting) may differ from the Designer view. Build checks do not verify interactive GUI behavior.
