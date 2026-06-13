using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                 // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Models.Utils;       // ISptLogger
using SPTarkov.Server.Core.Services;           // DatabaseService

namespace CustomizationPersistenceFix;

/// <summary>
///     Corrige um bug do SPT 4.0.x: <c>ProfileFixerService.FixProfileBreakingInventoryItemIssues</c>
///     reseta <c>Customization.Body/Hands/Feet</c> <b>válidos</b> para o default da facção a cada
///     <c>/client/game/start</c> — a checagem está com a lógica invertida (falta o <c>!</c>; só o Head
///     está correto). Efeito: QUALQUER skin/roupa volta ao default ao recarregar o jogo. O método só é
///     chamado quando <c>core.json → fixes.fixProfileBreakingInventoryItemIssues == true</c> (default
///     do SPT é <c>false</c>, mas instalações que o ligam disparam o bug).
///     Este mod aplica um patch Harmony que preserva os valores válidos no load.
///     ref: spt-source <c>Services/ProfileFixerService.cs</c> — Body/Hands/Feet usam
///          <c>if (customizationDb.ContainsKey(...))</c> em vez de <c>if (!customizationDb.ContainsKey(...))</c>.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader)]
public class CustomizationPersistenceFixMod(
    DatabaseService databaseService,
    ISptLogger<CustomizationPersistenceFixMod> logger
) : IOnLoad
{
    /// <summary>Referência ao DatabaseService p/ o patch estático validar as peças contra a DB de customização.</summary>
    internal static DatabaseService? Db;

    /// <summary>Logger exposto ao patch estático p/ confirmar que ele dispara (a 1ª versão era um no-op silencioso).</summary>
    internal static ISptLogger<CustomizationPersistenceFixMod>? Log;

    private static bool _patched;

    public Task OnLoad()
    {
        Db = databaseService;
        Log = logger;

        if (!_patched)
        {
            try
            {
                new Harmony("customizationpersistencefix.mdj").PatchAll(typeof(CustomizationPersistenceFixMod).Assembly);
                _patched = true;
                logger.Info("[CustomizationPersistenceFix] Patch aplicado — Body/Hands/Feet preservados no load (corrige reset de skin do SPT).");
            }
            catch (Exception ex)
            {
                logger.Error($"[CustomizationPersistenceFix] Falha ao aplicar o patch Harmony: {ex.Message}");
            }
        }

        return Task.CompletedTask;
    }
}
