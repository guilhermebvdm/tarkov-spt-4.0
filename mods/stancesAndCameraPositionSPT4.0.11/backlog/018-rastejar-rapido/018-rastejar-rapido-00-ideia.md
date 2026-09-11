# 018 — Correr agachado e rastejar rápido (crouch-run + high-crawl)

> **Data:** 2026-07-19 (criação) · **2026-09-08** (escopo ampliado + investigação técnica)<br>
> **Status:** ⚪ Backlog (investigação inicial concluída — pronto para `/create-spec`)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [005-velocidade-agachar-inclinar](../005-velocidade-agachar-inclinar/), [012-controlador-central-stamina](../012-controlador-central-stamina/), [021-toggle-tombamento-mira-lateral](../021-toggle-tombamento-mira-lateral/) (padrão de toggle F12 default-safe)<br>

---

## Ideia (do usuário, ampliada em 2026-09-08)

Duas posturas, mesmo mecanismo:

1. **Agachado:** ao segurar a tecla de **sprint nativa** (a mesma já configurada pelo jogador para correr em pé —
   **não** uma hotkey nova) estando agachado, **manter a postura agachada** e aumentar a velocidade dos passos
   nela, em vez do vanilla te levantar para correr em pé.
2. **Prone:** mesma mecânica — segurar sprint deitado mantém o rastejar, só que mais rápido ("crawl + run" /
   *high-crawl* tático).

Em ambos os casos, o custo extra de stamina por se mover mais rápido numa postura desconfortável deve **somar**
sobre o dreno nativo de sprint (que já escala com a skill de Força/Endurance) — não substituí-lo. Ou seja: quem
tem a skill mais alta continua pagando menos pelo dreno BASE, e paga só a sobretaxa da postura por cima.

## Investigação técnica preliminar (2026-09-08, via chat — antes da spec técnica formal)

Confirmado no Assembly decompilado (fonte primária, `references/eft-decompiled/Assembly-CSharp/`):

