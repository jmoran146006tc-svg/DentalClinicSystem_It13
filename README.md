# DENTAL CLINIC APPOINTMENT AND TREATMENT MANAGEMENT SYSTEM

A desktop application for a dental clinic front desk and dentists to manage patient records, dentist records, appointment scheduling, and treatment history.

Team Rabenda
* Llano
* Moran
* Valdez
* Villacin

---
## Requirements

Windows, a Visual Studio installation that supports .NET 10 WinForms (desktop development workload), the .NET 10 SDK, MySQL Server and MySQL Workbench. The solution is `DentalClinicSystem.slnx`; the application targets `net10.0-windows`. Restore packages and build before running.

## Database setup

Database setup is manual. The application only connects to an existing database and calls stored procedures.

1. Start MySQL Server and connect in MySQL Workbench.
2. Execute `Database/01_Schema.sql`, then `Database/02_StoredProcedures.sql`, then `Database/02b_FunctionsTriggersEvents.sql`, then `Database/03_SeedData.sql`.
3. Check the connection constants in `DentalClinicSystem/DBContent/DbConnectionHelper.cs` for your local server.
4. Optionally enable `SET GLOBAL event_scheduler = ON` with DBA permission for automatic patient inactivity. The procedure can also be run manually with `CALL sp_Patient_DeactivateStale();`.

**OPTIONAL — DEVELOPMENT DEMO ONLY:** manually run `Database/05_DemoData.sql` after setup. It adds three dentists and Dentist accounts (`drreyes`, `drdelrosario`, `drnavarro`, `drbautista`, all using the baseline demo password `dentist123`), 24 patients including minors, seniors, medical notes, an inactive patient and a patient with only cancelled visits, plus past/upcoming leave. Marked visits span 45 days before today through 14 days after today, using clinic hours, catalog durations, all five statuses and 1–3 treatments for completed visits. It skips Sundays, leave and dentist/patient overlaps. `Notes = 'Demo data v2'` identifies appointments/treatments; patient identities and dentist licenses identify the other demo rows. It is deterministic for the current date and re-runnable without replacing existing rows. Use a development database with the app closed. The temporary procedure runs a transaction, rolls back on failure, prints summary counts/net billed totals and is dropped afterwards. Today includes a synthetic CheckedIn example at the nearest slot, even when run before opening. Existing bookings/leave may reduce the inserted batch. The app never runs it.

For an existing database, run `Database/04_Migration_RealWorldFixes.sql`, then `Database/02_StoredProcedures.sql`, then `Database/02b_FunctionsTriggersEvents.sql`, then `Database/03_SeedData.sql` (04 → 02 → 02b → 03). The migration checks existing columns and constraints, keeps existing data, adds `AuditLog`, and removes its temporary helper procedures. Run it twice on a backup when verifying an upgrade. Fresh installs use 01 → 02 → 02b → 03; these scripts are designed to be re-runnable. Install 02b before using this application version: successful appointment inserts now reactivate inactive patients inside the database statement.

If an older copy of 03 fails with error 1267 at the NoShow `LEAST(...)` expression, reload the updated script: it casts session date variables back to `DATE` before comparison/arithmetic. In the original Workbench connection, check `SELECT @seed_appointments, @juan, @ana, @maria, @carlos, @week_start, @previous_open_day, @demo_day;` (the seed flag must still be 1 and the IDs/dates must be populated). If execution stopped at that error, run from the corrected NoShow INSERT through the end. If Workbench continued with later statements, run only the failed INSERT; the existing treatment INSERT is safe to repeat. Do not restart 03 from the top to finish a partial batch: its empty-appointments guard will skip the missing appointments. If the session was reset or the flag is no longer 1, inspect the existing demo rows before choosing a recovery range; do not delete appointments or force the guard.

