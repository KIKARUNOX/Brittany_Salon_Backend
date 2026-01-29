using Brittany_Salon_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Brittany_Salon_Backend.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;

        public DbSet<Appointment> Appointments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo explícito para Service
            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service");
                entity.HasKey(x => x.ServiceId);

                entity.Property(x => x.ServiceName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.ServiceDescription)
                      .HasMaxLength(255);

                entity.Property(x => x.Price)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(x => x.DurationMinutes)
                      .IsRequired();

                entity.Property(x => x.ImageUrl)
                      .HasMaxLength(255);

                entity.Property(x => x.ServiceType)
                      .HasMaxLength(50);

                entity.Property(x => x.IsActive)
                      .HasDefaultValue(true);
            });

            // Mapeo explícito para Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.Email)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(x => x.Password)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(x => x.Image)
                      .HasMaxLength(255);

                entity.Property(x => x.Specialty)
                      .HasMaxLength(100);

                entity.Property(x => x.IsActive)
                      .HasDefaultValue(true);
            });

            //Mapeo explícito para Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointment");
                entity.HasKey(x => x.AppointmentId);

                entity.Property(x => x.AppointmentDate)
                      .HasColumnType("date")
                      .IsRequired();

                entity.Property(x => x.StartTime)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(x => x.EndTime)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(x => x.AppointmentStatus)
                      .HasMaxLength(50);

                entity.Property(x => x.TotalCost)
                      .HasColumnType("decimal(10,2)");

                entity.Property(x => x.IsActive)
                      .HasDefaultValue(true);

                entity.Property(x => x.ClientId)
                      .IsRequired();

                entity.HasOne(x => x.Client)
                      .WithMany()
                      .HasForeignKey(x => x.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
