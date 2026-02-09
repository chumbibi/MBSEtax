using MBSWeb.Models.Dto;
using MBSWeb.Models.Entities;
using MBSWeb.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MBSWeb.Services.Repositories
{
    public class UserAuthenticationRepository : IUserAuthentication
    {
        private readonly UserManager<MBSUsers> _userManager;
        private readonly RoleManager<MBSAccessRoles> _roleManager;
        private readonly SignInManager<MBSUsers> _signInManager;

        public UserAuthenticationRepository(
            UserManager<MBSUsers> userManager,
            RoleManager<MBSAccessRoles> roleManager,
            SignInManager<MBSUsers> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<MBSResponse> RegisterUserAsync(RegisterUserDto model)
        {
            try
            {
                if (model == null)
                    return Fail("Invalid request payload");

                if (await _userManager.FindByEmailAsync(model.Email!) != null)
                    return Fail("User already exists");

                var user = new MBSUsers
                {
                    UserName = model.FirstName,
                    Email = model.Email,
                    CompanyId = model.CompanyId,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,                    
                    
                };          
              
                 

                  var result = await _userManager.CreateAsync(user, model.Password!);

                if (!result.Succeeded)
                    return Fail(result.Errors.Select(e => e.Description));

                if (!string.IsNullOrWhiteSpace(model.Role))
                {
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                        await _roleManager.CreateAsync(new MBSAccessRoles { Name = model.Role });

                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                
                return Success("User registered successfully");
            }
            catch (Exception ex)
            {
                return Fail($"Registration failed: {ex.Message}");
            }
        }

        //public async Task<MBSResponse> AuthenticateUserAsync(LoginDto model)
        //{
        //    try
        //    {
        //        var user = await _userManager.FindByEmailAsync(model.UserName);

        //        if (user == null)
        //            return Fail("Invalid username or password");

        //        var result = await _signInManager
        //            .CheckPasswordSignInAsync(user, model.Password, false);

        //        return result.Succeeded
        //            ? Success("Authentication successful", user)
        //            : Fail("Invalid username or password");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail($"Authentication error: {ex.Message}");
        //    }
        //}

        public async Task<MBSResponse> AuthenticateUserAsync(LoginDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.UserName);
                

                if (user == null)
                    return Fail("Invalid username or password");

                var result = await _signInManager
                    .CheckPasswordSignInAsync(user, model.Password, false);
                

                if (!result.Succeeded)
                    return Fail("Invalid username or password");

                // This returns ONLY roles assigned to this user
                var assignedRoles = await _userManager.GetRolesAsync(user);

                return Success("Authentication successful", new
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = assignedRoles   // e.g. ["Admin"] or ["User"]
                });
            }
            catch (Exception ex)
            {
                return Fail($"Authentication error: {ex.Message}");
            }
        }


        public async Task<MBSResponse> ChangePasswordAsync(ChangePasswordDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                    return Fail("User not found");

                var result = await _userManager
                    .ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                return result.Succeeded
                    ? Success("Password changed successfully")
                    : Fail(result.Errors.Select(e => e.Description));
            }
            catch (Exception ex)
            {
                return Fail($"Password change failed: {ex.Message}");
            }
        }

        //public async Task<MBSResponse> ResetPasswordAsync(string email)
        //{
        //    try
        //    {
        //        var user = await _userManager.FindByEmailAsync(email);

        //        if (user == null)
        //            return Fail("User not found");

        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        // Email Token to user logic would go here
        //        return Success("Password reset token generated", token);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail($"Reset password failed: {ex.Message}");
        //    }
        //}

        public async Task<MBSResponse> ResetPasswordAsync(ResetPasswordDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.email);

                if (user == null)
                    return Fail("User not found");

                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, model.newPassword);

                if (!result.Succeeded)
                    return Fail("Password reset failed");

                return Success("Password reset successful");
            }
            catch (Exception ex)
            {
                return Fail($"Reset password failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> UnregisterUserAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return Fail("User not found");

                await _userManager.DeleteAsync(user);
                return Success("User unregistered successfully");
            }
            catch (Exception ex)
            {
                return Fail($"User deletion failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                return Success("Users retrieved successfully", users);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve users: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetUsersByEmailAsync(string email)
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => u.Email!.Contains(email))
                    .ToListAsync();

                return Success("Users retrieved successfully", users);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetUserRolesAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                return Success("Roles retrieved successfully", roles);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve roles: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetRoleByUserAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                 
                if (user == null)
                    return Fail("User not found");

                var roles = await _userManager.GetRolesAsync(user);
                  
                return Success("User roles retrieved successfully", roles);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve user roles: {ex.Message}");
            }
        }

        private static MBSResponse Success(string message, object? data = null) =>
            new()
            {
                StatusCode = 200,
                Message = message,
                Data = data
            };

        private static MBSResponse Fail(object error) =>
            new()
            {
                StatusCode = 400,
                Message = "Operation failed",
                Data = error
            };


        public async Task<MBSResponse> ForgetPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return Fail("User not found");

                // Generate a temporary password
                var tempPassword = $"Temp@{Guid.NewGuid().ToString("N").Substring(0, 8)}";

                // Remove old password and set new one
                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, tempPassword);

                if (!result.Succeeded)
                    return Fail("Failed to reset password");

                // Send temporary password to user via email here
                // await _emailService.SendTemporaryPasswordAsync(user.Email, tempPassword);

                return Success("Temporary password generated and sent to email", tempPassword);
            }
            catch (Exception ex)
            {
                return Fail($"Forget password failed: {ex.Message}");
            }
        }

    }
}

