# 035 — Densidade global + redução de cliques (polish do épico UX) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** fechamento da comparação de UX com o viewer antigo
**Épico:** UX do editor (030–036) · **Wave:** UX-W4 (fechamento) · **Deps:** 030–034, 036

> Brief de kickoff — insumo para `/create-spec 035`. Não é a spec.

## Problema (UX)

Sobras de fricção depois dos itens estruturais: componentes MudBlazor com densidade default (airy), ações que ainda exigem ida à lista, e regressões visuais possíveis após 030–034.

## Escopo

- **Densidade global:** `Dense=true`/`Margin.Dense` nos MudTable/MudSelect/MudTextField/MudTabs de TODAS as páginas (lista, edit, pickers, diálogos); revisar paddings com a classe CSS do 033; meta: lista de classes e abas de edição com ~2× mais linhas por tela.
- **Cliques:**
  - Lista: linha ganha ação rápida "Edit" (ícone) além do detail; ícone da classe na linha (hoje não renderiza); **colunas ordenáveis** (nome / custo de skills / loadout ₽ — MudTable `SortBy`) (review #11).
  - Edit: trocar de classe pelo sidebar **preservando a aba ativa** (ex.: comparando aba Skills entre classes); `Ctrl+S` = Save; banner pós-save vira snackbar (não empurra o layout).
  - Matriz (032): célula → edit da classe direto na aba Skills.
  - Pickers: resultado único → Enter seleciona.
- **Preferências persistidas (`localStorage`, review #11):** drawer colapsado/aberto, última vista usada (detail vs edit), aba ativa do edit, ordenação da lista e toggles da matriz — hoje cada navegação reseta tudo.
- **Passada de regressão visual**: bateria Chrome MCP nas 5 vistas (lista, detalhe, edit, matriz, pickers) + screenshots de evidência; ajustes finos do que quebrou em 030–034.
- Atualizar `docs/class-editor.md` (rotas/fluxos novos: sidebar, matriz, dashboard) + memória.

## Refs

- Bateria de teste de referência: `backlog/029-docs-e-fechamento/epico-editor-04-code-review-02.md` (seção UI) + screenshots de evidência existentes
- Território: toques finos em todas as páginas `Web/` (por isso roda SOLO, sem paralelo)

## DoD (resumo)

- Tarefas-chave medidas em cliques: ver skills de outra classe = 1; comparar todas = 0 (matriz); editar a partir de qualquer vista ≤ 2.
- Bateria Chrome MCP passando com screenshots; docs atualizadas.
