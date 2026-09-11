# 005 — Hook genérico de velocidade da animação de cura observada · Review Técnica 01

**Mod:** FIKA
**Spec técnica revisada:** [005-hook-velocidade-cura-observada-02-spec-tech.md](005-hook-velocidade-cura-observada-02-spec-tech.md)
**Data:** 2026-09-08

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4

Memória consultada: snapshot 2026-09-08 (Sessão 4, `mods/FIKA/memory/sessions.md`) · pendências que afetam este item: nenhuma (P-3.1/P-4.1 são de itens anteriores, sem relação com este). Doc técnico lido (gatilho sempre): `spt-antipatterns.md` — nenhuma contradição encontrada; `fika-packet-desync-prevention-plan.md` não se aplica (o item explicitamente não declara `INetSerializable`, §9 check 11 já justifica N/A corretamente).

Assembly-evidence conferida: `ObservedMedsController.cs:133-143` (`ObservedStart`) e `:154-194` (`HealthController_EffectRemovedEvent`) batem exatamente com o arquivo real, tanto em `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs` quanto na referência vendorizada (diff vazio, reconfirmado nesta review). `FikaPlugin.FikaLogger` (citado no stub via `FikaPlugin.Instance?.FikaLogger`) confirmado em `FikaPlugin.cs:53-61` — é uma propriedade de instância pública que envelopa o `Logger` do BepInEx; o acesso `Instance?.FikaLogger` no stub está correto.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A | 🟡 Importante | Contrato de ciclo de vida do `Register()`/assinatura não documentado | ✅ Resolvido 2026-09-09 |
| PA-01-02 | B | 🟡 Importante | `ResolveExtra` não valida `player`/`item` nulos antes de invocar o hook | ✅ Resolvido 2026-09-09 |
| PA-01-03 | C | 🟢 Menor | Comentário `TODO confirmar` sobre `FikaLogger` está resolvido — remover antes do `/code-mod` | ✅ Resolvido 2026-09-09 |
| PA-01-04 | B | 🟢 Menor | Stub reformata o corpo de `HealthController_EffectRemovedEvent` (ifs de 1 linha vs. original com chaves) | ✅ Resolvido 2026-09-09 |

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

### PA-01-01 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-09

**Contrato de ciclo de vida do hook não documentado**

**Problema:** A spec funcional (critério "Estado entre raids") diz "quem atribui o hook é responsável pelo próprio ciclo de vida do que retorna", mas nem a spec funcional nem a técnica dizem **quando** um consumidor deve chamar a atribuição — uma vez por sessão (`Awake`, session-scoped) ou por raid (`OnGameStarted`)? Isso importa porque `ExtraSpeedMultiplier` é um campo `static` que sobrevive entre raids por natureza (não há hook de raid-start/raid-end em `ObservedMedsController.cs`) — se um consumidor futuro assumir (por engano) que precisa re-registrar a cada raid e usar `OnGameStarted`, e esse hook nunca disparar em algum contexto (ex.: hideout), o campo fica `null` justamente quando deveria estar setado.

**Por que importa:** Sem o contrato explícito, cada mod consumidor pode interpretar o ciclo de vida de um jeito diferente — um problema de integração typical entre mods independentes, e exatamente o tipo de ambiguidade que a doc de um hook público deveria fechar (é a única "API" que este item entrega).

**Sugestão:** Adicionar uma frase explícita na doc XML de `ObservedMedsSpeedHook` (no stub, §5) e na spec funcional: *"Atribuir `ExtraSpeedMultiplier` uma única vez, no `Awake()` do mod consumidor (ou quando o mod detectar que o Fika está carregado) — o campo é `static` e vale pra sessão inteira do processo, não por raid. Não há necessidade (nem hook disponível aqui) para re-atribuir por raid."* Isso também orienta a spec técnica do 090 do CustomClasses, que já assume isso implicitamente mas não declara.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Doc XML de `ObservedMedsSpeedHook` na spec técnica (§5) ganhou o parágrafo `<b>Ciclo de vida (PA-01-01):</b>` explicando exatamente isso.

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-09

**`ResolveExtra` não valida `player`/`item` nulos antes de invocar o hook**

