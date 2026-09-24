# 025 — EmergencyDrop na cirurgia própria (self-heal) · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [025-emergencydrop-autocirurgia-02-spec-tech.md](025-emergencydrop-autocirurgia-02-spec-tech.md)
**Data:** 2026-09-12

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

Memória consultada: `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`, snapshot de 2026-09-12 (Sessão 13, escrita nesta sessão). Nenhuma pendência 🔴 afeta este item.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟢 Menor | Chamada a `SetPhysicalCondition(UsingMeds, false)` em `EmergencyDropSelf` sem justificativa escrita | ✅ Resolvido em 2026-09-12 |
| PA-01-02 | A — Gap | 🟢 Menor | `ReleaseSurgeryImmobilize(doctor)` é confirmadamente um no-op no contexto de auto-cirurgia | ✅ Resolvido em 2026-09-12 |
| PA-01-03 | A — Gap | 🟢 Menor | Risco de auto-descarte nativo em §7 pode ser fechado (não só flagado) com a evidência já citada em §1 | ✅ Resolvido em 2026-09-12 |

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

### PA-01-01 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Chamada a `SetPhysicalCondition(UsingMeds, false)` em `EmergencyDropSelf` sem justificativa escrita**

**Problema:** O stub de `EmergencyDropSelf` (§5) chama `doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);` — copiado do padrão de `EmergencyDrop` (item 024) — mas a spec não explica por que essa chamada é necessária num contexto de auto-cirurgia nativa, onde o mod nunca é quem LIGA essa flag (grep em `BandAidController.cs` confirma que `UsingMeds` só é setado `true` dentro de `HealRoutine`, exclusivo da cura de aliado).

**Por que importa:** Investiguei e confirmei duas coisas que a spec deveria registrar: (1) `UsingMeds` é um gate de movimento **puro** — só consumido por checagens de permissão (`CanSprint`/`CanJump`-like, `MovementContext.cs:1193,1248,1276,1316,3306`; `PlayerPhysicalClass.cs:268,671`), nunca lido pela própria máquina de estado do efeito de cura — então escrever `false` nele é seguro por natureza, não corrompe nada internamente. (2) Como `EmergencyDropSelf` pula deliberadamente o caminho nativo de fechamento (via `DestroyController()`, que ignora `Drop()`/`method_2`), é bem plausível que o código nativo que LIGARIA essa flag no início do uso do item TAMBÉM seja responsável por desligá-la no fim — e se esse "fim" normal está dentro do mesmo trecho que estamos pulando, a chamada explícita não é só defensiva, é **necessária** pra não deixar o jogador travado sem poder correr depois do drop de emergência. Sem essa explicação escrita, um futuro leitor (ou o próprio `/code-review`) pode achar que é código copiado sem necessidade e sugerir remover — o que seria um regressão real se a hipótese acima estiver certa.

**Sugestão:** Adicionar ao §7 (Riscos) um parágrafo registrando: "UsingMeds confirmado como gate de movimento puro (sem leitura pela máquina de estado do efeito) — a chamada explícita de `SetPhysicalCondition(UsingMeds, false)` é mantida por precaução, já que o código nativo que normalmente a desligaria pode estar dentro do trecho de `Drop()`/fechamento que `DestroyController()` pula deliberadamente. Validar em raid (checklist §8) que o jogador NÃO fica impedido de correr depois do drop de emergência em auto-cirurgia — se esse teste passar, a chamada está confirmada como necessária; se o jogador já recupera o `UsingMeds` sozinho de outra forma, a chamada é redundante mas inofensiva."

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Nota adicionada ao §7 e comentário inline no stub de §5 explicando que `UsingMeds` é gate de movimento puro (confirmado seguro de escrever) e que a chamada é mantida por precaução, com item de validação em raid registrado no checklist §8.

---

### PA-01-02 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-12

**`ReleaseSurgeryImmobilize(doctor)` é confirmadamente um no-op no contexto de auto-cirurgia**

