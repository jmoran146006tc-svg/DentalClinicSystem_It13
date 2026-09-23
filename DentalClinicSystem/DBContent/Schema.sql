-- Schema.sql
-- Manual-run reference version of DatabaseInitializer's table creation.
-- The app creates all of this automatically on startup - you do not need to run
-- this by hand unless you want to inspect it directly in MySQL Workbench.

CREATE DATABASE IF NOT EXISTS dentalclinicdb CHARACTER SET utf8mb4;
USE dentalclinicdb;

CREATE TABLE IF NOT EXISTS Patients (
    PatientId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    ContactNumber VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NULL,
    Address VARCHAR(200) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Dentists (
    DentistId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Specialization VARCHAR(100) NULL,
    ContactNumber VARCHAR(20) NULL,
    LicenseNumber VARCHAR(50) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS TreatmentTypes (
    TreatmentTypeId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    DefaultCost DECIMAL(10,2) NOT NULL,
    Description VARCHAR(255) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Appointments (
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Treatments (
    TreatmentId INT AUTO_INCREMENT PRIMARY KEY,
    AppointmentId INT NOT NULL,
    TreatmentTypeId INT NOT NULL,
    ToothNumber VARCHAR(10) NULL,
    Cost DECIMAL(10,2) NOT NULL,
    DatePerformed DATE NOT NULL,
    Notes VARCHAR(500) NULL,
    CONSTRAINT FK_Treatments_Appointment FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
    CONSTRAINT FK_Treatments_TreatmentType FOREIGN KEY (TreatmentTypeId) REFERENCES TreatmentTypes(TreatmentTypeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    DentistId INT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT FK_Users_Dentist FOREIGN KEY (DentistId) REFERENCES Dentists(DentistId),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin','Receptionist','Dentist'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX IX_Appointments_DentistId_DateTime ON Appointments(DentistId, AppointmentDateTime);
CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
CREATE INDEX IX_Treatments_AppointmentId ON Treatments(AppointmentId);
