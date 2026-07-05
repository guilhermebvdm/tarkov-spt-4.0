# TRL-DynamicSpawn - Documentação de Arquitetura e Regras de Spawn

Este documento serve como memória descritiva para a criação do mod **TRL-DynamicSpawn**. Ele detalha como o sistema de spawn dinâmico deve funcionar, para garantir que as mecânicas desenhadas não se percam em futuras modificações.

## 1. Visão Geral da Mecânica (Paradigma)
O sistema de spawn nativo do SPT e do MOAR pré-calcula todas as ondas antes da raid começar e deixa os spawns travados em uma "fila" (queue) quando o limite de performance (`MaxBotCap`) é atingido. Isso causa o indesejado efeito de "spawn instantâneo ao matar um bot".

**Objetivo do TRL-DynamicSpawn:** Mudar o "Cérebro" do spawn para o **Cliente**. O cliente avaliará o mapa ao vivo a cada 6 minutos (sem filas e sem acúmulos) e preencherá as vagas vazias pontualmente e suavemente.

## 2. Regras de Tempo (Timers)
- **Atraso Inicial (Warm-up):** O mod deve aguardar **1 minuto (60 segundos)** inteiros após o carregamento da raid antes de injetar qualquer bot. (Isso evita stutters nos primeiros segundos de movimentação do player).
- **Ciclo de Ondas:** Após o atraso inicial, uma nova verificação de onda acontece cravada a cada **6 minutos (360 segundos)**.
- **Períodos de Calmaria:** Entre o minuto de uma onda e o minuto da próxima, **nenhum spawn pode ocorrer**, independentemente de quantos bots o jogador mate. Isso garante respiros de silêncio na raid para loot, cura e estratégia.

## 3. Lógica de Preenchimento e Vagas (Slots)
- No exato minuto da onda (ex: min 7:00), o mod fará a seguinte matemática ao vivo:
  - Ler limite do Host: `MaxBotCap = X` (definido no menu F12).
  - Ler bots vivos atualmente: `AliveBots = Y`.
  - Vagas Disponíveis = `MaxBotCap - AliveBots`.
- Se as vagas forem `<= 0`, a onda é **ignorada** e o mod só tentará de novo no próximo ciclo de 6 minutos.

## 4. Distribuição Demográfica e Hierarquia
Se houver vagas disponíveis, elas devem ser preenchidas seguindo a seguinte hierarquia estrita:
1. **Elites:** Prioridade máxima para Bosses, Cultistas, Raiders, Rogues, Guardas (Followers), Snipers (Marksman), Bloodhounds, Smugglers e outros bots especiais, dependendo das chances de spawn configuradas via Painel Web e limites do mapa.
2. **Divisão Plebeia:** Vagas restantes são divididas entre Família Scav e Família PMC baseando-se no **Preset Ativo** escolhido no Painel Web.
   - Presets controlarão o ratio. Ex: "Equilibrado" (50/50), "Guerra de PMCs" (20/80), "Infestação de Scavs" (80/20) ou "Aleatório" (Sorteia uma proporção diferente a cada onda). Se o número for ímpar, arredonda aleatoriamente.
3. **Divisão Interna de Facção:** 
   - Das vagas de Scav calculadas, divididas **50% para Scav Normal** e **50% para pScav** (Simulação de Player Scav).
   - Das vagas de PMC calculadas, divididas **50% para BEARs** e **50% para USECs**.

## 5. Fila Rápida e Suave (Smooth Spawn)
O ato de injetar 15 bots em 1 único frame destrói o FPS do jogo (micro-stuttering grave). 
- Para evitar isso, os spawns de uma onda não ocorrem de uma vez.
- Eles entram numa **Coroutine (Fila Rápida)** que dá spawn de **1 bot a cada 1 segundo**.
- Se a onda tiver 15 vagas, levará 15 segundos espalhados para colocar todo mundo na raid de forma imperceptível para o FPS.

## 6. Configurações e Painel Web (Servidor)
Todo o controle do mod será feito através de um **Painel de Controle Web** (Razor Pages) hospedado nativamente pelo Servidor SPT.
- O Cliente baixará essas configurações no momento do carregamento da raid.
- **Tradução Segura (Data Binding & Tooltips):** 
  - Todos os botões, sliders e campos do painel Web possuirão **Tooltips descritivas em Inglês** (`<MudTooltip>`).
  - Para evitar bugs ao usar ferramentas como "Google Tradutor" do navegador, o Data Binding dos campos será feito estritamente sobre *Valores Internos (Keys)* em vez do texto de exibição, e campos sensíveis (como nomes de BotZones) receberão a tag HTML `translate="no"`. Isso garante que o usuário possa traduzir a página para o seu idioma natal e salvar configurações sem corromper a comunicação com o C#.
- O Painel permitirá configurar:
  - **Dificuldade (Roleta):** Sliders de porcentagem (ex: Easy 10%, Normal 60%, Hard 25%, Impossible 5%). O Cliente sorteia a dificuldade de cada bot respeitando esses pesos.
  - **Elites por Mapa:** Controle individual de Enable/Disable, Chance de Spawn e BotZones exclusivas para Killa, Tagilla, Goons, Cultistas, Bloodhounds, etc. (Com opção de desativar escoltas).
  - **Timers:** Ajuste do Atraso Inicial (`DelayBeforeFirstWave`) e do Intervalo de Horda (`SecondsBetweenWaves`) por mapa.
  - **Starting PMCs:** Opção de injetar PMCs no início da raid independentes do Timer da Horda, simulando a corrida inicial.

*(Este arquivo deve ser consultado e expandido sempre que formos criar ou modificar scripts vitais em `TRL-DynamicSpawn/Client`)*
