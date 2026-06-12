// CustomClasses — UI preferences + Ctrl+S shortcut (item 035).
//
// Plain <script src> (NOT a module) — served at /CustomClasses-Server/js/customclasses.js by the
// same wwwroot mount as css/icons (CustomClassesMetadata : IModWebMetadata). Mirrors the MudBlazor
// plain-script pattern (BaseLayout UI-03). No bundler, no import() — the host does not guarantee the
// RCL JS-module pipeline for mods, so a plain global (window.ccPrefs) is the proven repo pattern.
window.ccPrefs = (function () {
    // localStorage access is wrapped: private/incognito or a denied quota throws on access, and a
    // single-user local tool must degrade to "no persistence" rather than break the page (PA-035-03).
    function get(key) { try { return window.localStorage.getItem(key); } catch { return null; } }
    function set(key, val) { try { window.localStorage.setItem(key, val); } catch { /* quota/denied */ } }
    function remove(key) { try { window.localStorage.removeItem(key); } catch { } }

    let saveHandler = null;

    // Registers ONE global keydown listener that turns Ctrl/Cmd+S into a .NET callback. The edit page
    // (ClassEdit) owns the DotNetObjectReference and MUST unregister + dispose it in Dispose().
    function registerSaveShortcut(dotNetRef) {
        unregisterSaveShortcut();
        saveHandler = function (e) {
            // PA-R1-02 (defense in depth): even an orphaned handler (a leaked dispose) must NOT hijack
            // Ctrl+S outside an edit page. The ClassEdit dispose is STILL mandatory — this is a backstop.
            if (!window.location.pathname.includes('/edit')) { return; }
            if ((e.ctrlKey || e.metaKey) && (e.key === 's' || e.key === 'S')) {
                e.preventDefault();                          // suppress the browser "save page" dialog
                dotNetRef.invokeMethodAsync('OnSaveShortcut');
            }
        };
        // capture phase: run before the browser's own Ctrl+S handling so preventDefault wins.
        window.addEventListener('keydown', saveHandler, true);
    }

    function unregisterSaveShortcut() {
        if (saveHandler) {
            window.removeEventListener('keydown', saveHandler, true);
            saveHandler = null;
        }
    }

    return { get, set, remove, registerSaveShortcut, unregisterSaveShortcut };
})();
