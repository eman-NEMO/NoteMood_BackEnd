using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteMoodUOW.Core.Interfaces;
using System.Security.Claims;

namespace NoteMoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AspectAnalysisController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AspectAnalysisController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("EntitySentimentPercentage")]
        public async Task<IActionResult> GetEntitySentimentPercentage([FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var entitySentimentPercentage = await _unitOfWork.AspectAnalysisService.CalculateEntitySentimentPercentagesAsync(userId, startDate, endDate);
            if (entitySentimentPercentage == null)
            {
                return NotFound();
            }
            return Ok(entitySentimentPercentage);
        }
       
       
    }
}
