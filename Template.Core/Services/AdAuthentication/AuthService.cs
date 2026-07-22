using Template.Core.Repository.Accounts;
using Template.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Template.Core.Services.AdAuthentication;
public class AuthService(
     IAdAuthenticationService _adAuthService
    , IAccountRepository _accountRepo
    , ILogger<AuthService> _logger
    , UserManager<ApplicationUser> _userManager
    , IHttpContextAccessor _httpContextAccessor
    , SignInManager<ApplicationUser> _signInManager) : IAuthService
{
    public async Task SignInApplicationUser(ApplicationUser user, bool isPersistent = false)
    {
        user.IsLoggedIn = true;
        user.LastActivity = DateTime.UtcNow;
        _httpContextAccessor.HttpContext.Session.SetString("userName", user.UserName);

        await _userManager.UpdateAsync(user);
        await _signInManager.SignInAsync(user, isPersistent);
    }
    public async Task SignOutApplicationUser()
    {
        var userName = _httpContextAccessor.HttpContext.Session.GetString("userName");
        var user = await _accountRepo.FindByName(userName);

        if (user != null)
        {
            user.IsLoggedIn = false;
            await _userManager.UpdateAsync(user);
        }

        await _signInManager.SignOutAsync();
        
        //Clear session data if needed
        _httpContextAccessor.HttpContext?.Session.Clear();
    }

    public async Task<(bool Success, string Status, ApplicationUser User)> ValidateApplicationUser(string username, string password)
    {
        // Find user
        var user = await _accountRepo.FindByName(username);
        if (user == null)
        {
            _logger.LogError($"Failed login attempt by {username}. User does not exist.");
            return (false, "Wrong username or password.", null);
        }

        // Validate AD credentials
        if (!_adAuthService.ValidateCredentials(username, password))
        {
            _logger.LogError($"Failed login, Active Directory authentication for username {username}.");
            return (false, "Failed Active Directory authentication.", null);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogError($"Failed login attempt by {username}. Account inactive.");
            return (false, "Account inactive.", null);
        }

        // Check if user account is disabled
        if (user.DisableDate < DateTime.Now)
        {
            _logger.LogError($"Failed login attempt by {username}. Account disabled.");
            return (false, "Account disabled.", null);
        }

        //Check if account has expired
        if (user.EndDate < DateTime.Now)
        {
            _logger.LogError($"Failed login by {username}, account was disabled because it reached its end date.");
            return (false, "Account expired.", null);
        }

        // A browser can be closed without executing the logout action, leaving
        // IsLoggedIn set in the database. Valid credentials must therefore be
        // allowed to establish a new session instead of permanently locking the
        // user out. The flag remains useful as informational login state and is
        // refreshed by SignInApplicationUser.
        if (user.IsLoggedIn)
        {
            _logger.LogInformation(
                "User {Username} is replacing an existing or abandoned login session.",
                username);
        }

        return (true, "success", user);

    }
}
