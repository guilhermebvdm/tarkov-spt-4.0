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
- **Commit do snapshot:** `5c7fb9f4c6dcf018d0771beb0012b9722cc43d25`
- **Data do snapshot:** 2026-05-01
- **Importado em:** 2026-05-03

## Licença e regras de uso

Conteúdo licenciado sob **[CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/)** — *SPT Wiki © 2025 by SPT Team*.

| Permitido | Proibido |
|-----------|----------|
| Ler, consultar, citar com atribuição | Modificar arquivos desta pasta |
| Distribuir cópia integral | Uso comercial |
|  | Republicar versão alterada |

**Regra prática para este repo:** **não edite arquivos dentro de `wiki/spt/`**. Se quiser anotar, comentar ou estender o conteúdo, faça em `docs/` apontando de volta para o arquivo da wiki como referência.

## Estrutura

```
wiki/spt/
├── home.md                           # Página inicial (TOC global)
├── Beginners_Guide.md                # Visão geral para iniciantes
├── system-requirements.md            # Requisitos de sistema
├── How_SPT_Works.md                  # Como o SPT funciona
├── Installation_Guide.md             # Guia de instalação
├── Manual-Install-Instructions.md    # Instalação manual
├── Updating_SPT.md                   # Atualizar SPT
├── Profiles.md                       # Profiles
├── Mod_Types.md                      # Tipos de mod (client vs server)
├── Installing_Mods.md                # Instalar mods
├── Uninstalling_Mods.md              # Desinstalar mods
├── Recommended_Mods_40.md            # Mods recomendados (4.0)
├── Performance_Tuning.md             # Ajustes de performance
├── FAQs_40.md                        # FAQ (4.0)
├── Known_EFT_Issues_40.md            # Bugs conhecidos do EFT
├── Known_SPT_Issues_40.md            # Bugs conhecidos do SPT
├── Known_Mod_Issues_40.md            # Bugs conhecidos de mods
├── 5050-method.md                    # Método 50/50 para isolar mod problemático
├── Reporting_Issues.md               # Como reportar problemas
├── Bot_Difficulties.md               # Dificuldade de bots
├── SPT_and_Commando_Bots.md          # SPT & Commando bots
├── Style_Guide.md                    # Style guide da wiki
├── how_to_contribute.md              # Como contribuir
│
├── modding/                          # 👈 Foco do nosso trabalho
│   ├── Modding_Resources.md          # Hub de recursos para modders
│   ├── tutorials/
│   │   ├── Client_Modding_Quick_Guide.md   # Quick start client (BepInEx/C#)
│   │   ├── WTT_Vol1.md                     # Tutorial WTT Volume 1
│   │   └── debug_dnSpy.md                  # Debugar com dnSpy
│   └── references/
│       ├── body-part-reference.md          # Referência de partes do corpo
│       ├── bot-types.md                    # Tipos de bot
│       ├── location-information.md         # IDs de mapas
│       ├── quest-values.md                 # Valores de quests
│       ├── skills-reference.md             # Referência de skills
│       └── trader-information.md           # IDs e info de traders
│
├── SPT_311/                          # Conteúdo legado SPT 3.11
│   ├── FAQs_311.md
│   ├── Manual-Installation-Instructions_311.md
│   └── Recommended_Mods_311.md
│
└── *.png, *.gif                      # Imagens referenciadas pelos .md
```

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