- **O vanilla força o personagem a ficar em pé ao entrar em sprint, incondicionalmente.**
  `SprintStateClass.Enter()` chama `MovementContext.SetPoseLevel(1f)`
  ([SprintStateClass.cs:21-27](../../../../references/eft-decompiled/Assembly-CSharp/SprintStateClass.cs#L21-L27),
  linha 24 é o `SetPoseLevel(1f)`). Isso confirma a suspeita do usuário: hoje, apertar sprint agachado ou deitado
  levanta o personagem antes de sprintar — não existe high-crawl nem crouch-run nativos, os dois precisam ser
  construídos pelo mod.
- **`MovementContext.CanSprint` não olha a postura atual** — só condições físicas (perna ferida, usando
  remédio/analgésico) ([MovementContext.cs:1240-1270](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1240-L1270)).
  Ou seja, o gate que hoje permite/bloqueia sprint não distingue pose — a IMPLEMENTAÇÃO precisa interceptar
  especificamente o `SetPoseLevel(1f)` do `Enter()` (ou o ponto que decide a transição de estado para
  `SprintStateClass`), não o `CanSprint`.
- **Reaproveitar a tecla nativa de sprint é o caminho certo** (não hotkey nova): o gatilho já existe
  (`MovementContext.IsSprintEnabled` / `_player.Physical.Sprinting`, [MovementContext.cs:1167](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1167)) —
  o trabalho é impedir/reverter o `SetPoseLevel(1f)` quando a pose de origem for agachado/prone, não capturar um
  input novo.
- **Stamina de sprint (pernas) é um sistema DIFERENTE do item 012** (que só cobre `HandsStamina`, o braço
  segurando a arma — ver `StaminaController.cs:12-14`). O dreno de pernas ao sprintar vive em `Physical`/
  `MovementContext` e — pelo padrão já confirmado nesta memória do mod para velocidade máxima
  (`Evaluate(BackendConfig.WalkSpeed, Strength/60)`, achado da investigação do P-11.1) — é plausível que também
  escale com a skill de Força. **Ainda não confirmado especificamente para o dreno de sprint** (só para o teto de
  velocidade) — fica como item a fechar na spec técnica: localizar a chamada real de dreno de stamina do sprint e
  confirmar se ela já é afetada pela skill antes de decidir como somar a sobretaxa da postura.

## Perguntas em aberto (fechar na spec)

- ~~**Gatilho exato:** qual combinação de inputs dispara?~~ **Resolvido:** reusar o sprint nativo
  (`IsSprintEnabled`), sem tecla nova — funciona igual para agachado e prone.
- ~~**O EFT tem estado/animação de high-crawl nativo?**~~ **Resolvido: não tem.** `SetPoseLevel(1f)` sempre
  levanta; qualquer "correr" agachado/prone é 100% construído pelo mod (multiplicador de velocidade sobre a pose
  atual, sem animação nova — mesmo padrão do item 005).
- **Onde exatamente interceptar** o `SetPoseLevel(1f)` de `SprintStateClass.Enter()` sem quebrar o sprint em pé
  normal (ele não pode deixar de levantar quando o jogador JÁ está em pé) — investigar na spec técnica se o ponto
  certo é um Prefix condicional em `SetPoseLevel` (checando a pose ANTES da chamada) ou interceptar a transição de
  estado que entra em `SprintStateClass`.
- **Dreno de stamina de pernas (sprint):** localizar a chamada real (fora do escopo do item 012) e confirmar se
  já respeita a skill de Força/Endurance nativamente. Se sim, a sobretaxa da postura deve ser um multiplicador
  adicional aplicado sobre esse mesmo dreno (não uma fórmula paralela).
- **Interações:** com as stances do mod (prone já força Stance 0 — item 013), com mount, com ADS deitado/agachado,
  com o snap-on-fire (item 002), com o bloqueio de sprint durante ações do item 012/HandsStamaGuard.
- **Sync Fika:** se a velocidade extra for só um multiplicador sobre o `MovementContext` nativo (não uma animação
  nova), o comportamento tende a propagar pro Fika como qualquer outra mudança de velocidade — confirmar na spec
  técnica, não assumir.
- **Teto de segurança:** impedir que a velocidade agachado/prone-acelerada ultrapasse (ou chegue perto d)a
  velocidade de sprint em pé — isso mudaria o cálculo de risco/recompensa do jogo (ficar baixo teria só vantagens).
  Precisa de um teto explícito no F12, não só um multiplicador livre.

## Escopo (ampliado 2026-09-08)

Duas sub-features com o mesmo mecanismo, provavelmente uma spec só:

1. **Crouch-run:** interceptar a entrada em `SprintStateClass` vinda de pose agachada — suprimir o
   `SetPoseLevel(1f)`, manter a pose agachada, aplicar multiplicador de velocidade (F12, com teto — ver acima).
2. **Prone high-crawl:** mesma mecânica, partindo de pose prone.
3. **Stamina:** sobretaxa somada sobre o dreno nativo de sprint (não substituição), enquanto a postura-acelerada
   estiver ativa. Confirmar na spec técnica se o dreno nativo já escala com skill antes de decidir a fórmula da
   sobretaxa.
4. Toggle F12 por postura (habilitar/desabilitar cada uma independentemente), seguindo o padrão de default seguro
   já usado no item 021 (mudança de comportamento sensível vem desligada por padrão até validação in-game).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação do item (ideia bruta capturada; pendente investigação técnica e spec). |
| 2026-09-08 | Guilherme + agente | Escopo ampliado para incluir **agachado** (não só prone); investigação técnica preliminar no Assembly confirmou que o vanilla sempre levanta o personagem ao entrar em sprint (`SprintStateClass.Enter():24`) e que `CanSprint` não olha a pose. Título do item atualizado. Pronto para `/create-spec`. |
