// AuthService.cs
using System;
using Microsoft.JSInterop;

namespace BlazorReport.Client.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _jsRuntime;

        public event Action OnChange;

        private string username;

        public string Username
        {
            get => username;
            set
            {
                username = value;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public void Login(string user)
        {
            Username = user;
            // Additional logic for login (e.g., store in local storage)
        }

        public void Logout()
        {
            Username = string.Empty;
            // Additional logic for logout (e.g., clear local storage)
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                // Check if username is set
                if (string.IsNullOrEmpty(Username))
                    return false;

                // Check if JWT token exists in localStorage
                var userData = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "user");
                if (string.IsNullOrEmpty(userData))
                    return false;

                // Verify the token format (email;jwt)
                var parts = userData.Split(';', 2);
                if (parts.Length != 2)
                    return false;

                // Verify the email matches the current username
                return parts[0] == Username;
            }
            catch
            {
                return false;
            }
        }
    }
}
