using System.Collections.Generic;
using SPT.Launcher.Sync;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 031 (achado F): fonte ÚNICA das mensagens de sync, usada pelos dois VMs (ProfileViewModel
    /// e ModUpdateViewModel). Tudo i18n — nada de string montada em código (o <see cref="SyncResult.Summary"/>
    /// PT-hardcoded fica só para logs internos).
    /// </summary>
    public static class SyncMessages
    {
        /// <summary>Frase de progresso fiel à ação em curso (não mais tudo "Baixando"). Item 031.</summary>
        public static string ProgressText(SyncActionKind? kind, string path, int current, int total)
        {
            var L = LocalizationProvider.Instance;
            string fmt = kind switch
            {
                SyncActionKind.DeleteExtra        => L.update_deleting,                 // "Removendo: …"
                SyncActionKind.MoveToDisabled     => L.update_archiving,                // "Arquivando (saiu do servidor): …"
                SyncActionKind.MoveDirToDisabled  => L.update_archiving,
                SyncActionKind.SeedCopy           => L.update_seeding,                  // "Instalando padrão: …"
                SyncActionKind.ForceCopy          => L.update_forcing_config,           // "Aplicando config obrigatória: …"
                SyncActionKind.OptionalConfigCopy => L.update_applying_optional_config, // "Aplicando config opcional: …"
                _                                 => L.update_downloading,              // Download (e fallback)
            };
            return string.Format(fmt, path, current, total);
        }

        /// <summary>
        /// Frase final composta de segmentos traduzidos (só os &gt; 0). Sem nenhuma ação → "tudo atualizado".
        /// Item 031: substitui o <see cref="SyncResult.Summary"/> PT-hardcoded na UI.
        /// </summary>
        public static string BuildSummary(SyncResult r)
        {
            var L = LocalizationProvider.Instance;
            var segs = new List<string>();
            void Add(int n, string fmt) { if (n > 0) segs.Add(string.Format(fmt, n)); }

            Add(r.Updated, L.sync_seg_downloaded);
            Add(r.MovedToDisabled, L.sync_seg_archived);
            Add(r.Deleted, L.sync_seg_removed);
            Add(r.Seeded, L.sync_seg_seeded);
            Add(r.Forced, L.sync_seg_forced);
            Add(r.OptionalConfigApplied, L.sync_seg_optional_config);
            Add(r.ConfigsBackedUp, L.sync_seg_backed_up);
            Add(r.Preserved + r.PreservedDevMode, L.sync_seg_kept);
            Add(r.Errors, L.sync_seg_errors); // redundante nos fluxos que ramificam Errors antes (CR: PA-01-04)

            if (segs.Count == 0) return L.update_up_to_date;
            return string.Format(L.sync_completed_prefix, string.Join(" · ", segs));
        }
    }
}
