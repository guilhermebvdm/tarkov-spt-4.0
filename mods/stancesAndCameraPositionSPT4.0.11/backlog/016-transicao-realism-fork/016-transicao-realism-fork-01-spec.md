# 016 — Fork realism: transições por curvas + gate de aim-speed

> **Status:** 🔴 **CANCELADO na F0 (NO-GO, 2026-07-17).** O usuário testou o **Fontaine-StanceOverhaul
> standalone** e não achou a experiência melhor que a nossa — portar a sensação dele para o nosso mod
> herdaria justamente o que foi rejeitado, então o fork foi cortado ainda na F0 (só instrumentação, nenhuma
> mudança de comportamento entregue). A pasta `modded-realism/` e o grafo dela foram removidos; o mod
> Fontaine vendorizado (`mods/Stance-Overhaul-test-1/`) foi **mantido como referência**.
>
> ⚠️ **Os 2 bugs que motivavam este item continuam ABERTOS** e serão atacados por abordagem própria (waypoint
> por Stance 0 + atenuação de offset por comprimento de arma) no **item [017](../017-transicao-ads-cirurgica/)**.
> Este documento fica como registro histórico da abordagem descartada.

---


**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Em progresso (F0)
**Criado:** 2026-07-17
**Sandbox:** ⚠️ este item trabalha **exclusivamente** em `modded-realism/` (fork experimental). `modded/`
(canônico 2.5.x) fica em regime de **só hotfix** durante o experimento; `original/` intocável.
**Plano aprovado:** `C:\Users\guime\.claude\plans\toasty-watching-wombat.md` (decisões congeladas em 2026-07-17)
**Estudo de referência:** [analise-porte-item-016.md](../../../Stance-Overhaul-test-1/assets/analise-porte-item-016.md)

## Visão geral

Porta a **experiência de movimento** do Fontaine-StanceOverhaul (vendorizado em `mods/Stance-Overhaul-test-1/`,
**com permissão do autor**) para as nossas 4 stances, num fork `modded-realism/` que pode ser promovido a
canônico. Dois bugs motivam:

- **(a)** braços deformados ao mirar em certas combinações (P-11.2, ex.: G36 + High Ready) — escopado à
  **janela de transição/ADS**;
- **(b)** transição Low Ready → mira abrupta: a arma sobe **~5 cm além da mira** e desce rápido (pior em armas
  curtas).

Causa raiz de (b): a transição atual é uma **mola sub-amortecida por design** (ζ≈0,49) e, ao mirar, o alvo troca
na mesma mola com **velocidade acumulada** + kick de ADS + stiffness que escala com o slider enquanto o damping
fica fixo. O Fontaine resolve com (1) **transições por progresso determinístico avaliando curvas** (easing nos
keyframes, overshoot só se desenhado) e (2) **gate de aim-speed** (a mira nativa fica ~travada até a pose de
stance sair — stance e ADS nunca disputam o alvo).

**O que NÃO portamos** (decisões congeladas): a máquina "ADS cancela stance + restaura" (nosso `CurrentStance`
alimenta snap-on-fire/stamina/Fika/mount — cancelar seria reescrita); `Melee`/`Mounting`/`AimPIDHandler`/
`SpringAnimators` (100% comentados no Fontaine); o ecossistema `RealismCommonLib` (adaptar apenas
`Vector3Curve`/`CurveDrawer`, com crédito).

## Comportamento desejado (essência)

1. **Pose = sliders, shaping = curvas**: a pose final de cada stance continua vindo dos sliders F12 (config
   calibrada do servidor, intocada). A transição vira `LerpUnclamped(from, alvo_do_slider, s(t))` com `s(t)`
   curva por classe de transição; `from` é **recapturado da pose visual** ao trocar de alvo em voo. Camada
   aditiva por eixo (delta 0→pico→0) para o caráter autoral na F3.
2. **Gate de aim-speed**: `_aimingSpeed` do PWA multiplicado por curva do progresso da transição ADS; floor
   0.05; toggle F12; ativo só com `_ResetOnADS && stance != Default` e transição ADS em curso. ⚠️ Como o
   progresso é escalado por `ADS Transition Speed`, baixar esse slider também **alarga a janela do gate**
   (mira nativa mais lenta) — efeito esperado, documentar no tooltip.
3. **Ponto de aplicação inalterado**: `WeaponRootAnim.SetPositionAndRotation` pré-IK (validado no item 014).
4. **Rollback embutido**: F12 `Transition Engine = [Spring (legacy) | Curves]` — legacy preserva o code path
   atual bit-idêntico.
5. **Kick** em canal separado com `SpringMath.SpringDamp` (ζ=1).

## Fases e critérios de aceite

> Gate humano **obrigatório** ao fim de cada fase (teste in-game do usuário). `/code-mod` + `/code-review` por fase.

### F0 — Baseline + diagnóstico + setup do fork
- [x] Fork `modded-realism/` criado (cópia limpa @ v2.5.0), versão **3.0.0**, banner `[REALISM FORK]` no Awake,
      `DIVERGENCE.md`, build limpa (0 erros / 0 avisos). *(commit `c0cdece`)*
