# 012 — Suporte completo a injeções/estimulantes no menu de cura

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-25

## Visão geral

Hoje toda seringa/estimulante do jogo (Propital, Morphine, Obdolbin, SJ1/SJ6 TGLabs, Adrenaline, Zagustin, L1 Norepinephrine, eTG-change, Meldonin, P22, e qualquer outra da mesma categoria, incluindo itens de evento) já funciona **perfeitamente quando o próprio jogador usa em si mesmo** — isso é 100% nativo do jogo, o mod não interfere e não precisa mudar nada nesse caminho. O que falta é a possibilidade de aplicar essa MESMA seringa, com o MESMO efeito, **em outro jogador/bot** (coop) através do menu de interação de cura do mod — hoje o menu só sabe lidar com bandagens/torniquetes/talas/medkits/cirurgia; seringas aplicadas em outra pessoa não têm caminho algum (ou caem num fallback genérico incoerente, tratando a seringa como se fosse um curativo comum).

**Princípio central deste item (confirmado com o usuário): não reescrever nenhum buff.** A meta não é reimplementar o que cada seringa faz — é reusar o EXATO MESMO mecanismo que já roda quando o jogo processa o auto-uso, só que endereçado ao paciente escolhido em vez de ao usuário. Se o mod conseguir, de alguma forma, disparar "aplique este item como se PACIENTE tivesse acabado de usá-lo em si mesmo", o efeito sai automaticamente correto — sem o mod precisar saber o que cada seringa específica faz.

## Comportamento atual

- O banco de itens do mod (`Helpers/ItemDatabase.cs`) só tem 2 seringas cadastradas: Propital (marcado para remover dor) e Zagustin (marcado para estancar todo sangramento). Nenhuma outra seringa/estimulante do jogo está cadastrada.
- **Propital está cadastrado mas não funciona:** o campo que marca "remove dor" nunca é lido pela lógica de aplicação de tratamento — não há nenhum código que trate esse efeito. Como o item não tem sangramento/fratura/cura de HP associados, a verificação de "pode usar este item aqui" reprova sempre, e o Propital nunca pode ser aplicado por ninguém através do menu de cura.
- **Zagustin funciona parcialmente:** o efeito de estancar sangramento é tratado, mas nenhum outro efeito real do item (energia, stamina) é considerado.
- **Qualquer seringa/estimulante NÃO cadastrado** (Morphine, Obdolbin, SJ1/SJ6, Adrenaline, L1 Norepinephrine, eTG-change, Meldonin, P22, itens de evento, etc.) cai num valor padrão genérico: o mod trata como se fosse um curativo qualquer, "curando" 50 de vida em 4 segundos — comportamento que não corresponde a nada do que aquele item realmente faz no jogo.
- O jogo já tem uma categoria própria e um conjunto de "buffs" bem definido para esse tipo de item (mais de 25 tipos de buff possíveis: energia, hidratação, resistência a habilidades, stamina máxima, taxa de recuperação de stamina, sangramento de estômago, embaçamento/tremedeira por concussão, dor, tremor de mãos, resistência a toxina, temperatura corporal, remoção de efeitos negativos, remoção de TODOS os buffs, remoção de TODO sangramento, entre outros) — ou seja, o comportamento correto de cada seringa já existe pronto no jogo; o mod hoje simplesmente não pluga nele.

## Comportamento desejado

