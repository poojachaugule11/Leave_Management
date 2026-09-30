using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Models;

public class LeaveType
{
    public int LeaveTypeId { get; set; }

    [Required, StringLength(50)]
    public string LeaveTypeName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
