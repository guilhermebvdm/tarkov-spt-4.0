# CustomClasses

**Versão base:** 0.1.0 · **SPT:** 4.0.13 · **Licença:** MIT (placeholder — confirmar) · **Autor:** mdj
**GUID:** `customclasses.mdj` (client BepInPlugin + server ModGuid)

---

## O que é

Mod **standalone, híbrido** (server C# + client BepInEx) que adiciona **classes iniciais de personagem** ao SPT 4.0. Cada classe é uma *edition* selecionável no launcher e define, via JSON:

- **Skills iniciais** (níveis estáticos / buff / debuff)
- **Itens iniciais** no stash, **equipados** no personagem e **compostos** (árvores `_id/_tpl/parentId/slotId`)
- **Multiplicadores de evolução de skill** por classe (sem distribuição dinâmica)
- **Outfits** iniciais (customization + suits)
- Compatibilidade **opcional** com mods de skill novas (ex.: Skills-Extended)

Sucessor do `mods/RZCustomProfiles` (DLL black-box de terceiros, formato limitado) — ver backlog item 007 (migração + coexistência).

## Mecanismo central

No SPT 4.0, perfis nascem de templates por **Edition+Side** (`databaseService.GetProfileTemplates()` → `Dictionary<string, ProfileSides>`). O launcher lista as edições pelas **chaves** desse dicionário. Cada classe = uma chave injetada no `PostDBModLoader`, com `Character` (skills + itens equipados/compostos nativos via `ParentId/SlotId`) e `Suits` (outfits).

## Estrutura desta pasta

> Mod **autoral** (não vendorizado) — **não há `original/`**. Todo o código fica em `modded/`.

| Pasta | Conteúdo |
|---|---|
| `modded/` | Código-fonte (`Server/`, `Client/`, `Common/` + `.sln`). |
| `backlog/` | `mod-backlog.md` + specs por item (`NNN-slug/`). |
| `scripts/` | Geradores de JSON de classe (portados/adaptados do RZCustomProfiles). |
| `assets/` | Imagens, prints. |
| `builds/` | Artefatos de build. |
| `memory/` | `sessions.md` — log cronológico de sessões. |
| `mod.json` | Metadados machine-readable (inventário de mods). |

## Build

Híbrido: requer estender `.agents/scripts/compile-mod.sh` para server-csharp + 2 destinos (server → `SPT/user/mods/`, client → `BepInEx/plugins/`). Ver backlog item 000.

- **Server:** C# .NET 9, NuGet `SPTarkov.Server.Core 4.0.x`.
- **Client:** BepInEx, `netstandard2.1`, DLLs do EFT/BepInEx.

## Roadmap

Plano aprovado: `~/.claude/plans/` (sessão 2026-06-07). Itens em [backlog/mod-backlog.md](backlog/mod-backlog.md).

---

_Criado em 2026-06-07._
