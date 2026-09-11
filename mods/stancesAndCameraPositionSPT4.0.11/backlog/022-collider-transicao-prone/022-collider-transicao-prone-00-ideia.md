# 022 — Exposição de collider durante a transição de saída do prone

> **Data:** 2026-09-09 (criação)<br>
> **Status:** ⚪ Backlog (investigação preliminar registrada — não é urgente, aguardando priorização)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [018-rastejar-rapido](../018-rastejar-rapido/) (onde o problema foi descoberto, ao tentar fazer "prone + agachar" levantar direto pro agachado em vez de em pé)<br>

---

## Ideia (origem: item 018, 2026-09-09)

Ao sair do prone por qualquer caminho (agachar, levantar, pular), o personagem passa por um estado de
transição dedicado, `Prone2StandStateClass`, cujo `Enter()` já troca o collider do personagem pro
tamanho de **em pé** de forma binária — o collider só distingue "prone vs. não-prone", não "agachado
vs. em pé" (isso é outro sistema, contínuo via `PoseLevel`/`SmoothedPoseLevel`). Ou seja: **durante toda
a animação de "levantar", o jogador já está com hitbox de tamanho em pé**, independente de qual pose
final ele vai ocupar depois que a transição terminar.

O item 018 tentou resolver um sintoma relacionado (apertar "agachar" enquanto prone levava direto pro
em pé, e o usuário morreu por causa disso) injetando um alvo de pose diferente (`PoseMemo=0`,
agachado) antes da transição rodar (`ProneToggleDuckStandUpPatch`). O fix mudava corretamente o
RESULTADO FINAL (o personagem terminava agachado, não em pé), mas **não resolvia a exposição real**,
porque o collider de tamanho em pé já está ativo durante toda a janela da transição, independente do
alvo de pose numérico — a "levantadinha" visual (e o hitbox de em pé) continuava acontecendo do mesmo
jeito. Por não resolver o problema que motivou o pedido, o fix foi **revertido** (ver histórico do item
018) — mas a causa raiz (collider binário durante a transição) fica registrada aqui para investigação
futura, sem pressa.

## O que já se sabe (investigação preliminar, via chat, 2026-09-09)

- `Prone2StandStateClass : IdleStateClass` (ref: `Prone2StandStateClass.cs`) — estado de transição
  dedicado, entrado sempre que se sai do prone (por `Prone()`, chamado incondicionalmente por
  `ProneMoveStateClass.ChangePose`/`Jump`/etc.).
- `Prone2StandStateClass.Enter()` chama `MovementContext.AdjustCharacterController(prone: false)` —
  ainda não investigado o que exatamente esse método faz internamente (provavelmente redimensiona o
  `CharacterController.height`/`center` pro tamanho padrão), nem se existe uma forma de fazê-lo manter
  um tamanho intermediário (agachado) em vez do tamanho de em pé.
- `ManualAnimatorMoveUpdate` desse estado usa `NormalizedTime` (progresso do clipe de animação atual) e
  um limiar fixo `Float_1 = 0.3f` pra decidir quando começar a restaurar alinhamento/grounder — sugere
  que o clipe de "levantar do chão" é único (sem variantes "levantar só até agachado"), e o
  comportamento de blend pra pose final só começa a valer DEPOIS dessa fase inicial rígida.
- **Não investigado ainda:** se dá pra patchear o TIMING de `AdjustCharacterController` (chamá-lo
  DEPOIS que a pose final já foi decidida, com um parâmetro de tamanho intermediário) sem quebrar a
  física de colisão durante a transição (risco de clipping através de geometria, ou o jogo não suportar
  um tamanho "agachado" de collider fora do binário prone/não-prone que ele já assume em vários lugares).

## Escopo da investigação futura

1. Ler `MovementContext.AdjustCharacterController` completo — o que exatamente ele redimensiona, e se
   aceita algum parâmetro de granularidade além do bool `prone`.
2. Entender se o clipe de "levantar do chão" (`Prone2StandStateClass`) tem alguma variante ou parâmetro
   de blend que dependa da pose-alvo, ou se é genuinamente um clipe único e fixo.
3. Avaliar se um patch no TIMING do resize de collider (aplicar o tamanho final de pose mais cedo, ou
   usar um tamanho intermediário durante toda a transição) é sequer seguro — risco de o personagem ficar
   preso em geometria que só "encaixa" no tamanho binário que o jogo já espera.
4. Se não houver caminho seguro, documentar como limitação conhecida do engine e fechar o item como não
   resolvível sem mudanças profundas demais pro risco/benefício.

## Fora de escopo (por ora)

- Reimplementar `ProneToggleDuckStandUpPatch` (o fix de `PoseMemo`) sem resolver a exposição de collider
  — já revertido por não resolver o problema real.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-09-09 | Guilherme + agente | Item criado a partir de uma investigação do item 018 — não urgente, registrado pra revisitar depois. |
