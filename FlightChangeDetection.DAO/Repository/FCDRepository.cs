using FlightChangeDetection.DAO.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightChangeDetection.DAO.Repository
{
    public class FCDRepository : DbContext, IFCDRepository
    {
        public DbSet<Flight>? Flights { get; set; }
        public DbSet<Route>? Routes { get; set; }
        public DbSet<Subscription>? Subscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configService = new ConfigurationService();
            var mySetting = configService.GetSetting("ConnectionStrings:DefaultConnection");
            Console.WriteLine($"MySetting value: {mySetting}");

            optionsBuilder.UseSqlServer(mySetting);
        }
    }
}
