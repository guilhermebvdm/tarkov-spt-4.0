# 057 — Fix 04 · Host REAL do deploy (PartyPlayerItem) — o RaidReadyPlayerPanel era código morto

**Mod:** CustomClasses
**Item raiz:** [057-class-identity-coop-01-spec.md](057-class-identity-coop-01-spec.md)
**Criado:** 2026-07-04
**Disparado por:** code-review adversarial do lote não-revisado ([code-review-2026-07-04-unreviewed-batch.md](../code-review-2026-07-04-unreviewed-batch.md)), finding **CR-057F3-01 (ALTA)**.

## O finding estrutural

O host do 06-fix-03 (`RaidReadyPlayerPanel.Show`) **nunca executa no SPT**:

1. `SPT.SinglePlayer.Patches.MainMenu.ForceRaidModeToLocalPatch` força `ERaidMode.Local`;
2. no branch Local, `MatchMakerAcceptScreen.Show` executa `_playersRaidReadyPanel.Close()` incondicionalmente
   (decompile da Assembly instalada, `MatchMakerAcceptScreen:354-357`) e nunca subscreve `method_14`;
3. a listagem superior-esquerda REAL do "DEPLOYING TO LOCATION" é `MatchmakerTimeHasCome._partyInfoPanel`
   (`PartyInfoPanel` → rows `PartyPlayerItem`), populada pelo FIKA via `FikaBackendUtils.AddPartyMembers`.

**Corolário:** a escala do item 015 (`DeployNameScale`, mesmo Postfix) também nunca disparou — o default 3.0
era calibragem às cegas. A identidade local vista in-game sempre veio do `ChatSpecialIconPatch`
(`PartyPlayerItem.Show → _chatSpecialIcon.Show(info)` → overload de 4 params patchado). Evidência in-game
corrobora: o usuário nunca viu escala nem popover na listagem superior.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `Patches/PartyPlayerItemPatch.cs` (**novo**) | Postfix em `PartyPlayerItem.Show(GroupPlayerViewModelClass)`: escala 015 no `_chatSpecialIcon` + popover `FollowCursor` por linha resolvida + `ClassTooltip.Clear` (CR-057F3-07: tooltip 015 + popover duplicariam no mesmo hover). `PartyInfoPanelPrefetchPatch`: Prefix em `PartyInfoPanel.Show` → `ClassIdentities.Reset()` + `SkillMultipliers.Reset()+EnsureLoaded()` (caches frescos POR TELA — CR-057F3-03 + CR-051-03). |
| `Patches/RaidReadyPlayerPanelPatch.cs` | **REMOVIDO** (código morto). |
| `Patches/ChatSpecialIconPatch.cs` | (CR-057F3-02) resolve identidade de players REMOTOS via `ClassIdentities.TryResolve` em vez de reverter — este widget é a superfície real da linha; revert agora só p/ vanilla/desconhecido, restaurando a cor NATIVA capturada (CR-UI5-02). |
| `Patches/ClassDetailLoadingPatch.cs` (`LoadingClassHover`) | (CR-057F3-05) `PositionAtPointer` usa o rect do PARENT real (= root canvas, novo parent no `Ensure`) + `eventData.enterEventCamera`; (CR-057F3-06) âncoras/pivot do modo cursor setados no `Ensure` (sem rect "fill" residual) e o painel SÓ exibe com posição válida (`PositionAtPointer` → bool). |
| `Plugin.cs` | Registra `PartyPlayerItemPatch` + `PartyInfoPanelPrefetchPatch`; `DeployNameScale` default 3.0 → **1.2** (cfg instalado também ajustado 3 → 1.2 — o valor era cego). |
| `Patches/RaidPerksNotificationPatch.cs` | `ClassIdentities.Reset()` saiu do raid-start (o deploy abre ANTES; o refetch mora no Prefix da tela). Entrou `SkillMultipliers.EnsureLoaded()` warm (CR-051-01). |

Tradeoff aceito (CR-057F3-04): o fetch do mapa segue **síncrono** no 1º consumidor após o Reset — 1 GET
pequeno na abertura da tela de deploy (LAN), em vez de prefetch assíncrono. Documentado, revisitar se o hitch
incomodar em server remoto.

## Checklist de validação (substitui o do 06-fix-03)

- [x] Compile client+server 0/0 (2026-07-04 19:13); DLLs instaladas.
- [ ] Lista inferior do FIKA: **zero** mudança.
- [ ] Listagem superior: brasão + nome na cor POR PLAYER com classe; vanilla → intocado; escala 1.2 discreta
      (recalibrar `DeployNameScale` no F12 se quiser maior — agora funciona de verdade).
- [ ] Hover na linha → popover NO CURSOR, sem estourar borda; **sem** tooltip "This player is..." duplicado.
- [ ] Coop 2+ como cliente: popover/cor da classe DAQUELE player; troca de classe no editor web → próxima tela
      de deploy já reflete (sem restart).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Fix criado e aplicado (aguardando re-teste in-game) |
