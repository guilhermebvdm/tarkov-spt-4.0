---
title: "Anexo profiling 2026-08-22 — frames-baseline"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22, análise dimensional)
---

# frames-baseline — Caracterização de frametime e do gap managed/não-managed

> Investigação de profiling SPT vs SPT_2, 2026-08-22. Dimensão: baseline de frames.
> Fontes: `frames.csv`, `methods.csv`, `edges.csv` das 4 capturas (vanilla-A/B = ModAttribution, modded-A/B = UpdateOnly).
> Scripts: `frames_baseline_calc.py` (+ resultados em `frames_baseline_results.json`, `frames_baseline_gap_adjust.json`), todos nesta pasta.
> Todos os números calculados com Python 3.14 sobre os CSVs brutos — nada estimado de cabeça.

## 0. Metodologia e semântica validada

- **Percentis**: interpolação linear sobre a série ordenada (mesma do `digest.py`). p999 = p99,9 (com 1.5–2.1k frames, ~2º pior frame — tratar como indicativo).
- **`ManagedProfiledMs` = Σ self de todos os métodos instrumentados na main thread do frame.** Validado empiricamente: soma de `ManagedProfiledMs` sobre os frames = soma de `SelfTotalMs` da main thread em `methods.csv` nas 4 capturas (ex.: vanilla-A 11.939 ms = 11.939 ms; modded-A 10.467 ≈ 10.460 ms). Equivale à soma dos inclusive dos roots instrumentados.
- **Consequência para o gap** (`FrameMs − ManagedProfiledMs`): método não-instrumentado chamado POR um root instrumentado é absorvido no self do root — **conta** no managed. Só cai no gap o que roda **fora de qualquer root instrumentado**: código nativo do engine (render, física, animação, present/vsync) + managed cujo call stack inteiro está fora da superfície instrumentada (no UpdateOnly: `Player.LateUpdate` e tudo que ele chama, patches Harmony disparados fora de Update/LateUpdate/FixedUpdate, corrotinas, continuations async, callbacks de render, OnGUI).
- Confirmação da assimetria: no vanilla `EFT.Player.LateUpdate` é ROOT (nenhum caller nos edges — Unity chama direto); no modded ele não existe em `methods.csv`, e `PlayerAIDataClass.LateUpdate` (callee dele no vanilla, 19,2 ms totais) aparece no modded como root órfão (79.104 root calls) — prova de que o profiler entra no meio da subárvore e perde o self do pai.
- Gap nunca negativo nas 4 capturas (0 frames com managed > frame) — consistência ok.

## 1. Distribuições completas

### 1.1 FrameMs

| Captura | n | avg | p50 | p90 | p95 | p99 | p99,9 | max | desvio |
|---|---|---|---|---|---|---|---|---|---|
| vanilla-A | 2082 | 14,41 | 13,92 | 17,62 | 19,77 | 23,66 | 48,07 | 51,21 | 2,97 |
| vanilla-B | 1766 | 16,99 | 16,82 | 19,84 | 20,90 | 23,81 | 46,97 | 53,41 | 2,86 |
| modded-A | 1507 | **19,92** | 18,92 | **24,35** | **25,76** | **31,74** | **115,53** | **211,55** | **7,11** |
| modded-B | 1767 | 16,97 | 16,60 | 21,06 | 22,89 | 26,68 | 50,45 | **110,53** | 4,41 |

### 1.2 ManagedProfiledMs

| Captura | avg | p50 | p90 | p95 | p99 | p99,9 | max |
|---|---|---|---|---|---|---|---|
| vanilla-A | 5,73 | 5,18 | 7,74 | 9,41 | 12,56 | 33,4* | 38,87 |
| vanilla-B | 5,87 | 5,48 | 7,63 | 8,53 | 10,98 | 33,6* | 39,08 |
| modded-A | **6,95** | 6,20 | 8,69 | 9,86 | 12,69 | 104,5* | **200,12** |
| modded-B | 5,42 | 5,04 | 6,43 | 8,89 | 11,56 | 46,5* | **95,40** |

