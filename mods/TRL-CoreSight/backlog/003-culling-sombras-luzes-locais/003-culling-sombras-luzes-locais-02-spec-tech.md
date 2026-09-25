# 003 — culling-sombras-luzes-locais · Spec Técnica

**Mod:** TRL-CoreSight  
**Spec funcional:** [003-culling-sombras-luzes-locais-01-spec.md](003-culling-sombras-luzes-locais-01-spec.md)  
**Criado:** 2026-09-15T22:30:00Z  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

---

## 1. Estratégia

Otimizar o pipeline gráfico e aliviar severamente a saturação de GPU (*pixel shaders* e passadas de cubemap de sombras) em interiores fechados com iluminação densa (como dormitórios de Customs, subsolo e lojas do Interchange, Resort de Shoreline e The Lab).

Fontes de luz do tipo `LightType.Point` com sombras ativas (`LightShadows.Hard` ou `LightShadows.Soft`) geram **6 passadas adicionais de desenho na GPU** para renderizar a geometria ao redor em um mapa de sombras cúbico (*cubemap*). Em corredores com dezenas de lâmpadas fracas de teto ou luminárias secundárias, esse custo acumula dezenas de passadas repetitivas por quadro.

### Mitigação Ativa de Light Leaking (Vazamento de Luz Através de Paredes)
Desativar sombras sem critério em luzes fortes ou de longo alcance faz com que a luz atravesse paredes sólidas e ilumine cômodos vizinhos escuros. Para blindar o mod contra esse efeito colateral:
1. **Modo Padrão `DisableWeakShadowsOnly`:** Desativa sombras apenas para luzes com intensidade $\le 1.2$ **E** alcance $\le 6.0$ metros. A atenuação quadrática da Unity assegura que a intensidade da luz já é insignificante ao atingir superfícies adjacentes.
2. **Modo `DowngradeSoftToHard`:** Para jogadores que desejam proteção total contra qualquer vazamento de luz, converte as sombras suaves (`SoftShadows`) para sombras duras (`HardShadows`), eliminando a dispendiosa amostragem de filtro PCF da GPU sem desligar a oclusão geométrica de paredes.
3. **Modo `DisableAllSecondaryShadows`:** Desliga sombras de todas as point/spot lights estáticas para jogadores com GPUs de entrada em busca do teto máximo de FPS.

