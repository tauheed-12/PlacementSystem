using Common.Contracts.Infrastructure;
using ApplicationService.Services.Interfaces;
using ApplicationService.Enums;
using Common.Contracts.Web;
using Microsoft.AspNetCore.Mvc;
using static ApplicationService.DTO.Dtos;

namespace ApplicationService.Controllers
{
    [ApiController]
    [Route("api/application")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _service;
        private readonly RequestContextAccessor _contextAccessor;

        public ApplicationController(IApplicationService service, RequestContextAccessor contextAccessor)
        {
            _service = service;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyRequest dto, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();
            var request = new CreateApplicationRequest(dto.DriveId, context.UserId);
            await _service.ApplyAsync(request, cancellationToken);
            return Created(string.Empty, ApiEnvelope<object>.Ok("Applied successfully", new { driveId = dto.DriveId }));
        }

        [HttpDelete("{applicationId:guid}")]
        public async Task<IActionResult> Delete(Guid applicationId, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();
            await _service.DeleteApplicationAsync(applicationId, context.UserId, cancellationToken);
            return NoContent();
        }

        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications(CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();
            var result = await _service.GetUserApplicationsAsync(context.UserId, cancellationToken);
            return Ok(ApiEnvelope<object>.Ok("Applications fetched successfully", result));
        }

        [HttpGet("student/{studentId:guid}")]
        public async Task<IActionResult> GetStudentApplications(Guid studentId, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator, Roles.TPO))
                return Forbid();
            var result = await _service.GetStudentApplicationsAsync(studentId, cancellationToken);
            return Ok(ApiEnvelope<object>.Ok("Student applications fetched successfully", result));
        }

        [HttpGet("drive/{driveId:guid}/applications")]
        public async Task<IActionResult> GetDriveApplications(Guid driveId, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.GetContext();
            if (!context.HasAnyRole(Roles.Admin, Roles.Recruiter, Roles.PlacementCoordinator, Roles.TPO))
                return Forbid();
            var result = await _service.GetDriveApplicationsAsync(driveId, cancellationToken);
            return Ok(ApiEnvelope<object>.Ok("Drive applications fetched successfully", result));
        }
    }
}