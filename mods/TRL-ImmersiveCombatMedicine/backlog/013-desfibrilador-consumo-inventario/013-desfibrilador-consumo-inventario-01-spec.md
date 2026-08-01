# 013 — Desfibrilador: consumo trava o inventário

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-26

## Visão geral

Achado do 1º teste in-game (2026-07-26, Umbigo como host): *"Ao usar o desfibrilador ele ficou piscando no inventário e não sumiu, sem possibilidade de uso e sem possibilidade de usar o espaço ocupado"*.

O revive de um aliado consome o desfibrilador por um caminho de descarte que o próprio mod já sabe estar errado — e que já causou exatamente esse sintoma antes, no sistema de cura, tendo sido corrigido lá em 2026-07-13 (CR-04/CR-05). A correção nunca foi propagada para o revive, que ficou como o único lugar do mod ainda usando o padrão antigo.

O resultado é o pior dos dois mundos: o desfibrilador não é efetivamente removido, mas fica **inutilizável** — pisca, não pode ser clicado, e o espaço na mochila fica bloqueado até o fim da raid.

## Comportamento atual

- Ao completar o revive de um aliado, o mod procura um desfibrilador no inventário do reanimador e o descarta.
- O descarte é feito de forma **imediata e não simulada**: a operação de inventário é iniciada e enviada à rede, mas nunca recebe a confirmação de conclusão.
- Consequência visível: o jogo entende que existe uma operação de remoção **em andamento e nunca finalizada** sobre aquele item. A interface responde a isso piscando o ícone e tornando o item não-interativo — é o comportamento normal do jogo para um item "em trânsito", só que o trânsito nunca termina.
- O espaço na grade continua ocupado por um item que não pode ser usado, movido nem descartado.
- **Segundo defeito no mesmo trecho:** o desfibrilador é consumido antes de o revive ser efetivamente concluído. O jogo aborta o revive se quem estava reanimando morreu durante os segundos finais da ação, mas o mod já cobrou o item nesse caso — o aliado continua caído e o desfibrilador foi perdido.
- Não é regressão do overhaul Trauma 2.0: o trecho é anterior e nunca foi validado em jogo (o revive por desfibrilador é do sistema de cura, e o único teste que o cobria estava na lista de pendências não executadas).

## Comportamento desejado

- O desfibrilador é efetivamente removido do inventário do reanimador ao concluir um revive: sem piscar, sem travar o espaço, com a operação confirmada nos dois PCs.
- Se o revive **não** se concretizar (quem reanimava morreu no último instante, ou o alvo deixou de existir), o desfibrilador **não é cobrado** — o jogador não perde um item consumível por uma ação que não aconteceu.
- O consumo usa o **mesmo mecanismo de descarte que o sistema de cura já usa e já validou em teste de 2 PCs**, em vez de um caminho paralelo próprio. Um único jeito de remover item consumido no mod inteiro.
- Se, apesar das tentativas, o descarte não se confirmar, isso é **registrado no log de forma inequívoca** — o mod já faz isso no caminho da cura, e o silêncio é o que tornou este bug invisível até um teste in-game.

## Critérios de aceite

- [ ] Reviver um aliado com desfibrilador remove o item do inventário do reanimador: o ícone não pisca em nenhum momento, o item desaparece e o espaço fica livre para outro item na mesma raid.
- [ ] O mesmo resultado é observável **nos dois PCs** — quem reanimou vê o item sair, e o outro peer não vê um item fantasma.
- [ ] Se o reanimador morre durante a ação de revive e o revive é abortado pelo jogo, o desfibrilador **permanece** no inventário (ou no chão, se ele morreu) — não é cobrado por uma ação que não se completou.
- [ ] Reviver sem ter desfibrilador continua indisponível, exatamente como hoje (nenhuma mudança na regra de quem pode reviver).
- [ ] Uma falha de descarte, se ocorrer, aparece no log com mensagem explícita — não passa em silêncio.
- [ ] **Fika/multiplayer:** o comportamento é o mesmo com o reanimador sendo o host ou o client. O bug foi observado no host; a correção não pode valer só para um dos lados.
- [ ] **Estado entre raids:** nenhum item fica em estado inconsistente ao fim da raid por causa de um descarte pendente — uma tentativa em andamento quando a raid acaba é abandonada sem deixar resíduo.

## Corner cases

- [ ] **Mãos ocupadas no instante do descarte** — o revive é uma ação de "plantar", diferente de usar um medkit. Confirmar que o mecanismo de descarte reusado (que foi desenhado para esperar a animação de cura liberar as mãos) se comporta corretamente aqui, onde essa espera não se aplica.
- [ ] **Reanimador morre entre o fim da ação e o descarte** — o item pode já estar num cadáver ou num contêiner de morte quando a remoção for tentada. A tentativa deve falhar de forma contida, sem exceção e sem remover item de lugar errado.
- [ ] **Mais de um desfibrilador no inventário** — apenas **um** é consumido por revive.
- [ ] **Fim da raid durante as tentativas de descarte** — a operação é abandonada, sem tentar mutar item de uma raid que já acabou.
- [ ] **Dois revives em sequência rápida** — cada um consome o seu próprio item, sem que uma tentativa pendente interfira na seguinte nem consuma dois itens por um revive.
- [ ] **Exceção no caminho de consumo não pode cancelar o revive** — o trecho roda dentro do callback da ação do jogo, e uma exceção ali aborta o revive inteiro. Essa proteção já existe hoje e precisa continuar existindo (foi um bug já corrigido, CR-01-04).

## Fora de escopo

- [ ] Mudar **qual** item é exigido para reviver, ou permitir reviver sem item — a regra de exigir desfibrilador para o coma fica como está. (A distinção entre "acordar" um desmaio e "reviver" um coma é o item 016.)
- [ ] Mudar o tempo, a animação ou o alcance do revive.
- [ ] Corrigir a perda de hitbox após o revive — é bug do Fika, tratado em `TRL-Fixes` item 002.
- [ ] Impedir que um desmaio do mod ofereça revive — é o item 016.
- [ ] Reescrever o mecanismo de descarte do sistema de cura — este item o **reusa**, não o altera.

## Referências

- [Patches/Trauma/FikaRevivePatch.cs](../../modded/Patches/Trauma/FikaRevivePatch.cs) (trecho a corrigir)
- [Patches/Medical/MedicalLogic.cs](../../modded/Patches/Medical/MedicalLogic.cs) (mecanismo de descarte a reusar)
- [Patches/Medical/BandAidController.cs](../../modded/Patches/Medical/BandAidController.cs) (agendamento com retry)
- [docs/happy-flow-test-plan.md](../../docs/happy-flow-test-plan.md) (cenário **C1**)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado a partir do achado do 1º teste in-game. Causa-raiz localizada: o revive usa o padrão de descarte que o sistema de cura abandonou em 2026-07-13 (CR-04/CR-05) por causar exatamente este sintoma. Segundo defeito encontrado na leitura do código do Fika: o item é cobrado antes de o revive ser confirmado, e o jogo aborta o revive se o reanimador morrer no último instante. |
