using EFT.UI;       // HoverTooltipArea, ItemUiContext
using UnityEngine;
using UnityEngine.UI;   // Graphic

namespace CustomClasses.Client;

/// <summary>
///     (015) Anexa um tooltip do EFT a um GameObject de UI (ex.: "This player is &lt;classe&gt;").
///     Reusa HoverTooltipArea + ItemUiContext.Instance.Tooltip (mesma infra do item 010 — SkillPanelPatch).
///     Idempotente: cria o HoverTooltipArea 1x; depois só atualiza o texto. Null-safe (no-op se o tooltip
///     do contexto ainda não existe). <see cref="Clear"/> esvazia o texto em células recicladas p/ outros jogadores.
/// </summary>
internal static class ClassTooltip
{
    public static void Attach(GameObject? go, string richText)
    {
        if (go == null)
        {
            return;
        }

        var ctx = ItemUiContext.Instance;
        if (ctx == null || ctx.Tooltip == null)
        {
            return;   // tooltip indisponível neste contexto → não quebra
        }

        // HoverTooltipArea precisa de um Graphic com raycastTarget p/ receber o ponteiro.
        var graphic = go.GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }

        var area = go.GetComponent<HoverTooltipArea>();
        if (area == null)
        {
            area = go.AddComponent<HoverTooltipArea>();
            area.Init(ctx.Tooltip, string.Empty, rawText: true);   // resolve o SimpleTooltip 1x
        }

        area.SetMessageText(richText, rawText: true);
    }

    public static void Clear(GameObject? go)
    {
        var area = go == null ? null : go.GetComponent<HoverTooltipArea>();
        if (area != null)
        {
            area.SetMessageText(string.Empty, rawText: true);
        }
    }
}
