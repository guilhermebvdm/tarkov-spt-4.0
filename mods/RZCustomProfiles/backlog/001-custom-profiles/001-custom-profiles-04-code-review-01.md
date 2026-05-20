# 001 — Perfis customizados temáticos · Code Review 01

**Mod:** RZCustomProfiles
**Spec funcional:** [001-custom-profiles-01-spec.md](001-custom-profiles-01-spec.md)
**Spec técnica:** [001-custom-profiles-02-spec-tech.md](001-custom-profiles-02-spec-tech.md)
**Asbuild:** [001-custom-profiles-05-asbuild.md](001-custom-profiles-05-asbuild.md)
**Data:** 2026-05-17

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Aplicados: 4 · ❌ Rejeitados: 1 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟠 Forte | Dependência de `cache/spt-raw.json` (gitignored) quebra reprodutibilidade | ✅ Aplicado 2026-05-17 |
| CR-01-02 | D — Arquitetura | 🟡 Médio | `build-loadouts.js` legado com recipes estale duplica `build-profile-jsons.js` | ✅ Aplicado 2026-05-17 |
| CR-01-03 | B — Bug latente | 🟡 Médio | Validação ocorre **após** escrever os JSONs | ✅ Aplicado 2026-05-17 |
| CR-01-04 | C — Gap vs. spec | 🟡 Médio | Corner case "stash não comporta loadout" não validado nem mitigado | ✅ Aplicado 2026-05-17 |
| CR-01-05 | E — Legibilidade | 🟢 Menor | `TRADER_IDS` sem comentários identificando cada trader | ❌ Rejeitado (falso alarme) |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · ✅ Aplicado em 2026-05-17

**Dependência de `cache/spt-raw.json` (gitignored) quebra reprodutibilidade em fresh checkout**

**Resolução:** opção (A) aceita — criado `scripts/extract-item-data.js` que lê do tarkov-itemdb (gitignored) e emite `scripts/item-data.json` versionado (100 TPLs com name/stackMax/width/height). `build-profile-jsons.js` agora lê de `item-data.json` em runtime, sem dependência do cache. Regenerar via `node scripts/extract-item-data.js` quando o EFT atualizar.

