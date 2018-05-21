﻿using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using PixelBattles.Chunkler.Grains;
using System;
using System.Net;
using System.Threading.Tasks;
using PixelBattles.Server.Client;

namespace PixelBattles.Chunkler.Hosting
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var builder = new SiloHostBuilder() 
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MyAwesomeService";
                })
                .AddMemoryGrainStorage("MemoryStore")
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureLogging(logging => logging.AddConsole())
                .ConfigureServices(c => c.AddApiClient(opt => opt.BaseUrl = "http://192.168.0.1:5000"))
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChunkGrain).Assembly).WithReferences());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
