using Microsoft.Extensions.Configuration;
using System.Reflection;

public class ConfigurationService
{
    private IConfigurationRoot _configuration;

    public ConfigurationService()
    {
        //var builder = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory()) // Set base path for the configuration file
        //    .AddJsonFile("config.json", optional: false, reloadOnChange: true); // Add the appsettings.json file

        //_configuration = builder.Build();

        var builder = new ConfigurationBuilder()
           .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
           .AddJsonFile("appsettings.json");

        _configuration = builder.Build();
    }

    public string GetSetting(string key)
    {
        return _configuration[key]; // Retrieve the setting from the configuration
    }
}
