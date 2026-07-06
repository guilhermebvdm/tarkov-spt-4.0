using EFT;                  // SkillManager, WeaponSkillClass
using EFT.InventoryLogic;   // Weapon

namespace CustomClasses.Client;

/// <summary>
///     Item 058 — mapeamento arma-em-mãos → skill de maestria (Perna 2, efeito por nível). Categorias
///     alcançáveis com arma NA MÃO: smg → SMG · machinegun → LMG (a única HMG real é estacionária de mapa —
///     DEFERIDA, ver asbuild) · grenadeLauncher → Launcher (standalone). O underbarrel NÃO passa por aqui
///     (tratado no <see cref="UnderbarrelMasteryXpPatch"/> via método dedicado do disparo).
///     ref: SkillManager.cs:1306-1320 (campos WeaponSkillClass) · §10 da spec técnica do 058.
/// </summary>
internal static class WeaponMastery
{
    /// <summary>Skill de maestria da arma em mãos. Null se categoria fora do alvo (ou args nulos).</summary>
    internal static WeaponSkillClass? SkillForHeld(SkillManager? skills, Weapon? weapon)
    {
        if (skills == null || weapon == null)
        {
            return null;
        }

        return weapon.WeapClass switch
        {
            "smg" => skills.SMG,                 // ref: SkillManager.cs:1306
            "machinegun" => skills.LMG,          // ref: SkillManager.cs:1314 (HMG deferida — só estacionária)
            "grenadeLauncher" => skills.Launcher, // ref: SkillManager.cs:1318
            _ => null,
        };
    }
}
