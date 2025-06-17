using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace BlazorReport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowerBIController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PowerBIController> _logger;

        // Replace these with your actual values
        private const string TenantId = "4eab22f7-3eef-44bd-a0ea-2fb63f4c9eca";
        private const string ClientId = "8f53068a-201c-4cf8-82af-729aa10ce341";
        private const string ClientSecret = "kxN8Q~E6sFQX7j3MZ1ykC73lOQZm2~z2ubZC5bvx";
        private const string Scope = "https://analysis.windows.net/powerbi/api/.default";

        // Power BI specific
        private const string GroupId = "fed74024-425e-42f2-a45c-28ef5a23578d";
        private const string ReportId = "0345d5bf-2fc1-4af0-a7ad-2dbec5f94067";

        public PowerBIController(IHttpClientFactory httpClientFactory, ILogger<PowerBIController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets a Microsoft access token for Power BI.
        /// </summary>
        [NonAction]
        public async Task<MicrosoftTokenResponse> GetMicrosoftToken()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var tokenEndpoint = $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("scope", Scope)
                });

                _logger.LogInformation("Requesting access token from Microsoft...");
                var response = await httpClient.PostAsync(tokenEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get access token. Status: {response.StatusCode}, Content: {errorContent}");
                    return null; // or throw an exception
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw token response: {responseContent}");

                try
                {
                    var tokenResponse = JsonSerializer.Deserialize<MicrosoftTokenResponse>(responseContent);
                    if (tokenResponse == null)
                    {
                        _logger.LogError("Failed to deserialize token response.");
                        return null;
                    }

                    _logger.LogInformation($"Successfully deserialized token response. Token type: {tokenResponse.TokenType}, Expires in: {tokenResponse.ExpiresIn}");
                    return tokenResponse;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing token response.");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error getting access token.");
                return null;
            }
        }

        /// <summary>
        /// Returns a Power BI embed token for the specified report.
        /// </summary>
        [HttpGet("powerbi-embed-token")]
        public async Task<IActionResult> GetEmbedToken()
        {
            try
            {
                // Get the Microsoft access token
                var microsoftResponse = await GetMicrosoftToken();
                if (microsoftResponse == null || string.IsNullOrWhiteSpace(microsoftResponse.AccessToken))
                {
                    return StatusCode(500, new { error = "Failed to acquire Microsoft access token." });
                }

                // Construct request for the Power BI embed token
                var embedTokenEndpoint = $"https://api.powerbi.com/v1.0/myorg/groups/{GroupId}/reports/{ReportId}/GenerateToken";

                var powerBiClient = _httpClientFactory.CreateClient();
                powerBiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", microsoftResponse.AccessToken);
                powerBiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _logger.LogInformation("Requesting embed token from Power BI...");
                var embedResponse = await powerBiClient.PostAsJsonAsync(embedTokenEndpoint, new
                {
                    accessLevel = "View",
                    allowSaveAs = false
                });

                if (!embedResponse.IsSuccessStatusCode)
                {
                    var errorContent = await embedResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get embed token. Status: {embedResponse.StatusCode}, Content: {errorContent}");
                    return StatusCode((int)embedResponse.StatusCode, new
                    {
                        error = "Failed to get embed token",
                        details = errorContent
                    });
                }

                var embedResponseContent = await embedResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw embed token response: {embedResponseContent}");

                try
                {
                    var embedTokenResponse = JsonSerializer.Deserialize<PowerBIEmbedTokenResponse>(embedResponseContent);
                    if (embedTokenResponse == null)
                    {
                        _logger.LogError("Failed to deserialize embed token response.");
                        return StatusCode(500, new
                        {
                            error = "Failed to deserialize embed token response.",
                            rawResponse = embedResponseContent
                        });
                    }

                    // Return the token and the embed URL
                    return Ok(new
                    {
                        embedToken = embedTokenResponse.Token,
                        embedUrl = $"https://app.powerbi.com/reportEmbed?reportId={ReportId}"
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing embed token response.");
                    return StatusCode(500, new
                    {
                        error = "Error deserializing embed token response.",
                        message = ex.Message,
                        rawResponse = embedResponseContent
                    });
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating embed token.");
                return StatusCode(500, new
                {
                    error = "Internal server error.",
                    message = ex.Message
                });
            }
        }

        // Make these classes PUBLIC if they're returned from a public method.
        // Otherwise, you will get 'inconsistent accessibility' build errors.

        public class MicrosoftTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }

        public class PowerBIEmbedTokenResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("tokenId")]
            public string TokenId { get; set; }

            [JsonPropertyName("expiration")]
            public string Expiration { get; set; }
        }
    }
}
