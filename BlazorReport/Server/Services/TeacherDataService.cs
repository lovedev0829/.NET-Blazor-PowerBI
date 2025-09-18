using BlazorReport.Shared;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BlazorReport.Server.Services
{
    public interface ITeacherDataService
    {
        Task<TeacherSearchResult> GetAllTeachersAsync();
        Task<TeacherScheduleResult> GetTeacherScheduleAsync(int employeeId);
        Task<TeacherInfo?> GetTeacherByIdAsync(int employeeId);
        Task<DropdownData> GetTeacherDropdownDataAsync();
    }

    public class TeacherDataService : ITeacherDataService
    {
        private readonly string _connectionString;
        private readonly ILogger<TeacherDataService> _logger;

        public TeacherDataService(IConfiguration configuration, ILogger<TeacherDataService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("ConnectionString not found");
            _logger = logger;
        }

        public async Task<TeacherSearchResult> GetAllTeachersAsync()
        {
            var result = new TeacherSearchResult();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetAllTeachers", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var teacher = new TeacherInfo
                    {
                        EmployeeID = reader.GetInt32("EmployeeID"),
                        LastName = reader.GetString("LastName") ?? "",
                        FirstName = reader.GetString("FirstName") ?? "",
                        Title = reader.GetString("Title") ?? "",
                        gsdemail = reader.GetString("gsdemail") ?? "",
                        School = reader.GetString("School") ?? "",
                        DashUserID = reader.GetString("DashUserID") ?? "",
                        prem_act_stat = reader.GetString("prem_act_stat") ?? "",
                        URole = reader.GetString("URole") ?? "",
                        PersonID = reader.IsDBNull("PersonID") ? null : reader.GetInt32("PersonID"),
                        PersonIDChar = reader.GetString("PersonIDChar") ?? ""
                    };

                    result.Teachers.Add(teacher);
                }

                result.TotalCount = result.Teachers.Count;
                result.Success = true;
                result.Message = "Teachers retrieved successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teachers");
                result.Success = false;
                result.Message = $"Error occurred: {ex.Message}";
            }

            return result;
        }

        public async Task<TeacherScheduleResult> GetTeacherScheduleAsync(int employeeId)
        {
            var result = new TeacherScheduleResult();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetTeacherSchedule", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@EmployeeID", employeeId);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var schedule = new TeacherSchedule
                    {
                        id = reader.GetInt32("id"),
                        DASH_CID = reader.GetString("DASH_CID") ?? "",
                        START_YY = reader.GetInt32("START_YY"),
                        SCHOOL = reader.GetString("SCHOOL") ?? "",
                        SHORT_NAME = reader.GetString("SHORT_NAME") ?? "",
                        CLASS_CD = reader.GetString("CLASS_CD") ?? "",
                        SECTION = reader.GetString("SECTION") ?? "",
                        SEMESTER = reader.GetString("SEMESTER") ?? "",
                        PERIOD = reader.IsDBNull("PERIOD") ? null : reader.GetInt32("PERIOD"),
                        DAYS = reader.GetString("DAYS") ?? "",
                        EmployeeID = reader.GetInt32("EmployeeID"),
                        TeacherName = reader.GetString("TeacherName") ?? "",
                        StudentCount = reader.GetInt32("StudentCount")
                    };

                    result.Schedules.Add(schedule);
                }

                result.TotalCount = result.Schedules.Count;
                result.Success = true;
                result.Message = "Teacher schedule retrieved successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher schedule for EmployeeID {EmployeeID}", employeeId);
                result.Success = false;
                result.Message = $"Error occurred: {ex.Message}";
            }

            return result;
        }

        public async Task<TeacherInfo?> GetTeacherByIdAsync(int employeeId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetTeacherById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@EmployeeID", employeeId);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new TeacherInfo
                    {
                        EmployeeID = reader.GetInt32("EmployeeID"),
                        LastName = reader.GetString("LastName") ?? "",
                        FirstName = reader.GetString("FirstName") ?? "",
                        Title = reader.GetString("Title") ?? "",
                        gsdemail = reader.GetString("gsdemail") ?? "",
                        School = reader.GetString("School") ?? "",
                        DashUserID = reader.GetString("DashUserID") ?? "",
                        prem_act_stat = reader.GetString("prem_act_stat") ?? "",
                        URole = reader.GetString("URole") ?? "",
                        PersonID = reader.IsDBNull("PersonID") ? null : reader.GetInt32("PersonID"),
                        PersonIDChar = reader.GetString("PersonIDChar") ?? ""
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher by ID {EmployeeID}", employeeId);
                return null;
            }
        }

        public async Task<DropdownData> GetTeacherDropdownDataAsync()
        {
            var dropdownData = new DropdownData();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GetTeacherDropdownData", connection)
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

                // Read Subjects
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    dropdownData.Subjects.Add(new DropdownItem
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher dropdown data");
                throw;
            }

            return dropdownData;
        }
    }
} 