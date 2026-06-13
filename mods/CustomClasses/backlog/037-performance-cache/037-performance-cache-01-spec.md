# 037 — Performance: cache de validação + índices do catálogo

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-11

## Visão geral

O editor web de classes está lento ao navegar entre as vistas (lista → detalhe → edição). A causa é que cada navegação revalida **todas** as classes do zero — uma operação pesada que reconstrói o loadout completo de cada uma — e o catálogo de itens é varrido inteiro a cada busca ou render de aba. Este item torna a navegação e a busca instantâneas com cache e índices, sem alterar nenhum comportamento visível ou resultado de validação, e exige **medição antes/depois** como evidência.

## Comportamento atual

- Abrir ou trocar para **qualquer** vista do editor (lista, detalhe, edição) dispara uma validação completa ("dry-run") de cada uma das 11 classes — reconstrói o loadout, outfit e hideout de cada classe só para listar/exibir. Como o framework renderiza cada página duas vezes (pré-render + circuito interativo), são ~22 validações pesadas por navegação.
- A **busca de item** percorre o catálogo inteiro do banco resolvendo o nome localizado item a item a cada tecla/consulta; a **aba de outfit** revarre toda a customização a cada render (e monta vários seletores); a árvore de categorias é recalculada a cada chamada.
- Na **edição**, cada interação (ex.: alterar a contagem de um item) recalcula o custo do loadout duas vezes (total e só-stash) e a capacidade do stash, inclusive durante a digitação — travando o campo em loadouts grandes.
- Não há instrumentação de tempo: a lentidão é percebida mas não medida.

## Comportamento desejado

- Listar/exibir classes usa um **cache por arquivo**, invalidado quando o arquivo muda (data de modificação) ou quando o próprio editor salva/cria/duplica/deleta. Com o cache "quente", navegar entre vistas **não dispara nenhuma revalidação**.
- A **busca de item** e a montagem da **aba de outfit** operam sobre **índices pré-computados** uma única vez, virando varredura de uma lista compacta em memória em vez de scan do banco a cada interação.

> **Premissa (revisão 2026-06-12) — imutabilidade do banco:** o código atual contradiz a spec. O comentário em `CatalogService.cs:231` (`BuildHandbookIndex`) diz literalmente *"Built per call — DB is live (mods)"*, enquanto a estratégia de índice assume DB imutável. Decisão autônoma: **tratar a DB como imutável a partir do fim do boot** (mods só mutam a DB durante o `PostDBModLoader`/load order, não em runtime). Os índices (`Lazy<T>`) devem ser construídos **no primeiro acesso pós-boot, nunca eager no construtor do singleton de DI** (evita pagar o custo no boot e evita ler a DB antes de outros mods terminarem). Se algum mod mutar a DB em runtime (caso não suportado), o índice ficará obsoleto — risco aceito, registrado na spec técnica. O comentário obsoleto em `CatalogService.cs:231` deve ser corrigido na implementação.
>
> **Premissa (revisão 2026-06-12) — locale e fallback:** o índice de busca deve indexar nome **en + pt (código EFT "po")** e shortname; itens sem nome localizado caem no `template.Name`/tpl, espelhando exatamente o comportamento atual de `Search` (`CatalogService.cs:212`). Nenhum item indexável pode "sumir" da busca por falta de locale.
>
> **Premissa (revisão 2026-06-12) — concorrência:** o editor é Blazor Server e pode receber requests concorrentes (múltiplas abas/circuitos). O cache de entries e os índices `Lazy<T>` devem ser **thread-safe** (`Lazy<T>` em modo `ExecutionAndPublication` — default; cache de entries via `ConcurrentDictionary` ou dicionário sob lock). Premissa do item 021 (single-user) cobre escrita concorrente de arquivos, mas **leitura concorrente do cache** ainda precisa ser segura.
- Na **edição**, o recálculo de custo/capacidade é **adiado/agrupado** durante a digitação e o custo só-stash é derivado do cálculo total (uma passada, não duas); a checagem de capacidade do stash só ocorre quando a aba relevante está visível.
- Há **instrumentação de tempo** (log de depuração) cobrindo a listagem (frio/quente), a busca e a navegação entre vistas, com números registrados **antes e depois** no documento as-built.
- Edição externa dos arquivos de classe (via `/sync-classes` ou à mão) é refletida após a invalidação por data de modificação, sem reiniciar o servidor.

## Critérios de aceite

- [ ] Após a primeira carga, navegar lista → detalhe → edição → lista com o cache quente emite **0 (zero)** linhas de log de validação ("dry-run") de classe (baseline: ~22 dry-runs por navegação — ver kickoff §Diagnóstico; comprovado pelos logs de instrumentação).
- [ ] Salvar, criar, duplicar ou deletar **uma** classe revalida **exatamente 1** entrada (a afetada): o log de instrumentação mostra **1** dry-run, não 11, na listagem seguinte; as outras 10 entradas vêm do cache. As 4 operações `Save`/`Delete`/`Create`/`Duplicate` (entry points em `ClassEditorService.cs:172/222/330/368`) são os únicos pontos de invalidação interna.
- [ ] Editar um arquivo de classe **por fora do editor** e recarregar a vista reflete a mudança sem reiniciar o servidor (invalidação por divergência de `(mtime, length)`).
- [ ] A busca de item quente executa em **< 5 ms** (lookup em lista em memória), comprovado por log de tempo, contra o baseline de scan completo do banco medido no "antes"; a abertura da aba de outfit também não varre o banco por render.
- [ ] Digitar a contagem/nível de um item em um loadout grande **não dispara recompute síncrono por keystroke**: o recálculo de custo/capacidade é agrupado por **debounce de ~250 ms** e dispara **≤1 vez** por pausa de digitação; o campo permanece responsivo durante a digitação.
- [ ] O documento as-built contém os **tempos antes e depois** (em ms, mediana de ≥3 amostras) de: listagem (frio e quente), busca de item (fria e quente) e navegação entre as três vistas.
- [ ] Nenhum resultado funcional muda: status de validação, diagnósticos (incl. a colisão de nome cross-file CR-EP-06), custos exibidos e capacidade do stash são **idênticos** aos de antes da otimização para o mesmo conjunto de classes.

