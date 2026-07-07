using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace TRLItemsManagement;

/// <summary>
///     Writes a <see cref="JsonNode"/> tree back to disk preserving the ORIGINAL file's indent style
///     (SPT vanilla configs use tabs) and trailing-newline state, atomically (tmp + rename). Ported from
///     <c>serve.js</c>'s <c>writeJsonPreservingStyle</c> — needed because <c>SPT_Data/checks.dat</c>'s
///     hash is computed over the exact bytes, not the logical JSON content; a "prettier" reserialization
///     (e.g. .NET's default 2-space indent) would produce a byte-identical-content-but-different-bytes
///     file and invalidate the manifest even though nothing meaningful changed.
///     <para>
///     Operates on <see cref="JsonNode"/> (not a strongly-typed model) so a surgical edit — flip one
///     dictionary entry, toggle one bool — never risks dropping a field the C# model doesn't happen to
///     declare; the rest of the tree round-trips untouched.
///     </para>
/// </summary>
internal static class StyleSensitiveJsonWriter
{
    private static readonly Regex IndentSniffPattern = new(@"^([\t ]+)""", RegexOptions.Multiline);

    internal static void Write(string path, JsonNode root)
    {
        var original = System.IO.File.ReadAllText(path);
        var normalized = original.TrimStart('﻿');

        var match = IndentSniffPattern.Match(normalized);
        char indentChar;
        int indentSize;
        if (!match.Success)
        {
            indentChar = '\t';
            indentSize = 1;
        }
        else
        {
            var indent = match.Groups[1].Value;
            if (indent[0] == '\t')
            {
                indentChar = '\t';
                indentSize = 1;
            }
            else
            {
                indentChar = ' ';
                indentSize = indent.Length;
            }
        }

        var writerOptions = new JsonWriterOptions
        {
            Indented = true,
            IndentCharacter = indentChar,
            IndentSize = indentSize,
            // Utf8JsonWriter's default encoder escapes every non-ASCII char (\uXXXX) — safe for a JSON
            // parser either way, but bloats the file and rewrites bytes that didn't need to change vs
            // the Node original (JSON.stringify never escapes non-ASCII). UnsafeRelaxedJsonEscaping only
            // escapes what JSON itself requires (control chars, ", \); "unsafe" refers to HTML-injection
            // contexts, irrelevant for a server-side config/database file.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, writerOptions))
        {
            root.WriteTo(writer);
        }

        // Utf8JsonWriter's indented mode emits Environment.NewLine for line breaks — CRLF on Windows.
        // The Node original (JSON.stringify) always emits bare \n; force the same here so an edit
        // doesn't rewrite every line's ending (defeats the whole point of "minimal diff against vanilla").
        var serialized = Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n");
        if (original.EndsWith('\n'))
        {
            serialized += "\n";
        }

        var tmp = path + ".tmp";
        System.IO.File.WriteAllText(tmp, serialized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        System.IO.File.Move(tmp, path, overwrite: true);
    }
}
