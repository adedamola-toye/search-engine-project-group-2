using SearchEngine.Models.Domain;
using SearchEngine.Data;

namespace SearchEngine.Repositories;

public class SQLDocumentRepository : IDocumentRepository
{
    private readonly SearchEngineDBContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SQLDocumentRepository(SearchEngineDBContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
    {
        this._context = context;
        this._webHostEnvironment = webHostEnvironment;
        this._httpContextAccessor = httpContextAccessor;
    }

    public async Task<Document> UploadDocumentAsync(Document document)
    {
        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(document.File.FileName);
        var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        await document.File.CopyToAsync(stream);

        var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Uploads/{uniqueFileName}";

        document.FilePath = urlFilePath;

        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        return document;
    }
}