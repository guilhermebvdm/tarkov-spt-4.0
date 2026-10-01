# Fontes e referências para agentes de IA

Guia de "onde buscar o quê" ao analisar mods, planejar features ou responder dúvidas técnicas sobre SPT 4.0 / EFT 0.16.9.

## Hierarquia de consulta (geral)

Ordem ao **responder dúvidas** sobre SPT/EFT:

1. **Repo local** — código dos mods em `mods/`, docs em `docs/`
2. **Wiki sincronizada** — `wiki/spt/` (snapshot read-only do upstream)
3. **Fontes externas** — APIs, DBs, deepwiki, Discord (quando o repo e a wiki não cobrem)

> ⚠️ `wiki/spt/` é sincronizada de github.com/SP-Tushonka/wiki (organização nova do projeto, renomeado para Single Player Tushonka; o repositório antigo `sp-tarkov/wiki` está arquivado) via [.agents/hooks/sync-wiki.sh](hooks/sync-wiki.sh) (CC BY-NC-ND 4.0). **Nunca editar** arquivos dentro de `wiki/` — mudanças serão sobrescritas no próximo sync.

## Hierarquia de evidência (spec/review técnicas)

**Fonte de verdade canônica** desta ordem — os demais arquivos (AGENTS.md, slash commands, skills) apontam para cá. Ao **citar evidência** numa spec técnica ou code review, toda assinatura, fórmula ou ponto de patch vem com `arquivo.cs:linha`.

1. 🥇 **Assembly descompilado (cliente EFT)** — [references/eft-decompiled/Assembly-CSharp/](../references/eft-decompiled/Assembly-CSharp/). Fonte de verdade do **cliente**. Completo (8.683 tipos, 0 namespaces vazios) mas **gitignored** — se os `.cs` não estiverem em disco, gere com `bash scripts/decompile-eft.sh`. **Existência de um tipo confere-se no [types-index.json](../references/eft-decompiled/types-index.json) (versionado), nunca num `grep` vazio** (AP-09).
2. 🥇 **Código-fonte do servidor SPT** — [references/spt-source/](../references/spt-source/) (gitignored, ~856 MB — obter via [references/README.md](../references/README.md)). Fonte de verdade do **servidor** (serviços, helpers, fórmulas, rotas).
3. 🥇 **Códigos do FIKA (coop)** — `references/fika-server/` (servidor), `references/fika-plugin/` (cliente, contém `Fika.Core`), `references/fika-headless/` (headless). Fonte de verdade da lógica cooperativa multiplayer.
4. 🥈 **Código do mod** — `mods/<mod>/original/` (upstream intocado) e `mods/<mod>/modded/` (fork local).
5. 🥉 **Wiki SPT** — [wiki/spt/](../wiki/spt/) para install/modding/server APIs.
6. 🪛 **Web** — último recurso. Marcar `[fonte externa]`.

> Distinta da **Hierarquia de consulta (geral)** acima: aquela é pra responder dúvidas; esta é pra **citar evidência** em specs/reviews.

> ⚠️ **Erros recorrentes já cometidos neste repo:** [docs/technical/spt-antipatterns.md](../docs/technical/spt-antipatterns.md) (`AP-NN`) — leitura obrigatória antes de escrever ou revisar spec técnica.

> 🧭 **Navegação estrutural:** os grafos de código do graphify ([references/graphs/](../references/graphs/), skill `graph-code-navigation`) aceleram achar callers, overrides e cadeias input→efeito — mas **NÃO são fonte de evidência**: todo achado do grafo é confirmado lendo o `arquivo.cs:linha`.

