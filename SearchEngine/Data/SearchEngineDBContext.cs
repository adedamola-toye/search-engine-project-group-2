using Microsoft.EntityFrameworkCore;
using SearchEngine.Models.Domain;

namespace SearchEngine.Data;

public class SearchEngineDBContext : DbContext
{
    public SearchEngineDBContext(DbContextOptions<SearchEngineDBContext> dbContextOptions) : base(dbContextOptions)
    {
    }
    
    // Define DbSets for all domain models
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentKeyword> DocumentKeywords { get; set; }
    public DbSet<InvertedIndexEntry> InvertedIndexEntries { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Document entity
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.HasIndex(e => e.FileType);
            entity.HasIndex(e => e.IsIndexed);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UpdatedAt);
        });
        
        // Configure DocumentKeyword entity
        modelBuilder.Entity<DocumentKeyword>(entity =>
        {
            entity.HasIndex(e => new { e.DocumentId, e.NormalizedTerm }).IsUnique();
            entity.HasIndex(e => e.Term);
            entity.HasIndex(e => e.NormalizedTerm);
            entity.HasIndex(e => e.TfIdfScore);
            entity.HasIndex(e => e.IsStopWord);
            
            entity.HasOne(dk => dk.Document)
                  .WithMany(d => d.DocumentKeywords)
                  .HasForeignKey(dk => dk.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure InvertedIndexEntry
        modelBuilder.Entity<InvertedIndexEntry>(entity =>
        {
            entity.HasIndex(e => new { e.DocumentKeywordId, e.NormalizedTerm }).IsUnique();
            entity.HasIndex(e => e.NormalizedTerm);
            entity.HasIndex(e => e.TfIdfScore);
            
            entity.HasOne(iie => iie.DocumentKeyword)
                  .WithMany()
                  .HasForeignKey(iie => iie.DocumentKeywordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}