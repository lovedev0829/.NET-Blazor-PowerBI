using System.ComponentModel.DataAnnotations;

namespace BlazorReport.Shared
{
    // ConsolidatedStudentBody table model
    public class StudentInfo
    {
        public int RowID { get; set; }
        public int Start_Year { get; set; }
        public bool? LatestStudent { get; set; }
        public int? iDASHsid { get; set; }
        public string LAST_NAME { get; set; } = string.Empty;
        public string FIRST_NAME { get; set; } = string.Empty;
        public int GRADE { get; set; }
        public int SCHOOLID { get; set; }
        public string HOMEROOM { get; set; } = string.Empty;
        public string SEX { get; set; } = string.Empty;
        public string ETHNIC { get; set; } = string.Empty;
        public string SPECIAL_ED { get; set; } = string.Empty;
        public string ESL_CODE { get; set; } = string.Empty;
        public int? FOOD_SERVICE_CODE { get; set; }
        public string Home { get; set; } = string.Empty;
        public string IS_504 { get; set; } = string.Empty;
        
        // Computed properties for display
        public string FullName => $"{FIRST_NAME} {LAST_NAME}";
        public string SchoolName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }

    // Users table model
    public class TeacherInfo
    {
        public int EmployeeID { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string gsdemail { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string DashUserID { get; set; } = string.Empty;
        public string prem_act_stat { get; set; } = string.Empty;
        public string URole { get; set; } = string.Empty;
        public int? PersonID { get; set; }
        public string PersonIDChar { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
        public bool IsActive => prem_act_stat == "A";
    }

    // TeacherSchedule table model
    public class TeacherSchedule
    {
        public int id { get; set; }
        public string DASH_CID { get; set; } = string.Empty;
        public int START_YY { get; set; }
        public string SCHOOL { get; set; } = string.Empty;
        public string SHORT_NAME { get; set; } = string.Empty;
        public string CLASS_CD { get; set; } = string.Empty;
        public string SECTION { get; set; } = string.Empty;
        public string SEMESTER { get; set; } = string.Empty;
        public int? PERIOD { get; set; }
        public string DAYS { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        
        // Navigation properties
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }

    // TeacherStudentSchedule table model - Junction table
    public class TeacherStudentSchedule
    {
        public int id { get; set; }
        public string DASH_CID { get; set; } = string.Empty;
        public int idashsid { get; set; }
        public int START_YY { get; set; }
    }

    // For backwards compatibility - renamed from ClassSchedule
    public class ClassSchedule : TeacherSchedule { }

    // Search criteria models
    public class StudentSearchCriteria
    {
        public string School { get; set; } = "All";
        public string Grade { get; set; } = "All";
        public string Teacher { get; set; } = "All";
        public string Class { get; set; } = "All";
    }

    public class TeacherSearchCriteria
    {
        public string School { get; set; } = "All";
        public string Subject { get; set; } = "All";
        public string Grade { get; set; } = "All";
        public string Status { get; set; } = "All";
    }

    // Result models
    public class StudentSearchResult
    {
        public List<StudentInfo> Students { get; set; } = new();
        public int TotalCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TeacherSearchResult
    {
        public List<TeacherInfo> Teachers { get; set; } = new();
        public int TotalCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TeacherScheduleResult
    {
        public List<TeacherSchedule> Schedules { get; set; } = new();
        public int TotalCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Dropdown data models
    public class DropdownData
    {
        public List<DropdownItem> Schools { get; set; } = new();
        public List<DropdownItem> Grades { get; set; } = new();
        public List<DropdownItem> Teachers { get; set; } = new();
        public List<DropdownItem> Classes { get; set; } = new();
        public List<DropdownItem> Subjects { get; set; } = new();
    }

    public class DropdownItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    // Individual student search models
    public class IndividualStudentSearchCriteria
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string SearchType { get; set; } = "Name"; // Name, ID, Email
    }

    // Student detail with schedule information
    public class StudentDetailInfo : StudentInfo
    {
        public List<StudentCourseInfo> Courses { get; set; } = new();
        public string PrimaryTeacher { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
    }

    public class StudentCourseInfo
    {
        public string DASH_CID { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Days { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
    }
} 