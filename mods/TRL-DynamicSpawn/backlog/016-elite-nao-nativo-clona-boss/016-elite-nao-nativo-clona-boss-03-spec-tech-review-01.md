# 016 — elite-nao-nativo-clona-boss · Review Técnica 01

**Mod:** TRL-DynamicSpawn
**Spec técnica revisada:** [016-elite-nao-nativo-clona-boss-02-spec-tech.md](016-elite-nao-nativo-clona-boss-02-spec-tech.md)
**Data:** 2026-09-23

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

Memória consultada: snapshot de 2026-09-04 (sessions.md) · pendências que afetam: nenhuma
Docs técnicos lidos (gatilho disparado): `docs/technical/spt-antipatterns.md` (AP-01 a AP-11, obrigatório)

Nenhum bloqueador. Os pontos abaixo têm sugestão pronta pra aceitar; a spec pode seguir para `/code-mod` incorporando as correções (todas são edições pontuais, sem redesenho).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C | 🟡 Importante | Duas citações de linha erradas no `EliteFollowerMap` (Knight e sectantWarrior) | ✅ Resolvido 2026-09-23 |
| PA-01-02 | A | 🟡 Importante | Guarda de `GroupChance < 100` foi descartada ao portar a fórmula pro caminho boss+guardas | ✅ Resolvido 2026-09-23 |
| PA-01-03 | C | 🟡 Importante | Spec funcional (01-spec.md) ainda diz "Apenas Rogues e Raiders" — contradiz a decisão de incluir Bloodhounds | ✅ Resolvido 2026-09-23 |
| PA-01-04 | C | 🟢 Menor | Diagrama §6 "Fluxo de dados" ficou desatualizado após Bloodhounds entrar no `isGruntSquad` | ✅ Resolvido 2026-09-23 |
| PA-01-05 | B | 🟢 Menor | `disableFollowers` vira campo inerte pra Bloodhounds no painel Web, sem aviso | ✅ Resolvido 2026-09-23 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-09-23

**Duas citações de linha erradas no `EliteFollowerMap` (Knight e sectantWarrior)**

**Problema:** Conferindo linha a linha contra [WildSpawnType.cs](../../../../references/eft-decompiled/Assembly-CSharp/EFT/WildSpawnType.cs) (lido nesta review), duas das citações de linha do stub em §5 não batem:
- `{ WildSpawnType.bossKnight, ... }` cita `// ref: ...WildSpawnType.cs:26,27 (bossKnight=26) / :32,33 (...)`. A linha real de `bossKnight = 26,` é **31**, não 26 — o comentário usou o *valor* do enum (26) como se fosse o número da linha. A linha 26 real do arquivo é `sectantPriest = 21,`, algo completamente não relacionado. A segunda metade da citação (`:32,33` pra `followerBigPipe`/`followerBirdEye`) está correta.
- `{ WildSpawnType.sectantPriest, ... }` cita `// ref: ...WildSpawnType.cs:26,20 (sectantPriest=21 / sectantWarrior=20)`. `sectantPriest=21` está corretamente na linha 26, mas `sectantWarrior=20` está na linha **25**, não 20 — de novo o valor do enum foi usado no lugar do número da linha. A linha 20 real é `followerGluharSnipe = 15,`.

**Por que importa:** Os `WildSpawnType` usados no `Dictionary` do stub estão corretos (`bossKnight`, `followerBigPipe`, `followerBirdEye`, `sectantPriest`, `sectantWarrior` são os nomes certos) — isso não afeta a compilação nem o comportamento do fix. Mas uma citação errada quebra o contrato de evidência do repo (AP-09 — "recon/decompile curado tratado como verdade pinada"): quem clicar no link da linha 26 esperando achar `bossKnight` cai em `sectantPriest`, e vice-versa. É só nos comentários, então não bloqueia o `/code-mod`, mas deve ser corrigido antes de fechar o item.

**Sugestão:** Trocar as duas linhas de comentário em `016-elite-nao-nativo-clona-boss-02-spec-tech.md` §5:
- `// ref: Assembly-CSharp/EFT/WildSpawnType.cs:26,27 (bossKnight=26) / :32,33 (followerBigPipe=27, followerBirdEye=28)` → `// ref: Assembly-CSharp/EFT/WildSpawnType.cs:31 (bossKnight=26) / :32,33 (followerBigPipe=27, followerBirdEye=28)`
- `// ref: Assembly-CSharp/EFT/WildSpawnType.cs:26,20 (sectantPriest=21 / sectantWarrior=20)` → `// ref: Assembly-CSharp/EFT/WildSpawnType.cs:25,26 (sectantWarrior=20 / sectantPriest=21)`

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Usuário pediu esclarecimento ("não entendi") — explicado em chat: os `WildSpawnType` citados no `Dictionary` (nomes) já estavam certos; só o número da linha no comentário `// ref:` usava por engano o *valor numérico* do enum em vez da linha real do arquivo. Corrigido em `016-...-02-spec-tech.md` §5: `bossKnight` agora cita `:31` e `sectantWarrior`/`sectantPriest` citam `:25,26`.

