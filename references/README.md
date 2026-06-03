# references/

Material de referência **read-only** — não é código do projeto, não é versionado quando volumoso.

## `spt-source/` — código-fonte do servidor SPT (C#)

**Gitignored** (~856 MB). Necessário para investigar a lógica interna do SPT 4.0 — usado, por exemplo, na descoberta da fórmula de preço do flea (ver [tools/tarkov-itemdb/docs/flea-override-plan.md](../tools/tarkov-itemdb/docs/flea-override-plan.md)).

### Como obter (versão pinada)

```bash
cd references
git clone https://github.com/sp-tarkov/server-csharp.git spt-source
cd spt-source
git checkout c87cc3c6853c622fd2addaf961f58467cd9754f2
git lfs pull
```

| Campo | Valor |
| --- | --- |
| Upstream | `https://github.com/sp-tarkov/server-csharp.git` |
| Branch | `main` |
| Commit pinado | `c87cc3c6853c622fd2addaf961f58467cd9754f2` |
| SPT version | 4.0.13 (`Build.props:SptVersion`) |
| Git LFS | `.lfsconfig` → `https://spt-lfs.sp-tarkov.com/sp-tarkov/server-csharp` |
| Licença | CC BY-NC-SA 4.0 (autor: Refringe) |

Esse commit corresponde ao SPT 4.0.13 instalado em `D:/SPT/SPT/`. Se atualizar o SPT, re-clonar no commit/tag da nova versão — senão a lógica diverge do runtime testado.

### Arquivos consultados com mais frequência

| Caminho dentro de `spt-source/` | Para quê |
| --- | --- |
| `Libraries/SPTarkov.Server.Core/Services/RagfairPriceService.cs` | Geração de preço de flea (`ReplaceFleaBasePrices`, quality modifier) |
| `Libraries/SPTarkov.Server.Core/Extensions/DictionaryExtensions.cs` | `AddOrUpdate` (o `+=` escondido) |
| `Libraries/SPTarkov.Server.Core/Services/PostDbLoadService.cs` | `ApplyFleaPriceOverrides`, blacklist |
| `Libraries/SPTarkov.Server.Assets/SPT_Data/database/hideout/production.json` | Receitas do hideout (craft items) |
| `Libraries/SPTarkov.Server.Assets/SPT_Data/configs/ragfair.json` | Config default do flea |
