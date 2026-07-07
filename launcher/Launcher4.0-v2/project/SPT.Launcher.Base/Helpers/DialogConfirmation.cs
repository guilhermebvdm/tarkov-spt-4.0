namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 022 (Grupo A / RN-1) — regra pura "abortar no ambíguo" das confirmações destrutivas.
    /// Um diálogo de confirmação só autoriza a ação quando devolve o bool <c>true</c> EXPLÍCITO;
    /// qualquer outro retorno (<c>false</c>, <c>null</c>, dispensa por ESC/click-away/fechamento
    /// programático, ou um tipo inesperado) significa NÃO executar. Extraído para o .Base como
    /// ponto único testável (o resto do fluxo de confirmação é UI-bound e não unit-testável).
    /// </summary>
    public static class DialogConfirmation
    {
        /// <summary>
        /// <c>true</c> somente quando <paramref name="result"/> é o bool <c>true</c>. "Sem resposta"
        /// nunca é "sim": <c>null</c> / não-bool / bool <c>false</c> resultam em <c>false</c>.
        /// </summary>
        public static bool IsConfirmed(object result) => result is bool confirmed && confirmed;
    }
}
