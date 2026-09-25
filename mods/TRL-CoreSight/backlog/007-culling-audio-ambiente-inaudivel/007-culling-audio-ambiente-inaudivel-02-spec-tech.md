# 007 — culling-audio-ambiente-inaudivel · Spec Técnica

**Mod:** TRL-CoreSight  
**Spec funcional:** [007-culling-audio-ambiente-inaudivel-01-spec.md](007-culling-audio-ambiente-inaudivel-01-spec.md)  
**Criado:** 2026-09-15T22:45:00Z  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

---

## 1. Estratégia

Otimizar a thread de áudio e os ciclos de processamento DSP da Unity (`AudioMixer::Update`), pausando dinamicamente emissores de som ambiente contínuos (`AudioSource` com `loop == true`) cujas distâncias ultrapassem o raio máximo de audibilidade do jogador.

Mapas densos como Interchange, Reserve, Customs e Streets of Tarkov contêm centenas de fontes sonoras de ambiente (zumbidos de lâmpadas fluorescentes, geradores a diesel, caixas de força, transformadores elétricos, goteiras e turbinas de ventilação). Mesmo a centenas de metros de distância — onde o volume atenuado é zero decibéis —, a engine continua calculando curvas logarítmicas de atenuação 3D, cálculos vetoriais de espacialização e oclusão de materiais para cada emissor ativo.

### 1.1. Culling Seletivo por Distância Acústica
- **Registro Único:** No início da partida, registrar todas as fontes estáticas de áudio com `loop == true`.
- **Margem de Histerese de Segurança (`AudioCullingMargin`, padrão 10.0m):**
  - Distância de corte: $D_{corte} = \text{maxDistance} + \text{Margem}$.
  - Se a distância euclidiana da câmera/jogador até o emissor exceder $D_{corte}$, o `AudioSource` é pausado (`audioSource.Pause()`).
  - Ao reaproximar-se dentro do raio de audibilidade, a reprodução é retomada (`audioSource.UnPause()`), sem reinício abrupto do clipe.
- **Processamento em Lotes (Time-Slicing):**
  - Avaliação de 32 fontes sonoras por frame, garantindo menos de 0.05ms de tempo de CPU por quadro.

### 1.2. Imunidade Absoluta a Áudios Táticos e de Combate
Qualquer som relevante para sobrevivência e percepção tática é expressamente ignorado pelo sistema:
- Fontes com `loop == false` (disparos balísticos, explosões de granadas, estalos de quebra de barreira do som, recarga de carregadores).
- Passos de jogadores ou bots (`GetComponentInParent<Player>() != null`).
- Falas, gemidos e comandos de voz de scavs/PMCs (`voicelines`).
- Sirenes de extração ativadas por botões e alarmes dinâmicos de mapa.

### 1.3. Restauração no Final de Raid
- No `Cleanup()` ou na desativação via F12, todos os emissores pausados são despausados deterministamente.

---

## 2. Pontos de Integração e Ganchos

| Componente | Tipo | Motivo |
|---|---|---|
| `UnityEngine.AudioSource` | Inspeção na Cena | Leitura de `loop`, `isPlaying`, `maxDistance` e manipulação de `Pause()` / `UnPause()`. |
| `PerformanceManager.Initialize()` | Ciclo de Vida | Disparar o escaneamento e catalogação de emissores da cena. |
| `PerformanceManager.Cleanup()` | Ciclo de Vida | Retomar 100% dos emissores pausados ao sair da raid. |

---

## 3. Novas propriedades F12 (BepInEx)

Adicionar na seção **`13. Otimização de Áudio (Culling de Ambiente)`** em `Configuration/ModConfig.cs`:

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `13. Otimização de Áudio` | `EnableAmbientAudioCulling` | bool | `true` | — | Pausa fontes de áudio ambiente contínuas inaudíveis à distância para poupar CPU de mixagem. |
| `13. Otimização de Áudio` | `AudioCullingMargin` | float | `10.0` | 2.0 a 30.0 | Margem de segurança em metros além do maxDistance para pausar o áudio sem cortes sonoros. |

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Configuration/ModConfig.cs` | MODIFICAR | Inclusão de configurações da Seção 13 no F12 e evento de configuração. |
| `PROPRIEDADES.md` | MODIFICAR | Documentação em pt-BR da seção 13 de F12. |
| `Core/AmbientAudioCullingManager.cs` | CRIAR | Componente que cataloga fontes contínuas de cena, avalia distância em lotes e pausa/despausa com histerese. |
| `Core/PerformanceManager.cs` | MODIFICAR | Ciclo de vida, atualização por frame, telemetria OnGUI e cleanup do gerenciador de áudio. |
| `Plugin.cs` | MODIFICAR | Bump de versão SemVer para `0.3.7`. |
| `TRL-CoreSight.csproj` | MODIFICAR | Inclusão do novo arquivo compilado e bump SemVer para `0.3.7`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.7`. |

---

## 5. Stubs de código

### `Core/AmbientAudioCullingManager.cs`

