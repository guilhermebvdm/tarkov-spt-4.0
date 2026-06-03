# Fontes e referências para agentes de IA

Guia de "onde buscar o quê" ao analisar mods, planejar features ou responder dúvidas técnicas sobre SPT 4.0 / EFT 0.16.9.

## Hierarquia de consulta (use nesta ordem)

1. **Repo local** — código dos mods em `mods/`, docs em `docs/`
2. **Wiki sincronizada** — `wiki/spt/` (snapshot read-only do upstream)
3. **Fontes externas** — APIs, DBs, deepwiki, Discord (quando o repo e a wiki não cobrem)

> ⚠️ `wiki/spt/` é sincronizada de github.com/sp-tarkov/wiki via [.agents/hooks/sync-wiki.sh](hooks/sync-wiki.sh) (CC BY-NC-ND 4.0). **Nunca editar** arquivos dentro de `wiki/` — mudanças serão sobrescritas no próximo sync.

## Mapa rápido por tipo de dúvida

| Tipo de informação | Onde buscar primeiro | Fallback externo |
|---|---|---|
| Modding (entry point, links) | [wiki/spt/modding/Modding_Resources.md](../wiki/spt/modding/Modding_Resources.md) | SPT Discord #mods-development |
| IDs de traders | [wiki/spt/modding/references/trader-information.md](../wiki/spt/modding/references/trader-information.md) | db.sp-tarkov.com |
| Estrutura de quests | [wiki/spt/modding/references/quest-values.md](../wiki/spt/modding/references/quest-values.md) | tarkov.dev API |
| IDs de mapas / locations | [wiki/spt/modding/references/location-information.md](../wiki/spt/modding/references/location-information.md) | tarkov.dev / Tarkynator |
| Tipos de bot (Scav/PMC/bosses) | [wiki/spt/modding/references/bot-types.md](../wiki/spt/modding/references/bot-types.md) | — |
| Skills / body parts | [wiki/spt/modding/references/skills-reference.md](../wiki/spt/modding/references/skills-reference.md) · [body-part-reference.md](../wiki/spt/modding/references/body-part-reference.md) | — |
| Tutorial client mod (BepInEx/Harmony) | [wiki/spt/modding/tutorials/Client_Modding_Quick_Guide.md](../wiki/spt/modding/tutorials/Client_Modding_Quick_Guide.md) | docs.bepinex.dev · harmony.pardeike.net |
| Debug do client | [wiki/spt/modding/tutorials/debug_dnSpy.md](../wiki/spt/modding/tutorials/debug_dnSpy.md) | — |
| Criação de itens (SDK/WTT) | [wiki/spt/modding/tutorials/WTT_Vol1.md](../wiki/spt/modding/tutorials/WTT_Vol1.md) | WTT Discord |
| FAQ / problemas comuns SPT 4.0 | [wiki/spt/FAQs_40.md](../wiki/spt/FAQs_40.md) | SPT Discord #support |
| Mods recomendados (curadoria) | [wiki/spt/Recommended_Mods_40.md](../wiki/spt/Recommended_Mods_40.md) | forge.sp-tarkov.com |
| Inventário de mods deste repo | [docs/migration/mods-inventory.md](../docs/migration/mods-inventory.md) · [README](../docs/migration/README.md) | — |
| **Item lookup por ID/nome** | _(wiki não cobre)_ | **db.sp-tarkov.com/search** |
| **Preços / economia / flea (EFT live)** | _(wiki não cobre)_ | **tarkov-market.com/dev/api** (PVP/PVE) · api.tarkov.dev |
| **Mecânicas EFT vivas (loot, hideout, weapon mods)** | _(wiki não cobre)_ | **tarkov.dev · Tarkynator** |
| **Código-fonte do servidor SPT** | **[references/spt-source/](../references/spt-source/)** (vendorizado, read-only) | deepwiki.com/sp-tarkov/server-csharp |
| **Mods publicados / instalador** | — | **forge.sp-tarkov.com** |

## Fontes externas — quando usar cada uma

