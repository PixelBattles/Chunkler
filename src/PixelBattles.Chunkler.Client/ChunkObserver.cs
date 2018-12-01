using Microsoft.Extensions.Logging;
using System;

namespace PixelBattles.Chunkler.Client
{
    public class ChunkObserver : IChunkObserver
    {
        ILogger _logger;

        public ChunkObserver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ChunkUpdated(ChunkKey chunkKey, ChunkUpdate update)
        {
            _logger.LogDebug($@"Chunk updated for battleId:{chunkKey.BattleId} and chunk:{chunkKey.ChunkXIndex}-{chunkKey.ChunkYIndex}. 
                    Change index: {update.ChangeIndex} in pixel:{update.XIndex}-{update.YIndex} to color:{update.Color}");
        }
    }
}