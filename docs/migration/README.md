# Inventário de mods — SPT 4.0

Inventário dos mods considerados para a stack do SPT 4.0 (migração 3.x → 4.0): status,
categoria, escopo, links do Forge/repo e quais estão instalados. Serve como o "banco de
dados" do projeto sobre mods.

## Arquivos

| Arquivo | Papel |
|---|---|
| [`mods-inventory.md`](mods-inventory.md) | **Fonte de verdade.** Tabela editável por humanos + versionada no git. |
| [`mods-inventory.html`](mods-inventory.html) | **Viewer interativo gerado** a partir do `.md` (filtros, busca, stats). Não editar à mão — é regenerado pelo sync. |
| [`../../scripts/sync-mods-html.js`](../../scripts/sync-mods-html.js) | Lê o `.md` e injeta o bloco `const MODS = [...]` no `.html`. |
| [`../../scripts/serve-inventory.js`](../../scripts/serve-inventory.js) | Servidor local que torna o `.html` editável (grava de volta no `.md`). |
| [`.archived/`](.archived/) | Arquivos obsoletos mantidos por histórico (ex.: `new-mods.md`, absorvido no inventário). |

## Como os dados fluem

```
mods-inventory.md  ──(node scripts/sync-mods-html.js)──>  mods-inventory.html
   (fonte, git)                                              (gerado)
```

O `.md` é a única fonte. O `.html` é derivado: qualquer alteração de dados vai no `.md`,
depois roda o sync. O `git` é a camada de sincronização entre máquinas/editores.

## Visualizar (somente leitura)

Abra `mods-inventory.html` direto no navegador (duplo-clique / `file://`). Funciona
offline, mas os toggles ficam em **modo preview**: alterações não são salvas (o navegador
não escreve em disco sob `file://`). O badge no canto inferior direito mostra
`○ file:// — preview (não salva)`.

## Editar localmente (servidor)

Para que clicar na interface **grave no `.md`**, rode o servidor (Node puro, sem `npm install`):

```bash
node scripts/serve-inventory.js          # porta padrão 8787
PORT=9000 node scripts/serve-inventory.js  # porta alternativa
```

Abra **http://localhost:8787** (não o `file://`). O badge fica verde:
`● Servidor — cliques salvam no .md`. Então:

- **Toggle Instalado** → escreve a coluna `Instalado` (`✓`/`—`) no `.md` e re-sincroniza o HTML.
- **Dropdown Status** → escreve a coluna `Status` no `.md` e re-sincroniza.

Cada ação grava na hora, **sem** adicionar linha ao `## Histórico` (evita poluir com um
registro por clique). Ao terminar: `Ctrl+C`, depois `git commit` + `git push` para
compartilhar com os outros editores. O servidor escuta só em `127.0.0.1` (não exposto na rede).

### O que dá pra editar pela interface

| Coluna | Como editar | Persistência |
|---|---|---|
| **Instalado** | toggle na tabela | coluna `Instalado` no `.md` |
| **Status** | dropdown na tabela | coluna `Status` no `.md` (texto canônico — ver abaixo) |
| Demais (Categoria, Escopo, Função, Tipo, etc.) | à mão no `.md` + sync | — |

> **Status — texto canônico (lossy):** salvar um status pela interface reescreve a célula
> inteira pelo texto canônico (`Instalar` → `🟢 À Instalar`, `Aguardar` → `🟠 Aguardar upstream`,
> etc.). Isso **apaga notas livres** na célula (ex.: `🟠 Aguardar (esperando PR #42)` viraria
> `🟠 Aguardar upstream`). Para preservar uma nota, edite o Status à mão no `.md`.

## Adicionar mods novos

- **Pelo Forge:** `/add-mod-inventory-list <forge-url> [...]` (busca dados, monta a linha, sincroniza).
  Use `--instalado` para já marcar a coluna `Instalado` como `✓`.
- **À mão:** adicione a linha no fim da tabela `## Inventário completo` do `.md` (todas as
  colunas, incluindo `Instalado`) e rode `node scripts/sync-mods-html.js`.

## Sincronizar o HTML manualmente

Depois de editar o `.md` à mão (qualquer coluna):

```bash
node scripts/sync-mods-html.js
```

Confirme a saída `Parsed N mods (0–N-1)` e o ✓ em ambos os arquivos. Diferente dos cliques
do servidor, o sync manual **adiciona** uma linha ao `## Histórico` do `.md`.

## Commands relacionados

| Command | Função |
|---|---|
| `/serve-inventory` | Sobe o servidor local de edição. |
| `/add-mod-inventory-list` | Adiciona mods a partir de URLs do Forge. |
| `/update-mods-inventory` | Roda o sync `.md` → `.html` (com entrada no histórico). |
