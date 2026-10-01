using System;
using System.Collections.Generic;   // ref: AUD-01-07c — cache do texto de tooltip
using System.Reflection;
using EFT;          // SkillClass
using EFT.UI;       // SkillPanel, HoverTooltipArea, ItemUiContext
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;        // TextMeshProUGUI
using UnityEngine;  // GameObject, RectTransform, Vector2

namespace CustomClasses.Client;

/// <summary>
///     (010) UI — marcador "▲ +X%" / "▼ -X%" à direita do nome da skill + tooltip dedicado da classe.
///     Postfix em SkillPanel.method_1 (refresh da linha).
///     ref: Assembly-CSharp.dll → EFT.UI.SkillPanel { TextMeshProUGUI _name; SkillClass skillClass; }.
///     Não mexe mais nas setas vanilla _effectivenessUp/Down (item 005) — volta ao comportamento original.
/// </summary>
internal class SkillPanelPatch : ModulePatch
{
    private const string MarkerName = "CC_MultMarker";
    private const float MarkerGap = 20f;   // px após o fim do texto do nome (não sobrepor)

    /// <summary>
    ///     ref: AUD-01-07c — a lista de Skills RECICLA <c>SkillPanel</c> ao rolar, então cada frame de scroll
    ///     remontava a string do tooltip por linha visível. Cacheado.
    ///     <para>
    ///     ⚠️ ref: PA-01-03 — o <c>className</c> é OBRIGATÓRIO na chave: <c>MultiplierFormat.TooltipText</c>
    ///     depende dele, e ele muda em dois cenários reais — troca de perfil sem reiniciar o cliente
    ///     (Reset + refetch) e troca de idioma do EFT (o getter <c>ClassName</c> resolve por
    ///     <c>GameLocale.IsPortuguese</c>). Sem isso o tooltip afirmaria a classe errada.
    ///     </para>
    ///     <para>
    ///     ⚠️ Desvio da spec técnica (§5.7), deliberado: ela previa a chave <c>(ESkillId, float, string?)</c>.
    ///     A assinatura real é <c>TooltipText(float factor, string? className)</c>
    ///     (<c>MultiplierFormat.cs:55</c>) — **não recebe o ESkillId**. Incluí-lo produziria N entradas
    ///     idênticas para o mesmo texto. A chave correta é o par de que o texto de fato depende.
    ///     </para>
    /// </summary>
    private static readonly Dictionary<(float, string?), string> TooltipCache = new();

    /// <summary>ref: PA-04-03 — assinado a <c>SkillMultipliers.ClassChanged</c> no <c>Plugin.Awake</c>.</summary>
    internal static void ClearTooltipCache() => TooltipCache.Clear();

    private static string TooltipFor(float factor, string? className)
    {
        var key = (factor, className);
        if (TooltipCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var text = MultiplierFormat.TooltipText(factor, className);
        TooltipCache[key] = text;
        return text;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SkillPanel), nameof(SkillPanel.method_1));
    }

    [PatchPostfix]
    private static void Postfix(SkillClass ___skillClass, TextMeshProUGUI ____name)
    {
        if (!Plugin.ShowOnUi || ___skillClass is null || ____name is null)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();   // perfil novo (sem XP) → garante cache+ClassName ao abrir a tela
            // Pula skills bloqueadas/"beta" (Locked = sem buffs; ex.: FirstAid/FieldMedicine sem o Skills-Extended):
            // não faz sentido marcar +X% numa skill que o jogador não pode usar. (got: TryGet sempre roda → f atribuído.)
            var got = SkillMultipliers.TryGet(___skillClass.Id, out var f);
            var has = got && !___skillClass.Locked && MultiplierFormat.IsActive(f);

            // Marcador reusável (a lista recicla SkillPanel ao rolar) — sempre reescrito/escondido.
            var marker = GetOrCreateMarker(____name);
            var tmp = marker.GetComponent<TextMeshProUGUI>();

            if (!has)
            {
                // ref: CR-01-02 — desativar (OnDisable fecha o tooltip) antes de mexer na mensagem
                // evita flash de tooltip vazio se o cursor estiver sobre o marcador durante o scroll.
                tmp.text = string.Empty;
                marker.SetActive(false);
                return;
            }

            tmp.text = MultiplierFormat.Marker(f);
            // Ponto 2: posiciona o marcador logo APÓS o fim do texto do nome (não sobre ele).
            // Usa a largura preferida do TMP do nome — robusto a nomes curtos/longos e ao layout do painel.
            ((RectTransform)marker.transform).anchoredPosition = new Vector2(____name.preferredWidth + MarkerGap, 0f);
            // ref: CR-01-03 — o SimpleTooltip é resolvido 1x na criação (GetOrCreateMarker);
            // aqui só atualiza o texto (rawText:true preserva <color>/<b>).
            marker.GetComponent<HoverTooltipArea>()
                .SetMessageText(TooltipFor(f, SkillMultipliers.ClassName), rawText: true);   // ref: AUD-01-07c
            marker.SetActive(true);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] skill panel marker falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     Acha (ou cria 1x) o marcador como filho do _name, herdando fonte/material do nome.
    ///     Ponto 2: ancorado à ESQUERDA do _name (pivot 0, cresce p/ direita); a posição X final é
    ///     definida no Postfix via _name.preferredWidth (logo após o fim do texto, sem sobrepor).
    /// </summary>
    private static GameObject GetOrCreateMarker(TextMeshProUGUI name)
    {
        var existing = name.transform.Find(MarkerName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        var go = new GameObject(MarkerName, typeof(RectTransform));
        go.transform.SetParent(name.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(200f, 32f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = name.font;
        tmp.fontSharedMaterial = name.fontSharedMaterial;
        tmp.fontSize = name.fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;   // texto cresce do anchor (após o nome) p/ a direita
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = true;                            // necessário p/ HoverTooltipArea receber o ponteiro

        // ref: CR-01-03 — resolve o SimpleTooltip uma única vez aqui (na criação); refreshes só fazem SetMessageText.
        var area = go.AddComponent<HoverTooltipArea>();       // Awake também resolve ItemUiContext.Instance.Tooltip
        area.Init(ItemUiContext.Instance.Tooltip, string.Empty, rawText: true);
        return go;
    }
}
