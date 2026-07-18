using System.Collections.Generic;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    public class SyncRuleResolverTests
    {
        [Theory]
        [InlineData("config/foo.cfg", SyncFolderRule.PreserveDivergent)]
        [InlineData("BepInEx/config/foo.cfg", SyncFolderRule.PreserveDivergent)]
        // ref: CR-01-03 — mirror-delete NÃO é default. config-server no fallback = MirrorReference
        // (só espelha config-server/; NUNCA semeia em config/; NÃO deleta extras).
        [InlineData("config-server/db/items.json", SyncFolderRule.MirrorReference)]
        [InlineData("BepInEx/config-server/graphics.cfg", SyncFolderRule.MirrorReference)]
        [InlineData("patchers/x.dll", SyncFolderRule.MirrorMoveDisabled)]
        [InlineData("BepInEx/patchers/x.dll", SyncFolderRule.MirrorMoveDisabled)]
        [InlineData("plugins/mod/mod.dll", SyncFolderRule.MirrorMoveDisabled)]
        [InlineData("BepInEx/plugins/mod/mod.dll", SyncFolderRule.MirrorMoveDisabled)]
        [InlineData("user/mods/some-mod/package.json", SyncFolderRule.Default)]
        [InlineData("EscapeFromTarkov_Data/whatever.bin", SyncFolderRule.Default)]
        public void Fallback_table_maps_card_folders(string path, SyncFolderRule expected)
        {
            var resolver = new SyncRuleResolver();

            Assert.Equal(expected, resolver.Resolve(path));
        }

        [Fact]
        public void Mirror_delete_requires_explicit_server_rule()
        {
            // ref: CR-01-03 — a regra mais destrutiva só ativa via folderRules explícito do server.
            // Sem regra do server, config-server é MirrorReference (só espelha a referência; NÃO deleta
            // extras nem toca config/), NUNCA mirror-delete. Também não entra em MirrorPrefixes (sem varredura/delete).
            var withoutRule = new SyncRuleResolver();
            Assert.Equal(SyncFolderRule.MirrorReference, withoutRule.Resolve("config-server/db/items.json"));
            Assert.DoesNotContain(withoutRule.MirrorPrefixes, p => p.Value == SyncFolderRule.MirrorDelete);
            Assert.DoesNotContain(withoutRule.MirrorPrefixes, p => p.Value == SyncFolderRule.MirrorReference);

            var withRule = new SyncRuleResolver(new Dictionary<string, string>
            {
                ["config-server"] = "mirror-delete",
            });
            Assert.Equal(SyncFolderRule.MirrorDelete, withRule.Resolve("config-server/db/items.json"));
        }

        [Fact]
        public void Server_rules_override_fallback()
        {
            var resolver = new SyncRuleResolver(new Dictionary<string, string>
            {
                ["BepInEx/plugins"] = "mirror-delete",
                ["SPT/user/configs"] = "mirror-delete",
            });

            Assert.Equal(SyncFolderRule.MirrorDelete, resolver.Resolve("BepInEx/plugins/mod.dll"));
            Assert.Equal(SyncFolderRule.MirrorDelete, resolver.Resolve("SPT/user/configs/x.json"));
            // Untouched fallback entries survive the merge
            Assert.Equal(SyncFolderRule.PreserveDivergent, resolver.Resolve("BepInEx/config/foo.cfg"));
        }

        [Fact]
        public void Longest_prefix_wins()
        {
            var resolver = new SyncRuleResolver(new Dictionary<string, string>
            {
                ["BepInEx/plugins/special"] = "preserve-divergent",
            });

            Assert.Equal(SyncFolderRule.PreserveDivergent, resolver.Resolve("BepInEx/plugins/special/tuning.cfg"));
            Assert.Equal(SyncFolderRule.MirrorMoveDisabled, resolver.Resolve("BepInEx/plugins/other/mod.dll"));
        }

        [Fact]
        public void Prefix_match_is_segment_aware()
        {
            var resolver = new SyncRuleResolver();

            // "plugins-disabled" must NOT match the "plugins" prefix
            Assert.Equal(SyncFolderRule.Default, resolver.Resolve("BepInEx/plugins-disabled/old.dll"));
        }

        [Fact]
        public void Unknown_rule_names_are_ignored()
        {
            var resolver = new SyncRuleResolver(new Dictionary<string, string>
            {
                ["BepInEx/plugins"] = "not-a-rule",
            });

            // Invalid name -> entry dropped -> fallback still applies
            Assert.Equal(SyncFolderRule.MirrorMoveDisabled, resolver.Resolve("BepInEx/plugins/mod.dll"));
        }
    }
}