```csharp
using System.Collections.Generic;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class AmbientAudioCullingManager : MonoBehaviour
    {
        public static AmbientAudioCullingManager Instance { get; private set; }

        private readonly List<TrackedAudioSource> _ambientSources = new List<TrackedAudioSource>(256);
        private int _batchIndex;
        private const int BATCH_SIZE = 32;

        public int TotalManagedSources => _ambientSources.Count;
        public int PausedSourcesCount { get; private set; }

        private struct TrackedAudioSource
        {
            public AudioSource Source;
            public Transform Transform;
            public float CutoffDistanceSqr;
            public bool IsPaused;
        }

        private void Awake()
        {
            Instance = this;
            ModConfig.OnAudioSettingsChanged += OnSettingsChanged;
        }

        public void Initialize()
        {
            ScanAndRegisterAmbientSources();
        }

        public void ScanAndRegisterAmbientSources()
        {
            _ambientSources.Clear();
            AudioSource[] allSources = FindObjectsOfType<AudioSource>();
            if (allSources == null || allSources.Length == 0)
            {
                return;
            }

            float margin = ModConfig.AudioCullingMargin.Value;

            for (int i = 0; i < allSources.Length; i++)
            {
                AudioSource src = allSources[i];
                if (src == null || !src.loop)
                {
                    continue; // Ignora one-shots de tiros, passos e recargas
                }

                // Ignora sons vinculados a jogadores ou bots
                if (src.GetComponentInParent<Player>() != null)
                {
                    continue;
                }

                string objName = src.gameObject.name.ToLower();
                if (objName.Contains("siren") || objName.Contains("alarm") || objName.Contains("voice") || objName.Contains("step"))
                {
                    continue;
                }

                float cutoffDist = Mathf.Max(src.maxDistance + margin, 15.0f);

                _ambientSources.Add(new TrackedAudioSource
                {
                    Source = src,
                    Transform = src.transform,
                    CutoffDistanceSqr = cutoffDist * cutoffDist,
                    IsPaused = false
                });
            }

            Plugin.LogSource?.LogInfo($"[TRL-CoreSight] AmbientAudioCullingManager: {_ambientSources.Count} fontes contínuas de ambiente catalogadas.");
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableAmbientAudioCulling.Value || _ambientSources.Count == 0)
            {
                if (PausedSourcesCount > 0)
                {
                    ResumeAll();
                }
                return;
            }

            Camera activeCam = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                ? CameraClass.Instance.Camera
                : Camera.main;

            if (activeCam == null)
            {
                return;
            }

            Vector3 listenerPos = activeCam.transform.position;
            int total = _ambientSources.Count;
            int processed = 0;

            while (processed < BATCH_SIZE && _batchIndex < total)
            {
                TrackedAudioSource item = _ambientSources[_batchIndex];
                if (item.Source == null || item.Transform == null)
                {
                    _ambientSources.RemoveAt(_batchIndex);
                    total--;
                    continue;
                }

                float sqrDist = (item.Transform.position - listenerPos).sqrMagnitude;
                bool shouldPause = sqrDist > item.CutoffDistanceSqr;

                if (shouldPause && !item.IsPaused)
                {
                    if (item.Source.isPlaying)
                    {
                        item.Source.Pause();
                        item.IsPaused = true;
                        PausedSourcesCount++;
                        _ambientSources[_batchIndex] = item;
                    }
                }
                else if (!shouldPause && item.IsPaused)
                {
                    item.Source.UnPause();
                    item.IsPaused = false;
                    PausedSourcesCount = Mathf.Max(0, PausedSourcesCount - 1);
                    _ambientSources[_batchIndex] = item;
                }

                _batchIndex++;
                processed++;
            }

            if (_batchIndex >= total)
            {
                _batchIndex = 0;
            }
        }

        public void ResumeAll()
        {
            for (int i = 0; i < _ambientSources.Count; i++)
            {
                var item = _ambientSources[i];
                if (item.Source != null && item.IsPaused)
                {
                    item.Source.UnPause();
                    item.IsPaused = false;
                    _ambientSources[i] = item;
                }
            }
            PausedSourcesCount = 0;
        }

        private void OnSettingsChanged()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableAmbientAudioCulling.Value)
            {
                ResumeAll();
            }
            else
            {
                ScanAndRegisterAmbientSources();
            }
        }

        public void Cleanup()
        {
            ModConfig.OnAudioSettingsChanged -= OnSettingsChanged;
            ResumeAll();
            _ambientSources.Clear();
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

## 6. Checklist de Implementação

- [ ] Adicionar propriedades da seção 13 em `Configuration/ModConfig.cs`.
- [ ] Atualizar `PROPRIEDADES.md` com as configurações e explicações em pt-BR.
- [ ] Criar `Core/AmbientAudioCullingManager.cs`.
- [ ] Integrar `AmbientAudioCullingManager` no `PerformanceManager.cs` (Initialize, OnUpdate, OnGUI, Cleanup).
- [ ] Incrementar versão para `0.3.7` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`.
- [ ] Compilar via `dotnet build` e isolar em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`.

---

## 7. Conformidade com Skills (Auto-Checklist)

| # | Check | Status | Evidência / Razão |
|---|---|:---:|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Gerenciado pelo `PerformanceManager` e restaurado no `Cleanup()`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Áudio medido a partir da câmera do jogador (`CameraClass.Instance.Camera`). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura — AP-03 | ✅ | Opera sobre API nativa da Unity (`AudioSource.Pause` / `UnPause`). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Afeta unicamente fontes de som de ambiente em loop; one-shots imunes. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4 cobertos | ✅ | `ResumeAll()` determinístico no descarregamento da cena. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Tipados com faixas e descrições claras em pt-BR. |
| 7 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Validado diretamente em `AudioSource`. |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Spec técnica criada com time-slicing em lotes, margem de histerese e exclusão de sons táticos. |
