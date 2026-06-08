# 004 — Outfits por classe · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [004-outfits-01-spec.md](004-outfits-01-spec.md)
**Spec técnica:** [004-outfits-02-spec-tech.md](004-outfits-02-spec-tech.md)
**Review técnica:** [review-01](004-outfits-03-spec-tech-review-01.md)
**Asbuild:** [004-outfits-05-asbuild.md](004-outfits-05-asbuild.md)
**Data:** 2026-06-07

> Código revisado: `ClassDefinition.cs` (Outfit/OutfitSide), `OutfitBuilder.cs` (endurecido p/ aparência direta), `CustomClassesMod.cs` (wiring). Validado in-game: outfit vanilla aplica/veste (CA OK). IDs `CR-01-MM`.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 2 · 🟢 1 · ✅ Aplicados: 2 (CR-01-01/03) · Aceito p/ reavaliar: 1 (CR-01-02) · Total: 3

**Positivo:** compila 0 warn/err; segue o padrão dos outros builders ([Injectable], DatabaseService); skip-com-aviso por peça; valida slot + facção; CA validado in-game (Caçador nasce vestido). Achados são resiliência + uma incerteza da aparência-direta (avaliar no playtest).

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | ID de outfit malformado pode abortar a classe inteira (sem try/catch) | Pendente |
| CR-01-02 | B — Bug latente | 🟡 | Aparência-direta adicionada aos Suits como SUITE — OBTAINED incerto | Pendente |
| CR-01-03 | E — Legibilidade | 🟢 | `item.Properties is null` em tipo não-anulável | Pendente |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria**

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

---

### CR-01-01 · B — Bug latente · 🟡 Médio

**ID de outfit malformado pode abortar o registro da classe inteira**

**Local:** [`mods/CustomClasses/modded/Server/OutfitBuilder.cs`](../../modded/Server/OutfitBuilder.cs) — `ApplyPiece`, `new MongoId(pieceId)`.

**Problema:** `new MongoId(pieceId)` com string inválida (não 24-hex) lança. `OutfitBuilder.Apply` não tem try/catch; é chamado de `RegisterClass`, que roda dentro do try/catch **por arquivo** do `OnLoad`. Logo, um ID de roupa digitado errado num `outfit` derruba o **registro da classe inteira** (skills + itens + hideout + outfit), não só a peça. Inconsistente com o `InventoryBuilder`, que já isola por slot (CR-01-01 do 003).

**Por que importa:** JSON editado à mão (ou IDs de mod) erram fácil; perder a classe toda por um id de roupa é desproporcional.

**Sugestão:** envolver a montagem de **cada peça** em try/catch no `ApplyPiece` (ou o corpo do `Apply`), logando e pulando só a peça. Mesmo padrão do `InventoryBuilder.Apply`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**Item de "aparência direta" adicionado aos Suits como SUITE — OBTAINED pode não funcionar**

**Local:** [`OutfitBuilder.cs`](../../modded/Server/OutfitBuilder.cs) — `ApplyPiece`, `side.Suits.Add(key)`.

**Problema:** para peças vanilla (suite), `key` é o id do **suite** — correto adicionar aos Suits (vira OBTAINED). Para a "aparência direta" do AllTheClothes (`BodyPart=Body`, `Body=null`), `key` é o id da **malha de aparência**, não de um suite. Adicioná-lo aos Suits (que viram `CustomisationUnlocks` Type=SUITE) pode não produzir um OBTAINED válido / pode criar uma entrada estranha na tela de roupas. O **vestir** (Customization.Body=key) é o que está garantido; o **possuir** é incerto.

**Por que importa:** o CA principal (vestido no login) funciona, mas a peça pode aparecer mal listada/sem OBTAINED na UI de roupas.

**Sugestão:** **avaliar no próximo playtest** (o teste do Op. Furtivo com `top_boss_tagilla_nohead` cobre isso). Se a UI ficar estranha: não adicionar aos Suits quando for aparência-direta (só vestir), ou descobrir o id do suite correspondente. Por ora, manter (best-effort) + este registro.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (manter + reavaliar pós-teste)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-01-03 · E — Legibilidade · 🟢 Menor

**`item.Properties is null` em propriedade não-anulável**

**Local:** [`OutfitBuilder.cs`](../../modded/Server/OutfitBuilder.cs) — `if (!db.TryGetValue(key, out var item) || item.Properties is null)`.

**Problema:** `CustomizationItem.Properties` é declarado não-anulável ([CustomizationItem.cs:21](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/CustomizationItem.cs#L21)). O check é defensivo (JSON pode trazer null), mas pode gerar confusão/"always false".

**Por que importa:** baixo — clareza.

**Sugestão:** manter o check (defensivo é OK aqui), mas adicionar um comentário curto `// defensivo: _props pode vir ausente em dados de mod`. Sem mudança funcional.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

## Resolução (2026-06-07)

- **CR-01-01** ✅ Aplicado — try/catch no `new MongoId(pieceId)` em `ApplyPiece`: id malformado pula só a peça, não a classe.
- **CR-01-03** ✅ Aplicado — comentário defensivo no check de `_props` ausente.
- **CR-01-02** 🔄 Aceito (manter best-effort) — ownership de aparência-direta nos Suits; **reavaliar no próximo playtest** (teste do Op. Furtivo com `top_boss_tagilla_nohead`). Se a UI de roupas ficar estranha, deixar de adicionar aos Suits quando for aparência-direta.

Recompilado 0 warn/err (50.2 KB).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Code review 01 criada via `/code-review` |
| 2026-06-07 | CR-01-01 + CR-01-03 aplicados; CR-01-02 aceito p/ reavaliar pós-teste |
