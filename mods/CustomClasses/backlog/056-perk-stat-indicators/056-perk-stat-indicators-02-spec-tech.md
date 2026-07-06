# 056 — Indicador de perk no peso (Pack Mule) · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [056-perk-stat-indicators-01-spec.md](056-perk-stat-indicators-01-spec.md)
**Criado:** 2026-07-03

> Fonte primária: **DLL vivo** `D:\SPT\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll` (via `ilspycmd -t <FQN>`) —
> a camada `EFT.UI` **não está** no decompile curado do repo (padrão já usado no 013). Refs de mecânica (`CarryingWeightRelativeModifier`, gate) vêm do código do mod (§4). Toda ref cita `arquivo.cs:linha`.

## 1. Estratégia

**Postfix em `HealthParametersPanel.method_0()`** (o refresh geral do painel de saúde). Adiciona/atualiza um marcador
**"▲ +X%"** + `HoverTooltipArea` ao lado do TMP do valor de peso, **se** a classe local tem Pack Mule ativo.

- **Por que `method_0`:** é chamado no fim do `Show` (`HealthParametersPanel.cs:134`) — **depois** de os
  `UI.BindEvent(OnWeightUpdated)` (`:59`) já terem setado o valor do peso (BindEvent dispara o handler na hora) — e
  periodicamente pelo `Update` (`:240`). Logo o TMP `_maxValue` do peso já tem "/NN" quando o Postfix roda, e o marcador
  é re-afirmado a cada refresh (idempotente). Não patcheia `method_1` (só roda em effects, não no Show) nem os delegates
  anônimos (não patcháveis).
