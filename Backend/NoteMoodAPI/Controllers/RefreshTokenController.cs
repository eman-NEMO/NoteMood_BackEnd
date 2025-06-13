using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteMoodUOW.Core.Dtos.RefreshTokenDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.EF.Repositories;

namespace NoteMoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class RefreshTokenController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("refreshToken")]
       // you can take the refresh token from the body or from the cookie
        public async Task<IActionResult> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            //var decodedToken = UrlEncoder.Default.Encode(refreshToken);
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Invalid refresh token");
            var result = await _unitOfWork.RefreshTokenService.RefreshTokenAsync(refreshToken);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            if (!string.IsNullOrEmpty(result.RefreshToken))
                _unitOfWork.RefreshTokenService.SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpires);
            return Ok(result);
        }
        [HttpPost("revokeRefreshToken")]
        public async Task<IActionResult> RevokeRefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var token = refreshTokenDto.Token ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required");
            var result = await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(token);
            if (!result)
                return BadRequest("Invalid token");
            return Ok();

        }
    }
}
