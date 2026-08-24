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
    internal static void Apply(MovementContext ctx, ref float result, bool isSprint)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(ctx, p.MovementContext))
            {
                return;   // 075: só o player local (não bots/remotos)
            }

            var m = 1f;

            // 🔻 Heavy Frame (Tanque): −10% de velocidade (andar E correr).
            if (PerksConfig.HeavyFrameEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Tank))
            {
                m *= PerksConfig.HeavyFrameMoveSpeed?.Value ?? 1f;
            }

            // 🔻 Rooted (Caçador, −15% vel ADS): MOVIDO para RootedAimSlowdownPatch (074/F1). Aqui, no MaxSpeed, era
            // INERTE — o teto de mira (StateSpeedLimit ≈0.33) é menor e o min() ignorava. Lever certo = SetAimingSlowdown.

            // 🔧 Execution (Furtivo): +10% com a MELEE na mão. No ANDAR o MaxSpeed morde; na CORRIDA o cap do
            // SpeedLimiter (ExecutionSpeedCapPatch, 074/F2) é o teto — a compensação do +1f (abaixo) garante que o
            // num2 do sprint suba o MESMO fator, senão o num2 diluído prenderia o min() ABAIXO do cap boostado.
            if (PerksConfig.ExecutionSpeedEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Stealth)
                && p.HandsController is Player.KnifeController)
            {
                m *= PerksConfig.ExecutionMoveSpeed?.Value ?? 1f;
            }

            // 🔧 Lebre (Saqueador — 081): +velocidade enquanto NÃO está pesado. Usa o overweight NATIVO do EFT
            // (BasePhysicalClass.Overweight == 0 = sem o ícone de bigorna). Lido fresco a cada getter → reativo ao
            // peso (lootou e ficou pesado → desliga sozinho). Não acumula (getter sem estado, ver doc acima).
            if (PerksConfig.LebreEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Scavenger)
                && p.Physical != null && p.Physical.Overweight <= 0f)
            {
                m *= PerksConfig.LebreSpeed?.Value ?? 1f;
            }

            if (m == 1f)
            {
                return;   // nenhum lever ativo (classes Tank/Stealth são mutuamente exclusivas → 1 multiplicador)
            }

            if (!isSprint)
            {
                result *= m;   // 🚶 ANDAR: MaxSpeed é linear (walk = RelativeSpeed × MaxSpeed) → ×m exato.
                return;
            }

            // 🏃 CORRER — 074/F3 (2026-07-18): o getter SprintingSpeed entra em num2 = (Physical.SprintSpeed ×
            // SprintingSpeed + 1) × limit (MovementContext:2547); o +1f DILUI um ×m cru (ex.: ×0.90 vira ~−8%, e
            // varia com peso/stamina via Physical.SprintSpeed). Compensa p/ num2 receber m EXATO:
            //   (S × patched + 1) = m × (S × V + 1)  →  patched = m·V + (m−1)/S,  S = Physical.SprintSpeed (live).
            // Seguro: SprintingSpeed tem UM ÚNICO consumidor (o num2 do sprint) → a compensação não vaza.
            var s = p.Physical.SprintSpeed;
            result = s > 0.0001f ? m * result + (m - 1f) / s : result * m;
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
        ClassMoveSpeed.Apply(__instance, ref __result, isSprint: false);
    }
}

/// <summary>
///     🏃 Velocidade de CORRER. <c>SprintingSpeed</c> (:912) é computado puro — o análogo do <c>MaxSpeed</c> para o
///     sprint (alimenta o alvo <c>num2</c> de <c>SprintAcceleration</c>, :2547). Postfix-mult aqui é estável.
///     ⚠️ NÃO patchar <c>SprintSpeed</c> (o campo de estado <c>SprintSpeed_1</c>) — foi a fonte do loop antigo.
///     074/F3: o <c>+1f</c> na fórmula do alvo diluiria um ×m cru (×0.9 ≈ −8% em vez de −10%); <see cref="ClassMoveSpeed"/>
///     compensa (<c>isSprint: true</c>) para a redução/aumento efetivo bater o fator configurado.
///     ⚠️ A compensação faz este getter retornar um valor PRÉ-DISTORCIDO, válido SÓ dentro da fórmula do num2
///     (<c>S·x+1</c>). Ela depende do invariante "SprintingSpeed tem 1 único consumidor" — se um update do EFT
///     adicionar um 2º leitor que use o valor direto, ele receberia lixo. Re-verificar a cada bump de versão do EFT
///     (junto da auditoria de eficácia 074).
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
        ClassMoveSpeed.Apply(__instance, ref __result, isSprint: true);
    }
}

// 079: OverladenInertiaPatch REMOVIDO — substituído pela Lebre (item 081).

