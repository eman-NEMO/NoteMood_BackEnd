using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NoteMoodUOW.Core.Configurations;
using NoteMoodUOW.Core.Dtos.AuthDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace NoteMoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _unitOfWork.Auth.RegisterAsync(registerDto);
            if(!result.IsAuthenticated)
                return BadRequest(result.Message);
            _unitOfWork.RefreshTokenService.SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpires);
            var user = await _unitOfWork.Auth.FindByEmailAsync(registerDto.Email);
            var token = await _unitOfWork.Auth.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ConfirmEmail), "Auth", new {email = user.Email, token }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Confirm Email", $"Please confirm your email by clicking here: {callbackUrl!}");
            _unitOfWork.EmailService.SendEmail(message);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDto loginDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _unitOfWork.Auth.LoginAsync(loginDto);
            if(!result.IsAuthenticated)
                return BadRequest(result.Message);
            //if login is successful, set refresh token cookie ????
            if (!string.IsNullOrEmpty(result.RefreshToken))
                _unitOfWork.RefreshTokenService.SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpires);
            return Ok(result);
        }
        [Authorize]

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await _unitOfWork.Auth.LogoutAsync();
            return Ok();
        }
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete()
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var email = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(JwtRegisteredClaimNames.Email, "")).Value;
            var isDeleted = await _unitOfWork.Auth.DeleteAccount(email);
            if (!isDeleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromQuery][Required] string email)
        {
            var user = await _unitOfWork.Auth.FindByEmailAsync(email);
            if (user == null)
                return BadRequest("Invalid email");
            var token = await _unitOfWork.Auth.GeneratePasswordResetTokenAsync(user);
            //var tokenEncoded = WebUtility.UrlEncode(tokenReset);
            var callbackUrl = $"{_unitOfWork.configuration.FrontEnd.Url}?email={email}";
            var message = new Message(new string[] { email }, "Reset Password", $"Please reset your password by clicking here: {callbackUrl!}");
            _unitOfWork.EmailService.SendEmail(message);
            // Add the token to the response headers
            //Response.Headers.Add("Password-Reset-Token", token);
            return Ok(new
            {
                message = "Password change request is sent to your email. Please open it.",
                token = token
            });
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _unitOfWork.Auth.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return BadRequest("Invalid email");
            //var decodedToken = WebUtility.UrlDecode(resetPasswordDto.Token);
            var result = await _unitOfWork.Auth.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            if (result.Succeeded)
            { 
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState); 
            }
            return Ok("Password reset successfully");
        }

        [AllowAnonymous]
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            var user = await _unitOfWork.Auth.FindByEmailAsync(email);
            if (user == null)
                return BadRequest("Invalid email");
            var result = await _unitOfWork.Auth.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest("Invalid token");
            return Ok("Email confirmed successfully");
        }
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _unitOfWork.Auth.FindByEmailAsync(changePasswordDto.Email);
            if (user == null)
                return BadRequest("Invalid email");
            var result = await _unitOfWork.Auth.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }
            return Ok("Password changed successfully");
        }

        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ResetPasswordDto { Email = email, Token = token };
            return Ok(new { model });

        }
    }
}
