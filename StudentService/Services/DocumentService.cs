using StudentService.Entities;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;

namespace StudentService.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IStudentRepository _repo;

        public DocumentService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public async Task UploadDocumentAsync(Guid userId, string documentUrl, string documentType)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Student profile not found");

            student.Documents.Add(new StudentDocument
            {
                Id = Guid.NewGuid(),
                DocumentUrl = documentUrl,
                DocumentType = documentType,
                UploadedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(Guid userId, Guid documentId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Student profile not found");

            var document = student.Documents.FirstOrDefault(d => d.Id == documentId);

            if (document == null)
                throw new KeyNotFoundException("Document not found");

            student.Documents.Remove(document);

            await _repo.SaveChangesAsync();
        }

        public async Task<List<StudentDocument>> GetDocumentsAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Student profile not found");

            return student.Documents.ToList();
        }
    }
}