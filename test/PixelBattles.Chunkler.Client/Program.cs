using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessAsync().Wait();
        }

        private static async Task ProcessAsync()
        {
            Console.WriteLine("Starting...");
            ChunklerClient chunklerClient = new ChunklerClient(new ChunklerClientOptions
            {
                ClusterId = "dev",
                ServiceId = "ChunklerService"
            });
            await chunklerClient.Connect();
            var chunkKey = new ChunkKey
            {
                BattleId = Guid.Parse("88d311d3-34bb-4584-9e6f-165e966d7cf7"),
                ChunkXIndex = 0,
                ChunkYIndex = 0,
            };
            var action = new ChunkAction
            {
                XIndex = 0,
                YIndex = 0,
                Color = 2465474
            };
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < 10000; i++)
            {
                await chunklerClient.ProcessAction(chunkKey, action);
                await chunklerClient.GetChunkState(chunkKey);
            }
            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadKey();
        }
    }
}
