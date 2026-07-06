using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SPT.Launcher.Security;
using Xunit;

namespace SPT.Launcher.Tests.Security
{
    /// <summary>
    /// Item 018 (auto-update-security / RN-5) — SPKI pinning (defense-in-depth). Self-signed fixture
    /// certs (no fixture files): a cert whose SPKI pin is in the list is accepted; a different cert or
    /// an empty/absent list is rejected (fail-closed, CA-4); null is safe.
    /// </summary>
    public class CertificatePinningTests
    {
        private static X509Certificate2 MakeSelfSigned(string cn)
        {
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest($"CN={cn}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        }

        [Fact]
        public void ComputeSpkiPin_is_stable_and_base64()
        {
            using var cert = MakeSelfSigned("pin-stable");
            string pin1 = CertificatePinning.ComputeSpkiPin(cert);
            string pin2 = CertificatePinning.ComputeSpkiPin(cert);

            Assert.False(string.IsNullOrEmpty(pin1));
            Assert.Equal(pin1, pin2);
            // base64 of a SHA-256 (32 bytes) is 44 chars ending in '='
            Assert.Equal(44, pin1.Length);
            Assert.EndsWith("=", pin1);
        }

        [Fact]
        public void ComputeSpkiPin_null_cert_is_null()
        {
            Assert.Null(CertificatePinning.ComputeSpkiPin(null));
        }

        [Fact]
        public void Cert_with_pin_in_list_is_pinned()
        {
            using var cert = MakeSelfSigned("in-list");
            string pin = CertificatePinning.ComputeSpkiPin(cert);

            Assert.True(CertificatePinning.IsPinned(cert, new[] { pin }));
            Assert.True(CertificatePinning.IsPinned(cert, new[] { "someOtherPinAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", pin }));
        }

        [Fact]
        public void Cert_not_in_list_is_rejected()
        {
            using var certA = MakeSelfSigned("cert-a");
            using var certB = MakeSelfSigned("cert-b");
            string pinB = CertificatePinning.ComputeSpkiPin(certB);

            Assert.False(CertificatePinning.IsPinned(certA, new[] { pinB }));
        }

        [Fact]
        public void Empty_or_null_pin_list_is_rejected()
        {
            using var cert = MakeSelfSigned("empty-list");

            Assert.False(CertificatePinning.IsPinned(cert, Array.Empty<string>()));
            Assert.False(CertificatePinning.IsPinned(cert, null));
        }

        [Fact]
        public void Null_cert_is_rejected()
        {
            Assert.False(CertificatePinning.IsPinned(null, new[] { "anything" }));
        }

        [Fact]
        public void IsAllowedForConnection_bootstrap_accepts_when_pins_empty()
        {
            // Default embedded UpdateTrust.SpkiPins is empty → bootstrap mode accepts the transport
            // (the exe signature is the real RCE barrier). This documents the pre-G-5 state.
            using var cert = MakeSelfSigned("bootstrap");
            Assert.Empty(UpdateTrust.SpkiPins);
            Assert.True(CertificatePinning.IsAllowedForConnection(cert));
        }
    }
}
