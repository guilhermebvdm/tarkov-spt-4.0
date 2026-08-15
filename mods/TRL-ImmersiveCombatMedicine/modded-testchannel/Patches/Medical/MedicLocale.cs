using System.Collections.Generic;
using Band_Aid; // PA-01-01 (review técnica 01): ItemDatabase vive em Band_Aid — sem este using,
                 // GetDenyReasonText não compila (CS0103).
using EFT;

namespace TRLImmersiveCombatMedicine
{
    /// <summary>Motivo de recusa do handshake de cura (Band-Aid) — trafega como ID pela rede,
    /// NUNCA como texto (a tradução acontece no médico, que exibe — ver MedicLocale.GetDenyReasonText).</summary>
    internal enum MedicDenyReasonId : byte
    {
        None = 0,
        UnknownItem = 1,        // ItemDatabase não tinha stats para o TemplateId
        NoCompatibleWound = 2,  // MedicalLogic.CanUseItem() reprovou (usa ItemTemplateId do próprio pacote p/ nome do item)
    }

    /// <summary>Chaves de texto dos sistemas legados (Band-Aid/torniquete/ActionPanel/HUD médico) migrados
    /// no item 010 (decisão 22). Textos com placeholder usam string.Format sobre o template já traduzido.</summary>
    internal enum MedicTextId
    {
        Aborted = 0,
        NoPatientResponseTimeout = 1,
        CheckingItem = 2,               // {0} = nome do item
        NoCompatibleWoundLocal = 3,     // {0} = nome do item (paciente local: bot/self)
        ShoulderTapSent = 4,            // {0} = nickname do alvo
        ItemDropped = 5,
        ApplyingItem = 6,               // {0} = nome do item
        TreatmentCompleteWithPart = 7,  // {0} = rótulo curto do membro
        TreatmentComplete = 8,
        ItemLostDuringTreatment = 9,
        TreatmentCancelled = 10,
        MedicExamining = 11,            // {0} = nickname do paciente
        TreatedByAlly = 12,
        ShoulderTapReceived = 13,       // {0} = nickname do remetente
        ActionExamine = 14,
        ActionShoulderTap = 15,
        HudTitle = 16,
        HudFooterDynamic = 17,          // {0} = verbo do modo (Press/Hold/DoubleTap), {1} = tecla
        HudUnavailable = 18,
        TourniquetAlreadyApplied = 19,  // {0} = rótulo longo do membro
        TourniquetApplied = 20,         // {0} = rótulo longo do membro
        TourniquetNotFound = 21,        // {0} = rótulo longo do membro
        TourniquetRemoved = 22,         // {0} = rótulo longo do membro, {1} = duração (s)
        TourniquetNecrosisWarning = 23, // {0} = rótulo longo do membro
        TourniquetDestroyed = 24,       // {0} = rótulo longo do membro
        DenyUnknownItem = 25,
        DenyNoCompatibleWound = 26,     // {0} = nome do item
        TreatingLabel = 27,             // {0} = rótulo curto do membro — CR-01-01 (code-review 010 r1)
        TreatingLabelWithItem = 28,     // {0} = nome do item (maiúsculo), {1} = rótulo curto do membro
        TreatmentCancelledWithItemLoss = 29, // {0} = nome do item (cancelamento punido)
    }

