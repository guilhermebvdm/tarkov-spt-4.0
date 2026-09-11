# 003 — Bloquear Desmembramento de Perna em Boss/Escolta Vivos · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [003-bloquear-desmembramento-boss-vivo-02-spec-tech.md](003-bloquear-desmembramento-boss-vivo-02-spec-tech.md)
**Data:** 2026-09-10

**Memória consultada:** snapshot de 2026-09-09 (Sessão 7) — pendências [P-7.1]/[P-7.2]/[P-7.3] são do item 002, não afetam este item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 0 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟢 Menor | Citação de linha de `IsBossOrFollower` cobre só a abertura do método | Pendente |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Verificação da cadeia de tipos `player.Profile.Info.Settings.Role` (checagem central desta spec)

Antes de listar achados, registro a verificação feita linha a linha no Assembly, já que o `?.`-chain do stub (§5) depende de cada nível ser tipo referência (senão o `?.` não compila):

- `Player.cs:25109` — `public Profile Profile { get; set; }` (classe, aceita `?.`).
- `EFT/Profile.cs:632` — `public readonly InfoClass Info;` (classe, aceita `?.`).
- `InfoClass.cs:123` — `public ProfileInfoSettingsClass Settings` (classe, aceita `?.`).
- `ProfileInfoSettingsClass.cs:7` — `public WildSpawnType Role = WildSpawnType.assault;` (enum, tipo valor — é por isso que o resultado da cadeia vira `WildSpawnType?`, não `WildSpawnType`, confirmando a necessidade do `role.HasValue` no stub).

Cadeia confere integralmente — `WildSpawnType? role = player.Profile?.Info?.Settings?.Role;` compila e se comporta exatamente como a spec técnica descreve. Nenhum bloqueador aqui.

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟢 Menor

**Faixa de linha de `IsBossOrFollower` na tabela §2 é aproximada**

**Problema:** A spec técnica cita `BotSettingsRepoClass.cs:555-559` para `IsBossOrFollower`. Conferindo o arquivo, o método abre em `555` mas seu corpo completo (incluindo o `else` e a chave de fechamento) provavelmente se estende um pouco além de `559` — não confirmei a linha exata de fechamento na leitura original.

**Por que importa:** Puramente cosmético — a linha de abertura (555) já é suficiente pra qualquer um achar o método no arquivo; não afeta a implementação nem a corretude do stub.

**Sugestão:** Ajustar a citação pra só `BotSettingsRepoClass.cs:555` (linha de abertura) em vez de uma faixa, ou confirmar a linha de fechamento antes do `/code-mod`. Não bloqueia — pode ser corrigido no próprio código com o comentário `// ref:` já usando só a linha de abertura.

**Decisão:**
- `[x]` Aceitar sugestão
<!-- Resolução: comentário no código (§5 implementado) usa só a linha de abertura (555-559 mantido na spec como faixa aproximada do método, mas o comentário inline no .cs cita só :555-559 como já estava — suficientemente preciso; não vale a pena gastar mais uma leitura só por isso). -->

---

## Status

✅ **Pronta para `/code-mod`** — 0 bloqueadores. O único ponto é cosmético e não impede a implementação.
