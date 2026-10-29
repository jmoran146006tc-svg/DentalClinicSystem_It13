-- =====================================================================
-- 001_schema_and_procedures_update.sql
-- Target : MariaDB 10.4.x  (uses ADD COLUMN IF NOT EXISTS)
-- Purpose: bring dentalclinicdb up to date with the C# code on branch
--          AntdUI-Integration-Testing
-- Safe to re-run (idempotent). BACKUP THE DATABASE FIRST.
-- =====================================================================

USE `dentalclinicdb`;

-- ---------------------------------------------------------------------
-- PART 1: Add missing columns
-- ---------------------------------------------------------------------
ALTER TABLE Patients
    ADD COLUMN IF NOT EXISTS GuardianName    VARCHAR(100)  NULL,
    ADD COLUMN IF NOT EXISTS GuardianContact VARCHAR(20)   NULL,
    ADD COLUMN IF NOT EXISTS Allergies       VARCHAR(500)  NULL,
    ADD COLUMN IF NOT EXISTS MedicalNotes    VARCHAR(1000) NULL;

ALTER TABLE Appointments
    ADD COLUMN IF NOT EXISTS DurationMinutes    INT          NOT NULL DEFAULT 30,
    ADD COLUMN IF NOT EXISTS CancellationReason VARCHAR(255) NULL,
    ADD COLUMN IF NOT EXISTS Notes              VARCHAR(500) NULL;

ALTER TABLE Treatments
    ADD COLUMN IF NOT EXISTS DiscountType    VARCHAR(20)  NOT NULL DEFAULT 'None',
    ADD COLUMN IF NOT EXISTS DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0.00;

-- ---------------------------------------------------------------------
-- PART 2: New table
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS DentistTimeOff (
    TimeOffId INT AUTO_INCREMENT PRIMARY KEY,
    DentistId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate   DATE NOT NULL,
    Reason    VARCHAR(255) NULL,
    INDEX idx_dentisttimeoff_dentist (DentistId, StartDate, EndDate)
);

-- ---------------------------------------------------------------------
-- PART 3: Procedures
-- ---------------------------------------------------------------------
DELIMITER $$

-- ===================== PATIENT =====================

DROP PROCEDURE IF EXISTS `sp_Patient_GetAll`$$
CREATE PROCEDURE `sp_Patient_GetAll`()
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address,
           GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt
    FROM Patients
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END$$

DROP PROCEDURE IF EXISTS `sp_Patient_GetAllIncludingInactive`$$
CREATE PROCEDURE `sp_Patient_GetAllIncludingInactive`()
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address,
           GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt
    FROM Patients
    ORDER BY LastName, FirstName;
END$$

DROP PROCEDURE IF EXISTS `sp_Patient_GetById`$$
CREATE PROCEDURE `sp_Patient_GetById`(IN p_PatientId INT)
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address,
           GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt
    FROM Patients
    WHERE PatientId = p_PatientId;
END$$

DROP PROCEDURE IF EXISTS `sp_Patient_Add`$$
CREATE PROCEDURE `sp_Patient_Add`(
    IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200),
    IN p_GuardianName VARCHAR(100), IN p_GuardianContact VARCHAR(20),
    IN p_Allergies VARCHAR(500), IN p_MedicalNotes VARCHAR(1000))
BEGIN
    INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber, Email, Address,
                          GuardianName, GuardianContact, Allergies, MedicalNotes)
    VALUES (p_FirstName, p_LastName, p_DateOfBirth, p_ContactNumber, p_Email, p_Address,
            p_GuardianName, p_GuardianContact, p_Allergies, p_MedicalNotes);
END$$

DROP PROCEDURE IF EXISTS `sp_Patient_Update`$$
CREATE PROCEDURE `sp_Patient_Update`(
    IN p_PatientId INT, IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200),
    IN p_GuardianName VARCHAR(100), IN p_GuardianContact VARCHAR(20),
    IN p_Allergies VARCHAR(500), IN p_MedicalNotes VARCHAR(1000))
