# TRL-Fixes — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.0.0 (SPT 4.0 / FIKA)
- **Estado:** Escopo enxuto mantido para correções pontuais do jogo base/FIKA (Flashbang IA e Revive Ragdoll).
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

---

## 2026-07-28 — Sessão 1: Inicialização de Governança
- **Ação:** Criação dos arquivos `mod.json`, `README.md`, `PROPRIEDADES.md` e `memory/sessions.md`.

---

## 2026-07-29 02:00 (GMT-3) — Sessão 2: Diagnóstico de Trava de Mãos e Racionalização de Escopo

**Tema central:** Investigação da causa raiz do erro de travamento de mãos ("mãos bugadas" / `hands controller can't perform this operation`) reportado em raid coop no FIKA e alinhamento do escopo do `TRL-Fixes`.

**Decisões-chave:**
- **Diagnóstico de Trava de Mãos:** Rastreada a stacktrace de exceção no log (`PoolManagerClass.CreateItem` -> `WeaponManagerClass.SetRoundIntoWeapon` -> `OnAddAmmoInChamber` -> `NullReferenceException`). Provado que desincronização de pacotes do FIKA em raid coop causava munição nula (`ammo = null`), o que interrompia o manipulador de eventos da animação da Unity e travava o `FirearmController` no estado `Busy`, fazendo o servidor rejeitar qualquer operação subsequente de mãos.
- **Remoção de `Patch_PoolManagerCreateItem.cs`:** Removido o patch `Patch_PoolManagerCreateItem.cs` a pedido do usuário após demonstrar que os mods TRL eram inocentes nesse erro e que a desincronização era nativa do transporte do FIKA.
- **Definição de Arquitetura do Mod:** Confirmado que o `TRL-Fixes` não precisará de patches pesados de tentativa de resync de rede do FIKA, pois cada mod TRL passará a tratar seu próprio tráfego em canais isolados. O `TRL-Fixes` permanece focado estritamente em suas correções de gameplay (Flashbang IA e Hitbox pós-Revive).

**Lições / hipóteses descartadas:**
- A hipótese de que o mod `TRL-Fixes` estaria causando desincronização de munição foi descartada após análise minuciosa da stacktrace do FIKA.

**Atividade cronológica:**
1. Leitura e análise da stacktrace do log do usuário (`LogOutput Cherno.log`).
2. Confirmação do fluxo de estado `Busy` do `FirearmController` em decorrência de `NullReferenceException` em `OnAddAmmoInChamber`.
3. Remoção do arquivo `Patch_PoolManagerCreateItem.cs`.

---

## 2026-08-02 — Sessão 3: Inclusão do DynamicMapsSafetyPatch e FikaMainThreadUISafetyPatch

**Tema central:** Adição de novos patches de proteção e resiliência no `TRL-Fixes` para eliminar erros de mods de terceiros (`DynamicMaps`) e chamadas de UI fora da thread principal no `Fika.Core`.

**Alterações Realizadas:**
1. **`DynamicMapsSafetyPatch.cs`**:
   - Refatorado para utilizar **`[PatchFinalizer]`** do Harmony em `DynamicMaps.Patches.GameWorldOnDestroyPatch`, garantindo a supressão absoluta de `NullReferenceException` lançada durante o encerramento da raid (`ModdedMapScreen.OnRaidEnd()`).
2. **`FikaMainThreadUISafetyPatch.cs`**:
   - Refatorado para utilizar **`[PatchFinalizer]`** do Harmony em `Fika.Core.UI.FikaUIGlobals.ShowFikaMessage`, capturando e absorvendo erros de chamadas de UI originadas fora da Main Thread do Unity.
3. **Registro no `Plugin.cs` & `TRLFixes.csproj`**:
   - Ativados os novos patches no `Awake()`, exposto `Plugin.Log` para os patches e adicionada a referência `SPT.Reflection.dll` no `.csproj`.
4. **Validação de Build**:
   - Compilado `TRLFixes.csproj` (`TRL-Fixes.dll`) com **0 Erros e 0 Warnings**.
