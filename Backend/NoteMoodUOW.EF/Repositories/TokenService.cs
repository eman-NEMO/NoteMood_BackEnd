using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoteMoodUOW.Core.Configurations;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF.Repositories
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWTConfiguration _JWT;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly ILogger<TokenService> _logger;

        public TokenService(UserManager<ApplicationUser> userManager, IOptions<JWTConfiguration> jwt, IHttpContextAccessor httpContextAccessor/*, ILogger<TokenService> logger*/)
        {
            _userManager = userManager;
            _JWT = jwt.Value;
            _httpContextAccessor = httpContextAccessor;
            //_logger = logger;
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom the token is generated.</param>
        /// <returns>The generated JWT token.</returns>
        public async Task<JwtSecurityToken> GenerateJwtToken(ApplicationUser user)
        {
            //_logger.LogInformation("Generating JWT token for user {UserId}", user.Id);

            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JWT.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _JWT.ValidIssuer,
                audience: _JWT.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_JWT.ExpiryInMinutes),
                signingCredentials: signingCredentials
            );

            //_logger.LogInformation("JWT token generated for user {UserId}", user.Id);
            return jwtSecurityToken;
        }

        /// <summary>
        /// Retrieves the JWT token from the current HttpContext.
        /// </summary>
        /// <returns>The JWT token from the HttpContext, or null if not found.</returns>
        public string GetTokenFromHttpContext()
        {
            if (_httpContextAccessor.HttpContext != null &&
                _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer "))
                {
                    //_logger.LogInformation("JWT token found in HttpContext");
                    return authHeader.Substring("Bearer ".Length);
                }
            }

            //_logger.LogWarning("JWT token not found in HttpContext");
            return null;
        }

        /// <summary>
        /// Extracts the specified claim from the provided JWT token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="claim">The claim to extract.</param>
        /// <returns>The extracted claim from the token.</returns>
        public Claim ExtractClaimsFromToken(string token, Claim claim)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claimFromToken = jwtToken.Claims.FirstOrDefault(c => c.Type == claim.Type);

            //if (claimFromToken != null)
            //{
            //    _logger.LogInformation("Claim {ClaimType} found in JWT token", claim.Type);
            //}
            //else
            //{
            //    _logger.LogWarning("Claim {ClaimType} not found in JWT token", claim.Type);
            //}

            return claimFromToken;
        }
    }
}
