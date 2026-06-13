---
name: memory-curation
description: Como manter memória cronológica de sessões de chat em `mods/<mod>/memory/sessions.md` e `memory/repo-sessions.md`. Aplicar durante `/update-memory` (escrita) E durante os commands de desenvolvimento (/create-spec, /review-spec, /create-technical-spec, /review-technical-spec, /code-mod, /code-review, /apply-code-review) no passo "Contexto de memória" (consumo — ver §14). Cobre: granularidade de timestamps GMT-3, detecção automática de pertinência por mod, regras de imutabilidade, snapshot "Estado atual" como delta (não acumulação), pendências classificadas com IDs P-N.M, lições/hipóteses descartadas, promoção de lições recorrentes, cross-references entre mods, e o que NÃO memorizar.
---

# Memory Curation

Skill para manter a memória cronológica de sessões de chat do repo. O objetivo da memória **não é registrar tudo** — é deixar uma trilha narrativa que permite a um chat futuro (potencialmente paralelo) carregar contexto em segundos, sem reler o backlog inteiro.

> Aplicar durante `/update-memory` (e qualquer atualização manual). Pareia com `repo-workflow-best-practices` (que diz onde os artefatos do backlog vivem) — a memória é o **narrativo** desses artefatos, não duplicação deles.

## 1. Estrutura de arquivos

Dois níveis:

| Caminho | Escopo | Quando entra aqui |
| --- | --- | --- |
| `mods/<mod>/memory/sessions.md` | Específico do mod | Trabalho em `mods/<mod>/modded/`, `mods/<mod>/backlog/`, ou decisões diretamente sobre esse mod. |
| `memory/repo-sessions.md` (raiz) | Repo-wide | Mudanças em `.claude/`, `.agents/`, `scripts/`, ou em commands/skills/templates que afetam o fluxo de todos os mods. |

Uma sessão real frequentemente toca **ambos** os níveis (ex.: criar `/code-review` é repo-wide, mas o mod-cobaia teve atividade própria). Nesses casos:

- Trabalho repo-wide vai em `memory/repo-sessions.md`.
- Trabalho mod-específico vai no `mods/<mod>/memory/sessions.md`.
- **Não duplicar**: o mod-específico cita "ver `memory/repo-sessions.md` 2026-05-11 para infra que afetou este trabalho", e vice-versa.

## 2. Timestamps GMT-3 — HH:MM obrigatório

**Toda entrada de sessão DEVE incluir hora e minuto** (`HH:MM`) além da data. Granularidade fina é o que permite ordenação correta entre chats paralelos no mesmo dia (ver §10).

Formato:

```text
## YYYY-MM-DD HH:MM (GMT-3) — Sessão N[<letra>]: <título resumido>
```

Exemplo: `## 2026-05-11 14:23 (GMT-3) — Sessão 4b: F4 fix patch target`.

Regras:

- **Para entrada nova (sessão atual):** obter hora via `Bash date '+%Y-%m-%d %H:%M'`. **Não estimar** se está rodando o command — o horário é o do relógio do sistema no momento da gravação.
- **Para backfill (entradas antigas sem registro de hora):** usar `~HH:MM` com **tilde** indicando estimativa baseada na ordem da conversa (ex.: `## 2026-05-10 ~14:00 (GMT-3)`). Tilde é o sinal honesto de aproximação.
- **NUNCA tilde para entrada nova.** Se está gravando ao vivo, o relógio é a fonte de verdade.
- **Não pular o minuto.** Mesmo se o trabalho parece curto, gravar `HH:MM` exato — auxilia merge cronológico de chats paralelos.
- **Numeração de sessão (`Sessão N`):** crescente por mod, reseta nunca. Permite cross-ref estável ("ver Sessão 2 da memory de X").

## 3. Detecção automática de pertinência por mod

Quando `/update-memory` rodar sem argumento, classificar cada trecho da conversa por mod usando esta hierarquia:

