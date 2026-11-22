using System.ComponentModel.DataAnnotations;
using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Expenses
{
    public class ExpenseFormViewModel
    {
        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [StringLength(
            ValidationConstants.ExpenseTitleMaxLength,
            MinimumLength = ValidationConstants.ExpenseTitleMinLength,
            ErrorMessage = ErrorMessages.ExpenseTitleLength)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessages.RequiredField)]
        [Range(
            typeof(decimal),
            ValidationConstants.ExpenseAmountMin,
            ValidationConstants.ExpenseAmountMax,
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