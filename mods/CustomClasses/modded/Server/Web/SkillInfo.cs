namespace CustomClasses.Web;

/// <summary>
///     Descrições por skill (tooltip do editor — item UX). Cada entrada tem duas partes em HTML
///     simples renderizadas como MarkupString no SkillCanonicalList:
///     <list type="number">
///         <item>frase qualitativa (o que a skill faz) com &lt;b&gt; nas palavras-chave;</item>
///         <item>
///             linha <c>.cc-skill-tip__fx</c> com os <b>números reais</b> (efeito no nível 50 e no
///             elite) — derivados de <c>docs/class-skill-catalog.md</c> (builder nativo do EFT,
///             SkillsConfig.json do Skills-Extended e globals.json). Valores marcados como inferidos
///             no catálogo ([inf]) são descritos sem percentual duro.
///         </item>
///     </list>
///     A chave é o nome do <c>SkillTypes</c> (casa com <see cref="CustomClasses.SkillMaster"/>,
///     case-insensitive). Skill sem descrição → sem tooltip (degrada limpo).
/// </summary>
public static class SkillInfo
{
    public static readonly IReadOnlyDictionary<string, string> Descriptions =
        new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            // ── Physical (Skills-Extended) ──────────────────────────────────────────────
            ["Endurance"] = "Aumenta a <b>stamina máxima</b> e reduz o <b>consumo</b> ao correr e pular. Melhora o <b>controle da respiração</b>, deixando a mira mais estável após esforço."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> stamina máx <b>+50%</b>, recuperação <b>+50%</b>, custo de pulo <b>−30%</b>, respiração <b>+100%</b>. <b>Elite:</b> stamina <b>+70%</b>, recuperação <b>+75%</b>.</span>",
            ["Strength"] = "Aumenta a <b>velocidade de corrida</b>, a <b>altura do pulo</b>, o <b>dano corpo-a-corpo</b> e o <b>peso</b> que dá pra carregar antes das penalidades de sobrepeso."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> corrida <b>+20%</b>, pulo <b>+20%</b>, carga <b>+30%</b>, melee <b>+30%</b>, arremesso <b>+20%</b>. <b>Elite:</b> crítico de melee <b>+50%</b>.</span>",
            ["Vitality"] = "Reduz a chance de <b>sangramento</b>, aumenta a <b>resistência a dano</b> e a vida efetiva das partes do corpo. Melhora a sobrevivência a <b>ferimentos graves</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> chance de sangrar <b>−60%</b>, sobrevivência <b>+20%</b>. <b>Elite:</b> regenera e estanca sangramento.</span>",
            ["Health"] = "Eleva os <b>pontos de vida</b> de cada parte do corpo e melhora a <b>regeneração natural</b> fora de combate."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> fratura/quebra <b>−60%</b>, energia e hidratação máx <b>+30%</b>. <b>Elite:</b> absorve parte do dano.</span>",
            ["StressResistance"] = "Reduz o <b>tremor das mãos</b> sob dano e vida baixa, mantém a <b>mira estável</b> quando ferido e diminui os efeitos de <b>pânico</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> tremor <b>−60%</b>, dor <b>−50%</b>. <b>Elite:</b> modo berserk.</span>",
            ["Metabolism"] = "Melhora o aproveitamento de <b>comida e água</b> (hidratação e energia duram mais) e acelera a eliminação de efeitos <b>tóxicos</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> recuperação de energia/sede <b>+50%</b>, duração de debuff <b>−50%</b>. <b>Elite:</b> não desidrata.</span>",
            ["Immunity"] = "Reduz a <b>chance</b> e a <b>severidade</b> dos efeitos negativos de estimulantes e comida, e aumenta a resistência a <b>intoxicação</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> efeitos negativos <b>−50%</b>, veneno <b>−50%</b>, painkiller <b>+30%</b>. <b>Elite:</b> <b>90%</b> de evitar veneno/efeitos.</span>",

