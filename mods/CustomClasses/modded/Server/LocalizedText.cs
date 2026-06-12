using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>
///     Item 008: texto por idioma. No JSON aceita uma <b>string</b> (legado = en/fallback) ou um
///     <b>objeto</b> { "en": "...", "pt": "..." }. Resolvido no load pela locale do servidor.
/// </summary>
[JsonConverter(typeof(LocalizedTextConverter))]
public sealed class LocalizedText
{
    public string? En { get; init; }
    public string? Pt { get; init; }

    /// <summary>locale "pt*" → Pt (vazio → En); outro → En; tudo vazio → null.</summary>
    public string? Resolve(string? locale)
    {
        var wantPt = !string.IsNullOrEmpty(locale) && locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase);
        var primary = wantPt ? Pt : En;
        if (!string.IsNullOrWhiteSpace(primary))
        {
            return primary;
        }

        return !string.IsNullOrWhiteSpace(En) ? En : Pt;   // fallback: en, depois qualquer um
    }
}

/// <summary>
///     Converte string OU objeto {en,pt} em <see cref="LocalizedText"/>. Item 021: também serializa
///     (o editor reescreve os arquivos de classe) preservando as duas formas de entrada — só En → string
///     (forma legada); com Pt → objeto { en, pt }.
/// </summary>
internal sealed class LocalizedTextConverter : JsonConverter<LocalizedText>
{
    public override LocalizedText? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.String:
                return new LocalizedText { En = reader.GetString() };   // legado: string = en/fallback

            case JsonTokenType.StartObject:
                string? en = null, pt = null;
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        continue;
                    }

                    var prop = reader.GetString();
                    reader.Read();
                    string? val = null;
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        val = reader.GetString();
                    }
                    else
                    {
                        // CR-EP-05: valor não-string (objeto/array aninhado) precisa ser consumido por
                        // inteiro, senão o while termina no EndObject ANINHADO e o reader dessincroniza.
                        reader.Skip();
                    }
                    if (string.Equals(prop, "en", StringComparison.OrdinalIgnoreCase))
                    {
                        en = val;
                    }
                    else if (string.Equals(prop, "pt", StringComparison.OrdinalIgnoreCase))
                    {
                        pt = val;
                    }
                }

                return new LocalizedText { En = en, Pt = pt };

            default:
                throw new JsonException($"LocalizedText: token inesperado '{reader.TokenType}'.");
        }
    }

    public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions options)
    {
        // Item 021 (ClassEditorService.Save): round-trip das duas formas aceitas no Read.
        if (value.Pt is null && value.En is not null)
        {
            writer.WriteStringValue(value.En);   // forma legada: string = en/fallback
            return;
        }

        writer.WriteStartObject();
        if (value.En is not null)
        {
            writer.WriteString("en", value.En);
        }
        if (value.Pt is not null)
        {
            writer.WriteString("pt", value.Pt);
        }
        writer.WriteEndObject();
    }
}
