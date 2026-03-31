using Microsoft.AspNetCore.Mvc;
using StudentService.Constants;
using StudentService.DTOs;
using StudentService.Infrastructure;
using StudentService.Services.Interfaces;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/student")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _service;
        private readonly RequestContextAccessor _requestContextAccessor;

        public StudentsController(
            IStudentService service,
            RequestContextAccessor requestContextAccessor)
        {
            _service = service;
            _requestContextAccessor = requestContextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentProfileDto dto)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            await _service.CreateProfileAsync(context.UserId, dto);
            return Created(string.Empty, null);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            var profile = await _service.GetProfileAsync(context.UserId);
            return Ok(profile);
        }

        [HttpGet("{studentId:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator, Roles.Recruiter))
                return Forbid();

            var profile = await _service.GetProfileByIdAsync(id);
            return Ok(profile);
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UpdateStudentProfileDto dto)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            await _service.UpdateProfileAsync(context.UserId, dto);
            return NoContent();
        }

        [HttpPost("bulk-profiles")]
        public async Task<IActionResult> Bulk([FromBody] BulkStudentProfileRequestDto dto)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator, Roles.Recruiter))
                return Forbid();

            if (dto?.UserIds == null || dto.UserIds.Count == 0)
                return Ok(new List<object>());

            if (dto.UserIds.Count > 100)
                return BadRequest(new { error = "Maximum 100 user IDs per request." });

            var profiles = await _service.GetProfilesInBulkAsync(dto.UserIds);
            return Ok(profiles);
        }

        [HttpGet("all-profiles")]
        public async Task<IActionResult> All()
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO))
                return Forbid();

            var profiles = await _service.GetAllProfilesAsync();
            return Ok(profiles);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.TPO, Roles.PlacementCoordinator))
                return Forbid();

            await _service.DeleteProfileAsync(id);
            return NoContent();
        }

        [HttpGet("profile-progress")]
        public async Task<IActionResult> GetProfileProgress()
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();
            var progress = await _service.GetProfileCompletionStatusAsync(context.UserId);
            return Ok(new { profileProgress = progress });
        }
    }
}