# 004 — Reformular Chance de Desmembramento por Calibre e Parte do Corpo

**Mod:** VisceralCombat
**Status:** Backlog
**Criado:** 2026-09-10

## Visão geral

A chance de desmembramento hoje é um número único por calibre, igual pra qualquer parte do corpo, sem base em física real — resultado: calibres fracos (5.56, 5.45) desmembram com chance real (10%/5%), calibres realmente devastadores (.50 BMG) nem aparecem na tabela, e munições de múltiplos projéteis (calibre 12, alguns 23x75/40x46) são avaliadas como se fossem um projétil só. Este item reformula a chance de desmembramento pra ser (a) específica por parte do corpo (braço, perna, cabeça-arranca, cabeça-estourar) e (b) coerente com o poder real de cada munição, usando uma combinação de regra dinâmica (soma de projéteis no mesmo disparo) e tabela calibrada manualmente, com uma lista pequena de exceções por munição individual quando o calibre sozinho não for granularidade suficiente.

## Comportamento atual

- `KillPatch.calibers` (`dismember_calibers` em `VD_Calibers.json`) guarda **um único** valor de chance por calibre, usado igual pra braço, perna e cabeça — não existe diferenciação por parte do corpo.
- Multi-projétil (calibre 12, alguns 23x75/40x46) é avaliado com o mesmo número fixo do calibre inteiro, sem somar o efeito de vários projéteis acertando no mesmo disparo (o único lugar do mod que já soma projéteis do mesmo disparo é o desmembramento de perna em bots **vivos**, agrupando por `shot.FireIndex` — mas só pra decidir se dispara a mecânica de agonia, não pra calcular uma chance ponderada).
- Cabeça só tem **um** comportamento de desmembramento: remoção completa (troca a malha por um dos props `Head_1/2/3`). Não existe um efeito intermediário de "cabeça estourada mas ainda presente" — investigação anterior encontrou assets não usados no bundle (`head_exploded01/02`, `gore_neck_cap01/03`) que sugerem que esse efeito pode ter existido em algum momento, mas não está conectado a nenhum código hoje.
- A tabela atual (`VD_Calibers.json`) cobre só 22 dos 31 calibres reais do jogo — faltam, entre outros, `.50 BMG` (127x99) e `12.7x108mm`, os dois calibres de maior poder do jogo.

## Comportamento desejado

1. **Chance por parte do corpo, não por calibre inteiro:** cada calibre passa a ter 4 valores independentes — chance de desmembrar braço, perna, arrancar a cabeça inteira, e "estourar" a cabeça (efeito visual distinto, cabeça permanece no lugar mas com dano aberto/exposto). Ver planilha de referência `mods/VisceralCombat/docs/municoes-tabela-completa.xlsx` (aba "Resumo por Calibre") com os valores calibrados manualmente pelo usuário.
2. **Três mecanismos de decisão, aplicados nesta ordem:**
   - **(A) Regra dinâmica de multi-projétil:** se a munição real do tiro tiver mais de um projétil (`ProjectileCount > 1`, dado do próprio jogo — sem lista manual), a chance é calculada somando o momento de todos os projéteis desse disparo que acertaram a mesma parte do corpo (reaproveitando o agrupamento por `FireIndex` já existente no mod), comparado a um limiar. Cobre calibre 12/20 (chumbo), Shrapnel-10/25, Volna-R e M576 automaticamente, sem precisar listar nenhuma dessas munições por nome.
   - **(B) Tabela por calibre:** pra munição de projétil único (`ProjectileCount == 1`), usa o valor calibrado da tabela por calibre (a planilha de referência).
   - **(C) Exceção por munição individual:** lista pequena, por nome interno/ID da munição, pra casos de projétil único que não devem herdar o valor genérico do calibre — hoje isso cobre pelo menos: balote de calibre 12 (ex.: Grizzly/Poleva — devastador, mas não deve seguir nenhum valor médio de "12g"), `Barrikada` (23x75, slug pesado), e `Zvezda` (23x75, flashbang — força 0 explícito pra não herdar nada por engano).
3. **Bloco de proteção existente (item 003) continua valendo:** Boss/escolta vivos continuam nunca desmembrando a perna, independente do calibre — essa reformulação não reabre esse bloqueio.
4. **Calibres irrelevantes ficam com chance 0 nas 4 colunas** (brinquedo, sinalizador, lançador de granada que já é tratado como explosão em outro caminho do código) — sem necessidade de mecanismo especial pra eles.

## Critérios de aceite

