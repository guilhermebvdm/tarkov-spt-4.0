using System;
using System.Reflection;
using BepInEx.Logging;
using EFT;
using Fika.Core.Main.GameMode;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TRLWeatherSync.Patches;

public class GameWorldOnGameStartedPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT/GameWorld.cs:2584
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        try
        {
            WeatherSyncSession.Begin();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao iniciar sessão de sync: {ex}");
        }
    }
}

public class GameWorldOnDestroyPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT/GameWorld.cs:2111
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnDestroy));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        try
        {
            WeatherSyncSession.End();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao encerrar sessão de sync (GameWorld.OnDestroy): {ex}");
        }
    }
}

public class CoopGameStopPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    protected override MethodBase GetTargetMethod()
    {
        // ref: mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:718
        // CoopGame : BaseLocalGame<EftGamePlayerOwner> sobrescreve Stop sem chamar base (PA-01-01,
        // review 01) — mirar a classe real usada em toda raid FIKA, não a base genérica.
        return AccessTools.Method(typeof(CoopGame), nameof(CoopGame.Stop));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        try
        {
            WeatherSyncSession.End();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao encerrar sessão de sync (CoopGame.Stop): {ex}");
        }
    }
}
