-- Existing databases: run 04, then 02, then 03 manually in MySQL Workbench.
USE dentalclinicdb;
DELIMITER $$
DROP PROCEDURE IF EXISTS sp_Migrate_RealWorldFixes$$
CREATE PROCEDURE sp_Migrate_RealWorldFixes()
BEGIN
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
DELIMITER ;
