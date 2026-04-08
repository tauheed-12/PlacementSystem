// StudentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentService.Constants;
using StudentService.Infrastructure;
using StudentService.Services.Interfaces;
using static StudentService.DTOs.Dtos;

namespace StudentService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/students")]  
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _service;
        private readonly RequestContextAccessor _requestContextAccessor;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            IStudentService service,
            RequestContextAccessor requestContextAccessor,
            ILogger<StudentsController> logger)
        {
            _service = service;
            _requestContextAccessor = requestContextAccessor;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentProfileRequest request)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                await _service.CreateProfileAsync(context.UserId, request);
                return Created(string.Empty, null);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("UQ_") == true
                   || ex.InnerException?.Message.Contains("unique") == true)
            {
                _logger.LogWarning(ex, "Duplicate profile creation attempt for user {UserId}", context.UserId);
                return Conflict(new { error = "A profile already exists for this account." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error creating profile for user {UserId}", context.UserId);
                return StatusCode(500, new { error = "Failed to create profile. Please try again." });
            }
        }

        [HttpGet("me")]  
        public async Task<IActionResult> Get()
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                var profile = await _service.GetProfileAsync(context.UserId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error fetching profile for user {UserId}", context.UserId);
                return StatusCode(500, new { error = "Failed to retrieve profile. Please try again." });
            }
        }

        [HttpGet("{studentId:guid}")]
        public async Task<IActionResult> GetById(Guid studentId)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator, Roles.Recruiter))
                return Forbid();

            try
            {
                var profile = await _service.GetProfileByIdAsync(studentId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error fetching profile {StudentId}", studentId);
                return StatusCode(500, new { error = "Failed to retrieve profile. Please try again." });
            }
        }

        [HttpPatch("me")]
        public async Task<IActionResult> Update([FromBody] UpdateStudentProfileRequest dto)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                await _service.UpdateProfileAsync(context.UserId, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error updating profile for user {UserId}", context.UserId);
                return StatusCode(500, new { error = "Failed to update profile. Please try again." });
            }
        }

        [HttpPost("bulk-profiles")]
        public async Task<IActionResult> Bulk([FromBody] List<Guid> userIds)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator, Roles.Recruiter))
                return Forbid();

            if (userIds == null || userIds.Count == 0)
                return Ok(new List<object>());

            if (userIds.Count > 100)
                return BadRequest(new { error = "Maximum 100 user IDs per request." });

            try
            {
                var profiles = await _service.GetProfilesInBulkAsync(userIds);
                return Ok(profiles);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error during bulk profile fetch");
                return StatusCode(500, new { error = "Failed to retrieve profiles. Please try again." });
            }
        }

        [HttpDelete("{studentId:guid}")]
        public async Task<IActionResult> Delete(Guid studentId)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator))
                return Forbid();

            try
            {
                await _service.DeleteProfileAsync(studentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error deleting profile {StudentId}", studentId);
                return StatusCode(500, new { error = "Failed to delete profile. Please try again." });
            }
        }

        [HttpGet("me/profile-progress")]
        public async Task<IActionResult> GetProfileProgress()
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                var progress = await _service.GetProfileCompletionStatusAsync(context.UserId);
                return Ok(new { profileProgress = progress });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error fetching profile progress for user {UserId}", context.UserId);
                return StatusCode(500, new { error = "Failed to retrieve profile progress. Please try again." });
            }
        }
    }
}