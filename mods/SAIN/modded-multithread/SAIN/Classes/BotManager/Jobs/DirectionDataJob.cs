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
        int count = OtherPlayers.Count;

        // ref: AUD-09-03 / CR-02-02 - Buffer nativo persistente redimensionado sob demanda com guarda count > 0
        if (count > 0 && (!OtherPlayerDirectionData.IsCreated || OtherPlayerDirectionData.Length != count))
        {
            if (OtherPlayerDirectionData.IsCreated)
            {
                OtherPlayerDirectionData.Dispose();
            }
            OtherPlayerDirectionData = new NativeArray<PlayerDirectionData>(count, Allocator.Persistent);
        }
        else if (count == 0 && OtherPlayerDirectionData.IsCreated)
        {
            OtherPlayerDirectionData.Dispose();
            OtherPlayerDirectionData = default;
        }

        for (int j = 0; j < count; j++)
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
        // ref: bugfix - OtherPlayerData pode ter sido limpo por Dispose() de outro player
        // (morte/despawn/extracao) enquanto este job estava em voo por 1 frame; revalidar
        // contra o tamanho atual da lista evita ArgumentOutOfRangeException em List.get_Item.
        int count = Mathf.Min(OtherPlayerDirectionData.Length, OtherPlayerData.Count);
        for (int i = 0; i < count; i++)
        {
            OtherPlayerData[i].DistanceData.SetPlayerDirectionData(OtherPlayerDirectionData[i]);
        }
        OtherPlayerData.Clear();
    }

    public void Dispose()
    {
        if (OtherPlayerDirectionData.IsCreated)
        {
            OtherPlayerDirectionData.Dispose();
        }
        OtherPlayerData.Clear();
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

    // ref: AUD-09-03 - Buffers nativos persistentes para PlayerTickJob (Zero-Alloc)
    private NativeArray<PlayerTickData> _inputBuffer;
    private NativeArray<PlayerTickData> _outputBuffer;
    private int _bufferCapacity = 0;

    private void EnsureCapacity(int required)
    {
        if (_bufferCapacity >= required && _inputBuffer.IsCreated && _outputBuffer.IsCreated)
        {
            return;
        }

        DisposeBuffers();

        _bufferCapacity = Mathf.Max(required + 16, 32);
        _inputBuffer = new NativeArray<PlayerTickData>(_bufferCapacity, Allocator.Persistent);
        _outputBuffer = new NativeArray<PlayerTickData>(_bufferCapacity, Allocator.Persistent);
    }

    private void DisposeBuffers()
    {
        if (_inputBuffer.IsCreated) _inputBuffer.Dispose();
        if (_outputBuffer.IsCreated) _outputBuffer.Dispose();
    }

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
                EnsureCapacity(jobCount);

                var inputSlice = _inputBuffer.GetSubArray(0, jobCount);
                var outputSlice = _outputBuffer.GetSubArray(0, jobCount);

                for (int i = 0; i < jobCount; i++)
                {
                    inputSlice[i] = _playerTickData[i];
                }

                _PlayerTickJob = new()
                {
                    Input = inputSlice,
                    Output = outputSlice,
                };

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
                    PlayerTickData data = outputSlice[i];

                    // ref: bugfix - dono do dado pode ter sido destruido (morte/despawn/extracao)
                    // durante o yield return null em que o job ficou em voo; Unity retorna
                    // "fake null" apos Destroy(), entao este check e seguro mesmo antes da
                    // destruicao nativa acontecer no fim do frame.
                    if (data.currentOwner == null)
                    {
                        continue;
                    }

                    data.ReadData();
                    data.currentOwner.SetTickData(data);
                }
                _playerTickData.Clear();
            }
            yield return null;
        }
    }

    public void Dispose()
    {
        if (!_PlayerTickJobHandle.IsCompleted)
        {
            _PlayerTickJobHandle.Complete();
        }
        DisposeBuffers();
    }
}
