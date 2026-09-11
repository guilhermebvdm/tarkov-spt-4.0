# 002 — Gerenciador Ciclo Natural Estações

**Mod:** TRL-WeatherSync
**Status:** Backlog
**Criado:** 2026-09-10

## Visão geral

Controla e automatiza a progressão da estação do ano ativa (Primavera Inicial → Primavera → Verão → Outono Dourado → Outono Tardio → Inverno) num ciclo real configurável — por padrão, 1 semana real por estação macro, totalizando 4 semanas (28 dias) para o ciclo anual completo. Além de trocar a estação visual (folhagem, neve acumulada, temperatura, iluminação), este item também ajusta a probabilidade de cada tipo de clima (sol/chuva/nublado) por estação, dando mais realismo — inverno chove/neva bem mais que o resto do ano, espelhando a proporção observada no próprio jogo (aproximadamente 2:1 a mais de chuva/neve no inverno vs. as demais estações).

## Comportamento atual

Sem este item, a estação ativa do jogo segue o comportamento nativo do EFT/SPT: decidida pela data real do calendário do servidor (ex.: verão de 23/06 a 15/10, inverno de 13/12 a 09/01), sem nenhum controle ou configuração do usuário, e sem garantia de que jogadores diferentes numa mesma raid FIKA estejam necessariamente vendo a mesma estação (cada instalação consulta sua própria data/hora local, dependendo de como o servidor de cada um resolve isso). O item 001 (Sincronização Contínua de Clima em Raid) já cobre a sincronização de curto prazo de chuva/vento/neblina/nuvem/temperatura dentro de uma raid — mas não decide qual é a estação ativa nem controla havoc de longo prazo.

## Comportamento desejado

A duração de cada estação macro (em dias, padrão 7) e o modo de progressão (segue o tempo real de forma contínua, ou fica travado numa estação fixa) são definidos por **um arquivo de configuração `.json` que o servidor SPT lê e entrega aos clients** — não são configuráveis individualmente por cada jogador no painel F12 do client. Os pesos de probabilidade de cada tipo de clima (sol/chuva/neve) por estação ficam **no mesmo arquivo**, também fora do F12.

**Precisão sobre "servidor" (esclarecido pelo usuário):** no FIKA, cada jogador roda sua própria instância local do servidor SPT (não existe um servidor único compartilhado pela raid — confirmado na investigação do item 001, `RaidWeatherService` é singleton por processo). Por isso, a consistência entre jogadores (critério Fika/multiplayer abaixo) depende de **todas as instalações terem o mesmo `.json`** — o cálculo da estação ativa precisa ser uma fórmula determinística por tempo real (ex.: timestamp de referência fixo + duração, ambos vindos desse arquivo), não um estado que cada servidor local decide sozinho. Isso é o mesmo tipo de exigência que o item 001 já tem pro mod em si ("todo mundo precisa ter o mod instalado") — aqui se estende a "todo mundo precisa ter a mesma config de estação". Se alguém editar esse `.json` localmente de forma diferente do resto do grupo, a estação diverge entre jogadores (ver corner case correspondente).

A estação avança automaticamente com o passar do tempo real, incluindo as sub-fases nativas do jogo dentro de cada macro-estação. A probabilidade de cada tipo de clima (sol/chuva/nublado) varia por estação, com o inverno sendo perceptivelmente mais chuvoso/nevado que as demais.

### Ciclo completo (ordem de progressão)

| # | Macro-estação | Sub-fase(s) nativa(s) | Duração padrão |
|---|---|---|---|
| 1 | Primavera | Primavera Inicial (degelo) → Primavera Plena | 7 dias |
| 2 | **Verão** | Verão Pleno | 7 dias |
| 3 | Outono | Outono Dourado → Outono Tardio | 7 dias |
| 4 | Inverno | Inverno Nevado | 7 dias |

Ciclo anual completo: 4 semanas (28 dias). "Tempestade" (chuva forte com trovão) não é uma fase deste ciclo — é um evento de curto prazo já coberto pelo item 001 (`CR-01-02`), disparado independente da estação.

## Critérios de aceite

- [ ] A estação ativa avança automaticamente com o tempo real decorrido, respeitando a duração configurada por estação (padrão 7 dias por estação macro, 28 dias para o ciclo anual completo).
- [ ] Cada macro-estação aplica corretamente as sub-fases nativas do jogo que lhe correspondem (Primavera Inicial → Primavera Plena; Outono Dourado → Outono Tardio; Verão e Inverno como fases únicas).
- [ ] A duração (em dias) de cada estação e o modo de progressão (calendário contínuo vs. estação fixa) são definidos pelo arquivo `.json` de configuração do servidor SPT — não existe `ConfigEntry` de F12 no client para isso; todo client conectado lê e respeita o mesmo arquivo.
- [ ] Os pesos de probabilidade de clima (sol/chuva/neve) por estação também vêm do mesmo `.json` do servidor SPT, não do F12 — a chuva/neve observada ao longo de várias raids no inverno é perceptivelmente maior que a observada nas demais estações (proporção configurável no `.json`, com um padrão inspirado na proporção nativa do jogo, ~2:1).
- [ ] **Fika/multiplayer:** todos os jogadores de uma mesma raid FIKA (Host, Clientes, e raid hospedada por servidor Headless) observam a mesma estação ativa ao mesmo tempo — nenhum jogador vê Inverno enquanto outro vê Verão na mesma raid, sob condições normais. (claro graças à leitura via server)
- [ ] **Estado entre raids:** a estação ativa não reseta a cada raid — é uma função contínua do tempo real decorrido (ou da configuração fixa, se ativa); raid1 → exit → raid2 mostra a mesma estação, a menos que o tempo configurado para a duração da estação tenha se esgotado nesse meio-tempo.

