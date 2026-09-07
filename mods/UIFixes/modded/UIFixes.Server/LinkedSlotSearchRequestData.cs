using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Ragfair;

namespace UIFixes.Server;

public record LinkedSlotSearchRequestData : SearchRequestData
{
    private string _linkedSearchId;

    [JsonPropertyName("linkedSearchId")]
    public new string LinkedSearchId
    {
        get => _linkedSearchId;
        set
        {
            _linkedSearchId = value;
            if (!string.IsNullOrEmpty(value))
            {
                var parts = value.Split(":", 2);
                base.LinkedSearchId = parts[0];
                if (parts.Length > 1)
                {
                    SlotName = parts[1];
                }
            }
        }
    }

    public string SlotName { get; set; }
}