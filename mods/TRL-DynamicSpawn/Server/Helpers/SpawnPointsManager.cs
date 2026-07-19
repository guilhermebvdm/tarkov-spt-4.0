using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TRLDynamicSpawnServer.Helpers
{
    public static class SpawnPointsManager
    {
        public static Dictionary<string, List<Vector3Model>> PmcSpawns = new();
        public static Dictionary<string, List<Vector3Model>> ScavSpawns = new();
        public static Dictionary<string, List<Vector3Model>> SniperSpawns = new();
        public static Dictionary<string, List<Vector3Model>> PlayerSpawns = new();

        public static void LoadSpawns()
        {
            string basePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory, "config", "Spawns");

            if (!Directory.Exists(basePath)) return;

            LoadFile(Path.Combine(basePath, "pmcSpawns.json"), ref PmcSpawns);
            LoadFile(Path.Combine(basePath, "scavSpawns.json"), ref ScavSpawns);
            LoadFile(Path.Combine(basePath, "sniperSpawns.json"), ref SniperSpawns);
            LoadFile(Path.Combine(basePath, "playerSpawns.json"), ref PlayerSpawns);
        }

        private static void LoadFile(string path, ref Dictionary<string, List<Vector3Model>> dict)
        {
            if (File.Exists(path))
            {
                try
                {
                    string content = File.ReadAllText(path);
                    dict = JsonSerializer.Deserialize<Dictionary<string, List<Vector3Model>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Dictionary<string, List<Vector3Model>>();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[TRL-DynamicSpawn] Error loading spawns from {path}: {e.Message}");
                }
            }
        }
    }

    public class Vector3Model
    {
        [JsonPropertyName("x")] public float X { get; set; }
        [JsonPropertyName("y")] public float Y { get; set; }
        [JsonPropertyName("z")] public float Z { get; set; }
    }
}

