# 012 — Identidade da classe no menu + tela de Skills · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [012-identidade-menu-skills-02-spec-tech.md](012-identidade-menu-skills-02-spec-tech.md)
**Data:** 2026-06-08

> Análise crítica. IDs `PA-01-MM`. Resolver bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4 — todos tratados no `/code-mod` (ver as-built §"PA resolvidos").

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 | `MenuScreen.Show` tem 2 overloads → `GetTargetMethod` ambíguo | ✅ Resolvido |
| PA-01-02 | D — Arquitetura | 🟡 | Identidade gateada por `ShowOnUi` (switch do multiplicador) | ✅ Resolvido |
| PA-01-03 | B — Edge Case | 🟡 | Coroutine pode rodar após o menu fechar (NRE) | ✅ Resolvido |
| PA-01-04 | B — Edge Case | 🟢 | `GameObject.Find("MainMenuPlayerModelView")` por nome | ✅ Resolvido |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟡 Importante

**`MenuScreen.Show` tem 2 overloads → `AccessTools.Method(typeof(MenuScreen), "Show")` ambíguo**

**Problema:** confirmado via ilspycmd que `MenuScreen` tem `Show(GClass3877 controller)` **e** `Show(Profile, MatchmakerPlayerControllerClass, ESessionMode)`. `AccessTools.Method(type, name)` sem tipos pode retornar o overload errado ou nulo → o patch não aplica (selo nunca aparece).

**Por que importa:** a feature do menu silenciosamente não funciona.

**Sugestão:** resolver o método por **contagem de parâmetros** (evita referenciar `MatchmakerPlayerControllerClass`): no `GetTargetMethod`, `AccessTools.GetDeclaredMethods(typeof(MenuScreen)).First(m => m.Name == "Show" && m.GetParameters().Length == 3)`. Aplicar o mesmo cuidado em `SkillsAndMasteringScreen.Show` (filtrar 3 params: Profile/InventoryController/IHealthController) para robustez.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-02 · D — Arquitetura · 🟡 Importante

**Identidade gateada por `ShowOnUi` (switch do multiplicador, item 010)**

**Problema:** os stubs fazem `if (!Plugin.ShowOnUi || !Plugin.ShowClassIdentity) return;`. `ShowOnUi` controla o **destaque do multiplicador** (borda/seta/tooltip — item 010). A identidade da classe (012) é independente: desligar o multiplicador não deveria sumir com o ícone+nome da classe.

**Por que importa:** acoplamento errado entre duas features; o usuário que desligar a UI do multiplicador perde a identidade sem querer.

**Sugestão:** gatear a identidade **apenas** por `Plugin.ShowClassIdentity`. Remover o `ShowOnUi` dos guards do `MenuClassIdentityPatch`/`SkillsScreenIdentityPatch`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-03 · B — Edge Case · 🟡 Importante

**Coroutine pode rodar após o menu fechar (NRE)**

**Problema:** `PlaceCoroutine` espera até ~30 frames pelo `MainMenuPlayerModelView`. Se o jogador sair do menu (entrar em raid/outra tela) antes, `menu`/`parent` podem ter sido destruídos → `menu.transform` ou `BuildOrRefresh(parent, …)` lançam NRE (o try/catch interno cobre o BuildOrRefresh, mas o acesso a `menu.transform` após o loop fica fora).

**Por que importa:** exceção no console + selo não criado; raro mas real (sair rápido do menu).

**Sugestão:** na coroutine, abortar se `menu == null` (UnityEngine null-check) após o loop e antes de usar `menu.transform`; idem checar `parent != null` antes do `BuildOrRefresh`. Envolver o corpo pós-loop de forma defensiva.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-04 · B — Edge Case · 🟢 Menor

**`GameObject.Find("MainMenuPlayerModelView")` depende do nome literal do MO**

**Problema:** o nome do GO é uma constante do Menu-Overhaul (`MainMenuPlayerModelViewName`). Se o MO renomear numa versão futura, a integração cai no canto fixo (degrada — não quebra).

**Sugestão:** aceitar (degradação limpa para canto fixo). Documentar a dependência do nome; se quiser robustez extra, procurar por `BottomField` dentro do MenuScreen como alternativa. Manter simples.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (aceitar com degradação p/ canto fixo)
- `[ ]` Caminho alternativo: _________________

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Review 01 criada via `/review-technical-spec` — 0 🔴 · 3 🟡 · 1 🟢 |
