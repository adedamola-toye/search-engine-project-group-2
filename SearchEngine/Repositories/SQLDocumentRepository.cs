using SearchEngine.Models.Domain;
using SearchEngine.Data;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Repositories;

public class SQLDocumentRepository : IDocumentRepository
{
    private readonly SearchEngineDBContext _context;

    public SQLDocumentRepository(SearchEngineDBContext context)
    {
        _context = context;
    }

    public async Task<Document> UploadDocumentAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<Document> UpdateDocumentAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<Document?> GetDocumentByIdAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.DocumentKeywords)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Document>> GetAllDocumentsAsync()
    {
        return await _context.Documents
            .Include(d => d.DocumentKeywords)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}