- Toda seringa/estimulante do jogo (identificado pela categoria correta do item, não por uma lista fixa de IDs mantida à mão) pode ser selecionada no menu de interação de cura e aplicada **em outro jogador/bot** — sem precisar de um menu separado.
- O efeito que o paciente recebe é **byte a byte o mesmo** que ele receberia se tivesse usado a seringa em si mesmo, da forma nativa do jogo — o mod não decide nem calcula o buff, só aciona o mecanismo nativo apontando para o paciente certo.
- Auto-aplicação (usar a seringa em si mesmo) **não muda** — continua 100% pelo caminho nativo do jogo, sem passar pelo mod. O escopo deste item é só destravar o caso hoje impossível: aplicar em OUTRO alvo.
- O item é consumido de acordo com a mesma regra que já existe hoje para itens de uso único ou com carga (a maioria das seringas é de 1 uso; se algum item tiver múltiplas cargas, seguir o mesmo padrão de consumo parcial já usado pelos medkits) — ou, se o mecanismo nativo já cuida do consumo sozinho ao ser acionado, não duplicar esse consumo por fora.
- Propital e Zagustin (já cadastrados hoje com tratamento manual parcial/quebrado) passam a usar o mesmo caminho novo — sem tratamento especial hardcoded para eles dois, evitando manter 2 caminhos paralelos para a mesma categoria de item.

## Critérios de aceite

- [ ] Aplicar qualquer seringa/estimulante do jogo em OUTRO jogador/bot através do menu de cura produz o mesmo efeito que aquela pessoa teria se usasse a seringa em si mesma (energia/stamina/hidratação/remoção de dor/remoção de efeitos negativos/etc., conforme o item específico) — verificável comparando o resultado (barras de status, buffs ativos) com o de um teste de controle: o mesmo paciente usando a mesma seringa em si mesmo, fora do menu de cura, deve produzir um resultado idêntico.
- [ ] Auto-aplicação (usar a seringa em si mesmo, fora do menu do mod) continua funcionando exatamente como hoje — nenhuma regressão no caminho nativo.
- [ ] Nenhuma seringa aplicada em outro jogador "cura HP" a menos que esse seja de fato um efeito real do item (isso não deve mais acontecer via fallback genérico).
- [ ] Propital e Zagustin, quando aplicados em outro jogador/bot, passam a produzir o efeito nativo completo (Zagustin sem regressão no estancamento de sangramento; Propital passa a de fato remover a dor, coisa que hoje não acontece nem localmente nem remotamente).
- [ ] O item aparece disponível no menu de cura, para aplicar em outro, sempre que a categoria de item bater (sem precisar adicionar manualmente cada template novo que o jogo já tem ou vier a ter na categoria) — a menos que uma decisão explícita de design exclua algum item específico (registrar na spec técnica se algum precisar ficar de fora).
- [ ] **Fika/multiplayer:** aplicar uma seringa num paciente remoto produz o mesmo efeito observado tanto por quem aplicou quanto pelo paciente (sem duplicar nem perder o efeito num dos dois lados) — mesma garantia que já existe hoje para bandagem/medkit remotos.
- [ ] **Estado entre raids:** nenhum efeito de seringa aplicado numa raid sobrevive para a raid seguinte além do que o próprio jogo já garante nativamente para esse tipo de buff (a maioria dos buffs de seringa já expira sozinha por tempo, dentro da mesma raid).

## Corner cases

