# 030 — Tela "Mods e Configs" · Review da spec técnica 02 (revisão completa)

> **Data:** 2026-07-20<br>
> **Status:** 🟡 Em revisão<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [02-spec-tech](./030-mods-e-configs-tela-02-spec-tech.md) · [01-spec funcional](./030-mods-e-configs-tela-01-spec.md) · [review 01](./030-mods-e-configs-tela-03-spec-tech-review-01.md)<br>

---

## Veredito

**A spec técnica NÃO está pronta para código.** A review 01 achou 8 pontos pontuais e os fechou; esta revisão — feita com três lentes independentes (rastreabilidade CA↔técnica, adversarial no motor, consistência entre documentos) — encontrou **falhas estruturais** que remendo pontual não resolve.

Os achados **convergem em cinco causas raiz**. Duas delas invalidam desenho já aprovado (D-10 é impossível como especificado; o canal híbrido não funciona sem um mecanismo que a spec não previu), e uma tem risco de **quebrar coop**.

**Volumetria da cobertura medida:** dos 34 critérios de aceite, **11 não têm nenhuma cobertura técnica** e 10 têm cobertura fraca (citação sem mecanismo). Dos 20 corner cases, **11 são ignorados** pela técnica. Dos 11 gates humanos, 4 dependem de comportamento não especificado.

## Índice

| ID | Cat | Impacto | Título |
|---|---|---|---|
| PA-02-01 | C | 🔴 | O eixo **desligar** não existe na spec técnica — e o planner não tem como saber se um item está ligado |
| PA-02-02 | C | 🔴 | D-10/CA-030.6 (espelho de referência) é **impossível** com o desenho aprovado |
| PA-02-03 | C | 🔴 | Canal de performance especificado por **dois modelos sobrepostos** — o skip do S-2 é a costura errada |
| PA-02-04 | C | 🔴 | O híbrido **não converge**: `ForceCopy` não grava baseline, então nunca reaplica nem reverte |
| PA-02-05 | C | 🔴 | `MoveToDisabled` explícito pula **todas** as proteções do `ScanExtras` — inclusive o guard coop-safe (Fika) |
| PA-02-06 | C | 🔴 | Remover o filtro `IsOptionalGroupEnabled` cria **loop de download-e-quarentena** a cada sync |
| PA-02-07 | B | 🔴 | `§5.7` quebra CA-030.20: quem aceita os defaults nunca conclui o onboarding |
| PA-02-08 | A | 🟡 | CA-030.19 × CA-030.22 se contradizem na própria funcional |
| PA-02-09 | B | 🟡 | Quarentena de performance guarda **dois conteúdos diferentes** no mesmo path |
| PA-02-10 | A | 🟡 | 11 critérios sem cobertura + CC-4/CC-5 estruturalmente irrepresentáveis |
| PA-02-11 | B | 🟡 | Skew de versão: mover o pack antes dos launchers atualizarem materializa a pasta-fonte no cliente |
| PA-02-12 | C | 🟡 | Inventário de testes que quebram — 1 deles derruba a compilação de todo o assembly |
| PA-02-13 | A | 🟢 | Inconsistências de nomenclatura, numeração e histórico |

**Contadores:** 🔴 7 · 🟡 5 · 🟢 1

✅ **Correção aplicada durante esta revisão:** a linha de decisão do **PA-01-06** na review 01 continha um parêntese órfão (*"Dev Mode bloqueia tudo, inclusive explícito"*) que registrava o **oposto** da decisão tomada — resíduo do script que marcou os pontos como resolvidos. Corrigido: ação explícita vence o Dev Mode.

---

### PA-02-01 · C — Erro de lógica · 🔴 Bloqueador

**O eixo "desligar" não existe na spec técnica, e o planner não tem input para saber se um item está ligado**

