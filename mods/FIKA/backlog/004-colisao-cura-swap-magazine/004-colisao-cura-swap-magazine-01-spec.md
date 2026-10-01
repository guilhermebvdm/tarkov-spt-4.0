# 004 — Swap de carregador rejeitado após curar aliado (colisão com transição de mãos não-magazine)

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-08

## Visão geral

Em raids coop via Host/Headless, um jogador que acabou de curar um aliado (via item de cura, ex. bandagem ou kit médico) e, logo em seguida, arrasta um carregador para cima da própria arma equipada (swap por drag-and-drop) tem essa troca rejeitada pelo servidor. A arma some das mãos do jogador e a mão trava — nem trocar de slot nem o botão de emergência de destravar mãos resolve, exigindo reconectar ou terminar a raid. O item `003-magazine-swap-inplace-fix` já corrigiu um caso parecido (colisão do swap de magazine consigo mesmo), mas a exceção aplicada lá só cobre quando a transição de mãos anterior foi aberta por outro carregador — não cobre quando foi aberta por um item de cura.

## Comportamento atual

1. Jogador A usa um item de cura em um aliado (jogador B) e a animação de cura é concluída.
2. A finalização da cura devolve a arma equipada às mãos do jogador A — isso abre e fecha internamente uma transição de "entrando/saindo das mãos" para a arma, associada ao item de cura (não a um carregador).
3. Se, logo em seguida (dentro da janela curta de tolerância a colisões, hoje ~0,35 s), o jogador A arrasta um carregador para a própria arma equipada (swap 1-para-1), o cliente aplica a troca localmente de forma otimista (a UI já mostra o carregador novo).
4. O servidor (Host/Headless) recebe o pedido de swap, encontra a transição de mãos ainda associada ao item de cura (não a um carregador) e rejeita a troca por concorrência, pois a exceção de tolerância existente só reconhece colisões abertas por outro carregador.
5. Resultado: divergência entre cliente (troca aplicada) e servidor (troca rejeitada) — o inventário do jogador trava permanentemente pelo resto da raid. Nem alternar de slot nem o botão de emergência de destravar mãos resolve; a arma "some" das mãos do jogador.

## Comportamento desejado

1. A troca de carregador por drag-and-drop na própria arma equipada deve ser aceita pelo servidor mesmo quando a transição de mãos mais recente naquela arma foi aberta por qualquer ação legítima do próprio jogador na própria arma (incluindo a devolução da arma às mãos ao final de uma cura) — não apenas quando aberta por outro carregador.
2. A proteção de concorrência real deve continuar bloqueando a colisão quando a transição de mãos mais recente pertence a **outro jogador** ou a **outro item/arma** — isto é, a tolerância deve ser ampliada apenas para o escopo "mesma arma, mesmo jogador, janela recente", nunca para concorrência genuína entre jogadores diferentes.
3. Após o fix, curar um aliado e imediatamente trocar o carregador da própria arma não deve travar a mão do jogador nem causar divergência entre cliente e servidor.

## Critérios de aceite

- [ ] Curar um aliado e, dentro da janela de tolerância a colisões, arrastar um carregador sobre a própria arma equipada resulta em troca de carregador bem-sucedida tanto no cliente quanto no servidor (sem divergência de status).
- [ ] A arma permanece visível e utilizável nas mãos do jogador após a sequência cura → swap de carregador (não "some").
- [ ] O jogador não precisa usar o botão de emergência de destravar mãos nem reconectar após essa sequência.
- [ ] Uma tentativa de swap de carregador que colide com uma transição de mãos aberta por **outro jogador** (concorrência real, item diferente) continua sendo rejeitada normalmente — a proteção original não pode ser enfraquecida.
- [ ] Repetir a sequência cura → swap de carregador várias vezes seguidas (inclusive curando a si mesmo, e curando mais de um aliado em sequência) não introduz travamento nem divergência.
- [ ] **Fika/multiplayer:** comportamento validado com pelo menos dois jogadores conectados a um Host/Headless dedicado — jogador A cura jogador B, depois troca o próprio carregador; jogador B (observador) não deve ver nenhuma anomalia visual na arma ou no inventário de A.
- [ ] **Estado entre raids:** a correção não introduz nenhum estado persistente entre raids (a janela de tolerância é por-transição, escopada à raid corrente) — sair e entrar em uma nova raid não deve herdar nem acumular nenhum resquício da raid anterior.

