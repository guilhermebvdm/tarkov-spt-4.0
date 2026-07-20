# 030 — Tela "Mods e Configs": mods opcionais + configs de performance · Spec funcional

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [009 — mods opcionais](../009-mods-opcionais-descricao/009-mods-opcionais-descricao-01-spec.md) · [021 — grupos + base-URL](../021-optional-mods-groups-baseurl/021-optional-mods-groups-baseurl-01-spec.md) · [008 — configs performance](../008-configs-performance/008-configs-performance-01-spec.md) · [007 — sincronização](../007-sincronizacao-arquivos/007-sincronizacao-arquivos-01-spec.md)<br>

---

## Objetivo

Dar ao player **uma tela só** para adequar o jogo à máquina dele, com dois eixos independentes:

1. **Mods opcionais** — mods pesados (afetam performance), muito punitivos, ou "malucos" (ex.: TarkovIRL, que muda o comportamento da mira). O player liga/desliga.
2. **Configs de performance** — configurações de mods existentes (ou, mais raro, de mods opcionais) curadas para performance.

Substitui o modelo atual de mods opcionais (pasta `Opcionais/` + `optionalGroups` + `offFolders`) e o toggle global de performance do item 008, unificando ambos no **motor de sync por regra de pasta** (item 007).

## Decisões travadas (2026-07-19)

| # | Decisão | Escolha |
|---|---|---|
| D-1 | Precedência quando o mesmo arquivo está em vários canais | **config-performance > config-force > config** (`config-server` segue só referência, nunca aplica) |
| D-2 | Escopo de um mod opcional | **Grupo multi-pasta** — plugin + config + bundles pertencem ao mesmo item e se movem juntos |
| D-3 | Sistema atual (`Opcionais/`, `optionalGroups`, `offFolders`, rotas `optionals-*`) | **Aposentar e migrar** |
| D-4 | Onboarding | Gatilho = cliente sem plugins → tela dedicada + modal one-shot |
| D-5 | Estado inicial no onboarding | **Tudo ligado** (mods opcionais marcados, performance desmarcada) |
| D-6 | Mod opcional novo, para player que já configurou | **Desligado + aviso de novidade** na tela e no item de menu |
| D-7 | Remarcar mod que está em `plugins-disabled` | **Restaura local validando hash**; se divergir, rebaixa do servidor |
| D-8 | Arquivo só-de-performance ao desligar performance | **Move para `config-disabled/`** (quarentena do motor, não a lixeira do Windows) |
| D-9 | Onde vive `config-performance` | **`mods_repo/BepInEx/config-performance/`**, junto das irmãs `config-*` |
| D-10 | Espelho local de `config-performance` | **Sim, `mirror-reference`** — igual `config-server` |
| D-11 | Atalho de recomendação | **Sem botão dedicado** — só os toggles "todos" de cada coluna |
| D-12 | Toggle "Usar configs de performance" em Configurações | **Removido** — a tela nova é o único lugar |

### Decisões da revisão (2026-07-19, `/review-spec`)

| # | Decisão | Escolha |
|---|---|---|
| D-14 | Colisão no `config-disabled/` (CC-11) | **Namespace por origem:** `config-disabled/force/<rel>`, `config-disabled/optional/<rel>`, `config-disabled/performance/<rel>`. Elimina a colisão entre origens por construção, em vez de confiar em nomes não coincidirem |
| D-15 | Mod opcional com parte server-side (CC-12) | **Client-only.** O servidor **recusa** `paths` sob `user/mods/` ao validar o `plugins-optional.json` |
| D-16 | Ligar item sobre config já customizada (CA-030.2) | **Aplica + preserva a anterior** em `config-disabled/performance/`. Ligar é ação explícita; não aplicar deixaria o toggle sem efeito visível |
| D-17 | Onboarding se o `plugins` esvaziar depois (CA-030.16b) | **Não repete.** A marca de "onboarding concluído" é a fonte de verdade; o sync restaura os plugins conforme as preferências já salvas, sem exigir nova escolha |

