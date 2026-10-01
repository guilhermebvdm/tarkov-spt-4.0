# 002 — Watchdog de Timeout de Inventário e Desync em Coop/Headless · Asbuild

**Mod:** FIKA  
**Data:** 2026-09-05  
**Autor:** Antigravity / saraiva  

---

## Arquivos Modificados / Criados

| Arquivo | Mudança Principal |
| :--- | :--- |
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs` | Adição de `CleanupExpiredCallbacks()`, dicionários de timestamp, auto-expiração em `WaitingForCallback`, e helper `RegisterOperationCallbackTimestamp`. |
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryController.cs` | Registro de timestamp inicial em `AddOperationCallback`. |
| `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Bump SemVer `2.3.11` $\rightarrow$ `2.3.12`. |

---

## Status da Compilação

- `Fika.Core.csproj` compilado com **0 Avisos, 0 Erros** em configuração `Release`.
- DLL gerada: `mods/FIKA/modded/Fika-Plugin/Fika.Core/bin/Release/netstandard2.1/Fika.Core.dll`.
- Validação reversa: `TRL-DynamicSpawn-Client.csproj` e `CameraRotationMod.csproj` compilaram com **0 Avisos, 0 Erros**.
