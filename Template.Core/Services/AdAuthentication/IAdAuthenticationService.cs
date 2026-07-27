using Template.Core.Models.Account;

namespace Template.Core.Services.AdAuthentication
{
    public interface IAdAuthenticationService
    {
        bool ValidateCredentials(string username, string password);
        bool ValidateUserCredentials(string username, string password);
        (bool Success, string ErrorMessage) ResetPassword(string username, string newPassword);

        AdUserResult IsExistsOnAd(ApplicationUserViewModel model);

    }
}
