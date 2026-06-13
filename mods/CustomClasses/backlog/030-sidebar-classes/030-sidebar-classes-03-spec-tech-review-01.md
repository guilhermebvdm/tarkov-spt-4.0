# 030 — Sidebar persistente de classes — Auto-review da spec técnica (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./030-sidebar-classes-02-spec-tech.md) · [01-spec](./030-sidebar-classes-01-spec.md)

Revisão crítica do 02. 🔴 = bloqueador (resolvido editando o 02 antes de codar); 🟡 = risco/decisão registrada; 🟢 = confirmação. Cada 🔴 abaixo já foi endereçado no 02.

## 🔴 Bloqueadores (resolvidos)

### 🔴-1 — Guard exige tocar `ClassEdit.razor`, que está FORA do território do 030 (RESOLVIDO)

O 02 §5 reconhece que o sinal de "sujo" e os handlers Save/Discard só existem no `ClassEdit.razor` (território do 025/026). O brief do 030 lista como território só `NavMenu.razor` + `BaseLayout.razor`. **Conflito real**: sem tocar `ClassEdit`, o `EditGuardState.IsDirty` nunca é setado e o guard nunca dispara — o critério de aceite "navegar com mudanças pendentes sempre pergunta" não é atingível só com os dois arquivos do território.

**Resolução aplicada (02 §5, item 3):** o toque em `ClassEdit.razor` foi **explicitado como dependência cross-item documentada** (mínimo: 1 `[CascadingParameter]`, set do flag nos `bind:after` já existentes, e wire-up de `SaveAsync`/`Discard`/`Reset`). É o análogo direto da permissão do brief para o 030 adicionar `ListClassSummaries()` ao `ClassEditorService` ("leitura da cache, território compartilhado aceitável: documente"). Sem essa permissão estendida, a parte do guard fica **bloqueada por dependência** e a implementação deve parar e sinalizar — registrado também no resumo final. **Premissa autônoma:** trato o toque mínimo no `ClassEdit` como território compartilhado aceitável (igual ao `ClassEditorService`), porque o guard é critério CRÍTICO do kickoff e é impossível satisfazê-lo sem um sinal de dirty originado no form. Marcado no 02 como "MODIFICAR (cross-item, §5)" na tabela de arquivos.

### 🔴-2 — Custo de skills por classe na sidebar pode reintroduzir trabalho pesado por navegação (RESOLVIDO)

O 01-spec e o kickoff proíbem `dry-run` por render, mas a sidebar exibe "custo de skills compacto" + dot OverBudget, ambos derivados de `CostService.ComputeSkillCost`. Se isso rodasse por item num loop de render do MudBlazor (que re-renderiza com frequência), seria N cálculos por frame.

**Resolução aplicada (02 §1a/§2/§4):** custo/status são computados UMA vez por navegação em `LoadRows()` (molde de `Classes.razor:121-146`, que já faz exatamente `ComputeSkillCost` por linha em `OnInitialized`), guardados no `SidebarRow` imutável, e o render só lê o valor pronto. `ComputeSkillCost` não dispara `dry-run` (não chama `ValidateAndBuild`/`InventoryBuilder` — confirmado em `CostService.cs:106-199`, só itera `def.Skills`). Recompute só em location-change que seja Save/Delete; navegação pura reusa o cache do 037. **Confirma o requisito "sem lag/sem dry-run por render".**

### 🔴-3 — Risco de falso-dirty: o form re-renderiza e marcaria "sujo" sem mudança real (RESOLVIDO/DELEGADO COM CONTRATO)

Se `Guard.IsDirty = true` for setado em todo `bind:after`/`OnAfterRender`, abrir a edição e só navegar (sem editar) poderia abrir o diálogo indevidamente — degradando o guard a ruído e treinando o usuário a clicar "Descartar" no automático (anula a proteção).

**Resolução aplicada (02 §5, item 3):** o **contrato** que o 030 impõe foi tornado explícito — `Guard.IsDirty == true` **sse** há mudança não persistida. A forma de detectar (flag nos eventos de mutação vs. comparação de snapshot serializado `ToDefinition()` contra o carregado) é decisão de implementação do 025/026, mas o 02 registra a recomendação de **snapshot-compare** para evitar falso-positivo, e o critério de aceite do 01-spec ("navegar a partir de uma edição sem mudanças NÃO mostra diálogo") trava o comportamento. Bloqueador rebaixado a contrato verificável.

### 🔴-4 — Highlight de "ativa" e troca de vista dependem de parsing de URL não validado (RESOLVIDO)

A primeira redação raciocinava sobre a rota sem fixar o parser. Rotas com `{FileName}` extensionless (`ClassDetail.razor:1`, `ClassEdit.razor:1`) e query/fragment poderiam quebrar um `Contains("/edit")` ingênuo (ex.: classe chamada literalmente algo com "edit", ou trailing slash).

