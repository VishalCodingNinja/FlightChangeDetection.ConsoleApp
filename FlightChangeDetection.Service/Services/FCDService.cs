using FlightChangeDetection.DAO.Entities;
using FlightChangeDetection.DAO.Repository;
using FlightChangeDetection.Service.Dtos;
using FlightChangeDetection.Service.Extensions;
using Microsoft.EntityFrameworkCore;
namespace FlightChangeDetection.Service.Services
{
    public class FCDService : IFCDService
    {
        private readonly IFCDRepository _context;

        public FCDService(IFCDRepository context)
        {
            _context = context;
        }

        public List<FlightDto> GetFilteredFlights(DateTime startDate, DateTime endDate, int agencyId)
        {
            // Fetch subscriptions related to the agency
            var subscriptionRoutes = _context.Subscriptions
                .Where(s => s.AgencyId == agencyId)
                .Select(s => new { s.OriginCityId, s.DestinationCityId })
                .ToList();

            // Fetch subscription routes into memory
            var subscriptionRoutesList = subscriptionRoutes.ToList();


            // Create a list of tuples representing (OriginCityId, DestinationCityId) from the in-memory list
            var cityPairs = subscriptionRoutesList
                .Select(s => (s.OriginCityId, s.DestinationCityId))
                .ToList();

            // Load all the routes into memory
            var allRoutes = _context.Routes.ToList();

            // Filter the routes based on the city pairs in memory
            var relevantRouteIds = allRoutes
                .Where(r => cityPairs
                    .Any(cp => cp.OriginCityId == r.OriginCityId && cp.DestinationCityId == r.DestinationCityId))
                .Select(r => r.RouteId)
                .ToList();

            // Adjust batch size according to your environment and dataset size and can be configured on configured file
            int batchSize = 1000;
            var flights = new List<Flight>();

            // Batch processing to fetch flights in chunks
            foreach (var routeIdBatch in relevantRouteIds.Batch(batchSize))
            {
                var batchFlights = _context.Flights
                    .Where(f => routeIdBatch.Contains(f.RouteId) && f.DepartureTime >= startDate && f.DepartureTime <= endDate)
                    .OrderBy(f => f.RouteId)
                    .ToList();

                flights.AddRange(batchFlights);
            }

            // Initialize FlightDto list to store results
            var flightDtos = new List<FlightDto>();

            // Process each flight to determine its status
            foreach (var flight in flights)
            {
                var previousFlight = _context.Flights
                    .Where(f => f.RouteId == flight.RouteId && f.AirlineId == flight.AirlineId && f.DepartureTime < flight.DepartureTime && f.DepartureTime >= flight.DepartureTime.AddDays(-7).AddMinutes(-30))
                    .OrderByDescending(f => f.DepartureTime)
                    .FirstOrDefault();

                var nextFlight = _context.Flights
                    .Where(f => f.RouteId == flight.RouteId && f.AirlineId == flight.AirlineId && f.DepartureTime > flight.DepartureTime && f.DepartureTime <= flight.DepartureTime.AddDays(7).AddMinutes(30))
                    .OrderBy(f => f.DepartureTime)
                    .FirstOrDefault();

                var status = DetermineFlightStatus(previousFlight, nextFlight);

                flightDtos.Add(new FlightDto
                {
                    FlightId = flight.FlightId,
                    OriginCityId = flight.Route.OriginCityId,
                    DestinationCityId = flight.Route.DestinationCityId,
                    DepartureTime = flight.DepartureTime,
                    ArrivalTime = flight.ArrivalTime,
                    AirlineId = flight.AirlineId,
                    Status = status
                });
            }

            return flightDtos;
        }

        public string DetermineFlightStatus(Flight previousFlight, Flight nextFlight)
        {
            if (previousFlight == null) return "New";
            if (nextFlight == null) return "Discontinued";
            return "Unchanged";
        }
    }
}
