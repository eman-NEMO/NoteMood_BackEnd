using NoteMoodUOW.Core.Dtos.EntryDtos;
using NoteMoodUOW.Core.Dtos.MachineDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface ITopicAnalysisService
    {
        Task AddTopicAnalysisAsync(string url, EntryDto entryDto);
        Task UpdateTopicAnalysisAsync(string url, EntryDto entryDto);
        Task<List<TopicDto>> CalculateTopicAnalysisAsync(string userId, DateOnly? startDate, DateOnly? endDate);
    }
}
