# 017 — Seed `config` a partir de `config-server` · Spec funcional

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Kickoff:** [00-kickoff](./017-seed-config-de-config-server-00-kickoff.md) · **Dep:** motor de sync do item 007 (`SPT.Launcher.Base/Sync/`)

> Sessão autônoma (execução sequencial 017→016). Spec funcional; a técnica + reconciliação com o 007 fica na [02-spec-tech](./017-seed-config-de-config-server-02-spec-tech.md).

## Objetivo

Preencher a pasta de configs do usuário com os **defaults** do server, por **nome de arquivo**, sem nunca sobrescrever nem deletar. É um "seed de defaults": o usuário recebe o que falta e customiza livremente depois — o launcher jamais mexe no que ele já tem.

## Regra (exata, do usuário)

Para cada arquivo em `BepInEx/config-server/<rel>` da distribuição do server:

- Se **não existir** arquivo de mesmo nome em `BepInEx/config/<rel>` do usuário → **copiar**.
- Se **já existir** (conteúdo/metadados irrelevantes — a checagem é **só por nome**) → **não fazer nada**.

## Comportamento observável

| Situação | Resultado |
|---|---|
| `config/<rel>` ausente | copia o default de `config-server/<rel>` |
| `config/<rel>` presente (mesmo com conteúdo diferente) | não toca (nunca sobrescreve) |
| Subpasta `config-server/a/x.cfg` | semeia `config/a/x.cfg` (preserva a subpasta) |
| Usuário apaga um arquivo semeado | reaparece no próximo seed (o seed não tem memória) |
| Server sem pasta `config-server` | no-op silencioso |
| Arquivo semeado + outra pasta rodando mirror-delete | o semeado **não** é deletado |

## Decisões e assunções (aplicadas, não perguntadas)

- **A-017.1 — "mesmo nome" = path relativo dentro da pasta.** Preserva subpastas e o casing do trecho relativo. `config-server/a/X.cfg` → `config/a/X.cfg`.
- **A-017.2 — presença só por nome, sem hash/baseline.** A decisão de copiar olha **só** se o alvo existe no disco. Nunca compara conteúdo nem consulta o baseline. Ainda respeita o apply atômico (`.sync-tmp` + move) e o guard de path sob o GameRoot do 007.
- **A-017.3 — seed sem memória.** Não grava baseline para o arquivo semeado. Consequência intencional: arquivo apagado pelo usuário reaparece no próximo seed.
- **A-017.4 — non-destrutivo.** O seed **nunca** deleta e **nunca** sobrescreve. Por ser o oposto do mirror-delete, é seguro como default (fallback embutido do client), diferente do mirror-delete do 007 que ficou atrás de `folderRules` explícito.
- **A-017.5 — contrato de operação.** Os defaults de seed ficam em `mods_repo/BepInEx/config-server/` no server (entram no manifesto pelo scan existente do `mods_repo` e baixam pelo `/launcher/mods/download`).

## Fora de escopo

- UI nova dedicada (o seed roda dentro da verificação de arquivos existente; a lista da `ModUpdateView` ganha o ícone 🌱).
- Reconciliar defaults já customizados (isso é justamente o que o seed **não** faz — preserva).

## Gates

`dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` · `dotnet build TarkovRedLine.Server.csproj -c Release` — verdes. Nunca rodar o exe.
