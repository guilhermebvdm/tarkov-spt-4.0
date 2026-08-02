using System.Collections.Generic;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Regra PURA da navegação por itens do menu lateral do launcher (comportamento "abas", não "voltar").
    /// A navigation stack é modelada como uma lista de tags do fundo→topo; <c>home</c> é a tag da tela
    /// Launcher. Clicar um item de menu deve levar SEMPRE àquela tela, venha o usuário de onde vier —
    /// sem "subir um nível" (pop cego) nem acumular telas duplicadas (push cego). Testável sem UI/Router;
    /// o <see cref="SPT.Launcher.ViewModels.ViewModelBase"/> aplica a mesma regra sobre o Router real.
    /// </summary>
    public static class MenuNavigationPlan
    {
        /// <summary>
        /// Aplica um clique de menu (<paramref name="target"/>) sobre a <paramref name="stack"/> atual:
        /// mantém a <paramref name="home"/> (1ª ocorrência) e tudo abaixo dela, descarta o que está acima,
        /// e empilha o alvo — exceto quando o alvo é a própria home (aí fica só na home). Retorna a nova
        /// stack; o ÚLTIMO elemento é a tela que fica visível. Home ausente (ex.: modo enxuto/pré-login):
        /// devolve <c>[target]</c>, ou <c>[]</c> quando o alvo é a home.
        /// </summary>
        public static IReadOnlyList<string> Apply(IReadOnlyList<string> stack, string home, string target)
        {
            var result = new List<string>();

            int homeIdx = -1;
            for (int i = 0; i < stack.Count; i++)
            {
                if (stack[i] == home) { homeIdx = i; break; }
            }

            if (homeIdx < 0)
            {
                // Sem home na pilha (modo enxuto/pré-login, sidebar escondido — caso defensivo): não há
                // para onde "voltar". Mantém a pilha e só empilha o alvo, espelhando o degradê do
                // NavigateMenu real (Navigate.Execute(target) sobre a pilha atual; target == home → nada).
                foreach (var s in stack) result.Add(s);
                if (target != home) result.Add(target);
                return result;
            }

            // Mantém a home (1ª ocorrência) + tudo abaixo, descarta o que está acima, e empilha o alvo
            // (salvo quando o alvo é a própria home — aí para na home).
            for (int i = 0; i <= homeIdx; i++) result.Add(stack[i]);
            if (target != home) result.Add(target);
            return result;
        }
    }
}
