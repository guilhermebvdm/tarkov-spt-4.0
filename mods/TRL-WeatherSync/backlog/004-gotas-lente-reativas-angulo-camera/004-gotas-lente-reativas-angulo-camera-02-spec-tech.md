# 004 — Gotas na Lente Reativas ao Ângulo da Câmera · Spec Técnica

**Mod:** TRL-WeatherSync  
**Spec funcional:** [004-gotas-lente-reativas-angulo-camera-01-spec.md](004-gotas-lente-reativas-angulo-camera-01-spec.md)  
**Criado:** 2026-09-12  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

O sistema de geração e simulação de gotas na tela/lente da câmera no EFT é gerenciado pela classe [`GClass986`](../../../../references/eft-decompiled/Assembly-CSharp/GClass986.cs#L7) (instanciada em [`RainScreenDrops.Init()`](../../../../references/eft-decompiled/Assembly-CSharp/RainScreenDrops.cs#L204)).

Em [`GClass986.Update(float dt)`](../../../../references/eft-decompiled/Assembly-CSharp/GClass986.cs#L114-L138), a cada frame, o jogo executa a seguinte rotina:
```csharp
public void Update(float dt)
{
    bool isCameraUnderRain = RainController.IsCameraUnderRain;
    if (Intensity > 0.15f && isCameraUnderRain)
    {
        float num = Mathf.Clamp01((Vector3.Dot(Transform_0.forward, Vector3.up) + 1f) * 0.5f * 1.2f);
        Float_6 += dt * UnityEngine.Random.Range(0.7f, 3f) * num;
        if (Float_6 > Float_1)
        {
            int num2 = Mathf.CeilToInt((float)UnityEngine.Random.Range(1, Int_0) * num * Intensity);
            for (int i = 0; i < num2; i++)
            {
                if (Queue_0.Count > 0 && num > 0.2f)
                {
                    method_1(num);
                }
            }
            Float_6 = 0f;
        }
    }
    if (List_0.Count != 0)
    {
        method_0(dt);
    }
}
```

### Problemas do código vanilla identificados:
1. `(Vector3.Dot(Transform_0.forward, Vector3.up) + 1f) * 0.5f * 1.2f` comprime o valor entre `0.6` (horizonte) e `1.0` (céu), gerando quase a mesma quantidade de chuva olhando para a frente ou para cima.
2. Em chuvas leves (onde `Intensity` é ~0.2 a 0.5), `num2` é truncado em no máximo 1 gota a cada vários segundos.
3. As gotas ativas possuem tempo de vida de 25 segundos com óculos (`Float_9` configurado por `Float_2` em `ChangeGlassState`), ocupando as 32 posições do pool `Queue_0` e travando o surgimento de novas gotas por quase meio minuto.
4. Ao olhar para o chão, `method_0(dt)` continua renderizando as gotas antigas sem qualquer aceleração de secagem.

### Abordagem adotada:
Implementar um patch `[PatchPrefix]` em `GClass986.Update(float dt)` via `ModulePatch`:
- Se a configuração `EnableLensDropsTuning.Value` for `false`, o Prefix retorna `true` (deixa o código nativo do jogo executar integralmente).
- Se `true`, intercepta a execução:
  1. Calcula `pitchDot = Vector3.Dot(__instance.Transform_0.forward, Vector3.up)`.
  2. **Ao olhar para cima (`pitchDot > 0f`):** Interpola suavemente a intensidade e o multiplicador de gotas entre a taxa frontal e o multiplicador de céu (`LookUpMultiplier.Value`), aumentando tanto a velocidade de acúmulo do timer (`Float_6`) quanto a quantidade de gotas geradas por rajada (`num2`), permitindo até 8–12 gotas simultâneas.
  3. **Ao olhar para a frente (`pitchDot ~ 0f`):** Aplica o `ForwardRateMultiplier.Value` (garantindo que mesmo com intensidade 0.2 de garoa, `num` permaneça em um piso de 0.6 a 0.9 com pelo menos 1 a 2 gotas por ciclo).
  4. **Ao olhar para baixo (`pitchDot < -0.1f`):** Zera `num = 0`, impedindo totalmente a geração de novos pingos. Caso `LookDownDrainEffect.Value` esteja ativado, acelera a atualização das gotas ativas em `List_0` (passando `dt * 3.5f` para as gotas existentes), simulando a água escorrendo/secando da viseira rapidamente.
  5. **Controle de vida útil máxima (`MaxDropLifetimeSeconds`):** Assegura que o tempo de permanência das gotas na tela não exceda o valor configurado (padrão 8.0s), permitindo um fluxo renovado e contínuo de pingos sem estagnação.
  6. Invoca o desenho das gotas ativas (`__instance.method_0(dt)`) e retorna `false` (suprime o método vanilla).

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`GClass986.Update(float dt)`](../../../../references/eft-decompiled/Assembly-CSharp/GClass986.cs#L114) | Prefix (`[PatchPrefix]`) | Substitui o cálculo da taxa de geração e drenagem de gotas na lente da câmera conforme o ângulo vertical. |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Lens Drops` | `Enable Lens Drops Tuning` | bool | `true` | — | — | Ativa a calibragem dinâmica de gotas de chuva na lente reativas ao ângulo da câmera. Se desativado, roda o padrão do jogo. |
| `Lens Drops` | `Look Up Multiplier` | float | `2.5` | 1.0 a 5.0 | — | Multiplicador de quantidade e frequência de gotas ao olhar para cima (para o céu). |
| `Lens Drops` | `Forward Rate Multiplier` | float | `1.5` | 0.5 a 3.0 | — | Multiplicador base da taxa de gotas ao olhar para a frente/horizonte (reforça a presença de pingos em chuvas fracas). |
| `Lens Drops` | `Look Down Drain Effect` | bool | `true` | — | — | Faz as gotas da lente secarem/escorrerem rapidamente ao inclinar a cabeça para o chão (simula a proteção do capacete e chuva na nuca). |
| `Lens Drops` | `Max Drop Lifetime Seconds` | float | `8.0` | 2.0 a 25.0 | — | Tempo máximo de vida de cada gota na lente (segundos). Valores menores deixam o ciclo de gotas mais contínuo e menos estático (vanilla é 25s). |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/Patches/CameraLensRainDropsPatch.cs` | CRIAR | Patch `CameraLensRainDropsPatch : ModulePatch` com Prefix em `GClass986.Update`. |
| `modded/Client/Plugin.cs` | MODIFICAR | Bump de versão para `1.3.0`, registrar novas `ConfigEntry` da seção `Lens Drops` e habilitar o patch no `Awake()`. |
| `modded/Client/TRL-WeatherSync.csproj` | MODIFICAR | Sincronizar `<Version>` para `1.3.0`. |
| `PROPRIEDADES.md` | MODIFICAR | Adicionar seção `Lens Drops` e atualizar versão para `v1.3.0`. |

## 5. Stubs de código

```csharp
// modded/Client/Patches/CameraLensRainDropsPatch.cs
using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TRLWeatherSync.Patches;

public class CameraLensRainDropsPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;
    private static bool _hasLoggedError;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/GClass986.cs:114
        return AccessTools.Method(typeof(GClass986), nameof(GClass986.Update));
    }

    [PatchPrefix]
    private static bool Prefix(GClass986 __instance, float dt)
    {
        if (!TRLWeatherSyncPlugin.EnableLensDropsTuning.Value)
        {
            return true;
        }

        try
        {
            bool isCameraUnderRain = RainController.IsCameraUnderRain;
            float intensity = __instance.Intensity;

            if (intensity > 0.05f && isCameraUnderRain && __instance.Transform_0 != null)
            {
                // pitchDot: -1 (olhando 100% chão), 0 (horizonte), +1 (olhando 100% céu)
                float pitchDot = Vector3.Dot(__instance.Transform_0.forward, Vector3.up);

                float forwardMultiplier = TRLWeatherSyncPlugin.LensRainDropsForwardRate.Value;
                float skyMultiplier = TRLWeatherSyncPlugin.LensRainDropsLookUpMultiplier.Value;

                float num;
                if (pitchDot > 0f)
                {
                    // Olhando para cima: interpola do multiplicador de horizonte até o multiplicador de céu
                    num = Mathf.Lerp(0.6f * forwardMultiplier, skyMultiplier, pitchDot);
                }
                else if (pitchDot > -0.15f)
                {
                    // Transição suave ao redor do horizonte
                    float t = Mathf.InverseLerp(-0.15f, 0f, pitchDot);
                    num = Mathf.Lerp(0f, 0.6f * forwardMultiplier, t);
                }
                else
                {
                    // Olhando para baixo: zera completamente novas gotas
                    num = 0f;
                }

                // Acumula tempo de spawn
                __instance.Float_6 += dt * UnityEngine.Random.Range(0.7f, 3f) * num;

                if (__instance.Float_6 > __instance.Float_1 && num > 0.1f)
                {
                    // Permite mais gotas por ciclo quando olhando para cima
                    int maxAllowed = Mathf.Clamp(Mathf.RoundToInt(__instance.Int_0 * (pitchDot > 0f ? (1f + pitchDot * 1.5f) : 1f)), 1, 16);
                    int numDrops = Mathf.CeilToInt(UnityEngine.Random.Range(1, maxAllowed + 1) * num * Mathf.Max(intensity, 0.35f));

                    for (int i = 0; i < numDrops; i++)
                    {
                        if (__instance.Queue_0.Count > 0)
                        {
                            __instance.method_1(Mathf.Clamp01(num));

                            // Limita o tempo de vida da gota recém-criada
                            if (__instance.List_0.Count > 0)
                            {
                                var lastDrop = __instance.List_0[__instance.List_0.Count - 1];
                                float maxLifetime = TRLWeatherSyncPlugin.LensRainDropsMaxLifetime.Value;
                                if (lastDrop.Lifetime > maxLifetime)
                                {
                                    lastDrop.Lifetime = maxLifetime;
                                }
                            }
                        }
                    }
                    __instance.Float_6 = 0f;
                }
            }

            // Atualização e renderização das gotas existentes
            if (__instance.List_0.Count != 0)
            {
                float updateDt = dt;
                // Secagem acelerada se olhando para baixo
                if (TRLWeatherSyncPlugin.LensRainDropsLookDownDrain.Value && __instance.Transform_0 != null)
                {
                    float pitchDot = Vector3.Dot(__instance.Transform_0.forward, Vector3.up);
                    if (pitchDot < -0.2f)
                    {
                        // Acelera em 3.5x o tempo de vida restante das gotas ativas
                        updateDt = dt * 3.5f;
                    }
                }

                __instance.method_0(updateDt);
            }

            return false;
        }
        catch (Exception ex)
        {
            if (!_hasLoggedError)
            {
                _hasLoggedError = true;
                Log.LogError($"[TRL-WeatherSync] Falha no patch de gotas da lente (GClass986.Update): {ex}");
            }
            return true; // Fallback vanilla se ocorrer exceção
        }
    }
}
```

## 6. Fluxo de dados

```
[1] Jogador ajusta parâmetros no menu F12 (Look Up Multiplier, Forward Rate, etc.)
  → BepInEx atualiza os valores na memória e persiste no .cfg.
