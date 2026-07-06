using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SPT.Launcher.Security
{
    /// <summary>
    /// Item 018 (auto-update-security / RN-5) — SPKI (SubjectPublicKeyInfo) SHA-256 certificate pinning.
    /// Defense-in-depth (SECONDARY to the exe signature): replaces the old
    /// <c>ServerCertificateCustomValidationCallback =&gt; true</c> that accepted ANY cert. Pins the
    /// server's PUBLIC KEY (not the whole cert), so the cert can be re-issued for the same key without
    /// breaking clients.
    ///
    /// Pure/testable: <see cref="ComputeSpkiPin"/> and <see cref="IsPinned(X509Certificate2, IReadOnlyCollection{string})"/>
    /// take their inputs explicitly. The app policy <see cref="IsAllowedForConnection"/> reads
    /// <see cref="UpdateTrust.SpkiPins"/> and applies the bootstrap rule (empty pins → transport not
    /// pinned yet; the SIGNATURE layer still fail-closes the RCE — see UpdateTrust / Gate G-5).
    /// </summary>
    public static class CertificatePinning
    {
        /// <summary>
        /// Computes the pin = base64( SHA-256( DER SubjectPublicKeyInfo of the cert's public key ) ).
        /// Returns <c>null</c> if the cert is null or the SPKI cannot be exported (never throws).
        /// </summary>
        public static string ComputeSpkiPin(X509Certificate2 cert)
        {
            if (cert == null) return null;

            try
            {
                byte[] spki = cert.PublicKey.ExportSubjectPublicKeyInfo();
                byte[] hash = SHA256.HashData(spki);
                return Convert.ToBase64String(hash);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Strict pin check against an explicit pin set (used by tests and by the app overload).
        /// <c>true</c> only if the cert's SPKI pin is present in <paramref name="pins"/>. A null cert,
        /// a null/empty pin set, or an uncomputable pin → <c>false</c> (fail-closed, CA-4).
        /// </summary>
        public static bool IsPinned(X509Certificate2 cert, IReadOnlyCollection<string> pins)
        {
            if (cert == null) return false;
            if (pins == null || pins.Count == 0) return false;

            string pin = ComputeSpkiPin(cert);
            if (pin == null) return false;

            foreach (var expected in pins)
            {
                if (string.Equals(expected, pin, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Strict pin check against the embedded <see cref="UpdateTrust.SpkiPins"/>.</summary>
        public static bool IsPinned(X509Certificate2 cert) => IsPinned(cert, UpdateTrust.SpkiPins);

        /// <summary>
        /// APP POLICY for <c>ServerCertificateCustomValidationCallback</c>. When pins are configured,
        /// enforce them strictly (fail-closed on a wrong/absent pin, CA-4). When the pin list is EMPTY
        /// (not yet provisioned — Gate G-5), the transport is accepted (bootstrap): the self-signed SPT
        /// cert has no CA to validate against, and the exe SIGNATURE — not the transport — is what stops
        /// the RCE. This is the deliberate, documented pre-G-5 state, NOT the old blanket <c>=&gt; true</c>.
        /// </summary>
        public static bool IsAllowedForConnection(X509Certificate2 cert)
        {
            if (UpdateTrust.SpkiPins == null || UpdateTrust.SpkiPins.Length == 0)
            {
                // Bootstrap: pinning off, signature still enforced downstream.
                return true;
            }

            return IsPinned(cert, UpdateTrust.SpkiPins);
        }
    }
}
