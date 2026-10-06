USE dentalclinicdb;

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Patient_GetAll $$
CREATE PROCEDURE sp_Patient_GetAll()
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt
    FROM Patients
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_GetById $$
CREATE PROCEDURE sp_Patient_GetById(IN p_PatientId INT)
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt
    FROM Patients
    WHERE PatientId = p_PatientId;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_Add $$
CREATE PROCEDURE sp_Patient_Add(
    IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200),
    IN p_GuardianName VARCHAR(100), IN p_GuardianContact VARCHAR(20), IN p_Allergies VARCHAR(255), IN p_MedicalNotes VARCHAR(500))
BEGIN
    INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, GuardianName, GuardianContact, Allergies, MedicalNotes)
    VALUES (p_FirstName, p_LastName, p_DateOfBirth, p_ContactNumber, p_Email, p_Address, p_GuardianName, p_GuardianContact, p_Allergies, p_MedicalNotes);
END $$

DROP PROCEDURE IF EXISTS sp_Patient_Update $$
CREATE PROCEDURE sp_Patient_Update(
    IN p_PatientId INT, IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200),
    IN p_GuardianName VARCHAR(100), IN p_GuardianContact VARCHAR(20), IN p_Allergies VARCHAR(255), IN p_MedicalNotes VARCHAR(500))
BEGIN
    UPDATE Patients
    SET FirstName = p_FirstName, LastName = p_LastName, DateOfBirth = p_DateOfBirth,
        ContactNumber = p_ContactNumber, Email = p_Email, Address = p_Address, GuardianName = p_GuardianName, GuardianContact = p_GuardianContact,
        Allergies = p_Allergies, MedicalNotes = p_MedicalNotes
    WHERE PatientId = p_PatientId;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_Delete $$
CREATE PROCEDURE sp_Patient_Delete(IN p_PatientId INT)
BEGIN
    UPDATE Patients SET IsActive = 0 WHERE PatientId = p_PatientId;
END $$

DROP PROCEDURE IF EXISTS sp_Dentist_GetAll $$
CREATE PROCEDURE sp_Dentist_GetAll()
BEGIN
    SELECT DentistId, FirstName, LastName, Specialization, ContactNumber, LicenseNumber, IsActive
    FROM Dentists
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END $$

DROP PROCEDURE IF EXISTS sp_Dentist_GetById $$
CREATE PROCEDURE sp_Dentist_GetById(IN p_DentistId INT)
BEGIN
    SELECT DentistId, FirstName, LastName, Specialization, ContactNumber, LicenseNumber, IsActive
    FROM Dentists
    WHERE DentistId = p_DentistId;
END $$

DROP PROCEDURE IF EXISTS sp_Dentist_Add $$
CREATE PROCEDURE sp_Dentist_Add(
    IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_Specialization VARCHAR(100),
    IN p_ContactNumber VARCHAR(20), IN p_LicenseNumber VARCHAR(50))
BEGIN
    INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber)
    VALUES (p_FirstName, p_LastName, p_Specialization, p_ContactNumber, p_LicenseNumber);
END $$

DROP PROCEDURE IF EXISTS sp_Dentist_Update $$
CREATE PROCEDURE sp_Dentist_Update(
    IN p_DentistId INT, IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_Specialization VARCHAR(100),
    IN p_ContactNumber VARCHAR(20), IN p_LicenseNumber VARCHAR(50))
BEGIN
    UPDATE Dentists
    SET FirstName = p_FirstName, LastName = p_LastName, Specialization = p_Specialization,
        ContactNumber = p_ContactNumber, LicenseNumber = p_LicenseNumber
    WHERE DentistId = p_DentistId;
END $$

