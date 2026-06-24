# Camera Flip (gimbal) Fix — Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11 (`modded`)
**Escopo revisado:** fix do bug "câmera de cabeça pra baixo ao aplicar stance" em [ApplyComplexRotationPatch.cs](Patches/ApplyComplexRotationPatch.cs) e [ApplySimpleRotationPatch.cs](Patches/ApplySimpleRotationPatch.cs)
**Data:** 2026-06-20
**Natureza:** hotfix fora do pipeline SDD (não há `01-spec`/`02-spec-tech`/`05-asbuild`; critérios de aceite derivados do bug reportado pelo usuário). Sandbox real é `modded/` (linha paralela do dev rocket), não `modded/`.

> Análise crítica usando `spt-mod-best-practices`, `csharp-mod-best-practices` e `repo-workflow-best-practices`. Cada achado tem ID `CR-01-MM` permanente.

## Critérios de aceite (derivados do bug) × status

| # | Critério | Status |
| --- | --- | --- |
| AC1 | Ao aplicar qualquer stance, a câmera **não** vira de cabeça pra baixo (1ª pessoa) / braços não sobem absurdo (3ª pessoa) | ✅ no código (batente ±60°) · ⏳ **pendente validação in-game** |
| AC2 | Comportamento **consistente entre todos os players**, independente de hardware/FPS | ⚠️ Parcial — cobre fonte interna (mola); **não** cobre `_temporaryRotation` adulterado por outro mod · ⏳ pendente |
| AC3 | Preserva a "quicada"/overshoot configurável (não virou Slerp seco) | ✅ overshoot preservado até o batente |
| AC4 | Não regride mount / hold breath / ADS kick / wiggle | ✅ mudança isolada na interpolação · ⏳ pendente validação |
| AC5 | Compila sem erros | ✅ confirmado (0 erros, `dotnet build -c Release`) |

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 2 · 🟡 Médios: 4 · 🟢 Menores: 2 · Total: 8

**Veredito:** sem bloqueadores de código — o fix é seguro e instalável **como rede de segurança**. Porém **não pode ser considerado "resolvido"** até validação in-game (memória **P-4.1**): a análise de estabilidade (ver CR-01-01) mostra que a mola com `damping` default é estável, então o batente é a real garantia, e a causa raiz exata segue não confirmada. Achado mais acionável: **CR-01-02** — o projeto já tem `SpringMath.SpringDamp` (estável e com overshoot), que resolveria a raiz melhor que o sub-stepping.

## Índice

| ID | Cat | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | C | 🟠 | Causa raiz não confirmada in-game; fix é defensivo, não cirúrgico | Pendente |
| CR-01-02 | D | 🟠 | `SpringMath.SpringDamp` já existe e é estável — patches reimplementam mola Euler | Pendente |
| CR-01-03 | D/B | 🟡 | Estado estático dos springs não reseta entre raids | Pendente |
| CR-01-04 | C | 🟡 | Falta o guard `CurrentState.Name == 21` que o Realism tem | Pendente |
| CR-01-05 | A/B | 🟡 | Postfix dos 2 patches sem `try/catch` (hot path) | Pendente |
| CR-01-06 | D | 🟡 | Fix fora do pipeline (`modded`) + DLL versionada divergente + build não-padrão | Pendente |
| CR-01-07 | F | 🟢 | `ClampMagnitude` (sqrt) por sub-step no hot path | Pendente |
| CR-01-08 | E | 🟢 | Três implementações de interpolação coexistem sem nota | Pendente |

---

## Pontos

### CR-01-01 · C — Validação/Causa raiz · 🟠 Forte

**Causa raiz não confirmada in-game; o fix é defensivo (batente), não cirúrgico**

**Local:** [Patches/ApplyComplexRotationPatch.cs](Patches/ApplyComplexRotationPatch.cs) (`SpringLerpAngle` + `ClampAngles`)

**Problema:** A hipótese de trabalho foi "mola Euler diverge dependente de FPS → gimbal flip". Mas a análise de estabilidade do integrador atual (Euler explícito com `damping=12` default) dá determinante `D = 1 − damping·dt` e autovalores complexos de módulo `√D` < 1 para qualquer FPS jogável — **ou seja, com config default a mola é estável e não diverge sozinha**. Logo o flip só ocorre se: (a) `damping` local for muito baixo (config não sincronizada de fato), (b) houver spike de `dt` na transição, (c) houver estado residual entre raids (CR-01-03), ou (d) o multiplicando `_temporaryRotation` (weapRotation) vier adulterado por outro mod. O fix instalado neutraliza (a)/(b)/(c) via sub-stepping + batente ±60°, mas **não** cobre (d).

**Por que importa:** Sem confirmar a fonte, não dá pra cravar que o bug acabou — só que o sintoma extremo (180°) ficou contido. Casa com a pendência **P-4.1** (memória): nada validado in-game.

