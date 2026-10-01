# 025 — EmergencyDrop na cirurgia própria (self-heal)

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-09-12

## Visão geral

O item `024` deu ao médico uma forma de largar o kit cirúrgico (CMS/Surv12) instantaneamente e sacar a arma sem esperar a animação de "fechar o kit" — mas isso só funciona **curando um aliado** pelo modo médico deste mod. Usar o mesmo kit **em si mesmo** (auto-cirurgia) passa inteiramente pelo fluxo nativo do jogo, que este mod não intercepta — então, hoje, apertar a mesma tecla durante uma cirurgia em si mesmo não tem efeito nenhum. Este item estende o mesmo botão/ação do drop de emergência pra também cobrir esse caminho nativo.

## Comportamento atual

- A tecla de drop de emergência (config `Emergency Drop Key`, mesma usada pelo item 024) só produz efeito quando `_isHealingInProgress` está ativo — uma flag que só é ligada dentro do `HealRoutine` deste mod, que por sua vez só é iniciado curando um **aliado** (bot ou outro jogador) através do modo médico (`Examinar` + tecla de slot vinculada).
- Auto-cirurgia (usar CMS/Surv12 em si mesmo, pelo fluxo nativo de uso de item do próprio jogo) nunca passa por `HealRoutine` — é tratada inteiramente pelo sistema vanilla do EFT, sem qualquer patch deste mod interceptando o início/fim dessa operação.
- Resultado observável: apertar a tecla de drop de emergência durante uma auto-cirurgia com CMS/Surv12 não cancela, não dropa e não libera as mãos — nada acontece.
- O cancelamento nativo do jogo (Mouse0/mover-se durante o uso) já existe e já funciona pra auto-cirurgia, mas sofre do mesmo problema que motivou o item 024: a animação de fechar o kit continua tocando por inteiro antes das mãos ficarem livres pra usar a arma.
- Enquanto o médico está curando um ALIADO (`_isHealingInProgress` do mod ativo), o item usado é redirecionado inteiramente pro paciente — o efeito nativo NÃO é aplicado no próprio médico nesse caminho (mecanismo de redirect já existente e validado, usado desde os itens de XP/consumo de aliado). Fora desse contexto (nenhuma interação de cura de aliado em andamento), usar um item em si mesmo é sempre auto-cirurgia pura, tratada 100% pelo jogo.
- <!-- review: RESOLVIDO em conversa (2026-09-12) — confirmado que a punição nativa de perda de carga por cancelamento tardio já se aplica sozinha no caminho de auto-cirurgia (regra vanilla), sem qualquer intervenção deste mod; ao contrário da cura de aliado, onde o mod precisou reimplementar essa punição manualmente porque ali o caminho é um redirect customizado que contorna a lógica nativa. Este item NÃO deve reimplementar/tocar em consumo de carga pra auto-cirurgia — decisão confirmada pelo usuário, risco de cobrar 2x descartado por design (nem entra no escopo). -->

## Comportamento desejado

- Ao apertar a mesma tecla de drop de emergência (mesmo config, mesmo modo de ativação já configurado) durante uma auto-cirurgia com CMS/Surv12, o comportamento passa a ser equivalente ao que o item 024 já entrega pra cura de aliado: a cirurgia é interrompida, o kit é dropado no chão, e as mãos ficam livres pra usar a arma **imediatamente** — sem esperar a animação de fechar o kit.
- Assim como no item 024, o efeito continua **exclusivo a itens de cirurgia** (CMS/Surv12) — usar bandagem/tala/torniquete/medkit comum em si mesmo não deve ser afetado por este item (o encerramento vanilla desses itens já é curto o bastante).
- A tecla continua funcionando normalmente pra cura de aliado (item 024) sem nenhuma regressão — este item só adiciona um novo contexto de ativação (auto-cirurgia), não substitui o existente.
- A detecção de "estou em auto-cirurgia" que este item precisa adicionar só pode se ativar quando **não há** cura de aliado em andamento (`_isHealingInProgress` do mod desligado) — os dois contextos são mutuamente exclusivos por natureza (o médico não consegue iniciar uma auto-cirurgia nativa enquanto está com as mãos ocupadas redirecionando o item pro paciente), mas a spec técnica deve implementar essa checagem explicitamente, não assumir por acaso que nunca vão coincidir.
- Sem punição de carga reimplementada por este item (ver nota resolvida acima) — a perda de carga por cancelamento tardio em auto-cirurgia continua 100% a cargo do próprio jogo, como já é hoje.

## Critérios de aceite

