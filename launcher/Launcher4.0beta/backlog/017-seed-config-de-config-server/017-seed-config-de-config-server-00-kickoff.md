# 017 — Preencher `config` do usuário a partir de `config-server` (seed por nome) · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** pedido do usuário (2026-07-04) · **Deps:** 007 (motor de sync)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo (regra exata do usuário)

Seed **unidirecional** da pasta de configs padrão do server para o usuário, por **nome de arquivo**, sem nunca sobrescrever:

- **Fonte:** `BepInEx/config-server/` na pasta do **server**.
- **Alvo:** `BepInEx/config/` do **usuário** que está sincronizando.
- **Regra, por arquivo em `config-server`:**
  - Se **não existir** um arquivo de **mesmo nome** em `BepInEx/config/` do usuário → **copiar** o arquivo de `config-server` para `BepInEx/config/`.
  - Se **já existir** um arquivo de mesmo nome → **não fazer nada** (o conteúdo/metadados **não** precisam ser idênticos — a checagem é **só por nome**).

Ou seja: preenche o que falta, preserva integralmente o que o usuário já tem. É um "seed de defaults" — o usuário customiza os configs livremente depois e nunca é sobrescrito.

## ⚠️ Reconciliação com o item 007 (obrigatória na spec)

O 007 implementou `config-server` como **mirror-delete** (`SyncRuleResolver` → `MirrorDelete`), e `config` (`BepInEx/config`) como **preserve-divergent** (compara com baseline por **hash**). A regra deste item **diverge** em dois pontos:

1. **Mapeamento de pasta:** aqui `config-server` (server) é **fonte** que popula `config` (usuário) — não uma pasta espelhada no lado do usuário. A spec técnica precisa definir se o 017 **substitui** a regra `config-server` do 007, coexiste, ou a reinterpreta.
2. **Critério de existência:** aqui é **por nome apenas**, não por hash/baseline. Diferente do preserve-divergent do 007 (que atualiza quando local==baseline). A spec deve deixar claro que o seed **nunca** compara conteúdo — só presença do nome.

A spec técnica **deve** decidir e registrar como as duas regras convivem (provável: o 007 `config-server` mirror-delete vira este seed-por-nome, já que o mirror-delete estava atrás de `folderRules` explícito e não é default — ver 007 CR-01-03). Reusar as primitivas do motor (`SyncPlanner`/`SyncEngine`, apply atômico, guard de path sob GameRoot) — adicionar uma estratégia nova `SeedIfMissingByName`, não um fluxo paralelo.

## Corner cases para a spec

- Subpastas dentro de `config-server` (recursivo? "mesmo nome" é por path relativo ou basename?) — **decidir e registrar**.
- Arquivo que o usuário deletou de propósito reaparece no próximo seed (é o comportamento desejado? o seed não tem memória de "já foi semeado uma vez").
- Server sem a pasta `config-server` → no-op silencioso.
- Interação com o toggle de `config-performance` (008) e com o cleanup do `GameStarter` (não deletar o que foi semeado).
