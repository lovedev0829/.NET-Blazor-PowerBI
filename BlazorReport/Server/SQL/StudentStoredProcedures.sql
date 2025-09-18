-- Stored Procedure to get dropdown data for Student search filters
IF OBJECT_ID('dbo.SP_GetStudentDropdownData', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetStudentDropdownData;
GO

CREATE PROCEDURE [dbo].[SP_GetStudentDropdownData]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get Schools
    SELECT DISTINCT 
        CAST(s.SCHOOLID AS NVARCHAR(50)) AS Value,
        'School ' + CAST(s.SCHOOLID AS NVARCHAR(10)) AS Text
    FROM Student s
    ORDER BY CAST(s.SCHOOLID AS NVARCHAR(50))
    
    -- Get Grades
    SELECT DISTINCT 
        CAST(s.GRADE AS NVARCHAR(50)) AS Value,
        'Grade ' + CAST(s.GRADE AS NVARCHAR(10)) AS Text
    FROM Student s
    ORDER BY CAST(s.GRADE AS NVARCHAR(50))
    
    -- Get Teachers
    SELECT DISTINCT 
        CAST(u.EmployeeID AS NVARCHAR(50)) AS Value,
        u.FirstName + ' ' + u.LastName AS Text
    FROM Users u
    INNER JOIN TeacherSchedule ts ON u.EmployeeID = ts.EmployeeID
    ORDER BY u.FirstName + ' ' + u.LastName
    
    -- Get Classes
    SELECT DISTINCT 
        ts.DASH_CID AS Value,
        ts.SHORT_NAME + ' (' + ts.CLASS_CD + ')' AS Text
    FROM TeacherSchedule ts
    ORDER BY ts.SHORT_NAME + ' (' + ts.CLASS_CD + ')'
END
GO

-- Stored Procedure to search students based on criteria
IF OBJECT_ID('dbo.SP_SearchStudents', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_SearchStudents;
GO

CREATE PROCEDURE [dbo].[SP_SearchStudents]
    @School NVARCHAR(50) = 'All',
    @Grade NVARCHAR(50) = 'All', 
    @Teacher NVARCHAR(50) = 'All',
    @Class NVARCHAR(50) = 'All'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ROW_NUMBER() OVER (ORDER BY s.LAST_NAME, s.FIRST_NAME) AS RowID,
        YEAR(GETDATE()) AS Start_Year,
        1 AS LatestStudent,
        ROW_NUMBER() OVER (ORDER BY s.LAST_NAME, s.FIRST_NAME) AS iDASHsid,
        s.LAST_NAME,
        s.FIRST_NAME,
        s.GRADE,
        s.SCHOOLID,
        s.HOMEROOM,
        s.SEX,
        s.ETHNIC,
        s.SPECIAL_ED,
        s.ESL_CODE,
        s.FOOD_SERVICE_CODE,
        s.Home,
        s.IS_504,
        -- Additional fields for display
        CASE s.SCHOOLID 
            WHEN 1 THEN 'Elementary School'
            WHEN 2 THEN 'Middle School' 
            WHEN 3 THEN 'High School'
            ELSE 'School ' + CAST(s.SCHOOLID AS NVARCHAR(10))
        END AS SchoolName,
        '' AS TeacherName,
        '' AS ClassName
    FROM Student s
    WHERE 1 = 1
        AND (@School = 'All' OR CAST(s.SCHOOLID AS NVARCHAR(50)) = @School)
        AND (@Grade = 'All' OR CAST(s.GRADE AS NVARCHAR(50)) = @Grade)
    ORDER BY s.LAST_NAME, s.FIRST_NAME
END
GO

-- Stored Procedure to get student count based on criteria
IF OBJECT_ID('dbo.SP_GetStudentCount', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetStudentCount;
GO

CREATE PROCEDURE [dbo].[SP_GetStudentCount]
    @School NVARCHAR(50) = 'All',
    @Grade NVARCHAR(50) = 'All', 
    @Teacher NVARCHAR(50) = 'All',
    @Class NVARCHAR(50) = 'All'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS StudentCount
    FROM Student s
    WHERE 1 = 1
        AND (@School = 'All' OR CAST(s.SCHOOLID AS NVARCHAR(50)) = @School)
        AND (@Grade = 'All' OR CAST(s.GRADE AS NVARCHAR(50)) = @Grade)
END
GO

-- Stored Procedure to search individual student
IF OBJECT_ID('dbo.SP_SearchIndividualStudent', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_SearchIndividualStudent;
GO

CREATE PROCEDURE [dbo].[SP_SearchIndividualStudent]
    @SearchTerm NVARCHAR(255),
    @SearchType NVARCHAR(50) = 'Name'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SearchPattern NVARCHAR(257) = '%' + @SearchTerm + '%'
    
    SELECT 
        ROW_NUMBER() OVER (ORDER BY s.LAST_NAME, s.FIRST_NAME) AS RowID,
        YEAR(GETDATE()) AS Start_Year,
        1 AS LatestStudent,
        ROW_NUMBER() OVER (ORDER BY s.LAST_NAME, s.FIRST_NAME) AS iDASHsid,
        s.LAST_NAME,
        s.FIRST_NAME,
        s.GRADE,
        s.SCHOOLID,
        s.HOMEROOM,
        s.SEX,
        s.ETHNIC,
        s.SPECIAL_ED,
        s.ESL_CODE,
        s.FOOD_SERVICE_CODE,
        s.Home,
        s.IS_504,
        -- Additional fields for display
        CASE s.SCHOOLID 
            WHEN 1 THEN 'Elementary School'
            WHEN 2 THEN 'Middle School' 
            WHEN 3 THEN 'High School'
            ELSE 'School ' + CAST(s.SCHOOLID AS NVARCHAR(10))
        END AS SchoolName,
        '' AS TeacherName,
        '' AS ClassName
    FROM Student s
    WHERE (
        (@SearchType = 'Name' AND (s.LAST_NAME LIKE @SearchPattern OR s.FIRST_NAME LIKE @SearchPattern))
        OR (@SearchType = 'ID' AND (s.LAST_NAME LIKE @SearchPattern OR s.FIRST_NAME LIKE @SearchPattern))
        OR (@SearchType = 'Email' AND s.Home LIKE @SearchPattern)
        OR (@SearchType = 'Homeroom' AND s.HOMEROOM LIKE @SearchPattern)
    )
    ORDER BY s.LAST_NAME, s.FIRST_NAME
END
GO 