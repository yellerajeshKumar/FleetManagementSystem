using System.Net.Http.Json;
using AlertService.Data;
using AlertService.DTOs;
using AlertService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlertService.Controllers
{
    [ApiController]
    [Route("api/demo/saga")]
    [AllowAnonymous]
    public class DemoSagaController : ControllerBase
    {
        private readonly AlertDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DemoSagaController> _logger;

        public DemoSagaController(
            AlertDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<DemoSagaController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("start-trip")]
        public async Task<IActionResult> StartTripSaga([FromBody] StartDemoTripSagaRequest request, CancellationToken cancellationToken)
        {
            var saga = new DemoSagaInstance
            {
                SagaId = Guid.NewGuid().ToString("N"),
                DriverId = request.DriverId,
                VehicleId = request.VehicleId,
                StartLocation = request.StartLocation,
                EndLocation = request.EndLocation,
                Status = "Started",
                CurrentStep = "Persisted"
            };

            _context.DemoSagaInstances.Add(saga);
            await _context.SaveChangesAsync(cancellationToken);

            var driverBaseUrl = _configuration["DemoSaga:DriverServiceBaseUrl"] ?? "http://localhost:5063";
            var tripBaseUrl = _configuration["DemoSaga:TripServiceBaseUrl"] ?? "http://localhost:5114";
            var client = _httpClientFactory.CreateClient();

            try
            {
                saga.CurrentStep = "ReservingDriver";
                saga.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                var reserveResponse = await client.PostAsJsonAsync(
                    $"{driverBaseUrl}/api/demo/saga/drivers/reserve",
                    new ReserveDriverRequest { DriverId = request.DriverId, SagaId = saga.SagaId },
                    cancellationToken);

                if (!reserveResponse.IsSuccessStatusCode)
                {
                    saga.Status = "Failed";
                    saga.CurrentStep = "DriverReservationFailed";
                    saga.ErrorMessage = await reserveResponse.Content.ReadAsStringAsync(cancellationToken);
                    saga.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                    return StatusCode((int)reserveResponse.StatusCode, new
                    {
                        Message = "Saga failed while reserving driver",
                        saga.SagaId,
                        saga.Status,
                        saga.CurrentStep,
                        saga.ErrorMessage
                    });
                }

                saga.CurrentStep = "CreatingTrip";
                saga.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                var createTripResponse = await client.PostAsJsonAsync(
                    $"{tripBaseUrl}/api/demo/cqrs/trips",
                    new CreateDemoTripRequest
                    {
                        DriverId = request.DriverId,
                        VehicleId = request.VehicleId,
                        StartLocation = request.StartLocation,
                        EndLocation = request.EndLocation,
                        StartTime = request.StartTime,
                        SagaId = saga.SagaId
                    },
                    cancellationToken);

                if (!createTripResponse.IsSuccessStatusCode)
                {
                    await CompensateDriverAsync(client, driverBaseUrl, request.DriverId, saga.SagaId, cancellationToken);

                    saga.Status = "Compensated";
                    saga.CurrentStep = "TripCreationFailedCompensated";
                    saga.ErrorMessage = await createTripResponse.Content.ReadAsStringAsync(cancellationToken);
                    saga.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);

                    return StatusCode((int)createTripResponse.StatusCode, new
                    {
                        Message = "Saga compensated after trip creation failure",
                        saga.SagaId,
                        saga.Status,
                        saga.CurrentStep,
                        saga.ErrorMessage
                    });
                }

                var tripPayload = await createTripResponse.Content.ReadFromJsonAsync<CreatedTripDto>(cancellationToken: cancellationToken);
                saga.TripId = tripPayload?.TripId;
                saga.Status = "Completed";
                saga.CurrentStep = "Completed";
                saga.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new
                {
                    Message = "Saga completed successfully",
                    saga.SagaId,
                    saga.Status,
                    saga.CurrentStep,
                    saga.TripId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demo saga {SagaId} failed unexpectedly", saga.SagaId);

                await CompensateDriverAsync(client, driverBaseUrl, request.DriverId, saga.SagaId, cancellationToken);

                saga.Status = "Compensated";
                saga.CurrentStep = "UnhandledErrorCompensated";
                saga.ErrorMessage = ex.Message;
                saga.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                return StatusCode(500, new
                {
                    Message = "Saga failed due to unexpected error and compensation was attempted",
                    saga.SagaId,
                    saga.Status,
                    saga.CurrentStep,
                    saga.ErrorMessage
                });
            }
        }

        [HttpGet("{sagaId}")]
        public async Task<IActionResult> GetSaga(string sagaId, CancellationToken cancellationToken)
        {
            var saga = await _context.DemoSagaInstances.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SagaId == sagaId, cancellationToken);

            if (saga == null)
            {
                return NotFound(new { Message = $"Saga {sagaId} not found" });
            }

            return Ok(saga);
        }

        private static async Task CompensateDriverAsync(HttpClient client, string driverBaseUrl, Guid driverId, string sagaId, CancellationToken cancellationToken)
        {
            await client.PostAsJsonAsync(
                $"{driverBaseUrl}/api/demo/saga/drivers/release",
                new ReleaseDriverRequest { DriverId = driverId, SagaId = sagaId },
                cancellationToken);
        }

        private sealed class CreatedTripDto
        {
            public Guid TripId { get; set; }
        }
    }
}
