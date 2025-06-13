using NoteMoodUOW.Core.Dtos.EntryDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface IEntryRepository
    {
        public Task<EntryDto> AddEntryAsync(EntryDto entryDto, string userId);

        public Task<EntryDto> GetEntryByIdAsync(int id, string userId);

        public Task<IEnumerable<EntryDto>> GetAllEntriesAsync(string userId);

        public Task<EntryDto> UpdateEntryAsync(EntryDto entryDto, string userId);

        public bool DeleteEntry(int id, string userId);

        public Task<IEnumerable<EntryDto>> FilterEntriesAsync(string userId, FilterEntriesDto filterDto);

        
    }
}