02b adds `fn_TreatmentNet` (deterministic, no SQL), `fn_DentistHasConflict` and `fn_DentistOnLeave` (read SQL data). Dentist-row locks serialize appointment inserts and slot updates; conflict/leave functions use locking current reads to avoid stale transaction snapshots. Triggers also guard status transitions and cancelled/no-show treatment parents. C# services retain their friendly validation. Audit triggers record patient changes, treatment inserts/updates/deletes, appointment status/slot changes and user Role/IsActive changes in `AuditLog`, indexed by table/record. They record what and when; actor attribution is a separate task. User password hashes are never included. MySQL 8.0+ and permission to create routines/triggers/events are required; function characteristics address binary-logging error 1418, but DBA privilege requirements still apply. See [MySQL stored-program logging](https://dev.mysql.com/doc/refman/8.0/en/stored-programs-logging.html).

The existing patient-inactivity event remains unchanged. `ev_flag_missed_appointments` would mark Scheduled visits NoShow once their end is at least 24 hours old; it ships **disabled**. Enable only after the clinic/adviser confirms this rule. Enabling the global event scheduler does not enable this disabled event.

**`Database/00_ResetDatabase.sql` destroys all data.** It is not part of an upgrade. Back up anything needed before using it to rebuild a schema. The app never creates or migrates the database.

DEMO ONLY: `admin / admin123`, `reception / reception123`, `drsantos / dentist123`.

Login centers a frosted glass card and one overlapping brand badge over a bright full-bleed clinic photo. The embedded `Resources/dentist.jpg` determines the window aspect ratio; photo, luminous edge gradients and 18 floating glass shapes, rings, a tooth outline, sparkles and accents are cached by size and DPI. The stronger three-pass blur captures the gradients before the crisp shapes and receives a 94% white wash for smooth frosting. Cached text uses grayscale antialiasing against an opaque sampled tone. The card and badge are centered together, with a 24 px password-to-button gap. A time-of-day greeting, username/password icons, visible password eye (also Alt+P), Enter to submit, and one generic inline error preserve the sign-in flow. The form fades over 150 ms, the card rises/fades over 320 ms, and the badge/field rows follow at 30 ms intervals. The decorative layers fade in over 320 ms after a 30 ms delay. Reduced motion shows the final state instantly.

Login photo by Benyamin Bohlouli on Unsplash.

The sidebar follows each account's role permissions and highlights the active page. Each page owns its title; the top bar shows the clock. Record pages use the shared design system and responsive runtime layouts. Appointments support walk-ins, check-in, rescheduling with a dentist change, visit reasons with an Other field for custom text, duration choices, overlap checks, and dentist time off. Dentist specialization uses the same Other flow. Admins and Receptionists can check in patients; Dentists can complete their own appointments. Only Scheduled appointments can be rescheduled or cancelled; CheckedIn appointments can only become Completed.

The dashboard shows Admins and Receptionists today's appointment count, active patients, cancellations for appointments dated this week, and a Monday–Sunday calendar with week navigation, dentist filtering, overlap columns and appointment details. Only Admins see the weekly net billed amount. Dentists see their own patients today, with patient history and completion actions for Scheduled or CheckedIn appointments. History is read-only and includes treatments and past appointments; Receptionists have no treatment-history access. Refresh reloads dashboard data. The calendar defaults to the clinic's 9:00 AM–5:00 PM range and expands only for displayed appointments outside that range. Day headers stay visible, closed days/non-working hours are shaded, patient names lead the blocks, and a minute-updated line marks now. The default grid fits without an inner vertical scrollbar; expanded ranges scroll with wheel chaining to the page at the boundaries.

Patients have guardian contact fields (required below age 18), allergies and medical notes. Duplicate names plus birthdates are rejected, and booking a valid slot for an inactive patient reactivates the record. Medical details appear in appointment details for Admins and Dentists; Receptionists can view them on the Patients page.

Treatments support Add and Update for eligible appointments through today, require the appointment's date, and validate optional FDI tooth numbers (permanent and primary teeth). Cost is the gross amount. None, Senior Citizen, PWD and Other discounts calculate a net amount; reports show **Billed**, which is not money collected. Net is calculated as `Cost - Cost * DiscountPercent / 100`; displayed currency uses two decimal places. All three report procedures use `fn_TreatmentNet`, returning `DECIMAL(16,6)` to preserve the unrounded C# result for two-decimal costs/percentages (a four-place return type would change fractional-discount totals). Admin reports total net billed amounts.

