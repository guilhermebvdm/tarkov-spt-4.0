# 008 — i18n (multilíngue pt-BR/en) · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [008-i18n-01-spec.md](008-i18n-01-spec.md)
**Criado:** 2026-06-07

> Item híbrido em 2 fatias independentes: **A (server)** descrição de edition por idioma; **B (client)** seletor de língua + textos da UI. Refs de servidor em `references/spt-source/`; o client não toca o EFT (só BepInEx config + MultiplierFormat).

## 1. Estratégia

**Fatia A — server/launcher.** `ClassDefinition.Description` passa de `string?` para um tipo `LocalizedText` decorado com um `JsonConverter` que aceita **string** (legado → `en`) **ou objeto** `{ "en": …, "pt": … }`. No `RegisterClass`, resolve a descrição pela língua do servidor (`LocaleService.GetDesiredServerLocale()`): locale `pt*` → `pt` (vazio → `en`); outro → `en`; tudo vazio → nome da classe. Mantém `DescriptionLocaleKey = <texto resolvido>` (GetText devolve verbatim — não há `AddText` no `ServerLocalisationService`).

**Fatia B — client/F12.** Um `ConfigEntry<Language>` (`English`/`Portugues`, **default English**) define a língua dos textos renderizados pelo mod. `MultiplierFormat` passa a ter as frases em en+pt e escolhe pela config. O marcador (`▲ +X%`) é language-neutral; só o **tooltip** muda de idioma. Os `ConfigDescription` do F12 ficam **bilíngues estáticos** (en + pt no mesmo tooltip), pois o BepInEx lê o texto do bind uma vez no `Awake` (não dá pra re-localizar dinamicamente sem re-bind, que é breaking).

**Alternativa descartada:** registrar as descrições nos arquivos de locale do server (`./SPT_Data/database/locales/server/*.json`) — exigiria escrever fora do mod folder e não há API pública de `AddText`. Resolver a string no load é mais simples e autocontido.

## 2. Pontos de referência (server SPT)

