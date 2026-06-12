# 035 — Densidade global + redução de cliques · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [035-densidade-cliques-02-spec-tech.md](035-densidade-cliques-02-spec-tech.md)
**Data:** 2026-06-12

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-R1-MM`. Execução autônoma (usuário ausente, não aprovável): cada 🔴 bloqueador foi **resolvido in-place na spec técnica** e marcado como resolvido aqui. Decisões de design genuinamente ambíguas ficam abertas como 🟡/🟢.

## Resumo

> 🔴 Bloqueadores: 0 (3 encontrados, 3 resolvidos) · 🟡 Importantes: 3 · 🟢 Menores: 3 · ✅ Resolvidos: 3 · Total: 9

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-R1-01 | C — Erro de Lógica | 🔴→✅ | `MudDrawer` do host não expõe `OpenChanged`/`@bind-Open` — persistir o drawer assim não compila/funciona | ✅ Resolvido |
| PA-R1-02 | C — Erro de Lógica | 🔴→✅ | Listener `keydown` global em capture sequestra Ctrl+S em TODA a aplicação (lista/detalhe/matriz) | ✅ Resolvido |
| PA-R1-03 | B — Edge Case | 🔴→✅ | `?tab` em `SupplyParameterFromQuery` persiste na URL e "gruda" a aba após cliques manuais | ✅ Resolvido |
| PA-R1-04 | A — Gap | 🟡 | Restauração da ordenação do `MudTable` sem mecanismo concreto (API não fixada) | 🟡 Aberto |
| PA-R1-05 | A — Gap | 🟡 | Densidade é "incremental" mas a tabela §4 lista arquivos que já estão densos (ruído de escopo) | 🟡 Aberto (mitigado) |
| PA-R1-06 | B — Edge Case | 🟡 | `OnSaveShortcut` roda fora do contexto de render → `StateHasChanged`/snackbar podem não refletir | 🟡 Aberto |
| PA-R1-07 | C — Erro de Lógica | 🟢 | Ordenação por `Loadout` usa `SkillCost is null` como proxy de "sem definição" | 🟢 Aberto |
| PA-R1-08 | A — Gap | 🟢 | `lastView` (`cc.ui.lastView`) listado na funcional mas sem consumidor na técnica | 🟢 Aberto |
| PA-R1-09 | B — Edge Case | 🟢 | Flash default→persistido visível a cada navegação (reconciliação pós-circuito) | 🟢 Aberto |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-R1-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-06-12

**Persistir o drawer via `@bind-Open` + `OpenChanged` pode não compilar contra o `MudDrawer` do host**

**Problema:** o stub §5(a) propõe `@bind-Open="_drawerOpen"` e um comentário "`bind:Open change → SetAsync`". `@bind-Open` gera o par `Open`/`OpenChanged`, mas o `MudDrawer` atual do `BaseLayout` (`:59-67`) usa `Open="true"` **literal** com `Variant.Mini` + `OpenMiniOnHover="true"`. No modo Mini, o drawer **não fica fechado** — ele encolhe e re-expande no hover; `Open` controla expandido vs. mini, mas o `OpenMiniOnHover` sobrescreve visualmente. Bindar `Open` e gravar no change pode (a) entrar em loop de StateHasChanged com o hover, (b) persistir um estado que o hover imediatamente contradiz. Pior: se a versão de MudBlazor não emite `OpenChanged` no toggle por hover, a gravação nunca dispara — preferência "drawer colapsado" nunca persiste.

**Por que importa:** é um dos 6 estados que a spec funcional §d promete persistir; do jeito escrito, ou não persiste (hover domina) ou causa render loop.

**Sugestão:** **não** persistir o `Open` do drawer Mini. Persistir, em vez disso, a intenção do usuário de **pin/unpin** (Mini vs. permanente) OU simplesmente remover o drawer da lista de preferências persistidas v1 (o Mini-on-hover já é o comportamento desejado e não tem "estado" estável a salvar). Decisão autônoma: **persistir um toggle de pin (`cc.ui.drawerPinned`) que alterna `Variant` Mini↔Persistent**, com default Mini (= hoje). Isso é um estado estável, não conflita com o hover.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (resolvido autonomamente na spec técnica)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §1/§4/§5(a) e checklist §8 ajustados — a preferência do drawer passa a ser um **pin** (`cc.ui.drawerPinned`: `Variant.Mini` default ↔ `Variant.Persistent` quando pinado), estado estável que não briga com `OpenMiniOnHover`. A chave `cc.ui.drawerOpen` vira `cc.ui.drawerPinned` com semântica de pin. O bind problemático em `Open` foi removido; o `Variant` é computado de `_drawerPinned`.

### PA-R1-02 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-06-12

**Listener `keydown` global em capture sequestra Ctrl+S em todas as páginas, não só no edit**

**Problema:** §5(d) registra `window.addEventListener('keydown', saveHandler, true)` (capture) e `e.preventDefault()` para qualquer Ctrl/Cmd+S. O `registerSaveShortcut` só é chamado no `OnAfterRenderAsync` do `ClassEdit`, então o handler **deveria** existir só na página de edit. Mas Blazor Server mantém um **único documento** (SPA): se o `Dispose` do `ClassEdit` falhar em chamar `unregisterSaveShortcut` (ou se a navegação para a lista não dispor o componente a tempo), o handler global continua vivo e bloqueia o Ctrl+S nativo na lista/detalhe/matriz. A spec funcional §corner case "Ctrl+S fora da página de edit" exige explicitamente **não** sequestrar globalmente.

**Por que importa:** regressão de UX em todas as outras páginas (o usuário perde o "salvar página" do browser onde não há save) + risco de handler órfão se o dispose vazar (relacionado ao risco de `DotNetObjectReference` já listado).

**Sugestão:** duas camadas: (1) o `Dispose` do `ClassEdit` **deve** `unregisterSaveShortcut` (já no checklist — reforçar como obrigatório, não opcional); (2) o próprio `saveHandler` checa o pathname antes de agir: `if (!location.pathname.includes('/edit')) return;` — assim, mesmo um handler órfão não sequestra fora do edit. Defesa em profundidade.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (resolvido autonomamente na spec técnica)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5(d) `customclasses.js` atualizado — o `saveHandler` ganha um guard `if (!window.location.pathname.includes('/edit')) return;` no topo (defesa em profundidade contra handler órfão), e §7 reforça que `unregisterSaveShortcut` + `_dotNetRef.Dispose()` no `Dispose()` do `ClassEdit` são **obrigatórios** (não opcionais). O checklist §8 já lista o desregistro.

### PA-R1-03 · B — Edge Case · 🔴 Bloqueador · ✅ Resolvido em 2026-06-12

**`?tab` em `SupplyParameterFromQuery` permanece na URL e re-aplica a aba a cada `OnParametersSet`**

**Problema:** §5(c) usa `[SupplyParameterFromQuery(Name="tab")] int? Tab` e, no `OnParametersSet`, `if (Tab is { } t) ActivePanelIndex = ClampTab(t)`. O problema: a query `?tab=1` **fica na URL** após a navegação. `OnParametersSet` roda de novo em qualquer mudança de parâmetro/re-render do mesmo componente; enquanto a URL tiver `?tab=1`, **toda** reavaliação força a aba de volta para 1 — o usuário clica na aba "Equipped", algo dispara `OnParametersSet`, e a aba pula de volta para Skills. A aba "gruda" na query.

**Por que importa:** quebra o próprio critério de aceite de preservar a aba ativa (o usuário não consegue mais trocar de aba de forma estável quando veio da matriz/sidebar). Bug garantido em qualquer re-render com `?tab` presente.

**Sugestão:** aplicar o `Tab` da query **uma única vez** (flag `_tabFromQueryApplied`), como já se faz para o `localStorage` (`_tabReconciled`). Ou melhor: após aplicar, **limpar a query** com `Nav.NavigateTo(uri-sem-tab, replace:true)` para não poluir a URL nem reagir a re-renders. Decisão autônoma: aplicar uma vez via flag (mais simples, sem navegação extra que poderia re-disparar `OnParametersSet`); a query residual é inofensiva porque o flag impede reaplicação.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (resolvido autonomamente na spec técnica)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5(c) atualizado — `Tab` da query é aplicado **uma só vez** via flag `_tabFromQueryApplied` no `OnParametersSet` (espelha o `_tabReconciled` do localStorage). Cliques de aba subsequentes não são sobrescritos por re-renders. Nota adicionada: como `ClassEdit` re-monta a cada troca de `{FileName}` na rota (componente diferente por classe via `@page` param), o flag reinicia naturalmente a cada classe nova — a aba da query vale para a montagem daquela classe, não trava trocas manuais depois.

### PA-R1-04 · A — Gap · 🟡 Importante

**Restauração da ordenação do `MudTable` sem mecanismo concreto**

**Problema:** §5(b) tem um bloco `OnAfterRenderAsync` que lê `cc.ui.listSort`, parseia `label|dir`, mas o corpo é um comentário ("call the table's sort API … see MudBlazor MudTable.SetSortLabel") — não há chamada concreta. A própria spec admite em §7 que "o mecanismo exato … deve ser confirmado contra a versão de MudBlazor do host". Persistir a ordenação (gravar no `OnSort`) é direto; **restaurar** programaticamente depende da API (`SetSortLabel`, `@bind` no `MudTableSortLabel`, ou `InitialDirection` + `SortLabel` default no markup).

**Por que importa:** "ordenação sobrevive a um reload" é critério de aceite. Sem o mecanismo de restore, só metade (persistir) funciona.

**Sugestão:** alternativa robusta independente de API instável — em vez de programaticamente "clicar" no header no restore, definir `InitialDirection` + `SortLabel` no `MudTableSortLabel` da coluna persistida **na primeira render interativa** (após ler a pref), reconstruindo o markup com a coluna/direção certa. Se MudBlazor expuser `MudTable.SetSortLabel`/`OnSortLabelChanged` na versão do host, usá-lo é mais simples. Confirmar a versão no `/code-mod` e escolher; o contrato funcional não muda.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: não é bug de lógica, é uma incógnita de API a resolver no `/code-mod` contra a versão real de MudBlazor. Anotado na spec técnica §5(b)/§7. Persistir já está especificado; restaurar tem 2 caminhos viáveis.

### PA-R1-05 · A — Gap · 🟡 Importante

**A tabela §4 manda "MODIFICAR" arquivos que já estão na densidade-alvo**

**Problema:** a §4 lista `ClassLifecycleCreateDialog.razor`, `ItemPicker.razor` etc. como "MODIFICAR — densidade". Verificação no código: o `ClassLifecycleCreateDialog` **já** usa `Margin="Margin.Dense"` no `MudTextField` (`:21`) e `Dense="true"` nos `MudAlert` (`:27,41`); o `ItemPicker` **já** usa `Margin.Dense`/`Dense` (`:24-31,42`). A própria spec reconhece isso na nota "Confirmar antes de editar" e no PA-035 incremental, mas a **tabela** ainda os marca como MODIFICAR, o que pode levar o `/code-mod` a tocar arquivos que não precisam (ruído de diff, risco de regressão).

**Por que importa:** clareza de escopo + risco de mexer no que está certo. O `/code-mod` deve saber **antes** quais arquivos provavelmente não mudam.

**Sugestão:** reclassificar na §4 os arquivos já-densos como "VERIFICAR (provável no-op)" em vez de "MODIFICAR", deixando "MODIFICAR" só para os que comprovadamente têm componente airy (`ClassEdit` `MudTabs PanelClass`, e os que a leitura confirmar). A passada continua sendo "ler → aplicar só onde falta".

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: refinamento de escopo/documento, não bug. Anotado: a nota incremental da §4 já cobre a intenção; a reclassificação dos rótulos fica para o `/code-mod` decidir por arquivo após leitura. Confirmado que Create dialog e ItemPicker já estão densos (provável no-op).

### PA-R1-06 · B — Edge Case · 🟡 Importante

**`OnSaveShortcut` (`[JSInvokable]`) roda fora do ciclo de render — UI pós-save pode não atualizar**

**Problema:** `OnSaveShortcut` é invocado pelo JS (`dotNetRef.invokeMethodAsync`). Métodos `[JSInvokable]` rodam fora do `SynchronizationContext` de render do circuito; `SaveAsync` interno faz `await Task.Run(...)` e mexe em `_saving`/`_saveDiagnostics`/snackbar. Sem `InvokeAsync(StateHasChanged)`, as mudanças de estado (botão "Saving…", banner pós-save, snackbar) podem não renderizar — ou pior, mutações de estado fora do contexto do circuito podem lançar.

**Por que importa:** o Ctrl+S "funciona" (salva o arquivo) mas a UI não dá feedback visual igual ao clique no botão — inconsistência. O `Snackbar.Add` dentro do `SaveAsync` provavelmente está OK (o provider tem seu próprio contexto), mas o `StateHasChanged` do `_saving` precisa de `InvokeAsync`.

**Sugestão:** o corpo do `OnSaveShortcut` deve envolver em `await InvokeAsync(async () => { if (_saving) return; await SaveAsync(); });` — garante o contexto de render correto para as mutações de `_saving` e o re-render. Remover o `StateHasChanged()` solto.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: hardening de contexto de render. Anotado na spec técnica §5(c) — `OnSaveShortcut` deve usar `InvokeAsync` para o gate `_saving` + `SaveAsync`. Padrão Blazor Server para handlers vindos de interop.

### PA-R1-07 · C — Erro de Lógica · 🟢 Menor

**Ordenação por Loadout usa `SkillCost is null` como proxy de "sem definição"**

**Problema:** o `SortBy` da coluna Loadout (§5b) é `r => r.SkillCost is null ? double.MaxValue : r.LoadoutTotal`. Isso reusa `SkillCost is null` como sinal de "classe sem definição parseável" — que é verdade hoje (`Classes.razor:132-133`: ambos derivam de `def is null`). Funciona, mas acopla a ordenação de Loadout a uma propriedade de outra coluna; se um dia `SkillCost` puder ser null com `LoadoutTotal` válido, a ordenação fica errada.

**Por que importa:** correto hoje, frágil amanhã. Menor.

**Sugestão:** ordenar por `r => r.HasError ? double.MaxValue : r.LoadoutTotal` (ou adicionar um `bool HasDefinition` explícito ao `Row`). Usa o sinal semântico certo ("sem definição/erro") em vez do proxy.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: micro-acoplamento. Anotado na spec técnica §5(b) como nota. `Row` já tem `HasError` (`Classes.razor:108`) — usá-lo é trivial no `/code-mod`.

### PA-R1-08 · A — Gap · 🟢 Menor

**`cc.ui.lastView` listado na funcional sem consumidor definido na técnica**

**Problema:** a spec funcional §d lista "última vista usada (detail vs edit)" com chave `cc.ui.lastView`. A técnica define a const em `UiPrefs` mas **nenhum** stub a grava ou lê — a sidebar (`NavMenu.NavigateToClass`) já deriva a vista atual de `Nav.Uri` (`DetectView`), não de uma preferência. Não está claro o que `lastView` muda: a navegação da sidebar já preserva a vista atual (edit→edit, detail→detail) pela URL corrente, não por uma pref salva.

**Por que importa:** preferência declarada sem efeito = dead config. Ou se define o comportamento (ex.: ao abrir uma classe a partir da lista, ir pra última vista usada) ou se remove da lista.

**Sugestão:** decisão autônoma — **remover `lastView` do escopo v1**. A vista já é preservada pela URL na troca pela sidebar (mecanismo do 030); uma pref de "última vista" só faria sentido para "ao clicar numa classe nova, abrir em edit se a última foi edit", o que conflita com o fluxo lista→detalhe (clique na linha) já estabelecido. Não persistir.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto (decisão de produto leve): anotar na spec técnica §4 que `cc.ui.lastView` sai do v1 (sem consumidor; a vista já vem da URL). Manter a const removida ou comentada para não virar dead code. Sem impacto em critério de aceite (a funcional cita drawer/aba/ordenação/toggles como os obrigatórios; lastView era "opcional").

### PA-R1-09 · B — Edge Case · 🟢 Menor

**Flash default→persistido visível a cada navegação**

**Problema:** PA-035-02/03 aceita que a UI monta com o default e reconcilia com a pref no `OnAfterRenderAsync(firstRender)` (interop é pós-circuito). Como cada navegação interna no Blazor Server re-renderiza o componente de destino, o usuário vê um "flash": ex. a matriz abre com toggles default (showDisabled on) e, um tick depois, aplica o valor salvo (off). Em navegação SPA (sem reload) o componente da matriz pode não remontar — mas em reload de página, sim.

**Por que importa:** cosmético. Single-user local, aceito. Mencionar para não surpreender na bateria visual do orquestrador.

**Sugestão:** aceitar (registrado em PA-035-03). Mitigação opcional futura: persistência server-side via cookie lido no prerender (fora de escopo v1 — a funcional já exclui server-side).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: comportamento aceito por premissa (PA-035-03), não bug. Anotado para a validação visual do orquestrador não reportar como regressão.

## Avaliação dos eixos pedidos

- **Densidade sem quebrar build:** o risco real é reintroduzir `Dense` ilegal em `MudTextField` (`MUD0002`, ref CR-01-02 do 034). A spec já alerta (§4 nota); verificação confirma que Create dialog e ItemPicker **já estão densos** (PA-R1-05) — a passada é genuinamente incremental, concentrada no `ClassEdit` (`MudTabs PanelClass`). Baixo risco.
- **Persistência localStorage:** os 3 🔴 estavam aqui — drawer Mini não tem estado de Open persistível (PA-R1-01, → pin), e a query `?tab` grudava a aba (PA-R1-03, → aplicar uma vez). Resolvidos. A regra "interop só pós-circuito" (PA-035-02) é o invariante que evita o crash de prerender — sólido.
- **Ctrl+S:** o listener global em capture era um sequestro de escopo (PA-R1-02, → guard de pathname + dispose obrigatório). `OnSaveShortcut` precisa de `InvokeAsync` (PA-R1-06). Com isso, o atalho fica contido na página de edit e dá feedback visual correto.
- **Aba preservada na troca de classe:** a decisão de usar a **query como fonte síncrona** (PA-035-04) é correta — o `localStorage` assíncrono perderia a corrida com o `OnParametersSet`. O furo era a query grudar (PA-R1-03, resolvido). A constante de aba duplicada (ClassEdit/SkillsMatrix) é um acoplamento aceito e documentado.
- **Contrato 037→030/032:** intocado — a ordenação é client-side sobre as `Row`s já projetadas, sem re-consultar o cache. Correto, zero regressão de performance.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Review 01 criada via `/review-technical-spec` (autônoma) — 3 🔴 (resolvidos in-place: drawer-pin, guard de pathname no Ctrl+S, aba-query aplicada uma vez) · 3 🟡 · 3 🟢. |
