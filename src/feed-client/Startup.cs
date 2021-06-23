using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using feed_grain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;

namespace feed_client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IClusterClient>(CreateClusterClient);
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "feed_client", Version = "v1"});
            });
        }

        private IClusterClient CreateClusterClient(IServiceProvider provider)
        {
            var attempt = 0;
            var maxAttempts = 20;
            IClusterClient  client;
            while (true)
            {
                try
                {
                    client = new ClientBuilder()
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(ClientSubscriptionGrain).Assembly).WithReferences())
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "feed";
                        })
                        .UseLocalhostClustering()
                        .ConfigureLogging(logging => logging.AddConsole())
                        .Build();
                    
                    client.Connect(RetryFilter).GetAwaiter().GetResult();
                    Console.WriteLine("Client connected successfully to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt+1} of {maxAttempts} failed to initialize the orleans client");
                    if (attempt > maxAttempts)
                    {
                        throw;
                    }
                }

                Task.Delay(TimeSpan.FromSeconds(4));
            }

            return client;
        }

        private async Task<bool> RetryFilter(Exception exception)
        {
            Console.WriteLine($"Exception while attempting to connect to Orleans cluster: {exception}");
            await Task.Delay(TimeSpan.FromSeconds(2));
            return true;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "feed_client v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}