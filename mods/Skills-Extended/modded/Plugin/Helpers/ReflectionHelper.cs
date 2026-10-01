using System;
using HarmonyLib;
using SkillsExtended.Exceptions;

namespace SkillsExtended.Helpers;

public static class ReflectionHelper
{
    internal static Type OldMovementIdleState;

    public static void GetOldMovementTypes()
    {
        OldMovementIdleState = AccessTools.TypeByName("OldIdleState");

        if (OldMovementIdleState is null)
        {
            throw new SkillsExtendedException("Could not find OldIdleState or OldStationaryState");
        }
    }
}
