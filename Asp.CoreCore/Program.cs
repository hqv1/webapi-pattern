using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WebApiPattern.Asp.CoreCore
{
    /// <summary>
    /// This project shows how to create a Restful API in .NET CORE.
    /// 
    /// Start from an empty ASP.NET core project
    /// Install Microsoft.AspNetCore.Mvc.Core to use MVC
    /// Install Microsoft.AspNetCore.Mvc.Formatters.Xml to read/respond in XML
    /// 
    /// Install AspNetCoreRateLimit for rate limiting 
    /// Install Automapper for mapping of models and entities
    /// Install FluentValidation.AspNetCore for validation
    /// Install NLog.Web.AspNetCore for logging
    /// Install Swashbuckle.AspNetCore. Doesn't support ProductCollectionsController but oh well.
    /// 
    /// To run 
    /// dotnet .\WebApiPattern.AspCore.dll --urls"http://*:50346"
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