**Aplicação:** criados [scripts/extract-item-data.js](../../scripts/extract-item-data.js), [scripts/item-data.json](../../scripts/item-data.json) (versionado); editado [build-profile-jsons.js:22-42](../../scripts/build-profile-jsons.js#L22) para usar item-data.json.

**Local:** [`mods/RZCustomProfiles/scripts/build-profile-jsons.js:30-37`](../../scripts/build-profile-jsons.js#L30)

**Problema:** O script lê `tools/tarkov-itemdb/cache/spt-raw.json` para obter `stackMaxSize` por TPL:

```js
const SPT_RAW_PATH = path.join(REPO_ROOT, 'tools/tarkov-itemdb/cache/spt-raw.json');
if (!fs.existsSync(SPT_RAW_PATH)) {
  console.error(`ERRO: ${SPT_RAW_PATH} não encontrado. Rode tools/tarkov-itemdb/scripts/load-spt.js antes.`);
  process.exit(1);
}
```

Per o [README do itemdb](../../../../tools/tarkov-itemdb/README.md): "cache/: gitignored; outputs intermediários, regeneráveis". Ou seja, **o arquivo não está versionado**. Num clone fresco do repo, ou em CI, ou em qualquer máquina que não rodou o pipeline do itemdb antes, o script falha imediatamente.

**Por que importa:**
- Quebra o "executable spec" — qualquer pessoa que pegar este repo e rodar `node mods/RZCustomProfiles/scripts/build-profile-jsons.js` precisa também rodar o pipeline do itemdb primeiro, que por sua vez exige `SPT_PATH` env var apontando para um install do SPT (D:/SPT/SPT por default) — não vai existir num CI ou em outra máquina sem instalação local do SPT.
- A informação realmente necessária é apenas o mapa `tpl → stackMaxSize` (~5630 entradas, dezenas de KB). O resto do `spt-raw.json` (preços, traders, handbook) não é usado por este script.

**Sugestão:** Extrair apenas o que o script precisa (`tpl → stackMaxSize`) para um arquivo versionado próprio do mod. Duas opções:

- **(A)** Criar `mods/RZCustomProfiles/scripts/stack-sizes.json` (versionado), gerado a partir do itemdb via comando one-shot: `node scripts/extract-stack-sizes.js` (ler `spt-raw.json` → emitir o subset). Esse arquivo entra no git e o `build-profile-jsons.js` lê dele.
- **(B)** Adicionar fallback que usa `data/items.json` (versionado, mas hoje não tem stackMaxSize). Requereria atualizar `tools/tarkov-itemdb/scripts/normalize.js` para incluir `stackMaxSize`.

Recomendação: **opção (A)** — mais simples, isolada ao mod, regenerável.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (extrair stack-sizes.json versionado via script auxiliar)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · D — Arquitetura · ✅ Aplicado em 2026-05-17

**`build-loadouts.js` legado com recipes estale duplica `build-profile-jsons.js`**

**Resolução:** opção (A) aceita — `build-loadouts.js` deletado. O markdown do planejamento já está estável; se precisar ser regenerado no futuro, o `build-profile-jsons.js` pode ser estendido com modo `--emit-markdown` reusando as mesmas recipes.

**Aplicação:** `git rm mods/RZCustomProfiles/scripts/build-loadouts.js`. Único source of truth para recipes agora é `build-profile-jsons.js`.

**Local:** [`mods/RZCustomProfiles/scripts/build-loadouts.js:75-433`](../../scripts/build-loadouts.js#L75) (PROFILES array)

**Problema:** O script `build-loadouts.js` (renderiza markdown) tem o array `PROFILES` com:

- Nomes antigos (`Sanitarista`, `Franco-Atirador`) que foram renomeados para `Médico de Combate` / `Caçador` no planejamento
- Item-tema para Caçador, Batedor, Saqueador ainda com mapas (`WOODS_MAP`, `INTERCHANGE_MAP`, `CUSTOMS_MAP`) que foram substituídos no planejamento por Vaseline / Aquamari / barter items
- Sem campos de `skillOverrides`, `hideout`, `description`, `fileName` que o novo script tem

Resultado: rodar `node build-loadouts.js` hoje gera markdown que **não bate** com o estado atual do `001-custom-profiles.md`. As recipes ficam duplicadas (recipes em ambos os scripts, mas apenas as do novo são corretas).

**Por que importa:**
- Confusão sobre qual é a fonte de verdade. Próxima pessoa a mexer pode editar o script errado.
- Risco de regenerar markdown estale e sobrescrever o planejamento atual.
- Manutenção dupla: qualquer mudança de recipe precisa ser feita em 2 lugares.

**Sugestão:** Três caminhos possíveis:

- **(A) Deletar `build-loadouts.js`** — markdown já foi escrito manualmente no planejamento, não precisa mais ser regenerado.
- **(B) Refatorar para compartilhar recipes** — extrair `PROFILES` + `BASELINE` para `mods/RZCustomProfiles/scripts/recipes.js` (módulo CommonJS exportado), ambos scripts importam dele.
- **(C) Sincronizar `build-loadouts.js` com o estado atual** — atualizar nomes e temas, mas manter ambos os scripts independentes.

Recomendação: **opção (B)** — DRY, baixo custo (~20min), preserva a capacidade de regenerar markdown se necessário.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (extrair `recipes.js` compartilhado)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · B — Bug latente · ✅ Aplicado em 2026-05-17

**Validação ocorre **após** escrever os JSONs no disco**

**Resolução:** sugestão aceita — main loop refatorado em 2 passadas: (1) build todos em memória + validar; (2) só escrever se a validação coletiva passar. Falha de qualquer perfil aborta com exit code 1 e nenhum arquivo é tocado.

**Aplicação:** [build-profile-jsons.js:692-716](../../scripts/build-profile-jsons.js#L692) — `builds = PROFILES.map(...)` agrega + valida; `if (allIssues.length > 0) process.exit(1)`; só depois o loop de write executa.

**Local:** [`mods/RZCustomProfiles/scripts/build-profile-jsons.js:651-700`](../../scripts/build-profile-jsons.js#L651) (loop main)

**Problema:** No bloco `// ── Main ──`:

```js
for (const p of PROFILES) {
  const json = buildProfileJson(p);
  const v = validateProfile(p);                       // ← validação
  const out = JSON.stringify(json, null, 2) + '\n';
  fs.writeFileSync(filePath, out, { encoding: 'utf8' }); // ← write JÁ ACONTECEU
  // ... mais validação BOM ...
  summary.push({ ..., issues: v.issues });
}
// ...
const issuesAll = summary.flatMap(s => s.issues.map(i => `[${s.fileName}] ${i}`));
if (issuesAll.length > 0) {
  console.log('\n⚠️  Issues encontradas:');
  for (const i of issuesAll) console.log('  -', i);
  process.exit(1);
}
```

Se algum perfil falhar validação (custo fora de [28, 32], total fora de [1.95M, 2.05M], etc), o script imprime issues e sai com código 1 — **mas os JSONs inválidos já estão escritos em `modded/profiles/`** e ficam lá até o próximo run bem-sucedido.

**Por que importa:**
- Estado inconsistente entre runs: meio-build pode deixar 5 JSONs OK + 5 inválidos na pasta.
- Se alguém olhar `modded/profiles/` depois de um run falho, vai assumir que está OK.

**Sugestão:** Inverter ordem — validar TODOS os perfis primeiro (em memória), só escrever no disco se nenhum tiver issues:

```js
// 1ª passada: buildar todos os JSONs em memória + validar
const builds = PROFILES.map(p => ({
  profile: p,
  json: buildProfileJson(p),
  validation: validateProfile(p),
}));

const allIssues = builds.flatMap(b =>
  b.validation.issues.map(i => `[${b.profile.fileName}] ${i}`)
);

if (allIssues.length > 0) {
  console.log('\n⚠️  Issues encontradas (NADA escrito):');
  for (const i of allIssues) console.log('  -', i);
  process.exit(1);
}

// 2ª passada: validação OK, escrever no disco
for (const b of builds) { /* fs.writeFileSync(...) */ }
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (inverter ordem validate → write)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-04 · C — Gap vs. spec · ✅ Aplicado em 2026-05-17

**Corner case "stash não comporta o loadout" não validado nem mitigado**

**Resolução:** sugestão aceita com mitigação automática — `stashSlotsRequired()` calcula slots como soma de `width × height` por entrada em `Items[]` (cada entry já é 1 stack após stack-rule). Auto-bump: se `slots > 280` (L1), set `HideoutStartingLevels.Stash: 2` (L2 = 380 slots). Validação final aborta se ainda ultrapassar L2.

**Resultado empírico:** **confirmou o risco da spec** — 7/10 classes têm 296-330 slots de loadout, ultrapassando L1 mas confortavelmente dentro de L2:
- **Stash:1 preservado** (3 classes): Caçador (278), Armeiro (231), Gerente (271)
- **Stash:2 auto-aplicado** (7 classes): Médico (296), Fuzileiro (309), Batedor (305), Op. Noturno (301), Op. Tático (302), Sobrevivencialista (304), Saqueador (330)

**Aplicação:** [build-profile-jsons.js:563-590](../../scripts/build-profile-jsons.js#L563) — `STASH_SLOTS_L1/L2` constants + `stashSlotsRequired()`; [linhas 695-700](../../scripts/build-profile-jsons.js#L695) — auto-mitigação no build loop.

**Local:** [Spec funcional §Corner cases](001-custom-profiles-01-spec.md) — corner case "Stash não comporta o loadout inteiro" / [Spec técnica §7 Riscos](001-custom-profiles-02-spec-tech.md)

**Problema:** A spec funcional reconhece o risco:
> "O stash inicial de Standard tem 10×28 slots (280 slots no nível 1). 1 primary + 3 backups com armas, mochilas, coletes e meds podem exceder essa capacidade — itens 'transbordando' podem ser descartados pelo SPT ou bloquear a criação."

A spec técnica documenta opções de mitigação: (a) reduzir backups, (b) elevar `Stash`, (c) consolidar `Count`. O **código não implementa nem valida nada disso**. Os JSONs gerados têm 71-98 entradas em `Items[]`, e cada arma/mochila ocupa múltiplas células no stash 10×28.

Estimativa rápida (Médico de Combate, 89 entradas):
- 4× AKM (5 células cada) = 20
- 16× mags AK (1) = 16
- 4× PM (1) = 4
- 8× mags PM (1) = 8
- 3× LShZ helmet (2) = 6 → wait, só 1 LShZ (primary), 3× SSh-68 (2) = 6
- 1× 6B23-1 (4) + 3× PACA (4) = 16
- 4× BlackRock rig (4) = 16
- 1× MBSS (6) + 3× ScavBP (6) = 24
- Munição: 6 stacks (1) = 6
- Meds/comida (~30 cells)
- **Total estimado: ~150 cells** — abaixo de 280, mas sem margem se algum item tiver dimensões maiores que estimadas.

Sobrevivencialista (98 entradas) e Operador Noturno (95 entradas) estão mais próximos do limite.

**Por que importa:**
- Risco real de overflow silencioso em pelo menos 1 das 10 classes.
- Spec funcional crítério "Loadout temático no stash" pode falhar in-game.
- Sem validação automatizada, problema só aparece em playtest.

**Sugestão:** Adicionar validação no script usando `dims.width` e `dims.height` de [tools/tarkov-itemdb/data/items.json](../../../../tools/tarkov-itemdb/data/items.json):

```js
function stashSlotsRequired(items) {
  // Soma simples width × height × Count (estimativa otimista — não considera packing)
  let total = 0;
  for (const it of items) {
    const tpl = it.Tpl;
    const data = ITEMS_JSON[tpl];
    if (!data || !data.dims) continue;
    total += (data.dims.width * data.dims.height) * it.Count;
  }
  return total;
}

// No validateProfile:
const slots = stashSlotsRequired(json.AdditionalStartingItems.Items);
if (slots > 280) issues.push(`Slots estimados ${slots} > 280 (stash inicial). Considerar Stash:2.`);
else if (slots > 220) issues.push(`Slots estimados ${slots} próximos do limite 280. Risco de overflow real (packing).`);
```

Mitigação automática: se `slots > 280`, elevar `HideoutStartingLevels.Stash` para 2 (que adiciona mais 28 fileiras, ou seja vira ~308 slots, dependendo da versão do EFT).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (validar slots + warning/auto-bump Stash)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida — validar em playtest): _________________

---

### CR-01-05 · E — Legibilidade · 🟢 Menor

**`TRADER_IDS` sem comentários identificando cada trader**

**Local:** [`mods/RZCustomProfiles/scripts/build-profile-jsons.js:91-103`](../../scripts/build-profile-jsons.js#L91)

**Problema:** A lista `TRADER_IDS` é apenas IDs hex:

```js
const TRADER_IDS = [
  '54cb50c76803fa8b248b4571', // Prapor
  '54cb57776803fa99248b456e', // Therapist
  // ...
];
```

Olhando o código bruto agora — os comentários estão lá. ✅ Falso alarme da minha análise inicial; isso já está corretamente comentado. Mantenho o ponto registrado para fechamento explícito.

**Por que importa:** N/A (já está OK).

**Sugestão:** Marcar como ✅ Resolvido na origem — o código já está bem documentado.

**Decisão:**
- `[ ]` Pendente
- `[x]` Rejeitar (já está bem documentado no código original — falso alarme da review)

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-17 | Code review 01 criada via `/code-review` |
