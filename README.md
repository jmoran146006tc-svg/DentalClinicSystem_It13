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

For an existing database, run `Database/04_Migration_RealWorldFixes.sql`, then `Database/02_StoredProcedures.sql`, then `Database/03_SeedData.sql` (04 → 02 → 03). The migration checks existing columns and constraints, keeps existing data, and removes its temporary helper procedures. It is designed to be re-runnable. Run it twice on a backup when verifying an upgrade. Fresh installs use 01 → 02 → 03; these scripts are also designed to be re-runnable.

**`Database/00_ResetDatabase.sql` destroys all data.** It is not part of an upgrade. Back up anything needed before using it to rebuild a schema. The app never creates or migrates the database.

DEMO ONLY: `admin / admin123`, `reception / reception123`, `drsantos / dentist123`.

Login centers a frosted glass card and one overlapping brand badge over a bright full-bleed clinic photo. The embedded `Resources/dentist.jpg` determines the window aspect ratio; photo, luminous edge gradients and 18 floating glass shapes, rings, a tooth outline, sparkles and accents are cached by size and DPI. The stronger three-pass blur captures the gradients before the crisp shapes and receives a 94% white wash for smooth frosting. Cached text uses grayscale antialiasing against an opaque sampled tone. The card and badge are centered together, with a 24 px password-to-button gap. A time-of-day greeting, username/password icons, visible password eye (also Alt+P), Enter to submit, and one generic inline error preserve the sign-in flow. The form fades over 150 ms, the card rises/fades over 320 ms, and the badge/field rows follow at 30 ms intervals. The decorative layers fade in over 320 ms after a 30 ms delay. Reduced motion shows the final state instantly.

Login photo by Benyamin Bohlouli on Unsplash.

The sidebar follows each account's role permissions, highlights the active page, and keeps the section title and clock visible during navigation. Record pages use the shared design system and responsive runtime layouts. Appointments support walk-ins, check-in, rescheduling with a dentist change, editable visit reasons and durations, overlap checks, and dentist time off. Admins and Receptionists can check in patients; Dentists can complete their own appointments. Only Scheduled appointments can be rescheduled or cancelled; CheckedIn appointments can only become Completed.

The dashboard shows Admins and Receptionists today's appointment count, active patients, cancellations for appointments dated this week, and a Monday–Sunday calendar with week navigation, dentist filtering, overlap columns and appointment details. Only Admins see the weekly net billed amount. Dentists see their own patients today, with patient history and completion actions for Scheduled or CheckedIn appointments. History is read-only and includes treatments and past appointments; Receptionists have no treatment-history access. Refresh reloads dashboard data. The calendar displays 8:00 AM–6:00 PM for context; booking still enforces the clinic hours below.

Patients have guardian contact fields (required below age 18), allergies and medical notes. Duplicate names plus birthdates are rejected, and booking a valid slot for an inactive patient reactivates the record. Medical details appear in appointment details for Admins and Dentists; Receptionists can view them on the Patients page.

Treatments support Add and Update for eligible appointments through today, require the appointment's date, and validate optional FDI tooth numbers (permanent and primary teeth). Cost is the gross amount. None, Senior Citizen, PWD and Other discounts calculate a net amount; reports show **Billed**, which is not money collected. Net is calculated as `Cost - Cost * DiscountPercent / 100`; displayed currency uses two decimal places. Admin reports total net billed amounts.

## Clinic assumptions

`DentalClinicSystem/Service/ClinicRules.cs` is the single place to change the assumed Monday–Saturday, 9:00 AM–5:00 PM clinic hours, Sunday closure, 15-minute past grace, and duration rules (30 minutes by default; 15–240 minutes in 15-minute steps). Senior Citizen and PWD choices default to 20% in that file. Confirm hours, discount handling/eligibility and actual prices with the clinic before real use; the catalog prices are placeholders. Catalog duration suggestions live in TreatmentTypes; initial seed durations are 45 minutes for cleaning/extraction, 90 for root canal, and 60 for filling. FDI validation lives in `Service/ToothNumbering.cs`.

Date-dependent services accept an optional `TimeProvider`; production uses the machine's local time. Configure the clinic computer for Philippine time (UTC+08:00).

## Known limitations

