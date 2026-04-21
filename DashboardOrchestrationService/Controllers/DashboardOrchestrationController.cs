using Common.Contracts.Infrastructure;
using DashboardOrchestrationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrchestrationService.Controllers;

[ApiController]
[Route("api/dashboard/student")]
public class StudentDashboardController : ControllerBase
{
    private readonly IStudentDashboardService _dashboardService;
    private readonly ILogger<StudentDashboardController> _logger;
    private readonly RequestContextAccessor _contextAccessor;

    public StudentDashboardController(
        IStudentDashboardService dashboardService,
        ILogger<StudentDashboardController> logger,
        RequestContextAccessor contextAccessor)
    {
        _dashboardService = dashboardService;
        _logger = logger;
        _contextAccessor = contextAccessor;
    }

    [HttpGet("{studentId:guid}")]
    public async Task<IActionResult> GetDashboard(Guid studentId, CancellationToken ct)
    {
        var context = _contextAccessor.GetContext();

        if (context.HasAnyRole(["Student"]) && context.UserId != studentId)
        {
            _logger.LogWarning("Unauthorized access attempt by {UserId}", context.UserId);
            return Forbid();
        }

        var result = await _dashboardService.GetStudentDashboardAsync(studentId, ct);
        return Ok(result);
    }
}