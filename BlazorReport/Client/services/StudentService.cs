using BlazorReport.Shared;
using System.Net.Http.Json;

namespace BlazorReport.Client.Services
{
    public class StudentService
    {
        private readonly HttpClient _httpClient;

        public StudentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<StudentSearchResult> SearchStudentsAsync(StudentSearchCriteria criteria)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/student/search", criteria);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<StudentSearchResult>();
                    return result ?? new StudentSearchResult
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }
                else
                {
                    return new StudentSearchResult
                    {
                        Success = false,
                        Message = $"API Error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new StudentSearchResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<DropdownData?> GetDropdownDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/student/dropdown-data");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DropdownData>();
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> GetStudentCountAsync(StudentSearchCriteria criteria)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/student/count", criteria);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<int>();
                }
                
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<StudentInfo?> GetStudentAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<StudentInfo>();
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<StudentSearchResult> SearchIndividualStudentAsync(IndividualStudentSearchCriteria criteria)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/student/search-individual", criteria);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<StudentSearchResult>();
                    return result ?? new StudentSearchResult
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }
                else
                {
                    return new StudentSearchResult
                    {
                        Success = false,
                        Message = $"API Error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new StudentSearchResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<StudentDetailInfo?> GetStudentDetailAsync(int studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/{studentId}/detail");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<StudentDetailInfo>();
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<StudentSearchResult> SearchStudentsByProgramAsync(string programType, string school = "All", string grade = "All")
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/search-by-program?programType={programType}&school={school}&grade={grade}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<StudentSearchResult>();
                    return result ?? new StudentSearchResult
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }
                else
                {
                    return new StudentSearchResult
                    {
                        Success = false,
                        Message = $"API Error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new StudentSearchResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Cascading dropdown methods
        public async Task<List<DropdownItem>> GetCascadingGradesAsync(string school)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/cascading-grades?school={Uri.EscapeDataString(school)}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<DropdownItem>>() ?? new List<DropdownItem>();
                }
                
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = "All Grades" } };
            }
            catch (Exception)
            {
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = "All Grades" } };
            }
        }

        public async Task<List<DropdownItem>> GetCascadingTeachersAsync(string school, string grade)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/cascading-teachers?school={Uri.EscapeDataString(school)}&grade={Uri.EscapeDataString(grade)}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<DropdownItem>>() ?? new List<DropdownItem>();
                }
                
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = "All Teachers" } };
            }
            catch (Exception)
            {
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = "All Teachers" } };
            }
        }

        public async Task<List<DropdownItem>> GetCascadingClassesAsync(string school, string grade, string teacher)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/cascading-classes?school={Uri.EscapeDataString(school)}&grade={Uri.EscapeDataString(grade)}&teacher={Uri.EscapeDataString(teacher)}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<DropdownItem>>() ?? new List<DropdownItem>();
                }
                
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = "All Classes" } };
            }
            catch (Exception)
            {
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = "All Classes" } };
            }
        }
    }
} 