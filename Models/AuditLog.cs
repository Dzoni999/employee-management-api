using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
