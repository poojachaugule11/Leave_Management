using LeaveManagement.Models;

namespace LeaveManagement.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.LeaveTypes.Any())
        {
            db.LeaveTypes.AddRange(
                new LeaveType { LeaveTypeName = "Casual Leave", IsActive = true },
                new LeaveType { LeaveTypeName = "Sick Leave", IsActive = true },
                new LeaveType { LeaveTypeName = "Earned Leave", IsActive = false },
                new LeaveType { LeaveTypeName = "Work From Home", IsActive = false });
        }

        if (!db.Employees.Any())
        {
            db.Employees.AddRange(
                new Employee { EmployeeName = "Aarav Sharma", Department = "Engineering" },
                new Employee { EmployeeName = "Priya Patel", Department = "Human Resources" },
                new Employee { EmployeeName = "Rohan Mehta", Department = "Finance" },
                new Employee { EmployeeName = "Sneha Iyer", Department = "Marketing" },
                new Employee { EmployeeName = "Vikram Singh", Department = "Operations" });
        }

        db.SaveChanges();
    }
}
