using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface ITokenService
    {
        Task<JwtSecurityToken> GenerateJwtToken(ApplicationUser user);
        public string GetTokenFromHttpContext();
        public Claim ExtractClaimsFromToken(string token, Claim claim);

    }
}
