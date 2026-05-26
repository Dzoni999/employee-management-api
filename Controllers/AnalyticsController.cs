using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("salary")]
        public async Task<IActionResult> GetSalaryAnalytics()
        {
            
            var employees = await _context.Employees.ToListAsync();

            if (!employees.Any())
                return NoContent();

            
            var salaries = employees.Select(e => e.Salary).ToList();

            
            var avg = salaries.Average();

            
            var sortedSalaries = salaries.OrderBy(s => s).ToList();

            decimal median;
            if (sortedSalaries.Count % 2 == 0)
            {
                median = (sortedSalaries[sortedSalaries.Count / 2 - 1]
                         + sortedSalaries[sortedSalaries.Count / 2]) / 2;
            }
            else
            {
                median = sortedSalaries[sortedSalaries.Count / 2];
            }

            
            var topFive = employees
                .OrderByDescending(e => e.Salary)
                .Take(5)
                .ToList();

           
            var byDept = employees
                .GroupBy(e => e.Department ?? "Unknown")
                .Select(g => new
                {
                    Department = g.Key,
                    AverageSalary = g.Average(e => e.Salary)
                })
                .ToList();

            var result = new
            {
                Average = avg,
                Median = median,
                TopFivePaid = topFive,
                ByDepartment = byDept
            };

            
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "SalaryAnalytics",
                Details = "Generated salary analytics report",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }
}