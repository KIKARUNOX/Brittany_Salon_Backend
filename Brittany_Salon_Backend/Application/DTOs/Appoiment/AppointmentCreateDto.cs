using System;

namespace Brittany_Salon_Backend.Application.DTOs.Appointment
{
    public class AppointmentCreateDto
    {
        public DateTime AppointmentDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? AppointmentStatus { get; set; }

        public decimal? TotalCost { get; set; }

        public int ClientId { get; set; }
    }
}
