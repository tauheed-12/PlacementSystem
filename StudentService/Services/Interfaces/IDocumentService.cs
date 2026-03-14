using StudentService.Entities;

namespace StudentService.Services.Interfaces
{
    public interface IDocumentService
    {
        Task UploadDocumentAsync(Guid userId, string documentUrl, string documentType);
        Task DeleteDocumentAsync(Guid userId, Guid documentId);
        Task<List<StudentDocument>> GetDocumentsAsync(Guid userId);
    }
}