**Problema:** o branch de performance (§5.4) consulta **apenas** `_options.ForceApplyGroups.Contains(...)` — que representa "acabou de ser alternado". Não existe nenhuma consulta a "este item está ligado?". Verificado no fonte: `SyncPlannerOptions` tem `IsOptionalGroupEnabled` ([:35](../../project/SPT.Launcher.Base/Sync/SyncPlannerOptions.cs#L35)) e a spec adiciona só `ForceApplyGroups` (§5.3) — **não há `IsPerformanceItemEnabled`**.

Consequências em cadeia:

- Um item **desligado** cai no caminho "alvo não existe → aplica" e tem a config **aplicada mesmo assim**;
- **CA-030.2b** (desligar → volta pra cadeia normal, ou preserva se customizado) — sem nenhum mecanismo;
- **CA-030.5** e **D-8** (arquivo só-de-performance → `config-disabled/performance/`) — sem nenhum mecanismo;
- **§2.2** já pressupõe o conceito ao escrever *"entrada de `config-force` cujo alvo tem performance **ligada** → pulada"*, sem que exista fonte para esse "ligada".

**Por que importa:** metade da feature. A spec especificou com rigor o caminho "ligar" e simplesmente não escreveu o caminho "desligar" — que é o que o player de máquina fraca mais vai usar depois de experimentar.

**Sugestão:** adicionar `Func<string,bool> IsPerformanceItemEnabled` a `SyncPlannerOptions` (espelhando `IsOptionalGroupEnabled`) e escrever o branch de desligado no §5.4, com os três casos: alvo igual ao baseline → reverte para a cadeia normal; alvo customizado → `PreserveCustomized` + entrada de relatório; arquivo sem par em `config`/`config-force` → `MoveToDisabled` com origem `performance`.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-02 · C — Erro de lógica · 🔴 Bloqueador

**D-10/CA-030.6 — o espelho de referência é impossível com o desenho aprovado**

**Problema:** você pediu explicitamente (D-10) que `config-performance` fosse espelhada no cliente como biblioteca de referência, igual ao `config-server`. Mas o `SyncRuleResolver` é um **dicionário prefixo→regra** ([SyncRuleResolver.cs:47-54](../../project/SPT.Launcher.Base/Sync/SyncRuleResolver.cs#L47-L54): `merged[Normalize(kvp.Key)] = rule`): **um prefixo resolve para exatamente uma regra**.

A técnica atribui `BepInEx/config-performance` → `performance-to-config` (E-3/S-6) e ainda adiciona `-performance` a `SourceFolderSuffixes` (E-4), o que a torna **pasta-fonte** — e pasta-fonte, por definição do canal irmão, "nunca é materializada no cliente". Logo o espelho **não existe**: nenhum arquivo aterrissa em `config-performance/` local. CA-030.6 e D-10 estão sem dono em toda a spec técnica (nem §4, nem §6, nem §8 os mencionam).

**Por que importa:** é um requisito seu, decidido conscientemente numa pergunta de múltipla escolha. Ele sumiu na tradução para o desenho técnico sem que ninguém registrasse a perda.

**Sugestão:** três saídas, escolher uma explicitamente:
1. **Duas pastas distintas** — a fonte (`config-performance/`, regra `performance-to-config`) e a referência espelhada (`config-performance-ref/`, regra `mirror-reference`). Custa uma pasta a mais no servidor, mas cada prefixo tem uma regra e ambos os requisitos vivem.
2. **Abandonar o espelho** (reverter D-10) — a config de performance passa a ser inspecionável só pelo servidor.
3. **Branch dedicado no planner** que emite as duas ações para o mesmo prefixo (mirror + apply), fora do mecanismo de `folderRules` — mais poderoso, mas quebra a premissa "uma regra por prefixo" que o resto do motor assume.

**Decisão:** `[ ]` Pendente · `[ ]` Duas pastas · `[ ]` Abandonar o espelho · `[ ]` Branch dedicado

---

### PA-02-03 · C — Erro de lógica · 🔴 Bloqueador

**O canal de performance foi especificado por dois modelos sobrepostos, e o skip do S-2 é a costura errada**

**Problema:** a spec mistura o modelo **antigo** (pack/overlay, com lista paralela `performanceOverlay` e rota `performance-download`) com o modelo **novo** (canal de pasta via `folderRules`), que D-13 diz substituir:

| Trecho | Modelo que assume |
|---|---|
| §2.4 (contrato) | Novo — arquivo em `files[]` com `performanceId`, path relativo à raiz do jogo |
| §4.1, §2.4 | Novo — campo `performanceItems` |
| **S-3** | Antigo — *"loop do pack"*, paths relativos à pasta |
| **S-7**, diagrama §6 | Antigo — campo `performanceOverlay` |
| **S-2** | Contraditório — manda **pular do manifesto tudo sob `config-performance/`** |

Se o skip do S-2 vale, **não existe entrada com `performanceId` no manifesto** e o canal E-3/E-4/E-9 nunca recebe arquivo — CA-030.1 quebra. A funcional é mais precisa que S-2: CA-030.7 diz que os metadados não são distribuídos *"como arquivos comuns de mod"* — o qualificador (ou seja: não sob `SyncFolderRule.Default`) sumiu em S-2, que virou skip total.

Verificado no fonte que `performanceOverlay` é o campo do modelo antigo ([ModUpdater.cs:562](../../../../mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ModUpdater.cs#L562)), consumido como `List<ManifestFile>` em `ProfileViewModel.cs:668` — incompatível com "lista de itens".

**Por que importa:** é o contrato servidor↔launcher, exatamente o que o PA-01-02 tentou fechar. Meio fechado é pior que aberto: o implementador segue o trecho que ler primeiro.

**Sugestão:** eliminar os resíduos do modelo antigo. Reescrever **S-3** (não há mais "pack": os arquivos entram pelo scan normal do `mods_repo`, com path relativo à raiz do jogo), **S-7** (campo `performanceItems`, nunca `performanceOverlay`) e o diagrama §6. Trocar **S-2** por: pular do manifesto apenas os **arquivos de metadados** (`plugins-optional.json`, `performance.json`); os arquivos de `config-performance/` **entram** no manifesto com `performanceId` e são governados pela `folderRule` — não por `Default`. E incluir na remoção (§4) a rota `performance-download` + `RequestHandler.DownloadPerformanceFile` + `_performanceFileMapCache`, que ficam órfãos com D-13.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-04 · C — Erro de lógica · 🔴 Bloqueador

**O híbrido não converge: o caminho force-like não grava baseline, então nunca reaplica nem reverte**

**Problema:** a review 01 estabeleceu que o canal é híbrido — force-like ao ligar, `preserve-divergent` nos syncs seguintes. Mas o `preserve-divergent` **depende de baseline**, e o caminho force-like **não grava baseline**. Verificado: o único `SetHash` de bytes aplicados está em [SyncEngine.cs:106](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L106), no branch `Download`; o bloco `ForceCopy` (`:207-281`) não grava.

Sequência do bug: player liga o item → aplica via caminho force-like → **sem baseline** → próximo sync: local ≠ hash do servidor e **sem entrada de baseline** → cai na regra "no baseline, treated as customized" ([SyncPlanner.cs:273](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L273)) → a config **nunca mais** é atualizada nem revertida ao desligar.

Isso é exatamente o wedge já documentado no teste `OverlayOn_WithoutBaseline_DoesNotApplyPack_KnownWedge` (`SyncOverlayTests.cs:189`) — teste que a spec manda deletar junto com o overlay, **sem recriar a cobertura no canal novo**.

**Por que importa:** quebra CA-030.3, CA-030.4 e CA-030.2b de uma vez. E é silencioso: aplica na primeira vez, e depois simplesmente para de funcionar.

**Sugestão:** o `SyncActionKind` novo (que a spec cita mas **nunca nomeia** — ver PA-02-13) precisa ser tratado no `SyncEngine` como "escreve **e grava baseline**", diferente do `ForceCopy`. Nomeá-lo explicitamente (ex.: `PerformanceCopy`), especificar que o engine chama `SetHash(seedTarget, appliedHash)` após aplicar, e somá-lo a `SyncPlan.IoActionCount`. Adicionar teste de convergência: aplicar → segundo sync sem I/O (o equivalente do `SteadyStateOn_SecondRunHasNoIoActions` que será deletado).

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-05 · C — Erro de lógica · 🔴 Bloqueador

**O `MoveToDisabled` explícito pula todas as proteções do `ScanExtras` — inclusive o guard coop-safe do Fika**

**Problema:** a solução do PA-01-01 (emitir quarentena explícita no planner em vez de depender do `ScanExtras`) resolve o critério, mas **contorna quatro proteções** que só existem dentro do `ScanExtras`:

| Proteção | Linha | O que se perde |
|---|---|---|
| `_protectedNormalized` | [SyncPlanner.cs:347](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L347) | Alimentado por `GetAllKnownOptionalPaths()` (`ProfileViewModel.cs:770`) — os dois mecanismos passam a se contradizer |
| `IsIgnored` / `IsExcludedFromCleanup` | `:345-346` | `ignoredFiles` do manifesto deixa de proteger |
| **coop-safe (Fika)** | `:386-391` | 🔴 Um plugin da família Fika oferecido como opcional e desligado seria quarentenado, **quebrando o join do cliente** — o cenário do item 023 |
| Dev Mode | `:364-376` | Contradiz CC-14 ("com Dev Mode, não move arquivos locais divergentes") |

**Por que importa:** o guard coop-safe existe porque quebrar o Fika tira o jogador do servidor — e num servidor coop o sintoma aparece só quando alguém tenta entrar. É risco de indisponibilidade, não de UX.

**Sugestão:** o branch de quarentena explícita deve **reusar as mesmas guardas** antes de emitir a ação: pular se o path está em `ProtectedPaths`, se é ignorado/excluído de cleanup, se casa o guard coop-safe, e respeitar Dev Mode conforme CC-14 (que trata da build local não solicitada — distinto de CC-19, que é sobre o clique do player). Extrair essas checagens do `ScanExtras` para um método compartilhado, em vez de duplicar a lista.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-06 · C — Erro de lógica · 🔴 Bloqueador

**Remover o filtro `IsOptionalGroupEnabled` cria loop de download-e-quarentena a cada sync**

**Problema:** o plano de aposentar o modelo antigo inclui remover o filtro de [SyncPlanner.cs:66](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L66). Sem ele, os arquivos de mods opcionais **desligados** voltam para `filesToCheck`, e — estando ausentes do disco (porque acabaram de ir para a quarentena) — viram `Download` (`:239-242`).

Combinado com a quarentena explícita do PA-01-01, o mesmo arquivo ganha **`Download` + `MoveToDisabled` no mesmo plano**, e no sync seguinte está ausente de novo: **loop infinito** a cada verificação, com o baseline sendo escrito (`SyncEngine.cs:106`) e apagado (`:155`) toda vez.

**Por que importa:** o player veria download e movimentação de arquivos toda vez que o launcher verificasse — sem nunca convergir. E consumiria banda do servidor a cada login de cada jogador.

**Sugestão:** o filtro **permanece** (a spec técnica já diz "repontar para `optionalId`" em E-11 — é o plano de remoção que estava mais agressivo). Registrar explicitamente na spec que `IsOptionalGroupEnabled` é **renomeado**, não removido, e que a convergência depende dele. Adicionar teste: dois syncs consecutivos com um mod desligado → o segundo tem `IoActionCount == 0`.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-07 · B — Edge case · 🔴 Bloqueador

**O stub §5.7 quebra CA-030.20: quem aceita os defaults nunca conclui o onboarding**

**Problema:** no `SaveAndReturn`, o early-return de `changed.Count == 0` acontece **antes** de `settings.ModsConfigsOnboardingDone = true`. No onboarding, o estado inicial é "tudo ligado" (D-5) — que é justamente a configuração que o player de máquina boa vai aceitar sem tocar em nada. Esse player sai com `changed == 0` e:

- a marca de onboarding **nunca** é gravada → o modal **repete a cada login** (contradiz CA-030.20 e D-17);
- nenhuma preferência é persistida;
- nenhuma ingestão roda com as escolhas dele (contradiz CA-030.19).

**Por que importa:** atinge o caminho mais provável do fluxo mais visível da feature — a primeira experiência de todo jogador novo.

**Sugestão:** separar as duas responsabilidades no stub: **sempre** gravar `ModsConfigsOnboardingDone` e persistir o estado corrente ao sair do fluxo de onboarding; o early-return passa a valer **apenas** para o disparo do sync (CA-030.22), não para a persistência. Isso também resolve o PA-02-08.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-08 · A — Gap de especificação · 🟡 Importante

**CA-030.19 e CA-030.22 se contradizem na própria spec funcional**

**Problema:** CA-030.19 diz *"ao sair da tela [...] a primeira ingestão roda já com as escolhas dele"*; CA-030.22 diz *"sair da tela sem nenhuma alteração não dispara sync"*. No onboarding os dois se aplicam ao mesmo evento e pedem coisas opostas — o player que aceita os defaults saiu "sem alteração", mas a primeira ingestão precisa rodar (é ela que instala os mods).

**Sugestão:** qualificar CA-030.22 para o fluxo **normal** (fora do onboarding) e deixar CA-030.19 soberano no **primeiro acesso**: no onboarding, sair sempre dispara a ingestão inicial, porque não há estado anterior em disco a preservar.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-09 · B — Edge case · 🟡 Importante

**A quarentena de performance guarda dois conteúdos diferentes no mesmo path**

**Problema:** `config-disabled/performance/<rel>` recebe conteúdos de naturezas opostas em momentos diferentes:

- ao **ligar** (CA-030.2): a config **do player**, que foi sobrescrita;
- ao **desligar** (CA-030.5): a config **do servidor**, que está sendo removida.

Ligar e depois desligar o mesmo item grava nos dois casos no mesmo path — a segunda escrita **destrói o backup da config customizada do player**. CC-6 autoriza a sobrescrita com o argumento de que é *"a mesma origem e a mesma config"* — o que é falso aqui. E G-7 só testa as três origens entre si, nunca este par.

**Por que importa:** D-14 foi criada exatamente para impedir que quarentena destrua config de player. O namespace por origem resolveu a colisão *entre* canais e deixou passar a colisão *dentro* do canal de performance.

**Sugestão:** separar por natureza, não só por canal: `config-disabled/performance/replaced/<rel>` (config do player sobrescrita ao ligar) e `config-disabled/performance/removed/<rel>` (config do servidor retirada ao desligar). Ajustar CC-6 (a sobrescrita só é aceitável quando origem **e natureza** coincidem) e estender G-7 para cobrir o par ligar→desligar.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-10 · A — Gap de especificação · 🟡 Importante

**11 critérios sem cobertura técnica, e dois corner cases estruturalmente irrepresentáveis**

**Problema:** a matriz de rastreabilidade completa apontou:

**Sem nenhuma cobertura técnica:** CA-030.2b, CA-030.5 (→ PA-02-01), CA-030.6 (→ PA-02-02), CA-030.10 (restaurar da quarentena — o enum não tem ação de restauração, e o caminho implícito `Download` **contradiz D-7**), CA-030.11 e CA-030.11b (marcador de novidade não tem estado persistido — §5.5 não tem conjunto de "ids já vistos"), CA-030.16c (navegação lateral no onboarding), CA-030.24, CA-030.25 (falha parcial → erro visível), CA-030.26 e CA-030.28 (migração do modelo antigo).

**Corner cases irrepresentáveis no shape aprovado:** CC-4 (mesmo arquivo em dois itens de performance) e CC-5 (arquivo compartilhado entre dois mods) — o contrato de §2.4 dá **um** `optionalId`/`performanceId` por arquivo, então a situação que os dois corner cases descrevem não tem como ser expressa nem detectada.

**Gates pendurados no vazio:** G-9 (bundles × cache 3D — CC-13 pede "definir" e ninguém definiu), G-10 (migração), G-1 (exige que os paths do grupo se movam **juntos**, mas §2.3 emite ações independentes por arquivo, sem atomicidade).

**Sugestão:** tratar em três lotes. (a) Os que dependem de PA-02-01/02/03 saem de graça quando aqueles forem resolvidos. (b) CA-030.10, CA-030.11/11b, CA-030.25 precisam de mecanismo próprio na técnica (ação de restauração, conjunto de ids vistos, agregação de falha parcial). (c) CC-4/CC-5, migração (CA-030.26/28) e bundles (CC-13/G-9) precisam de **decisão sua** — ou entram com desenho próprio, ou vão para Fora de escopo com o gate correspondente removido. Um gate humano obrigatório sem base técnica é pior que nenhum: dá falsa sensação de cobertura.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar os 3 lotes · `[ ]` Caminho alternativo

---

### PA-02-11 · B — Edge case · 🟡 Importante

**Skew de versão: mover o pack antes dos launchers atualizarem materializa a pasta-fonte no cliente**

**Problema:** o `config.json` de produção **não define `folderRules`** — todos os clientes usam a tabela fallback embutida no exe ([SyncRuleResolver.cs:26-40](../../project/SPT.Launcher.Base/Sync/SyncRuleResolver.cs#L26-L40)). Se S-1/S-3 mover o pack para `mods_repo/BepInEx/config-performance/` **antes** de todos os launchers estarem atualizados, um launcher antigo resolve esses paths como `SyncFolderRule.Default` e **baixa literalmente para `<gameroot>/BepInEx/config-performance/`** — materializando a pasta-fonte no cliente (a invariante que o teste `Force_source_folder_is_never_materialized_on_the_client` protege) e sem aplicar nada em `config/`.

**Por que importa:** é exatamente o defeito que este item veio corrigir, reintroduzido pela ordem de deploy. E atinge quem ainda não atualizou — justamente quem você não alcança remotamente.

**Sugestão:** adicionar à spec uma **ordem de rollout** explícita: (1) publicar o launcher novo e confirmar adoção; (2) só então mover a pasta no servidor. Alternativa que remove a dependência de ordem: o servidor passa a emitir `folderRules` explícito com a entrada nova — clientes antigos que não conhecem `performance-to-config` caem no fallback do parser (`TryParse` falha → entrada ignorada), o que é seguro. Registrar como gate de deploy, não só como nota.

**Decisão:** `[ ]` Pendente · `[ ]` Rollout ordenado · `[ ]` folderRules explícito do servidor · `[ ]` Ambos

---

### PA-02-12 · C — Erro de lógica · 🟡 Importante

**Inventário de testes que quebram — um deles derruba a compilação do assembly inteiro**

**Problema:** a spec menciona genericamente (R-7) que "testes existentes precisam de atualização", sem inventário. O levantamento real:

- 🔴 **`SyncTestFixture.cs:81`** (`optionalGroup = group`) — com o rename, **todo o assembly `SPT.Launcher.Tests` deixa de compilar**, porque cada `fx.Entry(...)` passa por ali. É o primeiro item a corrigir, antes de qualquer outro teste.
- **11 testes com assert no path exato de `-disabled`** quebram com D-14: `SyncForceConfigTests` (`:121`, `:135`, `:160`, `:184`, `:233`, `:254`), `SyncPlannerTests` (`:143`, `:301`), `SyncEngineTests` (`:43`, `:54`, `:331`).
- **2 testes viram falso-positivo** (passam mesmo com o motor errado): `SyncForceConfigTests.cs:148` e `:270` (`Contains("config-disabled")` continua casando com o path aninhado).
- **`SyncPlannerTests.cs:156`** (`Disabled_optional_group_files_are_never_extras`) quebra duas vezes: compilação e semântica.
- **`SyncOverlayTests.cs` inteiro** (8 testes) morre com o overlay — e com ele morrem as garantias documentadas de revert no OFF, preservação de customização e **convergência sem churn**, que é justamente o PA-02-04. Nada no plano recria essa cobertura.

**Sugestão:** substituir R-7 por este inventário explícito na spec, com a ordem de correção (fixture primeiro), marcar os 2 falso-positivos para serem endurecidos (assert no path completo, não `Contains`), e listar como obrigatória a recriação das 4 garantias do `SyncOverlayTests` no canal novo.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

### PA-02-13 · A — Gap de especificação · 🟢 Menor

**Nomenclatura, numeração e histórico inconsistentes**

**Problema:** achados de menor impacto, agrupados:

- **O `SyncActionKind` novo nunca é nomeado** — E-12 e §4 dizem "novo `ActionKind`" no singular, §5.4 chama `AddPerformanceAction(...)` (helper nunca definido), E-14/E-15 falam de "labels novos" no plural.
- **`ForceApplyGroups` tem três definições incompatíveis** — "recém-alternados" (§1), "acabou de ligar, de performance" (§5.3), "alimentado por `PendingApply`" (review 01). E `PendingApply` é persistido, então "recém" pode ser de três sessões atrás.
- **`performanceItems` × `performanceOverlay`** (coberto em PA-02-03) e **grupo × item × id** usados de forma intercambiável.
- **S-2 cita CA-030.8**, sendo que o critério correto é CA-030.7. **Review 01 cita "S-7 (§2.3)"**, mas S-7 está em §2.5.
- **§4 se auto-contradiz**: manda remover `ProfileViewModel:891-901` e a nota logo abaixo diz que `DownloadModFile` em `:893` não pode sair.
- **Call-sites órfãos** fora dos ranges de remoção: `ProfileViewModel.cs:770` (`ProtectedPaths`) e `:71` (`OptionalModToggle`).
- **§9 não foi atualizada após a review 01** — o check 2 ("sem invenção de API") continua ✅ apesar do `SyncActionKind.Preserve` inexistente que a review achou; o histórico da técnica tem só a linha "Criação".
- **Funcional:** CA-030.16 cita CC-14 para uma regra que CC-14 não contém; RNs fora de ordem (1,2,3,4,7,8,5,6); histórico diz "28 critérios" quando são 34.

**Sugestão:** varredura de nomenclatura ao aplicar os pontos maiores — nomear o `ActionKind`, definir `ForceApplyGroups` uma vez só, unificar o vocabulário em "item/id", corrigir as referências cruzadas e atualizar §9 e os históricos.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo

---

## Método desta revisão

Três lentes independentes, sem acesso ao raciocínio uma da outra:

1. **Rastreabilidade** — matriz CA↔técnica, decisão a decisão, corner a corner, gate a gate.
2. **Adversarial no motor** — dado o conjunto de mudanças propostas, o que quebra no código e nos testes existentes.
3. **Consistência documental** — contradições entre e dentro dos três documentos, com citação literal dos dois lados.

Os achados 🔴 foram **verificados no fonte** antes de entrar aqui: o dicionário do resolver ([SyncRuleResolver.cs:47-54](../../project/SPT.Launcher.Base/Sync/SyncRuleResolver.cs#L47-L54)), a ausência de `SetHash` no `ForceCopy` ([SyncEngine.cs](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs)), as guardas do `ScanExtras` ([SyncPlanner.cs:344-391](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L344-L391)) e a ausência de `IsPerformanceItemEnabled` em `SyncPlannerOptions`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-20 | Guilherme | Criação — revisão completa com 3 lentes independentes. 13 pontos (7 🔴, 5 🟡, 1 🟢) agrupados por causa raiz. Corrigido o registro invertido da decisão do PA-01-06 na review 01 |