\* p99,9 do managed derivado da mesma interpolação (indicativo).

Lembrete crítico: o managed do modded **subconta** (UpdateOnly não vê `Player.LateUpdate` nem patches fora da superfície Update), e o modded rodou com **~metade a ~1/3 dos bots** do vanilla (proxies: 25,45–32,56 `Player.LateUpdate` calls/f no vanilla vs ~13,5 / ~9,3 instâncias no modded). Mesmo assim modded-A tem managed médio **21% maior** que vanilla-A.

### 1.3 Histograma por faixas (frames | % frames | % do tempo total)

| Faixa (ms) | vanilla-A | vanilla-B | modded-A | modded-B |
|---|---|---|---|---|
| 0–8 | 1 (0,0% / 0,0%) | 1 (0,1% / 0,0%) | 0 | 0 |
| 8–16,7 | 1798 (86,4% / 81,3%) | 850 (48,1% / 42,7%) | 294 (19,5% / 15,1%) | 898 (50,8% / 42,4%) |
| 16,7–25 | 270 (13,0% / 17,2%) | 902 (51,1% / 55,8%) | 1099 (72,9% / 73,1%) | 828 (46,9% / 53,3%) |
| 25–33 | 8 (0,4% / 0,7%) | 7 (0,4% / 0,7%) | **103 (6,8% / 9,2%)** | 34 (1,9% / 3,0%) |
| 33–50 | 3 (0,1% / 0,4%) | 5 (0,3% / 0,7%) | 7 (0,5% / 0,9%) | 5 (0,3% / 0,7%) |
| 50–100 | 2 (0,1% / 0,3%) | 1 (0,1% / 0,2%) | 1 (0,1% / 0,2%) | 1 (0,1% / 0,2%) |
| >100 | 0 | 0 | **3 (0,2% / 1,5%)** | **1 (0,1% / 0,4%)** |

### 1.4 % do TEMPO total em frames lentos (métrica de experiência, não de contagem)

| Captura | % tempo em frames >25 ms | % tempo >33,3 ms | (% frames >25 / >33,3) |
|---|---|---|---|
| vanilla-A | 1,4% | 0,7% | 0,6% / 0,2% |
| vanilla-B | 1,5% | 0,7% | 0,7% / 0,3% |
| modded-A | **11,8%** | **2,6%** | 7,6% / 0,7% |
| modded-B | **4,3%** | **1,3%** | 2,3% / 0,4% |

O modded passa **3× a 8× mais do tempo de parede** acima de 25 ms que o vanilla. No modded-A a banda 25–33 ms sozinha consome 9,2% do tempo (103 frames) — é piora **crônica** de cauda moderada, não só hitch isolado. Frames >100 ms **só existem no modded** (211,5 e 2 outros em A; 110,5 em B); teto do vanilla nas duas capturas: 53,4 ms.

## 2. Gap FrameMs − ManagedProfiledMs

### 2.1 Distribuição do gap por frame

| Captura | avg | p50 | p95 | p99 | max | managed share médio |
|---|---|---|---|---|---|---|
| vanilla-A | 8,68 | 8,54 | 10,82 | 12,14 | 14,76 | 39,8% |
| vanilla-B | 11,12 | 11,05 | 13,57 | 15,17 | 20,31 | 34,6% |
| modded-A | **12,97** | 12,69 | **17,04** | 19,67 | 25,69 | 34,9% |
| modded-B | **11,55** | 11,39 | **15,35** | 17,13 | **40,92** | 32,0% |

Médias por ambiente: vanilla 9,90 ms de gap; modded 12,26 ms → **+2,36 ms/f de gap** (e +1,15 ms/f de managed instrumentado no caso modded-A vs média vanilla). Em p95 o gap modded é +3,5 a +6,2 ms acima do vanilla-A.