DROP PROCEDURE IF EXISTS sp_Dentist_Delete $$
CREATE PROCEDURE sp_Dentist_Delete(IN p_DentistId INT)
BEGIN
    UPDATE Dentists SET IsActive = 0 WHERE DentistId = p_DentistId;
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_GetAll $$
CREATE PROCEDURE sp_TreatmentType_GetAll()
BEGIN
    SELECT TreatmentTypeId, Name, DefaultCost, DefaultDurationMinutes, Description
    FROM TreatmentTypes
    ORDER BY Name;
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_GetById $$
CREATE PROCEDURE sp_TreatmentType_GetById(IN p_TreatmentTypeId INT)
BEGIN
    SELECT TreatmentTypeId, Name, DefaultCost, DefaultDurationMinutes, Description
    FROM TreatmentTypes
    WHERE TreatmentTypeId = p_TreatmentTypeId;
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_Add $$
CREATE PROCEDURE sp_TreatmentType_Add(IN p_Name VARCHAR(100), IN p_DefaultCost DECIMAL(10,2), IN p_DefaultDurationMinutes INT, IN p_Description VARCHAR(255))
BEGIN
    INSERT INTO TreatmentTypes (Name, DefaultCost, DefaultDurationMinutes, Description) VALUES (p_Name, p_DefaultCost, p_DefaultDurationMinutes, p_Description);
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_Update $$
CREATE PROCEDURE sp_TreatmentType_Update(IN p_TreatmentTypeId INT, IN p_Name VARCHAR(100), IN p_DefaultCost DECIMAL(10,2), IN p_DefaultDurationMinutes INT, IN p_Description VARCHAR(255))
BEGIN
    UPDATE TreatmentTypes SET Name = p_Name, DefaultCost = p_DefaultCost, DefaultDurationMinutes = p_DefaultDurationMinutes, Description = p_Description
    WHERE TreatmentTypeId = p_TreatmentTypeId;
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_Delete $$
CREATE PROCEDURE sp_TreatmentType_Delete(IN p_TreatmentTypeId INT)
BEGIN
    DELETE FROM TreatmentTypes WHERE TreatmentTypeId = p_TreatmentTypeId;
END $$

-- ===================== Appointments =====================

DROP PROCEDURE IF EXISTS sp_Appointment_GetAll $$
CREATE PROCEDURE sp_Appointment_GetAll()
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    ORDER BY AppointmentDateTime;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_GetById $$
CREATE PROCEDURE sp_Appointment_GetById(IN p_AppointmentId INT)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    WHERE AppointmentId = p_AppointmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_GetByDentistAndDate $$
CREATE PROCEDURE sp_Appointment_GetByDentistAndDate(IN p_DentistId INT, IN p_Date DATE)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    WHERE DentistId = p_DentistId
      AND AppointmentDateTime >= p_Date
      AND AppointmentDateTime < DATE_ADD(p_Date, INTERVAL 1 DAY)
      AND Status != 'Cancelled';
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_Add $$
CREATE PROCEDURE sp_Appointment_Add(
    IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME, IN p_DurationMinutes INT,
    IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255), IN p_CancellationReason VARCHAR(255), IN p_Notes VARCHAR(500))
BEGIN
    INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes)
    VALUES (p_PatientId, p_DentistId, p_AppointmentDateTime, p_DurationMinutes, p_Status, p_Reason, p_CancellationReason, p_Notes);
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_Update $$
CREATE PROCEDURE sp_Appointment_Update(
    IN p_AppointmentId INT, IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME, IN p_DurationMinutes INT,
    IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255), IN p_CancellationReason VARCHAR(255), IN p_Notes VARCHAR(500))
BEGIN
    UPDATE Appointments
    SET PatientId = p_PatientId, DentistId = p_DentistId, AppointmentDateTime = p_AppointmentDateTime, DurationMinutes = p_DurationMinutes,
        Status = p_Status, Reason = p_Reason, CancellationReason = p_CancellationReason, Notes = p_Notes
    WHERE AppointmentId = p_AppointmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_Delete $$
CREATE PROCEDURE sp_Appointment_Delete(IN p_AppointmentId INT)
BEGIN
    DELETE FROM Appointments WHERE AppointmentId = p_AppointmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_GetAll $$
CREATE PROCEDURE sp_Treatment_GetAll()
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DiscountType, DiscountPercent, DatePerformed, Notes
    FROM Treatments
    ORDER BY DatePerformed DESC;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_GetById $$
