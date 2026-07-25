# 009 — Coop/bots: hardening do Trauma 2.0 · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [009-coop-hardening-02-spec-tech.md](009-coop-hardening-02-spec-tech.md)
**Data:** 2026-07-20

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 4, entradas P-4.1/P-4.5) + pendências do bloco de topo (P-3.x). P-4.1 (débito do boilerplate `Update()`, aberto em 006 code-review-01 CR-01-02) é exatamente o que este item resolve — nenhuma pendência 🔴 do mod bloqueia esta rodada.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 Importante | Campos `_wasActive`/`_trackedWorld` dos 4 consumidores ficam mortos, sem instrução de remoção | ✅ Resolvido |
| PA-01-02 | C — Erro de citação | 🟢 Menor | Citação de linhas do `TraumaArmsConsumer.cs` (§5.3) inclui bookkeeping que não pertence ao callback, sem o mesmo aviso dado ao FallCycle | ✅ Resolvido |
| PA-01-03 | A — Gap | 🟢 Menor | Spec não avisa que o campo `_lifecycle` NUNCA pode virar `readonly` (footgun de cópia defensiva de struct) | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Verificação central desta rodada (releitura linha a linha dos 4 arquivos reais)

Antes dos pontos, registro o resultado da comparação adversarial pedida (Passo 3 do orquestrador): os 4 `Update()` reais (`TraumaLegsConsumer.cs:178-258`, `TraumaFallCycleConsumer.cs:216-260`, `TraumaArmsConsumer.cs:343-430`, `TraumaStomachConsumer.cs:116-149`) foram lidos por inteiro e comparados contra os stubs de §5.1-5.3.

- **Ordem dos 4 checks** (mundo nulo → world-swap → toggle → early-return) é **idêntica** nos 4 arquivos e bate exatamente com a ordem do `Tick()` proposto (§5.1). Nenhuma reordenação.
- **Nada roda fora do esqueleto capturado**: em nenhum dos 4, há código entre `GameWorld gw = ...` e o primeiro `if`, nem entre os `if`s, que o stub genérico não capture — confirmado por leitura completa, sem achado.
- **Equivalência do ponto de retorno**: `TraumaLegsConsumer`/`TraumaStomachConsumer` usam `if (active) { ... }` (sem `return`) enquanto o stub usa `if (!active) return;` — logicamente equivalentes porque nada roda depois desse bloco em nenhum dos dois métodos originais. `TraumaFallCycleConsumer`/`TraumaArmsConsumer` já usam `if (!active) return;` no original, idêntico ao stub. Nenhum dos 4 tem lógica que precise rodar com `gw == null` além do já capturado pelos callbacks.
- **`TraumaArmsConsumer`** (watchdog de tremor + deadline do timer ADS + hooks `HandsChanged`/`OnAimingChanged`): a poda oportunista, o watchdog (`_reestablishPending`) e o check de deadline (`_aimAnchor`) permanecem, no original E no stub, **depois** de `_wasActive = active; if (!active) return;` — ordem relativa preservada. Nenhum desses mecanismos lê `_wasActive`/`_trackedWorld` (confirmado por grep, ver PA-01-01 abaixo) — a extração desses 2 campos para dentro do struct não quebra nada fora do método `Update()`.
- **Grep `_trackedWorld|_wasActive` em `mods/TRL-ImmersiveCombatMedicine/modded/`**: as únicas ocorrências fora de `TraumaEngine.cs` (motor, fora do escopo de A4 — tem seu próprio par de campos independente, não tocado por este item) estão dentro do próprio `Update()` de cada um dos 4 consumidores. **Nenhum outro método os lê** — confirma que mover os 2 campos para dentro do `struct TraumaConsumerLifecycle` não quebra nenhum acesso externo. Ver PA-01-01 para a implicação (campos remanescentes na classe ficam mortos).
- **`TraumaStomachConsumer` — `OnToggleOn = null`**: confirmado por leitura direta (`TraumaStomachConsumer.cs:133-141`) — o bloco `if (_wasActive && !active) { CancelKind(...) }` **não tem `else if` nenhum**; a única coisa entre ele e `_wasActive = active;` é um comentário (`// Religar mid-raid: NADA a estabelecer...`). A spec técnica afirma isso corretamente — não é suposição, é o comportamento real.
- **A3 — citações do Assembly**: `PhraseSpeakerClass.Play` (`references/eft-decompiled/Assembly-CSharp/PhraseSpeakerClass.cs:176-239`) bate exatamente — assinatura `public TagBank Play(EPhraseTrigger trigger, ETagStatus tags, bool demand = false, int? importance = null)` na linha 176, chave de fechamento na 239. O predicado `Busy && importance <= Int_0` está exatamente em `:207-211`. `Player.Speaker` é campo público em `EFT/Player.cs:24347`. `EPhraseTrigger.OnAgony = 9` está na linha 6 de `EPhraseTrigger.cs`. As citações de `TraumaVoice.cs:21`/`:31` (chamadas de `PlayStrong`/`TryPlayStrong`) também batem linha a linha. **Nenhum erro de citação em A3.**
- **5º candidato a consumidor com o mesmo padrão**: existe `TraumaBlackoutTrigger.cs` na mesma pasta (item 007), mas é uma `internal static class` **stateless**, sem `MonoBehaviour`, sem `Update()`, com o próprio comentário de cabeçalho confirmando isso ("STATELESS: sem lifecycle de raid... Sem registro em TraumaConsumerRegistry"). `TraumaEngine.cs` tem seu próprio `Update()` com um par similar de campos (`_trackedWorld`), mas é o MOTOR (não um consumidor) e seu esqueleto é estruturalmente mais rico (guard de hideout, flag `_raidStarted`, ativação/desativação de tracking) — não é uma cópia do padrão dos 4 consumidores. **Confirmado: não há um 5º candidato esquecido.**

