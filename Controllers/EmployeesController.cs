using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _context.Employees.ToListAsync();
            await LogAudit("FetchEmployees", $"Fetched {employees.Count} employees");
            return employees;
        }
		
		[HttpGet("{id}")]
		public async Task<ActionResult<Employee>> GetEmployee(int id)
		{
			var employee = await _context.Employees.FindAsync(id);

				if (employee == null)
					{
						return NotFound();
					}

				await LogAudit("FetchEmployee", $"Fetched employee with ID {id}");

				return Ok(employee);
		}

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
        {
            var existing = await _context.Employees
                .FirstOrDefaultAsync(e =>
                    e.FirstName == employee.FirstName &&
                    e.LastName == employee.LastName);

            if (existing != null)
            {
                return Conflict("Employee already exists.");
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            await LogAudit(
                "CreateEmployee",
                $"Created employee: {employee.FirstName} {employee.LastName}");

            return CreatedAtAction(
                nameof(GetEmployee),
                new { id = employee.Id },
                employee);
        }

        [HttpPut("{id}/salary")]
        public async Task<IActionResult> UpdateSalary(int id, [FromBody] decimal newSalary)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            decimal oldSalary = employee.Salary;
            employee.Salary = newSalary;
            await _context.SaveChangesAsync();

            await LogAudit("UpdateSalary", $"Employee ID {id} salary changed from {oldSalary} to {newSalary}");
            return Ok(employee);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            await LogAudit(
                "DeleteEmployee",
                $"Deleted employee: {employee.FirstName} {employee.LastName} (ID: {employee.Id})");

            return NoContent();
        }

        private async Task LogAudit(string action, string details)
        {
            _context.AuditLogs.Add(new AuditLog {
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
