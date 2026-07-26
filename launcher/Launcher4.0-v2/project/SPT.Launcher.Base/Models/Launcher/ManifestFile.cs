namespace SPT.Launcher.Models.Launcher
{
    /// <summary>
    /// A single file entry from the server mods manifest (GET /launcher/mods/manifest).
    /// Canonical definition — consolidates the duplicated classes that previously lived in
    /// ModUpdateViewModel.cs and OptionalModsHelper.cs (item 007).
    /// Lower-case property names match the server JSON contract.
    /// </summary>
    public class ManifestFile
    {
        public string path { get; set; }
        public string hash { get; set; }
        public long size { get; set; }
        public bool optional { get; set; }

        /// <summary>
        /// Legado (modelo antigo de mods opcionais, itens 009/021). Substituído por <see cref="optionalId"/>
        /// no item 030 — mantido enquanto o OptionalModsHelper/SyncManifestOverlay ainda existem (removidos
        /// na Fase 3). O motor de sync novo lê <see cref="optionalId"/>.
        /// </summary>
        public string optionalGroup { get; set; }

        /// <summary>Item 030: id do MOD opcional dono deste arquivo (rename semântico de <see cref="optionalGroup"/>).</summary>
        public string optionalId { get; set; }

        /// <summary>Item 030: id do ITEM de config de performance dono deste arquivo (quando sob config-performance/).</summary>
        public string performanceId { get; set; }
    }
}
