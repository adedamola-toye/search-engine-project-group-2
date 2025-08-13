using Microsoft.EntityFrameworkCore;
using SearchEngine.Data;
using SearchEngine.Models.Domain;

namespace SearchEngine.Repositories;

/// <summary>
/// SQL Server implementation of InvertedIndex repository
/// </summary>
public class SQLInvertedIndexRepository : IInvertedIndexRepository
{
    private readonly SearchEngineDBContext _dbContext;

    public SQLInvertedIndexRepository(SearchEngineDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds multiple inverted index entries to the database
    /// </summary>
    public async Task AddInvertedIndexEntriesAsync(List<InvertedIndexEntry> entries)
    {
        if (entries?.Any() == true)
        {
            await _dbContext.InvertedIndexEntries.AddRangeAsync(entries);
            await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Searches for documents containing specific keywords using the inverted index
    /// </summary>
    public async Task<List<InvertedIndexEntry>> SearchByKeywordsAsync(List<string> keywords)
    {
        if (keywords?.Any() != true)
            return new List<InvertedIndexEntry>();

        var normalizedKeywords = keywords.Select(k => k.ToLowerInvariant()).ToList();

        return await _dbContext.InvertedIndexEntries
            .Include(ie => ie.DocumentKeyword)
            .ThenInclude(dk => dk.Document)
            .Where(ie => normalizedKeywords.Contains(ie.NormalizedTerm))
            .OrderByDescending(ie => ie.TfIdfScore)
            .ToListAsync();
    }

    /// <summary>
    /// Updates TF-IDF scores for all inverted index entries
    /// </summary>
    public async Task UpdateTfIdfScoresAsync()
    {
        // Get total number of documents
        var totalDocuments = await _dbContext.Documents.CountAsync();
        
        if (totalDocuments == 0)
            return;

        // Get all unique normalized terms with their document counts
        var termDocumentCounts = await _dbContext.InvertedIndexEntries
            .GroupBy(ie => ie.NormalizedTerm)
            .Select(g => new { Term = g.Key, DocumentCount = g.Count() })
            .ToListAsync();

        // Update IDF and TF-IDF scores
        foreach (var termInfo in termDocumentCounts)
        {
            var idf = Math.Log((double)totalDocuments / termInfo.DocumentCount);
            
            var entriesToUpdate = await _dbContext.InvertedIndexEntries
                .Include(ie => ie.DocumentKeyword)
                .Where(ie => ie.NormalizedTerm == termInfo.Term)
                .ToListAsync();

            foreach (var entry in entriesToUpdate)
            {
                entry.InverseDocumentFrequency = idf;
                entry.TfIdfScore = entry.DocumentKeyword.TermFrequency * idf;
                entry.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes all inverted index entries for a specific document
    /// </summary>
    public async Task DeleteEntriesByDocumentIdAsync(Guid documentId)
    {
        var entriesToDelete = await _dbContext.InvertedIndexEntries
            .Include(ie => ie.DocumentKeyword)
            .Where(ie => ie.DocumentKeyword.DocumentId == documentId)
            .ToListAsync();

        if (entriesToDelete.Any())
        {
            _dbContext.InvertedIndexEntries.RemoveRange(entriesToDelete);
            await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets all inverted index entries for a specific document
    /// </summary>
    public async Task<List<InvertedIndexEntry>> GetEntriesByDocumentIdAsync(Guid documentId)
    {
        return await _dbContext.InvertedIndexEntries
            .Include(ie => ie.DocumentKeyword)
            .Where(ie => ie.DocumentKeyword.DocumentId == documentId)
            .OrderByDescending(ie => ie.TfIdfScore)
            .ToListAsync();
    }
}
