namespace Voyage.Web.ViewModels.Shared
{
    public class EmptyStateViewModel
    {
        public string Icon { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string? ActionText { get; set; }

        public string? ActionController { get; set; }

        public string? ActionName { get; set; }
    }
}