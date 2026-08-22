using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace tarkin.ladders.bep
{
    internal class Patch_MoveInputTranslator_TranslateAxes : ModulePatch
    {
        private static readonly Dictionary<string, PlayerInputAxesDelegate> subscribers = new();

        public delegate void PlayerInputAxesDelegate(Player player, ref float[] axes);

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MoveInputTranslator), nameof(MoveInputTranslator.TranslateAxes));
        }

        [PatchPrefix]
        private static void PatchPrefix(MoveInputTranslator __instance, ref float[] axes)
        {
            Player controlledPlayer = __instance.Player_0;
            if (controlledPlayer != null)
            {
                Dispatch(controlledPlayer, ref axes);
            }
        }

        private static void Dispatch(Player player, ref float[] axes)
        {
            if (player.ProfileId != null && subscribers.TryGetValue(player.ProfileId, out var callback))
            {
                try { callback.Invoke(player, ref axes); } catch { }
            }
        }

        public static void Subscribe(Player player, PlayerInputAxesDelegate callback)
        {
            if (player == null) return;
            if (player.ProfileId == null) return;

            if (!subscribers.ContainsKey(player.ProfileId))
                subscribers[player.ProfileId] = callback;
            else
                subscribers[player.ProfileId] += callback;
        }

        public static void Unsubscribe(Player player, PlayerInputAxesDelegate callback)
        {
            if (player == null) return;
            if (player.ProfileId == null) return;

            if (subscribers.ContainsKey(player.ProfileId))
            {
                subscribers[player.ProfileId] -= callback;

                if (subscribers[player.ProfileId] == null)
                {
                    subscribers.Remove(player.ProfileId);
                }
            }
        }

        public static void ClearAllSubscribers()
        {
            subscribers.Clear();
        }
    }
}
