using System.Collections;
using System.Collections.Generic;
using SAIN.SAINComponent.Classes.EnemyClasses;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components;

public class EnemyPlaceRaycastJob : BotManagerBase
{
    public struct CalcEnemyPlaceJob : IJobFor
    {
        [ReadOnly]
        public NativeArray<Vector3> PlacePositions;

        [ReadOnly]
        public NativeArray<Vector3> BotPositions;

        [ReadOnly]
        public NativeArray<Vector3> EnemyPositions;

        [WriteOnly]
        public NativeArray<float> PlaceDistancesToBot;

        [WriteOnly]
        public NativeArray<float> PlaceDistancesToEnemy;

        public void Execute(int index)
        {
            Vector3 EnemyPlace = PlacePositions[index];
            Vector3 BotPosition = BotPositions[index];
            Vector3 EnemyPosition = EnemyPositions[index];
            PlaceDistancesToBot[index] = (BotPosition - EnemyPlace).magnitude;
            PlaceDistancesToEnemy[index] = (EnemyPosition - EnemyPlace).magnitude;
        }

        public void Dispose()
        {
            // ref: CR-02-01 - Memória nativa gerenciada exclusivamente pelos buffers persistentes de EnemyPlaceRaycastJob (fatias GetSubArray)
        }
    }

    public EnemyPlaceRaycastJob(BotManagerComponent botcontroller)
        : base(botcontroller)
    {
        botcontroller.StartCoroutine(EnemyPlaceJobLoop());
    }

    private JobHandle EnemyPlaceJobHandle;
    private CalcEnemyPlaceJob EnemyPlaceJob;
    private readonly List<EnemyPlace> PlacesToCheck = new();

    private IEnumerator EnemyPlaceJobLoop()
    {
        yield return null;

        WaitForSeconds wait = new(0.1f);
        while (true)
        {
            yield return wait;

            if (BotController == null)
            {
                continue;
            }

            var bots = BotController.BotSpawnController?.SAINBots;
            if (bots == null || bots.Count == 0)
            {
                continue;
            }

            if (BotController.BotGame?.Status == EFT.GameStatus.Stopping)
            {
                continue;
            }

            PlacesToCheck.Clear();
            foreach (BotComponent bot in bots)
            {
                if (bot?.BotActive == true)
                {
                    foreach (Enemy enemy in bot.EnemyController.EnemiesArray)
                    {
                        if (enemy?.EnemyKnown == true)
                        {
                            if (enemy.KnownPlaces.LastHeardPlace != null)
                            {
                                PlacesToCheck.Add(enemy.KnownPlaces.LastHeardPlace);
                            }

                            if (enemy.KnownPlaces.LastSeenPlace != null)
                            {
                                PlacesToCheck.Add(enemy.KnownPlaces.LastSeenPlace);
                            }

                            if (enemy.KnownPlaces.LastSquadHeardPlace != null)
                            {
                                PlacesToCheck.Add(enemy.KnownPlaces.LastSquadHeardPlace);
                            }

                            if (enemy.KnownPlaces.LastSquadSeenPlace != null)
                            {
                                PlacesToCheck.Add(enemy.KnownPlaces.LastSquadSeenPlace);
                            }
                        }
                    }
                }
            }
            int Count = PlacesToCheck.Count;
            if (Count == 0)
            {
                continue;
            }

            // ref: AUD-09-02 - Buffers nativos persistentes reutilizados via GetSubArray (Zero-Alloc)
            EnsureCapacity(Count);

            var placePositions = _placePositions.GetSubArray(0, Count);
            var botPositions = _botPositions.GetSubArray(0, Count);
            var enemyPositions = _enemyPositions.GetSubArray(0, Count);
            var distToBot = _placeDistancesToBot.GetSubArray(0, Count);
            var distToEnemy = _placeDistancesToEnemy.GetSubArray(0, Count);
            var commands = _commands.GetSubArray(0, Count);
            var hits = _hits.GetSubArray(0, Count);

            for (int i = 0; i < Count; i++)
            {
                EnemyPlace Place = PlacesToCheck[i];
                placePositions[i] = Place.Position;
                botPositions[i] = Place.PlaceData.Owner.Transform.EyePosition;
                enemyPositions[i] = Place.PlaceData.OwnerEnemy.EnemyTransform.Position;
            }

            EnemyPlaceJob = new CalcEnemyPlaceJob
            {
                PlacePositions = placePositions,
                BotPositions = botPositions,
                EnemyPositions = enemyPositions,
                PlaceDistancesToBot = distToBot,
                PlaceDistancesToEnemy = distToEnemy,
            };

            EnemyPlaceJobHandle = EnemyPlaceJob.Schedule(Count, new JobHandle());

            for (int i = 0; i < Count; i++)
            {
                EnemyPlace Place = PlacesToCheck[i];
                Vector3 HeadPosition = Place.PlaceData.Owner.Transform.EyePosition;
                Vector3 PlacePosition = Place.Position + Vector3.up;
                commands[i] = new RaycastCommand(HeadPosition, PlacePosition - HeadPosition, new QueryParameters { layerMask = Mask }, 1f);
            }

            RaycastJobHandle = RaycastCommand.ScheduleBatch(commands, hits, 32);

            yield return null;

            var handle = RaycastJobHandle;
            if (!handle.IsCompleted)
            {
                handle.Complete();
            }

            RaycastJobHandle = handle;

            handle = EnemyPlaceJobHandle;
            if (!handle.IsCompleted)
            {
                handle.Complete();
            }

            EnemyPlaceJobHandle = handle;

            for (int i = 0; i < Count; i++)
            {
                EnemyPlace Place = PlacesToCheck[i];
                if (Place != null)
                {
                    RaycastHit Hit = hits[i];
                    Place.SetDistances(distToBot[i], distToEnemy[i], Place.PlaceData.Owner);
                    Place.SetVisibilityOfPlace(Hit.collider == null, Place.PlaceData.Owner);
                }
            }

            PlacesToCheck.Clear();
        }
    }

