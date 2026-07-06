namespace TarkovRedLine.Server;

/// <summary>
/// Constantes que diferenciam o build de HOMOLOG do de PRODUÇÃO (compilação condicional).
/// Build normal (prod): tudo vazio/padrão — comportamento idêntico ao atual.
/// Build homolog (dotnet build -p:Homolog=true): rotas prefixadas, pasta/arquivos próprios.
/// Permite os dois mods coexistirem no mesmo server sem colidir.
/// </summary>
internal static class ModRouting
{
#if HOMOLOG
    public const string RoutePrefix = "homolog/";          // ex.: [Route(ModRouting.RoutePrefix + "launcher/mods")]
    public const string UpdaterFolderName = "Launcher-Updater-Homolog";
    public const string StateSuffix = ".homolog";           // ex.: player_ips.homolog.json
    public const string ModGuid = "com.saraiva.tarkovredline.homolog";
    public const string ModName = "TarkovRedLine-ServerMod-Homolog";
#else
    public const string RoutePrefix = "";
    public const string UpdaterFolderName = "Launcher-Updater";
    public const string StateSuffix = "";
    public const string ModGuid = "com.saraiva.tarkovredline";
    public const string ModName = "TarkovRedLine-ServerMod";
#endif
}
