using NoteMoodUOW.Core.Dtos.EntryDtos;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface ISearchService
    {
        bool IsIndexExists();
        void IndexEntry(Entry entry);
        void DeleteIndexEntry(int entryId);
        void DeleteIndex();
        Task<IEnumerable<Entry>> SearchEntries(string userId, FilterEntriesDto filterDto);
    }
}
