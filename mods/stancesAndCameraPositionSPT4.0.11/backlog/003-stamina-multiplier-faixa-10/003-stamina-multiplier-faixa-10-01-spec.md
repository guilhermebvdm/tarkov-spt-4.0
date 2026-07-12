# 003 — Stamina Multiplier: faixa até 10

> **Status de validação:** 🟢 **Entregue e validado in-game** (2026-07-11).
> A validação foi **funcional, por feature** — não critério a critério. Os checkboxes deste documento
> são **checklist de referência**, não registro de execução: o fato de estarem desmarcados **não** significa
> que o item não foi testado. A evidência do teste vive em [`memory/sessions.md`](../../memory/sessions.md).<br>
> ⚠️ Essa validação rodou sobre a build **anterior** à reorganização do F12. A revalidação sobre a **v2.0.0**
> é a pendência **P-7.1** (ver a memória).

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Entregue
**Criado:** 2026-05-10
**Prioridade:** Executar **antes** do item 002.

> ⚠️ **Exceção documentada** — este item pulou as etapas `02-spec-tech` / `03-spec-tech-review-NN` / `04-code-review-NN` por trivialidade (1 caractere de código: `3f` → `10f` em [Plugin.cs:867](../../modded/Plugin.cs)). **Não é padrão**. Futuros itens, mesmo simples, devem seguir o fluxo completo do `repo-workflow-best-practices` para preservar rastreabilidade. Este item existe como exceção histórica, não como precedente.

## Visão geral

Ampliar o teto da propriedade `Stance X Stamina Multiplier` (X = 0, 1, 2, 3) de `3.0` para `10.0` em todas as 4 stances. Mudança trivial num único `AcceptableValueRange` compartilhado pelo helper que constrói a config de cada stance.

## Comportamento atual

- Em [modded/Plugin.cs:867](../../modded/Plugin.cs) (helper `BuildStanceConfig` chamado uma vez por stance), as 4 entradas `Stance N Stamina Multiplier` usam `new AcceptableValueRange<float>(0f, 3f)`.
- F12 limita o slider a `3.0`. Valores fora do range são clamped pelo BepInEx no carregamento do `.cfg`.
- Defaults atuais: Stance 0 = `0.5`, Stance 1 = `1.5`, Stance 2 = `2.0`, Stance 3 = `1.0` ([PROPRIEDADES.md](../../PROPRIEDADES.md) linhas 82, 98, 114, 130).

## Comportamento desejado

- O mesmo `AcceptableValueRange` passa a ser `(0f, 10f)`.
- Slider F12 das 4 props vai até `10.0`.
- Defaults inalterados.
- Tooltip inalterado — os exemplos `0.5` e `2.0` continuam válidos como referência dentro do novo range.

## Critérios de aceite

- [ ] Slider F12 das 4 props (`Stance 0/1/2/3 Stamina Multiplier`) aceita valores até `10.0`.
- [ ] `.cfg` de usuários existentes (valores no range 0–3) carrega sem erro nem clamp.
- [ ] Tooltip não muda — texto e exemplos `0.5` / `2.0` permanecem.
- [ ] [PROPRIEDADES.md](../../PROPRIEDADES.md) atualizado: 4 linhas com coluna **Faixa** `0.0 a 10.0`.

## Corner cases

- [ ] **Valor manual fora de faixa no `.cfg`**: usuário coloca `15.0` → BepInEx clampa para `10.0` automaticamente (comportamento padrão de `AcceptableValueRange<float>`). Sem trabalho extra.
- [ ] **Multiplier extremo em raid**: setar `Stance 1 Stamina Multiplier = 10.0` resulta em recovery muito acelerado fora de ADS. O patch [StanceStaminaRecoveryPatch](../../modded/Patches/StanceStaminaRecoveryPatch.cs) já consome o valor diretamente — nenhum ajuste de lógica necessário; valor extremo é responsabilidade do usuário.

## Fora de escopo

- [ ] Alterar defaults das 4 stances.
- [ ] Modificar a lógica do `StanceStaminaRecoveryPatch`.
- [ ] Adicionar avisos de "valor extremo" no tooltip ou no log.
- [ ] Migração de configs antigas (não há quebra — range só amplia).

## Referências

- [modded/Plugin.cs:867](../../modded/Plugin.cs) — `AcceptableValueRange<float>(0f, 3f)` no helper `BuildStanceConfig`.
- [PROPRIEDADES.md](../../PROPRIEDADES.md) — linhas 82, 98, 114, 130 (Faixa por stance).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Item criado direto (sem `/add-backlog-item`) — mudança trivial, prioridade explícita antes do 002. |
| 2026-05-10 | Implementado via `/code-mod` — Plugin.cs:867 (`0f, 3f` → `0f, 10f`) + 4 linhas em PROPRIEDADES.md. Pulei `/create-technical-spec` e `/review-technical-spec` por ser literal numérico em chamada existente. |