            // ── Mental ────────────────────────────────────────────────────────────────
            ["Perception"] = "Aumenta o <b>alcance de audição</b> (passos e tiros mais distantes), melhora a <b>visão no escuro</b> e a chance de notar <b>loot destacado</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> raio de loot destacado <b>dobra</b> (0,1→0,2). <b>Elite:</b> distância de destaque <b>1,5→2,35</b>.</span>",
            ["Intellect"] = "Acelera o ganho de <b>XP das outras skills</b>, melhora a <b>inspeção</b> de armas e reduz o tempo de ações ligadas a <b>conhecimento</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> custo de pontos de reparo <b>−20%</b>; aprendizado e manutenção de arma sobem por nível. <b>Elite:</b> contador de munição, mira em container.</span>",
            ["Attention"] = "Aumenta a chance e a velocidade de <b>examinar itens</b> e de revelar itens <b>escondidos</b> em contêineres — melhora o <b>loot</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> exame e revelação de loot bem mais rápidos. <b>Elite:</b> examinar <b>instantâneo</b>.</span>",
            ["Charisma"] = "Melhora <b>preços e lealdade com os traders</b>, reduz custos de <b>seguro e serviços</b> e dá pequenos bônus sociais."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> cura/seguro/saída-paga/reroll <b>−5%</b> cada. <b>Elite:</b> scav case <b>−10%</b>, perda de Fence <b>−50%</b>, <b>+1</b> daily quest.</span>",
            ["Memory"] = "Faz as skills <b>decaírem mais devagar</b> e, em níveis altos, <b>trava</b> o nível das skills (não regridem mais)."
                + "<span class='cc-skill-tip__fx'>Sem efeito in-raid. <b>Elite:</b> skills <b>não decaem</b> mais.</span>",

            // ── Combat ────────────────────────────────────────────────────────────────
            ["Pistol"] = "Proficiência com <b>pistolas</b>: melhora <b>ergonomia</b>, <b>velocidade de mira (ADS)</b>, <b>recarga</b> e <b>controle de recuo</b> dessa classe."
                + "<span class='cc-skill-tip__fx'><b>L50 (maestria):</b> recuo da classe <b>−15%</b>; ergonomia sobe por nível.</span>",
            ["Revolver"] = "Proficiência com <b>revólveres</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b>, <b>recarga</b> e <b>controle de recuo</b>."
                + "<span class='cc-skill-tip__fx'><b>L50 (maestria):</b> recuo da classe <b>−15%</b>; ergonomia sobe por nível.</span>",
            ["Assault"] = "Proficiência com <b>fuzis de assalto / carabinas</b>: melhora <b>ergonomia</b>, <b>velocidade de mira (ADS)</b>, <b>recarga</b> e <b>controle de recuo</b>."
                + "<span class='cc-skill-tip__fx'><b>L50 (maestria):</b> recuo da classe <b>−15%</b>; ergonomia sobe por nível.</span>",
            ["Shotgun"] = "Proficiência com <b>espingardas</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b>, <b>recarga</b> e <b>controle de recuo</b>."
                + "<span class='cc-skill-tip__fx'><b>L50 (maestria):</b> recuo da classe <b>−15%</b>; ergonomia sobe por nível.</span>",
            ["Sniper"] = "Proficiência com <b>rifles de ferrolho (bolt-action)</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b> e o <b>ciclo do ferrolho</b>."
                + "<span class='cc-skill-tip__fx'><b>L50 (maestria):</b> recuo da classe <b>−15%</b>; ergonomia sobe por nível.</span>",
            ["DMR"] = "Proficiência com <b>rifles semiautomáticos de precisão (DMR)</b>: melhora <b>ergonomia</b>, <b>velocidade de mira</b> e <b>controle de recuo</b>."
                + "<span class='cc-skill-tip__fx'><b>L50 (maestria):</b> recuo da classe <b>−15%</b>; ergonomia sobe por nível.</span>",
            ["Throwing"] = "Aumenta a <b>distância</b> e a <b>precisão</b> de arremesso de <b>granadas</b> e acelera o saque/uso delas."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> custo de stamina do arremesso <b>−50%</b> (a distância usa a skill <b>Strength</b>). <b>Elite:</b> arremesso sem tremor.</span>",
            ["Melee"] = "Aumenta o <b>dano</b>, a <b>velocidade</b> e o <b>alcance</b> dos ataques <b>corpo-a-corpo</b>, e reduz o gasto de stamina."
                + "<span class='cc-skill-tip__fx'>Escala dano/velocidade/alcance por nível (valores nativos do EFT).</span>",
            ["RecoilControl"] = "Reduz o <b>recuo</b> vertical e horizontal de <b>todas as armas</b>, melhorando o <b>controle em rajada</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> recuo de <b>todas</b> as armas <b>−15%</b> (−0,3%/nível).</span>",
            ["AimDrills"] = "Aumenta a <b>velocidade de mira (ADS)</b> e a <b>estabilidade</b> ao mirar de todas as armas."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> velocidade de saque/mira <b>+50%</b>; som do saque <b>−50%</b>. <b>Elite:</b> saque sem tremor.</span>",
            ["TroubleShooting"] = "Reduz a chance de <b>emperramento (malfunction)</b> das armas e acelera o <b>conserto</b> de panes."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> conserto de pane <b>−25%</b> mais rápido. <b>Elite:</b> chance de falha de munição/durabilidade/pente <b>−30%</b>.</span>",

