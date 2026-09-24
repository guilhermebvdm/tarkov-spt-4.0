# 008 — Fix 04 · Intensidade de chute configurável, separada por corpo e item

**Mod:** VisceralCombat
**Item raiz:** [008-atropelar-acorda-corpos-itens-01-spec.md](008-atropelar-acorda-corpos-itens-01-spec.md)
**Asbuild:** [008-atropelar-acorda-corpos-itens-05-asbuild.md](008-atropelar-acorda-corpos-itens-05-asbuild.md)
**Criado:** 2026-09-21
**Disparado por:** pedido do usuário — "Legs Impulse Intensity" não afetava o atropelar (config errada, é só pra impacto de bala) e não havia nenhum jeito de ajustar a força do chute

## Contexto

Usuário testou o atropelar (fix 03) e confirmou que estava funcionando, mas tentou calibrar a intensidade via "Legs Impulse Intensity" (até `100`) sem nenhum efeito perceptível. Pediu dois campos configuráveis independentes: um pra intensidade de chute em corpos, outro pra itens.

## Causa raiz (da confusão, não um bug)

"Legs Impulse Intensity" é um multiplicador só pro ramo de **impacto de bala** em pernas de corpo (`BodiesImpulsePatch.cs`), sem nenhuma relação com o atropelar (`PlayerContactPushPatch.cs`). Este último usava uma constante fixa no código (`PushIntensity = 0.5f`, sem exposição no F12), compartilhada entre corpo e item — não configurável.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | 2 `ConfigEntry<float>` novos: `CorpseKickIntensity` ("Corpse Kick Intensity", seção `Ragdolls \| Ragdoll Physical Properties`, default `0.5`) e `ItemKickIntensity` ("Item Kick Intensity", seção `Physics \| Item Physical Properties`, default `0.5`). |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs` | `deltaV` deixou de ser calculado 1x fora do loop com `PushIntensity` fixo — agora é calculado por alvo, dentro do loop, lendo `ItemKickIntensity`/`CorpseKickIntensity` conforme o tipo (`isLootItem`/corpo). `PushIntensity` (constante) vira só fallback defensivo caso `VisceralEntry.Instance` ainda não tenha montado os `ConfigEntry`. |
| `mods/VisceralCombat/PROPRIEDADES.md` | Documentadas as 2 novas entradas. Aproveitado pra também documentar retroativamente a seção `Ragdolls \| Ragdoll Physical Properties` (grafia correta, existente no código desde antes desta sessão mas nunca documentada — Head/Torso/Arms/Legs Impulse Intensity), que não tinha entrada nenhuma no documento até agora. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (build 3.12.8)
- [ ] **In-raid:** ajustar "Item Kick Intensity" muda a força do chute em item, sem afetar corpo; ajustar "Corpse Kick Intensity" muda a força do chute em corpo, sem afetar item — pendente de novo teste do usuário
- [ ] **Fika/multiplayer:** sem regressão com outros players — ou `N/A: <razão>`
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Fix criado e aplicado — build 3.12.8 compilada, 0 erros. Aguardando validação em raid do usuário. |
