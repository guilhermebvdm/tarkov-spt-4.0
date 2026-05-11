# /add-mod-inventory-list

Adiciona um ou mais mods ao inventário (`mods-inventory.md` + `mods-inventory.html`) a partir de URLs do Forge.

## Uso

```
/add-mod-inventory-list <forge-url> [<forge-url2> ...]
/add-mod-inventory-list <forge-url> --instalado
```

`--instalado` — marca o mod como pré-instalado via localStorage na primeira abertura do HTML.

## O que fazer

### 1. Verificar duplicatas

Abra `docs/migration/mods-inventory.html` e procure cada `forge_id` dentro de `const MODS = [...]`.
Se já existir, **informe o usuário e pule esse mod** — não duplique.

### 2. Buscar dados do Forge

Use `WebFetch` em cada URL para extrair:

| Campo | Onde encontrar na página |
|---|---|
| **name** | `<h1>` da página |
| **forge_id** | número na URL: `.../mod/{id}/...` |
| **slug** | string final da URL: `.../mod/{id}/{slug}` |
| **image stem** | valor depois de `forge-static.sp-tarkov.com/mods/` na URL do `<img>` principal do mod (pode ser número ou hash longo) |
| **repo 4.0** | link GitHub/GitLab nos detalhes do mod |
| **tipo** | badge `Client` / `Server` / `Misto` |
| **spt_version** | versão SPT listada |
| **função** | resumo da função do mod em pt-BR, ≤ 120 chars |

### 3. Determinar o próximo número

Abra `docs/migration/mods-inventory.md`, encontre a última linha numerada em `## Inventário completo` e use `último_n + 1`.

### 4. Montar a linha do markdown

Colunas: `# | Mod | Tipo | Atuação | Categoria | Escopo | Forge | Repo 3.x | Repo 4.0 | SPT 4.0? | Função | Status | Prioridade | TRL 3.0?`

Valores padrão para mods novos:

| Coluna | Valor padrão |
|---|---|
| Tipo | `🖥️ Client` / `🌐 Server` / `🔀 Misto` |
| Atuação | inferir do mod, ou `🔍` |
| Categoria | inferir do mod, ou `🔍` |
| Escopo | inferir do mod, ou `🔍` |
| Forge | `[{id}](https://forge.sp-tarkov.com/mod/{id}/{slug})` |
| Repo 3.x | `—` |
| Repo 4.0 | `[user/repo](https://github.com/user/repo)` |
| SPT 4.0? | `✅` se versão 4.0 confirmada |
| Status | `🟢 Instalar` |
| Prioridade | `🔍` |
| TRL 3.0? | `New` |

### 5. Adicionar ao markdown

Insira a nova linha no final da tabela em `## Inventário completo` do `mods-inventory.md`.

### 6. Executar o script de sincronização

```
node scripts/sync-mods-html.js
```

Confirme a saída: `Parsed N mods (0–N-1) ✓`.

### 7. Atualizar `FORGE_SLUGS` no HTML

**Atenção:** o script modifica o HTML — releia o arquivo antes de editar.

Localize o objeto `FORGE_SLUGS` em `docs/migration/mods-inventory.html` e adicione:

```js
'{forge_id}': '{slug}',
```

### 8. Atualizar `FORGE_IMG` no HTML

Localize o objeto `FORGE_IMG` no mesmo arquivo e adicione:

```js
'{forge_id}': '{image_stem}',
```

O stem pode ser número (`279`) ou hash (`'yPa3Rv...'`). Se não for possível determinar, omita — o HTML exibe placeholder automaticamente.

### 9. Pré-instalar (somente com `--instalado`)

Localize o bloco de seeding no boot do HTML (array `[{n:..., key:...}]` seguido de `.forEach`).
Adicione o novo mod ao array existente:

```js
{n: N, key: 'spt4-seed-N'}
```

### 10. Confirmar ao usuário

Informe:
- Nome(s) e número(s) dos mods adicionados
- Total atualizado (`N mods, 0–N-1`)
- Se pré-instalado foi aplicado
- Campos que ficaram como `🔍` e requerem revisão manual

## Regras

- Nunca duplicar um `forge_id` já presente em `const MODS`
- O sync script substitui apenas `const MODS = [...]` — CSS, JS e layout não são tocados
- `mods-inventory.md` é a fonte de verdade; o HTML é derivado — dados vão no markdown, depois sync
- TRL sempre `New` para mods sem versão 3.x no inventário original