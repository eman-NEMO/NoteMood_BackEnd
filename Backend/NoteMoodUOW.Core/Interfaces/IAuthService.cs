using Microsoft.AspNetCore.Identity;
using NoteMoodUOW.Core.Dtos.AuthDtos;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface IAuthService 
    {
        Task<AuthStateDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthStateDto> LoginAsync(LoginDto loginDto);
        Task <bool> LogoutAsync();
        Task<bool> DeleteAccount(string email);

        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
        Task<ApplicationUser> FindByEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        
    }
}
