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
