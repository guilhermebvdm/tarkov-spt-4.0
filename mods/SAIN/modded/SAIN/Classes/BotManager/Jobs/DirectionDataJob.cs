using System.Collections;
using System.Collections.Generic;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Extensions;
using SAIN.Models.PlayerData;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts;

public struct PlayerTickData(PlayerComponent inOwner)
{
    public readonly PlayerComponent currentOwner = inOwner;
    public readonly string OwnerProfileId = inOwner.ProfileId;
    public Vector3 OwnerViewPosition = inOwner.Transform.EyePosition;
    public Vector3 OwnerPosition = inOwner.Position;
    public Vector3 OwnerLookDirection = inOwner.LookDirection;

    public List<OtherPlayerData> OtherPlayerData = [];
    public NativeArray<PlayerDirectionData> OtherPlayerDirectionData = [];

    public void Prepare(PlayerComponent Owner)
    {
        OwnerViewPosition = Owner.Transform.EyePosition;
        OwnerPosition = Owner.Position;
        OwnerLookDirection = Owner.LookDirection;

        OtherPlayerData.Clear();
        List<OtherPlayerData> OtherPlayers = Owner.OtherPlayersData.DataList;
        OtherPlayerDirectionData = new NativeArray<PlayerDirectionData>(OtherPlayers.Count, Allocator.TempJob);
        for (int j = 0; j < OtherPlayers.Count; j++)
        {
            OtherPlayerData otherPlayer = OtherPlayers[j];
            OtherPlayerDirectionData[j] = otherPlayer.DistanceData.Data.GetUpdatedDirectionData(Owner, otherPlayer.OtherPlayerComponent);
            OtherPlayerData.Add(otherPlayer);
        }
    }

    public void Execute()
    {
        for (int i = 0; i < OtherPlayerDirectionData.Length; i++)
        {
            var data = OtherPlayerDirectionData[i];
            data.MainDirectionData.Update(OwnerPosition);
            data.MainDirectionData.UpdateDotProductAndCalcNormal(OwnerViewPosition, OwnerLookDirection);
            OtherPlayerDirectionData[i] = data;
        }
    }

    public void ReadData()
    {
        for (int i = 0; i < OtherPlayerDirectionData.Length; i++)
        {
            OtherPlayerData[i].DistanceData.SetPlayerDirectionData(OtherPlayerDirectionData[i]);
        }
        OtherPlayerData.Clear();
        OtherPlayerDirectionData.Dispose();
    }
}

public struct PlayerTickJob : IJobFor
{
    [ReadOnly]
    public NativeArray<PlayerTickData> Input;

    [WriteOnly]
    public NativeArray<PlayerTickData> Output;

    public void Execute(int index)
    {
        PlayerTickData Data = Input[index];
        Data.Execute();
        Output[index] = Data;
    }

    public void Dispose()
    {
        if (Input.IsCreated)
        {
            Input.Dispose();
        }

        if (Output.IsCreated)
        {
            Output.Dispose();
        }
    }
}

public class DirectionDataJob : BotManagerBase
{
    private JobHandle _PlayerTickJobHandle;
    private PlayerTickJob _PlayerTickJob;
    private readonly List<PlayerTickData> _playerTickData = [];

    public DirectionDataJob(BotManagerComponent botController)
        : base(botController)
    {
        botController.StartCoroutine(DirectionDataJobLoop());
    }

    private IEnumerator DirectionDataJobLoop()
    {
        yield return null;
        while (GameWorldComponent.Instance != null)
        {
            var players = GameWorldComponent.Instance.PlayerTracker?.AlivePlayerArray;
            if (players == null || players.Count <= 1)
            {
                yield return null;
                continue;
            }

            foreach (PlayerComponent playerComp in players)
            {
                if (playerComp != null && playerComp.OtherPlayersData != null)
                {
                    _playerTickData.Add(playerComp.GetPreparedTickData());
                }
            }

            int jobCount = _playerTickData.Count;
            if (jobCount > 0)
            {
                _PlayerTickJob = new()
                {
                    Input = new NativeArray<PlayerTickData>(jobCount, Allocator.TempJob),
                    Output = new NativeArray<PlayerTickData>(jobCount, Allocator.TempJob),
                };
                for (int i = 0; i < jobCount; i++)
                {
                    _PlayerTickJob.Input[i] = _playerTickData[i];
                }

                // schedule job and wait for next frame to read data
                _PlayerTickJobHandle = _PlayerTickJob.Schedule(jobCount, new JobHandle());

                yield return null;

                var handle = _PlayerTickJobHandle;
                if (!handle.IsCompleted)
                {
                    handle.Complete();
                }

                _PlayerTickJobHandle = handle;

                for (int i = 0; i < jobCount; i++)
                {
                    PlayerTickData data = _PlayerTickJob.Output[i];
                    data.ReadData();
                    data.currentOwner.SetTickData(data);
                }
                _PlayerTickJob.Dispose();
                _playerTickData.Clear();
            }
            yield return null;
        }
    }

    public void Dispose()
    {
        _PlayerTickJobHandle.Complete();
        _PlayerTickJob.Dispose();
    }
}
