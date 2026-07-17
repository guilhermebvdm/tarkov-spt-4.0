# 016 — Spec técnica · F0 (instrumentação `Debug Transition Metrics`)

**Mod:** stancesAndCameraPositionSPT4.0.11 · **Sandbox:** `modded-realism/` SOMENTE
**Spec funcional:** [016-...-01-spec.md](016-transicao-realism-fork-01-spec.md) (F0)
**Criado:** 2026-07-17

> Escopo desta tech-spec: SÓ a instrumentação da F0. As tech-specs de F1+ virão após o gate humano da F0
> (baseline + diagnóstico G36), pois os números informam defaults e tolerâncias.

## Objetivo

Medir, por transição concluída, no fork (engine atual = mola legacy, paridade 2.5.0): **pico de excursão além do
alvo** (posição por eixo, rotação por eixo), **nº de cruzamentos de sinal do erro** após o 1º alcance do alvo, e
**tempo de assentamento**. 1 linha de log por transição, com **origem** (`local`/`observed:<id>`). Ferramenta
permanente (não log temporário), custo ~zero com a flag off.

## Arquivos

| Arquivo | Ação | O quê |
|---|---|---|
| `modded-realism/TransitionMetrics.cs` | **criar** | classe de medição, instanciável por corpo |
| `modded-realism/Patches/ApplyComplexRotationPatch.cs` | editar | alimentar a instância `local` no postfix |
| `modded-realism/Networking/ObservedStanceAnimator.cs` | editar | alimentar instância própria por observado |
| `modded-realism/Plugin.cs` | editar | bind `Debug Transition Metrics` (seção `Debug (Advanced)` existente) |

## Design — `TransitionMetrics`

Estado por instância (sem estático — origem vai no ctor: `new TransitionMetrics("local")` /
`new TransitionMetrics($"observed:{profileId}")`):

- `Vector3 _lastTargetPos/_lastTargetEuler` — detecção de **nova transição** = alvo mudou além de ε
  (`0.0005f` pos / `0.05f` rot; ângulos comparados via `Mathf.DeltaAngle`).
- Por eixo (6 canais: pos XYZ + rot PYR): `startValue`, `reached` (já cruzou/alcançou o alvo?),
  `peakBeyond` (máx. |valor−alvo| DEPOIS do 1º alcance, na direção oposta à de partida), `crossings`
  (mudanças de sinal do erro após 1º alcance).
- `float _elapsed` (soma de `dt`), `int _settledFrames` — assentado = |erro| < ε em TODOS os canais por
  **10 frames** consecutivos (ε: pos `0.001f` = 1 mm; rot `0.25f`°).
- `string _route` — capturada no INÍCIO da transição: `"{stanceFrom}{aimFrom}->{stanceTo}{aimTo}"`
  (ex.: `S2->ADS`, `S0->S1`, `ADS->S2`). Rota vem do chamador (que conhece stance/isAiming).
- Ao assentar OU em timeout (**5 s** — transição interrompida/nunca assenta): loga e zera. Formato:

```
[METRICS] local | S2->ADS | posPeak cm X 0.1 Y 0.3 Z 5.2 | rotPeak ° P 2.1 Y 0.4 R 0.2 | cross 3 | settle 0.42s
[METRICS] observed:5f... | S0->S1 | ... | cross 0 | settle 0.31s (timeout)
```

- Método único por frame: `Feed(Vector3 tgtPos, Vector3 tgtEuler, Vector3 curPos, Vector3 curEuler, float dt,
  string route)`. Early-return se `Plugin._DebugTransitionMetrics?.Value != true` (o if é o único custo com
  flag off). Nenhuma alocação por frame (buffers/structs reutilizados; string da rota só no início da transição).

## Pontos de integração

1. **Local** — `ApplyComplexRotationPatch` postfix, logo após `CurrentEuler/CurrentPosition` serem atualizados
   (`:275-276` na numeração atual do fork): `_metrics.Feed(targetPosition, targetEuler, CurrentPosition,
   CurrentEuler, dt, RouteString())`. `RouteString()` usa `StanceManager.CurrentStance` + `isAiming` (ambos já
   no escopo). Instância `static readonly` no patch (1 corpo local).
   ⚠️ NÃO instrumentar `ApplySimpleRotationPatch` nesta fase (3ª pessoa local não participa do baseline).
2. **Observados** — `ObservedStanceAnimator.ApplyToWeaponRoot` após o `SpringLerp*` (`:50-51`):
   instância por componente (`readonly`), rota derivada de `_stance`/`_isAiming` do pacote.
