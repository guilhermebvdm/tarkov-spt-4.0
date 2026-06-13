# 012 — Identidade da classe no menu + tela de Skills · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [012-identidade-menu-skills-01-spec.md](012-identidade-menu-skills-01-spec.md)
**Criado:** 2026-06-08

> Client-only. Reusa a base do **011** (`SkillMultipliers.ClassName/IconFile/NameColor`, `ClassIconCache`, `ClassIdentityView`). Refs do EFT confirmadas via ilspycmd (dump parcial). Posições finas de UI validadas em runtime/playtest (UI é visual).

## 1. Estratégia

Dois patches `Postfix`, ambos montando o "selo" via `ClassIdentityView.BuildOrRefresh` (011) com dados já carregados (`SkillMultipliers`):

- **Feature 1 (menu):** `MenuClassIdentityPatch` — postfix em `EFT.UI.MenuScreen.Show(Profile, MatchmakerPlayerControllerClass, ESessionMode)`. Detecta o Menu-Overhaul (`com.moxopixel.menuoverhaul`); como o MO cria o painel `MainMenuPlayerModelView` de forma **assíncrona e idempotente** (`PlayerProfileFeaturesPatch.cs:231`), usa-se uma **coroutine** (hospedada no `Plugin`) que espera o painel existir e então ancora o selo perto dele; **sem MO**, ancora num **canto fixo** do `MenuScreen`.
- **Feature 3 (Skills):** `SkillsScreenIdentityPatch` — postfix em `EFT.UI.SkillsAndMasteringScreen.Show(Profile, InventoryController, IHealthController)`. Ancora o selo no **topo** da tela (perto do `_playerExperiencePanel`).

Ambos: guard de `Plugin.ShowOnUi`/novo `ShowClassIdentity`, `SkillMultipliers.EnsureLoaded()`, e **só renderizam se `ClassName != null`** (classe do mod). Idempotência via `Find` (reuso do `ClassIdentityView`). Try/catch + log (não quebrar menu/tela).

## 2. Pontos de patch

| Alvo (Assembly real via ilspycmd) | Tipo | Motivo |
|---|---|---|
| `EFT.UI.MenuScreen.Show(Profile, MatchmakerPlayerControllerClass, ESessionMode)` | Postfix | montar o selo no menu principal |
| `EFT.UI.SkillsAndMasteringScreen.Show(Profile, InventoryController, IHealthController)` | Postfix | montar o selo no topo da tela de Skills |

Confirmado: `MenuScreen : EftScreen<...>`; `SkillsAndMasteringScreen : UIElement` com `_playerExperiencePanel` (PlayerExperiencePanel) e `_skillsTab`. MO: GO `MainMenuPlayerModelView` (filho do MenuScreen), com `BottomField`. Detecção MO: `BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.moxopixel.menuoverhaul")`.

## 3. Novas propriedades F12

| Seção | Nome (EN) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| `General` | `ShowClassIdentity` | bool | `true` | Mostra o ícone + nome da classe no menu e no topo da tela de Skills. / Show class icon + name in the menu and Skills screen. |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/Patches/MenuClassIdentityPatch.cs` | CRIAR | postfix `MenuScreen.Show`; coroutine; integra MO ou canto fixo. |
| `modded/Client/Patches/SkillsScreenIdentityPatch.cs` | CRIAR | postfix `SkillsAndMasteringScreen.Show`; selo no topo. |
| `modded/Client/UI/ClassIdentityView.cs` | MODIFICAR | + helper `ResolveColor(hex)` (default se nulo/inválido) + overload que pega cor/fonte de `SkillMultipliers`. |
| `modded/Client/Plugin.cs` | MODIFICAR | + `Instance` (coroutine host) + `ShowClassIdentity` + registra os 2 patches. |
| `mods/CustomClasses/PROPRIEDADES.md` | MODIFICAR | + `ShowClassIdentity`. |

## 5. Stubs de código

### Plugin.cs (trechos)

```csharp
internal static Plugin? Instance;       // host de coroutine (BaseUnityPlugin é MonoBehaviour)
internal static bool ShowClassIdentity = true;

// Awake:
Instance = this;
ShowClassIdentity = Config.Bind("General", "ShowClassIdentity", true,
    "Mostra o ícone + nome da classe no menu e no topo da tela de Skills. / Show class icon + name in the menu and Skills screen.").Value;
new MenuClassIdentityPatch().Enable();
new SkillsScreenIdentityPatch().Enable();
```

### ClassIdentityView.cs (+ helper de cor)

```csharp
public static Color ResolveColor(string? hex, Color fallback)
    => !string.IsNullOrWhiteSpace(hex) && ColorUtility.TryParseHtmlString(hex, out var c) ? c : fallback;
