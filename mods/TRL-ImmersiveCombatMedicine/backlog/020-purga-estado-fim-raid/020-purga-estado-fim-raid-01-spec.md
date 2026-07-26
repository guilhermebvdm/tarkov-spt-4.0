# 020 — Purga explícita do estado nas fronteiras de raid

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-26

## Visão geral

Achado **S1b** do 1º teste in-game: *"Este item está persistindo os efeitos entre as raids? Isso não deve acontecer, inclusive temos que matar todos efeitos quando a raid acaba!!"*.

A investigação mostrou que **o mod não persiste nada** — todo o estado é em memória e já é limpo por três caminhos independentes. Mas a desconfiança era legítima e o problema real é outro: **a limpeza é invisível**. Não há uma linha de log que diga o que havia e o que sobrou, então um teste não consegue *afirmar* que nada vazou — só supor. E "supor" é exatamente o que produziu a dúvida.

Escopo confirmado com o usuário: **não mexer na persistência vanilla** (membro zerado que volta ferido é do EFT/SPT, mudar isso exigiria tocar o servidor). O trabalho é tornar a purga do mod explícita e verificável.

## Comportamento atual

- Três detecções independentes de fim de raid, todas por observação de que o mundo do jogo deixou de existir: o motor de estados, o helper de ciclo de vida dos consumidores e o controller médico. A escolha por observação em vez de um gancho de "fim de raid" foi deliberada e está comentada no código — o Fika pode substituir o caminho de destruição e o gancho não dispararia.
- Na entrada da raid, um ponto único zera o estado de desmaio, restaura o volume do áudio e reinicia o motor.
- **Nenhum desses caminhos registra o que limpou.** O log tem uma frase fixa ("Estado limpo para nova raid") que é emitida sempre, independentemente de ter havido ou não resíduo — ou seja, ela não distingue sucesso de falha.
- Consequência: um vazamento de estado só se manifestaria como sintoma de jogo (o histórico do mod tem um caso real assim — áudio mudo e prone forçado no primeiro frame da raid seguinte), nunca como um aviso no log.

## Comportamento desejado

- Na entrada da raid, o mod **conta** o estado vivo e registra no log, em duas medições: uma **antes** de limpar e uma **depois**.
- A medição de antes responde "o que a raid anterior deixou para trás?". Qualquer coisa diferente de zero ali é vazamento real, e o log nomeia exatamente qual estado vazou e quanto — não um aviso genérico.
- A medição de depois responde "a limpeza desta entrada funcionou?". Resíduo aqui é falha do próprio mecanismo, e é registrado num nível mais severo que o anterior.
- Quando não há resíduo, o log diz isso explicitamente. É essa linha que permite ao teste **afirmar** a purga em vez de supor.
- **As duas medições avaliam conjuntos diferentes de estado**, e isso é essencial para a auditoria não gerar alarme falso: entre elas, o motor faz a avaliação de entrada que reconhece um jogador que chegou já ferido, e isso **legitimamente** repovoa parte do estado. Exigir zero depois disso acusaria erro em cima do comportamento correto (é o que os cenários de spawn ferido do plano mestre especificam). Então a segunda medição cobre só o estado **transitório** — desmaio, contagens de espera, voz, áudio —, nunca o estado derivado de ferimento.
- Nada muda no gameplay: é observabilidade. Os três caminhos de limpeza e a detecção por observação seguem como estão.

## Critérios de aceite

- [x] Entrar numa raid produz duas linhas de log de auditoria, identificáveis por um prefixo estável e pela fase medida.
- [x] Sem resíduo, a linha afirma isso de forma inequívoca (contagem total zero).
- [x] Com resíduo, a linha nomeia **cada** campo que ficou vivo e a respectiva contagem — não um total anônimo.
- [x] Resíduo antes da limpeza é registrado como aviso; resíduo depois, como erro. Os dois casos significam coisas diferentes e não devem parecer iguais no log.
- [x] Entrar numa raid **já ferido** não gera aviso nem erro: o estado que a avaliação de entrada reconhece é legítimo e está fora da segunda medição.
- [x] A auditoria cobre todo o estado que o mod mantém entre raids: desmaio (contagens de tempo, lista de desmaiados, período de graça, espera de repetição de bot), motor (registros e contagens de espera de eventos únicos), limites de velocidade das duas causas, fila de poses adiadas, tempo de permanência de bot no chão, janelas de anti-repetição de voz, o efeito de tremor gerenciado, e o volume do áudio.
- [x] Nenhuma mudança de comportamento de jogo — só log.
- [x] **Fika/multiplayer:** roda em cada máquina de forma independente, sem pacote nem sincronização. O estado auditado é local por construção.
- [x] **Estado entre raids:** é o próprio objeto do item.

## Corner cases

- [x] **Módulo nunca instanciado** (consumidor desligado desde o boot, motor ainda não criado) → contagem zero, sem exceção de referência nula.
- [x] **Primeira raid da sessão** → a medição de antes naturalmente dá zero; a linha de "limpo" aparece igual, o que também serve de confirmação de que a auditoria está ativa.
- [x] **Hideout** → o ponto de medição está no início de raid; o motor já se mantém desarmado fora de raid.
- [x] **Trânsito entre mapas** (novo mundo de jogo sem fim de raid) → passa pelo mesmo ponto de entrada, então é auditado igual.
- [x] **Volume de áudio alterado por outro mod** → seria contado como resíduo. Aceito: o volume é um estático global do Unity, o mod o manipula durante o desmaio, e um valor diferente de 1 na entrada da raid é exatamente o sintoma do vazamento histórico que este item existe para detectar.

## Fora de escopo

- [x] **Auditar no fim da raid.** Deliberadamente não implementado: a limpeza de fim de raid é uma cascata assíncrona em que cada consumidor limpa no seu próprio ciclo, sem ordem garantida no mesmo quadro. Medir no meio dela produziria falso positivo. O resíduo real aparece de qualquer forma na medição de entrada da raid seguinte, que é onde a observação é confiável. **Desvio consciente do plano de correções**, que pedia as duas fronteiras.
- [x] Substituir a detecção por observação do mundo por um gancho de fim de raid — a escolha atual é deliberada e está documentada no código (o Fika pode substituir o caminho de destruição).
- [x] Mudar a persistência vanilla de ferimentos ou efeitos entre raids.
- [x] Unificar os três caminhos de limpeza num só — refactor de arquitetura sem ganho observável, e cada um cobre uma fronteira diferente (morte, extração, encerramento forçado).

## Referências

- [Patches/Trauma/TraumaPurge.cs](../../modded/Patches/Trauma/TraumaPurge.cs) (novo)
- [TRLImmersiveCombatMedicinePlugin.cs](../../modded/TRLImmersiveCombatMedicinePlugin.cs) (`OnRaidStartCleanup`, os dois pontos de medição)
- [docs/happy-flow-test-plan.md](../../docs/happy-flow-test-plan.md) (cenário **H8**)
- [docs/trauma-behavior-matrix.md](../../docs/trauma-behavior-matrix.md) §5.5 (esclarecimento de que ferimento no spawn é vanilla)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado a partir do achado S1b do 1º teste in-game. Durante a implementação, descobriu-se que uma auditoria única na entrada acusaria erro em cima do comportamento correto de spawn ferido, porque a avaliação de entrada repovoa estado legítimo entre as duas medições — resolvido separando estado transitório de estado derivado de ferimento. A auditoria de fim de raid foi deliberadamente deixada de fora (cascata assíncrona, falso positivo garantido). |
