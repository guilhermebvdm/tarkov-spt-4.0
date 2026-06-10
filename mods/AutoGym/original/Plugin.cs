using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using EFT.Hideout;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;

namespace AutoGym;

[BepInPlugin("sweet.autogym", "AutoGym", "1.0.0")]
public sealed class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;
    internal static ConfigEntry<bool> Enabled = null!;
    internal static ConfigEntry<bool> HideWorkoutGear = null!;
    internal static ConfigEntry<float> SuccessWindowBias = null!;
    internal static ConfigEntry<int> ExtraDelayMs = null!;

    private void Awake()
    {
        Log = Logger;
        Enabled = Config.Bind("General", "Enabled", true, "Automatically completes the hideout gym QTE without pressing the QTE key.");
        HideWorkoutGear = Config.Bind("Visuals", "Hide Workout Gear", true, "Temporarily hides backpack, rig, armor, helmet, headset, face cover, and eyewear during hideout gym workouts.");
        SuccessWindowBias = Config.Bind(
            "Timing",
            "Success Window Bias",
            0.5f,
            new ConfigDescription("Where inside the success window AutoGym completes the QTE. 0 is early, 0.5 is center, 1 is late.",
                new AcceptableValueRange<float>(0f, 1f)));
        ExtraDelayMs = Config.Bind(
            "Timing",
            "Extra Delay Ms",
            0,
            new ConfigDescription("Optional extra delay after the calculated success timing.",
                new AcceptableValueRange<int>(0, 250)));

        new Harmony("sweet.autogym").PatchAll();
        Log.LogInfo("AutoGym loaded.");
    }
}

[HarmonyPatch(typeof(HideoutPlayerOwner), nameof(HideoutPlayerOwner.PrepareWorkout))]
internal static class HideoutPlayerOwnerPrepareWorkoutPatch
{
    private static void Prefix(HideoutPlayerOwner __instance)
    {
        if (Plugin.HideWorkoutGear?.Value == true)
        {
            WorkoutGearVisibility.Hide(__instance);
        }
    }
}

[HarmonyPatch(typeof(HideoutPlayerOwner), nameof(HideoutPlayerOwner.StopWorkout))]
internal static class HideoutPlayerOwnerStopWorkoutPatch
{
    private static void Finalizer(HideoutPlayerOwner __instance)
    {
        WorkoutGearVisibility.Restore(__instance);
    }
}

internal static class WorkoutGearVisibility
{
    private static readonly EquipmentSlot[] SlotsToHide =
    {
        EquipmentSlot.Backpack,
        EquipmentSlot.TacticalVest,
        EquipmentSlot.ArmorVest,
        EquipmentSlot.Headwear,
        EquipmentSlot.Earpiece,
        EquipmentSlot.FaceCover,
        EquipmentSlot.Eyewear
    };

    private static readonly Dictionary<HideoutPlayerOwner, List<IVisibilityState>> HiddenGear = new();

    internal static void Hide(HideoutPlayerOwner owner)
    {
        if (owner?.HideoutPlayer?.PlayerBody == null)
        {
            return;
        }

        Restore(owner);

        List<IVisibilityState> states = new();
        HashSet<int> seenObjects = new();
        HashSet<int> seenRenderers = new();
        PlayerBody playerBody = owner.HideoutPlayer.PlayerBody;

        foreach (EquipmentSlot slot in SlotsToHide)
        {
            if (!playerBody.SlotViews.ContainsKey(slot))
            {
                continue;
            }

            PlayerBody.EquipmentSlotClass slotView = playerBody.SlotViews.GetByKey(slot);
            Capture(slotView.ParentedModel?.Value, states, seenObjects);
            Capture(slotView.Model, states, seenObjects);
            Capture(slotView.MainDress?.Value?.gameObject, states, seenObjects);

            if (slotView.Dresses != null)
            {
                foreach (var dress in slotView.Dresses)
                {
                    Capture(dress?.gameObject, states, seenObjects);
                }
            }

            if (slotView.Renderers != null)
            {
                foreach (Renderer renderer in slotView.Renderers)
                {
                    Capture(renderer, states, seenRenderers);
                }
            }
        }

        if (states.Count > 0)
        {
            HiddenGear[owner] = states;
        }
    }