- [ ] **Paciente sem ferimento associado:** diferente de bandagem/medkit, uma seringa muitas vezes não exige nenhum "ferimento" específico para ser aplicada no colega (ex.: um estimulante de energia serve nele mesmo sem sangramento ativo) — a verificação de "pode aplicar neste paciente" não pode reprovar a seringa só por falta de sangramento/fratura, mas também não pode permitir infinitamente sem nenhuma condição (avaliar se existe alguma regra nativa equivalente, ex.: já ter o mesmo buff ativo, limite de doses do PACIENTE que recebe, não de quem aplica).
- [ ] **Sobreposição com sangramento/fratura tratados por outra via:** algumas seringas (Zagustin) também aparecem na mesma lista de efeitos que bandagem/tala tratam hoje (sangramento, fratura) — não pode haver dois caminhos divergentes tratando o mesmo efeito de forma diferente (um usando a lógica atual de bandagem, outro usando o pipeline nativo de buff da seringa) para o mesmo item.
- [ ] **Confusão de categoria com outros itens "médicos":** o jogo agrupa bandagem/tala/torniquete, medkits e seringas em famílias relacionadas mas DISTINTAS entre si. A spec técnica precisa confirmar qual categoria identifica exatamente "seringa/estimulante" sem capturar acidentalmente um medkit ou uma bandagem por engano (duplicando um item que já tem seu próprio caminho funcionando).
- [ ] **Efeitos negativos/tóxicos:** a mesma categoria de item também inclui variantes com efeito ruim (toxina desconhecida, toxina letal) — confirmar que o mod não bloqueia nem filtra esses (aplicar através do menu deve ser sempre fiel ao item real, mesmo quando o efeito real é negativo) e que a UI não engana o jogador dizendo que é sempre benéfico.
- [ ] **Item usado durante desmaio/incapacitação:** o paciente está desmaiado/caído/downed no momento da aplicação — confirmar que o comportamento é o mesmo já resolvido para bandagem/medkit hoje (aplicável, sem interromper o estado de incapacitação de forma inesperada).
- [ ] **Overdose / limite de uso:** o jogo já tem regras nativas de quantas vezes um estimulante pode ser usado antes de gerar penalidade — confirmar que aplicar em outro pelo menu de cura conta para o limite de overdose do PACIENTE (quem recebe), pela MESMA regra nativa que já vale quando ele usa em si mesmo — não deve ser possível contornar o limite só porque foi aplicado por outra pessoa.
- [ ] **Quem consome o item vs. quem recebe o efeito:** confirmar de qual inventário a seringa sai (do médico que aplica, como já acontece com bandagem/medkit hoje) — o efeito é sempre no paciente, mas o item consumido é o que o médico tinha na mão, não uma cópia tirada do inventário do paciente.

## Fora de escopo

- [ ] Adicionar um menu/categoria visual separada para seringas — usam o mesmo menu de cura já existente.
- [ ] Reimplementar manualmente a lógica de qualquer buff específico (energia, stamina, etc.) — a intenção é reusar o mecanismo nativo do jogo, não recriar os números. **Este é o requisito confirmado com o usuário e não deve ser reaberto na spec técnica.**
- [ ] Mudar qualquer coisa no caminho de auto-aplicação (usar em si mesmo, fora do menu do mod) — continua 100% nativo, sem nenhuma alteração.
- [ ] Migrar Zagustin/Propital para um sistema de config F12 novo — a spec técnica decide se algum parâmetro precisa virar configurável; por padrão, a expectativa é reuso 1:1 do comportamento nativo, sem novos números para calibrar.
- [ ] Itens médicos de uso tópico que não são seringas (ex.: pomadas/unguentos) — o pedido é especificamente sobre injeções/estimulantes; um item "médico" que já cura de outra forma (não por injeção) não entra neste item, mesmo que esteja numa categoria ampla parecida.

## Referências

- [Helpers/ItemDatabase.cs](../../modded/Helpers/ItemDatabase.cs) (banco atual, só Propital/Zagustin cadastrados)
- [Patches/Medical/MedicalLogic.cs](../../modded/Patches/Medical/MedicalLogic.cs) (`ApplyTreatment`/`CanUseItem`, hoje sem tratamento de buff de seringa)
- [PROPRIEDADES.md](../../PROPRIEDADES.md)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-25 | Item criado via `/add-backlog-item`, a partir do pedido do usuário para permitir usar todas as injeções do jogo pelo menu de interação de cura. |
| 2026-07-25 | Revisão `/review-spec` — confirmado no Assembly que o jogo tem uma categoria própria e distinta para estimulantes/seringas (irmã, não subclasse, das categorias de medkit/item médico tópico), reduzindo o risco de sobreposição acidental; adicionado corner case de confusão de categoria e exclusão explícita de itens médicos tópicos não-injetáveis. |
| 2026-07-26 | Clarificação do usuário: auto-uso já funciona nativamente e não muda — o escopo real é só destravar a aplicação em OUTRO jogador/bot (coop), reusando o mecanismo nativo do buff sem reescrever nada. Spec reescrita para deixar esse recorte explícito (visão geral, comportamento desejado, critérios de aceite, corners de overdose/consumo, fora de escopo). |
