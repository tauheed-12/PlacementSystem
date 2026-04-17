using ApplicationService.DTO;
using System.Threading.Tasks;
using static ApplicationService.DTO.Dtos;

namespace ApplicationService.HttpClients.Interfaces
{
    public interface IPlacementDriveServiceClient
    {
        public Task<Dictionary<Guid, PlacementDriveDetails>> GetDrivesBulkAsync(List<Guid> driveIds, CancellationToken cancellationToken);
    }
}
