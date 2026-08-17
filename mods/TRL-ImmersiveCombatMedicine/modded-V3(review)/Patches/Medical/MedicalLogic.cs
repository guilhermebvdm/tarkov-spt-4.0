using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TRLImmersiveCombatMedicine;
using TRLImmersiveCombatMedicine.Helpers;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using BepInEx.Logging;

namespace TRLImmersiveCombatMedicine.Medical
{
    public static class MedicalLogic
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BandAid_Logic");

        public static readonly EBodyPart[] AllBodyParts = new[]
        {
            EBodyPart.Head,
            EBodyPart.Chest,
            EBodyPart.Stomach,
            EBodyPart.LeftArm,
            EBodyPart.RightArm,
            EBodyPart.LeftLeg,
            EBodyPart.RightLeg
        };

        private static Type _heavyBleedType;
        private static Type _lightBleedType;
        private static Type _fractureType;
        private static bool _typesCached = false;

        private static void CacheTypes()
        {
            if (_typesCached) return;

            var ahcType = typeof(ActiveHealthController);
            var flags = BindingFlags.NonPublic | BindingFlags.Public;
            _heavyBleedType = ahcType.GetNestedType("HeavyBleeding", flags);
            _lightBleedType = ahcType.GetNestedType("LightBleeding", flags);
            _fractureType = ahcType.GetNestedType("Fracture", flags);

            _typesCached = true;
        }

