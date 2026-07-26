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

        /// <summary>Item 030: id do MOD opcional dono deste arquivo.</summary>
        public string optionalId { get; set; }

        /// <summary>Item 030: id do ITEM de config de performance dono deste arquivo (quando sob config-optional/).</summary>
        public string optionalConfigId { get; set; }
    }
}
