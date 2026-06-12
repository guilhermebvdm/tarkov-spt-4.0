# 011 — Infra de identidade visual da classe · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [011-infra-identidade-visual-01-spec.md](011-infra-identidade-visual-01-spec.md)
**Criado:** 2026-06-08

> Item **híbrido**, base para 012/013. Server: schema + registry + rota. Client: cache de ícone (PNG→Sprite) + componente de "selo" reutilizável. Build: entrega dos PNGs ao client. Não há patch de UI aqui (vem no 012/013). Refs do EFT (UI) são confirmadas via ilspycmd no Assembly real; o dump em `references/eft-decompiled/` é parcial.

## 1. Estratégia

Estender o pipeline server→client do item 005/010 para carregar **identidade** (nome + ícone + cor) por classe, e dar ao client a base para desenhá-la:
- **Server:** `ClassDefinition` ganha `iconFile`/`nameColor`. Um `ClassVisualRegistry` (Singleton) registra **toda** classe (`name → iconFile?/nameColor?`), servindo de fonte-de-verdade de "esta edition é classe do mod?". A rota `/customclasses/skill-multipliers` passa a devolver `className`/`iconFile`/`nameColor` sempre que a edition estiver no `ClassVisualRegistry` (independente de ter `skillMultipliers`).
- **Client:** `SkillMultipliers` expõe `IconFile`/`NameColor`. `ClassIconCache` carrega PNG→Sprite (padrão do Menu-Overhaul) com cache e null-safety. `ClassIdentityView` monta o selo (Image + TextMeshProUGUI colorido), idempotente — consumido por 012/013.
- **Build:** `compile-mod.sh` (ramo client) copia `modded/Client/icons/` → `BepInEx/plugins/<MOD>/icons/` (simétrico ao `config/` que o server já copia).
- **Assets:** PNGs placeholder em `modded/Client/icons/` (gerados via PowerShell/System.Drawing).

## 2. Pontos de referência

| Símbolo | Fonte | Uso |
|---|---|---|
| `SkillMultipliersRouter` rota `/customclasses/skill-multipliers` | `modded/Server/SkillMultipliersRouter.cs` | devolver identidade |
| `SkillMultiplierRegistry` (Singleton) | `modded/Server/SkillMultiplierRegistry.cs` | modelo p/ o `ClassVisualRegistry` |
| `CustomClassesMod.RegisterClass` | `modded/Server/CustomClassesMod.cs:100+` | popular o registry visual |
| `SaveServer.GetProfile(sessionId).ProfileInfo.Edition` | (já usado no router) | edition do perfil |
| PNG→Sprite: `File.ReadAllBytes` + `Texture2D.LoadImage` + `Sprite.Create` | `mods/SPT-Menu-Overhaul/.../Helpers/LayoutHelpers.cs` | padrão de carga de imagem em BepInEx |
| `compile-mod.sh` ramo client (instala DLLs) / server (copia `config/`) | `.agents/scripts/compile-mod.sh:216-236` | ponto de extensão p/ copiar `icons/` |
| Path do plugin em runtime | `Path.GetDirectoryName(typeof(ClassIconCache).Assembly.Location)` | achar `icons/` no plugin |

## 3. Novas propriedades F12

