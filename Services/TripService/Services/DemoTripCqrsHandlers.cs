using Microsoft.EntityFrameworkCore;
using TripService.Data;
using TripService.DTOs;
using TripService.Models;

namespace TripService.Services
{
    public record CreateDemoTripCommand(
        Guid DriverId,
        Guid VehicleId,
        string StartLocation,
        string EndLocation,
        DateTime StartTime,
        string? SagaId);

    public record CompleteDemoTripCommand(Guid TripId, DateTime EndTime);

    public record CancelDemoTripCommand(Guid TripId, string Reason);

    public record GetDemoTripByIdQuery(Guid TripId);

    public class CreateDemoTripCommandHandler
    {
        private readonly TripDbContext _context;
        private readonly IDemoTripEventStore _eventStore;

        public CreateDemoTripCommandHandler(TripDbContext context, IDemoTripEventStore eventStore)
        {
            _context = context;
            _eventStore = eventStore;
        }

        public async Task<DemoTripReadResponse> HandleAsync(CreateDemoTripCommand command, CancellationToken cancellationToken)
        {
            var tripId = Guid.NewGuid();
            var readModel = new DemoTripReadModel
            {
                TripId = tripId,
                DriverId = command.DriverId,
                VehicleId = command.VehicleId,
                StartLocation = command.StartLocation,
                EndLocation = command.EndLocation,
                StartTime = command.StartTime,
                Status = "Started",
                SagaId = command.SagaId,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.DemoTripReadModels.Add(readModel);
            await _context.SaveChangesAsync(cancellationToken);

            await _eventStore.AppendAsync(
                tripId,
                "DemoTripStarted",
                new
                {
                    TripId = tripId,
                    command.DriverId,
                    command.VehicleId,
                    command.StartLocation,
                    command.EndLocation,
                    command.StartTime,
                    command.SagaId
                },
                command.SagaId,
                cancellationToken);

            return ToResponse(readModel);
        }

        private static DemoTripReadResponse ToResponse(DemoTripReadModel model)
        {
            return new DemoTripReadResponse
            {
                TripId = model.TripId,
                DriverId = model.DriverId,
                VehicleId = model.VehicleId,
                StartLocation = model.StartLocation,
                EndLocation = model.EndLocation,
                Status = model.Status,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                SagaId = model.SagaId,
                LastUpdatedAt = model.LastUpdatedAt
            };
        }
    }

    public class CompleteDemoTripCommandHandler
    {
        private readonly TripDbContext _context;
        private readonly IDemoTripEventStore _eventStore;

        public CompleteDemoTripCommandHandler(TripDbContext context, IDemoTripEventStore eventStore)
        {
            _context = context;
            _eventStore = eventStore;
        }

        public async Task<DemoTripReadResponse?> HandleAsync(CompleteDemoTripCommand command, CancellationToken cancellationToken)
        {
            var readModel = await _context.DemoTripReadModels
                .FirstOrDefaultAsync(t => t.TripId == command.TripId, cancellationToken);

            if (readModel == null)
            {
                return null;
            }

            readModel.Status = "Completed";
            readModel.EndTime = command.EndTime;
            readModel.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _eventStore.AppendAsync(
                readModel.TripId,
                "DemoTripCompleted",
                new
                {
                    readModel.TripId,
                    command.EndTime,
                    readModel.SagaId
                },
                readModel.SagaId,
                cancellationToken);

            return new DemoTripReadResponse
            {
                TripId = readModel.TripId,
                DriverId = readModel.DriverId,
                VehicleId = readModel.VehicleId,
                StartLocation = readModel.StartLocation,
                EndLocation = readModel.EndLocation,
                Status = readModel.Status,
                StartTime = readModel.StartTime,
                EndTime = readModel.EndTime,
                SagaId = readModel.SagaId,
                LastUpdatedAt = readModel.LastUpdatedAt
            };
        }
    }

    public class CancelDemoTripCommandHandler
    {
        private readonly TripDbContext _context;
        private readonly IDemoTripEventStore _eventStore;

        public CancelDemoTripCommandHandler(TripDbContext context, IDemoTripEventStore eventStore)
        {
            _context = context;
            _eventStore = eventStore;
        }

        public async Task<DemoTripReadResponse?> HandleAsync(CancelDemoTripCommand command, CancellationToken cancellationToken)
        {
            var readModel = await _context.DemoTripReadModels
                .FirstOrDefaultAsync(t => t.TripId == command.TripId, cancellationToken);

            if (readModel == null)
            {
                return null;
            }

            readModel.Status = "Cancelled";
            readModel.EndTime = DateTime.UtcNow;
            readModel.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _eventStore.AppendAsync(
                readModel.TripId,
                "DemoTripCancelled",
                new
                {
                    readModel.TripId,
                    command.Reason,
                    readModel.SagaId
                },
                readModel.SagaId,
                cancellationToken);

            return new DemoTripReadResponse
            {
                TripId = readModel.TripId,
                DriverId = readModel.DriverId,
                VehicleId = readModel.VehicleId,
                StartLocation = readModel.StartLocation,
                EndLocation = readModel.EndLocation,
                Status = readModel.Status,
                StartTime = readModel.StartTime,
                EndTime = readModel.EndTime,
                SagaId = readModel.SagaId,
                LastUpdatedAt = readModel.LastUpdatedAt
            };
        }
    }

    public class GetDemoTripByIdQueryHandler
    {
        private readonly TripDbContext _context;

        public GetDemoTripByIdQueryHandler(TripDbContext context)
        {
            _context = context;
        }

        public async Task<DemoTripReadResponse?> HandleAsync(GetDemoTripByIdQuery query, CancellationToken cancellationToken)
        {
            var readModel = await _context.DemoTripReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TripId == query.TripId, cancellationToken);

            if (readModel == null)
            {
                return null;
            }

            return new DemoTripReadResponse
            {
                TripId = readModel.TripId,
                DriverId = readModel.DriverId,
                VehicleId = readModel.VehicleId,
                StartLocation = readModel.StartLocation,
                EndLocation = readModel.EndLocation,
                Status = readModel.Status,
                StartTime = readModel.StartTime,
                EndTime = readModel.EndTime,
                SagaId = readModel.SagaId,
                LastUpdatedAt = readModel.LastUpdatedAt
            };
        }
    }
}
