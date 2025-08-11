using SearchEngine.Models.Domain;  


public interface IDocumentRepository
{
    Task<Document> UploadDocumentAsync(Document document);
}
