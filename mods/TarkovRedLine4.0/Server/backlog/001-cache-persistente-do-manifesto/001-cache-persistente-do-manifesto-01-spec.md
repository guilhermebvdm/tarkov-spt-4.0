# 001 — Cache persistente do manifesto (fim da espera "preparing the list")

**Mod:** TarkovRedLine.Server
**Status:** Backlog
**Criado:** 2026-08-02

## Visão geral

Hoje o manifesto de sincronização (a lista de arquivos + o hash que o launcher usa pra saber o que baixar) é montado em memória e **gerado sob demanda**: some a cada reinício do servidor, e o primeiro jogador que loga depois de um boot "paga" a espera enquanto o servidor varre e hasheia todos os arquivos — é a mensagem *"Server is preparing the list. Retrying in 30s..."* no launcher. Este item faz o servidor **guardar o manifesto pronto em disco** e **carregá-lo no boot** quando o conteúdo do `mods_repo` não mudou, eliminando essa espera em todo restart que não altera nada. Só regera quando o conteúdo realmente muda (detectado por uma impressão leve do `mods_repo`), ou quando invalidado explicitamente.

## Comportamento atual

- **Cache só em memória, geração preguiçosa.** `_manifestHash`/`_manifestCache` são estáticos ([ModUpdater.cs:17-18](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L17)). A geração (`GenerateManifestAsync`) escaneia todo o `mods_repo` e calcula o MD5 de cada arquivo. Ela só é disparada quando um pedido encontra o cache vazio; enquanto não termina, os endpoints `manifest-hash`/`manifest` respondem **503 "Manifesto ainda sendo gerado"** ([ModUpdater.cs:134-153](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L134)).
- **O 503 dispara a espera no launcher.** O launcher tenta 5× (3s cada) e depois entra num countdown de 30s antes de repetir todo o ciclo — a mensagem que o jogador vê.
- **Some a cada restart.** Como o cache é em memória, todo reinício do servidor volta ao estado frio. E o `AutoSync-Cache.ps1` roda o servidor em **modo watcher com auto-restart** — então crash, reinício manual, ou o loop do watcher deixam o próximo jogador com a espera **mesmo sem nada ter mudado**.
- **`/refresh` já existe** ([ModUpdater.cs:198-203](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L198)) — zera o cache em memória e regera. É a única invalidação hoje.
- **O AutoSync não toca o manifesto** — ele cuida só do cache de bundles 3D ([AutoSync-Cache.ps1:19-20](../../AutoSync-Cache.ps1#L19)).

## Comportamento desejado

**1. Persistir o manifesto pronto em disco.** Ao terminar de gerar (por qualquer caminho — boot, lazy, ou `/refresh`), o servidor grava em disco: o manifesto completo, o hash, e uma **impressão leve** do `mods_repo` — uma assinatura barata que muda quando o conteúdo muda, calculada **sem reler o conteúdo dos arquivos**: contagem de arquivos + soma dos tamanhos + data de modificação mais recente + um **resumo dos caminhos relativos** (a lista ordenada de nomes, condensada). Os nomes entram porque renomear/adicionar/remover um arquivo pode não mexer no tamanho total nem na data (renomear não altera o `LastWriteTime` do próprio arquivo) mas muda o manifesto — sem os nomes, a impressão não pegaria isso (ver CC-8). A escrita é atômica (não deixa arquivo pela metade se o servidor cair no meio).

**2. Carregar do disco no boot, com validação automática.** No início do servidor (proativamente, sem esperar o primeiro pedido), ele:
- Recalcula a impressão leve do `mods_repo` (milissegundos) e compara com a salva.
- **Bate** → carrega o manifesto/hash do disco e fica pronto na hora. O primeiro jogador **não** vê "preparing the list".
- **Não bate**, ou arquivo salvo ausente/ilegível/de versão incompatível → **regera** do zero e regrava (comportamento atual, mas só quando realmente necessário).

Em qualquer dos dois caminhos, o servidor **loga qual decisão tomou** (carregou do disco, ou regerou e por quê — impressão diferente, arquivo ausente, versão incompatível), pra facilitar o diagnóstico e a verificação dos critérios.

**3. Invalidação explícita continua funcionando.** `/refresh` segue zerando e regerando — e agora também **regrava** o arquivo persistido. O AutoSync pode chamá-lo ao publicar, mas não é obrigatório: a validação do boot já detecta a mudança sozinha.

**4. Separação homolog/prod.** O arquivo persistido usa o mesmo sufixo de estado (`StateSuffix`) dos outros arquivos do mod, pra os builds de homolog e produção não compartilharem o mesmo cache.

Efeito: em todo boot/restart onde o conteúdo do `mods_repo` **não** mudou, o manifesto já nasce quente → a espera de 30s desaparece. A espera só pode aparecer na **primeira** vez após uma mudança real de conteúdo (enquanto a regeração roda), não em todo restart.

## Critérios de aceite

- [ ] **CA-001.1 (boot sem mudança = sem espera):** com o `mods_repo` inalterado, reiniciar o servidor **não** regera o manifesto — ele é carregado do disco. Verificável: subir o servidor duas vezes seguidas sem tocar em nada e confirmar no log que a 2ª vez carregou do disco (não regerou), e que o launcher não mostra "preparing the list".
- [ ] **CA-001.2 (mudança de conteúdo é detectada):** alterar/adicionar/remover um arquivo no `mods_repo` e reiniciar → o servidor detecta a impressão diferente, regera, regrava, e o **hash muda** — os jogadores recebem a atualização no próximo sync.
- [ ] **CA-001.3 (arquivo salvo ausente/corrompido não quebra):** apagar ou corromper o arquivo persistido e subir o servidor → ele regera do zero, sem erro/crash, e grava um novo.
- [ ] **CA-001.4 (`/refresh` regrava):** chamar `/refresh` zera o cache, regera e **atualiza o arquivo em disco** — o próximo boot já carrega a versão nova sem regerar.
- [ ] **CA-001.5 (download intacto):** depois de carregar o manifesto do disco (sem regerar), o endpoint `download` continua servindo todos os arquivos do manifesto corretamente — o mapeamento arquivo→caminho físico é reconstruído/válido.
- [ ] **CA-001.6 (homolog e prod não colidem):** os builds de homolog e produção usam arquivos persistidos separados; regerar/invalidar um não afeta o outro.
- [ ] **Fika/multiplayer:** `N/A direto` — é infraestrutura do servidor, não roda no cliente. Mas a persistência **não altera o conteúdo** do manifesto, só o momento em que ele fica pronto: o hash servido é idêntico ao que a geração sob demanda produziria, então todos os clientes continuam sincronizando exatamente o mesmo conjunto de arquivos (nenhum risco de divergência host↔cliente introduzido).
- [ ] **Estado entre raids:** `N/A` — roda no processo do servidor (boot/HTTP), fora de qualquer raid; nenhum estado de raid é criado ou alterado.

## Corner cases

- [ ] **CC-1 (impressão leve não pega uma edição "invisível"):** editar um arquivo mantendo **exatamente** o mesmo tamanho **e** a mesma data de modificação não seria detectado pela impressão leve. É raríssimo na prática (editar/copiar sempre atualiza o mtime; o AutoSync usa robocopy que preserva/atualiza os tempos), e o `/refresh` manual resolve. Documentar como limite conhecido; não justifica reler todo o conteúdo no boot (o que reintroduziria o custo que este item quer eliminar).
- [ ] **CC-2 (mudança durante a geração):** o `mods_repo` muda enquanto a regeração roda. Definir: a impressão salva deve refletir o estado **lido** na geração (capturada no início/consistente com os arquivos hasheados), pra não gravar uma impressão que já nasce "batendo" com um conteúdo diferente do hasheado.
- [ ] **CC-3 (concorrência boot × pedido):** a carga do disco no boot e um pedido `manifest`/`refresh` concorrente não podem gerar duas vezes nem servir um cache meio-montado — reusar o gate atômico existente (`_manifestGenerating`).
- [ ] **CC-4 (escrita interrompida):** o servidor cai no meio da gravação do arquivo persistido → o boot seguinte não pode carregar um arquivo truncado como válido (escrita atômica: grava em temp e renomeia; e/ou valida a integridade ao carregar).
- [ ] **CC-5 (versão do formato):** um arquivo persistido de uma versão **anterior** do mod (formato diferente) não pode ser carregado como se fosse válido — marcar um número de versão do formato e regerar se não bater.
- [ ] **CC-6 (primeiro player durante a regeração pós-mudança):** logo após uma mudança real de conteúdo, se um jogador logar **durante** a regeração, ele ainda vê o 503/espera uma vez. Aceito — é o único caso restante, e é raro (só após publicação). Não é regressão (hoje isso acontece em todo boot).
- [ ] **CC-7 (permissão/disco cheio na gravação):** falha ao gravar o arquivo persistido não pode derrubar a geração nem o servidor — o manifesto em memória continua servindo; a persistência é best-effort com log.
- [ ] **CC-8 (rename com mesmo tamanho e data):** renomear `Foo.dll` → `Bar.dll` não muda a contagem, o tamanho total, nem o `LastWriteTime` do arquivo — mas muda o manifesto (o `path`). A impressão leve **tem** que incluir o resumo dos **caminhos** pra detectar isso; sem ele, o servidor serviria o manifesto antigo apontando pra um nome que não existe mais — exatamente o "sync fantasma" que este item quer evitar. (Alternativa equivalente aceitável: incluir o `LastWriteTime` dos **diretórios** na impressão, já que renomear um arquivo atualiza o mtime da pasta-pai — decidir na spec técnica.)
- [ ] **CC-9 (o hash servido do disco = o hash original):** o hash é o MD5 do manifesto serializado. Ao carregar do disco, o servidor deve servir o **hash gravado junto** na geração, **não** recomputá-lo a partir do JSON recarregado — se a re-serialização mudar qualquer byte (ordem de chaves, espaçamento), o MD5 divergiria do que os clientes já têm no baseline e dispararia um re-sync desnecessário em todo mundo. Persistir e servir o hash original fecha isso.
- [ ] **CC-10 (mods_repo ausente/vazio no boot):** se o `mods_repo` não existir ou estiver vazio, o boot não pode crashar — gera/serve um manifesto vazio (comportamento atual) e persiste uma impressão de "vazio", que passa a validar normalmente nos boots seguintes.

## Fora de escopo

- [ ] Mudar a estratégia de retry/countdown do **launcher** (as 5×3s + 30s) — este item ataca a causa (servidor), não o sintoma; ajustar os tempos do launcher é outro item, se ainda fizer falta.
- [ ] Geração **incremental** (re-hashear só os arquivos que mudaram em vez do `mods_repo` inteiro) — otimização separada; aqui a regeração continua completa, só deixa de acontecer à toa.
- [ ] Fazer o AutoSync chamar `/refresh` automaticamente ao publicar — opcional, já que a validação do boot cobre; pode virar um item próprio no AutoSync se desejado.

## Referências

- [ModUpdater.cs](../../TarkovRedLine.Server/Controllers/ModUpdater.cs) (geração, cache em memória, endpoints, `/refresh`)
- [AutoSync-Cache.ps1](../../AutoSync-Cache.ps1) (watcher com auto-restart; hoje não toca o manifesto)
- Memória `reference_launcher_manifest_stale_phantom_sync` (o "sync fantasma" de manifesto stale — a validação automática deste item reduz esse risco)

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Item criado via `/create-spec`. Decisão do usuário travada: **validação automática no boot** (impressão leve do `mods_repo`), em vez de depender só do AutoSync para invalidar. Origem: investigação da mensagem "preparing the list" de 30s — cache em memória + geração lazy + watcher com auto-restart. |
| 2026-08-02 | Revisão `/review-spec` — 1 gap + 3 corner cases. A impressão leve ganhou o **resumo dos caminhos** (senão um rename com mesmo tamanho/data não seria detectado — CC-8). Adicionados CC-9 (servir o hash original, não recomputar da re-serialização) e CC-10 (mods_repo vazio/ausente no boot). Reforço: o servidor loga qual caminho tomou. Confirmado: AutoSync fora de escopo (decisão do usuário — só persistir no servidor). |
