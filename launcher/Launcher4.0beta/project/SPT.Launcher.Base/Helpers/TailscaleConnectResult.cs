using System;
using System.Collections.Generic;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 023 (Frente C / RN-7) — outcome of the headless Tailscale connection flow. Replaces the
    /// old opaque <c>bool</c> so a caller can tell an authkey problem (rejected / expired / exhausted /
    /// single-use already consumed) apart from a plain network failure — an extra client must know the
    /// problem is the SHARED KEY, not their own internet (CC-6).
    /// </summary>
    public enum TailscaleConnectResult
    {
        /// <summary>A Tailscale IP was obtained and the backend is Running.</summary>
        Connected,

        /// <summary>'tailscale up' failed with an authkey-related error (rejected/expired/exhausted).</summary>
        AuthKeyRejected,

        /// <summary>Failure not attributable to the authkey: network/DNS/control-plane, timeout, etc.</summary>
        NetworkFailure,

        /// <summary>The Tailscale CLI was not present even after the install attempt.</summary>
        NotInstalled,
    }

    /// <summary>
    /// Item 023 (Frente C) — pure, testable classification of a 'tailscale up' failure from its stderr.
    /// Extracted to <c>.Base</c> so the token logic is unit-testable (the process plumbing stays in
    /// <c>TailscaleHelper</c>, which is UI/Win32-bound and not testable).
    /// </summary>
    public static class TailscaleUpClassifier
    {
        /// <summary>
        /// ASCII lower-case tokens whose presence in 'tailscale up' stderr indicates an AUTHKEY problem
        /// rather than a network/control-plane failure. Kept locale-independent (R-5): only the stable
        /// English tokens tailscale emits are matched — never anything locale-dependent.
        /// </summary>
        private static readonly string[] AuthKeyTokens =
        {
            "authkey",
            "auth key",
            "invalid key",
            "key is invalid",
            "expired",
            "single-use",
            "already been used",
            "reusable",
            "unauthorized",
            "invalid authorization",
        };

        /// <summary>The authkey-error tokens — exposed for tests and diagnostics.</summary>
        public static IReadOnlyList<string> AuthKeyErrorTokens => AuthKeyTokens;

        /// <summary>
        /// Classifies a 'tailscale up' outcome. Exit code 0 → <see cref="TailscaleConnectResult.Connected"/>
        /// (the caller's success signal; handled here for completeness). A non-zero exit whose stderr
        /// carries a known authkey token → <see cref="TailscaleConnectResult.AuthKeyRejected"/>;
        /// everything else (empty stderr, network, DNS, control-plane unreachable) →
        /// <see cref="TailscaleConnectResult.NetworkFailure"/> — the safe default (R-5: a misclassified
        /// error degrades to the generic message, it never crashes).
        /// </summary>
        public static TailscaleConnectResult ClassifyUpFailure(string stderr, int exitCode)
        {
            if (exitCode == 0)
            {
                return TailscaleConnectResult.Connected;
            }

            string haystack = (stderr ?? string.Empty).ToLowerInvariant();

            foreach (var token in AuthKeyTokens)
            {
                if (haystack.Contains(token, StringComparison.Ordinal))
                {
                    return TailscaleConnectResult.AuthKeyRejected;
                }
            }

            return TailscaleConnectResult.NetworkFailure;
        }
    }
}
