using System.ComponentModel.DataAnnotations;

namespace BlazorReport.Shared
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public int Grade { get; set; }
        public string Teacher { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string SPED { get; set; } = string.Empty;
        public string ML { get; set; } = string.Empty;
        public string Interventions { get; set; } = string.Empty;
        public string IntvNotes { get; set; } = string.Empty;
        public string Assessments { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
    }

    public class StudentSearchCriteria
    {
        public string School { get; set; } = "All";
        public string Grade { get; set; } = "All";
        public string Teacher { get; set; } = "All";
        public string Class { get; set; } = "All Classes Selected";
    }

    public class StudentSearchResult
    {
        public List<Student> Students { get; set; } = new();
        public int TotalCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DropdownItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
} 