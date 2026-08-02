using System;
using BepInEx;
using BepInEx.Logging;
using TarkovRedLine.PvpMode.Patches;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// TRL-PvpMode — regras de modo de jogo para o servidor Tarkov Red Line.
    /// Primeiro modo: vidas por raid com renascimento em outro ponto de spawn,
    /// construído sobre o estado "downed" do Fika.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.trl.pvpmode";
        public const string PluginName = "TRL-PvpMode";
        public const string PluginVersion = "0.4.0";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Settings.Init(Config);

            // O Fika é dependência dura, então já está carregado aqui — os tipos internal
            // resolvem antes de qualquer GetTargetMethod precisar deles.
            FikaBridge.Resolve();

            EnableCorePatches();
            EnableFikaInternalPatches();

            Log.LogInfo($"[{PluginName}] {PluginVersion} carregado.");
        }

        /// <summary>
        /// O FIKA destrói e recria o gerenciador de rede a cada troca de sessão, e o novo nasce sem
        /// os registros do mod. Reconferir por quadro é o padrão do repo (AP-11) — a checagem é uma
        /// comparação de referência, não custa nada.
        /// </summary>
        private void Update()
        {
            Networking.RespawnNetwork.EnsurePacketsRegistered();
        }

        /// <summary>Patches que só dependem de tipos públicos — sempre seguros de habilitar.</summary>
        private void EnableCorePatches()
        {
            // Um try POR patch: uma falha isolada não pode derrubar os seguintes em silêncio
            // (code review 01, C-09).
            TryEnable(new GameWorldOnGameStartedPatch(), nameof(GameWorldOnGameStartedPatch));
            TryEnable(new GameWorldOnDestroyPatch(), nameof(GameWorldOnDestroyPatch));
            TryEnable(new KillGatePatch(), nameof(KillGatePatch));
            TryEnable(new CanBeRevivedByOtherPlayerPatch(), nameof(CanBeRevivedByOtherPlayerPatch));
            TryEnable(new InstantKillPatch(), nameof(InstantKillPatch));
            TryEnable(new RespawnInputPatch(), nameof(RespawnInputPatch));
        }

        private void TryEnable(SPT.Reflection.Patching.ModulePatch patch, string name)
        {
            try
            {
                patch.Enable();
            }
            catch (Exception ex)
            {
                Log.LogError($"[{PluginName}] Falha ao habilitar {name}: {ex}");
            }
        }

        /// <summary>
        /// Patches que miram tipos <c>internal</c> do Fika. Se a ponte não resolveu, ficam de fora
        /// em vez de estourar na inicialização — o modo se autodesativa e o motivo já foi logado.
        /// </summary>
        private void EnableFikaInternalPatches()
        {
            if (!FikaBridge.IsUsable)
            {
                Log.LogWarning($"[{PluginName}] Patches dependentes do Fika nao habilitados — ver avisos acima.");
                return;
            }

            TryEnable(new NoAllyRevivePatch(), nameof(NoAllyRevivePatch));
            TryEnable(new BleedoutOnPlayerDeathPatch(), nameof(BleedoutOnPlayerDeathPatch));
        }
    }
}
