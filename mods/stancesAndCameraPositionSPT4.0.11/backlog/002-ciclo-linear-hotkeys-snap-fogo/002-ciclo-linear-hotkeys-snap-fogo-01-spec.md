# 002 — Ciclo linear, hotkeys e snap fogo

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-05-09

## Visão geral

Cinco melhorias independentes no sistema de controle de posturas. Primeiro, o ciclo via scroll do mouse ganha um modo linear (não-circular) onde Stance 0 é o centro, scroll-up sobe para Stance 1 (High Ready) e scroll-down desce para Stance 2 (Low Ready) — com Stance 3 (Custom) acessível apenas via tecla dedicada. Segundo, cada stance ganha uma tecla de ativação direta configurável, com Stance 3 pré-configurada como `O`. Terceiro, Stances 1, 2 e 3 ganham toggles independentes de snap automático para Stance 0 ao pressionar o gatilho, evitando que o jogador dispare com a arma apontada para fora do campo de combate. Quarto (renomeação), os nomes das stances foram corrigidos para refletir os eixos reais: Stance 1 = High Ready (Pitch -15, cano sobe), Stance 2 = Low Ready (Pitch +30, cano desce), Stance 3 = Custom (Yaw -30, lateral). Quinto, o jogador inicia automaticamente em Stance 2 - Low Ready ao entrar em qualquer raid.

> **Nota (06-fix-01):** Stance 2 e Stance 3 trocaram de papel após teste in-raid. Originalmente, Stance 2 era "Custom" (Yaw) e Stance 3 era "Low Ready" (Pitch). O swap deixou o eixo Linear (Stance 1 ↔ Stance 0 ↔ Stance 2) entre as duas stances mais relevantes (High Ready e Low Ready), e moveu Custom (off-axis) para Stance 3. Histórico no fim da spec.

## Comportamento atual

### Ciclo de posturas (V e scroll)

O ciclo atual é **circular e sem diferenciação de direção**: tanto a tecla `V` quanto o scroll do mouse percorrem `Default → Stance 1 → Stance 2 → Stance 3 → Default → …` (ou subconjunto, se alguma stance estiver desabilitada). Scroll para cima e para baixo ambos ciciam na mesma direção.

A propriedade `Use Only Stances` (padrão `true`) exclui o Stance 0 - Vanilla do ciclo. Não existe tecla dedicada por stance — apenas a tecla de ciclo (`V`).

### Teclas dedicadas

Nenhuma stance possui tecla de ativação direta. O único controle de stance é a tecla de ciclo genérica e o scroll do mouse.

### Comportamento ao atirar em stance não-padrão

Atirar em qualquer stance mantém a stance ativa. O jogo não faz snap automático para Stance 0 ao pressionar o gatilho.

## Comportamento desejado

### Feature 1 — Consolidar "Enable Stance 0 in Cycle"

Substituir a propriedade `Use Only Stances` (lógica invertida, confusa) por uma propriedade explícita **`Include Stance 0 - Vanilla in Cycle`** (bool, padrão `false`). Quando `false`, Stance 0 não entra no ciclo — comportamento idêntico ao atual `Use Only Stances = true`. Quando `true`, Stance 0 entra no ciclo normalmente.

Esta propriedade afeta a tecla `V` **sempre** e o scroll apenas quando `Mouse Wheel Scroll Mode = Cycle`. Em modo `Linear`, o scroll usa eixo fixo e ignora todos os toggles de ciclo.

A propriedade `Use Only Stances` deve ser **removida** e substituída por esta.

> ⚠️ **Migração de config (Feature 1):** usuários com `Use Only Stances = false` (Stance 0 no ciclo) terão comportamento invertido após a atualização — o novo padrão `false` exclui Stance 0. Documentar no changelog do mod.
>
> ⚠️ **Migração de config (renomeação de seções):** renomear as seções de stance no código (`"Stance 1 - Ready Up"` → `"Stance 1 - High Ready"`, etc.) faz com que o BepInEx não encontre as entradas antigas no arquivo `.cfg` e recrie-as com os valores padrão. Usuários com Pitch/Yaw/Offset customizados nas seções de stance **perderão suas configurações**. A implementação deve incluir no changelog uma instrução manual de migração (copiar os valores do arquivo `.cfg` antigo para as novas chaves de seção).

