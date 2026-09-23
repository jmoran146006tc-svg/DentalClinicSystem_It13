using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public static class DatabaseInitializer
    {
        public static async Task EnsureDatabaseReadyAsync()
        {
            await EnsureDatabaseExistsAsync();

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();

            await EnsureTablesExistAsync(conn);
            await EnsureStoredProceduresExistAsync(conn);
            await SeedIfEmptyAsync(conn);
        }

        private static async Task EnsureDatabaseExistsAsync()
        {
            await using var conn = await DbConnectionHelper.GetOpenServerConnectionAsync();
            await using var cmd = new MySqlCommand(
                $"CREATE DATABASE IF NOT EXISTS `{DbConnectionHelper.DatabaseNameValue}` CHARACTER SET utf8mb4;", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task EnsureTablesExistAsync(MySqlConnection conn)
        {
            foreach (var ddl in TableScripts)
            {
                try
                {
                    await using var cmd = new MySqlCommand(ddl, conn);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex) when (ex.Number == 1061)
                {
                    // CREATE INDEX has no "IF NOT EXISTS" in MySQL - an index that's
                    // already there throws 1061 (Duplicate key name), which is exactly
                    // the "already set up, nothing to do" case this method allows for.
                }
            }
        }

        private static async Task EnsureStoredProceduresExistAsync(MySqlConnection conn)
        {
            foreach (var procedureSql in StoredProcedureScripts)
            {
                var name = ExtractProcedureName(procedureSql);

                await using (var drop = new MySqlCommand($"DROP PROCEDURE IF EXISTS {name};", conn))
                    await drop.ExecuteNonQueryAsync();

                await using var create = new MySqlCommand(procedureSql, conn);
                await create.ExecuteNonQueryAsync();
            }
        }

        private static string ExtractProcedureName(string procedureSql)
        {
            // Pulls "sp_Patient_GetAll" out of "CREATE PROCEDURE sp_Patient_GetAll(...".
            const string marker = "PROCEDURE ";
            var start = procedureSql.IndexOf(marker, StringComparison.OrdinalIgnoreCase) + marker.Length;
            var end = procedureSql.IndexOf('(', start);
            return procedureSql[start..end].Trim();
        }

        private static async Task SeedIfEmptyAsync(MySqlConnection conn)
        {
            if (await CountRowsAsync(conn, "Dentists") == 0)
            {
                await using var cmd = new MySqlCommand(
                    "INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber) VALUES " +
                    "('Maria', 'Santos', 'General Dentistry', '09171234567', 'PRC-00123')," +
                    "('Carlos', 'Reyes', 'Orthodontics', '09181234567', 'PRC-00456');", conn);
                await cmd.ExecuteNonQueryAsync();
            }

            if (await CountRowsAsync(conn, "TreatmentTypes") == 0)
            {
                await using var cmd = new MySqlCommand(
                    "INSERT INTO TreatmentTypes (Name, DefaultCost, Description) VALUES " +
                    "('Dental Cleaning', 800.00, 'Routine oral prophylaxis')," +
                    "('Tooth Extraction', 1500.00, 'Simple extraction')," +
                    "('Root Canal', 6000.00, 'Endodontic treatment')," +
                    "('Filling', 1200.00, 'Composite resin filling');", conn);
                await cmd.ExecuteNonQueryAsync();
            }

            if (await CountRowsAsync(conn, "Patients") == 0)
            {
                await using var cmd = new MySqlCommand(
                    "INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber, Email, Address) VALUES " +
                    "('Juan', 'Dela Cruz', '1998-04-12', '09981234567', 'juan.delacruz@email.com', 'Butuan City')," +
                    "('Ana', 'Lopez', '2001-09-03', '09991234567', 'ana.lopez@email.com', 'Butuan City');", conn);
                await cmd.ExecuteNonQueryAsync();
            }

            if (await CountRowsAsync(conn, "Users") == 0)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                await using var cmd = new MySqlCommand(
                    "INSERT INTO Users (Username, PasswordHash, Role, IsActive) VALUES ('admin', @hash, 'Admin', 1);", conn);
                cmd.Parameters.AddWithValue("@hash", passwordHash);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task<long> CountRowsAsync(MySqlConnection conn, string table)
        {
            await using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {table};", conn);
            return Convert.ToInt64(await cmd.ExecuteScalarAsync() ?? 0L);
        }

        private static readonly string[] TableScripts =
        [
            @"CREATE TABLE IF NOT EXISTS Patients (
                PatientId INT AUTO_INCREMENT PRIMARY KEY,
                FirstName VARCHAR(50) NOT NULL,
                LastName VARCHAR(50) NOT NULL,
                DateOfBirth DATE NOT NULL,
                ContactNumber VARCHAR(20) NOT NULL,
                Email VARCHAR(100) NULL,
                Address VARCHAR(200) NULL,
                IsActive TINYINT(1) NOT NULL DEFAULT 1,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS Dentists (
                DentistId INT AUTO_INCREMENT PRIMARY KEY,
                FirstName VARCHAR(50) NOT NULL,
                LastName VARCHAR(50) NOT NULL,
                Specialization VARCHAR(100) NULL,
                ContactNumber VARCHAR(20) NULL,
                LicenseNumber VARCHAR(50) NULL,
                IsActive TINYINT(1) NOT NULL DEFAULT 1
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS TreatmentTypes (
                TreatmentTypeId INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(100) NOT NULL,
                DefaultCost DECIMAL(10,2) NOT NULL,
                Description VARCHAR(255) NULL
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS Appointments (
                AppointmentId INT AUTO_INCREMENT PRIMARY KEY,
                PatientId INT NOT NULL,
                DentistId INT NOT NULL,
                AppointmentDateTime DATETIME NOT NULL,
                Status VARCHAR(20) NOT NULL DEFAULT 'Scheduled',
                Reason VARCHAR(255) NULL,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT FK_Appointments_Patient FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
                CONSTRAINT FK_Appointments_Dentist FOREIGN KEY (DentistId) REFERENCES Dentists(DentistId),
                CONSTRAINT CK_Appointments_Status CHECK (Status IN ('Scheduled','Completed','Cancelled','NoShow'))
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS Treatments (
                TreatmentId INT AUTO_INCREMENT PRIMARY KEY,
                AppointmentId INT NOT NULL,
                TreatmentTypeId INT NOT NULL,
                ToothNumber VARCHAR(10) NULL,
                Cost DECIMAL(10,2) NOT NULL,
                DatePerformed DATE NOT NULL,
                Notes VARCHAR(500) NULL,
                CONSTRAINT FK_Treatments_Appointment FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
                CONSTRAINT FK_Treatments_TreatmentType FOREIGN KEY (TreatmentTypeId) REFERENCES TreatmentTypes(TreatmentTypeId)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS Users (
                UserId INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(50) NOT NULL UNIQUE,
                PasswordHash VARCHAR(255) NOT NULL,
                Role VARCHAR(20) NOT NULL,
                DentistId INT NULL,
                IsActive TINYINT(1) NOT NULL DEFAULT 1,
                CONSTRAINT FK_Users_Dentist FOREIGN KEY (DentistId) REFERENCES Dentists(DentistId),
                CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin','Receptionist','Dentist'))
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            "CREATE INDEX IX_Appointments_DentistId_DateTime ON Appointments(DentistId, AppointmentDateTime);",
            "CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);",
            "CREATE INDEX IX_Treatments_AppointmentId ON Treatments(AppointmentId);"
        ];

        private static readonly string[] StoredProcedureScripts =
        [
            @"CREATE PROCEDURE sp_Patient_GetAll()
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, IsActive, CreatedAt
    FROM Patients
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END",

            @"CREATE PROCEDURE sp_Patient_GetById(IN p_PatientId INT)
BEGIN
    SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, IsActive, CreatedAt
    FROM Patients
    WHERE PatientId = p_PatientId;
END",

            @"CREATE PROCEDURE sp_Patient_Add(
    IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200))
BEGIN
    INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber, Email, Address)
    VALUES (p_FirstName, p_LastName, p_DateOfBirth, p_ContactNumber, p_Email, p_Address);
END",

            @"CREATE PROCEDURE sp_Patient_Update(
    IN p_PatientId INT, IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_DateOfBirth DATE,
    IN p_ContactNumber VARCHAR(20), IN p_Email VARCHAR(100), IN p_Address VARCHAR(200))
BEGIN
    UPDATE Patients
    SET FirstName = p_FirstName, LastName = p_LastName, DateOfBirth = p_DateOfBirth,
        ContactNumber = p_ContactNumber, Email = p_Email, Address = p_Address
    WHERE PatientId = p_PatientId;
END",

            @"CREATE PROCEDURE sp_Patient_Delete(IN p_PatientId INT)
BEGIN
    -- Soft delete only - never hard-delete a patient (see Section 5.3 notes).
    UPDATE Patients SET IsActive = 0 WHERE PatientId = p_PatientId;
END",

            @"CREATE PROCEDURE sp_Dentist_GetAll()
BEGIN
    SELECT DentistId, FirstName, LastName, Specialization, ContactNumber, LicenseNumber, IsActive
    FROM Dentists
    WHERE IsActive = 1
    ORDER BY LastName, FirstName;
END",

            @"CREATE PROCEDURE sp_Dentist_GetById(IN p_DentistId INT)
BEGIN
    SELECT DentistId, FirstName, LastName, Specialization, ContactNumber, LicenseNumber, IsActive
    FROM Dentists
    WHERE DentistId = p_DentistId;
END",

            @"CREATE PROCEDURE sp_Dentist_Add(
    IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_Specialization VARCHAR(100),
    IN p_ContactNumber VARCHAR(20), IN p_LicenseNumber VARCHAR(50))
BEGIN
    INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber)
    VALUES (p_FirstName, p_LastName, p_Specialization, p_ContactNumber, p_LicenseNumber);
END",

            @"CREATE PROCEDURE sp_Dentist_Update(
    IN p_DentistId INT, IN p_FirstName VARCHAR(50), IN p_LastName VARCHAR(50), IN p_Specialization VARCHAR(100),
    IN p_ContactNumber VARCHAR(20), IN p_LicenseNumber VARCHAR(50))
BEGIN
    UPDATE Dentists
    SET FirstName = p_FirstName, LastName = p_LastName, Specialization = p_Specialization,
        ContactNumber = p_ContactNumber, LicenseNumber = p_LicenseNumber
    WHERE DentistId = p_DentistId;
END",

            @"CREATE PROCEDURE sp_Dentist_Delete(IN p_DentistId INT)
BEGIN
    -- Soft delete - a dentist's appointment/treatment history must stay intact.
    UPDATE Dentists SET IsActive = 0 WHERE DentistId = p_DentistId;
END",

            @"CREATE PROCEDURE sp_TreatmentType_GetAll()
BEGIN
    SELECT TreatmentTypeId, Name, DefaultCost, Description
    FROM TreatmentTypes
    ORDER BY Name;
END",

            @"CREATE PROCEDURE sp_TreatmentType_GetById(IN p_TreatmentTypeId INT)
BEGIN
    SELECT TreatmentTypeId, Name, DefaultCost, Description
    FROM TreatmentTypes
    WHERE TreatmentTypeId = p_TreatmentTypeId;
END",

            @"CREATE PROCEDURE sp_TreatmentType_Add(IN p_Name VARCHAR(100), IN p_DefaultCost DECIMAL(10,2), IN p_Description VARCHAR(255))
BEGIN
    INSERT INTO TreatmentTypes (Name, DefaultCost, Description) VALUES (p_Name, p_DefaultCost, p_Description);
END",

            @"CREATE PROCEDURE sp_TreatmentType_Update(IN p_TreatmentTypeId INT, IN p_Name VARCHAR(100), IN p_DefaultCost DECIMAL(10,2), IN p_Description VARCHAR(255))
BEGIN
    UPDATE TreatmentTypes SET Name = p_Name, DefaultCost = p_DefaultCost, Description = p_Description
    WHERE TreatmentTypeId = p_TreatmentTypeId;
END",

            @"CREATE PROCEDURE sp_TreatmentType_Delete(IN p_TreatmentTypeId INT)
BEGIN
    -- Hard delete - the FK from Treatments correctly rejects this if any treatment
    -- record still points at this type; the repository translates that into a
    -- RepositoryConstraintException with a plain-English message.
    DELETE FROM TreatmentTypes WHERE TreatmentTypeId = p_TreatmentTypeId;
END",

            @"CREATE PROCEDURE sp_Appointment_GetAll()
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, Status, Reason, CreatedAt
    FROM Appointments
    ORDER BY AppointmentDateTime;
END",

            @"CREATE PROCEDURE sp_Appointment_GetById(IN p_AppointmentId INT)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, Status, Reason, CreatedAt
    FROM Appointments
    WHERE AppointmentId = p_AppointmentId;
END",

            @"CREATE PROCEDURE sp_Appointment_GetByDentistAndDate(IN p_DentistId INT, IN p_Date DATE)
BEGIN
    SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, Status, Reason, CreatedAt
    FROM Appointments
    WHERE DentistId = p_DentistId
      AND AppointmentDateTime >= p_Date
      AND AppointmentDateTime < DATE_ADD(p_Date, INTERVAL 1 DAY)
      AND Status != 'Cancelled';
END",

            @"CREATE PROCEDURE sp_Appointment_Add(
    IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME,
    IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255))
BEGIN
    INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, Status, Reason)
    VALUES (p_PatientId, p_DentistId, p_AppointmentDateTime, p_Status, p_Reason);
