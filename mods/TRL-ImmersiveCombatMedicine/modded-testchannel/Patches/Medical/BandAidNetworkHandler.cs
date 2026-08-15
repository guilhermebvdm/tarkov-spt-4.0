using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Communications;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using Fika.Core.Networking.LiteNetLib.Utils; // INetSerializable — constraint do helper SendPacket
using System;
using System.Linq;
using System.Reflection;
using TRLImmersiveCombatMedicine; // PA-01-02 (review técnica 01): MedicDenyReasonId/MedicLocale
                                   // vivem no namespace do plugin — sem este using, CS0103.

namespace Band_Aid
{
    public static class BandAidNetworkHandler
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BandAid_Network");

        // Cache tipos — GInterfaces para LEITURA, nested types para REMOÇÃO
        private static Type _heavyBleedType;   // HeavyBleeding — para HasEffect
        private static Type _lightBleedType;   // LightBleeding — para HasEffect
        private static Type _fractureType;     // GInterface342 — para HasEffect
        // Tipos concretos: necessários para method_15 (RemoveEffectNative)
        private static Type _heavyBleedConcreteType;
        private static Type _lightBleedConcreteType;
        private static Type _fractureConcreteType;
        private static bool _typesCached = false;

        private static void CacheTypes()
        {
            if (_typesCached) return;
            // Tipos concretos do ActiveHC
            var ahcType = typeof(ActiveHealthController);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            _heavyBleedType = ahcType.GetNestedType("HeavyBleeding", flags);
            _lightBleedType = ahcType.GetNestedType("LightBleeding", flags);
            _fractureType = ahcType.GetNestedType("Fracture", flags);
            _heavyBleedConcreteType = ahcType.GetNestedType("HeavyBleeding", flags);
            _lightBleedConcreteType = ahcType.GetNestedType("LightBleeding", flags);
            _fractureConcreteType = ahcType.GetNestedType("Fracture", flags);
            _typesCached = true;
        }

        private static IFikaNetworkManager _lastRegisteredNetworkManager = null;

        public static void EnsurePacketsRegistered()
        {
            if (!Singleton<IFikaNetworkManager>.Instantiated)
            {
                _lastRegisteredNetworkManager = null;
                return;
            }

            var currentManager = Singleton<IFikaNetworkManager>.Instance;
            if (currentManager == null) return;

            if (_lastRegisteredNetworkManager != currentManager)
            {
                try
                {
                    currentManager.RegisterPacket<BandAidHealPacketV2>(OnBandAidHealPacketReceived);
                    currentManager.RegisterPacket<BandAidShoulderTapPacketV2>(OnShoulderTapReceived);
                    currentManager.RegisterPacket<BandAidHealCheckPacketV2>(OnHealCheckReceived);
                    currentManager.RegisterPacket<BandAidHealCheckResponsePacketV2>(OnHealCheckResponseReceived);
                    currentManager.RegisterPacket<TraumaFaintPacketV2>(OnTraumaFaintReceived); // ref: CR-01-02
                    currentManager.RegisterPacket<BandAidTreatmentReportPacketV2>(OnTreatmentReportReceived); // feedback membro-alvo

                    // Stubs dos formatos ≤1.10.0: consomem o payload de um peer desatualizado em
                    // vez de deixá-lo sem handler — sem isso o ParseException derruba a fila de
                    // eventos do frame para TODOS os mods. Ver LegacyPacketCompat.
                    LegacyPacketCompat.Register(currentManager);

                    _lastRegisteredNetworkManager = currentManager;
                    Logger.LogInfo($"[BandAidNetworkHandler] Registered FIKA network packets on instance: {currentManager.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[BandAidNetworkHandler] Error registering FIKA packets: {ex}");
                }
            }
        }

        public static void CheckInit() => EnsurePacketsRegistered();

        /// <summary>
        /// Avisa que há peer com versão anterior na raid. Chamado uma única vez pelos stubs de
        /// compatibilidade — a interação daquele peer não vai funcionar, mas a rede segue sadia.
        /// </summary>
        internal static void LogVersionMismatch(string packetName)
        {
            Logger.LogWarning(
                $"[BandAidNetworkHandler] Recebido '{packetName}' no formato ≤1.10.0: há peer com " +
                "versão anterior do mod na raid. As interações médicas com esse jogador não vão " +
                "funcionar até que ele atualize (o formato de pacote mudou na 1.11.0).");
        }

        // Throttle de log: se a falha for sistemática (peer com layout divergente), logar por
        // pacote encheria o console do BepInEx de stack traces, causando hitching e escondendo o
        // erro real. Stack completo na primeira ocorrência de CADA TIPO de exceção — um booleano
        // global esconderia uma falha diferente que aparecesse raids depois.
        private const int ErrorLogIntervalMs = 5000;
        private static int _lastErrorLogTick;
        private static int _suppressedErrors;
        private static readonly System.Collections.Generic.HashSet<Type> _tracedExceptionTypes
            = new System.Collections.Generic.HashSet<Type>();

