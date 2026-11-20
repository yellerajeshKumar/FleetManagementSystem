using System.ComponentModel.DataAnnotations;

namespace TripService.Models
{
    public class DemoTripEvent
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid AggregateId { get; set; }

        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public string Payload { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public int Version { get; set; }

        [StringLength(80)]
        public string? SagaId { get; set; }
    }
}
