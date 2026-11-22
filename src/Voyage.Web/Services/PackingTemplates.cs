using Voyage.Common.Constants.Validation;
using Voyage.Data.Models.Enums;

namespace Voyage.Web.Services
{
    public record PackingTemplateItem(
        string Name,
        PackingCategory Category,
        int Quantity = 1);

    public static class PackingTemplates
    {
        public static IReadOnlyList<PackingTemplateItem> GetEssentials(
            int tripDays)
        {
            int days = Math.Clamp(
                tripDays,
                PackingItemValidationConstants.QuantityMin,
                PackingItemValidationConstants.QuantityMax);

            int halfDays = Math.Clamp(
                (days + 1) / 2,
                PackingItemValidationConstants.QuantityMin,
                PackingItemValidationConstants.QuantityMax);

            return new List<PackingTemplateItem>
            {
                new("Passport / ID card", PackingCategory.Documents),
                new("Tickets & bookings", PackingCategory.Documents),
                new("Travel insurance", PackingCategory.Documents),

                new("Phone charger", PackingCategory.Electronics),
                new("Power bank", PackingCategory.Electronics),
                new("Travel adapter", PackingCategory.Electronics),

                new("Toothbrush", PackingCategory.Toiletries),
                new("Toothpaste", PackingCategory.Toiletries),
                new("Deodorant", PackingCategory.Toiletries),

                new("Underwear", PackingCategory.Clothing, days),
                new("Socks (pairs)", PackingCategory.Clothing, days),
                new("T-shirts", PackingCategory.Clothing, halfDays),

                new("Personal medications", PackingCategory.Health),
                new("First aid kit", PackingCategory.Health)
            };
        }
    }
}