    internal static class MedicLocale
    {
        // Indexados por MedicTextId. EN é o default/fallback; PT vazio → EN (mesmo contrato do TraumaLocale).
        private static readonly string[] EnTexts =
        {
            /* Aborted                  */ "Aborted!",
            /* NoPatientResponseTimeout */ "No response from patient (timeout).",
            /* CheckingItem             */ "Checking {0}...",
            /* NoCompatibleWoundLocal   */ "{0}: no compatible wound.",
            /* ShoulderTapSent          */ "Shoulder tap → {0}",
            /* ItemDropped              */ "Item dropped!",
            /* ApplyingItem             */ "Applying {0}...",
            /* TreatmentCompleteWithPart*/ "Treatment complete ({0}).",
            /* TreatmentComplete        */ "Treatment complete.",
            /* ItemLostDuringTreatment  */ "Item lost during treatment.",
            /* TreatmentCancelled       */ "Treatment cancelled.",
            /* MedicExamining           */ "MEDIC: {0}",
            /* TreatedByAlly            */ "You were treated by an ally.",
            /* ShoulderTapReceived      */ "✈ You received a shoulder tap from {0}", // PA-02-01: ícone ✈ preservado
            /* ActionExamine            */ "Examine (Medic)",
            /* ActionShoulderTap        */ "Shoulder tap",
            /* HudTitle                 */ "OPERATOR STATUS",
            /* HudFooterDynamic         */ "Use your hotkeys to heal\n[{0} {1}] Close Examiner",
            /* HudUnavailable           */ "UNAVAILABLE",
            /* TourniquetAlreadyApplied */ "Tourniquet already applied: {0}",
            /* TourniquetApplied        */ "Tourniquet applied: {0}. Remove after bleeding stops!",
            /* TourniquetNotFound       */ "No tourniquet on: {0}",
            /* TourniquetRemoved        */ "Tourniquet removed: {0} ({1}s). Item returned.",
            /* TourniquetNecrosisWarning*/ "⚠ Tourniquet on {0}: necrosis risk! Remove now!", // PA-01-06: ícone ⚠ preservado
            /* TourniquetDestroyed      */ "☠ {0} destroyed by tourniquet necrosis!",          // PA-01-06: ícone ☠ preservado
            /* DenyUnknownItem          */ "Unknown item.",
            /* DenyNoCompatibleWound    */ "{0}: no compatible wound.",
            /* TreatingLabel            */ "► TREATING: {0}",
            /* TreatingLabelWithItem    */ "► {0} → {1}",
            /* TreatmentCancelledWithItemLoss */ "Treatment cancelled: {0} consumed/unsterilized.",
        };

        private static readonly string[] PtTexts =
        {
            /* Aborted                  */ "Abortado!",
            /* NoPatientResponseTimeout */ "Sem resposta do paciente (timeout).",
            /* CheckingItem             */ "Verificando {0}...",
            /* NoCompatibleWoundLocal   */ "{0}: Sem ferimento compatível.",
            /* ShoulderTapSent          */ "Toque no ombro → {0}",
            /* ItemDropped              */ "Item dropado!",
            /* ApplyingItem             */ "Aplicando {0}...",
            /* TreatmentCompleteWithPart*/ "Tratamento Completo ({0}).",
            /* TreatmentComplete        */ "Tratamento Completo.",
            /* ItemLostDuringTreatment  */ "Item perdido durante tratamento.",
            /* TreatmentCancelled       */ "Tratamento cancelado.",
            /* MedicExamining           */ "MÉDICO: {0}",
            /* TreatedByAlly            */ "Você foi tratado por um aliado.",
            /* ShoulderTapReceived      */ "✈ Você recebeu um toque no ombro de {0}", // PA-02-01
            /* ActionExamine            */ "Examinar (Médico)",
            /* ActionShoulderTap        */ "Tocar no ombro",
            /* HudTitle                 */ "SITUAÇÃO DO OPERADOR",
            /* HudFooterDynamic         */ "Utilize as suas teclas de atalhos para curar\n[{0} {1}] Fechar Examinador",
            /* HudUnavailable           */ "INDISPONÍVEL",
            /* TourniquetAlreadyApplied */ "Torniquete já aplicado: {0}",
            /* TourniquetApplied        */ "Torniquete aplicado: {0}. Remova após parar o sangramento!",
            /* TourniquetNotFound       */ "Nenhum torniquete em: {0}",
            /* TourniquetRemoved        */ "Torniquete removido: {0} ({1}s). Item devolvido.",
            /* TourniquetNecrosisWarning*/ "⚠ Torniquete em {0}: risco de necrose! Remova agora!", // PA-01-06
            /* TourniquetDestroyed      */ "☠ {0} destruído por necrose do torniquete!",           // PA-01-06
            /* DenyUnknownItem          */ "Item desconhecido.",
            /* DenyNoCompatibleWound    */ "{0}: Sem ferimento compatível.",
            /* TreatingLabel            */ "► TRATANDO: {0}",
            /* TreatingLabelWithItem    */ "► {0} → {1}",
            /* TreatmentCancelledWithItemLoss */ "Tratamento cancelado: {0} consumido/desesterilizado.",
        };

