using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteMoodUOW.Core.Interfaces;
using System.Security.Claims;

namespace NoteMoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TopicAnalysisController : ControllerBase
    {
       private readonly IUnitOfWork _unitOfWork;

        public TopicAnalysisController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("TopicAnalysis")]
        public async Task<IActionResult> GetTopicAnalysis([FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var topicAnalysis = await _unitOfWork.TopicAnalysisService.CalculateTopicAnalysisAsync(userId, startDate, endDate);
            if (topicAnalysis == null)
            {
                return NotFound();
            }
            return Ok(topicAnalysis);
        }
    }
}