END",

            @"CREATE PROCEDURE sp_Appointment_Update(
    IN p_AppointmentId INT, IN p_PatientId INT, IN p_DentistId INT, IN p_AppointmentDateTime DATETIME,
    IN p_Status VARCHAR(20), IN p_Reason VARCHAR(255))
BEGIN
    UPDATE Appointments
    SET PatientId = p_PatientId, DentistId = p_DentistId, AppointmentDateTime = p_AppointmentDateTime,
        Status = p_Status, Reason = p_Reason
    WHERE AppointmentId = p_AppointmentId;
END",

            @"CREATE PROCEDURE sp_Appointment_Delete(IN p_AppointmentId INT)
BEGIN
    -- Prefer updating Status to 'Cancelled' over calling this directly - an
    -- appointment with treatments attached can't be hard-deleted anyway (FK from
    -- Treatments), and cancelling preserves history.
    DELETE FROM Appointments WHERE AppointmentId = p_AppointmentId;
END",

            @"CREATE PROCEDURE sp_Treatment_GetAll()
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes
    FROM Treatments
    ORDER BY DatePerformed DESC;
END",

            @"CREATE PROCEDURE sp_Treatment_GetById(IN p_TreatmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes
    FROM Treatments
    WHERE TreatmentId = p_TreatmentId;
