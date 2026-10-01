# 003 — Chuva Fraca Mais Visível (Tamanho Mínimo de Gota) · Spec Técnica

**Mod:** TRL-WeatherSync  
**Spec funcional:** [003-chuva-fraca-visivel-01-spec.md](003-chuva-fraca-visivel-01-spec.md)  
**Criado:** 2026-09-12  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

O alvo de renderização de chuva no cliente EFT é o componente [`RainFallDrops`](../../../../references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs#L4).
Em [`RainFallDrops.Update()`](../../../../references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs#L101), o jogo chama `method_2()` incondicionalmente a cada frame em que `meshRenderer_0 != null`:

```csharp
public void Update()
{
    if (!(meshRenderer_0 == null))
    {
        meshRenderer_0.enabled = Intensity > _intensityThreshold;
        method_2();
    }
}
```

Dentro de [`RainFallDrops.method_2()`](../../../../references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs#L129), o jogo calcula e atribui as propriedades ao material da malha de partículas:

```csharp
public void method_2()
{
    material_0.SetVector(int_2, RainController.FallingVectorV3);
    material_0.SetFloat(int_3, Intensity);
    material_0.SetFloat(int_4, SideSpeed * Mathf.LerpUnclamped(MinMaxSideSpeed.x, MinMaxSideSpeed.y, Intensity));
    material_0.SetFloat(int_5, Mathf.LerpUnclamped(MinMaxDensity.x, MinMaxDensity.y, Intensity));
    float x = (float)Screen.height / (float)Screen.width;
    Vector2 a = Vector2.LerpUnclamped(MinSize, MaxSize, Intensity);
    a = Vector2.Scale(a, new Vector2(x, 1f));
    material_0.SetVector(int_6, a);
    material_0.SetFloat(int_7, MinAmbient + MinAmbientAddition * MinAmbientAdditionCoef);
}
```

Onde [`int_6 = Shader.PropertyToID("_Size")`](../../../../references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs#L66).

### Abordagens avaliadas e justificativa da escolha

1. **Tentativa de alterar os campos `MinSize`/`MaxSize`:**
   - Descartada. `MinSize` e `MaxSize` são campos `[SerializeField] public Vector2` definidos em [`RainFallDrops.cs:17-19`](../../../../references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs#L17). Embora possuam defaults em C#, esses valores frequentemente são sobrescritos pela serialização de prefabs ou cenas da Unity. Tentar mutar esses campos não é determinístico nem garante que o valor final em runtime venha daí.
2. **Postfix em `RainFallDrops.method_2()` (Abordagem adotada):**
   - Um patch `[PatchPostfix]` via `ModulePatch` em `RainFallDrops.method_2()`.
   - O método original executa seu cálculo completo normalmente. Em seguida, o Postfix obtém o vetor atual enviado ao shader via `___material_0.GetVector(_sizePropertyId)` (com `_sizePropertyId = Shader.PropertyToID("_Size")`).
   - Se o valor de `TRLWeatherSyncPlugin.MinRainDropSize.Value` for maior que zero, calcula-se o piso mínimo de largura proporcional ao aspect ratio da tela (`minSize * (Screen.height / Screen.width)`), garantindo coerência de proporção em telas widescreen e ultrawide.
   - Aplica-se `Mathf.Max` nas componentes X e Y em relação ao piso mínimo configurado. Caso o vetor tenha sido ajustado, reaplica-se com `___material_0.SetVector(_sizePropertyId, currentSize)`.
   - **Vantagens:** Não duplica as contas nativas do EFT (`LerpUnclamped`, `FallingVector`, etc.), tem custo de processamento ínfimo, não é afetado por overrides de prefab e garante atualização visual imediata ("ao vivo") a cada frame.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`RainFallDrops.method_2()`](../../../../references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs#L129) | Postfix (`[PatchPostfix]`) | Intercepta o material após `method_2()` e aplica piso mínimo configurável no vetor de tamanho `_Size`. |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Rain` | `Min Rain Drop Size` | float | `0.08` | 0.0 a 0.30 | — | Define o tamanho mínimo visível da gota de chuva (útil para enxergar chuvas fracas de frente). Ajusta ao vivo sem precisar reiniciar a raid. Não altera chuvas fortes caso já ultrapassem este tamanho. Defina 0 para desativar. |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/Patches/RainDropVisibilityPatch.cs` | CRIAR | Patch Harmony `RainDropVisibilityPatch : ModulePatch` mirando `RainFallDrops.method_2`. |
| `modded/Client/Plugin.cs` | MODIFICAR | Registrar `ConfigEntry<float> MinRainDropSize`, habilitar `new RainDropVisibilityPatch().Enable()` e bump de versão para `1.2.0`. |
| `modded/Client/TRL-WeatherSync.csproj` | MODIFICAR | Bump de versão para `1.2.0` em `<Version>`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar a nova propriedade `Min Rain Drop Size` sob a seção `Rain` e atualizar versão no topo. |

## 5. Stubs de código

```csharp
// modded/Client/Patches/RainDropVisibilityPatch.cs
using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TRLWeatherSync.Patches;

public class RainDropVisibilityPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;
    private static readonly int _sizePropertyId = Shader.PropertyToID("_Size");
    private static bool _hasLoggedError;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/RainFallDrops.cs:129
        return AccessTools.Method(typeof(RainFallDrops), nameof(RainFallDrops.method_2));
    }

    [PatchPostfix]
    private static void Postfix(Material ___material_0)
    {
        try
        {
            float minSize = TRLWeatherSyncPlugin.MinRainDropSize.Value;
            if (minSize <= 0f || ___material_0 == null)
            {
                return;
            }

            Vector4 currentSize = ___material_0.GetVector(_sizePropertyId);
            float aspect = (float)Screen.height / (float)Screen.width;
            float minWidth = minSize * aspect;
            float minLength = minSize;

            if (currentSize.x < minWidth || currentSize.y < minLength)
            {
                currentSize.x = Mathf.Max(currentSize.x, minWidth);
                currentSize.y = Mathf.Max(currentSize.y, minLength);
                ___material_0.SetVector(_sizePropertyId, currentSize);
            }
        }
        catch (Exception ex)
        {
            if (!_hasLoggedError)
            {
                _hasLoggedError = true;
                Log.LogError($"[TRL-WeatherSync] Falha ao ajustar tamanho da gota de chuva (RainFallDrops.method_2): {ex}");
            }
        }
    }
}
```

## 6. Fluxo de dados

```
[1] Jogador ajusta "Min Rain Drop Size" no F12
  → BepInEx atualiza TRLWeatherSyncPlugin.MinRainDropSize.Value na memória (e persiste no .cfg).
[2] RainFallDrops.Update() [RainFallDrops.cs:101] roda a cada frame
  → Chama method_2() [RainFallDrops.cs:129].
[3] RainFallDrops.method_2() executa o cálculo vanilla
  → material_0.SetVector(int_6, a) define o vetor de tamanho vanilla no shader.
[4] RainDropVisibilityPatch.Postfix() intercepta a conclusão de method_2()
  → Lê ___material_0.GetVector(_sizePropertyId).
  → Compara contra minSize * aspect (largura) e minSize (comprimento).
  → Se qualquer dimensão for inferior ao piso, aplica Mathf.Max e reatribui via ___material_0.SetVector().
[5] GPU renderiza as partículas de chuva
  → As gotas aparecem com a espessura mínima reforçada imediatamente na tela do jogador.
```

## 7. Riscos e dependências

- **Incerteza sobre campos serializados de Prefabs:** A abordagem de patchear `method_2` como Postfix e ler/escrever diretamente no `material_0` via shader property isola completamente o mod de quaisquer valores estáticos ou modificações de prefab da Unity.
- **Risco de performance por execução em cada frame:** `method_2` é chamado a cada frame enquanto houver chuva. O código do patch executa apenas poucas operações aritméticas locais (`float`, `Mathf.Max`), sem alocações no heap (`0 B GC alloc`). O `try/catch` conta com guarda estática `_hasLoggedError` para evitar spam de console caso ocorra qualquer imprevisto com o material em runtime.
- **Prevalência estética:** O recurso é 100% cosmético. Caso o usuário prefira a estética original da BSG, o valor `0` desativa qualquer intervenção no shader.

## 8. Checklist de implementação

- [ ] Criar `mods/TRL-WeatherSync/modded/Client/Patches/RainDropVisibilityPatch.cs`.
- [ ] Registrar `ConfigEntry<float> MinRainDropSize` e inicializar `RainDropVisibilityPatch` no `Awake()` de `Plugin.cs`.
- [ ] Atualizar versão para `1.2.0` no `[BepInPlugin]` em `Plugin.cs` e no `TRL-WeatherSync.csproj`.
- [ ] Atualizar `PROPRIEDADES.md` com a nova propriedade e versão.
- [ ] Criar `003-chuva-fraca-visivel-03-spec-tech-review-01.md`.
- [ ] Compilar localmente com `dotnet build` e validar 0 erros / 0 avisos.
- [ ] Criar `003-chuva-fraca-visivel-04-code-review-01.md` e `003-chuva-fraca-visivel-05-asbuild.md`.
- [ ] Atualizar `mod-backlog.md` e `sessions.md`.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Patch stateless registrado uma única vez no `Awake()` do plugin; não aloca estado por raid nem cria listeners que precisem ser desmontados. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Não reage a ações de player; é um efeito de câmera/shader puramente local no cliente. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `RainFallDrops.method_2` é `public void method_2()`, não-virtual e sem herdeiros no Assembly do EFT ([`RainFallDrops.cs:4, 129`](../../../../references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs#L129)). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | Não altera estado canônico de entidade; ajusta a propriedade de shader `_Size` diretamente no mesmo material instanciado pelo EFT após o método nativo calcular. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `ConfigEntry` do BepInEx persiste automaticamente no `.cfg`; sem estado transiente em memória. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Nome `Min Rain Drop Size`, faixa `0f` a `0.30f`, default `0.08f`, tooltip detalhado em pt-BR e valor `0f` como desativado neutro. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` — AP-07 | N/A | Postfix não invoca novamente o método original; nenhuma reentrância possível. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Sem caches de contexto (telas/armas); consulta `ConfigEntry.Value` diretamente a cada frame. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Confirmado diretamente em `references/eft-decompiled/Assembly-CSharp/RainFallDrops.cs:129`. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não interage com skills do EFT. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Não introduz pacotes de rede; modificação 100% client-side visual. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Spec técnica criada via handoff Gemini. |