### Feature 2 — Opção de modo de scroll (`Scroll Mode`)

Substituir o conceito de `Linear Scroll Mode` (bool) por uma propriedade enum **`Mouse Wheel Scroll Mode`** com dois valores: `Cycle` e `Linear`. Visível apenas quando `Enable Mouse Wheel Stance Cycle = true`. Padrão: `Linear`.

Dependendo do valor selecionado, o F12 mostra ou oculta propriedades relacionadas:

| Propriedade | Visível em `Cycle` | Visível em `Linear` |
| --- | --- | --- |
| `Include Stance 0 - Vanilla in Cycle` | ✓ | ✗ |
| `Enable Stance 1 - High Ready in Cycle` | ✓ | ✗ |
| `Enable Stance 2 - Low Ready in Cycle` | ✓ | ✗ |
| `Enable Stance 3 - Custom in Cycle` | ✓ | ✗ |

> ⚠️ **Restrição de implementação:** BepInEx ConfigurationManager suporta `ConfigurationManagerAttributes.Browsable = false` para ocultar entradas, mas a flag precisa ser alterada dinamicamente via `SettingChanged`. Isso requer manter referência mutável ao `ConfigurationManagerAttributes` de cada entry afetada. A mudança de visibilidade deve ocorrer **em tempo real** — enquanto o F12 está aberto, trocar `Mouse Wheel Scroll Mode` deve ocultar/exibir as propriedades dependentes imediatamente, sem necessidade de fechar e reabrir o painel. Isso pode exigir forçar um `SettingChanged` nas entries afetadas (além de mutar `Browsable`) para que o ConfigurationManager redesenhe a lista.

**Modo `Cycle`** — comportamento atual (circular), respeitando os toggles `Enable/Include Stance X in Cycle`.

**Modo `Linear`** — escala fixa, ignorando todos os toggles de ciclo:

```text
↑ Stance 1 - High Ready    (topo — scroll-up para aqui)
  Stance 0 - Vanilla     (centro neutro — sempre presente)
↓ Stance 2 - Low Ready  (fundo — scroll-down para aqui)
```

Regras do modo `Linear`:

- **Scroll-up**: avança um nível acima. No topo (Stance 1), scroll-up não faz nada.
- **Scroll-down**: avança um nível abaixo. No fundo (Stance 2), scroll-down não faz nada.
- Stance 0 é sempre o ponto central — o scroll passa por ela e para nela.
- **Stance 3 - Custom** não faz parte do eixo linear — acessível apenas via tecla dedicada (Feature 3).
- A tecla `V` não é afetada — continua ciclando normalmente (usando os toggles de ciclo, independente deste modo).

### Feature 3 — Teclas dedicadas por stance

Adicionar quatro propriedades de tecla, uma por stance, na seção Settings:

| Propriedade | Padrão | Comportamento |
|---|---|---|
| Stance 0 Hotkey | `None` | Retorna imediatamente ao Stance 0 - Vanilla |
| Stance 1 Hotkey | `None` | Vai diretamente para Stance 1 - High Ready |
| Stance 2 Hotkey | `None` | Vai diretamente para Stance 2 - Low Ready |
| Stance 3 Hotkey | `O`    | Vai diretamente para Stance 3 - Custom |

Comportamento de pressão:

- Pressionar a tecla de uma stance que **não é a ativa** → muda para ela imediatamente.
- Pressionar a tecla de uma stance que **já é a ativa** → retorna para Stance 0 (toggle). **Exceção:** pressionar a Stance 0 Hotkey quando já está em Stance 0 → sem efeito (não há stance anterior implícita).
- Pressionar a tecla enquanto **sprinting** → bloqueado (igual ao comportamento da tecla `V`).
- Pressionar a tecla enquanto **em ADS** → ignorado silenciosamente (não muda stance, não cancela ADS).
- Teclas com valor `None` são ignoradas (sem ação).

✅ `ConfigEntry<KeyCode>` aceita `KeyCode.None` (valor 0) nativamente — o ConfigurationManager renderiza como "None" no dropdown sem necessidade de wrapper.

