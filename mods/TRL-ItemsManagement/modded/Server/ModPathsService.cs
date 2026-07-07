using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;   // FileUtil

namespace TRLItemsManagement;

/// <summary>
///     Singleton, sole caller of <c>fileUtil.GetModPath("TRL-ItemsManagement")</c> in this mod. Every
///     controller/service that needs a path inside the mod's own install folder (<c>user/mods/TRL-
///     ItemsManagement/</c>) injects this instead of calling <see cref="FileUtil.GetModPath"/> directly —
///     if the mod folder is ever renamed again, this is the ONE place to update (see the plan's "Risco de
///     renomear" checklist: a name mismatch here fails silently, no crash, just "not configured" logs).
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ModPathsService
{
    public string ModDir { get; }
    public string ConfigDir { get; }
    public string DataDir { get; }
    public string LogsDir { get; }

    public ModPathsService(FileUtil fileUtil)
    {
        ModDir = fileUtil.GetModPath("TRL-ItemsManagement");
        ConfigDir = Path.Combine(ModDir, "config");
        DataDir = Path.Combine(ModDir, "data");
        LogsDir = Path.Combine(ModDir, "logs");
    }
}
