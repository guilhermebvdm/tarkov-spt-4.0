# 005 — Hook genérico de velocidade da animação de cura observada

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-08

## Visão geral

`ObservedMedsController.ObservedMedsOperation` (réplica visual, no cliente de QUEM OBSERVA, da operação de meds de um peer) recalcula a velocidade da animação (`FirearmsAnimator.SetUseTimeMultiplier`) usando só o bônus nativo da skill Cirurgia (`Skills.SurgerySpeed`). Nenhum mod consegue hoje ajustar essa velocidade sem reflection direta numa classe **privada aninhada** — frágil e específico demais pra qualquer mod de terceiro reaproveitar. Este item expõe um ponto de extensão público e genérico (um `Func` estático, sem conhecimento de nenhum mod específico) que qualquer mod pode usar pra compor um multiplicador extra em cima do que o Fika já calcula.

**Motivação concreta:** o item `090` do `CustomClasses` precisa que a velocidade da animação de cura (acelerada por um perk de classe) replique corretamente pros outros jogadores que observam a cena — hoje só quem está usando o item vê a velocidade certa. Sem um hook público, a única forma de resolver isso seria reflection em campos privados (`_observedMedsController`, `_fikaPlayer`) de uma classe aninhada privada (`ObservedMedsController.cs`) — funciona, mas é o tipo de acoplamento frágil que este item evita, dado que este repositório já mantém o fork do FIKA e pode expor a extensão de forma limpa.

## Comportamento atual

- `ObservedMedsOperation.ObservedStart(Action callback)` inicia a réplica visual sem ajustar `SetUseTimeMultiplier` nenhuma vez — a 1ª parte do corpo anima na velocidade padrão do Animator.
- `ObservedMedsOperation.HealthController_EffectRemovedEvent(IEffect effect)` recalcula a velocidade a cada parte SEGUINTE do corpo, usando só `_fikaPlayer.Skills.SurgerySpeed.Value / 100f` — nenhum outro fator entra nessa conta.
- Essas duas classes (`ObservedMedsController` e a aninhada `ObservedMedsOperation`) são `internal`/privadas — nenhum mod externo consegue assinar, estender ou interceptar esse cálculo sem reflection em membros privados.

## Comportamento desejado

- Um ponto de extensão público, genérico e opcional: um `Func<Player, Item, float>` estático (nome sugerido: `ObservedMedsSpeedHook.ExtraSpeedMultiplier`), que **qualquer mod** pode atribuir. Quando atribuído, o Fika multiplica a velocidade que já ia aplicar (o cálculo nativo da skill Cirurgia) pelo valor que esse `Func` retornar. `null` (ninguém assinou) = comportamento idêntico ao de hoje (multiplicador extra = 1, zero overhead perceptível).
- O hook é chamado nos DOIS pontos (`ObservedStart` e `HealthController_EffectRemovedEvent`), recebendo o `Player` que está executando a operação (o peer observado) e o `Item` em uso — informação suficiente pra qualquer mod decidir se/quanto ajustar, sem precisar saber nada sobre a implementação interna do Fika.
- O Fika **não** precisa saber que o CustomClasses (ou qualquer outro mod específico) existe — o hook é um `Func` cru, documentado como extensão genérica, do mesmo espírito da API oficial `Fika.Core/Modding/Events/` já existente no projeto (mas mais leve — não é um evento multicast, é um único delegate substituível).

## Critérios de aceite

- [ ] Sem nenhum mod atribuir o hook: o comportamento da animação de cura observada é **idêntico** ao de hoje (byte-a-byte o mesmo cálculo de velocidade) — nenhuma regressão pro caso comum (a maioria dos jogadores não terá mod nenhum usando o hook).
- [ ] Um mod atribui `ObservedMedsSpeedHook.ExtraSpeedMultiplier = (player, item) => 0.5f`: a velocidade da animação observada daquele jogador específico fica visivelmente mais rápida (a 1ª parte do corpo E as seguintes), sem afetar a velocidade observada de OUTROS jogadores que não disparam a condição do mod.
- [ ] O hook recebe o `Player` e o `Item` corretos — testável isolando um log temporário que imprime nickname do jogador + nome do item a cada chamada, e comparando com quem/o quê está sendo curado na tela.
- [ ] Uma exceção lançada DENTRO do `Func` atribuído por um mod terceiro não pode derrubar a animação nem propagar pro resto do Fika — o ponto de chamada do hook trata a exceção e trata como "sem ajuste" (multiplicador extra = 1) nesse caso.
- [ ] **Fika/multiplayer:** obrigatório por natureza — o hook só existe pra resolver um problema de replicação em coop. Testar com pelo menos 2 clientes: um observando o outro curar, com e sem um mod de teste atribuindo o hook.
- [ ] **Estado entre raids:** o `Func` estático não deve depender de estado que sobrevive entre raids de forma incorreta — quem atribui o hook é responsável pelo próprio ciclo de vida do que retorna; o Fika só chama o delegate corrente, sem cache.

## Corner cases

- [ ] Nenhum mod atribuiu o hook (`null`) — deve se comportar exatamente como hoje, sem branch extra custoso no caminho comum (checagem de `null` é o único custo).
- [ ] O mod que atribuiu o hook é descarregado/desabilitado no meio da raid (cenário raro, mas o campo estático pode ficar apontando pra um delegate de um `AppDomain`/contexto que não existe mais) — a chamada deve estar protegida por `try/catch` no ponto de invocação (dentro do Fika), não confiar que o assinante nunca vai quebrar.
- [ ] Dois mods diferentes tentam atribuir o hook (o 2º sobrescreve o 1º, `last-write-wins`) — documentar essa limitação explicitamente; não é objetivo deste item suportar múltiplos assinantes compostos.
- [ ] O `Func` retorna um valor inválido (`0`, negativo, `NaN`, `Infinity`) — o ponto de chamada deve ter uma salvaguarda mínima (ex.: ignorar valores não-positivos ou não-finitos, tratando como 1) pra não travar a animação num multiplicador absurdo.
- [ ] Operação de cura observada é cancelada/interrompida no meio (peer morre, sai da operação) — o hook não deve ser chamado depois que a operação já foi destruída/desalocada (checar o mesmo guard que o código nativo já usa, ex. `_destroyRequested`).

## Fora de escopo

- [x] Qualquer lógica de QUEM deve usar o hook e COM QUE VALOR — isso é decisão de cada mod consumidor (ex.: o item 090 do CustomClasses), não deste item.
- [x] Suporte a múltiplos assinantes compostos (multicast) — decisão consciente de manter simples (last-write-wins); revisitar só se um 2º mod real precisar do mesmo hook simultaneamente.
- [x] Qualquer ajuste na velocidade do lado do PRÓPRIO jogador que está curando (path local, `Player.MedsController.ObservedMedsControllerClass`, vanilla EFT) — esse caminho já é resolvido por cada mod consumidor no seu próprio lado (ex.: item 072 do CustomClasses), sem depender do Fika.

## Referências

- `references/fika-plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs` (idêntico ao que está em `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs` — conferido por diff em 2026-09-08).
- `mods/CustomClasses/backlog/090-velocidade-cura-nao-replica/` — item consumidor que motivou este hook.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Item criado a partir da investigação do item 090 do `CustomClasses` — decisão do usuário de expor um hook genérico no fork do FIKA em vez de reflection pura em classe privada. |
