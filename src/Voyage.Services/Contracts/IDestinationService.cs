using Voyage.Web.ViewModels.Destinations;
using Voyage.Web.ViewModels.Enums;

namespace Voyage.Services.Contracts
{
    public interface IDestinationService
    {
        Task<DestinationsIndexViewModel> GetAllAsync(
            string? searchTerm,
            int? categoryId,
            int currentPage,
            DestinationSorting sorting);

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
