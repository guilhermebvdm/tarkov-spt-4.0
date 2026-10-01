using System;
using System.Collections;
using System.Collections.Generic;
using EFT;
using SAIN.Components.PlayerComponentSpace;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.Types.Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components;

public class FlashlightRaycastJob : SainJobTemplate, IDisposable
{
    private const float LaserTraceDistance = 75;

    private const float Wide_FlashLightBeamAngle = 16f;
    private const int Wide_FlashlightBeamPointCount = 32;
    private const float Wide_FlashlightTraceDistance = 30;

    private const float Tight_FlashLightBeamAngle = 8f;
    private const int Tight_FlashlightBeamPointCount = 16;
    private const float Tight_FlashlightTraceDistance = 60;

    public FlashlightRaycastJob(MonoBehaviour gameWorld)
        : base("Flashlight Detection Job", gameWorld, true, 0.1f)
    {
        GenerateRandomYawPitchRotationsNonAlloc(_rotationsList_Wide, Wide_FlashlightBeamPointCount, Wide_FlashLightBeamAngle);
        GenerateRandomYawPitchRotationsNonAlloc(_rotationsList_Tight, Tight_FlashlightBeamPointCount, Tight_FlashLightBeamAngle);
    }

    protected readonly List<RaycastJob> RaycastJobs = [];
    protected readonly List<Quaternion> _rotationsList_Wide = [];
    protected readonly List<Quaternion> _rotationsList_Tight = [];

    private struct FlashlightSlice
    {
        public PlayerComponent Player;
        public int Offset;
        public int Count;
    }

    private struct DetectionSlice
    {
        public PlayerComponent Player;
        public IPlayer Target;
        public int Offset;
        public int Count;
    }

    private readonly List<FlashlightSlice> _flashlightSlices = [];
    private readonly List<DetectionSlice> _detectionSlices = [];
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

