# Revision notes

## Assumptions

- Work is local on `ui-redesign`; the repository's original branch is `master`. No push or changes to that branch.
- Existing Designer files and embedded image resources are retained. New UI is built in code.
- The explicit xUnit requirement in Phase 11 permits the test SDK and xUnit test packages despite the general package restriction.
- Runtime GUI verification is reserved for humans, as requested. Build success is not runtime proof.

## Changes by phase

### Phase 0 - orientation and baseline

- Read the application layers, UI source/Designer layouts, resource entries, solution, project and README.
- Baseline `dotnet build DentalClinicSystem/DentalClinicSystem.csproj`: succeeded, 0 errors, 17 warnings (16 CS8622 event sender nullability warnings and one CS0108 on ServiceResult<T>.Fail).
- The reported txtReason compile failure is already fixed in this checkout. Duplicate status entries, logout lifecycle, dead event handlers and automatic database setup remain.

## Unverified at runtime

- [ ] Run manual database scripts against a disposable MySQL database and rerun to check idempotence.
- [ ] Exercise each role, sidebar, validation and privacy boundaries.
- [ ] Check scheduling, cancellation, completion, history, calendar and reports.
- [ ] Check inactive patients and reactivation.
- [ ] Check logout, window close, resizing and Visual Studio Designer compatibility.

## Open questions for the adviser

- Is a separate appointment Notes field wanted?
- Is 365 days the correct inactivity threshold?
- May staff reactivate inactive patients?
- Is a daily MySQL event acceptable for automatic deactivation?

## Principles audit results

Pending Phase 13.
