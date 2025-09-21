using BlazorReport.Shared;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BlazorReport.Server.Services
{
    public interface IStudentDataService
    {
        Task<DropdownData> GetDropdownDataAsync();
        Task<StudentSearchResult> SearchStudentsAsync(StudentSearchCriteria criteria);
        Task<int> GetStudentCountAsync(StudentSearchCriteria criteria);
        Task<StudentSearchResult> SearchIndividualStudentAsync(IndividualStudentSearchCriteria criteria);
        Task<StudentDetailInfo?> GetStudentDetailAsync(int studentId);
        Task<StudentSearchResult> SearchStudentsByProgramAsync(string programType, string school, string grade);
        Task<Dictionary<string, string>> DiagnoseDatabaseAsync();
        
        // Cascading dropdown method
        Task<List<DropdownItem>> GetCascadingDropdownDataAsync(string level, string? school = null, string? grade = null, string? teacher = null);
    }

    public class StudentDataService : IStudentDataService
    {
        private readonly string _connectionString;
        private readonly ILogger<StudentDataService> _logger;

        public StudentDataService(IConfiguration configuration, ILogger<StudentDataService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("ConnectionString not found");
            _logger = logger;
        }

        public async Task<DropdownData> GetDropdownDataAsync()
        {
            var dropdownData = new DropdownData();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetStudentDropdownData", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();

                // Read Schools
                while (await reader.ReadAsync())
                {
                    dropdownData.Schools.Add(new DropdownItem
                    {
                        Value = reader["Value"].ToString() ?? "",
                        Text = reader["Text"].ToString() ?? ""
                    });
                }

                // Read Grades
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    dropdownData.Grades.Add(new DropdownItem
                    {
                        Value = reader["Value"].ToString() ?? "",
                        Text = reader["Text"].ToString() ?? ""
                    });
                }

                // Read Teachers
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    dropdownData.Teachers.Add(new DropdownItem
                    {
                        Value = reader["Value"].ToString() ?? "",
                        Text = reader["Text"].ToString() ?? ""
                    });
                }

                // Read Classes
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    dropdownData.Classes.Add(new DropdownItem
                    {
                        Value = reader["Value"].ToString() ?? "",
                        Text = reader["Text"].ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dropdown data");
                throw;
            }

            return dropdownData;
        }

        public async Task<StudentSearchResult> SearchStudentsAsync(StudentSearchCriteria criteria)
        {
            var result = new StudentSearchResult();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_SearchStudents", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@School", criteria.School);
                command.Parameters.AddWithValue("@Grade", criteria.Grade);
                command.Parameters.AddWithValue("@Teacher", criteria.Teacher);
                command.Parameters.AddWithValue("@Class", criteria.Class);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var student = new StudentInfo
                    {
                        RowID = reader.GetInt64(reader.GetOrdinal("RowID")),
                        Start_Year = reader.GetInt32(reader.GetOrdinal("Start_Year")),
                        LatestStudent = reader.IsDBNull(reader.GetOrdinal("LatestStudent"))
                            ? (bool?)null
                            : reader.GetInt32(reader.GetOrdinal("LatestStudent")) == 1,  // SQL returns int → convert to bool

                                            iDASHsid = reader.IsDBNull(reader.GetOrdinal("iDASHsid"))
                            ? null
                            : reader[reader.GetOrdinal("iDASHsid")].ToString(),

                                            LAST_NAME = reader.IsDBNull(reader.GetOrdinal("L_Name"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("L_Name")),

                                            FIRST_NAME = reader.IsDBNull(reader.GetOrdinal("F_Name"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("F_Name")),

                                            GRADE = reader.IsDBNull(reader.GetOrdinal("GRADE"))
                            ? 0
                            : Convert.ToInt32(reader["GRADE"]),  // tinyint → int

                                            SCHOOLID = reader.IsDBNull(reader.GetOrdinal("SCHOOL"))
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("SCHOOL")),

                                            HOMEROOM = reader.IsDBNull(reader.GetOrdinal("HOMEROOM"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("HOMEROOM")),

                                            SEX = reader.IsDBNull(reader.GetOrdinal("SEX"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("SEX")),

                                            ETHNIC = reader.IsDBNull(reader.GetOrdinal("ETHNIC"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("ETHNIC")),

                                            SPECIAL_ED = reader.IsDBNull(reader.GetOrdinal("SPECIAL_ED"))
                            ? string.Empty
                            : reader[reader.GetOrdinal("SPECIAL_ED")].ToString(), // tinyint → string

                                            ESL_CODE = reader.IsDBNull(reader.GetOrdinal("ESL_CODE"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("ESL_CODE")),

                        FOOD_SERVICE_CODE = reader.IsDBNull(reader.GetOrdinal("FOOD_SERVICE_CODE")) ? string.Empty : reader.GetString(reader.GetOrdinal("FOOD_SERVICE_CODE")),

                        Home = reader.IsDBNull(reader.GetOrdinal("Home"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("Home")),

                                            IS_504 = reader.IsDBNull(reader.GetOrdinal("IS_504"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("IS_504")),

                                            SchoolName = reader.IsDBNull(reader.GetOrdinal("SchoolName"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("SchoolName")),

                                            TeacherName = reader.IsDBNull(reader.GetOrdinal("TeacherName"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("TeacherName")),

                                            ClassName = reader.IsDBNull(reader.GetOrdinal("ClassName"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("ClassName")),
                                        };


                    result.Students.Add(student);
                }

                result.TotalCount = result.Students.Count;
                result.Success = true;
                result.Message = "Search completed successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students with criteria: {@Criteria}", criteria);
                result.Success = false;
                result.Message = $"Error occurred: {ex.Message}";
            }

            return result;
        }

        public async Task<int> GetStudentCountAsync(StudentSearchCriteria criteria)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetStudentCount", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@School", criteria.School);
                command.Parameters.AddWithValue("@Grade", criteria.Grade);
                command.Parameters.AddWithValue("@Teacher", criteria.Teacher);
                command.Parameters.AddWithValue("@Class", criteria.Class);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student count with criteria: {@Criteria}", criteria);
                return 0;
            }
        }

        public async Task<StudentSearchResult> SearchIndividualStudentAsync(IndividualStudentSearchCriteria criteria)
        {
            var result = new StudentSearchResult();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_SearchIndividualStudent", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@SearchTerm", criteria.SearchTerm);
                command.Parameters.AddWithValue("@SearchType", criteria.SearchType);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var student = new StudentInfo
                    {
                        RowID = Convert.ToInt32(reader["RowID"]),
                        Start_Year = reader.GetInt32("Start_Year"),
                        LatestStudent = reader.IsDBNull("LatestStudent") ? null : Convert.ToBoolean(reader.GetInt32("LatestStudent")),
                        iDASHsid = reader.IsDBNull("iDASHsid") ? null : reader.GetString("iDASHsid"),    
                        LAST_NAME = reader.IsDBNull("L_Name") ? "" : reader.GetString("L_Name"),
                        FIRST_NAME = reader.IsDBNull("F_name") ? "" : reader.GetString("F_name"),
                        GRADE = reader.IsDBNull("GRADE") ? 0 : TryParseGrade(reader.GetString("GRADE")),
                        SCHOOLID = reader.GetInt32("SCHOOL"),
                        HOMEROOM = reader.IsDBNull("HOMEROOM") ? "" : reader.GetString("HOMEROOM"),
                        SEX = reader.IsDBNull("SEX") ? "" : reader.GetString("SEX"),
                        ETHNIC = reader.IsDBNull("ETHNIC") ? "" : reader.GetString("ETHNIC"),
                        SPECIAL_ED = reader.IsDBNull("SPECIAL_ED") ? "" : reader.GetInt32("SPECIAL_ED").ToString(),
                        ESL_CODE = reader.IsDBNull("ESL_CODE") ? "" : reader.GetString("ESL_CODE"),
                        FOOD_SERVICE_CODE = reader.IsDBNull(reader.GetOrdinal("FOOD_SERVICE_CODE")) ? string.Empty : reader.GetString(reader.GetOrdinal("FOOD_SERVICE_CODE")),

                        Home = reader.IsDBNull("Home") ? "" : reader.GetString("Home"),
                        IS_504 = reader.IsDBNull("IS_504") ? "" : reader.GetString("IS_504"),
                        SchoolName = reader.IsDBNull("SchoolName") ? "" : reader.GetString("SchoolName"),
                        TeacherName = reader.IsDBNull("TeacherName") ? "" : reader.GetString("TeacherName"),
                        ClassName = reader.IsDBNull("ClassName") ? "" : reader.GetString("ClassName")
                    };

                    result.Students.Add(student);
                }

                result.TotalCount = result.Students.Count;
                result.Success = true;
                result.Message = "Individual search completed successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching individual student with criteria: {@Criteria}", criteria);
                result.Success = false;
                result.Message = $"Error occurred: {ex.Message}";
            }

            return result;
        }

        public async Task<StudentDetailInfo?> GetStudentDetailAsync(int studentId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetStudentDetail", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@StudentID", studentId);

                using var reader = await command.ExecuteReaderAsync();

                StudentDetailInfo? studentDetail = null;

                // Read student basic information
                if (await reader.ReadAsync())
                {
                    studentDetail = new StudentDetailInfo
                    {
                        RowID = Convert.ToInt32(reader["RowID"]),
                        Start_Year = reader.GetInt32("Start_Year"),
                        LatestStudent = reader.IsDBNull("LatestStudent") ? null : Convert.ToBoolean(reader.GetInt32("LatestStudent")),
                        iDASHsid = reader.IsDBNull("iDASHsid") ? null : reader.GetString("iDASHsid"),
                        LAST_NAME = reader.IsDBNull("L_Name") ? "" : reader.GetString("L_Name"),
                        FIRST_NAME = reader.IsDBNull("F_name") ? "" : reader.GetString("F_name"),
                        GRADE = reader.IsDBNull("GRADE") ? 0 : TryParseGrade(reader.GetString("GRADE")),
                        SCHOOLID = reader.GetInt32("SCHOOL"),
                        HOMEROOM = reader.IsDBNull("HOMEROOM") ? "" : reader.GetString("HOMEROOM"),
                        SEX = reader.IsDBNull("SEX") ? "" : reader.GetString("SEX"),
                        ETHNIC = reader.IsDBNull("ETHNIC") ? "" : reader.GetString("ETHNIC"),
                        SPECIAL_ED = reader.IsDBNull("SPECIAL_ED") ? "" : reader.GetInt32("SPECIAL_ED").ToString(),
                        ESL_CODE = reader.IsDBNull("ESL_CODE") ? "" : reader.GetString("ESL_CODE"),
                        FOOD_SERVICE_CODE = reader.IsDBNull(reader.GetOrdinal("FOOD_SERVICE_CODE")) ? string.Empty : reader.GetString(reader.GetOrdinal("FOOD_SERVICE_CODE")),
                        Home = reader.GetString("Home") ?? "",
                        IS_504 = reader.GetString("IS_504") ?? "",
                        SchoolName = reader.IsDBNull("SchoolName") ? "" : reader.GetString("SchoolName")
                    };
                }

                // Read course information
                if (studentDetail != null && await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var course = new StudentCourseInfo
                        {
                            DASH_CID = reader.IsDBNull("DASH_CID") ? "" : reader.GetString("DASH_CID"),
                            CourseName = reader.IsDBNull("CourseName") ? "" : reader.GetString("CourseName"),
                            TeacherName = reader.IsDBNull("TeacherName") ? "" : reader.GetString("TeacherName"),
                            Period = reader.IsDBNull("Period") ? "" : reader.GetString("Period"),
                            Days = reader.IsDBNull("Days") ? "" : reader.GetString("Days"),
                            Semester = reader.IsDBNull("Semester") ? "" : reader.GetString("Semester")
                        };

                        studentDetail.Courses.Add(course);
                    }

                    studentDetail.TotalCourses = studentDetail.Courses.Count;
                    studentDetail.PrimaryTeacher = studentDetail.Courses.FirstOrDefault()?.TeacherName ?? "";
                }

                return studentDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student detail for ID: {StudentId}", studentId);
                return null;
            }
        }

        public async Task<StudentSearchResult> SearchStudentsByProgramAsync(string programType, string school, string grade)
        {
            var result = new StudentSearchResult();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_SearchStudentsByProgram", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ProgramType", programType);
                command.Parameters.AddWithValue("@School", school);
                command.Parameters.AddWithValue("@Grade", grade);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var student = new StudentInfo
                    {
                        RowID = Convert.ToInt32(reader["RowID"]),
                        Start_Year = reader.GetInt32("Start_Year"),
                        LatestStudent = reader.IsDBNull("LatestStudent") ? null : Convert.ToBoolean(reader.GetInt32("LatestStudent")),
                        iDASHsid = reader.IsDBNull("iDASHsid") ? null : reader.GetString("iDASHsid"),
                        LAST_NAME = reader.IsDBNull("L_Name") ? "" : reader.GetString("L_Name"),
                        FIRST_NAME = reader.IsDBNull("F_name") ? "" : reader.GetString("F_name"),
                        GRADE = reader.IsDBNull("GRADE") ? 0 : TryParseGrade(reader.GetString("GRADE")),
                        SCHOOLID = reader.GetInt32("SCHOOL"),
                        HOMEROOM = reader.IsDBNull("HOMEROOM") ? "" : reader.GetString("HOMEROOM"),
                        SEX = reader.IsDBNull("SEX") ? "" : reader.GetString("SEX"),
                        ETHNIC = reader.IsDBNull("ETHNIC") ? "" : reader.GetString("ETHNIC"),
                        SPECIAL_ED = reader.IsDBNull("SPECIAL_ED") ? "" : reader.GetInt32("SPECIAL_ED").ToString(),
                        ESL_CODE = reader.IsDBNull("ESL_CODE") ? "" : reader.GetString("ESL_CODE"),
                        FOOD_SERVICE_CODE = reader.IsDBNull(reader.GetOrdinal("FOOD_SERVICE_CODE")) ? string.Empty : reader.GetString(reader.GetOrdinal("FOOD_SERVICE_CODE")),
                        Home = reader.IsDBNull("Home") ? "" : reader.GetString("Home"),
                        IS_504 = reader.IsDBNull("IS_504") ? "" : reader.GetString("IS_504"),
                        SchoolName = reader.IsDBNull("SchoolName") ? "" : reader.GetString("SchoolName"),
                        TeacherName = reader.IsDBNull("TeacherName") ? "" : reader.GetString("TeacherName"),
                        ClassName = reader.IsDBNull("ClassName") ? "" : reader.GetString("ClassName")
                    };

                    result.Students.Add(student);
                }

                result.TotalCount = result.Students.Count;
                result.Success = true;
                result.Message = $"Search by {programType} program completed successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students by program: {ProgramType}, School: {School}, Grade: {Grade}", 
                    programType, school, grade);
                result.Success = false;
                result.Message = $"Error occurred: {ex.Message}";
            }

            return result;
        }

        public async Task<Dictionary<string, string>> DiagnoseDatabaseAsync()
        {
            var diagnostics = new Dictionary<string, string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Check if we can connect
                diagnostics["DatabaseConnection"] = "SUCCESS";
                diagnostics["DatabaseName"] = connection.Database;

                // Check what tables exist
                using var command = new SqlCommand(@"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_TYPE = 'BASE TABLE'
                    ORDER BY TABLE_NAME", connection);

                using var reader = await command.ExecuteReaderAsync();
                var tables = new List<string>();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString("TABLE_NAME"));
                }

                diagnostics["AllTables"] = string.Join(", ", tables);
                diagnostics["TableCount"] = tables.Count.ToString();

                // Check specific required tables
                diagnostics["Student"] = tables.Contains("Student") ? "EXISTS" : "MISSING";
                diagnostics["Users"] = tables.Contains("Users") ? "EXISTS" : "MISSING";
                diagnostics["TeacherSchedule"] = tables.Contains("TeacherSchedule") ? "EXISTS" : "MISSING";
                diagnostics["TeacherStudentSchedule"] = tables.Contains("TeacherStudentSchedule") ? "EXISTS" : "MISSING";
            }
            catch (Exception ex)
            {
                diagnostics["DatabaseConnection"] = "FAILED";
                diagnostics["Error"] = ex.Message;
                diagnostics["SampleDataReason"] = "Cannot connect to database - using fallback sample data";
            }

            return diagnostics;
        }

        public async Task<List<DropdownItem>> GetCascadingDropdownDataAsync(string level, string? school = null, string? grade = null, string? teacher = null)
        {
            var items = new List<DropdownItem>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetCascadingDropdownData", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Level", level);
                command.Parameters.AddWithValue("@School", school ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Grade", grade ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Teacher", teacher ?? (object)DBNull.Value);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    items.Add(new DropdownItem
                    {
                        Value = reader["Value"].ToString() ?? "",
                        Text = reader["Text"].ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cascading dropdown data for level: {Level}, school: {School}, grade: {Grade}, teacher: {Teacher}", 
                    level, school, grade, teacher);
                // Return at least "All" option on error
                items.Add(new DropdownItem { Value = "All", Text = $"All {level}" });
            }

            return items;
        }

        private static int TryParseGrade(string gradeValue)
        {
            // Handle cases like "Grade 03 Instruction" or just "3"
            if (int.TryParse(gradeValue, out int numericGrade))
            {
                return numericGrade;
            }

            // Try to extract number from strings like "Grade 03 Instruction"
            var parts = gradeValue.Split(' ');
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int extractedGrade))
                {
                    return extractedGrade;
                }
            }

            // Default to 0 if we can't parse
            return 0;
        }
    }
} 