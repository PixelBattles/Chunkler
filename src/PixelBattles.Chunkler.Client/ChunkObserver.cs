using Microsoft.Extensions.Logging;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunkObserver : IAsyncObserver<ChunkUpdate>
    {
        private ILogger _logger;
        private Action<ChunkUpdate> _action;

        public ChunkObserver(ILogger logger, Action<ChunkUpdate> action)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }
        
        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task OnNextAsync(ChunkUpdate item, StreamSequenceToken token = null)
        {
            _action.Invoke(item);
            return Task.CompletedTask;
        }
    }
}