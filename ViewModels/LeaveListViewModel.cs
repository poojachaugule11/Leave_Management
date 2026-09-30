using LeaveManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagement.ViewModels;

public class LeaveListViewModel
{
    public string? Search { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }

    public List<LeaveRequest> Requests { get; set; } = new();
    public List<SelectListItem> LeaveTypes { get; set; } = new();
    public List<SelectListItem> Statuses { get; set; } = new();
}
