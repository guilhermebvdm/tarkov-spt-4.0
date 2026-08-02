# 032 — Velocidade de download (retoma o 016): medir durante a transferência

**Mod:** Launcher4.0-v2
**Status:** Backlog
**Criado:** 2026-08-02

## Visão geral

O launcher deveria mostrar a **velocidade de download** (ex.: MB/s) enquanto sincroniza os arquivos, mas ela **nunca funcionou de forma útil**: o motor de medição existe e está testado, porém a UI que o exibia foi removida, e — mais importante — a medição é feita **só quando cada arquivo termina** de baixar, não durante a transferência. Num sync com arquivos pequenos, o número pisca e some; num arquivo grande (um bundle de dezenas de MB), a barra fica **parada, sem taxa nenhuma**, e só mostra um número quando o arquivo já acabou — exatamente o inverso do que o jogador quer ver. Este item faz a taxa ser medida **durante** o download e atualizar em cadência fixa, e devolve a exibição à barra de update. É o item irmão do **031** (notificações) — feito em conjunto, coordenando o reset da taxa e a UI.

## Comportamento atual

- **O motor existe e está vivo, a UI foi arrancada.** O `DownloadRateMeter` (média móvel, MB/s com fallback KB/s, formatação PT-BR) está completo e com testes verdes, e a medição está plugada nos dois fluxos de sync via `WithSpeedMeter`. As propriedades `DownloadSpeedText`/`HasDownloadSpeed` existem nos ViewModels. Mas **nenhum `.axaml` faz binding** nelas — os dois `TextBlock` que mostravam a taxa foram removidos ([ProfileView.axaml:216](../../project/SPT.Launcher/Views/ProfileView.axaml#L216) e [ModUpdateView.axaml:29](../../project/SPT.Launcher/Views/ModUpdateView.axaml#L29), comentário "velocidade de download removida a pedido", commit `2f43a158`, um dia após o 016 ser entregue).
- **A medição é por arquivo concluído, não durante.** O downloader (`RequestHandler.DownloadModFile`) bufferiza o arquivo **inteiro** em memória antes de devolver; `WithSpeedMeter` só registra a amostra **depois** que o arquivo terminou. Consequências: (a) arquivos pequenos (configs, DLLs de KB) geram amostras que somem em milissegundos — taxa nunca legível; (b) amostras com tempo ~0 são descartadas por design — arquivo instantâneo não gera taxa; (c) um bundle grande fica **sem taxa durante todo o download** e só mostra um número no fim; (d) ao terminar o run, a taxa é zerada e o último valor some.
- Isso foi assumido conscientemente no as-built do 016 (granularidade por-arquivo, intra-arquivo descartado por custo) — e é essa decisão que este item revisita.

## Comportamento desejado

**1. Medir durante a transferência (intra-arquivo).** A taxa é alimentada com os bytes recebidos **ao longo** do download de cada arquivo (leitura em blocos), não só no fim. Um arquivo grande passa a ter taxa **enquanto** está baixando.

**2. Atualizar em cadência fixa.** O número na tela atualiza em intervalos regulares (~meio segundo) mesmo quando há um único arquivo grande em voo — o jogador vê a taxa se mexendo, não um valor congelado.

**3. Exibir a taxa na barra de update.** A velocidade volta a aparecer na barra de update das duas telas de sync (tela logada e tela de setup), no mesmo lugar/estilo previsto no 016.

**4. Sumir quando não há transferência.** Quando não há download acontecendo — ações que não baixam (mover/remover), fim do run, ou nada a fazer — a taxa some/zera, sem deixar um número velho pendurado.

## Critérios de aceite

- [ ] **CA-032.1 (taxa durante um download grande):** ao baixar um arquivo grande o suficiente (um bundle de vários MB), a velocidade em MB/s **aparece e se atualiza** durante a transferência — não só quando o arquivo termina. Verificável in-game contra o servidor real com um bundle grande.
- [ ] **CA-032.2 (cadência fixa):** com um único arquivo grande baixando, o número na tela muda em intervalos regulares (~500 ms), não fica congelado até o fim.
- [ ] **CA-032.3 (some sem download):** num run sem downloads (só mover/remover/preservar, ou nada a fazer), a taxa não aparece (ou some); ao terminar qualquer run, ela zera.
- [ ] **CA-032.4 (exibição):** a taxa é exibida na barra de update das duas telas de sync (`ProfileView` e `ModUpdateView`), com a fonte monoespaçada prevista no 016.
- [ ] **CA-032.5 (motor reaproveitado):** a formatação e a média do `DownloadRateMeter` (já testadas) são reaproveitadas — só a **fonte das amostras** muda (de por-arquivo para intra-arquivo).
- [ ] **Fika/multiplayer:** `N/A` — é a barra de progresso da UI do launcher (pré-jogo); não roda no cliente durante o raid nem troca pacote. A velocidade é medida localmente sobre o download HTTP do sync.
- [ ] **Estado entre raids:** `N/A` — fluxo de login/pré-jogo do launcher; nenhum estado de raid é criado ou alterado.

## Corner cases

- [ ] **CC-1 (arquivos pequenos):** numa sequência de arquivos de poucos KB, a taxa não deve "piscar" ruído. Resolução: a **média móvel** do `DownloadRateMeter` (já existente) + o **ticker de ~500 ms** (mecanismo 2) suavizam — a UI lê a média na cadência do ticker, não a cada arquivo. Sem piso de tamanho por ora; revisitar só se ainda piscar no gate in-game.
- [ ] **CC-2 (run sem download):** um plano só com mover/remover/preservar não gera taxa nenhuma (não há bytes de download) — a barra mostra o progresso das ações, sem MB/s.
- [ ] **CC-3 (cancelamento no meio de um arquivo grande):** cancelar durante um download interrompe a leitura em blocos e a taxa para/zera; o arquivo parcial é descartado (o motor de sync já trata isso — a taxa só não pode continuar contando).
- [ ] **CC-4 (Base compartilhada):** a mudança no caminho de download vive em `SPT.Launcher.Base` (código compartilhado com o upstream) — a leitura em blocos não pode quebrar o contrato do downloader existente (mesmos bytes entregues, mesma escrita atômica no motor).
- [ ] **CC-5 (reset coordenado com o 031):** o reset da taxa no início do run é o mesmo ponto único do item 031 (mecanismo 4) — os dois itens compartilham esse reset; não duplicar.
- [ ] **CC-6 (ticker vivo sem transferência):** o ticker de atualização da UI não pode continuar rodando/consumindo depois que o run termina — deve parar junto com o fim do sync.

## Fora de escopo

- [ ] **ETA / bytes restantes / barra por bytes** — o pedido é **só a velocidade**. ETA e "faltam X MB" ficam para um item próprio se desejado.
- [ ] O **texto e o fechamento** das notificações de sync — é o item irmão **031**.
- [ ] Reescrever o `DownloadRateMeter` (a média/formatação já passam nos testes) — só troca a fonte das amostras.

## Referências

- [032 — Kickoff](./032-velocidade-download-nunca-funcionou-00-kickoff.md) (diagnóstico: motor vivo, UI arrancada, medição por-arquivo)
- [Item 016 — Velocidade de download](../016-velocidade-download-verificacao/) (implementação original; **este item o substitui na prática** — ao entregar, marcar o 016 como superado)
- [Item 031 — Notificações de sync](../031-notificacao-sync-mensagem-final/) (irmão; reset e UI coordenados)

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Spec funcional criada via `/create-spec`, a partir do kickoff. Origem: relato do usuário ("a velocidade nunca funcionou; não vi acontecendo"). Decisão de desenho: medir intra-arquivo (streaming) em vez de por-arquivo concluído. Feito em conjunto com o 031 (notificações), coordenando reset e UI. Substitui o 016 na prática. |
