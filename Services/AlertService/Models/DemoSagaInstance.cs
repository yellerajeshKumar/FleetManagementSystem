using System.ComponentModel.DataAnnotations;

namespace AlertService.Models
{
    public class DemoSagaInstance
    {
        [Key]
        public string SagaId { get; set; } = Guid.NewGuid().ToString("N");

        public Guid DriverId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid? TripId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Started";

        [Required]
        public string CurrentStep { get; set; } = "Initialized";

        [Required]
        [StringLength(200)]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string EndLocation { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
