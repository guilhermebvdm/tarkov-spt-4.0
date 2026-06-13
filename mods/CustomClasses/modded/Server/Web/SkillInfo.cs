namespace CustomClasses.Web;

/// <summary>
///     Descrições por skill (tooltip do editor — item UX). Texto em HTML simples com &lt;b&gt; nas
///     palavras-chave, renderizado como MarkupString no SkillCanonicalList. Os efeitos são uma
///     aproximação fiel do EFT (variam por patch); a chave é o nome do <c>SkillTypes</c> (casa com
///     <see cref="CustomClasses.SkillMaster"/>). Skill sem descrição → sem tooltip (degrada limpo).
/// </summary>
public static class SkillInfo
{
    public static readonly IReadOnlyDictionary<string, string> Descriptions =
        new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            // ── Physical ──────────────────────────────────────────────────────────────
            ["Endurance"] = "Aumenta a <b>stamina máxima</b> e reduz o <b>consumo</b> ao correr e pular. Melhora o <b>controle da respiração</b>, deixando a mira mais estável após esforço.",
            ["Strength"] = "Aumenta a <b>velocidade de corrida</b>, a <b>altura do pulo</b>, o <b>dano corpo-a-corpo</b> e o <b>peso</b> que dá pra carregar antes das penalidades de sobrepeso.",
            ["Vitality"] = "Reduz a chance de <b>sangramento</b>, aumenta a <b>resistência a dano</b> e a vida efetiva das partes do corpo. Melhora a sobrevivência a <b>ferimentos graves</b>.",
            ["Health"] = "Eleva os <b>pontos de vida</b> de cada parte do corpo e melhora a <b>regeneração natural</b> fora de combate.",
            ["StressResistance"] = "Reduz o <b>tremor das mãos</b> sob dano e vida baixa, mantém a <b>mira estável</b> quando ferido e diminui os efeitos de <b>pânico</b>.",
            ["Metabolism"] = "Melhora o aproveitamento de <b>comida e água</b> (hidratação e energia duram mais) e acelera a eliminação de efeitos <b>tóxicos</b>.",
            ["Immunity"] = "Reduz a <b>chance</b> e a <b>severidade</b> dos efeitos negativos de estimulantes e comida, e aumenta a resistência a <b>intoxicação</b>.",

            // ── Mental ────────────────────────────────────────────────────────────────
            ["Perception"] = "Aumenta o <b>alcance de audição</b> (passos e tiros mais distantes), melhora a <b>visão no escuro</b> e a chance de notar <b>loot destacado</b>.",
            ["Intellect"] = "Acelera o ganho de <b>XP das outras skills</b>, melhora a <b>inspeção</b> de armas e reduz o tempo de ações ligadas a <b>conhecimento</b>.",
            ["Attention"] = "Aumenta a chance e a velocidade de <b>examinar itens</b> e de revelar itens <b>escondidos</b> em contêineres — melhora o <b>loot</b>.",
            ["Charisma"] = "Melhora <b>preços e lealdade com os traders</b>, reduz custos de <b>seguro e serviços</b> e dá pequenos bônus sociais.",
            ["Memory"] = "Faz as skills <b>decaírem mais devagar</b> e, em níveis altos, <b>trava</b> o nível das skills (não regridem mais).",

