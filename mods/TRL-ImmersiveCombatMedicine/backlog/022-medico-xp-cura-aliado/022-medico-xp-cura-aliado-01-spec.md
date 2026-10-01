# 022 — Médico ganha XP ao curar aliado

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Curar um ALIADO é uma mecânica própria do ICM (não existe no EFT vanilla, onde só se trata a si mesmo). Hoje, ao completar esse tratamento, **nenhum XP de skill é concedido a ninguém** — nem ao médico que aplica, nem ao aliado que recebe. O médico deve passar a ganhar XP, na **mesma quantidade** que o vanilla credita a um jogador que trata aquele mesmo tipo de ferimento em si mesmo — sem inventar um valor novo. **Escopo expandido em 2026-09-09** (revisão técnica PA-01-01): o vanilla tem DOIS mecanismos de XP de cura — (1) `HealExperience` por efeito curável resolvido (sangramento/fratura/intoxicação/dor) e (2) XP proporcional ao HP puro restaurado (`ExpForHeal`, mesmo quando nenhum efeito é removido). Este item cobre **os dois**.

## Comportamento atual

- O vanilla só concede XP de cura quando o próprio dono do efeito o resolve (auto-cura): cada efeito curável expõe seu valor de XP e o consome no momento em que é resolvido naturalmente, creditando ao profile de quem é dono do efeito.
- No ICM, curar um ALIADO segue um dos dois caminhos abaixo, e em nenhum dos dois esse crédito de XP chega ao médico:
  - **Caminho A — paciente local** (bot ou personagem com controlador de saúde real no processo local): o pipeline de XP do médico é bloqueado explicitamente antes de rodar; o efeito é criado/resolvido do lado do paciente, e — se algum XP fosse gerado por essa via — iria para o paciente, nunca para o médico.
  - **Caminho B — paciente remoto** (rede Fika, aplicação do tratamento no peer remoto): HP e efeitos são alterados/removidos por uma via direta que não passa pela resolução natural de efeito que o vanilla usa para disparar XP — logo esse caminho não gera XP para nenhum dos dois lados.
- Nenhuma documentação do mod (specs, reviews, matriz de trauma, memória de sessões) menciona XP de cura de aliado — é lacuna não tratada, não decisão deliberada.

## Comportamento desejado

- Ao curar um ALIADO (Caminho A ou B), o **médico** passa a ganhar XP de skill.
- O valor de XP concedido ao médico, para cada efeito realmente resolvido pelo tratamento, é **idêntico** ao valor que o vanilla concederia se aquele mesmo efeito, do mesmo tipo (sangramento, fratura, intoxicação, dor) e da mesma severidade, fosse resolvido por auto-cura — sem valor novo/arbitrário criado pelo mod.
- Se o valor vanilla variar por tipo de ferimento e/ou severidade, o mod replica essa variação fielmente (a confirmar contra o decompile na spec técnica).
- **Adicionalmente**, quando o tratamento restaura HP (com ou sem remover um efeito), o médico ganha o XP proporcional que o vanilla concederia pelo mesmo HP restaurado em auto-cura (fórmula vanilla `ExpForHeal × HP restaurado`, configurável no servidor — nunca um multiplicador novo do mod).
- O paciente continua sem ganhar XP pelo tratamento recebido de outra pessoa — isso não muda (nem pelo XP de efeito, nem pelo XP de HP).
- Cobre os dois caminhos (A e B) — hoje ambos são mudos em XP para o médico, nos dois mecanismos.

## Critérios de aceite

- [ ] Ao curar sangramento de aliado (qualquer severidade), o médico ganha XP idêntico ao que o vanilla concede ao tratar aquele mesmo sangramento em si mesmo.
- [ ] Ao curar fratura, intoxicação ou dor (Pain) de aliado, o médico ganha o XP correspondente a cada efeito — replicando fielmente qualquer variação vanilla por tipo/severidade (não usa valor fixo único).
- [ ] Ao restaurar HP de um aliado (com ou sem remover um efeito na mesma aplicação), o médico ganha XP proporcional ao HP restaurado, na mesma fórmula/multiplicador que o vanilla usa em auto-cura — inclusive quando a cura é gradual (ex.: MedKit ao longo do tempo), sem perder XP por arredondamento de incrementos pequenos.
- [ ] O paciente aliado curado não ganha XP pelo tratamento recebido — nem o de efeito, nem o de HP (comportamento atual desse lado permanece inalterado).
- [ ] Ao resolver múltiplos efeitos na mesma aplicação de tratamento (ex.: um kit que resolve sangramento e fratura juntos), o XP de cada efeito resolvido é somado ao médico sem duplicar nem perder nenhuma parcela.
- [ ] **Fika/multiplayer:** o médico só pode ser o `MainPlayer` local de quem inicia a interação de cura (confirmado: o mod só permite iniciar tratamento pelo jogador humano local — bots nunca são médicos <!-- review: reconfirmar contra o decompile atual na spec técnica, mas já validado nesta revisão via MedicHealPatch.cs:253-254,546-547 -->); funciona igualmente quando esse médico local é o host tratando um peer remoto (Caminho B) e quando é um client remoto tratando um bot local a ele (Caminho A) — o XP é creditado uma única vez, no profile local do médico que efetivamente executou o tratamento, nunca replicado para outro peer.
- [ ] **Estado entre raids:** N/A: XP de skill é sistema de persistência 100% vanilla (Profile.EftStats/progressão de skill); o item não introduz nenhuma persistência nova — só passa a chamar o crédito de XP vanilla em um caminho onde hoje ele nunca é chamado.

