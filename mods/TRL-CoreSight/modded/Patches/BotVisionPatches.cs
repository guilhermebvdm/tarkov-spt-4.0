using System;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Configuration;
using TRLCoreSight.Core;
using UnityEngine;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Modula a velocidade de percepção dos bots conforme a taxa de cobertura zonal do jogador.
    /// Inspirado na mecânica de stealth fotométrico do Ombarella, mas utilizando a oclusão geométrica física.
    /// </summary>
    public class BotVisionSpeedPatch : ModulePatch
    {
        private static readonly Type[] Method9Params = new Type[]
        {
            typeof(BotDifficultySettingsClass),
            typeof(IAIData),
            typeof(float),
            typeof(Vector3),
            typeof(float),
            typeof(float),
            typeof(float)
        };

        protected override MethodBase GetTargetMethod()
        {
            MethodInfo method = AccessTools.Method(typeof(EnemyInfo), "method_9", Method9Params);
            if (method != null)
            {
                return method;
            }

            // Fallback para versões com parâmetros alternativos
            MethodInfo[] methods = typeof(EnemyInfo).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var m in methods)
            {
                if (m.ReturnType == typeof(float) && m.Name.StartsWith("method_"))
                {
                    var pars = m.GetParameters();
                    if (pars.Length >= 6 && pars[0].ParameterType == typeof(BotDifficultySettingsClass) && pars[1].ParameterType == typeof(IAIData))
                    {
                        return m;
                    }
                }
            }

            throw new MissingMethodException("TRL-CoreSight: Não foi possível localizar o método EnemyInfo.method_9 para patch de visão.");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref float __result, EnemyInfo __instance)
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableNaturalConcealment.Value)
            {
                return;
            }

            NaturalConcealmentManager manager = NaturalConcealmentManager.Instance;
            if (manager == null || manager.CoverageRatio <= 0.05f)
            {
                return;
            }

            // Garante que o patch só atue quando o bot estiver observando o jogador humano local
            if (__instance == null || __instance.Person == null || !__instance.Person.IsYourPlayer)
            {
                return;
            }

            // Quanto maior a taxa de cobertura (zonas no mato), mais lento o bot adquire o alvo
            // Coverage 1.0 (100% no mato) -> Multiplicador 0.18f (~5.5x mais tempo para detectar)
            // Coverage 0.5 (metade coberto) -> Multiplicador 0.59f
            float multiplier = Mathf.Lerp(1.0f, 0.18f, manager.CoverageRatio);

            // Se o bot possuir visão noturna (NVG) ou térmica ativada, atenua a camuflagem
            if (__instance.HaveNightVision())
            {
                multiplier = Mathf.Lerp(multiplier, 1.0f, 0.5f);
            }

            __result *= multiplier;
        }
    }

    /// <summary>
    /// Desvia ligeiramente a mira dos bots caso o jogador esteja parcialmente coberto por vegetação.
    /// Simula que o bot avistou apenas um contorno imperfeito ou membro do jogador no matagal e erra os disparos.
    /// </summary>
    public class BotAimOffsetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotAimingClass), "method_13");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotAimingClass __instance)
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableNaturalConcealment.Value || !ModConfig.EnableAimOffsetNerf.Value)
            {
                return;
            }

            NaturalConcealmentManager manager = NaturalConcealmentManager.Instance;
            if (manager == null || manager.CoverageRatio <= 0.20f)
            {
                return;
            }

            if (__instance == null || __instance.BotOwner_0 == null || __instance.BotOwner_0.Memory == null)
            {
                return;
            }

            EnemyInfo goalEnemy = __instance.BotOwner_0.Memory.GoalEnemy;
            if (goalEnemy == null || goalEnemy.Person == null || !goalEnemy.Person.IsYourPlayer)
            {
                return;
            }

            // Aplica dispersão proporcional à cobertura e à intensidade configurável
            float intensity = ModConfig.AimOffsetNerfIntensity.Value;
            float maxScatter = manager.CoverageRatio * 0.75f * intensity;

            Vector3 scatterOffset = new Vector3(
                UnityEngine.Random.Range(-maxScatter, maxScatter),
                UnityEngine.Random.Range(-maxScatter * 0.5f, maxScatter * 0.5f),
                UnityEngine.Random.Range(-maxScatter, maxScatter)
            );

            __instance.EndTargetPoint += scatterOffset;
        }
    }
}
