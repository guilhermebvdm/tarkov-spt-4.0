# TRL-Fixes

Coleção de patches e correções essenciais para Tarkov Red Line (SPT 4.0 / FIKA).

## Correções Incluídas
1. **FlashbangBotPatch**: Ajustes no efeito e reação de IAs a granadas de atordoamento.
2. **FlashbangRadiusPatch**: Correção no raio de alcance e atenuação das flashbangs.
3. **Patch_PoolManagerCreateItem**: Prevenção de exceções na criação de itens pelo PoolManager.
4. **FixFikaReviveRagdollPatch**: Correção de ragdoll e dessincronização ao reviver no FIKA coop.
5. **PickupAimingSafetyPatch**: Impede a trava de controles ao pegar/equipar item do chão (o corpo congela e só
   a interface responde). Bug do jogo base; diagnóstico em [`docs/handoff-pickup-aiming-safety.md`](./docs/handoff-pickup-aiming-safety.md).

## Estrutura do Mod
- `modded/` — Código-fonte C# dos patches e plugin.
- `PROPRIEDADES.md` — Mapeamento de configurações do mod (sem opções F12 ativas no momento).
- `backlog/` — Especificações funcionais e técnicas das correções.
- `memory/sessions.md` — Histórico de sessões e pendências do mod.
