using SearchEngine.Models.Domain;

namespace SearchEngine.Repositories;

public interface IDocumentRepository
{
    Task<Document> UploadDocumentAsync(Document document);
    Task<Document> UpdateDocumentAsync(Document document);
    Task<Document?> GetDocumentByIdAsync(Guid id);
    Task<List<Document>> GetAllDocumentsAsync();
}