END",

            @"CREATE PROCEDURE sp_Treatment_GetByAppointmentId(IN p_AppointmentId INT)
BEGIN
    SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes
    FROM Treatments
    WHERE AppointmentId = p_AppointmentId;
END",

            @"CREATE PROCEDURE sp_Treatment_Add(
    IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    INSERT INTO Treatments (AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes)
    VALUES (p_AppointmentId, p_TreatmentTypeId, p_ToothNumber, p_Cost, p_DatePerformed, p_Notes);
END",

            @"CREATE PROCEDURE sp_Treatment_Update(
    IN p_TreatmentId INT, IN p_AppointmentId INT, IN p_TreatmentTypeId INT, IN p_ToothNumber VARCHAR(10),
    IN p_Cost DECIMAL(10,2), IN p_DatePerformed DATE, IN p_Notes VARCHAR(500))
BEGIN
    UPDATE Treatments
    SET AppointmentId = p_AppointmentId, TreatmentTypeId = p_TreatmentTypeId, ToothNumber = p_ToothNumber,
        Cost = p_Cost, DatePerformed = p_DatePerformed, Notes = p_Notes
    WHERE TreatmentId = p_TreatmentId;
END",

            @"CREATE PROCEDURE sp_Treatment_Delete(IN p_TreatmentId INT)
