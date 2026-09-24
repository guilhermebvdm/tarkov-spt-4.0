# 006 — Trava de mãos ao equipar arma/faca/granada após ação recente (item não-carregador) · Review Técnica 02

**Mod:** FIKA
**Spec técnica revisada:** [006-colisao-maos-item-nao-carregador-02-spec-tech.md](006-colisao-maos-item-nao-carregador-02-spec-tech.md)
**Data:** 2026-09-11

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-02-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Memória consultada

Snapshot de `mods/FIKA/memory/sessions.md` — última entrada Sessão 6 (2026-09-10), pendências P-6.1 (🔴, exatamente o que esta rodada resolve — teste em raid real já feito e documentado em `PA-01-01`), P-5.1, P-4.1, P-3.1, P-3.2, P-1.1/1.2/1.3 — nenhuma bloqueante nova pra esta review além de P-6.1, já endereçada.

## Escopo desta rodada

A review 01 encontrou (`PA-01-01`) que a causa raiz assumida não se sustentava; a spec técnica foi **inteiramente reescrita** entre a review 01 e esta rodada (Fix 1 dividido em Fix 1a + Fix 1b, novo patch `HandsBookkeepingTimestampPatch`). Como é um desenho novo, sem review própria ainda, esta rodada revisa o desenho reescrito do zero — não assume que "já foi revisado" só porque o item já teve uma review 01 (essa validou o desenho *anterior*, hoje superado).

Durante a própria redação da spec reescrita, 3 erros foram encontrados e corrigidos pelo autor antes desta review (registrados no Histórico da spec técnica, não repetidos aqui como achados): (1) janela de graça mal dimensionada (reaproveitava `GraceWindowSeconds` de 0.35s de um mecanismo com timing completamente diferente); (2) `using Fika.Core.Main.Utils;` faltando pro `FikaGlobals.LogError` compilar; (3) `GetTargetMethod()` retornando `null` lança exceção que pode derrubar o `Awake()` inteiro do plugin se o registro do patch não estiver protegido — verificado por investigação dedicada no `references/spt-source/`, não suposição.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | B — Edge Case | 🟡 Importante | Tolerância a `GEventArgs9` é extrapolada por simetria, sem confirmação empírica própria (só `GEventArgs10` foi observado no log real) | ✅ Resolvido |
| PA-02-02 | A — Gap | 🟢 Menor | Checklist não inclui validação do corner case "self-heal"/múltiplas ações rápidas específico do Fix 1b (só tem o cenário principal e o de concorrência genuína) | ✅ Resolvido |

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

### ✅ PA-02-01 · B — Edge Case · 🟡 Importante — Resolvido em 2026-09-11

**Tolerância a `GEventArgs9` é extrapolada por simetria, sem confirmação empírica própria**

**Problema:** A spec técnica (§1.1, §1.2, stub §5.2) trata `GEventArgs9` ("BeginSetInHands", `Class1311`/`method_137`) e `GEventArgs10` ("BeginRemoveFromHands", `Class1312`/`method_138`) simetricamente, tolerando ambos com a mesma lógica e a mesma constante `HandsBookkeepingGraceWindowSeconds`. Mas **toda a evidência empírica desta sessão** (o log capturado em raid real, §1.1) mostra apenas `GEventArgs10` disparando — nenhuma reprodução confirmou que um cenário equivalente com `GEventArgs9` (o lado "entrando nas mãos") realmente causa o mesmo tipo de trava. A simetria de design (mesmo par Begin/Confirm, mesma classe base `Class1310`) é uma razão estrutural sólida pra tratá-los igual, mas não é prova de que o cenário de colisão real existe pro lado `GEventArgs9` — pode ser que o "entrando nas mãos" nunca acumule a mesma latência de confirmação que o "saindo das mãos" acumula (a origem do bug real é especificamente a animação de `HideWeapon`, não de um equivalente "ShowWeapon").

**Por que importa:** Não é um risco de correção (tolerar `GEventArgs9` sem necessidade não quebra nada, dado que a proteção estrutural — `List_0` por jogador — continua valendo) — é um risco de **escopo não-testado**: se existir algum cenário legítimo onde uma colisão real via `GEventArgs9` deveria continuar bloqueada e o design amplo do Fix 1b acaba tolerando por engano, isso só seria pego numa validação in-game que a spec não pede explicitamente pra esse lado.

**Sugestão:** Duas opções, qualquer uma resolve:
1. **Manter o escopo amplo** (como está), mas adicionar ao checklist §8 um item explícito: "Validar in-game algum cenário que dispare `GEventArgs9` (ex.: sacar uma arma repetidamente em sucessão rápida) e confirmar que a tolerância não introduz nenhum comportamento estranho — mesmo nível de rigor dado ao cenário `GEventArgs10`."
2. **Reduzir o escopo pra só `GEventArgs10`** nesta primeira versão (já resolve o bug relatado), documentar `GEventArgs9` como possível extensão futura não implementada ainda, e reavaliar se aparecer um relato real desse lado.

Dado que a opção 1 não exige nenhuma mudança de código e o item já tem outros itens de validação in-game pendentes, ela é a mais simples de aplicar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (opção 1)
- `[ ]` Aceitar sugestão (opção 2)
- `[ ]` Caminho alternativo: _________________

**Resolução:** item de checklist adicionado à spec técnica §8, pedindo validação in-game de um cenário `GEventArgs9`.

---

### ✅ PA-02-02 · A — Gap · 🟢 Menor — Resolvido em 2026-09-11

**Checklist não inclui validação do cenário "self-heal"/múltiplas ações rápidas específico do Fix 1b**

**Problema:** O item 004 (mecanismo irmão, `GEventArgs17`) tem no seu checklist um item específico pra "repetir a sequência várias vezes seguidas" (`PA-01-01` daquela review). O checklist desta spec (§8) tem "reproduzir o cenário original... confirmar ausência de travamento em pelo menos 10-15 ciclos seguidos", que já cobre repetição — mas não menciona explicitamente variar a **velocidade** de municiamento (o usuário descreveu clicar rápido repetidamente; um teste com ritmo mais lento também deveria continuar funcionando, e é implícito mas não explícito no checklist).

**Por que importa:** Puramente de clareza/completude do checklist — o item "10-15 ciclos seguidos" já cobre a maior parte disso na prática, esse ponto é sobre deixar explícito que a variação de ritmo faz parte do teste, não sobre uma lacuna funcional real.

**Sugestão:** Adicionar ao item de checklist já existente ("Validação in-game bloqueadora") a cláusula: "— testar em pelo menos dois ritmos: municiamento rápido (clique repetido) e municiamento normal (esperando cada carregador terminar antes do próximo), confirmando que nenhum dos dois trava."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** cláusula sobre variação de ritmo adicionada ao item de checklist já existente na spec técnica §8.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-11 | Review 02 criada via `/review-technical-spec` |
