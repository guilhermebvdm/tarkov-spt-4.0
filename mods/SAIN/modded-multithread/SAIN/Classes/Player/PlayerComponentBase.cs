using System;
using EFT;
using SAIN.Classes;
using SAIN.Components.PlayerComponentSpace;
using UnityEngine;

namespace SAIN.SAINComponent;

public abstract class PlayerComponentBase(PlayerComponent player) : IDisposable
{
    public PlayerComponent PlayerComponent { get; private set; } = player;

    public Vector3 Position
    {
        get { return PlayerComponent.Position; }
    }

    public Vector3 LookDirection
    {
        get { return PlayerComponent.LookDirection; }
    }

    public PlayerTransformClass Transform
    {
        get { return PlayerComponent.Transform; }
    }

    public Player Player
    {
        get { return PlayerComponent.Player; }
    }

    public virtual void Dispose() { }
}
