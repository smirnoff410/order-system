using Microsoft.Extensions.Configuration;

namespace SharedDatabaseHelper
{
    public static class TemplateConnectionString
    {
        public static string Get()
        {
            // получаем конфигурацию из файла appsettings.json
            var currentDirectory = Directory.GetCurrentDirectory();

            var files = Directory.GetFiles(currentDirectory);
            var isAppsettingsExist = Directory.GetFiles(currentDirectory).Contains(Path.Combine(currentDirectory, "appsettings.json"));
            if (!isAppsettingsExist)
                throw new Exception("appsettings.json file not found");

            ConfigurationBuilder builder = new();
            builder.SetBasePath(currentDirectory);
            builder.AddJsonFile("appsettings.json");
            builder.AddJsonFile("Properties/launchSettings.json");
            IConfigurationRoot config = builder.Build();

            // получаем строку подключения из файла appsettings.json
            var templateConnectionString = config.GetConnectionString("MasterConnection");
            if (string.IsNullOrEmpty(templateConnectionString))
                throw new Exception("Template connection string is empty");
            return templateConnectionString;
        }
    }
}
