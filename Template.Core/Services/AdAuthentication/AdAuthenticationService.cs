using System.DirectoryServices.AccountManagement;
using Template.Core.Models.Account;
using Template.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace Template.Core.Services.AdAuthentication
{
    public class AdAuthenticationService(string _ldapServer
        , string _ldapContainer
        , ILogger<AdAuthenticationService> _logger
        ) : IAdAuthenticationService
    {
        [SupportedOSPlatform("windows")]
        public bool ValidateCredentials(string username, string password)
        {
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _ldapServer, _ldapContainer);
                return context.ValidateCredentials(username, password, ContextOptions.Negotiate);
            }
            catch (PrincipalServerDownException ex)
            {
                // Handle connection issues with LDAP server
                // Log the exception
                _logger.LogError(ex, "LDAP server is down or unreachable. Server: {LdapServer}", _ldapServer);
                return false;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                // Log the exception
                _logger.LogError(ex, "Unexpected Active Directory authentication error for {Username}", username);
                return false;
            }
        }

        [SupportedOSPlatform("windows")]
        public bool ValidateUserCredentials(string username, string password)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _ldapServer, _ldapContainer))
                {
                    // Attempt to validate the credentials
                    bool isValid = context.ValidateCredentials(username, password);

                    if (isValid)
                    {
                        return true; // Login successful
                    }
                    else
                    {
                        // Check if the account is now locked
                        var user = UserPrincipal.FindByIdentity(context, username);

                        if (user != null && user.IsAccountLockedOut())
                        {
                            _logger.LogError($"User account {username} has been locked due to three incorrect login attempts.");
                        }

                        return false; // Credentials are invalid
                    }
                }
            }
            catch (PrincipalServerDownException)
            {
                // Log the exception (uncomment if logging is in place)
                // _logger.LogError(ex, "LDAP server is down or unreachable. Server: {LdapServer}", _ldapServer);
                return false;
            }
            catch (Exception)
            {
                // Log other unexpected exceptions
                // _logger.LogError(ex, "An unexpected error occurred during LDAP authentication. Username: {Username}", username);
                return false;
            }
        }

        [SupportedOSPlatform("windows")]
        public (bool Success, string ErrorMessage) ResetPassword(string username, string newPassword)
        {
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _ldapServer, _ldapContainer);
                using var user = UserPrincipal.FindByIdentity(
                    context,
                    IdentityType.SamAccountName,
                    username);
                if (user == null)
                {
                    return (false, $"Active Directory user '{username}' was not found.");
                }

                user.SetPassword(newPassword);
                user.ExpirePasswordNow();
                user.Save();
                return (true, null);
            }
            catch (PrincipalServerDownException ex)
            {
                _logger.LogError(ex, "Active Directory is unavailable while resetting {Username}.", username);
                return (false, "Active Directory is currently unavailable.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "The application is not permitted to reset {Username}.", username);
                return (false, "The application does not have permission to reset this Active Directory password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Active Directory password reset failed for {Username}.", username);
                return (false, "Active Directory rejected the password reset.");
            }
        }

        [SupportedOSPlatform("windows")]
        public AdUserResult IsExistsOnAd(ApplicationUserViewModel model)
        {
            try
            {
                // Create a PrincipalContext for the domain
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    // Search for the user by SAM account name
                    var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, model.UserName);

                    // If the user is found, return true; otherwise, return false
                    if (user != null)
                    {
                        ApplicationUser appUser = new ApplicationUser()
                        {
                            UserName = model.UserName,
                            EndDate = model.EndDate,
                            FirstName = user.GivenName,
                            MiddleName = user.MiddleName,
                            LastName = user.Surname,
                            Email = user.EmailAddress,
                            Title = model.Title,
                        };

                        return new AdUserResult
                        {
                            User = user,
                            AppUser = appUser,
                            IsSuccess = true,
                            ResultType = AdResultTypes.Success
                        };
                    }
                    else
                    {
                        // User not found
                        //return null;
                        return new AdUserResult
                        {
                            IsSuccess = false,
                            ErrorMessage = $"User '{model.UserName}' not found in Active Directory",
                            ResultType = AdResultTypes.UserNotFound
                        };
                    }
                }
            }
            catch (PrincipalServerDownException ex)
            {
                //Console.WriteLine("Unable to connect to the domain controller.");
                //return null;
                return new AdUserResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Unable to connect to the Active Directory server",
                    Exception = ex,
                    ResultType = AdResultTypes.ServerUnavailable
                };
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"An error occurred: {ex.Message}");
                //return null;
                return new AdUserResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while checking Active Directory: {ex.Message}",
                    Exception = ex,
                    ResultType = AdResultTypes.OtherError
                };
            }
        }

    }
}