BEGIN
    UPDATE Patients
    SET FirstName = p_FirstName, LastName = p_LastName, DateOfBirth = p_DateOfBirth,
        ContactNumber = p_ContactNumber, Email = p_Email, Address = p_Address,
        GuardianName = p_GuardianName, GuardianContact = p_GuardianContact,
        Allergies = p_Allergies, MedicalNotes = p_MedicalNotes
    WHERE PatientId = p_PatientId;
END$$

DROP PROCEDURE IF EXISTS `sp_Patient_FindByNameAndDob`$$
CREATE PROCEDURE `sp_Patient_FindByNameAndDob`(
    IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE)
BEGIN
    -- Includes inactive patients on purpose (used to detect duplicates / reactivate)
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address,
           GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt
    FROM Patients
    WHERE FirstName = p_FirstName
      AND LastName = p_LastName
      AND DateOfBirth = p_DateOfBirth;
END$$

DROP PROCEDURE IF EXISTS `sp_Patient_Reactivate`$$
CREATE PROCEDURE `sp_Patient_Reactivate`(IN p_PatientId INT)
BEGIN
    UPDATE Patients SET IsActive = 1 WHERE PatientId = p_PatientId;
END$$

-- ===================== APPOINTMENT =====================

DROP PROCEDURE IF EXISTS `sp_Appointment_GetAll`$$
CREATE PROCEDURE `sp_Appointment_GetAll`()
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes,
           Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    ORDER BY AppointmentDateTime;
END$$

DROP PROCEDURE IF EXISTS `sp_Appointment_GetById`$$
CREATE PROCEDURE `sp_Appointment_GetById`(IN p_AppointmentId INT)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes,
           Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    WHERE AppointmentId = p_AppointmentId;
END$$

DROP PROCEDURE IF EXISTS `sp_Appointment_Add`$$
CREATE PROCEDURE `sp_Appointment_Add`(
    IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME,
    IN p_DurationMinutes INT, IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255),
    IN p_CancellationReason VARCHAR(255), IN p_Notes VARCHAR(500))
BEGIN
    INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes,
                              Status, Reason, CancellationReason, Notes)
    VALUES (p_PatientId, p_DentistId, p_AppointmentDateTime, p_DurationMinutes,
            p_Status, p_Reason, p_CancellationReason, p_Notes);
END$$

DROP PROCEDURE IF EXISTS `sp_Appointment_Update`$$
CREATE PROCEDURE `sp_Appointment_Update`(
    IN p_AppointmentId INT, IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME,
    IN p_DurationMinutes INT, IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255),
    IN p_CancellationReason VARCHAR(255), IN p_Notes VARCHAR(500))
BEGIN
    UPDATE Appointments
    SET PatientId = p_PatientId, DentistId = p_DentistId,
        AppointmentDateTime = p_AppointmentDateTime, DurationMinutes = p_DurationMinutes,
        Status = p_Status, Reason = p_Reason,
        CancellationReason = p_CancellationReason, Notes = p_Notes
    WHERE AppointmentId = p_AppointmentId;
END$$

DROP PROCEDURE IF EXISTS `sp_Appointment_GetByPatientId`$$
CREATE PROCEDURE `sp_Appointment_GetByPatientId`(IN p_PatientId INT)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes,
           Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    WHERE PatientId = p_PatientId
    ORDER BY AppointmentDateTime DESC;
END$$

DROP PROCEDURE IF EXISTS `sp_Appointment_GetByRange`$$
CREATE PROCEDURE `sp_Appointment_GetByRange`(IN p_From DATETIME, IN p_To DATETIME)
BEGIN
    -- From is inclusive, To is exclusive
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes,
           Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    WHERE AppointmentDateTime >= p_From AND AppointmentDateTime < p_To
    ORDER BY AppointmentDateTime;
END$$

DROP PROCEDURE IF EXISTS `sp_Appointment_GetByDentistAndRange`$$
CREATE PROCEDURE `sp_Appointment_GetByDentistAndRange`(
    IN p_DentistId INT, IN p_From DATETIME, IN p_To DATETIME)