1. **Path explícito (peso alto):** linhas/edits/reads/grep que mencionam `mods/<X>/...` → trecho pertence ao mod `<X>`.
2. **Command direcionado (peso alto):** `/code-mod <X>`, `/compile-mod <X>`, `/add-backlog-item <X>`, `/code-review <X>`, etc. Define o "mod ativo" do momento até outro command direcionado mudar.
3. **Menção textual (peso médio):** discussões que citam o nome do mod por extenso. Útil quando a conversa é teórica antes de aterrissar no código.
4. **Trabalho meta-repo (peso alto):** edits em `.claude/`, `.agents/`, `scripts/`, ou criação/modificação de commands/skills/templates → vai para `memory/repo-sessions.md`, NÃO para nenhum mod. Em cada mod tocado na mesma sessão, deixar pointer "infra repo-wide nesta sessão — ver `memory/repo-sessions.md` 2026-05-11".
5. **Ações não-mod, não-repo (peso baixo):** debug de plugin externo (não nosso), update de inventário, conversa solta. Descartar OU, se foi consequente, registrar em uma seção `## Notas relevantes (não-mod)` do mod em foco no momento.

**Quando ambíguo:** preferir o último mod com command direcionado (peso 2 winner). Se mesmo assim incerto, perguntar ao usuário em vez de chutar.

## 4. O que entra e o que NÃO entra

### Entra (sempre)

- **Decisões com porquê.** "Trocamos patch target porque virtual dispatch bypassa Harmony (1 de 14 overrides chama base)" — futuras sessões precisam do raciocínio, não só do resultado.
- **Bugs encontrados e como foram diagnosticados.** Inclui hipóteses descartadas — "tentamos X, falhou porque Y". Evita re-debugar.
- **Refs a arquivos com `file:linha`.** Cada claim técnico tem ancoragem clicável.
- **Pendências reais** (algo está incompleto, esperando validação, ou foi adiado).
- **Cross-refs a artefatos do backlog** (`05-asbuild.md`, `06-fix-NN.md`, `04-code-review-NN.md`). Memória aponta; não copia.
- **Resoluções de pendências anteriores** com cross-link para a sessão que abriu (ver §7).

### NÃO entra (sempre)

- **Tool calls que falharam por sintaxe e foram retentadas com sucesso** — ruído.
- **Exploração que não levou a nada** — apenas dilui sinal.
- **Conteúdo de commit messages.** Memória ≠ `git log`. Se está no commit, basta linkar.
- **Spec funcional / técnica / asbuild verbatim.** Esses são os artefatos. Memória é o narrativo sobre eles.
- **Diálogo casual, agradecimentos, retomadas de contexto.**
- **Conteúdo de logs de sistema** (avisos MD060 do linter, etc.) — sem aprendizado, sem valor.

Regra prática: se a entrada não ajuda a próxima sessão a **tomar uma decisão melhor** ou **economizar tempo**, ela não pertence à memória.

## 5. Estrutura de uma entrada de sessão

Template:

```markdown
## YYYY-MM-DD HH:MM (GMT-3) — Sessão N[<letra>]: <título resumido>

**Tema central:** [1 linha — qual era a meta principal]

**Decisões-chave:**
- [Decisão 1]: <o quê> — <por quê>. Ref: <artefato/file:linha>.
- [Decisão 2]: ...

**Lições / hipóteses descartadas:**
- <hipótese ou abordagem testada> — falhou/foi descartada porque <causa raiz>. Ref: <artefato/file:linha>.
- (Se a sessão não gerou lição: escrever `Nenhuma lição nova — sessão de <tipo>.` — ausência silenciosa não é permitida.)

**Atividade cronológica:**
1. <ação> — <resultado>.
2. <ação> — <resultado>.
3. ...

**Pendências abertas nesta sessão** (se houver):
- [P-N.M] <descrição>. Categoria: <bloqueador / débito / ideia>. (Ver §7)

**Cross-refs:**
- Resolve pendências [P-X.Y] da sessão `<data>` (se aplicável).
- Aponta para `memory/repo-sessions.md` Sessão K (se houve trabalho repo-wide).
```

