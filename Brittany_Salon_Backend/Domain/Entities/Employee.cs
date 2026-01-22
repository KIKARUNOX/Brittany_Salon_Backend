using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brittany_Salon_Backend.Domain.Entities
{
    [Table("Employee")]
    public class Employee : User
    {
        [MaxLength(100)]
        public string? Specialty { get; set; }

        public Employee() { }
    }
}
