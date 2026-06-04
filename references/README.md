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

## Códigos do FIKA (conexão coop)

Repositórios contendo os códigos do Fika (servidor e cliente), usados para habilitar conexão cooperativa (multiplayer) no SPT.

### `fika-server/` — Fika Server (C#)
Código-fonte do lado do servidor do Fika (Server C#).
- **Upstream:** `https://github.com/project-fika/Fika-Server-CSharp.git`

### `fika-plugin/` — Fika Plugin (C#)
Código-fonte do lado do cliente do Fika (Plugin C#). Contém a pasta `Fika.Core`, recomendada para validar classes, métodos e variáveis relacionados à lógica cooperativa cliente/servidor.
- **Upstream:** `https://github.com/project-fika/Fika-Plugin.git`

### `fika-headless/` — Fika Headless (TypeScript)
Código-fonte do cliente headless do Fika (usado para clientes/bots dedicados de coop).
- **Upstream:** `https://github.com/project-fika/Fika-Headless.git`

### Como obter

```bash
cd references
git clone https://github.com/project-fika/Fika-Server-CSharp.git fika-server
git clone https://github.com/project-fika/Fika-Plugin.git fika-plugin
git clone https://github.com/project-fika/Fika-Headless.git fika-headless
```