**Decisões-chave vem antes da cronologia.** O futuro leitor quer saber "o que mudou e por quê", não "em que ordem isso aconteceu". Cronologia é apoio.

**A seção "Lições / hipóteses descartadas" é obrigatória.** É a diferença entre memória ("por que NÃO fazer X") e diário ("fizemos X"). Red flag: sessão que tocou código sem nenhuma decisão com "— porquê" e sem lição registrada — voltar à conversa e extrair o raciocínio antes de gravar.

## 6. Snapshot "Estado atual" — delta, não acumulação

No **topo** de cada `sessions.md`, manter um bloco curto:

```markdown
## Estado atual (snapshot ao fim da última sessão)

- <fato chave 1>
- <fato chave 2>
- ...

## Pendências / próximos passos conhecidos

- <pendência 1>
- ...
```

Regras:

- **Snapshot reflete o ESTADO ATUAL, não a história.** Substituir os bullets a cada atualização. Não acumular.
- **Pendências:** listar apenas as ainda abertas. Resolvidas vão pra Histórico da sessão que resolveu (com ✅).
- **Manter ≤ 10 bullets em cada bloco.** Se está crescendo, é sinal de que algo precisa virar artefato (06-fix, item de backlog) em vez de pendência permanente.

## 7. Pendências — classificação tri-camada + garbage collection

Toda pendência ganha **ID local** `P-<NNNN>.<MM>` (NNNN = sessão, MM = ordem) e **categoria**.

**Enforcement de IDs (sem exceção):**

- Todo bullet do bloco "Pendências" no **topo** carrega o ID `[P-N.M]` herdado da sessão que o criou. Ao reescrever o topo (delta, §6), **preservar os IDs existentes**.
- Bullets legados sem ID: atribuir ID retroativo `[P-N.M]` na próxima gravação, usando a sessão de origem (rastreável pelo conteúdo). Isso NÃO viola a imutabilidade do §8 — o topo é mutável por definição (§6); só as entradas de sessão são append-only.
- Sem ID não há cross-ref estável ("resolve P-3.2") — bullet sem ID é red flag no checklist final.

| Categoria | Significado | Vida útil |
| --- | --- | --- |
| 🔴 **Bloqueador** | Próxima ação no mod depende disso. Resolver antes de seguir. | Curta — resolver na próxima sessão ou viraria 🟡. |
| 🟡 **Débito técnico** | Funciona mas tem dívida (perf, code smell, doc incompleta). | Média — pode ficar até virar problema. |
| 🟢 **Ideia / sugestão** | Melhoria opcional, não-essencial. | Indefinida — promover a item de backlog se ficar relevante. |

Quando uma pendência é resolvida:

1. Marcar `✅ Resolvido em YYYY-MM-DD — <link para sessão que resolveu>`.
2. Mover para o final do `Histórico` da sessão que resolveu.
3. Remover do bloco "Pendências" no topo.

**Garbage collection (executado pelo `/update-memory` a cada rodada):** pendência >30 dias sem progresso entra em revisão obrigatória — o command propõe ao usuário: **promover** a item de backlog (`/add-backlog-item`), **descartar** (com nota explícita "descartada por X") ou **manter** (com justificativa registrada). Nenhuma pendência fica >30 dias sem decisão explícita.

## 8. Imutabilidade + apêndice

Mesma regra do `repo-workflow-best-practices §5`: **entradas de sessões anteriores não são reescritas**. Append-only.

Se um fato de uma sessão antiga foi posteriormente refutado (ex.: "F4 funciona" → "F4 não funciona após teste"), **não editar** a entrada antiga. Em vez disso, na sessão atual:

