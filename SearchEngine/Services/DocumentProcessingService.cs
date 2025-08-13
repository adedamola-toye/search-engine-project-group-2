using IronWord;
using SearchEngine.Models.Domain;
using System.Text.RegularExpressions;

namespace SearchEngine.Services;

/// <summary>
/// Service for processing documents using IronWord and extracting keywords
/// </summary>
public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly HashSet<string> _stopWords;
    
    public DocumentProcessingService()
    {
        // Initialize common English stop words
        _stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "and", "are", "as", "at", "be", "by", "for", "from", "has", "he", "in", "is", "it",
            "its", "of", "on", "that", "the", "to", "was", "will", "with", "the", "this", "but", "they",
            "have", "had", "what", "said", "each", "which", "she", "do", "how", "their", "if", "up", "out",
            "many", "then", "them", "these", "so", "some", "her", "would", "make", "like", "into", "him",
            "time", "two", "more", "go", "no", "way", "could", "my", "than", "first", "been", "call", "who",
            "oil", "sit", "now", "find", "down", "day", "did", "get", "come", "made", "may", "part"
        };
    }

    /// <summary>
    /// Extracts text content from a document using IronWord
    /// </summary>
    public async Task<string> ExtractTextAsync(string filePath, string fileType)
    {
        try
        {
            return await Task.Run(() =>
            {
                switch (fileType.ToLower())
                {
                    case "doc":
                    case "docx":
                        var wordDoc = new WordDocument(filePath);
                        return wordDoc.ExtractText();
                    
                    case "txt":
                        return File.ReadAllText(filePath);
                    
                    default:
                        throw new NotSupportedException($"File type '{fileType}' is not supported for text extraction");
                }
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to extract text from {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Processes a document and creates DocumentKeyword entities
    /// </summary>
    public async Task<List<DocumentKeyword>> ProcessDocumentAsync(Document document, string filePath)
    {
        try
        {
            // Extract text from the document
            var text = await ExtractTextAsync(filePath, document.FileType);
            
            // Extract keywords with frequencies
            var keywordFrequencies = ExtractKeywords(text);
            
            // Calculate total word count for TF calculation
            var totalWords = keywordFrequencies.Values.Sum();
            
            // Create DocumentKeyword entities
            var documentKeywords = new List<DocumentKeyword>();
            
            foreach (var kvp in keywordFrequencies)
            {
                var keyword = kvp.Key;
                var frequency = kvp.Value;
                
                // Calculate Term Frequency (TF)
                var termFrequency = totalWords > 0 ? (double)frequency / totalWords : 0;
                
                var documentKeyword = new DocumentKeyword
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    Term = keyword,
                    NormalizedTerm = keyword.ToLowerInvariant(),
                    Frequency = frequency,
                    TermFrequency = termFrequency,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                documentKeywords.Add(documentKeyword);
            }
            
            return documentKeywords;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to process document {document.FileName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates inverted index entries from document keywords
    /// </summary>
    public List<InvertedIndexEntry> CreateInvertedIndexEntries(List<DocumentKeyword> documentKeywords)
    {
        var invertedIndexEntries = new List<InvertedIndexEntry>();
        
        foreach (var documentKeyword in documentKeywords)
        {
            var entry = new InvertedIndexEntry
            {
                Id = Guid.NewGuid(),
                DocumentKeywordId = documentKeyword.Id,
                NormalizedTerm = documentKeyword.NormalizedTerm,
                InverseDocumentFrequency = 0, // Will be calculated later
                TfIdfScore = 0, // Will be calculated later
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            invertedIndexEntries.Add(entry);
        }
        
        return invertedIndexEntries;
    }

    /// <summary>
    /// Extracts keywords from text content
    /// </summary>
    public Dictionary<string, int> ExtractKeywords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new Dictionary<string, int>();

        // Convert to lowercase and split into words
        var words = Regex.Split(text.ToLowerInvariant(), @"\W+")
                         .Where(word => !string.IsNullOrWhiteSpace(word))
                         .Where(word => word.Length >= 3) // Filter out very short words
                         .Where(word => !_stopWords.Contains(word)) // Remove stop words
                         .Where(word => !IsNumeric(word)); // Remove pure numbers

        // Count word frequencies
        var keywordFrequencies = new Dictionary<string, int>();
        
        foreach (var word in words)
        {
            if (keywordFrequencies.ContainsKey(word))
                keywordFrequencies[word]++;
            else
                keywordFrequencies[word] = 1;
        }

        // Return keywords sorted by frequency (most frequent first)
        return keywordFrequencies
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Checks if a string is purely numeric
    /// </summary>
    private static bool IsNumeric(string word)
    {
        return double.TryParse(word, out _);
    }
}
