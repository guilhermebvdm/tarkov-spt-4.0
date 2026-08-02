using System;
using System.Reflection;
using Fika.Core.Main.ClientClasses;
using HarmonyLib;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Ponte para os membros do Fika que o C# não alcança diretamente: tipos <c>internal</c>
    /// (Bleedout, ReviveInteractable) e o campo de apoio de uma propriedade só-leitura.
    ///
    /// Tudo é resolvido UMA vez em estático (SPT best-practices §3 — nunca reflexão por chamada)
    /// e cada ausência é registrada individualmente, para que uma atualização do Fika que
    /// renomeie um membro apareça no log em vez de virar exceção em plena raid.
    /// </summary>
    internal static class FikaBridge
    {
        private const string NS_COMPONENTS = "Fika.Core.Main.Components.";

        public static Type ReviveInteractableType { get; private set; }
        public static Type BleedoutType { get; private set; }

        private static FieldInfo _bleedoutTimeBackingField;
        private static FieldInfo _maxRevivesField;
        private static FieldInfo _spawnPointsField;
        private static FieldInfo _bledOutField;

        private static bool _resolved;

        /// <summary>
        /// Tudo que este item precisa está disponível. Falso ⇒ o modo se autodesativa
        /// em vez de quebrar a raid.
        /// </summary>
        public static bool IsUsable { get; private set; }

        public static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;

            // ref: fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs:15 (internal sealed)
            ReviveInteractableType = AccessTools.TypeByName(NS_COMPONENTS + "ReviveInteractable");
            // ref: fika-plugin/Fika.Core/Main/Components/Bleedout.cs:11 (internal sealed)
            BleedoutType = AccessTools.TypeByName(NS_COMPONENTS + "Bleedout");

            // ref: fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs:26
            // Auto-property só-leitura com inicializador. Escrevemos o campo de apoio em vez de
            // patchar o getter: um getter de duas instruções é candidato a ser embutido pelo
            // compilador, e o patch ficaria inerte sem ninguém perceber (review 01, R-04).
            _bleedoutTimeBackingField = AccessTools.Field(typeof(ClientHealthController), "<BleedoutTime>k__BackingField");

            // ref: ClientHealthController.cs:29 — readonly int alimentado por ReviveConfig.MaxRevives.
            // É o SEGUNDO termo de CanBeDowned (:22), que não patchamos: com maxRevives=2 no
            // servidor e 5 vidas no F12, o jogador pararia de cair na 3ª morte com 3 vidas ainda
            // no contador e no indicador de tela (code review 002, D-02).
            _maxRevivesField = AccessTools.Field(typeof(ClientHealthController), "_maxRevives");

            // ref: fika-plugin/Fika.Core/Main/GameMode/BaseGameController.cs:130 (protected)
            _spawnPointsField = AccessTools.Field(
                AccessTools.TypeByName("Fika.Core.Main.GameMode.BaseGameController"), "_spawnPoints");

            // ref: ClientHealthController.cs:33 - a UNICA marca confiavel de "acabou de vez".
            // IsAlive NAO serve: o prefixo de Kill do Fika o zera ao entrar no caido
            // (ClientHealthController_Kill_Patch.cs:22) e so ToggleDowned(false) o devolve.
            _bledOutField = AccessTools.Field(typeof(ClientHealthController), "_bledOut");

            LogMissing(ReviveInteractableType == null, "tipo ReviveInteractable");
            LogMissing(_bledOutField == null, "campo ClientHealthController._bledOut");
            LogMissing(_maxRevivesField == null, "campo ClientHealthController._maxRevives");
            LogMissing(_spawnPointsField == null, "campo BaseGameController._spawnPoints");
            LogMissing(BleedoutType == null, "tipo Bleedout");
            LogMissing(_bleedoutTimeBackingField == null, "campo de apoio de ClientHealthController.BleedoutTime");

            // Todos sao exigidos: sem os tipos não há como bloquear o resgate por aliado nem a
            // morte forçada por companheiros; sem o campo, a opção de tempo do F12 seria uma
            // mentira silenciosa (code review 01, C-06).
            IsUsable = ReviveInteractableType != null
                    && BleedoutType != null
                    && _bleedoutTimeBackingField != null
                    && _maxRevivesField != null
                    && _spawnPointsField != null
                    && _bledOutField != null;

            if (!IsUsable)
            {
                Plugin.Log.LogError(
                    "[TRL-PvpMode] Membros do Fika ausentes — o modo de vidas fica DESATIVADO nesta sessao. " +
                    "Provavel atualizacao do Fika: conferir FikaBridge.");
            }
        }

        private static void LogMissing(bool missing, string what)
        {
            if (missing) Plugin.Log.LogWarning($"[TRL-PvpMode] Nao resolvido: {what}");
        }

        /// <summary>
        /// O jogador acabou de vez (o prazo estourou)? Discriminante real do estado terminal.
        /// </summary>
        public static bool HasBledOut(ClientHealthController controller)
        {
            if (controller == null || _bledOutField == null) return false;
            try { return _bledOutField.GetValue(controller) is true; }
            catch { return false; }
        }

        /// <summary>O chat do Fika esta aberto? Tipo e propriedade sao publicos - sem reflexao.</summary>
        public static bool IsChatActive()
        {
            try { return Fika.Core.UI.Custom.FikaChatUIScript.IsActive; }
            catch { return false; }
        }

        private static MethodInfo _removeAllActiveEffectsMethod;

        /// <summary>
        /// Limpa os ícones de efeito da plaquinha de vida de um peer. Método <c>internal</c> do
        /// Fika — é o que o próprio handler de reconexão dele faz junto com a limpeza do histórico
        /// de posições (FikaClient.Callbacks.cs:43). Sem isto, sangramento e fratura de ANTES da
        /// queda ficam pendurados na plaquinha mesmo depois de a cura tê-los removido de verdade
        /// (code review 003, E-03).
        /// </summary>
        public static void TryClearHealthBarEffects(object healthBar)
        {
            if (healthBar == null) return;

            _removeAllActiveEffectsMethod ??= AccessTools.Method(healthBar.GetType(), "RemoveAllActiveEffects");
            if (_removeAllActiveEffectsMethod == null) return;

            try { _removeAllActiveEffectsMethod.Invoke(healthBar, null); }
            catch (Exception ex) { Plugin.Log.LogError($"[TRL-PvpMode] TryClearHealthBarEffects: {ex.Message}"); }
        }

        /// <summary>
        /// Zera o teto de revives do Fika (0 = ilimitado para ele), passando o controle da contagem
        /// inteiramente para o nosso contador de vidas — coerente com o resto do mod, que já assume
        /// por inteiro as decisões que divide com o Fika.
        ///
        /// Sem isto, `maxRevives` no `fika.jsonc` corta o modo pela metade **em silêncio**: o
        /// jogador para de cair mas o indicador continua mostrando vidas (code review 002, D-02).
        /// </summary>
        public static bool TryUncapRevives(ClientHealthController controller)
        {
            if (controller == null || _maxRevivesField == null) return false;

            try
            {
                _maxRevivesField.SetValue(controller, 0);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] TryUncapRevives: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lista de pontos de nascimento da partida, para sortearmos um de verdade.
        /// `SpawnPointManagerClass` implementa `IEnumerable&lt;ISpawnPoint&gt;`.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<EFT.Game.Spawning.ISpawnPoint> GetSpawnPoints()
        {
            if (_spawnPointsField == null) return null;

            try
            {
                var game = Comfort.Common.Singleton<Fika.Core.Main.GameMode.IFikaGame>.Instance;
                var controller = game?.GameController;
                if (controller == null) return null;

                return _spawnPointsField.GetValue(controller)
                    as System.Collections.Generic.IEnumerable<EFT.Game.Spawning.ISpawnPoint>;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] GetSpawnPoints: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sobrescreve o prazo de sangramento que o Fika leu do servidor. Um único ponto governa
        /// a contagem (Bleedout.Init), o desfecho por tempo (ShouldBleedOut) e o número na tela
        /// (Bleedout.ShowUI). Precisa ser aplicado ANTES da queda — é chamado no início da raid.
        /// </summary>
        public static bool TrySetBleedoutTime(ClientHealthController controller, float seconds)
        {
            if (controller == null || _bleedoutTimeBackingField == null) return false;

            try
            {
                _bleedoutTimeBackingField.SetValue(controller, seconds);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] TrySetBleedoutTime: {ex.Message}");
                return false;
            }
        }
    }
}
