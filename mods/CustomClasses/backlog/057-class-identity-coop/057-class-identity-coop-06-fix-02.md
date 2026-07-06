# 057 — Fix 02 · Popover migrado pro painel de grupo do deploy (hover nas rows do FIKA nunca dispara)

**Mod:** CustomClasses
**Item raiz:** [057-class-identity-coop-01-spec.md](057-class-identity-coop-01-spec.md)
**Asbuild:** [057-class-identity-coop-05-asbuild.md](057-class-identity-coop-05-asbuild.md)
**Criado:** 2026-07-03
**Disparado por:** feedback in-game 2026-07-03 (gate 2, com prints) — mesmo após o 06-fix-01, o hover nas linhas
de progresso do FIKA (rodapé do loading) não abre o popover; usuário pediu alternativa que funcione "para
qualquer player que a gente passar o mouse em cima do nome no momento do carregamento".

## Contexto

Duas rodadas de gate falharam no MESMO host (rows do `LoadingScreenUI` do FIKA). Evidência indireta: tooltips
nativos funcionam no **painel de grupo** do deploy (topo-esquerdo, `RaidReadyPlayerPanel` — o 015 já mexe nele),
mas nenhum evento de pointer chega às rows do FIKA (canvas próprio, sem raycast confiável).

## Causa raiz (provável)

O canvas do `LoadingScreenUI` (prefab FIKA) não participa do raycast de UI durante o deploy — sem
`GraphicRaycaster` efetivo naquela árvore, `IPointerEnterHandler` nunca dispara, independente do nosso Image
transparente. Não é determinável 100% sem inspeção runtime; a decisão foi trocar de host em vez de insistir.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Client/Patches/RaidReadyPlayerPanelPatch.cs` | Vira o HOST do popover no deploy: Postfix de `Show(GroupPlayerViewModelClass player, ...)` resolve `player.Info.Nickname` (ref: GClass1410.cs:9) via `ClassIdentities` (fallback local) e anexa `LoadingClassHover` com a `Identity` — funciona pra QUALQUER membro do grupo (1 painel por player). Raid scav degrada sozinha (linha usa `SavageNickname` → sem match). Gate: `ClassDetailOnLoading`. |
| `modded/Client/Patches/ClassDetailLoadingPatch.cs` | Mantido como redundância (se o canvas do FIKA um dia raycastar, tint+hover das rows passam a funcionar). |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (2026-07-03, 0 erros)
- [ ] **Deploy:** hover na linha do player no PAINEL DE GRUPO (topo-esquerdo) abre o popover da classe
- [ ] **Fika/multiplayer:** com 2+ players, cada linha do grupo mostra a classe do respectivo player (validar como CLIENTE)
- [ ] **raid1 → exit → raid2:** popover volta a funcionar no deploy seguinte
- [ ] **alt-F4 / morte / MIA:** sem exceção no LogOutput.log
- [ ] Memória do mod atualizada com a lição (canvas FIKA sem raycast; host correto = RaidReadyPlayerPanel)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Fix criado e aplicado (compilado; aguardando re-teste in-game) |
