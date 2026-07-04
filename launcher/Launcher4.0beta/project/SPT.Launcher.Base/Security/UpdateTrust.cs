namespace SPT.Launcher.Security
{
    /// <summary>
    /// Item 018 (auto-update-security / RN-2) — the client's ROOT OF TRUST for the launcher auto-update,
    /// baked into the binary at compile time (never downloaded). Compromising the update server does NOT
    /// change what the client trusts: only whoever holds the matching PRIVATE key can produce a signature
    /// this key accepts, so a tampered/forged exe is rejected even over a fully hostile transport.
    ///
    /// PRIMARY barrier = the signing keypair below (stops the RCE). SPKI pinning is defense-in-depth.
    ///
    /// == SECURITY GATES (see 01-spec G-4/G-5, D-018.1/D-018.2) ==
    ///  * <see cref="EmbeddedPublicKeyPem"/> below is a **DEV** key. PRODUCTION must generate its own
    ///    keypair, keep the PRIVATE key off the repo / off any synced path (Syncthing), embed the
    ///    matching PUBLIC key here, and sign the released exe with the private key. See
    ///    `launcher/Launcher4.0beta/tools/README.md` and `tools/sign-launcher.ps1`.
    ///  * <see cref="SpkiPins"/> is EMPTY by default → transport pinning is inert (bootstrap). G-5:
    ///    fill it with the SPKI SHA-256 pin(s) of the production server certificate before relying on
    ///    the pinning layer. The signature layer already fail-closes the RCE regardless of pins.
    /// </summary>
    public static class UpdateTrust
    {
        /// <summary>
        /// Signature algorithm the client expects. One of {"RSA-SHA256", "ECDSA-SHA256"} (D-018.1).
        /// Must match how the release signs the exe (see tools/sign-launcher.ps1). DEV default: RSA-SHA256
        /// (RSA-2048, PKCS#1 v1.5 over SHA-256 — what `openssl dgst -sha256 -sign` produces).
        /// </summary>
        public const string SignatureAlgorithm = "RSA-SHA256";

        /// <summary>
        /// PUBLIC key used to verify the detached signature of the downloaded exe. Public by nature —
        /// safe to keep in source. **DEV KEY — replace at production release (D-018.2 / Gate G-4).**
        /// The private counterpart lives ONLY under the gitignored `.keys/` folder on the signer machine.
        /// </summary>
        public const string EmbeddedPublicKeyPem =
            """
            -----BEGIN PUBLIC KEY-----
            MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAu5ETaUqitpY3Usdyr2dL
            0M/Fqirog5x08czk2GacViqbI7S8k/JbUty8/ZoYgqTbFyLKSqk42KH/hpAJajmA
            wo3dclSLtlnd0eO4OPjUwFM5kvBYzZwCszYW706va15PNjnA5cXG3C4IKQPVVetN
            UgVwfIfqYEcjRmseFhlR6biaeQLQS7Acznd7RPP943LlKRZicTfehNdvnDafhyAc
            4X8qm1iNJVBcxonkJqPUeoWSyx+t8OmHs4I+oe3f3utrnXmPkdKnciGkMqmx5lIj
            v0IOQQfXF1bSqs81BO8XsOAgTJrwdjaq5TwX1LcX13K/VN9G98j3V67QVOP8jnvA
            WQIDAQAB
            -----END PUBLIC KEY-----
            """;

        /// <summary>
        /// SPKI SHA-256 pins (base64) of the ACCEPTED server TLS certificate(s). Up to 2 entries so a
        /// cert can be rotated without bricking clients: publish the new pin in a release BEFORE
        /// rotating the server cert (D-018.4).
        ///
        /// EMPTY = pinning not yet provisioned (bootstrap). While empty, the transport is NOT pinned
        /// (the connection is accepted on TLS grounds) — but the exe SIGNATURE still fail-closes any
        /// RCE. Gate G-5: extract the production pin (see tools/get-spki-pin.ps1) and put it here to
        /// turn the defense-in-depth layer on.
        /// </summary>
        public static readonly string[] SpkiPins = System.Array.Empty<string>();
    }
}
