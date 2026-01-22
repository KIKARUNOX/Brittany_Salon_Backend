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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo explícito (por si acaso)
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
        }
    }
}
