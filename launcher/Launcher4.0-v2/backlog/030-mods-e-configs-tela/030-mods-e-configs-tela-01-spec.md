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

- [ ] **CA-030.1** — Dado um arquivo em `config-performance/<rel>` e o item correspondente **ligado**, quando o sync roda, então ele é aplicado em `config/<rel>`, **vencendo** `config-force` e `config` (D-1).
- [ ] **CA-030.2** — Dado o item **desligado**, quando o sync roda, então `config/<rel>` volta a ser governado pela cadeia normal (`config-force`, senão `config`).
- [ ] **CA-030.3** — Dado que o player **editou** o arquivo aplicado, quando o sync roda, então a edição dele é **preservada** (mesma regra `preserve-divergent` do `config`).
- [ ] **CA-030.4** — Dado que o servidor publica uma versão **nova** do arquivo e o player **não** o customizou, quando o sync roda, então ele recebe a versão nova.
- [ ] **CA-030.5** — Dado um arquivo que existe **só** em `config-performance` (sem par em `config/`), quando o player desliga o item, então o arquivo é movido para `config-disabled/` (D-8), nunca apagado de forma irrecuperável.
- [ ] **CA-030.6** — A pasta `config-performance` é espelhada no cliente como **biblioteca de referência** (`mirror-reference`): sempre a versão do servidor, extras não deletados, edição local ali é sobrescrita (D-10).
- [ ] **CA-030.7** — Nem `plugins-optional.json`, nem `performance.json`, nem a pasta `config-performance/` são distribuídos como arquivos comuns de mod para o jogo do player (hoje seriam — ver §Defeito atual).

### B. Mods opcionais

- [ ] **CA-030.8** — Dado um mod marcado como opcional e **desligado** pelo player, quando o sync roda, então **todos** os `paths` dele (plugin + config + bundles) são movidos para a quarentena `*-disabled` correspondente, e nada dele é baixado (D-2).
- [ ] **CA-030.9** — Dado um mod opcional **ligado**, quando o sync roda, então todos os `paths` dele são instalados normalmente.
- [ ] **CA-030.10** — Dado um mod que o player **remarca** e cujos arquivos estão em `*-disabled`, quando ele confirma, então o launcher **restaura da quarentena se o hash bater** com o servidor; se divergir, **baixa a versão do servidor** e descarta a antiga (D-7).
- [ ] **CA-030.11** — Dado um mod opcional **novo** publicado pelo servidor e um player que **já configurou** a tela antes, quando ele loga, então o mod vem **desligado** e a tela + o item de menu exibem **marcador de novidade** até ele visitar a tela (D-6).

### C. Tela e navegação

- [ ] **CA-030.12** — A tela "Mods e Configs" é acessível pelo **menu lateral** (abaixo de "Launcher") e por um **resumo clicável** na área superior direita da tela logada.
- [ ] **CA-030.13** — O resumo mostra o estado atual de forma que convide o clique (ex.: *"5 de 7 mods opcionais ligados · Performance: 0 de 4"*) e destaca quando há novidade não vista (CA-030.11).
- [ ] **CA-030.14** — A tela tem **2 colunas** (Mods opcionais | Configs de performance), cada item com **nome + descrição** no idioma ativo, e cada coluna com um toggle **"todos"** (D-11).
- [ ] **CA-030.15** — Toda string nova (tela, modal, resumo, descrições dos JSONs) existe em **pt e en** e segue o padrão i18n vigente.

### D. Onboarding (primeiro acesso)

- [ ] **CA-030.16** — Dado um cliente **sem nenhum plugin instalado**, quando o player loga, então ele é levado direto à tela "Mods e Configs", **antes do primeiro sync** (D-4).
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
- **RN-4 — Nada sai sem quarentena.** Toda remoção (mod desligado, arquivo só-de-performance) vai para a pasta `*-disabled` correspondente, protegida do `deleteFiles` pelo guard vigente.
- **RN-5 — Metadados nunca viram conteúdo.** Os JSONs de definição e a pasta `config-performance` não podem ser sincronizados como arquivos de mod (CA-030.7).
- **RN-6 — Preferência é do player, por id.** O estado de cada item é salvo por `id` e sobrevive a atualizações; item removido do servidor some da tela sem apagar a preferência de outros.

## Corner cases

- **CC-1 — Onboarding em cliente que já tem plugins.** Player existente nunca dispara o gatilho (D-4); ele conhece a tela pelo menu/resumo e por CA-030.11.
- **CC-2 — Gatilho avaliado tarde.** Se o sync rodar antes da checagem, o cliente passa a ter plugins e o onboarding nunca acontece. A ordem de CA-030.16 (checar antes do sync) é obrigatória.
- **CC-3 — Player liga performance, edita o arquivo, desliga.** A edição prevalece (RN-3): ele fica com o arquivo customizado mesmo com o item desligado. Intencional.
- **CC-4 — Mesmo arquivo em dois itens de performance.** Precisa ser detectado na geração (dois itens ligando/desligando o mesmo arquivo criam estado ambíguo).
- **CC-5 — Mod opcional cujo `paths` inclui arquivo também presente em outro mod.** Desligar um não pode arrastar arquivo compartilhado do outro.
- **CC-6 — Arquivo em `*-disabled` de uma desativação anterior ao remarcar/desmarcar de novo.** Sobrescrever a quarentena é aceitável, mas precisa ser logado (não silencioso).
- **CC-7 — Coop (Fika PVE).** Mod opcional que altere gameplay compartilhado (loot, spawns) diverge entre players. Client-side (mira, efeitos visuais) é seguro. Marcar no conteúdo o que não pode ser opcional.
- **CC-8 — Cancelar no meio da aplicação.** Resultado parcial + relatório (CA-030.25); nunca deixar arquivo parcial (escrita atômica do motor).
- **CC-9 — JSON de definição inválido/ausente.** Tela abre sem itens daquele eixo, com aviso — nunca crash nem lista fantasma.
- **CC-10 — `id` renomeado no servidor.** Equivale a "item novo" (D-6) e o antigo some; documentar que `id` é chave estável.

## Fora de escopo

- Detecção automática de hardware para sugerir preset (o modal orienta, o player decide).
- Perfis nomeados de configuração salvos por player.
- A feature de "configurações recomendadas de jogo" (teclas/gráficos/menu) — é [outro tema, parado](../../../CLAUDE.md), com fonte e fluxo próprios.
- Redesenho das outras áreas da tela logada além do resumo (CA-030.12/13).

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
