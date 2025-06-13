using NoteMoodUOW.Core.Dtos.EntryDtos;
using NoteMoodUOW.Core.Dtos.MachineDtos;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface IAspectAnalysisService
    {
        Task AddAspectAnalysisAsync(string url, EntryDto entry);
        Task UpdateAspectAnalysisAsync(string url, EntryDto entryDto);
        Task<List<AspectDto>> CalculateEntitySentimentPercentagesAsync(string userId, DateOnly? startDate, DateOnly? endDate);

    }
}
