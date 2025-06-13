using Microsoft.EntityFrameworkCore;
using NoteMoodUOW.Core.Dtos.EntryDtos;
using NoteMoodUOW.Core.Dtos.MachineDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF.Repositories
{
    /// <summary>
    /// Represents a service for topic analysis.
    /// </summary>
    public class TopicAnalysisService : ITopicAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMachineAPI _machineAPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicAnalysisService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="machineAPI">The machine API.</param>
        public TopicAnalysisService(ApplicationDbContext context, IMachineAPI machineAPI)
        {
            _context = context;
            _machineAPI = machineAPI;
        }

        /// <summary>
        /// Adds topic analysis for the given entry.
        /// </summary>
        /// <param name="url">The URL of the Flask API.</param>
        /// <param name="entryDto">The entry DTO.</param>
        public async Task AddTopicAnalysisAsync(string url, EntryDto entryDto)
        {
            var topicAnalysisResponse = await _machineAPI.callFlaskAPI<TopicAnalysisResponseDto>(entryDto.Content, url);
            if (topicAnalysisResponse == null)
            {
                return;
            }
            foreach (var topic in topicAnalysisResponse.topics)
            {
                var topicName = topic.Key;
                var Sentiment = topic.Value;
                // Check if topic exists in the database 
                await FindOrCreateTopicWithSentimentAsync(topicName, Sentiment, entryDto);
            }
        }

        /// <summary>
        /// Updates topic analysis for the given entry for the case when the content updated.
        /// </summary>
        /// <param name="url">The URL of the Flask API.</param>
        /// <param name="entryDto">The entry DTO.</param>
        public async Task UpdateTopicAnalysisAsync(string url, EntryDto entryDto)
        {
            await RemoveExistingAnalysisForEntry(entryDto.Id);
            await AddTopicAnalysisAsync(url, entryDto);
        }

        /// <summary>
        /// Finds or creates a topic with sentiment for the given entry.
        /// </summary>
        /// <param name="topicName">The name of the topic.</param>
        /// <param name="sentimentName">The name of the sentiment.</param>
        /// <param name="entryDto">The entry DTO.</param>
        private async Task FindOrCreateTopicWithSentimentAsync(string topicName, string sentimentName, EntryDto entryDto)
        {
            var topic = _context.Topics.FirstOrDefault(x => x.Name == topicName);
            if (topic == null)
            {
                topic = new Topic
                {
                    Name = topicName
                };
                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
            }
            var sentiment = _context.Sentiments.FirstOrDefault(x => x.Name == sentimentName);
            if (sentiment == null)
            {
                sentiment = new Sentiment
                {
                    Name = sentimentName
                };
                _context.Sentiments.Add(sentiment);
                await _context.SaveChangesAsync();
            }
            var topicSentiment = await _context.TopicSentiments
                                         .FirstOrDefaultAsync(x => x.TopicId == topic.Id && x.SentimentId == sentiment.Id && x.EntryId == entryDto.Id);
            if (topicSentiment == null)
            {
                topicSentiment = new TopicSentiment
                {
                    TopicId = topic.Id,
                    SentimentId = sentiment.Id,
                    EntryId = entryDto.Id
                };
                _context.TopicSentiments.Add(topicSentiment);
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes existing topic analysis for the given entry.
        /// </summary>
        /// <param name="entryId">The ID of the entry.</param>
        private async Task RemoveExistingAnalysisForEntry(int entryId)
        {
            var topicSentiments = await _context.TopicSentiments.Where(x => x.EntryId == entryId).ToListAsync();
            _context.TopicSentiments.RemoveRange(topicSentiments);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates topic analysis for the given user within the specified date range.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>A list of topic DTOs representing the topic analysis.</returns>
        public async Task<List<TopicDto>> CalculateTopicAnalysisAsync(string userId, DateOnly? startDate, DateOnly? endDate)
        {
            
            var query = _context.TopicSentiments
                                .Include(x => x.Topic)
                                .Include(x => x.Sentiment)
                                .Include(x => x.Entry)
                                .Where(x => x.Entry.ApplicationUserId == userId);
            if (startDate.HasValue)
            {
                query = query.Where(x => x.Entry.Date >= startDate);
            }
            if (endDate.HasValue)
            {
                query = query.Where(x => x.Entry.Date <= endDate);
            }
            var topicSentiments = await query.ToListAsync();
            var topicAnalysis = new List<TopicDto>();
            var groupedTopicSentiments = topicSentiments.GroupBy(x => x.Topic.Name);
            var totalEntries = topicSentiments.Select(x => x.EntryId).Distinct().Count();
            foreach (var groupedTopicSentiment in groupedTopicSentiments)
            {
                var topicDto = new TopicDto
                {
                    TopicName = groupedTopicSentiment.Key,
                    Frequency = groupedTopicSentiment.Count(),
                    Percentage = Math.Round(((double)groupedTopicSentiment.Count() / totalEntries) * 100,2),
                    Sentiments = new List<SentimentPercentageDto>()
                };
                var groupedSentiments = groupedTopicSentiment.GroupBy(x => x.Sentiment.Name);
                foreach (var groupedSentiment in groupedSentiments)
                {
                    var sentimentPercentageDto = new SentimentPercentageDto
                    {
                        SentimentName = groupedSentiment.Key,
                        Percentage = Math.Round((double)groupedSentiment.Count() / topicDto.Frequency * 100,2)
                    };
                    topicDto.Sentiments.Add(sentimentPercentageDto);
                }

                topicAnalysis.Add(topicDto);
            }
            topicAnalysis = topicAnalysis
                                        .OrderByDescending(topic => topic.Frequency)
                                        .Select(topic => new TopicDto
                                        {
                                            TopicName = topic.TopicName,
                                            Frequency = topic.Frequency,
                                            Percentage = topic.Percentage,
                                            Sentiments = topic.Sentiments.OrderByDescending(sentiment => sentiment.Percentage).ToList() 
                                        })
                                        .ToList();

            return topicAnalysis;
        }
    }
}