```markdown
**Revisão de fato anterior:** Sessão 2 (2026-05-10) registrava "F4 funciona após apply-code-review";
teste in-raid mostrou que não. Causa raiz em [`06-fix-01.md`](...). Histórico preservado para
rastreabilidade.
```

Isso evita revisionismo silencioso (que destrói confiança na memória).

## 9. Cross-references entre mods

Quando uma sessão tocou mod A e mod B:

- Entry em `mods/A/memory/sessions.md`: descreve o trabalho em A. No fim, adicionar:
  ```markdown
  **Trabalho paralelo neste dia em outro mod:** ver `mods/B/memory/sessions.md` 2026-05-NN.
  ```
- Entry em `mods/B/memory/sessions.md`: descreve o trabalho em B. Mesma pointer cruzada.

**NUNCA duplicar conteúdo entre mods.** Cada arquivo é fonte única para o que aconteceu no seu mod; cross-refs são pointers.

## 10. Chats paralelos no mesmo dia — merge por posicionamento cronológico

Quando duas sessões paralelas atualizam o mesmo mod no mesmo dia:

- **NÃO mesclar parágrafos.** Cada chat escreve a sua entrada, com seu próprio conteúdo, sem fundir o texto.
- **Mesclar pela POSIÇÃO no arquivo.** O command lê o arquivo existente, identifica entradas do mesmo dia, e insere a nova entrada na **ordem cronológica correta** (por timestamp GMT-3 do início do trabalho, não da gravação).

Exemplo:

- Chat A trabalhou às 09:15, rodou `/update-memory` às 10:30 → cria `## 2026-05-11 10:30 (GMT-3) — Sessão 3a: F4 debug`. (Horário gravado = momento do `/update-memory`, fonte = `date` do sistema.)
- Chat B trabalhou às 13:00-14:00, rodou `/update-memory` às 15:00 → cria `## 2026-05-11 15:00 (GMT-3) — Sessão 3b: ADS investigation`. Insere **DEPOIS** de 3a (15:00 > 10:30).

Caso inverso (raro, mas possível com sessões longas ou retomadas):

- Chat A rodou `/update-memory` às 11:45 → `Sessão 3a (2026-05-11 11:45 GMT-3)`.
- Chat B começou cedo mas só rodou `/update-memory` às 12:10 → `Sessão 3b (2026-05-11 12:10 GMT-3)`. Como 12:10 > 11:45, insere DEPOIS. Sub-letra 3b reflete ordem de gravação.

Para casos de "trabalhei cedo mas só registrei tarde" e o usuário quer reordem manual: aceitar timestamp `~HH:MM` (tilde) **só se o usuário fornecer explicitamente**. O command não infere.

Regras:

- **Sub-letras (`3a`, `3b`, …) refletem ordem de gravação, não posicionamento.** Permite cross-ref estável (ex.: "ver Sessão 3b da pendência X") sem renumerar tudo cada vez.
- **Posicionamento visual é por timestamp GMT-3.** Pode haver mismatch (3b aparece antes de 3a no arquivo). Tudo bem — sub-letra é ID, timestamp é ordem.
- **Append-only nas entradas existentes:** o segundo chat NUNCA edita texto da entrada do primeiro. Só insere a sua.
- **Mesma sub-letra base** para o dia (3a, 3b, 3c, ...); próximo dia novo começa em 4.
- **Divergência factual:** se os dois chats discordarem sobre algum fato (ex.: estado de uma feature), registrar a divergência explicitamente — o usuário arbitra. Não tentar "resolver".

**Quando ausente o timestamp do início do trabalho:** usar o timestamp de gravação como aproximação (ordem natural). Não fabricar precisão.

## 11. Densidade de refs

Toda afirmação sobre código deve ter ancoragem:

- **Bom:** "Patch target trocado para `Player.FirearmController.SetTriggerPressed` (ref: [Player.cs:13668](...))."
- **Ruim:** "Patch target trocado para o método da FC."

Toda decisão de design deve ter link para o artefato que captura:

