using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Communications;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using System;
using System.Linq;
using System.Reflection;

namespace Band_Aid
{
    public static class BandAidNetworkHandler
    {
        private static bool _initialized = false;
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

        public static void CheckInit()
        {
            // M1: Se o NetworkManager foi destruído (entre raids), resetar para re-registrar
            if (_initialized && !Singleton<IFikaNetworkManager>.Instantiated)
            {
                _initialized = false;
                Logger.LogInfo("NetworkManager destruído — reset de _initialized.");
            }

            if (_initialized) return;

            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                var netManager = Singleton<IFikaNetworkManager>.Instance;
                netManager.RegisterPacket<BandAidHealPacket>(OnBandAidHealPacketReceived);
                netManager.RegisterPacket<BandAidShoulderTapPacket>(OnShoulderTapReceived);
                netManager.RegisterPacket<BandAidHealCheckPacket>(OnHealCheckReceived);
                netManager.RegisterPacket<BandAidHealCheckResponsePacket>(OnHealCheckResponseReceived);
                _initialized = true;
                Logger.LogInfo("Fika Network Packets registrados (Heal + ShoulderTap + HealCheck)!");
            }
        }

        // === Callback para quando médico recebe resposta do check ===
        public static event System.Action<BandAidHealCheckResponsePacket> OnHealCheckResponse;