> 🗺️ **Deofuscação de nomes (SPT 4.0→4.1):** [docs/files-from-4.1/consolidated-mappings.txt](../docs/files-from-4.1/consolidated-mappings.txt) traduz o nome ofuscado do decompile para o conceito legível — 1 linha por tipo, `LEFT -> RIGHT` (ex.: `grep '^GClass680 -> '` → `ABotProfileCreator`). É **aid de compreensão, não evidência** — mesmo status do grafo ("aponta, não prova").
>
> **O SPT 4.1 foi lançado e a tabela oficial existe:** [wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md](../wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md) (tabela Markdown ``| `nome 4.0` | `nome 4.1` |``). Comparação medida em 2026-09-30 contra o snapshot `2b083a0` da wiki: cada tabela tem **5.961 pares**; **5.961 idênticos**, **0** só no `consolidated-mappings.txt`, **0** só na tabela oficial, **0** divergentes (mesma esquerda com direita diferente) — e na mesma ordem. As duas são o mesmo conteúdo em dois formatos; o `consolidated-mappings.txt` continua sendo a superfície de `grep` (1 linha por tipo, sem crases), e `docs/files-from-4.1/` fica como está. Como este repo segue em **SPT 4.0.13**, o lançamento não muda o status: nome 4.1 aponta o conceito, quem prova é o decompile do 4.0.
>
> Três regras ao usar:
> 1. **Esquerda = nome 4.0** (verificável no decompile/grafo, é o que decodifica) · **direita = nome 4.1** (chegou por fonte comunitária e hoje é igual ao nome publicado na tabela oficial — aponta o conceito, não prova assinatura). Desde 2026-07-19 esses aliases já vêm **injetados no topo de cada `.cs`** e no `types-index.json` (4.763 tipos), então `grep "<conceito>"` no dump costuma resolver sem consultar a tabela. ⚠️ O grafo indexa AST e **não** contém os aliases — busca por conceito passa pelo índice/grep, não pelo `query_graph`.
> 2. **Sem entrada ≠ não existe** (ex.: `GClass898`/`GClass3008`, usados no repo, não estão no mapa — nem na tabela oficial). Não concluir "não existe" a partir de uma ausência.
> 3. Cobre **tipos**, não **membros** (`method_5`, `_player`, `float_3`) nem inner ofuscado. Contexto/proveniência/ressalvas: [spt4-vs-spt41-gclass-deobfuscation.md](../docs/technical/spt4-vs-spt41-gclass-deobfuscation.md).

## Mapa rápido por tipo de dúvida

| Tipo de informação | Onde buscar primeiro | Fallback externo |
|---|---|---|
| Modding (entry point, links) | [wiki/spt/modding/Modding_Resources.md](../wiki/spt/modding/Modding_Resources.md) | SPT Discord #mods-development |
| **Criar mod SPT 4.0 (server + client)** | **[docs/technical/spt4-mod-creation.md](../docs/technical/spt4-mod-creation.md)** | server-mod-examples · deepwiki |
| **Migrar mod 3.x → 4.0** | **[docs/technical/spt3-to-spt4-mod-migration.md](../docs/technical/spt3-to-spt4-mod-migration.md)** | — |
| **Portar mod 4.1 → 4.0 (downgrade)** | **[docs/technical/spt41-to-spt4-mod-downgrade.md](../docs/technical/spt41-to-spt4-mod-downgrade.md)** — de-para, categorias que o compilador não acusa, execução dos patches fora do jogo; caso real em [mods/ORBIT-2.1/](../mods/ORBIT-2.1/) | — |
| **Migrar mod 4.0 ↔ 4.1 (cliente e servidor)** | Wiki oficial, pasta [wiki/spt/modding/SPT_41_Modding/](../wiki/spt/modding/SPT_41_Modding/) (páginas escritas no sentido 4.0 → 4.1): cliente — [Client_40_to_41.md](../wiki/spt/modding/SPT_41_Modding/Client_40_to_41.md) · [client/Class_Name_Mappings.md](../wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md); servidor — [Server_40_to_41.md](../wiki/spt/modding/SPT_41_Modding/Server_40_to_41.md) · [Server_413_Changes.md](../wiki/spt/modding/SPT_41_Modding/Server_413_Changes.md) · [server/Mod_Web_Pages.md](../wiki/spt/modding/SPT_41_Modding/server/Mod_Web_Pages.md); os dois — [414_Changes.md](../wiki/spt/modding/SPT_41_Modding/414_Changes.md) · [EnumExtensions.md](../wiki/spt/modding/SPT_41_Modding/EnumExtensions.md) | wiki.sp-tushonka.com |
| IDs de traders | [wiki/spt/modding/references/trader-information.md](../wiki/spt/modding/references/trader-information.md) | db.sp-tushonka.com |
| Estrutura de quests | [wiki/spt/modding/references/quest-values.md](../wiki/spt/modding/references/quest-values.md) | tarkov.dev API |
| IDs de mapas / locations | [wiki/spt/modding/references/location-information.md](../wiki/spt/modding/references/location-information.md) | tarkov.dev / Tarkynator |
| Tipos de bot (Scav/PMC/bosses) | [wiki/spt/modding/references/bot-types.md](../wiki/spt/modding/references/bot-types.md) | — |
| Skills / body parts | [wiki/spt/modding/references/skills-reference.md](../wiki/spt/modding/references/skills-reference.md) · [body-part-reference.md](../wiki/spt/modding/references/body-part-reference.md) | — |
| Tutorial client mod (BepInEx/Harmony) | [wiki/spt/modding/tutorials/Client_Modding_Quick_Guide.md](../wiki/spt/modding/tutorials/Client_Modding_Quick_Guide.md) | docs.bepinex.dev · harmony.pardeike.net |
| Debug do client | [wiki/spt/modding/tutorials/debug_dnSpy.md](../wiki/spt/modding/tutorials/debug_dnSpy.md) | — |
| Criação de itens (SDK/WTT) | [wiki/spt/modding/tutorials/WTT_Vol1.md](../wiki/spt/modding/tutorials/WTT_Vol1.md) | WTT Discord |
| FAQ / problemas comuns SPT 4.0 | [wiki/spt/SPT_40/FAQs_40.md](../wiki/spt/SPT_40/FAQs_40.md) | SPT Discord #support |
| Mods recomendados (curadoria) | _(a wiki não cobre mais: `Recommended_Mods_40.md` não existe no snapshot de 2026-09-27; só sobrou a lista do 3.11 em `Archived_Pending_Deletion/`)_ | sp-mod.com/mods · sp-mod.com/lists |
| Inventário de mods deste repo | [docs/migration/mods-inventory.md](../docs/migration/mods-inventory.md) · [README](../docs/migration/README.md) | — |
| **Item lookup por ID/nome** | _(wiki não cobre)_ | **db.sp-tushonka.com** |
| **Preços / economia / flea (EFT live)** | _(wiki não cobre)_ | **tarkov-market.com/dev/api** (PVP/PVE) · api.tarkov.dev |
| **Mecânicas EFT vivas (loot, hideout, weapon mods)** | _(wiki não cobre)_ | **tarkov.dev · Tarkynator** |
| **Código-fonte do servidor SPT** | **[references/spt-source/](../references/spt-source/)** (vendorizado, read-only) | deepwiki.com/sp-tarkov/server-csharp |
| **Erros recorrentes deste repo (antipatterns)** | **[docs/technical/spt-antipatterns.md](../docs/technical/spt-antipatterns.md)** | — |
| **Quem chama X / overrides de Y / cadeia A→B** | **MCP graphify + [references/graphs/](../references/graphs/)** (skill `graph-code-navigation`) | Grep manual com a mesma disciplina |
| **Memory leak / OOM / headless reinicia / consumo de RAM** | **skill `spt-memory-leak-analysis`** + command `/analyze-memory-leak` · [wiki/spt/SPT_4x/Performance_Tuning.md](../wiki/spt/SPT_4x/Performance_Tuning.md) · `references/fika-headless/Fika.Headless/FikaHeadlessPlugin.cs` (GC/restart nativos) | deepwiki server-csharp · Fika gitbook (headless-client) |
| **O que é este `GClassNNNN` / nome 4.1 de um tipo EFT** | **[docs/files-from-4.1/consolidated-mappings.txt](../docs/files-from-4.1/consolidated-mappings.txt)** (grep `^GClass680 -> `) — aid, não evidência · mesmos pares na tabela oficial [Class_Name_Mappings.md](../wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md) | `ilspycmd -t <FQN>` na DLL real |
| **Mods publicados / instalador** | — | **sp-mod.com** (Forge; instalador em sp-mod.com/installer) |

