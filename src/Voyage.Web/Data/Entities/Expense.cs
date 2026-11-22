using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants.Validation;
using Voyage.Web.Common;

namespace Voyage.Web.Data.Entities
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(ExpenseValidationConstants.TitleMaxLength)]
        public string Title { get; set; } = null!;

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public ExpenseCategory Category { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;
    }
}