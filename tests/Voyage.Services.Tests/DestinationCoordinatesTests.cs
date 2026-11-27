using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Data;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Destinations;
using Xunit;

namespace Voyage.Services.Tests
{
    public class DestinationCoordinatesTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;

        public DestinationCoordinatesTests()
        {
            dbContext = TestDbContextFactory.Create();
            dbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task SeededDestinations_AllHaveCoordinatesInRange()
        {
            var destinations = await dbContext.Destinations.ToListAsync();

            Assert.Equal(21, destinations.Count);
            Assert.All(destinations, d =>
            {
                Assert.NotNull(d.Latitude);
                Assert.NotNull(d.Longitude);
                Assert.InRange(d.Latitude!.Value, DestinationValidationConstants.LatitudeMin, DestinationValidationConstants.LatitudeMax);
                Assert.InRange(d.Longitude!.Value, DestinationValidationConstants.LongitudeMin, DestinationValidationConstants.LongitudeMax);
            });
        }

        [Fact]
        public async Task SeededDestinations_HaveDifferentCoordinates()
        {
            var points = await dbContext.Destinations
                .Select(d => new { d.Latitude, d.Longitude })
                .ToListAsync();

            Assert.Equal(points.Count, points.Distinct().Count());
        }

        [Fact]
        public void Validate_BothCoordinates_ReturnsNoErrors()
        {
            var model = CreateModel(38.7223, -9.1393);

            Assert.Empty(ValidateAll(model));
        }

        [Fact]
        public void Validate_NoCoordinates_ReturnsNoErrors()
        {
            var model = CreateModel(null, null);

            Assert.Empty(ValidateAll(model));
        }

        [Fact]
        public void Validate_OnlyLatitude_ReturnsErrorForLongitude()
        {
            var model = CreateModel(38.7223, null);

            var error = Assert.Single(ValidateAll(model));
            Assert.Equal(ErrorMessages.CoordinatesIncomplete, error.ErrorMessage);
            Assert.Contains(nameof(DestinationFormViewModel.Longitude), error.MemberNames);
        }

        [Fact]
        public void Validate_OnlyLongitude_ReturnsErrorForLatitude()
        {
            var model = CreateModel(null, -9.1393);

            var error = Assert.Single(ValidateAll(model));
            Assert.Equal(ErrorMessages.CoordinatesIncomplete, error.ErrorMessage);
            Assert.Contains(nameof(DestinationFormViewModel.Latitude), error.MemberNames);
        }

        [Theory]
        [InlineData(-90.0001)]
        [InlineData(90.5)]
        public void Validate_LatitudeOutOfRange_ReturnsRangeError(double latitude)
        {
            var model = CreateModel(latitude, 10);

            var error = Assert.Single(ValidateAll(model));
            Assert.Equal(
                string.Format(
                    ErrorMessages.CoordinateRange,
                    nameof(DestinationFormViewModel.Latitude),
                    DestinationValidationConstants.LatitudeMin,
                    DestinationValidationConstants.LatitudeMax),
                error.ErrorMessage);
        }

        [Theory]
        [InlineData(-180.5)]
        [InlineData(181)]
        public void Validate_LongitudeOutOfRange_ReturnsRangeError(double longitude)
        {
            var model = CreateModel(10, longitude);

            var error = Assert.Single(ValidateAll(model));
            Assert.Equal(
                string.Format(
                    ErrorMessages.CoordinateRange,
                    nameof(DestinationFormViewModel.Longitude),
                    DestinationValidationConstants.LongitudeMin,
                    DestinationValidationConstants.LongitudeMax),
                error.ErrorMessage);
        }

        [Theory]
        [InlineData(-90, -180)]
        [InlineData(90, 180)]
        [InlineData(0, 0)]
        public void Validate_BoundaryCoordinates_AreAllowed(double latitude, double longitude)
        {
            Assert.Empty(ValidateAll(CreateModel(latitude, longitude)));
        }

        private static List<ValidationResult> ValidateAll(DestinationFormViewModel model)
        {
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            return results;
        }

        private static DestinationFormViewModel CreateModel(double? latitude, double? longitude)
        {
            return new DestinationFormViewModel
            {
                Name = "Lisbon",
                Country = "Portugal",
                Description = "A sunny capital on seven hills.",
                CategoryId = 3,
                Latitude = latitude,
                Longitude = longitude
            };
        }
    }
}
