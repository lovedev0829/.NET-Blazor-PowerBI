using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorReport.Client.Services
{
    public class PowerBIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PowerBIService> _logger;
        private readonly AuthService _authService;
        private readonly IJSRuntime _jsRuntime;

        public PowerBIService(HttpClient httpClient, ILogger<PowerBIService> logger, AuthService authService, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authService = authService;
            _jsRuntime = jsRuntime;
        }

        private async Task<string> GetJWT()
        {
            var userData = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "user");
            if (!string.IsNullOrWhiteSpace(userData))
            {
                var dataArray = userData.Split(';', 2);
                if (dataArray.Length == 2)
                    return dataArray[1];
            }
            return null;
        }

        public async Task<(string EmbedToken, string EmbedUrl)> GetEmbedInfoAsync()
        {
            try
            {
                _logger.LogInformation("Requesting Power BI embed token from server...");

                // Check authentication using AuthService
                if (!await _authService.IsAuthenticatedAsync())
                {
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                // Get the JWT token
                var jwtToken = await GetJWT();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    throw new UnauthorizedAccessException("JWT token not found");
                }

                // Add the JWT token to the request headers
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                // Match the route used in your PowerBIController: [HttpGet("powerbi-embed-token")]
                var response = await _httpClient.GetFromJsonAsync<EmbedInfoResponse>("api/powerbi/powerbi-embed-token");

                if (response == null || string.IsNullOrWhiteSpace(response.EmbedToken))
                {
                    _logger.LogError("Received an empty or invalid embed token response from the server.");
                    throw new Exception("Failed to retrieve a valid Power BI embed token. Check server logs for details.");
                }
                // Return the parsed token and embed URL
                return (response.EmbedToken, response.EmbedUrl);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call Power BI token endpoint on the server");
                throw new Exception("Failed to get Power BI token. Check the server logs for more details.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while requesting Power BI embed token");
                throw;
            }
        }

        private class EmbedInfoResponse
        {
            public string EmbedToken { get; set; }
            public string EmbedUrl { get; set; }
        }
    }
}
