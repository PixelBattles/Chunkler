using Microsoft.Extensions.Logging;
using System;
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
            ChunklerClient chunklerClient = new ChunklerClient(cfg => cfg.AddConsole());
            await chunklerClient.Connect();
            GameAction gameAction = new GameAction
            {
                GameId = Guid.NewGuid(),
                ChunkXIndex = 0,
                ChunkYIndex = 0,
                XIndex = 0,
                YIndex = 0,
                Color = 2465474
            };
            await chunklerClient.ProcessAction(gameAction);
        }
    }
}
