using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementDriveService.Constants;
using PlacementDriveService.DTOs;
using PlacementDriveService.Infrastructure;
using PlacementDriveService.Services.Interfaces;

namespace PlacementDriveService.Controllers
{
    [ApiController]
    [Route("api/drives")]
    [Authorize]
    public class PlacementDriveController : ControllerBase
    {
        private readonly IPlacementDriveService _service;
        private readonly RequestContextAccessor _requestContextAccessor;

        public PlacementDriveController(IPlacementDriveService service, RequestContextAccessor requestContextAccessor)
        {
            _service = service;
            _requestContextAccessor = requestContextAccessor;
        }


        [HttpPost]
        public async Task<IActionResult> Create(PlacementDriveCreateDto dto)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator))
                return Forbid();

            var id = await _service.CreateDriveAsync(dto, context.UserId);

            return CreatedAtAction(nameof(GetById), new { id }, null);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, PlacementDriveUpdateDto dto)
        {
            var context = _requestContextAccessor.GetContext();
            if(!context.HasAnyRole(Roles.PlacementCoordinator, Roles.Admin, Roles.Recruiter))
                return Forbid();

            await _service.UpdateDriveAsync(id, dto);
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.PlacementCoordinator, Roles.Admin, Roles.Recruiter))
                return Forbid();

            await _service.DeleteDriveAsync(id);
            return NoContent();
        }


        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 10)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return Ok(await _service.GetOpenDrivesAsync(page, pageSize));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Student, Roles.TPO, Roles.Admin, Roles.PlacementCoordinator))
                return Forbid();

            return Ok(await _service.GetDriveByIdAsync(id));
        }


        [HttpPost("bulk")]
        public async Task<IActionResult> Bulk([FromBody] List<Guid> driveIds)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Student, Roles.TPO, Roles.Admin, Roles.PlacementCoordinator))
                return Forbid();

            if (driveIds == null || driveIds.Count == 0)
                return Ok(new List<PlacementDriveResponseDto>());

            return Ok(await _service.GetDrivesBulkAsync(driveIds));
        }
    }
}
