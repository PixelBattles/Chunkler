using Microsoft.Extensions.Logging;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunkObserver : IAsyncObserver<ChunkUpdate>
    {
        private ILogger _logger;
        private Func<ChunkUpdate, Task> _onUpdate;

        public ChunkObserver(ILogger logger, Func<ChunkUpdate, Task> onUpdate)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _onUpdate = onUpdate ?? throw new ArgumentNullException(nameof(onUpdate));
        }
        
        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(ChunkUpdate item, StreamSequenceToken token = null)
        {
            await _onUpdate.Invoke(item);
        }
    }
}