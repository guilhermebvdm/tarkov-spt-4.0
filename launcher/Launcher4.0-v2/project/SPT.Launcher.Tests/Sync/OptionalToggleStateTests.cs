using System.Collections.Generic;
using SPT.Launcher.Helpers;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 033 (Mecanismo 3): guard de alterações não salvas. Cobre CC-8 (sem alteração → não pergunta)
    /// e o caso oposto. <see cref="OptionalToggleState.HasUnsavedChanges"/> é PURA — sem UI nem IO.
    /// Convenção da tupla: (id, é-config, ligado-na-UI). "salvo" = default off para id ausente do dict.
    /// </summary>
    public class OptionalToggleStateTests
    {
        private static (string, bool, bool)[] Toggles(params (string Id, bool IsConfig, bool Enabled)[] items)
        {
            var arr = new (string, bool, bool)[items.Length];
            for (int i = 0; i < items.Length; i++) arr[i] = (items[i].Id, items[i].IsConfig, items[i].Enabled);
            return arr;
        }

        // CC-8 — UI bate 100% com o salvo (incluindo id ausente = desligado) → sem alteração pendente.
        [Fact]
        public void HasUnsavedChanges_false_when_toggles_match_saved()
        {
            var savedMods = new Dictionary<string, bool> { ["foo"] = true, ["bar"] = false };
            var savedConfigs = new Dictionary<string, bool> { ["cfg1"] = true };

            var current = Toggles(
                ("foo", false, true),    // salvo ligado, UI ligado
                ("bar", false, false),   // salvo desligado, UI desligado
                ("baz", false, false),   // ausente do dict (= off), UI desligado
                ("cfg1", true, true),    // config salva ligada, UI ligada
                ("cfg2", true, false));  // config ausente (= off), UI desligada

            Assert.False(OptionalToggleState.HasUnsavedChanges(current, savedMods, savedConfigs));
        }

        // Um mod difere do salvo → há alteração pendente.
        [Fact]
        public void HasUnsavedChanges_true_when_a_mod_toggle_differs()
        {
            var savedMods = new Dictionary<string, bool> { ["foo"] = true };
            var current = Toggles(("foo", false, false)); // salvo ligado, UI desligou

            Assert.True(OptionalToggleState.HasUnsavedChanges(current, savedMods, new Dictionary<string, bool>()));
        }

        // Uma config difere do salvo → há alteração pendente (o eixo de config conta igual).
        [Fact]
        public void HasUnsavedChanges_true_when_a_config_toggle_differs()
        {
            var savedConfigs = new Dictionary<string, bool> { ["cfg1"] = false };
            var current = Toggles(("cfg1", true, true)); // salvo desligado, UI ligou

            Assert.True(OptionalToggleState.HasUnsavedChanges(current, new Dictionary<string, bool>(), savedConfigs));
        }

        // Ligar um mod ausente do dict (salvo = off por default) conta como alteração.
        [Fact]
        public void HasUnsavedChanges_true_when_enabling_an_absent_id()
        {
            var current = Toggles(("novo", false, true)); // ausente do salvo (= off), UI ligou

            Assert.True(OptionalToggleState.HasUnsavedChanges(current, new Dictionary<string, bool>(), new Dictionary<string, bool>()));
        }

        // Lista vazia (tela sem itens) → nunca há alteração.
        [Fact]
        public void HasUnsavedChanges_false_when_no_toggles()
        {
            Assert.False(OptionalToggleState.HasUnsavedChanges(
                Toggles(), new Dictionary<string, bool>(), new Dictionary<string, bool>()));
        }

        // Dicts nulos são tratados como vazios (tudo desligado): UI desligada → false; UI ligada → true.
        [Fact]
        public void HasUnsavedChanges_treats_null_dicts_as_all_off()
        {
            Assert.False(OptionalToggleState.HasUnsavedChanges(Toggles(("a", false, false)), null, null));
            Assert.True(OptionalToggleState.HasUnsavedChanges(Toggles(("a", false, true)), null, null));
        }
    }
}
