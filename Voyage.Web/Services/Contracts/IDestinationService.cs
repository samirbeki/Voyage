using Voyage.Web.ViewModels.Destinations;

namespace Voyage.Web.Services.Contracts
{
    public interface IDestinationService
    {
        Task<IEnumerable<DestinationAllViewModel>> GetAllAsync();

        Task<DestinationDetailsViewModel?> GetDetailsAsync(int id);

        Task<IEnumerable<CategoryOptionViewModel>> GetCategoriesAsync();

        Task AddAsync(DestinationFormViewModel model);
    }
}