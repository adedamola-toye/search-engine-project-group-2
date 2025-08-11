using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Models.DTO;

public class DocumentUploadRequestDto
{
    [Required]
    public IFormFile File { get; set; }

    [Required]
    public string FileName { get; set; }
}