using System;
using System.Collections.Generic;

namespace SPT.Launcher.Sync
{
    /// <summary>Inputs for <see cref="SyncPlanner"/> beyond the manifest itself.</summary>
    public sealed class SyncPlannerOptions
    {
        /// <summary>Absolute game root (files in the manifest are relative to it).</summary>
        public string GameRoot { get; set; }

        /// <summary>
        /// Dev Mode: files diverging from BOTH manifest and baseline are preserved with a warning
        /// (never revert local dev builds — repo memory lesson). Extras in mirrored folders are
        /// preserved too. Off → folder rules apply fully.
        /// </summary>
        public bool DevMode { get; set; }

        /// <summary>Manifest "ignoredFiles" — substring match on the normalized path (legacy semantics).</summary>
        public IReadOnlyList<string> IgnoredFiles { get; set; } = Array.Empty<string>();

        /// <summary>Launcher settings ExcludeFromCleanup — root file/folder names never deleted or moved.</summary>
        public IReadOnlyList<string> ExcludeFromCleanup { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Extra protected relative paths (any normalization) — e.g.
        /// OptionalModsHelper.GetAllKnownOptionalPaths(). Never deleted or moved.
        /// </summary>
        public IReadOnlyCollection<string> ProtectedPaths { get; set; } = Array.Empty<string>();

        /// <summary>Manifest "managedPaths" — Default-rule folders whose extras are deleted (legacy behavior).</summary>
        public IReadOnlyList<string> ManagedPaths { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Item 030: um MOD opcional (id) está ligado? Ligado → seus arquivos são baixados/mantidos;
        /// desligado → seus arquivos são movidos para a quarentena. Rename de IsOptionalGroupEnabled.
        /// PERMANECE (não é removido): sem este filtro os arquivos de mod desligado voltariam a Download
        /// e criariam loop download↔quarentena a cada sync.
        /// </summary>
        public Func<string, bool> IsOptionalModEnabled { get; set; } = _ => false;

        /// <summary>
        /// Item 030: um ITEM de config de performance (id) está ligado? Não existia no motor antigo —
        /// sem isto o planner não distingue ligar de desligar no canal PerformanceToConfig.
        /// </summary>
        public Func<string, bool> IsPerformanceItemEnabled { get; set; } = _ => false;

        /// <summary>
        /// Item 030: ids que o player ACABOU de alternar (vem de PendingApply, persistido). Para eles a
        /// aplicação/remoção é EXPLÍCITA: ignora divergência e aplica/remove sempre, com quarentena do
        /// que havia. Fora deste conjunto o canal de performance respeita a customização (baseline).
        /// Cobre os dois eixos (mods e performance); vazio = sync de rotina.
        /// </summary>
        public IReadOnlyCollection<string> JustToggledIds { get; set; } = Array.Empty<string>();
    }
}
