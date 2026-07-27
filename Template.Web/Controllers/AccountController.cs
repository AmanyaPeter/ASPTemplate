using AutoMapper;
using Template.Core.Repository.Accounts;
using Template.Core.Repository.Roles;
using Template.Core.Services.AdAuthentication;
using Template.Core.Services.Authorization;
using Template.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SmartBreadcrumbs.Attributes;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Template.Web.Controllers;

public class AccountController(IMapper _mapper, ILogger<AccountController> _logger
    , IAdAuthenticationService _adAuthService
    , IAuthService _authService
    , IAccountRepository _accountRepo
    , IRoleRepository _roleRepo
    , UserManager<ApplicationUser> userManager
    , IHostEnvironment environment
    ) : Controller
{
    [RequirePermission(SystemPermissions.Account.ViewApplicationUsers)]
    [Breadcrumb("User Accounts", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public async Task<IActionResult> Index()
    {
        var users = await _accountRepo.FindAll();
        var userViewModels = _mapper.Map<List<ApplicationUserViewModel>>(users);

        var pageViewModel = new ApplicationUserListPageViewModel
        {
            Users = userViewModels
        };

        return View(pageViewModel);
    }

    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var result = await _authService.ValidateApplicationUser(username, password);

        if (!result.Success)
        {
            ViewData["status"] = result.Status;
            ModelState.AddModelError("", $"Failed login. {result.Status}");
            return View();
        }

        await _authService.SignInApplicationUser(result.User);
        _logger.LogInformation("Login successful");

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }


    [Breadcrumb("Create", FromAction = nameof(Index))]
    [RequirePermission(SystemPermissions.Account.CreateApplicationUser)]
    public IActionResult Create()
    {
        var model = new ApplicationUserViewModel
        {
            Id = string.Empty,
            EndDate = DateTime.UtcNow.Date.AddYears(1),
            IsActive = true
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.CreateApplicationUser)]
    public async Task<IActionResult> Create(ApplicationUserViewModel model)
    {
        if (model.EndDate.HasValue && model.EndDate.Value.Date < DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(nameof(model.EndDate), "The account end date cannot be in the past.");
            return View(model);
        }

        var adUserResult = _adAuthService.IsExistsOnAd(model);

        if (!adUserResult.IsSuccess)
        {
            switch (adUserResult.ResultType)
            {
                case AdResultTypes.ServerUnavailable:
                    ModelState.AddModelError("", "The Active Directory server is currently unavailable. Please contact administrator.");
                    _logger.LogError("Active Directory server unavailable.");
                    break;

                case AdResultTypes.UserNotFound:
                    ModelState.AddModelError("UserName", "Username does not exist in Active Directory.");
                    _logger.LogWarning("Attempted to create user with non-existent AD username: {Username}", model.UserName);
                    break;

                default:
                    ModelState.AddModelError("", $"Error validating username: {adUserResult.ErrorMessage}");
                    _logger.LogError(adUserResult.Exception, "Error checking AD for username {Username}: {Message}",
                        model.UserName, adUserResult.ErrorMessage);
                    break;
            }

            return View(model);
        }
        // Check if user already exists in our database
        var existingUser = await _accountRepo.FindByName(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError("UserName", "A user with this username already exists in the system.");
            return View(model);
        }

        var newUser = adUserResult.AppUser;
        newUser.FullName = string.Join(" ", new[]
            {
                newUser.FirstName,
                newUser.MiddleName,
                newUser.LastName
            }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
        newUser.Title = model.Title?.Trim() ?? string.Empty;
        newUser.BusinessUnit = model.BusinessUnit?.Trim() ?? string.Empty;
        newUser.JobTitle = model.JobTitle?.Trim() ?? string.Empty;
        newUser.Station = model.Station?.Trim() ?? string.Empty;
        newUser.AgeBracket = model.AgeBracket ?? string.Empty;
        newUser.Gender = model.Gender ?? string.Empty;
        newUser.EndDate = model.EndDate;
        newUser.IsActive = model.IsActive;
        newUser.LockoutEnabled = true;
        newUser.CreatedDate = DateTime.UtcNow;
        newUser.CreatedBy = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var creatorId)
            ? creatorId
            : Guid.Empty;

        var result = await _accountRepo.Create(newUser);

        if (result)
        {
            TempData["SuccessMessage"] = $"User {model.UserName} was created successfully.";
            _logger.LogInformation($"User {model.UserName} was created successfully.");
            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError("", "Failed to create user. Please contact administrator.");
            _logger.LogError("Failed to create user.");
            return View(model);
        }
    }

    [Breadcrumb("Edit", FromAction = nameof(Index))]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Update(string userId)
    {
        var result = await _accountRepo.FindById(userId);

        var user = _mapper.Map<ApplicationUserViewModel>(result);

        return View("Create", user);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Update(ApplicationUserViewModel model)
    {
        var userInfo = await _accountRepo.FindById(model.Id);

        if (!model.IsActive)
        {
            userInfo.DisableDate = DateTime.UtcNow;
        }
        else
        {
            userInfo.DisableDate = null;
        }

        var user = _mapper.Map<ApplicationUser>(model);

        var result = await _accountRepo.Update(user);

        if (result)
        {
            TempData["updated"] = "true";

            if (model.IsActive)
                _logger.LogInformation($"User account {model.UserName} has been activated.");
            else
                _logger.LogInformation($"User account {model.UserName} has been deactivated and disabled.");

            return RedirectToAction("Index");
        }
        else
        {
            TempData["updated"] = "false";
            _logger.LogError($"Failed to update user details for {model.UserName}. Please contact Administrator");
            return RedirectToAction("Index");
        }
    }

    [Breadcrumb("Manage Roles", FromAction = nameof(Index))]
    [RequirePermission(SystemPermissions.Roles.ViewRoles)]
    public async Task<IActionResult> ManageRoles(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User ID is required.";
            return RedirectToAction("Index");
        }

        // get user
        var user = await _accountRepo.FindById(userId);

        if (user == null)
        {
            TempData["ErrorMessage"] = $"User with ID {userId} was not found.";
            return RedirectToAction("Index");
        }

        // Get all roles and user's current roles
        var allRoles = await _roleRepo.FindAll();
        var userRoleNames = await _accountRepo.GetApplicationUserRoles(user);

        var userRoleSet = new HashSet<string>(
            userRoleNames ?? new List<string>(),
            StringComparer.OrdinalIgnoreCase
        );

        // Build view model
        var model = new ApplicationUserRolesViewModel
        {
            UserId = userId,
            UserName = user.UserName,
            Roles = new List<ApplicationUserRoleViewModel>()
        };

        // Add roles to model if any exist
        if (allRoles != null && allRoles.Any())
        {
            foreach (var role in allRoles.OrderBy(r => r.Name))
            {
                model.Roles.Add(new ApplicationUserRoleViewModel
                {
                    RoleId = role.Id.ToString(),
                    RoleName = role.Name ?? "(Unnamed Role)",
                    IsSelected = !string.IsNullOrEmpty(role.Name) && userRoleSet.Contains(role.Name)
                });
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Roles.EditRole)]
    public async Task<IActionResult> ManageRoles(ApplicationUserRolesViewModel model)
    {
        var user = await _accountRepo.FindById(model.UserId);
        if (user == null)
            return NotFound();

        var userRoles = await _accountRepo.GetApplicationUserRoles(user);

        // Remove roles that are no longer selected
        foreach (var role in userRoles)
        {
            if (!model.Roles.Any(r => r.IsSelected && _roleRepo.FindById(r.RoleId).Result.Name == role))
            {
                await _accountRepo.RemoveApplicationUserFromRole(user, role);

            }
        }


        foreach (var role in model.Roles.Where(r => r.IsSelected))
        {
            var roleName = (await _roleRepo.FindById(role.RoleId)).Name;
            if (!userRoles.Contains(roleName))
            {
                await _accountRepo.AddApplicationUserFromRole(user, roleName);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [ActionName("Logout")]
    public async Task<IActionResult> LogoutAsync(string? returnUrl = null)
    {
        await _authService.SignOutApplicationUser();
        HttpContext.Session.Clear();
        _logger.LogInformation("Logout successful");
        return RedirectToAction("Login", "Account", new { ReturnUrl = returnUrl });
    }

    [HttpGet]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> ResetPassword(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        var model = _mapper.Map<ApplicationUserViewModel>(user);
        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(ResetPassword))]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> ResetPasswordConfirmed(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        var tempPassword = CreateTemporaryPassword();

        var resetSucceeded = false;
        string? resetError = null;
        if (environment.IsDevelopment() || user.IsBreakGlassAccount)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, tempPassword);
            resetSucceeded = result.Succeeded;
            resetError = result.Succeeded
                ? null
                : string.Join(", ", result.Errors.Select(error => error.Description));
        }
        else
        {
            var result = _adAuthService.ResetPassword(user.UserName!, tempPassword);
            resetSucceeded = result.Success;
            resetError = result.ErrorMessage;
        }

        if (resetSucceeded)
        {
            // Force password change on next login
            user.PasswordResetRequired = true;
            await userManager.UpdateSecurityStampAsync(user);
            await userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Password for {user.UserName} has been reset. Temporary password: {tempPassword}";
            _logger.LogInformation("Password reset for user {Username}", user.UserName);
        }
        else
        {
            TempData["ErrorMessage"] = $"Failed to reset password: {resetError}";
            _logger.LogError("Password reset failed for user {Username}", user.UserName);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Lock(string userId, string? reason)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId) &&
            currentUserId == user.Id)
        {
            TempData["ErrorMessage"] = "You cannot lock your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.LockReason = string.IsNullOrWhiteSpace(reason)
            ? "Locked by an administrator."
            : reason.Trim();
        user.IsLoggedIn = false;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        var lockResult = await userManager.UpdateAsync(user);
        if (!lockResult.Succeeded)
        {
            TempData["ErrorMessage"] = $"Could not lock the account: {string.Join(", ", lockResult.Errors.Select(error => error.Description))}";
            return RedirectToAction(nameof(Index));
        }
        await userManager.UpdateSecurityStampAsync(user);

        TempData["SuccessMessage"] = $"{user.UserName} has been locked.";
        _logger.LogWarning("Administrator locked user {Username}. Reason: {Reason}", user.UserName, user.LockReason);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Unlock(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        user.LockReason = null;
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        var unlockResult = await userManager.UpdateAsync(user);
        if (!unlockResult.Succeeded)
        {
            TempData["ErrorMessage"] = $"Could not unlock the account: {string.Join(", ", unlockResult.Errors.Select(error => error.Description))}";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"{user.UserName} has been unlocked.";
        _logger.LogInformation("Administrator unlocked user {Username}", user.UserName);
        return RedirectToAction(nameof(Index));
    }

    private static string CreateTemporaryPassword()
    {
        var randomDigit = RandomNumberGenerator.GetInt32(0, 10);
        return $"Tmp!{Guid.NewGuid():N}"[..20] + randomDigit;
    }
}
