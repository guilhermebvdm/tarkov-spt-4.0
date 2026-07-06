using System;
using System.Security.Cryptography;

namespace SPT.Launcher.Security
{
    /// <summary>
    /// Item 018 (auto-update-security) — PURE, testable signature verification of a downloaded launcher
    /// exe against the embedded PUBLIC key. This is the PRIMARY barrier against the auto-update RCE
    /// (01-spec §Modelo de ameaça): it holds even if the transport is 100% compromised, because forging
    /// a valid signature requires the private key.
    ///
    /// FAIL-CLOSED (01-spec RN-1): every failure path — null/empty inputs, malformed PEM, malformed
    /// signature, unknown algorithm, any crypto exception — returns <c>false</c>. There is NO path that
    /// throws or returns <c>true</c> on doubt. The caller MUST NOT promote/execute the exe unless this
    /// returns <c>true</c>.
    ///
    /// No I/O here (spec: verifier lives in .Base so SPT.Launcher.Tests can exercise it). The caller
    /// recomputes nothing extra — it passes the exact bytes on disk (the same file it will promote,
    /// closing the TOCTOU window, R-1).
    /// </summary>
    public static class UpdateSignatureVerifier
    {
        /// <summary>
        /// Verifies that <paramref name="signature"/> is a valid signature of <paramref name="payload"/>
        /// under <paramref name="publicKeyPem"/> for <paramref name="algorithm"/>.
        /// </summary>
        /// <param name="payload">Raw bytes of the downloaded exe (authority = these bytes on disk, CA-6).</param>
        /// <param name="signature">Raw (already base64-decoded) detached signature bytes.</param>
        /// <param name="publicKeyPem">PEM of the trusted public key (SubjectPublicKeyInfo / RSA public key).</param>
        /// <param name="algorithm">"RSA-SHA256" or "ECDSA-SHA256" (D-018.1). Anything else → false.</param>
        /// <returns><c>true</c> only if the signature verifies; <c>false</c> on ANY problem (never throws).</returns>
        public static bool Verify(byte[] payload, byte[] signature, string publicKeyPem, string algorithm)
        {
            // Fail-closed on any missing/empty input.
            if (payload == null || payload.Length == 0) return false;
            if (signature == null || signature.Length == 0) return false;
            if (string.IsNullOrWhiteSpace(publicKeyPem)) return false;
            if (string.IsNullOrWhiteSpace(algorithm)) return false;

            try
            {
                switch (algorithm.Trim().ToUpperInvariant())
                {
                    case "RSA-SHA256":
                    {
                        using var rsa = RSA.Create();
                        rsa.ImportFromPem(publicKeyPem);
                        return rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    }
                    case "ECDSA-SHA256":
                    {
                        using var ecdsa = ECDsa.Create();
                        ecdsa.ImportFromPem(publicKeyPem);
                        // ECDSA .sig from `openssl dgst -sha256 -sign` is DER-encoded (Rfc3279 / SEQUENCE{r,s}).
                        return ecdsa.VerifyData(payload, signature, HashAlgorithmName.SHA256,
                            DSASignatureFormat.Rfc3279DerSequence);
                    }
                    default:
                        // Unknown algorithm → cannot vouch for the bytes → reject.
                        return false;
                }
            }
            catch (Exception)
            {
                // Malformed PEM, malformed signature, wrong key type, etc. — never trust on error.
                return false;
            }
        }

        /// <summary>
        /// Convenience overload: decodes a base64 signature (as delivered in the /version JSON) and
        /// verifies it. Invalid base64 → <c>false</c> (never throws). See <see cref="Verify(byte[],byte[],string,string)"/>.
        /// </summary>
        public static bool VerifyBase64(byte[] payload, string signatureBase64, string publicKeyPem, string algorithm)
        {
            if (string.IsNullOrWhiteSpace(signatureBase64)) return false;

            byte[] signature;
            try
            {
                signature = Convert.FromBase64String(signatureBase64.Trim());
            }
            catch (FormatException)
            {
                return false;
            }

            return Verify(payload, signature, publicKeyPem, algorithm);
        }
    }
}
