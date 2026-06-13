# SkillDistribution

**Versão base:** $(AssemblyVersion) · **Licença:** GPL-3.0
**Upstream:** https://github.com/danx91/SkillDistribution @ `b8fce545f91142568e5a95aa5b6b361d786823de` (branch `master`)
**Forge:** https://forge.sp-tarkov.com/mod/2144/skill-distribution

---

## O que é

(TODO: descrever o mod em 1-2 parágrafos)

## Estrutura desta pasta

| Pasta | Conteúdo |
|---|---|
| `original/` | Clone do repositório oficial, sem `.git`. **Não modificar.** Referência intocada usada para diff e atualizações. |
| `modded/` | Cópia de trabalho. Modificações vão aqui. |
| `assets/` | Imagens, prints, documentação externa. |
| `backlog/` | Ideias, bugs, próximos passos. |
| `builds/` | Builds geradas para distribuição. |
| `scripts/` | Scripts auxiliares específicos deste mod. |
| `mod.json` | Metadados machine-readable (alimenta o inventário de mods). |

## Comparar modificações com o original

```bash
diff -r mods/SkillDistribution/original/ mods/SkillDistribution/modded/
```

## Atualizar do upstream

Reclonar o repositório oficial e sobrescrever `original/` (sem tocar em `modded/`):

```bash
# TODO: criar /update-mod
```

Após atualizar, o diff acima mostrará suas modificações + drift do upstream.

## Build

(TODO: documentar processo de build — geralmente em `scripts/build.sh` gerando artefato em `builds/`)

---

_Adicionado em 2026-06-07T01:46:32Z_

---

**Workflow de desenvolvimento:** ver [WORKFLOW.md](../../WORKFLOW.md).