[2] RainScreenDrops.Update() [RainScreenDrops.cs:235] roda a cada frame
  → Chama GClass986.Update(dt) [GClass986.cs:114].
[3] CameraLensRainDropsPatch.Prefix intercepta a chamada
  → Se desativado, retorna true e executa rotina vanilla.
  → Se ativado:
      a) Avalia pitchDot = Dot(Camera.forward, Vector3.up).
      b) Se pitchDot > 0: calcula taxa reforçada de céu (spawn massivo de gotas).
      c) Se pitchDot ~ 0: aplica taxa base reforçada de horizonte (gotas visíveis em garoa).
      d) Se pitchDot < -0.15: zera novos pingos e avança dt * 3.5x para secar gotas existentes.
      e) Limita Lifetime das gotas para não exceder MaxDropLifetimeSeconds.
      f) Executa method_0() com dt ajustado para desenhar as partículas.
      g) Retorna false (suprime o método original).
[4] Imagem é enviada para o shader de pós-processamento da câmera
  → Gotas aparecem ou secam de forma fluida e reativa à visão do jogador.
```

## 7. Riscos e dependências

- **Compatibilidade com SPT / EFT:** `GClass986` é instanciada apenas quando o componente `RainScreenDrops` está ativo na câmera. Se a câmera não tiver o componente ou estiver em tempo seco, a rotina não aloca recursos.
- **Performance por frame:** O Prefix executa cálculos simples de produto escalar e aritmética de pontos flutuantes (`float`), sem instanciar novos objetos no heap (`0 B GC Alloc`).
- **Resiliência contra falhas (Airbag):** Caso ocorra qualquer exceção inesperada, o `try/catch` loga o erro uma única vez e retorna `true`, permitindo que o jogo utilize o comportamento vanilla imediatamente sem travar.

## 8. Checklist de implementação

- [ ] Criar `mods/TRL-WeatherSync/modded/Client/Patches/CameraLensRainDropsPatch.cs`.
- [ ] Atualizar `Plugin.cs`: registrar as 5 `ConfigEntry`, habilitar o patch e bump de versão para `1.3.0`.
- [ ] Atualizar `TRL-WeatherSync.csproj`: bump de versão para `1.3.0`.
- [ ] Atualizar `PROPRIEDADES.md` com as novas propriedades.
- [ ] Criar `004-gotas-lente-reativas-angulo-camera-03-spec-tech-review-01.md`.
- [ ] Compilar localmente com `dotnet build` e verificar 0 erros / 0 avisos.
- [ ] Criar `004-gotas-lente-reativas-angulo-camera-04-code-review-01.md` e `004-gotas-lente-reativas-angulo-camera-05-asbuild.md`.
- [ ] Atualizar `mod-backlog.md` e `sessions.md`.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Patch stateless registrado no `Awake()` do plugin; não cria listeners nem aloca estado próprio por raid. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Opera sobre a câmera local do jogador renderizada pelo `RainScreenDrops`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `GClass986.Update(float dt)` é público e não-virtual ([`GClass986.cs:114`](../../../../references/eft-decompiled/Assembly-CSharp/GClass986.cs#L114)). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Utiliza os métodos nativos `__instance.method_1()` e `__instance.method_0()` para gerenciar as partículas de gota. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Configurações persistidas nativamente pelo BepInEx. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Tooltips completos em pt-BR com valores padrão, faixas e toggle de fallback vanilla. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` — AP-07 | N/A | Prefix retorna `false` ou `true`, sem chamar `Update` recursivamente. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Sem caches persistentes entre telas. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Reconfirmado em `references/eft-decompiled/Assembly-CSharp/GClass986.cs:114`. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não utiliza skills do EFT. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Modificação 100% visual e local. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Spec técnica elaborada para o item 004. |
