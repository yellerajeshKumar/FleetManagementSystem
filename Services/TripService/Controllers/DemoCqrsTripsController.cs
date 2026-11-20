using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripService.DTOs;
using TripService.Services;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/demo/cqrs/trips")]
    [AllowAnonymous]
    public class DemoCqrsTripsController : ControllerBase
    {
        private readonly CreateDemoTripCommandHandler _createHandler;
        private readonly CompleteDemoTripCommandHandler _completeHandler;
        private readonly CancelDemoTripCommandHandler _cancelHandler;
        private readonly GetDemoTripByIdQueryHandler _queryHandler;
        private readonly IDemoTripEventStore _eventStore;

        public DemoCqrsTripsController(
            CreateDemoTripCommandHandler createHandler,
            CompleteDemoTripCommandHandler completeHandler,
            CancelDemoTripCommandHandler cancelHandler,
            GetDemoTripByIdQueryHandler queryHandler,
            IDemoTripEventStore eventStore)
        {
            _createHandler = createHandler;
            _completeHandler = completeHandler;
            _cancelHandler = cancelHandler;
            _queryHandler = queryHandler;
            _eventStore = eventStore;
        }

        [HttpPost]
        public async Task<ActionResult<DemoTripReadResponse>> CreateDemoTrip([FromBody] CreateDemoTripRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateDemoTripCommand(
                request.DriverId,
                request.VehicleId,
                request.StartLocation,
                request.EndLocation,
                request.StartTime,
                request.SagaId);

            var created = await _createHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetDemoTrip), new { tripId = created.TripId }, created);
        }

        [HttpPut("{tripId:guid}/complete")]
        public async Task<ActionResult<DemoTripReadResponse>> CompleteDemoTrip(Guid tripId, [FromBody] CompleteDemoTripRequest request, CancellationToken cancellationToken)
        {
            var completed = await _completeHandler.HandleAsync(new CompleteDemoTripCommand(tripId, request.EndTime), cancellationToken);
            if (completed == null)
            {
                return NotFound($"Demo trip {tripId} not found");
            }

            return Ok(completed);
        }

        [HttpPut("{tripId:guid}/cancel")]
        public async Task<ActionResult<DemoTripReadResponse>> CancelDemoTrip(Guid tripId, [FromBody] CancelDemoTripRequest request, CancellationToken cancellationToken)
        {
            var cancelled = await _cancelHandler.HandleAsync(new CancelDemoTripCommand(tripId, request.Reason), cancellationToken);
            if (cancelled == null)
            {
                return NotFound($"Demo trip {tripId} not found");
            }

            return Ok(cancelled);
        }

        [HttpGet("{tripId:guid}")]
        public async Task<ActionResult<DemoTripReadResponse>> GetDemoTrip(Guid tripId, CancellationToken cancellationToken)
        {
            var result = await _queryHandler.HandleAsync(new GetDemoTripByIdQuery(tripId), cancellationToken);
            if (result == null)
            {
                return NotFound($"Demo trip {tripId} not found");
            }

            return Ok(result);
        }

        [HttpGet("{tripId:guid}/events")]
        public async Task<ActionResult> GetEvents(Guid tripId, CancellationToken cancellationToken)
        {
            var events = await _eventStore.GetByAggregateIdAsync(tripId, cancellationToken);
            return Ok(events);
        }
    }
}
