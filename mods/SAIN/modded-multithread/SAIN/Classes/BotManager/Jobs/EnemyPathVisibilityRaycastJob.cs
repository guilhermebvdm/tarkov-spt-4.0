using System;
using System.Collections;
using System.Collections.Generic;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.Types.Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components;

public class EnemyPathVisibilityRaycastJob : SainJobTemplate, IDisposable
{
    static EnemyPathVisibilityRaycastJob()
    {
        PresetHandler.OnPresetUpdated += updateSettings;
        updateSettings(PresetHandler.LoadedPreset);
    }

    private static void updateSettings(SAINPresetClass preset)
    {
        _commandsPerJob = Mathf.RoundToInt(preset.GlobalSettings.Steering.PathVisionMinCommandsPerJob);
    }

    protected readonly List<PathVisionJob> VisionJobs = [];
    protected readonly List<PathVisionJob> ShootJobs = [];
    private QueryParameters queryParams;

    private static int _commandsPerJob = 256;

    public EnemyPathVisibilityRaycastJob(MonoBehaviour botcontroller)
        : base("Path Visibility Job", botcontroller, true, 1f / 20f)
    {
        LayerMask HighPolyWithTerrain = LayerMaskClass.HighPolyWithTerrainMask;
        LayerMask DoorLayer = LayerMaskClass.DoorLayer;
        LayerMask Mask = HighPolyWithTerrain & ~(1 << DoorLayer);
        queryParams = new(Mask, false, QueryTriggerInteraction.Ignore);
    }

    // ref: AUD-09-04 - Estrutura para rastreamento de fatias contíguas por inimigo no lote único
    private struct PathTargetSlice
    {
        public Enemy Enemy;
        public int Offset;
        public int Count;
    }

    private readonly List<PathTargetSlice> _targetsThisFrame = [];
    private NativeArray<RaycastCommand> _commandsBuffer;
    private NativeArray<RaycastHit> _hitsBuffer;
    private int _bufferCapacity = 0;
    private JobHandle _batchHandle;

    private void EnsureCapacity(int required)
    {
        if (_bufferCapacity >= required && _commandsBuffer.IsCreated && _hitsBuffer.IsCreated)
        {
            return;
        }

        DisposeBuffers();

        _bufferCapacity = Mathf.Max(required + 128, 256);
        _commandsBuffer = new NativeArray<RaycastCommand>(_bufferCapacity, Allocator.Persistent);
        _hitsBuffer = new NativeArray<RaycastHit>(_bufferCapacity, Allocator.Persistent);
    }

    private void DisposeBuffers()
    {
        if (_commandsBuffer.IsCreated) _commandsBuffer.Dispose();
        if (_hitsBuffer.IsCreated) _hitsBuffer.Dispose();
    }

    protected override IEnumerator PrimaryFunction()
    {
        HashSet<BotComponent> bots = SAINBotController.BotSpawnController.SAINBots;
        CalcEnemyPaths(bots);
        yield return null;

        // ref: AUD-09-04 - Despacho consolidado de lote único (Zero-Alloc & 1 ScheduleBatch por ciclo)
        int totalCommands = PrepareCommands(bots);
        if (totalCommands > 0)
        {
            var commandsSlice = _commandsBuffer.GetSubArray(0, totalCommands);
            var hitsSlice = _hitsBuffer.GetSubArray(0, totalCommands);

            _batchHandle = RaycastCommand.ScheduleBatch(commandsSlice, hitsSlice, _commandsPerJob);
            yield return null;

            if (!_batchHandle.IsCompleted)
            {
                _batchHandle.Complete();
            }

            ProcessResults(hitsSlice);
        }
    }

    private static void ScheduleJobs(List<PathVisionJob> jobs, int minCommandsPerJob = 256)
    {
        for (int i = 0; i < jobs.Count; i++)
        {
            PathVisionJob job = jobs[i];
            job.Handle = RaycastCommand.ScheduleBatch(job.Commands, job.Hits, minCommandsPerJob);
            jobs[i] = job;
        }
    }