Observação: o gap do vanilla também varia entre runs (8,68 → 11,12; vanilla-B tinha ~28% mais bots e o custo nativo por bot — animação/física — vive no gap). Usar 9,90 como referência já é generoso com o modded, porque o vanilla carregava 2–3,5× mais bots.

### 2.2 Quanto do gap modded é managed não-instrumentado? (ajuste)

Método: roots da main thread do vanilla (ModAttribution) **ausentes** do `methods.csv` do modded, somando inclusive-como-root (`InclusiveTotalMs × RootCalls/Calls`). Esses métodos existem e rodam no modded (código comum EFT/SPT/Fika) mas caem no gap lá. Não-roots ausentes NÃO entram (são absorvidos no self de algum root instrumentado do modded — já contam no managed).

| Comparação | Total roots ausentes | plugins vanilla-only (Freecam/DevTool) | plugins comuns (Fika/ConfigMgr/profiler) | código do jogo | Projeção managed-no-gap (excl. vanilla-only) |
|---|---|---|---|---|---|
| vanilla-A → modded-A | 2,232 ms/f | 0,080 | 0,121 | 2,031 | 2,151 |
| vanilla-A → modded-B | 2,242 ms/f | 0,080 | 0,122 | 2,040 | 2,162 |
| vanilla-B → modded-A | 2,027 ms/f | 0,003 | 0,161 | 1,863 | 2,024 |
| vanilla-B → modded-B | 2,035 ms/f | 0,003 | 0,161 | 1,870 | 2,032 |

Dominado por **um único método**: `EFT.Player.LateUpdate` = 1,969 ms/f (vanilla-A) / 1,814 ms/f (vanilla-B) inclusive-como-root — 87–90% da projeção. Resto: `Plugin.OnGUI` do profiler (0,085–0,090), `LaserBeam.LateUpdate` (0,039–0,057), roots Fika esporádicos (<0,02).

**Mas `Player.LateUpdate` escala com a população de players/bots**, e o modded tinha menos: por call, custa 77,4 µs (vanilla-A, 25,45 calls/f) a 55,7 µs (vanilla-B, 32,56 calls/f). Projeção escalada pela população do modded:

| Captura | população (proxy) | Player.LateUpdate projetado no gap | Projeção total managed-no-gap* |
|---|---|---|---|
| modded-A | ~13,5 | 0,75–1,04 ms/f | **~0,8–1,1 ms/f** |
| modded-B | ~9,3 | 0,52–0,72 ms/f | **~0,6–0,8 ms/f** |

\* somando os ~0,06–0,15 ms/f de roots comuns não-populacionais.

### 2.3 Decomposição do excesso médio do modded

Referência = média dos 2 vanilla (frame 15,70 / managed 5,80 / gap 9,90):

| | modded-A | modded-B |
|---|---|---|
| Δ FrameMs médio | **+4,22 ms/f** | +1,27 ms/f |
| … Δ managed instrumentado | +1,15 | −0,38 |
| … Δ gap bruto | +3,07 | +1,65 |
| …… managed comum projetado (esc. população) | ~0,8–1,1 | ~0,6–0,8 |
| …… **residual: nativo OU managed invisível dos mods** | **~+2,0 a +2,3** | **~+0,9 a +1,1** |

O residual é o teto do "custo nativo real" e o piso do desconhecido: patches Harmony dos ~100 mods disparados fora da superfície Update, corrotinas/async de mods, callbacks de render — tudo invisível no UpdateOnly e **não incluído** na projeção (ela só cobre código comum aos dois ambientes). E o residual está **subestimado**: o vanilla gerou seu gap de 9,90 ms carregando 25–33 bots (custo nativo de animação/física por bot), o modded gerou 11,55–12,97 com 9–14.

## 3. Variância run-to-run (régua de ruído)

Deltas dentro do mesmo ambiente (B − A):

