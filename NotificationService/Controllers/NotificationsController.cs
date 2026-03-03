using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Infrastructure.Persistence.Interfaces;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly IDeliveryRepository _deliveryRepository;
        public NotificationsController(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        [HttpGet]
        public IActionResult GetMyNotifications()
        {
            // MVP : mocked user identity
            Guid userId = new Guid();

            var notifications = _deliveryRepository
                .GetInAppNotificationsForUser(userId)
                .Select(n => new NotificationDto
                {
                    Id = n.NotificationId,
                    Title = n.Title,
                    Body = n.Body,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                });
            return Ok(notifications);
        }

        [HttpPost("{id}/read")]
        public IActionResult MarkAsRead(Guid id)
        {
            _deliveryRepository.MarkRead(id);
            return NoContent();
        }
    }
}