---

### PA-01-02 · A — Gap de Especificação · 🟡 Importante · ✅ Resolvido em 2026-09-23

**Guarda de `GroupChance < 100` foi descartada ao portar a fórmula pro caminho boss+guardas**

**Problema:** O código original (`DynamicSpawnManager.cs:748` antes do fix) só entrava no sorteio de tamanho de grupo quando `entry.info.GroupChance > 0 && entry.info.GroupChance < 100`. Com `GroupChance == 100`, o `if` inteiro era pulado e `targetGroupSize` ficava como o valor bruto de `GetBossGroupSizeForMap` (determinístico, sem sorteio). O stub da §5 desta spec técnica reescreve a condição como só `if (entry.info.GroupChance > 0 && UnityEngine.Random.Range(1, 101) <= entry.info.GroupChance)` — **sem o `< 100`**. Em `GroupChance = 100`, isso muda o comportamento: a condição passa a ser sempre verdadeira, e `totalSquadSize = UnityEngine.Random.Range(2, maxSquad + 1)` entra em jogo — ou seja, com `GroupChance=100` o número de guardas passa a ser **sorteado** entre 1 e `maxSquad-1`, e não mais fixo em `maxSquad-1`.

**Por que importa:** Não é um bug de compilação nem afeta o caso relatado (Sanitar em Labs), mas é uma mudança de comportamento no limite `GroupChance=100` que a spec não menciona como decisão deliberada — um admin que hoje configura `groupChance=100` esperando sempre o esquadrão máximo passaria a ver variação depois do fix. Pode ser o comportamento CERTO (mais intuitivo que o quirk original), mas precisa ser uma decisão explícita, não um efeito colateral da reescrita.

**Sugestão:** Duas opções — escolher uma e documentar em §1 ponto 4:
1. **Manter fidelidade ao original:** reintroduzir `&& entry.info.GroupChance < 100` na condição do stub (linha correspondente a `DynamicSpawnManager.cs:195` do trecho da §5) — em `GroupChance=100`, guardas = `maxSquad - 1` sempre, sem sorteio.
2. **Assumir o comportamento novo como intencional:** manter o stub como está, e adicionar uma frase em §1 explicando que a partir deste fix `GroupChance=100` passa a sortear entre 1 e `maxSquad-1` guardas (nunca zero, nunca fixo), e por quê isso é preferível.

**Decisão:**
- `[x]` Caminho alternativo: opção 1 (fidelidade), com um ajuste adicional — ver Resolução.

**Resolução:** Usuário confirmou a leitura de `GroupChance` como "chance do grupo spawnar, não do tamanho dele" — semântica que já era a intenção, só mal implementada no limite `GroupChance=100`. Reescrito em `016-...-02-spec-tech.md` §5: `GroupChance=100` agora forma grupo sempre, determinístico, no teto do mapa (fidelidade — opção 1); e, como efeito colateral corrigido na mesma linha de causa raiz, `GroupChance=0` deixa de se comportar como `100` (bug latente do código original, onde os dois pulavam o sorteio e caíam no mesmo "sempre forma") — agora `0` nunca forma grupo. Documentado em §1 ponto 5.

---

### PA-01-03 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-09-23

**Spec funcional (01-spec.md) ainda diz "Apenas Rogues e Raiders" — contradiz a decisão de incluir Bloodhounds**

**Problema:** A spec funcional [016-elite-nao-nativo-clona-boss-01-spec.md](016-elite-nao-nativo-clona-boss-01-spec.md), seção "Comportamento desejado", afirma: *"Apenas Rogues e Raiders continuam podendo formar grupo de múltiplos integrantes idênticos via `groupChance`/`maxGroupSize` (comportamento atual, inalterado)."* O critério de aceite correspondente repete: *"Rogues e Raiders continuam formando grupo de integrantes idênticos exatamente como hoje — nenhuma regressão nesse comportamento."* Nenhum dos dois menciona Bloodhounds. Mas a spec técnica (§1 ponto 1, §5, §7) já incorpora a decisão do usuário desta sessão: Bloodhounds (`arenaFighterEvent`) também vira esquadrão genérico (`isGruntSquad`), igual Rogues/Raiders — inclusive com um novo default de tamanho de grupo (3→5).

