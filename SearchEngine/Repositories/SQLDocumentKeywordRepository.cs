using Microsoft.EntityFrameworkCore;
using SearchEngine.Data;
using SearchEngine.Models.Domain;

namespace SearchEngine.Repositories;

/// <summary>
/// SQL Server implementation of DocumentKeyword repository
/// </summary>
public class SQLDocumentKeywordRepository : IDocumentKeywordRepository
{
    private readonly SearchEngineDBContext _dbContext;

    public SQLDocumentKeywordRepository(SearchEngineDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds multiple document keywords to the database
    /// </summary>
    public async Task AddDocumentKeywordsAsync(List<DocumentKeyword> documentKeywords)
    {
        if (documentKeywords?.Any() == true)
        {
            await _dbContext.DocumentKeywords.AddRangeAsync(documentKeywords);
            await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets all keywords for a specific document
    /// </summary>
    public async Task<List<DocumentKeyword>> GetKeywordsByDocumentIdAsync(Guid documentId)
    {
        return await _dbContext.DocumentKeywords
            .Where(dk => dk.DocumentId == documentId)
            .OrderByDescending(dk => dk.TfIdfScore)
            .ToListAsync();
    }

    /// <summary>
    /// Searches for documents containing specific keywords
    /// </summary>
    public async Task<List<DocumentKeyword>> SearchByKeywordsAsync(List<string> keywords)
    {
        if (keywords?.Any() != true)
            return new List<DocumentKeyword>();

        var normalizedKeywords = keywords.Select(k => k.ToLowerInvariant()).ToList();

        return await _dbContext.DocumentKeywords
            .Include(dk => dk.Document)
            .Where(dk => normalizedKeywords.Contains(dk.NormalizedTerm))
            .OrderByDescending(dk => dk.TfIdfScore)
            .ToListAsync();
    }

    /// <summary>
    /// Updates TF-IDF scores for all document keywords
    /// </summary>
    public async Task UpdateTfIdfScoresAsync()
    {
        // Get total number of documents
        var totalDocuments = await _dbContext.Documents.CountAsync();
        
        if (totalDocuments == 0)
            return;

        // Get all unique normalized terms with their document counts
        var termDocumentCounts = await _dbContext.DocumentKeywords
            .GroupBy(dk => dk.NormalizedTerm)
            .Select(g => new { Term = g.Key, DocumentCount = g.Count() })
            .ToListAsync();

        // Update IDF and TF-IDF scores
        foreach (var termInfo in termDocumentCounts)
        {
            var idf = Math.Log((double)totalDocuments / termInfo.DocumentCount);
            
            var keywordsToUpdate = await _dbContext.DocumentKeywords
                .Where(dk => dk.NormalizedTerm == termInfo.Term)
                .ToListAsync();

            foreach (var keyword in keywordsToUpdate)
            {
                keyword.InverseDocumentFrequency = idf;
                keyword.TfIdfScore = keyword.TermFrequency * idf;
                keyword.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes all keywords for a specific document
    /// </summary>
    public async Task DeleteKeywordsByDocumentIdAsync(Guid documentId)
    {
        var keywordsToDelete = await _dbContext.DocumentKeywords
            .Where(dk => dk.DocumentId == documentId)
            .ToListAsync();

        if (keywordsToDelete.Any())
        {
            _dbContext.DocumentKeywords.RemoveRange(keywordsToDelete);
            await _dbContext.SaveChangesAsync();
        }
    }
}
