using SPTarkov.Server.Core.Models.Utils;

namespace CompoundingPerf.Features;

/// <summary>
/// S1 control surface — just a kill-switch flag now. The actual coalescing is implemented
/// by <see cref="CoalescingSaveServer"/>, which SPT.DI registers via <c>TypeOverride</c>.
/// At construction time the override is in place but inert; this class flips
/// <see cref="IsEnabled"/> on at <c>OnLoad</c> if the user's config has S1 enabled.
/// </summary>
public static class ProfileSaveDebouncer
{
    /// <summary>When true, <see cref="CoalescingSaveServer.SaveProfileAsync"/> routes through
    /// the trailing-edge coalescer. When false, calls pass straight through to the base
    /// implementation. Read on every save call — flipping this at runtime is safe.</summary>
    public static volatile bool IsEnabled = false;

    public static void Apply(ProfileSaveDebouncerOptions options, ISptLogger<CompoundingPerfMod> logger)
    {
        if (options.Enabled)
        {
            IsEnabled = true;
            logger.Success("[CompoundingPerf/S1] profile-save coalescer ACTIVE — DI override of SaveServer in place, trailing-edge semantics");
        }
        else
        {
            IsEnabled = false;
            logger.Info("[CompoundingPerf/S1] profile-save coalescer disabled in config — SaveServer behaves as built-in");
        }
    }
}
