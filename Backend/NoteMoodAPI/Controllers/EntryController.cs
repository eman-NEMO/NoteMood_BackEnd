using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteMoodUOW.Core.Dtos.EntryDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System.Security.Claims;
using static Lucene.Net.Search.FieldValueHitQueue;

namespace NoteMoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EntryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public EntryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpGet("GetEntry")]
        public async Task<IActionResult> Get(int id)
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var entry = await _unitOfWork.Entry.GetEntryByIdAsync(id, userId);
            if(entry == null)
            {
                return NotFound();
            }
            return Ok(entry);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
           
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var entries = await _unitOfWork.Entry.GetAllEntriesAsync(userId);
            if (entries == null)
            {
                return NotFound();
            }
            return Ok(entries);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery]FilterEntriesDto filterDto)
        {
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var entries = await _unitOfWork.Entry.FilterEntriesAsync(userId, filterDto);
            return Ok(entries);

        }

        [HttpPost("Create")]
        public async Task<IActionResult> Post([FromBody] EntryDto entryDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId  = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var entry = await _unitOfWork.Entry.AddEntryAsync(entryDto, userId);
            if (entry == null)
            {
                return BadRequest();
            }
            BackgroundJob.Enqueue(() => _unitOfWork.DailySentiment.GetDailySentiment(entry.Date, userId));
            // do Aspect Analysis
            BackgroundJob.Enqueue(() => _unitOfWork.AspectAnalysisService.AddAspectAnalysisAsync("get_aspects", entry));
            BackgroundJob.Enqueue(() => _unitOfWork.TopicAnalysisService.AddTopicAnalysisAsync("get_topics", entry));
            return Ok(entry);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Put( [FromBody] EntryDto entryDto)
        {

            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var entry = await _unitOfWork.Entry.UpdateEntryAsync(entryDto, userId);
            if (entry == null)
            {
                return NotFound();
            }
            BackgroundJob.Enqueue(() => _unitOfWork.DailySentiment.GetDailySentiment(entry.Date, userId));
            // Update Aspect Analysis
            BackgroundJob.Enqueue(() => _unitOfWork.AspectAnalysisService.UpdateAspectAnalysisAsync("get_aspects", entry));
            BackgroundJob.Enqueue(() => _unitOfWork.TopicAnalysisService.UpdateTopicAnalysisAsync("get_topics", entry));


            return Ok(entry);
        }
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery]int id)
        {
            
            var token = _unitOfWork.TokenService.GetTokenFromHttpContext();
            var userId = _unitOfWork.TokenService.ExtractClaimsFromToken(token, new Claim(ClaimTypes.NameIdentifier, "")).Value;
            var entry = await _unitOfWork.Entry.GetEntryByIdAsync(id, userId);
            if (!_unitOfWork.Entry.DeleteEntry(id, userId))
            {
                return NotFound();
            }
            BackgroundJob.Enqueue(() => _unitOfWork.DailySentiment.RemoveDailySentimentIfExists(entry.Date, userId));
            return NoContent();
        }

    }
}
