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
    public class EntryRepository : BaseRepository<Entry>, IEntryRepository
    {
        private ISearchService _searchService;

        private IMachineAPI _machineAPI;

        public EntryRepository(ApplicationDbContext context, ISearchService searchService, IMachineAPI machineAPI) : base(context)
        {
            _searchService = searchService;
            _machineAPI = machineAPI;
        }

        /// <summary>
        /// Adds a new entry asynchronously.
        /// </summary>
        /// <param name="entryDto">The entry DTO.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The added entry DTO.</returns>
        public async Task<EntryDto> AddEntryAsync(EntryDto entryDto, string userId)
        {
            // it is better to use automapper here

            // call the machine learning api to get the overall sentiment of the entry
            var overallSentimentDto = await _machineAPI.callFlaskAPI<OverallSentimentDto>(entryDto.Content, "get_overall_sentiment");
            var sentiment = await _context.Sentiments
               .FirstOrDefaultAsync(s => s.Name == overallSentimentDto.overall_seniment);
            if (sentiment == null)
            {
                sentiment = new Sentiment { Name = overallSentimentDto.overall_seniment };
                _context.Sentiments.Add(sentiment);
                await _context.SaveChangesAsync();
            }
            // Set the creation date and time if not provided
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            entryDto.Date = entryDto.Date == default ? currentDate : entryDto.Date;
            entryDto.Time = entryDto.Time == default ? currentTime: entryDto.Time;
            var entry = new Entry
            {
                Title = entryDto.Title,
                Content = entryDto.Content,
                Date = entryDto.Date,
                Time = entryDto.Time,
                ApplicationUserId = userId,
                SentimentId = sentiment.Id,
                Sentiment = sentiment

            };
            await AddAsync(entry);
            var result = await _context.SaveChangesAsync();
            if (result == 0)
            {
                return null;
            }


            // index the entry
            _searchService.IndexEntry(entry);

            return new EntryDto
            {
                Id = entry.Id,
                Title = entry.Title,
                Content = entry.Content,
                Date = entry.Date,
                Time = entry.Time,
                OverallSentiment = sentiment.Name
            };

        }

        /// <summary>
        /// Deletes an entry.
        /// </summary>
        /// <param name="id">The entry ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the entry is deleted successfully, otherwise false.</returns>
        public bool DeleteEntry(int id, string userId)
        {
            var entry = GetAllAsync().Result.Where(e => e.ApplicationUserId == userId).Where(e => e.Id == id).FirstOrDefault();
            if (entry == null)
            {
                return false;
            }
            Delete(entry);
            var result = _context.SaveChanges();
            if (result == 0)
            {
                return false;
            }
            // delete index for the entry
            _searchService.DeleteIndexEntry(id);
            return true;

        }

        /// <summary>
        /// Gets an entry by ID asynchronously.
        /// </summary>
        /// <param name="id">The entry ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The entry DTO.</returns>
        public async Task<EntryDto> GetEntryByIdAsync(int id, string userId)
        {
            var entry = await GetAll().Include(e => e.Sentiment).FirstOrDefaultAsync(e => e.Id == id && e.ApplicationUserId == userId);
            if (entry == null)
            {
                return null;
            }
            return new EntryDto
            {
                Id = entry.Id,
                Title = entry.Title,
                Content = entry.Content,
                Date = entry.Date,
                Time = entry.Time,
                OverallSentiment = entry.Sentiment.Name
            };
        }

        /// <summary>
        /// Filters entries based on the provided criteria asynchronously.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="filterDto">The filter criteria DTO.</param>
        /// <returns>A collection of filtered entry DTOs.</returns>
        public Task<IEnumerable<EntryDto>> FilterEntriesAsync(string userId, FilterEntriesDto filterDto)
        {
            // there is no filters so it will not returns any entries 
            if (string.IsNullOrEmpty(filterDto.Query) && !filterDto.StartDate.HasValue && !filterDto.EndDate.HasValue && string.IsNullOrEmpty(filterDto.SentimentName))
            {
                return Task.FromResult<IEnumerable<EntryDto>>(null);
            }
            // if the start date is greater than the end date
            if (filterDto.StartDate.HasValue && filterDto.EndDate.HasValue)
            {
                if (filterDto.StartDate.Value > filterDto.EndDate.Value)
                {
                    return Task.FromResult<IEnumerable<EntryDto>>(null);
                }
            }
            // if one of the dates is null
            if ((filterDto.StartDate.HasValue && !filterDto.EndDate.HasValue) || (!filterDto.StartDate.HasValue && filterDto.EndDate.HasValue))
            {
                return Task.FromResult<IEnumerable<EntryDto>>(null);
            }

            // use search service to search for entries
            var entries = _searchService.SearchEntries(userId: userId, filterDto).Result;
            if (entries == null)
            {
                return Task.FromResult<IEnumerable<EntryDto>>(null);
            }
            var entryDtos = entries.Select(e => new EntryDto
            {
                Id = e.Id,
                Title = e.Title,
                Content = e.Content,
                Date = e.Date,
                Time = e.Time,
                OverallSentiment = e.Sentiment.Name
            }).ToList();
            return Task.FromResult<IEnumerable<EntryDto>>(entryDtos);
        }

        /// <summary>
        /// Updates an entry asynchronously.
        /// </summary>
        /// <param name="entryDto">The updated entry DTO.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The updated entry DTO.</returns>
        public async Task<EntryDto> UpdateEntryAsync(EntryDto entryDto, string userId)
        {
            var entries = await GetAllAsync();
            var entry = entries.FirstOrDefault(e => e.ApplicationUserId == userId && e.Id == entryDto.Id);
            if (entry == null)
            {
                return null;
            }

            // Only call API if the content changed
            if (entryDto.Content != entry.Content)
            {
                var overallSentimentDto = await _machineAPI.callFlaskAPI<OverallSentimentDto>(entryDto.Content, "get_overall_sentiment");
                var sentiment = await _context.Sentiments.FirstOrDefaultAsync(s => s.Name == overallSentimentDto.overall_seniment);
                if (sentiment == null)
                {
                    sentiment = new Sentiment { Name = overallSentimentDto.overall_seniment };
                    _context.Sentiments.Add(sentiment);
                    await _context.SaveChangesAsync();
                }
                entry.SentimentId = sentiment.Id;
            }
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            entryDto.Date = entryDto.Date == default ? currentDate : entryDto.Date;
            entryDto.Time = entryDto.Time == default ? currentTime : entryDto.Time;

            entry.Title = entryDto.Title;
            entry.Content = entryDto.Content;
            entry.Date = entryDto.Date;
            entry.Time = entryDto.Time;
            entry.Sentiment = await _context.Sentiments.FirstOrDefaultAsync(s => s.Id == entry.SentimentId);
            Update(entry);
            var result = await _context.SaveChangesAsync();
            if (result == 0)
            {
                return null;
            }

            // Update index for the entry
            _searchService.IndexEntry(entry);

            return new EntryDto
            {
                Id = entry.Id,
                Title = entry.Title,
                Content = entry.Content,
                Date = entry.Date,
                Time = entry.Time,
                OverallSentiment = entry.Sentiment.Name
            };
        }

        /// <summary>
        /// Gets all entries for a user asynchronously.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of entry DTOs.</returns>
        public async Task<IEnumerable<EntryDto>> GetAllEntriesAsync(string userId)
        {
            var entries = await GetAll()
                .Where(e => e.ApplicationUserId == userId)
                .OrderByDescending(e => e.Date).ThenByDescending(e => e.Time)
                .Include(e => e.Sentiment)
                .ToListAsync();

            if (entries == null)
            {
                return null;
            }

            var entryDtos = entries.Select(e => new EntryDto
            {
                Id = e.Id,
                Title = e.Title,
                Content = e.Content,
                Date = e.Date,
                Time = e.Time,
                OverallSentiment = e.Sentiment.Name
            }).ToList();

            return entryDtos;
        }

    }
}