using System;
using System.Linq.Expressions;   // ref: AUD-01-04 — accessor do emissor compilado 1× (molde do SainSoundPatch)
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     083 — 👻 <b>Morte Silenciosa</b> (Furtivo): a faca não faz SOM (sacar + golpe + acerto). Prefix ÚNICO no funil
///     de áudio <c>BaseSoundPlayer.PlayClip</c> → <c>return false</c> quando o emissor está com faca E é da classe
///     Furtivo. Um só ponto cobre deploy + swing + hit (tudo é roteado por eventos de animação para o mesmo
///     <c>PlayClip</c>; o TIRO de arma de fogo NÃO passa por aqui — sai por outro caminho no WeaponSoundPlayer).
///     <para>
///     <b>Coop (Fika):</b> cobre atacante LOCAL e peer sem protocolo novo. O peer usa
///     <c>ObservedKnifeController : Player.KnifeController</c> sobre <c>ObservedPlayer : … : Player</c>, inicializado
///     com um <b>PlayerBridge</b> cujo <c>iPlayer</c> é um <c>EFT.Player</c> REAL (não o <c>ObserverBridge</c>/
///     <c>ObservedPlayerView</c> do observed-view base do SPT, que é morto sob Fika) — por isso o guard
///     <c>is Player</c> basta e resolve a classe do EMISSOR (<see cref="ClassIdentities.ClassIdOf"/>). ⚠️ A
///     cobertura de peer DEPENDE do Fika instanciar controllers de Player real; num cenário SEM Fika (observed-view
///     puro), a faca de um peer/bot cairia no <c>ObserverBridge</c> → <c>iPlayer</c> é <c>ObservedPlayerView</c>
///     (não <c>Player</c>) → o guard <c>is Player</c> falha → som AUDÍVEL. Isso é um gap de cobertura na direção
///     SEGURA (nunca silencia indevidamente / nunca vaza), não um leak — NÃO relaxar o guard para "consertar".
///     </para>
///     <para>
///     <b>IA:</b> nada a suprimir. O EFT NÃO gera evento de audição de melee (o <c>AISoundType</c> só tem
///     step/silencedGun/gun; não há call-site de <c>BotEventHandler.PlaySound</c> para faca) — bots só reagem ao
///     DANO do golpe, nunca ao som. O "AI crítico" do board já é verdade no vanilla. ⚠️ Incógnita: se o SAIN
///     sintetizar percepção de melee própria (não verificável no decompile do EFT) — validar in-game.
///     </para>
///     <para>
///     <b>Gate obrigatório:</b> <c>WeaponSoundPlayer : BaseSoundPlayer</c> herda <c>PlayClip</c> — sem o descarte
///     <c>is WeaponSoundPlayer</c> mutaríamos mag-in/out, ferrolho etc. de armas de fogo. Ordem barata→cara:
///     perk-off → arma-de-fogo → emissor → é-faca → classe. O emissor sai do campo <c>playersBridge</c> (protected,
///     tipo <c>IObserverToPlayerBridge</c> num assembly NÃO referenciável pelo mod → <c>FieldRef</c> tipado é
///     impossível, daí a reflection crua cacheada). Granada/meds/quick-use também são <c>BaseSoundPlayer</c> puro,
///     então quem os barra é EXCLUSIVAMENTE o teste <c>Item is KnifeItemClass</c> (não o descarte de WeaponSoundPlayer).
///     </para>
/// </summary>
internal class SilentKnifePatch : ModulePatch
{
    // 'playersBridge' é protected + tipado por IObserverToPlayerBridge (assembly não referenciável) → o
    // FieldInfo/PropertyInfo precisam ser resolvidos por reflexão. Mas a INVOCAÇÃO não precisa ser reflexiva.
    //
    // ref: AUD-01-04 — era `BridgeField.GetValue(inst)` + `IPlayerProp.GetValue(bridge)` A CADA clip de som
    // não-arma de qualquer entidade (faca, granada, meds, quick-use). `PropertyInfo.GetValue` passa por
    // `MethodInfo.Invoke`, ~1 ordem de grandeza mais caro que um acesso direto. O padrão certo já existe no
    // mod, no arquivo vizinho: `SainSoundPatch` compila o getter 1× com Expression.Lambda "p/ tirar o
    // reflection do hot-path (review)" (ClassSoundPatches.cs:352-355). Aqui só não tinha sido aplicado.
    //
    // Um throw aqui NÃO pode virar TypeInitializationException e derrubar o patch → try/catch + null = inerte.
    private static readonly Func<BaseSoundPlayer, object?>? EmitterOf = BuildEmitterAccessor();

    /// <summary>Compila <c>sp => ((IObserverToPlayerBridge)sp.playersBridge).iPlayer</c> uma única vez.</summary>
    private static Func<BaseSoundPlayer, object?>? BuildEmitterAccessor()
    {
        try
        {
            var field = AccessTools.Field(typeof(BaseSoundPlayer), "playersBridge");
            var prop = field != null ? AccessTools.Property(field.FieldType, "iPlayer") : null;
            if (field == null || prop == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (083/AUD-01-04) 'playersBridge'/'iPlayer' não resolvidos — Morte Silenciosa inerte.");
                return null;
            }

            var sp = Expression.Parameter(typeof(BaseSoundPlayer), "sp");
            var body = Expression.Convert(Expression.Property(Expression.Field(sp, field), prop), typeof(object));
            return Expression.Lambda<Func<BaseSoundPlayer, object?>>(body, sp).Compile();
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"[CustomClasses] (083/AUD-01-04) accessor do emissor não compilado — Morte Silenciosa inerte: {ex.Message}");
            return null;
        }
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseSoundPlayer), nameof(BaseSoundPlayer.PlayClip));
    }

    [PatchPrefix]
    private static bool Prefix(BaseSoundPlayer __instance)
    {
        try
        {
            if (PerksConfig.SilentKnifeEnabled?.Value != true || EmitterOf == null)
            {
                return true;   // perk off ou accessor não compilado → som normal
            }

            // (1) arma de fogo (WeaponSoundPlayer herda PlayClip) → descarta sem custo (a maioria das chamadas).
            if (__instance is WeaponSoundPlayer)
            {
                return true;
            }

            // (2) emissor — local OU peer Fika: ambos são EFT.Player real (ObservedPlayer : … : Player via
            //     PlayerBridge). ref: AUD-01-04 — delegate compilado, não 2 chamadas reflexivas.
            if (EmitterOf(__instance) is not Player emitter)
            {
                return true;
            }

            // (3) só quando a arma na mão é FACA (o Item cobre KnifeController E QuickKnifeKickController/coronhada,
            // e é o ÚNICO filtro que barra granada/meds/quick-use, que também são BaseSoundPlayer puro).
            if (emitter.HandsController?.Item is not KnifeItemClass)
            {
                return true;
            }

            // (4) classe do EMISSOR (local via SkillMultipliers, peer via rota 057) — silencia só o Furtivo.
            // ref: AUD-01-02 · PA-03-01 — comparação de int; ClassIdOf é o único resolvedor.
            if (ClassIdentities.ClassIdOf(emitter) == EClassId.Stealth)
            {
                return false;   // 🔇 não toca o clip (deploy/golpe/acerto)
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (083) morte silenciosa falhou: {ex.Message}");
            return true;   // erro → nunca engole o som (fail-open)
        }
    }
}
