# 032 — Matriz de skills — Auto-review da spec técnica (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./032-matriz-skills-02-spec-tech.md) · [01-spec](./032-matriz-skills-01-spec.md)

Revisão crítica da spec técnica buscando bloqueadores (🔴), riscos (🟡) e observações (🟢). 🔴 resolvidos diretamente no 02 (linha de histórico adicionada lá).

## 🔴 Bloqueadores (encontrados e resolvidos no 02)

### R1 — `colspan` do separador de categoria/overflow era estático e ignorava o toggle
**Problema:** os separadores de categoria (`CategoryHeader`) e o de overflow precisam de um `<td colspan="N">` que cubra **a coluna de nomes + todas as colunas visíveis**. O 02 portava `SkillCanonicalList`/`profiles-skills.js` sem dizer que `N` é **dinâmico**: quando "Mostrar desabilitadas" é desligado, o número de colunas muda, e um colspan fixo deixaria a borda do separador curta/longa (linha desalinhada) ou, no viewer original (`profiles-skills.js:57`), exigiria um `<td class="cat-spacer">` por coluna. Sem isso, alternar o toggle quebra o alinhamento visual do separador — falha de CA1/CA2 na prática.
**Resolução:** o 02 agora define `ColumnSpan => 1 + VisibleColumns().Count` e manda os separadores (categoria e overflow) usarem `colspan="@ColumnSpan"`, recomputado em cada render (igual ao `ColumnCount` de `SkillCanonicalList.razor:413`). Alternativa do viewer (spacer por coluna) descartada por ser mais código e pior no hover.

### R2 — Ícone dentro do header com `writing-mode: vertical-rl` sai rotacionado/distorcido
**Problema:** o 02 punha ícone + nome dentro do mesmo `.cc-skill-col-header__inner` que tem `writing-mode: vertical-rl; transform: rotate(180deg)`. Um `<img>` herda esse contexto e fica **deitado**, e o `rotate(180deg)` deixaria o ícone de cabeça para baixo. O viewer original (`profiles-skills.js:45-48`) só tem texto no header — não há precedente para o ícone aí.
**Resolução:** o 02 agora separa o header em dois elementos: o **ícone fica fora** do bloco rotacionado (numa `<div class="cc-col-icon">` com `writing-mode: horizontal-tb`, acima ou abaixo do texto vertical), e só o **nome** recebe `writing-mode: vertical-rl`. O `@onclick` de navegação fica no `<th>` inteiro (não no inner), cobrindo ícone + texto. CSS ajustado no bloco scoped.

### R3 — Coluna "desabilitada" precisa esmaecer o header E todas as células daquela coluna, mas não há seletor de coluna em CSS
**Problema:** CSS não tem "selecionar a N-ésima célula de cada linha por estado de dado". O 02 dizia "classe extra `cc-col--disabled` (opacidade reduzida)" só no header — as **células** da coluna desabilitada continuariam em opacidade cheia, contrariando o critério "coluna esmaecida" (spec, comportamento desejado + CA2). `nth-child` não serve porque o índice da coluna muda quando o toggle some/mostra colunas.
**Resolução:** o 02 agora aplica a classe `cc-cell--disabled` **por célula** no laço de render (cada `Cell`/header recebe a classe quando `!col.Enabled`), não via seletor de coluna. O estado vem do dado (`col.Enabled`), avaliado por célula — robusto à reordenação/filtragem. CSS: `.cc-cell--disabled { opacity:.4; }` aplicada a `th` e `td`.

## 🟡 Riscos (registrados, não bloqueiam)

- **Y1 — Performance de render com muitas classes × ~35 skills:** uma matriz 35 linhas × N colunas gera 35·N células. Para N realista (dezenas de classes) é trivial; o custo é só DOM, não recomputo (custo já está em memória). Sem virtualização neste item (fora de escopo); reavaliar se N passar de ~50 colunas.
- **Y2 — `GetCachedEntries()` não é custo-zero:** faz scan de diretório + passada CR-EP-06 a cada chamada (`ClassEditorService.cs:176-180`). O 02 já chama **1×** em `OnInitialized` (D3) — ok. O risco é regressão futura: se alguém mover a chamada para um helper invocado no render, vira O(render). Mitigado pela nota explícita no 02.
- **Y3 — Chip de multiplicador em célula vazia (corner case 8):** decisão de produto registrada na spec (P4/CC8) — chip aparece mesmo em nível 0. Tecnicamente trivial (chip independe do nível), mas visualmente pode parecer "ruído" numa coluna com muitos multiplicadores e poucos níveis. Aceito como está; ajuste cosmético fica para densidade (035).

## 🟢 Observações

- **G1 — Ícone de NavLink (`GridOn`) e cores de tier são cosméticos** — ajustáveis sem nova spec. Documentado no 02.
- **G2 — Tokens do viewer (`--accent-bright` etc.) não existem no editor** — o 02 já manda substituir por `--mud-palette-*`/hex. Bom ter verificado: copiar o CSS do viewer cru quebraria silenciosamente (variável CSS indefinida = sem cor, não erro).
- **G3 — `BareName` via `Path.GetFileNameWithoutExtension`** casa com a rota do detalhe (`NavMenu.razor:216/281-283`, convenção 024) — navegação consistente com a sidebar.

## Conclusão

3 🔴 encontrados e resolvidos no 02 (colspan dinâmico, ícone fora do bloco rotacionado, esmaecimento por célula). 3 🟡 registrados sem ação (dentro do escopo/aceitáveis). **Bloqueadores abertos: 0.**

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | Auto-review inicial: 3 🔴 (R1 colspan, R2 ícone rotacionado, R3 esmaecimento por célula) resolvidos no 02; 3 🟡 + 3 🟢 registrados. |
