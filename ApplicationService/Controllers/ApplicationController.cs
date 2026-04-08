using ApplicationService.Infrastructure;
using ApplicationService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ApplicationService.DTO.Dtos;
using static ApplicationService.Enums.Enums;

namespace ApplicationService.Controllers
{
    [ApiController]
    [Route("api/application")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _service;
        private readonly RequestContextAccessor _contextAccessor;

        public ApplicationController(
            IApplicationService service,
            RequestContextAccessor contextAccessor)
        {
            _service = service;
            _contextAccessor = contextAccessor;
        }

        // ---------------- APPLY ----------------

        [HttpPost("apply")]
        public async Task<IActionResult> Apply(ApplyRequest dto, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                var request = new CreateApplicationRequest(
                    dto.DriveId,
                    context.UserId
                );

                await _service.ApplyAsync(request, cancellationToken);

                return Created(string.Empty, null);
            }
            catch (DbUpdateException ex)
            {
                return HandleDbException(ex);
            }
        }

        // ---------------- DELETE ----------------

        [HttpDelete("{applicationId:guid}")]
        public async Task<IActionResult> Delete(Guid applicationId, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                await _service.DeleteApplicationAsync(
                    applicationId,
                    context.UserId,
                    cancellationToken);

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return HandleDbException(ex);
            }
        }

        // ---------------- MY APPLICATIONS ----------------

        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications(CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.IsInRole(Roles.Student))
                return Forbid();

            var result = await _service.GetUserApplicationsAsync(
                context.UserId,
                cancellationToken);

            return Ok(result);
        }

        // ---------------- STUDENT APPLICATIONS ----------------

        [HttpGet("student/{studentId:guid}")]
        public async Task<IActionResult> GetStudentApplications(
            Guid studentId,
            CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator, Roles.TPO))
                return Forbid();

            var result = await _service.GetStudentApplicationsAsync(
                studentId,
                cancellationToken);

            return Ok(result);
        }

        // ---------------- DRIVE APPLICATIONS ----------------

        [HttpGet("drive/{driveId:guid}/applications")]
        public async Task<IActionResult> GetDriveApplications(
            Guid driveId,
            CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator, Roles.TPO))
                return Forbid();

            var result = await _service.GetDriveApplicationsAsync(
                driveId,
                cancellationToken);

            return Ok(result);
        }

        // ---------------- DB EXCEPTION HANDLER ----------------

        private IActionResult HandleDbException(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;

            // UNIQUE constraint violation (duplicate application)
            if (message.Contains("IX_Applications_StudentUserId_DriveId") ||
                message.Contains("duplicate"))
            {
                return Conflict("You have already applied to this drive");
            }

            // CHECK constraint (status etc.)
            if (message.Contains("CK_Application_Status_Valid"))
            {
                return BadRequest("Invalid application status");
            }

            // fallback
            return StatusCode(500, "A database error occurred");
        }
    }
}