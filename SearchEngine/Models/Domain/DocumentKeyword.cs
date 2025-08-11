using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Models.Domain;

/// <summary>
/// Represents keywords extracted from documents with their frequency and scoring information
/// Stores the relationship between documents and their keywords directly
/// Used for TF-IDF calculations and ranking
/// </summary>
public class DocumentKeyword
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid DocumentId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Term { get; set; } = string.Empty; // The actual keyword/term
    
    [MaxLength(100)]
    public string NormalizedTerm { get; set; } = string.Empty; // Lowercase, stemmed version for matching
    
    public int Frequency { get; set; } // How many times this keyword appears in this document
    
    public double TermFrequency { get; set; } // TF score (frequency / total words in document)
    
    public double InverseDocumentFrequency { get; set; } // IDF score (calculated across all documents)
    
    public double TfIdfScore { get; set; } // TF-IDF score for ranking
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Document Document { get; set; } = null!;
}
