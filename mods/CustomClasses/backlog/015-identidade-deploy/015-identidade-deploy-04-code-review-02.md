# 015 — Identidade da classe no nome do jogador · Code Review 02

**Mod:** CustomClasses
**Asbuild:** [015-identidade-deploy-05-asbuild.md](015-identidade-deploy-05-asbuild.md)
**Data:** 2026-06-09

> Revisão da **direção final** (cor + tooltip + ícone + menu 3 linhas) — pedido do usuário durante o playtest.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 3 · 🟢 Menores: 3

## Pontos

### CR-02-01 · B — Bug latente · 🟠 Forte

**O sprite do ícone vaza para outros jogadores em listas recicladas (Default)**

**Local:** [`ChatSpecialIconPatch.cs`](../../modded/Client/Patches/ChatSpecialIconPatch.cs) (ramo `!isLocalClass`)

**Problema:** no ramo não-local revertemos `____icon.color = white` e `localScale = one`, **mas não o `____icon.sprite`**. Para `EMemberCategory.Default` (jogador normal) o vanilla `ChatSpecialIcon.Show` **retorna cedo e não reseta o sprite**. Então uma célula que foi do jogador local (sprite = ícone da classe) e é reciclada para **outro jogador** mantém o **ícone da classe** (branco). Mesmo mecanismo do gradiente que vazava antes.

**Por que importa:** em coop/Fika com outros jogadores (chat/grupo/lista), outros aparecem com o ícone da sua classe. Baixo impacto jogando sozinho.

**Sugestão:** guardar o sprite original 1x por instância (`ConditionalWeakTable<ChatSpecialIcon, Sprite>`) e restaurar no ramo não-local; ou esconder o ícone (`____icon.enabled = false`) quando não-local e a categoria for Default. 

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Deferir (jogo solo)

### CR-02-02 · C — Gap/layout · 🟡 Médio

**`CC_ClassLine` (linha da classe no menu) sem `LayoutElement`**

**Local:** [`MenuClassIdentityPatch.cs`](../../modded/Client/Patches/MenuClassIdentityPatch.cs) `GetOrCreateClassLine`

**Problema:** a linha é criada num `VerticalLayoutGroup` (BottomField do MO) sem `LayoutElement`/altura explícita → pode ficar com altura 0 ou empurrar o layout. **Validar in-game** (é um dos pontos do playtest).

**Sugestão:** se não aparecer/ficar torto, adicionar `LayoutElement { preferredHeight = fontSize*1.2 }` e/ou `ContentSizeFitter` vertical.

### CR-02-03 · C — Gap/layout · 🟡 Médio

**Ícone do menu (`CC_MenuIcon`) ancorado ao RectTransform do nome**

**Problema:** o ícone é filho do `NicknameText`, ancorado à esquerda. Se o nome é centralizado / tem `ContentSizeFitter`, o ícone pode colar no texto ou sair do lugar. **Validar in-game.**

**Sugestão:** se desalinhar, mover o ícone para um wrapper horizontal `[ícone][nome]` ou para o `BottomField` com offset.

### CR-02-04 · B — Bug latente · 🟡 Médio

**"EXP branco" pinta TODOS os TMP do `BottomField`**

**Local:** `MenuClassIdentityPatch.cs` (loop final)

**Problema:** o loop pinta de branco todo TMP do BottomField exceto nome/classe — inclui EXP (desejado) mas também qualquer outro label que o MO colore de propósito.

**Sugestão:** se algum label perder a cor original, restringir o branco ao TMP do valor de EXP (`ExperienceRow/ExpValue`) por nome.

### CR-02-05 · B — Bug latente · 🟢 Menor

**Patches do character/PlayerNamePanel não revertem não-local**

`PlayerModelWithStatsIdentityPatch` e `PlayerNamePanelPatch` aplicam só ao local, mas não revertem se a instância for reusada para outro perfil. Baixo risco (essas telas são do jogador local). Aceitar.

### CR-02-06 · E — Manutenção · 🟢 Menor

**`ClassTooltip.Clear` esvazia o texto mas não remove o `HoverTooltipArea`**

Aceitável (texto vazio = sem tooltip). Remoção do component seria mais limpa, mas custa GetComponent extra.

### CR-02-07 · D — Código morto · 🟢 Menor

**`ClassIdentityView.BuildColoredName` ficou órfão**

Todos os patches passaram a usar `<color>` inline + tooltip; `BuildColoredName` (nick + [CLASSE]) não é mais chamado. Remover numa limpeza.

## Pontos sólidos
- Reflection cacheada (`ChatIconField`/`NickField`/`IconField` static readonly).
- Try/catch + log em todos os patches; hot path (chat) sai cedo p/ não-local.
- Helper único `ApplyClassIcon` (sprite+tint+escala+fix Default) — consistência.
- Tooltip reusa a infra do item 010 (`HoverTooltipArea`/`ItemUiContext.Tooltip`), null-safe.
- Idempotência no menu (Find por nome) e nos nomes (reconstrói do nickname puro).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-09 | Code review 02 — direção final. 1 🟠 (sprite vaza) + 3 🟡 (layout menu/EXP) + 3 🟢. 0 🔴. |
