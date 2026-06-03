# /serve-inventory

Sobe o servidor local de edição do inventário de mods. Com ele rodando, mexer no
toggle **Instalado** ou no dropdown de **Status** em `docs/migration/mods-inventory.html`
escreve direto na coluna correspondente de `docs/migration/mods-inventory.md` (a fonte de
verdade / "banco de dados") e re-sincroniza o HTML. Commit + push compartilham as
alterações com os outros editores — **o git é a camada de sincronização entre máquinas**,
não o servidor.

## Como usar

1. Inicie o servidor (Node puro, sem `npm install`):
   ```
   node scripts/serve-inventory.js
   ```
   Porta padrão `8787`. Para trocar: `PORT=9000 node scripts/serve-inventory.js`.

2. Abra **http://localhost:8787** no navegador (não abra o arquivo via `file://` — aí o
   toggle vira só preview e não salva). O badge no canto inferior direito indica o modo:
   - 🟢 `● Servidor — cliques salvam no .md`
   - 🟠 `○ file:// — preview (não salva)`

3. Clique nos toggles **Instalado** ou escolha um **Status** no dropdown. Cada ação grava
   na hora no `.md` e atualiza o HTML.

4. Quando terminar: `Ctrl+C` para parar, depois `git commit` + `git push` para publicar.

## Notas

- Endpoints: `POST /api/installed` (`{n, installed}`) e `POST /api/status` (`{n, status}`).
  Ambos re-sincronizam **sem** adicionar linha ao `## Histórico` do `.md` (evita poluir com
  um registro por clique). O histórico só ganha linha no sync manual via `/update-mods-inventory`
  (`node scripts/sync-mods-html.js`).
- **Status (Opção 1 — texto canônico):** salvar um status reescreve a célula inteira pelo
  texto canônico do mapa (`Avaliar`→`🟡 Avaliar`, `Instalar`→`🟢 À Instalar`, etc.). Isso
  **apaga notas livres** na célula (ex.: `🟠 Aguardar upstream` editado vira `🟠 Aguardar upstream`,
  mas `🟠 Aguardar (nota custom)` perderia a nota). Para preservar notas, edite o Status à mão no `.md`.
- As demais colunas (Categoria, Escopo, Função, etc.) continuam editadas à mão no `.md` + sync.
- O mod `#0` (UltraFika) fica num bloco vertical separado e não é alvo dos toggles.
