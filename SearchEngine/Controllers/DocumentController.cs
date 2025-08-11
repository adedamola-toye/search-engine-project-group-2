using Microsoft.AspNetCore.Mvc;
using SearchEngine.Models.Domain;
using SearchEngine.Models.DTO;
using SearchEngine.Repositories;
using SearchEngine.Services;

namespace SearchEngine.Controllers;

[ApiController]
public class DocumentController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentKeywordRepository _documentKeywordRepository;
    private readonly IDocumentProcessingService _documentProcessingService;

    public DocumentController(
        IDocumentRepository documentRepository,
        IDocumentKeywordRepository documentKeywordRepository,
        IDocumentProcessingService documentProcessingService)
    {
        _documentRepository = documentRepository;
        _documentKeywordRepository = documentKeywordRepository;
        _documentProcessingService = documentProcessingService;
    }

    [HttpPost]
    [Route("api/documents")]
    public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequestDto request)
    {
        try
        {
            ValidateFileUpload(request);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create the document entity
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = request.File.FileName,
                FileType = Path.GetExtension(request.File.FileName).TrimStart('.').ToLower(),
                FileSizeBytes = request.File.Length,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsIndexed = false
            };

            // Save the file to disk
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{document.Id}_{request.File.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            document.FilePath = filePath;

            // Save document to database
            await _documentRepository.UploadDocumentAsync(document);

            // Process document with IronWord to extract keywords
            var documentKeywords = await _documentProcessingService.ProcessDocumentAsync(document, filePath);

            // Save keywords to database
            await _documentKeywordRepository.AddDocumentKeywordsAsync(documentKeywords);

            // Update TF-IDF scores for all keywords
            await _documentKeywordRepository.UpdateTfIdfScoresAsync();

            // Mark document as indexed
            document.IsIndexed = true;
            document.IndexedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            
            // Update document in database
            await _documentRepository.UpdateDocumentAsync(document);

            var response = new
            {
                Id = document.Id,
                FileName = document.FileName,
                FileType = document.FileType,
                FileSizeBytes = document.FileSizeBytes,
                IsIndexed = document.IsIndexed,
                KeywordCount = documentKeywords.Count,
                CreatedAt = document.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to process document", details = ex.Message });
        }
    }

    private void ValidateFileUpload(DocumentUploadRequestDto request)
    {
        // Supported file types for text extraction
        var allowedExtensions = new string[] { ".doc", ".docx", ".txt" };
        
        if (request.File == null || request.File.Length == 0)
        {
            ModelState.AddModelError("File", "File is required");
        }
        
        if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName).ToLower()))
        {
            ModelState.AddModelError("File", $"Invalid file type. Supported types: {string.Join(", ", allowedExtensions)}");
        }

        if (request.File.Length > 10 * 1024 * 1024) // 10MB
        {
            ModelState.AddModelError("File", "File size is too large (max 10MB)");
        }
    }

    [HttpGet]
    [Route("api/documents")]
    public async Task<IActionResult> GetAllDocuments()
    {
        try
        {
            var documents = await _documentRepository.GetAllDocumentsAsync();
            return Ok(documents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve documents", details = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/documents/{id}")]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        try
        {
            var document = await _documentRepository.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound(new { error = "Document not found" });
            }
            return Ok(document);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve document", details = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/search")]
    public async Task<IActionResult> SearchDocuments([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Query parameter is required" });
            }

            // Split query into keywords
            var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            
            // Search for documents containing these keywords
            var documentKeywords = await _documentKeywordRepository.SearchByKeywordsAsync(keywords);
            
            // Group by document and calculate relevance scores
            var searchResults = documentKeywords
                .GroupBy(dk => dk.Document)
                .Select(g => new
                {
                    Document = new
                    {
                        g.Key.Id,
                        g.Key.FileName,
                        g.Key.FileType,
                        g.Key.FileSizeBytes,
                        g.Key.CreatedAt,
                        g.Key.IsIndexed
                    },
                    MatchedKeywords = g.Select(dk => new
                    {
                        dk.Term,
                        dk.Frequency,
                        dk.TfIdfScore
                    }).ToList(),
                    TotalRelevanceScore = g.Sum(dk => dk.TfIdfScore),
                    KeywordMatches = g.Count()
                })
                .OrderByDescending(r => r.TotalRelevanceScore)
                .ToList();

            return Ok(new
            {
                Query = query,
                ResultCount = searchResults.Count,
                Results = searchResults
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Search failed", details = ex.Message });
        }
    }
    
}