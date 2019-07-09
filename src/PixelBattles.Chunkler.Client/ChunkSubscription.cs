using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunkSubscription : IChunkSubscription
    {
        private StreamSubscriptionHandle<ChunkUpdate> _subscriptionHandle;
        public ChunkSubscription(StreamSubscriptionHandle<ChunkUpdate> subscriptionHandle)
        {
            _subscriptionHandle = subscriptionHandle ?? throw new ArgumentNullException(nameof(subscriptionHandle));
        }

        public async Task CloseAsync()
        {
            await _subscriptionHandle.UnsubscribeAsync();
        }
    }
}