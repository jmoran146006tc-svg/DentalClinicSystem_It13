-- Existing databases: run 04, then 02, then 03 manually in MySQL Workbench.
USE dentalclinicdb;
DELIMITER $$
DROP PROCEDURE IF EXISTS sp_Migrate_AddColumn$$
CREATE PROCEDURE sp_Migrate_AddColumn(IN p_Table VARCHAR(64), IN p_Column VARCHAR(64), IN p_Definition TEXT)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = p_Table AND COLUMN_NAME = p_Column) THEN
        SET @migration_ddl = CONCAT('ALTER TABLE ', p_Table, ' ADD COLUMN ', p_Column, ' ', p_Definition);
        PREPARE migration_stmt FROM @migration_ddl;
        EXECUTE migration_stmt;
        DEALLOCATE PREPARE migration_stmt;
    END IF;
END$$
DROP PROCEDURE IF EXISTS sp_Migrate_AddCheck$$
CREATE PROCEDURE sp_Migrate_AddCheck(IN p_Table VARCHAR(64), IN p_Name VARCHAR(64), IN p_Expression TEXT)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = p_Table AND CONSTRAINT_NAME = p_Name) THEN
        SET @migration_ddl = CONCAT('ALTER TABLE ', p_Table, ' ADD CONSTRAINT ', p_Name, ' CHECK (', p_Expression, ')');
        PREPARE migration_stmt FROM @migration_ddl;
        EXECUTE migration_stmt;
        DEALLOCATE PREPARE migration_stmt;
    END IF;
END$$

DROP PROCEDURE IF EXISTS sp_Migrate_RealWorldFixes$$
CREATE PROCEDURE sp_Migrate_RealWorldFixes()
BEGIN
    CALL sp_Migrate_AddColumn('Appointments', 'DurationMinutes', 'INT NOT NULL DEFAULT 30');
    CALL sp_Migrate_AddColumn('TreatmentTypes', 'DefaultDurationMinutes', 'INT NOT NULL DEFAULT 30');
    CALL sp_Migrate_AddCheck('Appointments', 'CK_Appointments_Duration', 'DurationMinutes BETWEEN 15 AND 240');
    CALL sp_Migrate_AddCheck('TreatmentTypes', 'CK_TreatmentTypes_Duration', 'DefaultDurationMinutes BETWEEN 15 AND 240');
    UPDATE TreatmentTypes SET DefaultDurationMinutes = CASE Name
        WHEN 'Dental Cleaning' THEN 45 WHEN 'Tooth Extraction' THEN 45 WHEN 'Root Canal' THEN 90 WHEN 'Filling' THEN 60 END
    WHERE DefaultDurationMinutes = 30 AND Name IN ('Dental Cleaning','Tooth Extraction','Root Canal','Filling');
    CREATE TABLE IF NOT EXISTS DentistTimeOff (
    TimeOffId INT AUTO_INCREMENT PRIMARY KEY,
    DentistId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Reason VARCHAR(100) NULL,
    INDEX IX_DentistTimeOff_Range (DentistId, StartDate, EndDate),
    CONSTRAINT FK_DentistTimeOff_Dentist FOREIGN KEY (DentistId) REFERENCES Dentists(DentistId),
    CONSTRAINT CK_DentistTimeOff_Dates CHECK (EndDate >= StartDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    IF EXISTS (SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = 'Appointments'
        AND CONSTRAINT_NAME = 'CK_Appointments_Status' AND CONSTRAINT_TYPE = 'CHECK') THEN
        ALTER TABLE Appointments DROP CHECK CK_Appointments_Status;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = 'Appointments'
        AND CONSTRAINT_NAME = 'CK_Appointments_Status') THEN
        ALTER TABLE Appointments ADD CONSTRAINT CK_Appointments_Status
            CHECK (Status IN ('Scheduled','CheckedIn','Completed','Cancelled','NoShow'));
    END IF;
END$$
CALL sp_Migrate_RealWorldFixes()$$
DROP PROCEDURE sp_Migrate_RealWorldFixes$$
DROP PROCEDURE sp_Migrate_AddColumn$$
DROP PROCEDURE sp_Migrate_AddCheck$$
DELIMITER ;
