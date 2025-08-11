using SearchEngine.Models.Domain;

namespace SearchEngine.Repositories;

/// <summary>
/// Repository interface for DocumentKeyword operations
/// </summary>
public interface IDocumentKeywordRepository
{
    /// <summary>
    /// Adds multiple document keywords to the database
    /// </summary>
    /// <param name="documentKeywords">List of DocumentKeyword entities to add</param>
    Task AddDocumentKeywordsAsync(List<DocumentKeyword> documentKeywords);
    
    /// <summary>
    /// Gets all keywords for a specific document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>List of DocumentKeyword entities</returns>
    Task<List<DocumentKeyword>> GetKeywordsByDocumentIdAsync(Guid documentId);
    
    /// <summary>
    /// Searches for documents containing specific keywords
    /// </summary>
    /// <param name="keywords">List of keywords to search for</param>
    /// <returns>List of DocumentKeyword entities matching the search</returns>
    Task<List<DocumentKeyword>> SearchByKeywordsAsync(List<string> keywords);
    
    /// <summary>
    /// Updates TF-IDF scores for all document keywords
    /// This should be called after adding new documents to recalculate IDF scores
    /// </summary>
    Task UpdateTfIdfScoresAsync();
    
    /// <summary>
    /// Deletes all keywords for a specific document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    Task DeleteKeywordsByDocumentIdAsync(Guid documentId);
}