- [ ] **Instrumentação `Debug Transition Metrics`** (bool, Advanced, seção `Debug (Advanced)`): por transição
      concluída, loga **1 linha**: rota (`Stance2->ADS` etc.), **pico de excursão além do alvo** por eixo de
      posição (com destaque para o eixo vertical local Z) e de rotação, **nº de cruzamentos de sinal** do erro
      após o 1º alcance do alvo, e **tempo de assentamento** (|erro| < ε por N frames). Cada linha inclui a
      **origem** (`local` ou `observed:<id>`) — obrigatório para o GATE de paridade Fika do F4 parear duração
      local × observado por amostra. Custo zero com a flag off; log rate-limited (só ao fim da transição).
- [ ] **GATE (usuário): baseline 2.5.0-equivalente medido** — fork com engine legacy; armas fixas: **MP5** +
      **1 pistola (nomear no teste, ex.: M9A3)**; rotas fixas: **Stance1→ADS, Stance2→ADS, Stance3→ADS e
      Stance0↔Stance2**; ≥5 amostras por rota; ambiente controlado (hideout/raid vazia). Os "~5 cm" viram número.
- [ ] **GATE (usuário): diagnóstico G36** — matriz {G36, 1 rifle longo, 1 arma curta} × {Stance 1/2/3} ×
      {parado, transição p/ ADS}: a deformação é estática, de transição, ou ambas? (fecha o escopo de P-11.2).

### F1 — TransitionEngine determinístico (mata o bug b)
- [ ] `TransitionEngine.cs`: progresso 0..1 determinístico; `from` recapturado em troca de alvo em voo;
      instância por corpo (local + cada observado Fika); absorve a semântica do `TransitionSpeedTracker`.
- [ ] Consolidação: `ApplyComplexRotationPatch` e `ApplySimpleRotationPatch` consomem o MESMO engine (o motor
      de mola hoje é 100% duplicado); a mola legacy extraída para lugar único.
- [ ] Kick reimplementado como perturbação `SpringDamp` ζ=1 somada ao output (preserva `Stance Kick Intensity`
      e `ADS Kick Delay`).
- [ ] F12 `Transition Engine` (enum Spring Legacy / Curves; default **Curves** no fork); com **Legacy**, o
      comportamento é o 2.5.0 (mesmo code path).