### Feature 4 — Snap para Stance 0 ao atirar

Adicionar por-stance (Stances 1, 2 e 3) a propriedade **`Snap to Stance 0 on Fire`** (bool):

| Stance | Nome da prop | Padrão |
|---|---|---|
| Stance 1 - High Ready | Stance 1 Snap to Stance 0 on Fire | `true` |
| Stance 2 - Low Ready | Stance 2 Snap to Stance 0 on Fire | `false` |
| Stance 3 - Custom | Stance 3 Snap to Stance 0 on Fire | `true` |

Comportamento quando `true`:

- **Clique único (pressionar e soltar rapidamente)**: nenhum tiro é disparado. A stance muda imediatamente para Stance 0 - Vanilla. O próximo clique dispara normalmente.
- **Segurar o botão de fogo**: a stance muda imediatamente para Stance 0 e o jogo começa a disparar durante a animação de transição (não é aguardada a conclusão visual da mudança — o tiro sai enquanto a arma ainda anima). Comportamento EFT padrão em modo automático/semi; não bloquear o disparo ao segurar.
- O snap não é acionado durante ADS. "ADS ativo" é definido pela mesma condição já usada no restante do mod para detectar mira — a definição exata fica para a technical spec.
- O snap não é acionado quando já está em Stance 0.
- O snap não é acionado ao atirar com arma branca (melee) ou ao lançar granadas — apenas armas de fogo (`FirearmController`).

**Limiar de clique único:** 200 ms (padrão). O timer começa no **button-down** do gatilho. Se o botão é solto antes de expirar o timer (< 200ms), o evento é classificado como "clique único" → snap sem disparo. Se o botão permanece pressionado por ≥ 200ms, é classificado como "segurar" → snap + disparo imediato. Exposto como `Snap Fire Threshold (ms)` no F12 (seção Settings, Advanced, próximo às hotkeys dedicadas). Faixa: 50–500 ms.

**Armas semi-automáticas ao segurar:** snap + 1 tiro natural (comportamento EFT padrão — não bloquear o disparo).

### Feature 5 — Iniciar em Stance 2 - Low Ready ao entrar em raid

Adicionar uma propriedade **`Start In Low Ready On Raid Begin`** (bool, padrão `true`) na seção Settings.

Quando `true`, ao disparar o evento `GameWorldOnGameStartedPatch.PatchPostfix` (início de raid do `MainPlayer`), o `StanceManager` deve aplicar `Stance.Stance2` (Low Ready após 06-fix-01) **sem interpolação** — setar tanto o alvo quanto a posição atual do spring para os valores de Stance2, de forma que o player inicie já posicionado, sem animar a partir de Stance 0. A implementação não deve usar o caminho normal de mudança de stance (que aciona a animação de transição via spring), mas sim um caminho de "set imediato" equivalente ao que o mod já usa para inicialização.

A atribuição deve ser feita **após** verificar que `MainPlayer.ProceduralWeaponAnimation` e seu `HandsContainer` estão não-nulos (a mesma guard que `UpdateCameraOffset()` já aplica). Se não estiverem prontos no momento do postfix, o estado deve ser mantido pendente e aplicado no primeiro frame em que estiverem disponíveis.

Quando `false`, o jogador inicia em `Stance 0 - Vanilla` (comportamento EFT padrão atual).

Esta feature é independente de `Enable Stance 2 in Cycle`: a stance inicial é aplicada mesmo que Stance 2 esteja excluída do ciclo de tecla/scroll.

## Critérios de aceite

### Feature 1

- [ ] A propriedade `Use Only Stances` não existe mais no F12.
- [ ] A propriedade `Include Stance 0 - Vanilla in Cycle` com padrão `false` aparece na seção Settings antes de `Enable Stance 1 in Cycle`.
- [ ] Com `Include Stance 0 - Vanilla in Cycle = false`, pressionar `V` não inclui Stance 0 no ciclo.
- [ ] Com `Include Stance 0 - Vanilla in Cycle = true`, pressionar `V` inclui Stance 0 no ciclo circular.

### Feature 2