            // ── Practical ───────────────────────────────────────────────────────────────
            ["Surgery"] = "Melhora <b>cirurgias de campo</b> (CMS/Surv12): restaura <b>mais vida</b> na parte operada e reduz o <b>tempo</b> de cirurgia."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> tempo <b>−16,7%</b>, penalidade de HP <b>−50%</b> (restaura mais vida). <b>Elite:</b> tempo <b>−28,6%</b>, penalidade <b>zerada</b> (restaura <b>100%</b>).</span>",
            ["CovertMovement"] = "Reduz o <b>ruído dos passos</b> e a assinatura sonora ao se mover — movimento mais <b>furtivo</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> volume de passo e de equipamento <b>−60%</b>; velocidade furtiva <b>×1,5</b>. <b>Elite:</b> quase silêncio agachado.</span>",
            ["Search"] = "Acelera a <b>revista</b> de contêineres e corpos e melhora a velocidade de revelar itens <b>ocultos</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> revista <b>+50%</b> mais rápida. <b>Elite:</b> busca <b>2 contêineres</b> ao mesmo tempo.</span>",
            ["MagDrills"] = "Acelera <b>checar munição</b>, <b>recarregar/inspecionar carregadores</b> e o <b>carregamento de balas</b> no pente."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> carregar/descarregar pente <b>−30%</b>, checar munição <b>−40%</b>. <b>Elite:</b> checagem <b>instantânea</b>.</span>",
            ["LightVests"] = "Aumenta a <b>durabilidade efetiva</b> e reduz a <b>penalidade de mobilidade</b> das <b>armaduras leves</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> penalidade de mobilidade <b>−30%</b>, desgaste no reparo <b>−40%</b>. <b>Elite:</b> <b>−50%</b> chance de deteriorar no reparo.</span>",
            ["HeavyVests"] = "Aumenta a <b>durabilidade efetiva</b> e reduz a <b>penalidade de mobilidade</b> das <b>armaduras pesadas</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> penalidade de mobilidade <b>−25%</b>, dano contuso atravessado <b>−20%</b>. <b>Elite:</b> <b>+5%</b> ricochete e <b>−50%</b> desgaste.</span>",
            ["WeaponTreatment"] = "Reduz o <b>desgaste de durabilidade</b> das armas com o uso e melhora a <b>manutenção</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> perda de durabilidade ao atirar <b>−25%</b>, desgaste ao reparar <b>−50%</b>. <b>Elite:</b> <b>−50%</b> chance de deteriorar.</span>",
            ["Crafting"] = "Acelera <b>produções no hideout</b> e melhora o rendimento de alguns <b>crafts</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> tempo de craft <b>−37,5%</b>. <b>Elite:</b> <b>+1</b> produção simultânea.</span>",
            ["HideoutManagement"] = "Reduz o <b>consumo de recursos</b> do hideout, acelera <b>construções</b> e potencializa os bônus das áreas."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> consumo de recursos <b>−25%</b>; bônus de área <b>+1%/nível</b>. <b>Elite:</b> <b>slots</b> extras.</span>",

