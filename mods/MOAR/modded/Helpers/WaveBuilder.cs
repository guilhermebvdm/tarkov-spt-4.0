using MOARServer.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace MOARServer.Helpers;

[Injectable]
public class WaveBuilder(
    ISptLogger<WaveBuilder> logger,
    RandomUtil randomUtil)
{
    public List<BossLocationSpawn> BuildPmcWaves(string location)
    {
        var result = new List<BossLocationSpawn>();
        
        if (!ModConfig.PmcSpawns.TryGetValue(location, out var spawnPositions))
            return result;
            
        foreach (var pos in spawnPositions)
        {
            var pmcType = randomUtil.GetChance100(50) ? "pmcUSEC" : "pmcBEAR";
            var boss = new BossLocationSpawn
            {
                BossName = pmcType,
                BossChance = 100,
                BossZone = "",
                BossDifficulty = "normal",
                BossEscortType = pmcType,
                BossEscortDifficulty = "normal",
                BossEscortAmount = "0",
                Time = 1,
                Delay = 0,
                Supports = []
                // FIXME: Adicionar posição x,y,z (nas versões novas do SPT os spawns usam zonas, ou precisam de suporte a SpawnPointParams)
            };
            result.Add(boss);
        }
        
        return result;
    }

    public static string GetDifficulty(double diff)
    {
        var randomNumb = Random.Shared.NextDouble() + diff;
        if (randomNumb < 0.55) return "easy";
        if (randomNumb < 1.4) return "normal";
        if (randomNumb < 1.85) return "hard";
        return "impossible";
    }

    public static void EnforceSmoothing(List<BossLocationSpawn> waves, double smoothingDistribution)
    {
        var bosses = new List<BossLocationSpawn>();
        var notBosses = new List<BossLocationSpawn>();

        var notBossesSet = new HashSet<string> {
            "infectedLaborant",
            "infectedAssault",
            "infectedCivil",
            "assault",
            "marksman",
            "pmcBEAR",
            "pmcUSEC"
        };

        foreach (var wave in waves)
        {
            if (notBossesSet.Contains(wave.BossName))
            {
                notBosses.Add(wave);
            }
            else
            {
                bosses.Add(wave);
            }
        }

        if (notBosses.Count == 0) return;

        double first = double.MaxValue;
        double last = double.MinValue;

        foreach (var notBoss in notBosses)
        {
            if (notBoss.Time.GetValueOrDefault(0) < first) first = notBoss.Time.GetValueOrDefault(0);
            if (notBoss.Time.GetValueOrDefault(0) > last) last = notBoss.Time.GetValueOrDefault(0);
        }

        if (first < 15) first = 15;
        if (first == double.MaxValue || last == double.MinValue) return;

        notBosses.Sort((a, b) => a.Time.GetValueOrDefault(0).CompareTo(b.Time.GetValueOrDefault(0)));

        double start = first;
        double increment = Math.Round((last - first) / notBosses.Count) * 2 * smoothingDistribution;

        for (int i = 0; i < notBosses.Count; i++)
        {
            double ratio = (i + 1.0) / notBosses.Count;
            notBosses[i].Time = (int)start;
            
            double inc = Math.Round(increment * ratio);
            if (inc < 10) inc = 5;
            start += inc;
        }

        waves.Clear();
        waves.AddRange(bosses);
        waves.AddRange(notBosses);
    }
}
