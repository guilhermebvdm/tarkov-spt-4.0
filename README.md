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

- `.agents/` — contexto compartilhado para AI assistants (workspace, convenções, resources, hooks)
- `.claude/` — config do Claude Code: `commands/` (slash commands), `skills/`, `settings.json`
- `design-system/` — TRL Design System (padrão visual dos editores web de mod)
- `docs/` — documentação técnica, de arquitetura e tracking da migração 3.x → 4.0
- `launcher/` — launcher TRL (Avalonia) e versões legadas
- `mods/` — mods do projeto (client C#/BepInEx, server C#/SPTarkov.Server.Core)
- `references/` — fontes read-only de verdade (Assembly EFT, server SPT, FIKA, grafos)
- `scripts/`, `tools/` — utilitários (setup de referências, inventário, gestão de itens TRL)
- `wiki/` — snapshot read-only de github.com/sp-tarkov/wiki (CC BY-NC-ND 4.0)

Ver [AGENTS.md](AGENTS.md) para o contrato completo dos agentes de IA e [WORKFLOW.md](WORKFLOW.md) para o ciclo de desenvolvimento.