**Sugestão:** Validar nos 2 PCs afetados: aplicar stance e checar `BepInEx/LogOutput.log`. (1) Câmera estável **e** aparece `[STANCE-CLAMP]` → confirmada a divergência/overshoot da mola (fonte interna). (2) Câmera estável **sem** `[STANCE-CLAMP]` → o sub-stepping já bastou. (3) Câmera **ainda** vira → fonte externa (`_temporaryRotation`/outro mod); pedir o log + modlist desse player e investigar conflito de Harmony em `ApplyComplex/SimpleRotation`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · D — Duplicação/Reuso · 🟠 Forte

**`SpringMath.SpringDamp` (estável, com overshoot) já existe — os patches reimplementam mola Euler inline**

**Local:** [SpringMath.cs:16](SpringMath.cs#L16) vs [Patches/ApplyComplexRotationPatch.cs](Patches/ApplyComplexRotationPatch.cs) e [Patches/ApplySimpleRotationPatch.cs](Patches/ApplySimpleRotationPatch.cs) (`SpringLerpAngle`/`SpringLerp`/`ClampAngles` duplicados nos dois)

**Problema:** O mod tem **três** implementações de interpolação de stance: (1) mola Euler explícita inline nos dois patches locais — a que recebeu o fix; (2) `Mathf.SmoothDampAngle` em [Networking/ObservedStanceAnimator.cs:51](Networking/ObservedStanceAnimator.cs#L51) (peers Fika); (3) `SpringMath.SpringDamp` — **solução analítica exata** do oscilador amortecido (closed-form `exp/cos/sin`), incondicionalmente estável e com overshoot configurável via `dampingRatio < 1` — **não usada por ninguém**. O fix aplicado (sub-stepping + clamp) é uma mitigação numérica de uma abordagem que o próprio projeto já resolveu corretamente. Além disso, `SpringLerpAngle`/`SpringLerp`/`ClampAngles` + constantes ficaram **duplicados** byte-a-byte nos dois patches.

**Por que importa:** `SpringDamp` mata a divergência na raiz (não há `dt` que a desestabilize) e preserva a quicada (AC3) — tornaria o batente uma salvaguarda, não a cura. Manter a Euler inline duplicada é dívida: dois lugares pra corrigir, e a inconsistência com os peers (que já usam SmoothDamp) produz transições visuais diferentes entre 1ª e 3ª pessoa.

**Sugestão:** Após validar (CR-01-01), migrar `SpringLerpAngle` → `SpringMath.SpringDamp(current, target, ref vel, dampingRatio, omega, dt)`, mapeando os configs: `dampingRatio` a partir de `_StanceOvershootDamping` (normalizar p/ ~0.4–0.7 = quicada), `omega = SpringMath.SmoothTimeToAngularFrequency(...)` a partir de `_StanceTransitionSpeed`. Centralizar a versão angular + o `ClampAngles` em `SpringMath` e chamar dos dois patches (remove a duplicação). Manter o batente ±60° como salvaguarda.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · D/B — Leak de estado entre raids · 🟡 Médio

**`CurrentEuler`/`_rotVelocity`/`_posVelocity`/`_clampLogBudget` estáticos sem reset em raid-end**

**Local:** [Patches/ApplyComplexRotationPatch.cs](Patches/ApplyComplexRotationPatch.cs) (campos estáticos) · [StanceManager.cs:866](StanceManager.cs#L866) (`ResetState` não os toca)

**Problema:** Os springs e o orçamento de log são `static` e só zeram via auto-reset quando geram NaN. `StanceManager.ResetState()` e os patches de raid-end (`RaidLifecyclePatches`) não os reinicializam. Confirmado por grep: nenhuma escrita externa em `CurrentEuler`/`_rotVelocity`. Em particular, `_clampLogBudget` decrementa até 0 e **nunca** restaura.

**Por que importa:** (1) Valor residual da raid anterior pode produzir um 1º frame de transição estranho na raid seguinte (lifecycle/leak — checklist spt §2). (2) Após 12 ocorrências, `[STANCE-CLAMP]` silencia para sempre na sessão → some o instrumento de diagnóstico que CR-01-01 depende.

**Sugestão:** Adicionar reset desses estáticos no raid-end já existente (hook de `GameWorld.OnDestroy`/`StanceManager.ResetState`): zerar `CurrentEuler`, `_rotVelocity`, `_posVelocity`, `CurrentEuler`/`CurrentPosition` e restaurar `_clampLogBudget = 12`. Idempotente.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-04 · C — Gap vs. referência (Realism) · 🟡 Médio

**Falta o early-return `MovementContext.CurrentState.Name == 21` presente nos dois patches do Realism**

**Local:** [Patches/ApplyComplexRotationPatch.cs](Patches/ApplyComplexRotationPatch.cs) / [Patches/ApplySimpleRotationPatch.cs](Patches/ApplySimpleRotationPatch.cs) (gate de player) vs. `mods/RealismMod/.../ApplyComplexRotationPatch.cs:53` e `ApplySimpleRotationPatch.cs:71`

**Problema:** A referência (Realism) faz `return` quando `player.MovementContext.CurrentState.Name == 21` em ambos os patches; o nosso não tem esse guard. O estado `21` não foi confirmado no Assembly decompilado (é um magic number da build 0.14.8 do Realism — pode ter mudado em 0.16.x).

**Por que importa:** Provável pose especial (prone/bipod/transição) em que aplicar a rotação de stance é justamente o que descasa a câmera. Pode ser uma das fontes do flip em cenário específico.

**Sugestão:** Antes de portar, confirmar o enum em `references/eft-decompiled/` (mapear `EPlayerState`/`MovementContext.CurrentState.Name == 21`). Se for pose onde a stance não deve atuar, adicionar o early-return resolvido por nome semântico, não pelo literal `21`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-05 · A/B — Robustez do patch · 🟡 Médio

**Postfix dos dois patches sem `try/catch` (hot path do procedural animation)**

**Local:** [Patches/ApplyComplexRotationPatch.cs](Patches/ApplyComplexRotationPatch.cs) / [Patches/ApplySimpleRotationPatch.cs](Patches/ApplySimpleRotationPatch.cs) (`[PatchPostfix] Postfix`) — grep `try` = 0 em ambos

**Problema:** Os Postfix rodam todo frame no pipeline de animação procedural e fazem várias leituras por reflection (`GetValue`) e acesso a `player.Physical.*`. Há guards de null parciais, mas nenhum `try/catch` de borda. Pré-existente (não introduzido pelo fix), mas os checklists `spt`/`csharp` o exigem e estamos editando este arquivo.

**Por que importa:** Uma exceção inesperada (campo renomeado numa build futura, `Physical`/`Oxygen` null em contexto atípico) propaga do Postfix e pode interromper o frame de animação / poluir o log a cada frame.

**Sugestão:** Envolver o corpo de cada Postfix em `try/catch (Exception ex) { Plugin.Logger.LogError(...) }` com throttle de log (reusar o orçamento estilo `_clampLogBudget`). Não engolir silenciosamente.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-06 · D — Sandbox / processo de build · 🟡 Médio

**Fix fora do pipeline (`modded`), DLL versionada estava divergente, build não-padrão**

**Local:** `mods/stancesAndCameraPositionSPT4.0.11/modded/` · `CameraRotationMod.csproj`

**Problema:** (1) O trabalho vive em `modded/`, que o pipeline SDD e o `compile-mod.sh` não reconhecem (o script é hardcoded em `modded/`). (2) A DLL versionada `shwngFpsCameraStances4.dll` estava em **47 KB** enquanto o código gera **136 KB** — o build commitado estava desatualizado vs. o fonte (sincronizado nesta sessão). (3) O `.csproj` referencia Fika por `..\..\references\...` (2 níveis → `mods/references/`, inexistente) e não declara LiteNetLib — só compilou após popular `References/` e criar `mods/references/` temporário.

**Por que importa:** Drift entre o que está versionado e o que roda; outro dev que clonar não consegue buildar via fluxo do repo. Risco de "passa aqui, quebra lá" — exatamente o sintoma original do bug.

**Sugestão:** Decidir o destino do `modded` (promover p/ `modded/` ou ensinar o `compile-mod.sh` a aceitar `--sandbox modded`). Corrigir os HintPaths do `.csproj` (3 níveis até `references/` da raiz) e padronizar Fika via `References\Fika.Core.dll` como o `.csproj` antigo. Confirmar se a DLL deve mesmo ser versionada (as outras DLLs são gitignored).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-07 · F — Performance hot path · 🟢 Menor

**`Vector3.ClampMagnitude` (sqrt) por sub-step**

**Local:** [Patches/ApplyComplexRotationPatch.cs](Patches/ApplyComplexRotationPatch.cs) (`SpringLerpAngle`/`SpringLerp`, dentro do loop)

**Problema:** `ClampMagnitude` calcula `sqrt` por chamada, executado até 8× por frame em cada patch (sub-steps). Sem alocação (struct), mas é hot path de animação.

**Por que importa:** Custo trivial isolado; some-se a tudo que roda por frame. Otimização opcional.

**Sugestão:** Comparar `sqrMagnitude` contra `max*max` antes de chamar `ClampMagnitude`, ou clampar fora do loop (uma vez no fim). Resolve-se naturalmente se CR-01-02 (SpringDamp) for aceito — o loop some.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-08 · E — Legibilidade/manutenção · 🟢 Menor

**Três implementações de interpolação coexistem sem nota explicativa**

**Local:** `SpringLerpAngle` (patches) · `Mathf.SmoothDampAngle` ([Networking/ObservedStanceAnimator.cs:51](Networking/ObservedStanceAnimator.cs#L51)) · `SpringMath.SpringDamp` (não usado)

**Problema:** Sem um comentário central explicando por que existem três caminhos de mola, o próximo dev (ou o rocket) não sabe qual é o "canônico" nem por que 1ª pessoa e peers usam matemáticas diferentes.

**Por que importa:** Aumenta o custo de manutenção e o risco de "consertar no lugar errado".

**Sugestão:** Subsumido por CR-01-02. Se a unificação em `SpringMath` não for feita agora, ao menos um comentário em cada call-site apontando o canônico e o porquê.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-20 | Code review 01 criada via `/code-review` (hotfix camera-flip, fora do pipeline SDD) |
