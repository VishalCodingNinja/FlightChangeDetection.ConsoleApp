using FlightChangeDetection.DAO.Entities;
using FlightChangeDetection.Service.Dtos;

namespace FlightChangeDetection.Service.Services
{
    public interface IFCDService
    {
        List<FlightDto> GetFilteredFlights(DateTime startDate, DateTime endDate, int agencyId);
        string DetermineFlightStatus(Flight previousFlight, Flight nextFlight);
    }
}
