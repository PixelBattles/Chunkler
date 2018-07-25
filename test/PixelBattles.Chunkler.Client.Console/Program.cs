﻿using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessAsync().Wait();
        }

        private static async Task ProcessAsync()
        {
            Console.WriteLine("Starting...");
            ChunklerClient chunklerClient = new ChunklerClient(cfg => cfg.AddConsole());
            await chunklerClient.Connect();
            BattleAction gameAction = new BattleAction
            {
                Key = new ChunkKey
                {
                    BattleId = Guid.Parse("88d311d3-34bb-4584-9e6f-165e966d7cf7"),
                    ChunkXIndex = 0,
                    ChunkYIndex = 0,
                },
                WidthIndex = 0,
                HeightIndex = 0,
                Color = 2465474
            };
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < 10000; i++)
            {
                await chunklerClient.ProcessAction(gameAction);
            }
            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadKey();
        }
    }
}
