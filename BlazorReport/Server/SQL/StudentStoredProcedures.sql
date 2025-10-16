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
        CAST(s.SCHOOL AS NVARCHAR(50)) AS Value,
        'School ' + CAST(s.SCHOOL AS NVARCHAR(10)) AS Text
    FROM Student s
    WHERE s.SCHOOL IS NOT NULL
    ORDER BY Value
    
    -- Get Grades
    SELECT DISTINCT 
        CAST(s.GRADE AS NVARCHAR(50)) AS Value,
        CAST(s.GRADE AS NVARCHAR(50)) AS Text
    FROM Student s
    WHERE s.GRADE IS NOT NULL
    ORDER BY Value
    
    -- Get Teachers
    SELECT DISTINCT 
        CAST(u.employeeID AS NVARCHAR(50)) AS Value,
        u.FirstName + ' ' + u.LastName AS Text
    FROM Users u
    INNER JOIN TeacherSchedule ts ON u.employeeID = ts.EmployeeID
    INNER JOIN StudentSchedule ss ON ts.DASH_CID = ss.DASH_CID
    WHERE u.employeeID IS NOT NULL
    ORDER BY Text
    
    -- Get Classes
    SELECT DISTINCT 
        ts.DASH_CID AS Value,
        ts.SHORT_NAME + 
        CASE 
            WHEN ts.SECTION IS NOT NULL AND ts.SECTION != '' 
            THEN ' - Section ' + CAST (ts.SECTION AS nvarchar(50)) 
            ELSE '' 
        END AS Text
    FROM TeacherSchedule ts
    INNER JOIN StudentSchedule ss ON ts.DASH_CID = ss.DASH_CID
    WHERE ts.DASH_CID IS NOT NULL
    ORDER BY Text
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
    
    SELECT DISTINCT
        ROW_NUMBER() OVER (ORDER BY s.L_Name, s.F_name) AS RowID,
        YEAR(GETDATE()) AS Start_Year,
        1 AS LatestStudent,
        s.StuNum AS iDASHsid,
        s.L_Name,
        s.F_name,
        CAST(s.GRADE AS NVARCHAR(50)) AS GRADE,
        s.SCHOOL,
        s.HOMEROOM,
        s.SEX,
        s.ETHNIC,
        s.SPECIAL_ED,
        s.ESL_CODE,
        s.FOOD_SERVICE_CODE,
        s.Home,
        s.IS_504,
        -- Additional fields for display
        CASE s.SCHOOL 
            WHEN 1 THEN 'Elementary School'
            WHEN 2 THEN 'Middle School' 
            WHEN 3 THEN 'High School'
            ELSE 'School ' + CAST(s.SCHOOL AS NVARCHAR(10))
        END AS SchoolName,
        COALESCE(u.FirstName + ' ' + u.LastName, '') AS TeacherName,
        COALESCE(ts.SHORT_NAME, '') AS ClassName
    FROM Student s
    LEFT JOIN StudentSchedule ss ON s.StuNum = ss.idashsid
    LEFT JOIN TeacherSchedule ts ON ss.DASH_CID = ts.DASH_CID
    LEFT JOIN Users u ON ts.EmployeeID = u.employeeID
    WHERE 1 = 1
        AND (@School = 'All' OR CAST(s.SCHOOL AS NVARCHAR(50)) = @School)
        AND (@Grade = 'All' OR CAST(s.GRADE AS NVARCHAR(50)) = @Grade)
        AND (@Teacher = 'All' OR CAST(ts.EmployeeID AS NVARCHAR(50)) = @Teacher)
        AND (@Class = 'All' OR ts.DASH_CID = @Class)
    ORDER BY s.L_Name, s.F_name
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
    
    SELECT COUNT(DISTINCT s.StuNum) AS StudentCount
    FROM Student s
    LEFT JOIN StudentSchedule ss ON s.StuNum = ss.idashsid
    LEFT JOIN TeacherSchedule ts ON ss.DASH_CID = ts.DASH_CID
    WHERE 1 = 1
        AND (@School = 'All' OR CAST(s.SCHOOL AS NVARCHAR(50)) = @School)
        AND (@Grade = 'All' OR CAST(s.GRADE AS NVARCHAR(50)) = @Grade)
        AND (@Teacher = 'All' OR CAST(ts.EmployeeID AS NVARCHAR(50)) = @Teacher)
        AND (@Class = 'All' OR ts.DASH_CID = @Class)
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
        ROW_NUMBER() OVER (ORDER BY s.L_Name, s.F_name) AS RowID,
        YEAR(GETDATE()) AS Start_Year,
        1 AS LatestStudent,
        s.StuNum AS iDASHsid,
        s.L_Name,
        s.F_name,
        CAST(s.GRADE AS NVARCHAR(50)) AS GRADE,
        s.SCHOOL,
        s.HOMEROOM,
        s.SEX,
        s.ETHNIC,
        s.SPECIAL_ED,
        s.ESL_CODE,
        s.FOOD_SERVICE_CODE,
        s.Home,
        s.IS_504,
        -- Additional fields for display
        CASE s.SCHOOL 
            WHEN 1 THEN 'Elementary School'
            WHEN 2 THEN 'Middle School' 
            WHEN 3 THEN 'High School'
            ELSE 'School ' + CAST(s.SCHOOL AS NVARCHAR(10))
        END AS SchoolName,
        '' AS TeacherName,
        '' AS ClassName
    FROM Student s
    WHERE (
        (@SearchType = 'Name' AND (s.L_Name LIKE @SearchPattern OR s.F_name LIKE @SearchPattern))
        OR (@SearchType = 'ID' AND (s.L_Name LIKE @SearchPattern OR s.F_name LIKE @SearchPattern))
        OR (@SearchType = 'Email' AND s.Home LIKE @SearchPattern)
        OR (@SearchType = 'Homeroom' AND s.HOMEROOM LIKE @SearchPattern)
    )
    ORDER BY s.L_Name, s.F_name
