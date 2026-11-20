using System.ComponentModel.DataAnnotations;

namespace AlertService.DTOs
{
    public class StartDemoTripSagaRequest
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
    }

    public class ReserveDriverRequest
    {
        public Guid DriverId { get; set; }
        public string SagaId { get; set; } = string.Empty;
    }

    public class ReleaseDriverRequest
    {
        public Guid DriverId { get; set; }
        public string SagaId { get; set; } = string.Empty;
    }

    public class CreateDemoTripRequest
    {
        public Guid DriverId { get; set; }
        public Guid VehicleId { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string? SagaId { get; set; }
    }

    public class CancelDemoTripRequest
    {
        public string Reason { get; set; } = "Compensation requested by saga";
    }
}