- **O número já está bufado:** o `Max` do peso = `UpperOverweightLimit × skillManager.CarryingWeightRelativeModifier`
  (`HealthParametersPanel.cs:67/251`), e o `PackMulePatch` postfixa esse getter
  ([PackMulePatch.cs:22](../../modded/Client/Patches/PackMulePatch.cs#L22)). O marcador só **anota** a origem.
- **Molde:** reusa o padrão do `SkillPanelPatch` (marcador TMP filho do texto-alvo + `HoverTooltipArea` resolvido 1× via
  `ItemUiContext.Instance.Tooltip`) — [SkillPanelPatch.cs:78-107](../../modded/Client/Patches/SkillPanelPatch.cs#L78).
- **Gate:** `Plugin.ShowOnUi` + `PerksConfig.PackMuleEnabled` + `SkillMultipliers.IsLocalClass("Scavenger"|"Tank")`
  (mesmo gate do `PackMulePatch`). **Sem novo F12** (escopo mínimo).

Alternativa descartada: Postfix em `Show` — roda 1× e cobre menos refreshes; `method_0` no `Update` mantém o marcador
vivo se algo reciclar o painel.

## 2. Pontos de patch

| Alvo (DLL vivo) | Tipo | Motivo |
|---|---|---|
| `EFT.UI.Health.HealthParametersPanel.method_0()` (`HealthParametersPanel.cs:169`) | **Postfix** | refresh do painel (Show+Update) — anexa/atualiza o marcador |
| `HealthParametersPanel._weight` (`:37`, `HealthParameterPanel`) | leitura (reflection) | o sub-painel do peso |
| `EFT.UI.Health.HealthParameterPanel._maxValue` (`HealthParameterPanel.cs:16`, `TMP_Text`) | leitura (reflection) | TMP do "/NN" — âncora do marcador |
| `SkillPanelPatch.GetOrCreateMarker` (`SkillPanelPatch.cs:78`) | molde reusado | marcador TMP + `HoverTooltipArea` |
| `PackMulePatch` gate (`PackMulePatch.cs:30,44`) + `SkillMultipliers.IsLocalClass` | padrão reusado | mesma condição de "Pack Mule ativo" |
| `MultiplierFormat.Marker`/`IsActive` (`MultiplierFormat.cs:20,40`) | reuso | texto "▲ +X%" + gate de fator≠1 |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. Reusa `PackMuleEnabled` (efeito) + `ShowOnUi` (indicadores de UI, item 010) + `PackMuleCarryBonus` (o X%).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/Patches/WeightMarkerPatch.cs` | **CRIAR** | Postfix em `HealthParametersPanel.method_0`; marcador "▲ +X%" + tooltip ao lado do peso, gate de classe/Pack Mule. Marcador próprio (não toca o `SkillPanelPatch`). |
| `modded/Client/MultiplierFormat.cs` | MODIFICAR | + `CarryTooltip(factor, className)` (texto i18n do tooltip do peso — "limite de carga +X% pela Classe …, piso"). |
| `modded/Client/Plugin.cs` | MODIFICAR | `new WeightMarkerPatch().Enable();` junto aos patches de UI. |

## 5. Stubs de código

```csharp
// modded/Client/Patches/WeightMarkerPatch.cs
using System;
using System.Reflection;
using EFT.UI;          // HoverTooltipArea, ItemUiContext
using EFT.UI.Health;   // HealthParametersPanel, HealthParameterPanel
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (056) Marcador "▲ +X%" + tooltip no LIMITE DE PESO (aba Health) atribuindo o bônus ao Pack Mule da classe.
///     O número já reflete o perk (PackMulePatch postfixa CarryingWeightRelativeModifier); aqui só anotamos a origem.
///     ref: Assembly-CSharp.dll → EFT.UI.Health.HealthParametersPanel { HealthParameterPanel _weight; void method_0(); }
///          EFT.UI.Health.HealthParameterPanel { TMP_Text _maxValue; }
/// </summary>
internal class WeightMarkerPatch : ModulePatch
{
    private const string MarkerName = "CC_WeightMarker";
    private const float MarkerGap = 14f;

    private static readonly FieldInfo? WeightField = AccessTools.Field(typeof(HealthParametersPanel), "_weight");
    private static readonly FieldInfo? MaxValueField = AccessTools.Field(typeof(HealthParameterPanel), "_maxValue");

    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(HealthParametersPanel), "method_0");   // ref: HealthParametersPanel.cs:169

    [PatchPostfix]
    private static void Postfix(HealthParametersPanel __instance)
    {
        try
        {
            var factor = 1f + (PerksConfig.PackMuleCarryBonus?.Value ?? 0f);
            var maxTmp = WeightField?.GetValue(__instance) is HealthParameterPanel w
                ? MaxValueField?.GetValue(w) as TMP_Text : null;
            var show = Plugin.ShowOnUi && PerksConfig.PackMuleEnabled?.Value == true
                       && MultiplierFormat.IsActive(factor);
            if (show)
            {
                SkillMultipliers.EnsureLoaded();
                show = SkillMultipliers.IsLocalClass("Scavenger") || SkillMultipliers.IsLocalClass("Tank");
            }

            if (maxTmp == null)
            {
                return;
            }

            var marker = GetOrCreateMarker(maxTmp);
            var tmp = marker.GetComponent<TextMeshProUGUI>();
            if (!show)
            {
                tmp.text = string.Empty;
                marker.SetActive(false);
                return;
            }

            tmp.text = MultiplierFormat.Marker(factor);
            ((RectTransform)marker.transform).anchoredPosition = new Vector2(maxTmp.preferredWidth + MarkerGap, 0f);
            marker.GetComponent<HoverTooltipArea>()
                .SetMessageText(MultiplierFormat.CarryTooltip(factor, SkillMultipliers.ClassName), rawText: true);
            marker.SetActive(true);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (056) weight marker falhou: {ex.Message}");
        }
    }

    // Igual a SkillPanelPatch.GetOrCreateMarker, mas ancorado a um TMP_Text (o _maxValue). Idempotente.
    private static GameObject GetOrCreateMarker(TMP_Text anchor)
    {
        var existing = anchor.transform.Find(MarkerName);
        if (existing != null) return existing.gameObject;

        var go = new GameObject(MarkerName, typeof(RectTransform));
        go.transform.SetParent(anchor.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(160f, 28f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = anchor.font;
        tmp.fontSharedMaterial = anchor.fontSharedMaterial;
        tmp.fontSize = anchor.fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = true;   // p/ o HoverTooltipArea receber o ponteiro

        var area = go.AddComponent<HoverTooltipArea>();
        area.Init(ItemUiContext.Instance.Tooltip, string.Empty, rawText: true);
        return go;
    }
}
```

```csharp
// modded/Client/MultiplierFormat.cs  (+ método)
public static string CarryTooltip(float factor, string? className)
{
    var pct = Percent(factor);                     // +30
    var sign = pct >= 0 ? "+" : string.Empty;
    var amount = $"<color={GreenHex}>{sign}{pct}%</color>";
    if (GameLocale.IsPortuguese)
    {
        var cls = string.IsNullOrWhiteSpace(className) ? "sua Classe" : $"Classe <b>{className}</b>";
        return $"Limite de carga {amount} pela {cls} (Pack Mule, piso)";
    }
    var clsEn = string.IsNullOrWhiteSpace(className) ? "your Class" : $"Class <b>{className}</b>";
    return $"Carry limit {amount} from {clsEn} (Pack Mule, floor)";
}
```

## 6. Fluxo de dados

```
[A] jogador abre a aba Health → HealthParametersPanel.Show(...)  (HealthParametersPanel.cs:50)
      ├─ UI.BindEvent(OnWeightUpdated) → seta _weight (_maxValue = "/NN" já com o +30%)   (:59-73)
      └─ method_0()                                                                        (:134/169)
            ↓ Postfix (056)
[B] gate: ShowOnUi + PackMuleEnabled + IsLocalClass(Scavenger|Tank)   (PackMulePatch.cs:30,44)
      ↓ passou
[C] _weight._maxValue (TMP) → GetOrCreateMarker → "▲ +30%" + CarryTooltip   (SkillPanelPatch.cs:78 molde)
      → marcador ao lado do peso (Update re-afirma; idempotente)
```

## 7. Riscos e dependências

- **Peso Max já bufado:** depende do `PackMulePatch` estar ativo (mesmo gate) — o número e o marcador ficam coerentes.
- **Piso não morde (Strength alta):** o marcador mostra o piso garantido; o tooltip diz "piso". Aceito (spec corner case).
- **`_weight`/`_maxValue` por reflection:** confirmados no DLL vivo; se um patch do EFT renomear, o `FieldInfo` vem null
  → `maxTmp==null` → early return (sem crash). Logado indiretamente (nada aparece).
- **Idempotência:** o painel de saúde é único (não reciclado como `SkillPanel`); mesmo assim `GetOrCreateMarker` +
  reescrita a cada `method_0` cobrem reabertura/Update.
- **Sem conflito** com `SkillPanelPatch` (aba Skills) nem `PackMulePatch` (mecânica) — pontos distintos.

## 8. Checklist de implementação

- [ ] Criar `WeightMarkerPatch.cs` (Postfix + `GetOrCreateMarker` ancorado no `_maxValue`).
- [ ] `MultiplierFormat.CarryTooltip(factor, className)` (i18n).
- [ ] `new WeightMarkerPatch().Enable()` no `Plugin.cs` (junto aos patches de UI).
- [ ] Compile 0/0.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid start/stop idempotente — AP-01 | N/A | UI de menu **fora da raid**; sem estado de raid nem hook de start/stop. |
| 2 | Filtro MainPlayer/Fika — AP-02 | ✅ | Gateia por **classe local** (`IsLocalClass`), como o `PackMulePatch` fora da raid (sem `MainPlayer`); é UI, não ação de player. Spec §Fika = N/A justificado. |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides — AP-03 | ✅ | `method_0` concreto (não virtual); `_weight`/`_maxValue` confirmados no DLL vivo (`HealthParametersPanel.cs:37`, `HealthParameterPanel.cs:16`). |
| 4 | Estado via API canônica; side-effects — AP-04 | ✅ | Não muda estado do jogo — só cria UI própria (marcador filho do TMP). Leitura read-only dos campos. |
| 5 | Estado entre raids — coberto | N/A | UI recriada a cada `Show`; sem persistência. |
| 6 | ConfigEntry semântica/defaults/neutro — AP-05 | ✅ | Reusa `PackMuleEnabled`/`ShowOnUi`/`PackMuleCarryBonus`; estado neutro (qualquer off → sem marcador). |
| 7 | Reentry-guard — AP-07 | N/A | Postfix não re-invoca `method_0`. |
| 8 | Flags/caches após troca de contexto — AP-08 | ✅ | `GetOrCreateMarker` idempotente; texto/posição/tooltip reescritos a cada `method_0` (reflete o `PackMuleCarryBonus` atual). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Spec técnica via `/create-technical-spec` (Postfix em `HealthParametersPanel.method_0`, refs do DLL vivo) |