Nenhuma neste item (o master switch `ShowClassIdentity` entra no 012).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/ClassDefinition.cs` | MODIFICAR | + `IconFile` (string?), + `NameColor` (string?). |
| `modded/Server/ClassVisualRegistry.cs` | CRIAR | Singleton `edition → (iconFile?, nameColor?)`; `Contains`/`Get`. Registra toda classe. |
| `modded/Server/CustomClassesMod.cs` | MODIFICAR | injeta `ClassVisualRegistry`; `RegisterClass` chama `Set(name, IconFile, NameColor)` sempre. |
| `modded/Server/SkillMultipliersResponse.cs` | MODIFICAR | + `iconFile`, + `nameColor`. |
| `modded/Server/SkillMultipliersRouter.cs` | MODIFICAR | className/icon/color quando `visualRegistry.Contains(edition)`. |
| `modded/Client/SkillMultipliers.cs` | MODIFICAR | parseia + expõe `IconFile`/`NameColor`; `Reset()` zera. |
| `modded/Client/UI/ClassIconCache.cs` | CRIAR | PNG→Sprite cacheado, sanitiza nome, null-safe, `Dispose`. |
| `modded/Client/UI/ClassIdentityView.cs` | CRIAR | factory idempotente do selo (Image+TMP colorido). |
| `modded/Client/Plugin.cs` | MODIFICAR | chama `ClassIconCache.Dispose()` no `OnDestroy` (teardown). |
| `modded/Client/icons/*.png` | CRIAR | placeholders (genérico + por classe). |
| `.agents/scripts/compile-mod.sh` | MODIFICAR | ramo client copia `modded/Client/icons/` → `plugins/<MOD>/icons/`. |
| `modded/Server/config/classes/_docs/exampleClass.jsonc` | MODIFICAR | documenta `iconFile`/`nameColor`. |

## 5. Stubs de código

### ClassDefinition.cs (trecho)

```csharp
[JsonPropertyName("iconFile")]
public string? IconFile { get; init; }    // nome do PNG (ex.: "cacador.png"); opcional

[JsonPropertyName("nameColor")]
public string? NameColor { get; init; }    // hex "#RRGGBB"; opcional
```

### ClassVisualRegistry.cs

```csharp
using SPTarkov.DI.Annotations;

namespace CustomClasses;

/// <summary>Item 011: identidade visual por classe (edition → ícone/cor). Registra TODA classe do mod,
/// mesmo sem ícone/cor — serve de fonte de "esta edition é classe do mod?" para o router.</summary>
[Injectable(InjectionType.Singleton)]
public class ClassVisualRegistry
{
    public sealed record Visual(string? IconFile, string? NameColor);

    private readonly Dictionary<string, Visual> _byEdition = new(StringComparer.Ordinal);

    public void Set(string edition, string? iconFile, string? nameColor)
        => _byEdition[edition] = new Visual(iconFile, nameColor);

    public bool Contains(string edition) => _byEdition.ContainsKey(edition);
    public Visual? Get(string edition) => _byEdition.TryGetValue(edition, out var v) ? v : null;
}
```

### CustomClassesMod.cs (trechos)

```csharp
// ctor: + ClassVisualRegistry visualRegistry

// em RegisterClass, sempre (após validar 'name'), antes de templates[name] = sides:
visualRegistry.Set(name, def.IconFile, def.NameColor);
```

### SkillMultipliersResponse.cs (trecho)

```csharp
[JsonPropertyName("iconFile")]  public string? IconFile { get; init; }
[JsonPropertyName("nameColor")] public string? NameColor { get; init; }
```

### SkillMultipliersRouter.cs (route)

```csharp
var edition = saveServer.GetProfile(sessionId)?.ProfileInfo?.Edition ?? string.Empty;
var isClass = visualRegistry.Contains(edition);
var mults = registry.Get(edition);
var visual = visualRegistry.Get(edition);
var dto = new SkillMultipliersResponse
{
    ClassName  = isClass ? edition : null,        // identidade mesmo sem multiplicadores
    IconFile   = visual?.IconFile,
    NameColor  = visual?.NameColor,
    Multipliers = mults,
};
return new ValueTask<string>(jsonUtil.Serialize(dto) ?? "{}");
```

### SkillMultipliers.cs (client — Payload + props)

```csharp
public static string? IconFile { get; private set; }
public static string? NameColor { get; private set; }
// Reset(): IconFile = NameColor = null;
// EnsureLoaded(): após desserializar, IconFile = payload.IconFile; NameColor = payload.NameColor;
// Payload: + [JsonProperty("iconFile")] string? IconFile {get;set;} + [JsonProperty("nameColor")] string? NameColor {get;set;}
```

### ClassIconCache.cs (client)

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>Item 011: carrega PNG → Sprite (padrão do Menu-Overhaul), com cache e null-safety.</summary>
internal static class ClassIconCache
{
    private static readonly Dictionary<string, Sprite?> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static string? _iconsDir;

    private static string IconsDir =>
        _iconsDir ??= Path.Combine(
            Path.GetDirectoryName(typeof(ClassIconCache).Assembly.Location) ?? ".", "icons");

    public static Sprite? Get(string? iconFile)
    {
        if (string.IsNullOrWhiteSpace(iconFile)) return null;
        var name = Path.GetFileName(iconFile);                 // sanitiza path traversal
        if (Cache.TryGetValue(name, out var cached)) return cached;

        Sprite? sprite = null;
        try
        {
            var path = Path.Combine(IconsDir, name);
            if (File.Exists(path))
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(File.ReadAllBytes(path)))     // dimensiona ao PNG
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100f);
                else UnityEngine.Object.Destroy(tex);
            }
            else Plugin.Log?.LogWarning($"[CustomClasses] ícone não encontrado: {path}");
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] falha ao carregar ícone '{name}': {ex.Message}"); }

        Cache[name] = sprite;   // cacheia inclusive null (não retentar)
        return sprite;
    }

    public static void Dispose()
    {
        foreach (var s in Cache.Values)
            if (s != null) { if (s.texture != null) UnityEngine.Object.Destroy(s.texture); UnityEngine.Object.Destroy(s); }
        Cache.Clear();
    }
}
```

### ClassIdentityView.cs (client — factory do selo, consumido por 012/013)

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>Item 011: monta/atualiza o selo "ícone + nome" idempotente. Dados já resolvidos (sem fetch).</summary>
internal static class ClassIdentityView
{
    public static GameObject BuildOrRefresh(Transform parent, string goName, string className,
        string? iconFile, Color color, TMP_FontAsset font, float iconSize = 48f, float fontSize = 28f)
    {
        var existing = parent.Find(goName);
        var go = existing != null ? existing.gameObject : CreateContainer(parent, goName);

        var img = go.transform.Find("Icon").GetComponent<Image>();
        var sprite = ClassIconCache.Get(iconFile);
        img.gameObject.SetActive(sprite != null);
        if (sprite != null) { img.sprite = sprite; img.preserveAspect = true; }

        var tmp = go.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = className;
        tmp.color = color;
        tmp.fontSize = fontSize;
        tmp.enableWordWrapping = false;
        return go;
    }

    private static GameObject CreateContainer(Transform parent, string goName)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);
        var hl = go.GetComponent<HorizontalLayoutGroup>(); hl.spacing = 8f; hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;
        go.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(go.transform, false);
        var le = icon.AddComponent<LayoutElement>(); le.preferredWidth = 48f; le.preferredHeight = 48f;
        icon.GetComponent<Image>().raycastTarget = false;

        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(go.transform, false);
        label.GetComponent<TextMeshProUGUI>().raycastTarget = false;
        return go;
    }
}
```

> Cor: o consumidor (012) faz `ColorUtility.TryParseHtmlString(SkillMultipliers.NameColor, out var c)` com default se falhar. A fonte TMP é passada pelo consumidor (do label vizinho com MO; `TMP_Settings.defaultFontAsset` sem MO).

### compile-mod.sh (ramo client, após install_own_dlls)

```bash
# Copiar assets de ícones do client (item 011), se existirem
if [[ -d "$(dirname "$CSPROJ")/icons" ]]; then
  mkdir -p "$CLIENT_DEST/icons"
  cp -f "$(dirname "$CSPROJ")"/icons/*.png "$CLIENT_DEST/icons/" 2>/dev/null || true
  echo "  ✓ icons/ → $CLIENT_DEST/icons"
fi
```

## 6. Fluxo de dados

```
config/classes/*.jsonc (iconFile, nameColor)
  → CustomClassesMod.RegisterClass → ClassVisualRegistry.Set(edition, icon, color)   (sempre)
  → rota /customclasses/skill-multipliers → { className, iconFile, nameColor, multipliers }  (se Contains(edition))
[client] SkillMultipliers.EnsureLoaded() → ClassName/IconFile/NameColor + Factors
  → (012/013) ClassIdentityView.BuildOrRefresh(... ClassIconCache.Get(IconFile) ...)
[build] compile-mod (client) → modded/Client/icons/*.png → BepInEx/plugins/CustomClasses/icons/
```

## 7. Riscos e dependências

- **Router (compat 010):** mudar o critério de `className` (de "tem multiplicador" para "é classe do mod") **não** quebra o 010 — quem tem multiplicador continua recebendo tudo; quem não tem passa a receber identidade. O client do 010 ignora campos extras.
- **Path do plugin:** `Assembly.Location` é populado para plugins carregados de disco (BepInEx) — ok. Sanitização `Path.GetFileName` evita path traversal no `iconFile`.
- **Leak de VRAM:** `ClassIconCache.Dispose()` no `OnDestroy` do Plugin destrói texturas/sprites. Cache guarda `null` para não retentar PNG ausente.
- **`ClassIdentityView` sem consumidor neste item:** é base; exercitada de fato no 012. Critério do 011 = compila + cache resolve um PNG existente.
- **Assets no client:** PNGs em `modded/Client/icons/` (simétrico ao `config/` do server). O `.gitignore` versiona PNGs do mod (não casam com os padrões de binário ignorados — confirmar; se preciso, exceção).
- **Placeholders:** gerar via PowerShell/System.Drawing no `/code-mod` (passo manual documentado).

## 8. Checklist de implementação

- [ ] `ClassDefinition` + `iconFile`/`nameColor`.
- [ ] `ClassVisualRegistry` (Singleton) + `CustomClassesMod` registra toda classe.
- [ ] `SkillMultipliersResponse` + campos; `SkillMultipliersRouter` devolve identidade quando `Contains(edition)`.
- [ ] `SkillMultipliers` client expõe `IconFile`/`NameColor` (+ `Reset`).
- [ ] `ClassIconCache` (PNG→Sprite, sanitiza, cacheia, `Dispose`).
- [ ] `ClassIdentityView` (selo idempotente).
- [ ] `Plugin.OnDestroy` → `ClassIconCache.Dispose()`.
- [ ] `compile-mod.sh` copia `modded/Client/icons` ao client.
- [ ] Placeholders PNG em `modded/Client/icons/` (genérico + por classe) + apontar nos `.jsonc`.
- [ ] `_docs/exampleClass.jsonc` documenta `iconFile`/`nameColor`.
- [ ] `/compile-mod` 0 warn/err; PNGs no plugin; rota devolve identidade (testar classe sem multiplicador).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Spec técnica criada via `/create-technical-spec` |