            // ── Médicas (Skills-Extended) ─────────────────────────────────────────────────
            ["FirstAid"] = "Aprimora o uso de <b>kits médicos</b>: <b>cura mais rápida</b> e <b>menos recursos</b> por uso de bandagem/medkit."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> uso de medkit <b>+30%</b> mais rápido, recurso consumido <b>−25%</b>. <b>Elite:</b> anda com <b>perna quebrada</b>.</span>",
            ["FieldMedicine"] = "Acelera o tratamento de <b>fraturas e dor</b> e potencializa <b>estimulantes/injetores</b>, com menos consumo."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> limite de efeito do stim sobe p/ <b>75</b>, duração <b>+50%</b>, chance de efeito positivo <b>+25%</b>.</span>",

            // ── Gems (Skills-Extended) ────────────────────────────────────────────────────
            ["UsecArsystems"] = "Domínio de <b>armas ocidentais (NATO)</b>: melhora <b>ergonomia</b> e reduz <b>recuo</b> dessas armas."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> em armas NATO, ergonomia <b>+30%</b>, recuo <b>−20%</b>; <b>+10%</b> do XP cruza p/ Eastern.</span>",
            ["BearAksystems"] = "Domínio de <b>armas do Leste (AK)</b>: melhora <b>ergonomia</b> e reduz <b>recuo</b> dessas armas."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> em armas do Leste, ergonomia <b>+20%</b>, recuo <b>−15%</b>.</span>",
            ["ProneMovement"] = "Mover-se <b>deitado (prone)</b> com mais <b>velocidade</b> e <b>silêncio</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> velocidade ao rastejar <b>+30%</b>, volume <b>−40%</b>.</span>",
            ["SilentOps"] = "Operações silenciosas: <b>melee</b> rápido, abrir <b>portas/loot</b> sem ruído e <b>supressores</b> mais baratos."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> melee <b>+25%</b> mais rápido, ruído de porta/loot <b>−50%</b>, preço de supressor <b>−25%</b>.</span>",
            ["Lockpicking"] = "Arrombar <b>fechaduras</b> (minigame): mais <b>força do pick</b> e <b>tolerância</b>, menos quebras."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> força do pick <b>+125%</b>, tolerância do sweet-spot <b>+75%</b>; quebra na 3ª falha. <b>Elite:</b> <b>não consome</b> lockpick.</span>",
            ["ShadowConnections"] = "Conexões obscuras (XP <b>matando cultistas</b>): reduz o <b>cooldown do Scav</b> e influencia o <b>Círculo de Cultistas</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> cooldown de Scav <b>−50%</b>, chance de Scav nascer cultista <b>25%</b>. <b>Elite:</b> cooldown <b>zerado</b>.</span>",

            // ── Faction passives ──────────────────────────────────────────────────────────
            ["UsecNegotiations"] = "Passiva <b>USEC</b>: descontos com <b>Peacekeeper</b> e mais <b>dinheiro de quest</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> custo no Peacekeeper <b>−25%</b>, recompensa de quest <b>+25%</b>. <b>Elite:</b> <b>−7%</b> em todos os traders.</span>",
            ["BearRawpower"] = "Passiva <b>BEAR</b>: descontos com <b>Prapor</b> e mais <b>XP de quest</b>."
                + "<span class='cc-skill-tip__fx'><b>L50:</b> custo no Prapor <b>−25%</b>, XP de quest <b>+25%</b>. <b>Elite:</b> <b>−7%</b> em todos os traders.</span>",
        };

    /// <summary>HTML da descrição da skill (com &lt;b&gt; / linha .cc-skill-tip__fx) ou null se não houver.</summary>
    public static string? Of(string skillName) =>
        Descriptions.TryGetValue(skillName, out var d) ? d : null;
}
