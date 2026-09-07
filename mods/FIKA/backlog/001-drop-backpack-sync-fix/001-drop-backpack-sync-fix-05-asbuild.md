# 001 — Fix descarte de mochila ("ZZ" / DropBackpack) no Headless/Host · As-Built

**Mod:** FIKA  
**Data:** 2026-09-03  
**Versão:** 2.3.11  

---

## Arquivos Criados e Modificados

| Arquivo | Status | Descrição |
|---|---|---|
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs` | Novo | Implementação do patch Harmony `ObservedPlayer_DropBackpackSafety_Patch`. |
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Modificado | Bump de versão SemVer de `2.3.10` para `2.3.11`. |
| `mods/FIKA/mod.json` | Modificado | Bump de versão SemVer de `2.3.10` para `2.3.11`. |
| `mods/FIKA/modded/Fika-Headless/References/Fika.Core.dll` | Sincronizado | Atualização da referência com o binário compilado v2.3.11. |

---

## Resultados da Compilação

- **Fika.Core (v2.3.11):**
  - Comando: `dotnet build -c Release`
  - Saída: `0 Erro(s)`, `0 Aviso(s)`
  - Destino: `mods/FIKA/modded/Fika-Plugin/Build/BepInEx/plugins/Fika.Core.dll`
- **Fika.Headless (v1.4.16):**
  - Comando: `dotnet build -c Release`
  - Saída: `0 Erro(s)`, `0 Aviso(s)`
  - Destino: `mods/FIKA/modded/Fika-Headless/Build/Fika.Headless.dll`
