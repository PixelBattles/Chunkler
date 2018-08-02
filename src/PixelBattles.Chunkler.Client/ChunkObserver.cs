using Microsoft.Extensions.Logging;
using System;

namespace PixelBattles.Chunkler.Client
{
    public class ChunkObserver : IChunkObserver
    {
        ILogger logger;

        public ChunkObserver(ILogger<ChunkObserver> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ChunkUpdated(int updateIndex)
        {
            logger.LogDebug($"Chunk updated {updateIndex}");
        }
    }
}