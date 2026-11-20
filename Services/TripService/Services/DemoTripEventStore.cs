using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TripService.Data;
using TripService.Models;

namespace TripService.Services
{
    public class DemoTripEventStore : IDemoTripEventStore
    {
        private readonly TripDbContext _context;

        public DemoTripEventStore(TripDbContext context)
        {
            _context = context;
        }

        public async Task<DemoTripEvent> AppendAsync(
            Guid aggregateId,
            string eventType,
            object payload,
            string? sagaId,
            CancellationToken cancellationToken)
        {
            var nextVersion = await _context.DemoTripEvents
                .Where(e => e.AggregateId == aggregateId)
                .MaxAsync(e => (int?)e.Version, cancellationToken) ?? 0;

            var evt = new DemoTripEvent
            {
                AggregateId = aggregateId,
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                OccurredAt = DateTime.UtcNow,
                Version = nextVersion + 1,
                SagaId = sagaId
            };

            _context.DemoTripEvents.Add(evt);
            await _context.SaveChangesAsync(cancellationToken);
            return evt;
        }

        public async Task<IReadOnlyList<DemoTripEvent>> GetByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken)
        {
            return await _context.DemoTripEvents
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => e.Version)
                .ToListAsync(cancellationToken);
        }
    }
}
