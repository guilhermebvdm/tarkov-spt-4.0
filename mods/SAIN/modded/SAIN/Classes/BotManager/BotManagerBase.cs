using System.Collections.Generic;
using EFT;
using SAIN.Components;

namespace SAIN;

public abstract class BotManagerBase(BotManagerComponent botController)
{
    public BotManagerComponent BotController { get; private set; } = botController;
    public Dictionary<string, BotComponent> Bots
    {
        get { return BotController?.BotSpawnController?.BotDictionary; }
    }

    public GameWorld GameWorld
    {
        get { return BotController.GameWorld; }
    }

    public GameWorldComponent SAINGameWorld
    {
        get { return BotController.SAINGameWorld; }
    }
}
