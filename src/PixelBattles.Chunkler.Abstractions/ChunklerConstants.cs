namespace PixelBattles.Chunkler.Abstractions
{
    public static class ChunklerConstants
    {
        public const string MongoDBGrainStorage = "MongoDBGrainStorage";
        public const string PubSubStore = "PubSubStore";
        public const string SimpleChunkStreamProvider = "SimpleChunkStreamProvider";

        public const string ChunkIncomingAction = "ChunkIncomingAction";
        public const string ChunkOutcomingUpdate = "ChunkOutcomingUpdate";
    }
}