using System.Globalization;

namespace CustomClasses;

/// <summary>
///     Item 029 (handoff do launcher) — SNAPSHOT server-side dos perks/drawbacks por classe, para a rota
///     <c>GET /customclasses/classes</c> expor <c>effects[]</c> ao launcher TRL (seletor de classe pré-registro).
///
///     ⚠️ FONTE DE VERDADE = <c>modded/Client/PerksCatalog.cs</c> (Library + ByClass) + <c>MultiplierFormat.ValueToken</c>.
///     O catálogo real é client-only (depende de EFT/EBuffId/PerksConfig). Aqui vive só o dado NOMINAL, sem
///     dependência de EFT: os títulos/labels/valores dos efeitos + a derivação de token/cor. Serve os valores
///     NOMINAIS (não o F12 vivo de cada cliente) — é o correto para um seletor pré-registro.
///
///     Isto é uma DUPLICAÇÃO deliberada (opção "a" do handoff 029-01): ao mexer nos valores/labels no
///     PerksCatalog do client, ESPELHE a mudança aqui — senão o launcher diverge do jogo (drift).
///     Só afeta a exibição no launcher; a mecânica in-game continua 100% no client.
/// </summary>
internal static class PerksCatalogData
{
    internal enum Fmt { Percent, Multiplier, Flag }

    internal enum Pol { HigherBetter, LowerBetter }

    /// <summary>Um efeito atômico (espelha PerksCatalog.PerkLine, sem Icon/Live).</summary>
    internal sealed record Line(
        string TitleEn, string TitlePt, string LabelEn, string LabelPt,
        Fmt Format, double Multiplier, Pol Polarity, bool FlagIsPerk = false, bool Pending = false);

    private static Line P(string te, string tp, string le, string lp, Fmt f, double m, Pol pol)
        => new(te, tp, le, lp, f, m, pol);

    private static Line Fl(string te, string tp, string le, string lp, bool isPerk)
        => new(te, tp, le, lp, Fmt.Flag, 1.0, Pol.HigherBetter, FlagIsPerk: isPerk);

