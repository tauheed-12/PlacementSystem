using ApplicationService.DTO;
using System.Threading.Tasks;

namespace ApplicationService.HttpClients.Interfaces
{
    public interface IPlacementDriveServiceClient
    {
        public Task<Dictionary<Guid, PlacementDriveDetailsDto>> GetDrivesBulkAsync(List<Guid> driveIds, CancellationToken cancellationToken);
    }
}
