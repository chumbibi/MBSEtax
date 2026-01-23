using MBSWeb.Models.Dto;
using MBSWeb.Services.Interfaces;

namespace MBSWeb.Managers
{
    public class UserAuthenticationManager
    {
        private readonly IUserAuthentication _userAuthenticationService;
        public UserAuthenticationManager(IUserAuthentication userAuthenticationService)
        {
            _userAuthenticationService = userAuthenticationService;
        }
        public async Task<MBSResponse> AuthenticateUserAsync(LoginDto model)
        {
            var result = await _userAuthenticationService.AuthenticateUserAsync(model);
            return result;
        }

        public async Task<MBSResponse> ChangePasswordAsync(ChangePasswordDto model)
        {
            var result = await _userAuthenticationService.ChangePasswordAsync(model);
            return result;
        }

        public async Task<MBSResponse> GetAllUsersAsync()
        {
            var result = await _userAuthenticationService.GetAllUsersAsync();
            return result;
        }

        public async Task<MBSResponse> GetRoleByUserAsync(string email)
        {
            var result = await _userAuthenticationService.GetRoleByUserAsync(email);
            return result;
        }

        public async Task<MBSResponse> GetUserRolesAsync()
        {
            var result = await _userAuthenticationService.GetUserRolesAsync();
            return result;
        }

        public async Task<MBSResponse> GetUsersByEmailAsync(string email)
        {
            var result = await _userAuthenticationService.GetUsersByEmailAsync(email);
            return result;
        }

        public async Task<MBSResponse> RegisterUserAsync(RegisterUserDto model)
        {
            var result = await _userAuthenticationService.RegisterUserAsync(model);
            return result;
        }
        public async Task<MBSResponse> ForgetPasswordAsync(string email)
        {
            var result = await _userAuthenticationService.ForgetPasswordAsync(email);
            return result;
        }

        public async Task<MBSResponse> ResetPasswordAsync(ResetPasswordDto model)
        {

            var result = await _userAuthenticationService.ResetPasswordAsync(model);
            return result;

        }

        public async Task<MBSResponse> UnregisterUserAsync(string email)
        {
            var result = await _userAuthenticationService.UnregisterUserAsync(email);
            return result;
        }
    }
}
