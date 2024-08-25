using CsvHelper;
using FlightChangeDetection.DAO.Repository;
using FlightChangeDetection.Service.Dtos;
using FlightChangeDetection.Service.Services;
using System.Globalization;

namespace FlightChangeDetection.ConsoleApp
{
    class FCDFacade
    {
        static async Task Main(string[] args)
        {
            if (!ValidateArguments(args, out DateTime startDate, out DateTime endDate, out int agencyId))
            {
                Console.WriteLine("Usage: FCDFacade.exe <start date> <end date> <agency id>");
                return;
            }

            Console.WriteLine($"Entered StartDate: {startDate}, Entered EndDate: {endDate}, AgencyId: {agencyId}");

            var dbContext = new FCDRepository();
            var flightService = new FCDService(dbContext);

            // Start the execution
            await ExecuteChangeDetectionAsync(flightService, startDate, endDate, agencyId);

            // Keep the application running until 'Q' is pressed
            await WaitForExitAsync();
        }

        static bool ValidateArguments(string[] args, out DateTime startDate, out DateTime endDate, out int agencyId)
        {
            startDate = default;
            endDate = default;
            agencyId = default;

            if (args.Length != 3)
            {
                return false;
            }

            // Attempt to parse arguments
            if (!DateTime.TryParse(args[0], out startDate) ||
                !DateTime.TryParse(args[1], out endDate) ||
                !int.TryParse(args[2], out agencyId))
            {
                Console.WriteLine("Invalid input. Ensure dates are in yyyy-MM-dd format and agency id is a valid integer.");
                return false;
            }

            return true;
        }

        static async Task ExecuteChangeDetectionAsync(FCDService flightService, DateTime startDate, DateTime endDate, int agencyId)
        {
            var startTime = DateTime.Now;

            // Get filtered flights
            var flights = flightService.GetFilteredFlights(startDate, endDate, agencyId);

            var endTime = DateTime.Now;
            Console.WriteLine($"Execution Time: {(endTime - startTime).TotalMilliseconds} ms");

            // Write the results to a CSV file asynchronously
            await WriteFlightsToCsvAsync(flights, "results.csv");
        }

        static async Task WriteFlightsToCsvAsync(List<FlightDto> flights, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(flights);
            }
        }

        static async Task WaitForExitAsync()
        {
            Console.WriteLine("Press Q to quit the Application.");
            while (true)
            {
                var key = await Task.Run(() => Console.ReadKey(intercept: true)); // Read key press asynchronously

                if (key.Key == ConsoleKey.Q)
                {
                    Console.WriteLine("Application exited.");
                    break;
                }
            }
        }
    }
}