- [ ] A propriedade `Mouse Wheel Scroll Mode` (enum `Cycle`/`Linear`) aparece na seção Settings, visível apenas quando `Enable Mouse Wheel Stance Cycle = true`.
- [ ] Com `Mouse Wheel Scroll Mode = Linear` e estando em Stance 0, scroll-up vai para Stance 1 e scroll-down vai para Stance 2.
- [ ] Com `Mouse Wheel Scroll Mode = Linear` e estando em Stance 1, scroll-up não muda a stance.
- [ ] Com `Mouse Wheel Scroll Mode = Linear` e estando em Stance 2, scroll-down não muda a stance.
- [ ] Com `Mouse Wheel Scroll Mode = Linear`, Stance 3 não é alcançável via scroll — requer tecla dedicada.
- [ ] Com `Mouse Wheel Scroll Mode = Cycle`, o scroll comporta-se de forma circular respeitando os toggles `Enable/Include Stance X in Cycle`.
- [ ] A tecla `V` não é afetada pelo modo — continua ciclando normalmente em todos os modos.
- [ ] Trocar `Mouse Wheel Scroll Mode` com o F12 aberto oculta/exibe as propriedades dependentes imediatamente, sem necessidade de fechar e reabrir o painel.
- [ ] Scroll-up partindo de Stance 2 retorna para Stance 0 (e não salta direto para Stance 1).
- [ ] Estando em Stance 3, scroll-up vai para Stance 1 e scroll-down vai para Stance 2.

### Feature 3

- [ ] Quatro propriedades de tecla aparecem na seção Settings com os padrões corretos (`None`/`None`/`None`/`O`).
- [ ] Pressionar a tecla de Stance 3 (`O` por padrão) ativa Stance 3 independentemente da stance atual.
- [ ] Pressionar a tecla de uma stance já ativa retorna para Stance 0.
- [ ] Pressionar a Stance 0 Hotkey quando já está em Stance 0 não faz nada (sem toggle para stance anterior).
- [ ] Pressionar qualquer tecla dedicada durante sprint não muda a stance.
- [ ] Pressionar qualquer tecla dedicada durante ADS não muda a stance nem cancela o ADS.
- [ ] Teclas configuradas como `None` não disparam nenhuma ação.

### Feature 4

- [ ] Em Stance 1 com `Snap to Stance 0 on Fire = true`, um clique único no gatilho não dispara e muda para Stance 0.
- [ ] Em Stance 1 com snap ativo, segurar o gatilho faz snap para Stance 0 e começa a disparar.
- [ ] Em Stance 2 com snap ativo, comportamento idêntico ao de Stance 1.
- [ ] Em Stance 3 com `Snap to Stance 0 on Fire = false` (padrão), atirar não muda stance.
- [ ] Com snap ativo mas em ADS, atirar não faz snap (disparo normal).
- [ ] Em Stance 0, o snap nunca é acionado independentemente da configuração.

### Feature 5

- [ ] Com `Start In Low Ready On Raid Begin = true`, ao entrar em qualquer raid o jogador começa diretamente em Stance 2 - Low Ready (sem animação de entrada).
- [ ] Com `Start In Low Ready On Raid Begin = false`, o jogador começa em Stance 0 - Vanilla (comportamento atual).
- [ ] A stance inicial independe de `Enable Stance 3 in Cycle` — funciona mesmo com Stance 3 fora do ciclo.
- [ ] Iniciar raid em scav (perfil separado) também aplica a stance inicial quando o toggle estiver ativo.

## Corner cases