        public static void ApplyTreatment(Player doctor, Player patient, Item item, ItemStats stats)
        {
            CacheTypes();

            var hc = patient.HealthController;

            // Jogadores remotos usam ClientHealthController/ObservedHealthController
            if (doctor.ProfileId != patient.ProfileId && !(hc is ActiveHealthController))
            {
                Logger.LogInfo($"Paciente remoto detectado ({patient.Profile?.Nickname}). Enviando tratamento via rede.");

                float consumeCost;
                bool isUseItemRemote = (stats.HealAmount == 0);

                if (stats.IsSurgery || stats.IsTourniquet)
                {
                    consumeCost = 1.0f;
                }
                else if (!isUseItemRemote && stats.HealAmount > 0)
                {
                    // MedKit: estimar HP necessário pelo HP visível do paciente (sincronizado via Fika)
                    float worstRatio = 1f;
                    EBodyPart worstPart = EBodyPart.Chest;
                    foreach (EBodyPart p in AllBodyParts)
                    {
                        var h = hc.GetBodyPartHealth(p);
                        if (h.Current > 0 && h.Maximum > 0)
                        {
                            float ratio = h.Current / h.Maximum;
                            if (ratio < worstRatio) { worstRatio = ratio; worstPart = p; }
                        }
                    }
                    var bodyHp = hc.GetBodyPartHealth(worstPart);
                    float hpNeeded = bodyHp.Maximum - bodyHp.Current;
                    consumeCost = Mathf.Max(1f, Mathf.Min(stats.HealAmount, hpNeeded));
                }
                else
                {
                    consumeCost = 1.0f; // Use item (bandage, splint, etc.)
                }

                // ref: CR-05 (religado no CR-04-01 da rodada 04) — CONSUMO
                // AUTORITATIVO: NÃO debitar aqui. O paciente aplica o tratamento
                // real e reporta o custo exato (HP curado + custos por efeito) no
                // TreatmentReport; o débito acontece em ResolvePendingConsumeFromReport.
                // A estimativa acima vira só FALLBACK do timeout de 4s.
                RegisterPendingConsume(doctor, item, patient.ProfileId, consumeCost);

                // Enviar pacote para o paciente aplicar em si mesmo com o saldo real disponível do kit
                float availableResource = item.GetItemComponent<MedKitComponent>()?.HpResource ?? 1.0f;
                BandAidNetworkHandler.SendHealPacket(doctor, patient, item.TemplateId.ToString(),
                    EBodyPart.Common, availableResource, stats.IsSurgery, 0f, false, false, false, true);

                Logger.LogInfo($"Pacote de tratamento remoto enviado. Consumo aguarda report do paciente (fallback: {consumeCost:F1}).");
                return;
            }

            float hpToConsume = 0f;
            bool actionTaken = false;

            // Flags para o pacote de rede
            bool removedHeavyBleed = false;
            bool removedLightBleed = false;
            bool removedFracture = false;
            float healApplied = 0f;

            bool isUseItem = (stats.HealAmount == 0);

            if (stats.IsSurgery)
            {
                ApplySurgery(doctor, patient, item, hc, stats);
                return;
            }

            // === TORNIQUETE DESATIVADO — manter vanilla por enquanto ===
            // if (stats.IsTourniquet)
            // {
            //     ApplyTourniquet(doctor, patient, item, hc, stats);
            //     return;
            // }

            // Encontra o membro alvo inteligente
            EBodyPart target = GetSmartTarget(hc, stats);
            if (target == EBodyPart.Common) return;

            // === REMOÇÃO DE EFEITOS ESPECÍFICOS (Nativo do Jogo) ===

            // Heavy Bleed
            if (stats.StopsHeavyBleed || stats.StopsAllBleeds)
            {
                float cost = isUseItem ? 0f : stats.HeavyBleedCost;
                if (CanAfford(item, cost, hpToConsume))
                {
                    if (RemoveEffect(hc, target, _heavyBleedType, "HeavyBleeding"))
                    {
                        hpToConsume += cost;
                        actionTaken = true;
                        removedHeavyBleed = true;
                        Logger.LogInfo($"Heavy Bleed removido de {target} (nativo ForceResidue).");
                    }
                }
            }

            // Light Bleed
            if (stats.StopsLightBleed || stats.StopsAllBleeds)
            {
                if (!actionTaken || stats.StopsAllBleeds)
                {
                    float cost = isUseItem ? 0f : stats.LightBleedCost;
                    if (CanAfford(item, cost, hpToConsume))
                    {
                        if (RemoveEffect(hc, target, _lightBleedType, "LightBleeding"))
                        {
                            hpToConsume += cost;
                            actionTaken = true;
                            removedLightBleed = true;
                            Logger.LogInfo($"Light Bleed removido de {target} (nativo ForceResidue).");
                        }
                    }
                }
            }

            // Fratura
            if (stats.FixesFracture)
            {
                float cost = isUseItem ? 0f : stats.FractureCost;
                if (CanAfford(item, cost, hpToConsume))
                {
                    if (RemoveEffect(hc, target, _fractureType, "Fracture"))
                    {
                        hpToConsume += cost;
                        actionTaken = true;
                        removedFracture = true;
                        Logger.LogInfo($"Fratura removida de {target} (nativo ForceResidue).");
                    }
                }
            }

            // === CURAR HP (Apenas MedKits) ===
            if (!isUseItem && stats.HealAmount > 0)
            {
                float currentResource = item.GetItemComponent<MedKitComponent>()?.HpResource ?? 0;
                float remainingResource = currentResource - hpToConsume;

                // Calcula quanto HP o membro precisa (dano faltante)
                var bodyHealth = hc.GetBodyPartHealth(target);
                float hpNeeded = bodyHealth.Maximum - bodyHealth.Current;

                // Cura = mínimo entre (HealRate do item, HP faltante, recurso restante)
                float healToApply = Mathf.Min(stats.HealAmount, Mathf.Min(hpNeeded, remainingResource));

                if (healToApply > 0)
                {
                    if (hc is ActiveHealthController activeHc)
                    {
                        activeHc.ChangeHealth(target, healToApply, default(DamageInfoStruct));
                        hpToConsume += healToApply;
                        healApplied = healToApply;
                        actionTaken = true;
                        Logger.LogInfo($"Curado {healToApply}/{hpNeeded} HP em {target}. (HealRate:{stats.HealAmount}, Recurso:{remainingResource})");
                    }
                    else
                    {
                        var methods = hc.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var hpMethod = methods.FirstOrDefault(m => m.Name == "ChangeHealth" && m.GetParameters().Length == 3);
                        if (hpMethod != null)
                        {
                            hpMethod.Invoke(hc, new object[] { target, healToApply, default(DamageInfoStruct) });
                            hpToConsume += healToApply;
                            healApplied = healToApply;
                            actionTaken = true;
                        }
                    }
                }
            }

            // === CONSUMO FINAL ===
            if (actionTaken)
            {
                float finalCost = isUseItem ? 1.0f : hpToConsume;
                ConsumeSafe(doctor, item, finalCost);

                if (doctor.ProfileId != patient.ProfileId)
                {
                    BandAidNetworkHandler.SendHealPacket(doctor, patient, item.TemplateId.ToString(), target,
                        healApplied, false, 0f, removedHeavyBleed, removedLightBleed, removedFracture);
                }
            }
            else
            {
                Logger.LogWarning($"Nenhuma ação foi tomada em {target}. Verifique se o membro tem o efeito correspondente ao item.");
            }
        }