    /// <summary>
    ///     Nome EN da classe (DisplayName.En = chave do ByClass do client) → efeitos ACHATADOS na ordem de
    ///     exibição (ordem de ByClass × ordem das Lines do grupo). Perks e drawbacks misturados; a coluna é
    ///     decidida por <see cref="IsPerk"/>. Classe ausente aqui (ex.: Peladão/Naked) → sem effects.
    /// </summary>
    internal static readonly Dictionary<string, Line[]> ByClass = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Combat Medic"] = new[]
        {
            P("Rapid Care", "Cuidado Rápido", "heal/stab use time", "tempo de cura/estabilização", Fmt.Percent, 0.7, Pol.LowerBetter),
            P("Swift Surgeon", "Cirurgião Ágil", "surgery time", "tempo de cirurgia", Fmt.Percent, 0.5, Pol.LowerBetter),
            Fl("Mobile Surgery", "Cirurgia em Movimento", "walk during surgery", "andar durante a cirurgia", true),
            P("Efficient Metabolism", "Metabolismo Eficiente", "hunger/thirst drain", "fome/sede", Fmt.Percent, 0.85, Pol.LowerBetter),
            P("Shaky Hands", "Mãos Trêmulas", "recoil", "recuo", Fmt.Multiplier, 1.25, Pol.LowerBetter),
        },
        ["Rifleman"] = new[]
        {
            P("Cool Under Fire", "Sangue-frio", "flinch when hit", "flinch ao levar dano", Fmt.Multiplier, 0.5, Pol.LowerBetter),
            P("Anti-Jam", "Antitravamento", "weapon jam chance", "chance de travamento", Fmt.Multiplier, 0.5, Pol.LowerBetter),
            P("Adrenaline Grip", "Pegada de Adrenalina", "recoil (combat window)", "recuo (janela de combate)", Fmt.Multiplier, 0.7, Pol.LowerBetter),
            P("Adrenaline Reload", "Recarga de Adrenalina", "reload time (combat window)", "recarga (janela de combate)", Fmt.Multiplier, 0.8, Pol.LowerBetter),
            P("Adrenaline Focus", "Foco de Adrenalina", "ADS time (combat window)", "ADS (janela de combate)", Fmt.Multiplier, 0.8, Pol.LowerBetter),
            P("Loud Operator", "Barulhento", "noise", "ruído", Fmt.Percent, 1.3, Pol.LowerBetter),
        },
        ["Hunter"] = new[]
        {
            P("Sharpshooter", "Atirador", "aim (ADS) time, all weapons", "mira (ADS), todas as armas", Fmt.Percent, 0.85, Pol.LowerBetter),
            P("Iron Lungs", "Fôlego de Aço", "breath hold duration", "duração da respiração", Fmt.Percent, 1.5, Pol.HigherBetter),
            P("Steady Arms", "Braços Firmes", "arm fatigue when aiming", "fadiga de braço ao mirar", Fmt.Percent, 0.65, Pol.LowerBetter),
            P("Calm Sights", "Mira Serena", "aim/movement sway", "oscilação de mira/movimento", Fmt.Percent, 0.7, Pol.LowerBetter),
            P("Stalker", "Espreita", "movement/action noise", "ruído de movimento/ações", Fmt.Percent, 0.8, Pol.LowerBetter),
            P("Rooted", "Enraizado", "move speed while aiming", "velocidade ao mirar", Fmt.Percent, 0.85, Pol.HigherBetter),
        },
        ["Stealth"] = new[]
        {
            P("Ghost Step", "Passo Fantasma", "movement/action noise", "ruído de movimento/ações", Fmt.Percent, 0.7, Pol.LowerBetter),
            P("Execution", "Execução", "melee damage", "dano de melee", Fmt.Multiplier, 3.5, Pol.HigherBetter),
            P("Swift Blade", "Lâmina Veloz", "move speed with melee", "velocidade c/ melee na mão", Fmt.Percent, 1.1, Pol.HigherBetter),
            P("Rattled", "Abalado", "aim punch when hit", "tranco na mira ao ser atingido", Fmt.Percent, 1.5, Pol.LowerBetter),
        },
        ["Scavenger"] = new[]
        {
            Fl("Quick Hands", "Mãos Rápidas", "search 2 containers at once", "revista 2 contêineres de uma vez", true),
            Fl("Silent Looter", "Saque Silencioso", "silent looting", "saque silencioso", true),
            P("Pack Mule", "Mula de Carga", "carry limit", "limite de carga", Fmt.Percent, 1.3, Pol.HigherBetter),
            Fl("Overladen", "Sobrecarregado", "inertia scales with weight", "inércia escala com o peso", false),
        },
        ["Tank"] = new[]
        {
            P("Pack Mule", "Mula de Carga", "carry limit", "limite de carga", Fmt.Percent, 1.3, Pol.HigherBetter),
            P("Bulwark", "Couraça", "damage taken (with heavy armor)", "dano recebido (com colete pesado)", Fmt.Percent, 0.85, Pol.LowerBetter),
            P("Steady Mount", "Apoio Firme", "recoil (LMG/HMG/GL)", "recuo (LMG/HMG/GL)", Fmt.Multiplier, 0.85, Pol.LowerBetter),
            P("Heavy Handling", "Manejo Pesado", "ergonomics (LMG/HMG/GL)", "ergonomia (LMG/HMG/GL)", Fmt.Multiplier, 1.15, Pol.HigherBetter),
            Fl("Grenadier", "Granadeiro", "GL: no ergo penalty", "lança-granadas: sem penalidade de ergo", true),
            P("Tireless Arms", "Braços Incansáveis", "arm fatigue (heavy weapon)", "fadiga de braço (arma pesada)", Fmt.Multiplier, 0.20, Pol.LowerBetter),
            P("Heavy Frame", "Estrutura Pesada", "move speed", "velocidade", Fmt.Percent, 0.9, Pol.HigherBetter),
            P("Heavy Appetite", "Apetite Pesado", "hunger/thirst drain", "fome/sede", Fmt.Percent, 1.3, Pol.LowerBetter),
            P("Loud Operator", "Barulhento", "noise", "ruído", Fmt.Percent, 1.3, Pol.LowerBetter),
        },
    };

    /// <summary>Espelha PerksCatalog.PerkLine.IsPerk (Flag → FlagIsPerk; senão direção boa × direção do mult).</summary>
    internal static bool IsPerk(Line l) => l.Format == Fmt.Flag
        ? l.FlagIsPerk
        : (l.Polarity == Pol.HigherBetter) == (l.Multiplier > 1.0);

    /// <summary>
    ///     Espelha MultiplierFormat.ValueToken: Percent "+30%"/"−10%" (menos = U+2212), Multiplier "×0.85"
    ///     (× = U+00D7), Flag "✓"/"✗" (U+2713/U+2717). InvariantCulture p/ o ponto decimal do "0.##".
    /// </summary>
    internal static string ValueToken(Line l) => l.Format switch
    {
        Fmt.Percent => (l.Multiplier > 1.0 ? "+" : "−")
            + ((int)System.Math.Round(System.Math.Abs(l.Multiplier - 1.0) * 100.0)).ToString(CultureInfo.InvariantCulture) + "%",
        Fmt.Multiplier => "×" + l.Multiplier.ToString("0.##", CultureInfo.InvariantCulture),
        _ => IsPerk(l) ? "✓" : "✗",
    };
}
