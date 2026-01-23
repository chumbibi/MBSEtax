using MBSWeb.Models.Dto;

namespace MBSWeb.Services.Interfaces
{
    public interface IUserAuthentication
    {
        Task<MBSResponse> AuthenticateUserAsync(LoginDto model);
        Task<MBSResponse> RegisterUserAsync(RegisterUserDto model);
        Task<MBSResponse> GetUserRolesAsync();
        Task<MBSResponse> GetRoleByUserAsync(string email);
        Task<MBSResponse> ChangePasswordAsync(ChangePasswordDto model);
        Task<MBSResponse> ForgetPasswordAsync(string email);
        Task<MBSResponse> ResetPasswordAsync(ResetPasswordDto model);
        Task<MBSResponse> UnregisterUserAsync(string email);
        Task<MBSResponse> GetAllUsersAsync();
        Task<MBSResponse> GetUsersByEmailAsync(string email);

    }
}