END
GO 

-- Stored Procedure for Cascading Dropdown Data
IF OBJECT_ID('dbo.SP_GetCascadingDropdownData', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetCascadingDropdownData;
GO

CREATE PROCEDURE [dbo].[SP_GetCascadingDropdownData]
    @Level NVARCHAR(20), -- 'Schools', 'Grades', 'Teachers', 'Classes'
    @School NVARCHAR(50) = NULL,
    @Grade NVARCHAR(50) = NULL,
    @Teacher NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Level = 'Schools'
    BEGIN
        -- Get all schools from Student table
        SELECT DISTINCT 
            CAST(s.SCHOOL AS NVARCHAR(50)) AS Value,
            CASE s.SCHOOL 
                WHEN 1 THEN 'Elementary School'
                WHEN 2 THEN 'Middle School' 
                WHEN 3 THEN 'High School'
                ELSE 'School ' + CAST(s.SCHOOL AS NVARCHAR(10))
            END AS Text
        FROM Student s
        WHERE s.SCHOOL IS NOT NULL
        ORDER BY Value
    END
    ELSE IF @Level = 'Grades'
    BEGIN
        -- Get grades for selected school
        SELECT DISTINCT 
            CAST(s.GRADE AS NVARCHAR(50)) AS Value,
            CAST(s.GRADE AS NVARCHAR(50)) AS Text
        FROM Student s
        WHERE s.GRADE IS NOT NULL
          AND (@School IS NULL OR @School = 'All' OR s.SCHOOL = CAST(@School AS INT))
        ORDER BY Value
    END
    ELSE IF @Level = 'Teachers'
    BEGIN
        -- Get teachers for selected school and grade using correct relationships
        -- Student.StuNum -> StudentSchedule.idashsid -> StudentSchedule.DashCID -> TeacherSchedule.DASH_CID -> TeacherSchedule.EmployeeID -> Users.EmployeeID
        SELECT DISTINCT 
            CAST(u.employeeID AS NVARCHAR(50)) AS Value,
            u.FirstName + ' ' + u.LastName AS Text
        FROM Users u
        INNER JOIN TeacherSchedule ts ON u.employeeID = ts.EmployeeID
        INNER JOIN StudentSchedule ss ON ts.DASH_CID = ss.DASH_CID
        INNER JOIN Student s ON ss.idashsid = s.StuNum
        WHERE u.employeeID IS NOT NULL
          AND (@School IS NULL OR @School = 'All' OR s.SCHOOL = CAST(@School AS INT))
          AND (@Grade IS NULL OR @Grade = 'All' OR CAST(s.GRADE AS NVARCHAR(50)) = @Grade)
        ORDER BY Text
    END
    ELSE IF @Level = 'Classes'
    BEGIN
        -- Get classes for selected school, grade, and teacher using correct relationships
        SELECT DISTINCT 
            ts.DASH_CID AS Value,
            ts.SHORT_NAME + 
            CASE 
                WHEN ts.SECTION IS NOT NULL AND ts.SECTION != '' 
                THEN ' - Section ' + CAST(ts.SECTION AS nvarchar(50)) 
                ELSE '' 
            END AS Text
        FROM TeacherSchedule ts
        INNER JOIN StudentSchedule ss ON ts.DASH_CID = ss.DASH_CID
        INNER JOIN Student s ON ss.idashsid = s.StuNum
        WHERE ts.DASH_CID IS NOT NULL
          AND (@School IS NULL OR @School = 'All' OR s.SCHOOL = CAST(@School AS INT))
          AND (@Grade IS NULL OR @Grade = 'All' OR CAST(s.GRADE AS NVARCHAR(50)) = @Grade)
          AND (@Teacher IS NULL OR @Teacher = 'All' OR ts.EmployeeID = CAST(@Teacher AS INT))
        ORDER BY Text
    END
END
GO

-- Stored Procedure to search students by program type (ESL, SPED, etc.)
IF OBJECT_ID('dbo.SP_SearchStudentsByProgram', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_SearchStudentsByProgram;
GO

CREATE PROCEDURE [dbo].[SP_SearchStudentsByProgram]
    @ProgramType NVARCHAR(50),
    @School NVARCHAR(50) = 'All',
    @Grade NVARCHAR(50) = 'All'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        ROW_NUMBER() OVER (ORDER BY s.L_Name, s.F_name) AS RowID,
        YEAR(GETDATE()) AS Start_Year,
        1 AS LatestStudent,
        s.StuNum AS iDASHsid,
        s.L_Name,
        s.F_name,
        CAST(s.GRADE AS NVARCHAR(50)) AS GRADE,
        s.SCHOOL,
        s.HOMEROOM,
        s.SEX,
        s.ETHNIC,
        s.SPECIAL_ED,
        s.ESL_CODE,
        s.FOOD_SERVICE_CODE,
        s.Home,
        s.IS_504,
        -- Additional fields for display
        CASE s.SCHOOL 
            WHEN 1 THEN 'Elementary School'
            WHEN 2 THEN 'Middle School' 
            WHEN 3 THEN 'High School'
            ELSE 'School ' + CAST(s.SCHOOL AS NVARCHAR(10))
        END AS SchoolName,
        COALESCE(u.FirstName + ' ' + u.LastName, '') AS TeacherName,
        COALESCE(ts.SHORT_NAME, '') AS ClassName
    FROM Student s
    LEFT JOIN StudentSchedule ss ON s.StuNum = ss.idashsid
    LEFT JOIN TeacherSchedule ts ON ss.DASH_CID = ts.DASH_CID
    LEFT JOIN Users u ON ts.EmployeeID = u.employeeID
    WHERE 1 = 1
        AND (@School = 'All' OR CAST(s.SCHOOL AS NVARCHAR(50)) = @School)
        AND (@Grade = 'All' OR CAST(s.GRADE AS NVARCHAR(50)) = @Grade)
        AND (
            (@ProgramType = 'ESL' AND s.ESL_CODE IS NOT NULL AND s.ESL_CODE != '')
            OR (@ProgramType = 'SPED' AND s.SPECIAL_ED IS NOT NULL AND s.SPECIAL_ED != '')
            OR (@ProgramType = '504' AND s.IS_504 IS NOT NULL AND s.IS_504 != '')
        )
    ORDER BY s.L_Name, s.F_name
END
GO
