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

        /// <summary>Skill XP multipliers in effect. Rendered as the "MULTIPLICADORES DE XP" section.</summary>
        [JsonProperty("skillMultipliers")]
        public Dictionary<string, double> SkillMultipliers { get; set; }

        /// <summary>
        /// Perks/drawbacks da classe (contrato estendido, item 029). Array OPCIONAL — ausente/omitido =
        /// classe sem perks. Cada efeito já vem com o token de valor PRÉ-FORMATADO pelo server (o launcher
        /// não reimplementa a derivação). Servido com os valores NOMINAIS do catálogo (não o F12 do cliente).
        /// </summary>
        [JsonProperty("effects")]
        public List<ClassEffect> Effects { get; set; }
    }

    /// <summary>Um perk (isPerk=true) ou drawback (isPerk=false) da classe, no contrato do item 029.</summary>
    public class ClassEffect
    {
        /// <summary>true = perk (verde, coluna esquerda) · false = drawback (vermelho, direita).</summary>
        [JsonProperty("isPerk")]
        public bool IsPerk { get; set; }

        /// <summary>true = efeito definido mas ainda não ativo in-game ("em breve", âmbar).</summary>
        [JsonProperty("pending")]
        public bool Pending { get; set; }

        /// <summary>Título único do efeito (ex.: Sharpshooter / Atirador).</summary>
        [JsonProperty("title")]
        public LocalizedPair Title { get; set; }

        /// <summary>Descrição curta do efeito (ex.: "aim (ADS) time" / "mira (ADS)").</summary>
        [JsonProperty("label")]
        public LocalizedPair Label { get; set; }

        /// <summary>Token pré-formatado: "+30%" / "−15%" / "×0.85" / "✓" / "✗" / "" (vazio = sem número).</summary>
        [JsonProperty("valueToken")]
        public string ValueToken { get; set; }
    }
}