- [ ] Ao apertar a tecla de drop de emergência durante o uso de CMS/Surv12 **em si mesmo**, a cirurgia é cancelada, o kit é dropado no chão e a arma está pronta pra atirar no mesmo instante (sem espera perceptível).
- [ ] Ao apertar a mesma tecla durante o uso de um item **não-cirúrgico** em si mesmo (bandagem, tala, torniquete, medkit comum), nada acontece — mesmo comportamento de exclusividade já estabelecido pelo item 024.
- [ ] A tecla continua funcionando exatamente como hoje durante a cura de um **aliado** (item 024) — nenhuma regressão nesse fluxo.
- [ ] Este item não introduz nenhuma lógica de consumo/perda de carga própria para auto-cirurgia — a punição de cancelamento tardio continua inteiramente nativa (comportamento vanilla, sem intervenção do mod), exatamente como já é hoje sem este item.
- [ ] Exemplo de referência pra validar o isolamento entre os dois contextos: médico com perna zerada examina um aliado com braço zerado e inicia o tratamento nele — usar o CMS nesse momento cura **exclusivamente** o aliado (perna do médico continua zerada); ao encerrar a interação com o aliado (sem cura em andamento), usar o CMS em si mesmo cura normalmente o próprio médico, sem qualquer interferência do contexto anterior.
- [ ] **Fika/multiplayer:** curar a si mesmo é uma ação estritamente local e visível apenas como animação pros outros jogadores (igual qualquer uso de item nativo) — o drop de emergência em auto-cirurgia não pode gerar nenhuma trava de mãos nem log de colisão do lado FIKA para quem está observando.
- [ ] **Estado entre raids:** N/A — mudança de sequenciamento local de uma única ativação, sem estado persistente entre raids (mesmo raciocínio do item 024).

## Corner cases

- [ ] Jogador aciona a tecla bem no início do uso do item (quase nenhum tempo decorrido) — precisa funcionar igual (instantâneo), sem aplicar a penalidade de cancelamento tardio (< 1s).
- [ ] **Isolamento aliado × self (cenário concreto validado pelo usuário):** médico com a própria perna zerada examina um aliado com o braço zerado e inicia o tratamento nele (`_isHealingInProgress` ligado, redirect ativo) — usar o CMS nesse momento deve curar **exclusivamente** o aliado; a perna zerada do médico não pode ser afetada. Só depois de encerrar essa interação (sem cura de aliado em andamento) é que usar o CMS em si mesmo deve contar como auto-cirurgia pra este item. A detecção de auto-cirurgia deste item precisa checar explicitamente que nenhuma cura de aliado está em andamento antes de agir — nunca assumir que os dois contextos "nunca vão coincidir" só porque é raro.
- [ ] Jogador aciona a tecla e, no exato momento, morre ou é derrubado durante a própria cirurgia — o descarte abrupto das mãos não pode deixar o jogo num estado inconsistente.
- [ ] Jogador aciona a tecla duas vezes seguidas rapidamente durante auto-cirurgia — não pode aplicar a penalidade de carga mais de uma vez pra uma única cura, nem gerar log duplicado.
- [ ] Curar-se com um item de cirurgia que já está quase sem carga — mesmo cuidado já identificado no item 024 (CR-01-01): garantir que a ordem de descarte/consumo não tente jogar no chão um item que a punição nativa/mod já removeu por falta de carga.

## Fora de escopo

- [x] Mudar o comportamento do cancelamento nativo comum (Mouse0/mover-se) durante auto-cirurgia sem a tecla de drop de emergência — esse fluxo já existe e não é tocado por este item.
- [ ] A definir: se vale a pena, depois deste item, unificar a lógica de "drop de emergência" (aliado + self) num único helper compartilhado, em vez de duas implementações paralelas — decisão de arquitetura que fica pra spec técnica decidir, não pra esta spec funcional.

## Referências

- [024-acelerar-fechamento-kit-emergencydrop/](../024-acelerar-fechamento-kit-emergencydrop/) — item que entregou o mecanismo de drop instantâneo (via `DestroyController()`) pro caminho de cura de aliado; este item estende o mesmo princípio pro caminho nativo de auto-cirurgia.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Item criado via `/add-backlog-item`, a partir de um teste do usuário: apertar a tecla de drop de emergência durante auto-cirurgia com CMS não teve efeito, porque o recurso (item 024) só cobre a cura de aliado, nunca a auto-cura, que passa por um caminho totalmente nativo e não interceptado pelo mod. |
| 2026-09-12 | Revisão `/review-spec` — 2 gaps resolvidos por decisão do usuário (sem reimplementar punição de carga nativa; isolamento aliado×self esclarecido com exemplo concreto) + 1 corner case reescrito com o cenário exato validado (médico com perna zerada + aliado com braço zerado) + 1 critério de aceite novo (nenhuma lógica de consumo própria pra auto-cirurgia). |
| 2026-09-12 | **Fix 01 → Fix 02 pós-implementação:** `TrySetLastEquippedWeapon()` causou dois bugs reais em raid — (1) arma puxada e guardada sozinha, sem nenhuma ação do jogador, deixando as mãos vazias por mais de 1 minuto até reequipar manualmente; (2) travamento total de mãos (`HandsController` nulo, crash em loop) ao repetir o drop rápido demais. Fix 01 (guard de reentrância) não resolveu — o sintoma (1) acontecia numa única ativação, sem repetição, provando que a causa não era o mod reentrando. Fix 02 removeu `TrySetLastEquippedWeapon()` por completo (experimento proposto pelo usuário): `EmergencyDropSelf` agora só solta as mãos instantaneamente, sem reequipar a arma. Ver `025-emergencydrop-autocirurgia-06-fix-02.md`. **Critério de aceite "arma pronta pra atirar no mesmo instante" não se aplica mais** — passa a ser "mãos livres no mesmo instante, reequipar é manual". v1.14.5. |