BEGIN
    DELETE FROM Treatments WHERE TreatmentId = p_TreatmentId;
END",

            @"CREATE PROCEDURE sp_User_GetAll()
BEGIN
    -- Deliberately does not filter WHERE IsActive = 1 - an admin managing accounts
    -- needs to see deactivated ones too, in case someone needs to be reactivated.
    SELECT UserId, Username, PasswordHash, Role, DentistId, IsActive
    FROM Users
    ORDER BY Username;
END",

            @"CREATE PROCEDURE sp_User_GetById(IN p_UserId INT)
BEGIN
    SELECT UserId, Username, PasswordHash, Role, DentistId, IsActive
    FROM Users
    WHERE UserId = p_UserId;
END",

            @"CREATE PROCEDURE sp_User_GetByUsername(IN p_Username VARCHAR(50))
BEGIN
    -- Deliberately does not filter WHERE IsActive = 1 - AuthService needs to see a
    -- disabled account so it can return ""Invalid username or password"" rather than
    -- silently treating it as ""user not found"".
    SELECT UserId, Username, PasswordHash, Role, DentistId, IsActive
    FROM Users
    WHERE Username = p_Username;
END",

            @"CREATE PROCEDURE sp_User_Add(
    IN p_Username VARCHAR(50), IN p_PasswordHash VARCHAR(255), IN p_Role VARCHAR(20),
    IN p_DentistId INT, IN p_IsActive TINYINT(1))
BEGIN
    INSERT INTO Users (Username, PasswordHash, Role, DentistId, IsActive)
    VALUES (p_Username, p_PasswordHash, p_Role, p_DentistId, p_IsActive);
END",

            @"CREATE PROCEDURE sp_User_Update(
    IN p_UserId INT, IN p_Username VARCHAR(50), IN p_PasswordHash VARCHAR(255), IN p_Role VARCHAR(20),
    IN p_DentistId INT, IN p_IsActive TINYINT(1))
BEGIN
    UPDATE Users
    SET Username = p_Username, PasswordHash = p_PasswordHash, Role = p_Role,
        DentistId = p_DentistId, IsActive = p_IsActive
    WHERE UserId = p_UserId;
END"
        ];
    }
}
