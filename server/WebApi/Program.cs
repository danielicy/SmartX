using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;


namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
           /* DBManager dbmanager = new DBManager();
            dbmanager.Run();*/

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:4000")
                .Build();
    }
}


