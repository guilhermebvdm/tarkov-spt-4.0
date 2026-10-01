---
title: "Item 003 — Culling de Sombras de Luzes Locais — As-Built"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 003 — Culling de Sombras de Luzes Locais — As-Built

## 1. Resumo Executivo

O Item 003 implementou no mod **TRL-CoreSight** o módulo de **Culling de Sombras de Luzes Locais**, otimizando o pipeline de renderização em interiores escuros e corredores densos (Interchange, Dorms de Customs, Resort de Shoreline e The Lab).

Lâmpadas do tipo pontual (*Point Lights*) com sombras ativas no Unity exigem 6 passadas de desenho na GPU por luz (uma para cada face do cubemap de sombra). Ao desativar ou simplificar a projeção de sombras em lâmpadas fracas de teto e arandelas decorativas, a GPU ganha alívio imediato de amostragem de sombra, mantendo o cômodo iluminado e com 100% de sua coloração original. O módulo conta com blindagem ativa contra vazamento de luz (*light leaking*) através de paredes e preservação integral de lanternas táticas em armas.

---

## 2. Componentes e Funcionalidades Implementadas

### 2.1. Gerenciador de Sombras Locais (`Core/LocalLightShadowManager.cs`)
- **Varredura Única no Carregamento:**
  - Varre todas as instâncias de `UnityEngine.Light` da cena após o carregamento da raid.
  - Ignora o sol (`LightType.Directional` / `TOD_Sky`), luzes efêmeras de tiros/flares e qualquer luz vinculada a jogadores ou bots (`TacticalComboVisualController` e `Player`).
- **Tripla Modalidade de Otimização no F12:**
  - **`DisableWeakShadowsOnly` (Padrão):** Oculta sombras (`LightShadows.None`) apenas para luzes com intensidade $\le 1.2$ e alcance $\le 6.0$m. A atenuação quadrática da Unity assegura que não há vazamento visível de luz para quartos vizinhos.
  - **`DowngradeSoftToHard`:** Reduz sombras suaves para sombras duras nas luzes secundárias, reduzindo o custo de amostragem de sombra na GPU sem desligar a oclusão geométrica de paredes.
  - **`DisableAllSecondaryShadows`:** Desliga sombras de todas as point e spot lights estáticas da cena para ganho máximo de FPS em máquinas de entrada.
- **Restauração Perfeita:**
  - Cacheia os valores nativos em `Dictionary<Light, LightShadows> _originalShadowStates`.
  - Restaura deterministamente todas as luzes para o estado original ao sair da raid (`Cleanup()`) ou ao desativar a funcionalidade no F12.
- **Respeito a Lâmpadas Quebradas:**
  - O mod manipula estritamente `light.shadows`. Jamais altera `light.enabled` ou `light.intensity`, respeitando interruptores da BSG e lâmpadas destruídas por tiros (`LampController`).

### 2.2. Menu BepInEx F12 (`Configuration/ModConfig.cs` e `PROPRIEDADES.md`)
Criada a seção **`10. Otimização de Iluminação`**:
- `EnableLocalLightShadowCulling` (`bool`, padrão `true`): Ativa ou desativa a otimização de sombras secundárias.
- `ShadowOptimizationMode` (`enum`, padrão `DisableWeakShadowsOnly`): Modo de otimização de sombras.
- `WeakLightIntensityThreshold` (`float`, padrão `1.2`, faixa `0.2` a `5.0`): Limiar máximo de intensidade.
- `WeakLightRangeThreshold` (`float`, padrão `6.0m`, faixa `1.0m` a `20.0m`): Limiar máximo de alcance.

### 2.3. Telemetria On-Screen (`Core/PerformanceManager.cs`)
- Adicionada a contagem de luzes otimizadas em tempo real no overlay de debug (F11):
  `Luzes Otimizadas: X/Y`.

---

## 3. Arquivos Criados e Modificados

| Arquivo | Ação | Descrição |
|---|---|---|
| `modded/Core/LocalLightShadowManager.cs` | CRIAR | Componente MonoBehaviour de catálogo, otimização e restauração de luzes locais. |
| `modded/Configuration/ModConfig.cs` | MODIFICAR | Inclusão das propriedades da seção 10, enum e evento de configuração. |
| `modded/Core/PerformanceManager.cs` | MODIFICAR | Integração do `LocalLightShadowManager` no ciclo de vida e no overlay OnGUI. |
| `modded/Plugin.cs` | MODIFICAR | Bump de versão SemVer para `0.3.4`. |
| `modded/TRL-CoreSight.csproj` | MODIFICAR | Inclusão do arquivo compilado e bump SemVer para `0.3.4`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.4`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentação em pt-BR da seção 10 de F12. |

---

## 4. Verificação e Compilação

- **Compilação:** `dotnet build mods/TRL-CoreSight/modded/TRL-CoreSight.csproj -c Release`
- **Resultado:** 0 Erros, 0 Avisos. Tempo decorrido: 1.15s.
- **Binário isolado gerado:** `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (59.904 bytes).