    private void CreateJobs(HashSet<BotComponent> bots)
    {
        float currentTime = Time.time;
        foreach (BotComponent bot in bots)
        {
            if (bot != null && bot.SAINLayersActive)
            {
                EnemyList knownEnemies = bot.EnemyController.KnownEnemies;
                if (knownEnemies.Count > 0)
                {
                    //Vector3 botPosition = bot.Transform.Position;
                    //Vector3 eyePosition = bot.Transform.EyePosition;
                    //Vector3 neutralViewPosition = new(botPosition.x, eyePosition.y, botPosition.z);
                    Vector3 neutralViewPosition = bot.Transform.WeaponRoot;
                    foreach (Enemy enemy in bot.EnemyController.KnownEnemies)
                    {
                        if (enemy.IsVisible)
                        {
                            enemy.SetLastCornerAsVisiblePathPoint(enemy.EnemyPosition);
                            continue;
                        }
                        if (enemy.Path.ShallCheckPathVision(currentTime, neutralViewPosition))
                        {
                            int nodeCount = enemy.Path.AllPathNodeCount;
                            if (nodeCount > 0)
                            {
                                VisionJobs.Add(new(enemy.Path.AllPathNodes, nodeCount, neutralViewPosition, enemy, queryParams));
                                continue;
                            }
                            //if (enemy.Path.PathCorners.Length > 1)
                            //{
                            //    enemy.SetLastCornerAsVisiblePathPoint(enemy.Path.PathCorners[1], 1);
                            //    continue;
                            //}
                            enemy.ClearVisiblePathPoint();
                        }
                    }
                }
            }
        }
    }

    private void ScheduleShootCommands()
    {
        for (int i = 0; i < VisionJobs.Count; i++)
        {
            PathVisionJob job = VisionJobs[i];
            if (!job.Handle.IsCompleted)
            {
                job.Handle.Complete();
            }

            Enemy enemy = job.Enemy;
            if (enemy.EnemyKnown)
            {
                var Hits = job.Hits;
                //int lastVisibleIndex = 0;
                var nodes = enemy.Path.AllPathNodes;
                var visibleNodes = enemy.Path.VisibleNodes;
                visibleNodes.Clear();
                for (int j = 0; j < Hits.Length; j++)
                {
                    BotVisiblePathNode node = nodes[j];
                    if (Hits[j].collider == null)
                    {
                        node.Visible = true;
                        visibleNodes.Add(node);
                    }
                    else
                    {
                        node.Visible = false;
                    }
                    nodes[j] = node;
                }
                if (visibleNodes.Count == 0)
                {
                    if (enemy.Path.PathCorners.Length > 1)
                    {
                        enemy.SetLastCornerAsVisiblePathPoint(enemy.Path.PathCorners[1]);
                    }
                    else
                    {
                        //Logger.LogDebug($"[{enemy.Bot.name}] No visible path point found for enemy {enemy.EnemyName}");
                        enemy.ClearVisiblePathPoint();
                    }
                }
                else
                {
                    ShootJobs.Add(new(visibleNodes, enemy.Bot.Transform.WeaponRoot, enemy, queryParams));
                }
            }
            job.Hits.Dispose();
            job.Commands.Dispose();
        }
        VisionJobs.Clear();
    }

