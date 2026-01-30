using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brittany_Salon_Backend.Domain.Entities
{
    [Table("Client")]
    public class Clients
    {
        [Key]
        [Column("clientId")]
        public int ClientId { get; set; }
    }
}