| Estatística | ruído intra-vanilla | ruído intra-modded | Δ cross-env (média mod − média van) | excede a régua?* |
|---|---|---|---|---|
| avg | +2,58 | −2,95 | +2,74 | não (marginal) |
| p50 | +2,90 | −2,32 | +2,40 | não |
| p90 | +2,21 | −3,30 | +3,98 | **sim** |
| p95 | +1,13 | −2,87 | +3,98 | **sim** |
| p99 | +0,14 | −5,06 | +5,48 | **sim (marginal)** |
| p99,9 | −1,10 | −65,08 | +35,47 | não pela régua modded** |
| max | +2,20 | −101,01 | +108,73 | **sim** |

\* régua = max(|ruído vanilla|, |ruído modded|).
\*\* armadilha: o "ruído" intra-modded em p99,9/max (65–101 ms) É o próprio fenômeno investigado (hitches gigantes variando entre runs), não ruído de base. Régua limpa é a do vanilla: p99,9 ±1,1 e max ±2,2 — pelas quais +35,5 e +108,7 excedem por ordens de magnitude. **As DUAS capturas modded têm max >110 ms; as DUAS vanilla ficam ≤53,4 ms** — separação perfeita entre ambientes, sem sobreposição.

Leituras honestas:
- **Na média, modded-B (16,97) ≅ vanilla-B (16,99)** — o custo médio crônico do modded com população baixa (~9 bots) fica dentro do ruído run-to-run. A piora média só excede ruído no modded-A (19,92, que está 2,93 ms acima do PIOR vanilla).
- Mas a comparação de médias é **enviesada a favor do modded**: vanilla rodou com 2–3,5× mais bots. Piora robusta a ruído e população: cauda p90/p95 (+4,0 ms), % do tempo >25 ms (3–8×), max (2–4×).

## 4. Evolução temporal dentro dos 30 s

Média de FrameMs por segundo (ms):

```
vanilla-A: 13,4 14,8 14,5 13,4 12,1 12,9 13,8 14,2 14,4 13,7 12,3 13,2 15,2 11,9 13,3 15,1 21,3 20,7 15,9 14,6 16,8 14,5 16,3 15,9 14,5 14,0 15,1 15,5 13,1 13,4
vanilla-B: 19,0 17,6 18,6 17,1 16,9 17,2 18,0 16,9 16,5 16,0 17,1 19,0 19,5 18,2 15,1 16,0 18,3 18,1 19,6 17,7 16,1 14,8 15,4 13,1 19,5 16,4 16,0 15,9 16,6 18,0
modded-A:  23,5 24,7 24,1 25,2 31,3 23,4 19,6 18,8 19,8 17,1 18,1 17,3 20,1 20,5 23,7 21,6 18,3 19,1 20,0 19,9 19,5 18,3 17,5 16,8 19,5 17,9 16,8 18,4 19,2 19,6
modded-B:  19,3 19,2 21,5 18,8 23,7 17,8 16,0 16,4 15,3 15,9 17,2 13,7 12,8 14,7 17,6 16,0 19,8 19,7 16,4 16,7 15,2 12,0 12,9 15,3 23,8 20,1 19,4 20,3 18,7 17,5
```

Regressão linear FrameMs ~ t (por frame) e metades:

| Captura | slope (ms/s) | Δ projetado em 30 s | avg 1ª metade → 2ª | p95 1ª → 2ª |
|---|---|---|---|---|
| vanilla-A | +0,059 | +1,76 | 13,48 → 15,34 | 16,27 → 21,27 |
| vanilla-B | −0,062 | −1,85 | 17,39 → 16,60 | 20,81 → 21,03 |
| modded-A | **−0,194** | −5,82 | 21,26 → 18,58 | 27,34 → 22,09 |
| modded-B | −0,031 | −0,92 | 16,90 → 17,03 | 22,82 → 23,01 |

**Não há degradação progressiva dentro de nenhuma captura de 30 s.** O modded-A na verdade MELHORA (−0,19 ms/s): os primeiros ~6 s são os piores (23,5–31,3 ms de média por segundo; o hitch de 211,5 ms cai no segundo 4) — a captura começou durante um período pesado (consistente com atividade de spawn/estabilização) que decai. modded-B mostra ondas (picos nos segundos 4, 16–17, 24–27) sem tendência. Limitação: 30 s é janela curta demais para detectar o crescimento lento já observado externamente (RAM 10→33 GB em escala de dezenas de minutos) — esta dimensão não descarta GROW, só não o enxerga.

