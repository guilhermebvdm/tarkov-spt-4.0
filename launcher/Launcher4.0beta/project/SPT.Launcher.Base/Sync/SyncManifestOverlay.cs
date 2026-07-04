using System;
using System.Collections.Generic;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Item 008: merges the server performance-overlay pack over the main manifest, producing
    /// the EFFECTIVE manifest for a single planner/engine pass ("planner with an extra source").
    ///
    /// Overlay entries override main-manifest entries by normalized path; overlay-only entries
    /// are appended. Rationale (spec 008, D3): a literal second pass after the normal sync would
    /// churn on every run — the normal pass reverts overlaid files to server defaults (they equal
    /// the baseline) and the overlay pass re-applies the pack. Merging is semantically identical
    /// ("the overlay wins over the normal source") without the double download, and the planner,
    /// engine and baseline logic run unchanged: the engine records the EFFECTIVE hash in the
    /// baseline after each apply, which is what makes turning the toggle OFF revertible (D1).
    /// </summary>
    public sealed class SyncManifestOverlay
    {
        private readonly List<ManifestFile> _files;
        private readonly HashSet<string> _overlayPaths;

        private SyncManifestOverlay(List<ManifestFile> files, HashSet<string> overlayPaths)
        {
            _files = files;
            _overlayPaths = overlayPaths;
        }

        /// <summary>Effective manifest: base entries (overlaid where applicable) + overlay-only entries.</summary>
        public IReadOnlyList<ManifestFile> Files => _files;

        /// <summary>
        /// Builds the effective manifest. Base entries whose normalized path exists in the overlay
        /// take the overlay hash/size (download source becomes the pack) but KEEP the base
        /// optional/optionalGroup flags — the pack never force-installs files of disabled optional
        /// groups (A-008.3). Overlay entries without a base counterpart are appended as mandatory.
        /// </summary>
        public static SyncManifestOverlay Merge(
            IReadOnlyList<ManifestFile> baseFiles,
            IReadOnlyList<ManifestFile> overlayFiles)
        {
            baseFiles = baseFiles ?? Array.Empty<ManifestFile>();
            overlayFiles = overlayFiles ?? Array.Empty<ManifestFile>();

            var overlayByPath = new Dictionary<string, ManifestFile>(StringComparer.Ordinal);
            foreach (var overlay in overlayFiles)
            {
                if (overlay == null || string.IsNullOrWhiteSpace(overlay.path)) continue;
                overlayByPath[SyncPathUtil.Normalize(overlay.path)] = overlay;
            }

            var files = new List<ManifestFile>(baseFiles.Count + overlayByPath.Count);
            var consumed = new HashSet<string>(StringComparer.Ordinal);

            foreach (var baseFile in baseFiles)
            {
                if (baseFile == null || string.IsNullOrWhiteSpace(baseFile.path)) continue;

                string normalized = SyncPathUtil.Normalize(baseFile.path);

                if (overlayByPath.TryGetValue(normalized, out var overlay))
                {
                    consumed.Add(normalized);
                    files.Add(new ManifestFile
                    {
                        path = baseFile.path,
                        hash = overlay.hash,
                        size = overlay.size,
                        optional = baseFile.optional,
                        optionalGroup = baseFile.optionalGroup,
                    });
                }
                else
                {
                    files.Add(baseFile);
                }
            }

            foreach (var overlay in overlayFiles)
            {
                if (overlay == null || string.IsNullOrWhiteSpace(overlay.path)) continue;

                string normalized = SyncPathUtil.Normalize(overlay.path);
                if (consumed.Contains(normalized)) continue;
                if (!consumed.Add(normalized)) continue; // duplicate inside the pack itself

                files.Add(new ManifestFile
                {
                    path = overlay.path,
                    hash = overlay.hash,
                    size = overlay.size,
                    optional = false,
                    optionalGroup = null,
                });
            }

            return new SyncManifestOverlay(files, new HashSet<string>(overlayByPath.Keys, StringComparer.Ordinal));
        }

        /// <summary>True when the path belongs to the performance pack (download must hit the pack endpoint).</summary>
        public bool IsOverlayPath(string relativePath)
        {
            return _overlayPaths.Contains(SyncPathUtil.Normalize(relativePath));
        }

        /// <summary>
        /// Wraps two download sources into one: pack paths go to <paramref name="overlayDownloader"/>,
        /// everything else to <paramref name="baseDownloader"/>. Keeps the engine source-agnostic.
        /// </summary>
        public SyncDownloader CreateDownloader(SyncDownloader baseDownloader, SyncDownloader overlayDownloader)
        {
            if (baseDownloader == null) throw new ArgumentNullException(nameof(baseDownloader));
            if (overlayDownloader == null) throw new ArgumentNullException(nameof(overlayDownloader));

            return (relativePath, cancellationToken) =>
                IsOverlayPath(relativePath)
                    ? overlayDownloader(relativePath, cancellationToken)
                    : baseDownloader(relativePath, cancellationToken);
        }
    }
}
