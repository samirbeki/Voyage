using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Web.ViewModels.Trips;
using Xunit;

namespace Voyage.Services.Tests
{
    public class TripFormViewModelTests
    {
        [Fact]
        public void Validate_CorrectTrip_ReturnsNoErrors()
        {
            var model = CreateModel(DateTime.Today.AddDays(5), DateTime.Today.AddDays(10));

            Assert.Empty(Validate(model));
        }

        [Fact]
        public void Validate_EndDateBeforeStartDate_ReturnsError()
        {
            var model = CreateModel(DateTime.Today.AddDays(10), DateTime.Today.AddDays(5));

            var error = Assert.Single(Validate(model));
            Assert.Equal(ErrorMessages.EndDateBeforeStartDate, error.ErrorMessage);
            Assert.Contains(nameof(TripFormViewModel.EndDate), error.MemberNames);
        }

        [Fact]
        public void Validate_SameStartAndEndDate_IsAllowed()
        {
            var day = DateTime.Today.AddDays(3);

            Assert.Empty(Validate(CreateModel(day, day)));
        }

        [Fact]
        public void Validate_TripLongerThanTheLimit_ReturnsError()
        {
            var start = DateTime.Today.AddDays(1);
            var model = CreateModel(start, start.AddDays(TripValidationConstants.MaxDays + 1));

            var error = Assert.Single(Validate(model));
            Assert.Equal(
                string.Format(ErrorMessages.TripTooLong, TripValidationConstants.MaxDays),
                error.ErrorMessage);
        }

        [Fact]
        public void Validate_TripWithExactlyTheMaximumLength_IsAllowed()
        {
            var start = DateTime.Today.AddDays(1);

            Assert.Empty(Validate(CreateModel(start, start.AddDays(TripValidationConstants.MaxDays))));
        }

        [Fact]
        public void Validate_NoDestinations_ReturnsError()
        {
            var model = CreateModel(DateTime.Today.AddDays(5), DateTime.Today.AddDays(10));
            model.SelectedDestinationIds.Clear();

            var error = Assert.Single(Validate(model));
            Assert.Equal(ErrorMessages.DestinationsRequired, error.ErrorMessage);
            Assert.Contains(nameof(TripFormViewModel.SelectedDestinationIds), error.MemberNames);
        }

        private static List<ValidationResult> Validate(TripFormViewModel model)
        {
            return model.Validate(new ValidationContext(model)).ToList();
        }

        private static TripFormViewModel CreateModel(DateTime start, DateTime end)
        {
            return new TripFormViewModel
            {
                Title = "Weekend in Bansko",
                StartDate = start,
                EndDate = end,
                Budget = 400m,
                SelectedDestinationIds = new List<int> { 1 }
            };
        }
    }
}
