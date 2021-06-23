using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using feed_grain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace feed_silo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder.UseLocalhostClustering()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "local";
                            options.ServiceId = "feeder";
                        })
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(ClientSubscriptionGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole());

                    siloBuilder.UseDashboard(options => options.HostSelf = false);

                    siloBuilder.AddRedisGrainStorage("RedisSubscriptionGrain", options =>
                    {
                        options.ConnectionString = "localhost:6379";
                        options.DatabaseNumber = 2;
                        options.UseJson = true;
                    });

                    siloBuilder.AddRedisGrainStorageAsDefault();
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}