Nenhum desses pontos vira um achado formal abaixo porque todos **confirmam** a spec, não a contradizem. Os 3 achados a seguir são as únicas divergências reais encontradas.

---

## Pontos

### PA-01-01 · A — Gap · 🟡 Importante · Resolvido em 2026-07-20

**Campos `_wasActive`/`_trackedWorld` dos 4 consumidores ficam mortos após a extração, sem instrução de remoção**

**Problema:** cada um dos 4 consumidores hoje declara `_wasActive`/`_trackedWorld` como campos de instância (`TraumaLegsConsumer.cs:28-29`, `TraumaFallCycleConsumer.cs:26-27`, `TraumaArmsConsumer.cs:38-39`, `TraumaStomachConsumer.cs:17-18`). No design de A4, essa mesma bookkeeping passa a viver **dentro do `struct TraumaConsumerLifecycle`** (`_trackedWorld`/`_wasActive` privados do struct, §5.1). A spec técnica lista os campos NOVOS de cada consumidor (`_lifecycle` + 4 delegates) mas em nenhum lugar instrui a REMOÇÃO dos 2 campos antigos — e o checklist de implementação é explícito no sentido contrário: "adicionar os 6 campos novos (`_lifecycle` + 4 delegates + **nada mais**)" (§8, item 2). Seguido ao pé da letra, os 2 campos antigos de cada consumidor permanecem declarados, mas nunca mais são lidos nem escritos por `Update()` (que agora delega tudo a `_lifecycle.Tick(...)`) — confirmado por grep (nenhum outro método os referencia).

**Por que importa:** são 8 campos mortos (2 × 4 consumidores) que sobrevivem à refatoração sem propósito, gerando (no mínimo) warnings do compilador (`CS0169`/`CS0414`, campo nunca usado) em 4 arquivos que antes eram limpos — o oposto do objetivo declarado de A4 ("puramente uma limpeza de manutenibilidade"). Mais concretamente: um futuro mantenedor lendo `TraumaArmsConsumer.cs` encontraria `_wasActive`/`_trackedWorld` declarados ao lado de `_lifecycle`, sem saber que os dois primeiros são vestígios inertes — risco real de alguém "consertar" um bug futuro escrevendo nesses campos mortos, achando que ainda fazem algo. Não é um bloqueador (não muda comportamento, não quebra compilação — confirmado que não há `TreatWarningsAsErrors` no projeto), mas é uma lacuna concreta que o checklist atual (que diz literalmente "e nada mais") impede de fechar por acidente.

**Sugestão:** adicionar ao §8 (checklist) e a cada célula da tabela §4, para os 4 consumidores, a instrução explícita "remover os campos `_wasActive`/`_trackedWorld` da classe (a bookkeeping agora vive dentro de `_lifecycle`)". Trocar a frase "adicionar os 6 campos novos... e nada mais" por "adicionar os 6 campos novos **e remover os 2 campos antigos (`_wasActive`/`_trackedWorld`) de cada consumidor**".

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** §4 (tabela de arquivos) e §8 (checklist) atualizados exigindo a remoção explícita de `_wasActive`/`_trackedWorld` em cada um dos 4 consumidores.

---

### PA-01-02 · C — Erro de citação · 🟢 Menor · Resolvido em 2026-07-20

**Citação de linhas do `TraumaArmsConsumer.cs` em §5.3 inclui bookkeeping que não pertence ao callback, sem o aviso dado ao FallCycle**

