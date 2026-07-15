using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.1 — modificadores de velocidade/inércia de movimento por classe.
///     Cobre: 🔻 Heavy Frame (Tanque −10% vel) · 🔻 Rooted (Caçador −15% em ADS) · 🔧 Execution (Furtivo +vel c/ melee)
///     · 🔻 Overladen (Saqueador inércia↑ por peso). Gating: só o player LOCAL (MainPlayer) + classe; lê o F12 no apply-time.
///
///     <para>
///     ⚠️⚠️ <b>BUG CORRIGIDO 2026-07-15 — velocidade decaía a cada movimento até quase parar</b> (report do usuário:
///     "diminui cada vez que move WASD, ia diminuindo sempre"). A versão de 2026-06-24 patchava os <b>drivers</b>
///     (<c>SetCharacterMovementSpeed</c>, <c>SprintAcceleration</c>), achando que os getters eram "só teto". Errado, e
///     o oposto: esses drivers gravam em <b>campos que o próprio EFT relê e regrava a cada frame</b> — um multiplicador
///     ali <b>COMPÕE</b> geometricamente (×0.9, ×0.81, ×0.729…) até o piso.
///     </para>
///     <para>
///     Prova (decompile da DLL real): <c>UpdateCharacterControllerSpeedLimit()</c> roda por frame e faz
///     <c>SetCharacterMovementSpeed(RelativeSpeed * MaxSpeed)</c> (MovementContext.cs:4181); como
///     <c>RelativeSpeed = CharacterMovementSpeed / MaxSpeed</c> (:2377), isso é <c>SetCMS(CMS)</c> — um no-op de
///     manutenção. Um Prefix ×0.9 no input o transforma em <c>CMS *= 0.9</c> a cada frame, e o campo persiste →
///     decaimento infinito. O sprint tinha o mesmo vício: <c>SprintSpeed</c> é um campo de estado
///     (<c>SprintSpeed_1 = 1f</c>) que <c>SprintAcceleration()</c> lê e regrava todo frame (:2550).
///     </para>
///     <para>
///     <b>Correção:</b> reduzir a velocidade SÓ nos getters <b>sem estado</b> (computados puros, recalculam do zero,
///     não acumulam): <c>MaxSpeed => Evaluate(WalkSpeed, Strength/60)</c> (:910, o walk) e
///     <c>SprintingSpeed => Evaluate(...)</c> (:912, o sprint). Reduzir <c>MaxSpeed</c> é a forma CANÔNICA de deixar o
///     personagem mais lento — é o mesmo eixo que a skill Strength move. Velocidade real = <c>RelativeSpeed × MaxSpeed</c>,
///     então o efeito é real e ESTÁVEL (o RelativeSpeed é o comando do jogador, satura em 1). Sem loop.
///     </para>
/// </summary>
internal static class ClassMoveSpeed
{
    internal static void Apply(MovementContext ctx, ref float result)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(ctx, p.MovementContext))
            {
                return;   // só o player local (não bots/remotos)
            }

            // 🔻 Heavy Frame (Tanque): −10% de velocidade (sempre). ✅ FUNCIONA: reduzir MaxSpeed abaixo do
            // cap absoluto do SpeedLimiter (Run 4.6 m/s) de fato desacelera o boneco (0.9×0.717≈0.645 → ~4.14 m/s).
            if (PerksConfig.HeavyFrameEnabled?.Value == true && SkillMultipliers.IsLocalClass("Tank"))
            {
                result *= PerksConfig.HeavyFrameMoveSpeed?.Value ?? 1f;
            }

            // 🔻 Rooted (Caçador): −15% de velocidade enquanto MIRA (ADS).
            // ⚠️ INERTE por este lever (code-review 2026-07-15, F1 — item de backlog 074): a velocidade em ADS é
            // governada pelo TETO DE MIRA (ClampSpeed via StateSpeedLimit ≈ 0.33–0.50, MovementContext.cs:1843 +
            // Player.cs:12155), que já é MENOR que o MaxSpeed reduzido pelo Rooted (0.85×0.717≈0.61) → o min() nunca
            // pega o Rooted. Para morder, o lever certo é o AimMovementSpeed / StateSpeedLimit de mira, não o MaxSpeed.
            if (PerksConfig.RootedEnabled?.Value == true && SkillMultipliers.IsLocalClass("Hunter")
                && p.HandsController is Player.FirearmController fc && fc.IsAiming)
            {
                result *= PerksConfig.RootedAdsSpeed?.Value ?? 1f;
            }

            // 🔧 Execution (Furtivo): +velocidade com a MELEE na mão.
            // ⚠️ CLAMPADO por este lever (code-review 2026-07-15, F2 — item de backlog 074): a velocidade real tem
            // um TETO ABSOLUTO no SpeedLimiter/CharacterController (GClass2175, Run 4.6 m/s) que NÃO lê MaxSpeed →
            // subir o getter acima do vanilla é cortado de volta. O getter é lever válido pra BAIXO, não pra CIMA.
            if (PerksConfig.ExecutionSpeedEnabled?.Value == true && SkillMultipliers.IsLocalClass("Stealth")
                && p.HandsController is Player.KnifeController)
            {
                result *= PerksConfig.ExecutionMoveSpeed?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] move speed falhou: {ex.Message}");
        }
    }
}

