using System.Threading.Tasks;
using Xunit;

namespace PixelBattles.Chunkler.Client.Tests
{
    public class CommonIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task ApiClient_Can_Get_BattleInfo()
        {
            var battle = await ApiClient.GetBattleAsync(ActiveBattleId);

            Assert.NotNull(battle);
            Assert.Equal(battle.BattleId, ActiveBattleId);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_Can_GetChunkState()
        {
            var state = await ChunklerClient.GetChunkStateAsync(ActiveBattleChunkKey);
            Assert.NotNull(state);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_Can_ProcessAction()
        {
            var currentState = await ChunklerClient.GetChunkStateAsync(ActiveBattleChunkKey);
            var changeIndex = await ChunklerClient.ProcessActionAsync(ActiveBattleChunkKey, new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 });

            Assert.NotEqual(changeIndex, currentState.ChangeIndex);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_Can_ProcessAction_And_StateUpdated()
        {
            var currentState = await ChunklerClient.GetChunkStateAsync(ActiveBattleChunkKey);
            var changeIndex = await ChunklerClient.ProcessActionAsync(ActiveBattleChunkKey, new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 });
            var newState = await ChunklerClient.GetChunkStateAsync(ActiveBattleChunkKey);

            Assert.Equal(changeIndex, newState.ChangeIndex);
            Assert.NotEqual(currentState.ChangeIndex, newState.ChangeIndex);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_CanHandle_SubscribeUpdates()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            ChunkUpdate update = null;
            await ChunklerClient.SubscribeOnUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                update = chunkUpdate;
                taskCompletionSource.SetResult(new object());
            });
            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            var changeIndex = await ChunklerClient.ProcessActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(10000));

            Assert.NotNull(update);
            Assert.Equal(changeIndex, update.ChangeIndex);
            Assert.Equal(action.XIndex, update.XIndex);
            Assert.Equal(action.YIndex, update.YIndex);
            Assert.Equal(action.Color, update.Color);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_CanHandle_UnsubscribeUpdates()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            ChunkUpdate update = null;
            await ChunklerClient.SubscribeOnUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                update = chunkUpdate;
                taskCompletionSource.SetResult(new object());
            });
            await ChunklerClient.UnsubscribeOnUpdateAsync(ActiveBattleChunkKey);
            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            var changeIndex = await ChunklerClient.ProcessActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(1000));

            Assert.Null(update);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_CanEnqueueActions()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            ChunkUpdate update = null;
            await ChunklerClient.SubscribeOnUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                update = chunkUpdate;
                taskCompletionSource.SetResult(new object());
            });
            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            await ChunklerClient.EnqueueActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(10000));

            Assert.NotNull(update);
            Assert.Equal(action.XIndex, update.XIndex);
            Assert.Equal(action.YIndex, update.YIndex);
            Assert.Equal(action.Color, update.Color);
        }
    }
}