## Fontes externas — quando usar cada uma

- **[db.sp-tushonka.com](https://db.sp-tushonka.com/)** — lookup de IDs de itens; use sempre que precisar de `_id` para configs/quests.
- **[api.tarkov.dev](https://api.tarkov.dev/)** — GraphQL ao vivo do EFT (preços, traders, quests, loot, mapas). Lembre que reflete EFT **online**, não SPT — checar se o build do SPT está alinhado.
- **[tarkov-market.com/dev/api](https://tarkov-market.com/dev/api)** — preços do flea ao vivo, suporta **PVP e PVE** separadamente (no projeto usamos mais o **PVE**). Requer header `x-api-key: slnpflSLOoYTJJG4` (token público de uso, sem ações sensíveis). Mesma ressalva da api.tarkov.dev: reflete EFT online, não SPT.
- **[tarkynator.com](https://tarkynator.com/)** — busca rápida de itens e dados; útil quando a UI do db.sp-tushonka.com pesa.
- **[references/spt-source/](../references/spt-source/)** — código-fonte C# do servidor SPT vendorizado no repo (read-only). **Primeira parada** para lógica de servidor: serviços, helpers, fórmulas, rotas. Citar com `arquivo.cs:linha`. Ver [VENDORED.md](../references/spt-source/VENDORED.md) para commit/versão.
- **[deepwiki.com/sp-tarkov/server-csharp](https://deepwiki.com/sp-tarkov/server-csharp/1-overview)** — documentação técnica gerada do server C# (SPT 4.0). Útil para visão arquitetural de alto nível quando o código fonte vendorizado for denso demais.
- **[sp-mod.com](https://sp-mod.com/)** — Forge, o repositório oficial de mods; consultar para versão atual, downloads, deps. Substitui `forge.sp-tarkov.com`, que está fora do ar; o caminho `/mod/<id>/<slug>` é o mesmo.
- **[github.com/SP-Tushonka](https://github.com/SP-Tushonka/)** — código-fonte oficial; em especial [server-mod-examples](https://github.com/SP-Tushonka/server-mod-examples) (a `main` dos exemplos está em SPT 4.1.3 com pacotes `SPTushonka.*`, commit `c0adf95` de 2026-08-21 — conferir contra `references/spt-source/` antes de copiar para um mod 4.0.13). Substitui a organização `sp-tarkov`, cujos repositórios `wiki`, `server-csharp` e `server-mod-examples` estão arquivados. ⚠️ Este repo é SPT **4.0.13**: em `SP-Tushonka/server-csharp` o código dessa versão está na tag `4.0.13`; há tags mais novas, como `4.1.6` e `5.0.0-BEM-20260914`. A partir do SPT 4.1.3 os pacotes NuGet do servidor se chamam `SPTushonka.*` — aqui continuam `SPTarkov.*`.
- **[docs.bepinex.dev](https://docs.bepinex.dev/)** — framework de mods client (C#). Consultar para hooks, plugin lifecycle, configs.
- **[harmony.pardeike.net](https://harmony.pardeike.net/)** — patching de IL para mods client.
- **[SPT Discord](https://discord.sp-tushonka.com/)** — canais `#mods-development` e `#mods-resources` quando docs falham.

## Páginas-chave da wiki

Páginas da wiki que ensinam **como fazer** (não só tabelas de IDs). Leitura recomendada antes de iniciar qualquer mod ou dar suporte:

| Arquivo | O que oferece |
|---|---|
| [wiki/spt/SPT_4x/Updating_SPT.md](../wiki/spt/SPT_4x/Updating_SPT.md) | Semver oficial: **major/minor quebram todos os mods**, só patch (Z) preserva compat |
| [wiki/spt/SPT_4x/Mod_Types.md](../wiki/spt/SPT_4x/Mod_Types.md) | Estrutura: server (C# em `/SPT/user/mods/`) vs client (BepInEx em `/BepInEx/plugins/`) + safety profile |
| [wiki/spt/SPT_40/Known_Mod_Issues_40.md](../wiki/spt/SPT_40/Known_Mod_Issues_40.md) | Pitfalls de instalação; aponta para o "50/50 method" ([wiki/spt/SPT_4x/5050-method.md](../wiki/spt/SPT_4x/5050-method.md)) pra isolar mod ruim |
| [wiki/spt/SPT_40/Known_SPT_Issues_40.md](../wiki/spt/SPT_40/Known_SPT_Issues_40.md) | Bugs do server (caractere especial no nome do PC, runtimes .NET 9 — ASP.NET Core e Desktop — na versão mais recente) |
| [wiki/spt/SPT_40/FAQs_40.md](../wiki/spt/SPT_40/FAQs_40.md) | Resposta oficial: **nenhum mod 3.11 é compatível com 4.0**; profile sem mod migra |
| [wiki/spt/SPT_4x/How_SPT_Works.md](../wiki/spt/SPT_4x/How_SPT_Works.md) | Diferença interna 3.x vs 4.0 (DLLs soltas em `/SPT/`, não empacotadas) |
| [wiki/spt/Style_Guide.md](../wiki/spt/Style_Guide.md) | Padrão de docs (markdown, paths, version disclaimer) |

> **Gap importante:** a wiki **não** tem tutorial de server mod 4.0 nem doc de `[Injectable]` / `IOnLoad` / `ConfigLoader<T>`. Guias internos preenchem isso: [docs/technical/spt4-mod-creation.md](../docs/technical/spt4-mod-creation.md) e [spt3-to-spt4-mod-migration.md](../docs/technical/spt3-to-spt4-mod-migration.md). Para detalhes além deles, use **deepwiki** + [github.com/SP-Tushonka/server-mod-examples](https://github.com/SP-Tushonka/server-mod-examples) + código real em [mods/RZ-SPTMods/](../mods/RZ-SPTMods/).

## Regras ao usar fontes externas

1. **Cite a fonte** ao trazer dado externo para uma resposta ou commit message (ex: "ID via db.sp-tushonka.com").
2. **Confronte versão** — `api.tarkov.dev` e wikis comunitárias refletem EFT atual; SPT pode estar atrás. Em conflito, o que está no `Assembly-CSharp` do build instalado vence.
3. **Não copie texto da wiki upstream** para `docs/` (licença CC BY-NC-ND 4.0). Linke em vez de copiar.
4. **Prefira referências internas** (`wiki/spt/modding/references/*`) antes de buscar externamente — são versionadas e offline.
5. **Para arquitetura do server**, use **deepwiki** antes de ler o código bruto — economiza contexto.