## Clinic assumptions

`DentalClinicSystem/Service/ClinicRules.cs` is the single place to change the assumed Monday–Saturday, 9:00 AM–5:00 PM clinic hours, Sunday closure, 15-minute past grace, and duration rules (30 minutes by default; 15–240 minutes in 15-minute steps). Senior Citizen and PWD choices default to 20% in that file. Confirm hours, discount handling/eligibility and actual prices with the clinic before real use; the catalog prices are placeholders. Catalog duration suggestions live in TreatmentTypes; initial seed durations are 45 minutes for cleaning/extraction, 90 for root canal, and 60 for filling. FDI validation lives in `Service/ToothNumbering.cs`.

Date-dependent services accept an optional `TimeProvider`; production uses the machine's local time. Configure the clinic computer for Philippine time (UTC+08:00).

## Known limitations

- No collection tracking; Billed is the net amount after discounts.
- No medical history beyond allergies and notes.
- One clinic-hours configuration, not a separate schedule per dentist.
- No SMS reminders.
- Leave/deactivation workflows still perform service checks before their writes. Appointment triggers serialize bookings for the same dentist; concurrent leave creation/deactivation workflows need separate live verification.
- Automated tests use repository/service fakes. Live MySQL script execution and end-to-end database operations need manual verification.

## Manual MySQL verification

1. On a backup of an existing database, run 04 twice, then 02 → 02b → 03 twice; check preserved records, new columns/constraints and absence of migration helper procedures. On a separate fresh database, run 01 → 02 → 02b → 03, then repeat them.
2. Book a walk-in during clinic hours, check in as Receptionist, and complete as Admin or the linked Dentist. Check that terminal statuses cannot be reopened.
3. Add a treatment to a completed appointment. Try a cancelled/no-show appointment and a future appointment; verify rejection and performed-date locking.
4. Reschedule a Scheduled appointment with a dentist change; verify unchanged reason/notes/duration, self-exclusion, row refresh and flash. Try an adjacent slot and a one-minute overlap with a long procedure.
5. Add dentist time off, then try booking in the range. Try adding leave over Scheduled/CheckedIn appointments and removing leave. Check clinic opening, closing and Sunday boundaries.
6. Try deactivating a dentist with an upcoming appointment, then reschedule/cancel it and retry. Verify that their linked Dentist account cannot sign in while inactive.
7. Book an inactive patient into a valid slot and verify reactivation. Try an invalid slot and confirm the patient remains inactive.
8. Register a minor without guardian details, then with valid details. Check the 18th-birthday boundary and active/inactive duplicates.
9. Add a gross 1,000 treatment with a 20% discount. Verify Net and Billed equal 800 in daily, top-treatment and dentist reports; verify editing, None and Other discounts too.
10. Insert an overlapping appointment directly from Workbench; expect the trigger's clear overlap message. Try adjacent half-open intervals and an inclusive leave-date boundary.
11. In two Workbench sessions, book the same dentist/slot concurrently; exactly one insert should succeed. Repeat with an already-open REPEATABLE READ transaction and concurrent slot updates; the current-read checks must see the winning commit.
12. Book an inactive patient: the successful statement must reactivate them. An overlapping/failed booking must leave them inactive, including when a later trigger fails and the statement rolls back.
13. Update Completed → Scheduled directly; expect rejection. Verify Scheduled → CheckedIn/Completed/Cancelled/NoShow and CheckedIn → Completed, with terminal statuses remaining terminal.
14. Change a patient, treatment, appointment slot/status and user role/active flag; expect one AuditLog row per applicable change, with old/new JSON and no password hash. No-op updates and password-only user changes must not add audit rows. Verify treatment insert/delete auditing too.
15. Save all three report result sets on the demo data before replacing the report expressions, then compare them afterwards without changing data or date ranges. Also compare the original formula with the function using the queries below; fractional discounts must retain exact totals.
16. Run 02b and 05 twice on the same date; expect no errors or duplicate demo rows. Check the baseline 03 seed on every weekday, upcoming leave without Scheduled/CheckedIn visits, durations, valid FDI numbers, patient/dentist overlap checks and the inactive/cancelled-only examples.
17. Check `information_schema.EVENTS`: `ev_flag_missed_appointments` must exist with `STATUS = 'DISABLED'`; the existing inactivity event is unchanged.

