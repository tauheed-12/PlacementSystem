using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence.Interfaces;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/preferences")]
    public sealed class PreferencesController : ControllerBase
    {
        private readonly IUserPreferenceRepository _userPreferenceRepository;
        public PreferencesController(IUserPreferenceRepository userPreferenceRepository)
        {
            _userPreferenceRepository = userPreferenceRepository;
        }

        [HttpGet]
        public IActionResult Get() 
        {
            // MVP: mocked user identity
            Guid userId = new Guid();
            var prefs = _userPreferenceRepository.Get(userId);

            return Ok(new PreferenceDto
            {
                InAppEnabled = prefs.InAppEnabled,
                EmailEnabled = prefs.EmailEnabled,
                EmailAddress = prefs.EmailAddress
            });
        }

        [HttpPut]
        public IActionResult Update(PreferenceDto dto) 
        {
            Guid userId = new Guid();
            var updated = new UserPreferences(
                dto.InAppEnabled,
                dto.EmailEnabled,
                dto.EmailAddress
            );

            _userPreferenceRepository.Save(userId, updated);
            return NoContent();
        }
    }
}
