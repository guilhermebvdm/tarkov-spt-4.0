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
        public const string PluginVersion = "0.2.0";

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

        /// <summary>Patches que só dependem de tipos públicos — sempre seguros de habilitar.</summary>
        private void EnableCorePatches()
        {
            try
            {
                new GameWorldOnGameStartedPatch().Enable();
                new GameWorldOnDestroyPatch().Enable();
                new CanBeRevivedByOtherPlayerPatch().Enable();
                new InstantKillPatch().Enable();
            }
            catch (Exception ex)
            {
                Log.LogError($"[{PluginName}] Falha ao habilitar os patches principais: {ex}");
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

            try
            {
                new NoAllyRevivePatch().Enable();
                new BleedoutOnPlayerDeathPatch().Enable();
            }
            catch (Exception ex)
            {
                Log.LogError($"[{PluginName}] Falha ao habilitar os patches do Fika: {ex}");
            }
        }
    }
}