        _bufferCapacity = Mathf.Max(required + 64, 128);
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
        // ref: AUD-09-07 - Fase 1: Traçado de Fachos de Luz (Zero-Alloc)
        int totalFlashlightCommands = CreateFlashlightCommands();
        if (totalFlashlightCommands > 0)
        {
            var commandsSlice = _commandsBuffer.GetSubArray(0, totalFlashlightCommands);
            var hitsSlice = _hitsBuffer.GetSubArray(0, totalFlashlightCommands);

            _batchHandle = RaycastCommand.ScheduleBatch(commandsSlice, hitsSlice, 32);
            yield return null;

            if (!_batchHandle.IsCompleted)
            {
                _batchHandle.Complete();
            }

            ReadFlashlightResults(hitsSlice);

            // ref: AUD-09-07 - Fase 2: Detecção de Facho por Inimigos (Zero-Alloc)
            int totalDetectionCommands = CreateLightDetectionCommands();
            if (totalDetectionCommands > 0)
            {
                commandsSlice = _commandsBuffer.GetSubArray(0, totalDetectionCommands);
                hitsSlice = _hitsBuffer.GetSubArray(0, totalDetectionCommands);

                _batchHandle = RaycastCommand.ScheduleBatch(commandsSlice, hitsSlice, 32);
                yield return null;

                if (!_batchHandle.IsCompleted)
                {
                    _batchHandle.Complete();
                }

                ReadLightDetectionResults(hitsSlice);
            }
        }
    }

    private int CreateFlashlightCommands()
    {
        _flashlightSlices.Clear();
        List<RandomDir> directions = _directionsList;
        HashSet<PlayerComponent> players = GameWorldComponent.Instance?.PlayerTracker?.AlivePlayerArray;
        if (players == null || players.Count == 0)
        {
            return 0;
        }

        int total = 0;
        foreach (var player in players)
        {
            if (player != null && player.IsActive && player.Flashlight.DeviceActive)
            {
                int dirCount = 0;
                if (player.Flashlight.Laser || player.Flashlight.IRLaser)
                {
                    dirCount += 1;
                }
                if (player.Flashlight.WhiteLight || player.Flashlight.IRLight)
                {
                    dirCount += _rotationsList_Wide.Count + _rotationsList_Tight.Count;
                }
                total += dirCount;
            }
        }

        if (total == 0)
        {
            return 0;
        }

        EnsureCapacity(total);

        int currentOffset = 0;
        LayerMask mask = LayerMaskClass.HighPolyWithTerrainMaskAI;

        foreach (var player in players)
        {
            if (player != null && player.IsActive && player.Flashlight.DeviceActive)
            {
                directions.Clear();
                Vector3 weaponPointDir = player.Transform.WeaponData.PointDirection;
                if (player.Flashlight.Laser || player.Flashlight.IRLaser)
                {
                    directions.Add(new(LaserTraceDistance, weaponPointDir));
                }
                if (player.Flashlight.WhiteLight || player.Flashlight.IRLight)
                {
                    CreateFlashlightBeam(directions, _rotationsList_Wide, weaponPointDir, Wide_FlashlightTraceDistance);
                    CreateFlashlightBeam(directions, _rotationsList_Tight, weaponPointDir, Tight_FlashlightTraceDistance);
                }

                int count = directions.Count;
                if (count > 0)
                {
                    Vector3 origin = player.Transform.WeaponData.PointDirection;
                    for (int i = 0; i < count; i++)
                    {
                        _commandsBuffer[currentOffset + i] = new RaycastCommand(
                            origin,
                            directions[i].DirectionNormal,
                            new QueryParameters { layerMask = mask },
                            directions[i].Magnitude
                        );
                    }

                    _flashlightSlices.Add(new FlashlightSlice
                    {
                        Player = player,
                        Offset = currentOffset,
                        Count = count
                    });

                    currentOffset += count;
                    directions.Clear();
                }
            }
        }

        return currentOffset;
    }

    private void ReadFlashlightResults(NativeArray<RaycastHit> hitsSlice)
    {
        for (int i = 0; i < _flashlightSlices.Count; i++)
        {
            FlashlightSlice slice = _flashlightSlices[i];
            PlayerComponent player = slice.Player;
            if (player != null)
            {
                List<Vector3> lightPoints = player.Flashlight.LightDetection.LightPoints;
                lightPoints.Clear();
                for (int j = slice.Count - 1; j >= 0; j--)
                {
                    RaycastHit hit = hitsSlice[slice.Offset + j];
                    if (hit.collider != null)
                    {
                        lightPoints.Add(hit.point + (hit.normal * 0.05f));
                    }
                }
            }
        }
        _flashlightSlices.Clear();
    }

    private int CreateLightDetectionCommands()
    {
        _detectionSlices.Clear();
        var aliveBots = AliveBots;
        if (aliveBots == null || aliveBots.Count == 0)
        {
            return 0;
        }

        int total = 0;
        foreach (BotComponent bot in aliveBots.Values)
        {
            if (bot != null && bot.BotActive)
            {
                foreach (Enemy enemy in bot.EnemyController.Enemies.Values)
                {
                    if (enemy != null && enemy.PlayerComponent.IsActive)
                    {
                        FlashLightClass enemyLight = enemy.EnemyPlayerComponent.Flashlight;
                        if (
                            enemyLight.DeviceActive
                            && bot.PlayerComponent.Flashlight.LightDetection.CheckIsBeamVisible(enemyLight)
                            && enemy.RealDistance <= 125f
                        )
                        {
                            total += enemyLight.LightDetection.LightPoints.Count;
                        }
                    }
                }
            }
        }

        if (total == 0)
        {
            return 0;
        }

        EnsureCapacity(total);

        int currentOffset = 0;
        LayerMask mask = LayerMaskClass.HighPolyWithTerrainMaskAI;

        foreach (BotComponent bot in aliveBots.Values)
        {
            if (bot != null && bot.BotActive)
            {
                foreach (Enemy enemy in bot.EnemyController.Enemies.Values)
                {
                    if (enemy != null && enemy.PlayerComponent.IsActive)
                    {
                        FlashLightClass enemyLight = enemy.EnemyPlayerComponent.Flashlight;
                        if (
                            enemyLight.DeviceActive
                            && bot.PlayerComponent.Flashlight.LightDetection.CheckIsBeamVisible(enemyLight)
                            && enemy.RealDistance <= 125f
                        )
                        {
                            var points = enemyLight.LightDetection.LightPoints;
                            int count = points.Count;
                            if (count > 0)
                            {
                                Vector3 eyePosition = bot.Transform.EyePosition;
                                for (int i = 0; i < count; i++)
                                {
                                    Vector3 pt = points[i];
                                    _commandsBuffer[currentOffset + i] = new RaycastCommand(
                                        eyePosition,
                                        pt - eyePosition,
                                        new QueryParameters { layerMask = mask },
                                        1f
                                    );
                                }

                                _detectionSlices.Add(new DetectionSlice
                                {
                                    Player = bot.PlayerComponent,
                                    Target = enemy.Player,
                                    Offset = currentOffset,
                                    Count = count
                                });

                                currentOffset += count;
                            }
                        }
                    }
                }
            }
        }

        return currentOffset;
    }

    private void ReadLightDetectionResults(NativeArray<RaycastHit> hitsSlice)
    {
        for (int i = 0; i < _detectionSlices.Count; i++)
        {
            DetectionSlice slice = _detectionSlices[i];
            PlayerComponent player = slice.Player;
            if (player != null)
            {
                bool visiblePoint = false;
                for (int j = 0; j < slice.Count; j++)
                {
                    if (hitsSlice[slice.Offset + j].collider == null)
                    {
                        visiblePoint = true;
                        break;
                    }
                }

                if (visiblePoint && slice.Target != null)
                {
                    player.Flashlight.LightDetection.TryToInvestigate(slice.Target);
                }
            }
        }
        _detectionSlices.Clear();
    }

    private readonly List<RandomDir> _directionsList = [];

    private void ScheduleJobs(int Total)
    {
        for (int i = 0; i < Total; i++)
        {
            RaycastJobs[i].Schedule();
        }
    }

    /// <summary>
    /// Generates a list of random rotations with given angle.
    /// </summary>
    /// <param name="count">Number of quaternions to generate.</param>
    /// <param name="maxYaw">Max yaw in degrees (horizontal rotation around Y).</param>
    /// <param name="maxPitch">Max pitch in degrees (vertical rotation around right axis).</param>
    /// <returns>List of Quaternion rotations.</returns>
    public static void GenerateRandomYawPitchRotationsNonAlloc(List<Quaternion> nonAllocList, int count, float coneAngle)
    {
        for (int i = 0; i < count; i++)
        {
            float yaw = UnityEngine.Random.Range(-coneAngle, coneAngle); // Y axis
            float pitch = UnityEngine.Random.Range(-coneAngle, coneAngle); // X axis
            float roll = UnityEngine.Random.Range(-coneAngle, coneAngle); // Z axis

            nonAllocList.Add(Quaternion.Euler(pitch, yaw, roll)); // (X, Y, Z) = (Pitch, Yaw, Roll)
        }
    }

    private static void CreateFlashlightBeam(
        List<RandomDir> beamDirections,
        List<Quaternion> rotationsList,
        Vector3 weaponPointDir,
        float distance
    )
    {
        for (int i = 0; i < rotationsList.Count; i++)
        {
            beamDirections.Add(new(distance, (rotationsList[i] * weaponPointDir).normalized));
        }
    }

    protected static RandomDir[] GenerateRandomDirections(int Count, float LengthMin, float LengthMax)
    {
        RandomDir[] Result = new RandomDir[Count];
        for (int i = 0; i < Count; i++)
        {
            Result[i] = new RandomDir(LengthMin, LengthMax);
        }
        return Result;
    }

    protected override bool CanProceed()
    {
        var bots = SAINBotController?.BotSpawnController?.BotDictionary;
        return bots != null && bots.Count > 0;
    }

    protected override bool LoopCondition()
    {
        return SAINGameWorld != null;
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
        _flashlightSlices.Clear();
        _detectionSlices.Clear();

        foreach (RaycastJob Job in RaycastJobs)
        {
            Job.Dispose();
        }

        RaycastJobs.Clear();
    }
}