            // ── Combat ────────────────────────────────────────────────────────────────
            ["Pistol"] = "Proficiência com <b>pistolas</b>: melhora <b>ergonomia</b>, <b>velocidade de mira (ADS)</b>, <b>recarga</b> e <b>controle de recuo</b> dessa classe.",
            ["Revolver"] = "Proficiência com <b>revólveres</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b>, <b>recarga</b> e <b>controle de recuo</b>.",
            ["Assault"] = "Proficiência com <b>fuzis de assalto / carabinas</b>: melhora <b>ergonomia</b>, <b>velocidade de mira (ADS)</b>, <b>recarga</b> e <b>controle de recuo</b>.",
            ["Shotgun"] = "Proficiência com <b>espingardas</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b>, <b>recarga</b> e <b>controle de recuo</b>.",
            ["Sniper"] = "Proficiência com <b>rifles de ferrolho (bolt-action)</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b> e o <b>ciclo do ferrolho</b>.",
            ["DMR"] = "Proficiência com <b>rifles semiautomáticos de precisão (DMR)</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b> e <b>controle de recuo</b>.",
            ["Throwing"] = "Aumenta a <b>distância</b> e a <b>precisão</b> de arremesso de <b>granadas</b> e acelera o saque/uso delas.",
            ["Melee"] = "Aumenta o <b>dano</b>, a <b>velocidade</b> e o <b>alcance</b> dos ataques <b>corpo-a-corpo</b>, e reduz o gasto de stamina.",
            ["RecoilControl"] = "Reduz o <b>recuo</b> vertical e horizontal de <b>todas as armas</b>, melhorando o <b>controle em rajada</b>.",
            ["AimDrills"] = "Aumenta a <b>velocidade de mira (ADS)</b> e a <b>estabilidade</b> ao mirar de todas as armas.",
            ["TroubleShooting"] = "Reduz a chance de <b>emperramento (malfunction)</b> das armas e acelera o <b>conserto</b> de panes.",

            // ── Practical ───────────────────────────────────────────────────────────────
            ["Surgery"] = "Melhora <b>cirurgias de campo</b> (CMS/Surv12): restaura <b>mais vida</b> na parte operada e reduz o <b>tempo</b> de cirurgia.",
            ["CovertMovement"] = "Reduz o <b>ruído dos passos</b> e a assinatura sonora ao se mover — movimento mais <b>furtivo</b>.",
            ["Search"] = "Acelera a <b>revista</b> de contêineres e corpos e melhora a velocidade de revelar itens <b>ocultos</b>.",
            ["MagDrills"] = "Acelera <b>checar munição</b>, <b>recarregar/inspecionar carregadores</b> e o <b>carregamento de balas</b> no pente.",
            ["LightVests"] = "Aumenta a <b>durabilidade efetiva</b> e reduz a <b>penalidade de mobilidade</b> das <b>armaduras leves</b>.",
            ["HeavyVests"] = "Aumenta a <b>durabilidade efetiva</b> e reduz a <b>penalidade de mobilidade</b> das <b>armaduras pesadas</b>.",
            ["WeaponTreatment"] = "Reduz o <b>desgaste de durabilidade</b> das armas com o uso e melhora a <b>manutenção</b>.",
            ["Crafting"] = "Acelera <b>produções no hideout</b> e melhora o rendimento de alguns <b>crafts</b>.",
            ["HideoutManagement"] = "Reduz o <b>consumo de recursos</b> do hideout, acelera <b>construções</b> e potencializa os bônus das áreas.",

            // ── Special Elite (Skills-Extended) ──────────────────────────────────────────
            ["FirstAid"] = "Aprimora o uso de <b>kits médicos</b>: <b>cura mais rápida</b> e <b>menos recursos</b> por uso de bandagem/medkit.",
            ["FieldMedicine"] = "Acelera o tratamento de <b>fraturas e dor</b> e o uso de <b>analgésicos/talas</b>, com menos consumo.",
            ["UsecNegotiations"] = "Passiva <b>USEC</b>: bônus de <b>diálogo e lealdade</b> e vantagens de facção em preços/quests.",
            ["BearRawpower"] = "Passiva <b>BEAR</b>: bônus de <b>força bruta</b> — leve aumento de <b>vida</b>, <b>peso carregável</b> e <b>dano corpo-a-corpo</b>.",
        };

    /// <summary>HTML da descrição da skill (com &lt;b&gt;) ou null se não houver.</summary>
    public static string? Of(string skillName) =>
        Descriptions.TryGetValue(skillName, out var d) ? d : null;
}
