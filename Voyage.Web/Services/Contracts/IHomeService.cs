using Voyage.Web.ViewModels.Home;

namespace Voyage.Web.Services.Contracts
{
    public interface IHomeService
    {
        Task<HomeIndexViewModel> GetIndexAsync();
    }
}