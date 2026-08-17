using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using tarkin.ladders.shared;
using System.Collections.Generic;
using UnityEngine;

#if SPT_4_0
using IInteractive = GInterface177;
using InteractionContextHelper = GetActionsClass;
using AvailableInteractionState = ActionsReturnClass;
using InteractionAction = ActionsTypesClass;
#endif

namespace tarkin.ladders.bep
{
    internal class Patch_InteractionContextHelper_GetAvailableActions : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionContextHelper), nameof(InteractionContextHelper.GetAvailableActions), [typeof(GamePlayerOwner), typeof(IInteractive)]);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref AvailableInteractionState __result, GamePlayerOwner owner, IInteractive interactive)
        {
            Ladder ladder = interactive as Ladder;
            if (ladder == null)
                return true;

            if (owner.Player.IsInBufferZone) // this is the value I'm using to determine if on ladder
                return false;

            Vector3 playerForward = owner.Player.PlayerBones.BodyTransform.forward;
            Vector3 ladderForward = ladder.transform.forward;
            bool behindLadder = Vector3.Dot(playerForward, ladderForward) > 0f;
            bool aboveLadder = owner.Player.Position.y > ladder.transform.position.y + ladder.MaxHeight - ladder.RungSpacing * 2f;

            if (behindLadder && !aboveLadder)
                return false;

            __result = new AvailableInteractionState
            {
                Actions = new List<InteractionAction>()
                {
                    new InteractionAction()
                    {
                        Name = "Climb",
                        Action = () => 
                        {
                            owner.Player.gameObject.AddComponent<PlayerLadderController>().Init(ladder);
                            owner.Player.UpdateInteractionCast();
                        },
                        Disabled = false,
                        TargetName = null
                    }
                }
            };

            return false;
        }
    }
}
