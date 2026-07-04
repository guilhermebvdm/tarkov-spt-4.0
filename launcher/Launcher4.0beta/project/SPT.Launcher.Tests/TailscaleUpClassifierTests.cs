using SPT.Launcher.Helpers;
using Xunit;

namespace SPT.Launcher.Tests
{
    /// <summary>
    /// Item 023 (Frente C / RN-7) — pure classification of a 'tailscale up' failure by stderr signature.
    /// AuthKey rejection must be distinguishable from a network failure (CA-C1 vs CA-C2).
    /// </summary>
    public class TailscaleUpClassifierTests
    {
        [Fact]
        public void ExitZero_is_connected_regardless_of_stderr()
        {
            Assert.Equal(TailscaleConnectResult.Connected,
                TailscaleUpClassifier.ClassifyUpFailure("some noise", 0));
        }

        [Theory]
        [InlineData("authkey expired")]
        [InlineData("provided auth key was already been used")]      // single-use consumed (CC-6)
        [InlineData("invalid key: unauthorized")]
        [InlineData("auth key is not reusable")]
        [InlineData("AUTHKEY EXPIRED")]                              // case-insensitive
        public void AuthKey_signatures_classify_as_authkey_rejected(string stderr)
        {
            Assert.Equal(TailscaleConnectResult.AuthKeyRejected,
                TailscaleUpClassifier.ClassifyUpFailure(stderr, 1));
        }

        [Theory]
        [InlineData("control plane unreachable")]
        [InlineData("dial tcp: lookup controlplane.tailscale.com: no such host")]
        [InlineData("")]                                            // empty stderr → safe default
        [InlineData(null)]                                          // null stderr → safe default
        public void Network_or_empty_signatures_classify_as_network_failure(string stderr)
        {
            Assert.Equal(TailscaleConnectResult.NetworkFailure,
                TailscaleUpClassifier.ClassifyUpFailure(stderr, 1));
        }

        [Fact]
        public void AuthKey_tokens_are_exposed()
        {
            Assert.NotEmpty(TailscaleUpClassifier.AuthKeyErrorTokens);
        }
    }
}
