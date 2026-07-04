using System.Collections.Generic;
using SPT.Launcher.Helpers;
using Xunit;

namespace SPT.Launcher.Tests
{
    /// <summary>
    /// Item 020 (DP-020.A = A2) — cobertura da fonte de verdade do critério de igualdade do cofre.
    /// O server-mod TRL (PasswordController) espelha estes mesmos casos em helpers privados.
    /// </summary>
    public class VaultKeyMatcherTests
    {
        [Theory]
        [InlineData("Bob", "bob")]
        [InlineData("BOB", "bob")]
        [InlineData("bob", "bob")]
        [InlineData("  ", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void Canonicalize_lowercases_and_empties_nulls(string input, string expected)
        {
            Assert.Equal(expected, VaultKeyMatcher.Canonicalize(input));
        }

        [Theory]
        [InlineData("Bob")]
        [InlineData("bob")]
        [InlineData("BoB")]
        public void Canonicalize_is_idempotent(string input)
        {
            string once = VaultKeyMatcher.Canonicalize(input);
            Assert.Equal(once, VaultKeyMatcher.Canonicalize(once));
        }

        [Theory]
        // AC-020.1/020.2/020.3 — mesmo usuário sob o critério (case-insensitive)
        [InlineData("Bob", "bob", true)]
        [InlineData("Bob", "Bob", true)]
        [InlineData("BOB", "bob", true)]
        // usuários distintos
        [InlineData("Bob", "Alice", false)]
        // vazios/nulos nunca casam
        [InlineData("", "", false)]
        [InlineData(null, "bob", false)]
        [InlineData("Bob", null, false)]
        [InlineData("   ", "bob", false)]
        public void Matches_uses_case_insensitive_criterion(string a, string b, bool expected)
        {
            Assert.Equal(expected, VaultKeyMatcher.Matches(a, b));
            // simétrico
            Assert.Equal(expected, VaultKeyMatcher.Matches(b, a));
        }

        [Fact]
        public void CollidesWith_detects_casing_variant_of_existing_user()
        {
            // AC-020.4 — registrar `bob` com `Bob` já existente é recusado.
            var existing = new List<string> { "Bob", "Alice" };

            Assert.True(VaultKeyMatcher.CollidesWith(existing, "bob"));
            Assert.True(VaultKeyMatcher.CollidesWith(existing, "BOB"));
            Assert.True(VaultKeyMatcher.CollidesWith(existing, "ALICE"));
        }

        [Fact]
        public void CollidesWith_allows_genuinely_new_username()
        {
            var existing = new List<string> { "Bob", "Alice" };

            Assert.False(VaultKeyMatcher.CollidesWith(existing, "Charlie"));
        }

        [Fact]
        public void CollidesWith_is_safe_with_empty_or_null_inputs()
        {
            Assert.False(VaultKeyMatcher.CollidesWith(null, "bob"));
            Assert.False(VaultKeyMatcher.CollidesWith(new List<string>(), "bob"));
            Assert.False(VaultKeyMatcher.CollidesWith(new List<string> { "Bob" }, ""));
            Assert.False(VaultKeyMatcher.CollidesWith(new List<string> { "Bob" }, null));
            Assert.False(VaultKeyMatcher.CollidesWith(new List<string> { "Bob" }, "   "));
        }

        [Fact]
        public void CollidesWith_ignores_null_entries_in_the_existing_list()
        {
            var existing = new List<string> { null, "Bob", null };

            Assert.True(VaultKeyMatcher.CollidesWith(existing, "bob"));
            Assert.False(VaultKeyMatcher.CollidesWith(existing, "charlie"));
        }
    }
}
