using System.ComponentModel.DataAnnotations;

namespace TripService.DTOs
{
    public class CreateDemoTripRequest
    {
        [Required]
        public Guid DriverId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        public string EndLocation { get; set; } = string.Empty;

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public string? SagaId { get; set; }
    }

    public class CompleteDemoTripRequest
    {
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
    }

    public class CancelDemoTripRequest
    {
        public string Reason { get; set; } = "Compensation requested";
    }

    public class DemoTripReadResponse
    {
        public Guid TripId { get; set; }
        public Guid DriverId { get; set; }
        public Guid VehicleId { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? SagaId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