| Símbolo | Arquivo | Uso |
|---|---|---|
| `LocaleService.GetDesiredServerLocale()` → string (lower, ex. `en`, `pt-br`) | [LocaleService.cs:74](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/LocaleService.cs#L74) | língua do servidor p/ resolver a descrição |
| `ServerLocalisationService.GetText` devolve a key verbatim se não registrada | [ServerLocalisationService.cs:92](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/ServerLocalisationService.cs#L92) | por isso `DescriptionLocaleKey = texto literal` |
| `LauncherController.GetProfileDescriptions` → `GetText(DescriptionLocaleKey)` | [LauncherController.cs:63](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L63) | onde a descrição é lida pelo launcher |
| `ClassDefinition.Description` (`string?` hoje) | [ClassDefinition.cs:26](../../modded/Server/ClassDefinition.cs#L26) | vira `LocalizedText?` |
| `RegisterClass` seta `DescriptionLocaleKey` | [CustomClassesMod.cs:132](../../modded/Server/CustomClassesMod.cs#L132) | resolve por locale |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `General` | `Language` | enum `Language` | `English` | English / Portugues | — | Idioma dos textos do mod na tela (tooltip dos multiplicadores). / Language of the mod's in-game texts. |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/LocalizedText.cs` | CRIAR | Tipo `{ En, Pt }` + `Resolve(locale)` + `JsonConverter` (string OU objeto). |
| `modded/Server/ClassDefinition.cs` | MODIFICAR | `Description` → `LocalizedText?`. |
| `modded/Server/CustomClassesMod.cs` | MODIFICAR | Injeta `LocaleService`; resolve a descrição por locale no `RegisterClass`. |
| `modded/Client/Plugin.cs` | MODIFICAR | + `ConfigEntry<Language> Lang` (default English); F12 tooltips bilíngues. |
| `modded/Client/MultiplierFormat.cs` | MODIFICAR | `TooltipText` en+pt escolhido por `Plugin.Lang`. |
| `scripts/class-recipes.js` | MODIFICAR | `description` de cada classe vira `{ en, pt }` (10 traduções). |
| `scripts/build-class-jsons.js` | MODIFICAR | emite `description` como objeto (passthrough). |
| `modded/Server/config/classes/*.jsonc` | REGENERAR | descrições bilíngues. |
| `modded/Server/config/classes/_docs/exampleClass.jsonc` | DOC | documenta `description` string|objeto. |

## 5. Stubs de código

### LocalizedText.cs (server)

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>Item 008: texto por idioma. Aceita string (legado = en) ou objeto { en, pt } no JSON.</summary>
[JsonConverter(typeof(LocalizedTextConverter))]
public sealed class LocalizedText
{
    public string? En { get; init; }
    public string? Pt { get; init; }

    /// <summary>Resolve pela locale do server (ex. "pt-br","en"); pt* → Pt (vazio→En); outro → En; nada → null.</summary>
    public string? Resolve(string? locale)
    {
        var wantPt = !string.IsNullOrEmpty(locale) && locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase);
        var primary = wantPt ? Pt : En;
        if (!string.IsNullOrWhiteSpace(primary)) return primary;
        return !string.IsNullOrWhiteSpace(En) ? En : Pt;   // fallback en, depois qualquer um
    }
}

internal sealed class LocalizedTextConverter : JsonConverter<LocalizedText>
{
    public override LocalizedText? Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return new LocalizedText { En = reader.GetString() };   // legado: string = en/fallback
            case JsonTokenType.StartObject:
                string? en = null, pt = null;
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName) continue;
                    var prop = reader.GetString();
                    reader.Read();
                    var val = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    if (string.Equals(prop, "en", StringComparison.OrdinalIgnoreCase)) en = val;
                    else if (string.Equals(prop, "pt", StringComparison.OrdinalIgnoreCase)) pt = val;
                }
                return new LocalizedText { En = en, Pt = pt };
            default:
                throw new JsonException($"LocalizedText: token inesperado {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions o)
        => throw new NotSupportedException();   // só desserializa
}
```

### CustomClassesMod.cs (trecho)

```csharp
// ctor: + LocaleService localeService   (using SPTarkov.Server.Core.Services;)

// no RegisterClass, no lugar da linha 132:
var serverLocale = localeService.GetDesiredServerLocale();              // ref: LocaleService.cs:74
var desc = def.Description?.Resolve(serverLocale);
sides.DescriptionLocaleKey = string.IsNullOrWhiteSpace(desc) ? name : desc;
```

### Plugin.cs + MultiplierFormat.cs (client)

```csharp
// Plugin.cs
internal enum Language { English, Portugues }
internal static Language Lang = Language.English;
// no Awake:
Lang = Config.Bind("General", "Language", Language.English,
    "Idioma dos textos do mod na tela / Language of the mod's in-game texts.").Value;

// MultiplierFormat.cs — TooltipText escolhe por Plugin.Lang:
public static string TooltipText(float factor, string? className)
{
    var pct = Percent(factor); var up = factor > 1f;
    var hex = up ? GreenHex : RedHex;
    var sign = pct >= 0 ? "+" : string.Empty;
    if (Plugin.Lang == Plugin.Language.Portugues)
    {
        var word = up ? "buff" : "debuff";
        var amount = $"<color={hex}>{sign}{pct}% de {word}</color>";
        var cls = string.IsNullOrWhiteSpace(className) ? "sua Classe" : $"Classe <b>{className}</b>";
        return $"Você possui {amount} nessa skill devido à {cls}";
    }
    else
    {
        var word = up ? "buff" : "debuff";
        var amount = $"<color={hex}>{sign}{pct}% {word}</color>";
        var cls = string.IsNullOrWhiteSpace(className) ? "your Class" : $"Class <b>{className}</b>";
        return $"You have a {amount} on this skill from {cls}";
    }
}
```

> `Language` fica em `Plugin` (ou arquivo próprio) — referenciado por `MultiplierFormat`. Ajustar o namespace/acesso (`Plugin.Language`/`Plugin.Lang`) na implementação.

## 6. Fluxo de dados

```
[A] config/classes/*.jsonc  description: "x" | {en,pt}
      → JsonUtil.Deserialize<ClassDefinition> (LocalizedTextConverter)
      → RegisterClass: desc = Description.Resolve(GetDesiredServerLocale())   (LocaleService.cs:74)
      → DescriptionLocaleKey = desc → launcher GetText (LauncherController.cs:63) → mostra na língua do server
[B] F12 General/Language (default English)
      → Plugin.Lang → MultiplierFormat.TooltipText escolhe en/pt
      → SkillPanelPatch tooltip do marcador na língua escolhida
```

## 7. Riscos e dependências

- **STJ honra `[JsonConverter]` no tipo:** o `JsonUtil` do SPT usa System.Text.Json; o atributo no `LocalizedText` é respeitado. Confirmar no build (compila + carrega as classes sem erro).
- **F12 não re-localiza dinamicamente:** os `ConfigDescription` são lidos no `Awake`; trocar o seletor muda só os textos **renderizados pelo mod** (tooltip dos multiplicadores) na próxima montagem da tela, **não** os labels do F12. Labels do F12 ficam bilíngues estáticos. Documentado.
- **Enum `ConfigEntry`:** BepInEx renderiza enum como dropdown. `Portugues` sem acento no identificador (label exibida pode ser ajustada via `ConfigDescription`/atributo, mas manter simples).
- **Tradução das 10 descrições:** conteúdo novo (en+pt) no `class-recipes.js`. Sem o en, a classe cai no pt como fallback? Não — `Resolve` faz en-first; manter ambos preenchidos.
- **Compat 010:** `MultiplierFormat.Marker()` inalterado (language-neutral). Só `TooltipText` ganha idioma.
- **Restart:** server (descrição) + jogo (DLL client) para validar.

## 8. Checklist de implementação

- [ ] `LocalizedText.cs` (tipo + converter string|objeto).
- [ ] `ClassDefinition.Description` → `LocalizedText?`.
- [ ] `CustomClassesMod`: injeta `LocaleService`; resolve descrição por locale.
- [ ] `Plugin.cs`: `enum Language` + `ConfigEntry<Language>` (default English) + tooltips F12 bilíngues.
- [ ] `MultiplierFormat.TooltipText` en+pt por `Plugin.Lang`.
- [ ] `class-recipes.js`: `description` → `{en, pt}` nas 10 classes (traduções).
- [ ] `build-class-jsons.js`: emite `description` objeto; regenerar `.jsonc`.
- [ ] `_docs/exampleClass.jsonc`: documenta `description` string|objeto.
- [ ] `PROPRIEDADES.md` (se existir) / criar: documenta `Language`.
- [ ] `/compile-mod` 0 warn/err.
- [ ] Playtest: server en → descrição inglês; server pt-br → português; F12 Language alterna o tooltip in-game.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` (LocalizedText+converter; LocaleService.GetDesiredServerLocale; F12 enum; refs verificadas) |