```

### MenuClassIdentityPatch.cs

```csharp
using System;
using System.Collections;
using System.Reflection;
using BepInEx.Bootstrap;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>(012) Selo da classe no menu principal. Integra ao Menu-Overhaul ou usa canto fixo.</summary>
internal class MenuClassIdentityPatch : ModulePatch
{
    private const string SealName = "CC_ClassSeal_Menu";
    private const string MoGuid = "com.moxopixel.menuoverhaul";

    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(MenuScreen), nameof(MenuScreen.Show));

    [PatchPostfix]
    private static void Postfix(MenuScreen __instance)
    {
        if (!Plugin.ShowOnUi || !Plugin.ShowClassIdentity) return;
        try
        {
            SkillMultipliers.EnsureLoaded();
            if (string.IsNullOrEmpty(SkillMultipliers.ClassName)) return;   // edition vanilla → nada
            if (Plugin.Instance != null) Plugin.Instance.StartCoroutine(PlaceCoroutine(__instance));
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] menu identity falhou: {ex.Message}"); }
    }

    private static IEnumerator PlaceCoroutine(MenuScreen menu)
    {
        var hasMo = Chainloader.PluginInfos.ContainsKey(MoGuid);
        Transform? parent = null;

        // Com MO: espera o painel do jogador (MainMenuPlayerModelView) existir (criado async/idempotente).
        for (var i = 0; i < 30 && hasMo && parent == null; i++)   // ~30 frames de tolerância
        {
            var pmv = GameObject.Find("MainMenuPlayerModelView");
            if (pmv != null) { parent = (pmv.transform.Find("BottomField") ?? pmv.transform); break; }
            yield return null;
        }

        // Sem MO (ou timeout): canto fixo do MenuScreen.
        parent ??= menu.transform;
        var fixedCorner = parent == menu.transform;

        try
        {
            var font = FindMenuFont(parent) ?? TMP_Settings.defaultFontAsset;
            var color = ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, Color.white);
            var go = ClassIdentityView.BuildOrRefresh(parent, SealName, SkillMultipliers.ClassName!,
                SkillMultipliers.IconFile, color, font);

            if (fixedCorner)
            {
                var rt = (RectTransform)go.transform;       // canto superior-esquerdo (ajustável)
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(40f, -40f);
            }
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] menu seal place falhou: {ex.Message}"); }
    }

    private static TMP_FontAsset? FindMenuFont(Transform t)
    {
        var any = t.GetComponentInChildren<TextMeshProUGUI>(true);
        return any != null ? any.font : null;
    }
}
```

### SkillsScreenIdentityPatch.cs

```csharp
using System;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>(012) Selo da classe no topo da tela de Skills (SkillsAndMasteringScreen).</summary>
internal class SkillsScreenIdentityPatch : ModulePatch
{
    private const string SealName = "CC_ClassSeal_Skills";

    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(SkillsAndMasteringScreen), nameof(SkillsAndMasteringScreen.Show));

    [PatchPostfix]
    private static void Postfix(SkillsAndMasteringScreen __instance)
    {
        if (!Plugin.ShowOnUi || !Plugin.ShowClassIdentity) return;
        try
        {
            SkillMultipliers.EnsureLoaded();
            if (string.IsNullOrEmpty(SkillMultipliers.ClassName)) return;

            var font = __instance.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
            var color = ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, Color.white);
            var go = ClassIdentityView.BuildOrRefresh(__instance.transform, SealName, SkillMultipliers.ClassName!,
                SkillMultipliers.IconFile, color, font, iconSize: 40f, fontSize: 24f);

            // topo-esquerda da tela (ajustável no playtest, perto da barra de XP/_playerExperiencePanel)
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(40f, -20f);
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] skills identity falhou: {ex.Message}"); }
    }
}
```

## 6. Fluxo de dados

```
[server 011] rota → { className, iconFile, nameColor, ... }
[client] SkillMultipliers.EnsureLoaded() → ClassName/IconFile/NameColor
MenuScreen.Show ─Postfix→ coroutine: acha MainMenuPlayerModelView (MO) | canto fixo (sem MO)
   → ClassIdentityView.BuildOrRefresh(parent, "CC_ClassSeal_Menu", ClassName, IconFile, ResolveColor(NameColor), font)
SkillsAndMasteringScreen.Show ─Postfix→ ClassIdentityView.BuildOrRefresh(__instance.transform, "CC_ClassSeal_Skills", ...)
   → ClassIconCache.Get(IconFile) (011) resolve o sprite
```

## 7. Riscos e dependências

- **Timing/ordem com o MO:** coroutine espera o `MainMenuPlayerModelView` (até ~30 frames). MO cria o painel 1x (idempotente). Se não aparecer (sem MO/timeout) → canto fixo. Idempotência (`Find` do selo) evita duplicar entre aberturas.
- **Posição fina:** os `anchoredPosition` são chutes iniciais — **ajustar no playtest** (UI visual). Não cravar pixel-perfect.
- **Fonte:** com MO/Skills herda de um TMP vizinho; fallback `TMP_Settings.defaultFontAsset`.
- **Lifecycle:** patches de menu/tela; sem GameWorld. Não acumular GameObjects (idempotência). Sprites são do cache compartilhado do 011 (`Dispose` no teardown já existe).
- **Coroutine concorrente:** múltiplos `Show` → várias coroutines; a idempotência (`Find` do selo) garante 1 selo. (Opcional: flag p/ não iniciar concorrente.)
- **Não quebrar o menu:** todos os corpos em try/catch + log — falha do selo nunca trava o menu/tela.

## 8. Checklist de implementação

- [ ] `Plugin`: `Instance` + `ShowClassIdentity` + registrar os 2 patches.
- [ ] `ClassIdentityView.ResolveColor`.
- [ ] `MenuClassIdentityPatch` (coroutine MO / canto fixo).
- [ ] `SkillsScreenIdentityPatch` (topo da tela).
- [ ] `PROPRIEDADES.md` + `ShowClassIdentity`.
- [ ] `/compile-mod` 0 warn/err (build local + instala).
- [ ] Playtest: menu (com MO → perto do painel; sem MO → canto) e tela de Skills mostram ícone+nome; edition vanilla → nada; reabrir não duplica; ajustar posições.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Spec técnica criada via `/create-technical-spec` (MenuScreen.Show + SkillsAndMasteringScreen.Show; coroutine p/ MO; reuso do 011) |
