# references/

Material de referência **read-only** — não é código do projeto, não é versionado quando volumoso.

## Obter as referências (máquina nova)

As fontes vendorizadas (gitignored) são descritas no inventário canônico [manifest.json](./manifest.json) e obtidas com **um comando**:

```bash
node scripts/setup-references.js           # clona o que faltar
node scripts/setup-references.js --check    # só confere o que está presente/faltando
```

O metadado canônico (upstream, commit pinado, LFS, licença) vive no `manifest.json` — **não duplicar aqui**. O script clona cada fonte conforme o `pin` (commit fixo p/ `spt-source`, branch `main` p/ os repos do FIKA), roda `git lfs pull` quando aplicável e remove o `.git` dos snapshots.

## `spt-source/` — código-fonte do servidor SPT (C#)

**Gitignored** (~856 MB). Necessário para investigar a lógica interna do SPT 4.0 — usado, por exemplo, na descoberta da fórmula de preço do flea (ver [tools/tarkov-itemdb/docs/flea-override-plan.md](../tools/tarkov-itemdb/docs/flea-override-plan.md)). Metadado (commit pinado, versão SPT, LFS): [manifest.json](./manifest.json) → `id: spt-source`.

O commit pinado corresponde ao SPT 4.0.13 instalado em `D:/SPT/SPT/`. Se atualizar o SPT, atualizar o `pin` no `manifest.json` e re-rodar o setup — senão a lógica diverge do runtime testado.

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

> Obter os três (e o `spt-source`) de uma vez: `node scripts/setup-references.js`. Upstreams e pins canônicos em [manifest.json](./manifest.json).