## Corner cases

- [ ] Médico realiza auto-cura (tratando a si mesmo) com o mod ativo — o crédito novo do ICM não deve duplicar o XP que o vanilla já concede nesse caminho; o crédito explícito só se aplica a cura de ALIADO.
- [ ] Tratamento é cancelado/interrompido antes de completar — nenhum XP deve ser concedido por efeitos que não foram efetivamente resolvidos.
- [ ] Paciente morre, desconecta ou o efeito deixa de existir por outro motivo (ex.: já removido) enquanto o tratamento está em andamento — nenhum XP é concedido por esse efeito.
- [ ] Dois médicos tratando o mesmo aliado ao mesmo tempo (concorrência sobre os mesmos efeitos) — XP não é concedido em duplicidade para o mesmo efeito resolvido.
- [ ] Caminho B (rede): pacote de conclusão de tratamento chega duplicado, fora de ordem ou é reenviado — XP não é creditado mais de uma vez pelo mesmo tratamento.
- [ ] Skill do médico já no teto — crédito de XP não causa erro/overflow (mesmo comportamento do vanilla ao exceder o limite).
- [ ] Efeito sem valor de XP definido, ou com valor zero — não credita XP negativo/nulo nem lança exceção.
- [ ] Tratamento apenas reduz a severidade/HP restante do efeito sem REMOVÊ-LO por completo (ex.: bandagem que estanca mas não consome todo o sangramento, ou resource do kit insuficiente para fechar o efeito) — XP só é concedido quando o efeito é de fato resolvido/removido, igual à regra vanilla (Residue/Removed), nunca por tratamento parcial. <!-- review: confirmar no decompile se HealExperience vanilla já é condicionado à remoção completa do efeito ou se paga por qualquer redução — decisão afeta o ponto exato de crédito na spec técnica -->
- [ ] Definição de "aliado" elegível para o crédito de XP (bot amigável, PMC/Scav do mesmo grupo Fika, NPC de escolta) segue a mesma definição já usada pelo sistema de interação de cura do ICM (`docs/coop-heal-matrix.md` e/ou `MedicInteractable`) — este item não redefine quem pode ser paciente, só decide se aquele tratamento já permitido gera XP.
- [ ] Cura de HP puro em incrementos pequenos e frequentes (ex.: MedKit curando ao longo do tempo) não deve perder XP por truncar cada incremento individualmente para inteiro — o total de XP de HP pago ao médico ao fim do tratamento deve corresponder ao HP total restaurado naquela operação, não à soma de arredondamentos por tick que zeram incrementos sub-unitários.
- [ ] MedKit que só restaura HP sem nenhum efeito ativo no aliado (nenhum sangramento/fratura/intoxicação/dor a resolver) ainda credita o XP de HP ao médico — os dois mecanismos (efeito e HP) são independentes; a ausência de um não bloqueia o outro.
- [ ] Qualquer HP restaurado no aliado enquanto o médico está tratando-o conta pro XP do médico, mesmo que não venha diretamente do item aplicado (ex.: alguma regeneração passiva concorrente) — decisão aceita (PA-02-01): o vanilla também não filtra a origem do HP restaurado ao calcular esse XP, então o mod não inventa um filtro que o vanilla não tem.

## Fora de escopo

- [ ] Alterar o valor de `HealExperience` do vanilla ou criar uma categoria de XP nova/arbitrária diferente da vanilla (constraint explícita do usuário).
- [ ] Conceder XP ao paciente pelo tratamento recebido (mantém-se mudo — ver critério de aceite correspondente).
- [ ] A definir (demais pontos)

## Referências

- Investigação prévia (mecanismo vanilla de XP de cura, os dois caminhos A/B do ICM) — a reconfirmar linha a linha contra o decompile atual na spec técnica (regra AP-09, `docs/technical/spt-antipatterns.md`).
- `mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/MedicHealPatch.cs` — Caminho A.
- `mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs` — Caminho B.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` |
| 2026-09-09 | Revisão `/review-spec` — 2 gaps + 2 corner cases corrigidos/adicionados |
| 2026-09-09 | Escopo expandido (decisão do usuário em PA-01-01, `/review-technical-spec` rodada 01): inclui também XP de HP restaurado (`ExpForHeal`), além do `HealExperience` por efeito |