- [ ] Atirar no braço/perna de um bot com um calibre "fraco" da tabela (ex.: 5.56, 9mm) nunca desmembra esse membro (chance 0 aplicada corretamente).
- [ ] Atirar com um calibre pesado (ex.: `.50 BMG`/127x99, agora presente na tabela) desmembra braço/perna/cabeça com a chance alta calibrada, incluindo o caso de cabeça (arranca ou estourar, conforme os dois valores da tabela).
- [ ] Atirar com chumbo de calibre 12/20 num membro com múltiplos pellets do mesmo disparo acertando o mesmo membro produz uma chance de desmembrar **maior** do que um pellet isolado acertaria sozinho (a soma de momento realmente muda o resultado, não é só decorativo).
- [ ] Atirar com balote de calibre 12 usa o valor da lista de exceção (mecanismo C), não um valor genérico de "12g" nem a regra de soma de projéteis (já que balote é projétil único).
- [ ] Cabeça "estourada" (mecanismo/valor "Cabeça (abrir ao meio/estourar)") produz um resultado visualmente diferente de cabeça "arrancada" — a cabeça permanece na cena, só com o efeito de dano aberto, em vez de sumir e virar um prop de coto. <!-- review: este critério só se aplica SE o corner case do asset (ver abaixo) for resolvido positivamente antes da spec técnica; caso contrário, este critério sai do escopo junto com o efeito, e a coluna correspondente da tabela fica só documentada, sem verificação neste item. -->
- [ ] O bloqueio de Boss/escolta vivos (item 003) continua funcionando sem regressão depois da reformulação.
- [ ] **Fika/multiplayer:** o cálculo de chance (incluindo a soma de projéteis do mecanismo A) produz o mesmo resultado em todos os peers — não deve haver divergência de qual membro foi desmembrado entre quem atirou e quem observa.
- [ ] **Estado entre raids:** nenhuma configuração de calibre/chance persiste ou se acumula entre raids — a tabela é estática (carregada do JSON), sem estado dinâmico a limpar.

## Corner cases

- [ ] Munição não listada em nenhuma das 3 tabelas (calibre novo do jogo, ou DLC futuro) — precisa de um fallback explícito e seguro (ex.: 0%, não um default alto como o `0.5f` atual do código), pra nunca "desmembrar por engano" uma munição desconhecida.
- [ ] Um disparo de calibre 12/20 onde os pellets se espalham e acertam **partes do corpo diferentes** (um pellet no braço, outro na perna) — a soma de momento do mecanismo A deve ser calculada **por parte do corpo atingida**, não uma soma total do disparo aplicada a qualquer parte que peça.
- [ ] Munição da lista de exceção (mecanismo C) cujo nome interno mude numa atualização futura do jogo (ex.: SPT atualiza o `_name` da munição) — a lista deve ser resiliente o bastante pra não falhar silenciosamente (logar aviso se uma entrada da lista C não for mais encontrada no banco de itens).
- [ ] `.50 BMG`/`12.7x108mm` (calibres antes ausentes da tabela) devem ter valores altos e coerentes, incluindo o efeito de cabeça — evitar reproduzir o esquecimento que motivou este item.
- [ ] Cabeça "estourada" (mecanismo novo) precisa de um asset 3D confirmado antes da spec técnica — investigação anterior encontrou candidatos não conectados no bundle (`head_exploded01/02`), mas nenhum foi confirmado visualmente ainda. Se nenhum asset viável existir, este item entrega só a diferenciação de chance por parte do corpo + os mecanismos A/B/C, e o efeito de "estourar" cai como item futuro (`Fora de escopo` abaixo, condicional).
- [ ] Interação com o mecanismo de chance configurável do capacete (`ShootOffHelmetPatch`, item existente) e com o drop de capacete/óculos a 100% no desmembramento real (item 002) — "estourar" a cabeça também deve disparar o drop de capacete/óculos do item 002 (o efeito de cabeça continua sendo "desmembramento real de cabeça" pro propósito daquele item), enquanto o hit simples que não desmembra continua só na chance configurável do `ShootOffHelmetPatch`.
- [ ] **Consistência do resultado da rolagem entre peers em coop** — o sistema atual (mantido por este item para os mecanismos A/B/C) decide a rolagem localmente em quem processa o evento primeiro e propaga o resultado via pacote (`isFromNetwork`); um observador remoto só reflete o resultado quando o pacote chega, sem re-rolar. Este item **não muda** esse padrão (mesma arquitetura de hoje) — o critério de aceite "Fika/multiplayer" acima cobre o resultado final convergente, não a ausência de uma janela de latência normal de rede.

## Fora de escopo

- [ ] Efeito visual de "cabeça estourada" — **condicional**: entra no escopo só se um asset 3D viável for confirmado (ver corner case acima) antes da spec técnica. Se não houver asset, a coluna "Cabeça (abrir ao meio/estourar)" da tabela fica sem efeito neste item (as chances calibradas ficam documentadas, mas sem implementação visual, aguardando asset).
- [ ] Reformular a auditoria dos pacotes Fika do mod (`[P-7.3]`, dívida técnica já registrada na memória) — fora de escopo, item separado.

## Referências

- `mods/VisceralCombat/docs/municoes-tabela-completa.xlsx` (aba "Resumo por Calibre") — tabela de referência com os 31 calibres do jogo e os 4 valores calibrados por parte do corpo, além do momento (N·s) de cada um.
- `KillPatch.cs:88-92` (leitura atual da tabela de chance única por calibre), `LimbKillPatch.cs:204-243` (estratégias de detecção de parte do corpo pós-morte), `LimbKillPatch.cs` (agrupamento por `FireIndex` já existente pra vivos, reaproveitável pro mecanismo A).
- Item de backlog anterior: `003-bloquear-desmembramento-boss-vivo` (bloqueio de Boss/escolta vivos, que este item preserva).
- Conversa da sessão 2026-09-09/2026-09-10 (`mods/VisceralCombat/memory/sessions.md`) — histórico completo da investigação que motivou este item (fórmula de momento existente, achado do `.50 BMG` ausente, assets de cabeça não conectados).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Item criado via `/add-backlog-item` |
| 2026-09-10 | Revisão `/review-spec` — 2 corner cases/critérios amarrados à condição do asset de "estourar" e ao padrão de rede já existente (sem gaps que exijam decisão humana antes da spec técnica) |
