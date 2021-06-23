using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using feed_grain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Placement;
using Orleans.Runtime;

namespace feed_siloclient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IConfiguration Configuration;

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    Configuration = config.Build();
                })
                .UseOrleans((ctx, siloBuilder) =>
                {
                    var redisConnectionString = Configuration.GetConnectionString("Redis");

                    if (ctx.HostingEnvironment.IsDevelopment())

                    {
                        Console.WriteLine("Starting Local Hosting");
                        siloBuilder.UseLocalhostClustering()
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = "local";
                                options.ServiceId = "feeder";
                            })
                            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                            .ConfigureLogging(logging => logging.AddConsole());
                    }
                    else
                    {
                        Console.WriteLine("Starting Kubernetes Hosting");

                        siloBuilder.UseKubernetesHosting().Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "feedstarter-app";
                            options.ServiceId = "feeder";
                        });
                        siloBuilder.UseRedisClustering(options =>
                            {
                                options.ConnectionString = redisConnectionString;
                                options.Database = 3;
                            })
                            //.Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                            .ConfigureLogging(logging => logging.AddConsole());
                    }

                    siloBuilder.ConfigureApplicationParts(parts =>
                        parts.AddApplicationPart(typeof(ClientSubscriptionGrain).Assembly).WithReferences());

                    siloBuilder.UseDashboard(options => options.HostSelf = false);

                    siloBuilder.AddRedisGrainStorage("RedisSubscriptionGrain", options =>
                    {
                        options.ConnectionString = redisConnectionString;
                        options.DatabaseNumber = 7;
                        options.UseJson = true;
                    });

                    siloBuilder.AddRedisGrainStorageAsDefault(options =>
                    {
                        options.ConnectionString = redisConnectionString;
                        options.DatabaseNumber = 9;
                        options.UseJson = true;
                    });

                    siloBuilder.ConfigureServices(services =>
                    {
                        services.AddSingleton<PlacementStrategy, ActivationCountBasedPlacement>();
                    });
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}