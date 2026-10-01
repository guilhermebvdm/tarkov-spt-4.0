// SPT 4.0 compatibility layer for the ORBIT server mod (written against the SPT 4.1 server API).
// ISptLogger<T> lives in SPTarkov.Server.Core.Models.Utils in 4.0; 4.1 moved it to SPTarkov.Common.Models.Logging.
global using SPTarkov.Server.Core.Models.Utils;
global using Orbit.Server.Compat;

using MudBlazor;

// The 4.1 namespace does not exist in the 4.0 assemblies. Declaring it empty keeps the upstream
// `using SPTarkov.Common.Models.Logging;` directives compiling without touching every file.
namespace SPTarkov.Common.Models.Logging { }

namespace Orbit.Server.Compat
{
    internal static class MudBlazorCompat
    {
        // SPT 4.0 ships MudBlazor 8.13, where the message box entry point is IDialogService.ShowMessageBox.
        // The upstream pages call the later ShowMessageBoxAsync name with the same arguments.
        public static Task<bool?> ShowMessageBoxAsync(
            this IDialogService dialogs,
            string? title,
            string message,
            string yesText = "OK",
            string? noText = null,
            string? cancelText = null,
            DialogOptions? options = null
        ) => dialogs.ShowMessageBox(title, message, yesText, noText, cancelText, options);
    }
}

namespace Orbit.Server.Compat
{
    internal static class Spt40Paths
    {
        /// <summary>Name of the folder this mod was installed in, under user/mods.</summary>
        public static string ModFolderName { get; } =
            Path.GetFileName(Path.GetDirectoryName(typeof(Spt40Paths).Assembly.Location)!
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        /// <summary>
        /// Addon folder as seen from the game root. SPT 4.0 keeps the server in SPT/ (4.1 uses SPT_Runtime/),
        /// and the mod reads its addons from wherever its DLL is, so exported preset ZIPs must carry that path.
        /// </summary>
        public static string AddonFolder { get; } = $"SPT/user/mods/{ModFolderName}/addon";
    }
}