- [ ] **Scroll rápido no limite**: scroll-down rápido repetido estando já em Stance 2 não gera mudanças de stance nem erros.
- [ ] **Tecla dedicada + snap simultâneos**: jogador pressiona tecla de Stance 1 e imediatamente pressiona fogo — o snap deve ocorrer na ordem correta (muda para Stance 1, depois deteta o fogo e faz snap para Stance 0).
- [ ] **Scroll a partir de Stance 3 em modo Linear**: estando em Stance 3, scroll-up vai diretamente para Stance 1 e scroll-down vai diretamente para Stance 2 (Stance 3 é tratada como fora do eixo — o scroll a direciona ao extremo correspondente).
- [ ] **Snap durante recarga**: pressionar fogo durante animação de recarga em Stance 1/2 — o snap deve ocorrer mesmo que o tiro não seja possível.
- [ ] **Troca de arma em stance não-padrão**: trocar de arma enquanto snap for pendente não deve causar snap residual na nova arma.
- [ ] **Saída de raid com snap pendente**: sair de raid (extração, morte, MIA) enquanto a stance não-padrão está ativa não deve vazar estado para a próxima raid.
- [ ] **Tecla dedicada = mesma tecla que `V`**: se o jogador configurar uma tecla dedicada igual à tecla de ciclo, o comportamento deve ser determinístico (tecla dedicada tem prioridade, ou documentar claramente o conflito).
- [ ] **Bot não afetado**: nenhuma das features afeta bots ou outros jogadores — apenas `MainPlayer`.
- [ ] **Stance 1 e Stance 2 ambas desabilitadas no modo linear**: com ambas fora do ciclo, o scroll não faz nada em nenhuma direção — sem erro, sem loop infinito de busca.
- [ ] **Scroll multi-passo Stance 2 → Stance 1**: scroll-down de Stance 0 → Stance 2; depois scroll-up → Stance 0; scroll-up novamente → Stance 1. O eixo linear deve ser atravessado em ambas as direções.
- [ ] **Snap + melee/granada**: pressionar fogo com arma branca equipada ou granada em mãos em Stance 1/2 não aciona snap (apenas `FirearmController`).
- [ ] **Snap com arma em modo burst (rajada de 3)**: clique único em arma burst em Stance 1/2 com snap ativo — nenhum tiro sai e faz snap para Stance 0, mesmo que a rajada tivesse 3 balas. O limiar de 200 ms se aplica normalmente.
- [ ] **`Include Stance 0 in Cycle` oculto em Linear mode**: ao selecionar `Mouse Wheel Scroll Mode = Linear`, a propriedade `Include Stance 0 - Vanilla in Cycle` desaparece do F12; ao voltar para `Cycle`, reaparece com seu valor preservado.
- [ ] **Snap em hideout com `Debug Apply In Hideout = true`**: o snap deve funcionar no firing range da mesma forma que em raid quando o toggle de debug está ativo.
- [ ] **Snap state no início de nova raid**: ao entrar em uma nova raid, o estado de snap pendente deve estar limpo — sem snap residual de uma sessão anterior.
- [ ] **Stance inicial + snap simultâneos**: ao iniciar em Stance 2 - Low Ready com `Snap to Stance 0 on Fire = false`, o primeiro clique não deve acionar snap — o padrão de Stance 2 (Low Ready) é `false`.
- [ ] **Stance inicial com Stance 2 desabilitada no ciclo**: `Start In Low Ready On Raid Begin = true` com `Enable Stance 2 in Cycle = false` — o jogador inicia em Stance 2, mas `V` e scroll não a incluem no ciclo subsequente.
- [ ] **Duas hotkeys de stance com a mesma tecla**: se o jogador configurar Stance 1 Hotkey = `O` e Stance 3 Hotkey = `O`, a tecla não pode ativar as duas ao mesmo tempo. A prioridade deve ser determinística e documentada (ex.: menor índice de stance tem prioridade, ou ambas são ignoradas com log de warning).
- [ ] **`ProceduralWeaponAnimation` não pronto em `OnGameStarted`**: se `MainPlayer.ProceduralWeaponAnimation?.HandsContainer` for null no momento do postfix, a stance inicial (Feature 5) deve ser aplicada no primeiro frame do `Update` em que o guard passar — sem erros nem estado inconsistente entre esses frames.

## Fora de escopo

- [ ] Modificar o comportamento de ADS durante o snap (o mod não controla ADS).
- [ ] Suporte a controles de gamepad / bindings de input fora do sistema de teclas do BepInEx.
- [ ] Efeito sonoro ou visual específico para o snap de stance (além da transição já existente).
- [ ] Alterar a lógica de tiro em si (recuo, spread, dano) — apenas o snap de stance é modificado.

## Layout do F12 após implementação

Legenda: `[NOVO]` · `[ALTERADO]` · `~~REMOVIDO~~` · `*(Avançado)*` · `†visível condicionalmente`

### Settings