    internal static void Restore(HideoutPlayerOwner owner)
    {
        if (owner == null || !HiddenGear.TryGetValue(owner, out List<IVisibilityState> states))
        {
            return;
        }

        HiddenGear.Remove(owner);

        foreach (IVisibilityState state in states)
        {
            state.Restore();
        }
    }

    private static void Capture(GameObject? gameObject, List<IVisibilityState> states, HashSet<int> seenObjects)
    {
        if (gameObject == null || !seenObjects.Add(gameObject.GetInstanceID()))
        {
            return;
        }

        states.Add(new GameObjectVisibilityState(gameObject, gameObject.activeSelf));
        gameObject.SetActive(false);
    }

    private static void Capture(Renderer? renderer, List<IVisibilityState> states, HashSet<int> seenRenderers)
    {
        if (renderer == null || !seenRenderers.Add(renderer.GetInstanceID()))
        {
            return;
        }

        states.Add(new RendererVisibilityState(renderer, renderer.enabled));
        renderer.enabled = false;
    }

    private interface IVisibilityState
    {
        void Restore();
    }

    private sealed class GameObjectVisibilityState : IVisibilityState
    {
        private readonly GameObject _gameObject;
        private readonly bool _activeSelf;

        public GameObjectVisibilityState(GameObject gameObject, bool activeSelf)
        {
            _gameObject = gameObject;
            _activeSelf = activeSelf;
        }

        public void Restore()
        {
            if (_gameObject != null)
            {
                _gameObject.SetActive(_activeSelf);
            }
        }
    }

    private sealed class RendererVisibilityState : IVisibilityState
    {
        private readonly Renderer _renderer;
        private readonly bool _enabled;

        public RendererVisibilityState(Renderer renderer, bool enabled)
        {
            _renderer = renderer;
            _enabled = enabled;
        }

        public void Restore()
        {
            if (_renderer != null)
            {
                _renderer.enabled = _enabled;
            }
        }
    }
}

[HarmonyPatch(typeof(ShrinkingCircleQTE), "method_0")]
internal static class ShrinkingCircleQtePatch
{
    private static readonly FieldInfo SpeedField = AccessTools.Field(typeof(ShrinkingCircleQTE), "_speed");
    private static readonly FieldInfo MinScaleField = AccessTools.Field(typeof(ShrinkingCircleQTE), "_minScale");
    private static readonly FieldInfo SuccessOuterField = AccessTools.Field(typeof(ShrinkingCircleQTE), "double_0");
    private static readonly FieldInfo SuccessInnerField = AccessTools.Field(typeof(ShrinkingCircleQTE), "double_1");

    private static bool Prefix(ShrinkingCircleQTE __instance, ref Task<bool> __result)
    {
        if (Plugin.Enabled == null || !Plugin.Enabled.Value)
        {
            return true;
        }

        __result = CompleteInSuccessWindow(__instance);
        return false;
    }

    private static async Task<bool> CompleteInSuccessWindow(ShrinkingCircleQTE qte)
    {
        try
        {
            await Task.Yield();

            int delayMs = CalculateDelayMs(qte);
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }

            await qte.method_3(success: true);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"AutoGym failed to complete QTE: {ex}");
            return false;
        }
    }

    private static int CalculateDelayMs(ShrinkingCircleQTE qte)
    {
        float speed = Math.Max(0.1f, ReadField<float>(SpeedField, qte, 1f));
        float minScale = ReadField<float>(MinScaleField, qte, 0.25f);
        double successOuter = ReadField<double>(SuccessOuterField, qte, 0.55d);
        double successInner = ReadField<double>(SuccessInnerField, qte, 0.45d);
        float bias = Mathf.Clamp01(Plugin.SuccessWindowBias?.Value ?? 0.5f);

        double targetScale = successOuter + (successInner - successOuter) * bias;
        double travel = Math.Max(0.001d, 1d - minScale);
        double normalizedElapsed = Math.Max(0d, Math.Min(1d, (1d - targetScale) / travel));
        double durationMs = 3000d / speed;
        int calculatedDelay = (int)Math.Round(durationMs * normalizedElapsed);

        return Math.Max(0, calculatedDelay + (Plugin.ExtraDelayMs?.Value ?? 0));
    }

    private static T ReadField<T>(FieldInfo field, object instance, T fallback)
    {
        try
        {
            object value = field.GetValue(instance);
            return value is T typed ? typed : fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
