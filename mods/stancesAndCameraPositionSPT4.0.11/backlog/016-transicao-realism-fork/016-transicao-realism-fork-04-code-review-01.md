# 016 — Fork realism · Code Review 01 (F0: instrumentação)

**Mod:** stancesAndCameraPositionSPT4.0.11 · **Sandbox:** `modded-realism/`
**Escopo:** F0 completa — `TransitionMetrics.cs` + 4 integrações + fork em si (diff total vs `modded/` @ 2.5.0)
**Data:** 2026-07-17
**Método:** 2 lentes adversariais paralelas (runtime corners · paridade fork×canônico) + **diff estrutural de
grafos** (graphify fresh do fork × grafo publicado do canônico) + verificação dos achados no código.

> Complementa o code-review inline feito durante a implementação (6 achados, já aplicados — ver histórico da
> tech-spec F0). Esta rodada valida o CONJUNTO da F0 com foco em corners de runtime e integridade do fork.

## Resumo

> 🔴 1 · 🟡 3 · 🟢 3 — **todos os 🔴/🟡 aplicados nesta rodada**; 🟢 documentados.
> **Paridade fork×canônico: 100% íntegra** (8 diferenças, todas mapeadas ao contrato do fork; zero contaminação).
> **Grafo (fork fresh): 528 nós / 720 arestas** — delta vs canônico = exatamente o `TransitionMetrics` (+21 nós,
> +35 arestas); **zero nós/arestas perdidos**.

## Achado colateral do processo (fora do fork): grafo canônico com cache sujo

O diff de grafos revelou **8 arestas fantasma no grafo do CANÔNICO** (`awake -[calls]-> bindstance/safeenable/...`)
— chamadas que deixaram de existir na reestruturação do `Awake` (v2.3.0), mas que o `graphify update` incremental
nunca removeu do cache. O grafo fresh do fork é o correto. **Ação tomada:** grafo canônico regenerado com cache
limpo nesta rodada. **Lição:** após refatoração estrutural, regenerar o grafo com `graphify-out/` limpo — o
update incremental preserva arestas de código que não existe mais.

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR2-1 | B | 🔴 | Flag off→on no meio de medição deixa `_measuring` órfão → linha falsa com rota/alvos stale | ✅ Aplicado |
| CR2-2 | B | 🟡 | Kick contamina pico/settle sem rastro — baseline dependente de config e atribuição falsa | ✅ Aplicado |
| CR2-3 | B | 🟡 | Amostra sucessora a `(interrupted)` parte do meio do caminho sem marcador — envenena agregados | ✅ Aplicado |
| CR2-4 | B | 🟡 | Debounce cegava a medição em voo (relógio parado + pico perdido durante dither de alvo) | ✅ Aplicado |
| CR2-5 | E | 🟢 | Observado sem guard `dt <= 0` (o local tem) | ✅ Aplicado |
| CR2-6 | E | 🟢 | Holster no meio da medição congela a amostra (fecha como `(interrupted)` ao ressacar) | 📝 Documentado |
| CR2-7 | C | 🟢 | `ApplySimpleRotationPatch` (mola própria) não alimenta métricas — gap de cobertura sem dado falso | 📝 Deferido p/ F1 (consolidação já prevista) |

## Correções aplicadas (resumo técnico)

- **CR2-1**: o gate da flag agora faz `if (_primed) Reset()` — desligar fecha tudo; religar re-prima do zero.
- **CR2-2**: `ApplyComplexRotationPatch.KickActive` exposto; amostras com kick ativo saem marcadas **`(kick)`**
  (pico/settle contaminados pela perturbação — filtráveis). ⚠️ **Baseline formal: medir com
  `Stance Kick Intensity = 0`** (ou filtrar as linhas `(kick)`), senão os números embutem a config do kick.
- **CR2-3**: amostra que nasce de uma interrupção sai marcada **`(chained)`** — partiu do meio do caminho, não
  entra na mediana da rota.
- **CR2-4**: `Sample()` extraído e chamado ANTES do tratamento de mudança de alvo — a medição em voo continua
  amostrando (relógio + picos) durante o debounce/dither.
- **CR2-5**: `if (dt <= 0f) return;` no `ObservedStanceAnimator.ApplyToWeaponRoot`.

## Verificado sem problema (cobertura da rodada)

Runtime: snap-on-fire (1 mudança de alvo; o snap é rejeitado durante ADS) · hold-breath (dreno roda antes; ordem
intacta) · sem arma (guard de `firearmController` precede tudo) · morte/MIA/extract/desconexão (`ResetMetrics` ←
`ResetState` ← `OnRaidEnd` idempotente ← `GameWorld.OnDestroy`) · hideout (métrica FUNCIONA no estande — o
baseline pode ser medido lá; `Debug Apply In Hideout` gateia só stamina/speed) · observado destruído no meio
(amostra descartada em silêncio; `_metrics` nunca null pós-Init) · dt=0/F12 (guard local + `_frames==0` no
Finish) · timeout não re-abre sem nova mudança de alvo (1 linha por transição, sem spam) · instâncias
independentes por corpo.

Paridade: 5 arquivos divergentes (todos mapeados: versão+banner, integrações), 2 só-fork (DIVERGENCE.md,
TransitionMetrics.cs), artefatos binários só no canônico, `Resources/` e 15 patches idênticos por `cmp` binário,
`git status` limpo. CRLF em disco no fork = artefato de checkout (`.gitattributes`), blobs commitados idênticos
em EOL.

## Notas para o gate F0 (usuário)

1. **Baseline com `Stance Kick Intensity = 0`** ou descartar linhas `(kick)` (CR2-2).
2. Linhas `(interrupted)`, `(chained)`, `(timeout)` **não entram** nas medianas por rota — são dado de
   interrupção, não de assentamento.
3. O baseline pode ser coletado **no hideout** (estande de tiro) — ambiente controlado confirmado funcional.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-17 | Code review 01 da F0 (2 lentes adversariais + diff de grafos). 1 🔴 + 3 🟡 aplicados; paridade fork×canônico 100%; grafo canônico regenerado (8 arestas fantasma de cache incremental removidas). Build final 0/0; artefato `-realism` atualizado. |
