using LeaveManagement.Data;
using LeaveManagement.Models;
using LeaveManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Controllers;

public class LeaveController : Controller
{
    private readonly AppDbContext _db;

    public LeaveController(AppDbContext db) => _db = db;

    // ---------- Leave list ----------
    public async Task<IActionResult> Index(string? search, int? leaveTypeId, string? status)
    {
        var query = _db.LeaveRequests
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.LeaveType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r => r.Employee!.EmployeeName.Contains(term));
        }

        if (leaveTypeId.HasValue)
            query = query.Where(r => r.LeaveTypeId == leaveTypeId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        var model = new LeaveListViewModel
        {
            Search = search,
            LeaveTypeId = leaveTypeId,
            Status = status,
            Requests = await query
                .OrderByDescending(r => r.CreatedDate)
                .ThenByDescending(r => r.LeaveRequestId)
                .ToListAsync(),
            // The filter lists every type so old requests stay filterable.
            LeaveTypes = await _db.LeaveTypes
                .AsNoTracking()
                .OrderBy(t => t.LeaveTypeName)
                .Select(t => new SelectListItem(t.LeaveTypeName, t.LeaveTypeId.ToString()))
                .ToListAsync(),
            Statuses = LeaveStatus.All.Select(s => new SelectListItem(s, s)).ToList()
        };

        return View(model);
    }

    // ---------- Apply for leave ----------
    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        var model = new ApplyLeaveViewModel();
        await LoadDropdownsAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyLeaveViewModel model)
    {
        // Only active leave types may be used.
        if (model.LeaveTypeId.HasValue &&
            !await _db.LeaveTypes.AnyAsync(t => t.LeaveTypeId == model.LeaveTypeId && t.IsActive))
        {
            ModelState.AddModelError(nameof(model.LeaveTypeId), "Selected leave type is not available.");
        }

        if (model.EmployeeId.HasValue &&
            !await _db.Employees.AnyAsync(e => e.EmployeeId == model.EmployeeId))
        {
            ModelState.AddModelError(nameof(model.EmployeeId), "Selected employee does not exist.");
        }

        if (!ModelState.IsValid)
        {
            RecalculateDays(model);
            await LoadDropdownsAsync(model);
            return View(model);
        }

        var from = model.FromDate!.Value.Date;
        var to = model.ToDate!.Value.Date;

        var request = new LeaveRequest
        {
            EmployeeId = model.EmployeeId!.Value,
            LeaveTypeId = model.LeaveTypeId!.Value,
            FromDate = from,
            ToDate = to,
            NumberOfDays = CalculateDays(from, to),   // always computed on the server
            Reason = model.Reason!.Trim(),
            Status = LeaveStatus.Pending,             // always Pending on creation
            CreatedDate = DateTime.Now
        };

        _db.LeaveRequests.Add(request);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Leave request submitted and is now pending.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Approve / Reject ----------
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id) => ChangeStatusAsync(id, LeaveStatus.Approved);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id) => ChangeStatusAsync(id, LeaveStatus.Rejected);

    private async Task<IActionResult> ChangeStatusAsync(int id, string newStatus)
    {
        var request = await _db.LeaveRequests.FindAsync(id);

        if (request == null)
        {
            TempData["Error"] = "Leave request not found.";
        }
        else if (request.Status != LeaveStatus.Pending)
        {
            TempData["Error"] = $"Only pending requests can be changed. This request is already {request.Status.ToLower()}.";
        }
        else
        {
            request.Status = newStatus;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Leave request {newStatus.ToLower()}.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- Helpers ----------
    private static int CalculateDays(DateTime from, DateTime to) => (to.Date - from.Date).Days + 1;

    private static void RecalculateDays(ApplyLeaveViewModel model)
    {
        model.TotalDays = model.FromDate.HasValue && model.ToDate.HasValue &&
                          model.FromDate.Value.Date <= model.ToDate.Value.Date
            ? CalculateDays(model.FromDate.Value, model.ToDate.Value)
            : 0;
    }

    private async Task LoadDropdownsAsync(ApplyLeaveViewModel model)
    {
        model.Employees = await _db.Employees
            .AsNoTracking()
            .OrderBy(e => e.EmployeeName)
            .Select(e => new SelectListItem(e.EmployeeName, e.EmployeeId.ToString()))
            .ToListAsync();

        model.LeaveTypes = await _db.LeaveTypes
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.LeaveTypeName)
            .Select(t => new SelectListItem(t.LeaveTypeName, t.LeaveTypeId.ToString()))
            .ToListAsync();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
