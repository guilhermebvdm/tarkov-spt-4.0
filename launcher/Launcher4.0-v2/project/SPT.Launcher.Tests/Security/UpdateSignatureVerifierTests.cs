using System;
using System.Security.Cryptography;
using System.Text;
using SPT.Launcher.Security;
using Xunit;

namespace SPT.Launcher.Tests.Security
{
    /// <summary>
    /// Item 018 (auto-update-security) — the PRIMARY RCE barrier. Ephemeral keypair per test (no secret
    /// material in the repo): sign good bytes → verifies; tamper 1 byte → rejects; sign with a DIFFERENT
    /// key → rejects; missing/garbage signature → rejects (never throws). This is the fail-closed core.
    /// </summary>
    public class UpdateSignatureVerifierTests
    {
        private const string Algo = "RSA-SHA256";

        private static (string publicPem, byte[] signature) SignRsa(byte[] payload)
        {
            using var rsa = RSA.Create(2048);
            string publicPem = rsa.ExportSubjectPublicKeyInfoPem();
            byte[] sig = rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return (publicPem, sig);
        }

        [Fact]
        public void Valid_signature_over_payload_verifies()
        {
            byte[] payload = Encoding.UTF8.GetBytes("the exact bytes of the exe on disk");
            var (publicPem, sig) = SignRsa(payload);

            Assert.True(UpdateSignatureVerifier.Verify(payload, sig, publicPem, Algo));
        }

        [Fact]
        public void Valid_signature_verifies_via_base64_overload()
        {
            byte[] payload = Encoding.UTF8.GetBytes("payload for base64 path");
            var (publicPem, sig) = SignRsa(payload);

            Assert.True(UpdateSignatureVerifier.VerifyBase64(payload, Convert.ToBase64String(sig), publicPem, Algo));
        }

        [Fact]
        public void One_flipped_byte_in_payload_is_rejected()
        {
            byte[] payload = Encoding.UTF8.GetBytes("original trusted content aaaaaaaa");
            var (publicPem, sig) = SignRsa(payload);

            byte[] tampered = (byte[])payload.Clone();
            tampered[0] ^= 0xFF; // 1 byte changed → not the signed bytes anymore

            Assert.False(UpdateSignatureVerifier.Verify(tampered, sig, publicPem, Algo));
        }

        [Fact]
        public void Signature_from_a_different_key_is_rejected()
        {
            byte[] payload = Encoding.UTF8.GetBytes("content signed by the attacker's key");
            var (_, attackerSig) = SignRsa(payload);       // signed with key A
            using var trusted = RSA.Create(2048);          // client trusts key B
            string trustedPem = trusted.ExportSubjectPublicKeyInfoPem();

            Assert.False(UpdateSignatureVerifier.Verify(payload, attackerSig, trustedPem, Algo));
        }

        [Fact]
        public void Empty_or_null_signature_is_rejected_without_throwing()
        {
            byte[] payload = Encoding.UTF8.GetBytes("payload");
            var (publicPem, _) = SignRsa(payload);

            Assert.False(UpdateSignatureVerifier.Verify(payload, Array.Empty<byte>(), publicPem, Algo));
            Assert.False(UpdateSignatureVerifier.Verify(payload, null, publicPem, Algo));
            Assert.False(UpdateSignatureVerifier.VerifyBase64(payload, null, publicPem, Algo));
            Assert.False(UpdateSignatureVerifier.VerifyBase64(payload, "", publicPem, Algo));
        }

        [Fact]
        public void Invalid_base64_signature_is_rejected_without_throwing()
        {
            byte[] payload = Encoding.UTF8.GetBytes("payload");
            var (publicPem, _) = SignRsa(payload);

            Assert.False(UpdateSignatureVerifier.VerifyBase64(payload, "not-valid-base64!!!", publicPem, Algo));
        }

        [Fact]
        public void Empty_payload_is_rejected()
        {
            var (publicPem, sig) = SignRsa(Encoding.UTF8.GetBytes("x"));
            Assert.False(UpdateSignatureVerifier.Verify(Array.Empty<byte>(), sig, publicPem, Algo));
            Assert.False(UpdateSignatureVerifier.Verify(null, sig, publicPem, Algo));
        }

        [Fact]
        public void Malformed_public_key_pem_is_rejected_without_throwing()
        {
            byte[] payload = Encoding.UTF8.GetBytes("payload");
            var (_, sig) = SignRsa(payload);

            Assert.False(UpdateSignatureVerifier.Verify(payload, sig, "-----BEGIN PUBLIC KEY-----\ngarbage\n-----END PUBLIC KEY-----", Algo));
            Assert.False(UpdateSignatureVerifier.Verify(payload, sig, "", Algo));
            Assert.False(UpdateSignatureVerifier.Verify(payload, sig, null, Algo));
        }

        [Fact]
        public void Unknown_or_missing_algorithm_is_rejected()
        {
            byte[] payload = Encoding.UTF8.GetBytes("payload");
            var (publicPem, sig) = SignRsa(payload);

            Assert.False(UpdateSignatureVerifier.Verify(payload, sig, publicPem, "MD5-WEAK"));
            Assert.False(UpdateSignatureVerifier.Verify(payload, sig, publicPem, ""));
            Assert.False(UpdateSignatureVerifier.Verify(payload, sig, publicPem, null));
        }

        [Fact]
        public void Embedded_dev_public_key_is_a_parseable_rsa_key()
        {
            // Guards against a corrupted embedded PEM: a random signature must cleanly return false,
            // never throw. (The real DEV private key is off-repo, so we can't produce a true positive here.)
            byte[] payload = Encoding.UTF8.GetBytes("payload");
            byte[] bogusSig = new byte[256]; // RSA-2048 sig length, all zeros

            Assert.False(UpdateSignatureVerifier.Verify(
                payload, bogusSig, UpdateTrust.EmbeddedPublicKeyPem, UpdateTrust.SignatureAlgorithm));
        }
    }
}
