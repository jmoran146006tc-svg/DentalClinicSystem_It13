-- Run manually in MySQL Workbench after 02 and before 03. MySQL 8.0+.
-- Fresh: 01 -> 02 -> 02b -> 03. Existing: 04 -> 02 -> 02b -> 03.
-- Business validation remains in Service/. These objects enforce write atomicity.
USE dentalclinicdb;
DELIMITER $$

DROP FUNCTION IF EXISTS fn_TreatmentNet$$
CREATE FUNCTION fn_TreatmentNet(p_Cost DECIMAL(10,2), p_Percent DECIMAL(5,2))
RETURNS DECIMAL(16,6) DETERMINISTIC NO SQL
RETURN p_Cost - p_Cost * p_Percent / 100$$
-- Six fractional places retain Treatment.Net for two-decimal costs/percentages.
-- DECIMAL(14,4) would round e.g. 1.23 at 12.34% from 1.078218 to 1.0782.

DROP FUNCTION IF EXISTS fn_DentistHasConflict$$
CREATE FUNCTION fn_DentistHasConflict(p_DentistId INT, p_Start DATETIME, p_Minutes INT, p_ExcludeId INT)
RETURNS TINYINT(1) READS SQL DATA
BEGIN
    DECLARE v_conflict INT DEFAULT NULL;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_conflict = NULL;
    -- A locking current read sees the previous booker's commit even when the
    -- caller's transaction already has an older REPEATABLE READ snapshot.
    SELECT AppointmentId INTO v_conflict FROM Appointments
    WHERE DentistId = p_DentistId AND Status <> 'Cancelled'
      AND (p_ExcludeId IS NULL OR AppointmentId <> p_ExcludeId)
      AND AppointmentDateTime < DATE_ADD(p_Start, INTERVAL p_Minutes MINUTE)
      AND DATE_ADD(AppointmentDateTime, INTERVAL DurationMinutes MINUTE) > p_Start
    ORDER BY AppointmentId LIMIT 1 FOR SHARE;
    RETURN v_conflict IS NOT NULL;
END$$

DROP FUNCTION IF EXISTS fn_DentistOnLeave$$
CREATE FUNCTION fn_DentistOnLeave(p_DentistId INT, p_Day DATE)
RETURNS TINYINT(1) READS SQL DATA
BEGIN
    DECLARE v_leave INT DEFAULT NULL;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_leave = NULL;
    SELECT TimeOffId INTO v_leave FROM DentistTimeOff
    WHERE DentistId = p_DentistId AND p_Day BETWEEN StartDate AND EndDate
    ORDER BY TimeOffId LIMIT 1 FOR SHARE;
    RETURN v_leave IS NOT NULL;
END$$

DROP TRIGGER IF EXISTS trg_Appointments_BI$$
CREATE TRIGGER trg_Appointments_BI BEFORE INSERT ON Appointments FOR EACH ROW
BEGIN
    DECLARE v_lock INT;
    SELECT DentistId INTO v_lock FROM Dentists WHERE DentistId = NEW.DentistId FOR UPDATE;
    IF fn_DentistHasConflict(NEW.DentistId, NEW.AppointmentDateTime, NEW.DurationMinutes, NULL) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'This dentist is already booked during that appointment.';
    END IF;
    IF fn_DentistOnLeave(NEW.DentistId, DATE(NEW.AppointmentDateTime)) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'This dentist is on leave on that date.';
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Appointments_BU$$
CREATE TRIGGER trg_Appointments_BU BEFORE UPDATE ON Appointments FOR EACH ROW
BEGIN
    DECLARE v_lock INT;
    IF NOT (OLD.DentistId <=> NEW.DentistId)
       OR NOT (OLD.AppointmentDateTime <=> NEW.AppointmentDateTime)
       OR NOT (OLD.DurationMinutes <=> NEW.DurationMinutes) THEN
        SELECT DentistId INTO v_lock FROM Dentists WHERE DentistId = NEW.DentistId FOR UPDATE;
        IF fn_DentistHasConflict(NEW.DentistId, NEW.AppointmentDateTime, NEW.DurationMinutes, OLD.AppointmentId) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'This dentist is already booked during that appointment.';
        END IF;
        IF fn_DentistOnLeave(NEW.DentistId, DATE(NEW.AppointmentDateTime)) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'This dentist is on leave on that date.';
        END IF;
    END IF;
    IF NOT (OLD.Status <=> NEW.Status) AND NOT (
        (OLD.Status = 'Scheduled' AND NEW.Status IN ('CheckedIn','Completed','Cancelled','NoShow'))
        OR (OLD.Status = 'CheckedIn' AND NEW.Status = 'Completed')) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'That appointment status transition is not allowed.';
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Appointments_AI$$
CREATE TRIGGER trg_Appointments_AI AFTER INSERT ON Appointments FOR EACH ROW
BEGIN
    UPDATE Patients SET IsActive = 1 WHERE PatientId = NEW.PatientId AND IsActive = 0;
END$$