### Exclusão Estrita de Luzes Táticas e Gameplay
Lâmpadas móveis montadas em armas, capacetes ou pertencentes a jogadores/bots nunca são alteradas:
- Componentes associados a [`TacticalComboVisualController`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/TacticalComboVisualController.cs#L8).
- Componentes que possuem [`Player`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) em sua hierarquia de pais (`GetComponentInParent<Player>()`).
- Luzes direcionais solares (`LightType.Directional` / `TOD_Sky`).
- Luzes efêmeras de disparos e explosões (`MuzzleJet`, `Flame`, etc.).

---

## 2. Pontos de Integração e Ganchos

| Componente | Tipo | Motivo |
|---|---|---|
| `UnityEngine.Light` | Varredura na Cena | Leitura de `type`, `shadows`, `intensity`, `range` de luzes estáticas no carregamento da partida. |
| `PerformanceManager.Initialize()` | Ciclo de Vida | Disparar o escaneamento inicial das luzes após o carregamento completo do mapa. |
| `PerformanceManager.Cleanup()` | Ciclo de Vida | Restaurar 100% dos estados originais de `LightShadows` ao sair da raid. |
| `TacticalComboVisualController` | Exclusão | Garantir que lanternas táticas e feixes de armas não sofram alteração. |

---

## 3. Novas propriedades F12 (BepInEx)

Adicionar na seção **`10. Otimização de Iluminação (Sombras Secundárias)`** em `Configuration/ModConfig.cs`:

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `10. Otimização de Iluminação` | `EnableLocalLightShadowCulling` | bool | `true` | — | Otimiza o cálculo de sombras de lâmpadas secundárias em corredores e interiores para aliviar a GPU. |
| `10. Otimização de Iluminação` | `ShadowOptimizationMode` | enum | `DisableWeakShadowsOnly` | `DisableWeakShadowsOnly`, `DowngradeSoftToHard`, `DisableAllSecondaryShadows` | Modo de otimização de sombras (WeakShadows protege contra vazamento de luz através de paredes). |
| `10. Otimização de Iluminação` | `WeakLightIntensityThreshold` | float | `1.2` | 0.2 a 5.0 | Intensidade máxima da luz para ser considerada fraca no modo DisableWeakShadowsOnly. |
| `10. Otimização de Iluminação` | `WeakLightRangeThreshold` | float | `6.0` | 1.0 a 20.0 | Alcance máximo em metros da luz para ser considerada secundária no modo DisableWeakShadowsOnly. |

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Configuration/ModConfig.cs` | MODIFICAR | Registrar opções na seção 10 de F12 e enum `LightShadowOptimizationMode`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar as propriedades da seção 10 em pt-BR. |
| `Core/LocalLightShadowManager.cs` | CRIAR | Componente MonoBehaviour que varre, classifica, otimiza e restaura lâmpadas secundárias da cena. |
| `Core/PerformanceManager.cs` | MODIFICAR | Instanciação, atualização periódica e restauração segura do `LocalLightShadowManager`. |
| `TRL-CoreSight.csproj` | MODIFICAR | Incluir `Core/LocalLightShadowManager.cs` e bump de versão para `0.3.4`. |
| `Plugin.cs` | MODIFICAR | Bump de versão para `0.3.4`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.4`. |

---

## 5. Stubs de código

### `Core/LocalLightShadowManager.cs`

```csharp
using System.Collections.Generic;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class LocalLightShadowManager : MonoBehaviour
    {
        public static LocalLightShadowManager Instance { get; private set; }

        private readonly Dictionary<Light, LightShadows> _originalShadowStates = new Dictionary<Light, LightShadows>(512);
        private bool _isOptimized;
        private LightShadowOptimizationMode _lastMode;
        private float _lastIntensityThreshold;
        private float _lastRangeThreshold;

        public int TotalManagedLights => _originalShadowStates.Count;
        public int OptimizedLightsCount { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            ScanAndCacheLights();
            ApplyOptimization();
        }

        public void ScanAndCacheLights()
        {
            _originalShadowStates.Clear();
            Light[] allLights = FindObjectsOfType<Light>();
            if (allLights == null || allLights.Length == 0)
            {
                return;
            }

            for (int i = 0; i < allLights.Length; i++)
            {
                Light light = allLights[i];
                if (light == null || !light.enabled)
                {
                    continue;
                }

                // 1. Ignorar sol e iluminação global
                if (light.type == LightType.Directional)
                {
                    continue;
                }

                // 2. Ignorar se não tiver projeção de sombra original
                if (light.shadows == LightShadows.None)
                {
                    continue;
                }

                // 3. Ignorar lanternas táticas e acessórios de armas de players/bots
                if (light.GetComponentInParent<TacticalComboVisualController>() != null ||
                    light.GetComponentInParent<Player>() != null)
                {
                    continue;
                }

                string lightName = light.gameObject.name.ToLower();
                if (lightName.Contains("muzzle") || lightName.Contains("flare") || lightName.Contains("flash"))
                {
                    continue;
                }

                _originalShadowStates[light] = light.shadows;
            }
        }

        public void ApplyOptimization()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableLocalLightShadowCulling.Value)
            {
                RestoreOriginalShadows();
                return;
            }

            var mode = ModConfig.ShadowOptimizationMode.Value;
            float maxIntensity = ModConfig.WeakLightIntensityThreshold.Value;
            float maxRange = ModConfig.WeakLightRangeThreshold.Value;

            int count = 0;
            foreach (var kvp in _originalShadowStates)
            {
                Light light = kvp.Key;
                LightShadows originalShadow = kvp.Value;

                if (light == null)
                {
                    continue;
                }

                switch (mode)
                {
                    case LightShadowOptimizationMode.DisableWeakShadowsOnly:
                        if (light.intensity <= maxIntensity && light.range <= maxRange)
                        {
                            light.shadows = LightShadows.None;
                            count++;
                        }
                        else
                        {
                            light.shadows = originalShadow;
                        }
                        break;

                    case LightShadowOptimizationMode.DowngradeSoftToHard:
                        if (originalShadow == LightShadows.Soft)
                        {
                            light.shadows = LightShadows.Hard;
                            count++;
                        }
                        else
                        {
                            light.shadows = originalShadow;
                        }
                        break;

                    case LightShadowOptimizationMode.DisableAllSecondaryShadows:
                        light.shadows = LightShadows.None;
                        count++;
                        break;
                }
            }

            OptimizedLightsCount = count;
            _isOptimized = true;
            _lastMode = mode;
            _lastIntensityThreshold = maxIntensity;
            _lastRangeThreshold = maxRange;
        }

        public void CheckConfigChanges()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableLocalLightShadowCulling.Value)
            {
                if (_isOptimized)
                {
                    RestoreOriginalShadows();
                }
                return;
            }

            if (!_isOptimized ||
                _lastMode != ModConfig.ShadowOptimizationMode.Value ||
                !Mathf.Approximately(_lastIntensityThreshold, ModConfig.WeakLightIntensityThreshold.Value) ||
                !Mathf.Approximately(_lastRangeThreshold, ModConfig.WeakLightRangeThreshold.Value))
            {
                ApplyOptimization();
            }
        }

        public void RestoreOriginalShadows()
        {
            foreach (var kvp in _originalShadowStates)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.shadows = kvp.Value;
                }
            }

            _isOptimized = false;
            OptimizedLightsCount = 0;
        }

        public void Cleanup()
        {
            RestoreOriginalShadows();
            _originalShadowStates.Clear();
            Instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
```

---

## 6. Riscos e Dependências

- **Light Leaking (Vazamento de Luz):** Controlado via modo padrão `DisableWeakShadowsOnly` e opção alternativa `DowngradeSoftToHard`.
- **Lanternas Táticas:** Totalmente preservadas através da exclusão por `TacticalComboVisualController` e `Player`.
- **Integridade da Cena:** Nenhum GameObject de luz é destruído ou desativado (`light.enabled` permanece `true`), preservando cores, reflexos e intensidade de iluminação do ambiente.

---

## 7. Checklist de Implementação

- [ ] Definir enum `LightShadowOptimizationMode` e adicionar as configurações da seção 10 em `Configuration/ModConfig.cs`.
- [ ] Atualizar `PROPRIEDADES.md` com as novas propriedades e explicações em pt-BR.
- [ ] Criar `Core/LocalLightShadowManager.cs`.
- [ ] Integrar inicialização, verificação periódica e cleanup em `Core/PerformanceManager.cs`.
- [ ] Adicionar status de luzes no HUD de telemetria `OnGUI()`.
- [ ] Incluir `Core/LocalLightShadowManager.cs` em `TRL-CoreSight.csproj`.
- [ ] Incrementar versão para `0.3.4` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`.
- [ ] Compilar a build via `dotnet build` e isolar em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`.

---

## 8. Conformidade com Skills (Auto-Checklist)

| # | Check | Status | Evidência / Razão |
|---|---|:---:|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Gerenciado pelo `PerformanceManager` e restaurado no `Cleanup()`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Luzes de players são excluídas via `GetComponentInParent<Player>()`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura — AP-03 | ✅ | Opera diretamente sobre tipos canônicos de Unity (`UnityEngine.Light`). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Modifica apenas `Light.shadows`, preservando cores, intensidades e objetos da Unity. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4 cobertos | ✅ | `RestoreOriginalShadows()` determinístico no encerramento da partida. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Enum e limites numéricos definidos com clareza. |
| 7 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `TacticalComboVisualController.cs:8` e `Player.cs` reconfirmados. |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Spec técnica criada com tripla modalidade de proteção contra light leaking e isolamento de lanternas táticas. |
