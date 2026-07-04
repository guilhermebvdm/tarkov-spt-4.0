using System;
using System.Collections;
using System.Reflection;
using EFT.Communications;   // ENotificationDurationType
using EFT.UI;               // BaseNotificationView
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     Fix in-game 2026-07-03 — notificação de perks por 10 segundos. O EFT não tem duração em segundos:
///     `Long` só põe o animator a meia velocidade (ref: BaseNotificationView.Init, switch em Duration) e
///     `Infinite` desliga o auto-complete. Estratégia: Prefix em <c>BaseNotificationView.Init</c> — quando a
///     notificação é a NOSSA (Description == texto registrado pelo <see cref="RaidPerksNotificationPatch"/>),
///     promove `Duration → Infinite` e agenda o hide NATIVO após <see cref="DurationSeconds"/> (mesmo caminho
///     do click — `HideNotification(true)` recicla a view via OnHideComplete).
///     ref: NotificationManagerClass.cs:523-526 (DisplayMessageNotification → GClass2551(message, ...)).
/// </summary>
internal class NotificationDurationPatch : ModulePatch
{
    internal const float DurationSeconds = 10f;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseNotificationView), nameof(BaseNotificationView.Init));
    }

    [PatchPrefix]
    private static void Prefix(BaseNotificationView __instance, NotificationAbstractClass notification)
    {
        try
        {
            var ours = RaidPerksNotificationPatch.LastNotificationText;
            if (string.IsNullOrEmpty(ours) || notification == null
                || !string.Equals(notification.Description, ours, StringComparison.Ordinal))
            {
                return;
            }

            RaidPerksNotificationPatch.LastNotificationText = null;   // 1 match por exibição (não vaza p/ toasts iguais)
            notification.Duration = ENotificationDurationType.Infinite;   // Init desliga o auto-complete
            Plugin.Instance?.StartCoroutine(HideAfter(__instance, notification));
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] notif duration falhou: {ex.Message}");
        }
    }

    private static IEnumerator HideAfter(BaseNotificationView view, NotificationAbstractClass notification)
    {
        yield return new WaitForSeconds(DurationSeconds);

        // a view pode ter sido fechada no click e RECICLADA p/ outra notificação → HasSameNotification guarda.
        if (view != null && view.isActiveAndEnabled && view.HasSameNotification(notification))
        {
            view.HideNotification(invokeCallback: true);
        }
    }
}
