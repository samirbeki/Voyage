using Voyage.Web.ViewModels.Categories;

namespace Voyage.Web.Services.Contracts
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryAllViewModel>> GetAllAsync();

        Task<CategoryAllViewModel?> GetByIdAsync(int id);

        Task<CategoryFormViewModel?> GetForEditAsync(int id);

        Task AddAsync(CategoryFormViewModel model);

        Task<bool> EditAsync(int id, CategoryFormViewModel model);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);

        Task<bool> HasDestinationsAsync(int id);
    }
}