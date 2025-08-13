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
    private readonly IInvertedIndexRepository _invertedIndexRepository;
    private readonly IDocumentProcessingService _documentProcessingService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DocumentController(
        IDocumentRepository documentRepository,
        IDocumentKeywordRepository documentKeywordRepository,
        IInvertedIndexRepository invertedIndexRepository,
        IDocumentProcessingService documentProcessingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _documentRepository = documentRepository;
        _documentKeywordRepository = documentKeywordRepository;
        _invertedIndexRepository = invertedIndexRepository;
        _documentProcessingService = documentProcessingService;
        _httpContextAccessor = httpContextAccessor;
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
            var physicalFilePath = Path.Combine(uploadsFolder, uniqueFileName);
            var urlPath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Uploads/{uniqueFileName}";
            
            using (var stream = new FileStream(physicalFilePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            document.FilePath = urlPath;

            // Save document to database
            await _documentRepository.UploadDocumentAsync(document);

            // Process document with IronWord to extract keywords
            var documentKeywords = await _documentProcessingService.ProcessDocumentAsync(document, physicalFilePath);

            // Save keywords to database
            await _documentKeywordRepository.AddDocumentKeywordsAsync(documentKeywords);

            // Create inverted index entries
            var invertedIndexEntries = _documentProcessingService.CreateInvertedIndexEntries(documentKeywords);
            await _invertedIndexRepository.AddInvertedIndexEntriesAsync(invertedIndexEntries);

            // Update TF-IDF scores for all inverted index entries
            await _invertedIndexRepository.UpdateTfIdfScoresAsync();

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
            
            // Convert to DTOs to avoid circular references
            var documentDtos = documents.Select(doc => new DocumentResponseDto
            {
                Id = doc.Id,
                FileName = doc.FileName,
                FilePath = doc.FilePath,
                FileType = doc.FileType,
                FileSizeBytes = doc.FileSizeBytes,
                CreatedAt = doc.CreatedAt,
                UpdatedAt = doc.UpdatedAt,
                IndexedAt = doc.IndexedAt,
                IsIndexed = doc.IsIndexed
            }).ToList();
            
            return Ok(documentDtos);
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
            
            // Convert to DTO to avoid circular references
            var documentDto = new DocumentResponseDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FilePath = document.FilePath,
                FileType = document.FileType,
                FileSizeBytes = document.FileSizeBytes,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                IndexedAt = document.IndexedAt,
                IsIndexed = document.IsIndexed
            };
            
            return Ok(documentDto);
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
            
            // Search for documents containing these keywords using inverted index
            var invertedIndexEntries = await _invertedIndexRepository.SearchByKeywordsAsync(keywords);
            
            // Group by document and calculate relevance scores
            var searchResults = invertedIndexEntries
                .GroupBy(ie => ie.DocumentKeyword.Document)
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
                    MatchedKeywords = g.Select(ie => new
                    {
                        ie.DocumentKeyword.Term,
                        ie.DocumentKeyword.Frequency,
                        ie.TfIdfScore
                    }).ToList(),
                    TotalRelevanceScore = g.Sum(ie => ie.TfIdfScore),
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