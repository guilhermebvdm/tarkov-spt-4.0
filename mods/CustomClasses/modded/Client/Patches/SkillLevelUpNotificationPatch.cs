using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     (014) Reescreve a notificação de skill level-up inserindo EASILY (buff, verde) / FINALLY (debuff,
///     vermelho) quando a skill tem multiplicador da classe. Mantém ícone/duração/estilo vanilla.
///     Ponto REAL (confirmado via ilspycmd): LocalPlayer.OnSkillLevelChanged → DisplayNotification(new GClass2549(skill)),
///     cujo ctor monta o texto em String_0 = Format(Localized("SkillLevelUpMessage"), Localized(skill.Id), skill.Level).
///     Patcha o CONSTRUTOR dessa notificação resolvido por ASSINATURA (NotificationAbstractClass + ctor(AbstractSkillClass)),
///     não pelo nome GClassNNNN (estável entre versões). Cobre raid e academia/gym (HideoutPlayer herda LocalPlayer).
/// </summary>
internal class SkillLevelUpNotificationPatch : ModulePatch
{
    // Tipo da notificação de skill: NotificationAbstractClass concreto cujo ctor recebe AbstractSkillClass.
    private static readonly Type? NotifType = AccessTools
        .GetTypesFromAssembly(typeof(NotificationAbstractClass).Assembly)
        .FirstOrDefault(t => !t.IsAbstract
                             && typeof(NotificationAbstractClass).IsAssignableFrom(t)
                             && t.GetConstructor(new[] { typeof(AbstractSkillClass) }) != null);

    // Campo do texto exibido (String_0) — único campo string declarado no tipo.
    private static readonly FieldInfo? TextField = NotifType == null
        ? null
        : AccessTools.GetDeclaredFields(NotifType).FirstOrDefault(f => f.FieldType == typeof(string));

    internal static bool CanEnable => NotifType != null && TextField != null;

    protected override MethodBase GetTargetMethod()
    {
        return NotifType!.GetConstructor(new[] { typeof(AbstractSkillClass) });
    }

    [PatchPostfix]
    private static void Postfix(object __instance, AbstractSkillClass skill)
    {
        if (!Plugin.ShowLevelUpFlavor || skill == null || TextField == null)
        {
            return;
        }

        try
        {
            if (!SkillMultipliers.TryGet(skill.Id, out var factor) || !MultiplierFormat.IsActive(factor))
            {
                return;   // sem multiplicador da classe → notificação vanilla
            }

            if (!(TextField.GetValue(__instance) is string text) || string.IsNullOrEmpty(text))
            {
                return;
            }

            var up = factor > 1f;
            var keyword = up ? "EASILY" : "FINALLY";
            var hex = up ? MultiplierFormat.GreenHex : MultiplierFormat.RedHex;
            var colored = $"<color={hex}>{keyword}</color>";

            // "Endurance skill leveled up to 9" → "Endurance skill EASILY leveled up to 9 ;)"
            var newText = text.Replace(" leveled up to ", $" {colored} leveled up to ");
            if (up)
            {
                newText += " ;)";
            }

            TextField.SetValue(__instance, newText);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] levelup notif falhou: {ex.Message}");
        }
    }
}
