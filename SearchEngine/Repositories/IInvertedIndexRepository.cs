using SearchEngine.Models.Domain;

namespace SearchEngine.Repositories;

/// <summary>
/// Repository interface for InvertedIndexEntry operations
/// </summary>
public interface IInvertedIndexRepository
{
    /// <summary>
    /// Adds multiple inverted index entries to the database
    /// </summary>
    /// <param name="entries">List of InvertedIndexEntry entities to add</param>
    Task AddInvertedIndexEntriesAsync(List<InvertedIndexEntry> entries);
    
    /// <summary>
    /// Searches for documents containing specific keywords using the inverted index
    /// </summary>
    /// <param name="keywords">List of keywords to search for</param>
    /// <returns>List of InvertedIndexEntry entities matching the search</returns>
    Task<List<InvertedIndexEntry>> SearchByKeywordsAsync(List<string> keywords);
    
    /// <summary>
    /// Updates TF-IDF scores for all inverted index entries
    /// This should be called after adding new documents to recalculate IDF scores
    /// </summary>
    Task UpdateTfIdfScoresAsync();
    
    /// <summary>
    /// Deletes all inverted index entries for a specific document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    Task DeleteEntriesByDocumentIdAsync(Guid documentId);
    
    /// <summary>
    /// Gets all inverted index entries for a specific document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>List of InvertedIndexEntry entities</returns>
    Task<List<InvertedIndexEntry>> GetEntriesByDocumentIdAsync(Guid documentId);
}
