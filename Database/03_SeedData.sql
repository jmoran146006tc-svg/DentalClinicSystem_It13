-- All treatment prices are placeholders; confirm real prices with the clinic.
-- DEMO accounts only. Never use these passwords for a real clinic.
USE dentalclinicdb;

INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber)
SELECT 'Maria', 'Santos', 'General Dentistry', '09171234567', 'PRC-00123' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Dentists WHERE LicenseNumber = 'PRC-00123');

INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber)
SELECT 'Carlos', 'Reyes', 'Orthodontics', '09181234567', 'PRC-00456' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Dentists WHERE LicenseNumber = 'PRC-00456');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Dental Cleaning', 800, 45 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Dental Cleaning');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Tooth Extraction', 1500, 45 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Tooth Extraction');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Root Canal', 6000, 90 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Root Canal');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Filling', 1200, 60 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Filling');

-- Placeholder prices and durations: confirm real prices and procedure times with the clinic.
INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Consultation / Check-up', 500, 30 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Consultation / Check-up');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Dental X-ray (Periapical)', 400, 15 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Dental X-ray (Periapical)');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Surgical Extraction / Wisdom Tooth', 5000, 90 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Surgical Extraction / Wisdom Tooth');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Teeth Whitening', 8000, 90 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Teeth Whitening');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Braces Adjustment', 1000, 30 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Braces Adjustment');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Dentures (per arch)', 10000, 60 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Dentures (per arch)');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Dental Crown', 8000, 60 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Dental Crown');

INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes)
SELECT 'Fluoride Treatment', 500, 30 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TreatmentTypes WHERE Name = 'Fluoride Treatment');

INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber)
SELECT 'Juan', 'Dela Cruz', '1998-04-12', '09981234567' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Patients WHERE FirstName = 'Juan' AND LastName = 'Dela Cruz' AND DateOfBirth = '1998-04-12');

INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber)
SELECT 'Ana', 'Lopez', '2001-09-03', '09991234567' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Patients WHERE FirstName = 'Ana' AND LastName = 'Lopez' AND DateOfBirth = '2001-09-03');

INSERT INTO Users (Username, PasswordHash, Role, DentistId)
SELECT 'admin', '$2a$11$hGGRSHay/lWHCtzAdvo.Xeng96/.tsU7ZAWHQBUpR9bLbgp4SY87G', 'Admin', NULL FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin');

INSERT INTO Users (Username, PasswordHash, Role, DentistId)
SELECT 'reception', '$2a$11$rLg3TYkiuBCfm7ySor6uAOVMHokAbNjhM8o0XbEBrQL.qbxpylhWy', 'Receptionist', NULL FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'reception');

INSERT INTO Users (Username, PasswordHash, Role, DentistId)
SELECT 'drsantos', '$2a$11$CqzW7tBTUEIjEWgJ0.tXp.W76Gx0xwofe.dQsFtl12Hgd2MYk7n3S', 'Dentist', (SELECT MIN(DentistId) FROM Dentists WHERE FirstName = 'Maria' AND LastName = 'Santos') FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'drsantos');

SET @seed_appointments = (SELECT COUNT(*) = 0 FROM Appointments);
-- MySQL stores temporal session variables as strings. Cast them back to DATE
-- where used so LEAST/date arithmetic do not depend on connection collations.
SET @week_start = DATE_SUB(CURDATE(), INTERVAL WEEKDAY(CURDATE()) DAY);
-- Keep the demo no-show on a past open day, even when seeded early in the week.
SET @previous_open_day = IF(DAYOFWEEK(CURDATE()) = 2, DATE_SUB(CURDATE(), INTERVAL 2 DAY), DATE_SUB(CURDATE(), INTERVAL 1 DAY));
SET @demo_day = IF(DAYOFWEEK(CURDATE()) = 1, DATE_ADD(CURDATE(), INTERVAL 1 DAY), CURDATE());
SET @juan = (SELECT MIN(PatientId) FROM Patients WHERE FirstName = 'Juan' AND LastName = 'Dela Cruz');
SET @ana = (SELECT MIN(PatientId) FROM Patients WHERE FirstName = 'Ana' AND LastName = 'Lopez');
SET @maria = (SELECT MIN(DentistId) FROM Dentists WHERE FirstName = 'Maria' AND LastName = 'Santos');
SET @carlos = (SELECT MIN(DentistId) FROM Dentists WHERE FirstName = 'Carlos' AND LastName = 'Reyes');

INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @juan, @maria, DATE_ADD(LEAST(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 0 DAY), CURDATE()), INTERVAL 9 HOUR), 45, 'Completed', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @ana, @carlos, DATE_ADD(LEAST(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 0 DAY), CURDATE()), INTERVAL 11 HOUR), 45, 'Completed', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @juan, @maria, DATE_ADD(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 1 DAY), INTERVAL 10 HOUR), 30, 'Cancelled', 'Demo consultation', 'Patient Rescheduled' FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @ana, @carlos, DATE_ADD(LEAST(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 2 DAY), CAST(@previous_open_day AS DATE)), INTERVAL 14 HOUR), 30, 'NoShow', 'Demo consultation', 'No Show' FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @juan, @maria, DATE_ADD(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 3 DAY), INTERVAL 9 HOUR), 30, 'Scheduled', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @ana, @carlos, DATE_ADD(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 4 DAY), INTERVAL 10 HOUR), 30, 'Scheduled', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @juan, @maria, DATE_ADD(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 5 DAY), INTERVAL 11 HOUR), 30, 'Scheduled', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @ana, @carlos, DATE_ADD(DATE_ADD(CAST(@week_start AS DATE), INTERVAL 5 DAY), INTERVAL 16 HOUR), 30, 'Scheduled', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @juan, @maria, DATE_ADD(CAST(@demo_day AS DATE), INTERVAL 14 HOUR), 30, 'Scheduled', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;
INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason)
SELECT @ana, @carlos, DATE_ADD(CAST(@demo_day AS DATE), INTERVAL 15 HOUR), 30, 'Scheduled', 'Demo consultation', NULL FROM DUAL WHERE @seed_appointments;

INSERT INTO Treatments (AppointmentId, TreatmentTypeId, Cost, DatePerformed, Notes)
SELECT a.AppointmentId, tt.TreatmentTypeId, tt.DefaultCost, DATE(a.AppointmentDateTime), 'Demo treatment'
FROM Appointments a JOIN TreatmentTypes tt ON tt.Name = 'Dental Cleaning'
WHERE @seed_appointments AND a.Status = 'Completed'
AND NOT EXISTS (SELECT 1 FROM Treatments t WHERE t.AppointmentId = a.AppointmentId);