> **Custo de D-14 que vale registrar:** o backup do `config-force` hoje grava direto em `config-disabled/<rel>` e está em produção desde a 2.3.0. Mover para `config-disabled/force/<rel>` é uma linha no motor + ajuste dos testes daquele item. Optou-se pela simetria (as três origens tratadas igual) em vez de deixar o force assimétrico só para evitar tocar em código estável. **Backups já existentes na raiz de `config-disabled/` permanecem válidos e recuperáveis** — não há migração, apenas o destino de escrita muda.

### Decisão derivada (consequência de D-9 + D-1)

**D-13 — `config-performance` vira canal de pasta, não overlay de manifesto.** Com a pasta morando junto das irmãs (D-9), o caminho natural é `config-performance/<rel>` → `config/<rel>`, exatamente como `config-force/<rel>` → `config/<rel>`. Isso a coloca no mesmo motor de `folderRules` (item 007) e **aposenta o `SyncManifestOverlay`** (item 008), que existia só porque a pasta vivia fora do `mods_repo`. Menos um mecanismo paralelo.

## Modelo de dados (servidor)

### Mods opcionais — `mods_repo/BepInEx/plugins-optional.json`

```jsonc
{
  "version": 1,
  "mods": [
    {
      "id": "tarkov-irl",                       // único e estável (chave da preferência do player)
      "name": "Tarkov IRL",                     // string OU { "pt": "...", "en": "..." }
      "paths": [                                // TODOS os arquivos do mod, qualquer pasta (D-2)
        "BepInEx/plugins/TarkovIRL.dll",
        "BepInEx/config/com.tarkovirl.cfg"
      ],
      "description": { "pt": "...", "en": "..." }
    }
  ]
}
```

### Configs de performance — `mods_repo/BepInEx/config-performance/performance.json`

```jsonc
{
  "version": 1,
  "items": [
    {
      "id": "shadows-low",
      "name": { "pt": "Sombras reduzidas", "en": "Reduced shadows" },
      "files": ["com.sombras.cfg"],             // relativo à própria config-performance/
      "description": { "pt": "...", "en": "..." }
    }
  ]
}
```

Um item agrupa 1+ arquivos: o player liga "Sombras reduzidas", não `com.sombras.cfg`.

## Critérios de aceite

### A. Canal config-performance

> **Distinção que governa esta seção:** *ligar/desligar um item* é **ação explícita do player** (aplica na hora, sobrepondo o que estiver lá); *os syncs seguintes* respeitam a customização dele (`preserve-divergent`). Sem separar os dois momentos, "performance vence tudo" (D-1) e "edição do jogador prevalece" (RN-3) se contradizem no caso mais comum — o player que já tinha mexido no arquivo antes de ligar o item.

- [ ] **CA-030.1** — Dado um arquivo em `config-performance/<rel>` e o item correspondente **ligado**, quando o sync roda, então ele é aplicado em `config/<rel>`, **vencendo** `config-force` e `config` (D-1).
- [ ] **CA-030.2 (ligar sobre config já customizada)** — Dado que o player **já havia editado** `config/<rel>` **antes** de ligar o item, quando ele liga, então a versão de performance **é aplicada mesmo assim** — ligar é ação explícita; se não aplicasse, o toggle não teria efeito visível e pareceria quebrado — e o arquivo anterior dele é **preservado em `config-disabled/performance/<rel>`** antes da sobrescrita (D-16, D-14).
- [ ] **CA-030.2b (desligar)** — Dado o item **desligado**, quando o sync roda, então: se o arquivo **não** foi customizado desde a aplicação, `config/<rel>` volta a ser governado pela cadeia normal (`config-force`, senão `config`); se **foi** customizado, a edição é preservada (RN-3) e o relatório registra que a reversão foi pulada por customização.
- [ ] **CA-030.3 (edição posterior)** — Dado que o item está ligado e o player edita o arquivo **depois** disso, quando o sync roda, então a edição é **preservada** (`preserve-divergent`, tendo a versão de performance como baseline).
- [ ] **CA-030.4** — Dado que o servidor publica uma versão **nova** do arquivo e o player **não** o customizou desde a última aplicação, quando o sync roda, então ele recebe a versão nova.
- [ ] **CA-030.5** — Dado um arquivo que existe **só** em `config-performance` (sem par em `config/` nem em `config-force`), quando o player desliga o item, então o arquivo é movido para `config-disabled/performance/<rel>` (D-8, D-14), nunca apagado de forma irrecuperável.
- [ ] **CA-030.6** — A pasta `config-performance` é espelhada no cliente como **biblioteca de referência** (`mirror-reference`): sempre a versão do servidor, extras não deletados, edição local ali é sobrescrita (D-10).
- [ ] **CA-030.7** — Nem `plugins-optional.json`, nem `performance.json`, nem a pasta `config-performance/` são distribuídos como arquivos comuns de mod para o jogo do player (hoje seriam — ver §Defeito atual).

