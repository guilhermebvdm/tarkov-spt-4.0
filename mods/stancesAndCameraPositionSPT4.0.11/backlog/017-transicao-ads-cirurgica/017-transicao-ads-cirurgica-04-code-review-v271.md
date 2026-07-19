# 017 — Code Review · v2.7.1 (waypoint por stance + movement mult fora de Advanced)

**Mod:** stancesAndCameraPositionSPT4.0.11 · **Escopo:** commit `7329a57` (v2.7.1)
**Data:** 2026-07-19 · **Método:** lente adversarial de runtime/semântica, refs verificadas no código.
**Motivo:** review pedido pelo usuário — a v2.7.1 tinha ido sem review formal (era mudança mecânica de config).

## Resumo

> 🔴 0 · 🟡 1 · 🟢 1 — o 🟡 e o 🟢 **corrigidos** na v2.8.1.

A mudança (waypoint global → por stance) preservou o comportamento byte-a-byte; o único defeito real era de
**apresentação no F12** (colisão de Order), não de runtime.

## Achados

### CR-1 · 🟡 → ✅ Corrigido (v2.8.1) — colisão de Order na Stance 2

**Local:** `Plugin.cs` (BindStance) — `Order = orderBase + 12/+11` (17/16).

**Problema:** as entries de rotação/posição de cada stance usam Orders **absolutos distintos por seção** (Stance 1
~22-28, Stance 2 ~15-21, Stance 3 ~8-14). Um Order **relativo fixo** (17/16) para o waypoint cai em posição
diferente em cada seção e **colide exatamente na Stance 2**: `Stance 2 ADS Waypoint` (17) empata com
`Stance 2 Forward/Backward` (17), e o Time (16) com `Stance 2 Up/Down` (16). No F12 os toggles apareciam
intercalados com os sliders de posição na Low Ready, e em posição inconsistente entre as stances.

**Correção (v2.8.1):** Order **fixo 30/29**, acima do teto de qualquer seção de stance (Stance 1 Sprint = 28) → o
waypoint aparece no **topo** das três seções, consistente e sem empate.

**Revisão de layout (v2.8.2):** por pedido do usuário, o par foi movido para o **rodapé** de cada seção — Order
**-1/-2**, abaixo da entry mais baixa (`Snap to Stance 0 on Fire` = 0). As posições ficam na ordem natural em cima;
os pares experimentais/calibráveis do waypoint no fim, consistentes nas três. Continua fixo (nenhuma posição usa
Order negativo) → sem empate.

### CR-2 · 🟢 → ✅ Corrigido (v2.8.1) — fallback do time

`AdsWaypoint.cs`: `cfg.AdsWaypointTime?.Value ?? 0` → `?? 120`. Inalcançável hoje (o `AdsWaypoint` e o
`AdsWaypointTime` são bindados juntos, então quando o enable é true o time nunca é null), mas `120` é simétrico ao
default e defende contra um caller futuro que pule o guard.

## Verificado sem problema

- **Ordem bind-antes-patch:** `BindAllConfig()` popula `_stanceConfigs` inteiro dentro do `try` do `Awake` e só
  então `EnableEverything()` liga os patches — nenhum patch roda com o dicionário parcial.
- **Sentinel null (Stance 0):** `TryGetValue` só executa com `isInStance` true (curto-circuito), e
  `IsInStance ⇔ CurrentStance != Default`, então `stance` nunca é Default nesse ramo; ambos os call-sites
  (local `currentStance`, observado `(Stance)_stance` com guard `_stance>0`) respeitam o invariante. O
  `cfg.AdsWaypoint?.Value ?? false` é defensivo.
- **Mudança de default:** true/120 nas 3 stances = idêntico ao global antigo para quem não mexeu; quem tinha a
  chave global customizada perde o valor (chave órfã no `.cfg`) — reconhecido no commit, não é regressão silenciosa.
- **Movement Speed Multiplier fora de Advanced:** só o flag `IsAdvanced` mudou (Order intacto); nada lê `IsAdvanced`
  para lógica — só visibilidade.
- **Regressão vs F1:** com defaults, comportamento byte-a-byte idêntico ao waypoint global — só a fonte da config
  mudou de campo estático para `_stanceConfigs[stance]`.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Code review da v2.7.1 (adversarial). 1 🟡 (colisão de Order na Stance 2) + 1 🟢 corrigidos na v2.8.1. Comportamento do waypoint inalterado; o defeito era de apresentação no F12. |
