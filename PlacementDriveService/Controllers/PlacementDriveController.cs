using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementDriveService.Constants;
using PlacementDriveService.DTOs;
using PlacementDriveService.Services.Interfaces;
using PlacementDriveService.NewFolder;

namespace PlacementDriveService.Controllers
{
    [ApiController]
    [Route("api/drives")]
    [Authorize]
    public class PlacementDriveController : ControllerBase
    {
        private readonly IPlacementDriveService _service;

        public PlacementDriveController(IPlacementDriveService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Recruiter},{Roles.PlacementCoordinator}")]
        public async Task<IActionResult> Create(PlacementDriveCreateDto dto)
        {
            Guid userId = User.GetUserId();
            var id = await _service.CreateDriveAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Recruiter},{Roles.PlacementCoordinator}")]
        public async Task<IActionResult> Update(Guid id, PlacementDriveUpdateDto dto)
        {
            await _service.UpdateDriveAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Recruiter},{Roles.PlacementCoordinator}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteDriveAsync(id);
            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return Ok(await _service.GetOpenDrivesAsync(page, pageSize));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetDriveByIdAsync(id));
        }

        [HttpPost("bulk")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Recruiter},{Roles.PlacementCoordinator},{Roles.Student}")]
        public async Task<IActionResult> Bulk([FromBody] List<Guid> driveIds)
        {
            if (driveIds == null || driveIds.Count == 0)
                return Ok(new List<PlacementDriveResponseDto>());
            return Ok(await _service.GetDrivesBulkAsync(driveIds));
        }

        [HttpPost("{id}/apply")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Apply(Guid id)
        {
            var studentId = User.GetUserId();
            await _service.ApplyAsync(id, studentId);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpDelete("{id}/apply")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Withdraw(Guid id)
        {
            var studentId = User.GetUserId();
            await _service.WithdrawAsync(id, studentId);
            return NoContent();
        }
    }
}
