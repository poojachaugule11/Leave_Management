using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagement.ViewModels;

// Status is deliberately NOT part of this model: the user cannot set it.
public class ApplyLeaveViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Please select an employee.")]
    [Display(Name = "Employee")]
    public int? EmployeeId { get; set; }

    [Required(ErrorMessage = "Please select a leave type.")]
    [Display(Name = "Leave type")]
    public int? LeaveTypeId { get; set; }

    [Required(ErrorMessage = "From date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "From date")]
    public DateTime? FromDate { get; set; }

    [Required(ErrorMessage = "To date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "To date")]
    public DateTime? ToDate { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(500, ErrorMessage = "Reason can be at most 500 characters.")]
    public string? Reason { get; set; }

    [Display(Name = "Total days")]
    public int TotalDays { get; set; }

    public List<SelectListItem> Employees { get; set; } = new();
    public List<SelectListItem> LeaveTypes { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
        {
            yield return new ValidationResult(
                "From date cannot be later than To date.",
                new[] { nameof(FromDate) });
        }
    }
}
