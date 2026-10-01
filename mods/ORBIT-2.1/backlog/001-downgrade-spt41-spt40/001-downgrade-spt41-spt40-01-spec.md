# 001 — Downgrade SPT 4.1 → 4.0

**Mod:** ORBIT-2.1
**Status:** Em progresso
**Criado:** 2026-09-30

## Visão geral

O ORBIT 2.1.0 foi escrito pelo autor para o SPT 4.1. O servidor do grupo roda SPT 4.0.13 (EFT 0.16.9), onde hoje está instalado o ORBIT 1.2.1. Este item porta as três partes do ORBIT 2.1 — o plugin de cliente, o addon de sincronização para o Fika e o mod de servidor com o painel web — para que carreguem e funcionem no SPT 4.0.13, sem mudar o comportamento que o mod tem no 4.1.

O caminho oficial documentado é o de subida (4.0 → 4.1). Aqui o mesmo material é lido no sentido inverso: o de-para de nomes de classes e os guias de migração de cliente e de servidor.

## Comportamento atual

- O ORBIT 2.1.0 não compila contra o jogo e o servidor do SPT 4.0.13: usa os nomes de classes e de membros do jogo que só existem no 4.1, a interface de mod de servidor do 4.1 e o .NET 10.
- O mod de servidor declara que só aceita SPT 4.1; o servidor 4.0 recusa carregá-lo.
- O jogo no SPT 4.0 roda o ORBIT 1.2.1, que não tem Ghost Mode, painel web, editor de zonas nem presets.

## Comportamento desejado

- As três partes compilam contra a instalação do SPT 4.0.13 apontada pelo arquivo `.spt-path`.
- O servidor 4.0.13 carrega o mod de servidor, serve o painel em `/orbit` e entrega ao cliente a configuração e as zonas.
- O plugin de cliente carrega no jogo 4.0, aplica todos os seus pontos de intervenção no jogo e controla os bots como no 4.1, incluindo o Ghost Mode para bots do ORBIT e para bots nativos e de facções.
- O addon do Fika carrega junto do Fika instalado e sincroniza o áudio dos combates fantasmas e o estado das portas.
- Nenhuma funcionalidade do 2.1 é removida para fazer o port caber; o que não puder ser portado fica registrado como pendência com o motivo.

## Critérios de aceite

- [ ] Compilar os três projetos com `/compile-mod ORBIT-2.1` contra o SPT 4.0.13, com zero erros.
- [ ] Iniciar o servidor SPT 4.0.13 com o mod de servidor instalado e ver no log a linha de carga do ORBIT Server e nenhuma linha de erro; abrir `/orbit` e cada página do painel sem erro.
- [ ] Obter do servidor, pelas rotas que o cliente usa, a configuração e as zonas em JSON válido.
- [ ] Iniciar o jogo com o plugin instalado e ver no log do BepInEx a linha "ORBIT 2.1.0 (SPT 4.0 port) fully loaded" e as três linhas de prontidão ("body guards ready", "scripted medicine guard ready", "scoped resume guard ready"), sem nenhuma linha "failed to enable" nem "unavailable".
- [ ] Jogar uma raid em mapa com PMCs e scavs e observar esquadrões do ORBIT indo a objetivos, saqueando e extraindo; com o Ghost Mode ligado, observar bots distantes dormindo e acordando ao se aproximar.
- [ ] Abrir o painel, mudar uma opção, salvar e ver o valor novo aplicado na raid seguinte.
- [ ] **Fika/multiplayer:** em raid com host (ou headless) e ao menos um cliente, todos com o plugin e o addon na mesma versão: os bots aparecem e se comportam igual para todos, uma porta aberta por bot abre para o cliente, e o cliente ouve os tiros dos combates fantasmas. Sem o addon em algum cliente, o log avisa a incompatibilidade e a raid segue.
- [ ] **Estado entre raids:** raid 1 → saída → raid 2 no mesmo processo do jogo, e também após morte e após fechar o jogo no meio da raid (alt-F4): a raid seguinte inicia com os bots controlados e sem erro repetido do ORBIT no log.

## Corner cases

- [ ] Um ponto de intervenção do ORBIT não encontra seu alvo no jogo 4.0: o mod precisa seguir carregando os demais e o log precisa dizer qual falhou. Nenhum pode falhar em silêncio.
- [ ] Nome de classe do jogo comparado como texto: no 4.0 a mesma classe tem outro nome em tempo de execução. Toda decisão do mod que compara esse texto precisa continuar acertando.
- [ ] Mods opcionais que o ORBIT reconhece (SAIN, mods de facção como UNTAR, RUAF, Black Division, ISB) em versões da linha 4.0, diferentes das que o autor testou no 4.1: ausentes, o ORBIT segue sem eles; presentes, a integração funciona ou se desliga com aviso no log.
- [ ] Servidor sem o mod de servidor instalado, ou fora do ar: o cliente carrega com os valores padrão.
- [ ] ORBIT 1.2.1 ainda instalado junto do 2.1: os dois têm o mesmo identificador de plugin; só um pode ficar instalado.
- [ ] Pasta de instalação com nome diferente do padrão do autor (`ORBIT-2.1` em vez de `ORBIT`), no cliente e no servidor.
- [ ] Headless do Fika: o cliente headless não constrói o manual de preços do jogo; os preços de loot precisam vir do servidor.

## Fora de escopo

- [ ] Mudar comportamento, balanceamento ou padrões do ORBIT.
- [ ] Portar mods de facção ou o SAIN.
- [ ] Publicar o port no Forge (exige permissão do autor original; ver `/prepare-mod-for-publish`).
- [ ] Instalar em produção.

## Referências

- Upstream: <https://github.com/Chazut/ORBIT>, branch `2.1`, commit `8fd7e661`. O commit `00e7ad8` ("Migrate to SPT 4.1.2") é o port 4.0 → 4.1 feito pelo autor.
- Guias oficiais de migração 4.0 → 4.1: [Client_40_to_41.md](../../../../wiki/spt/modding/SPT_41_Modding/Client_40_to_41.md), [Server_40_to_41.md](../../../../wiki/spt/modding/SPT_41_Modding/Server_40_to_41.md), [Class_Name_Mappings.md](../../../../wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md).
- ORBIT 1.2.1, que roda no SPT 4.0: [mods/ORBIT/](../../../ORBIT/).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-30 | Item criado |
