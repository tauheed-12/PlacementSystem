using ApplicationService.Constants;
using ApplicationService.DTO;
using ApplicationService.Infrastructure;
using ApplicationService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyRequestDto dto, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.IsInRole(Roles.Student))
                return Forbid();

            var request = new ApplicationRequestDto
            {
                DriveId = dto.DriveId,
                StudentId = context.UserId
            };

            await _service.ApplyAsync(request, cancellationToken);

            return Created(string.Empty, null);
        }

        [HttpDelete("{applicationId:guid}")]
        public async Task<IActionResult> Delete(Guid applicationId, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.IsInRole(Roles.Student))
                return Forbid();

            await _service.DeleteApplication(applicationId, context.UserId, cancellationToken);

            return NoContent();
        }

        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications(CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.IsInRole(Roles.Student))
                return Forbid();

            var result = await _service.GetUsersApplications(context.UserId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId:guid}")]
        public async Task<IActionResult> GetStudentApplications(Guid studentId, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator, Roles.TPO))
                return Forbid();

            var result = await _service.GetStudentApplication(studentId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("drive/{driveId:guid}/applications")]
        public async Task<IActionResult> GetDriveApplications(Guid driveId, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();

            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator, Roles.TPO))
                return Forbid();

            var result = await _service.GetDriveApplications(driveId, cancellationToken);
            return Ok(result);
        }
    }
}