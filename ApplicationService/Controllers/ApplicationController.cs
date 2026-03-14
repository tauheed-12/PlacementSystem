using ApplicationService.Constants;
using ApplicationService.DTO;
using ApplicationService.Helpers;
using ApplicationService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationService.Controllers
{
    [ApiController]
    [Route("api/application")]
    [Authorize]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _service;

        public ApplicationController(IApplicationService service)
        {
            _service = service;
        }

        [HttpPost("apply")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Apply([FromBody] ApplyRequestDto dto, CancellationToken cancellationToken)
        {
            var studentId = User.GetUserId();
            var request = new ApplicationRequestDto { DriveId = dto.DriveId, StudentId = studentId };
            await _service.ApplyAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetMyApplications), null, null);
        }

        [HttpDelete("{applicationId:guid}")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Delete(Guid applicationId, CancellationToken cancellationToken)
        {
            var studentId = User.GetUserId();
            await _service.DeleteApplication(applicationId, studentId, cancellationToken);
            return NoContent();
        }

        [HttpGet("my-applications")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> GetMyApplications(CancellationToken cancellationToken)
        {
            var studentId = User.GetUserId();
            return Ok(await _service.GetUsersApplications(studentId, cancellationToken));
        }

        [HttpGet("drive/{driveId:guid}/applications")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Recruiter},{Roles.PlacementCoordinator},{Roles.TPO}")]
        public async Task<IActionResult> GetDriveApplications(Guid driveId, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetDriveApplications(driveId, cancellationToken));
        }
    }
}
