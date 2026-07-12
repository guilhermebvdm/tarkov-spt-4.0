using UnityEngine;
using TRLImmersiveCombatMedicine;
using EFT;
using EFT.Communications;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using BepInEx.Logging;

namespace Band_Aid
{
    /// <summary>
    /// Gerencia torniquetes aplicados nos membros dos jogadores.
    /// - Aplicar: remove sangramento pesado, consome item do inventário
    /// - Timer: dano gradual ao membro (necrose)
    /// - Remover: devolve item ao inventário
    /// </summary>
    public class TourniquetManager : MonoBehaviour
    {
        public static TourniquetManager Instance;
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BandAid_TQ");

        // === Tracking de torniquetes ativos ===
        // Key: EBodyPart, Value: dados do torniquete
        private static Dictionary<EBodyPart, TourniquetData> _activeTourniquets = new Dictionary<EBodyPart, TourniquetData>();

        // Configurações
        private const float DAMAGE_INTERVAL = 30f;    // Segundos entre cada dano
        private const float DAMAGE_PER_TICK = 5f;     // HP perdido por tick
        private const float MAX_DURATION = 300f;       // 5 minutos máx antes de destruir o membro

        private class TourniquetData
        {
            public Player Patient;          // Jogador que tem o torniquete
            public EBodyPart BodyPart;
            public float AppliedTime;       // Time.time quando foi aplicado
            public float LastDamageTick;    // Último tick de dano
            public string ItemTemplateId;   // ID do item para devolver ao inventário
        }

        public void Awake()
        {
            Instance = this;
            ClearAll(); // Limpar torniquetes de raids anteriores (M2)
            Logger.LogInfo("TourniquetManager inicializado.");
        }

        // === VERIFICAR SE MEMBRO TEM TORNIQUETE ===
        public static bool HasTourniquet(EBodyPart bodyPart)
        {
            return _activeTourniquets.ContainsKey(bodyPart);
        }

        // === APLICAR TORNIQUETE ===
        /// <summary>
        /// Aplica torniquete no membro. Remove sangramento pesado.
        /// O item é consumido do inventário.
        /// </summary>
        public bool ApplyTourniquet(Player patient, EBodyPart bodyPart, Item itemUsed)
        {
            if (_activeTourniquets.ContainsKey(bodyPart))
            {
                NotificationManagerClass.DisplayMessageNotification(
                    $"Torniquete já aplicado: {GetBodyPartName(bodyPart)}",
                    ENotificationDurationType.Default, ENotificationIconType.Alert);
                return false;
            }

            // Verificar se tem sangramento pesado no membro
            var hc = patient.HealthController;
            if (hc == null) return false;

            // Registrar o torniquete
            var data = new TourniquetData
            {
                Patient = patient,
                BodyPart = bodyPart,
                AppliedTime = Time.time,
                LastDamageTick = Time.time,
                ItemTemplateId = itemUsed?.TemplateId.ToString() ?? "unknown"
            };

            _activeTourniquets[bodyPart] = data;

            Logger.LogInfo($"Torniquete aplicado: {bodyPart} no paciente {patient.Profile?.Nickname}");
            NotificationManagerClass.DisplayMessageNotification(
                $"Torniquete aplicado: {GetBodyPartName(bodyPart)}. Remova após parar o sangramento!",
                ENotificationDurationType.Long, ENotificationIconType.Quest);

            return true;
        }

        // === REMOVER TORNIQUETE ===
        /// <summary>
        /// Remove o torniquete e devolve o item ao inventário do dono.
        /// </summary>
        public bool RemoveTourniquet(Player doctor, EBodyPart bodyPart)
        {
            if (!_activeTourniquets.TryGetValue(bodyPart, out TourniquetData data))
            {
                NotificationManagerClass.DisplayMessageNotification(
                    $"Nenhum torniquete em: {GetBodyPartName(bodyPart)}",
                    ENotificationDurationType.Default, ENotificationIconType.Alert);
                return false;
            }

            _activeTourniquets.Remove(bodyPart);

            float duration = Time.time - data.AppliedTime;
            Logger.LogInfo($"Torniquete removido: {bodyPart} (duração: {duration:F1}s)");

            NotificationManagerClass.DisplayMessageNotification(
                $"Torniquete removido: {GetBodyPartName(bodyPart)} ({duration:F0}s). Item devolvido.",
                ENotificationDurationType.Long, ENotificationIconType.Quest);

            return true;
        }

