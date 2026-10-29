-- =====================================================================
-- 002_treatmenttype_duration.sql
-- Target : MariaDB 10.4.x
-- Purpose: MapTreatmentType in C# reads DefaultDurationMinutes, which
--          did not exist in TreatmentTypes.
-- Safe to re-run (idempotent). Back up the database first.
-- =====================================================================

USE `dentalclinicdb`;

ALTER TABLE TreatmentTypes
    ADD COLUMN IF NOT EXISTS DefaultDurationMinutes INT NOT NULL DEFAULT 30;

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_TreatmentType_GetAll`$$
CREATE PROCEDURE `sp_TreatmentType_GetAll`()
BEGIN
    SELECT TreatmentTypeId, Name, DefaultCost, DefaultDurationMinutes, Description
    FROM TreatmentTypes
    ORDER BY Name;
END$$

DROP PROCEDURE IF EXISTS `sp_TreatmentType_GetById`$$
CREATE PROCEDURE `sp_TreatmentType_GetById`(IN p_TreatmentTypeId INT)
BEGIN
    SELECT TreatmentTypeId, Name, DefaultCost, DefaultDurationMinutes, Description
    FROM TreatmentTypes
    WHERE TreatmentTypeId = p_TreatmentTypeId;
END$$

DELIMITER ;

-- Verify: should list 5 columns, including DefaultDurationMinutes
SELECT COLUMN_NAME, COLUMN_TYPE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dentalclinicdb' AND TABLE_NAME = 'TreatmentTypes'
ORDER BY ORDINAL_POSITION;
