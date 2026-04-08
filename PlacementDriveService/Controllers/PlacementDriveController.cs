using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public PlacementDriveController(
            IPlacementDriveService service,
            RequestContextAccessor requestContextAccessor)
        {
            _service = service;
            _requestContextAccessor = requestContextAccessor;
        }

        // ---------------- CREATE ----------------

        [HttpPost]
        public async Task<IActionResult> Create(DriveCreateRequest request)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator))
                return Forbid();

            try
            {
                var id = await _service.CreateDriveAsync(request, context.UserId);
                return CreatedAtAction(nameof(GetById), new { id }, null);
            }
            catch (DbUpdateException ex)
            {
                return HandleDbException(ex);
            }
        }

        // ---------------- UPDATE ----------------

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, DriveUpdateRequest request)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.PlacementCoordinator, Roles.Admin, Roles.Recruiter))
                return Forbid();

            try
            {
                await _service.UpdateDriveAsync(id, request);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return HandleDbException(ex);
            }
        }

        // ---------------- DELETE ----------------

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.PlacementCoordinator, Roles.Admin, Roles.Recruiter))
                return Forbid();

            try
            {
                await _service.DeleteDriveAsync(id);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return HandleDbException(ex);
            }
        }

        // ---------------- GET ----------------

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

        // ---------------- GET BY ID ----------------

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Student, Roles.TPO, Roles.Admin, Roles.PlacementCoordinator))
                return Forbid();

            return Ok(await _service.GetDriveByIdAsync(id));
        }

        // ---------------- BULK ----------------

        [HttpPost("bulk")]
        public async Task<IActionResult> Bulk([FromBody] List<Guid> driveIds)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Student, Roles.TPO, Roles.Admin, Roles.PlacementCoordinator))
                return Forbid();

            if (driveIds == null || driveIds.Count == 0)
                return Ok(new List<DriveResponse>());

            return Ok(await _service.GetDrivesBulkAsync(driveIds));
        }

        // ---------------- DB EXCEPTION HANDLER ----------------

        private IActionResult HandleDbException(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;

            if (message.Contains("CK_PlacementDrive_Package_Positive"))
                return BadRequest("Package must be greater than 0");

            if (message.Contains("CK_PlacementDrive_Deadline_Before_DriveDate"))
                return BadRequest("Application deadline must be before drive date");

            if (message.Contains("CK_PlacementDrive_DriveDate_Future"))
                return BadRequest("Drive date must be in the future");

            if (message.Contains("Cannot insert"))
                return BadRequest("Invalid data provided");

            return StatusCode(500, "A database error occurred");
        }
    }
}