/// <summary>🚶 Velocidade de ANDAR. <c>MaxSpeed</c> é computado puro (sem campo) → Postfix-mult é estável, não acumula.</summary>
internal class MaxSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.MaxSpeed));
    }

    [PatchPostfix]
    private static void Postfix(MovementContext __instance, ref float __result)
    {
        ClassMoveSpeed.Apply(__instance, ref __result);
    }
}

/// <summary>
///     🏃 Velocidade de CORRER. <c>SprintingSpeed</c> (:912) é computado puro — o análogo do <c>MaxSpeed</c> para o
///     sprint (alimenta o alvo <c>num2</c> de <c>SprintAcceleration</c>, :2547). Postfix-mult aqui é estável.
///     ⚠️ NÃO patchar <c>SprintSpeed</c> (o campo de estado <c>SprintSpeed_1</c>) — foi a fonte do loop antigo.
///     Nota: o <c>+1f</c> na fórmula do alvo torna a redução do sprint sublinear (×0.9 no getter ≈ −5% na velocidade
///     efetiva, não −10%); é uma aproximação aceitável para o drawback, e ESTÁVEL, que é o que importa.
/// </summary>
internal class SprintingSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.SprintingSpeed));
    }

    [PatchPostfix]
    private static void Postfix(MovementContext __instance, ref float __result)
    {
        ClassMoveSpeed.Apply(__instance, ref __result);
    }
}

/// <summary>🔻 Overladen (Saqueador): inércia escala mais com o peso (movimento clunky carregado).</summary>
internal class OverladenInertiaPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BasePhysicalClass), nameof(BasePhysicalClass.OnWeightUpdated));
    }

    [PatchPostfix]
    private static void Postfix(BasePhysicalClass __instance)
    {
        try
        {
            if (PerksConfig.OverladenEnabled?.Value != true)
            {
                return;
            }

            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.Physical))
            {
                return;   // só o player local
            }

            if (!SkillMultipliers.IsLocalClass("Scavenger"))
            {
                return;
            }

            // Overladen (fix review 2026-06-24): OnWeightUpdated já DERIVOU os campos de inércia reais a partir de
            // Inertia — multiplicar só o Inertia cru (pós-derivação) quase não muda o "clunky". Multiplicamos também
            // os derivados que de fato movem (lateral/diagonal).
            var m = PerksConfig.OverladenInertia?.Value ?? 1f;
            __instance.Inertia *= m;
            __instance.MoveSideInertia *= m;
            __instance.MoveDiagonalInertia *= m;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] overladen falhou: {ex.Message}");
        }
    }
}

// ──────────────────────────────────────────────────────────────────────────────────────────────
// REMOVIDOS 2026-07-15 (bug de decaimento de velocidade):
//   • SetCharacterMovementSpeedPatch (Prefix em SetCharacterMovementSpeed) — o input realimentado
//     (UpdateCharacterControllerSpeedLimit → SetCMS(RelativeSpeed×MaxSpeed) = SetCMS(CMS)) fazia o ×0.9
//     COMPOR a cada frame → CMS decaía a ~0.
//   • SprintAccelerationPatch (Postfix em SprintAcceleration) — lia o campo de estado SprintSpeed já
//     reduzido e o regravava reduzido de novo → mesmo decaimento no sprint.
// A redução de velocidade voltou aos getters SEM ESTADO (MaxSpeedPatch / SprintingSpeedPatch, acima),
// que era a abordagem correta desde o início. Ver o doc de ClassMoveSpeed.
// ──────────────────────────────────────────────────────────────────────────────────────────────
