import re

with open('Home.razor', 'r', encoding='utf-8') as f:
    content = f.read()

# I will replace the Portuguese paragraphs with English.
replacements = [
    ("A evolu.+? do spawn no SPTarkov: Unindo o melhor do MOAR com o controle do ABPS.", "The evolution of spawn in SPTarkov: Combining the best of MOAR with the control of ABPS."),
    ("O <b>TRL Dynamic Spawn</b> n.+?o .+? apenas um simples gerenciador de bots. Ele reconstr.+?i completamente a forma como o jogo decide <b>quando</b>, <b>onde</b> e <b>quem</b> vai spawnar na sua raid.", "<b>TRL Dynamic Spawn</b> is not just a simple bot manager. It completely rebuilds how the game decides <b>when</b>, <b>where</b>, and <b>who</b> will spawn in your raid."),
    ("Ao inv.+?s de deixar o SPTarkov jogar todos os bots de uma vez no in.+?cio da partida, o mod cria <b>ondas din.+?micas \(Hordas\)</b>, que avaliam a quantidade m.+?xima de bots permitidos no mapa \(BotCap\) e completam as vagas dispon.+?veis periodicamente.", "Instead of letting SPTarkov dump all bots at once at the start of the match, the mod creates <b>dynamic waves (Hordes)</b>, which evaluate the maximum amount of bots allowed on the map (BotCap) and fill the available slots periodically."),
    ("<b>A L.+?gica do MOAR:</b> Herda o conceito de <i>ondas cont.+?nuas \(Waves\)</i> e o sistema de Custom Spawns. O mod decide a distribui.+?o das fac.+?es \(Ex: Guerra de PMCs 80/20\) e continua enviando refor.+?os conforme os bots morrem.", "<b>The Logic of MOAR:</b> Inherits the concept of <i>continuous waves</i> and the Custom Spawns system. The mod decides the faction distribution (e.g. PMC War 80/20) and keeps sending reinforcements as bots die."),
    ("<b>O Controle do ABPS \(Advanced Bot Placement System\):</b> O TRLDynamicSpawn puxou a incr.+?vel interface e o n.+?vel absurdo de controle sobre <b>Bosses e Elites</b>. Voc.+? pode configurar as chances de spawn e, principalmente, <i>em quais zonas espec.+?ficas</i> cada Boss tem permiss.+?o para aparecer em cada mapa.", "<b>The Control of ABPS (Advanced Bot Placement System):</b> TRLDynamicSpawn brings the incredible interface and absurd level of control over <b>Bosses and Elites</b>. You can configure spawn chances and, most importantly, <i>in exactly which specific zones</i> each Boss is allowed to appear on each map."),
    ("Voc.+? configura na p.+?gina Global o tempo para a primeira onda \(Delay Before First Wave\) e o intervalo entre as pr.+?ximas ondas \(Seconds Between Waves\).", "You configure in the Global page the time for the first wave (Delay Before First Wave) and the interval between subsequent waves (Seconds Between Waves)."),
    ("A cada ciclo, o mod verifica quantos bots est.+?o vivos. Se o mapa suporta 20 bots e s.+? tem 10 vivos, a pr.+?xima onda tentar.+? gerar 10 novos bots para encher o mapa novamente.", "At each cycle, the mod checks how many bots are alive. If the map supports 20 bots and only has 10 alive, the next wave will try to generate 10 new bots to fill the map again."),
    ("Bosses, Rogues, Raiders e Cultistas <b>sempre</b> spawnam primeiro, assim que o mapa carregar. Se o Reshala tem 100% de chance de spawnar nos Dormit.+?rios, ele e os guardas j.+? v.+?o reservar a vaga deles no BotCap imediatamente.", "Bosses, Rogues, Raiders, and Cultists <b>always</b> spawn first, as soon as the map loads. If Reshala has a 100% chance to spawn at Dorms, he and his guards will reserve their spots in the BotCap immediately."),
    ("Guia de Configura.+?o \(Menu Lateral\)", "Configuration Guide (Sidebar Menu)"),
    ("Escolha os presets de propor.+?o PMC/Scav \(Ex: 50/50, 80/20\) e os temporizadores das ondas de Horda.", "Choose the PMC/Scav ratio presets (e.g. 50/50, 80/20) and the Horde wave timers."),
    ("Injeta uma lista de <b>novos pontos de spawn customizados</b> no mapa. Ou seja, ele adiciona novas possibilidades e lugares \(que n.+?o existem no jogo Vanilla ou no ABPS\) para PMCs, Scavs, Snipers e at.+? o seu Player nascerem, tornando o in.+?cio da raid e a movimenta.+?o muito mais imprevis.+?veis.", "Injects a list of <b>new custom spawn points</b> into the map. That is, it adds new possibilities and places (that do not exist in Vanilla or ABPS) for PMCs, Scavs, Snipers, and even your Player to spawn, making the start of the raid and movement much more unpredictable."),
    ("Nota Avan.+?ada: .+? poss.+?vel criar e adicionar novas .+?reas e pontos de spawn mexendo diretamente no arquivo JSON do mod, por.+?m essa .+? uma configura.+?o avan.+?ada. Recomendamos fortemente n.+?o alterar os arquivos JSON manualmente a menos que voc.+? saiba exatamente o que est.+? fazendo!", "Advanced Note: It is possible to create and add new areas and spawn points by editing the mod's JSON file directly, but this is an advanced configuration. We strongly recommend not altering JSON files manually unless you know exactly what you are doing!"),
    ("A joia da coroa. Clique no nome de um Boss \(Ex: Tagilla, Reshala\) e altere a chance dele aparecer \(%\) e <b>exatamente</b> as zonas em que ele tem permiss.+?o de spawnar \(Ex: Apenas Gas Station, ou Apenas Dormit.+?rios\).", "The crown jewel. Click on a Boss name (e.g. Tagilla, Reshala) and change their spawn chance (%) and <b>exactly</b> the zones they are allowed to spawn in (e.g. Gas Station Only, or Dorms Only)."),
    ("Preenchimento Inteligente", "Smart Filling"),
    ("Prioridade Elite", "Elite Priority")
]

for old, new_text in replacements:
    content = re.sub(old, new_text, content)

with open('Home.razor', 'w', encoding='utf-8') as f:
    f.write(content)

print("Home updated")

with open('CustomSpawns.razor', 'r', encoding='utf-8') as f:
    content = f.read()

replacements = [
    ("Configure locais \(zonas\) adicionais de spawn personalizados nos mapas, permitindo que os bots surjam em .+?reas onde n.+?o surgiriam normalmente pelo jogo base.", "Configure additional custom spawn locations (zones) on maps, allowing bots to spawn in areas where they normally wouldn't in the base game."),
    ("Substitui o spawn aleat.+?rio por pontos espec.+?ficos de PMC.", "Replaces random spawn with specific PMC spawn points."),
    ("Substitui o spawn aleat.+?rio por pontos espec.+?ficos de Scav.", "Replaces random spawn with specific Scav spawn points."),
    ("Substitui o spawn aleat.+?rio por pontos espec.+?ficos de Sniper.", "Replaces random spawn with specific Sniper spawn points."),
    ("Substitui o spawn inicial aleat.+?rio por pontos espec.+?ficos de Player.", "Replaces random initial spawn with specific Player spawn points.")
]

for old, new_text in replacements:
    content = re.sub(old, new_text, content)

with open('CustomSpawns.razor', 'w', encoding='utf-8') as f:
    f.write(content)

print("CustomSpawns updated")

