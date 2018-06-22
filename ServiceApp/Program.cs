using System.Diagnostics;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ServiceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var isService = true;
            
            if (Debugger.IsAttached || args.Contains("--console"))
            {
                Log.Information("Setting isService to true");
                isService = false;
            }
            
            var pathToContentRoot = Directory.GetCurrentDirectory();
            
            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }
            
            var webHostArgs = args.Where(arg => arg != "--console").ToArray();
            
            var host = WebHost.CreateDefaultBuilder(webHostArgs)
                .ConfigureServices(services => services.AddAutofac())
                .UseContentRoot(pathToContentRoot)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
            
            if (isService)
            {
                Log.Information("Running as a service");
                host.RunAsService();
            }
            else
            {
                Log.Information("Running as a console app");
                host.Run();
            }
        }
    }
}