### B. Mods opcionais

- [ ] **CA-030.8** — Dado um mod marcado como opcional e **desligado** pelo player, quando o sync roda, então **todos** os `paths` dele (plugin + config + bundles) são movidos para a quarentena correspondente sob o namespace do canal — `plugins-disabled/optional/<rel>`, `config-disabled/optional/<rel>` (D-14) — e nada dele é baixado (D-2).
- [ ] **CA-030.8b (validação de conteúdo)** — Dado um `plugins-optional.json` cujo `paths` inclua caminho sob `user/mods/`, quando o servidor gera o manifesto, então o item é **recusado com erro explícito** e não é oferecido ao player (D-15) — mod opcional é client-only.
- [ ] **CA-030.9** — Dado um mod opcional **ligado**, quando o sync roda, então todos os `paths` dele são instalados normalmente.
- [ ] **CA-030.10** — Dado um mod que o player **remarca** e cujos arquivos estão em `*-disabled`, quando ele confirma, então o launcher **restaura da quarentena se o hash bater** com o servidor; se divergir, **baixa a versão do servidor** e descarta a antiga (D-7).
- [ ] **CA-030.11** — Dado um mod opcional **novo** publicado pelo servidor e um player que **já configurou** a tela antes, quando ele loga, então o mod vem **desligado** e tanto o item de menu quanto a linha do mod exibem **marcador de novidade** (D-6). O marcador some quando o player **sai da tela** (mesma regra do modal, CA-030.20) — não ao simplesmente abri-la, para que abrir sem querer não descarte o aviso.
- [ ] **CA-030.11b** — Dado um mod opcional que o servidor **removeu** do `plugins-optional.json`, quando o sync roda, então: se ele continua em `mods_repo`, vira mod **obrigatório** e é instalado para todos; se saiu também do `mods_repo`, o mirror o move para `*-disabled`. Em nenhum dos casos o player fica com um item fantasma na tela.

### C. Tela e navegação

- [ ] **CA-030.12** — A tela "Mods e Configs" é acessível pelo **menu lateral** (abaixo de "Launcher") e por um **resumo clicável** na área superior direita da tela logada.
- [ ] **CA-030.13** — O resumo exibe, de forma verificável: (a) contagem de mods opcionais ligados sobre o total, (b) contagem de configs de performance ligadas sobre o total, (c) marcador visual quando há item novo não visto (CA-030.11). Ex.: *"5 de 7 mods opcionais · Performance: 0 de 4"*.
- [ ] **CA-030.14** — A tela tem **2 colunas** (Mods opcionais | Configs de performance), cada item com **nome + descrição** no idioma ativo, e cada coluna com um toggle **"todos"** (D-11).
- [ ] **CA-030.15** — Toda string nova (tela, modal, resumo, descrições dos JSONs) existe em **pt e en**, com **paridade total de chaves** entre os dois locales — o loader é all-or-nothing: uma única chave ausente derruba o locale inteiro e o launcher cai no fallback pt.
- [ ] **CA-030.15b (estado vazio)** — Dado um servidor **sem** mods opcionais e **sem** configs de performance, quando o player abre a tela, então cada coluna exibe estado vazio explicativo (não lista fantasma nem erro), e o resumo na tela logada não induz clique inútil.