DROP TRIGGER IF EXISTS trg_Treatments_BI$$
CREATE TRIGGER trg_Treatments_BI BEFORE INSERT ON Treatments FOR EACH ROW
BEGIN
    DECLARE v_status VARCHAR(20);
    SELECT Status INTO v_status FROM Appointments WHERE AppointmentId = NEW.AppointmentId FOR SHARE;
    IF v_status IN ('Cancelled','NoShow') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Treatments cannot be saved for cancelled or no-show appointments.';
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Treatments_BU$$
CREATE TRIGGER trg_Treatments_BU BEFORE UPDATE ON Treatments FOR EACH ROW
BEGIN
    DECLARE v_status VARCHAR(20);
    SELECT Status INTO v_status FROM Appointments WHERE AppointmentId = NEW.AppointmentId FOR SHARE;
    IF v_status IN ('Cancelled','NoShow') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Treatments cannot be saved for cancelled or no-show appointments.';
    END IF;
END$$

-- Audit records what/when, not who: the repository does not supply an actor.
-- Adding actor attribution is a separate task. Credential hashes are never included.

DROP TRIGGER IF EXISTS trg_Patients_AU_Audit$$
CREATE TRIGGER trg_Patients_AU_Audit AFTER UPDATE ON Patients FOR EACH ROW
BEGIN
    IF NOT (OLD.PatientId <=> NEW.PatientId)
       OR NOT (OLD.FirstName <=> NEW.FirstName)
       OR NOT (OLD.LastName <=> NEW.LastName)
       OR NOT (OLD.DateOfBirth <=> NEW.DateOfBirth)
       OR NOT (OLD.ContactNumber <=> NEW.ContactNumber)
       OR NOT (OLD.Email <=> NEW.Email)
       OR NOT (OLD.Address <=> NEW.Address)
       OR NOT (OLD.GuardianName <=> NEW.GuardianName)
       OR NOT (OLD.GuardianContact <=> NEW.GuardianContact)
       OR NOT (OLD.Allergies <=> NEW.Allergies)
       OR NOT (OLD.MedicalNotes <=> NEW.MedicalNotes)
       OR NOT (OLD.IsActive <=> NEW.IsActive)
       OR NOT (OLD.CreatedAt <=> NEW.CreatedAt) THEN
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues)
        VALUES ('Patients', NEW.PatientId, 'UPDATE', JSON_OBJECT('PatientId', OLD.PatientId, 'FirstName', OLD.FirstName, 'LastName', OLD.LastName, 'DateOfBirth', OLD.DateOfBirth, 'ContactNumber', OLD.ContactNumber, 'Email', OLD.Email, 'Address', OLD.Address, 'GuardianName', OLD.GuardianName, 'GuardianContact', OLD.GuardianContact, 'Allergies', OLD.Allergies, 'MedicalNotes', OLD.MedicalNotes, 'IsActive', OLD.IsActive, 'CreatedAt', OLD.CreatedAt), JSON_OBJECT('PatientId', NEW.PatientId, 'FirstName', NEW.FirstName, 'LastName', NEW.LastName, 'DateOfBirth', NEW.DateOfBirth, 'ContactNumber', NEW.ContactNumber, 'Email', NEW.Email, 'Address', NEW.Address, 'GuardianName', NEW.GuardianName, 'GuardianContact', NEW.GuardianContact, 'Allergies', NEW.Allergies, 'MedicalNotes', NEW.MedicalNotes, 'IsActive', NEW.IsActive, 'CreatedAt', NEW.CreatedAt));
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Treatments_AI_Audit$$
CREATE TRIGGER trg_Treatments_AI_Audit AFTER INSERT ON Treatments FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues)
        VALUES ('Treatments', NEW.TreatmentId, 'INSERT', NULL, JSON_OBJECT('TreatmentId', NEW.TreatmentId, 'AppointmentId', NEW.AppointmentId, 'TreatmentTypeId', NEW.TreatmentTypeId, 'ToothNumber', NEW.ToothNumber, 'Cost', NEW.Cost, 'DiscountType', NEW.DiscountType, 'DiscountPercent', NEW.DiscountPercent, 'DatePerformed', NEW.DatePerformed, 'Notes', NEW.Notes));
END$$

