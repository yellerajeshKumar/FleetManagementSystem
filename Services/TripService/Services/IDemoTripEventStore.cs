using TripService.Models;

namespace TripService.Services
{
    public interface IDemoTripEventStore
    {
        Task<DemoTripEvent> AppendAsync(Guid aggregateId, string eventType, object payload, string? sagaId, CancellationToken cancellationToken);
        Task<IReadOnlyList<DemoTripEvent>> GetByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken);
    }
}
