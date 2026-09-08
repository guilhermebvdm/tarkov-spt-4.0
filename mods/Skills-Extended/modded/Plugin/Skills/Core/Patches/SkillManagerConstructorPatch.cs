using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using HarmonyLib;
using SkillsExtended.Helpers;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.Core.Patches;

internal class SkillManagerConstructorPatch : ModulePatch
{
    // ref: AUD-01-10 — mapa fraco (não impede GC do SkillManager/Profile dono ao fim da raid). Permite
    // AbstractSkillClassSummaryLevelPatch resolver o dono real de um AbstractSkillClass em vez de sempre
    // presumir o MainPlayer via GameUtils.GetSkillManager().
    internal static readonly ConditionalWeakTable<AbstractSkillClass, SkillManager> SkillOwners = new();

    protected override MethodBase GetTargetMethod() =>
        typeof(SkillManager).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            [],
            null);

    [PatchPrefix]
    public static void Prefix(SkillManager __instance)
    {
        __instance.SkillManagerExtended = new SkillManagerExt(__instance);
    }

    [PatchPostfix]
    public static void Postfix(SkillManager __instance, ref SkillClass[] ___DisplayList, ref SkillClass[] ___Skills)
    {
        InitializeNewSkills(__instance, ref ___Skills);
        ModifyDisplayList(__instance, ref ___DisplayList);
        LockSkills(__instance);

        // ref: AUD-01-10 — EFT/SkillManager.cs:2530, FieldMedicine já construído pelo ctor base neste ponto.
        if (!SkillOwners.TryGetValue(__instance.FieldMedicine, out _))
        {
            SkillOwners.Add(__instance.FieldMedicine, __instance);
        }
	}

    /// <summary>
    ///     Initializes new skills
    /// </summary>
    /// <param name="skillManager">skill manager</param>
    /// <param name="skills">skills</param>
    private static void InitializeNewSkills(SkillManager skillManager, ref SkillClass[] skills)
    {
        skillManager.UsecArsystems = new SkillClass(
            skillManager, 
            ESkillId.UsecArsystems, 
            ESkillClass.Combat, 
            [], 
            []);
        
        skillManager.BearAksystems = new SkillClass(
            skillManager, 
            ESkillId.BearAksystems, 
            ESkillClass.Combat, 
            [], 
            []);

        skillManager.UsecNegotiations = new SkillClass(
            skillManager,
            ESkillId.UsecNegotiations,
            ESkillClass.Special,
            [],
            []);
        
        skillManager.BearRawpower = new SkillClass(
            skillManager,
            ESkillId.BearRawpower,
            ESkillClass.Special,
            [],
            []);
        
        Array.Resize(ref skills, skills.Length + 7);

        skills[^1] = skillManager.UsecArsystems;
        skills[^2] = skillManager.BearAksystems;
        skills[^3] = skillManager.Lockpicking;
        skills[^4] = skillManager.ProneMovement;
        skills[^5] = skillManager.SilentOps;
        skills[^6] = skillManager.UsecNegotiations;
        skills[^7] = skillManager.BearRawpower;
        
    }

    /// <summary>
    ///     Modifies the display list so we can add new skills
    /// </summary>
    /// <param name="skillManager">skill manager</param>
    /// <param name="displayList">display list</param>
    private static void ModifyDisplayList(SkillManager skillManager, ref SkillClass[] displayList)
    {
        const int insertIndex = 12;
        
        var newDisplayList = new SkillClass[displayList.Length + 7];

        Array.Copy(displayList, newDisplayList, insertIndex);

        newDisplayList[12] = skillManager.UsecArsystems;
        newDisplayList[12 + 1] = skillManager.BearAksystems;
        newDisplayList[12 + 2] = skillManager.Lockpicking;
        newDisplayList[12 + 3] = skillManager.ProneMovement;
        newDisplayList[12 + 4] = skillManager.SilentOps;
        newDisplayList[12 + 5] = skillManager.UsecNegotiations;
        newDisplayList[12 + 6] = skillManager.BearRawpower;
        
        Array.Copy(
            displayList, insertIndex, 
            newDisplayList, 
            insertIndex + 7, 
            displayList.Length - insertIndex
            );

        displayList = newDisplayList;
    }

    // ref: AUD-01-27 — resolvido uma vez em vez de 8x a cada SkillManager construído.
    private static readonly FieldInfo LockedField = AccessTools.Field(typeof(SkillClass), "Locked");

    /// <summary>
    ///     Locks skills if they are not enabled
    /// </summary>
    /// <param name="skillManager">skill manager</param>
    private static void LockSkills(SkillManager skillManager)
    {
        LockedField.SetValue(skillManager.UsecArsystems, !SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled);
        LockedField.SetValue(skillManager.BearAksystems, !SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled);
        LockedField.SetValue(skillManager.Lockpicking, !SkillsExtendedPlugin.SkillData.LockPicking.Enabled);
        LockedField.SetValue(skillManager.FieldMedicine, !SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled);
        LockedField.SetValue(skillManager.FirstAid, !SkillsExtendedPlugin.SkillData.FirstAid.Enabled);
        LockedField.SetValue(skillManager.ProneMovement, !SkillsExtendedPlugin.SkillData.ProneMovement.Enabled);
        LockedField.SetValue(skillManager.SilentOps, !SkillsExtendedPlugin.SkillData.SilentOps.Enabled);
        LockedField.SetValue(skillManager.Shadowconnections, !SkillsExtendedPlugin.SkillData.ShadowConnections.Enabled);
    }
}