        // === Rótulos de membro — DUAS granularidades já existiam no código pré-migração:
        // BandAidUI usava rótulos CURTOS ("CABEÇA"), TourniquetManager usava rótulos LONGOS
        // ("Cabeça"). Preservados como dois resolvers para não alterar a UX existente.
        private static readonly Dictionary<EBodyPart, string> ShortEn = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "HEAD" }, { EBodyPart.Chest, "CHEST" }, { EBodyPart.Stomach, "STOMACH" },
            { EBodyPart.LeftArm, "L. ARM" }, { EBodyPart.RightArm, "R. ARM" },
            { EBodyPart.LeftLeg, "L. LEG" }, { EBodyPart.RightLeg, "R. LEG" },
        };
        private static readonly Dictionary<EBodyPart, string> ShortPt = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "CABEÇA" }, { EBodyPart.Chest, "TÓRAX" }, { EBodyPart.Stomach, "ESTÔMAGO" },
            { EBodyPart.LeftArm, "BRAÇO ESQ." }, { EBodyPart.RightArm, "BRAÇO DIR." },
            { EBodyPart.LeftLeg, "PERNA ESQ." }, { EBodyPart.RightLeg, "PERNA DIR." },
        };
        // ref: Assembly-CSharp/EBodyPart.cs:1-11 — 8 valores (Head..Common). PA-02-05 (review
        // técnica 02, corrige comentário anterior factualmente errado): Common CHEGA via
        // BandAidUI.ShowTreatment (membro-alvo ainda não resolvido, ex.: _expectedTreatmentPart
        // default em BandAidController.cs:375, ou catch silencioso em MedicHealPatch.cs) —
        // resolvido pelo fallback "..." abaixo, preservando o comportamento atual de PartLabel.
        // TourniquetManager nunca passa Common (só opera sobre membros com torniquete ativo).
        private static readonly Dictionary<EBodyPart, string> LongEn = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "Head" }, { EBodyPart.Chest, "Chest" }, { EBodyPart.Stomach, "Stomach" },
            { EBodyPart.LeftArm, "Left Arm" }, { EBodyPart.RightArm, "Right Arm" },
            { EBodyPart.LeftLeg, "Left Leg" }, { EBodyPart.RightLeg, "Right Leg" },
        };
        private static readonly Dictionary<EBodyPart, string> LongPt = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "Cabeça" }, { EBodyPart.Chest, "Tórax" }, { EBodyPart.Stomach, "Estômago" },
            { EBodyPart.LeftArm, "Braço Esquerdo" }, { EBodyPart.RightArm, "Braço Direito" },
            { EBodyPart.LeftLeg, "Perna Esquerda" }, { EBodyPart.RightLeg, "Perna Direita" },
        };

        private static readonly string[] PressVerbEn = { "Press", "Hold", "Double-tap" };
        private static readonly string[] PressVerbPt = { "Pressione", "Segure", "Duplo" };

        /// <summary>Reusa TraumaLocale.IsGamePortuguese() (internal, mesma assembly) — SEM duplicar a
        /// leitura de LocaleManagerClass (regra explícita do item 010).</summary>
        private static bool IsPt() => TRLImmersiveCombatMedicine.Trauma.TraumaLocale.IsGamePortuguese();

        internal static string Get(MedicTextId id, params object[] args)
        {
            int i = (int)id;
            if (i < 0 || i >= EnTexts.Length) return string.Empty;
            string template = EnTexts[i];
            if (IsPt())
            {
                string pt = i < PtTexts.Length ? PtTexts[i] : null;
                if (!string.IsNullOrEmpty(pt)) template = pt;
            }
            return (args == null || args.Length == 0) ? template : string.Format(template, args);
        }

        internal static string BodyPartShort(EBodyPart part)
        {
            var dict = IsPt() ? ShortPt : ShortEn;
            return dict.TryGetValue(part, out var l) ? l : "...";
        }

        internal static string BodyPartLong(EBodyPart part)
        {
            var dict = IsPt() ? LongPt : LongEn;
            return dict.TryGetValue(part, out var l) ? l : part.ToString();
        }

        internal static string PressModeVerb(EBandAidPressMode mode)
        {
            int idx = mode == EBandAidPressMode.Hold ? 1 : (mode == EBandAidPressMode.DoubleTap ? 2 : 0);
            return IsPt() ? PressVerbPt[idx] : PressVerbEn[idx];
        }

        /// <summary>Resolve o texto de recusa do handshake NO PONTO DE EXIBIÇÃO (médico) — o pacote
        /// carrega só o ID + o ItemTemplateId (já existia no pacote, reusado para o nome do item).</summary>
        internal static string GetDenyReasonText(MedicDenyReasonId reasonId, string itemTemplateId)
        {
            switch (reasonId)
            {
                case MedicDenyReasonId.UnknownItem:
                    return Get(MedicTextId.DenyUnknownItem);
                case MedicDenyReasonId.NoCompatibleWound:
                    var stats = ItemDatabase.GetStats(itemTemplateId);
                    return Get(MedicTextId.DenyNoCompatibleWound, stats?.Name ?? "?");
                default:
                    return string.Empty;
            }
        }
    }
}
