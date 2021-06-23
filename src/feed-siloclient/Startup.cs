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
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace feed_siloclient
{
    public class Startup
    {
        private readonly IHostingEnvironment _currentEnvironment;


        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _currentEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "feed_siloclient", Version = "v1"});
            });

            //services.AddSingleton<IClusterClient>(CreateClusterClient);
                       
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "feed_siloclient v1"));
            //}

            app.UseOrleansDashboard();
            app.Map("/dashboard", applicationBuilder => { applicationBuilder.UseOrleansDashboard(); });


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private IClusterClient CreateClusterClient(IServiceProvider provider)
        {
            var attempt = 0;
            var maxAttempts = 20;
            IClusterClient client;
            while (true)
            {
                try
                {
                    var clientBuilder = new ClientBuilder()
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(ClientSubscriptionGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole());

                    if (_currentEnvironment.IsDevelopment())
                    {
                        clientBuilder
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = "dev";
                                options.ServiceId = "feed";
                            })
                            .UseLocalhostClustering();
                    }
                    else
                    {
                        clientBuilder
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = "feedstarter-app";
                                options.ServiceId = "feed";
                            })
                            .UseLocalhostClustering();
                    }

                    client = clientBuilder.Build();

                    client.Connect(RetryFilter).GetAwaiter().GetResult();
                    Console.WriteLine("Client connected successfully to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine(
                        $"Attempt {attempt + 1} of {maxAttempts} failed to initialize the orleans client");
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
    }
}