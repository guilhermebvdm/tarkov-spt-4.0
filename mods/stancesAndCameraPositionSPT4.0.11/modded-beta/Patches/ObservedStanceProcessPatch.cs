using System;
using System.Reflection;
using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 014 (fix-01): aplica o offset de stance do jogador OBSERVADO (Fika) no WeaponRootAnim, num
    /// Postfix de ProceduralWeaponAnimation.ProcessEffectors. Esse é o ponto certo porque:
    ///  - ProcessEffectors roda para TODA PWA, inclusive observados/peers (mesmo ponto do item 011);
    ///  - o Postfix roda DEPOIS de todo o processamento da PWA (ApplyComplexRotation + AvoidObstacles, etc),
    ///    então o offset NÃO é sobrescrito;
    ///  - e ANTES de o ObservedPlayer copiar WeaponRootAnim -> PlayerBones.Offset/DeltaRotation
    ///    (ObservedPlayer.cs:1852) -> ShiftWeaponRoot (1876), que renderiza a arma em 3a pessoa.
    /// O jogador LOCAL continua tratado pelo ApplyComplexRotationPatch (1a pessoa) — gate IsYourPlayer.
    /// </summary>
    public class ObservedStanceProcessPatch : ModulePatch
    {
        private static FieldInfo _firearmControllerField;

        protected override MethodBase GetTargetMethod()
        {
            _firearmControllerField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return AccessTools.Method(typeof(ProceduralWeaponAnimation),
                                      nameof(ProceduralWeaponAnimation.ProcessEffectors));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            try
            {
                var fc = (Player.FirearmController)_firearmControllerField.GetValue(__instance);
                if (fc == null) return;
                Player player = Traverse.Create(fc).Field<Player>("_player").Value;
                if (player == null || player.IsYourPlayer) return;   // só jogadores OBSERVADOS (remotos)

                player.gameObject.GetComponent<Networking.ObservedStanceAnimator>()?.ApplyToObserved(__instance);
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ObservedStanceProcess] {ex.Message}"); }
        }
    }
}