**Problema:** a nota de §5.3 sobre Arms diz: *"OnWorldGone/OnWorldSwap viram, cada um, uma chamada a `TearDownLocal(reason, worldDead: true)` + `ResetLockout()` (corpo idêntico a `TraumaArmsConsumer.cs:349-354`/`358-361`, só a string de razão difere)"*. Conferindo o arquivo real: `349-354` abrange `// worldDead=true → ...` (comentário) até `_trackedWorld = null; _wasActive = IsActive(); return;` — ou seja, inclui as 3 linhas de bookkeeping (`_trackedWorld = null`, `_wasActive = IsActive()`, `return`) que na spec migram para dentro de `Tick()`, não para o callback `OnWorldGone()`. O mesmo vale para `358-361` (inclui `_trackedWorld = gw;`, linha 361, que também fica em `Tick()`). Comparando com a citação equivalente para `TraumaFallCycleConsumer` na MESMA seção (§5.3): *"corpo idêntico ao branch `gw == null` original (`TraumaFallCycleConsumer.cs:220-227`, **exceto o bookkeeping `_trackedWorld`/`_wasActive`, agora do struct**)"* — ali a spec inclui a mesma faixa "generosa" de linhas, mas adiciona explicitamente a ressalva "exceto...". A citação de Arms **não tem essa ressalva**, tornando-a a única, no documento inteiro, que cita um range de linhas incluindo bookkeeping sem avisar que ele fica de fora.

**Por que importa:** Arms é justamente o consumidor mais complexo (watchdog + timer ADS + hooks de evento), citado no próprio pedido desta revisão como merecedor de atenção redobrada — é exatamente onde a citação deveria ser mais precisa, não menos. Sem a ressalva, alguém implementando a partir só da prosa de §5.3 (Legs e FallCycle têm código completo em §5.2/§5.3; Arms só tem a nota resumida) pode copiar `_trackedWorld = null;`/`_wasActive = IsActive();`/`_trackedWorld = gw;` para dentro de `OnWorldGone()`/`OnWorldSwap()` — o que não quebra a compilação (os campos antigos ainda existem, ver PA-01-01) mas reintroduz exatamente a duplicação de bookkeeping que A4 existe para eliminar.

**Sugestão:** ajustar a nota de Arms em §5.3 para o mesmo padrão do FallCycle: *"corpo idêntico a `TraumaArmsConsumer.cs:350-351`/`359-360` (chamadas de `TearDownLocal`+`ResetLockout`), exceto o bookkeeping `_trackedWorld`/`_wasActive`/`return`, agora do struct"*.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** citação de Arms em §5.3 corrigida para excluir explicitamente o bookkeeping, com nota de "não copiar essas linhas".

---

### PA-01-03 · A — Gap · 🟢 Menor · Resolvido em 2026-07-20

**Spec não avisa que o campo `_lifecycle` de cada consumidor nunca pode virar `readonly`**

**Problema:** §1 tem uma "Nota de conformidade (csharp-mod-best-practices §5)" explicando por que o **tipo** `struct TraumaConsumerLifecycle` é deliberadamente mutável (não `readonly struct`). Mas a spec não estende o mesmo aviso ao **campo** `_lifecycle` declarado em cada um dos 4 consumidores (`private TraumaConsumerLifecycle _lifecycle;`, §5.2/§5.3). Em C#, chamar um método mutante (`Tick(...)`) num campo de struct marcado `readonly` não dá erro de compilação — o compilador silenciosamente opera sobre uma **cópia defensiva**, descartada ao fim da chamada. Se esse campo virar `readonly` (um "cuidado" plausível: `_lifecycle` nunca é reatribuído por `=` em lugar nenhum, só mutado via `Tick()` — exatamente o padrão que `csharp-mod-best-practices §5` recomenda marcar `readonly`, "campos atribuídos só no construtor"), a detecção de mundo nulo/world-swap/toggle para de funcionar **silenciosamente** (sempre opera sobre uma cópia zerada), sem exceção, sem warning de compilação — só bug de comportamento em produção.

**Por que importa:** é exatamente o tipo de "limpeza" que um revisor ou o próprio `/code-mod` de um item futuro poderia aplicar de boa-fé seguindo a regra geral de `csharp-mod-best-practices §5` (`readonly` para campos só atribuídos no construtor) sem saber da exceção. Como o próprio A4 é vendido como "zero mudança de comportamento" e "menor raio de impacto possível", vale o mesmo cuidado de documentação que já foi dado ao tipo do struct.

**Sugestão:** adicionar ao comentário XML do `struct TraumaConsumerLifecycle` (§5.1) ou a um comentário inline no campo `_lifecycle` de cada consumidor (§5.2/§5.3): *"NUNCA marcar este campo `readonly` — `Tick()` muta o struct em-place; `readonly` faria o C# operar sobre uma cópia defensiva silenciosa, quebrando toda a detecção de mundo/toggle sem erro de compilação."*

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** comentário de aviso adicionado no `struct TraumaConsumerLifecycle` (§5.1) e no campo `_lifecycle` de cada consumidor (§5.2/§5.3).
