using System.ComponentModel.DataAnnotations;

namespace TripService.Models
{
    public class DemoTripReadModel
    {
        [Key]
        public Guid TripId { get; set; }

        public Guid VehicleId { get; set; }
        public Guid DriverId { get; set; }

        [Required]
        [StringLength(200)]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string EndLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Started";

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [StringLength(80)]
        public string? SagaId { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
