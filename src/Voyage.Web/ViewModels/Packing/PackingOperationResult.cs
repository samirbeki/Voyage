using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Packing
{
    public class PackingOperationResult
    {
        public PackingResult Result { get; init; }

        public int TripId { get; init; }

        public bool IsPacked { get; init; }

        public bool Succeeded => Result == PackingResult.Success;

        public static PackingOperationResult Fail(PackingResult result)
            => new() { Result = result };
    }
}