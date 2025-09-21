-- =============================================
-- Teacher Management System - Stored Procedures
-- Database: sms_db
-- Tables: dbo.Student, dbo.Users, dbo.TeacherSchedule, dbo.StudentSchedule
-- =============================================

USE sms_db;
GO

-- =============================================
-- SP_GetAllTeachers
-- Get all teachers from the Users table
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetAllTeachers
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            u.employeeID,
            u.LastName,
            u.FirstName,
            u.Title,
            u.gsdemail,
            u.School,
            u.DashUserID,
            u.prem_act_stat,
            u.URole,
            u.PersonIDChar as PersonID,
            u.PersonIDChar
        FROM dbo.Users u
        WHERE u.URole LIKE '%Teacher%' 
        OR u.URole LIKE '%Instructor%'
        OR u.employeeID IS NOT NULL
        ORDER BY u.LastName, u.FirstName;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- SP_GetTeacherSchedule
-- Get teacher's schedule and classes
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetTeacherSchedule
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            ROW_NUMBER() OVER (ORDER BY ts.DASH_CID) as id,
            ts.DASH_CID,
            ts.START_YY,
            ts.SCHOOL,
            ts.SHORT_NAME,
            ts.L as CLASS_CD,
            ts.SECTION,
            ts.SEMESTER,
            ts.DAYS as PERIOD,
            ts.DAYS,
            ts.EmployeeID,
            u.FirstName + ' ' + u.LastName AS TeacherName,
            COUNT(ss.idashsid) AS StudentCount
        FROM dbo.TeacherSchedule ts
        LEFT JOIN dbo.Users u ON ts.EmployeeID = u.employeeID
        LEFT JOIN dbo.StudentSchedule ss ON ts.DASH_CID = ss.DASH_CID
        WHERE ts.EmployeeID = @EmployeeID
        GROUP BY ts.DASH_CID, ts.START_YY, ts.SCHOOL, ts.SHORT_NAME, 
                 ts.L, ts.SECTION, ts.SEMESTER, ts.DAYS, 
                 ts.EmployeeID, u.FirstName, u.LastName
        ORDER BY ts.DAYS, ts.SHORT_NAME;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- SP_GetTeacherById
-- Get specific teacher information
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetTeacherById
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            u.employeeID,
            u.LastName,
            u.FirstName,
            u.Title,
            u.gsdemail,
            u.School,
            u.DashUserID,
            u.prem_act_stat,
            u.URole,
            u.PersonIDChar as PersonID,
            u.PersonIDChar
        FROM dbo.Users u
        WHERE u.employeeID = @EmployeeID;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- SP_GetTeacherDropdownData
-- Get dropdown data for teacher filters
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetTeacherDropdownData
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Schools
        SELECT DISTINCT 
            u.School AS Value,
            u.School AS Text
        FROM dbo.Users u
        WHERE u.School IS NOT NULL 
        AND u.School != ''
        ORDER BY Value;
        
        -- Subjects (from TeacherSchedule)
        SELECT DISTINCT 
            ts.SHORT_NAME AS Value,
            ts.SHORT_NAME AS Text
        FROM dbo.TeacherSchedule ts
        WHERE ts.SHORT_NAME IS NOT NULL 
        AND ts.SHORT_NAME != ''
        ORDER BY Value;
        
        -- Grades (based on classes taught) - FIXED: Handle string values
        SELECT DISTINCT 
            CAST(s.GRADE AS NVARCHAR(50)) AS Value,
            CAST(s.GRADE AS NVARCHAR(50)) AS Text
        FROM dbo.Student s
        INNER JOIN dbo.StudentSchedule ss ON s.StuNum = ss.idashsid
        INNER JOIN dbo.TeacherSchedule ts ON ss.DASH_CID = ts.DASH_CID
        WHERE s.GRADE IS NOT NULL
        ORDER BY Value;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- SP_GetTeacherWorkloadAnalysis
-- Get teacher workload analysis
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetTeacherWorkloadAnalysis
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            u.employeeID,
            u.FirstName + ' ' + u.LastName AS TeacherName,
            u.School,
            COUNT(DISTINCT ts.DASH_CID) AS TotalClasses,
            COUNT(DISTINCT ss.idashsid) AS TotalStudents,
            CASE 
                WHEN COUNT(DISTINCT ts.DASH_CID) > 0 
                THEN CAST(COUNT(DISTINCT ss.idashsid) AS FLOAT) / COUNT(DISTINCT ts.DASH_CID)
                ELSE 0 
            END AS AvgStudentsPerClass,
            STRING_AGG(ts.SHORT_NAME, ', ') AS SubjectsTaught
        FROM dbo.Users u
        LEFT JOIN dbo.TeacherSchedule ts ON u.employeeID = ts.EmployeeID
        LEFT JOIN dbo.StudentSchedule ss ON ts.DASH_CID = ss.DASH_CID
        WHERE u.URole LIKE '%Teacher%' OR u.URole LIKE '%Instructor%'
        GROUP BY u.employeeID, u.FirstName, u.LastName, u.School
        ORDER BY TotalStudents DESC, u.LastName, u.FirstName;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- SP_GetTeacherStudents
-- Get students for a specific teacher
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetTeacherStudents
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT DISTINCT
            s.StuNum AS StudentID,
            s.F_name,
            s.L_Name,
            CAST(s.GRADE AS NVARCHAR(50)) AS GRADE,
            s.HOMEROOM,
            ts.SHORT_NAME AS ClassName,
            ts.DAYS as PERIOD,
            ts.SEMESTER
        FROM dbo.Student s
        INNER JOIN dbo.StudentSchedule ss ON s.StuNum = ss.idashsid
        INNER JOIN dbo.TeacherSchedule ts ON ss.DASH_CID = ts.DASH_CID
        WHERE ts.EmployeeID = @EmployeeID
        ORDER BY ts.DAYS, s.L_Name, s.F_name;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

PRINT 'Teacher stored procedures created successfully for database: sms_db';
PRINT 'Tables used: dbo.Student, dbo.Users, dbo.TeacherSchedule, dbo.StudentSchedule';
PRINT 'Ready to execute in SQL Server Management Studio!'; 