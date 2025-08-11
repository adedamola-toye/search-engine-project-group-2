using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Models.Domain;

/// <summary>
/// Represents a document in the search engine repository
/// Supports multiple file types: pdf, doc, docx, ppt, ppts, xls, xlsx, txt, html, xml
/// </summary>
public class Document
{
    public Guid Id { get; set; }

    [NotMapped]
    public IFormFile File { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    
    public string FilePath { get; set; } = string.Empty;
    
    public string FileType { get; set; } = string.Empty; // pdf, doc, docx, ppt, ppts, xls, xlsx, txt, html, xml
    
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime IndexedAt { get; set; }
    public bool IsIndexed { get; set; } = false;
    
    // Navigation properties
    public virtual ICollection<DocumentKeyword> DocumentKeywords { get; set; } = new List<DocumentKeyword>();
}
