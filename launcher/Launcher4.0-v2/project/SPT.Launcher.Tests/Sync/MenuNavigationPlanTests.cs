using System.Collections.Generic;
using SPT.Launcher.Helpers;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Navegação por menu (comportamento "abas"): clicar um item leva SEMPRE àquela tela, sem "subir um
    /// nível" nem acumular telas. Cobre TODAS as combinações origem×destino entre Launcher/ModsConfigs/
    /// Settings — inclusive as pilhas profundas que causavam o bug (ModsConfigs→LAUNCHER ia p/ Settings;
    /// Settings→LAUNCHER passava por ModsConfigs).
    /// </summary>
    public class MenuNavigationPlanTests
    {
        private const string L = "Launcher";
        private const string MC = "ModsConfigs";
        private const string ST = "Settings";

        private static string Top(IReadOnlyList<string> s) => s.Count == 0 ? null : s[s.Count - 1];

        // 9 combinações origem×destino a partir das pilhas rasas canônicas ([L], [L,MC], [L,ST]).
        [Theory]
        // origem = Launcher [L]
        [InlineData(new[] { L }, L, new[] { L })]
        [InlineData(new[] { L }, MC, new[] { L, MC })]
        [InlineData(new[] { L }, ST, new[] { L, ST })]
        // origem = ModsConfigs [L, MC]
        [InlineData(new[] { L, MC }, L, new[] { L })]
        [InlineData(new[] { L, MC }, MC, new[] { L, MC })]
        [InlineData(new[] { L, MC }, ST, new[] { L, ST })]
        // origem = Settings [L, ST]
        [InlineData(new[] { L, ST }, L, new[] { L })]
        [InlineData(new[] { L, ST }, MC, new[] { L, MC })]
        [InlineData(new[] { L, ST }, ST, new[] { L, ST })]
        public void All_menu_transitions_resolve_to_the_clicked_screen(string[] stack, string target, string[] expected)
        {
            var result = MenuNavigationPlan.Apply(stack, L, target);

            Assert.Equal(expected, result);
            Assert.Equal(target, Top(result));   // o topo é SEMPRE a tela clicada
            Assert.True(result.Count <= 2);       // pilha nunca passa de [Launcher, Tela]
        }

        // Pilhas profundas/acumuladas (o cenário do bug com push cego) ainda resolvem certo.
        [Theory]
        [InlineData(new[] { L, MC, ST }, L, new[] { L })]
        [InlineData(new[] { L, MC, ST }, MC, new[] { L, MC })]
        [InlineData(new[] { L, MC, ST }, ST, new[] { L, ST })]
        [InlineData(new[] { L, MC, ST, MC }, L, new[] { L })]     // ModsConfigs → LAUNCHER (ia p/ Settings)
        [InlineData(new[] { L, MC, ST, MC }, ST, new[] { L, ST })]
        [InlineData(new[] { L, ST, MC }, L, new[] { L })]         // Settings → LAUNCHER (passava por ModsConfigs)
        [InlineData(new[] { L, ST, MC }, ST, new[] { L, ST })]
        [InlineData(new[] { L, MC, ST, MC, ST }, MC, new[] { L, MC })]
        public void Deep_stacks_still_resolve_to_launcher_plus_target(string[] stack, string target, string[] expected)
        {
            var result = MenuNavigationPlan.Apply(stack, L, target);

            Assert.Equal(expected, result);
            Assert.Equal(target, Top(result));
            Assert.True(result.Count <= 2);
        }

        // Clicar o mesmo item de novo não muda nada (idempotente).
        [Theory]
        [InlineData(L)]
        [InlineData(MC)]
        [InlineData(ST)]
        public void Applying_the_same_target_twice_is_idempotent(string target)
        {
            var once = MenuNavigationPlan.Apply(new[] { L, MC, ST }, L, target);
            var twice = MenuNavigationPlan.Apply(once, L, target);

            Assert.Equal(once, twice);
        }

        // Home ausente (modo enxuto / pré-login): não trava — mantém a pilha e empilha o alvo (espelha
        // o degradê do NavigateMenu real). Alvo == home e home ausente → pilha inalterada.
        [Fact]
        public void Home_absent_keeps_stack_and_pushes_target()
        {
            Assert.Equal(new[] { "ConnectServer", ST }, MenuNavigationPlan.Apply(new[] { "ConnectServer" }, L, ST));
            Assert.Equal(new[] { "ConnectServer" }, MenuNavigationPlan.Apply(new[] { "ConnectServer" }, L, L));
        }
    }
}
