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
        /// config-server → biblioteca de REFERÊNCIA (mirror-only). Espelha a pasta do server na pasta
        /// "config-server/" do cliente: baixa a última versão sempre que faltar ou o hash divergir
        /// (restaura — a pasta é pristina, o usuário não edita, só consulta e copia manualmente pra "config/").
        /// NUNCA semeia/escreve em "config/". NUNCA deleta extras (fica fora de MirrorPrefixes, pulado no ScanExtras).
        /// Substitui o antigo SeedAndMirror (que também semeava em config/) — o papel de distribuir defaults
        /// passou pro canal "config" (preserve-divergent). Reusa SyncActionKind.Download (via AddDownload).
        /// </summary>
        MirrorReference = 5,

        /// <summary>
        /// config-force → config FORÇADO. Cada arquivo do SERVER em "&lt;name&gt;-force/&lt;rel&gt;" sobrescreve o
        /// "&lt;name&gt;/&lt;rel&gt;" do USUÁRIO SEMPRE que o conteúdo divergir (ou faltar) — **ignora customização**.
        /// É o canal deliberado de "essa config vai pra todo mundo" (ex.: corrigir um valor que quebra o coop),
        /// em contraste com o 'config' (preserve-divergent, respeita quem customizou) e o 'config-server'
        /// (seed, só se faltar). Não deleta extras; a pasta config-force NUNCA é materializada no cliente.
        /// </summary>
        ForceToConfig = 6,

        /// <summary>
        /// Item 030: config-optional → config quando o item está LIGADO. Vence config-force e config
        /// (precedência performance &gt; force &gt; config). Híbrido — NÃO é clone do ForceToConfig, que
        /// ignora baseline: no momento em que o player alterna o item (ação explícita) aplica/remove mesmo
        /// divergente, preservando o anterior na quarentena; nos syncs de rotina respeita a customização
        /// via baseline (preserve-divergent). O SyncEngine grava baseline no <see cref="SyncActionKind.OptionalConfigCopy"/>
        /// — sem isso o híbrido não convergiria. A pasta-espelho config-optional-ref (D-18) é MirrorReference.
        /// </summary>
        OptionalConfigToConfig = 7,
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
                case "mirror-reference":
                    rule = SyncFolderRule.MirrorReference;
                    return true;
                case "force-to-config":
                    rule = SyncFolderRule.ForceToConfig;
                    return true;
                case "optional-config-to-config":
                    rule = SyncFolderRule.OptionalConfigToConfig;
                    return true;
                default:
                    rule = SyncFolderRule.Default;
                    return false;
            }
        }
    }
}
