# 015 — Ambiente Acústico: Reverb e Curva de Distância

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Pedido direto do usuário (2026-09-09), levantado ao comentar o achado `AUD-03-03` da auditoria Review 03: além do bug de oclusão por porta (tratado no item `011-threading-oclusao-voz/`), o usuário quer investigar dois pontos de imersão acústica que hoje não estão implementados ou não estão calibrados: (1) efeitos de eco/reverberação ao falar em ambientes fechados/específicos do jogo (garagem/estacionamento da Interchange, corredores e bunkers da Reserve), usando os parâmetros de áudio 3D que o motor do jogo já oferece, se existirem; e (2) a curva de atenuação de volume por distância, que hoje obriga o usuário a configurar o volume dos amigos em 200% no mixer pra manter uma percepção de volume razoável enquanto eles se afastam um pouco.

**Nota de ordem de execução:** este item toca a mesma curva de atenuação/oclusão de `RemoteSpeaker.cs` que o item `011-threading-oclusao-voz/` corrige (o bug de bitmask que faz a oclusão por porta não funcionar direito). Recomenda-se implementar o `011` primeiro — calibrar a curva de distância *antes* de a oclusão estar corrigida corre o risco de compensar um bug que está de saída, exigindo recalibração depois.

## Comportamento atual

- Falar dentro de ambientes com características acústicas distintas (garagens, corredores, bunkers) não produz nenhum efeito perceptível de eco/reverberação na voz — o áudio se comporta da mesma forma em qualquer ambiente, aberto ou fechado.
- O volume da voz cai perceptivelmente rápido conforme a distância entre jogadores aumenta, a ponto de o usuário precisar configurar o volume de amigos específicos em até 200% no mixer de volume pra compensar, em vez de manter o padrão de 100%.

## Comportamento desejado

- Investigar se o motor do jogo (Unity/FMOD, por trás do EFT) expõe algum recurso de zona de reverberação 3D ou snapshot de áudio ambiental que possa ser aplicado à voz dos jogadores de forma consistente com o ambiente em que estão (interior fechado, corredor, área aberta). Se existir um recurso nativo utilizável, aplicá-lo à voz; se não existir ou não for viável, documentar por que e encerrar essa frente sem implementar uma simulação própria não solicitada.
- Revisar a curva de atenuação de volume por distância e o amortecimento atmosférico (a "névoa" que abafa o som conforme a distância aumenta) para que o volume permanente no mixer não precise passar de 100% para os casos de uso comuns do usuário — seja ajustando a curva, o alcance de referência, ou ambos, com base numa investigação de qual dos dois fatores (curva muito agressiva vs. distância de referência muito curta) é o principal responsável pela queda perceptível.

## Critérios de aceite

- [ ] Existe uma resposta documentada (sim ou não, com justificativa) sobre a viabilidade de aplicar reverb/eco 3D nativo do motor à voz dos jogadores, com uma demonstração em pelo menos um ambiente fechado do jogo (ex.: bunker da Reserve) caso viável.
- [ ] Depois do ajuste da curva de distância/amortecimento (e depois do item `011` já ter corrigido a oclusão), o usuário consegue manter o volume de amigos em torno de 100% no mixer nas distâncias de uso comum de squad (aproximadamente 10-30m, em campo aberto sem oclusão), sem precisar compensar manualmente para cima, validado em pelo menos uma sessão de raid real.
- [ ] O ajuste na curva de distância não faz a voz de jogadores muito distantes (fora do alcance de audição configurado) ficar audível além do esperado — a mudança afeta a suavidade da queda dentro do alcance, não o alcance máximo em si.
- [ ] **Fika/multiplayer:** o comportamento é idêntico para todos os jogadores da squad, independente de quem é o host.
- [ ] **Estado entre raids:** N/A — os efeitos são calculados em tempo real por posição/ambiente, sem estado persistente entre raids.

## Corner cases

- [ ] Jogador se movendo rapidamente entre um ambiente aberto e um fechado (ex.: entrando/saindo de um prédio) — a transição do efeito de reverb (se implementado) não deve ser abrupta ou perceptível como um "corte".
- [ ] Múltiplos jogadores falando simultaneamente dentro do mesmo ambiente fechado — o efeito de reverb não deve empilhar de forma que degrade a inteligibilidade da fala.
- [ ] Jogador na borda do alcance de audição após o ajuste da curva de distância — o comportamento de corte no limite deve continuar previsível, sem volume residual perceptível além do alcance configurado.
- [ ] Ambientes do jogo sem um mapeamento claro de "tipo de ambiente" (transições graduais entre semi-aberto e fechado) — o comportamento nesses casos deve degradar de forma razoável, não binária.
- [ ] Canal de espectador/mortos (2D global, sem posicionamento 3D) — reverb e curva de distância não se aplicam a esse canal, que já é intencionalmente plano.

## Fora de escopo

- [ ] Correção do bug de oclusão por porta/parede (bitmask) — tratada no item `011-threading-oclusao-voz/`, que deve ser implementado antes deste.
- [ ] Simulação própria de reverb via processamento de sinal (DSP) caso o motor não ofereça um recurso nativo utilizável — nesse caso o item se encerra com a investigação documentada, sem implementação.

## Referências

- Comentário do usuário sobre o achado `AUD-03-03` (`docs/relatorio-auditoria-codigo-03.md`), 2026-09-09
- `Audio/RemoteSpeaker.cs` — curva de atenuação por distância e amortecimento atmosférico atuais
- Documentação de arquitetura de áudio 3D nativa (a investigar): `docs/tarkov_native_audio_architecture.md`

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` a partir do feedback do usuário sobre o achado AUD-03-03 |
| 2026-09-09 | Revisão `/review-spec` — nota de ordem de execução (011 antes de 015) adicionada, critério de volume tornado mais verificável, 1 corner case do canal de espectador adicionado, "Fora de escopo" reescrito para ser afirmativo |
