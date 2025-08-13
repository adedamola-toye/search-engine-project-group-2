using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Models.Domain;

/// <summary>
/// Represents an entry in the inverted index
/// Maps terms to documents for efficient searching
/// This is the core data structure for fast query processing
/// </summary>
public class InvertedIndexEntry
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid DocumentKeywordId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string NormalizedTerm { get; set; } = string.Empty; // The search term (normalized for fast lookup)
    
    public double InverseDocumentFrequency { get; set; } // IDF score (calculated across all documents)
    
    public double TfIdfScore { get; set; } // Pre-calculated TF-IDF score for fast ranking
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual DocumentKeyword DocumentKeyword { get; set; } = null!;
}
