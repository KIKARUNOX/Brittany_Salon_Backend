using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brittany_Salon_Backend.Domain.Entities
{
    //[Table("Usuarios")]
    public class User
    {
        //[Key]
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        private int Id { get; set; }

        //[Required(ErrorMessage = "El nombre es obligatorio")]
        //[StringLength (100, ErrorMessage = "El nombre no puede exceder 100 caracteres")] //logitud de 100

        private string name { get; set; }

        private int Phone { get; set; }

        private string email { get; set; }

        private string password { get; set; }
    
        private string image { get; set; }

        private DateTime dateCreated { get; set; } = DateTime.Now;

        private Boolean isActive { get; set; }

        public User(string name, int phone, string email, string password, string image, Boolean isActive)
        {
            this.name = name;
            this.Phone = phone;
            this.email = email;
            this.password = password;
            this.image = image;
            this.isActive = isActive;
        }

        public User() { }
    }
}