        internal static void LogErrorThrottled(string context, Exception ex)
        {
            var now = Environment.TickCount;

            // Esvazia o contador junto com o trace: senão as falhas acumuladas somem do log.
            var suppressed = _suppressedErrors;

            if (ex != null && _tracedExceptionTypes.Add(ex.GetType()))
            {
                _suppressedErrors = 0;
                _lastErrorLogTick = now;
                Logger.LogError($"[BandAidNetworkHandler] {context}: {ex}"
                    + (suppressed > 0 ? $" (+{suppressed} falhas de outros tipos suprimidas)" : string.Empty));
                return;
            }

            // Subtração de ints trata o wrap-around de TickCount corretamente.
            if (now - _lastErrorLogTick < ErrorLogIntervalMs)
            {
                _suppressedErrors++;
                return;
            }

            _lastErrorLogTick = now;
            _suppressedErrors = 0;
            Logger.LogError($"[BandAidNetworkHandler] {context}: {ex?.Message}"
                + (suppressed > 0 ? $" (+{suppressed} falhas suprimidas nos últimos {ErrorLogIntervalMs / 1000}s)" : string.Empty));
        }

        /// <summary>
        /// Ponto ÚNICO de transmissão do mod. Garante o registro dos pacotes antes de qualquer
        /// envio (a instância do IFikaNetworkManager é recriada a cada transição de sessão) e
        /// encapsula a assimetria host/client: no servidor todo envio é broadcast; no cliente vai
        /// para o host, que retransmite.
        ///
        /// Centralizar aqui é o que garante a regra de "Ensure antes de enviar" nos ~14 pontos de
        /// envio do mod, incluindo os relays feitos de dentro dos callbacks.
        /// </summary>
        /// <returns>
        /// O papel deste processo no envio (<c>"Host"</c>/<c>"Client"</c>), ou <c>null</c> se nada
        /// foi transmitido. Os chamadores logam a partir daí — logar incondicionalmente depois da
        /// chamada afirmaria um envio que pode ter falhado, justamente no cenário que se quer
        /// diagnosticar.
        /// </returns>
        public const uint TRLM_MAGIC_HEADER = 0x4D4C5254; // "TRLM" (0x54 0x52 0x4C 0x4D) - Assinatura do Canal 3 TRL Medicine

        private static string SendPacket<T>(ref T packet, string context) where T : INetSerializable
        {
            EnsurePacketsRegistered();

            try
            {
                // ref: Roadmap 1.1 — Canal 3 Isolado de Dados TRL (ReliableUnordered para não travar a fila do Channel 0 de inventário)
                if (Singleton<FikaServer>.Instantiated)
                {
                    Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableUnordered, true);
                    return "Host";
                }

                if (Singleton<FikaClient>.Instantiated)
                {
                    Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableUnordered);
                    return "Client";
                }

