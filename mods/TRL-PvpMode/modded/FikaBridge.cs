using System;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Ponte para os membros do Fika e do EFT que o C# não alcança diretamente:
    /// tipos <c>internal</c> (Bleedout, ReviveInteractable) e campos <c>protected</c>/privados.
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

        private static FieldInfo _lastDamageInfoField;   // EFT Player.LastDamageInfo (protected)
        private static FieldInfo _bleedoutTimeBackingField;

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

            // ref: Assembly-CSharp/EFT/Player.cs:24360 — protected DamageInfoStruct LastDamageInfo
            _lastDamageInfoField = AccessTools.Field(typeof(Player), "LastDamageInfo");

            // ref: fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs:26
            // Auto-property com inicializador. Escrevemos o campo de apoio em vez de patchar o
            // getter: um getter de duas instruções é candidato a ser embutido pelo compilador,
            // e o patch ficaria inerte sem ninguém perceber (review 01, R-04).
            _bleedoutTimeBackingField = AccessTools.Field(
                typeof(Fika.Core.Main.ClientClasses.ClientHealthController), "<BleedoutTime>k__BackingField");

            LogMissing(ReviveInteractableType == null, "tipo ReviveInteractable");
            LogMissing(BleedoutType == null, "tipo Bleedout");
            LogMissing(_lastDamageInfoField == null, "campo Player.LastDamageInfo");
            LogMissing(_bleedoutTimeBackingField == null, "campo de apoio de ClientHealthController.BleedoutTime");

            IsUsable = ReviveInteractableType != null
                    && BleedoutType != null
                    && _lastDamageInfoField != null;

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
        /// Tipo do último dano sofrido pelo jogador. O caminho do Fika (LatestDamageInfo) é
        /// <c>internal</c> e o campo do EFT é <c>protected</c> — daí a reflexão (review 01, R-07).
        /// </summary>
        public static EDamageType GetLastDamageType(Player player)
        {
            if (player == null || _lastDamageInfoField == null) return EDamageType.Undefined;

            try
            {
                var info = (DamageInfoStruct)_lastDamageInfoField.GetValue(player);
                return info.DamageType;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] GetLastDamageType: {ex.Message}");
                return EDamageType.Undefined;
            }
        }

        /// <summary>
        /// Sobrescreve o prazo de sangramento que o Fika leu do servidor. Um único ponto governa
        /// a contagem (Bleedout.Init), o desfecho por tempo (ShouldBleedOut) e o número na tela
        /// (Bleedout.ShowUI). Precisa ser aplicado ANTES da queda — é chamado no início da raid.
        /// </summary>
        public static bool TrySetBleedoutTime(Fika.Core.Main.ClientClasses.ClientHealthController controller, float seconds)
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
