namespace SearchEngine.Models.DTO;

/// <summary>
/// Response DTO for Document with its keywords
/// Prevents circular reference issues in JSON serialization
/// </summary>
public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime IndexedAt { get; set; }
    public bool IsIndexed { get; set; }
    
    public List<DocumentKeywordDto> DocumentKeywords { get; set; } = new List<DocumentKeywordDto>();
}

/// <summary>
/// Response DTO for DocumentKeyword without circular reference to Document
/// </summary>
public class DocumentKeywordDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Term { get; set; } = string.Empty;
    public string NormalizedTerm { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double TermFrequency { get; set; }
    public double InverseDocumentFrequency { get; set; }
    public double TfIdfScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