- [ ] **Testes de unidade** (C# puro, sem Unity): Evaluate, recaptura em voo, reversão, clamps, curva de gate.
- [ ] Reset de raid via `StanceManager.ResetState`; zero alloc/frame no caminho quente.
- [ ] **GATE (usuário)**: transições 0↔1↔2↔3 suaves; métrica da F0 mostra overshoot ≈ 0 com curva default;
      toggle legacy reproduz o baseline.

### F2 — Gate de aim-speed (bug a, janela de transição)
- [ ] `Patches/AimSpeedCapturePatch.cs` (Postfix `PWA.UpdateWeaponVariables`, SÓ captura o original — padrão
      Fontaine); escrita `original × gateCurve(progresso)` no postfix existente. ⚠️ tech-spec confirma no
      Assembly real (via `ilspycmd`, PWA não está no decompilado) que `_aimingSpeed` é recomputado a cada
      `UpdateWeaponVariables`.
- [ ] Curvas ads-in/ads-out; F12 `ADS Aim Gate` (bool, default true) + `ADS Aim Gate Release Point` (0.5–0.95,
      default 0.75).
- [ ] **GATE (usuário)**, contra o baseline F0: pico vertical além da pose final de ADS **≤ 0,5 cm**;
      ≤ 1 cruzamento de sinal; assentamento ≤ +30% do baseline; G36+High Ready→ADS sem hiperextensão visível
      (vídeo lado a lado); scopes/canted/binóculo OK; snap-on-fire durante ADS-in OK.

### F3 — Curvas autorais por stance (caráter)
- [ ] `TransitionCurveLibrary.cs` (adaptação **creditada** de `Vector3Curve`/`CurveDrawer` do Fontaine) +
      deltas aditivos por classe de transição partindo do `curves.json` dele, adaptados ao mapeamento:
      S1←`high_ready_*` · S2←`low_ready_*` · S3←`short_*` · ads←`active_*`/aim-speed curves. Pose em t=1
      idêntica ao slider (delta termina em 0 por construção).
- [ ] F12: `Transition Style Stance 1|2|3` (presets) + `Transition Style Intensity` (0–200%); override opcional
      por JSON para tuning sem recompilar.
- [ ] **GATE (usuário)**: caráter perceptível (ex.: "mergulho" do low-ready) **e** métrica da F0 confirmando
      pose final idêntica ao slider (delta residual < 0,1 cm / 0,1°) apesar da curva autoral.

### F4 — Calibração, paridade Fika e promoção
- [ ] Checklist de regressão: snap-on-fire, mount passivo/bloqueio ativo, hold-breath (dreno mora DENTRO do
      postfix de pose), kick, TacSprint, prone, reset de raid, contrato `ExternalHandsDrainMult` **intocado**,
      speed caps, P-11.1 não piorou.
- [ ] Paridade 1ª/3ª pessoa no Fika com **os 2 clientes no fork** (duração da transição do observado ±10% da
      local); pacote de rede inalterado.
- [ ] Zero `[STANCE-CLAMP]` em 30 min com engine Curves.
- [ ] **GATE (usuário): decisão GO/NO-GO.** Regra: item do checklist de regressão falho classificado como
      **bloqueante** (perda de funcionalidade existente) → NO-GO automático; falha **cosmética/tuning** vira
      pendência pós-GO. GO → fork vira `modded/` (3.x), antigo vira `modded-bak-2.5/`, cfg recalibrado e
      distribuído com a DLL. NO-GO → aprendizados na memória; achados aproveitáveis viram itens; pasta
      arquivada/removida (decisão na hora).

## Corner cases

- [ ] **Troca de alvo em voo** (stance→stance no meio de transição; mirar no meio de troca de stance; snap-on-fire
      no meio de ADS-in): sempre recaptura `from` da pose visual — sem salto, sem herança de velocidade.
- [ ] **Gate nunca "mata" a mira**: floor 0.05; se o progresso travar (estado inconsistente), a mira destrava
      pelo floor; timeout defensivo na transição (força t=1 após X s).
- [ ] **Prone força Stance 0** (comportamento do 013): a transição forçada também passa pelo engine.
- [ ] **Mount ativo/arma montada força Stance 0 (MESMO branch do prone — `StanceManager.Update:164-175`):** se
      ocorrer com o gate de aim-speed ativo (ADS em curso, progresso < release point), o gate **não** pode ser
      liberado como corte no mesmo frame — herda a regra de un-gate suave abaixo.
- [ ] **Un-gate por interrupção** (mount, Action Stance de reload/inspeção — que hoje pode disparar em pleno ADS —,
      sprint, prone, qualquer coisa que force `stance == Default` durante ADS-in gateado): o multiplicador decai
      pela própria `gateCurve` (ou release de N ms), nunca salta para o `_aimingSpeed` nativo instantaneamente.
- [ ] **Fika com versões mistas** (peer no 2.5.x observando quem usa o fork e vice-versa): pacote idêntico —
      cada cliente renderiza com o próprio engine; sem erro, apenas sensação diferente por cliente.
- [ ] **Config**: chaves novas apenas (zero renames, nenhum `=` no nome — regra BepInEx); `.cfg` distribuído
      binda as novas com default; fork **nunca** vai ao config-server antes do GO. ⚠️ **Exceção declarada:**
      `ADS/Stance Transition Speed` mantêm o NOME mas mudam de SIGNIFICADO sob `Curves` (deixam de multiplicar
      stiffness de mola e passam a escalar a velocidade do progresso) — um `.cfg` calibrado no legacy não soa
      idêntico em Curves com o mesmo número; a recalibração da F4 cobre isso.
- [ ] **Deploy de teste**: mesma AssemblyName → a DLL do fork **substitui** a canônica em
      `plugins/RealisticMobility/`. Testes com **Dev Mod ON** no launcher (sync reverte DLL e cfg). Artefatos
      em `builds/` sempre `-realism`.

## Fora de escopo

- [ ] **P-11.1** (velocidade presa devagar — speed limit stale): item próprio; F4 apenas confere não-regressão.
- [ ] **Deformação estática da G36** (se o diagnóstico F0 confirmar): item novo (ex.: atenuação por comprimento
      de arma).
- [ ] Mudanças em StaminaController/`ExternalHandsDrainMult`, TacSprint, mount passivo, speed caps, pacote Fika.
- [ ] Novas stances (Patrol/Left Shoulder do Fontaine) — só as nossas 4.

## Referências

- [Estudo do porte (Fontaine)](../../../Stance-Overhaul-test-1/assets/analise-porte-item-016.md) — arquitetura,
  refs `arquivo:linha`, o que está vivo/morto, mapeamento de curvas
- [014 — Sync stances Fika](../014-sync-stances-fika/014-sync-stances-fika-01-spec.md) (ponto de aplicação
  pré-IK validado, que este item preserva)
- [009 — Animação orgânica](../009-animacao-transicao-stances/009-animacao-transicao-stances-01-spec.md)
  (precedente interno de curva aditiva 0→pico→0)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-17 | Spec criada a partir do plano aprovado (g-autodev). F0 estrutural já entregue (commit `c0cdece`). |
| 2026-07-17 | Review adversarial 01 (sub-agent, contexto limpo): 9 achados (2 🔴, 4 🟡, 3 🟢) — todos aplicados. Destaques: un-gate suave por interrupção (mount/ActionStance/sprint força Stance 0 no MESMO branch do prone), origem por corpo nas métricas (pré-requisito da paridade Fika F4), exceção semântica declarada dos sliders de velocidade sob Curves. |
