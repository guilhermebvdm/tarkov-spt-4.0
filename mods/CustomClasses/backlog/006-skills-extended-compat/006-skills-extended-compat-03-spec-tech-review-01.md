# 006 — Compat opcional com Skills-Extended · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [006-skills-extended-compat-02-spec-tech.md](006-skills-extended-compat-02-spec-tech.md)
**Data:** 2026-06-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3 — todos tratados no `/code-mod`.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | ✅ | `ModMetadata` pode ser nulo — `IsPresent` já é null-safe | Resolvido |
| PA-01-02 | B — Edge Case | ✅ | Aviso por classe pode multiplicar logs sem o SE | Resolvido (mantido por-classe) |
| PA-01-03 | A — Gap | ✅ | Onde documentar as 4 skills do SE | Resolvido (`_docs/exampleClass.jsonc`) |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · C — Erro de Lógica · ✅ Resolvido em 2026-06-07

**`ModMetadata` pode ser nulo**

**Verificação:** `LauncherController.GetLoadedServerMods` usa `sptMod.ModMetadata?.Name ?? "UNKNOWN MOD"` — sinal de que `ModMetadata` pode ser nulo p/ algum mod. O `IsPresent` do stub já usa `m.ModMetadata?.ModGuid` (null-safe) + `string.Equals(..., Ordinal)`, então um mod sem metadata não causa NRE.

**Resolução:** stub já correto (null-conditional). Sem mudança.

**Decisão:**
- `[x]` Aceitar sugestão (manter `?.` + `string.Equals` Ordinal)

### PA-01-02 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-06-07

**Aviso por classe pode multiplicar logs quando o SE está ausente**

**Problema:** o aviso roda no loop por classe. Se várias classes usarem skills do SE e o SE não estiver instalado, haverá um warning por (classe × skill-do-SE). Hoje só o Médico usa (FirstAid+FieldMedicine) → 2 warnings; aceitável. Mas se o usuário espalhar skills do SE em muitas classes, o log enche.

**Por que importa:** ruído de log (não funcional).

**Sugestão:** aceitável como está (informa exatamente qual classe/skill). Se incomodar, agregar num único aviso resumido ao fim do `OnLoad` ("N multiplicadores de skills do SE sem efeito — SE não detectado"). Deixar a versão por-classe agora (mais acionável) e revisitar só se virar ruído.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (manter por-classe; agregar só se virar ruído)
- `[ ]` Caminho alternativo: _________________

### PA-01-03 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-06-07

**Onde documentar as 4 skills do SE suportadas**

**Problema:** a spec lista "doc das 4 skills" sem fixar o local. Existe `config/classes/_docs/`.

**Por que importa:** consistência — o usuário precisa achar a info ao configurar uma classe.

**Sugestão:** adicionar a nota em `config/classes/_docs/` (junto da doc de schema das classes) listando as 4 skills do SE (`FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations`) e que dependem do mod `com.cj.SkillsExtended`. Se não houver arquivo de schema lá, criar um curto.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (documentar em `config/classes/_docs/`)
- `[ ]` Caminho alternativo: _________________

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review 01 criada via `/review-technical-spec` — 0 🔴 · 0 🟡 · 2 🟢 · 1 ✅. Sem bloqueadores → liberado p/ `/code-mod`. |
