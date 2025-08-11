using Microsoft.EntityFrameworkCore;
using SearchEngine.Models.Domain;

namespace SearchEngine.Data;

/// <summary>
/// Database context for the Search Engine application.
/// Manages entities for document indexing and search functionality.
/// </summary>
public class SearchEngineDBContext : DbContext
{
    public SearchEngineDBContext(DbContextOptions<SearchEngineDBContext> options) : base(options)
    {
    }

    
    /// <summary>
    /// Collection of documents in the search index.
    /// </summary>
    public DbSet<Document> Documents { get; set; } = null!;
    
    /// <summary>
    /// Collection of keywords extracted from documents with their metadata.
    /// </summary>
    public DbSet<DocumentKeyword> DocumentKeywords { get; set; } = null!;
    
    /// <summary>
    /// Inverted index entries for efficient keyword-based document retrieval.
    /// </summary>
    public DbSet<InvertedIndexEntry> InvertedIndexEntries { get; set; } = null!;

}