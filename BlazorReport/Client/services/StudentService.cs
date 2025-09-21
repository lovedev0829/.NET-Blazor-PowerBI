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


        public async Task<List<DropdownItem>> GetCascadingDropdownDataAsync(string level, string? school = null, string? grade = null, string? teacher = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(school)) queryParams.Add($"school={Uri.EscapeDataString(school)}");
                if (!string.IsNullOrEmpty(grade)) queryParams.Add($"grade={Uri.EscapeDataString(grade)}");
                if (!string.IsNullOrEmpty(teacher)) queryParams.Add($"teacher={Uri.EscapeDataString(teacher)}");
                
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"api/student/cascading-dropdown/{level}{queryString}";
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<DropdownItem>>() ?? new List<DropdownItem>();
                }
                
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = $"All {level}" } };
            }
            catch (Exception)
            {
                return new List<DropdownItem> { new DropdownItem { Value = "All", Text = $"All {level}" } };
            }
        }

        public async Task<List<DropdownItem>> GetCascadingGradesAsync(string school)
        {
            return await GetCascadingDropdownDataAsync("Grades", school);
        }

        public async Task<List<DropdownItem>> GetCascadingTeachersAsync(string school, string grade)
        {
            return await GetCascadingDropdownDataAsync("Teachers", school, grade);
        }

        public async Task<List<DropdownItem>> GetCascadingClassesAsync(string school, string grade, string teacher)
        {
            return await GetCascadingDropdownDataAsync("Classes", school, grade, teacher);
        }
    }
} 