### D. Onboarding (primeiro acesso)

- [ ] **CA-030.16** — Dado um cliente **sem nenhum plugin instalado** — definido como: `BepInEx/plugins` **inexistente ou sem nenhum `.dll` em qualquer profundidade** — quando o player loga, então ele é levado direto à tela "Mods e Configs", **antes do primeiro sync** (D-4). O Dev Mode **não** dispara o onboarding (CC-14).
- [ ] **CA-030.16b** — Dado um player que já passou pelo onboarding mas cujo `BepInEx/plugins` ficou vazio depois (apagou à mão, antivírus removeu, instalação corrompida), quando ele loga, então o onboarding **não** se repete — o gatilho consulta a marca de "onboarding concluído" persistida, não só o estado do disco (D-17). O sync normal restaura os plugins conforme as preferências já salvas, sem exigir nova escolha.
- [ ] **CA-030.16c** — Dado que o player é levado ao onboarding, quando ele tenta navegar para outra tela sem concluir, então ou é permitido (e vale CA-030.20, o fluxo se repete no próximo login) ou é bloqueado com aviso — o comportamento é explícito, nunca uma tela sem saída.
- [ ] **CA-030.17** — A tela abre com **tudo ligado**: mods opcionais marcados, performance desmarcada (D-5).
- [ ] **CA-030.18** — Um **modal** explica a escolha ("se a sua máquina não é high-end, remova os mods opcionais e ative as configs de performance"); ao clicar OK ele fecha e o player configura.
- [ ] **CA-030.19** — Ao sair da tela, o player volta para a aba **Launcher** e a **primeira ingestão** roda já com as escolhas dele.
- [ ] **CA-030.20** — Dado que o player **fechou o launcher** durante o onboarding sem concluir, quando ele loga de novo, então o fluxo se repete (o modal só para de aparecer depois que ele sair da tela uma vez).

### E. Aplicação das mudanças

- [ ] **CA-030.21** — Ao sair da tela com alterações pendentes, elas são aplicadas **em sequência**, reusando a mesma experiência visual de atualização de arquivos (barra de progresso + status + relatório).
- [ ] **CA-030.22** — Sair da tela **sem nenhuma alteração** não dispara sync.
- [ ] **CA-030.23** — Dado o **jogo em execução**, quando o player tenta aplicar, então a operação é **bloqueada com aviso** (mover DLL com o EFT aberto falha) — mesmo gate do wipe/excluir conta.
- [ ] **CA-030.24** — Em **atualizações automáticas** posteriores, as escolhas do player continuam valendo dentro da regra global de sync (não são reavaliadas nem resetadas).
- [ ] **CA-030.25** — Falha parcial (alguns itens aplicados, outros não) termina com **estado de erro visível** e relatório listando o que falhou — nunca sucesso silencioso.

### F. Migração do modelo antigo (D-3)

- [ ] **CA-030.26** — Os grupos atuais são migrados: `gore` e `hollywood` viram **mods opcionais**; `grass` (que usa `offFolders`) vira **config de performance**.
- [ ] **CA-030.27** — A pasta `Opcionais/`, o `optionalGroups[]` do config e as rotas `optionals-list` / `optionals-manifest` / `optional-download` são **removidos** do servidor; o `SyncManifestOverlay` e o `OptionalModsHelper` saem do launcher (D-13).
- [ ] **CA-030.28** — Player que tinha estado salvo no modelo antigo não fica em estado inconsistente após atualizar (migração de preferência ou reset explícito com o onboarding).

## Regras de negócio

