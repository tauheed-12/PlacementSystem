using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentService.Constants;
using StudentService.Data;
using StudentService.DTOs;
using StudentService.Entities;
using StudentService.Helpers;
using StudentService.Services.Interfaces;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/students")]
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
            await _service.CreateProfileAsync(User.GetUserId(), dto);
            return Created("", null);
        }

        [HttpGet("get-profile")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetProfileAsync(User.GetUserId()));
        }

        [HttpPatch("update-profile")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Update(UpdateStudentProfileDto dto)
        {
            await _service.UpdateProfileAsync(User.GetUserId(), dto);
            return NoContent();
        }

        [HttpPost("bulk-profiles")]
        public async Task<IActionResult> Bulk(BulkStudentProfileRequestDto dto)
        {
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
    }
}