/// <summary>
///     🔻 Rooted (Caçador) — −15% de velocidade enquanto MIRA (ADS). 074/F1 (2026-07-18, auditoria de eficácia):
///     o lever ANTIGO (Postfix no MaxSpeed) era INERTE — em mira a velocidade é governada pelo TETO DE MIRA
///     (<c>StateSpeedLimit</c>, o MIN dos <c>SpeedLimits</c>), que já é menor que o MaxSpeed reduzido → o
///     <c>min()</c> nunca pegava o Rooted. Lever CERTO: o <c>FirearmController</c> chama
///     <c>MovementContext.SetAimingSlowdown(isAiming, slow=0.33+AimMovementSpeed)</c> (FirearmController:12155),
///     que registra <c>slow</c> como o limite <c>ESpeedLimit.Aiming</c> — o teto que MANDA em ADS. Prefix escala
///     <c>slow ×0.85</c> ANTES do <c>AddStateSpeedLimit</c> → teto de mira 15% menor = −15% de velocidade em ADS.
///     Gate: MovementContext do MainPlayer (bots têm o seu → gate de instância obrigatório, regra 075) + Caçador.
///     <para>Idempotente: <c>AddStateSpeedLimit</c> só grava se a chave <c>Aiming</c> ainda não existe (MovementContext:1674)
///     → o Prefix escala 1× por aim-in, sem empilhar entre frames.</para>
/// </summary>
internal class RootedAimSlowdownPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.SetAimingSlowdown));
    }

    [PatchPrefix]
    private static void Prefix(MovementContext __instance, bool isAiming, ref float slow)
    {
        try
        {
            if (!isAiming || PerksConfig.RootedEnabled?.Value != true)
            {
                return;
            }

            // 075: SÓ o MovementContext do player local — bots também miram e chamam SetAimingSlowdown no SEU
            // MovementContext, então o gate de INSTÂNCIA é obrigatório (senão o Caçador-host afetaria todo bot mirando).
            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.MovementContext))
            {
                return;
            }

            if (SkillMultipliers.IsLocalClass(EClassId.Hunter))
            {
                slow *= PerksConfig.RootedAdsSpeed?.Value ?? 1f;   // 0.85 → teto de mira 15% menor
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] rooted falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     🔧 Execution (Furtivo) — +10% de velocidade com a MELEE na mão, na CORRIDA/SPRINT. 074/F2 (2026-07-18,
///     auditoria de eficácia): o lever antigo (Postfix no MaxSpeed, em ClassMoveSpeed.Apply) era CLAMPADO — a
///     corrida satura o TETO ABSOLUTO do SpeedLimiter (<c>GClass2175</c>, ~4.6 m/s Run) que NÃO lê MaxSpeed.
///     Aqui subimos o próprio cap: Postfix em <c>GClass2175.method_1</c> (retorna o cap em m/s, lido por
///     <c>Update → Speed → OnSpeedChanged → CharacterController.SpeedLimit</c>) — multiplica o cap ×1.10. O ANDAR
///     segue no branch do MaxSpeed (que morde abaixo do cap); os dois cobrem andar+correr sem compor no sprint.
///     Gate: o SpeedLimiter do MainPlayer (cada player/bot tem o seu → gate de INSTÂNCIA, regra 075) + Furtivo + faca.
///     <para>⚠️ FRÁGIL / version-specific: <c>GClass2175</c>/<c>method_1</c> são nomes OBFUSCADOS. Se um update do
///     EFT renomear, <c>AccessTools.Method</c> devolve null e o <c>Enable</c> (try/catch no Plugin) degrada
///     gracioso — Execution-sprint volta a inerte, SEM crash (o andar segue funcionando). Re-achar: o campo
///     <c>MovementContext.SpeedLimiter</c> e o método que retorna o cap em m/s
///     (<c>Lerp(MinSpeed, MaxSpeed, str/max) × Lerp(sprintLower, 1, walk)</c>).</para>
/// </summary>
internal class ExecutionSpeedCapPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GClass2175), nameof(GClass2175.method_1));
    }

    [PatchPostfix]
    private static void Postfix(GClass2175 __instance, ref float __result)
    {
        try
        {
            if (PerksConfig.ExecutionSpeedEnabled?.Value != true)
            {
                return;
            }

            // 075: cada player/bot tem o SEU SpeedLimiter (campo de MovementContext) → gate de INSTÂNCIA obrigatório.
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.MovementContext?.SpeedLimiter))
            {
                return;
            }

            if (SkillMultipliers.IsLocalClass(EClassId.Stealth) && p.HandsController is Player.KnifeController)
            {
                __result *= PerksConfig.ExecutionMoveSpeed?.Value ?? 1f;   // +10% no cap absoluto (m/s)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] execution speed cap falhou: {ex.Message}");
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
