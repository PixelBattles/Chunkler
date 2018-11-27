using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace PixelBattles.Chunkler.Client.Tests
{
    public class Common
    {
        [Fact(Skip = "Integration")]
        public async Task Test1()
        {
            ChunklerClient chunklerClient = new ChunklerClient(new ChunklerClientOptions
            {
                ClusterId = "dev",
                ServiceId = "ChunklerService"
            });
            await chunklerClient.Connect();
            var chunkKey = new ChunkKey
            {
                BattleId = Guid.Parse("53f0c496-7a95-43ae-8e9a-81d65fc3d478"),
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
        }
    }
}
