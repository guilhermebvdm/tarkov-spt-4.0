# Skills-Extended

**Versão base:** unknown · **Licença:** see LICENSE.md
**Upstream:** https://github.com/CJ-SPT/Skills-Extended @ `d7afed666f884a4816e933d09089540569203727` (branch `master`)
**Forge:** 

---

## O que é

Skills-Extended é um mod híbrido SPT 4.0.2 / EFT 0.16.x (`com.cj.SkillsExtended`) que reativa skills dormentes do vanilla (Lockpicking, Prone Movement, Silent Ops) e adiciona 4 skills novas (Usec Ar Systems, Bear Ak Systems, Usec Negotiations, Bear Raw Power), com um minigame customizado de arrombamento de fechaduras, configuração extensa via JSON/Web UI e sincronização multiplayer via Fika.

Ver a documentação técnica completa em [`docs/README.md`](docs/README.md) e o catálogo de configuração F12 em [`PROPRIEDADES.md`](PROPRIEDADES.md).

## Documentação

- [`docs/README.md`](docs/README.md) — índice da documentação técnica completa (arquitetura, sistema de skills/buffs, lockpicking, servidor/Web UI, multiplayer Fika)
- [`PROPRIEDADES.md`](PROPRIEDADES.md) — catálogo de configuração F12 (BepInEx ConfigurationManager) do lado cliente

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
diff -r mods/Skills-Extended/original/ mods/Skills-Extended/modded/
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

_Adicionado em 2026-06-07T02:17:29Z_

---

**Workflow de desenvolvimento:** ver [WORKFLOW.md](../../WORKFLOW.md).