**Problema:** No stub de `ObservedMedsSpeedHook.ResolveExtra` (§5), o `try/catch` protege contra o delegate LANÇAR, mas passa `player`/`item` direto pro delegate sem checar se são `null` antes. Hoje os 2 call-sites (`ObservedStart`/`HealthController_EffectRemovedEvent`) sempre têm `_observedMedsController._fikaPlayer` e `.Item` não-nulos nesse ponto do fluxo — mas isso é um invariante do código ATUAL, não documentado nem garantido pelo contrato do hook. Se um 3º call-site for adicionado no futuro (ou o Fika mudar a ordem de inicialização), um `item == null` (ex.: arma trocada no meio de uma transição) chegaria cru no delegate de um mod consumidor, que pode não esperar isso.

**Por que importa:** Um consumidor descuidado (`item.TryGetItemComponent(...)` sem checar null primeiro) tomaria `NullReferenceException` — o `try/catch` do `ResolveExtra` PEGA essa exceção (então não quebra a animação), mas emite um `LogError` a cada cura, poluindo o log de forma enganosa (parece bug do consumidor, mas a causa real é a falta de uma garantia de contrato do hook).

**Sugestão:** Em `ResolveExtra`, adicionar um guard antes do `try`: `if (player == null || item == null) return 1f;` — silencioso, sem log (não é uma falha, é o hook simplesmente não se aplicando a um estado onde não faz sentido). Documentar na doc XML que o hook só é chamado com argumentos não-nulos hoje, mas que consumidores devem tratar como *possivelmente nulo* por robustez.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Guard `if (player == null || item == null) return 1f;` adicionado no topo de `ResolveExtra` (§5), antes do lookup do delegate. Doc XML de `ExtraSpeedMultiplier` atualizada com a nota de robustez.

### PA-01-03 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-09-09

**Comentário `TODO confirmar` sobre `FikaLogger` já está resolvido**

**Problema:** O stub de `ObservedMedsSpeedHook` (§5) tem `private static ManualLogSource? Log => FikaPlugin.Instance?.FikaLogger; // TODO confirmar: nome real do logger estático do plugin`. Conferido nesta review: `FikaPlugin.cs:53-61` confirma que `FikaLogger` é uma propriedade de instância pública válida (`public ManualLogSource FikaLogger => Logger;`), e `FikaPlugin.Instance` é o singleton estático (`FikaPlugin.cs:53`). O acesso no stub está **correto** — o `TODO` é falso-alarme, não um erro real.

**Por que importa:** Deixar o `TODO confirmar` na spec passaria a falsa impressão, no `/code-mod`, de que esse ponto ainda precisa de investigação — trabalho duplicado.

**Sugestão:** Remover o comentário `// TODO confirmar: ...` da linha do stub antes do `/code-mod` — a linha já está correta como está.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Comentário trocado por `// confirmado: FikaPlugin.cs:53-61 (...)` no stub (§5).

### PA-01-04 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-09

**Stub reformata o corpo de `HealthController_EffectRemovedEvent`**

**Problema:** O stub em §5 reescreve os 3 primeiros `if` de `HealthController_EffectRemovedEvent` no estilo `if (...) return;` de uma linha, enquanto o arquivo real (`ObservedMedsController.cs:157-170`) usa chaves em blocos de 3 linhas cada. Funcionalmente idêntico, mas se implementado literalmente como está no stub, o diff contra o arquivo original fica maior do que precisa (reescreve linhas que não mudaram), dificultando revisar o PR e futuros merges com upstream do Fika.

**Por que importa:** Qualidade/manutenção — não afeta comportamento, mas aumenta o custo de revisão e de reconciliar com atualizações futuras do Fika upstream (prática já sensível neste mod, dado que é um fork ativamente mantido).

**Sugestão:** Ao implementar (`/code-mod`), preservar o formato de chaves original do método e adicionar SÓ as 2 linhas novas (a chamada a `ResolveExtra` + a troca de `SetUseTimeMultiplier(1f + mult)` por `SetUseTimeMultiplier((1f + mult) * extra)`), em vez de reescrever o método inteiro como no stub.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Stub de `HealthController_EffectRemovedEvent` reescrito em §5 preservando o formato de chaves original (blocos de 3 linhas), com nota explícita de que só 1 linha muda de verdade.
