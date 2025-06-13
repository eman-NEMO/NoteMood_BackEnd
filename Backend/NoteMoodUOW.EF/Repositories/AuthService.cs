using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoteMoodUOW.Core.Configurations;
using NoteMoodUOW.Core.Constants;
using NoteMoodUOW.Core.Dtos.AuthDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using Org.BouncyCastle.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace NoteMoodUOW.EF.Repositories
{



    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IRefreshTokenService refreshTokenService,
                           ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _refreshTokenService = refreshTokenService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Logs in a user with the provided credentials.
        /// </summary>
        /// <param name="loginDto">The login credentials.</param>
        /// <returns>An AuthStateDto object representing the authentication state.</returns>
        public async Task<AuthStateDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthStateDto { Message = "Invalid email or password" };
            }

            var jwtToken = await _tokenService.GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            var authStateDto = new AuthStateDto
            {
                Message = "Login successful",
                Email = user.Email,
                Roles = roles.ToList(),
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Expires = jwtToken.ValidTo
            };

            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authStateDto.RefreshToken = activeRefreshToken.Token;
                authStateDto.RefreshTokenExpires = activeRefreshToken.Expires;
            }
            else
            {
                var refreshToken = _refreshTokenService.GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
                authStateDto.RefreshToken = refreshToken.Token;
                authStateDto.RefreshTokenExpires = refreshToken.Expires;
            }

            return authStateDto;
        }

        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <returns>A boolean indicating whether the logout was successful.</returns>
        public async Task<bool> LogoutAsync()
        {
            // remove refresh token
            var user = await _userManager.GetUserAsync(_signInManager.Context.User);

            if (user is null)
                return false;

            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            if (activeRefreshToken is not null)
            {
                activeRefreshToken.Revoked = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
            await _signInManager.SignOutAsync();
            return true;
        }

        /// <summary>
        /// Deletes the user account with the specified email.
        /// </summary>
        /// <param name="email">The email of the user account to delete.</param>
        /// <returns>A boolean indicating whether the account deletion was successful.</returns>
        public Task<bool> DeleteAccount(string email)
        {
            var user = _userManager.FindByEmailAsync(email).Result;
            if (user == null)
            {
                return Task.FromResult(false);
            }
            var result = _userManager.DeleteAsync(user);
            if (!result.Result.Succeeded)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Registers a new user with the provided registration details.
        /// </summary>
        /// <param name="registerDto">The registration details.</param>
        /// <returns>An AuthStateDto object representing the authentication state.</returns>
        public async Task<AuthStateDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) is not null)
            {
                return new AuthStateDto { Message = "Email already exists" };
            }
            // cast month to enum 
            Month month = Enum.TryParse<Month>(registerDto.Month, true, out var tempMonth) ? tempMonth : Month.Unknown;
            var user = new ApplicationUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                FullName = registerDto.FullName,
                Country = registerDto.Country,
                DateOfBirth = new DateOnly(registerDto.Year, (int)month, registerDto.Day),
                Gender = Enum.TryParse<Gender>(registerDto.Gender, true, out var tempGender) ? tempGender : Gender.Unknown
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = String.Empty;
                foreach (var error in result.Errors)
                {
                    errors += error.Description + Environment.NewLine;
                }
                return new AuthStateDto { Message = errors };
            }

            var jwtToken = await _tokenService.GenerateJwtToken(user);
            var refreshToken = _refreshTokenService.GenerateRefreshToken();
            user.RefreshTokens?.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var Role = Roles.DiaryUser;
            await _userManager.AddToRoleAsync(user, Role.ToString());
            return new AuthStateDto
            {
                Message = "Registration successful",
                Email = user.Email,
                Roles = new List<string> { Role.ToString() },
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Expires = jwtToken.ValidTo,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpires = refreshToken.Expires
            };
        }

        // find by email
        public async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        // GeneratePasswordResetTokenAsync
        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public virtual async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public virtual async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public virtual async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public virtual async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
    }
}