    public void Dispose()
    {
        if (!RaycastJobHandle.IsCompleted)
        {
            RaycastJobHandle.Complete();
        }

        if (!EnemyPlaceJobHandle.IsCompleted)
        {
            EnemyPlaceJobHandle.Complete();
        }

        DisposeBuffers();
    }

    private NativeArray<Vector3> _placePositions;
    private NativeArray<Vector3> _botPositions;
    private NativeArray<Vector3> _enemyPositions;
    private NativeArray<float> _placeDistancesToBot;
    private NativeArray<float> _placeDistancesToEnemy;
    private NativeArray<RaycastHit> _hits;
    private NativeArray<RaycastCommand> _commands;
    private int _bufferCapacity = 0;

    private void EnsureCapacity(int required)
    {
        if (_bufferCapacity >= required && _placePositions.IsCreated)
        {
            return;
        }

        DisposeBuffers();

        _bufferCapacity = Mathf.Max(required + 32, 64);
        _placePositions = new NativeArray<Vector3>(_bufferCapacity, Allocator.Persistent);
        _botPositions = new NativeArray<Vector3>(_bufferCapacity, Allocator.Persistent);
        _enemyPositions = new NativeArray<Vector3>(_bufferCapacity, Allocator.Persistent);
        _placeDistancesToBot = new NativeArray<float>(_bufferCapacity, Allocator.Persistent);
        _placeDistancesToEnemy = new NativeArray<float>(_bufferCapacity, Allocator.Persistent);
        _commands = new NativeArray<RaycastCommand>(_bufferCapacity, Allocator.Persistent);
        _hits = new NativeArray<RaycastHit>(_bufferCapacity, Allocator.Persistent);
    }

    private void DisposeBuffers()
    {
        if (_placePositions.IsCreated) _placePositions.Dispose();
        if (_botPositions.IsCreated) _botPositions.Dispose();
        if (_enemyPositions.IsCreated) _enemyPositions.Dispose();
        if (_placeDistancesToBot.IsCreated) _placeDistancesToBot.Dispose();
        if (_placeDistancesToEnemy.IsCreated) _placeDistancesToEnemy.Dispose();
        if (_commands.IsCreated) _commands.Dispose();
        if (_hits.IsCreated) _hits.Dispose();
    }

    private JobHandle RaycastJobHandle;
    private readonly LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMaskAI;
}
