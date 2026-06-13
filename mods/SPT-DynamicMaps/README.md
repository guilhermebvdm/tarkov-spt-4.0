# SPT-DynamicMaps

**Versão base:** unknown · **Licença:** MIT
**Upstream:** https://github.com/mpstark/SPT-DynamicMaps @ `b6d8bf85ecc231af4c61d9ed6649d9a5cf241fde` (branch `main`)
**Forge:** 

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
diff -r mods/SPT-DynamicMaps/original/ mods/SPT-DynamicMaps/modded/
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

_Adicionado em 2026-05-10T01:13:35Z_

---

**Workflow de desenvolvimento:** ver [WORKFLOW.md](../../WORKFLOW.md).
