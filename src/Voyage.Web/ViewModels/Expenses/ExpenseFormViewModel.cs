using System.ComponentModel.DataAnnotations;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Data.Models.Enums;

namespace Voyage.Web.ViewModels.Expenses
{
    public class ExpenseFormViewModel
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ExpenseValidationConstants.TitleMaxLength,
            MinimumLength = ExpenseValidationConstants.TitleMinLength,
            ErrorMessage = ErrorMessages.ExpenseTitleLength)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [Range(
            typeof(decimal),
            ExpenseValidationConstants.AmountMinValue,
            ExpenseValidationConstants.AmountMaxValue,
            ErrorMessage = ErrorMessages.ExpenseAmountRange)]
        [Display(Name = "Amount")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        public ExpenseCategory Category { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public int TripId { get; set; }

        public string TripTitle { get; set; } = string.Empty;

        public DateTime TripStartDate { get; set; }

        public DateTime TripEndDate { get; set; }
    }
}