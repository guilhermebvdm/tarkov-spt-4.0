---
title: Wiki SPT — Base de Conhecimento (Snapshot)
date: 2026-05-03
status: 🟢 Vivo
authors: [guilhermebvdm]
---

# Wiki SPT — Base de Conhecimento

Esta pasta é um **snapshot somente-leitura** da wiki oficial do Single Player Tarkov, mantida em [github.com/sp-tarkov/wiki](https://github.com/sp-tarkov/wiki) e publicada em [wiki.sp-tarkov.com](https://wiki.sp-tarkov.com/).

Serve como referência local para desenvolver, ajustar e modificar mods do SPT 4.0 sem depender de acesso à internet ou de renderização Wiki.js.

## Origem

- **Repo upstream:** `sp-tarkov/wiki` (branch `main`)
- **Commit do snapshot:** `8e3c35040799819d58935080d711b98d58d87fe1`
- **Data do snapshot:** 2026-06-25
- **Importado em:** 2026-07-01

## Licença e regras de uso

Conteúdo licenciado sob **[CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/)** — *SPT Wiki © 2025 by SPT Team*.

| Permitido | Proibido |
|-----------|----------|
| Ler, consultar, citar com atribuição | Modificar arquivos desta pasta |
| Distribuir cópia integral | Uso comercial |
|  | Republicar versão alterada |

**Regra prática para este repo:** **não edite arquivos dentro de `wiki/spt/`**. Se quiser anotar, comentar ou estender o conteúdo, faça em `docs/` apontando de volta para o arquivo da wiki como referência.

## Estrutura

- **Raiz** — guias gerais: instalação/atualização, profiles, tipos de mod, performance, FAQ e bugs conhecidos (`*_40.md`), diagnóstico (`5050-method.md`).
- **`modding/`** 👈 foco do nosso trabalho — hub de recursos, `tutorials/` (client BepInEx/C#, dnSpy) e `references/` (IDs de bots, traders, skills, mapas, quests).
- **`SPT_311/`** — conteúdo legado do SPT 3.11.
- Imagens (`*.png`, `*.gif`), `LICENSE` e páginas `.html` acompanham os `.md`.

> A árvore completa muda a cada sync do upstream — use `ls`/busca para o inventário exato, não uma lista fixa aqui. Navegação por tarefa na tabela abaixo.

## Como sincronizar com upstream

A wiki upstream recebe updates frequentes. Para atualizar este snapshot:

```bash
bash .agents/hooks/sync-wiki.sh
```

O script:
1. Baixa o tarball atual de `sp-tarkov/wiki@main`.
2. Substitui o conteúdo de `wiki/` (exceto este `README.md`).
3. Atualiza o SHA do commit registrado acima.

Depois revise o diff (`git diff wiki/`) e faça commit em separado:

```bash
git add wiki/
git commit -m "chore(wiki): sync snapshot from sp-tarkov/wiki@<sha>"
```

## Início rápido por tarefa

| Tarefa | Comece por |
|--------|------------|
| Entender SPT 4.0 do zero | [home.md](home.md) → [How_SPT_Works.md](How_SPT_Works.md) |
| Criar mod client (C#/BepInEx) | [modding/tutorials/Client_Modding_Quick_Guide.md](modding/tutorials/Client_Modding_Quick_Guide.md) |
| Criar mod server (TypeScript) | [modding/Modding_Resources.md](modding/Modding_Resources.md) |
| Debugar mod existente | [modding/tutorials/debug_dnSpy.md](modding/tutorials/debug_dnSpy.md) |
| Procurar IDs de bot/trader/skill | [modding/references/](modding/references/) |
| Diagnosticar problema | [5050-method.md](5050-method.md) → [Known_Mod_Issues_40.md](Known_Mod_Issues_40.md) |

## Atribuição

Ao reproduzir trechos desta wiki em docs internas (`docs/`), cite no formato:

> Fonte: [SPT Wiki — &lt;Página&gt;](https://wiki.sp-tarkov.com/&lt;path&gt;) — CC BY-NC-ND 4.0