    private void CheckVisionCommandsTest()
    {
        for (int i = 0; i < VisionJobs.Count; i++)
        {
            PathVisionJob job = VisionJobs[i];
            if (!job.Handle.IsCompleted)
            {
                job.Handle.Complete();
            }

            Enemy enemy = job.Enemy;
            if (enemy.EnemyKnown)
            {
                var Hits = job.Hits;
                //int lastVisibleIndex = 0;
                var nodes = enemy.Path.AllPathNodes;
                bool pointFound = false;
                for (int j = Hits.Length - 1; j >= 0; j--)
                {
                    BotVisiblePathNode node = nodes[j];
                    if (Hits[j].collider == null)
                    {
                        enemy.SetLastVisiblePathPoint(node);
                        pointFound = true;
                        break;
                    }
                }
                if (!pointFound)
                {
#if DEBUG
                    Logger.LogDebug($"[{enemy.Bot.name}] No visible path point found for enemy {enemy.EnemyName}");
#endif
                    enemy.ClearVisiblePathPoint();
                    //if (enemy.Path.PathCorners.Length > 1)
                    //{
                    //    enemy.SetLastCornerAsVisiblePathPoint(enemy.Path.PathCorners[1], 1);
                    //}
                    //else
                    //{
                    //    //Logger.LogDebug($"[{enemy.Bot.name}] No visible path point found for enemy {enemy.EnemyName}");
                    //    enemy.ClearVisiblePathPoint();
                    //}
                }
            }
            job.Hits.Dispose();
            job.Commands.Dispose();
        }
        VisionJobs.Clear();
    }

    private void ReadResults()
    {
        for (int i = 0; i < ShootJobs.Count; i++)
        {
            PathVisionJob job = ShootJobs[i];
            if (!job.Handle.IsCompleted)
            {
                job.Handle.Complete();
            }

            NativeArray<RaycastHit> hits = job.Hits;
            Enemy enemy = job.Enemy;
            if (enemy.EnemyKnown)
            {
                var visibleNodes = enemy.Path.VisibleNodes;
                bool PointFound = false;
                for (int j = hits.Length - 1; j >= 0; j--)
                {
                    BotVisiblePathNode node = visibleNodes[j];
                    if (hits[j].collider == null)
                    {
                        enemy.SetLastVisiblePathPoint(node);
                        PointFound = true;
                        break;
                    }
                }
                if (!PointFound)
                {
                    if (visibleNodes.Count > 0)
                    {
                        enemy.SetLastVisiblePathPoint(visibleNodes[visibleNodes.Count - 1]);
                    }
                    else
                    {
                        enemy.ClearVisiblePathPoint();
                        //Logger.LogDebug($"[{enemy.Bot.name}] No shootable path point found for enemy {enemy.EnemyName}");
                    }
                }
            }
            job.Hits.Dispose();
            job.Commands.Dispose();
        }
        ShootJobs.Clear();
    }

    private static void CalcEnemyPaths(HashSet<BotComponent> bots)
    {
        PathVisibilityConfig config = new(GlobalSettingsClass.Instance.Steering);
        float currentTime = Time.time;
        foreach (BotComponent bot in bots)
        {
            if (bot != null && bot.SAINLayersActive)
            {
                foreach (Enemy enemy in bot.EnemyController.KnownEnemies)
                {
                    enemy.Path.CheckCalcPath(config, currentTime);
                }
            }
        }
    }

    protected override bool CanProceed()
    {
        var bots = SAINBotController?.BotSpawnController?.SAINBots;
        return bots != null && bots.Count > 0;
    }

    protected override bool LoopCondition()
    {
        return SAINGameWorld != null;
    }

