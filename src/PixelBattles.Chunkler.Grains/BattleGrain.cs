using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using PixelBattles.API.Client;
using PixelBattles.API.DataTransfer.Battles;
using PixelBattles.API.DataTransfer.Images;
using PixelBattles.Chunkler.Abstractions;
using PixelBattles.Chunkler.Grains.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    [StorageProvider(ProviderName = ChunklerConstants.MongoDBGrainStorage)]
    public class BattleGrain : Grain<BattleGrainState>, IBattleGrain
    {
        private const string PreviewReminder = "BattlePreviewReminder";

        private readonly ILogger _logger;
        private readonly IApiClient _apiClient;
        private readonly IImageProcessor _imageProcessor;

        private long _battleId;
        private BattleDTO _battle;

        public BattleGrain(
            IApiClient apiClient,
            IImageProcessor imageProcessor,
            ILogger<BattleGrain> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ReadStateAsync()
        {
            await base.ReadStateAsync();

            _battleId = this.GetPrimaryKeyLong();
            _battle = await _apiClient.GetBattleAsync(_battleId);
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        protected override Task ClearStateAsync()
        {
            State.Timestamp = null;
            return base.ClearStateAsync();
        }

        public Task<BattleState> GetStateAsync()
        {
            return Task.FromResult(new BattleState
            {
                Timestamp = State.Timestamp ?? DateTime.UtcNow
            });
        }

        private async Task<byte[]> GenerateBattlePreviewAsync()
        {
            int centerX = 0;
            int centerY = 0;
            int previewHalfHeight = 150;
            int previewHalfWidth = 100;

            int chunkXFrom = (int)Math.Max(Math.Ceiling(((decimal)(centerX - previewHalfWidth)) / _battle.Settings.ChunkWidth), _battle.Settings.MinWidthIndex);
            int chunkYFrom = (int)Math.Max(Math.Ceiling(((decimal)(centerY - previewHalfHeight)) / _battle.Settings.ChunkHeight), _battle.Settings.MinHeightIndex);
            int chunkXTo = (int)Math.Min(Math.Floor(((decimal)(centerX + previewHalfWidth)) / _battle.Settings.ChunkWidth), _battle.Settings.MaxWidthIndex);
            int chunkYTo = (int)Math.Min(Math.Floor(((decimal)(centerY + previewHalfHeight)) / _battle.Settings.ChunkHeight), _battle.Settings.MaxHeightIndex);

            var chunks = new List<(int x, int y, byte[] image)>((chunkXTo - chunkXFrom) * (chunkYTo - chunkYFrom));
            for (int i = chunkXFrom; i <= chunkXTo; i++)
            {
                for (int j = chunkYFrom; j <= chunkYTo; j++)
                {
                    var chunkGrain = this.GrainFactory.GetGrain<IChunkGrain>(GuidExtensions.ToGuid(_battleId, i, j));
                    var chunkState = await chunkGrain.GetStateAsync();
                    chunks.Add((i, j, chunkState.Image));
                }
            }

            return _imageProcessor.GenerateImageFromChunks(
                chunks: chunks,
                chunkHeight: _battle.Settings.ChunkHeight,
                chunkWidth: _battle.Settings.ChunkWidth,
                previewHeight: previewHalfHeight,
                previewWidth: previewHalfWidth,
                centerX: centerX,
                centerY: centerY);
        }

        private async Task UpdatePreviewImageAsync()
        {
            try
            {
                var image = await GenerateBattlePreviewAsync();
                if (_battle.ImageId == null)
                {
                    var imageResult = await _apiClient.CreateImageAsync(new CreateImageDTO
                    {
                        Name = $"{_battleId} battle preview image.",
                        Description = $"This is {_battleId} battle preview image."
                    }, image, "preview.png", "image/png");
                    if (!imageResult.Succeeded)
                    {
                        return;
                    }
                    var battleResult = await _apiClient.UpdateBattleImageAsync(_battleId, new UpdateBattleImageDTO
                    {
                        ImageId = imageResult.ImageId.Value
                    });
                    if (!imageResult.Succeeded)
                    {
                        return;
                    }
                    _battle.ImageId = imageResult.ImageId.Value;
                }
                else
                {
                    var imageResult = await _apiClient.UpdateImageAsync(_battle.ImageId.Value, new UpdateImageDTO
                    {
                        Name = $"{_battleId} battle preview image.",
                        Description = $"This is {_battleId} battle preview image."
                    }, image, "preview.png", "image/png");
                    if (!imageResult.Succeeded)
                    {
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to update {_battleId} battle preview image.");
            }
            
        }
        
        public async Task ActivateBattleReminderAsync(TimeSpan refreshInterval)
        {
            await RegisterOrUpdateReminder(PreviewReminder, _battle.EndDateUTC - DateTime.UtcNow + refreshInterval, refreshInterval);
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == PreviewReminder)
            {
                await UpdatePreviewImageAsync();
            }
        }
    }
}