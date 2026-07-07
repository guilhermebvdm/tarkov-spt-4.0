using SPT.Launcher.Helpers;
using Xunit;

namespace SPT.Launcher.Tests
{
    /// <summary>
    /// Item 022 (Grupo A / RN-1, AC-022.1..4) — cobertura da regra pura "abortar no ambíguo"
    /// das confirmações destrutivas (wipe/remove/delete). Só o bool <c>true</c> explícito autoriza
    /// a ação; qualquer outro retorno do diálogo (false/null/não-bool) significa NÃO executar.
    /// </summary>
    public class DialogConfirmationTests
    {
        [Fact]
        public void IsConfirmed_true_when_result_is_boolean_true()
        {
            // AC-022.1 — confirmação explícita executa a ação.
            Assert.True(DialogConfirmation.IsConfirmed(true));
        }

        [Fact]
        public void IsConfirmed_false_when_result_is_boolean_false()
        {
            // AC-022.2/022.3 — negação explícita aborta.
            Assert.False(DialogConfirmation.IsConfirmed(false));
        }

        [Fact]
        public void IsConfirmed_false_when_result_is_null()
        {
            // AC-022.2/022.3 — dispensa por ESC/click-away/fechamento programático (null) aborta.
            Assert.False(DialogConfirmation.IsConfirmed(null));
        }

        [Theory]
        // AC-022.2/022.3/022.4 — qualquer retorno não-bool aborta ("sem resposta" nunca é "sim").
        [InlineData("true")]
        [InlineData("x")]
        [InlineData("")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1.5)]
        public void IsConfirmed_false_when_result_is_not_a_boolean(object result)
        {
            Assert.False(DialogConfirmation.IsConfirmed(result));
        }

        [Fact]
        public void IsConfirmed_false_when_result_is_an_arbitrary_object()
        {
            Assert.False(DialogConfirmation.IsConfirmed(new object()));
        }
    }
}
