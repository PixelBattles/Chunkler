using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace PixelBattles.Chunkler.Client
{
    public static class ChunklerClientExtensions
    {
        public static IServiceCollection AddChunklerClient(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var options = configuration.GetSection(nameof(ChunklerClientOptions)).Get<ChunklerClientOptions>();
            return AddChunklerClient(services, options);
        }

        public static IServiceCollection AddChunklerClient(this IServiceCollection services, Action<ChunklerClientOptions> configureOptions)
        {
            var options = new ChunklerClientOptions();
            configureOptions(options);
            return AddChunklerClient(services, options);
        }

        public static IServiceCollection AddChunklerClient(this IServiceCollection services, ChunklerClientOptions options)
        {
            return services
                .AddScoped<IChunklerClient, ChunklerClient>()
                .AddSingleton<ChunklerClientOptions>(options);
        }
    }
}