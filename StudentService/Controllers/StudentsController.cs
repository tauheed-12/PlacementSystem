using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentService.Constants;
using StudentService.DTOs;
using StudentService.Services.Interfaces;
using System.Security.Claims;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _service;

        public StudentsController(IStudentService service)
        {
            _service = service;
        }

        [HttpPost("profile")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Create(CreateStudentProfileDto dto)
        {
            Guid userId = GetUserIdFromToken();
            await _service.CreateProfileAsync(userId, dto);
            return Created("", null);
        }

        [HttpGet("get-profile")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserIdFromToken();
            return Ok(await _service.GetProfileAsync(userId));
        }

        [HttpPatch("update-profile")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Update(UpdateStudentProfileDto dto)
        {
            var userId = GetUserIdFromToken();
            await _service.UpdateProfileAsync(userId, dto);
            return NoContent();
        }

        [HttpPost("bulk-profiles")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.TPO},{Roles.PlacementCoordinator},{Roles.Recruiter}")]
        public async Task<IActionResult> Bulk(BulkStudentProfileRequestDto dto)
        {
            if (dto?.UserIds == null || dto.UserIds.Count == 0)
                return Ok(new List<object>());
            if (dto.UserIds.Count > 100)
                return BadRequest("Maximum 100 user IDs per request.");
            return Ok(await _service.GetProfilesInBulkAsync(dto.UserIds));
        }

        [HttpGet("all-profiles")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.TPO}")]
        public async Task<IActionResult> All()
        {
            return Ok(await _service.GetAllProfilesAsync());
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.TPO},{Roles.PlacementCoordinator}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteProfileAsync(id);
            return NoContent();
        }

        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return Guid.Parse(userIdClaim.Value);
        }
    }
}
