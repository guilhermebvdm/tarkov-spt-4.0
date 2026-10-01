---
title: Wiki SPT — Base de Conhecimento (Snapshot)
date: 2026-05-03
status: 🟢 Vivo
authors: [guilhermebvdm]
---

# Wiki SPT — Base de Conhecimento

Esta pasta é um **snapshot somente-leitura** da wiki oficial do SPT (Single Player Tushonka, antes Single Player Tarkov), mantida em [github.com/SP-Tushonka/wiki](https://github.com/SP-Tushonka/wiki) e publicada em [wiki.sp-tushonka.com](https://wiki.sp-tushonka.com/).

Serve como referência local para desenvolver, ajustar e modificar mods do SPT 4.0 sem depender de acesso à internet ou de renderização Wiki.js.

## Origem

- **Repo upstream:** `SP-Tushonka/wiki` (branch `main`) — até o snapshot de 2026-05-01 o upstream era `sp-tarkov/wiki`, hoje arquivado
- **Commit do snapshot:** `2b083a08594c2d53833855bb5fb7b1047dac8698`
- **Data do snapshot:** 2026-09-27
- **Importado em:** 2026-09-30

## Licença e regras de uso

Conteúdo licenciado sob **[CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/)** — *SPT Wiki © 2025 by SPT Team*.

| Permitido | Proibido |
|-----------|----------|
| Ler, consultar, citar com atribuição | Modificar arquivos desta pasta |
| Distribuir cópia integral | Uso comercial |
|  | Republicar versão alterada |

**Regra prática para este repo:** **não edite arquivos dentro de `wiki/spt/`**. Se quiser anotar, comentar ou estender o conteúdo, faça em `docs/` apontando de volta para o arquivo da wiki como referência.

## Estrutura

- **Raiz** — páginas de entrada (`home.md`, `Beginners_Guide.md`, `Reporting_Issues.md`, `Style_Guide.md`) e os índices de cada versão (`SPT_40.md`, `SPT_41.md`, `SPT_4x.md`, `SPT_50.md`, `modding.md`).
- **`SPT_4x/`** — guias comuns ao SPT 4.x: instalação/atualização, profiles, tipos de mod, performance, diagnóstico (`5050-method.md`).
- **`SPT_40/`** 👈 versão deste repo — FAQ e problemas conhecidos do SPT 4.0 (`*_40.md`).
- **`SPT_41/`** — FAQ, problemas conhecidos e instalação "bleeding edge" do SPT 4.1 · **`SPT_50/`** — instalação "bleeding edge" do SPT 5.0. Nenhuma das duas é a versão deste repo.
- **`modding/`** 👈 foco do nosso trabalho — hub de recursos, `tutorials/` (client BepInEx/C#, dnSpy), `references/` (IDs de bots, traders, skills, mapas, quests) e `SPT_41_Modding/` (migração de mod 4.0 → 4.1, cliente e servidor, com a tabela oficial de nomes de classe).
- **`Archived_Pending_Deletion/`** — conteúdo legado do SPT 3.11, marcado pelo upstream como candidato a remoção.
- Imagens (`*.png`, `*.gif`), `LICENSE` e páginas `.html` acompanham os `.md`.

> A árvore completa muda a cada sync do upstream — use `ls`/busca para o inventário exato, não uma lista fixa aqui. Navegação por tarefa na tabela abaixo.

## Como sincronizar com upstream

A wiki upstream recebe updates frequentes. Para atualizar este snapshot:

```bash
bash .agents/hooks/sync-wiki.sh
```

O script:
1. Baixa o tarball atual de `SP-Tushonka/wiki@main`.
2. Substitui o conteúdo de `wiki/` (exceto este `README.md`).
3. Atualiza o SHA do commit registrado acima.

Depois revise o diff (`git diff wiki/`) e faça commit em separado:

```bash
git add wiki/
git commit -m "chore(wiki): sync snapshot from SP-Tushonka/wiki@<sha>"
```

## Início rápido por tarefa

| Tarefa | Comece por |
|--------|------------|
| Entender SPT 4.0 do zero | [home.md](home.md) → [SPT_4x/How_SPT_Works.md](SPT_4x/How_SPT_Works.md) |
| Criar mod client (C#/BepInEx) | [modding/tutorials/Client_Modding_Quick_Guide.md](modding/tutorials/Client_Modding_Quick_Guide.md) |
| Criar mod server (TypeScript) | [modding/Modding_Resources.md](modding/Modding_Resources.md) |
| Debugar mod existente | [modding/tutorials/debug_dnSpy.md](modding/tutorials/debug_dnSpy.md) |
| Procurar IDs de bot/trader/skill | [modding/references/](modding/references/) |
| Diagnosticar problema | [SPT_4x/5050-method.md](SPT_4x/5050-method.md) → [SPT_40/Known_Mod_Issues_40.md](SPT_40/Known_Mod_Issues_40.md) |

## Atribuição

Ao reproduzir trechos desta wiki em docs internas (`docs/`), cite no formato:

> Fonte: [SPT Wiki — &lt;Página&gt;](https://wiki.sp-tushonka.com/&lt;path&gt;) — CC BY-NC-ND 4.0
