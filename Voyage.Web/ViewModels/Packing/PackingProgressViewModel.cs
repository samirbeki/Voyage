namespace Voyage.Web.ViewModels.Packing
{
    public class PackingProgressViewModel
    {
        public int Total { get; set; }

        public int Packed { get; set; }

        public int Percent => Total == 0
            ? 0
            : (int)Math.Round((double)Packed / Total * 100);

        public bool IsComplete => Total > 0 && Packed == Total;
    }
}