## Corner cases

- [ ] Jogador cura a si mesmo (self-heal) e em seguida troca o próprio carregador — mesma janela de tolerância deve se aplicar.
- [ ] Jogador cura um aliado, mas antes de trocar o carregador, troca de arma (arma secundária) — a tolerância não deve vazar para a arma nova quando a transição de mãos que a abriu não é dela.
- [ ] Dois jogadores diferentes mexem quase simultaneamente em armas diferentes (um curando, outro trocando carregador na própria arma) — cada um deve ser avaliado de forma independente, sem um afetar a tolerância do outro.
- [ ] Cura é interrompida (jogador cancela ou é interrompido por dano) antes de concluir — não deve abrir uma transição de mãos "órfã" que crie uma janela de tolerância indevida.
- [ ] Jogador troca o carregador imediatamente após reviver um aliado (fluxo de reanimação, que também mexe nas mãos) — avaliar se esse fluxo aciona o mesmo tipo de transição de mãos e se deve ou não ser coberto pela mesma tolerância.
- [ ] Janela de tolerância expira (ação de swap ocorre bem depois da cura, fora da janela) — a rejeição original deve continuar valendo (não é para tolerar qualquer colisão indefinidamente).
- [ ] Sequência ocorre logo após o jogador entrar na raid (poucos segundos de jogo) ou logo antes de sair — não deve haver diferença de comportamento em relação ao meio da raid.
- [ ] Outras ações do próprio jogador que também abrem uma transição de "entrando/saindo das mãos" na própria arma (ex.: arremessar granada, sacar/guardar faca, reanimar um aliado — ver critério de reanimação acima) e são seguidas de swap de carregador — devem se beneficiar da mesma tolerância generalizada, não só o caso específico de cura.
- [ ] Uma transição de mãos aberta nos instantes finais de uma raid não deve deixar resquício que afete o swap de carregador de uma raid **seguinte** (mesmo jogador, nova sessão de raid) — risco de estado estático não limpo no fim da raid. <!-- review: confirmar na spec técnica que o mecanismo de tolerância é escopado por objeto/transição (não por campo estático global) e que há um ponto de limpeza no fim da raid; se o `GraceWindowSeconds` (pendência P-3.1 do mod) for reaproveitado, citar essa dependência -->

## Fora de escopo

- [ ] Qualquer mudança em `mods/UIFixes/` — o swap por drag-and-drop já delega corretamente pro pipeline nativo de troca; o mecanismo de tolerância a ser ampliado é inteiramente do lado do FIKA (validação no Host/servidor).
- [ ] Limpeza do patch órfão do UIFixes no alvo errado (pendência `P-3.2` do mod FIKA, "Trilha B do item 003") — item de backlog separado, não relacionado a este bug.
- [ ] Calibração definitiva do valor da janela de tolerância (`GraceWindowSeconds`, hoje ~0,35 s, pendência `P-3.1` do mod FIKA) — este item reaproveita/generaliza o mecanismo existente, não recalibra o tempo. Se a spec técnica concluir que a generalização exige um valor diferente, tratar como decisão explícita, não herança silenciosa.

## Referências

- Item de backlog relacionado (fix anterior, mesma família de bug): [003-magazine-swap-inplace-fix/](../003-magazine-swap-inplace-fix/)
- Diagnóstico completo da causa raiz desta sessão: `mods/UIFixes/memory/sessions.md`, Sessão 9b (2026-09-08)
- Pendências relacionadas na memória do mod FIKA (`mods/FIKA/memory/sessions.md`): `P-3.1` (calibração de `GraceWindowSeconds`) e `P-3.2` (limpeza do patch órfão no UIFixes) — ambas fora de escopo aqui, ver seção acima.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Item criado via `/add-backlog-item` |
| 2026-09-08 | Spec funcional criada via `/create-spec` |
| 2026-09-08 | Revisão `/review-spec` — 3 gaps + 2 corner cases corrigidos |
