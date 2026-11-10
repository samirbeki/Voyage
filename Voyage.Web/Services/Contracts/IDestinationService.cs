using Voyage.Web.ViewModels.Destinations;

namespace Voyage.Web.Services.Contracts
{
    public interface IDestinationService
    {
        Task<IEnumerable<DestinationAllViewModel>> GetAllAsync();
    }
}