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

        public async Task<object?> GetDropdownDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/student/dropdown-data");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<object>();
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Student?> GetStudentAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/student/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Student>();
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
} 