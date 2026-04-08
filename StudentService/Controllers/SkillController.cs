// SkillController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentService.Constants;
using StudentService.Infrastructure;
using StudentService.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace StudentService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/students/skills")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _service;
        private readonly RequestContextAccessor _requestContextAccessor;
        private readonly ILogger<SkillController> _logger;

        public SkillController(
            ISkillService service,
            RequestContextAccessor requestContextAccessor,
            ILogger<SkillController> logger)
        {
            _service = service;
            _requestContextAccessor = requestContextAccessor;
            _logger = logger;
        }

        public class AddSkillRequest
        {
            [Required]
            [MinLength(1)]
            [MaxLength(100)]
            public string SkillName { get; set; } = null!;
        }

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillRequest request)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                await _service.AddSkillAsync(context.UserId, request.SkillName);
                return Ok();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("UQ_") == true
                   || ex.InnerException?.Message.Contains("unique") == true)
            {
                _logger.LogWarning("Duplicate skill '{Skill}' for user {UserId}", request.SkillName, context.UserId);
                return Conflict(new { error = $"Skill '{request.SkillName}' already exists." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error adding skill for user {UserId}", context.UserId);
                return StatusCode(500, new { error = "Failed to save skill. Please try again." });
            }
        }

        [HttpDelete("{skillId:guid}")]
        public async Task<IActionResult> RemoveSkill(Guid skillId)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                await _service.RemoveSkillAsync(context.UserId, skillId);
                return NoContent(); 
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error removing skill {SkillId} for user {UserId}", skillId, context.UserId);
                return StatusCode(500, new { error = "Failed to remove skill. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSkills()
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            try
            {
                var skills = await _service.GetSkillsAsync(context.UserId);
                return Ok(skills);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error fetching skills for user {UserId}", context.UserId);
                return StatusCode(500, new { error = "Failed to retrieve skills. Please try again." });
            }
        }
    }
}