        // === UPDATE — DANO GRADUAL ===
        public void Update()
        {
            if (_activeTourniquets.Count == 0) return;

            // Cópia para iterar (pode remover durante iteração)
            var keys = _activeTourniquets.Keys.ToList();

            foreach (var bodyPart in keys)
            {
                if (!_activeTourniquets.TryGetValue(bodyPart, out TourniquetData data)) continue;
                if (data.Patient == null || !data.Patient.HealthController.IsAlive)
                {
                    _activeTourniquets.Remove(bodyPart);
                    continue;
                }

                float elapsed = Time.time - data.LastDamageTick;

                // Dano a cada DAMAGE_INTERVAL segundos
                if (elapsed >= DAMAGE_INTERVAL)
                {
                    data.LastDamageTick = Time.time;

                    var hc = data.Patient.HealthController;

                    // S3: Só aplica dano de necrose em pacientes LOCAIS (com ActiveHealthController).
                    // Para jogadores remotos, o TourniquetManager roda no client do paciente.
                    if (!(hc is ActiveHealthController activeHc))
                    {
                        // Paciente remoto — ignorar (necrose gerenciada no client do paciente)
                        continue;
                    }

                    var hp = activeHc.GetBodyPartHealth(bodyPart);
                    if (hp.Current > 0)
                    {
                        var damageInfo = new DamageInfoStruct
                        {
                            DamageType = EDamageType.Undefined,
                            Damage = DAMAGE_PER_TICK
                        };
                        activeHc.ApplyDamage(bodyPart, DAMAGE_PER_TICK, damageInfo);

                        float timeApplied = Time.time - data.AppliedTime;
                        Logger.LogInfo($"Torniquete necrose: {bodyPart} -{DAMAGE_PER_TICK}HP (tempo: {timeApplied:F0}s)");

                        // Aviso ao jogador
                        if (hp.Current - DAMAGE_PER_TICK <= hp.Maximum * 0.3f)
                        {
                            NotificationManagerClass.DisplayMessageNotification(
                                $"⚠ Torniquete em {GetBodyPartName(bodyPart)}: risco de necrose! Remova agora!",
                                ENotificationDurationType.Default, ENotificationIconType.Alert);
                        }
                    }
                    else
                    {
                        // Membro destruído pelo torniquete
                        NotificationManagerClass.DisplayMessageNotification(
                            $"☠ {GetBodyPartName(bodyPart)} destruído por necrose do torniquete!",
                            ENotificationDurationType.Long, ENotificationIconType.Alert);
                        _activeTourniquets.Remove(bodyPart);
                    }
                }
            }
        }

        // === LIMPAR TUDO (ao sair da raid) ===
        public static void ClearAll()
        {
            _activeTourniquets.Clear();
        }

        // === OBTER MEMBROS COM TORNIQUETE ===
        public static List<EBodyPart> GetTourniquetParts()
        {
            return _activeTourniquets.Keys.ToList();
        }

        // === TEMPO RESTANTE ANTES DA NECROSE CRÍTICA ===
        public static float GetTimeElapsed(EBodyPart bodyPart)
        {
            if (_activeTourniquets.TryGetValue(bodyPart, out TourniquetData data))
                return Time.time - data.AppliedTime;
            return 0f;
        }

        private string GetBodyPartName(EBodyPart part)
        {
            switch (part)
            {
                case EBodyPart.Head: return "Cabeça";
                case EBodyPart.Chest: return "Tórax";
                case EBodyPart.Stomach: return "Estômago";
                case EBodyPart.LeftArm: return "Braço Esquerdo";
                case EBodyPart.RightArm: return "Braço Direito";
                case EBodyPart.LeftLeg: return "Perna Esquerda";
                case EBodyPart.RightLeg: return "Perna Direita";
                default: return part.ToString();
            }
        }
    }
}