## Corner cases

- [ ] **Arquivo de classe alterado entre dois renders** (ex.: `/sync-classes` rodando enquanto a lista está aberta): a entrada cacheada deve ser revalidada na próxima leitura por divergência de data/tamanho, nunca servir dado obsoleto silenciosamente.
- [ ] **Resolução de data de modificação do sistema de arquivos** (granularidade de ~1–2 s): duas escritas no mesmo segundo não podem mascarar a segunda — a invalidação por save/create/delete do próprio editor cobre esse caso independentemente da data de modificação.
- [ ] **Aliasing do objeto cacheado:** o objeto guardado no cache (plano de registro/definição) não pode ser mutado pelo formulário de edição — o form precisa trabalhar sobre uma cópia, sob risco de corromper a entrada cacheada de outras vistas.
- [ ] **Classe inválida/arquivo corrompido:** a entrada cacheada deve registrar o estado de erro (com diagnósticos) como hoje, e não relançar a validação pesada a cada render só porque falhou.
- [ ] **Cache frio após boot / primeira navegação:** o primeiro acesso ainda paga o custo da validação; o ganho é da segunda navegação em diante — a medição "frio" deve deixar isso explícito.
- [ ] **Debounce x salvar rápido:** se o usuário altera um campo e salva antes do recálculo agrupado disparar, o save deve usar o valor final correto (forçar o recálculo pendente antes de persistir), não um custo desatualizado.
- [ ] **Arquivo de classe deletado por fora:** se um arquivo some do diretório entre dois renders, a entrada cacheada correspondente não pode ser servida — a varredura de diretório (`fileUtil.GetFiles`) continua rodando a cada `ListClassFiles`; só o dry-run é cacheado. Entradas órfãs (arquivo inexistente) são descartadas na próxima listagem.
- [ ] **Arquivo de classe novo por fora** (ex.: `/sync-classes` cria um `.jsonc`): aparece na próxima listagem — o cache não pode "esconder" arquivos ainda não vistos. Confirma que o cache é por-entry sobre o resultado do dry-run, não um cache da lista inteira de arquivos.
- [ ] **Colisão de nome cross-file (CR-EP-06):** a detecção de dois arquivos reivindicando o mesmo `name` (`ClassEditorService.cs:121-138`) é uma passada **agregada** que roda DEPOIS dos dry-runs por arquivo. O cache por entry **não pode pular** essa passada — ela deve rodar sempre (é barata, opera sobre as entries já cacheadas) e marcar o Error em todos os arquivos envolvidos, sem regredir o comportamento atual.
- [ ] **Capacidade do stash nunca calculada (aba Stash fechada) + save:** como `CheckStashCapacity` passa a rodar só quando a aba Stash está visível, salvar com a aba Stash nunca aberta deve **forçar** o cálculo de capacidade antes de persistir/validar — não confiar num valor que nunca foi computado (mesmo princípio do corner case "debounce x salvar rápido").

## Fora de escopo

- [ ] Otimização do cálculo de "perfis usando a edição" (`ProfilesUsingEdition`) — só roda no diálogo de delete, em background; fica como está.
- [ ] Mudança de qualquer comportamento visível, layout ou resultado de validação — este item é **só performance** (as melhorias de UX vêm nos itens 030–036).
- [ ] Decisão final sobre desabilitar o pré-render por página: **investigar** se o framework permite o override; se não compensar, registrar a decisão na spec técnica e contar com o cache para baratear o pré-render.

> **Decisão autônoma (revisão 2026-06-12) — prerender:** premissa default = **manter o pré-render** e confiar no cache (escopo *a*) para torná-lo barato (o 2º `OnInitialized` lê do dicionário). Só investigar `prerender: false` por página na spec técnica **se** a medição pós-cache ainda mostrar custo de pré-render relevante (> ~50 ms na 2ª navegação). Isso remove a indecisão da spec funcional sem fechar a porta para a investigação técnica.

## Referências

- Kickoff: [037-performance-cache-00-kickoff.md](./037-performance-cache-00-kickoff.md)
- Épico UX (visão geral + métricas-alvo): [mod-backlog.md](../mod-backlog.md) — seção "Épico: UX do editor (030–037)"

## Histórico

| Data | Evento |
|---|---|
| 2026-06-11 | Spec funcional criada via `/create-spec` (a partir do kickoff de 2026-06-10) |
| 2026-06-12 | Revisão /review-spec (autônoma) — 5 gaps + 4 corner cases endereçados |
