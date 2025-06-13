using NoteMoodUOW.Core.Dtos.AuthDtos;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<AuthStateDto> RefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
        RefreshToken GenerateRefreshToken();
        void SetRefreshTokenCookie(string refreshToken, DateTime expires);



    }
}