BEGIN
    -- From is inclusive, To is exclusive
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, DurationMinutes,
           Status, Reason, CancellationReason, Notes, CreatedAt
    FROM Appointments
    WHERE DentistId = p_DentistId
      AND AppointmentDateTime >= p_From AND AppointmentDateTime < p_To
    ORDER BY AppointmentDateTime;
END$$

DROP PROCEDURE IF EXISTS `sp_Appointment_CountUpcomingByDentist`$$
CREATE PROCEDURE `sp_Appointment_CountUpcomingByDentist`(IN p_DentistId INT, IN p_From DATETIME)
BEGIN
    SELECT COUNT(*) AS Total
    FROM Appointments
    WHERE DentistId = p_DentistId
      AND AppointmentDateTime >= p_From
      AND Status NOT IN ('Cancelled', 'Completed');
END$$

-- ===================== TREATMENT =====================

DROP PROCEDURE IF EXISTS `sp_Treatment_GetAll`$$
CREATE PROCEDURE `sp_Treatment_GetAll`()
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost,
           DiscountType, DiscountPercent, DatePerformed, Notes
    FROM Treatments
    ORDER BY DatePerformed DESC, TreatmentId DESC;
END$$

DROP PROCEDURE IF EXISTS `sp_Treatment_GetById`$$
CREATE PROCEDURE `sp_Treatment_GetById`(IN p_TreatmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost,
           DiscountType, DiscountPercent, DatePerformed, Notes
    FROM Treatments
    WHERE TreatmentId = p_TreatmentId;
END$$

DROP PROCEDURE IF EXISTS `sp_Treatment_GetByAppointmentId`$$
CREATE PROCEDURE `sp_Treatment_GetByAppointmentId`(IN p_AppointmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost,
           DiscountType, DiscountPercent, DatePerformed, Notes
    FROM Treatments
    WHERE AppointmentId = p_AppointmentId
    ORDER BY TreatmentId;
END$$

DROP PROCEDURE IF EXISTS `sp_Treatment_GetByPatientId`$$
CREATE PROCEDURE `sp_Treatment_GetByPatientId`(IN p_PatientId INT)
BEGIN
    SELECT t.TreatmentId, t.AppointmentId, t.TreatmentTypeId, t.ToothNumber, t.Cost,
           t.DiscountType, t.DiscountPercent, t.DatePerformed, t.Notes
    FROM Treatments t
    INNER JOIN Appointments a ON a.AppointmentId = t.AppointmentId
    WHERE a.PatientId = p_PatientId
    ORDER BY t.DatePerformed DESC, t.TreatmentId DESC;
END$$

DROP PROCEDURE IF EXISTS `sp_Treatment_Add`$$
CREATE PROCEDURE `sp_Treatment_Add`(
    IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DiscountType VARCHAR(20), IN p_DiscountPercent DECIMAL(5,2),
    IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    INSERT INTO Treatments (AppointmentId, TreatmentTypeId, ToothNumber, Cost,
                            DiscountType, DiscountPercent, DatePerformed, Notes)
    VALUES (p_AppointmentId, p_TreatmentTypeId, p_ToothNumber, p_Cost,
            p_DiscountType, p_DiscountPercent, p_DatePerformed, p_Notes);
END$$

DROP PROCEDURE IF EXISTS `sp_Treatment_Update`$$
CREATE PROCEDURE `sp_Treatment_Update`(
    IN p_TreatmentId INT, IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DiscountType VARCHAR(20), IN p_DiscountPercent DECIMAL(5,2),
    IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    UPDATE Treatments
    SET AppointmentId = p_AppointmentId, TreatmentTypeId = p_TreatmentTypeId,
        ToothNumber = p_ToothNumber, Cost = p_Cost,
        DiscountType = p_DiscountType, DiscountPercent = p_DiscountPercent,
        DatePerformed = p_DatePerformed, Notes = p_Notes
    WHERE TreatmentId = p_TreatmentId;
END$$

-- ===================== DENTIST TIME OFF =====================

DROP PROCEDURE IF EXISTS `sp_DentistTimeOff_GetByDentist`$$
CREATE PROCEDURE `sp_DentistTimeOff_GetByDentist`(IN p_DentistId INT)
BEGIN
    SELECT TimeOffId, DentistId, StartDate, EndDate, Reason
    FROM DentistTimeOff
    WHERE DentistId = p_DentistId
    ORDER BY StartDate DESC;
END$$

DROP PROCEDURE IF EXISTS `sp_DentistTimeOff_GetOverlapping`$$
CREATE PROCEDURE `sp_DentistTimeOff_GetOverlapping`(IN p_DentistId INT, IN p_From DATE, IN p_To DATE)
BEGIN
    SELECT TimeOffId, DentistId, StartDate, EndDate, Reason
    FROM DentistTimeOff
    WHERE DentistId = p_DentistId
      AND StartDate <= p_To
      AND EndDate >= p_From
    ORDER BY StartDate;
END$$

DROP PROCEDURE IF EXISTS `sp_DentistTimeOff_Add`$$
CREATE PROCEDURE `sp_DentistTimeOff_Add`(
    IN p_DentistId INT, IN p_StartDate DATE, IN p_EndDate DATE, IN p_Reason VARCHAR(255))
BEGIN
    INSERT INTO DentistTimeOff (DentistId, StartDate, EndDate, Reason)
    VALUES (p_DentistId, p_StartDate, p_EndDate, p_Reason);
END$$

DROP PROCEDURE IF EXISTS `sp_DentistTimeOff_Delete`$$
CREATE PROCEDURE `sp_DentistTimeOff_Delete`(IN p_TimeOffId INT)
BEGIN
    DELETE FROM DentistTimeOff WHERE TimeOffId = p_TimeOffId;
END$$

-- ===================== REPORTS =====================
-- Billed = Cost - discount, where DiscountPercent is a 0-100 value.

DROP PROCEDURE IF EXISTS `sp_Report_AppointmentsByStatus`$$
CREATE PROCEDURE `sp_Report_AppointmentsByStatus`(IN p_From DATE, IN p_To DATE)
BEGIN
    SELECT Status, COUNT(*) AS Total
    FROM Appointments
    WHERE AppointmentDateTime >= p_From
      AND AppointmentDateTime <  DATE_ADD(p_To, INTERVAL 1 DAY)
    GROUP BY Status
    ORDER BY Total DESC;
END$$

DROP PROCEDURE IF EXISTS `sp_Report_RevenueByDay`$$
CREATE PROCEDURE `sp_Report_RevenueByDay`(IN p_From DATE, IN p_To DATE)
BEGIN
    SELECT t.DatePerformed AS `Day`,
           ROUND(SUM(t.Cost * (1 - t.DiscountPercent / 100)), 2) AS Billed
    FROM Treatments t
    WHERE t.DatePerformed BETWEEN p_From AND p_To
    GROUP BY t.DatePerformed
    ORDER BY t.DatePerformed;
END$$

DROP PROCEDURE IF EXISTS `sp_Report_TopTreatmentTypes`$$
CREATE PROCEDURE `sp_Report_TopTreatmentTypes`(IN p_From DATE, IN p_To DATE, IN p_Top INT)
BEGIN
    SELECT tt.Name AS Name,
           COUNT(*) AS Total,
           ROUND(SUM(t.Cost * (1 - t.DiscountPercent / 100)), 2) AS Billed
    FROM Treatments t
    INNER JOIN TreatmentTypes tt ON tt.TreatmentTypeId = t.TreatmentTypeId
    WHERE t.DatePerformed BETWEEN p_From AND p_To
    GROUP BY tt.TreatmentTypeId, tt.Name
    ORDER BY Total DESC, Billed DESC
    LIMIT p_Top;
END$$

DROP PROCEDURE IF EXISTS `sp_Report_DentistWorkload`$$
CREATE PROCEDURE `sp_Report_DentistWorkload`(IN p_From DATE, IN p_To DATE)
BEGIN
    SELECT CONCAT(d.FirstName, ' ', d.LastName) AS Dentist,
           COUNT(a.AppointmentId) AS Total,
           COUNT(CASE WHEN a.Status = 'Completed' THEN 1 END) AS Completed,
           COALESCE(b.Billed, 0) AS Billed
    FROM Dentists d
    LEFT JOIN Appointments a
           ON a.DentistId = d.DentistId
          AND a.AppointmentDateTime >= p_From
          AND a.AppointmentDateTime <  DATE_ADD(p_To, INTERVAL 1 DAY)
    LEFT JOIN (
        SELECT a2.DentistId,
               ROUND(SUM(t.Cost * (1 - t.DiscountPercent / 100)), 2) AS Billed
        FROM Treatments t
        INNER JOIN Appointments a2 ON a2.AppointmentId = t.AppointmentId
        WHERE t.DatePerformed BETWEEN p_From AND p_To
        GROUP BY a2.DentistId
    ) b ON b.DentistId = d.DentistId
    GROUP BY d.DentistId, d.FirstName, d.LastName, d.IsActive, b.Billed
    HAVING COUNT(a.AppointmentId) > 0 OR d.IsActive = 1
    ORDER BY Total DESC, Dentist;
END$$

DELIMITER ;

-- ---------------------------------------------------------------------
-- PART 4: Verification (every row below should say OK)
-- ---------------------------------------------------------------------
SELECT n.ProcName,
       IF(r.ROUTINE_NAME IS NULL, 'MISSING', 'OK') AS Result
FROM (
    SELECT 'sp_Patient_GetAll' AS ProcName UNION ALL
    SELECT 'sp_Patient_GetAllIncludingInactive' UNION ALL
    SELECT 'sp_Patient_GetById' UNION ALL
    SELECT 'sp_Patient_Add' UNION ALL
    SELECT 'sp_Patient_Update' UNION ALL
    SELECT 'sp_Patient_FindByNameAndDob' UNION ALL
    SELECT 'sp_Patient_Reactivate' UNION ALL
    SELECT 'sp_Appointment_GetAll' UNION ALL
    SELECT 'sp_Appointment_GetById' UNION ALL
    SELECT 'sp_Appointment_Add' UNION ALL
    SELECT 'sp_Appointment_Update' UNION ALL
    SELECT 'sp_Appointment_GetByPatientId' UNION ALL
    SELECT 'sp_Appointment_GetByRange' UNION ALL
    SELECT 'sp_Appointment_GetByDentistAndRange' UNION ALL
    SELECT 'sp_Appointment_CountUpcomingByDentist' UNION ALL
    SELECT 'sp_Treatment_GetAll' UNION ALL
    SELECT 'sp_Treatment_GetById' UNION ALL
    SELECT 'sp_Treatment_GetByAppointmentId' UNION ALL
    SELECT 'sp_Treatment_GetByPatientId' UNION ALL
    SELECT 'sp_Treatment_Add' UNION ALL
    SELECT 'sp_Treatment_Update' UNION ALL
    SELECT 'sp_DentistTimeOff_GetByDentist' UNION ALL
    SELECT 'sp_DentistTimeOff_GetOverlapping' UNION ALL
    SELECT 'sp_DentistTimeOff_Add' UNION ALL
    SELECT 'sp_DentistTimeOff_Delete' UNION ALL
    SELECT 'sp_Report_AppointmentsByStatus' UNION ALL
    SELECT 'sp_Report_RevenueByDay' UNION ALL
    SELECT 'sp_Report_TopTreatmentTypes' UNION ALL
    SELECT 'sp_Report_DentistWorkload'
) n
LEFT JOIN INFORMATION_SCHEMA.ROUTINES r
       ON r.ROUTINE_SCHEMA = 'dentalclinicdb'
      AND r.ROUTINE_NAME = n.ProcName
      AND r.ROUTINE_TYPE = 'PROCEDURE'
ORDER BY Result, n.ProcName;