**Resolução aplicada (02 §4):** parser explícito via `Nav.ToBaseRelativePath(Nav.Uri)`, split de `?`/`#`, `Trim('/')`, split por `/` e match posicional estrito (`parts[0]=="customclasses" && parts[1]=="classes"`, `parts[3]=="edit"`). `Uri.UnescapeDataString` no segmento. Espelha a convenção de navegação de `Classes.razor:157-158`. Edit→edit só quando `HasDefinition` (senão detail) — fallback do 01-spec coberto no próprio `NavigateToClass`.

## 🟡 Riscos / decisões registradas

- **🟡-A — Vazamento do handler `LocationChanged`:** `NavMenu` passa a `@implements IDisposable` e remove o handler no `Dispose` (registrado no 02 §4). Sem isso, cada navegação acumularia um handler no `NavigationManager` do circuito. Mesmo cuidado já existe em `ClassEdit.razor:896` (Dispose do CTS).
- **🟡-B — `NavigationLock` cobre só o que o framework intercepta:** navegação interna (links/`NavigateTo`) é coberta; refresh/fechar aba só dispara o prompt **nativo** do browser (texto genérico), não o diálogo Save/Discard/Cancel. Já declarado fora de escopo no 01-spec e reafirmado no 02 §5 (premissa `NavigationLock`).
- **🟡-C — Custo recomputado após Save/Delete:** quando a navegação resulta de um Save/Delete (a cache do 037 invalida 1 entrada), o `NavMenu` precisa recomputar a linha afetada. Como `LoadRows()` reprojeta de `ListClassSummaries()` (que lê a cache já invalidada/revalidada pelo Save em `ClassEditorService.cs:333`), o número novo aparece naturalmente na próxima montagem. Sem trabalho extra — registrado no 02 §4.
- **🟡-D — `CascadingValue IsFixed=true` com objeto mutável:** `EditGuardState` é a MESMA instância durante todo o circuito (o layout não recria); `IsFixed=true` é correto e barato (não força re-render dos consumidores quando `IsDirty` muda — o que é desejável, ninguém precisa re-renderizar por causa do flag). O guard lê `_guard.IsDirty` no momento do `OnBeforeInternalNavigation`, então não depende de propagação reativa. Confirmado seguro.
- **🟡-E — Ordem de status quando inválida E desabilitada:** uma classe pode estar `enabled:false` E ter Error. A árvore (02 §3) prioriza **Invalid** sobre Disabled — paridade exata com `ClassDetail.razor:534-541` / `Classes.razor:219-228` (o teste de erro vem primeiro). Mantido de propósito para não divergir da lista.

## 🟢 Confirmações

- 🟢 Gancho do 037 existe e é o previsto: `GetCachedEntries()` (`ClassEditorService.cs:182`), com XML-doc reservando `ListClassSummaries()` para o 030 e prescrevendo a projeção. O 02 segue a prescrição.
- 🟢 `ComputeSkillCost` é barato e sem `dry-run` (`CostService.cs:106-199`).
- 🟢 `SkillWeights.BudgetMin/Max` (28/32) é a fonte única de budget, já usada em `Classes.razor:253` e `ClassDetail.razor:177`.
- 🟢 Navegação extensionless + escaping replicam `Classes.razor:157-158` e a decisão do 024 (`024-...-02:17`).
- 🟢 `MudDrawer Variant=Mini`/`OpenMiniOnHover` preservado (`BaseLayout.razor:41-42`); responsividade não regride.
- 🟢 Thread-safety: cache `ConcurrentDictionary` (`ClassEditorService.cs:94`); `EditGuardState` por circuito.

## Estado pós-review

- **Bloqueadores abertos: 0.** 🔴-1 resolvido por premissa autônoma (toque mínimo no `ClassEdit` tratado como território compartilhado documentado — análogo ao `ClassEditorService`); 🔴-2/🔴-3/🔴-4 resolvidos no 02.
- **Atenção do implementador:** 🔴-1 e 🔴-3 cruzam o território do 025/026 (`ClassEdit.razor`). A spec autoriza o toque MÍNIMO e o contrata (`IsDirty` sse mudança não persistida + wire-up Save/Discard). Se a política de território for estrita e proibir qualquer edição no `ClassEdit`, a parte do **guard** fica bloqueada por dependência e deve ser fatiada para um item conjunto com o 025/026 — o resto da sidebar (lista/filtro/status/navegação 1-clique) é entregável só com `NavMenu` + `BaseLayout` + `ListClassSummaries()`.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Auto-review crítico via `/review-technical-spec`; 4 🔴 endereçados, 5 🟡 registrados |
