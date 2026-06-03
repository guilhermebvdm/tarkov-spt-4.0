# /serve-inventory

Sobe o servidor local de edição do inventário de mods. Com ele rodando, clicar no
toggle **Instalado** em `docs/migration/mods-inventory.html` escreve direto na coluna
`Instalado` de `docs/migration/mods-inventory.md` (a fonte de verdade / "banco de dados")
e re-sincroniza o HTML. Commit + push compartilham as marcações com os outros editores —
**o git é a camada de sincronização entre máquinas**, não o servidor.

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

3. Clique nos toggles **Instalado**. Cada clique grava na hora no `.md` e atualiza o HTML.

4. Quando terminar: `Ctrl+C` para parar, depois `git commit` + `git push` para publicar.

## Notas

- O clique usa `POST /api/installed` e re-sincroniza **sem** adicionar linha ao
  `## Histórico` do `.md` (evita poluir com um registro por clique). O histórico só
  ganha linha no sync manual via `/update-mods-inventory` (`node scripts/sync-mods-html.js`).
- Só a coluna **Instalado** é editável pelo servidor. As demais (Status, Categoria, etc.)
  continuam editadas à mão no `.md` + sync.
- O mod `#0` (UltraFika) fica num bloco vertical separado e não é alvo do toggle.
