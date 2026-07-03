# tarkov-spt-4.0

Repositório de mods para SPT 4.0 (Single Player Tarkov) — EFT `0.16.9`.
Reúne mods migrados de SPT 3.x, mods desenvolvidos para 4.0 e perfis customizados.

## Setup

```bash
git clone https://github.com/guilhermebvdm/tarkov-spt-4.0.git
cd tarkov-spt-4.0
bash .agents/hooks/install-hooks.sh
```

Dependência opcional (recomendada): `jq` para o pre-commit hook.
- Windows: `winget install jqlang.jq`
- Linux: `apt install jq`

## Estrutura

```
.
├── .agents/               # contexto compartilhado para AI assistants
│   ├── hooks/             # scripts (validate, pre-commit, sync-wiki)
│   ├── workflows/         # workflows reutilizáveis
│   ├── conventions.md     # convenções do projeto
│   ├── resources.md       # router de fontes (wiki local, APIs, deepwiki)
│   ├── skills-backlog.md  # propostas de skills priorizadas
│   └── workspace.md       # detalhes técnicos do workspace
├── .claude/               # config do Claude Code (hooks, settings)
├── docs/                  # docs técnicas e tracking da migração 3.x → 4.0
│   └── migration/          # inventário de mods (ver migration/README.md)
│       ├── README.md
│       ├── mods-inventory.md   # fonte de verdade do inventário
│       └── mods-inventory.html # viewer gerado (sync-mods-html.js)
├── mods/                  # mods do projeto (client C#/BepInEx, server C#/SPTarkov.Server.Core)
├── wiki/                  # snapshot read-only de github.com/sp-tarkov/wiki
│                          # (CC BY-NC-ND 4.0; sync via .agents/hooks/sync-wiki.sh)
├── AGENTS.md              # contrato completo dos agentes de IA
└── README.md
```

Ver [AGENTS.md](AGENTS.md) para o contrato completo dos agentes de IA.
