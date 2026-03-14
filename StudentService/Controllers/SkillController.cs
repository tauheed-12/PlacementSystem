using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentService.Helpers;
using StudentService.Services.Interfaces;
using System.Security.Claims;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/students/skills")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _service;

        public SkillController(ISkillService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddSkill(string skillName)
        {
            var userId = GetUserIdFromToken();
            await _service.AddSkillAsync(userId, skillName);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{skillId}")]
        public async Task<IActionResult> RemoveSkill(Guid skillId)
        {
            var userId = GetUserIdFromToken();
            await _service.RemoveSkillAsync(userId, skillId);
            return Ok();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSkills()
        {
            var userId = GetUserIdFromToken();
            return Ok(await _service.GetSkillsAsync(userId));
        }


        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return Guid.Parse(userIdClaim.Value);
        }
    }
}
