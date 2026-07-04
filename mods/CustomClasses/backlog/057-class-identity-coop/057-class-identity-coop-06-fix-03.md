# 057 — Fix 03 · Host definitivo (listagem superior do jogo) + popover no cursor; FIKA intocado

**Mod:** CustomClasses
**Item raiz:** [057-class-identity-coop-01-spec.md](057-class-identity-coop-01-spec.md)
**Criado:** 2026-07-04
**Disparado por:** feedback do usuário (gate 3) — direcionamento explícito de UX.

## Decisões do usuário (2026-07-04)

1. **A lista INFERIOR-esquerda (progresso de carregamento do FIKA) não deve ser tocada em NADA** — nem tint,
   nem hover. É propriedade do FIKA.
2. O host da identidade/popover é a **listagem SUPERIOR-esquerda do deploy** (painel de grupo do PRÓPRIO JOGO,
   `RaidReadyPlayerPanel` — onde o 015 já aplica nome+classe): ali entram **brasão + cor da classe POR PLAYER**.
3. O popover abre **NO CURSOR** (popover de verdade), não ancorado à direita sobre o carrossel de imagens.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Client/Plugin.cs` | `ClassDetailLoadingPatch` **DESREGISTRADO** (rows do FIKA 100% intocadas — a classe fica no repo como referência, inerte). |
| `modded/Client/Patches/RaidPerksNotificationPatch.cs` | `ClassIdentities.Reset()` no raid-start (o refetch por raid — PA-01-04 — morava no patch desativado). |
| `modded/Client/Patches/RaidReadyPlayerPanelPatch.cs` | Identidade NA LINHA pra cada player resolvido: `ApplyClassIcon` no `ChatSpecialIcon` + `ApplyGradient` no nome (tint-only, CR-01-04); `hover.FollowCursor = true`. |
| `modded/Client/Patches/ClassDetailLoadingPatch.cs` (`LoadingClassHover`) | Modo `FollowCursor`: `Show(PointerEventData)` → `PositionAtPointer` (pivot topo-esquerdo, abre pra baixo/direita do mouse, +18/−14 de respiro, **clamp** aos limites do canvas usando a área visual constante Base×escala-compensada). Âncora fixa à direita só no modo legado. |

## Checklist de validação

- [x] Compile 0/0 (2026-07-04 18:00); DLL instalada.
- [ ] Lista inferior do FIKA: **zero** mudança visual/comportamental.
- [ ] Listagem superior: cada player com classe → brasão + nome na cor; vanilla → intocado.
- [ ] Hover na linha → popover abre NO CURSOR, sem estourar a tela (testar linha perto da borda).
- [ ] Coop 2+ como cliente: popover mostra a classe DAQUELE player.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Fix criado e aplicado (aguardando re-teste) |
