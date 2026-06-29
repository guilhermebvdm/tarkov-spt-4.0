using MOARServer.Globals;
using MOARServer.Models;
using SPTarkov.Server.Core.Models.Eft.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOARServer.Helpers;

public static class SpawnZoneCulling
{
    private static double GetDistance(double x, double y, double z, double x2, double y2, double z2)
    {
        double dx = Math.Abs(x - x2);
        double dy = Math.Abs(y - y2);
        double dz = Math.Abs(z - z2);
        double d1 = Math.Sqrt(dx * dx + dz * dz);
        return Math.Sqrt(d1 * d1 + dy * dy);
    }

    private static string GetClosestZone(IEnumerable<SpawnPointParam> paramsList, double x, double y, double z)
    {
        string closestZone = "";
        double closestDist = double.MaxValue;
        
        foreach (var p in paramsList)
        {
            if (string.IsNullOrEmpty(p.BotZoneName)) continue;
            
            double dist = GetDistance(x, y, z, p.Position.X ?? 0, p.Position.Y ?? 0, p.Position.Z ?? 0);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestZone = p.BotZoneName;
            }
        }
        return closestZone;
    }

    public static void ApplyCullingAndInjection(LocationBase location, string map, string configKey)
    {
        if (!ModConfig.MapConfig.TryGetValue(configKey, out var mapConfig)) return;

        double mapCullingNearPointValue = (mapConfig.MapCullingNearPointValuePlayer + 
                                         mapConfig.MapCullingNearPointValuePmc + 
                                         mapConfig.MapCullingNearPointValueScav) / 3.0;

        var originalSpawns = location.SpawnPointParams.ToList();
        var okayList = new HashSet<string>();
        
        // 1. Cull original spawns
        foreach (var point in originalSpawns)
        {
            var X = point.Position.X ?? 0;
            var Y = point.Position.Y ?? 0;
            var Z = point.Position.Z ?? 0;
            
            bool result = !originalSpawns.Any(other => 
            {
                if (other == point) return false;
                double dist = GetDistance(X, Y, Z, other.Position.X ?? 0, other.Position.Y ?? 0, other.Position.Z ?? 0);
                return mapCullingNearPointValue > dist && dist > 0 && !okayList.Contains(other.Id ?? "");
            });
            
            if (!result)
            {
                okayList.Add(point.Id ?? "");
                point.DelayToCanSpawnSec = 9999999;
                point.CorePointId = 99999;
                point.Categories = Array.Empty<string>();
                point.Sides = Array.Empty<string>();
            }
        }
        
        // 2. Inject custom spawns
        var allCustomSpawns = new List<SpawnPointParam>();
        var random = new Random();
        
        if (ModConfig.PmcSpawns.TryGetValue(configKey, out var customPmcSpawns))
        {
            foreach (var custom in customPmcSpawns)
            {
                allCustomSpawns.Add(CreateCustomSpawn(custom, new[] { "Pmc" }, new[] { "Coop", random.Next(0, 2) == 0 ? "Group" : "Opposite" }, 0, 20, originalSpawns));
            }
        }
        
        if (ModConfig.ScavSpawns.TryGetValue(configKey, out var customScavSpawns))
        {
            foreach (var custom in customScavSpawns)
            {
                allCustomSpawns.Add(CreateCustomSpawn(custom, new[] { "Savage" }, new[] { "Bot" }, 1, 20, originalSpawns));
            }
        }
        
        if (ModConfig.SniperSpawns.TryGetValue(configKey, out var customSniperSpawns))
        {
            foreach (var custom in customSniperSpawns)
            {
                allCustomSpawns.Add(CreateCustomSpawn(custom, new[] { "Savage" }, new[] { "Bot" }, 1, 20, originalSpawns));
            }
        }
        
        if (ModConfig.PlayerSpawns.TryGetValue(configKey, out var customPlayerSpawns))
        {
            foreach (var custom in customPlayerSpawns)
            {
                allCustomSpawns.Add(CreateCustomSpawn(custom, new[] { "Pmc" }, new[] { "Player" }, 0, 1, originalSpawns));
            }
        }
        
        var finalSpawns = originalSpawns.Where(x => x.Categories != null && x.Categories.Any()).ToList();
        finalSpawns.AddRange(allCustomSpawns);
        
        location.SpawnPointParams = finalSpawns.ToArray();
        
        // Update OpenZones
        var openZones = finalSpawns.Where(x => !string.IsNullOrEmpty(x.BotZoneName))
                                   .Select(x => x.BotZoneName)
                                   .Distinct();
        location.OpenZones = string.Join(",", openZones);
    }
    
    private static SpawnPointParam CreateCustomSpawn(SpawnPosition coords, string[] sides, string[] categories, int corePointId, float radius, List<SpawnPointParam> originalSpawns)
    {
        var random = new Random();
        return new SpawnPointParam
        {
            BotZoneName = GetClosestZone(originalSpawns, coords.X, coords.Y, coords.Z),
            Categories = categories,
            Sides = sides,
            CorePointId = corePointId,
            DelayToCanSpawnSec = 4,
            Id = Guid.NewGuid().ToString(),
            Infiltration = "",
            Position = new XYZ { X = (float)coords.X, Y = (float)coords.Y, Z = (float)coords.Z },
            Rotation = (float)(random.NextDouble() * 360.0)
        };
    }
}
