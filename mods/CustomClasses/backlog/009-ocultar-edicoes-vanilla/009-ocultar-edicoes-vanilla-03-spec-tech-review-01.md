# 009 — Ocultar edições vanilla no launcher · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [009-ocultar-edicoes-vanilla-02-spec-tech.md](009-ocultar-edicoes-vanilla-02-spec-tech.md)
**Data:** 2026-06-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 | Launcher **v2** (`/launcher/v2/types`) NÃO respeita a blacklist | ✅ Resolvido (v1-only aceito) |
| PA-01-02 | C — Erro de Lógica | ✅ | Keys exatas das edições — verificadas contra `profiles.json` | Resolvido |
| PA-01-03 | B — Edge Case | ✅ | `.jsonc` com comentários — o deserializer do SPT aceita | Resolvido |
| PA-01-04 | A — Gap | 🟢 | `GetAbsolutePathToModFolder` + `IOnLoad` — confirmar assinaturas | Pendente |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-06-07

**Launcher v2 (`/launcher/v2/types`) NÃO respeita a `CreateNewProfileTypesBlacklist`**

**Problema:** a spec técnica assume que adicionar as keys à blacklist oculta as edições "no launcher". Isso vale para o launcher **v1**, mas **não** para o **v2**:
- **v1** — `LauncherController.Connect()` filtra: `GetProfileTemplates().Where(p => !CoreConfig.Features.CreateNewProfileTypesBlacklist.Contains(p.Key))` ([LauncherController.cs:39-42](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L39)). Rota `/launcher/server/connect` ([LauncherStaticRouter.cs:19](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Routers/Static/LauncherStaticRouter.cs#L19)). ✅ oculta.
- **v2** — `LauncherV2Controller.Types()` itera **todas** as `GetProfileTemplates()` e faz `result.TryAdd(templateName, …)` **sem** consultar a blacklist ([LauncherV2Controller.cs:42-53](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherV2Controller.cs#L42)). Rota `/launcher/v2/types` ([LauncherV2StaticRouter.cs:16](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Routers/Static/LauncherV2StaticRouter.cs#L16)). ❌ **não** oculta.

**Por que importa:** se/quando o usuário usar um launcher baseado em v2, a ocultação **falha silenciosamente** (todas as vanilla reaparecem) — sem erro, difícil de diagnosticar. O launcher SPT **atual** usa v1 (`/launcher/server/connect`), então a abordagem da spec **funciona hoje**; o risco é futuro.

**Sugestão:** manter a estratégia da blacklist (resolve o launcher v1 atual, simples e não-destrutivo) e **documentar explicitamente** a limitação do v2 no as-built + README do item. Não tentar remover templates de `GetProfileTemplates` (quebraria o carregamento de perfis já criados — corner case da spec). Se cobrir o v2 virar necessário, abrir follow-up: um patch/override do `LauncherV2Controller.Types()` server-side aplicando o mesmo filtro (DI override ou Harmony no server). Por ora: **aceitar como limitação conhecida** (v1-only) — não bloqueia.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (blacklist v1-only + documentar limitação do v2; follow-up se necessário)
- `[ ]` Caminho alternativo: _________________

**Resolução:** aceito blacklist v1-only (cobre o launcher SPT atual). Limitação do v2 documentada no as-built. Sem patch extra agora; follow-up se o v2 virar default. (decisão do usuário: "pode seguir")

### PA-01-02 · C — Erro de Lógica · ✅ Resolvido em 2026-06-07

**Keys exatas das edições — verificadas contra `profiles.json`**

**Problema/verificação:** a spec depende das keys baterem **exatamente** com as de `GetProfileTemplates`. Conferido no fonte: `templates/profiles.json` tem exatamente `Standard`, `Left Behind`, `Prepare To Escape`, `Edge Of Darkness`, `Unheard`, `Tournament`, `SPT Easy start` (note o `start` minúsculo), além de `SPT Developer` e `SPT Zero to hero`. O default da spec (`hidden-editions.jsonc`) bate 1:1.

**Resolução:** keys do default confirmadas idênticas às de `profiles.json` — sem risco de no-op por digitação. Nenhuma mudança na spec necessária.

**Decisão:**
- `[x]` Aceitar sugestão (keys confirmadas; manter o default como está)

### PA-01-03 · B — Edge Case · ✅ Resolvido em 2026-06-07

**`.jsonc` com comentários — o deserializer do SPT aceita**

**Problema/verificação:** `hidden-editions.jsonc` tem comentários `//`. `ModHelper.GetJsonDataFromFile` ([ModHelper.cs:22](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ModHelper.cs#L22)) delega a `JsonUtil.Deserialize`, cujas options têm `ReadCommentHandling = JsonCommentHandling.Skip` (JsonUtil.cs:20). Os configs `config/classes/*.jsonc` do próprio mod já carregam com comentários in-game (item 002+), confirmando empiricamente.

**Resolução:** comentários no `.jsonc` são ignorados pelo parser — sem ação.

**Decisão:**
- `[x]` Aceitar sugestão (`.jsonc` com comentários é suportado; manter)

### PA-01-04 · A — Gap · 🟢 Menor

**Confirmar `GetAbsolutePathToModFolder` + assinatura do `IOnLoad`/`ModHelper`**

**Problema:** o stub usa `modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly())` + `GetJsonDataFromFile<HiddenEditionsConfig>(configPath, "hidden-editions.jsonc")`. Confirmado que `ModHelper.GetAbsolutePathToModFolder(Assembly)` ([ModHelper.cs:10](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ModHelper.cs#L10)) e `GetJsonDataFromFile<T>(pathToFile, fileName)` ([ModHelper.cs:22](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ModHelper.cs#L22)) existem com essas assinaturas. O mesmo padrão já é usado por `CustomClassesMod` (loader das classes) — então o `IOnLoad`/`TypePriority = OnLoadOrder.PostDBModLoader + 1` está validado no mod.

**Por que importa:** se as assinaturas divergissem, o `/code-mod` quebraria. Estão corretas — risco zerado; ponto registrado para fechar a dúvida.

**Sugestão:** nenhuma mudança. Reusar exatamente o padrão de path/leitura do `CustomClassesMod.OnLoad` (consistência). Fechar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (reusar o padrão do CustomClassesMod; sem mudança na spec)
- `[ ]` Caminho alternativo: _________________

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review 01 criada via `/review-technical-spec` — 0 🔴 · 1 🟡 · 1 🟢 · 2 ✅ (verificações de v1/v2 launcher, keys e jsonc) |
