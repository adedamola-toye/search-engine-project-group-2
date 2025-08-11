using Microsoft.AspNetCore.Mvc;
using SearchEngine.Models.Domain;
using SearchEngine.Models.DTO;

namespace SearchEngine.Controllers;

public class DocumentController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentController(IDocumentRepository documentRepository)
    {
        this._documentRepository = documentRepository;
    }

    [HttpPost]
    [Route("api/documents")]
    public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequestDto request)
    {
        ValidateFileUpload(request);
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var document = new Document
        {
            File = request.File,
            FileName = request.File.FileName,
            FileType = Path.GetExtension(request.File.FileName).ToLower(),
            FileSizeBytes = request.File.Length,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IndexedAt = DateTime.UtcNow,
            IsIndexed = false,
        };
        await _documentRepository.UploadDocumentAsync(document);
        return Ok(document);
    }

    private void ValidateFileUpload(DocumentUploadRequestDto request)
    {
        var allowedExtensions = new string[] { ".pdf", ".docx", ".txt", ".csv", ".xls", ".xlsx", ".ppt", ".pptx" };
        if (request.File == null || request.File.Length == 0)
        {
            ModelState.AddModelError("File", "File is required");
        }
        if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName).ToLower()))
        {
            ModelState.AddModelError("File", "Invalid file type");
        }

        if (request.File.Length > 3 * 1024 * 1024) // 3MB
        {
            ModelState.AddModelError("File", "File size is too large");
        }
    }
    
}