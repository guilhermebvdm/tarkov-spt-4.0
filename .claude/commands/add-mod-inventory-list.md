# /add-mod-inventory-list

Adiciona um ou mais mods ao inventário (`mods-inventory.md` + `mods-inventory.html`) a partir de URLs do Forge.

## Uso

```
/add-mod-inventory-list <forge-url> [<forge-url2> ...]
/add-mod-inventory-list <forge-url> --instalado
```

`--instalado` — marca o mod como instalado (coluna `Instalado` = `✓` no `.md`; versionado no git).

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

Colunas: `# | Mod | Tipo | Atuação | Categoria | Escopo | Forge | Repo 3.x | Repo 4.0 | SPT 4.0? | Função | Status | Prioridade | TRL 3.0? | Instalado`

Valores padrão para mods novos:

| Coluna | Valor padrão |
|---|---|
| Tipo | `🖥️ Client` / `🌐 Server` / `🔀 Misto` |
| Atuação | Inferir com emojis do projeto (ex: `⚔️ Raid`, `🌐 Geral`, `🏚️ Hideout`). Use `🔍` só se impossível |
| Categoria | Inferir com emojis do projeto (ex: `⚖️ Balanceamento`, `🛋️ QoL`, `⚙️ Core`). Use `🔍` só se impossível |
| Escopo | Inferir com emojis do projeto (ex: `🤖 IA`, `🔫 Armas`, `🖼️ UI`). Use `🔍` só se impossível |
| Forge | `[{id}](https://forge.sp-tarkov.com/mod/{id}/{slug})` |
| Repo 3.x | `—` |
| Repo 4.0 | `[user/repo](https://github.com/user/repo)` |
| SPT 4.0? | `✅` se versão 4.0 confirmada |
| Status | `🟢 Instalar` |
| Prioridade | `🔍` |
| TRL 3.0? | `New` |
| Instalado | `—` (ou `✓` com `--instalado`) |

> ⚠️ **Nunca use `|` literal dentro de uma célula** (nem em Função, nem em nome): o `|` é o separador de colunas do markdown — um `|` extra desloca todas as colunas, faz o parser ler campos errados e deixa a linha **ineditável** pelo servidor. Reescreva (ex.: use `/` ou `·`) ou escape.

### 5. Adicionar ao markdown

Insira a nova linha no final da tabela em `## Inventário completo` do `mods-inventory.md`.

### 6. Executar o script de sincronização

```
node scripts/sync-mods-html.js
```

Confirme a saída: `Parsed N mods (starting at #1)`.

Se o script imprimir `⚠ validação:` e **sair com código ≠ 0**, há um **número duplicado** ou uma **célula com `\|`** — corrija o `.md` e rode de novo **antes** de prosseguir. Não ignore.

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

A marcação de instalado vive na coluna `Instalado` do `.md` (não há mais seeding via
localStorage). Com `--instalado`, basta a nova linha já ter `✓` nessa coluna (passo 4) —
o sync do passo 6 carrega para o campo `inst` do `const MODS`. Nada a editar no HTML.

### 10. Confirmar ao usuário

Informe:
- Nome(s) e número(s) dos mods adicionados
- Total atualizado (`N mods, 0–N-1`)
- Se pré-instalado foi aplicado
- Campos que ficaram como `🔍` e requerem revisão manual

## Regras

- Nunca duplicar um `forge_id` já presente em `const MODS`
- Cada número `n` é **único** — sempre `último_n + 1`, nunca reutilizar (o sync falha com exit ≠ 0 em número duplicado; e o servidor só edita a 1ª linha que casa com `#n`)
- Nunca usar `|` literal dentro de uma célula (quebra o parser — ver passo 4)
- O sync script substitui apenas `const MODS = [...]` — CSS, JS e layout não são tocados
- `mods-inventory.md` é a fonte de verdade; o HTML é derivado — dados vão no markdown, depois sync
- A tabela `## Inventário completo` é a **única** fonte de dados do sync (começa no mod **#1**). Não há mais bloco vertical "## Base" separado — não é preciso olhar nenhum outro trecho do `.md` para atualizar o HTML
- TRL sempre `New` para mods sem versão 3.x no inventário original
- **⚠️ CUIDADO COM ENCODING:** Nunca use comandos do PowerShell (`Add-Content`, `Set-Content`, etc) para adicionar linhas no `.md`, pois isso corrompe os emojis e o encoding UTF-8. Utilize *exclusivamente* as ferramentas nativas de manipulação de arquivo do seu ambiente (`replace_file_content`, `write_to_file`) ou scripts Node.js (`fs.writeFileSync(..., 'utf8')`).