# SPT-ContinuousLoadAmmo — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.1.8 (SPT 4.0 / EFT 0.16.9)
- **Estado:** Documentação técnica modular completa (01 a 05 + índice), auditoria técnica profunda concluída e resolução integral de todos os achados (`AUD-01-01` a `AUD-01-06`), eliminando vazamentos de memória (RAM Leaks) entre raids, restaurando o estado do `InventoryScreen` via `__state` e otimizando o Garbage Collector com buffers zero-alloc.
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

---

## 2026-08-27 / 2026-08-28 — Sessão 1: Documentação Técnica Modular, Auditoria Estática e Release v1.1.8

**Tema central:** Geração da documentação técnica e arquitetural completa do mod, auditoria estática rigorosa de código com base nas referências canônicas do EFT/SPT/FIKA e aplicação do pacote de estabilização v1.1.8.

**Decisões-chave:**
1. **Documentação Modular Estruturada:** Criada a suíte de documentação em `docs/` dividida em 5 artigos temáticos com diagramas Mermaid conceituais, tabelas comparativas e índice central `docs/README.md`.
2. **Eliminação de Memory Leak (`AUD-01-01` / `CR-02-01`):** Identificado que o encerramento de raid por extração ou abort não disparava `OnIPlayerDeadOrUnspawn`, retendo o `LoadAmmoController` e o `Player` em 5 eventos estáticos. Corrigido adicionando a chamada `Close()` e `_loadAmmoControllerController?.Dispose()` no `OnDestroy` do `LoadAmmoComponent`, tornando o `Dispose()` idempotente através de `_disposed`.
3. **Preservação de Estado da UI (`AUD-01-02` / `CR-02-02`):** Substituída a mutação destrutiva `___inventoryController_0 = null` no `InventoryScreenClosePatch` pelo padrão `__state` do Harmony, restaurando o ponteiro original no `Postfix` e evitando `NullReferenceException` em acessos subsequentes de interface.
4. **Desacoplamento de Classes Ofuscadas (`AUD-01-03` / `CR-02-03`):** Removida a referência volátil `MagazineBuildPresetClass.Class1023.String_0.Localized()` em `MagazinePresetLoader.cs`, adotando a chave canônica `"Preset missing ammo".Localized()`.
5. **Otimização Zero-Alloc (`AUD-01-04` / `CR-02-04`):** Implementado o scratch buffer `_allAmmoScratch` e o delegate estático `_ammoComparison` em `LoadAmmoController.GetAllAmmoForMagazine`, eliminando churn no GC.
6. **Prevenção de Concorrência (`AUD-01-06` / `CR-02-06`):** Adicionado cancelamento automático de presets assíncronos caso o jogador inicie um arrasto manual de munição no inventário.
7. **Bump SemVer:** Versão elevada de `1.1.7` para `1.1.8` sincronizada em `ContinuousLoadAmmo.cs` e `ContinuousLoadAmmo.csproj`.

**Atividade cronológica:**
1. Execução do `/document-mod` gerando 5 artigos modulares e o índice `docs/README.md`.
2. Execução do `/audit-mod-code` produzindo o [relatorio-auditoria-codigo-01.md](../docs/relatorio-auditoria-codigo-01.md) com 6 achados técnicos.
3. Elaboração e aprovação do Plano de Implementação para resolução dos achados.
4. Implementação do código corretivo em `LoadAmmoComponent.cs`, `LoadAmmoController.cs`, `InventoryScreenClosePatch.cs`, `MagazinePresetLoader.cs`, `ContinuousLoadAmmo.cs` e `.csproj`.
5. Compilação Release via MSBuild gerando binários `ContinuousLoadAmmo.dll` v1.1.8 e pacote `ozen-ContinuousLoadAmmo-1.1.8.zip` (0 erros, 0 avisos).
6. Execução do `/code-review` gerando o [relatorio-auditoria-codigo-02.md](../docs/relatorio-auditoria-codigo-02.md) com aprovação técnica final para produção.
