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
            .OrderByDescending(dk => dk.TermFrequency)
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
            .OrderByDescending(dk => dk.TermFrequency)
            .ToListAsync();
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
