using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NoteMoodUOW.Core.Dtos.EntryDtos;
using Microsoft.Extensions.Logging;  // Add this namespace

namespace NoteMoodUOW.EF.Repositories
{
    /// <summary>
    /// Provides functionality for indexing and searching entries using Lucene.NET search engine and efficient search queries.
    /// </summary>
    public class LuceneSearchService : ISearchService, IDisposable
    {
        private const LuceneVersion version = LuceneVersion.LUCENE_48;
        private const int MaxResults = 10;
        private readonly FSDirectory _directory;
        private readonly Analyzer _analyzer;
        private readonly IndexWriter _writer;
        //private readonly ILogger<LuceneSearchService> _logger;  // Add this field
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the LuceneSearchService.
        /// </summary>
        public LuceneSearchService(/*ILogger<LuceneSearchService> logger*/)  // Modify constructor to accept ILogger
        {
            //_logger = logger;
            // Construct a machine-independent path for the index
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var indexPath = Path.Combine(basePath, "index");
            _directory = FSDirectory.Open(indexPath);
            _analyzer = new StandardAnalyzer(version);
            var config = new IndexWriterConfig(version, _analyzer);
            _writer = new IndexWriter(_directory, config);
        }

        /// <summary>
        /// Disposes of the resources used by the LuceneSearchService.
        /// </summary>
        /// <param name="disposing">Indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _writer?.Dispose();
                    _analyzer?.Dispose();
                    _directory?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deletes all documents from the index.
        /// </summary>
        public void DeleteIndex()
        {
            // _logger.LogInformation("Deleting all documents from the index.");
            _writer.DeleteAll();
            _writer.Commit();
        }

        /// <summary>
        /// Indexes a single entry.
        /// </summary>
        /// <param name="entry">The entry to index.</param>
        public void IndexEntry(Entry entry)
        {
            // _logger.LogInformation("Indexing entry with ID: {EntryId}", entry.Id);
            var doc = new Document
                {
                    new StringField("Id", entry.Id.ToString(), Field.Store.YES),
                    new StringField("UserId", entry.ApplicationUserId, Field.Store.YES),
                    new TextField("Title", entry.Title, Field.Store.YES),
                    new TextField("Content", entry.Content, Field.Store.YES),
                    new StringField("Date", entry.Date.ToString("yyyy-MM-dd"), Field.Store.YES),
                    new StoredField("Time", entry.Time.ToString()),
                    new StoredField("SentimentId", entry.SentimentId.ToString()),
                    new StringField("SentimentName", entry.Sentiment.Name, Field.Store.YES)
                };
            _writer.UpdateDocument(new Term("Id", entry.Id.ToString()), doc);
            _writer.Commit();
        }

        /// <summary>
        /// Deletes a single entry from the index based on the entry Id.
        /// </summary>
        /// <param name="entryId">The ID of the entry to delete.</param>
        public void DeleteIndexEntry(int entryId)
        {
            // _logger.LogInformation("Deleting entry with ID: {EntryId} from the index.", entryId);
            _writer.DeleteDocuments(new Term("Id", entryId.ToString()));
            _writer.Commit();
        }

        /// <summary>
        /// Checks if the index exists.
        /// </summary>
        /// <returns>True if the index exists, otherwise false.</returns>
        public bool IsIndexExists()
        {
            // _logger.LogInformation("Checking if the index exists.");
            return DirectoryReader.IndexExists(_directory);
        }