## Corner cases

- [ ] Jogador altera manualmente o relógio/data do sistema operacional (adianta ou atrasa) — o que acontece com a estação calculada? Precisa de uma fonte de tempo resistente a isso: a fórmula deve usar um timestamp de referência fixo salvo no `.json`, não o relógio absoluto do sistema em si (o mesmo padrão que o `ROADMAP.md` §3 já propõe com `referenceEpochUtc`).
- [ ] Uma raid longa atravessa exatamente o momento de virada de uma estação (ex.: começa 1 minuto antes da troca) — a estação muda no meio da raid ou só se aplica a partir da próxima raid? Resposta: Só aplica na próxima raid.
- [ ] Instalações da mesma raid FIKA (Host, Clientes, processo Headless) têm arquivos `.json` de estação DIFERENTES entre si (ex.: alguém editou manualmente) — qual prevalece, e o que acontece com quem diverge? A consistência (critério Fika/multiplayer) só é garantida se todas as instalações tiverem o **mesmo** `.json` — isso vira um requisito equivalente ao "mod instalado em todos" do item 001 (documentar em `README.md`), não algo resolvido automaticamente pelo código. Comportamento em caso de divergência real (json diferente entre peers) ainda não definido — provavelmente cada jogador só aplica sua própria leitura local, sem detecção de divergência (nenhum pacote de rede troca essa informação, ao contrário do item 001).
- [ ] Primeira execução do mod, sem nenhum estado/configuração prévia salva — de que ponto do ciclo a contagem começa? Resposta: Outono
- [ ] Mapa em que o próprio FIKA hoje pula a geração de clima customizado (ex.: Laboratory) — a estação/sub-fase deve ser aplicada mesmo assim, ou esse mapa fica de fora, como já acontece com o clima de curto prazo do item 001? <!-- review: não é resposta fechada ainda — usuário pediu pra investigar como o vanilla trata isso por mapa e replicar o mesmo padrão. Fica como tarefa de pesquisa para o /create-technical-spec, não uma decisão pré-definida. -->
- [ ] Modo "estação fixa" ativado durante uma raid em andamento — a mudança de configuração se aplica imediatamente ou só na próxima raid? Só na proxima raid, mas isso vai ser definido via server e não client.
- [ ] Modo "estação fixa" travado, por exemplo, em Inverno — a probabilidade de clima (chuva/neve) usada também trava nos pesos do Inverno, ou continua variando como se fosse a estação real do calendário?
- [ ] Interação com o item 001: o peso de chuva/nuvem mais alto no Inverno (este item) aumenta indiretamente a frequência de tempestade sincronizada (`RollForStorm`, que sorteia contra `LightningThunderProbability`, derivada de nebulosidade) — esse efeito em cascata é esperado/aceitável, ou a frequência de tempestade do item 001 precisa ser recalibrada quando este item entrar em produção?
- [ ] A estação é algo computado uma vez no início da raid (parecido com o lifecycle do item 001 — sessão criada/destruída por raid) ou é uma leitura consultada a qualquer momento, inclusive fora de raid (menu, hideout)? Afeta se precisa de hooks de início/fim de raid iguais aos do item 001.

## Fora de escopo

- [ ] Sincronização de curto prazo de chuva/vento/neblina/nuvem/temperatura e o evento de tempestade dentro de uma raid — já coberto pelo item 001 (Sincronização Contínua de Clima em Raid). Este item só decide qual é a estação/sub-fase ativa e os pesos de probabilidade de clima por estação.

## Referências

- [ROADMAP.md — §2 (Ciclo Natural e Mapeamento de Fases) e §3 (Controle de Período das Estações)](../../ROADMAP.md)
- [docs/investigacao-fika-eft-2026-09-10.md — seção I (dados reais de `weather.json`: datas de calendário e pesos de clima por estação) e seção H (confirmação da nevasca de inverno real, `RainController.ERainControllerStatus.WinterStorm`)](../../docs/investigacao-fika-eft-2026-09-10.md)
- [001-sincronizacao-continua-clima-raid/ — item que cobre a sincronização de curto prazo de clima dentro da raid](../001-sincronizacao-continua-clima-raid/)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Item criado via `/add-backlog-item` |
| 2026-09-10 | Revisão `/review-spec` — 2 gaps marcados para decisão (definição precisa de "regra do servidor" e se os pesos de clima seguem a mesma regra) + 4 corner cases adicionados (peso de clima em estação fixa, interação com tempestade do item 001, lifecycle de raid, preenchida "Fora de escopo"). |
| 2026-09-10 | Gaps resolvidos pelo usuário: "servidor" = arquivo `.json` de configuração que o servidor SPT lê e entrega aos clients (pesos de clima incluídos no mesmo arquivo); nenhum dos dois é F12. Precisão adicionada em "Comportamento desejado" sobre consistência entre instalações FIKA depender de todas terem o mesmo `.json` (cada jogador roda seu próprio servidor SPT local, não há servidor único compartilhado pela raid). Corner case de divergência de config reescrito para refletir isso. 0 gaps pendentes. |
