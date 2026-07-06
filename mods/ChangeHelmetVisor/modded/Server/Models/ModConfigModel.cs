namespace ChangeHelmetVisorServer.Models
{
    public record ModConfig
    {
        public bool AllFaceShields { get; set; }
        public string? MaskType { get; set; }
        public Dictionary<string, string> SpecificFaceShields { get; set; } = new();
    }
}
