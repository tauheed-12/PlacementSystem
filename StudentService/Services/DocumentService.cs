using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StudentService.Entities;
using StudentService.Exceptions;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;

namespace StudentService.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IStudentRepository _repo;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IStudentRepository repo, ILogger<DocumentService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task UploadDocumentAsync(Guid userId, string documentUrl, string documentType)
        {
            if (string.IsNullOrWhiteSpace(documentUrl))
            {
                _logger.LogError("UploadDocument validation failed for user {UserId}: documentUrl is required", userId);
                throw new ValidationException("Document URL is required");
            }

            if (!Uri.IsWellFormedUriString(documentUrl, UriKind.Absolute))
            {
                _logger.LogError("UploadDocument validation failed for user {UserId}: invalid URL '{Url}'", userId, documentUrl);
                throw new ValidationException("Document URL is not valid");
            }

            if (string.IsNullOrWhiteSpace(documentType))
            {
                _logger.LogError("UploadDocument validation failed for user {UserId}: documentType is required", userId);
                throw new ValidationException("Document type is required");
            }

            documentType = documentType.Trim();

            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            // Prevent exact duplicate document (same url and type)
            if (student.Documents.Any(d =>
                string.Equals(d.DocumentUrl, documentUrl, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(d.DocumentType, documentType, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Duplicate document upload attempted for user {UserId}", userId);
                throw new ConflictException("Document already exists");
            }

            const int MaxDocuments = 20;
            if (student.Documents.Count >= MaxDocuments)
            {
                _logger.LogWarning("User {UserId} has reached max documents ({MaxDocuments})", userId, MaxDocuments);
                throw new ValidationException($"Maximum number of documents ({MaxDocuments}) reached");
            }

            student.Documents.Add(new StudentDocument
            {
                Id = Guid.NewGuid(),
                DocumentUrl = documentUrl,
                DocumentType = documentType,
                UploadedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Document uploaded for user {UserId}: {Type}", userId, documentType);
        }

        public async Task DeleteDocumentAsync(Guid userId, Guid documentId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            var document = student.Documents.FirstOrDefault(d => d.Id == documentId);

            if (document == null)
            {
                _logger.LogWarning("Attempt to delete non-existent document {DocumentId} for user {UserId}", documentId, userId);
                throw new NotFoundException("Document not found");
            }

            student.Documents.Remove(document);

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} deleted for user {UserId}", documentId, userId);
        }

        public async Task<List<StudentDocument>> GetDocumentsAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            return student.Documents.ToList();
        }
    }
}