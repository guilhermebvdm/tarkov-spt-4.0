# /update-mods-inventory

Atualiza `docs/migration/mods-inventory.html` com os dados mais recentes de `docs/migration/mods-inventory.md`.

## O que fazer

1. Execute o script de sincronização:
   ```
   node scripts/sync-mods-html.js
   ```

2. Confirme que a saída mostra **105 mods (0–104)** e que ambos os arquivos foram atualizados com ✓.

3. Se o script falhar, relate o erro ao usuário. Não tente fazer o parse manual da tabela.

## O que o script faz

- Lê o markdown e parseia a tabela `## Inventário completo` + o bloco vertical do UltraFika (mod #0)
- Extrai os 12 campos por mod: `n, name, tipo, atuacao, categoria, escopo, forge_id, r4_path, fn, status, prioridade, interno`
- Substitui apenas o bloco `const MODS = [...]` no HTML — CSS, JS e layout não são tocados
- Adiciona uma linha no `## Histórico` do markdown com a data e descrição `docs(migration): sync mods-inventory.html from markdown`

## Regras de parse (implementadas no script)

- `forge_id`: extrai o número da URL `forge.sp-tarkov.com/mod/{id}/` → `"123"`; `—` → `null`; `🔍` → `""`
- `r4_path`: GitHub → `"user/repo"`; GitLab/outro → URL completa; `—` → `null`; `🔍` → `""`
- `tipo/atuacao/categoria`: remove emoji do início, extrai primeira palavra
- `escopo`: separa por ` · `, remove emoji de cada parte, une com vírgula
- `status`: detecta palavra-chave na célula (`Instalar`, `Evoluir`, `Aguardar`, etc.)
- Coluna `Repo 3.x` é ignorada (não usada no HTML)
