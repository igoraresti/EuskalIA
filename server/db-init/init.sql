-- Script to seed EuskalIA Database for SQL Server
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EuskalIA')
BEGIN
  CREATE DATABASE EuskalIA;
END
GO

USE EuskalIA;
GO

-- Seed Data (Only if tables exist and are empty)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Users)
    BEGIN
        -- Insert Admin User (Password is 'EuskalIA2026!' hashed with BCrypt - mocked for simplicity)
        INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt, IsActive, PreferredLanguage)
        VALUES ('igoraresti', 'admin@euskalia.eus', '$2a$11$9/o5n1VpL5hU.l1L1L1L1uL1L1L1L1L1L1L1L1L1L1L1L1L1L1L1L', 'Admin', GETDATE(), 1, 'es');

        -- Insert Progress for Admin
        DECLARE @UserId INT = SCOPE_IDENTITY();
        INSERT INTO Progresses (UserId, XP, Level, Streak, LastActivityDate)
        VALUES (@UserId, 500, 'A1', 5, GETDATE());
    END
END

GO
