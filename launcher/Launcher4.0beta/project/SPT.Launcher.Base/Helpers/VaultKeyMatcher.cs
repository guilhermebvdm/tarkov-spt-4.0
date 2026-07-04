using System;
using System.Collections.Generic;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 020 (DP-020.A = A2) — ponto único do critério de igualdade de username do cofre
    /// de senhas (<c>user/profiles/redline_passwords.json</c>).
    ///
    /// O cofre é indexado por uma chave canônica lowercase; duas contas do core que diferem só no
    /// casing (<c>Bob</c> vs <c>bob</c>) colapsariam numa mesma entrada e uma senha gravaria no
    /// perfil errado. O modelo escolhido (A2) mantém a chave lowercase e IMPEDE, no registro, criar
    /// um username que colida case-insensitive com um já existente — assim a colisão é impossível na
    /// origem, sem depender de o core casar case-sensitive ou não.
    ///
    /// Esta é a fonte de verdade (pura, testável) usada pelo launcher. O
    /// <c>PasswordController</c> do server-mod TRL espelha o MESMO critério em helpers privados
    /// (assemblies separados não compartilham código — o <c>.Tests</c> referencia só o <c>.Base</c>).
    /// </summary>
    public static class VaultKeyMatcher
    {
        /// <summary>
        /// Chave canônica de um username no cofre: lowercase invariant (formato herdado do fix D4).
        /// <c>null</c>/vazio/whitespace → string vazia. NÃO faz trim para preservar byte-a-byte o
        /// formato de chave já persistido em instalações existentes.
        /// </summary>
        public static string Canonicalize(string username)
            => string.IsNullOrWhiteSpace(username) ? string.Empty : username.ToLowerInvariant();

        /// <summary>
        /// <c>true</c> quando dois usernames são "o mesmo usuário" sob o critério do cofre
        /// (igualdade case-insensitive). Usernames vazios/nulos NUNCA casam.
        /// </summary>
        public static bool Matches(string a, string b)
        {
            string ca = Canonicalize(a);
            return ca.Length > 0 && string.Equals(ca, Canonicalize(b), StringComparison.Ordinal);
        }

        /// <summary>
        /// <c>true</c> quando <paramref name="candidate"/> colide (case-insensitive) com algum username
        /// já existente — inclusive um match exato (o core já barra duplicata exata; aqui a proteção
        /// nova é a variante de casing). Entradas nulas em <paramref name="existingUsernames"/> são
        /// ignoradas; candidate vazio/nulo nunca colide.
        /// </summary>
        public static bool CollidesWith(IEnumerable<string> existingUsernames, string candidate)
        {
            if (existingUsernames == null) return false;

            string canonicalCandidate = Canonicalize(candidate);
            if (canonicalCandidate.Length == 0) return false;

            foreach (string existing in existingUsernames)
            {
                if (string.Equals(Canonicalize(existing), canonicalCandidate, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
