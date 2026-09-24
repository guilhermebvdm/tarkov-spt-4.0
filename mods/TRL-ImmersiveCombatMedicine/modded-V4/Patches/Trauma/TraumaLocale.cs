using System;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Chaves de texto do motor. No 002 entram as linhas de estado (toast de 1ª ocorrência) — textos calibráveis nos consumidores.</summary>
    internal enum TraumaTextId { LegsLimpN1 = 0, LegsLimpN2 = 1, LegsCrouch = 2, LegsFall = 3, ArmsTremor = 4, ArmsAdsCancel = 5, StomachZeroed = 6 }

    /// <summary>i18n EN default + PT (decisão 22). Tabela PRÓPRIA do plugin — NUNCA injeção de chave no locale do servidor
    /// ("key".Localized() devolve chave crua no headless — P8 Recomendação (4)).</summary>
    internal static class TraumaLocale
    {
        // Indexados por TraumaTextId. EN é o default/fallback; PT vazio → EN.
        private static readonly string[] EnTexts =
        {
            "Your legs are hurt — you limp slightly.",
            "Your legs are badly hurt — you limp heavily.",
            "Your legs give in — you can barely stand.",
            "Your legs collapse under you.",
            "Your arms tremble.",
            "Your arms are too damaged to hold aim.",
            "A gut wound knocks the air out of you."
        };

        private static readonly string[] PtTexts =
        {
            "Suas pernas estão feridas — você manca de leve.",
            "Suas pernas estão muito feridas — você manca forte.",
            "Suas pernas cedem — você mal consegue ficar de pé.",
            "Suas pernas não aguentam — você desaba.",
            "Seus braços tremem.",
            "Seus braços estão feridos demais para sustentar a mira.",
            "Um ferimento no estômago tira seu ar."
        };

        /// <summary>Idioma do jogo é PT? Ler no MOMENTO DE EXIBIR (leitura viva; cobre troca mid-session). NUNCA cachear no Awake.</summary>
        internal static bool IsGamePortuguese()
        {
            // ref: trauma-primitives.md P8 Recomendação (1) — usar o CAMPO estático (null-check sem efeito colateral;
            // o GETTER estático constrói o singleton com "en" default + Resources.LoadAll se chamado cedo).
            // ref: LocaleManagerClass_1 e String_0 confirmados por ilspycmd no assembly real; id "po" — SPT database languages.json (P8 evid.)
            LocaleManagerClass lm = LocaleManagerClass.LocaleManagerClass_1;
            return lm != null && string.Equals(lm.String_0, "po", StringComparison.OrdinalIgnoreCase);
            // Race de boot: lm == null → false → fallback EN (corner da funcional). Headless: Fika força "en" → false (comportamento desejado — P8).
        }

        internal static string Get(TraumaTextId id)
        {
            int i = (int)id;
            if (i < 0 || i >= EnTexts.Length) return string.Empty;
            if (IsGamePortuguese())
            {
                string pt = i < PtTexts.Length ? PtTexts[i] : null;
                if (!string.IsNullOrEmpty(pt)) return pt; // chave sem texto PT → EN
            }
            return EnTexts[i];
        }

        /// <summary>Mapeia linha da matriz → chave de texto (toast). Linhas AdsCancel compartilham o texto (decisão 3 — toda linha inclui tremor).</summary>
        internal static TraumaTextId MapLine(TraumaLine line)
        {
            switch (line)
            {
                case TraumaLine.LegsLimpN1: return TraumaTextId.LegsLimpN1;
                case TraumaLine.LegsLimpN2: return TraumaTextId.LegsLimpN2;
                case TraumaLine.LegsCrouchPlusLimpN2: return TraumaTextId.LegsCrouch;
                case TraumaLine.LegsFallCycle: return TraumaTextId.LegsFall;
                case TraumaLine.ArmsTremor: return TraumaTextId.ArmsTremor;
                case TraumaLine.ArmsTremorAdsCancel4s:
                case TraumaLine.ArmsTremorAdsCancel3s:
                case TraumaLine.ArmsTremorAdsCancel2s: return TraumaTextId.ArmsAdsCancel;
                case TraumaLine.StomachZeroed: return TraumaTextId.StomachZeroed;
                default: return TraumaTextId.LegsLimpN1; // não alcançável (toast só em To != None)
            }
        }
    }
}
