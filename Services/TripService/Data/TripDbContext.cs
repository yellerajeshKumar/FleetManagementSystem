using Microsoft.EntityFrameworkCore;
using TripService.Models;

namespace TripService.Data
{
    public class TripDbContext : DbContext
    {
        public TripDbContext(DbContextOptions<TripDbContext> options) : base(options)
        {
        }

        public DbSet<Trip> Trips { get; set; }
         public DbSet<DemoTripEvent> DemoTripEvents { get; set; }
        public DbSet<DemoTripReadModel> DemoTripReadModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(t => t.TripID);
                entity.HasIndex(t => t.VehicleID);
                entity.HasIndex(t => t.DriverID);
                entity.HasIndex(t => t.Status);
                entity.Property(t => t.Status).HasDefaultValue("InProgress");
            });
              modelBuilder.Entity<DemoTripEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AggregateId);
                entity.HasIndex(e => e.OccurredAt);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.SagaId).HasMaxLength(80);
            });

            modelBuilder.Entity<DemoTripReadModel>(entity =>
            {
                entity.HasKey(r => r.TripId);
                entity.HasIndex(r => r.DriverId);
                entity.HasIndex(r => r.Status);
                entity.Property(r => r.Status).HasMaxLength(50).IsRequired();
                entity.Property(r => r.StartLocation).HasMaxLength(200).IsRequired();
                entity.Property(r => r.EndLocation).HasMaxLength(200).IsRequired();
                entity.Property(r => r.SagaId).HasMaxLength(80);
            });
        
        }
    }
}
