using SPTarkov.DI.Annotations;

namespace TRLItemsManagement;

/// <summary>
///     Paths into the SPT server's OWN data (not the mod's) — <c>SPT_Data/</c>, relative to the server
///     process's working directory. Mirrors <c>FileUtil.GetModPath</c>'s own convention (also a bare
///     relative path, e.g. <c>Path.Combine("user/mods/", modName)</c>) rather than resolving an absolute
///     path — the server is always launched with its own install folder as CWD (confirmed: <c>SPT.Server.exe</c>
///     under <c>D:\SPT\SPT</c>), so a relative path here resolves the same way FileUtil's does.
///     <para>
///     These are files the SPT server itself owns and reloads from disk on every boot — writing to them
///     (flea price/ban/flea-level controllers, Estágio 3) requires recomputing <see cref="ChecksDatPath"/>'s
///     manifest afterwards, or the next boot logs an integrity failure. See the plan's "Escritas fora da
///     pasta do mod (SPT_Data)" section.
///     </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class SptDataPathsService
{
    public string SptDataDir { get; } = Path.Combine("SPT_Data");
    public string RagfairConfigPath { get; } = Path.Combine("SPT_Data", "configs", "ragfair.json");
    public string GlobalsPath { get; } = Path.Combine("SPT_Data", "database", "globals.json");
    public string ItemsJsonPath { get; } = Path.Combine("SPT_Data", "database", "templates", "items.json");
    public string ChecksDatPath { get; } = Path.Combine("SPT_Data", "checks.dat");
    public string ImagesDir { get; } = Path.Combine("SPT_Data", "images");
}
