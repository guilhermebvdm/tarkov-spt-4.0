using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // TemplateSide, Customization, CustomizationItem

namespace CustomClasses;

/// <summary>
///     Veste uma classe por lado (USEC/BEAR): seta a aparência (Customization.Body/Feet/Hands) a partir
///     das peças escolhidas e marca-as como possuídas (Suits → AddSuitsToProfile = OBTAINED no criar perfil,
///     ref: CreateProfileService.cs:134). Head/Voice NÃO são controláveis — vêm da escolha do jogador na
///     criação (ref: CreateProfileService.cs:58/61). Catálogo de peças: scripts/suits-catalog.json.
///     Modelo de customization: ver memória reference_spt_customization_model.
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

        var customization = side?.Character?.Customization;   // ref: BotBase.cs:36
        if (customization is null)
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): base sem Customization — outfit pulado.");
            return 0;
        }

        var db = databaseService.GetCustomization();   // ref: DatabaseService.cs:117
        side!.Suits ??= [];

        // upper → Body + Hands ; lower → Feet  (ref: CustomizationItem.cs:54/57/60)
        var applied = 0;
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

        // CR-01-01: id malformado lança em `new MongoId` → pula só esta peça, não a classe inteira.
        MongoId key;
        try
        {
            key = new MongoId(pieceId);
        }
        catch (Exception ex)
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): roupa '{pieceId}' inválida ({ex.Message}) — pulada.");
            return 0;
        }

        // defensivo: `_props` pode vir ausente em dados de mod (CR-01-03).
        if (!db.TryGetValue(key, out var item) || item.Properties is null)
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): roupa '{pieceId}' não existe — pulada.");
            return 0;
        }

        var props = item.Properties;

        // Resolve a aparência da peça, cobrindo os 2 padrões (PA-01-01 endurecido):
        //  - vanilla / suite: _props.Body / _props.Feet referenciam OUTRA customization (a malha).
        //  - "aparência direta" (ex.: AllTheClothes): _props.Body/Feet nulos, mas _props.BodyPart == "Body"/"Feet"
        //    → a própria peça é a malha (via Prefab) → usa o próprio id.
        var part = props.BodyPart;   // ref: CustomizationItem.cs:48
        var bodyAppearance = isUpper
            ? props.Body ?? (string.Equals(part, "Body", StringComparison.OrdinalIgnoreCase) ? (MongoId?)key : null)
            : (MongoId?)null;
        var feetAppearance = !isUpper
            ? props.Feet ?? (string.Equals(part, "Feet", StringComparison.OrdinalIgnoreCase) ? (MongoId?)key : null)
            : (MongoId?)null;

        if (isUpper && bodyAppearance is null)
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): '{pieceId}' não é peça upper (sem Body nem BodyPart=Body) — pulada.");
            return 0;
        }
        if (!isUpper && feetAppearance is null)
        {
            logger.Warning($"[CustomClasses] '{className}' ({sideName}): '{pieceId}' não é peça lower (sem Feet nem BodyPart=Feet) — pulada.");
            return 0;
        }

        // Restrição de facção RELAXADA (decisão do usuário 2026-06-15): aplica a roupa MESMO de facção
        // diferente (Savage/AllTheClothes incluídas) — o autor da classe escolhe o outfit deliberadamente,
        // e o mod AllTheClothes torna qualquer roupa válida para PMC. Antes pulava (return 0) quando
        // props.Side não continha o lado; agora só registra e segue. Sem AllTheClothes, uma malha
        // scav-only pode renderizar estranho — responsabilidade do autor. (ref: CustomizationItem.cs:66)
        if (props.Side is { Count: > 0 } sides && !sides.Contains(sideName))
        {
            logger.Info($"[CustomClasses] '{className}' ({sideName}): roupa '{pieceId}' é de [{string.Join(",", sides)}] — aplicada mesmo assim (guard de Side relaxado).");
        }

        if (isUpper)
        {
            customization.Body = bodyAppearance;   // ref: BotBase.cs:307
            if (props.Hands is { } hands)
            {
                customization.Hands = hands;        // ref: BotBase.cs:311 (vanilla traz ref; aparência direta pode não ter)
            }
        }
        else
        {
            customization.Feet = feetAppearance;   // ref: BotBase.cs:309
        }

        if (!side.Suits!.Contains(key))
        {
            side.Suits.Add(key);   // possuída/desbloqueada (ref: ProfileTemplate.cs:31 → CreateProfileService.cs:134)
        }

        return 1;
    }
}
