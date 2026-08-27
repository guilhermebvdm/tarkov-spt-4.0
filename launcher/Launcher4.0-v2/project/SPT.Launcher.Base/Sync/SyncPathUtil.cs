using System;
using System.IO;
using System.Security.Cryptography;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Path normalization + hashing helpers shared by the sync engine.
    /// Canonical relative-path form: forward slashes, no leading slash, lower-case.
    /// </summary>
    public static class SyncPathUtil
    {
        /// <summary>Normalizes a relative path to the canonical comparison form.</summary>
        public static string Normalize(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return string.Empty;
            return relativePath.Replace('\\', '/').TrimStart('/').TrimEnd('/').ToLowerInvariant();
        }

        /// <summary>Converts a manifest-style relative path to an absolute local path under <paramref name="root"/>.</summary>
        public static string ToLocalPath(string root, string relativePath)
        {
            return Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// ref: CR-01-05 / item 019 — defense in depth guard, single source of truth shared by the
        /// sync engine AND the legacy FS paths (deleteFiles loop, OptionalModsHelper). Resolves
        /// <paramref name="relativePath"/> under <paramref name="root"/> and requires the result to
        /// stay under the root: rejects "..", absolute paths and sibling-prefix escapes
        /// (the trailing separator stops "D:\SPT" from matching "D:\SPTevil").
        /// Throws <see cref="InvalidOperationException"/> ("path escapes game root: ...") on escape —
        /// the same message the engine's traversal test asserts on.
        /// </summary>
        public static string ResolveUnderRoot(string root, string relativePath)
        {
            string fullPath = Path.GetFullPath(ToLocalPath(root, relativePath));
            string rootPrefix = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"path escapes game root: {relativePath}");
            }

            return fullPath;
        }

        /// <summary>True when <paramref name="normalizedPath"/> is equal to or nested under <paramref name="normalizedPrefix"/>.</summary>
        public static bool IsUnderPrefix(string normalizedPath, string normalizedPrefix)
        {
            if (string.IsNullOrEmpty(normalizedPrefix)) return false;
            return normalizedPath.Equals(normalizedPrefix, StringComparison.Ordinal)
                   || normalizedPath.StartsWith(normalizedPrefix + "/", StringComparison.Ordinal);
        }

        /// <summary>
        /// True quando o path está DENTRO de uma pasta de DADOS DE RUNTIME de um mod — um segmento
        /// "data" abaixo do mod (ex.: "bepinex/plugins/careerlog/data/x.json"). O
        /// <paramref name="matchedPrefix"/> é a raiz-espelho casada ("bepinex/plugins"); o restante
        /// "&lt;mod&gt;/data/..." só conta se "data" aparece a partir do 2º segmento — o próprio nome do
        /// mod (1º segmento) nunca dispara, então um mod que por acaso se chame "data" não é preservado.
        /// Case-insensitive de graça: ambos vêm de <see cref="Normalize"/> (lower-case). Esses dados são
        /// criados pelo mod, nunca vêm no manifesto, e não devem ser quarentenados como "extra".
        /// </summary>
        public static bool IsRuntimeDataPath(string normalizedPath, string matchedPrefix)
        {
            if (string.IsNullOrEmpty(normalizedPath) || string.IsNullOrEmpty(matchedPrefix)) return false;
            if (!IsUnderPrefix(normalizedPath, matchedPrefix)) return false;
            if (normalizedPath.Length <= matchedPrefix.Length) return false;

            string remainder = normalizedPath.Substring(matchedPrefix.Length).TrimStart('/');
            string[] segs = remainder.Split('/');
            for (int i = 1; i < segs.Length; i++) // começa em 1: pula o nome do mod (segs[0])
            {
                if (segs[i] == "data") return true;
            }
            return false;
        }

        /// <summary>
        /// Sufixos das pastas-FONTE do server que mapeiam para a pasta do usuário sem o sufixo:
        /// "-server" (SeedIfMissingByName, opt-in) e "-force" (force-to-config). Ambos: "&lt;name&gt;-X/&lt;rel&gt;" → "&lt;name&gt;/&lt;rel&gt;".
        /// NB: o default do config-server (MirrorReference) NÃO usa isto — espelha "config-server/&lt;rel&gt;"
        /// direto, sem derivar alvo em "config/". Hoje só o config-force (e o seed opt-in) derivam alvo.
        /// </summary>
        private static readonly string[] SourceFolderSuffixes = { "-server", "-force", "-optional" };

        /// <summary>
        /// Item 030 (D-14/D-20): origem de um arquivo posto em quarentena, para que backups de canais
        /// diferentes nunca colidam por homonímia dentro de "&lt;pasta&gt;-disabled/".
        /// Force e MirrorExtra ficam na RAIZ (formato legado, já em produção desde a 2.3.0 — mover
        /// partiria os backups do force em dois lugares, o oposto do objetivo de D-14). As origens NOVAS
        /// ganham subpasta: mod opcional (optional), config do player sobrescrita ao ligar performance
        /// (optional-config/replaced), config do servidor retirada ao desligar (optional-config/removed).
        /// </summary>
        public enum DisabledOrigin { MirrorExtra, Force, Optional, OptionalConfigReplaced, OptionalConfigRemoved }

        /// <summary>Segmento de subpasta por origem — null = raiz de "&lt;pasta&gt;-disabled/" (Force/MirrorExtra).</summary>
        public static string OriginSegment(DisabledOrigin origin) => origin switch
        {
            DisabledOrigin.Optional => "optional",
            DisabledOrigin.OptionalConfigReplaced => "optional-config/replaced",
            DisabledOrigin.OptionalConfigRemoved => "optional-config/removed",
            _ => null, // MirrorExtra, Force
        };

        /// <summary>
        /// Maps a SERVER source path (e.g. "BepInEx/config-server/a/x.cfg" ou "BepInEx/config-force/a/x.cfg")
        /// to the USER target under the sibling folder without the suffix ("BepInEx/config/a/x.cfg").
        /// "Same name" = relative path within the folder, so subfolders are preserved and keep their
        /// original casing. Returns null when there is no file remainder after the prefix, or when
        /// the inputs are empty.
        /// </summary>
        /// <param name="originalPath">Manifest path, original casing (may use back- or forward slashes).</param>
        /// <param name="normalizedMatchedPrefix">The seed-rule prefix returned by <see cref="SyncRuleResolver.Resolve(string, out string)"/> (normalized/lower-case, length-preserving vs the original).</param>
        public static string DeriveSeedTarget(string originalPath, string normalizedMatchedPrefix)
        {
            if (string.IsNullOrEmpty(originalPath) || string.IsNullOrEmpty(normalizedMatchedPrefix))
            {
                return null;
            }

            string forward = originalPath.Replace('\\', '/').TrimStart('/');
            if (forward.Length < normalizedMatchedPrefix.Length)
            {
                return null;
            }

            // Normalize() only lower-cases + swaps slashes (length-preserving), so the prefix span
            // aligns byte-for-byte with the original — Substring recovers the original casing.
            string originalPrefix = forward.Substring(0, normalizedMatchedPrefix.Length);
            string remainder = forward.Substring(normalizedMatchedPrefix.Length).TrimStart('/');
            if (remainder.Length == 0)
            {
                return null;
            }

            // Sufixos de pasta-FONTE do server que mapeiam para a pasta do USUÁRIO:
            //   "<name>-server/<rel>" -> "<name>/<rel>"   (SeedIfMissingByName opt-in — não o default MirrorReference)
            //   "<name>-force/<rel>"  -> "<name>/<rel>"   (force-to-config)
            // Prefixo sem sufixo conhecido (misconfig do operador): semeia em si mesmo.
            string targetPrefix = originalPrefix;
            foreach (var suffix in SourceFolderSuffixes)
            {
                if (originalPrefix.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    targetPrefix = originalPrefix.Substring(0, originalPrefix.Length - suffix.Length);
                    break;
                }
            }

            return targetPrefix + "/" + remainder;
        }

        /// <summary>
        /// Backup do arquivo que o config-force vai SOBRESCREVER: dado o ALVO ("BepInEx/config/a/x.cfg")
        /// e o prefixo da FONTE ("bepinex/config-force"), devolve "BepInEx/config-disabled/a/x.cfg".
        /// O force é não-destrutivo: a config do jogador é MOVIDA pra cá antes de ser trocada, e pastas
        /// "-disabled" nunca são re-sincronizadas nem deletadas (R3.4) — então nada se perde.
        /// Devolve null se não der pra derivar (aí o engine não faz backup).
        /// </summary>
        public static string DeriveDisabledBackup(string targetPath, string normalizedSourcePrefix) =>
            DeriveDisabledBackup(targetPath, normalizedSourcePrefix, DisabledOrigin.Force);

        /// <summary>
        /// Como o overload acima, mas com a ORIGEM (item 030, D-14/D-20): Force/MirrorExtra caem na raiz
        /// "&lt;pasta&gt;-disabled/&lt;rel&gt;" (formato legado); as origens novas ganham subpasta
        /// "&lt;pasta&gt;-disabled/&lt;origem&gt;/&lt;rel&gt;". Assim a quarentena de um mod opcional ou de uma config
        /// de performance nunca sobrescreve o backup do config-force de mesmo nome.
        /// </summary>
        public static string DeriveDisabledBackup(string targetPath, string normalizedSourcePrefix, DisabledOrigin origin)
        {
            if (string.IsNullOrEmpty(targetPath) || string.IsNullOrEmpty(normalizedSourcePrefix))
            {
                return null;
            }

            // Comprimento do prefixo do ALVO = o da FONTE menos o sufixo que o DeriveSeedTarget removeu.
            int targetPrefixLength = normalizedSourcePrefix.Length;
            foreach (var suffix in SourceFolderSuffixes)
            {
                if (normalizedSourcePrefix.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    targetPrefixLength -= suffix.Length;
                    break;
                }
            }

            string forward = targetPath.Replace('\\', '/').TrimStart('/');
            if (targetPrefixLength <= 0 || targetPrefixLength >= forward.Length)
            {
                return null;
            }

            string prefix = forward.Substring(0, targetPrefixLength);
            string remainder = forward.Substring(targetPrefixLength).TrimStart('/');
            if (remainder.Length == 0)
            {
                return null;
            }

            string segment = OriginSegment(origin);
            return segment == null
                ? prefix + "-disabled/" + remainder
                : prefix + "-disabled/" + segment + "/" + remainder;
        }

        /// <summary>True when any segment of the path ends with "-disabled" (quarantine folders are never re-synced).</summary>
        public static bool ContainsDisabledSegment(string normalizedPath)
        {
            foreach (var segment in normalizedPath.Split('/'))
            {
                if (segment.EndsWith("-disabled", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>MD5 hex (lower-case) of a file — same format used by the server manifest.</summary>
        public static string ComputeMd5(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// MD5 hex (lower-case) of an in-memory buffer.
        /// ref: CR-01-05 (008) — the engine hashes the bytes it actually writes so the baseline
        /// reflects the DISK, not a possibly stale manifest hash.
        /// </summary>
        public static string ComputeMd5(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data ?? Array.Empty<byte>());
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
