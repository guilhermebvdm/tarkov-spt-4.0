using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 019 — ao CHECAR A CÂMARA in-raid, exibe o mesmo painel do check-carregador (AmmoCountPanel)
    /// mostrando se há bala e QUAL é. O vanilla não mostra nada no HUD ao checar a câmara — ele só toca a
    /// animação e marca o estado para a tela de inspeção do INVENTÁRIO (ver backlog/019, investigação 01).
    ///
    /// v1 (2.10.0) tentava reusar o evento <c>Player.OnShowAmmoDetails</c> via reflexão — funcionava (subscriber
    /// confirmado, sem exceção) mas o painel nunca aparecia na tela em teste ao vivo (estande de tiro do Hideout,
    /// HideoutPlayerOwner). Trocado (v2, ver code-review 02) pela abordagem do RealismMod 0.14.8/SPT 3.11
    /// (`ChamberCheckUIPatch.cs`, decompilado em `mods/RealismMod/Client/DLL descompilada/.../ChamberCheckUIPatch.cs`):
    /// chamar <c>EftBattleUIScreen.ShowAmmoDetails</c> DIRETO via <c>Singleton&lt;CommonUI&gt;</c>, sem depender de
    /// qual `GamePlayerOwner`/`HideoutPlayerOwner` está inscrito no evento do `Player` local.
    /// UI LOCAL (sem sync Fika — só quem checa vê). Postfix puro: não altera o fluxo nem o resultado do check nativo.
    /// </summary>
    public class ChamberCheckAmmoPatch : ModulePatch
    {
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
                if (weapon.MalfState.State != Weapon.EMalfunctionState.None) return; // OBRIGATÓRIO: CheckChamber() retorna true mesmo em malfunction (Player.cs:5834) — sem este guard o painel apareceria com a câmara emperrada.

                var player = Traverse.Create(__instance).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer || !player.FirstPersonPointOfView) return;

                var screen = Singleton<CommonUI>.Instantiated ? Singleton<CommonUI>.Instance.EftBattleUIScreen : null;
                if (screen == null) return;

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

                // maxAmmoCount depende de qual fórmula de threshold o AmmoCountPanel usa (EFT.UI/AmmoCountPanel.cs):
                // - GetAmmoCountByLevel (folding=false) decide "Full" com `ammoCount >= maxAmmoCount-1` → precisa
                //   max=2 pra "Empty" (0) não cair no mesmo threshold de "Full" (com max=1, 0>=0 também é true).
                // - GetAmmoCountByLevelForFoldingMechanismWeapon (folding=true) decide "Full" com `ammoCount >=
                //   maxAmmoCount` (sem o "-1") → com max=2 o "Full" de 1 bala vira o texto cru "1"; precisa max=1.
                int maxAmmoCount = folding ? 1 : 2;
                if (round != null)
                {
                    screen.ShowAmmoDetails(1, maxAmmoCount, 2, GClass2348.Localized(round.Name), folding);
                }
                else
                {
                    screen.ShowAmmoDetails(0, maxAmmoCount, 2, null, folding);
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ChamberCheck] {ex.Message}"); }
        }
    }
}
