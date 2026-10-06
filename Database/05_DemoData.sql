-- OPTIONAL, DEVELOPMENT ONLY. Run manually after 01 -> 02 -> 02b -> 03
-- (or 04 -> 02 -> 02b -> 03). Never run against a real clinic's database.
-- Synthetic discount choices illustrate the UI, not eligibility determinations.
-- Deterministic for CURDATE(); repeated runs retain existing rows and add no duplicates.
USE dentalclinicdb;
DROP PROCEDURE IF EXISTS sp_DemoData_05;
DELIMITER $$
CREATE PROCEDURE sp_DemoData_05()
BEGIN
    DECLARE v_people JSON;
    DECLARE v_index INT DEFAULT 0;
    DECLARE v_first VARCHAR(50);
    DECLARE v_last VARCHAR(50);
    DECLARE v_dob DATE;
    DECLARE v_patient INT;
    DECLARE v_maria INT;
    DECLARE v_carlos INT;
    DECLARE v_isabel INT;
    DECLARE v_paolo INT;
    DECLARE v_teresa INT;
    DECLARE v_dentist INT;
    DECLARE v_offset INT DEFAULT -45;
    DECLARE v_slot INT;
    DECLARE v_sequence INT;
    DECLARE v_checked_slot INT;
    DECLARE v_checked_created TINYINT DEFAULT 0;
    DECLARE v_day DATE;
    DECLARE v_start DATETIME;
    DECLARE v_end DATETIME;
    DECLARE v_leave DATE;
    DECLARE v_minutes INT;
    DECLARE v_status VARCHAR(20);
    DECLARE v_cancellation VARCHAR(255);
    DECLARE v_reason VARCHAR(100);
    DECLARE v_type INT;
    DECLARE v_appointment INT;
    DECLARE v_item INT;
    DECLARE v_items INT;
    DECLARE v_tooth VARCHAR(10);
    DECLARE v_discount VARCHAR(20);
    DECLARE v_percent DECIMAL(5,2);
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    IF (SELECT COUNT(DISTINCT Name) FROM TreatmentTypes WHERE Name IN ('Consultation / Check-up', 'Dental Cleaning',
        'Tooth Extraction', 'Root Canal', 'Filling', 'Dental X-ray (Periapical)')) <> 6 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Demo only: run the baseline 03 seed first.';
    END IF;
    SELECT MIN(DentistId) INTO v_maria FROM Dentists WHERE LicenseNumber = 'PRC-00123' AND IsActive = 1;
    SELECT MIN(DentistId) INTO v_carlos FROM Dentists WHERE LicenseNumber = 'PRC-00456' AND IsActive = 1;
    IF v_maria IS NULL OR v_carlos IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Demo only: the two active baseline dentists are required.';
    END IF;

    START TRANSACTION;
    INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber)
    SELECT 'Isabel', 'Del Rosario', 'Pediatric Dentistry', '09170001001', 'PRC-DEMO-01' FROM DUAL
    WHERE NOT EXISTS (SELECT 1 FROM Dentists WHERE LicenseNumber = 'PRC-DEMO-01');
    INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber)
    SELECT 'Paolo', 'Navarro', 'Endodontics', '09170001002', 'PRC-DEMO-02' FROM DUAL
    WHERE NOT EXISTS (SELECT 1 FROM Dentists WHERE LicenseNumber = 'PRC-DEMO-02');
    INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber)
    SELECT 'Teresa', 'Bautista', 'Prosthodontics', '09170001003', 'PRC-DEMO-03' FROM DUAL
    WHERE NOT EXISTS (SELECT 1 FROM Dentists WHERE LicenseNumber = 'PRC-DEMO-03');
    SELECT MIN(DentistId) INTO v_isabel FROM Dentists WHERE LicenseNumber = 'PRC-DEMO-01' AND IsActive = 1;
    SELECT MIN(DentistId) INTO v_paolo FROM Dentists WHERE LicenseNumber = 'PRC-DEMO-02' AND IsActive = 1;
    SELECT MIN(DentistId) INTO v_teresa FROM Dentists WHERE LicenseNumber = 'PRC-DEMO-03' AND IsActive = 1;
    IF v_isabel IS NULL OR v_paolo IS NULL OR v_teresa IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Demo only: the three demo dentists must be active.';
    END IF;

    -- Exact dentist123 bcrypt hash from 03_SeedData.sql; accounts are demo only.
    INSERT INTO Users (Username, PasswordHash, Role, DentistId)
    SELECT names.Username, '$2a$11$CqzW7tBTUEIjEWgJ0.tXp.W76Gx0xwofe.dQsFtl12Hgd2MYk7n3S', 'Dentist', names.DentistId
    FROM (SELECT 'drreyes' AS Username, v_carlos AS DentistId
          UNION ALL SELECT 'drdelrosario', v_isabel
          UNION ALL SELECT 'drnavarro', v_paolo
          UNION ALL SELECT 'drbautista', v_teresa) names
    WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.Username = names.Username);

    SET v_people = '[{"first":"Mateo","last":"Dela Cruz","dob":"2016-03-14"},{"first":"Sofia","last":"Garcia","dob":"2015-08-21"},{"first":"Liam","last":"Mendoza","dob":"2018-01-09"},{"first":"Amara","last":"Torres","dob":"2017-11-30"},{"first":"Lourdes","last":"Villanueva","dob":"1954-05-18"},{"first":"Ramon","last":"Bautista","dob":"1958-09-02"},{"first":"Teresita","last":"Aquino","dob":"1951-12-11"},{"first":"Camille","last":"Ramos","dob":"1993-04-07"},{"first":"Gabriel","last":"Flores","dob":"1988-06-24"},{"first":"Bianca","last":"Castillo","dob":"1999-02-16"},{"first":"Joshua","last":"Fernandez","dob":"1996-10-04"},{"first":"Alyssa","last":"Rivera","dob":"1997-07-12"},{"first":"Mark","last":"Gonzales","dob":"1985-03-20"},{"first":"Nicole","last":"Santiago","dob":"2000-09-14"},{"first":"Daniel","last":"Cruz","dob":"1991-01-28"},{"first":"Patricia","last":"Reyes","dob":"1994-05-06"},{"first":"Adrian","last":"Lim","dob":"1989-08-10"},{"first":"Jasmine","last":"Chua","dob":"1998-12-22"},{"first":"Francis","last":"Velasco","dob":"1982-06-15"},{"first":"Angela","last":"Soriano","dob":"1995-11-08"},{"first":"Carlo","last":"Manalo","dob":"1990-04-25"},{"first":"Erika","last":"Navarro","dob":"2002-02-03"},{"first":"Roberto","last":"Salazar","dob":"1976-07-19"},{"first":"Michelle","last":"Domingo","dob":"1987-10-27"}]';
    WHILE v_index < JSON_LENGTH(v_people) DO
        SET v_first = JSON_UNQUOTE(JSON_EXTRACT(v_people, CONCAT('$[', v_index, '].first')));
        SET v_last = JSON_UNQUOTE(JSON_EXTRACT(v_people, CONCAT('$[', v_index, '].last')));
        SET v_dob = CAST(JSON_UNQUOTE(JSON_EXTRACT(v_people, CONCAT('$[', v_index, '].dob'))) AS DATE);
        INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber, Email, Address,
            GuardianName, GuardianContact, Allergies, MedicalNotes, IsActive, CreatedAt)
        SELECT v_first, v_last, v_dob, CONCAT('0917', LPAD(v_index + 2000001, 7, '0')),
            CONCAT(LOWER(v_first), '.', REPLACE(LOWER(v_last), ' ', ''), '@example.test'), 'Davao City',
            IF(v_index < 4, CONCAT('Maria ', v_last), NULL),
            IF(v_index < 4, CONCAT('0918', LPAD(v_index + 2000001, 7, '0')), NULL),
            CASE v_index WHEN 7 THEN 'Penicillin' WHEN 8 THEN 'Latex' ELSE NULL END,
            CASE v_index WHEN 7 THEN 'Hypertension; taking maintenance medication.' WHEN 8 THEN 'Asthma; carries a rescue inhaler.' ELSE NULL END,
            IF(v_index = 22, 0, 1), IF(v_index = 22, CURDATE() - INTERVAL 2 YEAR, CURDATE() - INTERVAL 60 DAY)
        FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Patients
            WHERE FirstName = v_first AND LastName = v_last AND DateOfBirth = v_dob);
        SET v_index = v_index + 1;
    END WHILE;

    -- Insert leave before generating appointments, without changing existing visits.
    SET v_leave = CURDATE() - INTERVAL 30 DAY;
    IF NOT EXISTS (SELECT 1 FROM Appointments WHERE DentistId = v_paolo AND Status <> 'Cancelled'
        AND DATE(AppointmentDateTime) BETWEEN v_leave AND v_leave + INTERVAL 2 DAY)
       AND NOT EXISTS (SELECT 1 FROM DentistTimeOff WHERE DentistId = v_paolo AND StartDate = v_leave AND EndDate = v_leave + INTERVAL 2 DAY) THEN
        INSERT INTO DentistTimeOff (DentistId, StartDate, EndDate, Reason)
        VALUES (v_paolo, v_leave, v_leave + INTERVAL 2 DAY, 'Demo data v2: past leave');
    END IF;
    SET v_leave = CURDATE() + INTERVAL 7 DAY;
    WHILE EXISTS (SELECT 1 FROM Appointments WHERE DentistId = v_paolo AND Status IN ('Scheduled','CheckedIn')
        AND DATE(AppointmentDateTime) BETWEEN v_leave AND v_leave + INTERVAL 2 DAY) DO
        SET v_leave = v_leave + INTERVAL 1 DAY;
    END WHILE;
    IF NOT EXISTS (SELECT 1 FROM DentistTimeOff WHERE DentistId = v_paolo AND StartDate = v_leave AND EndDate = v_leave + INTERVAL 2 DAY) THEN
        INSERT INTO DentistTimeOff (DentistId, StartDate, EndDate, Reason)
        VALUES (v_paolo, v_leave, v_leave + INTERVAL 2 DAY, 'Demo data v2: upcoming leave');
    END IF;
    SET v_checked_slot = CASE WHEN TIME(NOW()) >= '15:30:00' THEN 5 WHEN TIME(NOW()) >= '14:30:00' THEN 4
        WHEN TIME(NOW()) >= '13:30:00' THEN 3 WHEN TIME(NOW()) >= '11:00:00' THEN 2 WHEN TIME(NOW()) >= '10:00:00' THEN 1 ELSE 0 END;

    WHILE v_offset <= 14 DO
        SET v_day = DATE_ADD(CURDATE(), INTERVAL v_offset DAY);
        SET v_slot = 0;
        WHILE v_slot < 6 AND DAYOFWEEK(v_day) <> 1 DO
            SET v_sequence = (v_offset + 45) * 6 + v_slot;
            SET v_start = TIMESTAMP(v_day, CASE v_slot WHEN 0 THEN '09:00:00' WHEN 1 THEN '10:00:00'
                WHEN 2 THEN '11:00:00' WHEN 3 THEN '13:30:00' WHEN 4 THEN '14:30:00' ELSE '15:30:00' END);
            SET v_dentist = CASE MOD(v_sequence, 5) WHEN 0 THEN v_maria WHEN 1 THEN v_carlos
                WHEN 2 THEN v_isabel WHEN 3 THEN v_paolo ELSE v_teresa END;
            -- Patient 22 (zero based) stays inactive and never receives appointments.
            -- Patient 23 receives only cancelled visits.
            SET v_index = IF(v_day <> CURDATE() AND v_slot = 5 AND MOD(v_offset + 45, 5) = 0, 23, MOD(v_sequence, 22));
            SET v_first = JSON_UNQUOTE(JSON_EXTRACT(v_people, CONCAT('$[', v_index, '].first')));
            SET v_last = JSON_UNQUOTE(JSON_EXTRACT(v_people, CONCAT('$[', v_index, '].last')));
            SET v_dob = CAST(JSON_UNQUOTE(JSON_EXTRACT(v_people, CONCAT('$[', v_index, '].dob'))) AS DATE);
            SELECT MIN(PatientId) INTO v_patient FROM Patients WHERE FirstName = v_first AND LastName = v_last AND DateOfBirth = v_dob;
            SELECT TreatmentTypeId, Name, DefaultDurationMinutes INTO v_type, v_reason, v_minutes FROM TreatmentTypes
            WHERE Name = CASE MOD(v_sequence + FLOOR(v_sequence / 6), 6) WHEN 0 THEN 'Dental Cleaning' WHEN 1 THEN 'Tooth Extraction'
                WHEN 2 THEN 'Root Canal' WHEN 3 THEN 'Filling' WHEN 4 THEN 'Dental X-ray (Periapical)' ELSE 'Consultation / Check-up' END
            ORDER BY TreatmentTypeId LIMIT 1;
            SET v_end = DATE_ADD(v_start, INTERVAL v_minutes MINUTE);
            SET v_status = CASE WHEN v_index = 23 THEN 'Cancelled'
                WHEN v_day < CURDATE() THEN CASE WHEN MOD(v_sequence, 20) < 14 THEN 'Completed' WHEN MOD(v_sequence, 20) < 17 THEN 'Cancelled' ELSE 'NoShow' END
                WHEN v_day = CURDATE() THEN CASE WHEN v_slot >= v_checked_slot AND v_checked_created = 0 THEN 'CheckedIn' WHEN v_end <= NOW() THEN 'Completed' ELSE 'Scheduled' END
                ELSE IF(MOD(v_sequence, 13) = 0, 'Cancelled', 'Scheduled') END;
            SET v_cancellation = CASE WHEN v_status = 'NoShow' THEN 'No Show' WHEN v_status = 'Cancelled'
                THEN IF(MOD(v_sequence, 2) = 0, 'Patient Rescheduled', 'Clinic Rescheduled') ELSE NULL END;
            SELECT MIN(AppointmentId) INTO v_appointment FROM Appointments
            WHERE Notes = 'Demo data v2' AND DentistId = v_dentist AND AppointmentDateTime = v_start AND PatientId = v_patient;
            IF v_appointment IS NULL AND v_end <= TIMESTAMP(v_day, '17:00:00')
                AND NOT fn_DentistOnLeave(v_dentist, v_day)
                AND NOT fn_DentistHasConflict(v_dentist, v_start, v_minutes, NULL)
                AND NOT EXISTS (SELECT 1 FROM Appointments WHERE PatientId = v_patient AND Status <> 'Cancelled'
                    AND AppointmentDateTime < v_end AND DATE_ADD(AppointmentDateTime, INTERVAL DurationMinutes MINUTE) > v_start) THEN
                INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, DurationMinutes, Status, Reason, CancellationReason, Notes)
                VALUES (v_patient, v_dentist, v_start, v_minutes, v_status, v_reason, v_cancellation, 'Demo data v2');
                SET v_appointment = LAST_INSERT_ID();
            END IF;
            -- If the nearest slot is blocked, use the next available one. On
            -- reruns, retain the existing checked-in example instead of adding one.
            IF v_day = CURDATE() AND v_appointment IS NOT NULL AND EXISTS (
                SELECT 1 FROM Appointments WHERE AppointmentId = v_appointment AND Status = 'CheckedIn') THEN
                SET v_checked_created = 1;
            END IF;
            IF v_appointment IS NOT NULL AND EXISTS (SELECT 1 FROM Appointments WHERE AppointmentId = v_appointment AND Status = 'Completed') THEN
                SET v_item = 0; SET v_items = 1 + MOD(v_sequence, 3);
                WHILE v_item < v_items DO
                    -- First treatment matches the appointment reason. Subsequent types vary.
                    IF v_item > 0 THEN
                        SELECT MIN(TreatmentTypeId) INTO v_type FROM TreatmentTypes WHERE Name =
                            CASE MOD(v_sequence + v_item, 3) WHEN 0 THEN 'Dental Cleaning' WHEN 1 THEN 'Dental X-ray (Periapical)' ELSE 'Fluoride Treatment' END;
                    END IF;
                    SET v_tooth = IF(TIMESTAMPDIFF(YEAR, v_dob, v_day) < 18,
                        CASE MOD(v_sequence + v_item, 4) WHEN 0 THEN '51' WHEN 1 THEN '61' WHEN 2 THEN '75' ELSE '85' END,
                        CASE MOD(v_sequence + v_item, 8) WHEN 0 THEN '11' WHEN 1 THEN '16' WHEN 2 THEN '21' WHEN 3 THEN '26'
                            WHEN 4 THEN '31' WHEN 5 THEN '36' WHEN 6 THEN '41' ELSE '46' END);
                    SET v_discount = CASE WHEN TIMESTAMPDIFF(YEAR, v_dob, v_day) >= 60 THEN 'Senior'
                        WHEN MOD(v_sequence, 11) = 0 THEN 'PWD' WHEN MOD(v_sequence, 7) = 0 THEN 'Other' ELSE 'None' END;
                    SET v_percent = CASE v_discount WHEN 'Senior' THEN 20 WHEN 'PWD' THEN 20 WHEN 'Other' THEN 10 + MOD(v_sequence, 6) ELSE 0 END;
                    INSERT INTO Treatments (AppointmentId, TreatmentTypeId, ToothNumber, Cost, DiscountType, DiscountPercent, DatePerformed, Notes)
                    SELECT v_appointment, TreatmentTypeId, v_tooth,
                        IF(MOD(v_sequence + v_item, 17) = 0, DefaultCost + 50, DefaultCost),
                        v_discount, v_percent, v_day, 'Demo data v2' FROM TreatmentTypes
                    WHERE TreatmentTypeId = v_type AND NOT EXISTS (SELECT 1 FROM Treatments
                        WHERE AppointmentId = v_appointment AND TreatmentTypeId = v_type AND ToothNumber = v_tooth AND Notes = 'Demo data v2');
                    SET v_item = v_item + 1;
                END WHILE;
            END IF;
            SET v_slot = v_slot + 1;
        END WHILE;
        SET v_offset = v_offset + 1;
    END WHILE;
    COMMIT;
    SELECT Status, COUNT(*) AS DemoAppointments FROM Appointments WHERE Notes = 'Demo data v2' GROUP BY Status ORDER BY Status;
    SELECT CONCAT('Dr. ', d.FirstName, ' ', d.LastName) AS Dentist, COUNT(*) AS DemoAppointments
    FROM Appointments a JOIN Dentists d ON d.DentistId = a.DentistId WHERE a.Notes = 'Demo data v2' GROUP BY d.DentistId, d.FirstName, d.LastName ORDER BY Dentist;
    SELECT COUNT(*) AS DemoTreatments, COALESCE(SUM(fn_TreatmentNet(Cost, DiscountPercent)), 0) AS DemoBilledNet
    FROM Treatments WHERE Notes = 'Demo data v2';
END$$
DELIMITER ;
CALL sp_DemoData_05();
DROP PROCEDURE IF EXISTS sp_DemoData_05;
