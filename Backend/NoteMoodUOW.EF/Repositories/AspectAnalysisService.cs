using Microsoft.EntityFrameworkCore;
using NoteMoodUOW.Core.Dtos.EntryDtos;
using NoteMoodUOW.Core.Dtos.MachineDtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF.Repositories
{
    public class AspectAnalysisService : IAspectAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMachineAPI _machineAPI;

        public AspectAnalysisService(ApplicationDbContext context, IMachineAPI machineAPI)
        {
            _context = context;
            _machineAPI = machineAPI;
        }

        /// <summary>
        /// Adds aspect analysis for a given entry by calling a Flask API.
        /// </summary>
        /// <param name="url">The URL of the Flask API.</param>
        /// <param name="entryDto">The entry DTO containing the content.</param>
        public async Task AddAspectAnalysisAsync(string url, EntryDto entryDto)
        {
            var aspectAnalysisResponse = await _machineAPI.callFlaskAPI<AspectAnalysisResponseDto>(entryDto.Content, url);

            // Check if the response is null and handle accordingly
            if (aspectAnalysisResponse == null)
            {
                Console.WriteLine("Aspect analysis response was null.");
                return;
            }

            // Iterate over each aspect
            foreach (var aspect in aspectAnalysisResponse.aspects)
            {
                var aspectName = aspect.Key;
                var entities = aspect.Value.entities;

                // Check if the aspect exists in the database
                var dbAspect = await FindOrCreateAspectAsync(aspectName);

                // Iterate over each entity in the aspect
                foreach (var entity in entities)
                {
                    var entityName = entity.Key;
                    var sentiment = entity.Value;

                    // Check if the entity exists in the database, and create it with its sentiment if not
                    await FindOrCreateEntityWithSentimentAsync(dbAspect, entityName, sentiment, entryDto);
                }
            }
        }

        /// <summary>
        /// Finds or creates an aspect in the database.
        /// </summary>
        /// <param name="aspectName">The name of the aspect.</param>
        /// <returns>The aspect entity.</returns>
        private async Task<Aspect> FindOrCreateAspectAsync(string aspectName)
        {
            var aspect = await _context.Aspects.FirstOrDefaultAsync(a => a.Name == aspectName);
            if (aspect == null)
            {
                aspect = new Aspect { Name = aspectName };
                _context.Aspects.Add(aspect);
                await _context.SaveChangesAsync();
            }
            return aspect;
        }

        /// <summary>
        /// Finds or creates an entity with its sentiment under a specific aspect and links it to an entry.
        /// </summary>
        /// <param name="aspect">The aspect entity.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="sentimentName">The name of the sentiment.</param>
        /// <param name="entry">The entry DTO.</param>
        private async Task FindOrCreateEntityWithSentimentAsync(Aspect aspect, string entityName, string sentimentName, EntryDto entry)
        {
            // Ensure the sentiment exists or create it
            var sentiment = await _context.Sentiments.FirstOrDefaultAsync(s => s.Name == sentimentName);
            if (sentiment == null)
            {
                sentiment = new Sentiment { Name = sentimentName };
                _context.Sentiments.Add(sentiment);
                await _context.SaveChangesAsync();
            }

            // Find or create the entity and associate it with the sentiment
            var entity = await _context.Entities
                                       .Include(e => e.EntitySentiments)
                                       .FirstOrDefaultAsync(e => e.Name == entityName && e.AspectId == aspect.Id);

            if (entity == null)
            {
                entity = new Entity
                {
                    Name = entityName,
                    AspectId = aspect.Id
                };
                _context.Entities.Add(entity);
                await _context.SaveChangesAsync(); // Ensure the entity is saved to generate an Id if it's new
            }

            // Now, find or create the EntitySentiment association, considering the Entry and Frequency
            var entitySentiment = await _context.EntitySentiments
                                                .FirstOrDefaultAsync(es => es.EntityId == entity.Id && es.SentimentId == sentiment.Id && es.EntryId == entry.Id);

            if (entitySentiment == null)
            {
                entitySentiment = new EntitySentiment
                {
                    EntityId = entity.Id,
                    SentimentId = sentiment.Id,
                    EntryId = entry.Id,
                };
                _context.EntitySentiments.Add(entitySentiment);
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the aspect analysis for a given entry by removing existing analysis and adding new analysis.
        /// </summary>
        /// <param name="url">The URL of the Flask API.</param>
        /// <param name="entryDto">The entry DTO containing the content.</param>
        public async Task UpdateAspectAnalysisAsync(string url, EntryDto entryDto)
        {
            // Remove existing analysis for the entry
            await RemoveExistingAnalysisForEntry(entryDto.Id);

            // Add new analysis for the entry
            await AddAspectAnalysisAsync(url, entryDto);
        }

        /// <summary>
        /// Removes existing aspect analysis for a given entry.
        /// </summary>
        /// <param name="entryId">The ID of the entry.</param>
        private async Task RemoveExistingAnalysisForEntry(int entryId)
        {
            // Find all EntitySentiment records associated with the entry
            var entitySentiments = await _context.EntitySentiments
                                                 .Where(es => es.EntryId == entryId)
                                                 .ToListAsync();

            // Remove the found records
            _context.EntitySentiments.RemoveRange(entitySentiments);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates the entity sentiment percentages for a given user within a specified date range.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="startDate">The start date of the date range.</param>
        /// <param name="endDate">The end date of the date range.</param>
        /// <returns>A list of aspect DTOs containing entity sentiment percentages.</returns>
        public async Task<List<AspectDto>> CalculateEntitySentimentPercentagesAsync(string userId, DateOnly? startDate, DateOnly? endDate)
        {
            // Filter entries by user and date range, handling null start or end dates
            var entriesQuery = _context.Entries.AsQueryable();

            if (startDate.HasValue)
            {
                entriesQuery = entriesQuery.Where(e => e.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                entriesQuery = entriesQuery.Where(e => e.Date <= endDate.Value);
            }

            var entries = entriesQuery.Where(e => e.ApplicationUserId == userId)
                                      .Select(e => e.Id);

            // Aggregate data to get counts of each entity and sentiment combination, including names and aspect names
            var query = from es in _context.EntitySentiments
                        join entity in _context.Entities on es.EntityId equals entity.Id
                        join aspect in _context.Aspects on entity.AspectId equals aspect.Id
                        join sentiment in _context.Sentiments on es.SentimentId equals sentiment.Id
                        where entries.Contains(es.EntryId)
                        group es by new { AspectName = aspect.Name, EntityName = entity.Name, SentimentName = sentiment.Name } into g
                        select new
                        {
                            g.Key.AspectName,
                            g.Key.EntityName,
                            g.Key.SentimentName,
                            Count = g.Count()
                        };

            var results = await query.ToListAsync();

            // Organize results into hierarchical structure and order by max count for each entity
            var aspects = results.GroupBy(r => r.AspectName)
                                 .Select(g => new AspectDto
                                 {
                                     AspectName = g.Key,
                                     Entities = g.GroupBy(e => e.EntityName)
                                                 .Select(e =>
                                                 {
                                                     var totalEntityCount = e.Sum(s => s.Count);
                                                     var orderedSentiments = e.OrderByDescending(s => s.Count) // Order by count descending
                                                                             .Select(s => new SentimentPercentageDto
                                                                             {
                                                                                 SentimentName = s.SentimentName,
                                                                                 Percentage = Math.Round((double)s.Count / totalEntityCount * 100, 2)
                                                                             }).ToList();
                                                     return new EntityDto
                                                     {
                                                         EntityName = e.Key,
                                                         Sentiments = orderedSentiments
                                                     };
                                                 })
                                                 .OrderByDescending(e => e.Sentiments.Max(s => s.Percentage)) // Order entities by max percentage
                                                 .ToList()
                                 })
                                 .OrderByDescending(a => a.Entities.Max(e => e.Sentiments.Max(s => s.Percentage))) // Order aspects by max entity percentage
                                 .ToList();

            return aspects;
        }
    }
}