DROP TRIGGER IF EXISTS trg_Treatments_AU_Audit$$
CREATE TRIGGER trg_Treatments_AU_Audit AFTER UPDATE ON Treatments FOR EACH ROW
BEGIN
    IF NOT (OLD.TreatmentId <=> NEW.TreatmentId)
       OR NOT (OLD.AppointmentId <=> NEW.AppointmentId)
       OR NOT (OLD.TreatmentTypeId <=> NEW.TreatmentTypeId)
       OR NOT (OLD.ToothNumber <=> NEW.ToothNumber)
       OR NOT (OLD.Cost <=> NEW.Cost)
       OR NOT (OLD.DiscountType <=> NEW.DiscountType)
       OR NOT (OLD.DiscountPercent <=> NEW.DiscountPercent)
       OR NOT (OLD.DatePerformed <=> NEW.DatePerformed)
       OR NOT (OLD.Notes <=> NEW.Notes) THEN
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues)
        VALUES ('Treatments', NEW.TreatmentId, 'UPDATE', JSON_OBJECT('TreatmentId', OLD.TreatmentId, 'AppointmentId', OLD.AppointmentId, 'TreatmentTypeId', OLD.TreatmentTypeId, 'ToothNumber', OLD.ToothNumber, 'Cost', OLD.Cost, 'DiscountType', OLD.DiscountType, 'DiscountPercent', OLD.DiscountPercent, 'DatePerformed', OLD.DatePerformed, 'Notes', OLD.Notes), JSON_OBJECT('TreatmentId', NEW.TreatmentId, 'AppointmentId', NEW.AppointmentId, 'TreatmentTypeId', NEW.TreatmentTypeId, 'ToothNumber', NEW.ToothNumber, 'Cost', NEW.Cost, 'DiscountType', NEW.DiscountType, 'DiscountPercent', NEW.DiscountPercent, 'DatePerformed', NEW.DatePerformed, 'Notes', NEW.Notes));
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Treatments_AD_Audit$$
CREATE TRIGGER trg_Treatments_AD_Audit AFTER DELETE ON Treatments FOR EACH ROW
BEGIN
    INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues)
        VALUES ('Treatments', OLD.TreatmentId, 'DELETE', JSON_OBJECT('TreatmentId', OLD.TreatmentId, 'AppointmentId', OLD.AppointmentId, 'TreatmentTypeId', OLD.TreatmentTypeId, 'ToothNumber', OLD.ToothNumber, 'Cost', OLD.Cost, 'DiscountType', OLD.DiscountType, 'DiscountPercent', OLD.DiscountPercent, 'DatePerformed', OLD.DatePerformed, 'Notes', OLD.Notes), NULL);
END$$

DROP TRIGGER IF EXISTS trg_Appointments_AU_Audit$$
CREATE TRIGGER trg_Appointments_AU_Audit AFTER UPDATE ON Appointments FOR EACH ROW
BEGIN
    IF NOT (OLD.Status <=> NEW.Status)
       OR NOT (OLD.DentistId <=> NEW.DentistId)
       OR NOT (OLD.AppointmentDateTime <=> NEW.AppointmentDateTime)
       OR NOT (OLD.DurationMinutes <=> NEW.DurationMinutes) THEN
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues)
        VALUES ('Appointments', NEW.AppointmentId, 'UPDATE', JSON_OBJECT('AppointmentId', OLD.AppointmentId, 'PatientId', OLD.PatientId, 'DentistId', OLD.DentistId, 'AppointmentDateTime', OLD.AppointmentDateTime, 'DurationMinutes', OLD.DurationMinutes, 'Status', OLD.Status, 'Reason', OLD.Reason, 'CancellationReason', OLD.CancellationReason, 'Notes', OLD.Notes), JSON_OBJECT('AppointmentId', NEW.AppointmentId, 'PatientId', NEW.PatientId, 'DentistId', NEW.DentistId, 'AppointmentDateTime', NEW.AppointmentDateTime, 'DurationMinutes', NEW.DurationMinutes, 'Status', NEW.Status, 'Reason', NEW.Reason, 'CancellationReason', NEW.CancellationReason, 'Notes', NEW.Notes));
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Users_AU_Audit$$
CREATE TRIGGER trg_Users_AU_Audit AFTER UPDATE ON Users FOR EACH ROW
BEGIN
    IF NOT (OLD.Role <=> NEW.Role)
       OR NOT (OLD.IsActive <=> NEW.IsActive) THEN
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues)
        VALUES ('Users', NEW.UserId, 'UPDATE', JSON_OBJECT('UserId', OLD.UserId, 'Username', OLD.Username, 'Role', OLD.Role, 'DentistId', OLD.DentistId, 'IsActive', OLD.IsActive), JSON_OBJECT('UserId', NEW.UserId, 'Username', NEW.Username, 'Role', NEW.Role, 'DentistId', NEW.DentistId, 'IsActive', NEW.IsActive));
    END IF;
END$$

-- ev_deactivate_stale_patients remains unchanged in 02_StoredProcedures.sql.
DROP EVENT IF EXISTS ev_flag_missed_appointments$$
CREATE EVENT ev_flag_missed_appointments
ON SCHEDULE EVERY 1 DAY STARTS CURRENT_TIMESTAMP + INTERVAL 1 DAY
DISABLE
COMMENT 'Enable only after the clinic/adviser confirms this rule.'
DO
    UPDATE Appointments SET Status = 'NoShow', CancellationReason = 'No Show'
    WHERE Status = 'Scheduled'
      AND DATE_ADD(AppointmentDateTime, INTERVAL DurationMinutes MINUTE) <= NOW() - INTERVAL 24 HOUR$$
DELIMITER ;

-- The event scheduler requires DBA permission; the missed-appointment event ships disabled.
-- SET GLOBAL event_scheduler = ON;
