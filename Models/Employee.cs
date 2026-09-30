using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Models;

public class Employee
{
    public int EmployeeId { get; set; }

    [Required, StringLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Department { get; set; }
}