- **Bom:** "F4 reescrito via [06-fix-01.md](...)."
- **Ruim:** "F4 reescrito."

Memória sem refs vira folclore — afirmação sem prova, futuro leitor não consegue verificar.

## 12. Casos especiais

- **Sessão sem mudança em código (só leitura / discussão):** registrar apenas se houve **decisão tomada** ou **descoberta relevante**. Sessão de "explorei o código e não fiz nada" não entra.
- **Backlog item criado mas não implementado:** entra como pendência 🟢, não como sessão completa.
- **Build/compile sem mudança de código:** não vira sessão. Mencionar em uma sessão existente se houve aprendizado (ex.: erro de compilação revelou bug).
- **Reversão de decisão:** registrar a reversão com link para a sessão original que tomou a decisão. Não editar a sessão original.

## 13. Sugestões mais inteligentes (além da premissa do usuário)

Adições que valem como prática:

1. **Forward-pointers em pendências.** Quando uma sessão cria pendência `P-3.2`, deixa link inline `(será endereçado quando X acontecer)`. Quando outra sessão fecha, marca `✅` com link para a sessão de fechamento. Bidirecional.

2. **Decision-first ordering.** Decisões-chave aparecem ANTES da cronologia em cada sessão. Otimiza skim — leitor vê o "o quê / por quê" sem precisar reconstruir do histórico.

3. **Cross-mod see-also vs duplicação.** Já coberto em §9, mas reforçar: a tentação de "copiar contexto" entre mods cria drift. Sempre cross-ref.

4. **Repo-wide em arquivo próprio.** `memory/repo-sessions.md` evita poluir memórias mod-específicas com infra. Mod aponta; não absorve.

5. **Snapshot diff format (opcional para sessões longas).** Quando o "Estado atual" muda muito numa sessão, antes de reescrever os bullets, listar o delta:
   ```markdown
   ### Delta vs. último snapshot
   - REMOVIDO: "F4 não validado in-raid"
   - ADICIONADO: "F4 funcionando — validado em raid no 06-fix-01"
   ```
   Ajuda leitores rápidos.

6. **Hard reference density.** §11 já cobre — afirmação técnica sem `file:linha` é red flag.

7. **Anti-revisionismo (§8).** Reviews/sessões passadas são imutáveis. Refutações vão em sessão atual com link.

8. **Pendência → backlog promotion path.** Pendência 🟢 que persiste por 2+ sessões deve virar item de backlog (`/add-backlog-item`). Memória é tracking efêmero; backlog é compromisso.

9. **"Trabalho paralelo neste dia" marker.** Sempre que houver atividade em outro mod no mesmo dia, marker no fim da entrada com pointer. Útil para entender por que algo levou X tempo (o dia foi distribuído entre mods).

10. **Numeração de sessão por mod (não global).** `Sessão 3` no stances é independente de `Sessão 3` no SPT-Realism. Permite que cada mod tenha cadência própria.

11. **Idempotência de `/update-memory`.** Rodar 2x na mesma conversa não duplica entradas — o command detecta o que já foi gravado e só anexa o delta (ou avisa "nada novo a registrar").

12. **Dry-run flag.** `/update-memory --dry` mostra o que seria escrito sem efetivar. Espelha `migrate-backlog-naming.sh --dry`. Permite revisar antes.

## 14. Consumo de memória por commands

A memória só paga seu custo se for LIDA. Os commands de desenvolvimento (`/create-spec`, `/review-spec`, `/create-technical-spec`, `/review-technical-spec`, `/code-mod`, `/code-review`, `/apply-code-review`) executam um passo **"Contexto de memória"** no início. Regras:

**O que ler (nesta ordem — sem reler o arquivo inteiro):**

1. Topo de `mods/<mod>/memory/sessions.md` — blocos "Estado atual" e "Pendências / próximos passos conhecidos" (≈ primeiras 30-40 linhas).
2. Entradas que mencionam o item: Grep por `<NNN>-`, pelo slug e por nomes de feature do item. Ler só as entradas que casam.
3. Se nada casa (item novo): ler apenas a última entrada de sessão.
4. Se `sessions.md` não existe: seguir normalmente e registrar "sem memória prévia" no relatório.

