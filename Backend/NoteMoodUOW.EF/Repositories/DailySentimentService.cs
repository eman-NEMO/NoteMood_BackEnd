using Microsoft.EntityFrameworkCore;
using NoteMoodUOW.Core.Dtos.MachineDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF.Repositories
{
    public class DailySentimentService : IDailySentimentService
    {
        private readonly ApplicationDbContext _context;
        private const int numberOfDays = 7;
        private const double threshold = 50.00;

        public DailySentimentService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the daily sentiment for a specific date and user.
        /// </summary>
        /// <param name="date">The date for which to retrieve the daily sentiment.</param>
        /// <param name="userId">The ID of the user.</param>
        public async Task GetDailySentiment(DateOnly date, string userId)
        {
            Console.WriteLine("GetDailySentiment started");

            var entries = await _context.Entries
                .Include(e => e.Sentiment)
                .Where(e => e.ApplicationUserId == userId && e.Date == date)
                .ToListAsync();

            if (entries.Count == 0)
            {
                await RemoveDailySentimentIfExists(date, userId);
            }
            else
            {
                await AddOrUpdateDailySentiment(date, userId, entries);
            }

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Changes saved to the database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes the daily sentiment entry if it exists for a specific date and user.
        /// </summary>
        /// <param name="date">The date for which to remove the daily sentiment.</param>
        /// <param name="userId">The ID of the user.</param>
        public async Task RemoveDailySentimentIfExists(DateOnly date, string userId)
        {
            var dailySentiment = await _context.DailySentiments
                .FirstOrDefaultAsync(ds => ds.Date == date && ds.ApplicationUserId == userId);

            if (dailySentiment != null)
            {
                _context.DailySentiments.Remove(dailySentiment);
                Console.WriteLine("DailySentiment entry deleted due to no entries");
            }
        }

        /// <summary>
        /// Adds or updates the daily sentiment entry for a specific date and user.
        /// </summary>
        /// <param name="date">The date for which to add or update the daily sentiment.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="entries">The list of entries for the specified date and user.</param>
        private async Task AddOrUpdateDailySentiment(DateOnly date, string userId, List<Entry> entries)
        {
            var sentimentCounts = CountSentiments(entries, e => e.Sentiment.Name);
            var sentimentPercentages = CalculatePercentage(sentimentCounts);
            var overallSentiment = DetermineOverallSentiment(sentimentCounts, sentimentPercentages);


            var dailySentiment = await _context.DailySentiments
                .FirstOrDefaultAsync(ds => ds.Date == date && ds.ApplicationUserId == userId);

            if (dailySentiment == null)
            {
                dailySentiment = new DailySentiment
                {
                    Date = date,
                    ApplicationUserId = userId,
                    Sentiment = overallSentiment.Name,
                    Percentage = sentimentPercentages.ContainsKey(overallSentiment.Name) ? sentimentPercentages[overallSentiment.Name] : 50
                };

                await _context.DailySentiments.AddAsync(dailySentiment);
            }
            else
            {
                dailySentiment.Sentiment = overallSentiment.Name;
                dailySentiment.Percentage = sentimentPercentages.ContainsKey(overallSentiment.Name) ? sentimentPercentages[overallSentiment.Name] : 50; // Default percentage or handle as appropriate
            }
        }

        /// <summary>
        /// Retrieves the mood per day for a specific user within a date range.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="startDate">The start date of the date range (optional).</param>
        /// <param name="endDate">The end date of the date range (optional).</param>
        /// <returns>A list of DailySentimentDto objects representing the mood per day.</returns>
        public async Task<List<DailySentimentDto>> GetMoodPerDay(string userId, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _context.DailySentiments
                .Where(uds => uds.ApplicationUserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(uds => uds.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(uds => uds.Date <= endDate.Value);
            }

            var sentimentList = await query
                .Select(uds => new DailySentimentDto
                {
                    Date = uds.Date,
                    Sentiment = uds.Sentiment,
                })
                .ToListAsync();

            return sentimentList;
        }

        /// <summary>
        /// Retrieves the daily sentiment counts for a specific user within a date range.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="startDate">The start date of the date range (optional).</param>
        /// <param name="endDate">The end date of the date range (optional).</param>
        /// <returns>A list of DailySentimentFreqDto objects representing the daily sentiment counts.</returns>
        public async Task<List<DailySentimentFreqDto>> GetDailySentimentCounts(string userId, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _context.DailySentiments
                .Where(uds => uds.ApplicationUserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(uds => uds.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(uds => uds.Date <= endDate.Value);
            }

            var dailySentiments = await query.ToListAsync();
            return CalculateDailySentimentFreqs(dailySentiments);
        }
        public async Task<TestDto> TakeTest(string userId)
        {
            // Retrieve the latest date in the DailySentiments table for the specified user
            var latestDate = await _context.DailySentiments
                .Where(ds => ds.ApplicationUserId == userId)
                .OrderByDescending(ds => ds.Date)
                .Select(ds => ds.Date)
                .FirstOrDefaultAsync();

            if (latestDate == default)
            {
                // Handle the case where there are no daily sentiments for the user
                return new TestDto { Flag = false };
            }

            // Calculate the start date as 7 days before the latest date
            var startDate = latestDate.AddDays(-6); // Latest date is included, so we use -6

            // Retrieve daily sentiment counts for the last 7 days from the latest date
            var dailySentimentCounts = await GetDailySentimentCounts(userId, startDate, latestDate);

            // Check if there is any negative sentiment with a percentage >= 50%
            var flag = dailySentimentCounts.Any(p => p.Sentiment == "Negative" && p.Percentage >= threshold);

            return new TestDto
            {
                Flag = flag
            };
        }


        /// <summary>
        /// Calculates the daily sentiment frequencies based on the daily sentiment entries.
        /// </summary>
        /// <param name="dailySentiments">The list of daily sentiment entries.</param>
        /// <returns>A list of DailySentimentFreqDto objects representing the daily sentiment frequencies.</returns>
        private List<DailySentimentFreqDto> CalculateDailySentimentFreqs(List<DailySentiment> dailySentiments)
        {
            var sentimentCounts = CountSentiments(dailySentiments, ds => ds.Sentiment);
            var sentimentFreqs = new List<DailySentimentFreqDto>();
            var sentimentPercentages = CalculatePercentage(sentimentCounts);

            foreach (var sentiment in sentimentCounts.Keys)
            {
                sentimentFreqs.Add(new DailySentimentFreqDto
                {
                    Sentiment = sentiment.ToString(),
                    Count = sentimentCounts[sentiment],
                    Percentage = sentimentPercentages[sentiment]
                });
            }

            return sentimentFreqs;
        }

        /// <summary>
        /// Counts the occurrences of each sentiment in a list of items.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The list of items.</param>
        /// <param name="sentimentSelector">A function to select the sentiment from each item.</param>
        /// <returns>A dictionary containing the sentiment counts.</returns>
        private Dictionary<string, int> CountSentiments<T>(List<T> items, Func<T, string> sentimentSelector)
        {
            var sentimentCounts = new Dictionary<string, int>();

            foreach (var item in items)
            {
                var sentiment = sentimentSelector(item);
                if (!sentimentCounts.ContainsKey(sentiment))
                {
                    sentimentCounts[sentiment] = 0;
                }
                sentimentCounts[sentiment]++;
            }

            return sentimentCounts;
        }

        /// <summary>
        /// Calculates the percentage of each sentiment count in a dictionary.
        /// </summary>
        /// <param name="counts">The dictionary containing the sentiment counts.</param>
        /// <returns>A dictionary containing the sentiment percentages.</returns>
        private Dictionary<string, double> CalculatePercentage(Dictionary<string, int> counts)
        {
            var totalEntries = counts.Values.Sum();
            var percentages = new Dictionary<string, double>();

            foreach (var key in counts.Keys)
            {
                percentages[key] = Math.Round(((double)counts[key] / totalEntries) * 100, 2);
            }

            return percentages;
        }

        /// <summary>
        /// Determines the overall sentiment based on the sentiment counts and percentages.
        /// </summary>
        /// <param name="sentimentCounts">The dictionary containing the sentiment counts.</param>
        /// <param name="sentimentPercentages">The dictionary containing the sentiment percentages.</param>
        /// <returns>The overall sentiment.</returns>
        private Sentiment DetermineOverallSentiment(Dictionary<string, int> sentimentCounts, Dictionary<string, double> sentimentPercentages)
        {
            // Determine the sentiment with the highest percentage
            var maxPercentage = sentimentPercentages.Values.Max();
            var topSentiments = sentimentPercentages.Where(sp => sp.Value == maxPercentage).Select(sp => sp.Key).ToList();

            // If there's a tie, return Neutral or another default sentiment
            if (topSentiments.Count > 1)
            {
                var neutralSentiment = _context.Sentiments.FirstOrDefault(s => s.Name == "Neutral");
                if (neutralSentiment != null)
                {
                    return neutralSentiment;
                }
                throw new Exception("Neutral sentiment not found in the Sentiments table.");
            }

            return _context.Sentiments.FirstOrDefault(s => s.Name == topSentiments.First());
        }

        
    }
}