                return null;
            }
            catch (Exception ex)
            {
                LogErrorThrottled($"Falha ao enviar {typeof(T).Name} ({context})", ex);
                return null;
            }
        }

        // === Callback para quando médico recebe resposta do check ===
        public static event System.Action<BandAidHealCheckResponsePacketV2> OnHealCheckResponse;

        // ============================================================
        // ref: CR-01-02 — SYNC DE DESMAIO (migrado do TrueTrauma 3.11)
        // ============================================================

        /// <summary>Envia o estado de desmaio de um player DESTE processo para os peers.</summary>
        public static void SendTraumaFaintPacket(string profileId, bool isFainted, float durationSeconds, float graceSeconds)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return; // solo/sem Fika: estado local basta

            var packet = new TraumaFaintPacketV2
            {
                ProfileId = profileId,
                IsFainted = isFainted,
                DurationSeconds = durationSeconds,
                GraceSeconds = graceSeconds
            };

            var role = SendPacket(ref packet, "TraumaFaint");
            if (role != null)
                Logger.LogInfo($"[Faint] {role} enviou estado de desmaio: {profileId} = {isFainted} (dur={durationSeconds:F0}s)");
        }

        private static void OnTraumaFaintReceived(TraumaFaintPacketV2 packet)
        {
            try
            {
                // Corpo truncado: nao processar NEM retransmitir — o host relayaria lixo re-serializado.
                if (!packet.Valid) return;

                // Host/headless retransmite (mesmo padrão de relay dos pacotes de heal)
                if (Singleton<FikaServer>.Instantiated)
                {
                    var hostPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                    string myId = hostPlayer?.ProfileId ?? "";
                    if (packet.ProfileId != myId)
                    {
                        var relay = packet;
                        SendPacket(ref relay, "relay");
                    }
                }

                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null) return;

                var mainPlayer = gameWorld.MainPlayer;
                if (mainPlayer != null && packet.ProfileId == mainPlayer.ProfileId) return; // eco do próprio estado

                Logger.LogInfo($"[Faint] Recebido: {packet.ProfileId} = {packet.IsFainted} (dur={packet.DurationSeconds:F0}s)");
                TrueTrauma.FikaBridge.UpdateFaintedList(packet.ProfileId, packet.IsFainted);

                // ref: CR-02 — espelho serve a consultas por ContainsKey (BotPatches/
                // SilenceVoice); a REMOÇÃO é dirigida pelo pacote false do DONO (o
                // MainLoopPatch não roda para ObservedPlayer — CR-01-28). Duração vem
                // do PACOTE (config do dono), nunca da config local deste processo.
                if (packet.IsFainted)
                {
                    float duration = packet.DurationSeconds > 0f ? packet.DurationSeconds : 20f;
                    float grace = packet.GraceSeconds > 0f ? packet.GraceSeconds : duration + 5f;
                    float now = UnityEngine.Time.time;
                    TrueTrauma.TraumaState.BlackoutTimers[packet.ProfileId] = now + duration;
                    TrueTrauma.TraumaState.BlackoutStartTimes[packet.ProfileId] = now;
                    TrueTrauma.TraumaState.GraceTimers[packet.ProfileId] = now + grace;
                }
                else
                {
                    TrueTrauma.TraumaState.BlackoutTimers.Remove(packet.ProfileId);
                    TrueTrauma.TraumaState.BlackoutStartTimes.Remove(packet.ProfileId);
                    TrueTrauma.TraumaState.GraceTimers.Remove(packet.ProfileId);
                }

                // Aggro: bots são locais no host/headless — agir onde eles existem
                var victim = gameWorld.GetAlivePlayerByProfileID(packet.ProfileId);
                if (victim != null)
                {
                    if (packet.IsFainted) TrueTrauma.AggroHelper.NeutralizeAggro(victim);
                    else TrueTrauma.AggroHelper.RestoreAggro(victim);
                }
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error in OnTraumaFaintReceived", ex);
            }
        }

        public static void SendHealPacket(Player doctor, Player patient, string templateId, EBodyPart bodyPart,
            float healAmount, bool isSurgery, float surgeryPenalty = 0f,
            bool removedHeavyBleed = false, bool removedLightBleed = false, bool removedFracture = false,
            bool applyFullTreatment = false)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return;

            var packet = new BandAidHealPacketV2
            {
                DoctorProfileId = doctor.ProfileId,
                PatientProfileId = patient.ProfileId,
                ItemTemplateId = templateId,
                BodyPart = bodyPart,
                HealAmount = healAmount,
                IsSurgery = isSurgery,
                SurgeryPenalty = surgeryPenalty,
                RemovedHeavyBleed = removedHeavyBleed,
                RemovedLightBleed = removedLightBleed,
                RemovedFracture = removedFracture,
                ApplyFullTreatment = applyFullTreatment
            };

            // Host faz broadcast direto; client envia ao host, que retransmite ao paciente.
            var role = SendPacket(ref packet, "Heal");
            if (role != null)
                Logger.LogInfo($"{role} enviou pacote de cura para {patient.Profile.Nickname} [FullTreatment={applyFullTreatment}]");
        }

        private static void OnBandAidHealPacketReceived(BandAidHealPacketV2 packet)
        {
            try
            {
                // Corpo truncado: nao processar NEM retransmitir — o host relayaria lixo re-serializado.
                if (!packet.Valid) return;

                if (!Singleton<GameWorld>.Instantiated) return;

                CacheTypes();

                // === HEADLESS / HOST: Retransmitir para todos os clients ===
                // O host SEMPRE retransmite pacotes que não são dele (relay Client→Client)
                if (Singleton<FikaServer>.Instantiated)
                {
                    var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                    string myProfileId = mainPlayer?.ProfileId ?? "";

                    // Retransmitir se EU não sou o médico (veio de um client)
                    if (packet.DoctorProfileId != myProfileId)
                    {
                        var retransmit = packet;
                        SendPacket(ref retransmit, "relay");
                        Logger.LogInfo("Host retransmitiu pacote de cura para clients.");
                    }

                    // Se headless (sem MainPlayer): retransmitir e, se o paciente for um
                    // BOT local (ref: CR-01-01 — headless é o dono dos bots), aplicar nele.
                    if (mainPlayer == null)
                    {
                        if (packet.ApplyFullTreatment && TryApplyFullTreatmentOnLocalBot(packet)) return;
                        Logger.LogInfo("Headless: pacote retransmitido, sem ação local.");
                        return;
                    }
                }

                var localPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                if (localPlayer == null) return;

                // Ignorar se EU sou o médico (já apliquei localmente)
                if (packet.DoctorProfileId == localPlayer.ProfileId)
                {
                    Logger.LogInfo("Eu sou o médico, ignorando pacote (já apliquei localmente).");
                    return;
                }

                Logger.LogInfo($"Pacote recebido! Alvo: {packet.PatientProfileId}, FullTreatment: {packet.ApplyFullTreatment}");

                // === TRATAMENTO COMPLETO (paciente remoto → paciente aplica em si mesmo) ===
                if (packet.ApplyFullTreatment && packet.PatientProfileId == localPlayer.ProfileId)
                {
                    Logger.LogInfo("Eu sou o paciente. Aplicando tratamento completo em mim mesmo.");
                    ApplyFullTreatmentLocally(localPlayer, packet);
                    return;
                }

                // ref: CR-01-01 — paciente pode ser um BOT local deste processo (host-player
                // é o dono dos bots): aplicar em nome dele.
                if (packet.ApplyFullTreatment && TryApplyFullTreatmentOnLocalBot(packet)) return;

                // ref: G-1 (coop-heal-matrix) — pacote FullTreatment é EXCLUSIVO do paciente.
                // Receptor terceiro (host-player no C1→C2, ou 3º client no lobby) não pode
                // cair no branch de tratamento específico abaixo: com IsSurgery=true ele
                // tentaria cirurgia via reflection no boneco OBSERVADO do paciente.
                if (packet.ApplyFullTreatment)
                {
                    Logger.LogInfo("FullTreatment para outro paciente — nada a fazer localmente.");
                    return;
                }

                // === TRATAMENTO ESPECÍFICO (ações pontuais, com dados no pacote) ===
                Player patient = FindPatient(packet.PatientProfileId, localPlayer);
                if (patient == null) return;

                var hc = patient.HealthController;

                if (packet.IsSurgery)
                {
                    ApplySurgeryFromNetwork(hc, packet.BodyPart, packet.SurgeryPenalty);
                }
                else
                {
                    if (packet.RemovedHeavyBleed)
                        RemoveEffectNative(hc, packet.BodyPart, _heavyBleedConcreteType, "HeavyBleeding");
                    if (packet.RemovedLightBleed)
                        RemoveEffectNative(hc, packet.BodyPart, _lightBleedConcreteType, "LightBleeding");
                    if (packet.RemovedFracture)
                        RemoveEffectNative(hc, packet.BodyPart, _fractureConcreteType, "Fracture");

                    if (packet.HealAmount > 0)
                    {
                        if (hc is ActiveHealthController activeHc)
                        {
                            activeHc.ChangeHealth(packet.BodyPart, packet.HealAmount, default(DamageInfoStruct));
                            Logger.LogInfo($"HP +{packet.HealAmount} em {packet.BodyPart}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error in OnBandAidHealPacketReceived", ex);
            }
        }

        /// <summary>
        /// Aplica tratamento completo no jogador local usando ItemDatabase.
        /// Chamado quando o paciente recebe um pacote ApplyFullTreatment=true.
        /// O paciente tem ActiveHealthController (CoopClientHealthController), então tudo funciona.
        /// </summary>
        private static void ApplyFullTreatmentLocally(Player patient, BandAidHealPacketV2 packet)
        {
            CacheTypes();

            var stats = ItemDatabase.GetStats(packet.ItemTemplateId);
            if (stats == null)
            {
                Logger.LogWarning($"ItemDatabase não encontrou stats para {packet.ItemTemplateId}!");
                return;
            }

            var hc = patient.HealthController;
            if (!(hc is ActiveHealthController activeHc))
            {
                Logger.LogWarning("HealthController local não é ActiveHealthController — impossível aplicar tratamento.");
                return;
            }

            // Cirurgia
            if (stats.IsSurgery)
            {
                EBodyPart surgeryPart = GetBlackedPart(hc);
                float surgeryPenalty = UnityEngine.Random.Range(stats.SurgeryPenaltyMin, stats.SurgeryPenaltyMax);
                // 076: este path computa o penalty FRESCO (não vem do packet de cirurgia já ajustado) → ajusta pela
                // classe do OPERADOR (doctor do packet). Resolve o doctor por ProfileId (reusa FindPatient).
                var localPlayer076 = Singleton<GameWorld>.Instance?.MainPlayer;
                var doctor076 = localPlayer076 != null ? FindPatient(packet.DoctorProfileId, localPlayer076) : null;
                surgeryPenalty = CustomClassesBridge.AdjustSurgeryPenalty(doctor076, surgeryPenalty);
                ApplySurgeryFromNetwork(hc, surgeryPart, surgeryPenalty);
                Logger.LogInfo($"Cirurgia aplicada pelo paciente em {surgeryPart} (via rede).");
                SendTreatmentReport(packet, surgeryPart, 0f, 1f); // cirurgia consome 1 uso
                return;
            }

            // TORNIQUETE DESATIVADO — manter vanilla por enquanto
            // if (stats.IsTourniquet)
            // {
            //     foreach (EBodyPart p in System.Enum.GetValues(typeof(EBodyPart)))
            //     {
            //         if (p == EBodyPart.Common) continue;
            //         if (HasEffect(activeHc, p, _heavyBleedType))
            //         {
            //             RemoveEffectNative(hc, p, _heavyBleedType, "HeavyBleeding");
            //             if (TourniquetManager.Instance != null)
            //                 TourniquetManager.Instance.ApplyTourniquet(patient, p, null);
            //             Logger.LogInfo($"Torniquete aplicado em {p} pelo paciente (via rede).");
            //             break;
            //         }
            //     }
            //     return;
            // }

            // Efeitos (sangramento, fratura)
            EBodyPart target = FindSmartTarget(activeHc, stats);

            // ref: CR-05 — registrar o que foi DE FATO removido para cobrar o custo
            // real por efeito (tabela vanilla do ItemDatabase: Salewa HeavyBleed=175,
            // IFAK=210, Grizzly Fracture=50 etc.) no item do MÉDICO via report.
            bool hadHeavy = HasEffect(activeHc, target, _heavyBleedType);
            bool hadLight = HasEffect(activeHc, target, _lightBleedType);
            bool hadFracture = HasEffect(activeHc, target, _fractureType);
            float effectCost = 0f;

            // ref: CR-04-20 — custo só quando a remoção de fato executou
            if ((stats.StopsHeavyBleed || stats.StopsAllBleeds) && hadHeavy &&
                RemoveEffectNative(hc, target, _heavyBleedConcreteType, "HeavyBleeding"))
                effectCost += stats.HeavyBleedCost;
            if ((stats.StopsLightBleed || stats.StopsAllBleeds) && hadLight &&
                RemoveEffectNative(hc, target, _lightBleedConcreteType, "LightBleeding"))
                effectCost += stats.LightBleedCost;
            if (stats.FixesFracture && hadFracture &&
                RemoveEffectNative(hc, target, _fractureConcreteType, "Fracture"))
                effectCost += stats.FractureCost;

            // HP — proporção 1:1 rigorosa do EFT vanilla respeitando o saldo real do médico
            float healedTotal = 0f;
            if (stats.HealAmount > 0)
            {
                var bodyHp = hc.GetBodyPartHealth(target);
                float hpNeeded = bodyHp.Maximum - bodyHp.Current;
                float doctorAvailableResource = packet.HealAmount > 0f ? packet.HealAmount : stats.MaxHP;
                float availableForHp = UnityEngine.Mathf.Max(0f, doctorAvailableResource - effectCost);
                float heal = UnityEngine.Mathf.Min(availableForHp, UnityEngine.Mathf.Min(stats.HealAmount, hpNeeded));
                if (heal > 0)
                {
                    activeHc.ChangeHealth(target, heal, default(DamageInfoStruct));
                    healedTotal = heal;
                    Logger.LogInfo($"HP +{heal:F1} em {target} pelo paciente (via rede | Disp:{availableForHp:F1}).");
                }
            }

            // Custo real p/ o médico: kits (HealAmount>0) pagam HP curado + efeitos;
            // itens de uso (bandagem/tala/torniquete/CALOK) pagam 1 uso fixo.
            float totalCost = stats.HealAmount > 0
                ? UnityEngine.Mathf.Max(1f, healedTotal + effectCost)
                : 1f;
            Logger.LogInfo($"[CR-05] Custo real do tratamento: {totalCost:F1} (HP {healedTotal:F1} + efeitos {effectCost:F0}).");

            // Report ao médico: membro real + HP curado + custo autoritativo
            SendTreatmentReport(packet, target, healedTotal, totalCost);

            // ref: CR-02 — a notificação é do PACIENTE humano; quando o host aplica em
            // nome de um BOT (CR-01-01), o toast não pode aparecer para o host.
            if (patient == Singleton<GameWorld>.Instance?.MainPlayer)
            {
                NotificationManagerClass.DisplayMessageNotification(
                    MedicLocale.Get(MedicTextId.TreatedByAlly), ENotificationDurationType.Default, ENotificationIconType.Quest);
            }
            else
            {
                Logger.LogInfo($"FullTreatment aplicado no bot {patient?.Profile?.Nickname} (sem toast local).");
            }
        }

        private static EBodyPart FindSmartTarget(ActiveHealthController activeHc, ItemStats stats)
        {
            // Procurar membro com efeito que o item trata
            if (stats.StopsHeavyBleed || stats.StopsAllBleeds)
            {
                foreach (EBodyPart p in System.Enum.GetValues(typeof(EBodyPart)))
                {
                    if (p == EBodyPart.Common) continue;
                    if (HasEffect(activeHc, p, _heavyBleedType)) return p;
                }
            }
            if (stats.StopsLightBleed || stats.StopsAllBleeds)
            {
                foreach (EBodyPart p in System.Enum.GetValues(typeof(EBodyPart)))
                {
                    if (p == EBodyPart.Common) continue;
                    if (HasEffect(activeHc, p, _lightBleedType)) return p;
                }
            }
            if (stats.FixesFracture)
            {
                foreach (EBodyPart p in System.Enum.GetValues(typeof(EBodyPart)))
                {
                    if (p == EBodyPart.Common) continue;
                    if (HasEffect(activeHc, p, _fractureType)) return p;
                }
            }

            // MedKit: membro mais ferido
            if (stats.HealAmount > 0)
            {
                EBodyPart worst = EBodyPart.Common;
                float lowestRatio = 1f;
                foreach (EBodyPart p in System.Enum.GetValues(typeof(EBodyPart)))
                {
                    if (p == EBodyPart.Common) continue;
                    var h = activeHc.GetBodyPartHealth(p);
                    if (h.Current > 0 && h.Current / h.Maximum < lowestRatio)
                    { lowestRatio = h.Current / h.Maximum; worst = p; }
                }
                return worst == EBodyPart.Common ? EBodyPart.Chest : worst;
            }

            return EBodyPart.Chest;
        }

        private static bool HasEffect(ActiveHealthController activeHc, EBodyPart bodyPart, Type effectType)
        {
            if (effectType == null) return false;
            try
            {
                var findMethod = typeof(ActiveHealthController).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == "FindActiveEffect" && m.IsGenericMethod && m.GetParameters().Length == 1);
                if (findMethod != null)
                {
                    var genericFind = findMethod.MakeGenericMethod(effectType);
                    return genericFind.Invoke(activeHc, new object[] { bodyPart }) != null;
                }
            }
            catch { }
            return false;
        }

        private static EBodyPart GetBlackedPart(IHealthController hc)
        {
            foreach (EBodyPart p in System.Enum.GetValues(typeof(EBodyPart)))
            {
                if (p == EBodyPart.Common || p == EBodyPart.Head || p == EBodyPart.Chest) continue;
                if (hc.GetBodyPartHealth(p).Current <= 0) return p;
            }
            return EBodyPart.Common;
        }

        private static Player FindPatient(string patientProfileId, Player localPlayer)
        {
            if (patientProfileId == localPlayer.ProfileId)
                return localPlayer;

            var registeredPatient = Singleton<GameWorld>.Instance.RegisteredPlayers
                .FirstOrDefault(p => p.ProfileId == patientProfileId);
            var patient = registeredPatient as Player;

            if (patient != null)
                Logger.LogInfo($"Paciente encontrado: {patient.Profile.Nickname}");
            else
                Logger.LogWarning($"Paciente {patientProfileId} não encontrado!");

            return patient;
        }

        // ref: CR-04-20 — retorna se a remoção de fato executou (o custo por efeito
        // só é cobrado do médico quando true).
        private static bool RemoveEffectNative(IHealthController hc, EBodyPart bodyPart, Type effectType, string effectName)
        {
            if (effectType == null) return false;

            try
            {
                if (hc is ActiveHealthController activeHc)
                {
                    var method15 = typeof(ActiveHealthController).GetMethod("method_15",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method15 != null)
                    {
                        var genericMethod = method15.MakeGenericMethod(effectType);
                        var result = genericMethod.Invoke(activeHc, new object[] { bodyPart });
                        Logger.LogInfo($"[Rede] method_15<{effectName}>({bodyPart}) = {(result != null ? "OK" : "sem efeito")}");
                        return result != null;
                    }
                }

                // Fallback: FindActiveEffect + ForceResidue
                var findMethod = hc.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == "FindActiveEffect" && m.IsGenericMethod && m.GetParameters().Length == 1);
                if (findMethod != null)
                {
                    var genericFind = findMethod.MakeGenericMethod(effectType);
                    var effect = genericFind.Invoke(hc, new object[] { bodyPart });
                    if (effect != null)
                    {
                        var forceResidue = effect.GetType().GetMethod("ForceResidue",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        forceResidue?.Invoke(effect, null);
                        Logger.LogInfo($"[Rede] ForceResidue<{effectName}>({bodyPart}) OK (fallback).");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Rede] Erro ao remover {effectName}: {ex.Message}");
                return false;
            }
        }

        private static void ApplySurgeryFromNetwork(IHealthController hc, EBodyPart bodyPart, float penalty)
        {
            // 076: o penalty que chega aqui JÁ foi ajustado pela classe do OPERADOR (no #2 ApplySurgery p/ o packet,
            // ou no full-treatment). Marca p/ o patch nativo do CustomClasses PULAR — senão ele re-ajustaria pela
            // classe do PACIENTE (que roda no ActiveHealthController do paciente aqui). Pareado no finally.
            CustomClassesBridge.SetExternalHandling(true);
            try
            {
                if (hc is ActiveHealthController activeHc)
                {
                    bool result = activeHc.RestoreBodyPart(bodyPart, penalty);
                    Logger.LogInfo($"[Rede] Cirurgia em {bodyPart} = {(result ? "OK" : "falhou")}. Penalty: {penalty:P0}");
                }
                else
                {
                    var restore = hc.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(m => m.Name == "RestoreBodyPart" && m.GetParameters().Length == 2);
                    if (restore != null)
                    {
                        restore.Invoke(hc, new object[] { bodyPart, penalty });
                        Logger.LogInfo($"[Rede] Cirurgia em {bodyPart} via Reflection. Penalty: {penalty:P0}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Rede] Erro na cirurgia: {ex.Message}");
            }
            finally
            {
                CustomClassesBridge.SetExternalHandling(false);   // 076
            }
        }

        // === SHOULDER TAP ===
        public static void SendShoulderTapPacket(Player target)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return;
            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer == null) return;

            var packet = new BandAidShoulderTapPacketV2
            {
                SenderProfileId = mainPlayer.ProfileId,
                SenderNickname = mainPlayer.Profile.Nickname,
                TargetProfileId = target.ProfileId
            };

            SendPacket(ref packet, "ShoulderTap");
        }

        private static void OnShoulderTapReceived(BandAidShoulderTapPacketV2 packet)
        {
            try
            {
                // Corpo truncado: nao processar NEM retransmitir — o host relayaria lixo re-serializado.
                if (!packet.Valid) return;

                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;

                // Headless: apenas retransmitir
                if (Singleton<FikaServer>.Instantiated && (mainPlayer == null || packet.SenderProfileId != mainPlayer?.ProfileId))
                {
                    var retransmit = packet;
                    SendPacket(ref retransmit, "relay");
                }

                if (mainPlayer == null) return;

                // Só mostra se EU sou o alvo
                if (packet.TargetProfileId == mainPlayer.ProfileId)
                {
                    NotificationManagerClass.DisplayMessageNotification(
                        MedicLocale.Get(MedicTextId.ShoulderTapReceived, packet.SenderNickname),
                        ENotificationDurationType.Default, ENotificationIconType.Quest);
                    Logger.LogInfo($"Toque no ombro recebido de {packet.SenderNickname}");
                }
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error in OnShoulderTapReceived", ex);
            }
        }

        // === HEAL CHECK HANDSHAKE ===

        /// <summary>
        /// Step 1: Médico envia pedido de check ao paciente.
        /// </summary>
        public static void SendHealCheck(Player doctor, Player patient, string itemTemplateId)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return;

            var packet = new BandAidHealCheckPacketV2
            {
                DoctorProfileId = doctor.ProfileId,
                PatientProfileId = patient.ProfileId,
                ItemTemplateId = itemTemplateId
            };

            var role = SendPacket(ref packet, "HealCheck");
            if (role != null)
                Logger.LogInfo($"{role} enviou HealCheck para {patient.Profile.Nickname} | Item: {itemTemplateId}");
        }

        /// <summary>
        /// Step 2: Paciente recebe check, valida localmente, responde.
        /// </summary>
        private static void OnHealCheckReceived(BandAidHealCheckPacketV2 packet)
        {
            try
            {
                // Corpo truncado: nao processar NEM retransmitir — o host relayaria lixo re-serializado.
                if (!packet.Valid) return;

                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;

                // Host/Headless retransmite
                if (Singleton<FikaServer>.Instantiated)
                {
                    string myId = mainPlayer?.ProfileId ?? "";
                    if (packet.DoctorProfileId != myId)
                    {
                        var relay = packet;
                        SendPacket(ref relay, "relay");
                    }

                    // ref: CR-01-01 — BOTS nunca são MainPlayer de ninguém: o DONO deles
                    // (host/headless, onde têm ActiveHealthController) valida e responde
                    // o handshake em nome do bot — sem isso, client mirando bot = timeout.
                    if (mainPlayer == null || packet.PatientProfileId != mainPlayer.ProfileId)
                    {
                        if (TryAnswerForLocalBot(packet)) return;
                    }

                    if (mainPlayer == null) return;
                }

                // Só o paciente processa
                if (mainPlayer == null || packet.PatientProfileId != mainPlayer.ProfileId) return;

                CacheTypes();

                // Validar localmente com ActiveHealthController
                var stats = ItemDatabase.GetStats(packet.ItemTemplateId);
                bool approved = false;
                var denyReasonId = MedicDenyReasonId.UnknownItem;

                if (stats != null)
                {
                    approved = MedicalLogic.CanUseItem(mainPlayer, stats);
                    denyReasonId = approved ? MedicDenyReasonId.None : MedicDenyReasonId.NoCompatibleWound;
                }

                Logger.LogInfo($"HealCheck recebido | Item: {packet.ItemTemplateId} | Approved: {approved} | Reason: {denyReasonId}");

                // Membro-alvo esperado (mesma lógica da aplicação) — médico vê ANTES da animação
                EBodyPart expectedPart = EBodyPart.Common;
                if (approved && stats != null && mainPlayer.HealthController is ActiveHealthController patientAhc)
                {
                    try { expectedPart = stats.IsSurgery ? GetBlackedPart(patientAhc) : FindSmartTarget(patientAhc, stats); }
                    catch { }
                }

                // Enviar resposta
                var response = new BandAidHealCheckResponsePacketV2
                {
                    DoctorProfileId = packet.DoctorProfileId,
                    PatientProfileId = packet.PatientProfileId,
                    ItemTemplateId = packet.ItemTemplateId,
                    Approved = approved,
                    DenyReasonId = denyReasonId,
                    ExpectedBodyPart = (byte)expectedPart
                };

                SendPacket(ref response, "HealCheckResponse");
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error in OnHealCheckReceived", ex);
            }
        }

        // ============================================================
        // Feedback membro-alvo (report PACIENTE → MÉDICO)
        // ============================================================

        /// <summary>Paciente (ou dono do bot) reporta ao médico o membro tratado e o custo real.</summary>
        private static void SendTreatmentReport(BandAidHealPacketV2 source, EBodyPart part, float healed, float cost)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return;
            var report = new BandAidTreatmentReportPacketV2
            {
                DoctorProfileId = source.DoctorProfileId,
                PatientProfileId = source.PatientProfileId,
                ItemTemplateId = source.ItemTemplateId,
                BodyPart = (byte)part,
                HealedAmount = healed,
                CostAmount = cost
            };
            SendPacket(ref report, "TreatmentReport");
        }

        private static void OnTreatmentReportReceived(BandAidTreatmentReportPacketV2 packet)
        {
            try
            {
                // Corpo truncado: nao processar NEM retransmitir — o host relayaria lixo re-serializado.
                if (!packet.Valid) return;

                // Host relayeia reports que não são para ele (mesmo padrão dos demais)
                if (Singleton<FikaServer>.Instantiated)
                {
                    var hostPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                    string myId = hostPlayer?.ProfileId ?? "";
                    if (packet.DoctorProfileId != myId)
                    {
                        var relay = packet;
                        SendPacket(ref relay, "relay");
                    }
                }

                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer == null || packet.DoctorProfileId != mainPlayer.ProfileId) return;

                var part = (EBodyPart)packet.BodyPart;
                Logger.LogInfo($"[Report] Tratamento remoto aplicado em {part} (+{packet.HealedAmount:F0} HP, custo {packet.CostAmount:F1}).");

                // ref: CR-05 — CONSUMO AUTORITATIVO: debitar o item do médico com o custo
                // real reportado pelo paciente. Roda ANTES do gate de HUD (o consumo vale
                // mesmo com o examinador fechado).
                MedicalLogic.ResolvePendingConsumeFromReport(packet.PatientProfileId, packet.ItemTemplateId, packet.CostAmount);

                // ref: CR-03 — identidade do report (mesmo padrão do G-5): só pintar o HUD
                // se o report é do PACIENTE atualmente examinado; report atrasado de outro
                // paciente (ou pós-HideUI) não polui a UI da cura corrente.
                string activePatient = TRLImmersiveCombatMedicine.BandAidController.Instance?.ActivePatientProfileId;
                if (string.IsNullOrEmpty(activePatient) || activePatient != packet.PatientProfileId)
                {
                    Logger.LogInfo("[Report] descartado para a UI (paciente do report não é o examinado atual).");
                    return;
                }

                // itemName null → preserva o nome exibido desde o início da cura (CR-03)
                BandAidUI.Instance?.ShowTreatment(part, null);
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error in OnTreatmentReportReceived", ex);
            }
        }

        /// <summary>
        /// ref: CR-01-01 — no host/headless, aplica o FullTreatment em nome de um BOT
        /// local (paciente com ActiveHealthController que não é o MainPlayer).
        /// Retorna false se o paciente não é um bot local deste processo.
        /// </summary>
        private static bool TryApplyFullTreatmentOnLocalBot(BandAidHealPacketV2 packet)
        {
            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null) return false;

                var players = gameWorld.AllAlivePlayersList;
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    if (p == null || p.ProfileId != packet.PatientProfileId) continue;
                    if (!(p.HealthController is ActiveHealthController)) return false; // observado — não sou o dono
                    Logger.LogInfo($"Aplicando FullTreatment EM NOME do bot local {p.Profile?.Nickname}.");
                    ApplyFullTreatmentLocally(p, packet);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"TryApplyFullTreatmentOnLocalBot: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ref: CR-01-01 — no host/headless, valida e responde o HealCheck em nome de
        /// um BOT local (paciente com ActiveHealthController que não é MainPlayer).
        /// Retorna false se o paciente não é um bot local deste processo.
        /// </summary>
        private static bool TryAnswerForLocalBot(BandAidHealCheckPacketV2 packet)
        {
            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null || !Singleton<FikaServer>.Instantiated) return false;

                Player bot = null;
                var players = gameWorld.AllAlivePlayersList;
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    if (p != null && p.ProfileId == packet.PatientProfileId) { bot = p; break; }
                }
                if (bot == null || !(bot.HealthController is ActiveHealthController)) return false;

                CacheTypes();
                var stats = ItemDatabase.GetStats(packet.ItemTemplateId);
                bool approved = stats != null && MedicalLogic.CanUseItem(bot, stats);
                var denyReasonId = approved ? MedicDenyReasonId.None : (stats == null ? MedicDenyReasonId.UnknownItem : MedicDenyReasonId.NoCompatibleWound);

                // Membro-alvo esperado do bot (médico vê antes da animação)
                EBodyPart expectedPart = EBodyPart.Common;
                if (approved && bot.HealthController is ActiveHealthController botAhc)
                {
                    try { expectedPart = stats.IsSurgery ? GetBlackedPart(botAhc) : FindSmartTarget(botAhc, stats); }
                    catch { }
                }

                var response = new BandAidHealCheckResponsePacketV2
                {
                    DoctorProfileId = packet.DoctorProfileId,
                    PatientProfileId = packet.PatientProfileId,
                    ItemTemplateId = packet.ItemTemplateId,
                    Approved = approved,
                    DenyReasonId = denyReasonId,
                    ExpectedBodyPart = (byte)expectedPart
                };
                SendPacket(ref response, "HealCheckResponse/bot");
                Logger.LogInfo($"HealCheck respondido EM NOME do bot {bot.Profile?.Nickname} | Approved={approved}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"TryAnswerForLocalBot: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Step 3: Médico recebe resposta do paciente.
        /// </summary>
        private static void OnHealCheckResponseReceived(BandAidHealCheckResponsePacketV2 packet)
        {
            try
            {
                // Corpo truncado: nao processar NEM retransmitir — o host relayaria lixo re-serializado.
                if (!packet.Valid) return;

                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;

                // Host/Headless retransmite
                if (Singleton<FikaServer>.Instantiated)
                {
                    string myId = mainPlayer?.ProfileId ?? "";
                    if (packet.PatientProfileId != myId) // Veio do paciente, retransmitir
                    {
                        var relay = packet;
                        SendPacket(ref relay, "relay");
                    }
                    if (mainPlayer == null) return;
                }

                // Só o médico processa
                if (mainPlayer == null || packet.DoctorProfileId != mainPlayer.ProfileId) return;

                Logger.LogInfo($"HealCheck Response recebido | Approved: {packet.Approved} | Reason: {packet.DenyReasonId}");

                // Dispara evento para BandAidPlugin processar
                OnHealCheckResponse?.Invoke(packet);
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error in OnHealCheckResponseReceived", ex);
            }
        }
    }
}