**O que fazer com pendências:**

- 🔴 **do item em questão** (ou que bloqueia o mod inteiro — ex.: build quebrado, validação in-game pendente do mesmo código que será tocado): **alertar o usuário antes de prosseguir** e perguntar se continua. Não é bloqueio automático — é decisão humana informada.
- 🟡 **relacionadas ao mesmo sistema/arquivo:** citar como risco no artefato (spec técnica §7 "Riscos e dependências"; review como evidência de ponto).
- 🟢: mencionar só se diretamente relacionada à tarefa.

**Output obrigatório do passo** — 2-5 linhas no início do trabalho do command:

```text
Memória consultada: snapshot de YYYY-MM-DD (Sessão N).
Pendências que afetam esta tarefa: [P-N.M <resumo>] / nenhuma.
```

**Regra de evidência:** memória aponta, artefato confirma — nenhum claim técnico vindo da memória entra num artefato sem reconferir o `arquivo:linha` atual (o código pode ter mudado desde a gravação).

## 15. Promoção de lições para skills/antipatterns

Memória é tracking efêmero; regra recorrente vira conhecimento institucional. Critérios:

- Mesma classe de erro/lição aparece em **≥2 sessões** (mesmo mod ou mods diferentes) → candidata a promoção.
- Classe já existe em `docs/technical/spt-antipatterns.md` → adicionar o exemplo/ref novo lá (na seção AP-NN correspondente).
- Classe nova de **domínio SPT/EFT** → nova seção AP-NN no `spt-antipatterns.md` + avaliar regra na skill `spt-mod-best-practices`.
- Pitfall de **linguagem C#/Unity** → skill `csharp-mod-best-practices`.
- A memória mantém o narrativo e ganha link para o destino promovido — **não duplica** o conteúdo.

Fluxo: o `/update-memory` **PROPÕE** a promoção (bloco `💡 Candidata a promoção` no relatório); o usuário aprova; a edição do doc/skill é trabalho repo-wide (registrar em `memory/repo-sessions.md`).

## Checklist final (usar antes de commit)

Ao escrever/atualizar uma entrada de memória:

1. **Cabeçalho:** data GMT-3, número de sessão, título resumido.
2. **Decisões-chave antes da cronologia:** 1-5 bullets com "o quê / por quê / ref".
3. **Cronologia:** passos em ordem, com resultados.
4. **Pendências:** categorizadas (🔴/🟡/🟢), com IDs locais.
5. **Cross-refs:** se trabalho paralelo em outro mod, pointer. Se trabalho repo-wide, pointer para `memory/repo-sessions.md`.
6. **Refs densos:** toda claim técnica tem `file:linha`.
7. **Snapshot atualizado no topo:** "Estado atual" e "Pendências" refletem o FIM da sessão, não acumulam.
8. **Pendências resolvidas:** removidas do topo, marcadas ✅ na sessão que resolveu.
9. **Sem revisionismo:** entradas antigas não editadas. Refutações em sessão atual.
10. **Sem duplicação:** conteúdo de spec/asbuild/review não copiado — só linkado.
11. **Lições:** seção "Lições / hipóteses descartadas" presente — com conteúdo real ou justificativa explícita de ausência (§5).
12. **IDs de pendência:** todo bullet de pendência (no topo e nas entradas) com `[P-N.M]`; legados receberam ID retroativo (§7).
13. **Tamanho do snapshot:** ≤10 bullets por bloco (alerta); >15 → parar e consolidar/promover antes de gravar (§6).
14. **GC:** nenhuma pendência >30 dias sem decisão explícita (promover/descartar/manter justificado) (§7).

Se algum item falha, parar e corrigir antes de marcar a entrada como concluída.