using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementDriveService.Constants;
using PlacementDriveService.Data;
using PlacementDriveService.DTOs;
using PlacementDriveService.Services.Interfaces;
using System.Security.Claims;

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
            var id = await _service.CreateDriveAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, PlacementDriveUpdateDto dto)
        {
            await _service.UpdateDriveAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteDriveAsync(id);
            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 10)
        {
            return Ok(await _service.GetOpenDrivesAsync(page, pageSize));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetDriveByIdAsync(id));
        }

        [HttpPost("{id}/apply")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Apply(Guid id)
        {
            var studentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.ApplyAsync(id, studentId);
            return Created("", null);
        }

        [HttpDelete("{id}/apply")]
        [Authorize(Roles = Roles.Student)]
        public async Task<IActionResult> Withdraw(Guid id)
        {
            var studentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.WithdrawAsync(id, studentId);
            return NoContent();
        }
    }
}
