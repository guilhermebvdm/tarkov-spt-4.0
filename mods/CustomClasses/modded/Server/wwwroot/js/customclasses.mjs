// CustomClasses — UI preferences + Ctrl+S shortcut (item 035) + stash drag-and-drop (pointer-based).
//
// Plain globals (window.ccPrefs / window.ccGridDnd) — NOT RCL JS modules: the host does not guarantee
// the mod JS-module pipeline, so a plain global is the proven repo pattern (BaseLayout UI-03). Served at
// /CustomClasses-Server/js/customclasses.mjs by the wwwroot mount. Shipped as .mjs (NOT .js/.ts) so the
// SPT ModValidator does not reject the install dir (modloader-is-old-js-mod, QA 2026-06-12).
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

// ── Stash drag-and-drop (pointer-based) ──────────────────────────────────────────────────────────
// The HTML5 native DnD under Blazor Server never worked reliably here (drop OffsetX/Y depends on the
// element under the cursor + SignalR round-trip latency on dragstart). This is a pointer-event drag with
// a ghost following the cursor; on drop it computes the target cell from the grid's own rect (always
// correct, scroll-safe) and calls back into .NET (StashGrid.MoveItemTo). One listener per grid, delegated
// from .cc-grid2d. The grid carries data-cell (px) and the items carry data-index (position in the list).
window.ccGridDnd = (function () {
    let active = null;

    function init(grid, dotNetRef) {
        if (!grid) { return; }
        // re-init safe: drop a previous binding before attaching a fresh one.
        dispose(grid);
        grid.__ccDnd = { ref: dotNetRef };
        grid.addEventListener('pointerdown', onDown);
    }

    function dispose(grid) {
        if (grid && grid.__ccDnd) {
            grid.removeEventListener('pointerdown', onDown);
            grid.__ccDnd = null;
        }
    }

    function cellPxOf(grid) {
        const v = parseFloat(grid.dataset.cell);
        return Number.isFinite(v) && v > 0 ? v : 36;
    }

    function onDown(e) {
        if (e.button !== 0) { return; }                       // left button only
        if (e.target.closest('.cc-grid2d__rotate')) { return; } // the ⟳ button is not a drag handle
        const item = e.target.closest('.cc-grid2d__item');
        const grid = e.currentTarget;
        if (!item || !grid.__ccDnd) { return; }
        const index = parseInt(item.dataset.index, 10);
        if (Number.isNaN(index)) { return; }

        const rect = item.getBoundingClientRect();
        active = {
            grid, item, index, ref: grid.__ccDnd.ref,
            startX: e.clientX, startY: e.clientY,
            grabDX: e.clientX - rect.left, grabDY: e.clientY - rect.top,
            w: rect.width, h: rect.height, moved: false, ghost: null,
        };
        window.addEventListener('pointermove', onMove);
        window.addEventListener('pointerup', onUp);
    }

    function onMove(e) {
        if (!active) { return; }
        if (!active.moved) {
            // small threshold so a plain click still reaches Blazor's @onclick (open editor).
            if (Math.hypot(e.clientX - active.startX, e.clientY - active.startY) < 5) { return; }
            active.moved = true;
            const g = active.item.cloneNode(true);
            // mantém as classes do item (ícone/qty/layout) e marca como ghost (position:fixed via CSS).
            g.classList.add('cc-grid2d__ghost');
            g.removeAttribute('data-index');
            g.style.width = active.w + 'px';
            g.style.height = active.h + 'px';
            document.body.appendChild(g);
            active.ghost = g;
            active.item.classList.add('cc-grid2d__item--dragging');
            active.grid.classList.add('cc-grid2d--dragging');
        }
        active.ghost.style.left = (e.clientX - active.grabDX) + 'px';
        active.ghost.style.top = (e.clientY - active.grabDY) + 'px';
    }

    function onUp(e) {
        if (!active) { return; }
        const a = active;
        active = null;
        window.removeEventListener('pointermove', onMove);
        window.removeEventListener('pointerup', onUp);
        if (a.ghost) { a.ghost.remove(); }
        a.item.classList.remove('cc-grid2d__item--dragging');
        a.grid.classList.remove('cc-grid2d--dragging');
        if (!a.moved) { return; }                              // it was a click — let @onclick handle it

        // a drag happened → swallow the click that the browser fires next so we don't open the editor.
        const swallow = (ev) => { ev.preventDefault(); ev.stopPropagation(); };
        a.item.addEventListener('click', swallow, { capture: true, once: true });
        setTimeout(() => a.item.removeEventListener('click', swallow, true), 300);

        // target cell = item's top-left (cursor − grab offset) relative to the grid, snapped to the cell.
        const cellPx = cellPxOf(a.grid);
        const gridRect = a.grid.getBoundingClientRect();
        const cx = Math.round((e.clientX - a.grabDX - gridRect.left) / cellPx);
        const cy = Math.round((e.clientY - a.grabDY - gridRect.top) / cellPx);
        a.ref.invokeMethodAsync('MoveItemTo', a.index, cx, cy);
    }

    return { init, dispose };
})();
