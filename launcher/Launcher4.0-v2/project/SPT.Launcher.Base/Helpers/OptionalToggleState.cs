using System.Collections.Generic;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 033 (Mecanismo 3): lógica PURA do guard de alterações não salvas da tela "Mods e Configs".
    /// Extraída de <see cref="Settings"/> para ser testável sem IO (o construtor de Settings toca disco).
    /// A semântica de "salvo" replica <c>IsOptionalEnabled</c>/<c>IsOptionalConfigEnabled</c>: id ausente
    /// do dicionário conta como desligado (default off).
    /// </summary>
    public static class OptionalToggleState
    {
        /// <summary>
        /// True se algum toggle atual (id, é-config, ligado?) difere do estado salvo — i.e. há alteração
        /// pendente de gravação. Dicionários nulos são tratados como vazios (tudo desligado).
        /// </summary>
        public static bool HasUnsavedChanges(
            IEnumerable<(string Id, bool IsConfig, bool Enabled)> current,
            IReadOnlyDictionary<string, bool> savedMods,
            IReadOnlyDictionary<string, bool> savedConfigs)
        {
            if (current == null) return false;

            foreach (var (id, isConfig, enabled) in current)
            {
                bool persisted = isConfig
                    ? savedConfigs != null && savedConfigs.TryGetValue(id, out bool c) && c
                    : savedMods != null && savedMods.TryGetValue(id, out bool m) && m;

                if (enabled != persisted) return true;
            }

            return false;
        }
    }
}
