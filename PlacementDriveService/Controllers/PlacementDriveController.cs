using Microsoft.AspNetCore.Mvc;
using PlacementDriveService.DTOs;
using PlacementDriveService.Enums;
using PlacementDriveService.Infrastructure;
using PlacementDriveService.Services.Interfaces;
using Common.Contracts.Web;

namespace PlacementDriveService.Controllers
{
    [ApiController]
    [Route("api/drives")]
    public class PlacementDriveController : ControllerBase
    {
        private readonly IPlacementDriveService _service;
        private readonly RequestContextAccessor _contextAccessor;

        public PlacementDriveController(IPlacementDriveService service, RequestContextAccessor contextAccessor)
        {
            _service = service;
            _contextAccessor = contextAccessor;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DriveCreateRequest dto, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator))
                return Forbid();

            var id = await _service.CreateDriveAsync(dto, context.UserId);
            return Created(string.Empty, ApiEnvelope<object>.Ok("Drive created successfully", new { id }));
        }

      
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DriveUpdateRequest dto, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator))
                return Forbid();

            await _service.UpdateDriveAsync(id, dto);
            return Ok(ApiEnvelope<object>.Ok("Drive updated successfully", new { id }));
        }

       
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator))
                return Forbid();

            await _service.DeleteDriveAsync(id);
            return NoContent();
        }

        
        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            page = page < 1 ? 1 : page;
            pageSize = (pageSize < 1 || pageSize > 100) ? 10 : pageSize;

            var result = await _service.GetOpenDrivesAsync(page, pageSize);
            return Ok(ApiEnvelope<object>.Ok("Drives fetched successfully", result));
        }

        
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Student, Roles.TPO, Roles.Admin, Roles.PlacementCoordinator))
                return Forbid();

            var result = await _service.GetDriveByIdAsync(id);
            return Ok(ApiEnvelope<object>.Ok("Drive fetched successfully", result));
        }

      
        [HttpPost("bulk")]
        public async Task<IActionResult> Bulk([FromBody] List<Guid> driveIds, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Student, Roles.TPO, Roles.Admin, Roles.PlacementCoordinator))
                return Forbid();

            if (driveIds == null || driveIds.Count == 0)
                return Ok(ApiEnvelope<object>.Ok("No drives provided", new List<DriveResponse>()));

            var result = await _service.GetDrivesBulkAsync(driveIds);
            return Ok(ApiEnvelope<object>.Ok("Drives fetched successfully", result));
        }
    }
}