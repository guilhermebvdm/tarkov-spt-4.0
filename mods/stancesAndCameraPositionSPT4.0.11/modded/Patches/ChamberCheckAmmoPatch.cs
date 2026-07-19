using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 019 — ao CHECAR A CÂMARA in-raid, exibe o mesmo painel do check-carregador (AmmoCountPanel)
    /// mostrando se há bala e QUAL é. O vanilla não mostra nada no HUD ao checar a câmara — ele só toca a
    /// animação e marca o estado para a tela de inspeção do INVENTÁRIO (ver backlog/019, investigação 01).
    ///
    /// Reutiliza o evento nativo <c>Player.OnShowAmmoDetails</c> (o mesmo que o check-carregador dispara em
    /// FirearmController.CheckAmmo, Player.cs:5770): com bala → "Full" + nome da munição; câmara vazia → "Empty".
    /// UI LOCAL (sem sync Fika — só quem checa vê). Postfix puro: não altera o fluxo nem o resultado do check nativo.
    /// </summary>
    public class ChamberCheckAmmoPatch : ModulePatch
    {
        // Evento nativo: Action<ammoCount, maxAmmoCount, mastering, details, foldingMechanimWeapon>. Lido por
        // reflexão porque é privado para invocação externa — mesmo caminho interno usado pelo CheckAmmo.
        private static bool _warnedNoEvent; // log one-shot se o backing field do evento sumir (obfuscação futura)

        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.CheckChamber));

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance, bool __result)
        {
            try
            {
                if (!Plugin._ShowChamberAmmoOnCheck.Value) return;
                if (!__result) return; // check nativo não rodou (gatilho pressionado, stationary, revólver, malf...)

                var weapon = __instance.Weapon;
                if (weapon == null || !weapon.HasChambers) return;
                if (weapon.MalfState.State != Weapon.EMalfunctionState.None) return; // câmara não é "lida" limpa em malf

                var player = Traverse.Create(__instance).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer || !player.FirstPersonPointOfView) return;

                var show = Traverse.Create(player).Field("OnShowAmmoDetails")
                                   .GetValue<Action<int, int, int, string, bool>>();
                if (show == null)
                {
                    // Backing field do evento não resolveu (update do jogo renomeou/obfuscou?). A feature fica
                    // inerte — loga UMA vez para o forense futuro em vez de sumir sem rastro.
                    if (!_warnedNoEvent) { _warnedNoEvent = true; Plugin.Logger.LogWarning("[ChamberCheck] OnShowAmmoDetails não resolveu por reflexão — feature inativa."); }
                    return;
                }

                // Primeira bala VIVA em qualquer câmara (IsUsed==false; true = estojo deflagrado).
                // Iterar cobre armas multi-câmara (ex.: double-barrel) — a bala pode estar no 2º cano; para
                // câmara única é idêntico a ler Chambers[0].
                AmmoItemClass round = null;
                foreach (var chamber in weapon.Chambers)
                {
                    if (chamber?.ContainedItem is AmmoItemClass a && !a.IsUsed) { round = a; break; }
                }

                // 5º arg (foldingMechanimWeapon): espelha o vanilla (Chambers.Length > 1) para o mesmo
                // posicionamento do painel; revólver não chega aqui (CheckChamber retorna false).
                bool folding = weapon.Chambers.Length > 1;

                if (round != null)
                {
                    // count=1/max=1 → linha "Full" (idêntico ao check-carregador cheio) + nome da bala localizado.
                    show(1, 1, 2, GClass2348.Localized(round.Name), folding);
                }
                else
                {
                    // count=0 → linha "Empty", sem 2ª linha (details == null).
                    show(0, 1, 2, null, folding);
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ChamberCheck] {ex.Message}"); }
        }
    }
}
