# tarkov-spt-4.0

Ambiente de desenvolvimento de mods da **TRL (TarkovRedLine)** para **SPT 4.0** — Single Player Tarkov, EFT `0.16.9`, jogado em coop via Fika.

Reúne num só repositório:

- **Mods próprios TRL** — TarkovRedLine, TRL-*, CustomClasses… (client C#/BepInEx e server C#/SPTarkov.Server.Core);
- **Mods migrados** de SPT 3.x e **mods de terceiros** vendorizados como referência/integração;
- O **harness de desenvolvimento assistido por IA** — workflow de backlog (slash commands), skills, memória, grafos de código e design system;
- O **launcher TRL** (Avalonia).

Contrato para agentes de IA: [AGENTS.md](AGENTS.md). Ciclo de desenvolvimento: [WORKFLOW.md](WORKFLOW.md).

## Setup

```bash
git clone https://github.com/guilhermebvdm/tarkov-spt-4.0.git
cd tarkov-spt-4.0
bash .agents/hooks/install-hooks.sh    # git pre-commit hook
node scripts/setup-references.js       # clona as referências vendorizadas (spt-source, FIKA)
cp .spt-path.example .spt-path         # define o caminho do SEU SPT (ver abaixo)
```

Dependência opcional (recomendada): `jq` para o pre-commit hook — Windows `winget install jqlang.jq` · Linux `apt install jq`.

## Caminho do SPT (`.spt-path`)

O caminho da instalação local do SPT/EFT **nunca é hardcoded** no código nem nos docs: ele fica **sempre** no arquivo **`.spt-path`** na raiz do repo (gitignored, um por máquina). Crie o seu copiando o exemplo:

```bash
cp .spt-path.example .spt-path
```

Depois edite a linha `SPT_PATH=` apontando para a sua instalação — barras normais mesmo no Windows (`D:/SPT`, não `D:\SPT`) e sem aspas. É lido pelos comandos de build/sync (`/compile-mod`, `/sync-classes`). Precedência: `$SPT_PATH` / `--spt-path` > `.spt-path` > default `D:/SPT`.

## Estrutura

- `.agents/` — contexto compartilhado para AI assistants (workspace, convenções, resources, hooks)
- `.claude/` — config do Claude Code: `commands/` (slash commands), `skills/`, `settings.json`
- `design-system/` — TRL Design System (padrão visual dos editores web de mod)
- `docs/` — documentação técnica, de arquitetura e tracking da migração 3.x → 4.0
- `launcher/` — launcher TRL (Avalonia) e versões legadas
- `mods/` — mods do projeto (client C#/BepInEx, server C#/SPTarkov.Server.Core)
- `references/` — fontes read-only de verdade (Assembly EFT, server SPT, FIKA, grafos)
- `scripts/`, `tools/` — utilitários (setup de referências, inventário, gestão de itens TRL)
- `wiki/` — snapshot read-only de github.com/sp-tarkov/wiki (CC BY-NC-ND 4.0)
