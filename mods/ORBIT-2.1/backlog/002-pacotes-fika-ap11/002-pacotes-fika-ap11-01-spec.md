# 002 — Pacotes Fika do addon no padrão AP-11

**Mod:** ORBIT-2.1
**Status:** Backlog
**Criado:** 2026-09-30

## Visão geral

O addon `Orbit.Fika` envia dois pacotes pela rede do Fika: um com o áudio dos combates fantasmas e outro com o estado das portas abertas por bots. Os dois foram escritos pelo autor do ORBIT e não seguem o padrão que este repositório adotou depois dos incidentes de dessincronização registrados em [fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md). Este item os traz para o padrão.

O item nasceu da review técnica do port (PA-01-11) e do code review (CR-01-15). Não é efeito do port: os desvios existem no upstream.

## Comportamento atual

- Os dois pacotes gravam os campos em sequência e os leem em sequência, sem marcar o tamanho do corpo. Se um cliente com versão diferente do addon ler um pacote com campos a mais ou a menos, a leitura fica desalinhada.
- A leitura lança erro quando faltam bytes. O Fika descarta então os demais pacotes daquele quadro de rede, de todos os mods.
- O pacote de combate fantasma decide se há lista de atiradores olhando quantos bytes sobram no leitor. Isso só funciona se ele for o último pacote do leitor.
- A ponte de portas remove o registro do pacote quando o gerenciador de rede é destruído ou trocado.
- O pacote de portas tem um campo de versão de protocolo e é ignorado quando a versão não bate. O de combate fantasma não tem.
- Já corrigido no item 001: o recebimento do pacote de combate fantasma passou a capturar exceção.

## Comportamento desejado

- Cada pacote grava o corpo com o tamanho na frente e lê só o que o tamanho indica.
- A leitura nunca lança erro: corpo truncado marca o pacote como inválido, e pacote inválido não é processado nem retransmitido.
- Nenhuma remoção de registro de pacote.
- Os tipos mudam de nome (sufixo `V2`), para que um cliente com o addon antigo não leia o formato novo.

## Critérios de aceite

- [ ] Rodar `node scripts/check-packet-hashes.js` sem colisão com os dois tipos novos.
- [ ] Conferir cada item do checklist §7 do guia de pacotes para os dois pacotes, com a linha do código que o atende.
- [ ] Em raid com host e um cliente na mesma versão: ouvir no cliente os tiros de um combate fantasma e ver no cliente uma porta aberta por bot.
- [ ] Em raid com um cliente sem o addon ou com o addon antigo: a raid segue, sem linha de erro de leitura de pacote no log do host nem do cliente.
- [ ] **Fika/multiplayer:** os dois critérios acima são o próprio teste de coop; repetir com headless como host.
- [ ] **Estado entre raids:** raid 1 → saída → raid 2 com os mesmos jogadores: os pacotes continuam chegando na segunda raid (o registro acompanha o gerenciador de rede novo).

## Corner cases

- [ ] Pacote truncado no meio de um texto: a leitura não lança e o pacote é descartado.
- [ ] Gerenciador de rede recriado sem que o plugin veja o evento de destruição: o registro precisa valer para o gerenciador novo.
- [ ] Cliente que entra com a raid em andamento: recebe o estado das portas já abertas.
- [ ] Host do upstream (formato antigo) com cliente deste fork, e o contrário: nenhum dos dois lados processa lixo.

## Fora de escopo

- [ ] Mudar o que é sincronizado.
- [ ] Propor a mudança ao autor do ORBIT.

## Referências

- [fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md) §5.1 (envelope) e §7 (checklist)
- [spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md) AP-11
- Spec técnica do item 001, §7.5: tabela de desvios com arquivo e linha

## Histórico

| Data | Evento |
|---|---|
| 2026-09-30 | Item criado a partir de PA-01-11 e CR-01-15 do item 001 |
