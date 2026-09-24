namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Resolução PURA da matriz (docs/trauma-matrix.md): avalia TODAS as linhas cuja condição casa
    /// e retorna a de maior severidade (decisão 2) — ranking = ordem numérica do enum (D1 / decisão 3).
    /// D4: mesmo membro zerado E quebrado conta nas DUAS contagens (responsabilidade do chamador, que
    /// deriva zeroed/broken de IsBodyPartDestroyed/IsBodyPartBroken independentes).</summary>
    internal static class TraumaMatrixResolver
    {
        // Testes de mesa (spec 002 §5 — semântica "pelo menos N"; max() das linhas satisfeitas):
        //   Z2  b0 pk=F → z>=1:N1, z>=2:Crouch+N2                    → LegsCrouchPlusLimpN2
        //   Z0  b2 pk=F → b>=1:N1, b>=2:FallCycle                    → LegsFallCycle
        //   Z2  b2 pk=F → ...z2q2:FallCycle                          → LegsFallCycle
        //   Z2  b1 pk=F → z>=1:N1, b>=1:N1, z1q1:N2, z>=2:Crouch+N2  → LegsCrouchPlusLimpN2 (combo misto do stub)
        //   Z2  b0 pk=T → z>=1:None, z>=2:N2                         → LegsLimpN2  (item 017)
        //   Z2  b1 pk=T → z1q1:N1, z>=2:N2                           → LegsLimpN2  (item 017 — era N1)
        //   Z2  b2 pk=T → z1q1:N1, z>=2:N2, b>=2:N1, z2q2:N2         → LegsLimpN2
        //   Z1  b0 pk=T → z>=1:None                                  → None
        internal static TraumaLine ResolveLegs(int zeroed, int broken, bool painkiller)
        {
            TraumaLine best = TraumaLine.None;

            // Sem analgésico:  Z1→N1 | Q1→N1 | Z1+Q1→N2 | Z2→CrouchPlusLimpN2 | Q2→FallCycle | Z2+Q2→FallCycle
            // Com analgésico:  Z1→None | Q1→None | Z1+Q1→N1 | Z2→N2 | Q2→N1 | Z2+Q2→N2
            if (zeroed >= 1) best = Max(best, painkiller ? TraumaLine.None : TraumaLine.LegsLimpN1);
            if (broken >= 1) best = Max(best, painkiller ? TraumaLine.None : TraumaLine.LegsLimpN1);
            if (zeroed >= 1 && broken >= 1) best = Max(best, painkiller ? TraumaLine.LegsLimpN1 : TraumaLine.LegsLimpN2);
            // ref: item 017 (achado S1.7 do 1º teste in-game) — Z2 sob analgésico era rebaixado a LegsLimpN1,
            // e o bloqueio de sprint só vale no tier N2 (IsN2Tier), então duas pernas zeradas com analgésico
            // LIBERAVAM correr. Agora resolve para LegsLimpN2: mesma manqueira severa e mesmo bloqueio de
            // sprint do caso sem analgésico, mas SEM o one-shot de agachar — só LegsCrouchPlusLimpN2 publica
            // o one-shot, e a decisão do usuário é que o analgésico continue removendo o agachar involuntário.
            // LegsLimpN2 já existia (produzido por Z2+Q2 sob analgésico), já é tier N2 e já mapeia em
            // LineTargetPercent — nenhum valor de enum, locale ou mapeamento novo.
            if (zeroed >= 2) best = Max(best, painkiller ? TraumaLine.LegsLimpN2 : TraumaLine.LegsCrouchPlusLimpN2);
            if (broken >= 2) best = Max(best, painkiller ? TraumaLine.LegsLimpN1 : TraumaLine.LegsFallCycle);
            if (zeroed >= 2 && broken >= 2) best = Max(best, painkiller ? TraumaLine.LegsLimpN2 : TraumaLine.LegsFallCycle);

            return best;
        }

        // Testes de mesa:
        //   Z2 b0 pk=F → Tremor, AdsCancel4s              → ArmsTremorAdsCancel4s
        //   Z0 b2 pk=F → Tremor, AdsCancel3s              → ArmsTremorAdsCancel3s
        //   Z2 b2 pk=F → 4s, 3s, 2s                       → ArmsTremorAdsCancel2s
        //   Z1 b1 pk=F → Tremor (x3)                      → ArmsTremor
        //   Z2 b1 pk=T → z>=2:Tremor                      → ArmsTremor (combo misto)
        //   Z1 b0 pk=T → None                             → None
        internal static TraumaLine ResolveArms(int zeroed, int broken, bool painkiller)
        {
            TraumaLine best = TraumaLine.None;

            // Sem analgésico:  Z1/Q1/Z1+Q1→Tremor | Z2→AdsCancel4s | Q2→AdsCancel3s | Z2+Q2→AdsCancel2s
            // Com analgésico:  Z1/Q1/Z1+Q1→None | Z2/Q2/Z2+Q2→Tremor
            if (zeroed >= 1) best = Max(best, painkiller ? TraumaLine.None : TraumaLine.ArmsTremor);
            if (broken >= 1) best = Max(best, painkiller ? TraumaLine.None : TraumaLine.ArmsTremor);
            if (zeroed >= 1 && broken >= 1) best = Max(best, painkiller ? TraumaLine.None : TraumaLine.ArmsTremor);
            if (zeroed >= 2) best = Max(best, painkiller ? TraumaLine.ArmsTremor : TraumaLine.ArmsTremorAdsCancel4s);
            if (broken >= 2) best = Max(best, painkiller ? TraumaLine.ArmsTremor : TraumaLine.ArmsTremorAdsCancel3s);
            if (zeroed >= 2 && broken >= 2) best = Max(best, painkiller ? TraumaLine.ArmsTremor : TraumaLine.ArmsTremorAdsCancel2s);

            return best;
        }

        // Estômago NÃO tem resolver de coluna: linha única latched no engine (D8).

        private static TraumaLine Max(TraumaLine a, TraumaLine b)
        {
            return a >= b ? a : b;
        }
    }
}
