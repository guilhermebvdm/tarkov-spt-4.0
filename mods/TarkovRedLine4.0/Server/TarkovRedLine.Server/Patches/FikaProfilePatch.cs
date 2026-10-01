namespace TarkovRedLine.Server.Patches;

/// <summary>
/// Módulo de compatibilidade e interceptação para interoperabilidade com o FIKA Coop.
/// No SPT 4.0+, a sanitização de perfis e sincronização de diálogos é manipulada nativamente pelo Fika.Core.
/// </summary>
public static class FikaProfilePatch
{
    public static void Enable()
    {
        // No-op: Integridade garantida nativamente pelo Fika.Core no SPT 4.0
    }
}