**Por que importa:** As duas specs do mesmo item agora descrevem comportamentos diferentes pro mesmo sistema. Um leitor futuro (ou o `/code-review`) que confira a spec funcional como fonte de verdade dos critérios de aceite vai marcar "Bloodhounds forma grupo" como uma regressão não coberta, quando na verdade foi uma decisão explícita do usuário capturada só na spec técnica.

**Sugestão:** Editar `016-elite-nao-nativo-clona-boss-01-spec.md`:
- Em "Comportamento desejado": trocar "Apenas Rogues e Raiders continuam podendo formar grupo..." por "Rogues, Raiders e Bloodhounds continuam podendo formar grupo de múltiplos integrantes idênticos via `groupChance`/`maxGroupSize` (comportamento atual, inalterado pro mecanismo; Bloodhounds passa a usar o mesmo caminho, com novo default de tamanho de grupo)."
- No critério de aceite correspondente: incluir Bloodhounds na lista junto de Rogues/Raiders.
- Opcional: adicionar uma linha no critério 2 (guarda com papel correto) mencionando que Cultistas (`sectantPriest`) agora tem `sectantWarrior` como guarda dedicado, já que isso também não estava no mapa original de chefes citados na spec funcional.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aprovado pelo usuário. `016-elite-nao-nativo-clona-boss-01-spec.md` atualizada: "Comportamento desejado" e o critério de aceite de Rogues/Raiders agora incluem Bloodhounds; critério de guarda com papel correto ganhou menção a Cultistas/`sectantWarrior`.

---

### PA-01-04 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-09-23

**Diagrama §6 "Fluxo de dados" ficou desatualizado após Bloodhounds entrar no `isGruntSquad`**

**Problema:** O stub de código em §5 (linhas 165-167 do trecho) define `isGruntSquad` como `exUsec || pmcBot || arenaFighterEvent`. Mas o diagrama em §6 ainda diz `isGruntSquad? (exUsec/pmcBot)`, sem mencionar `arenaFighterEvent`/Bloodhounds — ficou desatualizado quando a decisão sobre Bloodhounds foi incorporada depois de o diagrama já estar escrito.

**Por que importa:** Inconsistência interna dentro do mesmo documento; não afeta a implementação (o stub de código, que é o que se copia, está correto), só a legibilidade de quem lê o fluxo de dados isoladamente.

**Sugestão:** Em §6, trocar `isGruntSquad? (exUsec/pmcBot)` por `isGruntSquad? (exUsec/pmcBot/arenaFighterEvent)`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aplicado em `016-...-02-spec-tech.md` §6.

---

### PA-01-05 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-23

**`disableFollowers` vira campo inerte pra Bloodhounds no painel Web, sem aviso**

**Problema:** O painel Web expõe um checkbox "Desativar Seguidores/Guardas" (`disable_followers`, [Index.razor:921](../../modded/Server/Web/Pages/Index.razor#L921)) por `EliteLocationInfo`, incluindo `arenaFighterEvent` (Bloodhounds). Depois do fix, Bloodhounds passa pelo caminho `isGruntSquad`, que nunca lê `entry.info.DisableFollowers` (só o caminho boss+guardas lê esse campo). O checkbox continua visível e editável no painel pra Bloodhounds, mas não tem mais nenhum efeito — mesma situação que já existe hoje pra Rogues/Raiders (não é regressão nova), só que agora um terceiro campo fica nessa condição.

**Por que importa:** Puramente cosmético/UX — não afeta o bug relatado nem a correção. Um usuário configurando Bloodhounds pelo painel pode ficar confuso ao marcar "Desativar Seguidores" e não ver efeito nenhum.

**Sugestão:** Fora do escopo deste fix (é um ajuste de UI do painel, não de lógica de spawn) — registrar como pendência 🟢 na memória do mod (`mods/TRL-DynamicSpawn/memory/sessions.md`) pra uma futura limpeza do painel esconder/desabilitar "Disable Followers" quando o role selecionado for um dos grunt-squad (Rogues/Raiders/Bloodhounds). Não bloqueia `/code-mod`.

**Decisão:**
- `[x]` Aceitar sugestão (registrar como pendência de memória, não implementar agora)

**Resolução:** Usuário concordou. Registro formal fica pro próximo `/update-memory` (a entrada de sessão precisa de timestamp real via `date` e numeração `Sessão N` própria — fora do escopo de uma edição pontual de review). Até lá, este ponto serve de referência: pendência 🟢 sugerida — "painel Web esconder/desabilitar `disable_followers` pra roles grunt-squad (Rogues/Raiders/Bloodhounds), já que o campo não tem efeito nesse caminho desde o item 016".
