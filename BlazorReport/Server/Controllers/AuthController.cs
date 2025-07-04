﻿using BlazorReport.Server.Models;
using BlazorReport.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BlazorExample.Server.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private string GenerateSecureKey()
        {
            var key = new byte[32]; // 256 bits
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);
        }

        private string CreateJWT(User user)
        {
            var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(GenerateSecureKey())); // Use a secure key
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: "domain.com",
                audience: "domain.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private IUserDatabase userdb { get; }

        public AuthController(IUserDatabase userdb)
        {
            this.userdb = userdb;
        }

        [HttpPost]
        [Route("api/auth/register")]
        public async Task<LoginResult> Post([FromBody] RegModel reg)
        {
            if (reg.password != reg.confirmpwd)
                return new LoginResult { message = "Password and confirm password do not match.", success = false };

            User newuser = await userdb.AddUser(reg.email, reg.password);
            if (newuser != null)
                return new LoginResult { message = "Registration successful.", jwtBearer = CreateJWT(newuser), email = reg.email, success = true };

            return new LoginResult { message = "User already exists.", success = false };
        }

        [HttpPost]
        [Route("api/auth/login")]
        public async Task<LoginResult> Post([FromBody] LoginModel log)
        {
            User user = await userdb.AuthenticateUser(log.email, log.password);
            if (user != null)
                return new LoginResult { message = "Login successful.", jwtBearer = CreateJWT(user), email = log.email, success = true };

            return new LoginResult { message = "User/password not found.", success = false };
        }
    }
}
