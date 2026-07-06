using SPT.Launcher.Helpers;
using Xunit;

namespace SPT.Launcher.Tests.Security
{
    /// <summary>
    /// Item 018 (auto-update-security / CA-8) — version comparison is fail-SAFE: only a clean strictly
    /// greater version triggers an update; garbage never does, and nothing throws.
    /// </summary>
    public class UpdateVersionTests
    {
        [Theory]
        [InlineData("2.0.0", "2.1.0", true)]
        [InlineData("2.0.0", "2.0.1", true)]
        [InlineData("2.0.0", "3.0.0", true)]
        [InlineData("2.0.0", "2.0.0", false)]   // equal → not newer
        [InlineData("2.1.0", "2.0.0", false)]   // older → not newer
        [InlineData("2.0.0", "2.1.0+abc123", true)]  // build metadata stripped, then compared
        [InlineData("2.0.0+deadbeef", "2.0.0+cafe", false)] // same core version → not newer
        public void IsNewerVersion_compares_core_versions(string current, string candidate, bool expected)
        {
            Assert.Equal(expected, UpdateVersion.IsNewerVersion(current, candidate));
        }

        [Theory]
        [InlineData("2.0.0", "")]
        [InlineData("2.0.0", null)]
        [InlineData("2.0.0", "not-a-version")]
        [InlineData("", "2.1.0")]
        [InlineData(null, "2.1.0")]
        [InlineData("garbage", "also-garbage")]
        public void IsNewerVersion_malformed_input_is_not_newer_and_does_not_throw(string current, string candidate)
        {
            Assert.False(UpdateVersion.IsNewerVersion(current, candidate));
        }

        [Theory]
        [InlineData("2.1.0+abc", "2.1.0")]
        [InlineData("  2.1.0  ", "2.1.0")]
        [InlineData("2.1.0", "2.1.0")]
        [InlineData(null, "")]
        [InlineData("", "")]
        public void Normalize_strips_metadata_and_whitespace(string input, string expected)
        {
            Assert.Equal(expected, UpdateVersion.Normalize(input));
        }
    }
}
