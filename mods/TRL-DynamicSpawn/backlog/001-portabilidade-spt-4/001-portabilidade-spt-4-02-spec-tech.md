# Spec Técnica — portabilidade-spt-4

**Mod:** TRL-DynamicSpawn
**Status:** Entregue

## Solução proposta

Corrigir a inicialização do DynamicSpawnManager no cliente local obtendo referências seguras via `Singleton<IBotGame>.Instance` no cliente.

## Arquivos Modificados

- [Plugin.cs](../../Client/Plugin.cs)
- [DynamicSpawnManagerPatch.cs](../../Client/Patches/DynamicSpawnManagerPatch.cs)
- [TRL-DynamicSpawn-Client.sln](../../Client/TRL-DynamicSpawn-Client.sln)
