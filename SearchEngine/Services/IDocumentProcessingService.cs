using SearchEngine.Models.Domain;

namespace SearchEngine.Services;

/// <summary>
/// Service interface for processing documents and extracting keywords
/// </summary>
public interface IDocumentProcessingService
{
    /// <summary>
    /// Extracts text content from a document file
    /// </summary>
    /// <param name="filePath">Path to the document file</param>
    /// <param name="fileType">Type of the document (doc, docx, pdf, etc.)</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextAsync(string filePath, string fileType);
    
    /// <summary>
    /// Processes a document and creates DocumentKeyword entities
    /// </summary>
    /// <param name="document">The document to process</param>
    /// <param name="filePath">Path to the document file</param>
    /// <returns>List of DocumentKeyword entities</returns>
    Task<List<DocumentKeyword>> ProcessDocumentAsync(Document document, string filePath);
    
    /// <summary>
    /// Extracts keywords from text content
    /// </summary>
    /// <param name="text">Text content to analyze</param>
    /// <returns>Dictionary of keywords with their frequencies</returns>
    Dictionary<string, int> ExtractKeywords(string text);
}
