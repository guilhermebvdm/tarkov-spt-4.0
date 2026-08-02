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
    // Suave de proposito: sem o TRL-Fixes o mod carrega, mas todo jogador que renascer fica
    // impossivel de acertar para os outros - o RemoveRagdoll do Fika devolve os colisores na
    // camada errada e quem conserta e o postfix do com.trl.fixes. Avisamos na tela em vez de
    // recusar a carga (review coop, G-05).
    [BepInDependency(TrlFixesGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.trl.pvpmode";
        public const string PluginName = "TRL-PvpMode";
        public const string PluginVersion = "0.11.0";
        public const string TrlFixesGuid = "com.trl.fixes";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        /// <summary>
        /// Carregamento falhou no meio. O Unity aborta o Awake mas SEGUE chamando Update/OnGUI,
        /// entao sem esta marca o mod ficaria meio-carregado: sem patches, mas ainda registrando
        /// pacote e mexendo em peers observados, e sem nada no log (H-04).
        /// </summary>
        private static bool _bootFailed;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            try
            {
                Boot();
            }
            catch (Exception ex)
            {
                _bootFailed = true;
                Log.LogError($"[{PluginName}] Falha no carregamento — mod DESATIVADO nesta sessao: {ex}");
            }
        }

        private void Boot()
        {
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
            if (_bootFailed) return;
            Networking.RespawnNetwork.EnsurePacketsRegistered();
            Networking.RespawnNetwork.TickGuards();
        }

        private void OnGUI()
        {
            if (_bootFailed) return;
            LivesHud.Draw();
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
            TryEnable(new BlockJumpWhileDownedPatch(), nameof(BlockJumpWhileDownedPatch));
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
