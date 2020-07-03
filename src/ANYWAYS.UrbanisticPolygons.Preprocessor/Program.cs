﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ANYWAYS.UrbanisticPolygons.Data;
using ANYWAYS.UrbanisticPolygons.Tiles;
using Microsoft.Extensions.Configuration;
using OsmSharp.Logging;
using OsmSharp.Tags;
using Serilog;
using Serilog.Formatting.Json;

namespace ANYWAYS.UrbanisticPolygons.Preprocessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            EnableLogging(config);
            
            var cacheFolder = config["cache_folder"];
            var tileUrl = config["tile_url"];
            
            var osmTileSource = new OsmTileSource(tileUrl, cacheFolder);

            bool IsBarrier(TagsCollectionBase? tags)
            {
                if (tags == null) return false;

                return DefaultMergeFactorCalculator.Barriers.TryCalculateValue(tags, out _);
            }
            
            var box = ((2.3785400390625, 51.52241608253253), (6.5093994140625, 49.40024999665212)); // belgium
            //var box = ((4.3402862548828125, 51.30099875579057), (4.75982666015625, 51.13627812193317));

            var tiles = box.TilesFor(14).Select(x => TileStatic.ToLocalId(x, 14));
            foreach (var tile in tiles)
            {
                Log.Information($"Processing tile {TileStatic.ToTile(14, tile)}...");
                await TiledBarrierGraphBuilder.BuildForTile(tile, cacheFolder, x =>
                {
                    //Log.Information($"Fetching OSM data tile {TileStatic.ToTile(14, x)}...");
                    return osmTileSource.GetTile(x);
                }, IsBarrier);
            }
        }
        
        private static void EnableLogging(IConfigurationRoot config)
        {
            // enable logging.
            Logger.LogAction = (origin, level, message, parameters) =>
            {
                var formattedMessage = $"{origin} - {message}";
                switch (level)
                {
                    case "critical":
                        Log.Fatal(formattedMessage);
                        break;
                    case "error":
                        Log.Error(formattedMessage);
                        break;
                    case "warning":
                        Log.Warning(formattedMessage);
                        break;
                    case "verbose":
                        Log.Verbose(formattedMessage);
                        break;
                    case "information":
                        Log.Information(formattedMessage);
                        break;
                    default:
                        Log.Debug(formattedMessage);
                        break;
                }
            };
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                var formattedMessage = $"{origin} - {message}";
                switch (level)
                {
                    case "critical":
                        Log.Fatal(formattedMessage);
                        break;
                    case "error":
                        Log.Error(formattedMessage);
                        break;
                    case "warning":
                        Log.Warning(formattedMessage);
                        break;
                    case "verbose":
                        Log.Verbose(formattedMessage);
                        break;
                    case "information":
                        Log.Information(formattedMessage);
                        break;
                    default:
                        Log.Debug(formattedMessage);
                        break;
                }
            };


            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
        }
    }
}