        public static void SendHealPacket(Player doctor, Player patient, string templateId, EBodyPart bodyPart,
            float healAmount, bool isSurgery, float surgeryPenalty = 0f,
            bool removedHeavyBleed = false, bool removedLightBleed = false, bool removedFracture = false,
            bool applyFullTreatment = false)
        {
            if (!_initialized) return;

            var packet = new BandAidHealPacket
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

            if (Singleton<FikaServer>.Instantiated)
            {
                // Host envia para todos os clients (broadcast)
                Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                Logger.LogInfo($"Host enviou pacote de cura para {patient.Profile.Nickname} [FullTreatment={applyFullTreatment}]");
            }
            else if (Singleton<FikaClient>.Instantiated)
            {
                // Client envia para o host (que vai retransmitir para o paciente)
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);
                Logger.LogInfo($"Client enviou pacote de cura para {patient.Profile.Nickname} [FullTreatment={applyFullTreatment}]");
            }
        }

        private static void OnBandAidHealPacketReceived(BandAidHealPacket packet)
        {
            if (Singleton<GameWorld>.Instance == null) return;

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
                    Singleton<FikaServer>.Instance.SendData(ref retransmit, DeliveryMethod.ReliableOrdered, true);
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

        /// <summary>
        /// Aplica tratamento completo no jogador local usando ItemDatabase.
        /// Chamado quando o paciente recebe um pacote ApplyFullTreatment=true.
        /// O paciente tem ActiveHealthController (CoopClientHealthController), então tudo funciona.
        /// </summary>
        private static void ApplyFullTreatmentLocally(Player patient, BandAidHealPacket packet)
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
                ApplySurgeryFromNetwork(hc, GetBlackedPart(hc), UnityEngine.Random.Range(stats.SurgeryPenaltyMin, stats.SurgeryPenaltyMax));
                Logger.LogInfo("Cirurgia aplicada pelo paciente (via rede).");
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

            if (stats.StopsHeavyBleed || stats.StopsAllBleeds)
                RemoveEffectNative(hc, target, _heavyBleedConcreteType, "HeavyBleeding");
            if (stats.StopsLightBleed || stats.StopsAllBleeds)
                RemoveEffectNative(hc, target, _lightBleedConcreteType, "LightBleeding");
            if (stats.FixesFracture)
                RemoveEffectNative(hc, target, _fractureConcreteType, "Fracture");

            // HP
            if (stats.HealAmount > 0)
            {
                var bodyHp = hc.GetBodyPartHealth(target);
                float hpNeeded = bodyHp.Maximum - bodyHp.Current;
                float heal = UnityEngine.Mathf.Min(stats.HealAmount, hpNeeded);
                if (heal > 0)
                {
                    activeHc.ChangeHealth(target, heal, default(DamageInfoStruct));
                    Logger.LogInfo($"HP +{heal:F1} em {target} pelo paciente (via rede).");
                }
            }

            NotificationManagerClass.DisplayMessageNotification(
                "Você foi tratado por um aliado.", ENotificationDurationType.Default, ENotificationIconType.Quest);
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

        private static void RemoveEffectNative(IHealthController hc, EBodyPart bodyPart, Type effectType, string effectName)
        {
            if (effectType == null) return;

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
                        return;
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
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Rede] Erro ao remover {effectName}: {ex.Message}");
            }
        }

        private static void ApplySurgeryFromNetwork(IHealthController hc, EBodyPart bodyPart, float penalty)
        {
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
        }

        // === SHOULDER TAP ===
        public static void SendShoulderTapPacket(Player target)
        {
            if (!_initialized) return;
            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer == null) return;

            var packet = new BandAidShoulderTapPacket
            {
                SenderProfileId = mainPlayer.ProfileId,
                SenderNickname = mainPlayer.Profile.Nickname,
                TargetProfileId = target.ProfileId
            };

            if (Singleton<FikaServer>.Instantiated)
                Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
            else if (Singleton<FikaClient>.Instantiated)
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }

        private static void OnShoulderTapReceived(BandAidShoulderTapPacket packet)
        {
            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;

            // Headless: apenas retransmitir
            if (Singleton<FikaServer>.Instantiated && (mainPlayer == null || packet.SenderProfileId != mainPlayer?.ProfileId))
            {
                var retransmit = packet;
                Singleton<FikaServer>.Instance.SendData(ref retransmit, DeliveryMethod.ReliableOrdered, true);
            }

            if (mainPlayer == null) return;

            // Só mostra se EU sou o alvo
            if (packet.TargetProfileId == mainPlayer.ProfileId)
            {
                NotificationManagerClass.DisplayMessageNotification(
                    $"\u2708 Você recebeu um toque no ombro de {packet.SenderNickname}",
                    ENotificationDurationType.Default, ENotificationIconType.Quest);
                Logger.LogInfo($"Toque no ombro recebido de {packet.SenderNickname}");
            }
        }

        // === HEAL CHECK HANDSHAKE ===

        /// <summary>
        /// Step 1: Médico envia pedido de check ao paciente.
        /// </summary>
        public static void SendHealCheck(Player doctor, Player patient, string itemTemplateId)
        {
            if (!_initialized) return;

            var packet = new BandAidHealCheckPacket
            {
                DoctorProfileId = doctor.ProfileId,
                PatientProfileId = patient.ProfileId,
                ItemTemplateId = itemTemplateId
            };

            if (Singleton<FikaServer>.Instantiated)
                Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
            else if (Singleton<FikaClient>.Instantiated)
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);

            Logger.LogInfo($"HealCheck enviado para {patient.Profile.Nickname} | Item: {itemTemplateId}");
        }

        /// <summary>
        /// Step 2: Paciente recebe check, valida localmente, responde.
        /// </summary>
        private static void OnHealCheckReceived(BandAidHealCheckPacket packet)
        {
            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;

            // Host/Headless retransmite
            if (Singleton<FikaServer>.Instantiated)
            {
                string myId = mainPlayer?.ProfileId ?? "";
                if (packet.DoctorProfileId != myId)
                {
                    var relay = packet;
                    Singleton<FikaServer>.Instance.SendData(ref relay, DeliveryMethod.ReliableOrdered, true);
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
            string denyReason = "Item desconhecido.";

            if (stats != null)
            {
                approved = MedicalLogic.CanUseItem(mainPlayer, stats);
                denyReason = approved ? "" : $"{stats.Name}: Sem ferimento compatível.";
            }

            Logger.LogInfo($"HealCheck recebido | Item: {packet.ItemTemplateId} | Approved: {approved} | Reason: {denyReason}");

            // Enviar resposta
            var response = new BandAidHealCheckResponsePacket
            {
                DoctorProfileId = packet.DoctorProfileId,
                PatientProfileId = packet.PatientProfileId,
                ItemTemplateId = packet.ItemTemplateId,
                Approved = approved,
                DenyReason = denyReason
            };

            if (Singleton<FikaServer>.Instantiated)
                Singleton<FikaServer>.Instance.SendData(ref response, DeliveryMethod.ReliableOrdered, true);
            else if (Singleton<FikaClient>.Instantiated)
                Singleton<FikaClient>.Instance.SendData(ref response, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// ref: CR-01-01 — no host/headless, aplica o FullTreatment em nome de um BOT
        /// local (paciente com ActiveHealthController que não é o MainPlayer).
        /// Retorna false se o paciente não é um bot local deste processo.
        /// </summary>
        private static bool TryApplyFullTreatmentOnLocalBot(BandAidHealPacket packet)
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
        private static bool TryAnswerForLocalBot(BandAidHealCheckPacket packet)
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
                string denyReason = approved ? "" : (stats == null ? "Item desconhecido." : $"{stats.Name}: Sem ferimento compatível.");

                var response = new BandAidHealCheckResponsePacket
                {
                    DoctorProfileId = packet.DoctorProfileId,
                    PatientProfileId = packet.PatientProfileId,
                    ItemTemplateId = packet.ItemTemplateId,
                    Approved = approved,
                    DenyReason = denyReason
                };
                Singleton<FikaServer>.Instance.SendData(ref response, DeliveryMethod.ReliableOrdered, true);
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
        private static void OnHealCheckResponseReceived(BandAidHealCheckResponsePacket packet)
        {
            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;

            // Host/Headless retransmite
            if (Singleton<FikaServer>.Instantiated)
            {
                string myId = mainPlayer?.ProfileId ?? "";
                if (packet.PatientProfileId != myId) // Veio do paciente, retransmitir
                {
                    var relay = packet;
                    Singleton<FikaServer>.Instance.SendData(ref relay, DeliveryMethod.ReliableOrdered, true);
                }
                if (mainPlayer == null) return;
            }

            // Só o médico processa
            if (mainPlayer == null || packet.DoctorProfileId != mainPlayer.ProfileId) return;

            Logger.LogInfo($"HealCheck Response recebido | Approved: {packet.Approved} | Reason: {packet.DenyReason}");

            // Dispara evento para BandAidPlugin processar
            OnHealCheckResponse?.Invoke(packet);
        }
    }
}