- **[db.sp-tarkov.com/search](https://db.sp-tarkov.com/search)** — lookup de IDs de itens; use sempre que precisar de `_id` para configs/quests.
- **[api.tarkov.dev](https://api.tarkov.dev/)** — GraphQL ao vivo do EFT (preços, traders, quests, loot, mapas). Lembre que reflete EFT **online**, não SPT — checar se o build do SPT está alinhado.
- **[tarkov-market.com/dev/api](https://tarkov-market.com/dev/api)** — preços do flea ao vivo, suporta **PVP e PVE** separadamente (no projeto usamos mais o **PVE**). Requer header `x-api-key: slnpflSLOoYTJJG4` (token público de uso, sem ações sensíveis). Mesma ressalva da api.tarkov.dev: reflete EFT online, não SPT.
- **[tarkynator.com](https://tarkynator.com/)** — busca rápida de itens e dados; útil quando a UI do db.sp-tarkov.com pesa.
- **[references/spt-source/](../references/spt-source/)** — código-fonte C# do servidor SPT vendorizado no repo (read-only). **Primeira parada** para lógica de servidor: serviços, helpers, fórmulas, rotas. Citar com `arquivo.cs:linha`. Ver [VENDORED.md](../references/spt-source/VENDORED.md) para commit/versão.
- **[deepwiki.com/sp-tarkov/server-csharp](https://deepwiki.com/sp-tarkov/server-csharp/1-overview)** — documentação técnica gerada do server C# (SPT 4.0). Útil para visão arquitetural de alto nível quando o código fonte vendorizado for denso demais.
- **[forge.sp-tarkov.com](https://forge.sp-tarkov.com/)** — repositório oficial de mods; consultar para versão atual, downloads, deps.
- **[github.com/sp-tarkov](https://github.com/sp-tarkov/)** — código-fonte oficial; em especial [server-mod-examples](https://github.com/sp-tarkov/server-mod-examples).
- **[docs.bepinex.dev](https://docs.bepinex.dev/)** — framework de mods client (C#). Consultar para hooks, plugin lifecycle, configs.
- **[harmony.pardeike.net](https://harmony.pardeike.net/)** — patching de IL para mods client.
- **[SPT Discord](http://discord.sp-tarkov.com/)** — canais `#mods-development` e `#mods-resources` quando docs falham.

## Boas práticas e dicas (wiki upstream)

Páginas da wiki que ensinam **como fazer** (não só tabelas de IDs). Leitura recomendada antes de iniciar qualquer mod ou dar suporte:

| Arquivo | O que oferece |
|---|---|
| [wiki/spt/Updating_SPT.md](../wiki/spt/Updating_SPT.md) | Semver oficial: **major/minor quebram todos os mods**, só patch (Z) preserva compat |
| [wiki/spt/Mod_Types.md](../wiki/spt/Mod_Types.md) | Estrutura: server (C# em `/SPT/user/mods/`) vs client (BepInEx em `/BepInEx/plugins/`) + safety profile |
| [wiki/spt/Known_Mod_Issues_40.md](../wiki/spt/Known_Mod_Issues_40.md) | Pitfalls de instalação, "50/50 method" pra isolar mod ruim |
| [wiki/spt/Known_SPT_Issues_40.md](../wiki/spt/Known_SPT_Issues_40.md) | Bugs do server (unicode no PC name, `.NET 9.0.15` obrigatório) |
| [wiki/spt/FAQs_40.md](../wiki/spt/FAQs_40.md) | Resposta oficial: **nenhum mod 3.11 é compatível com 4.0**; profile sem mod migra |
| [wiki/spt/How_SPT_Works.md](../wiki/spt/How_SPT_Works.md) | Diferença interna 3.x vs 4.0 (DLLs soltas em `/SPT/`, não empacotadas) |
| [wiki/spt/Style_Guide.md](../wiki/spt/Style_Guide.md) | Padrão de docs (markdown, paths, version disclaimer) |
| [wiki/spt/Recommended_Mods_40.md](../wiki/spt/Recommended_Mods_40.md) | Curadoria oficial — referência ao decidir "vale criar do zero ou usar existente?" |

> **Gap importante:** a wiki **não** tem tutorial de server mod 4.0 nem doc de `[Injectable]` / `IOnLoad` / `ConfigLoader<T>`. Para isso use **deepwiki** + [github.com/sp-tarkov/server-mod-examples](https://github.com/sp-tarkov/server-mod-examples) + código real em [mods/RZ-SPTMods/](../mods/RZ-SPTMods/).

## Boas práticas ao consultar fontes externas

1. **Cite a fonte** ao trazer dado externo para uma resposta ou commit message (ex: "ID via db.sp-tarkov.com").
2. **Confronte versão** — `api.tarkov.dev` e wikis comunitárias refletem EFT atual; SPT pode estar atrás. Em conflito, o que está no `Assembly-CSharp` do build instalado vence.
3. **Não copie texto da wiki upstream** para `docs/` (licença CC BY-NC-ND 4.0). Linke em vez de copiar.
4. **Prefira referências internas** (`wiki/spt/modding/references/*`) antes de buscar externamente — são versionadas e offline.
5. **Para arquitetura do server**, use **deepwiki** antes de ler o código bruto — economiza contexto.
