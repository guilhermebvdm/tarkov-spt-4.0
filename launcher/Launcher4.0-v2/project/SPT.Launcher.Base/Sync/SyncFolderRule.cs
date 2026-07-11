using System;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Per-folder synchronization strategy (item 007).
    /// </summary>
    public enum SyncFolderRule
    {
        /// <summary>Current launcher behavior: download outdated/missing; extras inside managedPaths are deleted.</summary>
        Default = 0,

        /// <summary>config: replace files equal to the last-sync baseline; preserve user-customized ones.</summary>
        PreserveDivergent = 1,

        /// <summary>config-server: full mirror; local extras are deleted.</summary>
        MirrorDelete = 2,

        /// <summary>patchers/plugins: full mirror; local extras are moved to a sibling "&lt;folder&gt;-disabled" directory.</summary>
        MirrorMoveDisabled = 3,

        /// <summary>
        /// Item 017: config-server → config seed. Each SERVER file under "&lt;name&gt;-server/&lt;rel&gt;"
        /// is copied to the USER "&lt;name&gt;/&lt;rel&gt;" ONLY when the target is absent by name.
        /// Never deletes, never overwrites, never consults hash/baseline (memory-less seed).
        /// </summary>
        SeedIfMissingByName = 4,

        /// <summary>
        /// config-server DUAL sync (server → cliente, dois destinos ao mesmo tempo):
        ///  1. SEED em "config/&lt;rel&gt;" — copia só se o arquivo NÃO existir (idêntico ao SeedIfMissingByName:
        ///     nunca sobrescreve/deleta o 'config' do usuário).
        ///  2. MIRROR em "config-server/&lt;rel&gt;" — réplica: baixa a última versão sempre que faltar ou
        ///     o hash divergir (sobrescreve edições do usuário), mas NÃO deleta extras (conservador,
        ///     ref CR-01-03; delete fica opt-in — não é MirrorPrefix, é pulado no ScanExtras).
        /// Uso: 'config' = configs vivas do usuário (seed único); 'config-server' = referência sempre atual
        /// de onde o usuário copia manualmente ao atualizarmos a config de um mod existente.
        /// </summary>
        SeedAndMirror = 5,

        /// <summary>
        /// config-force → config FORÇADO. Cada arquivo do SERVER em "&lt;name&gt;-force/&lt;rel&gt;" sobrescreve o
        /// "&lt;name&gt;/&lt;rel&gt;" do USUÁRIO SEMPRE que o conteúdo divergir (ou faltar) — **ignora customização**.
        /// É o canal deliberado de "essa config vai pra todo mundo" (ex.: corrigir um valor que quebra o coop),
        /// em contraste com o 'config' (preserve-divergent, respeita quem customizou) e o 'config-server'
        /// (seed, só se faltar). Não deleta extras; a pasta config-force NUNCA é materializada no cliente.
        /// </summary>
        ForceToConfig = 6,
    }

    public static class SyncFolderRuleParser
    {
        /// <summary>
        /// Parses the canonical rule names used by the server manifest ("default",
        /// "preserve-divergent", "mirror-delete", "mirror-move-disabled"). Case-insensitive.
        /// </summary>
        public static bool TryParse(string value, out SyncFolderRule rule)
        {
            switch (value?.Trim().ToLowerInvariant())
            {
                case "default":
                    rule = SyncFolderRule.Default;
                    return true;
                case "preserve-divergent":
                    rule = SyncFolderRule.PreserveDivergent;
                    return true;
                case "mirror-delete":
                    rule = SyncFolderRule.MirrorDelete;
                    return true;
                case "mirror-move-disabled":
                    rule = SyncFolderRule.MirrorMoveDisabled;
                    return true;
                case "seed-if-missing":
                    rule = SyncFolderRule.SeedIfMissingByName;
                    return true;
                case "seed-and-mirror":
                    rule = SyncFolderRule.SeedAndMirror;
                    return true;
                case "force-to-config":
                    rule = SyncFolderRule.ForceToConfig;
                    return true;
                default:
                    rule = SyncFolderRule.Default;
                    return false;
            }
        }
    }
}
