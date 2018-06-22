using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace ServiceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isService = true;
            
            if (Debugger.IsAttached || args.Contains("--console"))
            {
                isService = false;
            }
            
            var pathToContentRoot = Directory.GetCurrentDirectory();
            
            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }
            
            var webHostArgs = args.Where(arg => arg != "--console").ToArray();
            
            var host = WebHost.CreateDefaultBuilder(webHostArgs).UseContentRoot(pathToContentRoot).UseStartup<Startup>().Build();
            
            if (isService)
            {
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }
    }
}
