# Workspace SPT 4.0

## Versões

- **SPT:** 4.0.13
- **Tarkov:** 0.16.9
- **Game install path:** `D:\SPT\`

## Tipos de mod

### Client (C# / BepInEx)
- **Local:** `mods/client/<NomeDoMod>/`
- **Compilação:** `dotnet build` → DLL em `bin/Release/net471/`
- **Deploy:** copiar DLL para `<game-path>/BepInEx/plugins/`

### Server (C# / SPTarkov.Server.Core)
- **Local:** `mods/server/<NomeDoMod>/`
- **Compilação:** `dotnet build` (referencia NuGet `SPTarkov.Server.Core` e `SPTarkov.DI`, ambos `4.0.*`)
- **Padrão:** classes anotadas com `[Injectable]` + `IOnLoad` (lifecycle); configs via `ConfigLoader<T>` lendo JSONC
- **Deploy:** copiar pasta/DLL para `<game-path>/SPT/user/mods/`
- **Nota:** TypeScript foi o padrão em SPT 3.x — em 4.0 tudo migrou para C# (single language no server)

### Dependências
- **Local:** `mods/deps/<NomeDep>/`
- Mods que outros mods dependem (ex: BigBrain, Waypoints)

## Avisos críticos

- **SPT 3.x ≠ 4.0** — arquiteturas incompatíveis; nunca portar código diretamente
- **Game fechado** ao copiar DLLs (locks de arquivo no Windows)
- **JSONC files:** sempre usar parser AST, nunca regex (causa corrupção silenciosa)

## Tools de apoio

- **`tools/tarkov-itemdb/`** — pipeline + viewer que unifica preços de SPT + tarkov.dev + tarkov-market e permite calibração manual via UI. Lê o SPT install via env `SPT_PATH` (default `D:/SPT/SPT`). Edit pelo viewer dispara `POST /api/price` que atualiza `prices.json` + `checks.dat` (hash MD5) + log em `tools/tarkov-itemdb/logs/price-edits.jsonl`. Ver [tools/tarkov-itemdb/README.md](../tools/tarkov-itemdb/README.md).

## Env vars

| Variável | Default | Quem usa |
|---|---|---|
| `SPT_PATH` | `D:/SPT/SPT` | `tools/tarkov-itemdb/scripts/*` |
| `TARKOV_MARKET_API_KEY` | — | `tools/tarkov-itemdb/scripts/fetch-tarkov-market.js` |

## Referências

- Assembly-CSharp do jogo: `deps/Assembly-CSharp/` (read-only, quando existir)
- Onde buscar dados de itens/quests/APIs externas: [resources.md](resources.md)
- Internals do SPT 4.0 (checks.dat, LiveFleaPrices, trader assort, categorias): [tools/tarkov-itemdb/docs/spt-internals.md](../tools/tarkov-itemdb/docs/spt-internals.md)