Read-only formula comparisons (same treatment rows as each report; both numeric columns must match):

```sql
SELECT DatePerformed, SUM(Cost - Cost * DiscountPercent / 100) AS BeforeFunction,
       SUM(fn_TreatmentNet(Cost, DiscountPercent)) AS AfterFunction
FROM Treatments GROUP BY DatePerformed;
SELECT TreatmentTypeId, SUM(Cost - Cost * DiscountPercent / 100) AS BeforeFunction,
       SUM(fn_TreatmentNet(Cost, DiscountPercent)) AS AfterFunction
FROM Treatments GROUP BY TreatmentTypeId;
SELECT a.DentistId, SUM(t.Cost - t.Cost * t.DiscountPercent / 100) AS BeforeFunction,
       SUM(fn_TreatmentNet(t.Cost, t.DiscountPercent)) AS AfterFunction
FROM Treatments t JOIN Appointments a ON a.AppointmentId = t.AppointmentId GROUP BY a.DentistId;
SELECT fn_TreatmentNet(1.23, 12.34) AS FractionalExample; -- 1.078218
```

In Debug, press **Ctrl+Shift+F12** on the dashboard to open the style guide. To review it without a database, run `dotnet run --project DentalClinicSystem/DentalClinicSystem.csproj -- --style-guide`. The guide is not included in Release.

The UI uses **AntdUI 2.4.12** (Apache-2.0) with a shared light palette and teal accent. Buttons, navigation actions, text/password inputs, choices, date/time pickers, discount inputs, record/report tables, status tags, checkboxes, inline alerts and toast notifications use AntdUI. Shared adapters retain the clinic's existing model IDs, date limits, required selections, validation, busy guards and read-only locks. The appointment week calendar, report charts, login artwork, dashboard cards and page layouts remain clinic-specific.

The appointment date/time editor uses a 24-hour clock so AntdUI exposes both hour and minute selection. Read-only tables retain the clinic's existing 12-hour display. The build and publish output include the third-party notice and AntdUI license.

Existing Designer layouts are retained. Runtime layout methods move the original controls into new containers, so the Designer view shows the legacy layout. The dashboard, calendar and history are built in code. `GlobalUsings.cs` maps common control names to AntdUI and the clinic adapters, allowing event handlers to stay focused on clinic behavior.

Run all tracked tests with `dotnet test DentalClinicSystem.slnx -c Release`. `tests/DentalClinicSystem.Tests` covers clinic services, permissions, validation and pure presentation helpers; `tests/DentalClinicSystem.UiTests` covers adapter controls, selection/search, dialogs, role navigation, required dates, input filtering, sorting, reports, CSV, chart painting and motion lifecycle. Tests use synthetic data, in-memory repositories or boundary spies and service stubs; they never connect to MySQL. The ignored root `DentalClinicSystem.Tests/` folder contains older local checks and is not part of the solution. Build/unit checks do not prove live MySQL operations, interactive GUI behavior or real display scaling.

## Roles

| Role | Pages and actions |
|---|---|
| Admin | All seven pages and their actions: Dashboard, Patients, Dentists, Appointments, Treatments, Users, Reports |
| Receptionist | Dashboard; view/manage Patients; view/manage/check-in/cancel Appointments. No Treatments, Dentists, Users, Reports or completion action |
| Dentist | Dashboard; own Appointments and completion; own Treatments. No patient management, Dentists, Users or Reports |

Services enforce permissions and dentist ownership independently of sidebar visibility. Admins/Dentists can open read-only patient history through permitted appointments. Patients, dentists and users are retained through inactivity/reactivation or soft deactivation; these records are not hard deleted.

