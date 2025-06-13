using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NoteMoodUOW.Core.Configurations;
using NoteMoodUOW.Core.Dtos.AuthDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF.Repositories
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWTConfiguration _JWT;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RefreshTokenService(UserManager<ApplicationUser> userManager, IOptions<JWTConfiguration> jwt, ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _JWT = jwt.Value;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        /// <param name="token">The refresh token.</param>
        /// <returns>An instance of AuthStateDto containing the refreshed token and some information.</returns>
        public async Task<AuthStateDto> RefreshTokenAsync(string token)
        {
            var authStateDto = new AuthStateDto();
            var user = await _userManager.Users
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user is null)
            {
                authStateDto.Message = "Invalid token";
                return authStateDto;
            }
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
            {
                authStateDto.Message = "Inactive token";
                return authStateDto;
            }

            refreshToken.Revoked = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var jwtToken = await _tokenService.GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            authStateDto.Message = "Token refreshed";
            authStateDto.Email = user.Email;
            authStateDto.Roles = roles.ToList();
            authStateDto.IsAuthenticated = true;
            authStateDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authStateDto.RefreshToken = newRefreshToken.Token;
            authStateDto.RefreshTokenExpires = newRefreshToken.Expires;
            authStateDto.Expires = jwtToken.ValidTo;

            return authStateDto;
        }

        /// <summary>
        /// Revokes the provided refresh token.
        /// </summary>
        /// <param name="token">The refresh token.</param>
        /// <returns>True if the token was successfully revoked, otherwise false.</returns>
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var user = await _userManager.Users
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user is null)
            {
                return false;
            }
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
            if (!refreshToken.IsActive)
            {
                return false;
            }
            refreshToken.Revoked = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return true;
        }

        /// <summary>
        /// Generates a new refresh token.
        /// </summary>
        /// <returns>The generated refresh token.</returns>
        public RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(_JWT.RefreshTokenExpiryInDays),
                Created = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Sets the refresh token cookie in the HTTP response.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="expires">The expiration date of the refresh token.</param>
        public void SetRefreshTokenCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
