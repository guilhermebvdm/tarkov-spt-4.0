# 004 — Outfits por classe · Spec Técnica

**Mod:** CustomClasses
**Slug:** 004-outfits
**Criado:** 2026-06-07

> Mod **server-side** (SPT 4.0). Fonte: [references/spt-source/](../../../../references/spt-source/). Estende o loader (002/003) com um `OutfitBuilder` que, por lado (USEC/BEAR), seta a **aparência** do personagem (`Customization.Body/Feet/Hands`) e marca as roupas como **possuídas** (`TemplateSide.Suits` → `AddSuitsToProfile` no criar perfil = estado *OBTAINED*). Catálogo de peças: [scripts/suits-catalog.json](../../scripts/suits-catalog.json) (gerado do DB de customization).

## 1. Estratégia

Não há patch de cliente nem hook de raid — é injeção em **template de perfil** no `PostDBModLoader`, igual a skills/itens/hideout. No `RegisterClass`, depois do hideout, chamar `OutfitBuilder.Apply` para `sides.Usec` e `sides.Bear`.

Descoberta-chave (define o que dá pra controlar):
- No `CreateProfileService`, **Head e Voice vêm da escolha do jogador** na criação (`pmcData.Customization.Head = request.HeadId` [CreateProfileService.cs:61], `Voice` [:58]) — **sobrescrevem o template**, então **não** são controláveis por classe.
- **Body, Feet, Hands** vêm do template (`pmcData = profileTemplateClone.Character` [:46], não sobrescritos) → **controláveis**.
- As roupas do template (`TemplateSide.Suits`) são adicionadas ao perfil como **possuídas/desbloqueadas** via `profileDetails.AddSuitsToProfile(profileTemplateClone.Suits)` [CreateProfileService.cs:134] → `CustomisationUnlocks` (`Source = UNLOCKED_IN_GAME, Type = SUITE`). É isso que tira a skin do estado **UNAVAILABLE**.

Modelo de dados confirmado no DB: uma **peça de roupa** (entrada `_type: "Item"` no customization DB) tem `_props.Side` (facção) e referencia a aparência — **upper** traz `Body`+`Hands`, **lower** traz `Feet`. Ex. validado: peça "USEC Standard" upper (`5cde9ec1…`) → `Body=5cde95d9…`, `Hands=5cde95fa…`, que são exatamente a `Customization` do perfil base.

**Conclusão:** para vestir uma classe, por lado: pegar a peça **upper** e a peça **lower** escolhidas → setar `Customization.Body/Hands` (da upper) e `Customization.Feet` (da lower) → adicionar os dois IDs em `TemplateSide.Suits`. Validar `_props.Side` (skip-com-aviso se a peça não vale pro lado). Head fica como o jogador escolher.

## 2. Pontos de referência (SPT server source)

