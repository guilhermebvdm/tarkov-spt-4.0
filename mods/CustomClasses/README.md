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

Sucessor do `mods/RZCustomProfiles` (DLL black-box de terceiros, formato limitado) — aposentado no item 007 (mod autossuficiente).

## Mecanismo central

No SPT 4.0, perfis nascem de templates por **Edition+Side** (`databaseService.GetProfileTemplates()` → `Dictionary<string, ProfileSides>`). O launcher lista as edições pelas **chaves** desse dicionário. Cada classe = uma chave injetada no `PostDBModLoader`, com `Character` (skills + itens equipados/compostos nativos via `ParentId/SlotId`) e `Suits` (outfits).

Cada classe é um arquivo `.jsonc` em `modded/Server/config/classes/` — schema completo em [docs/class-schema.md](docs/class-schema.md).

## Editor web de classes (in-game server)

O mod embute um **editor web** (Blazor Server + MudBlazor) servido pelo próprio servidor SPT: com o server rodando, abra `https://<ip-do-bind>:6969/customclasses` (cert self-signed — aceite a exceção). Lista/detalhe/edição completa das classes (abas General/Skills/Multipliers/Hideout/Outfit/Equipped/Stash), criar/duplicar/deletar, custo de balanceamento ao vivo e **hot-apply** (classe salva aparece no launcher sem reiniciar o server).

Guia de uso, fluxo install↔repo (`/sync-classes` + guard do `/compile-mod`) e os **4 limites** (hot-apply só p/ perfis novos; perfis existentes imutáveis; comentários `.jsonc` perdidos no save; rename = duplicar): **[docs/class-editor.md](docs/class-editor.md)**.

## Estrutura desta pasta

> Mod **autoral** (não vendorizado) — **não há `original/`**. Todo o código fica em `modded/`.

| Pasta | Conteúdo |
|---|---|
| `modded/` | Código-fonte (`Server/`, `Client/`). `Server/` inclui o editor web (`Web/` — páginas Blazor; `wwwroot/` — estáticos/ícones) e os dados (`config/classes/*.jsonc`). `Client/icons/` — PNGs de identidade visual. |
| `docs/` | Docs canônicas: [class-schema.md](docs/class-schema.md) (schema do JSON de classe) e [class-editor.md](docs/class-editor.md) (guia do editor web). |
| `backlog/` | `mod-backlog.md` + specs/as-builts por item (`NNN-slug/`). |
| `scripts/` | Tooling de autoria: `build-class-jsons.js` (gerador **congelado** — bootstrap-only, `--force` regenera), `build-icons.mjs` (SVG→PNG p/ Client/icons e Server/wwwroot/icons), `check-skill-costs.mjs` (paridade da fórmula de custo), `sync-classes.sh` (install→repo). |
| `assets/` | Imagens, prints. |
| `builds/` | Artefatos de build. |
| `memory/` | `sessions.md` — log cronológico de sessões. |
| `mod.json` | Metadados machine-readable (inventário de mods). |

## Build

Híbrido via `.agents/scripts/compile-mod.sh` (`/compile-mod CustomClasses`): builda os projetos e instala nos 2 destinos (server → `SPT/user/mods/`, client → `BepInEx/plugins/`), incluindo `config/` (com **guard anti-clobber** — `--force-config` força repo→install) e `wwwroot/`.

- **Server:** C# .NET 9, `Microsoft.NET.Sdk.Web`, NuGet `SPTarkov.Server.Core` + `SPTarkov.Server.Web` 4.0.2 (MudBlazor transitivo).
- **Client:** BepInEx, `netstandard2.1`, DLLs do EFT/BepInEx.

## Roadmap

Itens, status e o épico do editor web (018–029) em [backlog/mod-backlog.md](backlog/mod-backlog.md).

---

_Criado em 2026-06-07._