- No payment or collection tracking; Billed does not mean paid.
- No medical history beyond allergies and notes.
- One clinic-hours configuration, not a separate schedule per dentist.
- No SMS reminders.
- Booking, leave and deactivation checks happen before writes and do not serialize concurrent users. Patient reactivation and booking are separate stored-procedure calls; a failed booking write may leave the patient active.
- Automated tests use repository/service fakes. Live MySQL script execution and end-to-end database operations need manual verification.

## Manual MySQL verification

1. On a backup of an existing database, run 04 twice, then 02 and 03 twice; check preserved records, new columns/constraints and absence of migration helper procedures. On a separate fresh database, run 01 → 02 → 03, then repeat them.
2. Book a walk-in during clinic hours, check in as Receptionist, and complete as Admin or the linked Dentist. Check that terminal statuses cannot be reopened.
3. Add a treatment to a completed appointment. Try a cancelled/no-show appointment and a future appointment; verify rejection and performed-date locking.
4. Reschedule a Scheduled appointment with a dentist change; verify unchanged reason/notes/duration, self-exclusion, row refresh and flash. Try an adjacent slot and a one-minute overlap with a long procedure.
5. Add dentist time off, then try booking in the range. Try adding leave over Scheduled/CheckedIn appointments and removing leave. Check clinic opening, closing and Sunday boundaries.
6. Try deactivating a dentist with an upcoming appointment, then reschedule/cancel it and retry. Verify that their linked Dentist account cannot sign in while inactive.
7. Book an inactive patient into a valid slot and verify reactivation. Try an invalid slot and confirm the patient remains inactive.
8. Register a minor without guardian details, then with valid details. Check the 18th-birthday boundary and active/inactive duplicates.
9. Add a gross 1,000 treatment with a 20% discount. Verify Net and Billed equal 800 in daily, top-treatment and dentist reports; verify editing, None and Other discounts too.

In Debug, press **Ctrl+Shift+F12** on the dashboard to open the style guide. To review it without a database, run `dotnet run --project DentalClinicSystem/DentalClinicSystem.csproj -- --style-guide`. The guide is not included in Release.

The UI uses **AntdUI 2.4.12** (Apache-2.0) with a shared light palette and teal accent. Buttons, navigation actions, text/password inputs, choices, date/time pickers, discount inputs, record/report tables, status tags, checkboxes, inline alerts, toast notifications and report tabs use AntdUI. Shared adapters retain the clinic's existing model IDs, date limits, required selections, validation, busy guards and read-only locks. The appointment week calendar, login artwork, dashboard cards and page layouts remain clinic-specific.

The appointment date/time editor uses a 24-hour clock so AntdUI exposes both hour and minute selection. Read-only tables retain the clinic's existing 12-hour display. The build and publish output include the third-party notice and AntdUI license.

Existing Designer layouts are retained. Runtime layout methods move the original controls into new containers, so the Designer view shows the legacy layout. The dashboard, calendar and history are built in code. `GlobalUsings.cs` maps common control names to AntdUI and the clinic adapters, allowing event handlers to stay focused on clinic behavior.

Run the included UI regression suite with `dotnet test tests/DentalClinicSystem.UiTests/DentalClinicSystem.UiTests.csproj -c Release`, or run `dotnet test DentalClinicSystem.slnx -c Release`. It uses sample data and service fakes, never MySQL, and covers record selection/search, dialogs, role navigation, choices/IDs, required dates, input filtering, table sorting and report tabs. Older optional tests in the ignored `DentalClinicSystem.Tests/` folder may require updates for the former standard WinForms control types. Build and unit checks do not verify live MySQL operations or display scaling.

Record pages support Ctrl+F for search, Ctrl+N for a new record, Ctrl+S to save, and Enter to move through single-line form inputs. Multiline fields keep Enter for new lines. Login submits with Enter. The existing Animator handles transitions, feedback and loading reveals; reduced motion shows the final state immediately. The optional collapsible sidebar remains deferred.

For local previews using test data, set `DENTAL_UI_SNAPSHOTS` to an output directory before running the UI suite, for example `$env:DENTAL_UI_SNAPSHOTS = Join-Path (Get-Location) 'artifacts/antdui-previews'` in PowerShell. The suite renders the actual controls at two window widths and captures record dialogs, dashboards, login and reports. `artifacts/` and previews in `docs/` remain local and ignored. Real MySQL flows, motion recordings, 125%/150% display scaling and Visual Studio Designer opening still need manual review. AntdUI attribution and its license are in `THIRD-PARTY-NOTICES.md` and `Licenses/AntdUI-LICENSE.txt`.
