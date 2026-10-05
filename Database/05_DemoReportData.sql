-- OPTIONAL, DEMO ONLY. Run manually after 01 -> 02 -> 03 (or 04 -> 02 -> 03).
-- Inserts synthetic report data only; never updates/deletes existing clinic rows.
-- Uses only the two exact active seed patients and dentists from 03_SeedData.sql.
-- Re-running tops up to 25 marked appointments in the last 30 days, at most one
-- marked visit per day. Existing marked visits/treatments are left unchanged.
-- Fewer than 25 may be inserted if leave, existing bookings or the clock block slots.
-- Discount categories are synthetic examples, not claims of patient eligibility.
USE dentalclinicdb;

DROP PROCEDURE IF EXISTS sp_DemoReportData_05;
DELIMITER $$
CREATE PROCEDURE sp_DemoReportData_05()
BEGIN
    DECLARE v_juan INT;
    DECLARE v_ana INT;
    DECLARE v_maria INT;
    DECLARE v_carlos INT;
    DECLARE v_patient INT;
    DECLARE v_dentist INT;
    DECLARE v_appointment INT;
    DECLARE v_total INT DEFAULT 0;
    DECLARE v_offset INT DEFAULT 0;
    DECLARE v_hour INT;
    DECLARE v_item INT;
    DECLARE v_day DATE;
    DECLARE v_start DATETIME;
    DECLARE v_end DATETIME;
    DECLARE v_discount VARCHAR(20);
    DECLARE v_percent DECIMAL(5,2);
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    SELECT MIN(PatientId) INTO v_juan FROM Patients WHERE FirstName = 'Juan' AND LastName = 'Dela Cruz'
        AND DateOfBirth = '1998-04-12' AND ContactNumber = '09981234567' AND IsActive = 1;
    SELECT MIN(PatientId) INTO v_ana FROM Patients WHERE FirstName = 'Ana' AND LastName = 'Lopez'
        AND DateOfBirth = '2001-09-03' AND ContactNumber = '09991234567' AND IsActive = 1;
    SELECT MIN(DentistId) INTO v_maria FROM Dentists WHERE FirstName = 'Maria' AND LastName = 'Santos'
        AND LicenseNumber = 'PRC-00123' AND IsActive = 1;
    SELECT MIN(DentistId) INTO v_carlos FROM Dentists WHERE FirstName = 'Carlos' AND LastName = 'Reyes'
        AND LicenseNumber = 'PRC-00456' AND IsActive = 1;
    IF v_juan IS NULL OR v_ana IS NULL OR v_maria IS NULL OR v_carlos IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Demo only: the exact active seed patients/dentists are required.';
    END IF;
    IF (SELECT COUNT(DISTINCT Name) FROM TreatmentTypes WHERE Name IN ('Dental Cleaning', 'Tooth Extraction',
        'Root Canal', 'Filling', 'Dental X-ray (Periapical)', 'Fluoride Treatment')) <> 6 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Demo only: six treatment types from 03_SeedData.sql are required.';
    END IF;

    START TRANSACTION;
    SELECT COUNT(*) INTO v_total FROM Appointments WHERE Notes = 'Demo report data'
        AND PatientId IN (v_juan, v_ana) AND DentistId IN (v_maria, v_carlos)
        AND AppointmentDateTime >= DATE_SUB(CURDATE(), INTERVAL 29 DAY)
        AND AppointmentDateTime < DATE_ADD(CURDATE(), INTERVAL 1 DAY);
    WHILE v_offset < 30 AND v_total < 25 DO
        SET v_day = DATE_SUB(CURDATE(), INTERVAL v_offset DAY);
        SET v_patient = IF(MOD(v_total, 2) = 0, v_juan, v_ana);
        SET v_dentist = IF(MOD(v_total, 2) = 0, v_maria, v_carlos);
        SET v_hour = 9;
        -- Sunday closure, inclusive leave dates and both patient/dentist overlaps.
        WHILE v_hour <= 15 AND DAYOFWEEK(v_day) <> 1 AND v_total < 25
            AND NOT EXISTS (SELECT 1 FROM Appointments WHERE Notes = 'Demo report data'
                AND PatientId IN (v_juan, v_ana) AND DentistId IN (v_maria, v_carlos)
                AND AppointmentDateTime >= v_day AND AppointmentDateTime < DATE_ADD(v_day, INTERVAL 1 DAY)) DO
            SET v_start = DATE_ADD(v_day, INTERVAL v_hour HOUR);
            SET v_end = DATE_ADD(v_start, INTERVAL 90 MINUTE);
            IF v_end <= NOW()
                AND NOT EXISTS (SELECT 1 FROM DentistTimeOff WHERE DentistId = v_dentist AND v_day BETWEEN StartDate AND EndDate)
                AND NOT EXISTS (SELECT 1 FROM Appointments WHERE (DentistId = v_dentist OR PatientId = v_patient)
                    AND AppointmentDateTime < v_end AND DATE_ADD(AppointmentDateTime, INTERVAL DurationMinutes MINUTE) > v_start) THEN
                INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, Notes)
                VALUES (v_patient, v_dentist, v_start, 90, 'Completed', 'Demo report visit', 'Demo report data');
                SET v_appointment = LAST_INSERT_ID();
                SET v_item = 0;
                WHILE v_item < 1 + MOD(v_total, 3) DO
                    SET v_discount = CASE MOD(v_total + v_item, 4) WHEN 1 THEN 'Senior' WHEN 2 THEN 'PWD' WHEN 3 THEN 'Other' ELSE 'None' END;
                    SET v_percent = CASE v_discount WHEN 'Senior' THEN 20 WHEN 'PWD' THEN 20 WHEN 'Other' THEN 10 ELSE 0 END;
                    INSERT INTO Treatments (AppointmentId, TreatmentTypeId, Cost, DiscountType, DiscountPercent, DatePerformed, Notes)
                    SELECT v_appointment, TreatmentTypeId, DefaultCost, v_discount, v_percent, v_day, 'Demo report data'
                    FROM TreatmentTypes WHERE Name = CASE MOD(v_total + v_item, 6)
                        WHEN 0 THEN 'Dental Cleaning' WHEN 1 THEN 'Tooth Extraction' WHEN 2 THEN 'Root Canal'
                        WHEN 3 THEN 'Filling' WHEN 4 THEN 'Dental X-ray (Periapical)' ELSE 'Fluoride Treatment' END
                    ORDER BY TreatmentTypeId LIMIT 1;
                    SET v_item = v_item + 1;
                END WHILE;
                SET v_total = v_total + 1;
            END IF;
            SET v_hour = v_hour + 3;
        END WHILE;
        SET v_offset = v_offset + 1;
    END WHILE;
    COMMIT;
    SELECT v_total AS DemoAppointmentsInLast30Days;
END$$
DELIMITER ;
CALL sp_DemoReportData_05();
DROP PROCEDURE sp_DemoReportData_05;
