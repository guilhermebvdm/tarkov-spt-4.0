---
title: "Item 006 — Culling de Cápsulas de Balas (Interceptação Precoce) — As-Built"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 006 — Culling de Cápsulas de Balas (Interceptação Precoce) — As-Built

## 1. Resumo Executivo

O Item 006 implementou no mod **TRL-CoreSight** o módulo de **Culling de Física e Ejeção de Cápsulas de Bala** com a estratégia arquitetural de **interceptação precoce no spawn**.

Em vez de permitir a instanciação de cartuchos no mundo para depois tentar desligar sua física ou renderização (o que arriscava gerar cápsulas flutuando no ar), o sistema atua no momento exato do disparo, cancelando a corrotina de spawn (`StartSpawnShell`) para armas empunhadas por bots que estejam além do raio de percepção do jogador (padrão: 25 metros), aliviando dezenas de corpos rígidos na PhysX e chamadas de renderização na Unity durante tiroteios intensos.

---

## 2. Componentes e Funcionalidades Implementadas

### 2.1. Patches Harmony de Interceptação (`Patches/ShellSpawnCullingPatch.cs`)
- **`ShellSpawnCullingPatch` (`WeaponManagerClass.StartSpawnShell`):**
  - Intercepta disparos únicos de fuzis, pistolas e submetralhadoras efetuados por bots.
  - Verifica se o atirador é o jogador local (`weaponManager.Player.IsYourPlayer`): se for, permite execução normal (retorna `true`).
  - Se a distância da arma até a câmera ativa exceder `ModConfig.ShellCullingDistance.Value` (padrão 25m), retorna `false` no Prefix, abortando imediatamente a corrotina `StartCoroutine(method_6)`.
- **`ShellSpawnAllCullingPatch` (`WeaponManagerClass.StartSpawnAllShells`):**
  - Intercepta ejeções múltiplas simultâneas em espingardas de dois canos e revólveres disparados à distância.
- **`ShellSpawnJamCullingPatch` (`WeaponManagerClass.SpawnShellAfterJam`):**
  - Intercepta cartuchos ejetados ao sanar panes de armas a longa distância.

### 2.2. Integração com o Motor Nativo do EFT (`Core/PerformanceManager.cs`)
- **Sincronização com `EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE`:**
  - Armazena a distância nativa original da BSG (`25f`) no `Initialize()` da partida.
  - Aplica o valor configurado pelo jogador no F12 diretamente na engine.
  - Restaura o valor original deterministamente no `Cleanup()` ao final da raid.

### 2.3. Menu BepInEx F12 (`Configuration/ModConfig.cs` e `PROPRIEDADES.md`)
Criada a seção **`8. Otimização de Física (Cápsulas de Bala)`**:
- `EnableShellCulling` (`bool`, padrão `true`): Ativa ou desativa a supressão de cápsulas distantes.
- `ShellCullingDistance` (`float`, padrão `25.0m`, faixa `10.0m` a `60.0m`): Distância máxima em metros em relação à câmera para renderizar e simular cartuchos ejetados.

### 2.4. Telemetria e Debug On-Screen (`PerformanceManager.OnGUI`)
- Adicionada a linha de telemetria `Shell Culling: 25m` (ou `OFF`) ao overlay do CoreSight quando `DebugMode = true`.

---

## 3. Arquivos Criados e Modificados

| Arquivo | Ação | Descrição |
|---|---|---|
| `modded/Patches/ShellSpawnCullingPatch.cs` | CRIAR | Patches Harmony Prefix para `StartSpawnShell`, `StartSpawnAllShells` e `SpawnShellAfterJam`. |
| `modded/Configuration/ModConfig.cs` | MODIFICAR | Registro das configurações `EnableShellCulling` e `ShellCullingDistance`. |
| `modded/Core/PerformanceManager.cs` | MODIFICAR | Sincronização e restauração do limiar nativo `FLYING_SHELLS_VISIBLE_DISTANCE` e telemetria no `OnGUI`. |
| `modded/Plugin.cs` | MODIFICAR | Ativação dos novos patches no `Awake()` e bump SemVer para `0.3.2`. |
| `modded/TRL-CoreSight.csproj` | MODIFICAR | Inclusão do arquivo compilado e bump SemVer para `0.3.2`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.2`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentação da seção 8 de Otimização de Física. |

---

## 4. Verificação e Compilação

- **Compilação:** `dotnet build mods/TRL-CoreSight/modded/TRL-CoreSight.csproj -c Release`
- **Resultado:** 0 Erros, 0 Avisos. Tempo decorrido: 1.24s.
- **Binário isolado gerado:** `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (49.152 bytes).
