# 004 — Outfits por classe · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [004-outfits-02-spec-tech.md](004-outfits-02-spec-tech.md)
**Data:** 2026-06-07

> Refs ao spt-source conferidas (batem): `ProfileTemplate.cs:31` Suits, `BotBase.cs:307+` Body/Feet/Hands, `DatabaseService.cs:117` GetCustomization, `CustomizationItem.cs` Side/Body/Feet/Hands, `CreateProfileService.cs:58/61/134`. Modelo validado contra o perfil base. IDs `PA-01-MM`.

## Resumo

> 🔴 0 · 🟡 1 · 🟢 2 · ✅ Resolvidos: 3 · Total: 3 (todos aceitos; dobrados no `/code-mod`)

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge | 🟡 | Validar o "tipo" da peça antes de aplicar/adicionar aos Suits | ✅ Resolvido |
| PA-01-02 | C — Lógica | 🟢 | Linhas citadas do `CustomizationItem` são attr vs decl (±1) | ✅ Resolvido |
| PA-01-03 | B — Edge | 🟢 | Peça com `Side` nulo/vazio aplica aos dois lados (lenient) | ✅ Resolvido |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 **Bloqueador** · 🟡 **Importante** · 🟢 **Menor**

---

### PA-01-01 · B — Edge · 🟡 Importante

**Validar o "tipo" da peça antes de aplicar e adicionar aos Suits**

**Problema:** `GetCustomization()` retorna **todas** as customizations (cabeças, vozes, dogtags, roupas). O stub do `OutfitBuilder.ApplyPiece` resolve qualquer id e: para `upper` lê `Body/Hands`, para `lower` lê `Feet`. Se o autor colocar por engano um id de **cabeça** (ou uma peça lower no campo `upper`), o builder não seta nada de aparência mas **ainda adiciona o id aos `Suits`** — e `AddSuitsToProfile` não valida nada (só insere com `Type=SUITE`). Resultado: um "unlock" de roupa bogus / item errado possuído.

**Por que importa:** JSON editado à mão erra id de slot fácil; um id errado vira uma roupa-fantasma desbloqueada, ou um `upper` sem efeito visual sem aviso claro.

**Sugestão:** em `ApplyPiece`, **antes** de aplicar/adicionar, exigir que a peça seja do slot certo: `upper` → `Properties.Body is not null` (camisa/jaqueta); `lower` → `Properties.Feet is not null` (calça). Se não casar, **pular com aviso** ("'{pieceId}' não é uma peça {upper/lower}") e **não** adicionar aos `Suits`. Adicionar o id aos `Suits` só quando algo de aparência foi de fato setado.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-02 · C — Lógica · 🟢 Menor

**Linhas citadas do `CustomizationItem` são do atributo, não da declaração (±1)**

**Problema:** a tabela §2 cita `CustomizationItem.cs:66/60/57/54` para Side/Body/Feet/Hands. No arquivo, esses números ora batem na linha do `[JsonPropertyName(...)]`, ora na `public MongoId? ...` (diferença de 1 linha). Não muda nada de comportamento, mas pode confundir na implementação.

**Por que importa:** baixo — só precisão de navegação.

**Sugestão:** durante o `/code-mod`, confirmar os nomes/`?` exatos lendo `Models/Eft/Common/Tables/CustomizationItem.cs` (campos `Side` `List<string>`, `Body`/`Feet`/`Hands` `MongoId?`, prop `Properties` = `_props`). Sem mudança na spec.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-03 · B — Edge · 🟢 Menor

**Peça com `Side` nulo/vazio aplica aos dois lados (comportamento lenient)**

**Problema:** o stub usa `if (item.Properties.Side is { Count: > 0 } sides && !sides.Contains(sideName))` → se `Side` for nulo/vazio, **não** pula (aplica em qualquer lado). É um comportamento permissivo intencional (peça sem restrição serve aos dois), mas não está documentado.

**Por que importa:** clareza — alguém pode achar que é bug.

**Sugestão:** manter o comportamento (lenient = OK), e adicionar um comentário no código: "Side nulo/vazio = sem restrição de facção (aplica em ambos)".

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

## Resolução (2026-06-07)

Todos aceitos, a aplicar no `/code-mod`:
- **PA-01-01** ✅ — `ApplyPiece` valida o slot: `upper` exige `Body`, `lower` exige `Feet`; senão pula com aviso e **não** adiciona aos `Suits`.
- **PA-01-02** ✅ — confirmar campos exatos do `CustomizationItem` ao codar.
- **PA-01-03** ✅ — comentar o comportamento lenient (Side nulo = sem restrição).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review técnica 01 criada via `/review-technical-spec` |
| 2026-06-07 | 3 aceitos; serão dobrados no `/code-mod` |
