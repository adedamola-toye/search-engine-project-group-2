using Microsoft.EntityFrameworkCore;
using SearchEngine.Data;
using SearchEngine.Models.Domain;
using Xunit;

public class SearchEngineDBContextTests
{
    [Fact]
    public void CanAddAndRetrieveDocument()
    {
        var options = new DbContextOptionsBuilder<SearchEngineDBContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new SearchEngineDBContext(options))
        {
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "test.txt",
                FilePath = "Uploads/test.txt",
                FileType = "txt",
                FileSizeBytes = 123,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IndexedAt = DateTime.UtcNow,
                IsIndexed = false
            };
            context.Documents.Add(doc);
            context.SaveChanges();
        }

        using (var context = new SearchEngineDBContext(options))
        {
            var doc = context.Documents.FirstOrDefault();
            Assert.NotNull(doc);
            Assert.Equal("test.txt", doc.FileName);
        }
    }

    [Fact]
    public void CanAddAndRetrieveDocumentKeyword()
    {
        var options = new DbContextOptionsBuilder<SearchEngineDBContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Keyword")
            .Options;

        using (var context = new SearchEngineDBContext(options))
        {
            var keyword = new DocumentKeyword
            {
                Id = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                Keyword = "search",
                Frequency = 5
            };
            context.DocumentKeywords.Add(keyword);
            context.SaveChanges();
        }

        using (var context = new SearchEngineDBContext(options))
        {
            var keyword = context.DocumentKeywords.FirstOrDefault();
            Assert.NotNull(keyword);
            Assert.Equal("search", keyword.Keyword);
            Assert.Equal(5, keyword.Frequency);
        }
    }

    [Fact]
    public void CanAddAndRetrieveInvertedIndexEntry()
    {
        var options = new DbContextOptionsBuilder<SearchEngineDBContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Index")
            .Options;

        using (var context = new SearchEngineDBContext(options))
        {
            var entry = new InvertedIndexEntry
            {
                Id = Guid.NewGuid(),
                Keyword = "engine",
                DocumentId = Guid.NewGuid(),
                Position = 1
            };
            context.InvertedIndexEntries.Add(entry);
            context.SaveChanges();
        }

        using (var context = new SearchEngineDBContext(options))
        {
            var entry = context.InvertedIndexEntries.FirstOrDefault();
            Assert.NotNull(entry);
            Assert.Equal("engine", entry.Keyword);
            Assert.Equal(1, entry.Position);
        }
    }

    [Fact]
    public void CanDeleteDocument()
    {
        var options = new DbContextOptionsBuilder<SearchEngineDBContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Delete")
            .Options;

        Guid docId;
        using (var context = new SearchEngineDBContext(options))
        {
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "delete.txt",
                FilePath = "Uploads/delete.txt",
                FileType = "txt",
                FileSizeBytes = 50,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IndexedAt = DateTime.UtcNow,
                IsIndexed = false
            };
            docId = doc.Id;
            context.Documents.Add(doc);
            context.SaveChanges();
        }

        using (var context = new SearchEngineDBContext(options))
        {
            var doc = context.Documents.Find(docId);
            context.Documents.Remove(doc);
            context.SaveChanges();
        }

        using (var context = new SearchEngineDBContext(options))
        {
            var doc = context.Documents.Find(docId);
            Assert.Null(doc);
        }
    }
}