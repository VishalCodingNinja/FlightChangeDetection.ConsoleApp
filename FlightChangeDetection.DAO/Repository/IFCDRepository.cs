using FlightChangeDetection.DAO.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightChangeDetection.DAO.Repository
{
    public interface IFCDRepository
    {
        DbSet<Flight>? Flights { get; set; }
        DbSet<Route>? Routes { get; set; }
        DbSet<Subscription>? Subscriptions { get; set; }
    }
}