3. **Config** — `Plugin.cs`, seção `Debug (Advanced)` (constante `DebugSettings`), após `Debug Apply In Hideout`:
   key **`Debug Transition Metrics`** (sem `=` no nome ✓), bool, default `false`,
   `IsAdvanced = true`, tooltip bilíngue EN/PT explicando as colunas do log.
4. **Reset de raid** — `StanceManager.ResetState()` chama `TransitionMetrics.ResetAll()` (registro estático de
   instâncias criadas ou reset individual pelos donos — decidir na implementação pelo mais simples; observados
   morrem com o componente, então o crítico é a instância local).

## Decisões/armadilhas

- **Eixos**: manter a convenção LOCAL da arma em tudo (X=pitch, Y=roll, Z=yaw na rotação; X=lateral,
  Y=longitudinal, Z=vertical na posição — o "pico vertical" do critério (b) é o **Z da posição**).
- **Ângulos**: erro angular SEMPRE via `Mathf.DeltaAngle` (wrap-around; lição da mola).
- **Rota em transição interrompida**: se o alvo muda ANTES de assentar, fecha a medição corrente com sufixo
  `(interrupted)` e abre nova — interrupções são dado, não lixo (é o caso "troca de alvo em voo" da spec).
- **Sem `Time.time` para timestamp de log** (o log do BepInEx já data); `_elapsed` soma `dt` recebido.
- **Threading**: tudo no main thread do Unity (postfixes de PWA e LateUpdate) — sem locks.

## Fora de escopo (F0)

Qualquer mudança de comportamento da mola/transição; o engine novo (F1); o gate (F2). A instrumentação lê,
nunca escreve no estado da transição.

## Review técnico 01 — aplicado (2026-07-17)

9 achados (4 🔴, 4 🟡, 1 🟢), todos incorporados ao design final:

1. 🔴 `Feed` recebe `(Stance stance, bool isAiming)` cru — a string de rota é montada DENTRO, só após o
   early-return da flag e só na detecção de nova transição (senão a interpolação no call-site alocaria todo
   frame mesmo com a flag off).
2. 🔴 Colunas de rotação do log com NOME por extenso (`pitch/roll/yaw`), mapeadas explicitamente para
   `.x/.y/.z` — "P Y R" abreviado reintroduziria o bug de rótulo da MP-01-02 pela 3ª via.
3. 🔴 `cross` = **soma** dos cruzamentos dos 6 canais (documentado no tooltip).
4. 🔴 Assentamento por **tempo acumulado** (`_settledTime ≥ 0.15s`), não por frames — FPS-independente; o log
   registra também o dt médio da transição.
5. 🟡 Canal com `|start−target| < ε` no frame 0: `reached=true`, `peakBeyond=0`, excluído do `cross` (sem
   direção de referência). É o caso comum (Yaw/Roll default 0 em várias stances).
6. 🟡 Debounce de slider-drag: nova transição só é aberta quando o alvo fica **estável por 3 frames**; enquanto
   o alvo muda a cada frame (drag no F12), a medição corrente **congela** e, quando o alvo estabiliza, fecha como
   `(interrupted)` — 1 linha, sem spam. *(Ajustado no code-review 01: o rótulo `(unstable)` não existe; o
   comportamento real é congelar+interrupted.)*
7. 🟡 Conversão `×100` (m→cm) SÓ na formatação do log; estado interno em metros.
8. 🟡 `_metrics` do observado é criada em `Init(ObservedPlayer p)` (não em inicializador de campo — `Init` roda
   depois do `AddComponent`, e só ali existe `ProfileId`).
9. 🟢 Rota = 1 token por lado; rótulo `ADS` só quando `isAiming && _ResetOnADS` (senão a mira não muda o alvo e
   o rótulo enganaria).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-17 | Tech-spec F0 criada (g-autodev). |
| 2026-07-17 | Review técnico 01 (sub-agent adversarial): 9 achados aplicados — ver seção acima. Refs de integração conferidas contra o fork (3 aspectos sem problema). |
| 2026-07-17 | Code-review 01 da implementação (sub-agent adversarial): 0 🔴, 4 🟡, 2 🟢 — todos aplicados. Destaques: token `S0` quando fora de stance (rota não mente mais ao sair de stance mirando/prone), priming ao ligar a flag (sem amostra falsa `settle 0.00s`), promoção de canal excluído quando a velocidade residual o tira do alvo, regra única de cruzamento (sem ruído ±1 por sorteio de frame), reposição dos frames de debounce no relógio. |