        /// <summary>
        /// Searches entries based on the provided user ID and filter criteria like ( Query , StartDate, EndDate, SentimentName).
        /// </summary>
        /// <param name="userId">The user ID to scope the search.</param>
        /// <param name="filterDto">The filter criteria for the search.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of matching entries.</returns>
        public async Task<IEnumerable<Entry>> SearchEntries(string userId, FilterEntriesDto filterDto)
        {
            // _logger.LogInformation("Searching entries for user ID: {UserId} with filter criteria.", userId);
            try
            {
                using var reader = DirectoryReader.Open(_directory);
                var searcher = new IndexSearcher(reader);

                var booleanQuery = new BooleanQuery();

                // Ensure search is scoped to the user
                var userQuery = new TermQuery(new Term("UserId", userId));
                booleanQuery.Add(userQuery, Occur.MUST);

                if (!string.IsNullOrEmpty(filterDto.Query))
                {
                    // Sanitize the query string
                    var sanitizedQuery = SanitizeQuery(filterDto.Query);

                    var fields = new[] { "Title", "Content" };

                    // Use a MultiFieldQueryParser for flexible queries
                    var parser = new MultiFieldQueryParser(version, fields, _analyzer);

                    // Build a fuzzy query and a wildcard query
                    var fuzzyQuery = parser.Parse(sanitizedQuery + "~"); // Fuzzy query
                    var wildcardQuery = parser.Parse(sanitizedQuery + "*"); // Wildcard query

                    // Combine the fuzzy and wildcard queries
                    var combinedQuery = new BooleanQuery
                        {
                            { fuzzyQuery, Occur.SHOULD },
                            { wildcardQuery, Occur.SHOULD }
                        };

                    booleanQuery.Add(combinedQuery, Occur.MUST);
                }

                if (filterDto.StartDate.HasValue && filterDto.EndDate.HasValue)
                {
                    var start = filterDto.StartDate?.ToString("yyyy-MM-dd") ?? DateOnly.MinValue.ToString("yyyy-MM-dd");
                    var end = filterDto.EndDate?.ToString("yyyy-MM-dd") ?? DateOnly.MaxValue.ToString("yyyy-MM-dd");

                    var dateRangeQuery = TermRangeQuery.NewStringRange("Date", start, end, true, true);

                    booleanQuery.Add(dateRangeQuery, Occur.MUST);
                }

                // Add sentiment name filter if SentimentName has a value
                if (!string.IsNullOrEmpty(filterDto.SentimentName))
                {
                    var sentimentNameQuery = new TermQuery(new Term("SentimentName", filterDto.SentimentName));
                    booleanQuery.Add(sentimentNameQuery, Occur.MUST);
                }

                var hits = searcher.Search(booleanQuery, MaxResults).ScoreDocs;
                var entries = hits.Select(hit =>
                {
                    var doc = searcher.Doc(hit.Doc);
                    return new Entry
                    {
                        Id = int.Parse(doc.Get("Id")),
                        ApplicationUserId = doc.Get("UserId"),
                        Title = doc.Get("Title"),
                        Content = doc.Get("Content"),
                        Date = DateOnly.Parse(doc.Get("Date")),
                        Time = TimeOnly.Parse(doc.Get("Time")),
                        SentimentId = int.Parse(doc.Get("SentimentId")),
                        Sentiment = new Sentiment
                        {
                            Id = int.Parse(doc.Get("SentimentId")),
                            Name = doc.Get("SentimentName")
                        }
                    };
                }).ToList();

                return await Task.FromResult(entries);
            }
            // Catch any exceptions that occur parsing the query
            catch (ParseException ex)
            {
                // _logger.LogError(ex, "Error parsing the query.");
                return Enumerable.Empty<Entry>();
            }
            // Catch any other exceptions
            catch (Exception ex)
            {
                // _logger.LogError(ex, "An error occurred while searching entries.");
                return Enumerable.Empty<Entry>();
            }
        }

        /// <summary>
        /// Sanitizes the query string by removing special characters used by Lucene.
        /// </summary>
        /// <param name="query">The query string to sanitize.</param>
        /// <returns>The sanitized query string.</returns>
        private string SanitizeQuery(string query)
        {
            // Regular expression to remove all special characters used by Lucene
            var sanitizedQuery = Regex.Replace(query, @"[+\-&|!(){}[\]^""~*?:\\;]", "");
            return sanitizedQuery.Trim();
        }
    }
}
