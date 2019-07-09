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
            var changeIndex = await ChunklerClient.ProcessChunkActionAsync(ActiveBattleChunkKey, new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 });

            Assert.NotEqual(changeIndex, currentState.ChangeIndex);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_Can_ProcessAction_And_StateUpdated()
        {
            var currentState = await ChunklerClient.GetChunkStateAsync(ActiveBattleChunkKey);
            var changeIndex = await ChunklerClient.ProcessChunkActionAsync(ActiveBattleChunkKey, new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 });
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
            await ChunklerClient.SubscribeOnChunkUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                update = chunkUpdate;
                taskCompletionSource.SetResult(new object());
                return Task.CompletedTask;
            });
            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            var changeIndex = await ChunklerClient.ProcessChunkActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(10000));

            Assert.NotNull(update);
            Assert.Equal(changeIndex, update.ChangeIndex);
            Assert.Equal(action.XIndex, update.XIndex);
            Assert.Equal(action.YIndex, update.YIndex);
            Assert.Equal(action.Color, update.Color);
        }

        [Trait("Category", "Integration")]
        public async Task ChunklerClient_CanHandle_SeveralSubscribeUpdates()
        {
            var firstTaskCompletionSource = new TaskCompletionSource<object>();
            ChunkUpdate firstUpdate = null;
            var secondTaskCompletionSource = new TaskCompletionSource<object>();
            ChunkUpdate secondUpdate = null;

            await ChunklerClient.SubscribeOnChunkUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                firstUpdate = chunkUpdate;
                firstTaskCompletionSource.SetResult(new object());
                return Task.CompletedTask;
            });

            await ChunklerClient.SubscribeOnChunkUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                secondUpdate = chunkUpdate;
                secondTaskCompletionSource.SetResult(new object());
                return Task.CompletedTask;
            });

            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            var changeIndex = await ChunklerClient.ProcessChunkActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(Task.WhenAll(firstTaskCompletionSource.Task, secondTaskCompletionSource.Task), Task.Delay(10000));

            Assert.NotNull(firstUpdate);
            Assert.Equal(changeIndex, firstUpdate.ChangeIndex);
            Assert.Equal(action.XIndex, firstUpdate.XIndex);
            Assert.Equal(action.YIndex, firstUpdate.YIndex);
            Assert.Equal(action.Color, firstUpdate.Color);

            Assert.NotNull(secondUpdate);
            Assert.Equal(changeIndex, secondUpdate.ChangeIndex);
            Assert.Equal(action.XIndex, secondUpdate.XIndex);
            Assert.Equal(action.YIndex, secondUpdate.YIndex);
            Assert.Equal(action.Color, secondUpdate.Color);
        }

        [Trait("Category", "Integration")]
        public async Task ChunklerClient_CanHandle_UnsubscribeAfterSeveralSubscribes()
        {
            var firstTaskCompletionSource = new TaskCompletionSource<object>();
            var secondTaskCompletionSource = new TaskCompletionSource<object>();

            var subscription = await ChunklerClient.SubscribeOnChunkUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                firstTaskCompletionSource.SetResult(new object());
                return Task.CompletedTask;
            });

            await ChunklerClient.SubscribeOnChunkUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                secondTaskCompletionSource.SetResult(new object());
                return Task.CompletedTask;
            });

            await subscription.CloseAsync();

            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            var changeIndex = await ChunklerClient.ProcessChunkActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(Task.WhenAll(firstTaskCompletionSource.Task, secondTaskCompletionSource.Task), Task.Delay(1000));


            Assert.False(firstTaskCompletionSource.Task.IsCompleted);
            Assert.True(secondTaskCompletionSource.Task.IsCompleted);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_CanHandle_UnsubscribeUpdates()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            ChunkUpdate update = null;
            var subscription = await ChunklerClient.SubscribeOnChunkUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                update = chunkUpdate;
                taskCompletionSource.SetResult(new object());
                return Task.CompletedTask;
            });
            await subscription.CloseAsync();
            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            var changeIndex = await ChunklerClient.ProcessChunkActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(1000));

            Assert.Null(update);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ChunklerClient_CanEnqueueActions()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            ChunkUpdate update = null;
            var subscription = await ChunklerClient.SubscribeOnChunkUpdateAsync(ActiveBattleChunkKey, chunkUpdate =>
            {
                update = chunkUpdate;
                taskCompletionSource.SetResult(new object());
                return Task.CompletedTask;
            });
            var action = new ChunkAction() { Color = 0, XIndex = 0, YIndex = 0 };
            await ChunklerClient.EnqueueChunkActionAsync(ActiveBattleChunkKey, action);

            await Task.WhenAny(taskCompletionSource.Task, Task.Delay(10000));
            subscription.CloseAsync();

            Assert.NotNull(update);
            Assert.Equal(action.XIndex, update.XIndex);
            Assert.Equal(action.YIndex, update.YIndex);
            Assert.Equal(action.Color, update.Color);            
        }
    }
}
