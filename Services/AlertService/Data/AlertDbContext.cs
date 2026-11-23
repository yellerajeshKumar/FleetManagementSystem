using Microsoft.EntityFrameworkCore;
using AlertService.Models;

namespace AlertService.Data
{
    public class AlertDbContext : DbContext
    {
        public AlertDbContext(DbContextOptions<AlertDbContext> options) : base(options)
        {
        }

        public DbSet<NotificationMessage> NotificationMessages { get; set; }
        public DbSet<DemoSagaInstance> DemoSagaInstances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NotificationMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Event).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.ReceivedAt);
                entity.HasIndex(e => e.Source);
            });
            modelBuilder.Entity<DemoSagaInstance>(entity =>
            {
                entity.HasKey(e => e.SagaId);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CurrentStep).IsRequired();
                entity.Property(e => e.StartLocation).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EndLocation).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
