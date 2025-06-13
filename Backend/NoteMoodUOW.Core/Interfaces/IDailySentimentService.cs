using NoteMoodUOW.Core.Dtos.MachineDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface IDailySentimentService
    {
        Task GetDailySentiment(DateOnly date, string userId);
        Task<List<DailySentimentDto>> GetMoodPerDay(string userId, DateOnly? startDate, DateOnly? endDate);
        Task<List<DailySentimentFreqDto>> GetDailySentimentCounts(string userId, DateOnly? startDate, DateOnly? endDate);
        Task RemoveDailySentimentIfExists(DateOnly date, string userId);
        Task<TestDto> TakeTest(string userId);

    }
}
