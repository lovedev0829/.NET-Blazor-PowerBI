# Database Setup Guide

## Overview
This School Management System (SMS) connects to SQL Server database `sms_db` with the following tables:
- `dbo.Student` - Student information and demographics
- `dbo.Users` - User accounts (teachers, administrators, etc.)
- `dbo.TeacherSchedule` - Teacher class schedules and assignments
- `dbo.StudentSchedule` - Student-to-class enrollment relationships

## Prerequisites
- SQL Server (LocalDB, Express, or full version)
- SQL Server Management Studio (SSMS)
- Existing database: `sms_db`

## Connection String
Update your `appsettings.json` with your actual database connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=sms_db;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"
  }
}
```

## Database Schema Assumptions

### dbo.Student Table
Expected columns:
- `StudentID` (Primary Key)
- `FIRST_NAME`, `LAST_NAME`
- `GRADE` (numeric grade level)
- `SCHOOLID` (links to Users table)
- `HOMEROOM`
- `SEX`, `ETHNIC`
- `SPECIAL_ED`, `ESL_CODE`, `IS_504`
- `FOOD_SERVICE_CODE`
- `Home`, `Email`

### dbo.Users Table
Expected columns:
- `EmployeeID` (for teachers)
- `PersonID` (for schools/general users)
- `FirstName`, `LastName`
- `Title`, `URole`
- `School`
- `gsdemail`
- `DashUserID`
- `prem_act_stat`
- `PersonIDChar`

### dbo.TeacherSchedule Table
Expected columns:
- `id` (Primary Key)
- `EmployeeID` (links to Users table)
- `DASH_CID` (Course ID)
- `START_YY` (Start Year)
- `SCHOOL`
- `SHORT_NAME` (Course Name)
- `CLASS_CD`, `SECTION`
- `SEMESTER`
- `PERIOD` (numeric period)
- `DAYS`

### dbo.StudentSchedule Table
Expected columns:
- `StudentID` (links to Student table)
- `TeacherScheduleID` (links to TeacherSchedule table)

## Setup Instructions

### Step 1: Execute Student Stored Procedures
1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Open the file: `BlazorReport/Server/SQL/StudentStoredProcedures_Simple.sql`
4. Execute the entire script

This creates the following stored procedures:
- `SP_GetStudentDropdownData` - Dropdown filters
- `SP_SearchStudents` - Student search functionality
- `SP_GetStudentCount` - Student count queries
- `SP_SearchIndividualStudent` - Individual student search
- `SP_GetStudentDetail` - Detailed student information
- `SP_SearchStudentsByProgram` - ESL/SPED program searches

### Step 2: Execute Teacher Stored Procedures
1. In SSMS, open the file: `BlazorReport/Server/SQL/TeacherStoredProcedures.sql`
2. Execute the entire script

This creates the following stored procedures:
- `SP_GetAllTeachers` - Teacher listing
- `SP_GetTeacherSchedule` - Teacher schedules
- `SP_GetTeacherById` - Individual teacher info
- `SP_GetTeacherDropdownData` - Teacher filter dropdowns
- `SP_GetTeacherWorkloadAnalysis` - Workload analysis
- `SP_GetTeacherStudents` - Students per teacher

### Step 3: Verify Setup
Run these test queries to verify your setup:

```sql
-- Test student data access
EXEC SP_GetStudentDropdownData;

-- Test teacher data access  
EXEC SP_GetAllTeachers;

-- Count students
EXEC SP_GetStudentCount @School='All', @Grade='All', @Teacher='All', @Class='All';
```

## Features Enabled

### Student Management
- ✅ Search by School/Grade/Teacher/Class
- ✅ Individual student search (Name/ID/Email)
- ✅ ESL student identification
- ✅ Special Education student tracking
- ✅ 504 Plan student support
- ✅ Student detailed view with course enrollment

### Teacher Management  
- ✅ Teacher directory and contact info
- ✅ Class schedules and assignments
- ✅ Student enrollment per class
- ✅ Workload analysis
- ✅ Subject and grade level tracking

### No Year-Based Filtering
- ✅ Removed StartYear dependency
- ✅ Works with current/latest student data
- ✅ Simplified search criteria

## Troubleshooting

### Common Issues

1. **"Could not find stored procedure" error**
   - Ensure you executed the SQL scripts in the correct database
   - Verify you're connected to `sms_db` database
   - Check stored procedure names match exactly

2. **Empty dropdown lists**
   - Verify your table data exists
   - Check table relationships are correct
   - Ensure column names match expected schema

3. **Connection string errors**
   - Update `appsettings.json` with your actual server name
   - Use Windows Authentication or SQL Authentication as appropriate
   - Test connection in SSMS first

4. **Permission errors**
   - Ensure application user has EXECUTE permissions on stored procedures
   - Grant SELECT permissions on required tables

### Data Validation
Run these queries to check your data:

```sql
-- Check student count
SELECT COUNT(*) FROM dbo.Student;

-- Check teacher count  
SELECT COUNT(*) FROM dbo.Users WHERE URole LIKE '%Teacher%';

-- Check schedule relationships
SELECT COUNT(*) FROM dbo.TeacherSchedule;
SELECT COUNT(*) FROM dbo.StudentSchedule;

-- Check data relationships
SELECT TOP 5 s.FIRST_NAME, s.LAST_NAME, u.School
FROM dbo.Student s
LEFT JOIN dbo.Users u ON s.SCHOOLID = u.PersonID;
```

## Support
If you encounter issues:
1. Verify your database schema matches the expected structure
2. Check that your data relationships are correctly established
3. Test stored procedures individually in SSMS
4. Review application logs for detailed error messages 