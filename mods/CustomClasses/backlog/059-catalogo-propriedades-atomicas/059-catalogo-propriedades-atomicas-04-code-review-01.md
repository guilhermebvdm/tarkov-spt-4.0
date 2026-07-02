# 059 — Catálogo de propriedades atômicas + fix da aba CLASS · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [059-catalogo-propriedades-atomicas-01-spec.md](059-catalogo-propriedades-atomicas-01-spec.md)
**Spec técnica:** [059-catalogo-propriedades-atomicas-02-spec-tech.md](059-catalogo-propriedades-atomicas-02-spec-tech.md)
**Asbuild:** [059-catalogo-propriedades-atomicas-05-asbuild.md](059-catalogo-propriedades-atomicas-05-asbuild.md)
**Data:** 2026-07-02

> Análise crítica do código do `/code-mod` (Fatias A+B). Compila 0/0. **Nenhum 🔴/🟠** — os achados são
> "verificar in-game" (🟡, o gate humano) e cosméticos (🟢). Item **code-complete**, aguardando validação visual.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 3 · 🟢 Menores: 2 · ✅ Resolvidos: 0 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 Médio | Ícone nativo da aba escondido por heurística "*icon*" — risco de ícone duplo | `[ ]` Pendente (verificar in-game) |
| CR-01-02 | B — Bug latente | 🟡 Médio | Card multi-linha (Bunker, 4 linhas): layout aninhado VLG-em-HLG pode clipar | `[ ]` Pendente (verificar in-game) |
| CR-01-03 | C — Gap vs. spec | 🟡 Médio | Mensagem vanilla vai na coluna esquerda (não "largura total") | `[ ]` Aceito (dívida documentada) |
| CR-01-04 | F — Melhoria | 🟢 Menor | Fonte do overlay da aba (20px) pode não casar com SKILLS/MASTERING | `[ ]` Pendente (verificar in-game) |
| CR-01-05 | E — Legibilidade | 🟢 Menor | Comentários/log ainda citam "053"/"CR-xx" (agora é 059) | `[ ]` Aceito (histórico) |

## Categorias / Impacto

_(idênticas às reviews anteriores)_

---

## Pontos

### CR-01-01 · B — Bug latente · 🟡 Médio

**Ícone nativo da aba escondido por heurística "*icon*" — risco de ícone duplo**

**Local:** [`SkillsClassTabPatch.cs` StyleClassTab](../../modded/Client/Patches/SkillsClassTabPatch.cs)

**Problema:** `StyleClassTab` esconde os Images nativos cujo GameObject contém `"icon"` no nome e sobrepõe um
overlay próprio `[ícone][CLASS]`. Se o ícone nativo da MASTERING **não** casar a heurística, ele fica visível
**junto** com o overlay → ícone duplo.

**Por que importa:** cosmético, mas visível. Mitigação: o baseline (053) já provou que a heurística casa o ícone
(o brasão trocado aparecia), então é provável que funcione; o log `[053-tabicon] images=[...]` confirma os nomes.

**Sugestão:** verificar in-game (screenshot + linha `[053-tabicon]`). Se casar → fechar. Se não → ajustar o
predicado de esconder (ex.: esconder todos os Images das versões exceto `_targetImage`).

**Decisão:**
- `[ ]` Pendente (verificar in-game)
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar: _________________

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**Card multi-linha (Bunker, 4 linhas): layout aninhado pode clipar**

**Local:** [`SkillsClassTabPatch.cs` BuildGroupCard](../../modded/Client/Patches/SkillsClassTabPatch.cs)

**Problema:** o card é HLG `[frame][col VLG]`; a col tem [Nome + N linhas]. A altura do card vem da propagação
preferred-height col(VLG)→card(HLG)→coluna(VLG), aninhada. Para o Bunker (4 linhas) numa coluna estreita (~50%),
a altura precisa acomodar o wrap; se a passada de layout subestimar, a última linha pode clipar.

**Por que importa:** o card mais denso (Bunker) é o mais visível do Tanque. Mitigada por
`LayoutRebuilder.ForceRebuildLayoutImmediate` no `RefreshPanel` (mesmo padrão que funcionou nos cards do 053).

**Sugestão:** verificar in-game o card do Bunker (4 linhas visíveis, sem corte). Se clipar, adicionar
`ContentSizeFitter (PreferredSize)` na col ou no card.

**Decisão:**
- `[ ]` Pendente (verificar in-game)
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar: _________________

---

### CR-01-03 · C — Gap vs. spec · 🟡 Médio

**Mensagem vanilla vai na coluna esquerda (não "largura total")**

**Local:** [`SkillsClassTabPatch.cs` RefreshPanel](../../modded/Client/Patches/SkillsClassTabPatch.cs)

**Problema:** o corner case da spec pede a mensagem "sem perks/drawbacks" em **largura total**; a implementação
a coloca na `PerksCol` (esquerda, ~50%). Divergência consciente (documentada no asbuild).

**Por que importa:** só afeta **classe vanilla** (não-mod) — o usuário testa com classes do mod, não reproduz.

**Sugestão:** aceitar como dívida (edge raro). Se incomodar, mover a mensagem pro root do painel com
`LayoutElement.ignoreLayout` cobrindo as 2 colunas.

**Decisão:**
- `[x]` Rejeitar (aceitar como dívida — edge raro, documentado no asbuild)

---

### CR-01-04 · F — Melhoria · 🟢 Menor

**Fonte do overlay da aba (20px) pode não casar com SKILLS/MASTERING**

**Local:** [`SkillsClassTabPatch.cs` BuildTabOverlay](../../modded/Client/Patches/SkillsClassTabPatch.cs)

**Problema:** o overlay usa `fontSize = 20f` fixo; o texto nativo das outras abas pode ter outro tamanho →
"CLASS" ligeiramente maior/menor que "SKILLS".

**Sugestão:** verificar in-game; se destoar, ler o `fontSize` de um TMP nativo da barra e reusar.

**Decisão:**
- `[ ]` Pendente (verificar in-game)

---

### CR-01-05 · E — Legibilidade · 🟢 Menor

**Comentários/log ainda citam "053"/"CR-xx"**

**Local:** vários (`SkillsClassTabPatch.cs`, log tag `[053-tabicon]`/`[053-tabs]`)

**Problema:** parte dos comentários e o tag de log ainda referenciam o item 053 e CR-xx do 053, agora que o
código é do 059.

**Por que importa:** só clareza histórica; nada funcional. Manter os tags de log ajuda a rastrear a continuidade.

**Sugestão:** aceitar como histórico (a rastreabilidade 053→059 é útil).

**Decisão:**
- `[x]` Rejeitar (aceitar — rastreabilidade histórica útil)

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-02 | Code review 01 criada via `/code-review` — 0 🔴/🟠; 3 🟡 (verificar in-game) + 2 🟢 (aceitos) |
