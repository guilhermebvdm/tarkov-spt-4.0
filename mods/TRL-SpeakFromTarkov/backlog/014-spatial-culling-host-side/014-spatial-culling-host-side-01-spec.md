# 014 — Spatial Culling Host-Side

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Hoje o host retransmite o áudio de cada jogador para todos os outros jogadores conectados, sempre, independente da distância entre eles — o descarte por distância só acontece no jogador que recebe, depois do pacote já ter atravessado a rede. Esta feature move essa decisão para o host, antes da retransmissão, cortando banda tanto no upload do host quanto no download de jogadores fora de alcance. Recomendação levantada na investigação de rede (`docs/investigacao-canal-litenetlib-voip.md`, item (d)) como alternativa de maior ganho e menor risco do que separar o VOIP num canal de rede dedicado — decisão essa já fechada e não reaberta por este item.

## Comportamento atual

Quando um jogador fala, o pacote de áudio é enviado para **todos** os jogadores da raid, sem checagem de distância no momento do envio. Cada jogador que recebe o pacote decide localmente se vai reproduzi-lo ou descartá-lo, com base na distância até quem falou e no alcance de audição configurado (`MaxHearingDistance`). Um jogador a 500m de quem está falando recebe o pacote pela rede da mesma forma que um jogador a 5m, e só descarta depois de já ter sido baixado.

## Comportamento desejado

O host, ao repassar a voz de um jogador para os demais, verifica a distância entre quem fala e cada jogador conectado, e só retransmite para os que estão dentro do alcance de audição configurado (mais uma margem de tolerância para evitar corte abrupto perto do limite). Jogadores fora de alcance simplesmente não recebem o tráfego de rede correspondente àquela fala — nem o host gasta banda de upload com eles, nem eles gastam banda de download com um pacote que seria descartado de qualquer forma.

## Critérios de aceite

- [ ] Um jogador falando dentro do alcance de audição configurado (`MaxHearingDistance`) continua sendo ouvido normalmente por todos os jogadores dentro desse alcance, sem corte perceptível de início ou fim de fala.
- [ ] Jogadores fora do alcance de audição configurado não recebem pacotes de rede correspondentes a essa fala (verificável por contagem de pacotes recebidos, não apenas pela ausência de reprodução sonora).
- [ ] O comportamento de audição dentro do alcance (incluindo a margem de tolerância já existente) permanece idêntico ao atual — esta mudança afeta só quem recebe o pacote pela rede, não a experiência sonora de quem já ouve corretamente hoje.
- [ ] **Fika/multiplayer:** a lógica se aplica especificamente ao papel de host (seja ele um jogador ativo na raid ou uma instância headless/dedicada) — o comportamento é correto nos dois casos.
- [ ] **Estado entre raids:** N/A — a decisão de retransmitir ou não é feita por pacote, em tempo real, sem depender de nenhum estado acumulado entre raids.

## Corner cases

- [ ] Jogador se movendo rapidamente para dentro/fora do alcance durante uma fala contínua — não deve haver corte perceptível de ida e volta se ele permanece majoritariamente dentro do alcance (considerar a mesma margem de tolerância já usada no filtro atual do lado do cliente).
- [ ] Canal de espectador/mortos (2D global, sem noção de distância) não é afetado por este culling — continua sendo sempre retransmitido a todos que estão nesse canal, como hoje.
- [ ] Raid com apenas 2 jogadores, sempre dentro do alcance um do outro — o culling não deve introduzir nenhuma latência ou diferença de comportamento perceptível em relação a hoje.
- [ ] Um jogador ainda entrando na raid (avatar não totalmente spawnado, posição indisponível) não deve travar ou atrasar a retransmissão para os demais jogadores.
- [ ] Jogador host trocando de estado vivo/morto no meio de uma fala — a interação com o filtro de vivo/morto já existente (Canal 0) deve continuar funcionando como hoje, sem regressão.
- [ ] O próprio host falando — a retransmissão pros demais jogadores dentro do alcance continua ocorrendo normalmente, sem tentar "repassar pra si mesmo".
- [ ] O host, quando também é um jogador ativo na raid (não headless), continua ouvindo localmente a fala de outros jogadores exatamente como hoje — o culling afeta só o que é retransmitido pela rede pra outros, não o processamento/reprodução local do próprio host.

## Fora de escopo

- [ ] A decisão sobre canal LiteNetLib dedicado (já fechada, ver `docs/investigacao-canal-litenetlib-voip.md`).
- [ ] Qualquer alteração no filtro de distância que já existe do lado de quem recebe o pacote.

## Referências

- `docs/investigacao-canal-litenetlib-voip.md` — recomendação (d)
- `docs/relatorio-auditoria-codigo-03.md` — contexto da auditoria que motivou a investigação
- Item 06 do backlog `009-otimizacoes-arquiteturais-v2` (ideia original, nunca implementada)
- **Nota de possível sinergia:** o item `010-bot-nao-ouve-convidado/` também vai precisar que o host saiba quem está falando, com que intensidade e em que posição (pra decidir a reação dos bots). A spec técnica deste item deve verificar se cabe reaproveitar o mesmo dado/mensagem em vez de duplicar lógica parecida em dois lugares — não é um requisito, é só um ponto a avaliar.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` a partir da recomendação da investigação de rede |
| 2026-09-09 | Revisão `/review-spec` — 2 corner cases sobre o próprio host falando/ouvindo adicionados, nota de sinergia com o item 010 registrada em Referências |