- **RN-1 — Precedência única.** `config-performance` (item ligado) > `config-force` > `config`. `config-server` e `config-performance` **como espelho** são referência e nunca escrevem em `config/`.
- **RN-2 — Colisão visível.** Quando um arquivo existe em `config-force` **e** em `config-performance`, o servidor **avisa ao gerar o manifesto** e o relatório registra `performance-sobrepos-force: <arquivo>`. A regra não muda (RN-1), mas a colisão não fica invisível — ela é um risco de paridade em coop.
- **RN-3 — Edição do jogador prevalece.** Arquivo customizado pelo player é preservado, inclusive contra atualização do servidor. Consequência aceita: quem editou não recebe mais updates daquele arquivo — o relatório deve deixar isso legível.
- **RN-4 — Nada sai sem quarentena, e cada origem tem a sua.** Toda remoção (mod desligado, arquivo só-de-performance, config sobrescrita ao ligar) vai para `<canal>-disabled/<origem>/<rel>`, com `<origem>` ∈ {`force`, `optional`, `performance`} (D-14). A raiz de `*-disabled` segue protegida do `deleteFiles` pelo guard vigente, e o guard cobre as subpastas por herança de prefixo.
- **RN-7 — Ligar/desligar é ação explícita; sync é conservador.** No **momento** em que o player alterna um item, a escolha dele vence o que estiver no disco (aplica/remove, sempre com quarentena). Nos **syncs seguintes**, vale `preserve-divergent`: o que ele editou depois é preservado. Sem essa separação, "performance vence tudo" (D-1) e "edição prevalece" (RN-3) se contradizem.
- **RN-8 — Mod opcional é client-only.** Nenhum item pode referenciar `user/mods/` (D-15); a validação é no servidor, ao gerar o manifesto (CA-030.8b).
- **RN-5 — Metadados nunca viram conteúdo.** Os JSONs de definição e a pasta `config-performance` não podem ser sincronizados como arquivos de mod (CA-030.7).
- **RN-6 — Preferência é do player, por id.** O estado de cada item é salvo por `id` e sobrevive a atualizações; item removido do servidor some da tela sem apagar a preferência de outros.

## Corner cases