## 5. Caveat: assimetria de overhead do próprio profiler

| Captura | métodos instrumentados (main) | calls instrumentadas | calls/frame |
|---|---|---|---|
| vanilla-A | 900 | 3.733.386 | 1.793 |
| vanilla-B | 846 | 3.479.656 | 1.970 |
| modded-A | 351 | 2.665.926 | 1.769 |
| modded-B | 338 | 2.696.618 | 1.526 |

ModAttribution instrumenta 2,5× mais métodos, mas o volume de **calls/frame** (o que custa de fato) é comparável (1,5–2,0 mil nos quatro). O vanilla ainda paga timing de 35 patches Harmony. Direção do viés: se algo, o overhead do vanilla é ligeiramente MAIOR → o frametime vanilla medido está inflado → os deltas modded−vanilla estão **subestimados**, não superestimados. Magnitude pequena (mesma ordem nos dois); não muda nenhuma conclusão.

## 6. Conclusões da dimensão

1. **Quanto pior:** na média, +4,2 ms/f (modded-A vs média vanilla) e +1,3 ms/f (modded-B) — este último dentro do ruído run-to-run (±2,6–2,9 ms), MAS com o vanilla carregando 2–3,5× mais bots. Na cauda a piora é inequívoca e excede qualquer régua de ruído: p95 +4,0 ms, p99 +5,5 ms, % do tempo >25 ms de 1,4–1,5% para 4,3–11,8% (3–8×), max de ≤53,4 ms para 110,5–211,5 ms nas duas capturas modded.
2. **Onde vive a piora média (modded-A):** +1,15 ms/f em managed instrumentado (subcontado — o real é maior) + +3,07 ms/f de gap, do qual só ~0,8–1,1 ms/f se explica por managed comum não-instrumentado (quase todo `Player.LateUpdate`, escalado pela população). Sobram **~+2,0–2,3 ms/f** de nativo ou managed invisível dos mods (patches Harmony fora de Update, corrotinas, callbacks) — invisível por construção no modo UpdateOnly.
3. **Onde vive a piora de cauda:** duas naturezas distintas. A cauda moderada (25–33 ms, 9,2% do tempo do modded-A) é **gap-pesada** — nos frames >25 ms do modded o managed é só 43,6–47,3% e o gap médio sobe para 16,7–17,5 ms/f (vs 12–13 do frame típico). Já os hitches extremos (>50 ms) são **80–89% managed** e instrumentados (200,1 ms managed no frame de 211,5 ms — o `AsyncWorker` já mapeado em outra dimensão). Vanilla nos frames >25 ms mantém 59–75% managed.
4. **Sem degradação intra-captura** em 30 s; modded-A melhora ao longo da janela (começou num período pesado).

## 7. Próximos passos de medição sugeridos

1. **Recapturar o modded em ModAttribution** (mesmo modo dos dois lados): torna visíveis os patches Harmony dos ~100 mods e elimina 90% da incerteza do item 2.3 — o residual de ~2 ms/f é a maior incógnita aberta desta dimensão.
2. **Registrar/pareear população de bots** (contagem contínua ou captura com população fixada) — toda comparação média atual é enviesada a favor do modded.
3. **Capturas longas (5–10 min)** para a dimensão GROW (degradação lenta, RAM) que 30 s não enxerga.
4. Para o residual nativo do gap: um perfilador nativo (Unity Profiler build development, ou ETW/Superluminal) numa sessão modded, focando render/física/animação — o profiler managed não alcança isso.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-22 | Claude (subagente frames-baseline) | Análise inicial completa. |
| 2026-08-22 | Guilherme | docs(perf): add DynamicSpawn audit report + ICM/Stances optimization handoffs |
