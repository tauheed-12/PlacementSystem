// Controllers/StudentDashboardController.cs
using DashboardOrchestrationService.DTOs;
using DashboardOrchestrationService.Infrastructure;
using DashboardOrchestrationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrchestrationService.Controllers
{
    [ApiController]
    [Route("api/dashboard/student")]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IStudentDashboardService _dashboardService;
        private readonly ILogger<StudentDashboardController> _logger;
        private readonly RequestContextAccessor _requestContextAccessor;

        public StudentDashboardController(
            IStudentDashboardService dashboardService,
            ILogger<StudentDashboardController> logger,
            RequestContextAccessor requestContextAccessor)
        {
            _dashboardService = dashboardService;
            _logger = logger;
            _requestContextAccessor = requestContextAccessor;
        }

        [HttpGet("{studentId:guid}")]
        public async Task<IActionResult> GetDashboard(Guid studentId)
        {
            var context = _requestContextAccessor.GetContext();

            if (context.HasAnyRole(["Student"]) && context.UserId != studentId)
            {
                _logger.LogWarning("Unauthorized access attempt by user {UserId} for student {StudentId}", context.UserId, studentId);
                return Forbid();
            }
            var dashboard = await _dashboardService.GetStudentDashboardAsync(studentId);
            return Ok(dashboard);
        }
    }
}