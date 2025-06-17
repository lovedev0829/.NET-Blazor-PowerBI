using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlazorReport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthController> _logger;

        public GoogleAuthController(IConfiguration configuration, ILogger<GoogleAuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback"),
                AllowRefresh = true,
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                Items =
                {
                    { ".xsrf", Guid.NewGuid().ToString() },
                    { "returnUrl", "/" }
                }
            };

            _logger.LogInformation("Initiating Google login with redirect URI: {RedirectUri}", properties.RedirectUri);
            return Challenge(properties, "Google");
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                _logger.LogInformation("Received Google callback");
                
                var result = await HttpContext.AuthenticateAsync("Google");
                if (!result.Succeeded)
                {
                    _logger.LogError("Google authentication failed: {Error}", result.Failure?.Message);
                    return BadRequest("Google authentication failed");
                }

                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email claim is missing from Google authentication");
                    return BadRequest("Email claim is missing");
                }

                _logger.LogInformation("Successfully authenticated user: {Email}", email);

                // Create JWT token
                var token = CreateJwtToken(email, name);

                // Redirect back to the client with the token and email
                return Redirect($"/login-callback?token={token}&email={Uri.EscapeDataString(email)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication callback");
                return BadRequest($"Authentication error: {ex.Message}");
            }
        }

        private string CreateJwtToken(string email, string name)
        {
            // Use a longer secret key (at least 32 bytes for HS256)
            var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "your-256-bit-secret-key-here-minimum-32-bytes-long"));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "domain.com",
                audience: _configuration["Jwt:Audience"] ?? "domain.com",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 