using Microsoft.AspNetCore.Mvc;
using StudentService.Constants;
using StudentService.Infrastructure;
using StudentService.Services.Interfaces;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/students/skills")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _service;
        private readonly RequestContextAccessor _requestContextAccessor;

        public SkillController(
            ISkillService service,
            RequestContextAccessor requestContextAccessor)
        {
            _service = service;
            _requestContextAccessor = requestContextAccessor;
        }

        public class AddSkillRequest
        {
            public string SkillName { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillRequest request)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            await _service.AddSkillAsync(context.UserId, request.SkillName);
            return Ok();
        }

        [HttpDelete("{skillId}")]
        public async Task<IActionResult> RemoveSkill(Guid skillId)
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            await _service.RemoveSkillAsync(context.UserId, skillId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetSkills()
        {
            var context = _requestContextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student))
                return Forbid();

            var skills = await _service.GetSkillsAsync(context.UserId);
            return Ok(skills);
        }
    }
}