    private int PrepareCommands(HashSet<BotComponent> bots)
    {
        float currentTime = Time.time;
        _targetsThisFrame.Clear();

        int totalCommands = 0;
        foreach (BotComponent bot in bots)
        {
            if (bot != null && bot.SAINLayersActive)
            {
                // ref: AUD-09-04 - Bots distantes em Tier 2 checam rota com frequência reduzida (interleaved a cada 200ms)
                if (bot.CurrentLodTier == 2 && !bot.IsLODTier0 && (Time.frameCount + (bot.BotOwner?.Id ?? 0)) % 4 != 0)
                {
                    continue;
                }

                EnemyList knownEnemies = bot.EnemyController.KnownEnemies;
                if (knownEnemies.Count > 0)
                {
                    Vector3 neutralViewPosition = bot.Transform.WeaponRoot;
                    foreach (Enemy enemy in knownEnemies)
                    {
                        if (enemy.IsVisible)
                        {
                            enemy.SetLastCornerAsVisiblePathPoint(enemy.EnemyPosition);
                            continue;
                        }
                        if (enemy.Path.ShallCheckPathVision(currentTime, neutralViewPosition))
                        {
                            int nodeCount = enemy.Path.AllPathNodeCount;
                            if (nodeCount > 0)
                            {
                                totalCommands += nodeCount;
                            }
                            else
                            {
                                enemy.ClearVisiblePathPoint();
                            }
                        }
                    }
                }
            }
        }

        if (totalCommands == 0)
        {
            return 0;
        }

        EnsureCapacity(totalCommands);

        int currentOffset = 0;
        foreach (BotComponent bot in bots)
        {
            if (bot != null && bot.SAINLayersActive)
            {
                if (bot.CurrentLodTier == 2 && !bot.IsLODTier0 && (Time.frameCount + (bot.BotOwner?.Id ?? 0)) % 4 != 0)
                {
                    continue;
                }

                EnemyList knownEnemies = bot.EnemyController.KnownEnemies;
                if (knownEnemies.Count > 0)
                {
                    Vector3 neutralViewPosition = bot.Transform.WeaponRoot;
                    foreach (Enemy enemy in knownEnemies)
                    {
                        if (enemy.IsVisible)
                        {
                            continue;
                        }
                        if (enemy.Path.ShallCheckPathVision(currentTime, neutralViewPosition))
                        {
                            int nodeCount = enemy.Path.AllPathNodeCount;
                            if (nodeCount > 0)
                            {
                                var nodes = enemy.Path.AllPathNodes;
                                for (int i = 0; i < nodeCount; i++)
                                {
                                    _commandsBuffer[currentOffset + i] = new RaycastCommand(
                                        neutralViewPosition,
                                        nodes[i].Point - neutralViewPosition,
                                        queryParams,
                                        1f
                                    );
                                }

                                _targetsThisFrame.Add(new PathTargetSlice
                                {
                                    Enemy = enemy,
                                    Offset = currentOffset,
                                    Count = nodeCount
                                });

                                currentOffset += nodeCount;
                            }
                        }
                    }
                }
            }
        }

        return currentOffset;
    }

    private void ProcessResults(NativeArray<RaycastHit> hitsSlice)
    {
        for (int i = 0; i < _targetsThisFrame.Count; i++)
        {
            PathTargetSlice target = _targetsThisFrame[i];
            Enemy enemy = target.Enemy;
            if (enemy.EnemyKnown)
            {
                var nodes = enemy.Path.AllPathNodes;
                bool pointFound = false;
                for (int j = target.Count - 1; j >= 0; j--)
                {
                    int hitIndex = target.Offset + j;
                    BotVisiblePathNode node = nodes[j];
                    if (hitsSlice[hitIndex].collider == null)
                    {
                        enemy.SetLastVisiblePathPoint(node);
                        pointFound = true;
                        break;
                    }
                }
                if (!pointFound)
                {
#if DEBUG
                    Logger.LogDebug($"[{enemy.Bot.name}] No visible path point found for enemy {enemy.EnemyName}");
#endif
                    enemy.ClearVisiblePathPoint();
                }
            }
        }
        _targetsThisFrame.Clear();
    }

    public override void Stop()
    {
        Dispose();
        base.Stop();
    }

    public void Dispose()
    {
        if (!_batchHandle.IsCompleted)
        {
            _batchHandle.Complete();
        }

        DisposeBuffers();
        _targetsThisFrame.Clear();

        foreach (var Job in VisionJobs)
        {
            Job.Dispose();
        }

        VisionJobs.Clear();
        foreach (var job in ShootJobs)
        {
            job.Dispose();
        }

        ShootJobs.Clear();
    }
}