| Ordem | Propriedade | Tipo | Padrão | Nota |
| --- | --- | --- | --- | --- |
| 65 | **`Include Stance 0 - Vanilla in Cycle`** | bool | `false` | **[NOVO]** †oculto quando `Scroll Mode = Linear`; substitui `Use Only Stances` |
| 64 | `Enable Stance 1 - High Ready in Cycle` | bool | `true` | **[ALTERADO]** renomeado de `Enable Stance 1 in Cycle`; †oculto quando `Scroll Mode = Linear` |
| 63 | `Enable Stance 2 - Low Ready in Cycle` | bool | `true` | **[ALTERADO]** renomeado (06-fix-01 swap); †oculto quando `Scroll Mode = Linear` |
| 62 | `Enable Stance 3 - Custom in Cycle` | bool | `true` | **[ALTERADO]** renomeado (06-fix-01 swap); †oculto quando `Scroll Mode = Linear` |
| 61 | `Stance Toggle Hotkey` | KeyCode | `V` | sem alteração |
| 60 | `Enable Mouse Wheel Stance Cycle` | bool | `false` | sem alteração |
| 59 | `Mouse Wheel Modifier Key` | KeyCode | `LeftAlt` | sem alteração (Order verdadeiramente preservado — tech-review-04 PA-04-01) |
| 58 | **`Mouse Wheel Scroll Mode`** | enum (`Cycle`/`Linear`) | `Linear` | **[NOVO]** †visível quando Mouse Wheel = true; ocupa o slot livre por `Use Only Stances` removido |
| ~~58~~ | ~~`Use Only Stances`~~ | — | — | **[REMOVIDO]** substituído por `Include Stance 0` |
| 56 | `Stance Transition Speed` | float | `1` | sem alteração |
| 55 | `ADS Transition Speed` | float | `1` | sem alteração |
| 54 | `Stance Change Sound Volume` | float | `1` | sem alteração |
| 53 | **`Stance 0 - Vanilla Hotkey`** | KeyCode | `None` | **[NOVO]** |
| 52 | **`Stance 1 - High Ready Hotkey`** | KeyCode | `None` | **[NOVO]** |
| 51 | **`Stance 2 - Low Ready Hotkey`** | KeyCode | `None` | **[NOVO]** |
| 50 | **`Stance 3 - Custom Hotkey`** | KeyCode | `O` | **[NOVO]** |
| 49 | **`Snap Fire Threshold`** *(Avançado)* | int (ms) | `200` | **[NOVO]** faixa 50–500 |
| 48 | **`Start In Low Ready On Raid Begin`** | bool | `true` | **[NOVO]** inicia raid em Stance 2 - Low Ready (após 06-fix-01) |

### Stance 1 - High Ready

| Ordem | Propriedade | Tipo | Padrão | Nota |
| --- | --- | --- | --- | --- |
| … | *(propriedades existentes)* | — | — | sem alteração |
| 0 | **`Stance 1 Snap to Stance 0 on Fire`** | bool | `true` | **[NOVO]** ao final da seção |

### Stance 2 - Low Ready

| Ordem | Propriedade | Tipo | Padrão | Nota |
| --- | --- | --- | --- | --- |
| … | *(propriedades existentes — defaults swap por 06-fix-01: Pitch 30°, Yaw 0°, Forward 0.03, Stamina 1.0, Speed 90)* | — | — | **[ALTERADO]** valores swapped com Stance 3 |
| 0 | **`Stance 2 Snap to Stance 0 on Fire`** | bool | `false` | **[NOVO]** (default swapped por 06-fix-01: Low Ready usa pré-mira, sem snap) |

### Stance 3 - Custom

| Ordem | Propriedade | Tipo | Padrão | Nota |
| --- | --- | --- | --- | --- |
| … | *(propriedades existentes — defaults swap por 06-fix-01: Pitch 0°, Yaw -30°, Forward 0, Stamina 2.0, Speed 100)* | — | — | **[ALTERADO]** valores swapped com Stance 2 |
| 0 | **`Stance 3 Snap to Stance 0 on Fire`** | bool | `true` | **[NOVO]** (default swapped por 06-fix-01: Custom snap on) |

### Resumo de delta