## Architecture

Forms/UserControls orchestrate service interfaces. Services in `Service/` own business rules, validation and authorization; repository interfaces and MySQL implementations in `DBContent/` own stored-procedure access. `Program.BuildServices()` is the composition root; repositories share `StoredProcedureRunner`. Models carry clinic data. The app probes the database connection and never runs schema, migration or seed scripts.

`Helpers/Design/` owns tokens, theme, control adapters and the single `Animator`/`Motion` engine. `UiFactory`, `UiMessages`, `UiAction`, `GridHelper`, `InputRules` and `DisplayFormat` centralize UI construction, feedback, async state and formatting. `NavItem`/`PageFactory` centralize navigation and page construction. AntdUI controls sit behind `GlobalUsings.cs`, `ClinicTable`, `ClinicSelect`, `ClinicDatePicker`, `FieldBox` and notification/alert adapters. Custom GDI+ charts live in `Helpers/Charts/` and consume the same tokens and motion system.

## Reports

Reports are Admin-only. Four summary cards show billed amount, appointments, completion rate and treatment count above Appointments by status, Billed by day, Top treatment types and Dentist workload. The status legend includes all five statuses and zero counts when there are appointments; a completely empty range shows one empty state per section. The top eight treatment types appear in the chart alongside the fetched top-ten table. At the query limit, treatment count shows n/a because an exact total cannot be inferred. Zero-activity dentists remain visible in workload. Hover and keyboard focus provide chart summaries. Default dates are this month through today; This week, This month and Last 30 days segments refresh the range, and changing either date selects Custom. Date fields use picker-only entry.

**Billed amounts are not money collected.** Billed is the treatment net after discounts. A gross PHP 1,000 treatment with a 20% discount contributes PHP 800. Compare the daily total and reports with Treatments Net for the same date range.

Export CSV saves one UTF-8 file with a BOM and four title/header/data sections, comma separators and CRLF rows. Export uses exactly the last successful displayed snapshot and its dates, even if toolbar dates have changed or refresh failed. Numeric values are invariant raw decimals, with PHP units in headers; dates use yyyy-MM-dd. Text beginning with =, +, - or @ receives a leading apostrophe to neutralize spreadsheet formulas; numeric cells retain their values. Quotes, commas and newlines are escaped. Saving failure shows friendly feedback; export makes no extra service calls.

Local design/revision notes and the screenshot matrix are in ignored `docs/`. In Debug, open the style guide via Ctrl+Shift+F12 on the dashboard or the standalone --style-guide option above; use its Chart motion replay and reduced-motion toggle. If a running app locks the normal output, build with `dotnet build DentalClinicSystem/DentalClinicSystem.csproj -o DentalClinicSystem/bin/HandoffDebug`, then launch `DentalClinicSystem/bin/HandoffDebug/DentalClinicSystem.exe --style-guide`. Open `docs/UI_REVIEW.md` for the screen/state/motion checklist. The standalone guide does not require MySQL; real DPI, Designer loading, saving CSV and authenticated flows require human review.

Record pages support Ctrl+F for search, Ctrl+N for a new record, Ctrl+S to save, and Enter to move through single-line form inputs. Multiline fields keep Enter for new lines. Login submits with Enter. The existing Animator handles transitions, feedback and loading reveals; reduced motion shows the final state immediately. The optional collapsible sidebar remains deferred.

For local previews using test data, set `DENTAL_UI_SNAPSHOTS` to an output directory before running the UI suite, for example `$env:DENTAL_UI_SNAPSHOTS = Join-Path (Get-Location) 'artifacts/antdui-previews'` in PowerShell. The suite renders the actual controls at two window widths and captures record dialogs, dashboards, login and reports. `artifacts/` and previews in `docs/` remain local and ignored. Real MySQL flows, motion recordings, 125%/150% display scaling and Visual Studio Designer opening still need manual review. AntdUI attribution and its license are in `THIRD-PARTY-NOTICES.md` and `Licenses/AntdUI-LICENSE.txt`.
