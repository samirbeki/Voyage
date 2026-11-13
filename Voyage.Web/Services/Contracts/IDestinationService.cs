using Voyage.Web.ViewModels.Destinations;

namespace Voyage.Web.Services.Contracts
{
    public interface IDestinationService
    {
        Task<DestinationsIndexViewModel> GetAllAsync(
            string? searchTerm,
            int? categoryId,
            int currentPage);

        Task<DestinationDetailsViewModel?> GetDetailsAsync(int id);

        Task<IEnumerable<CategoryOptionViewModel>> GetCategoriesAsync();

        Task AddAsync(DestinationFormViewModel model);

        Task<DestinationFormViewModel?> GetForEditAsync(int id);

        Task<bool> EditAsync(int id, DestinationFormViewModel model);

        Task<bool> DeleteAsync(int id);

        Task<bool> CategoryExistsAsync(int categoryId);

        Task<bool> DestinationExistsAsync(
            string name,
            string country,
            int? excludeId = null);
    }
}