**Problema:** O stub de `EmergencyDropSelf` chama `ReleaseSurgeryImmobilize(doctor)` no final, mas o próprio doc-comment desse método já afirma: *"nunca desliga um `HealingLegs` de AUTO-cirurgia (aquele contexto não passa pelo `HealRoutine`)"* (`BandAidController.cs:803-808`). Ou seja, a própria spec cita evidência que contradiz a necessidade dessa chamada aqui, sem comentar a contradição.

**Por que importa:** Não é um bug (a chamada é comprovadamente inofensiva — reset incondicional de uma flag que nunca foi ligada, mais um reset de `AllyAnimSpeedMult` que também não afeta a animação nativa de auto-cirurgia) — mas incluir uma chamada que o próprio código-fonte documenta como irrelevante nesse contexto, sem nota explicando "é de propósito, por consistência/baixo custo, não porque faz algo aqui", é uma pequena lacuna de clareza que pode confundir quem ler o código depois.

**Sugestão:** Adicionar uma linha ao comentário do stub em §5: `// ReleaseSurgeryImmobilize é um no-op aqui (HealingLegs nunca é ligado fora de HealRoutine) — mantido só por consistência com EmergencyDrop, custo desprezível.` Alternativa aceitável: remover a chamada de `EmergencyDropSelf` já que não faz nada — decisão estética, sem impacto funcional de qualquer forma.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Comentário inline adicionado ao stub de `EmergencyDropSelf` em §5 explicando que a chamada é um no-op confirmado neste contexto, mantida só por consistência de código.

---

### PA-01-03 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Risco de auto-descarte nativo em §7 pode ser fechado (não só flagado) com a evidência já citada em §1**

**Problema:** §7 lista como risco em aberto: *"não foi confirmado por leitura de código se `MedKitComponent`/itens simples podem se auto-descartar nativamente ao chegar a zero carga"* — mas o próprio trecho de `ActiveHealthController.cs:1935-1964`, já citado e transcrito em §1 desta mesma spec, mostra a lógica completa do ramo "interrompido" de `Residue()`: ela só mutila `MedKitComponent_0.HpResource` (`Mathf.Round(...)`/`Mathf.Floor(...)`), chama `Item.RaiseRefreshEvent()` e `HealthController.method_33(...)` — nenhuma chamada a discard/remove/destroy aparece nesse trecho.

**Por que importa:** Deixar isso como "não confirmado" quando a própria spec já tem a evidência que resolve a dúvida é uma oportunidade perdida de fechar completamente a análise antes do `/code-mod` — o guard defensivo (`item.CurrentAddress != null` antes do `ThrowItem`) continua correto de manter (custo zero, blinda contra qualquer coisa que a leitura não tenha coberto), mas a justificativa deveria ser "confirmado que não há discard nativo automático" em vez de "não confirmado".

**Sugestão:** Reescrever a frase de §7 de *"não foi confirmado por leitura de código se... podem se auto-descartar nativamente"* para: *"confirmado por leitura de `ActiveHealthController.cs:1935-1964` (já citado em §1) que o ramo nativo de cancelamento tardio só decrementa `HpResource` e atualiza a UI — nenhum discard/remove automático acontece nesse caminho, ao contrário do helper próprio do mod (`ConsumeSafe`/`DiscardItemNetworked`, exclusivo do caminho de aliado). O guard `item.CurrentAddress != null` antes do `ThrowItem` é mantido por consistência/defesa, não porque essa situação seja esperada aqui."**

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** §7 reescrito conforme sugerido — risco fechado com a evidência já citada, guard mantido só por consistência/defesa.

---

## Status

**0 🔴 Bloqueadores, 3/3 pontos resolvidos** — spec técnica atualizada com as 3 resoluções. Pronta pra `/code-mod`.

**Próximo passo:** para cada ponto, marcar "Aceitar sugestão" ou descrever um caminho alternativo. Depois, atualizar `02-spec-tech.md` conforme a decisão e seguir para `/code-mod`.
