/* ClassInfo.cs
 * TRL — DTOs for GET /customclasses/classes (CustomClasses item 058, SP0 contract).
 * Absent fields are omitted by the server (WhenWritingNull) => null here.
 * editionKey is unique in the array (server-side dedupe), but consumers still
 * key defensively (P-058.4).
 */

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPT.Launcher.Models.TRL
{
    /// <summary>Localized {en, pt} pair as served by the CustomClasses class list route.</summary>
    public class LocalizedPair
    {
        [JsonProperty("en")]
        public string En { get; set; }

        [JsonProperty("pt")]
        public string Pt { get; set; }
    }

    /// <summary>One selectable class (profile edition) served by GET /customclasses/classes.</summary>
    public class ClassInfo
    {
        /// <summary>EXACT edition key registered in ProfileTemplates — what /launcher/profile/register expects.</summary>
        [JsonProperty("editionKey")]
        public string EditionKey { get; set; }

        [JsonProperty("displayName")]
        public LocalizedPair DisplayName { get; set; }

        [JsonProperty("description")]
        public LocalizedPair Description { get; set; }

        /// <summary>Backend-relative icon route (e.g. /CustomClasses-Server/icons/cacador.png); null when the class has no icon.</summary>
        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        /// <summary>Hex color for the class name (e.g. #c2973f); null when unset.</summary>
        [JsonProperty("nameColor")]
        public string NameColor { get; set; }

        /// <summary>Starting skill levels (normalized server-side). Not rendered yet — kept for future use (kickoff 004).</summary>
        [JsonProperty("skills")]
        public Dictionary<string, int> Skills { get; set; }

        /// <summary>Skill XP multipliers in effect. Not rendered yet — kept for future use (kickoff 004).</summary>
        [JsonProperty("skillMultipliers")]
        public Dictionary<string, double> SkillMultipliers { get; set; }
    }
}