| Símbolo | Arquivo | Uso |
|---|---|---|
| `TemplateSide.Suits` (`List<MongoId>?`) | [ProfileTemplate.cs:31](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/ProfileTemplate.cs#L31) | roupas possuídas do template |
| `ProfileSides.Usec/Bear` (`TemplateSide?`) | [ProfileTemplate.cs:12/16](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/ProfileTemplate.cs#L12) | os dois lados |
| `PmcData.Customization` | [BotBase.cs:36](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L36) | aparência do personagem |
| `Customization.Body/Feet/Hands` (`MongoId?`) | [BotBase.cs:307/309/311](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L307) | campos setáveis |
| `DatabaseService.GetCustomization()` → `Dictionary<MongoId, CustomizationItem>` | [DatabaseService.cs:117](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/DatabaseService.cs#L117) | resolver/validar peça |
| `CustomizationItem.Properties` (`_props`) | [CustomizationItem.cs:21](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/CustomizationItem.cs#L21) | props da peça |
| `CustomizationProperties.Side/Body/Feet/Hands` | [CustomizationItem.cs:66/60/57/54](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/CustomizationItem.cs#L66) | facção + refs de aparência |
| `CreateProfileService` (Head/Voice do request; `AddSuitsToProfile`) | [CreateProfileService.cs:58/61/134](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CreateProfileService.cs#L58) | porque Head não é controlável + como Suits viram OBTAINED |

## 3. Novas propriedades F12

Nenhuma (mod server-side).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/ClassDefinition.cs` | MODIFICAR | + `Outfit` (record com `Usec`/`Bear` = `OutfitSide`); `OutfitSide` (`upper`, `lower`). |
| `modded/Server/OutfitBuilder.cs` | CRIAR | Resolve as peças upper/lower do DB, valida facção, seta `Customization.Body/Feet/Hands` e adiciona aos `Suits`. |
| `modded/Server/CustomClassesMod.cs` | MODIFICAR | Injeta `OutfitBuilder` + chama no `RegisterClass` (Usec/Bear). |
| `modded/Server/config/classes/*.jsonc` | (depois) | Popular `outfit` das 10 classes — **depende das escolhas do amigo** (D1). |

## 5. Stubs de código

### ClassDefinition.cs (acréscimo)

```csharp
/// <summary>Optional. Per-side outfit (USEC/BEAR). (Item 004)</summary>
[JsonPropertyName("outfit")]
public Outfit? Outfit { get; init; }

// ... (no fim do arquivo, junto aos outros records)

/// <summary>Outfit por lado — roupas são específicas de facção.</summary>
public sealed record Outfit
{
    [JsonPropertyName("usec")] public OutfitSide? Usec { get; init; }
    [JsonPropertyName("bear")] public OutfitSide? Bear { get; init; }
}

/// <summary>IDs das peças de roupa (customization "Item"): upper (camisa/jaqueta) e lower (calça).</summary>
public sealed record OutfitSide
{
    [JsonPropertyName("upper")] public string? Upper { get; init; }
    [JsonPropertyName("lower")] public string? Lower { get; init; }
}
```

### OutfitBuilder.cs (novo)

```csharp
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // TemplateSide

namespace CustomClasses;

/// <summary>
///     Veste uma classe por lado (USEC/BEAR): seta a aparência (Customization.Body/Feet/Hands) a partir
///     das peças escolhidas e marca-as como possuídas (Suits → AddSuitsToProfile = OBTAINED no criar perfil).
///     Head/Voice NÃO são controláveis (vêm da escolha do jogador — CreateProfileService.cs:58/61).
///     Estrutura: ver docs/technical/inventario-itens-spt4.md (parte de customization) e suits-catalog.json.
/// </summary>
[Injectable]
public class OutfitBuilder(DatabaseService databaseService, ISptLogger<OutfitBuilder> logger)
{
    /// <summary>sideName = "Usec" | "Bear". Retorna o nº de peças aplicadas.</summary>
    public int Apply(TemplateSide? side, string sideName, OutfitSide? outfit, string className)
    {
        if (outfit is null)
        {
            return 0;
        }

        var customization = side?.Character?.Customization;
        if (customization is null)
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): base sem Customization — outfit pulado.");
            return 0;
        }

        var db = databaseService.GetCustomization();   // ref: DatabaseService.cs:117
        side!.Suits ??= [];
        var applied = 0;

        // upper → Body + Hands; lower → Feet. (ref: CustomizationItem.cs:54/57/60)
        applied += ApplyPiece(db, customization, side, sideName, outfit.Upper, isUpper: true, className);
        applied += ApplyPiece(db, customization, side, sideName, outfit.Lower, isUpper: false, className);
        return applied;
    }

    private int ApplyPiece(
        IReadOnlyDictionary<MongoId, CustomizationItem> db,
        Customization customization,
        TemplateSide side,
        string sideName,
        string? pieceId,
        bool isUpper,
        string className)
    {
        if (string.IsNullOrWhiteSpace(pieceId))
        {
            return 0;
        }

        var key = new MongoId(pieceId);
        if (!db.TryGetValue(key, out var item) || item.Properties is null)
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): roupa '{pieceId}' não existe — pulada.");
            return 0;
        }

        // restrição de facção (ref: CustomizationItem.cs:66 — valores "Usec"/"Bear"/"Savage")
        if (item.Properties.Side is { Count: > 0 } sides && !sides.Contains(sideName))
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): roupa '{pieceId}' é de [{string.Join(",", sides)}] — pulada.");
            return 0;
        }

        if (isUpper)
        {
            if (item.Properties.Body is { } body) customization.Body = body;
            if (item.Properties.Hands is { } hands) customization.Hands = hands;
        }
        else
        {
            if (item.Properties.Feet is { } feet) customization.Feet = feet;
        }

        if (!side.Suits!.Contains(key))
        {
            side.Suits.Add(key);   // possuída/desbloqueada (ref: CreateProfileService.cs:134)
        }

        return 1;
    }
}
```

### CustomClassesMod.cs (wiring)

```csharp
// ctor: + OutfitBuilder outfitBuilder
// em RegisterClass, após o hideout e antes de templates[name] = sides:
var outfitsUsec = 0; var outfitsBear = 0;
if (def.Outfit is not null)
{
    outfitsUsec = outfitBuilder.Apply(sides.Usec, "Usec", def.Outfit.Usec, name);
    outfitsBear = outfitBuilder.Apply(sides.Bear, "Bear", def.Outfit.Bear, name);
}
// incluir outfits no logger.Info de "Registered"
```

## 6. Fluxo de dados

```
class JSON .outfit.{usec,bear}.{upper,lower}  (IDs de peça)
  → OutfitBuilder.Apply(side, sideName, outfitSide)
      → GetCustomization()[pieceId]  (DatabaseService.cs:117)
      → valida _props.Side contém sideName  (CustomizationItem.cs:66)
      → upper: Customization.Body/Hands ; lower: Customization.Feet  (BotBase.cs:307+)
      → side.Suits += pieceId  (ProfileTemplate.cs:31)
  → (no criar perfil) AddSuitsToProfile(template.Suits) → CustomisationUnlocks = OBTAINED  (CreateProfileService.cs:134)
  → personagem nasce vestido + dono das peças (não UNAVAILABLE)
```

## 7. Riscos e dependências

- **Head/Voice não controláveis** — escolha do jogador na criação sobrescreve o template (documentar; fora do escopo deste item).
- **Facção** — roupa USEC aplicada ao lado BEAR é pulada (skip-com-aviso); aplicar por lado independentemente.
- **Clobber do RZ** (conhecido do 001/007) — testar com RZ desabilitado.
- **IDs das peças** (D2) — resolvidos via [suits-catalog.json](../../scripts/suits-catalog.json) (nome↔ID↔aparência); regenerável do DB.
- **Popular as 10 classes** (D1) — depende das escolhas do amigo (skin por classe). Este item entrega a **capacidade**; popular é passo de dados depois (ajuste no `build-class-jsons.js`/`class-recipes.js`).

## 8. Checklist de implementação

- [x] Estender `ClassDefinition` (+ `Outfit`/`OutfitSide`).
- [x] Criar `OutfitBuilder` (resolve peça, **valida slot+facção** (PA-01-01), seta Customization + Suits).
- [x] Injetar + chamar no `CustomClassesMod.RegisterClass` (Usec/Bear) + log com contagem.
- [x] `/compile-mod` (0 warn/err). **Teste in-game pendente** (validação do usuário): classe com `outfit` → personagem nasce vestido e **dono** das peças (OBTAINED), USEC e BEAR; sem `outfit` → aparência padrão.
- [x] Corner cases tratados em código: peça inexistente, slot errado (upper sem Body/lower sem Feet), facção errada, só upper ou só lower, outfit vazio, aplicação independente por lado, Side nulo=lenient.
- [ ] (Depois, D1) Popular `outfit` das 10 classes com as escolhas do amigo. **(pendente — aguardando skins)**

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` (Customization Body/Feet/Hands + Suits/OBTAINED; Head/Voice não controláveis; catálogo de peças gerado) |