- **CC-1 — Onboarding em cliente que já tem plugins.** Player existente nunca dispara o gatilho (D-4); ele conhece a tela pelo menu/resumo e por CA-030.11.
- **CC-2 — Gatilho avaliado tarde.** Se o sync rodar antes da checagem, o cliente passa a ter plugins e o onboarding nunca acontece. A ordem de CA-030.16 (checar antes do sync) é obrigatória.
- **CC-3 — Player liga performance, edita o arquivo, desliga.** A edição prevalece (RN-3): ele fica com o arquivo customizado mesmo com o item desligado. Intencional.
- **CC-4 — Mesmo arquivo em dois itens de performance.** Precisa ser detectado na geração (dois itens ligando/desligando o mesmo arquivo criam estado ambíguo).
- **CC-5 — Mod opcional cujo `paths` inclui arquivo também presente em outro mod.** Desligar um não pode arrastar arquivo compartilhado do outro.
- **CC-6 — Arquivo em `*-disabled` de uma desativação anterior ao remarcar/desmarcar de novo.** Com D-14 a colisão só pode ocorrer **dentro da mesma origem** (mesmo item, desativado duas vezes). Aí sobrescrever é aceitável — é a mesma origem e a mesma config —, mas precisa ser logado (não silencioso).
- **CC-7 — Coop (Fika PVE).** Mod opcional que altere gameplay compartilhado (loot, spawns) diverge entre players. Client-side (mira, efeitos visuais) é seguro. Marcar no conteúdo o que não pode ser opcional.
- **CC-8 — Cancelar no meio da aplicação.** Resultado parcial + relatório (CA-030.25); nunca deixar arquivo parcial (escrita atômica do motor).
- **CC-9 — JSON de definição inválido/ausente.** Tela abre sem itens daquele eixo, com aviso — nunca crash nem lista fantasma.
- **CC-10 — `id` renomeado no servidor.** Equivale a "item novo" (D-6) e o antigo some; documentar que `id` é chave estável.
- **CC-11 — `config-disabled/` com três origens → resolvido por namespace (D-14).** A pasta passaria a guardar três coisas distintas — backup do `config-force`, quarentena de config de mod opcional desligado (CA-030.8) e backup da edição sobrescrita ao ligar performance (CA-030.2) — com risco de um sobrescrever o outro por homonímia, destruindo config do player. **Resolvido:** cada origem grava em subpasta própria (D-14), então a colisão entre origens deixa de existir. Sobra apenas colisão *dentro* da mesma origem, tratada em CC-6.
- **CC-12 — Mod opcional com parte server-side → proibido por construção (D-15).** A quarentena `*-disabled` resolve plugin de cliente. Módulo de servidor em `user/mods/<mod>/` **não** se desabilita movendo dentro de `user/mods` (o SPT carrega mesmo assim — só sai da pasta resolve). **Resolvido:** mod opcional é **client-only** (D-15), com validação no servidor recusando qualquer `paths` sob `user/mods/`. Razão de fundo: num servidor coop compartilhado, mod de servidor por-player é conceitualmente impossível — o servidor é único, um player não pode ter economia/loot diferente do outro.
- **CC-13 — Mod opcional com bundles e o cache 3D.** Ligar/desligar mod que traz bundles interage com o pipeline de cache do SPT 4.0: o servidor calcula o hash de bundles no boot e o cliente só popula `user/cache/bundles` abrindo o jogo. Trocar bundles pelo launcher pode deixar cache obsoleto (asset velho carregado, ou download 3D na primeira raid). Definir se a troca invalida o cache local do bundle afetado.
- **CC-14 — Dev Mode ligado.** O Dev Mode preserva arquivos/builds locais e pula a verificação automática. Um dev com build própria de um plugin que é opcional não pode ter essa build movida para quarentena por um toggle. Regra: com Dev Mode ligado, a aplicação de mods opcionais **não** move arquivos locais divergentes — registra no relatório e segue.
- **CC-15 — Concorrência com o sync automático do login.** O player pode sair da tela (CA-030.21) enquanto a verificação automática ainda roda. As duas execuções não podem se sobrepor no mesmo arquivo: serializar (a segunda espera) ou bloquear a saída até a primeira terminar.
- **CC-16 — Arquivo em uso / sem permissão.** DLL carregada por outro processo, config aberta em editor, pasta somente-leitura. Falha por arquivo, contada e reportada (CA-030.25), sem abortar os demais itens — distinto de "cancelado" (CC-8).
- **CC-17 — Item de performance ligado cujo arquivo sumiu do servidor.** O que já foi aplicado em `config/` permanece (não há de onde reverter); o item some da tela e o relatório registra. Não pode virar erro recorrente a cada sync.
- **CC-18 — Player liga e desliga o mesmo item várias vezes antes de sair da tela.** Só o **estado final** é aplicado; nada de aplicar e reverter em sequência (CA-030.21 opera sobre o diff entre estado inicial e final, não sobre o histórico de cliques).

## Fora de escopo

- Detecção automática de hardware para sugerir preset (o modal orienta, o player decide).
- Perfis nomeados de configuração salvos por player.
- A feature de "configurações recomendadas de jogo" (teclas, gráficos, fundo do menu) — tema separado, parado a pedido do usuário até o conteúdo curado ser fechado; tem fonte e fluxo próprios.
- Redesenho das outras áreas da tela logada além do resumo (CA-030.12/13).
- Reversão automática de bundles no cache 3D do cliente (CC-13 define o comportamento esperado; a implementação do invalidador de cache, se necessária, é item próprio).
- **Mods opcionais com parte server-side** (`user/mods/`) — proibidos por D-15. Se algum dia um mod de servidor precisar ser opcional, é item próprio: exige quarentena fora de `user/mods` e esbarra no fato de o servidor ser compartilhado entre todos os players do coop.

## Defeito atual que este item corrige

