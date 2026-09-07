# 010 — Manual Chambering · 06-fix-02

**Mod:** stancesAndCameraPositionSPT4.0.11  
**Data:** 2026-09-04  
**Status:** 🟢 Implementado, Validado In-Game e Código Limpo (v2.19.9)  
**Origem:** Solicitação do usuário sobre Dry Rack, Rearme do Cão Mecânico, Inserção Suave de Carregador sem Puxada de Ferrolho via Mecanim e Limpeza de Código Morto.

---

## 1. Sintomas e Problemas Abordados

1. **Rearme do cão mecânico no Dry Rack:**
   Ao puxar o ferrolho no seco sem munição, `Weapon.Armed` não ficava ativo confiavelmente, impedindo o som do cão batendo na arma no disparo seco subsequente.
2. **Rearme indevido do cão na Inspeção de Câmara:**
   Ao verificar a câmara (`CheckChamber`) com a arma desarmada após Dry Fire, o Animation Event `IEventsConsumerOnArm` rearmava o cão incorretamente.
3. **Esvaziar câmara com a última munição (`mag.Count == 0`):**
   Ao ejetar a última bala com carregador vazio na arma, o EFT vanilla abortava a animação em `OnShellEjectEvent` antes de `RemoveAmmoFromChamber()`, revertendo a ejeção.
4. **Inserção de carregador puxando o ferrolho:**
   Ao colocar um carregador na arma desmuniciada (câmara vazia e sem carregador), o EFT vanilla fazia `PopTo` da 1ª bala para a câmara (`HasNewAmmo = true`) e executava a puxada de ferrolho (`OnAddAmmoInChamber`), alimentando e armando a arma sem permissão manual.

---

## 2. Soluções Implementadas

| Arquivo | Componente / Patch | Ação |
|---|---|---|
| `Patches/ManualChamberingPatches.cs` | `ManualChamberingComponent` | Força `FirearmController.Weapon.Armed = true` e `SetHammerArmed(true)` nas Phases 1 e 2 do ciclo de puxada de ferrolho manual. |
| `Patches/ManualChamberingPatches.cs` | `CheckChamberArmPatch` | Intercepta `IEventsConsumerOnArm`: se a operação for `CheckChamber` e a arma já estiver desarmada, suprime o rearme espúrio. |
| `Patches/ManualChamberingPatches.cs` | `RechamberOperationEmptyMagFixPatch` | Em `RechamberOperationClass.OnShellEjectEvent`, quando `Item_1 == null`, chama defensivamente `RemoveAmmoFromChamber()`, engatilha `Weapon.Armed = true` e atualiza Bolt Catch. |
| `Patches/ManualChamberingPatches.cs` | `InstallMagChamberPatch` | Intercepta `InstallMagResultClass.Run` (`GClass2005.Run`): suprime `PopTo` quando a câmara está vazia, deixando `HasNewAmmo = false`. |
| `Patches/ManualChamberingPatches.cs` | `InstallMagStartPatch` e `StartReloadResetPatch` | Em `Start`, define temporariamente `AmmoInChamber = 1f` e `SetBoltCatch(false)` com checagens seguras contra NRE, permitindo a transição suave de Tactical Reload / MAG IN NOT CHAMBERED. |
| `Patches/ManualChamberingPatches.cs` | `IdleStartEventPatch` e `ReloadIdleStartEventPatch` | No evento nativo `OnIdleStartEvent`, quando a mão esquerda já retornou suavemente para a empunhadura em Idle, restaura `AmmoInChamber = 0f` sem cortes secos. |
| `Patches/ManualChamberingPatches.cs` | `ReloadResetPatch` e `InstallMagResetPatch` | Em caso de interrupção abrupta (sprint), restaura imediatamente `AmmoInChamber = 0f`. |
| `Plugin.cs` | `SafeEnable` | Registra os patches de transição suave com isolamento defensivo e bump de versão para `2.19.8`. |
| `CameraRotationMod.csproj` | `<Version>` | Atualizado para `2.19.8` / `2.19.8.0`. |

---

## 3. Critérios de Aceite Validados

- [x] Puxar o ferrolho sem munição arma o cão (`Weapon.Armed = true`) e permite o som do cão batendo no próximo clique seco.
- [x] Inspeção de câmara com a arma desarmada não arma o cão indevidamente.
- [x] Esvaziar câmara com última munição ejeta o projétil no chão/mão e mantém a câmara vazia.
- [x] Colocar carregador na arma sem carregador e com câmara vazia (via inventário ou tecla `R`) apenas encaixa o carregador e volta para Idle, sem puxar o ferrolho.
- [x] Compilação limpa com 0 erros e 0 avisos em `Release/netstandard2.1/TRL-StancesAndMobility.dll`.

---

## 4. Limpeza de Código Morto (v2.19.9)

- **Remoção de `ManualChamberingState.AllowVanillaChamberUnload`:** Variável órfã criada em testes anteriores de permissão de câmara.
- **Remoção de `GamePlayerOwner GamePlayerOwner` em `ManualChamberingComponent`:** Campo não atribuído nem lido.
- **Versão sincronizada em SemVer:** `v2.19.9` em `Plugin.cs` e `CameraRotationMod.csproj`.

