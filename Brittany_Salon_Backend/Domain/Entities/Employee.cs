namespace Brittany_Salon_Backend.Domain.Entities
{
    public class Employee : User
    {
        private string specialty { get; set; }

        public Employee(string specialty)
        {
            this.specialty = specialty;
        }
    }
}
