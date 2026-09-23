-- StoredProcedures.sql
-- Manual-run / Workbench reference version of DatabaseInitializer's stored
-- procedures. The app creates/updates all of these automatically on startup - you
-- do not need to run this by hand unless you want to inspect them directly.
-- Run Schema.sql first if you're setting up by hand instead of just launching the app.

USE dentalclinicdb;

DELIMITER $$


DROP PROCEDURE IF EXISTS sp_Patient_GetAll $$
CREATE PROCEDURE sp_Patient_GetAll()
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, IsActive, CreatedAt
    FROM Patients
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_GetById $$
CREATE PROCEDURE sp_Patient_GetById(IN p_PatientId INT)
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, IsActive, CreatedAt
    FROM Patients
    WHERE PatientId = p_PatientId;
END $$

DROP PROCEDURE IF EXISTS sp_Patient_Add $$
CREATE PROCEDURE sp_Patient_Add(
    IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200))
BEGIN
    INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber, Email, Address)
    VALUES (p_FirstName, p_LastName, p_DateOfBirth, p_ContactNumber, p_Email, p_Address);
END $$

DROP PROCEDURE IF EXISTS sp_Patient_Update $$
CREATE PROCEDURE sp_Patient_Update(
    IN p_PatientId INT, IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200))
BEGIN
    UPDATE Patients
    SET FirstName = p_FirstName, LastName = p_LastName, DateOfBirth = p_DateOfBirth,
        ContactNumber = p_ContactNumber, Email = p_Email, Address = p_Address
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
    SELECT TreatmentTypeId, Name, DefaultCost, Description
    FROM TreatmentTypes
    ORDER BY Name;
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_GetById $$
CREATE PROCEDURE sp_TreatmentType_GetById(IN p_TreatmentTypeId INT)
BEGIN
    SELECT TreatmentTypeId, Name, DefaultCost, Description
    FROM TreatmentTypes
    WHERE TreatmentTypeId = p_TreatmentTypeId;
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_Add $$
CREATE PROCEDURE sp_TreatmentType_Add(IN p_Name VARCHAR(100), IN p_DefaultCost DECIMAL(10,2), IN p_Description VARCHAR(255))
BEGIN
    INSERT INTO TreatmentTypes (Name, DefaultCost, Description) VALUES (p_Name, p_DefaultCost, p_Description);
END $$

DROP PROCEDURE IF EXISTS sp_TreatmentType_Update $$
CREATE PROCEDURE sp_TreatmentType_Update(IN p_TreatmentTypeId INT, IN p_Name VARCHAR(100), IN p_DefaultCost DECIMAL(10,2), IN p_Description VARCHAR(255))
BEGIN
    UPDATE TreatmentTypes SET Name = p_Name, DefaultCost = p_DefaultCost, Description = p_Description
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
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, Status, Reason, CreatedAt
    FROM Appointments
    ORDER BY AppointmentDateTime;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_GetById $$
CREATE PROCEDURE sp_Appointment_GetById(IN p_AppointmentId INT)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, Status, Reason, CreatedAt
    FROM Appointments
    WHERE AppointmentId = p_AppointmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_GetByDentistAndDate $$
CREATE PROCEDURE sp_Appointment_GetByDentistAndDate(IN p_DentistId INT, IN p_Date DATE)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, Status, Reason, CreatedAt
    FROM Appointments
    WHERE DentistId = p_DentistId
      AND AppointmentDateTime >= p_Date
      AND AppointmentDateTime < DATE_ADD(p_Date, INTERVAL 1 DAY)
      AND Status != 'Cancelled';
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_Add $$
CREATE PROCEDURE sp_Appointment_Add(
    IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME,
    IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255))
BEGIN
    INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, Status, Reason)
    VALUES (p_PatientId, p_DentistId, p_AppointmentDateTime, p_Status, p_Reason);
END $$

DROP PROCEDURE IF EXISTS sp_Appointment_Update $$
CREATE PROCEDURE sp_Appointment_Update(
    IN p_AppointmentId INT, IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME,
    IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255))
BEGIN
    UPDATE Appointments
    SET PatientId = p_PatientId, DentistId = p_DentistId, AppointmentDateTime = p_AppointmentDateTime,
        Status = p_Status, Reason = p_Reason
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
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes
    FROM Treatments
    ORDER BY DatePerformed DESC;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_GetById $$
CREATE PROCEDURE sp_Treatment_GetById(IN p_TreatmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes
    FROM Treatments
    WHERE TreatmentId = p_TreatmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_GetByAppointmentId $$
CREATE PROCEDURE sp_Treatment_GetByAppointmentId(IN p_AppointmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes
    FROM Treatments
    WHERE AppointmentId = p_AppointmentId;
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_Add $$
CREATE PROCEDURE sp_Treatment_Add(
    IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    INSERT INTO Treatments (AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes)
    VALUES (p_AppointmentId, p_TreatmentTypeId, p_ToothNumber, p_Cost, p_DatePerformed, p_Notes);
END $$

DROP PROCEDURE IF EXISTS sp_Treatment_Update $$
CREATE PROCEDURE sp_Treatment_Update(
    IN p_TreatmentId INT, IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    UPDATE Treatments
    SET AppointmentId = p_AppointmentId, TreatmentTypeId = p_TreatmentTypeId, ToothNumber = p_ToothNumber,
        Cost = p_Cost, DatePerformed = p_DatePerformed, Notes = p_Notes
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

DELIMITER ;
