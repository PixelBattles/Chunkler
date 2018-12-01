using System;
using System.Threading.Tasks;
using Xunit;

namespace PixelBattles.Chunkler.Client.Tests
{
    public class CommonIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_Can_ProcessAction()
        {
            var battleId = Guid.Parse("53f0c496-7a95-43ae-8e9a-81d65fc3d478");
            var battle = await ApiClient.GetBattleAsync(battleId);

            Assert.NotNull(battle);

            var chunkKey = new ChunkKey
            {
                BattleId = Guid.Parse("53f0c496-7a95-43ae-8e9a-81d65fc3d478"),
                ChunkXIndex =  0,
                ChunkYIndex = 0,
            };

            var action = new ChunkAction
            {
                XIndex = 0,
                YIndex = 0,
                Color = 2465474
            };

            var currentState = await ChunklerClient.GetChunkState(chunkKey);
            var changeIndex = await ChunklerClient.ProcessAction(chunkKey, action);
            var newState = await ChunklerClient.GetChunkState(chunkKey);

            Assert.Equal(changeIndex, newState.ChangeIndex);
            Assert.Equal(changeIndex, currentState.ChangeIndex + 1);
        }
    }
}
