using System;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Fika: o revive devolve a hierarquia do modelo para a layer "Player" e nunca repromove os
    /// BodyPartCollider para "HitCollider" — a única layer de corpo que a máscara balística enxerga.
    /// Resultado: jogador revivido visível, jogável, e ATRAVESSÁVEL por bala/faca até o fim da raid.
    ///
    /// ref: ReviveInteractable.cs:132 (fika-plugin) — SetLayersRecursively(gameObject, "Player") sem
    ///      o SetupHitColliders() que o vanilla SEMPRE faz na linha seguinte (Player.cs:28646-28647).
    /// ref: LayerMasksDataAbstractClass.cs:69-83 — HitMask = Water|Terrain|HighPolyCollider|
    ///      TransparentCollider|Deadbody|HitCollider. "Player" NÃO está lá.
    /// ref: EftBulletClass.cs:819 — o traçado do projétil usa HitMask nos dois argumentos.
    ///
    /// Enquanto derrubado a layer é "Deadbody", que ESTÁ no HitMask — o downed é baleável e correto.
    /// O defeito nasce no revive.
    /// </summary>
    public class Patch_FikaReviveHitbox
    {
        private const string ReviveInteractableType = "Fika.Core.Main.Components.ReviveInteractable";

        private static FieldInfo _observedPlayerField;

        public void Enable()
        {
            try
            {
                var type = AccessTools.TypeByName(ReviveInteractableType);
                if (type == null)
                {
                    // Fika ausente: nada a corrigir. Informação, não erro (o mod é multi-fix).
                    Plugin.Log?.LogInfo("TRL-Fixes: Fika nao encontrado — patch de hitbox pos-revive dispensado.");
                    return;
                }

                var targetMethod = AccessTools.Method(type, "RemoveRagdoll");
                if (targetMethod == null)
                {
                    Plugin.Log?.LogError("TRL-Fixes: ReviveInteractable.RemoveRagdoll nao encontrado — hitbox pos-revive NAO sera corrigida (Fika mudou de shape?).");
                    return;
                }

                // O campo é lido 1x aqui: se o shape do Fika mudar, falha no boot com log claro,
                // em vez de silenciosamente por-revive dentro do try/catch do Postfix.
                _observedPlayerField = AccessTools.Field(type, "_observedPlayer");
                if (_observedPlayerField == null)
                {
                    Plugin.Log?.LogError("TRL-Fixes: campo _observedPlayer nao encontrado em ReviveInteractable — hitbox pos-revive NAO sera corrigida.");
                    return;
                }

                var harmony = new Harmony("com.trl.fixes.fikarevivehitbox");
                var postfixMethod = AccessTools.Method(typeof(Patch_FikaReviveHitbox), nameof(Postfix));
                harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfixMethod));
                Plugin.Log?.LogInfo("TRL-Fixes: Hook no ReviveInteractable.RemoveRagdoll aplicado com sucesso!");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar patch ReviveInteractable.RemoveRagdoll: {ex}");
            }
        }

        /// <summary>
        /// POSTFIX obrigatoriamente: tem de rodar DEPOIS do SetLayersRecursively da linha 132,
        /// senão é sobrescrito por ele.
        ///
        /// Alvo é RemoveRagdoll, e não RevivePlayer, porque o defeito é POR OBSERVADOR e há dois
        /// caminhos: quem executou o revive passa por ReviveInteractable.cs:240, e os demais peers
        /// por ObservedPlayer.cs:1327 (ToggleDowned(false) ao receber o RevivedPlayerPacket).
        /// Patchar RevivePlayer deixaria todos os observadores não-revivedores quebrados.
        /// </summary>
        private static void Postfix(object __instance)
        {
            // Uma exceção aqui roda dentro do fluxo de revive do Fika e poderia cancelá-lo.
            try
            {
                if (_observedPlayerField == null) return;

                if (!(_observedPlayerField.GetValue(__instance) is Player observedPlayer)) return;

                // Repromove todo BodyPartCollider/ArmorPlateCollider para "HitCollider" e refaz
                // SetUpPlayer/PlayerProfileID. Idempotente: recoleta os componentes e reatribui.
                // ref: Player.cs:29832 (public virtual; ObservedPlayer não sobrescreve)
                observedPlayer.SetupHitColliders();

                // As placas de armadura foram desativadas na criação do ragdoll
                // (RagdollClass.cs:132-136, SetActive(false)) e o revive não as reativa;
                // SetupHitColliders as coleta com includeInactive mas não as liga de volta.
                // Este é o único caminho que chama PlayerBones.SetArmorPlateCollidersState.
                // ref: Player.cs:30235 / :30250-30253
                observedPlayer.RecalculateEquipmentParams();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[TRL-Fixes] Falha ao restaurar hitbox pos-revive: {ex}");
            }
        }
    }
}