        // === REMOÇÃO DE EFEITO NATIVA (replica method_15<T> do ActiveHealthController) ===
        private static bool RemoveEffect(IHealthController hc, EBodyPart bodyPart, Type effectType, string effectName)
        {
            if (effectType == null)
            {
                Logger.LogWarning($"Tipo de efeito {effectName} não encontrado no cache.");
                return false;
            }

            try
            {
                // 1. Tenta via ActiveHealthController direto (method_15 = FindActiveEffect + ForceResidue)
                if (hc is ActiveHealthController activeHc)
                {
                    // Chama method_15<T>(bodyPart) via reflection genérica
                    var method15 = typeof(ActiveHealthController).GetMethod("method_15", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method15 != null)
                    {
                        var genericMethod = method15.MakeGenericMethod(effectType);
                        var result = genericMethod.Invoke(activeHc, new object[] { bodyPart });
                        if (result != null)
                        {
                            Logger.LogInfo($"method_15<{effectName}>({bodyPart}) executado com sucesso.");
                            return true;
                        }
                        else
                        {
                            Logger.LogInfo($"Nenhum efeito {effectName} ativo em {bodyPart}.");
                            return false;
                        }
                    }
                }

                // 2. Fallback: procura FindActiveEffect + ForceResidue manualmente
                var findMethod = hc.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == "FindActiveEffect" && m.IsGenericMethod && m.GetParameters().Length == 1);
                
                if (findMethod != null)
                {
                    var genericFind = findMethod.MakeGenericMethod(effectType);
                    var effect = genericFind.Invoke(hc, new object[] { bodyPart });
                    if (effect != null)
                    {
                        var forceResidue = effect.GetType().GetMethod("ForceResidue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (forceResidue != null)
                        {
                            forceResidue.Invoke(effect, null);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro ao remover efeito {effectName}: {ex.Message}");
                return false;
            }
        }

        // === VERIFICAR SE EFEITO EXISTE NUMA PARTE ===
        private static bool HasEffect(IHealthController hc, EBodyPart bodyPart, Type effectType)
        {
            if (effectType == null || hc == null) return false;

            try
            {
                // FindActiveEffect<T> está na interface IHealthController — funciona em qualquer HC
                var findMethod = typeof(IHealthController).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "FindActiveEffect" && m.IsGenericMethod && m.GetParameters().Length == 1);

                if (findMethod != null)
                {
                    var genericFind = findMethod.MakeGenericMethod(effectType);
                    return genericFind.Invoke(hc, new object[] { bodyPart }) != null;
                }
            }
            catch { }
            return false;
        }

        // === TORNIQUETE REALISTA ===
        private static void ApplyTourniquet(Player doctor, Player patient, Item item, IHealthController hc, ItemStats stats)
        {
            CacheTypes();

            // Encontra membro com sangramento pesado
            EBodyPart target = EBodyPart.Common;
            foreach (EBodyPart p in AllBodyParts)
            {
                if (HasEffect(hc, p, _heavyBleedType))
                {
                    target = p;
                    break;
                }
            }

            if (target == EBodyPart.Common)
            {
                Logger.LogWarning("Torniquete: nenhum sangramento pesado encontrado.");
                return;
            }

            // Remove sangramento pesado
            bool removed = RemoveEffect(hc, target, _heavyBleedType, "HeavyBleeding");
            if (!removed)
            {
                Logger.LogWarning($"Torniquete: falha ao remover sangramento em {target}.");
                return;
            }

            // Aplica torniquete no membro
            if (TourniquetManager.Instance != null)
                TourniquetManager.Instance.ApplyTourniquet(patient, target, item);

            // Consome o item
            ConsumeSafe(doctor, item, 1.0f);

            Logger.LogInfo($"Torniquete aplicado em {target}. Sangramento pesado removido.");

            // Sync multiplayer
            if (doctor.ProfileId != patient.ProfileId)
            {
                BandAidNetworkHandler.SendHealPacket(doctor, patient, item.TemplateId.ToString(), target,
                    0f, false, 0f, true, false, false);
            }
        }

        // === CIRURGIA ===
        private static void ApplySurgery(Player doctor, Player patient, Item item, IHealthController hc, ItemStats stats)
        {
            EBodyPart blacked = GetBlackedPart(hc);
            if (blacked == EBodyPart.Common) return;

            float penalty = UnityEngine.Random.Range(stats.SurgeryPenaltyMin, stats.SurgeryPenaltyMax);
            // 076 (CustomClasses): se o OPERADOR é Médico de Combate, o perk Restorative Surgery zera a penalidade
            // de HP máximo. Ajustado AQUI (doctor conhecido) → o valor flui p/ a aplicação local E p/ o SendHealPacket
            // (o cliente do paciente recebe o penalty já ajustado). No-op se o CustomClasses estiver ausente.
            penalty = CustomClassesBridge.AdjustSurgeryPenalty(doctor, penalty);

            // 076: o penalty já está ajustado pela classe do OPERADOR → o patch nativo do CustomClasses (no ActiveHC
            // do paciente) deve PULAR, senão re-ajustaria pela classe do PACIENTE. Pareado num try/finally.
            CustomClassesBridge.SetExternalHandling(true);
            try
            {
                if (hc is ActiveHealthController activeHc)
                {
                    bool restored = activeHc.RestoreBodyPart(blacked, penalty);
                    if (restored)
                    {
                        ConsumeSafe(doctor, item, 1.0f);
                        Logger.LogInfo($"Cirurgia em {blacked} (nativo RestoreBodyPart). Penalidade: {penalty:P0}");

                        if (doctor.ProfileId != patient.ProfileId)
                        {
                            BandAidNetworkHandler.SendHealPacket(doctor, patient, item.TemplateId.ToString(), blacked, 0f, true, penalty);
                        }
                    }
                }
                else
                {
                    var methods = hc.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var restore = methods.FirstOrDefault(m => m.Name == "RestoreBodyPart" && m.GetParameters().Length == 2);
                    if (restore != null)
                    {
                        var result = restore.Invoke(hc, new object[] { blacked, penalty });
                        if (result is bool ok && ok)
                        {
                            ConsumeSafe(doctor, item, 1.0f);
                            Logger.LogInfo($"Cirurgia em {blacked} (reflection). Penalidade: {penalty:P0}");
                            if (doctor.ProfileId != patient.ProfileId)
                                BandAidNetworkHandler.SendHealPacket(doctor, patient, item.TemplateId.ToString(), blacked, 0f, true, penalty);
                        }
                    }
                }
            }
            finally
            {
                CustomClassesBridge.SetExternalHandling(false);
            }
        }

        // === CONSUMO PENDENTE (CR-05: autoritativo pelo report do paciente) ===
        // O médico NÃO estima mais o custo pela saúde observada (defasada) — registra
        // um pendente e debita quando o TreatmentReport chega com o custo REAL (HP
        // curado + custos por efeito removido). Fallback: timeout → estimativa local.
        private class PendingConsume
        {
            public Player Doctor;
            public Item Item;
            public string PatientId;
            public string TemplateId;
            public float FallbackCost;
            public float Deadline;
        }

        private static readonly List<PendingConsume> _pendingConsumes = new List<PendingConsume>();
        private const float PENDING_CONSUME_TIMEOUT = 4f;

        private static void RegisterPendingConsume(Player doctor, Item item, string patientId, float fallbackCost)
        {
            string templateId = item.TemplateId.ToString();

            // ref: CR-04-17 — máximo 1 pendente por (paciente, template): se um report
            // se perdeu e a cura seguinte usa o mesmo item no mesmo paciente, o
            // pendente antigo é liquidado com o fallback AGORA (em vez de o report da
            // cura nova casar com o pendente velho e cruzar os custos).
            for (int i = 0; i < _pendingConsumes.Count; i++)
            {
                var old = _pendingConsumes[i];
                if (old.PatientId == patientId && old.TemplateId == templateId)
                {
                    _pendingConsumes.RemoveAt(i);
                    Logger.LogWarning($"[CR-05] Pendente antigo do mesmo item/paciente liquidado com fallback {old.FallbackCost:F1} antes do novo registro.");
                    ConsumeSafe(old.Doctor, old.Item, old.FallbackCost, isRemotePatient: true);
                    break;
                }
            }

            _pendingConsumes.Add(new PendingConsume
            {
                Doctor = doctor,
                Item = item,
                PatientId = patientId,
                TemplateId = templateId,
                FallbackCost = fallbackCost,
                Deadline = Time.time + PENDING_CONSUME_TIMEOUT
            });
        }

        /// <summary>Chamado pelo handler do TreatmentReport (custo autoritativo do paciente).</summary>
        public static void ResolvePendingConsumeFromReport(string patientId, string templateId, float cost)
        {
            for (int i = 0; i < _pendingConsumes.Count; i++)
            {
                var p = _pendingConsumes[i];
                if (p.PatientId == patientId && p.TemplateId == templateId)
                {
                    _pendingConsumes.RemoveAt(i);
                    Logger.LogInfo($"[CR-05] Consumo pelo report: custo real {cost:F1} (fallback seria {p.FallbackCost:F1}).");
                    ConsumeSafe(p.Doctor, p.Item, UnityEngine.Mathf.Max(0f, cost), isRemotePatient: true);
                    return;
                }
            }
            Logger.LogInfo("[CR-05] Report sem consumo pendente correspondente (já resolvido por timeout?).");
        }

        /// <summary>Tick do BandAidController: pendentes expirados consomem o fallback.</summary>
        public static void TickPendingConsumes()
        {
            for (int i = _pendingConsumes.Count - 1; i >= 0; i--)
            {
                var p = _pendingConsumes[i];
                if (Time.time >= p.Deadline)
                {
                    _pendingConsumes.RemoveAt(i);
                    Logger.LogWarning($"[CR-05] Report não chegou em {PENDING_CONSUME_TIMEOUT:F0}s — consumindo fallback {p.FallbackCost:F1}.");
                    ConsumeSafe(p.Doctor, p.Item, p.FallbackCost, isRemotePatient: true);
                }
            }
        }

        public static void ClearPendingConsumes() => _pendingConsumes.Clear();

        // === CONSUMO DO ITEM ===
        // ref: CR-04 (feedback 2-PCs) — o consumo PARCIAL local (mutar HpResource no
        // componente) é o MESMO que o MedEffect vanilla faz e é benigno para a
        // validação de layout do host (que valida endereços, não resource). O que
        // quebrava tudo era o DESCARTE: Discard(simulate:false) destacava o item na
        // hora, silenciosamente, e a RemoveOperation seguinte lançava em Item.Parent
        // → host com espelho fantasma, client com slot morto, mão travada. O descarte
        // correto (padrão vanilla SetupItem) é simulate:true + mutação DENTRO da
        // operação de rede — ver DiscardItemNetworked.
        public static void ConsumeSafe(Player doctor, Item item, float calculatedCost, bool isRemotePatient = false)
        {
            try
            {
                var medKit = item.GetItemComponent<MedKitComponent>();
                if (medKit != null)
                {
                    // ref: CR-05 — SEMPRE subtrair primeiro (no teste 2-PCs o descarte
                    // falhava com o item ainda nas mãos e o kit ficava com recurso
                    // INTACTO — AI-2/Salewa "eternos"). Descarte-em-zero é ADIADO até
                    // as mãos liberarem (DiscardItemNetworked deferred).
                    float charge = UnityEngine.Mathf.Min(calculatedCost, medKit.HpResource);
                    medKit.HpResource = UnityEngine.Mathf.Max(0f, medKit.HpResource - charge);
                    item.RaiseRefreshEvent();
                    Logger.LogInfo($"MedKit: -{charge:F1} HP. Restante: {medKit.HpResource:F1}");
                    if (medKit.HpResource <= 0.005f)
                        DiscardItemNetworked(doctor, item);
                    return;
                }

                var resource = item.GetItemComponent<ResourceComponent>();
                if (resource != null)
                {
                    resource.Value = UnityEngine.Mathf.Max(0f, resource.Value - 1.0f);
                    item.RaiseRefreshEvent();
                    Logger.LogInfo($"Recurso: -1 Uso. Restante: {resource.Value}");
                    if (resource.Value <= 0.005f)
                        DiscardItemNetworked(doctor, item);
                    return;
                }

                // Item simples (esmarch, bandagem, tala, injetor) — sempre descarta.
                // ref: CR-04-03 — o gate isRemotePatient deixava o caminho LOCAL
                // (host→bot, solo) com itens de 1 uso INFINITOS (sem MedKitComponent,
                // MaxHpResource=0 no template — nunca caíam nos branches acima).
                Logger.LogInfo("Item simples de 1 uso. Descartando (networked).");
                DiscardItemNetworked(doctor, item);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"ConsumeSafe erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove item via TryRunNetworkTransaction (Fika-aware).
        /// ref: CR-04 — Discard com simulate:TRUE: a mutação real roda DENTRO da
        /// RemoveOperationClass no pipeline de rede (host executa+propaga; client
        /// espera validação do host e executa no Started) — padrão vanilla
        /// (PlayerInventoryController.SetupItem).
        /// ref: CR-05 — o descarte é DIFERIDO: no fim forçado da animação o item
        /// ainda está NAS MÃOS e a operação falha em CanExecute ("Can't execute
        /// 'operationResult.Value.CanExecute()'", 100% dos casos no log do client).
        /// A coroutine do BandAidController espera as mãos liberarem e tenta com
        /// retry; sem controller disponível, tenta imediato (melhor esforço).
        /// </summary>
        public static void DiscardItemNetworked(Player doctor, Item item)
        {
            if (doctor == null || item == null || item.CurrentAddress == null) return;

            // Fast-path: se o médico NÃO está segurando este item ativamente e o inventário
            // não está bloqueado, dispara o descarte de imediato no mesmo frame sem coroutine.
            bool itemInHands = doctor.HandsController is Player.MedsController meds && meds.Item == item;
            var inventoryController = doctor.InventoryController;
            if (!itemInHands && inventoryController != null && !inventoryController.IsInventoryBlocked())
            {
                var watch = new DiscardWatch();
                int r = StartDiscardAttempt(doctor, item, watch);
                if (r == DISCARD_SENT || r == DISCARD_DONE)
                {
                    Logger.LogInfo($"[FastPath] Descarte imediato executado no mesmo frame: {item.ShortName.Localized()}");
                    return;
                }
            }

            // Fallback: item ainda em transição de mãos ou inventário momentaneamente bloqueado
            var controller = BandAidController.Instance;
            if (controller != null)
            {
                Logger.LogInfo($"Descarte agendado (aguarda liberação de mãos): {item.ShortName.Localized()}");
                controller.ScheduleNetworkedDiscard(doctor, item);
            }
            else
            {
                // Sem controller p/ coroutine: melhor esforço em tentativa única
                StartDiscardAttempt(doctor, item, new DiscardWatch());
            }
        }

        /// <summary>
        /// ref: CR-04-02 — resultado observável de uma tentativa de descarte. O
        /// callback do Fika NÃO é síncrono no caso geral (Client/HostInventoryController
        /// fazem await Task.Yield() antes de validar): a coroutine espera o callback
        /// disparar (ou timeout) antes de decidir sucesso/retry.
        /// </summary>
        public class DiscardWatch
        {
            public volatile bool CallbackFired;
            public volatile bool Failed;
        }

        public const int DISCARD_DONE = 2;    // item já sem endereço — nada a fazer
        public const int DISCARD_SENT = 1;    // operação enviada — aguardar o watch
        public const int DISCARD_RETRY = 0;   // simulação/exceção — vale retry

        /// <summary>Inicia UMA tentativa de descarte networked (decisão fica na coroutine).</summary>
        public static int StartDiscardAttempt(Player doctor, Item item, DiscardWatch watch)
        {
            try
            {
                // Guard: item já removido/sem endereço — nunca tocar em item.Parent
                // (o getter LANÇA); CurrentAddress é o accessor seguro. Também é o
                // que torna o retry pós-timeout seguro contra double-discard: se a
                // tentativa anterior executou tarde, o item já destacou e caímos aqui.
                if (item == null || item.CurrentAddress == null)
                {
                    Logger.LogInfo("Descarte: item sem endereço (já removido) — nada a fazer.");
                    return DISCARD_DONE;
                }

                var controller = doctor.InventoryController;
                var discardResult = InteractionsHandlerClass.Discard(item, controller, simulate: true);
                if (!discardResult.Succeeded)
                {
                    Logger.LogWarning($"Descarte: simulação falhou ({discardResult.Error}) — retry.");
                    return DISCARD_RETRY;
                }

                controller.TryRunNetworkTransaction(discardResult, result =>
                {
                    watch.Failed = result?.Failed == true;
                    watch.CallbackFired = true;
                    if (watch.Failed)
                        Logger.LogWarning($"Descarte: operação rejeitada ({result.Error}).");
                });
                return DISCARD_SENT;
            }
            catch (Exception ex)
            {
                Logger.LogError($"StartDiscardAttempt erro: {ex.Message}");
                return DISCARD_RETRY;
            }
        }

        private static bool CanAfford(Item item, float cost, float currentDebt)
        {
            if (cost == 0) return true;
            var medKit = item.GetItemComponent<MedKitComponent>();
            if (medKit == null) return true;
            return (medKit.HpResource - currentDebt) >= cost;
        }

        // === SMART TARGET: Procura a parte que realmente tem o efeito ===
        private static EBodyPart GetSmartTarget(IHealthController hc, ItemStats stats)
        {
            CacheTypes();

            // Se o item trata efeitos, procura o membro com esse efeito
            if (stats.StopsHeavyBleed || stats.StopsAllBleeds)
            {
                foreach (EBodyPart p in AllBodyParts)
                {
                    if (HasEffect(hc, p, _heavyBleedType)) return p;
                }
            }

            if (stats.StopsLightBleed || stats.StopsAllBleeds)
            {
                foreach (EBodyPart p in AllBodyParts)
                {
                    if (HasEffect(hc, p, _lightBleedType)) return p;
                }
            }

            if (stats.FixesFracture)
            {
                foreach (EBodyPart p in AllBodyParts)
                {
                    if (HasEffect(hc, p, _fractureType)) return p;
                }
            }

            // Se é MedKit com HealAmount, procura o membro mais ferido
            if (!stats.IsSurgery && stats.HealAmount > 0)
            {
                EBodyPart w = EBodyPart.Common; float l = 1f;
                foreach (EBodyPart p in AllBodyParts)
                {
                    var h = hc.GetBodyPartHealth(p);
                    if (h.Current > 0 && h.Current / h.Maximum < l) { l = h.Current / h.Maximum; w = p; }
                }
                return w == EBodyPart.Common ? EBodyPart.Chest : w;
            }

            // Fallback: qualquer parte com dano
            foreach (EBodyPart p in AllBodyParts)
            {
                var h = hc.GetBodyPartHealth(p);
                if (h.Current < h.Maximum && h.Current > 0) return p;
            }
            return EBodyPart.Chest;
        }

        private static EBodyPart GetBlackedPart(IHealthController hc)
        {
            foreach (EBodyPart p in AllBodyParts)
            {
                if (p == EBodyPart.Head || p == EBodyPart.Chest) continue;
                if (hc.GetBodyPartHealth(p).Current <= 0) return p;
            }
            return EBodyPart.Common;
        }

        /// <summary>
        /// Verifica se o item pode ser usado no paciente (se existe o ferimento correspondente).
        /// Para pacientes remotos (ObservedHealthController): HasEffect não funciona,
        /// então usamos bypass — o lado do paciente validará ao receber o pacote.
        /// </summary>
        public static bool CanUseItem(Player patient, ItemStats stats)
        {
            CacheTypes();
            var hc = patient.HealthController;
            bool isRemotePatient = !(hc is ActiveHealthController);

            // Cirurgia: precisa de membro destruído (GetBodyPartHealth funciona em ObservedHC)
            if (stats.IsSurgery)
            {
                return GetBlackedPart(hc) != EBodyPart.Common;
            }

            // === PACIENTE REMOTO: bypass para efeitos (HasEffect não funciona) ===
            // Para itens de efeito (bandage, splint, tourniquet), confiamos que o paciente
            // tem o ferimento — o lado do paciente validará via ApplyFullTreatmentLocally.
            if (isRemotePatient)
            {
                // Torniquete / bandage / splint: aceitar se paciente está vivo
                if (stats.StopsHeavyBleed || stats.StopsLightBleed || stats.StopsAllBleeds || 
                    stats.FixesFracture || stats.IsTourniquet)
                {
                    Logger.LogInfo("CanUseItem: paciente remoto — bypass de efeitos (validação no lado do paciente).");
                    return patient.HealthController.IsAlive;
                }

                // MedKit: verificar HP pelo ObservedHC (sincronizado via Fika)
                if (stats.HealAmount > 0)
                {
                    foreach (EBodyPart p in AllBodyParts)
                    {
                        var h = hc.GetBodyPartHealth(p);
                        if (h.Current > 0 && h.Current < h.Maximum) return true;
                    }
                }

                return false;
            }

            // === PACIENTE LOCAL: detecção normal de efeitos ===

            // Itens que param sangramento pesado
            if (stats.StopsHeavyBleed || stats.StopsAllBleeds)
            {
                foreach (EBodyPart p in AllBodyParts)
                {
                    if (HasEffect(hc, p, _heavyBleedType)) return true;
                }
            }

            // Itens que param sangramento leve
            if (stats.StopsLightBleed || stats.StopsAllBleeds)
            {
                foreach (EBodyPart p in AllBodyParts)
                {
                    if (HasEffect(hc, p, _lightBleedType)) return true;
                }
            }

            // Itens que consertam fratura
            if (stats.FixesFracture)
            {
                foreach (EBodyPart p in AllBodyParts)
                {
                    if (HasEffect(hc, p, _fractureType)) return true;
                }
            }

            // MedKits com HealAmount: precisa de membro com dano
            if (stats.HealAmount > 0)
            {
                foreach (EBodyPart p in AllBodyParts)
                {
                    var h = hc.GetBodyPartHealth(p);
                    if (h.Current > 0 && h.Current < h.Maximum) return true;
                }
            }

            return false;
        }
    }
}
