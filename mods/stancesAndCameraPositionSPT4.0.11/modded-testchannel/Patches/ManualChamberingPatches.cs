using System;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.Animations;
using EFT.InputSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using Comfort.Common;
using EFT.AssetsManager;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    // EFT 0.16.9 semantic mappings & 4.1 readiness (AP-09):
    // GClass2055 = SpawnOperation (operação de equip/spawn da arma nas mãos) // ref: CR-06-05
    using ChamberWeaponClass = EFT.Player.FirearmController.GClass2055;
    // GClass2016 = ReloadExternalMagOperation (animação e eventos de recarga de carregador externo) // ref: CR-06-05
    using ReloadWeaponClass = EFT.Player.FirearmController.GClass2016;
    // GClass2006 = ReloadExternalMagResult (cálculo e execução das operações de inventário na recarga) // ref: CR-06-05
    using ReloadExternalMagResultClass = EFT.Player.FirearmController.GClass2006;
    // DefaultWeaponOperationClass / Class1268 = Operações de disparo de arma bolt-action
    using DefaultWeaponOpClass = EFT.Player.FirearmController.DefaultWeaponOperationClass;
    using ClientBoltOpClass = EFT.ClientFirearmController.Class1268;
    // GClass2037 = IdleWeaponOperation (operação idle/padrão da arma nas mãos) // ref: CR-06-05
    using IdleWeaponOpClass = EFT.Player.FirearmController.GClass2037;
    // GClass2005 = InstallMagResult (instalação de magazine pelo inventário) // ref: CR-06-05
    using InstallMagResultClass = EFT.Player.FirearmController.GClass2005;
    // GClass2039 = InstallMagOperation (animação e eventos de instalação de magazine pelo inventário) // ref: CR-06-05
    using InstallMagOperationClass = EFT.Player.FirearmController.GClass2039;

    public static class ManualChamberingState
    {
        public static bool BlockChambering = false;

        // Default FALSE — alinhado ao RealismMod.
        // Vira `true` apenas via RechamberRound (puxar ferrolho manual), quando a arma já possui bala na câmara ou quando a feature está desligada.
        public static bool CanLoadChamber = false;

        // Flag de "raid recém-iniciada" — setada no GameWorldOnGameStartedPatch
        public static bool JustSpawned = false;

        // Flag para autorizar o ciclo de bolt action via Shift+T (Manual Bolt Action)
        public static bool AllowManualBoltActionCycle = false;

        public static void Reset()
        {
            BlockChambering = false;
            CanLoadChamber = false;
            JustSpawned = false;
            AllowManualBoltActionCycle = false;
        }
    }

    public static class ManualChamberingPatches
    {
        /// <summary>
        /// CR-03-03 / CR-04-03: Limpeza defensiva do Transform e instâncias 3D na câmara (patron_in_weapon).
        /// Remove qualquer GameObject residual devolvendo à pool da Unity quando ChamberAmmoCount == 0,
        /// cobrindo armas de câmara única e multi-barrel.
        /// </summary>
        public static void CleanResidualChamberModel(Player.FirearmController fc)
        {
            if (fc == null || fc.Weapon == null || fc.Weapon.ChamberAmmoCount > 0) return;
            try
            {
                var wm = Traverse.Create(fc).Field("weaponManagerClass").GetValue<WeaponManagerClass>();
                if (wm != null)
                {
                    wm.DestroyAllPatronsInWeapon();
                    if (wm.Transform_0 != null)
                    {
                        for (int ch = 0; ch < wm.Transform_0.Length; ch++)
                        {
                            Transform t = wm.Transform_0[ch];
                            if (t == null) continue;
                            for (int i = t.childCount - 1; i >= 0; i--)
                            {
                                var child = t.GetChild(i);
                                if (child != null && child.gameObject != null)
                                {
                                    AssetPoolObject.ReturnToPool(child.gameObject);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[ManualChamber] CleanResidualChamberModel: {ex.Message}");
            }
        }

        /// <summary>
        /// CR-03-04: Identificação estrita de escopetas de bomba (Pump Action).
        /// Exclui escopetas semi/automáticas como Saiga-12, AA-12, MP-153 e MP-155.
        /// </summary>
        public static bool IsPumpActionShotgun(Weapon weapon)
        {
            if (weapon == null || weapon.Template == null) return false;
            string templateId = weapon.Template._id;
            return templateId == "5a7828548dc32e5a9c28b516"  // Remington Model 870 12g
                || templateId == "54491c4f4bdc2db1078b4568"  // Izhmash MP-133 12g
                || templateId == "5e8488fa988a99591e728351"; // TOZ KS-23M 23x75
        }

        /// <summary>
        /// Verifica se o jogador local é um cliente convidado em sessão cooperativa do FIKA.
        /// </summary>
        public static bool IsFikaGuestClient()
        {
            try
            {
                return Fika.Core.Main.Utils.FikaBackendUtils.ClientType == Fika.Core.Main.Utils.EClientType.Client;
            }
            catch
            {
                return false;
            }
        }

        // ref: CR-06-02 - Cache estático do FieldInfo de IsObservedAI (FIKA) para evitar reflexão dinâmica repetida
        private static FieldInfo _fikaObservedAiField;
        private static bool _fikaFieldResolved = false;

        /// <summary>
        /// Determina se um jogador é humano (o jogador local OU um companheiro humano remoto em coop/FIKA).
        /// Retorna false se o jogador for um bot de IA (Scavs, Bosses, PMCs de IA), garantindo que a IA
        /// continue estritamente no fluxo vanilla de recarga automática.
        /// </summary>
        public static bool IsHumanPlayer(Player player)
        {
            if (player == null) return false;
            if (player.IsYourPlayer) return true;
            if (player.IsAI) return false;

            // Fika compatibility: no cliente coop, bots replicados possuem IsObservedAI = true
            // ref: CR-06-02 - Resolução com cache estático
            try
            {
                if (!_fikaFieldResolved)
                {
                    _fikaObservedAiField = AccessTools.Field(player.GetType(), "IsObservedAI");
                    _fikaFieldResolved = true;
                }

                if (_fikaObservedAiField != null && (bool)_fikaObservedAiField.GetValue(player))
                    return false;
            }
            catch { }

            return true;
        }

        /// <summary>
        /// Determina se um InventoryController pertence a um jogador humano (local ou companheiro remoto no FIKA).
        /// </summary>
        public static bool IsHumanInventory(TraderControllerClass controller)
        {
            if (controller == null) return false;
            var gw = Singleton<GameWorld>.Instance;
            if (gw == null) return false;

            if (gw.MainPlayer != null && controller == gw.MainPlayer.InventoryController)
                return true;

            // ref: CR-06-01 - Fast-path O(1) nativo do EFT via ProfileId no dicionário interno
            if (!string.IsNullOrEmpty(controller.ID))
            {
                var player = gw.GetAlivePlayerByProfileID(controller.ID);
                if (player != null)
                    return IsHumanPlayer(player);
            }

            // Fallback defensivo O(N) caso o ProfileId não esteja indexado de imediato
            var players = gw.AllAlivePlayersList;
            if (players != null)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    if (p != null && p.InventoryController == controller)
                    {
                        return IsHumanPlayer(p);
                    }
                }
            }

            return false;
        }
    }

    public class ManualChamberingComponent : MonoBehaviour
    {
        public Player Player;
        public Player.FirearmController FirearmController;
        public WeaponManagerClass WeaponStateClass;
        public AmmoItemClass Bullet;
        public float Timer = 0f;
        public int Phase = 0;

        void Update()
        {
            if (Phase == 0) return;

            Timer += Time.deltaTime;

            // Phase 1: Esperar a arma ir pro centro (Stance 0) antes de rodar a animação
            if (Phase == 1 && Timer >= 0.2f)
            {
                if (FirearmController != null && FirearmController.FirearmsAnimator != null)
                {
                    if (Player != null && Player.BodyAnimatorCommon != null)
                    {
                        Player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
                    }

                    if (Bullet != null && WeaponStateClass != null)
                    {
                        Plugin.Logger.LogInfo("[ManualChamber] Stance alcançada. Executando animação e abastecendo câmara.");
                        WeaponStateClass.RemoveAllShells();
                        FirearmController.FirearmsAnimator.SetAmmoInChamber(1f);
                        FirearmController.FirearmsAnimator.SetAmmoOnMag(FirearmController.Weapon.GetCurrentMagazineCount());
                        WeaponStateClass.SetRoundIntoWeapon(Bullet, 0);
                        FirearmController.FirearmsAnimator.Rechamber(true);
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("[ManualChamber] Stance alcançada. Dry Rack (manuseio no seco).");
                        if (WeaponStateClass != null) WeaponStateClass.RemoveAllShells();
                        // Para o Animator Controller autorizar a transição de puxar o ferrolho (Rechamber),
                        // informamos temporariamente SetAmmoInChamber(1f) e SetAmmoCountForRemove(1).
                        // Como a câmara física está vazia (Bullet == null e RemoveAllShells), nenhuma bala 3D é criada nem ejetada.
                        FirearmController.FirearmsAnimator.SetAmmoInChamber(1f);
                        FirearmController.FirearmsAnimator.SetAmmoCountForRemove(1);
                        FirearmController.FirearmsAnimator.SetFire(false);
                        FirearmController.FirearmsAnimator.SetInventory(false);
                        FirearmController.FirearmsAnimator.SetAmmoOnMag(FirearmController.Weapon.GetCurrentMagazineCount());
                        FirearmController.FirearmsAnimator.Rechamber(true);
                        if (FirearmController.Weapon != null)
                        {
                            FirearmController.Weapon.Armed = true; // Rearma percussor mecânico
                        }
                        FirearmController.FirearmsAnimator.SetHammerArmed(true);
                    }
                }
                Phase = 2;
                Timer = 0f;
            }
            // Phase 2: Parar o trigger da animação e restaurar postura após ciclo completo (~1.0s)
            else if (Phase == 2 && Timer >= 1.0f)
            {
                if (FirearmController != null && FirearmController.FirearmsAnimator != null)
                {
                    FirearmController.FirearmsAnimator.Rechamber(false);
                    FirearmController.FirearmsAnimator.SetAmmoInChamber(0f);
                    FirearmController.FirearmsAnimator.SetAmmoCountForRemove(0);
                    if (FirearmController.Weapon != null)
                    {
                        FirearmController.Weapon.Armed = true;
                        if (FirearmController.Weapon.ChamberAmmoCount == 0)
                        {
                            ManualChamberingPatches.CleanResidualChamberModel(FirearmController);
                        }
                    }
                    FirearmController.FirearmsAnimator.SetHammerArmed(true);
                }
                if (Player != null && Player.BodyAnimatorCommon != null)
                {
                    Player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 0f);
                }
                // PR-01: Restaura a postura do jogador para não ficar travado na Stance 0
                StanceManager.EndActionStance();
                Phase = 0;
                Timer = 0f;
            }
        }
    }

    /// <summary>
    /// Intercepta o Spawn/Equip de arma nas mãos (GClass2055.Start).
    /// Bloqueia o carregamento automático da primeira bala no spawn da raid ou ao equipar arma com câmara vazia.
    /// Exclusivo para o jogador local (bots seguem o fluxo vanilla).
    /// </summary>
    public class StartEquipWeapPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ChamberWeaponClass).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m =>
                m.GetParameters().Length == 1
                && m.GetParameters()[0].Name == "onWeaponAppear");
        }

        [PatchPrefix]
        private static bool Prefix(ChamberWeaponClass __instance, Action onWeaponAppear)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value) return true;

                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null) return true;

                var player = Traverse.Create(fc).Field<Player>("_player").Value;
                // Exclusividade do jogador local: bots e armas estacionárias seguem o vanilla
                if (player == null || !player.IsYourPlayer || player.MovementContext == null || player.MovementContext.CurrentState == null || player.MovementContext.CurrentState.Name == EPlayerState.Stationary) return true;

                int chamberAmmoCount = __instance.Weapon_0.ChamberAmmoCount;
                bool isSpawnEquip = ManualChamberingState.JustSpawned;
                ManualChamberingState.JustSpawned = false;

                if (__instance.Weapon_0.HasChambers && chamberAmmoCount == 0)
                {
                    bool blockThisEquip = isSpawnEquip ? Plugin._ManualChamberingOnRaidStart.Value : true;
                    if (blockThisEquip)
                    {
                        ManualChamberingState.CanLoadChamber = false;
                        ManualChamberingState.BlockChambering = true;
                        Plugin.Logger.LogDebug($"[ManualChamber] Equip vazio: auto-chamber BLOQUEADO (spawn={isSpawnEquip})");
                    }
                }
                else
                {
                    ManualChamberingState.CanLoadChamber = true;
                    ManualChamberingState.BlockChambering = false;
                }

                Action wrappedOnWeaponAppear = () =>
                {
                    try
                    {
                        if (ManualChamberingState.BlockChambering && chamberAmmoCount == 0 && __instance.FirearmsAnimator_0 != null)
                        {
                            __instance.FirearmsAnimator_0.SetAmmoInChamber(0f);
                        }
                    }
                    catch { }
                    onWeaponAppear?.Invoke();
                };

                __instance.Action_0 = wrappedOnWeaponAppear;
                __instance.Start();
                __instance.FirearmsAnimator_0.SetActiveParam(active: true);
                __instance.FirearmsAnimator_0.SetLayerWeight(__instance.FirearmsAnimator_0.LACTIONS_LAYER_INDEX, 0);
                __instance.Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, __instance.Weapon_0.CalculateCellSize().X);

                int currentMagazineCount = __instance.Weapon_0.GetCurrentMagazineCount();
                __instance.MagazineItemClass = __instance.Weapon_0.GetCurrentMagazine();
                __instance.FirearmController_0.AmmoInChamberOnSpawn = chamberAmmoCount;
                if (__instance.Weapon_0.HasChambers)
                {
                    // CR-02-03: Para inibir a animação de engatilhamento no saque, se a câmara estiver vazia e o
                    // bloqueio ativo, definimos temporariamente AmmoInChamber = 1f e BoltCatch = false no Animator.
                    // O Mecanim seleciona o clipe de Draw Tático (empunhar direto sem puxar ferrolho).
                    // Ao final do Draw (wrappedOnWeaponAppear) ou Reset, restauramos para 0f.
                    float animChamberCount = (ManualChamberingState.BlockChambering && chamberAmmoCount == 0) ? 1f : chamberAmmoCount;
                    __instance.FirearmsAnimator_0.SetAmmoInChamber(animChamberCount);
                    if (ManualChamberingState.BlockChambering && chamberAmmoCount == 0)
                    {
                        __instance.FirearmsAnimator_0.SetBoltCatch(false);
                    }
                }
                else
                {
                    __instance.FirearmsAnimator_0.SetHammerArmed(__instance.Weapon_0.Armed);
                }
                if (__instance.Weapon_0.GetCurrentMagazine() is CylinderMagazineItemClass cylinderMagazineItemClass)
                {
                    bool hammerArmed = !__instance.Weapon_0.CylinderHammerClosed;
                    __instance.FirearmsAnimator_0.SetHammerArmed(hammerArmed);
                    __instance.FirearmsAnimator_0.SetCamoraIndex(cylinderMagazineItemClass.CurrentCamoraIndex);
                    for (int i = 0; i < cylinderMagazineItemClass.Count; i++)
                    {
                        if (cylinderMagazineItemClass.Camoras[i].ContainedItem != null)
                        {
                            __instance.Weapon_0.ShellsInChambers[i] = null;
                            __instance.WeaponManagerClass.RemoveShellInWeapon(i);
                        }
                    }
                }
                if (__instance.Weapon_0.IsMultiBarrel)
                {
                    for (int j = 0; j < __instance.Weapon_0.Chambers.Length; j++)
                    {
                        if (__instance.Weapon_0.Chambers[j].ContainedItem != null)
                        {
                            __instance.Weapon_0.ShellsInChambers[j] = null;
                            __instance.WeaponManagerClass.RemoveShellInWeapon(j);
                        }
                    }
                }
                __instance.FirearmsAnimator_0.SetAmmoOnMag(currentMagazineCount);
                __instance.Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
                __instance.Player_0.Skills.OnWeaponDraw(__instance.Weapon_0);

                bool compatible = (__instance.Bool_1 = __instance.MagazineItemClass == null || __instance.MagazineItemClass.IsAmmoCompatible(__instance.Weapon_0.Chambers));
                __instance.FirearmsAnimator_0.SetAmmoCompatible(compatible);
                if (__instance.Bool_1 && __instance.MagazineItemClass != null && __instance.MagazineItemClass.Count > 0 && __instance.FirearmController_0.Item.Chambers.Length != 0 && __instance.Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
                {
                    __instance.FirearmsAnimator_0.SetLayerWeight(__instance.FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
                }

                if (ManualChamberingState.CanLoadChamber && __instance.MagazineItemClass != null && chamberAmmoCount == 0 && currentMagazineCount > 0 && compatible && __instance.FirearmController_0.Item.Chambers.Length != 0)
                {
                    Weapon.EMalfunctionState state = __instance.FirearmController_0.Item.MalfState.State;
                    if (state == Weapon.EMalfunctionState.Misfire)
                    {
                        __instance.FirearmController_0.Item.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
                    }
                    GStruct154<GInterface424> gStruct = __instance.MagazineItemClass.Cartridges.PopTo(player.InventoryController, __instance.FirearmController_0.Item.Chambers[0].CreateItemAddress());
                    __instance.FirearmController_0.Item.MalfState.ChangeStateSilent(state);
                    if (gStruct.Value != null)
                    {
                        __instance.WeaponManagerClass.RemoveAllShells();
                        __instance.Player_0.UpdatePhones();
                        __instance.AmmoItemClass = (AmmoItemClass)gStruct.Value.ResultItem;
                    }
                }

                return false;
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] StartEquip {ex.Message}"); return true; }
        }
    }

    /// <summary>
    /// CR-02-03: Restaura AmmoInChamber = 0f caso a operação de Draw seja interrompida
    /// abruptamente antes de WeaponAppeared (ex: troca rápida de arma).
    /// </summary>
    public class StartEquipResetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ChamberWeaponClass), nameof(ChamberWeaponClass.Reset));
        }

        [PatchPrefix]
        private static void Prefix(ChamberWeaponClass __instance)
        {
            try
            {
                if (ManualChamberingState.BlockChambering && __instance.FirearmsAnimator_0 != null && __instance.Weapon_0 != null && __instance.Weapon_0.ChamberAmmoCount == 0)
                {
                    __instance.FirearmsAnimator_0.SetAmmoInChamber(0f);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Intercepta o cálculo e criação da operação de recarga externa (GClass2006.Run).
    /// Quando a câmara estiver vazia e _ManualChamberingOnReload estiver ativo para o MainPlayer,
    /// suprime a geração de PopNewAmmoResult, fazendo o EFT concluir o reload sem carregar a câmara.
    /// Bots e outros jogadores no coop seguem o fluxo vanilla.
    /// </summary>
    public class ReloadExternalMagChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ReloadExternalMagResultClass), nameof(ReloadExternalMagResultClass.Run));
        }

        [PatchPrefix]
        private static bool Prefix(
            TraderControllerClass itemController,
            Weapon weapon,
            MagazineItemClass nextMagazine,
            bool quickReload,
            bool isKnownMalfunction,
            ItemAddress vestTargetAddress,
            ref GStruct156<ReloadExternalMagResultClass> __result)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value || !Plugin._ManualChamberingOnReload.Value)
                    return true;

                // Convidado FIKA: a supressão de câmara local ocorre normalmente (UX manual).
                // O inventário do host será atualizado via ChamberStateSyncPacket quando o
                // convidado puxar o ferrolho (RechamberRound). Não retornar early aqui garante
                // que o convidado também experiencie a mecânica manual sem breaking change.
                //
                // Nota: em versões anteriores havia early-return aqui (CanLoadChamber=true) para
                // evitar PlayerIsBusyError. Isso foi removido pois o sync via pacote resolve o
                // problema de forma mais correta — o host recebe o estado real via rede.

                // Guarda de Jogador: Executa para o MainPlayer local e companheiros humanos no coop/FIKA (bots seguem vanilla)
                if (!ManualChamberingPatches.IsHumanInventory(itemController))
                    return true;

                if (weapon == null || weapon.IsStationaryWeapon || !weapon.HasChambers || weapon.Chambers.Length != 1)
                    return true;

                bool isLocalPlayer = Singleton<GameWorld>.Instance?.MainPlayer?.InventoryController == itemController;

                // Se a câmara já tem munição (reload tático com bala na câmara), o reload vanilla roda normalmente
                if (weapon.ChamberAmmoCount > 0)
                {
                    if (isLocalPlayer)
                    {
                        ManualChamberingState.CanLoadChamber = true;
                        ManualChamberingState.BlockChambering = false;
                    }
                    return true;
                }

                // CÂMARA VAZIA + MANUAL CHAMBERING ON RELOAD:
                // Executa a operação de reload mas SUPRIME a colocação automática de munição na câmara (PopNewAmmoResult = null)
                Slot slot = weapon.Chambers[0];
                AmmoItemClass ammoItemClass = slot?.ContainedItem as AmmoItemClass;
                MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
                Slot magazineSlot = weapon.GetMagazineSlot();

                Weapon.EMalfunctionState state = weapon.MalfState.State;
                if (state == Weapon.EMalfunctionState.Misfire)
                {
                    weapon.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
                }

                GStruct154<GClass3410> gStruct = ((ammoItemClass == null || !weapon.MustBoltBeOpennedForExternalReload) ? default(GStruct154<GClass3410>) : InteractionsHandlerClass.Remove(ammoItemClass, itemController));
                weapon.MalfState.ChangeStateSilent(state);
                if (gStruct.Failed)
                {
                    __result = gStruct.Error;
                    return false;
                }

                GStruct154<GClass3410> gStruct2 = default(GStruct154<GClass3410>);
                GStruct154<GClass3411> gStruct3 = default(GStruct154<GClass3411>);
                if (currentMagazine != null)
                {
                    if (vestTargetAddress != null)
                    {
                        gStruct3 = InteractionsHandlerClass.Move(currentMagazine, vestTargetAddress, itemController);
                        if (gStruct3.Failed)
                        {
                            gStruct.Value?.RollBack();
                            __result = gStruct3.Error;
                            return false;
                        }
                    }
                    else
                    {
                        gStruct2 = InteractionsHandlerClass.Remove(currentMagazine, itemController);
                        if (gStruct2.Failed)
                        {
                            gStruct.Value?.RollBack();
                            __result = gStruct2.Error;
                            return false;
                        }
                    }
                }

                GStruct154<GClass3411> gStruct4 = InteractionsHandlerClass.Move(nextMagazine, magazineSlot.CreateItemAddress(), itemController);
                if (gStruct4.Failed)
                {
                    gStruct2.Value?.RollBack();
                    gStruct3.Value?.RollBack();
                    gStruct.Value?.RollBack();
                    __result = gStruct4.Error;
                    return false;
                }

                bool flag = nextMagazine.IsAmmoCompatible(weapon.Chambers);

                // gStruct5 (PopNewAmmoResult) é omitido (fica null), mantendo a câmara vazia!
                if (isLocalPlayer)
                {
                    ManualChamberingState.CanLoadChamber = false;
                    ManualChamberingState.BlockChambering = true;
                }
                Plugin.Logger.LogDebug("[ManualChamber] Reload com câmara vazia: auto-chamber bloqueado, mantendo câmara vazia.");

                __result = new ReloadExternalMagResultClass(
                    itemController,
                    gStruct.Value,
                    gStruct2.Value,
                    gStruct3.Value,
                    gStruct4.Value,
                    null, // PopNewAmmoResult = null
                    weapon,
                    flag,
                    quickReload,
                    isKnownMalfunction);

                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[ManualChamber] ReloadExternalMagChamberPatch {ex}");
                return true;
            }
        }
    }

    public class StartReloadResetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ReloadWeaponClass).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m =>
                m.Name == "Start"
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType == typeof(Callback));
        }

        [PatchPrefix]
        private static void Prefix(ReloadWeaponClass __instance)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value) return;
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;

                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null) return;

                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                if (Plugin._ManualChamberingOnReload.Value && fc.Weapon.ChamberAmmoCount == 0)
                {
                    if (player.IsYourPlayer)
                    {
                        ManualChamberingState.CanLoadChamber = false;
                        ManualChamberingState.BlockChambering = true;
                    }
                    // Informa temporariamente ao Mecanim AmmoInChamber = 1f para que ele selecione
                    // a transição suave de Tactical Reload / Hands.RELOAD END sem ciclar o ferrolho.
                    __instance.FirearmsAnimator_0.SetAmmoInChamber(1f);
                    if (!fc.Weapon.MustBoltBeOpennedForExternalReload)
                    {
                        __instance.FirearmsAnimator_0.SetBoltCatch(false);
                    }
                    Plugin.Logger.LogDebug("[ManualChamber] Reload start: câmara vazia forçada para Tactical Reload no Animator");
                }
                else
                {
                    if (player.IsYourPlayer)
                    {
                        ManualChamberingState.CanLoadChamber = true;
                        ManualChamberingState.BlockChambering = false;
                    }
                    Plugin.Logger.LogDebug("[ManualChamber] Reload start: flags resetadas (CanLoad=true, Block=false)");
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] StartReloadReset {ex.Message}"); }
        }

        [PatchPostfix]
        private static void Postfix(ReloadWeaponClass __instance)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value || !Plugin._ManualChamberingOnReload.Value) return;
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;

                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null || fc.Weapon.ChamberAmmoCount > 0) return;

                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                // Garante que o Mecanim mantenha AmmoInChamber = 1f após o Start()
                if (__instance.FirearmsAnimator_0 != null)
                {
                    __instance.FirearmsAnimator_0.SetAmmoInChamber(1f);
                    if (!fc.Weapon.MustBoltBeOpennedForExternalReload)
                    {
                        __instance.FirearmsAnimator_0.SetBoltCatch(false);
                    }
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Restaura AmmoInChamber = 0f no Animator quando o evento OnIdleStartEvent dispara na operação Idle,
    /// indicando que o clipe de recarga concluiu suavemente o retorno para a empunhadura (Idle).
    /// </summary>
    public class IdleStartEventPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(IdleWeaponOpClass), nameof(IdleWeaponOpClass.OnIdleStartEvent));
        }

        [PatchPostfix]
        private static void Postfix(IdleWeaponOpClass __instance)
        {
            try
            {
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;
                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                var weapon = __instance.Weapon_0;
                var animator = __instance.FirearmsAnimator_0;

                // ref: CR-06-03 - Garante que a arma e o animator correspondam ao FirearmController ativo nas mãos
                if (player.HandsController is Player.FirearmController fc && fc.Weapon == weapon && animator != null && weapon.ChamberAmmoCount == 0)
                {
                    animator.SetAmmoInChamber(0f);
                    if (!weapon.MustBoltBeOpennedForExternalReload)
                    {
                        animator.SetBoltCatch(false);
                    }
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Restaura AmmoInChamber = 0f no Animator caso a operação de recarga ainda esteja ativa
    /// quando o clipe disparar OnIdleStartEvent.
    /// </summary>
    public class ReloadIdleStartEventPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ReloadWeaponClass), nameof(ReloadWeaponClass.OnIdleStartEvent));
        }

        [PatchPostfix]
        private static void Postfix(ReloadWeaponClass __instance)
        {
            try
            {
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;
                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                var weapon = __instance.Weapon_0;
                var animator = __instance.FirearmsAnimator_0;
                if (weapon != null && animator != null && weapon.ChamberAmmoCount == 0)
                {
                    animator.SetAmmoInChamber(0f);
                    if (!weapon.MustBoltBeOpennedForExternalReload)
                    {
                        animator.SetBoltCatch(false);
                    }
                }

                // Salvaguarda: Se a recarga atingiu OnIdleStartEvent (retorno à postura idle)
                // e o estado da operação ainda não estiver Finalizado (State != Finished),
                // invoca SwitchToIdlingState() para emitir CommandStatus.Succeed e destravar
                // o inventário (evitando que o magazine fique indefinidamente piscando em List_0).
                if (player.IsYourPlayer && __instance.State != Player.EOperationState.Finished)
                {
                    // ref: CR-06-06 - Log de warning rate-limited para evitar poluição do console
                    ThrottledLog.Warning("ManualChamber", "ReloadIdleStartEvent: Operação de recarga ainda pendente no retorno a idle. Forçando SwitchToIdlingState() para destravar o inventário.");
                    __instance.SwitchToIdlingState();
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[ManualChamber] ReloadIdleStartEventPatch: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// CR-02-02: Restaura AmmoInChamber = 0f caso a operação de reload seja interrompida
    /// abruptamente antes do fim (ex: sprint imediato).
    /// </summary>
    public class ReloadResetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ReloadWeaponClass), nameof(ReloadWeaponClass.Reset));
        }

        [PatchPrefix]
        private static void Prefix(ReloadWeaponClass __instance)
        {
            try
            {
                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                if (__instance.FirearmsAnimator_0 != null && __instance.Weapon_0 != null && __instance.Weapon_0.ChamberAmmoCount == 0)
                {
                    __instance.FirearmsAnimator_0.SetAmmoInChamber(0f);
                    __instance.FirearmsAnimator_0.SetBoltCatch(false);
                }

                // ref: CR-06-04 - Reset defensivo de estado se a recarga do jogador local for interrompida antes do fim
                if (player.IsYourPlayer)
                {
                    ManualChamberingState.BlockChambering = false;
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Intercepta a instalação de magazine via inventário (GClass2005.Run).
    /// Quando a câmara estiver vazia e _ManualChamberingOnReload estiver ativo para o MainPlayer,
    /// suprime a colocação automática de munição na câmara (PopTo), deixando PopNewAmmoResult = null (HasNewAmmo = false).
    /// Assim, ao inserir o magazine, o EFT encerra a operação no evento OnMagInsertedToWeapon chamando method_5 (Idle),
    /// sem puxar o ferrolho e mantendo a câmara vazia.
    /// </summary>
    public class InstallMagChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InstallMagResultClass), nameof(InstallMagResultClass.Run));
        }

        [PatchPrefix]
        private static bool Prefix(
            InventoryController inventoryController,
            Weapon weapon,
            string playerId,
            ref GStruct156<InstallMagResultClass> __result)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value || !Plugin._ManualChamberingOnReload.Value)
                    return true;

                // Guarda de Jogador: Executa para o MainPlayer local e companheiros humanos no coop/FIKA (bots seguem vanilla)
                if (!ManualChamberingPatches.IsHumanInventory(inventoryController))
                    return true;

                if (weapon == null || weapon.IsStationaryWeapon || !weapon.HasChambers || weapon.Chambers.Length != 1)
                    return true;

                bool isLocalPlayer = Singleton<GameWorld>.Instance?.MainPlayer?.InventoryController == inventoryController;

                // Se a câmara já tem munição, segue o fluxo vanilla normal
                if (weapon.ChamberAmmoCount > 0)
                {
                    if (isLocalPlayer)
                    {
                        ManualChamberingState.CanLoadChamber = true;
                        ManualChamberingState.BlockChambering = false;
                    }
                    return true;
                }

                // CÂMARA VAZIA:
                // Instala o carregador mas SUPRIME a alimentação automática para a câmara
                Slot slot = weapon.Chambers[0];
                MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();
                bool ammoCompatible = currentMagazine != null && currentMagazine.IsAmmoCompatible(weapon.Chambers);

                GStruct154<GClass3410> removeResult = default(GStruct154<GClass3410>);
                if (slot != null && weapon.MustBoltBeOpennedForExternalReload && slot.ContainedItem != null)
                {
                    removeResult = InteractionsHandlerClass.Remove(slot.ContainedItem, inventoryController, false);
                }
                if (removeResult.Failed)
                {
                    __result = removeResult.Error;
                    return false;
                }

                Weapon.EMalfunctionState state = weapon.MalfState.State;
                bool isMalfClearable = state == Weapon.EMalfunctionState.None || (state == Weapon.EMalfunctionState.Feed && weapon.MalfState.IsKnownMalfunction(playerId));
                if (isMalfClearable && state != Weapon.EMalfunctionState.None)
                {
                    weapon.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
                }

                // SUPRIME O POPTO: deixamos popNewAmmoResult = null
                if (isMalfClearable && state != Weapon.EMalfunctionState.None)
                {
                    weapon.MalfState.ChangeStateSilent(state);
                }

                if (isLocalPlayer)
                {
                    ManualChamberingState.CanLoadChamber = false;
                    ManualChamberingState.BlockChambering = true;
                }
                Plugin.Logger.LogDebug("[ManualChamber] InstallMag (inventário): auto-chamber suprimido, mantendo câmara vazia e HasNewAmmo = false.");

                __result = new InstallMagResultClass(weapon, currentMagazine, ammoCompatible, removeResult.Value, null);
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[ManualChamber] InstallMagChamberPatch: {ex}");
                return true;
            }
        }
    }

    /// <summary>
    /// Configura os parâmetros do Animator ao iniciar a inserção de magazine pelo inventário.
    /// Se a câmara estiver vazia, define temporariamente AmmoInChamber = 1f e BoltCatch = false
    /// para que o Mecanim execute suavemente a transição MAG IN NOT CHAMBERED para Idle sem puxar o ferrolho.
    /// </summary>
    public class InstallMagStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(InstallMagOperationClass),
                nameof(InstallMagOperationClass.Start),
                new Type[] { typeof(InstallMagResultClass), typeof(Callback) });
        }

        [PatchPrefix]
        private static void Prefix(InstallMagOperationClass __instance, InstallMagResultClass insertMagResult, Callback callback)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value || !Plugin._ManualChamberingOnReload.Value) return;
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;

                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                var weapon = __instance.Weapon_0;
                if (weapon == null || weapon.ChamberAmmoCount > 0) return;

                if (__instance.FirearmsAnimator_0 != null)
                {
                    __instance.FirearmsAnimator_0.SetAmmoInChamber(1f);
                    if (!weapon.MustBoltBeOpennedForExternalReload)
                    {
                        __instance.FirearmsAnimator_0.SetBoltCatch(false);
                    }
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] InstallMagStartPatch: {ex.Message}"); }
        }

        [PatchPostfix]
        private static void Postfix(InstallMagOperationClass __instance, InstallMagResultClass insertMagResult, Callback callback)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value || !Plugin._ManualChamberingOnReload.Value) return;
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;

                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                var weapon = __instance.Weapon_0;
                if (weapon == null || weapon.ChamberAmmoCount > 0) return;

                if (__instance.FirearmsAnimator_0 != null)
                {
                    __instance.FirearmsAnimator_0.SetAmmoInChamber(1f);
                    if (!weapon.MustBoltBeOpennedForExternalReload)
                    {
                        __instance.FirearmsAnimator_0.SetBoltCatch(false);
                    }
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Salvaguarda no evento OnMagInsertedToWeapon da inserção de magazine pelo inventário.
    /// Se a câmara estiver vazia e a operação ainda não estiver Finalizada (State != Finished),
    /// força method_5() para emitir Callback_0.Succeed() e destravar o inventário,
    /// impedindo terminantemente que o carregador pisque indefinidamente.
    /// </summary>
    public class InstallMagInsertedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InstallMagOperationClass), nameof(InstallMagOperationClass.OnMagInsertedToWeapon));
        }

        [PatchPostfix]
        private static void Postfix(InstallMagOperationClass __instance)
        {
            try
            {
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;
                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                var weapon = __instance.Weapon_0;
                if (weapon == null) return;

                // Se a câmara estiver vazia e a operação ainda não estiver Finalizada,
                // invoca method_5() para concluir a transação de inventário imediatamente (apenas jogador local).
                if (player.IsYourPlayer && weapon.ChamberAmmoCount == 0 && __instance.State != Player.EOperationState.Finished)
                {
                    // ref: CR-06-06 - Log de warning rate-limited para evitar poluição do console
                    ThrottledLog.Warning("ManualChamber", "InstallMag: Forçando method_5() no OnMagInsertedToWeapon para destravar o inventário.");
                    __instance.method_5();
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[ManualChamber] InstallMagInsertedPatch: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Restaura AmmoInChamber = 0f no Animator caso a operação de inserção seja interrompida (Reset)
    /// e garante que qualquer callback pendente seja concluído com segurança para não travar o inventário.
    /// </summary>
    public class InstallMagResetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InstallMagOperationClass), nameof(InstallMagOperationClass.Reset));
        }

        [PatchPrefix]
        private static void Prefix(InstallMagOperationClass __instance)
        {
            try
            {
                if (Singleton<GameWorld>.Instance?.MainPlayer == null) return;
                var player = __instance.Player_0;
                if (player == null || !ManualChamberingPatches.IsHumanPlayer(player)) return;

                if (__instance.Weapon_0 != null && __instance.Weapon_0.ChamberAmmoCount == 0)
                {
                    if (__instance.FirearmsAnimator_0 != null)
                    {
                        __instance.FirearmsAnimator_0.SetAmmoInChamber(0f);
                        __instance.FirearmsAnimator_0.SetBoltCatch(false);
                    }
                }

                // Salvaguarda: se a operação for resetada com callback pendente, conclui para não travar o inventário local
                if (player.IsYourPlayer && __instance.Callback_0 != null)
                {
                    // ref: CR-06-06 - Log de warning rate-limited para evitar poluição do console
                    ThrottledLog.Warning("ManualChamber", "InstallMagReset: Callback pendente no Reset. Concluindo para não travar o inventário.");
                    var cb = __instance.Callback_0;
                    __instance.Callback_0 = null;
                    cb.Succeed();
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Gerencia a funcionalidade de Manual Bolt Action em rifles de ferrolho.
    /// Impede o ciclo automático ao soltar o mouse em SetTriggerPressed(),
    /// mantendo a cápsula deflagrada na câmara até que o jogador aperte Shift + T.
    /// </summary>
    public class ManualBoltActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(DefaultWeaponOpClass), nameof(DefaultWeaponOpClass.SetTriggerPressed));
        }

        [PatchPrefix]
        private static bool Prefix(DefaultWeaponOpClass __instance, bool pressed)
        {
            try
            {
                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null) return true;

                bool isPump = ManualChamberingPatches.IsPumpActionShotgun(fc.Weapon);
                bool enabled = isPump ? (Plugin._EnableManualPumpAction?.Value == true) : (Plugin._EnableManualBoltAction?.Value == true);
                if (!enabled) return true;

                var player = Traverse.Create(fc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return true;

                // Atualiza o estado lógico do gatilho
                fc.IsTriggerPressed &= pressed;

                // Se o ciclo não foi disparado explicitamente pelo Shift + T, mantém o ferrolho parado
                if (!ManualChamberingState.AllowManualBoltActionCycle)
                {
                    __instance.FirearmsAnimator_0.SetBoltActionReload(false);
                    return false; // bloqueia chamada nativa que acionaria o ferrolho ao soltar o mouse
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualBolt] SetTriggerPressed {ex.Message}"); }
            return true;
        }
    }

    /// <summary>
    /// CR-02-01: Garante que cliques rápidos (tap no mouse) não ciclem o ferrolho no Start().
    /// </summary>
    public class ManualBoltActionStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(DefaultWeaponOpClass), nameof(DefaultWeaponOpClass.Start));
        }

        [PatchPostfix]
        private static void Postfix(DefaultWeaponOpClass __instance)
        {
            try
            {
                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null) return;

                bool isPump = ManualChamberingPatches.IsPumpActionShotgun(fc.Weapon);
                bool enabled = isPump ? (Plugin._EnableManualPumpAction?.Value == true) : (Plugin._EnableManualBoltAction?.Value == true);
                if (!enabled) return;

                var player = Traverse.Create(fc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                if (!ManualChamberingState.AllowManualBoltActionCycle)
                {
                    __instance.FirearmsAnimator_0.SetBoltActionReload(false);
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualBolt] Start {ex.Message}"); }
        }
    }

    /// <summary>
    /// CR-02-04: Bloqueia o despacho automático de rede FIKA/cliente em Class1268.method_14
    /// até que o ciclo manual seja acionado via Shift + T.
    /// </summary>
    public class ManualBoltActionNetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ClientBoltOpClass), "method_14");
        }

        [PatchPrefix]
        private static bool Prefix(ClientBoltOpClass __instance, bool value)
        {
            try
            {
                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null) return true;

                bool isPump = ManualChamberingPatches.IsPumpActionShotgun(fc.Weapon);
                bool enabled = isPump ? (Plugin._EnableManualPumpAction?.Value == true) : (Plugin._EnableManualBoltAction?.Value == true);
                if (!enabled) return true;

                var player = Traverse.Create(fc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return true;

                if (value && !ManualChamberingState.AllowManualBoltActionCycle)
                {
                    return false; // bloqueia pacote de rede automático
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualBolt] method_14 {ex.Message}"); }
            return true;
        }
    }

    public class ManualChamberingInputPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GamePlayerOwner), nameof(GamePlayerOwner.TranslateCommand));
        }

        private static void RechamberRound(Player.FirearmController fc, Player player)
        {
            ManualChamberingState.CanLoadChamber = true;
            ManualChamberingState.BlockChambering = false;

            int currentMagazineCount = fc.Weapon.GetCurrentMagazineCount();
            MagazineItemClass mag = fc.Weapon.GetCurrentMagazine();
            if (mag == null) return;

            fc.FirearmsAnimator.SetAmmoInChamber(0f);
            fc.FirearmsAnimator.SetAmmoOnMag(currentMagazineCount);
            fc.FirearmsAnimator.SetAmmoCompatible(true);

            GStruct154<GInterface424> gstruct = mag.Cartridges.PopTo(player.InventoryController, fc.Item.Chambers[0].CreateItemAddress());
            WeaponManagerClass weaponStateClass = Traverse.Create(fc).Field("weaponManagerClass").GetValue<WeaponManagerClass>();

            if (weaponStateClass != null && gstruct.Value != null)
            {
                Plugin.Logger.LogInfo("[ManualChamber] RechamberRound Iniciado: Aguardando transição de stance.");
                StanceManager.StartActionStance();

                var comp = player.gameObject.GetOrAddComponent<ManualChamberingComponent>();
                comp.Player = player;
                comp.FirearmController = fc;
                comp.WeaponStateClass = weaponStateClass;
                comp.Bullet = (AmmoItemClass)gstruct.Value.ResultItem;
                comp.Phase = 1;
                comp.Timer = 0f;

                // Notificar o host (se for convidado FIKA) que a câmara foi carregada manualmente.
                // O host aplica PopTo autoritativo no inventário do jogador remoto.
                // Em sessão solo, SendChamberState é no-op (guarda IsClient interna).
                CameraRotationMod.Networking.FikaSyncManager.SendChamberState(
                    player.ProfileId, fc.Weapon.Id, chamberFilled: true);
            }
        }

        [PatchPrefix]
        private static bool Prefix(GamePlayerOwner __instance, ECommand command)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value && !Plugin._EnableManualBoltAction.Value && !Plugin._EnableManualPumpAction.Value) return true;

                if (command == ECommand.UnloadMagazine)
                {
                    var player = __instance.Player;
                    if (player != null && player.IsYourPlayer)
                    {
                        StanceManager.StartActionStance();
                    }
                }

                if (command == ECommand.ChamberUnload)
                {
                    var player = __instance.Player;
                    if (player == null || !player.IsYourPlayer) return true;

                    var fc = player.HandsController as Player.FirearmController;
                    if (fc == null || fc.Weapon == null) return true;

                    // CR-03-02: Se a arma estiver em pane mecânica (Jam, Feed, Misfire), preserva o pipeline vanilla
                    if (fc.Weapon.MalfState.State != Weapon.EMalfunctionState.None) return true;

                    // Caso 1: Sniper Bolt Action OU Escopeta Pump aguardando ciclo manual via comando de câmara
                    if ((Plugin._EnableManualBoltAction.Value || Plugin._EnableManualPumpAction.Value) && fc.Weapon.BoltAction && fc.CurrentOperation is DefaultWeaponOpClass boltOp)
                    {
                        Plugin.Logger.LogInfo("[ManualChamber] Ciclando ferrolho/bomba manual via comando de câmara");
                        StanceManager.StartActionStance();
                        ManualChamberingState.AllowManualBoltActionCycle = true;
                        boltOp.FirearmsAnimator_0.SetBoltActionReload(true);
                        if (boltOp is ClientBoltOpClass clientBoltOp)
                        {
                            clientBoltOp.method_14(true);
                        }
                        ManualChamberingState.AllowManualBoltActionCycle = false;
                        return false;
                    }

                    if (!Plugin._EnableManualChambering.Value) return true;

                    if (fc.Weapon.HasChambers && fc.Weapon.Chambers.Length == 1)
                    {
                        // Verifica se a câmara contém QUALQUER item físico (bala viva OU cápsula deflagrada)
                        bool chamberHasItem = fc.Weapon.FirstLoadedChamberSlot?.ContainedItem != null;
                        if (!chamberHasItem)
                        {
                            var mag = fc.Weapon.GetCurrentMagazine();
                            if (mag != null && mag.Count > 0)
                            {
                                if (fc.IsAiming) fc.ToggleAim();
                                RechamberRound(fc, player);
                                return false; 
                            }
                            else
                            {
                                // Dry Rack (ciclo de manuseio no seco com magazine vazio ou ausente)
                                if (fc.IsAiming) fc.ToggleAim();
                                StanceManager.StartActionStance();
                                var comp = player.gameObject.GetOrAddComponent<ManualChamberingComponent>();
                                comp.Player = player;
                                comp.FirearmController = fc;
                                comp.WeaponStateClass = Traverse.Create(fc).Field("weaponManagerClass").GetValue<WeaponManagerClass>();
                                comp.Bullet = null;
                                comp.Phase = 1;
                                comp.Timer = 0f;
                                return false;
                            }
                        }
                        else
                        {
                            // Esvaziamento nativo de câmara (bala viva OU cápsula deflagrada):
                            // NUNCA bloqueia. Deixa o pipeline do EFT executar o descarregamento nativo (GClass2024 / Rechamber)
                            Plugin.Logger.LogInfo("[ManualChamber] Câmara ocupada (bala ou cápsula). Permitindo esvaziamento nativo EFT.");
                            if (fc.IsAiming) fc.ToggleAim();
                            StanceManager.StartActionStance();
                            return true;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] ChamberInput {ex.Message}"); return true; }
        }
    }

    /// <summary>
    /// CR-03-03: Garante que o interior da câmara seja visualmente limpo antes de abrir o ferrolho
    /// na inspeção de câmara (CheckChamber), destruindo modelos 3D residuais quando a câmara estiver vazia.
    /// </summary>
    public class ChamberCheckModelCleanupPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(IdleWeaponOpClass), nameof(IdleWeaponOpClass.CheckChamber));
        }

        [PatchPrefix]
        private static void Prefix(IdleWeaponOpClass __instance)
        {
            try
            {
                var fc = __instance.FirearmController_0;
                if (fc != null && fc.Weapon != null && fc.Weapon.ChamberAmmoCount == 0)
                {
                    ManualChamberingPatches.CleanResidualChamberModel(fc);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// CR-03-03: Limpa modelos 3D residuais quando uma operação de descarregar câmara conclui e retorna para Idle.
    /// </summary>
    public class ChamberUnloadModelCleanupPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController.RechamberOperationClass), nameof(Player.FirearmController.RechamberOperationClass.SwitchToIdle));
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController.RechamberOperationClass __instance)
        {
            try
            {
                var fc = __instance.FirearmController_0;
                if (fc != null && fc.Weapon != null)
                {
                    if (fc.Weapon.ChamberAmmoCount == 0)
                    {
                        ManualChamberingPatches.CleanResidualChamberModel(fc);
                    }
                    fc.Weapon.Armed = true;
                    if (fc.FirearmsAnimator != null)
                    {
                        fc.FirearmsAnimator.SetHammerArmed(true);
                    }
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Corrige o esvaziamento de câmara quando o carregador está vazio ou ausente (Item_1 == null).
    /// No EFT vanilla, se Item_1 == null, OnShellEjectEvent transiciona prematuramente para Idle
    /// antes do evento RemoveAmmoFromChamber ser disparado, impedindo a remoção da munição da câmara.
    /// Este patch garante que a bala seja removida da câmara, ejetada e o percussor seja engatilhado.
    /// </summary>
    public class RechamberOperationEmptyMagFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController.RechamberOperationClass), nameof(Player.FirearmController.RechamberOperationClass.OnShellEjectEvent));
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController.RechamberOperationClass __instance)
        {
            try
            {
                if (__instance == null || __instance.Weapon_0 == null) return;

                // Se não há próxima bala para alimentar do magazine (carregador vazio ou ausente)
                if (__instance.Item_1 == null)
                {
                    // Garante a remoção da munição da câmara antes do SwitchToIdle()
                    if (!__instance.Bool_0)
                    {
                        __instance.RemoveAmmoFromChamber();
                    }

                    // Garante que o percussor mecânico seja armado na puxada do ferrolho
                    __instance.Weapon_0.Armed = true;
                    if (__instance.FirearmsAnimator_0 != null)
                    {
                        __instance.FirearmsAnimator_0.SetHammerArmed(true);
                    }

                    // Se a arma possui bolt catch e o magazine está vazio, aciona a trava
                    var currentMag = __instance.Weapon_0.GetCurrentMagazine();
                    bool isExternal = __instance.Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazine || __instance.Weapon_0.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport;
                    bool hasAmmoInMag = currentMag != null && currentMag.Count > 0;
                    if (__instance.Weapon_0.IsBoltCatch && !hasAmmoInMag && ((isExternal && currentMag != null) || !isExternal))
                    {
                        __instance.OnOnOffBoltCatchEvent(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[ManualChamber] RechamberEmptyMagFix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// CR-03-04: Ativa o modo de repetição manual (BoltAction = true) para escopetas pump-action
    /// quando a opção _EnableManualPumpAction estiver ligada no F12.
    /// </summary>
    public class WeaponBoltActionGetterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Weapon), nameof(Weapon.BoltAction));
        }

        [PatchPostfix]
        private static void Postfix(Weapon __instance, ref bool __result)
        {
            try
            {
                if (!__result && Plugin._EnableManualPumpAction != null && Plugin._EnableManualPumpAction.Value)
                {
                    if (ManualChamberingPatches.IsPumpActionShotgun(__instance))
                    {
                        __result = true;
                    }
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Ao pressionar o gatilho com câmara vazia, desliga o percussor (Armed = false e SetHammerArmed = false),
    /// mantendo a animação do gatilho (SetFire) e o clique seco (DryShot) sempre disponíveis a cada clique.
    /// </summary>
    public class DryFireFeedbackPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(IdleWeaponOpClass), nameof(IdleWeaponOpClass.SetTriggerPressed));
        }

        [PatchPrefix]
        private static bool Prefix(IdleWeaponOpClass __instance, bool pressed)
        {
            try
            {
                if (!pressed) return true;
                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null) return true;
                var player = Traverse.Create(fc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return true;

                if (fc.Weapon.HasChambers && fc.Weapon.ChamberAmmoCount == 0 && fc.Weapon.MalfState.State == Weapon.EMalfunctionState.None)
                {
                    if (fc.Weapon.Armed)
                    {
                        fc.Weapon.Armed = false;
                        if (fc.FirearmsAnimator != null)
                        {
                            fc.FirearmsAnimator.SetHammerArmed(false);
                        }
                        Plugin.Logger.LogDebug("[ManualChamber] Dry Fire: cão desarmado no disparo seco");
                    }
                    // Mantém SetFire(pressed) e DryShot() sempre disponíveis a cada tentativa de tiro sem munição
                    return true;
                }
            }
            catch { }
            return true;
        }
    }

    /// <summary>
    /// Impede que o Animation Event IEventsConsumerOnArm() da inspeção de câmara (CheckChamber)
    /// arme indevidamente o cão quando a arma já estava desarmada (após Dry Fire).
    /// O cão só deve ser rearmado ao puxar o ferrolho até o fim (Dry Rack / Esvaziar câmara ou disparo real).
    /// </summary>
    public class CheckChamberArmPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.IEventsConsumerOnArm));
        }

        [PatchPrefix]
        private static bool Prefix(Player.FirearmController __instance)
        {
            try
            {
                if (__instance == null || __instance.Weapon == null) return true;

                // Se a operação atual for CheckChamber (GClass2038 do tipo CheckChamber)
                if (__instance.CurrentOperation is Player.FirearmController.GClass2038 utilityOp
                    && utilityOp.EutilityType_0 == Player.FirearmController.GClass2038.EUtilityType.CheckChamber)
                {
                    // Se o cão já estava desarmado antes da checagem de câmara, bloqueia o rearme
                    if (!__instance.Weapon.Armed)
                    {
                        Plugin.Logger.LogDebug("[ManualChamber] CheckChamber: suprimindo rearme indevido do cão no press check.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[ManualChamber] CheckChamberArmPatch: {ex.Message}");
            }
            return true;
        }
    }
}
