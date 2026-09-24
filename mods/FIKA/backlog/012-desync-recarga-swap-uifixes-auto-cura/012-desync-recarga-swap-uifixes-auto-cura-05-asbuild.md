# 012 — Desync de Recarga Rápida (Swap UIFixes) e Auto-Cura de Slots de Inventário · As-Built

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Spec Técnica:** [012-desync-recarga-swap-uifixes-auto-cura-02-spec-tech.md](012-desync-recarga-swap-uifixes-auto-cura-02-spec-tech.md)  
**Versão Produzida:** `Fika.Core.dll` v2.4.2  
**Data:** 2026-09-15  

---

## 1. Resumo da Entrega

Formalização retroativa das correções que solucionaram o deadlock de recarga de carregadores na AK-12 com a tecla "R" (caso "Manodavis", servidor Headless Interchange), compiladas e entregues na versão **2.4.2** no fork `mods/FIKA/modded-V2`.

---

## 2. Arquivos Alterados no Fork `modded-V2`

| Arquivo | Ação | Descrição Técnica |
| :--- | :---: | :--- |
| `ReloadMagPacket.cs` | MODIFICADO | Verificação prévia de colisão de slot no Headless antes de `ReloadMag`. Realocação via `QuickFindAppropriatePlace` ou descarte no chão com `gridItemAddress = null`. |
| `ClientInventoryOperationHandler.cs` | MODIFICADO | Força `operation.Status = EOperationStatus.Failed` antes do descarte da operação em rejeições do host, acionando o `RollBack` nativo do EFT e liberando a trava de `WaitingForCallback`. |
| `FikaPlayer.cs` | MODIFICADO | Reconciliação tardia em `FikaPlayer.cs:1938-1946`: respostas que chegam após os 5s do Watchdog aceitam o resultado `Succeeded` do servidor em vez de descartar como desconhecido. |
| `SplitOperationDescriptorPatch.cs` | CRIADO | Auto-reconciliação de origem em operações de split (`GClass1538` prevenido para itens do próprio jogador). |
| `MoveOperationDescriptorPatch.cs` | CRIADO | Auto-reconciliação de origem em operações de move com construtor de 4 parâmetros de `MoveOperationClass`. |
| `FikaPlugin.cs` | MODIFICADO | Bump de versão para `2.4.2` e registro dos novos patches no `_patchManager`. |
| `Fika.Core.csproj` | MODIFICADO | Atualização da tag `<Version>2.4.2</Version>`. |

---

## 3. Binários Produzidos (Release)

- **Binário:** `Fika.Core.dll` (v2.4.2)
- **Path:** `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/bin/Release/netstandard2.1/Fika.Core.dll`
- **Compilação:** `dotnet build -c Release` (0 erros).
- **Isolamento:** Nenhuma DLL foi copiada para fora da pasta do mod.
