using Orleans.Hosting;
using System;
using System.Threading.Tasks;

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
                .ConfigureDefaultHost();

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
