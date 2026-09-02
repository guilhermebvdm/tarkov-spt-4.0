using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public class PlayerSpawnTracker
{
    public event Action<PlayerComponent> OnPlayerAdded;
    public event Action<string, PlayerComponent> OnPlayerRemoved;
    public readonly HashSet<PlayerComponent> AlivePlayerArray = [];
    public readonly Dictionary<string, PlayerComponent> AlivePlayersDictionary = [];
    public readonly List<IPlayer> DeadPlayers = [];

    public PlayerComponent GetPlayerComponent(IPlayer Player)
    {
        if (Player != null &&
            AlivePlayersDictionary.TryGetValue(Player.ProfileId, out PlayerComponent component))
        {
            return component;
        }
        return null;
    }

    public PlayerComponent GetPlayerComponent(string profileId)
    {
        if (!profileId.IsNullOrEmpty() &&
            AlivePlayersDictionary.TryGetValue(profileId, out PlayerComponent component))
        {
            return component;
        }
        return null;
    }

    public PlayerComponent FindClosestHumanPlayer(out float closestPlayerSqrMag, Vector3 targetPosition, out Player player)
    {
        PlayerComponent closestPlayer = null;
        closestPlayerSqrMag = float.MaxValue;
        player = null;

        foreach (var component in AlivePlayersDictionary.Values)
        {
            if (component != null &&
                component.Player != null &&
                !component.IsAI)
            {
                float sqrMag = (component.Position - targetPosition).sqrMagnitude;
                if (sqrMag < closestPlayerSqrMag)
                {
                    player = component.Player;
                    closestPlayer = component;
                    closestPlayerSqrMag = sqrMag;
                }
            }
        }
        return closestPlayer;
    }

    // ref: AUD-01-01 - Busca linear O(N) zero-alloc sem mutar a lista compartilhada de jogadores
    public PlayerComponent FindClosestHumanPlayer(out float distance, PlayerComponent quierrier, out Player player)
    {
        List<OtherPlayerData> otherPlayers = quierrier.OtherPlayersData.DataList;
        PlayerComponent closestHuman = null;
        float minDistance = float.MaxValue;
        player = null;

        for (int i = 0; i < otherPlayers.Count; i++)
        {
            OtherPlayerData otherPlayer = otherPlayers[i];
            // ref: AUD-13-01 - Null-safety em OtherPlayerComponent ao buscar jogador humano mais proximo
            var otherComp = otherPlayer?.OtherPlayerComponent;
            if (otherComp != null && !otherComp.IsAI)
            {
                float dist = otherPlayer.DistanceData.Distance;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestHuman = otherComp;
                    player = otherComp.Player;
                }
            }
        }

        distance = minDistance;
        return closestHuman;
    }

    public PlayerComponent AddPlayerManual(IPlayer player)
    {
        if (player == null)
        {
            return null;
        }
        //Logger.LogDebug($"Manually trying to recreate Player Component for [{player.Profile?.Nickname} : {player.ProfileId}]");
        AddPlayer(player);
        if (AlivePlayersDictionary.TryGetValue(player.ProfileId, out var component))
        {
#if DEBUG
            Logger.LogDebug($"Successfully created new Player Component for [{player.Profile?.Nickname} : {player.ProfileId}]");
#endif
            return component;
        }
        return null;
    }

    private void AddPlayer(IPlayer iPlayer)
    {
        var player = iPlayer as Player;
        if (player == null)
        {
#if DEBUG
            Logger.LogError("Could not add PlayerComponent for non-Player type or null Player.");
#endif
            return;
        }

        string profileId = player.ProfileId;
        if (TryRemove(profileId, out bool compDestroyed))
        {
#if DEBUG
            Logger.LogWarning($"PlayerComponent already exists for Player: {player.name} : {player.Profile?.Nickname} : {profileId}");
            if (compDestroyed)
            {
                Logger.LogWarning($"Destroyed old Component for: {player.name} : {player.Profile?.Nickname} : {profileId}");
            }
#endif
        }
        if (TryAddPlayerComponent(player))
        {
#if DEBUG
            Logger.LogDebug($"Added New Player [{player.name}] : [{player.Profile?.Nickname}]");
#endif
        }
    }

    private void RemovePerson(IPlayer player)
    {
        if (player == null)
        {
#if DEBUG
            Logger.LogError("Can't Remove player. Player Null");
#endif
            return;
        }
        if (TryRemove(player.ProfileId, out _))
        {
#if DEBUG
            Logger.LogDebug($"Removed Player Component [{player.Profile.Nickname}]");
#endif
        }
        else
        {
#if DEBUG
            Logger.LogWarning($"Could not find player [{player.Profile.Nickname}] in Player Component Dictionary!");
#endif
        }
        player.OnIPlayerDeadOrUnspawn -= RemovePerson;
    }

    public PlayerSpawnTracker(GameWorldComponent sainGameWorld)
    {
        _sainGameWorld = sainGameWorld;
        sainGameWorld.GameWorld.OnPersonAdd += AddPlayer;
    }

    public void Dispose()
    {
        if (_sainGameWorld != null)
        {
            var gameWorld = _sainGameWorld.GameWorld;
            if (gameWorld != null)
            {
                gameWorld.OnPersonAdd -= AddPlayer;
            }
        }
        foreach (var (_, player) in AlivePlayersDictionary)
        {
            if (player != null)
            {
                player.Dispose();
            }
        }
        AlivePlayersDictionary.Clear();
        // ref: AUD-01-01 / AUD-07-01 - Limpar conjuntos, listas e delegates no fim da raid
        AlivePlayerArray.Clear();
        DeadPlayers.Clear();
        _ids.Clear();
        OnPlayerAdded = null;
        OnPlayerRemoved = null;
    }

    private bool TryAddPlayerComponent(Player player)
    {
        PlayerComponent component = player.gameObject.AddComponent<PlayerComponent>();
        if (component != null && component.Init(player))
        {
            player.OnIPlayerDeadOrUnspawn += RemovePerson;
            AlivePlayersDictionary.Add(player.ProfileId, component);
            AlivePlayerArray.Add(component);
            OnPlayerAdded?.Invoke(component);
#if DEBUG
            Logger.LogDebug($"Initialized Player Component {player.name} : {player.ProfileId}");
#endif
            return true;
        }
#if DEBUG
        Logger.LogError($"Init PlayerComponent Failed for {player.name} : {player.ProfileId}");
#endif
        UnityEngine.Object.Destroy(component);
        return false;
    }

    private bool TryRemove(string profileId, out bool destroyedComponent)
    {
        destroyedComponent = false;
        if (profileId.IsNullOrEmpty())
        {
            ClearNullPlayers();
            return false;
        }
        if (AlivePlayersDictionary.TryGetValue(profileId, out PlayerComponent playerComponent))
        {
            OnPlayerRemoved?.Invoke(profileId, playerComponent);
            if (playerComponent != null)
            {
                destroyedComponent = true;
                playerComponent.Dispose();
            }
            AlivePlayersDictionary.Remove(profileId);
            // ref: AUD-01-01 - Remover bot morto do HashSet para cessar updates e liberar memoria
            if (playerComponent != null)
            {
                AlivePlayerArray.Remove(playerComponent);
            }
            return true;
        }
        return false;
    }

    private void ClearNullPlayers()
    {
        foreach ((string profileId, PlayerComponent playerComponent) in AlivePlayersDictionary)
        {
            if (playerComponent == null ||
                playerComponent.Player == null)
            {
                _ids.Add(profileId);
#if DEBUG
                if (playerComponent.Player != null)
                {
                    Logger.LogDebug($"Removing {playerComponent.Player.Profile?.Nickname} from player dictionary");
                }
#endif
            }
        }
        if (_ids.Count > 0)
        {
#if DEBUG
            Logger.LogDebug($"Removing {_ids.Count} null players");
#endif
            foreach (var id in _ids)
            {
                TryRemove(id, out _);
            }
            _ids.Clear();
        }
    }

    private readonly List<string> _ids = [];

    private readonly GameWorldComponent _sainGameWorld;
}