A pasta `config-performance` criada no servidor está em `mods_repo/BepInEx/config-performance/`, mas o código lê de `Launcher-Updater/config-performance` ([ModUpdater.cs:50](../../../../mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ModUpdater.cs#L50)). Hoje, portanto: o overlay de performance vem **vazio**, e como a pasta está dentro do `mods_repo` (varrido inteiro pelo gerador) e não há regra para esse prefixo no resolver, ela cai em `SyncFolderRule.Default` e é **distribuída inerte para o jogo de todos os players**. D-9 + D-13 resolvem os dois lados.

## Gates

### Gates de build (agente)
`dotnet build` + `dotnet test` verdes. Nunca rodar o exe.

### Gates humanos (validação in-game — obrigatórios)
- [ ] **G-1** — Ligar/desligar cada mod opcional e confirmar **em disco** que todos os `paths` do grupo se movem juntos (plugin + config + bundles).
- [ ] **G-2** — Confirmar in-game que o efeito aparece/some (build verde não prova asset carregado).
- [ ] **G-3** — Ligar performance e confirmar que a config aplicada vence o `config-force` do mesmo arquivo (D-1) e que a colisão aparece no relatório (RN-2).
- [ ] **G-4** — Editar um arquivo aplicado, rodar sync, confirmar que a edição sobreviveu (CA-030.3); publicar versão nova no servidor e confirmar que quem **não** editou recebeu (CA-030.4).
- [ ] **G-5 — Coop (Fika PVE).** Host + ao menos 1 cliente com escolhas **diferentes**: confirmar que cada um recebe o seu e que nada de gameplay compartilhado diverge (CC-7). Solo não cobre.
- [ ] **G-6** — Onboarding real em instalação limpa: gatilho dispara, modal aparece, escolhas valem no primeiro sync, aba volta pra Launcher.
- [ ] **G-7 — Quarentena não destrói config do player (CC-11 / D-14).** Ter um backup de `config-force` e então desligar um mod opcional **e** ligar performance, todos produzindo quarentena de **mesmo nome de arquivo**; confirmar que as três cópias coexistem em `config-disabled/force|optional|performance/` e que nenhuma sobrescreveu a outra.
- [ ] **G-7b — Retrocompat da quarentena (D-14).** Cliente que já tinha backups na **raiz** de `config-disabled/` (gravados pela 2.3.0) atualiza sem perdê-los: os antigos continuam lá e recuperáveis, os novos passam a cair nas subpastas.
- [ ] **G-8 — Ligar sobre config customizada (CA-030.2).** Editar um arquivo à mão, ligar o item de performance correspondente, confirmar que a versão de performance entrou em vigor **e** que a edição anterior está recuperável.
- [ ] **G-9 — Bundles e cache 3D (CC-13).** Ligar/desligar um mod opcional que traga bundles e entrar em raid: confirmar que o asset correto carrega (sem asset velho de cache nem download 3D inesperado no meio da partida).
- [ ] **G-10 — Migração do modelo antigo (CA-030.28).** Cliente que já tinha estado salvo no modelo `Opcionais/`/`offFolders` atualiza sem ficar com mod órfão instalado nem preferência perdida em silêncio.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação — 12 decisões travadas, 28 critérios de aceite, 10 corner cases (commit 6d212d2e) |
| 2026-07-19 | Guilherme | Revisão `/review-spec` — resolvida contradição CA-030.1×CA-030.3 (ligar × preservar edição); 3 critérios vagos reescritos; +8 corner cases (CC-11 a CC-18) incluindo colisão do `config-disabled/`, mods server-side e cache de bundles; +4 gates humanos; 4 trechos marcados para decisão |
| 2026-07-19 | Guilherme | Fechadas as 4 decisões pendentes (D-14 namespace por origem no `*-disabled`; D-15 mod opcional client-only com validação no servidor; D-16 ligar aplica sobre config customizada com backup; D-17 onboarding não repete se `plugins` esvaziar). CC-11 e CC-12 resolvidos, +3 regras de negócio (RN-7/RN-8 e RN-4 reescrita), +2 critérios, +2 gates. Zero marcas `<!-- review -->` pendentes |
