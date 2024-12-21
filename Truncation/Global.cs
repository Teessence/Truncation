using Microsoft.Extensions.Configuration;

namespace Truncation
{
    public class Global
    {
        public static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        public static string TessdataFolderPath = config["Paths:Tessdata"];
    }
}