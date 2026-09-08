using System.Reflection;
using HarmonyLib;
using SkillsExtended.Core;
using SkillsExtended.Utils;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace SkillsExtended.Patches;

/// <summary>
///     Aplica o desconto de tempo do círculo cultista (skill Shadow Connections) direto no
///     parâmetro craftingTime de RegisterCircleOfCultistProduction — o método só é chamado quando
///     uma produção nova é de fato registrada, então nunca reaplica o desconto numa produção antiga
///     (ref: AUD-01-05; CR-01-01 — corrige o caso de borda do StartSacrifice sair cedo sem criar
///     produção nova, que a versão anterior desta classe, patchando StartSacrifice.Postfix, podia
///     confundir com uma produção pré-existente).
/// </summary>
public class RegisterCircleOfCultistProductionPatch : AbstractPatch
{
    private static readonly ConfigController ConfigController = ServiceLocator.ServiceProvider.GetRequiredService<ConfigController>();
    private static readonly SkillUtil SkillUtil = ServiceLocator.ServiceProvider.GetRequiredService<SkillUtil>();

    protected override MethodBase? GetTargetMethod()
    {
        return AccessTools.Method(typeof(CircleOfCultistService), "RegisterCircleOfCultistProduction");
    }

    [PatchPrefix]
    public static void Prefix(MongoId sessionId, ref double craftingTime)
    {
        if (!SkillUtil.TryGetSkillLevel(sessionId, SkillTypes.Shadowconnections, out var skillLevel))
        {
            return;
        }

#if DEBUG
        Console.WriteLine($"Cultist circle original time: `{craftingTime}` seconds");
#endif

        var timeBonusPerLevel = ConfigController.SkillsConfig.ShadowConnections.CultistCircleReturnTimeReduction;

        var buff = Math.Clamp(1f - timeBonusPerLevel * skillLevel, 0f, 1f);

#if DEBUG
        Console.WriteLine($"Cultist Circle Buff: {buff}");
#endif

        craftingTime *= buff;

#if DEBUG
        Console.WriteLine($"Cultist circle modified time: `{craftingTime}` seconds");
#endif
    }
}