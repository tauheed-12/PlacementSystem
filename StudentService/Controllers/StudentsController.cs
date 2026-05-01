using Common.Contracts.Web;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using StudentService.Constants;
using Common.Contracts.Infrastructure;
using StudentService.Services.Interfaces;
using static StudentService.DTOs.Dtos;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _service;
        private readonly RequestContextAccessor _contextAccessor;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService service, RequestContextAccessor contextAccessor, ILogger<StudentsController> logger)
        {
            _service = service;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }


        private async Task<IActionResult?> ValidateAsync<T>(T model, IValidator<T> validator, CancellationToken ct)
        {
            var result = await validator.ValidateAsync(model, ct);
            if (!result.IsValid)
                return BadRequest(ApiEnvelope<object>.Fail(
                    "Validation failed",
                    result.Errors.Select(e => e.ErrorMessage)));

            return null;
        }

    
        [HttpPost]
        public async Task<IActionResult> Create( [FromBody] CreateStudentProfileRequest dto, [FromServices] IValidator<CreateStudentProfileRequest> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(dto, validator, ct);
            if (error != null) return error;

            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student)) return Forbid();

            await _service.CreateProfileAsync(context.UserId, context.EmailId, dto, ct);
            return Created(string.Empty, ApiEnvelope<object>.Ok("Profile created successfully"));
        }

       
        [HttpGet("me")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            _logger.LogInformation("User {UserId} with roles {Roles} is attempting to access their profile", context.UserId, string.Join(",", context.Roles));
            if (!context.IsInRole(Roles.Student)) return Forbid();

            var result = await _service.GetProfileAsync(context.UserId, ct);
            return Ok(ApiEnvelope<object>.Ok("Profile fetched successfully", result));
        }

        
        [HttpGet("{studentId:guid}")]
        public async Task<IActionResult> GetById(Guid studentId, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator, Roles.Recruiter))
                return Forbid();

            var result = await _service.GetProfileByIdAsync(studentId, ct);
            return Ok(ApiEnvelope<object>.Ok("Profile fetched successfully", result));
        }

        
        [HttpPatch("me")]
        public async Task<IActionResult> Update( [FromBody] UpdateStudentProfileRequest dto, [FromServices] IValidator<UpdateStudentProfileRequest> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(dto, validator, ct);
            if (error != null) return error;

            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student)) return Forbid();

            await _service.UpdateProfileAsync(context.UserId, dto, ct);
            return Ok(ApiEnvelope<object>.Ok("Profile updated successfully"));
        }

        
        [HttpPost("bulk-profiles")]
        public async Task<IActionResult> Bulk( [FromBody] List<Guid> userIds, [FromServices] IValidator<List<Guid>> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(userIds, validator, ct);
            if (error != null) return error;

            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator, Roles.Recruiter))
                return Forbid();

            var result = await _service.GetProfilesInBulkAsync(userIds, ct);
            return Ok(ApiEnvelope<object>.Ok("Profiles fetched successfully", result));
        }

        
        [HttpDelete("{studentId:guid}")]
        public async Task<IActionResult> Delete(Guid studentId, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator))
                return Forbid();

            await _service.DeleteProfileAsync(studentId, ct);
            return Ok(ApiEnvelope<object>.Ok("Profile deleted successfully"));
        }

        
        [HttpGet("me/profile-progress")]
        public async Task<IActionResult> GetProfileProgress(CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student)) return Forbid();

            var result = await _service.GetProfileCompletionStatusAsync(context.UserId, ct);
            return Ok(ApiEnvelope<object>.Ok(
                "Profile progress fetched successfully",
                new { profileProgress = result }));
        }
    }
}