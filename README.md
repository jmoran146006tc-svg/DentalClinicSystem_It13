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

The sidebar follows each account's role permissions, highlights the active page, and keeps the section title and clock visible during navigation. Login and record pages use the shared design system and responsive runtime layouts. Appointments have search/status/date filters and a details dialog with role-controlled completion and cancellation. Treatments support Add and Update. Admin accounts can open reports with date filters and read-only tables.

In Debug, press **Ctrl+Shift+F12** on the dashboard to open the style guide. To review it without a database, run `dotnet run --project DentalClinicSystem/DentalClinicSystem.csproj -- --style-guide`. The guide is not included in Release.

Existing Designer layouts are retained. Runtime layout methods move the original controls into new containers, so the Designer view shows the legacy layout. Run regression tests with `dotnet test DentalClinicSystem.slnx`. Build and unit checks do not verify live MySQL operations or display scaling.

Record pages support Ctrl+F for search, Ctrl+N for a new record, Ctrl+S to save, and Enter to move through single-line form inputs. Multiline fields keep Enter for new lines. Login submits with Enter. The existing Animator handles transitions, feedback and loading reveals; reduced motion shows the final state immediately. The optional collapsible sidebar remains deferred.

For local previews using test data, set `DENTAL_UI_SNAPSHOTS` to an output directory before running tests. Previews in `docs/` remain local and ignored. Real MySQL flows, motion recordings, 125%/150% display scaling and Visual Studio Designer opening still need manual review.