| Tipo | Qtd | Itens |
| --- | --- | --- |
| **[NOVO]** | 11 | `Include Stance 0 in Cycle`, `Mouse Wheel Scroll Mode`, `Stance 0–3 Hotkey` (×4), `Snap Fire Threshold`, `Snap to Stance 0 on Fire` (×3), `Start In Low Ready On Raid Begin` |
| **[ALTERADO]** | 3 | `Enable Stance 1/2/3 in Cycle` (renomeados para incluir o nome da stance) |
| **[REMOVIDO]** | 1 | `Use Only Stances` |
| **†Condicional** | 5 | `Include Stance 0 in Cycle`, `Enable Stance 1/2/3 in Cycle`, `Mouse Wheel Scroll Mode` |

## Referências

- [PROPRIEDADES.md](../../PROPRIEDADES.md) — propriedades atuais do F12
- [modded/StanceManager.cs](../../modded/StanceManager.cs) — lógica de ciclo atual (`GetNextStance`, `GetPreviousStance`, `IsStanceEnabled`)
- [modded/Plugin.cs](../../modded/Plugin.cs) — bindings de config (`_UseOnlyStances`, `_EnableMouseWheelCycle`, `_StanceToggleKey`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-09 | Item criado via `/add-backlog-item` |
| 2026-05-09 | Revisão `/review-spec` — 4 gaps corrigidos + 6 corner cases adicionados + 4 marcações `<!-- review -->` para decisão |
| 2026-05-09 | Segunda revisão `/review-spec` — critérios de Feature 2 atualizados para enum `Mouse Wheel Scroll Mode`, corner case de Stance 3 em Linear mode adicionado, 1 AC de Feature 3 adicionado, inconsistência Feature 1 / Linear mode resolvida |
| 2026-05-09 | Terceira revisão `/review-spec` — `†` adicionado ao `Include Stance 0` na tabela F12, resumo de delta corrigido (4→5 condicionais), corner cases de burst fire e visibilidade de `Include Stance 0` adicionados, `<!-- review -->` para comportamento de hotkeys durante ADS |
| 2026-05-09 | Decisão: hotkeys dedicadas durante ADS → ignorar silenciosamente (Opção A). `<!-- review -->` resolvido, AC adicionado. |
| 2026-05-09 | Nomes de stances corrigidos pelos eixos X/Y/Z: Stance 1 = High Ready (Pitch -15°), Stance 2 = Custom (Yaw -30°), Stance 3 = Low Ready (Pitch +30°). Feature 5 adicionada: `Start In Low Ready On Raid Begin`. |
| 2026-05-09 | Revisão `/review-spec` — 8 gaps corrigidos: aviso de breaking change por renomeação de seção, timer F4 definido como button-down, disparo durante animação de snap explicitado, "ADS ativo" delegado à technical spec, mecanismo "set imediato" e guard de `ProceduralWeaponAnimation` especificados na F5, `Enable Stance N in Cycle` marcados como `[ALTERADO]` no delta, 1 `<!-- review -->` para refresh do F12 após `Browsable`, 2 corner cases adicionados (hotkeys duplicadas entre stances, `ProceduralWeaponAnimation` não pronto). |
| 2026-05-09 | Decisão: `Browsable` deve refletir em tempo real (sem reabrir F12). `<!-- review -->` resolvido; restrição de implementação atualizada + AC adicionado. |
| 2026-05-09 | Revisão `/review-spec` — 2 gaps corrigidos: nomes antigos `Enable Stance 1/2/3 in Cycle` atualizados na tabela de visibilidade (F2); contagem `[NOVO]` no delta corrigida de 9 → 11. |
| 2026-05-10 | **06-fix-01**: Stance 2 e Stance 3 trocaram de papel — agora **Stance 2 = Low Ready** (Pitch +30, cano desce), **Stance 3 = Custom** (Yaw -30, lateral). Defaults de axis, stamina, speed e snap swap para refletir o novo significado. F5 passa a iniciar em Stance 2 - Low Ready. Linear scroll (Stance 1 ↔ Stance 0 ↔ Stance 2) continua semanticamente igual — apenas os rótulos das stances 2 e 3 são trocados. Disparado por feedback do usuário pós-teste in-raid (item 002 entregue). Ver [002-…-06-fix-01.md](002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md). |
