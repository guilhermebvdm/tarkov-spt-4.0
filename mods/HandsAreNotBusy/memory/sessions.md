# HandsAreNotBusy — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.7.6 (SPT 4.0 / EFT 0.16.9)
- **Estado:** Estabilização de rede FIKA (eliminação de desregistro nocivo no LiteNetLib), guarda de interoperabilidade com `Climbable Ladders` e desacoplamento defensivo com `LoadAmmoAnim` via reflexão robustecida.
- **Pendências:** 🟢 Nenhuma pendência registrada.

---

## 2026-09-05 — Sessão 1: Estabilização de Rede Coop (FIKA), Guarda de Escadas e Release v1.7.5 / v1.7.6

**Tema central:** Auditoria de compatibilidade cruzada com a camada de rede do FIKA e com o ecossistema de mods clientes (especialmente `Climbable Ladders` e `LoadAmmoAnim`).

**Decisões-chave:**
1. **Remoção de `UnregisterPacket` Destrutivo (`HANB_FikaSync.cs`):** Removida a chamada `_lastRegisteredNetworkManager?.UnregisterPacket<HanbClearInventoryPacket>()` no fechamento de raid. No LiteNetLib, desregistrar pacotes causava `ParseException` catastrófica caso chegassem datagramas atrasados no buffer de rede.
2. **Guarda contra Escadas (`Climbable Ladders`):** Adicionada verificação dinâmica em `HANB_Component.FixHandsController()`: se `player.gameObject.GetComponent("PlayerLadderController") != null`, o reset de mãos é cancelado. Evita que o HANB force `EmptyHandsController` ou saque armas enquanto o jogador está subindo escadas de mão.
3. **Desacoplamento de Bundle (`LoadAmmoAnim`):** Se as mãos do jogador estiverem controladas por `LoadAmmoBundleController`, o método `LoadAmmoAnimDriver.StopAnimationInstantly(player)` é invocado via reflexão antes de destruir o controller, evitando itens dinâmicos órfãos no cenário.
4. **Resiliência de Classloader (v1.7.6):** Implementada busca defensiva com varredura dos assemblies no `AppDomain.CurrentDomain` para localizar o tipo `LoadAmmoAnimDriver`, garantindo compatibilidade mesmo sob peculiaridades do classloader do BepInEx.
5. **Isolamento de Build e SemVer:** Versão incrementada para `1.7.5` e posteriormente `1.7.6` em `HANB_Plugin.cs` e `HandsAreNotBusy.csproj`. Compilação Release com 0 avisos e 0 erros.
