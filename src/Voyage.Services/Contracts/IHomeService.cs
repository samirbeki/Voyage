using Voyage.Web.ViewModels.Home;

namespace Voyage.Services.Contracts
{
    public interface IHomeService
    {
        Task<HomeIndexViewModel> GetIndexAsync();
    }
}
