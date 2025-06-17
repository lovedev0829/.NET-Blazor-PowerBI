// Program.cs
using Microsoft.AspNetCore.ResponseCompression;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Authentication.JwtBearer; // NOTE: THIS LINE OF CODE IS NEWLY ADDED
using Microsoft.IdentityModel.Tokens; // NOTE: THIS LINE OF CODE IS NEWLY ADDED
using BlazorReport.Server.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddTransient<IUserDatabase, UserDatabase>();  // NOTE: LOCAL AUTHENTICATION ADDED HERE; AddTransient() IS OK TO USE BECAUSE STATE IS SAVED TO THE DRIVE

// NOTE: the following block of code is newly added
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;  // <-- change to None
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.Cookie.Name = ".BlazorReport.Auth";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "domain.com",
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "domain.com",
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? "your-256-bit-secret-key-here-minimum-32-bytes-long"))
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/api/googleauth/google-callback/";
    options.SaveTokens = true;
    options.AccessDeniedPath = "/";
    options.ReturnUrlParameter = new PathString("/");
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7077", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// NOTE: end block

builder.Services.AddHttpClient();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add CORS middleware before authentication
app.UseCors();

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication(); // NOTE: line is newly added

app.UseAuthorization(); // NOTE: line is newly addded, notice placement after UseRouting()


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();