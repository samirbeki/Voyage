namespace Voyage.Services.Results
{
    public class PackingBulkResult
    {
        public PackingResult Result { get; init; }

        public int Affected { get; init; }

        public int NotAddedDueToLimit { get; init; }

        public bool Succeeded => Result == PackingResult.Success;

        public static PackingBulkResult Fail(PackingResult result)
            => new() { Result = result };
    }
}
