using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Models.Domain;

/// <summary>
/// Represents a document in the search engine repository
/// Supports multiple file types: pdf, doc, docx, ppt, ppts, xls, xlsx, txt, html, xml
/// </summary>
public class Document
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string FileType { get; set; } = string.Empty; // pdf, doc, docx, ppt, ppts, xls, xlsx, txt, html, xml
    
    public long FileSizeBytes { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime IndexedAt { get; set; }
    
    public bool IsIndexed { get; set; } = false;
    
    // Navigation properties
    public virtual ICollection<DocumentKeyword> DocumentKeywords { get; set; } = new List<DocumentKeyword>();
}
