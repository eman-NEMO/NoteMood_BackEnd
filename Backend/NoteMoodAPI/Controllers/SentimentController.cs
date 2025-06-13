using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteMoodUOW.Core.Interfaces;
using System.Security.Claims;

namespace NoteMoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SentimentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SentimentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("MoodPerDay")]
        public async Task<IActionResult> GetMoodPerDay([FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var sentiment = await _unitOfWork.DailySentiment.GetMoodPerDay(userId, startDate, endDate);
            if (sentiment == null)
            {
                return NotFound();
            }
            return Ok(sentiment);
        }
        [HttpGet("DailySentimentCounts")]
        public async Task<IActionResult> GetDailySentimentCounts([FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var sentiment = await _unitOfWork.DailySentiment.GetDailySentimentCounts(userId, startDate, endDate);
            if (sentiment == null)
            {
                return NotFound();
            }
            return Ok(sentiment);
        }

        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var test = await _unitOfWork.DailySentiment.TakeTest(userId);
            if (test == null)
            {
                return NotFound();
            }
            return Ok(test);
        }


    }
}