CREATE PROCEDURE sp_Treatment_GetById(IN p_TreatmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DiscountType, DiscountPercent, DatePerformed, Notes
    FROM Treatments
    WHERE TreatmentId = p_TreatmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_GetByAppointmentId $$
CREATE PROCEDURE sp_Treatment_GetByAppointmentId(IN p_AppointmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DiscountType, DiscountPercent, DatePerformed, Notes
    FROM Treatments
    WHERE AppointmentId = p_AppointmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_Add $$
CREATE PROCEDURE sp_Treatment_Add(
    IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DiscountType VARCHAR(20), IN p_DiscountPercent DECIMAL(5,2), IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    INSERT INTO Treatments (AppointmentId, TreatmentTypeId, ToothNumber, Cost, DiscountType, DiscountPercent, DatePerformed, Notes)
    VALUES (p_AppointmentId, p_TreatmentTypeId, p_ToothNumber, p_Cost, p_DiscountType, p_DiscountPercent, p_DatePerformed, p_Notes);
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_Update $$
CREATE PROCEDURE sp_Treatment_Update(
    IN p_TreatmentId INT, IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DiscountType VARCHAR(20), IN p_DiscountPercent DECIMAL(5,2), IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    UPDATE Treatments
    SET AppointmentId = p_AppointmentId, TreatmentTypeId = p_TreatmentTypeId, ToothNumber = p_ToothNumber,
        Cost = p_Cost, DiscountType = p_DiscountType, DiscountPercent = p_DiscountPercent, DatePerformed = p_DatePerformed, Notes = p_Notes
    WHERE TreatmentId = p_TreatmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_Delete $$
CREATE PROCEDURE sp_Treatment_Delete(IN p_TreatmentId INT)
BEGIN
    DELETE FROM Treatments WHERE TreatmentId = p_TreatmentId;
END $$

DROP PROCEDURE IF EXISTS sp_User_GetAll $$
CREATE PROCEDURE sp_User_GetAll()
BEGIN
    SELECT UserId, Username, PasswordHash, Role, DentistId, IsActive
    FROM Users
    ORDER BY Username;
END $$

DROP PROCEDURE IF EXISTS sp_User_GetById $$
CREATE PROCEDURE sp_User_GetById(IN p_UserId INT)
BEGIN
    SELECT UserId, Username, PasswordHash, Role, DentistId, IsActive
    FROM Users
    WHERE UserId = p_UserId;
END $$

DROP PROCEDURE IF EXISTS sp_User_GetByUsername $$
CREATE PROCEDURE sp_User_GetByUsername(IN p_Username VARCHAR(50))
BEGIN
    SELECT UserId, Username, PasswordHash, Role, DentistId, IsActive
    FROM Users
    WHERE Username = p_Username;
END $$

DROP PROCEDURE IF EXISTS sp_User_Add $$
CREATE PROCEDURE sp_User_Add(
    IN p_Username VARCHAR(50), IN p_PasswordHash VARCHAR(255), IN p_Role VARCHAR(20),
    IN p_DentistId INT, IN p_IsActive TINYINT(1))
BEGIN
    INSERT INTO Users (Username, PasswordHash, Role, DentistId, IsActive)
    VALUES (p_Username, p_PasswordHash, p_Role, p_DentistId, p_IsActive);
END $$

DROP PROCEDURE IF EXISTS sp_User_Update $$
CREATE PROCEDURE sp_User_Update(
    IN p_UserId INT, IN p_Username VARCHAR(50), IN p_PasswordHash VARCHAR(255), IN p_Role VARCHAR(20),
    IN p_DentistId INT, IN p_IsActive TINYINT(1))
BEGIN
    UPDATE Users
    SET Username = p_Username, PasswordHash = p_PasswordHash, Role = p_Role,
        DentistId = p_DentistId, IsActive = p_IsActive
    WHERE UserId = p_UserId;
END $$


DROP PROCEDURE IF EXISTS sp_Patient_GetAllIncludingInactive $$
CREATE PROCEDURE sp_Patient_GetAllIncludingInactive()
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt FROM Patients ORDER BY LastName, FirstName;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_FindByNameAndDob $$
CREATE PROCEDURE sp_Patient_FindByNameAndDob(IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE)
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt FROM Patients
    WHERE FirstName = p_FirstName AND LastName = p_LastName AND DateOfBirth = p_DateOfBirth
    ORDER BY IsActive DESC, PatientId;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_Reactivate $$
CREATE PROCEDURE sp_Patient_Reactivate(IN p_PatientId INT)
BEGIN
    UPDATE Patients SET IsActive = 1 WHERE PatientId = p_PatientId;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_DeactivateStale $$
CREATE PROCEDURE sp_Patient_DeactivateStale()
BEGIN
    -- Patients with only cancelled appointments use their creation date.
    UPDATE Patients p
    LEFT JOIN (SELECT PatientId, MAX(AppointmentDateTime) AS LastVisit FROM Appointments
               WHERE Status <> 'Cancelled' GROUP BY PatientId) a ON a.PatientId = p.PatientId
    SET p.IsActive = 0
    WHERE p.IsActive = 1 AND COALESCE(a.LastVisit, p.CreatedAt) < DATE_SUB(NOW(), INTERVAL 365 DAY);
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_GetByPatientId $$
CREATE PROCEDURE sp_Appointment_GetByPatientId(IN p_PatientId INT)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes, CreatedAt FROM Appointments WHERE PatientId = p_PatientId ORDER BY AppointmentDateTime DESC;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_GetByDentistAndRange $$
CREATE PROCEDURE sp_Appointment_GetByDentistAndRange(IN p_DentistId INT, IN p_From DATETIME, IN p_To DATETIME)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes, CreatedAt FROM Appointments WHERE DentistId = p_DentistId AND AppointmentDateTime >= p_From AND AppointmentDateTime < p_To ORDER BY AppointmentDateTime;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_GetByRange $$
CREATE PROCEDURE sp_Appointment_GetByRange(IN p_From DATETIME, IN p_To DATETIME)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes, CreatedAt FROM Appointments WHERE AppointmentDateTime >= p_From AND AppointmentDateTime < p_To ORDER BY AppointmentDateTime;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_GetByPatientId $$
CREATE PROCEDURE sp_Treatment_GetByPatientId(IN p_PatientId INT)
BEGIN
    SELECT t.TreatmentId, t.AppointmentId, t.TreatmentTypeId, t.ToothNumber, t.Cost, t.DiscountType, t.DiscountPercent, t.DatePerformed, t.Notes FROM Treatments t JOIN Appointments a ON a.AppointmentId = t.AppointmentId WHERE a.PatientId = p_PatientId ORDER BY t.DatePerformed DESC;
END $$

DROP PROCEDURE IF EXISTS sp_Report_AppointmentsByStatus $$
CREATE PROCEDURE sp_Report_AppointmentsByStatus(IN p_From DATE, IN p_To DATE)
BEGIN
    SELECT Status, COUNT(*) AS Total FROM Appointments
    WHERE AppointmentDateTime >= p_From AND AppointmentDateTime < DATE_ADD(p_To, INTERVAL 1 DAY)
    GROUP BY Status ORDER BY Status;
END $$

DROP PROCEDURE IF EXISTS sp_Report_RevenueByDay $$
CREATE PROCEDURE sp_Report_RevenueByDay(IN p_From DATE, IN p_To DATE)
BEGIN
    SELECT DatePerformed AS Day, SUM(fn_TreatmentNet(Cost, DiscountPercent)) AS Billed FROM Treatments
    WHERE DatePerformed BETWEEN p_From AND p_To GROUP BY DatePerformed ORDER BY DatePerformed;
END $$

DROP PROCEDURE IF EXISTS sp_Report_TopTreatmentTypes $$
CREATE PROCEDURE sp_Report_TopTreatmentTypes(IN p_From DATE, IN p_To DATE, IN p_Top INT)
BEGIN
    SELECT tt.Name, COUNT(*) AS Total, SUM(fn_TreatmentNet(t.Cost, t.DiscountPercent)) AS Billed FROM Treatments t
    JOIN TreatmentTypes tt ON tt.TreatmentTypeId = t.TreatmentTypeId
    WHERE t.DatePerformed BETWEEN p_From AND p_To
    GROUP BY tt.TreatmentTypeId, tt.Name ORDER BY Total DESC, Billed DESC, tt.Name LIMIT p_Top;
END $$

DROP PROCEDURE IF EXISTS sp_Report_DentistWorkload $$
CREATE PROCEDURE sp_Report_DentistWorkload(IN p_From DATE, IN p_To DATE)
BEGIN
    -- Separate aggregates avoid multiplying appointments with several treatments.
    SELECT CONCAT('Dr. ', d.FirstName, ' ', d.LastName) AS Dentist,
           COALESCE(a.Total, 0) AS Total, COALESCE(a.Completed, 0) AS Completed,
           COALESCE(t.Billed, 0) AS Billed
    FROM Dentists d
    LEFT JOIN (SELECT DentistId, COUNT(*) AS Total, SUM(Status = 'Completed') AS Completed
               FROM Appointments WHERE AppointmentDateTime >= p_From AND AppointmentDateTime < DATE_ADD(p_To, INTERVAL 1 DAY)
               GROUP BY DentistId) a ON a.DentistId = d.DentistId
    LEFT JOIN (SELECT ap.DentistId, SUM(fn_TreatmentNet(tr.Cost, tr.DiscountPercent)) AS Billed FROM Treatments tr
               JOIN Appointments ap ON ap.AppointmentId = tr.AppointmentId
               WHERE tr.DatePerformed BETWEEN p_From AND p_To GROUP BY ap.DentistId) t ON t.DentistId = d.DentistId
    ORDER BY d.LastName, d.FirstName;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_CountUpcomingByDentist $$
CREATE PROCEDURE sp_Appointment_CountUpcomingByDentist(IN p_DentistId INT, IN p_From DATETIME)
BEGIN
    SELECT COUNT(*) AS Total FROM Appointments
    WHERE DentistId = p_DentistId AND AppointmentDateTime >= p_From AND Status IN ('Scheduled','CheckedIn');
END $$

DROP PROCEDURE IF EXISTS sp_DentistTimeOff_GetByDentist $$
CREATE PROCEDURE sp_DentistTimeOff_GetByDentist(IN p_DentistId INT)
BEGIN
    SELECT TimeOffId, DentistId, StartDate, EndDate, Reason FROM DentistTimeOff WHERE DentistId = p_DentistId ORDER BY StartDate, EndDate;
END $$
DROP PROCEDURE IF EXISTS sp_DentistTimeOff_GetOverlapping $$
CREATE PROCEDURE sp_DentistTimeOff_GetOverlapping(IN p_DentistId INT, IN p_From DATE, IN p_To DATE)
BEGIN
    SELECT TimeOffId, DentistId, StartDate, EndDate, Reason FROM DentistTimeOff
    WHERE DentistId = p_DentistId AND StartDate <= p_To AND EndDate >= p_From ORDER BY StartDate;
END $$
DROP PROCEDURE IF EXISTS sp_DentistTimeOff_Add $$
CREATE PROCEDURE sp_DentistTimeOff_Add(IN p_DentistId INT, IN p_StartDate DATE, IN p_EndDate DATE, IN p_Reason VARCHAR(100))
BEGIN
    INSERT INTO DentistTimeOff (DentistId, StartDate, EndDate, Reason) VALUES (p_DentistId, p_StartDate, p_EndDate, p_Reason);
END $$
DROP PROCEDURE IF EXISTS sp_DentistTimeOff_Delete $$
CREATE PROCEDURE sp_DentistTimeOff_Delete(IN p_TimeOffId INT)
BEGIN
    DELETE FROM DentistTimeOff WHERE TimeOffId = p_TimeOffId;
END $$

DROP EVENT IF EXISTS ev_deactivate_stale_patients $$
CREATE EVENT ev_deactivate_stale_patients
ON SCHEDULE EVERY 1 DAY STARTS CURRENT_TIMESTAMP + INTERVAL 1 DAY
DO CALL sp_Patient_DeactivateStale() $$
DELIMITER ;

-- The event scheduler must be ON for the daily event to run (requires DBA permission).
-- SET GLOBAL event_scheduler = ON;
-- Manual alternative: CALL sp_Patient_DeactivateStale();
