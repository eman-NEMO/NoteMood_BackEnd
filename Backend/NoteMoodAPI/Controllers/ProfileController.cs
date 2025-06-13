using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteMoodUOW.Core.Dtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NoteMoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProfileController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var token =  _unitOfWork.TokenService.GetTokenFromHttpContext();
            var email = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(JwtRegisteredClaimNames.Email, "")).Value;
            var user = await _unitOfWork.Profile.GetProfile(email);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
            var users = await _unitOfWork.Profile.GetAllProfiles();
            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var users = await _unitOfWork.Profile.SearchProfiles(query);
            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Put([FromBody] ProfileDto user)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingUser = await _unitOfWork.Profile.UpdateProfile(user);
            if (existingUser == null)
            {
                return NotFound();
            }
            return Ok(existingUser);
        }

    }
}
