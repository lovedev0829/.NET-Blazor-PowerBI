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
            u.EmployeeID,
            u.LastName,
            u.FirstName,
            u.Title,
            u.gsdemail,
            u.School,
            u.DashUserID,
            u.prem_act_stat,
            u.URole,
            u.PersonID,
            u.PersonIDChar
        FROM dbo.Users u
        WHERE u.URole LIKE '%Teacher%' 
        OR u.URole LIKE '%Instructor%'
        OR u.EmployeeID IS NOT NULL
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
            ts.id,
            ts.DASH_CID,
            ts.START_YY,
            ts.SCHOOL,
            ts.SHORT_NAME,
            ts.CLASS_CD,
            ts.SECTION,
            ts.SEMESTER,
            ts.PERIOD,
            ts.DAYS,
            ts.EmployeeID,
            u.FirstName + ' ' + u.LastName AS TeacherName,
            COUNT(tss.idashsid) AS StudentCount
        FROM dbo.TeacherSchedule ts
        LEFT JOIN dbo.Users u ON ts.EmployeeID = u.EmployeeID
        LEFT JOIN dbo.TeacherStudentSchedule tss ON ts.DASH_CID = tss.DASH_CID
        WHERE ts.EmployeeID = @EmployeeID
        GROUP BY ts.id, ts.DASH_CID, ts.START_YY, ts.SCHOOL, ts.SHORT_NAME, 
                 ts.CLASS_CD, ts.SECTION, ts.SEMESTER, ts.PERIOD, ts.DAYS, 
                 ts.EmployeeID, u.FirstName, u.LastName
        ORDER BY ts.PERIOD, ts.SHORT_NAME;
        
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
            u.EmployeeID,
            u.LastName,
            u.FirstName,
            u.Title,
            u.gsdemail,
            u.School,
            u.DashUserID,
            u.prem_act_stat,
            u.URole,
            u.PersonID,
            u.PersonIDChar
        FROM dbo.Users u
        WHERE u.EmployeeID = @EmployeeID;
        
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
        ORDER BY u.School;
        
        -- Subjects (from TeacherSchedule)
        SELECT DISTINCT 
            ts.SHORT_NAME AS Value,
            ts.SHORT_NAME AS Text
        FROM dbo.TeacherSchedule ts
        WHERE ts.SHORT_NAME IS NOT NULL 
        AND ts.SHORT_NAME != ''
        ORDER BY ts.SHORT_NAME;
        
        -- Grades (based on classes taught)
        SELECT DISTINCT 
            CAST(s.GRADE AS VARCHAR(10)) AS Value,
            CASE 
                WHEN s.GRADE = -1 THEN 'Pre-K'
                WHEN s.GRADE = 0 THEN 'Kindergarten'
                ELSE 'Grade ' + CAST(s.GRADE AS VARCHAR(10))
            END AS Text
        FROM dbo.Student s
        INNER JOIN dbo.TeacherStudentSchedule tss ON s.StudentID = tss.idashsid
        INNER JOIN dbo.TeacherSchedule ts ON tss.DASH_CID = ts.DASH_CID
        WHERE s.GRADE IS NOT NULL
        ORDER BY CAST(s.GRADE AS VARCHAR(10));
        
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
            u.EmployeeID,
            u.FirstName + ' ' + u.LastName AS TeacherName,
            u.School,
            COUNT(DISTINCT ts.id) AS TotalClasses,
            COUNT(DISTINCT tss.idashsid) AS TotalStudents,
            AVG(CAST(COUNT(tss.idashsid) AS FLOAT)) AS AvgStudentsPerClass,
            STRING_AGG(ts.SHORT_NAME, ', ') AS SubjectsTaught
        FROM dbo.Users u
        LEFT JOIN dbo.TeacherSchedule ts ON u.EmployeeID = ts.EmployeeID
        LEFT JOIN dbo.TeacherStudentSchedule tss ON ts.DASH_CID = tss.DASH_CID
        WHERE u.URole LIKE '%Teacher%' OR u.URole LIKE '%Instructor%'
        GROUP BY u.EmployeeID, u.FirstName, u.LastName, u.School
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
            s.StudentID,
            s.FIRST_NAME,
            s.LAST_NAME,
            s.GRADE,
            s.HOMEROOM,
            ts.SHORT_NAME AS ClassName,
            ts.PERIOD,
            ts.SEMESTER
        FROM dbo.Student s
        INNER JOIN dbo.TeacherStudentSchedule tss ON s.StudentID = tss.idashsid
        INNER JOIN dbo.TeacherSchedule ts ON tss.DASH_CID = ts.DASH_CID
        WHERE ts.EmployeeID = @EmployeeID
        ORDER BY ts.PERIOD, s.LAST_NAME, s.FIRST_NAME;
        
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