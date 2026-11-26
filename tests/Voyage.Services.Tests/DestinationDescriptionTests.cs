using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants.Validation;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Destinations;
using Xunit;

namespace Voyage.Services.Tests
{
    public class DestinationDescriptionTests
    {
        [Fact]
        public void DescriptionParagraphs_SplitsTextOnEmptyLines()
        {
            var model = new DestinationDetailsViewModel
            {
                Description = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph."
            };

            Assert.Equal(
                new[] { "First paragraph.", "Second paragraph.", "Third paragraph." },
                model.DescriptionParagraphs);
        }

        [Fact]
        public void DescriptionParagraphs_HandlesWindowsLineEndings()
        {
            var model = new DestinationDetailsViewModel
            {
                Description = "First paragraph.\r\n\r\nSecond paragraph."
            };

            Assert.Equal(new[] { "First paragraph.", "Second paragraph." }, model.DescriptionParagraphs);
        }

        [Fact]
        public void DescriptionParagraphs_SingleParagraph_ReturnsIt()
        {
            var model = new DestinationDetailsViewModel
            {
                Description = "Only one paragraph."
            };

            Assert.Equal("Only one paragraph.", Assert.Single(model.DescriptionParagraphs));
        }

        [Fact]
        public void DescriptionParagraphs_IgnoresWhitespaceOnlyLines()
        {
            var model = new DestinationDetailsViewModel
            {
                Description = "  First paragraph.  \n   \n\n  Second paragraph."
            };

            Assert.Equal(new[] { "First paragraph.", "Second paragraph." }, model.DescriptionParagraphs);
        }

        [Fact]
        public async Task SeededDestinations_HaveThreeParagraphDescriptions()
        {
            using var dbContext = TestDbContextFactory.Create();
            dbContext.Database.EnsureCreated();

            var descriptions = await dbContext.Destinations
                .Select(d => d.Description)
                .ToListAsync();

            Assert.Equal(21, descriptions.Count);
            Assert.All(descriptions, description =>
                Assert.Equal(3, description.Split("\n\n").Length));
        }

        [Fact]
        public async Task SeededDestinations_DescriptionsAreWithinValidationLimits()
        {
            using var dbContext = TestDbContextFactory.Create();
            dbContext.Database.EnsureCreated();

            var descriptions = await dbContext.Destinations
                .Select(d => d.Description)
                .ToListAsync();

            Assert.All(descriptions, description =>
                Assert.InRange(
                    description.Length,
                    DestinationValidationConstants.DescriptionMinLength,
                    DestinationValidationConstants.DescriptionMaxLength));
        }
    }
}
