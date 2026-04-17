using Common.Contracts.Web;
using Microsoft.AspNetCore.Mvc;
using StudentService.Constants;
using Common.Contracts.Infrastructure;
using StudentService.Services.Interfaces;
using FluentValidation;
using static StudentService.DTOs.Dtos;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/students/skills")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _service;
        private readonly RequestContextAccessor _contextAccessor;

        public SkillController(ISkillService service, RequestContextAccessor contextAccessor)
        {
            _service = service;
            _contextAccessor = contextAccessor;
        }

        private async Task<IActionResult?> ValidateAsync<T>(T model, IValidator<T> validator, CancellationToken ct)
        {
            var result = await validator.ValidateAsync(model, ct);
            if (!result.IsValid)
                return BadRequest(ApiEnvelope<object>.Fail(
                    "Validation failed",
                    result.Errors.Select(e => e.ErrorMessage)));

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillRequest dto, [FromServices] IValidator<AddSkillRequest> validator, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student)) return Forbid();

            var validationResult = await ValidateAsync(dto, validator, ct);
            if (validationResult != null) return validationResult;

            await _service.AddSkillAsync(context.UserId, dto, ct);
            return Ok(ApiEnvelope<object>.Ok("Skill added successfully"));
        }

        
        [HttpDelete("{skillId:guid}")]
        public async Task<IActionResult> RemoveSkill(Guid skillId, CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student)) return Forbid();

            await _service.RemoveSkillAsync(context.UserId, skillId, ct);
            return Ok(ApiEnvelope<object>.Ok("Skill removed successfully"));
        }

        
        [HttpGet]
        public async Task<IActionResult> GetSkills(CancellationToken ct)
        {
            var context = _contextAccessor.GetContext();
            if (!context.IsInRole(Roles.Student)) return Forbid();

            var result = await _service.GetSkillsAsync(context.UserId, ct);
            return Ok(ApiEnvelope<object>.Ok("Skills fetched successfully", result));
        }
    }
}