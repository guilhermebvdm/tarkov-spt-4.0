using Microsoft.JSInterop;

namespace CustomClasses.Web;

/// <summary>
///     Item 035 — thin wrapper over <c>window.ccPrefs</c> (localStorage), see
///     wwwroot/js/customclasses.js. ALL calls must run AFTER the interactive circuit connects
///     (OnAfterRenderAsync(firstRender) or an event handler), NEVER during prerender (PA-035-02):
///     a prerender interop call throws InvalidOperationException. Every method swallows that +
///     JSException and falls back to the default — a missing/denied/corrupted key never breaks the
///     page (PA-035-03). No own state, no DI: the keys are constants and the methods are static.
/// </summary>
public static class UiPrefs
{
    // PA-R1-01: the drawer persists a PIN (Mini↔Persistent), not an Open flag — the Mini +
    // OpenMiniOnHover drawer has no stable Open state to bind.
    public const string DrawerPinned = "cc.ui.drawerPinned";   // "1" pinned (Persistent) / "0" Mini
    public const string EditTab = "cc.ui.editTab";             // active ClassEdit tab index (int)
    public const string ListSort = "cc.ui.listSort";           // "<label>|asc" / "<label>|desc"
    public const string MatrixToggles = "cc.ui.matrixToggles"; // "<showDisabled>|<showMultipliers>" e.g. "1|0"
    public const string SidebarFilter = "cc.ui.sidebarFilter"; // last sidebar filter text

    public static async Task<string?> GetAsync(IJSRuntime js, string key)
    {
        try
        {
            return await js.InvokeAsync<string?>("ccPrefs.get", key);
        }
        catch (JSException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;   // prerender / no JS circuit yet (PA-035-02)
        }
    }

    public static async Task SetAsync(IJSRuntime js, string key, string value)
    {
        try
        {
            await js.InvokeVoidAsync("ccPrefs.set", key, value);
        }
        catch (JSException) { }
        catch (InvalidOperationException) { }
    }

    public static async Task<int> GetIntAsync(IJSRuntime js, string key, int @default) =>
        int.TryParse(await GetAsync(js, key), out var v) ? v : @default;

    public static async Task<bool> GetBoolAsync(IJSRuntime js, string key, bool @default) =>
        await GetAsync(js, key) is { } s ? s == "1" : @default;
}
