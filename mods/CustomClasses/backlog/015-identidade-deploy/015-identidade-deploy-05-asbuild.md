# 015 — Identidade da classe no nome do jogador · As-Built

**Mod:** CustomClasses
**Spec funcional:** [015-identidade-deploy-01-spec.md](015-identidade-deploy-01-spec.md)
**Spec técnica:** [015-identidade-deploy-02-spec-tech.md](015-identidade-deploy-02-spec-tech.md)
**Build inicial:** 2026-06-08

> Patch no widget compartilhado `ChatSpecialIcon` (jogador local) → ícone da classe + nome com gradiente + `[Classe]` (sufixo mesma-linha). Cobre deploy/character/online/chat num só ponto. Selo separado (012) desligado por padrão (reversível no F12). Compilado **0 warn/err** (client 28.2 KB). **Posições/efeito a validar in-game.**

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded/Server/SkillMultipliersResponse.cs` | + `nickname`. |
| MODIFICADO | `modded/Server/SkillMultipliersRouter.cs` | `Nickname` = `CharacterData.PmcData.Info.Nickname` do perfil. |
| MODIFICADO | `modded/Client/SkillMultipliers.cs` | + `Nickname` (prop + Payload + Reset). |
| CRIADO | `modded/Client/Patches/ChatSpecialIconPatch.cs` | postfix em `ChatSpecialIcon.Show` (4 params); só jogador local; ícone+gradiente+`[Classe]`. |
| MODIFICADO | `modded/Client/Plugin.cs` | registra patch; `ShowClassOnPlayerName` (default true); **`ShowClassIdentity` default false**. |
| MODIFICADO | `mods/CustomClasses/PROPRIEDADES.md` | novas/alteradas. |

## Decisões implementadas

- **Só ChatSpecialIcon:** identidade no nome do jogador local em deploy/character/online/chat (1 patch). Selo do 012 (menu-MO/Skills) **off por padrão**, reversível no F12 (`ShowClassIdentity`).
- **Jogador local:** `playerName == SkillMultipliers.Nickname` (nickname vindo da rota). Outros jogadores: intactos.
- **Título:** sufixo mesma-linha `[Classe]` (`<size=80%>`) — seguro em listas. 2ª-linha-no-character = refinamento futuro (precisa detectar contexto).
- **Cor:** gradiente (`ClassIdentityView.ApplyGradient`).

## Riscos a validar (playtest)

- **Outros jogadores** (chat/online list) devem ficar **intactos** (só o local muda) — verificar em coop/Fika.
- O ícone da classe no slot do `_icon` (tamanho/aspect) — conferir que não estoura.
- O sufixo `[Classe]` em listas compactas — conferir que cabe.

## Mudanças posteriores

**2026-06-09 — replanejamento por tela (a 1ª versão "1 patch ChatSpecialIcon" não aparecia no menu nem na OVERALL):** investigação (ilspycmd + source do MO/FIKA) mostrou que cada tela usa um widget diferente. Plano aprovado em `~/.claude/plans/`. **Fase 1** (menu + character):
- `MenuClassIdentityPatch` **reescrito**: integra no `NicknameText` do Menu-Overhaul (`MainMenuPlayerModelView/BottomField/NicknameText`) — gradiente da cor da classe + título `[Classe]`, reconstruído do nickname puro (idempotente). Não mais selo separado. Gated por `ShowClassOnPlayerName`.
- `PlayerModelWithStatsIdentityPatch` **(novo)**: tela de character/OVERALL — `InventoryPlayerModelWithStatsWindow.Show` (7 params); recolore `_nicknameLabel` + título + troca o `_icon` do `_specialIcon` (reflection cacheado) pelo ícone da classe. Só o jogador local (nickname bate).
- `ChatSpecialIconPatch` mantido (deploy/chat).
- Selo separado do **menu removido**; configs `MenuClassPos*` removidas (selo só na tela de Skills, sob `ShowClassIdentity`).
- Recompilado **0 warn/err** (client 27.6 KB). **Fase 2 (botão SKILLS, item 013) pendente.**

**2026-06-09 — direção final (cor + tooltip + ícone; plano aprovado em `~/.claude/plans/`):** após playtest, mudança de "[CLASSE] no texto/gradiente" para **nome colorido + ícone da classe (tingido + escala) + tooltip "This player is \<classe\>"** (i18n). Causa raiz do ícone do deploy: `EMemberCategory.Default==0` → `ChatSpecialIcon.Show` retorna cedo (não seta sprite) → `ApplyClassIcon` força sprite/ativação.
- **Helpers (novos/ajustados):** `ClassTooltip` (HoverTooltipArea + ItemUiContext.Tooltip, idempotente, Clear p/ células recicladas); `ClassIdentityView.ApplyClassIcon(Image/ChatSpecialIcon)` (sprite+tint+escala+fix Default) e `BuildTooltip` (i18n EN/PT).
- **Patches:** `ChatSpecialIconPatch` (deploy/chat/grupo) + `PlayerModelWithStatsIdentityPatch` (character/OVERALL) → nome colorido (sem `[CLASSE]`) + ícone + tooltip; reverte outros jogadores. **Novo** `PlayerNamePanelPatch` (confirmation). `MenuClassIdentityPatch` **reescrito**: layout 3 linhas (ícone+nome / NOME DA CLASSE CAPSLOCK / EXP branco).
- **Config:** `ClassIconScale` (slider F12, default 1.12). Selo da tela de Skills (012) mantido (cor sólida + CAPSLOCK).
- Recompilado **0 warn/err** (client 31.2 KB). **A validar in-game.**

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-08 | Build via `/code-mod`. 0 warn/err. ChatSpecialIcon (jogador local) + 012 off por padrão. Review técnica condensada (riscos na spec-tech §6). |
