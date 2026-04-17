using StudentService.Entities;

namespace StudentService.Services.Interfaces
{
    public interface IDocumentService
    {
        Task UploadDocumentAsync(Guid userId, string documentUrl, string documentType, CancellationToken cancellationToken);
        Task DeleteDocumentAsync(Guid userId, Guid documentId, CancellationToken cancellationToken);
        Task<List<StudentDocument>> GetDocumentsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
