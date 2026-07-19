# Backlog — stancesAndCameraPositionSPT4.0.11

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
| --- | --- | --- | --- | --- |
| 001 | Stamina e velocidade por postura | Adiciona controle de drain de stamina e multiplicador de velocidade (50–100%) por postura, com props no F12. | [001-stamina-e-velocidade/](./001-stamina-e-velocidade/) | 🟢 |
| 003 | Stamina Multiplier — faixa até 10 | Amplia o teto de `Stance X Stamina Multiplier` de 3.0 para 10.0 nas 4 stances. | [003-stamina-multiplier-faixa-10/](./003-stamina-multiplier-faixa-10/) | 🟢 |
| 002 | Ciclo linear, hotkeys e snap fogo | Ciclo de scroll não-circular, teclas dedicadas por stance, e snap automático para Stance 0 ao atirar nas Stances 1/2/3. | [002-ciclo-linear-hotkeys-snap-fogo/](./002-ciclo-linear-hotkeys-snap-fogo/) | 🟢 |
| 004 | Apoiar a arma em superfícies | Mount próprio **descartado** (suprimia o mount vanilla e nunca funcionou em 0.16). Substituído pelo **011**. | [004-apoiar-arma-superficie/](./004-apoiar-arma-superficie/) | 🔴 |
| 005 | Aumentar velocidade agachar/inclinar | Multiplicadores customizáveis no F12 para acelerar as transições de agachar, deitar e inclinar. | [005-velocidade-agachar-inclinar/](./005-velocidade-agachar-inclinar/) | 🟢 |
| 006 | Sync visual das stances | Envia pacotes de rede via Fika para sincronizar visualmente posturas e montagem de arma entre jogadores. **Sem pasta de artefatos** — implementado direto, sem passar pelo ciclo de spec. A implementação foi depois **substituída pelo 014** (que corrigiu a aplicação do offset); o histórico técnico vive lá. | — | 🟢 |
| 007 | Inércia e velocidade máxima | Ajusta peso (inércia/turn penalty) e diminui velocidade máxima de caminhada/corrida com sliders no F12. | [007-inercia-velocidade-maxima/](./007-inercia-velocidade-maxima/) | 🟢 |
| 008 | Stance para Recarga e Checagem | Altera automaticamente para a Stance "Pronto Alto" ao recarregar/checar arma e retorna à stance original após o término. | [008-stance-recarga-checagem/](./008-stance-recarga-checagem/) | 🟢 |
| 009 | Animação Orgânica (Wiggle) | Adiciona a animação de "jogar" a arma para frente/trás (Wiggle Effect) ao trocar de postura, fugindo do movimento 100% linear. | [009-animacao-transicao-stances/](./009-animacao-transicao-stances/) | 🟢 |
| 010 | Manual Chambering | Impede o auto-chamber da primeira bala (no spawn, equip e reload com câmara vazia) — o jogador puxa o ferrolho manualmente. Toggles separados por cenário no F12. ⚠️ **Sem spec** (funcional ou técnica) — a pasta só tem o `06-fix-01`. Implementado direto; o comportamento vigente está em `Patches/ManualChamberingPatches.cs` e nas opções do F12. | [010-manual-chambering/](./010-manual-chambering/) | 🟢 |
| 011 | Mount passivo sobre o vanilla | Reconstrói o mount sobre o nativo do EFT: ativo = mount vanilla; passivo = buffs leves de stamina/recoil/sway + ícones direcionais (left/right/down) ao encostar, sem tecla. Substitui o 004. | [011-mount-passivo-vanilla/](./011-mount-passivo-vanilla/) | 🟢 |
| 012 | Controlador central de stamina de braço | Controlador único que escreve a HandsStamina (neutraliza o vanilla), com multiplicador por cenário (Stand/Prone/Passive/Active × Stance/ADS/Hold Breath) num grupo F12 "Stamina Management" + debug de estado. Evolui o coordenador do 06-fix-01. | [012-controlador-central-stamina/](./012-controlador-central-stamina/) | 🟢 |
| 013 | Refinamentos de transição de stance | Arma montada (stationary) reconhecida como Mount Active + força Stance 0 ao entrar; corrida a partir de Stance 1/2/3 não "pisca" pela Stance 0. | [013-refino-transicao-stance/](./013-refino-transicao-stance/) | 🟢 |
| 014 | Corrigir sync visual de stances no Fika | Aplicação remota mexia no Spine3 (só torso) → arma ficava imóvel. Corrige para aplicar o offset no WeaponRootAnim (braço+arma juntos), de forma aditiva, coexistindo com lean/troca de ombro. Substitui a implementação do 006. | [014-sync-stances-fika/](./014-sync-stances-fika/) | 🟢 |
| 015 | Bloquear mount ativo em Stance 1/2/3 | Impede o mount vanilla (apoiar a arma em superfícies) quando o jogador está em Stance 1/2/3 — só permite em Stance 0 ou ADS. Fase 2 do corner-case do 012, via patch em `TryMountWeapon`. | [015-bloquear-mount-ativo-stances/](./015-bloquear-mount-ativo-stances/) | 🟢 |
| 016 | Fork realism: transições por curvas + gate de aim-speed | **CANCELADO na F0 (NO-GO).** O usuário testou o Fontaine-StanceOverhaul standalone e não achou a experiência melhor — portar a sensação dele herdaria o que foi rejeitado. Fork `modded-realism/` removido; Fontaine vendorizado mantido como referência. Os bugs que motivavam (overshoot Low Ready→ADS, braço deformado) **continuam abertos** e serão atacados por abordagem própria — ver [017](./017-transicao-ads-cirurgica/). | [016-transicao-realism-fork/](./016-transicao-realism-fork/) | 🔴 |
| 017 | Transição Low/High Ready → ADS cirúrgica (waypoint Stance 0 + atenuar offset por comprimento) | Ataca os 2 bugs reais que o 016 mirava, sem curvas do Fontaine. **(A)** overshoot ao mirar (a arma sobe além da mira antes de descer, pior em armas leves; High Ready faz "onda" de cima p/ baixo): ideia = transição rápida e smooth para Stance 0 ANTES do ADS, em vez de ir direto. **(B)** braço esquerdo quebra em Low Ready → Stance 0 com armas longas (a arma desloca p/ frente e o braço hiperestende): ideia = atenuar o offset longitudinal em função do comprimento da arma / distância dos IK markers de mão. | [017-transicao-ads-cirurgica/](./017-transicao-ads-cirurgica/) | 🟡 |
| 018 | Rastejar rápido (crawl + run / high-crawl) | Enquanto prone, permitir um rastejar acelerado ("high-crawl") acionado por **andar-para-frente + agachado + correr** — mobilidade tática sem levantar. Ideia bruta; pendente investigação (o EFT tem high-crawl nativo? só velocidade ou animação?) e spec. | [018-rastejar-rapido/](./018-rastejar-rapido/) | ⚪ |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Estado (2026-07-19)

**Em progresso:** 017 (transição ADS cirúrgica — F1 entregue e em releases 2.7–2.9; F2 fechado por outro caminho).
**Em aberto:** 018 (rastejar rápido — ideia bruta, não investigada). Demais: 14 entregues, 2 cancelados (004, 016).

⚠️ **Os itens foram validados in-game sobre a build anterior à v2.0.0.** A reorganização do F12 (que removeu 23
propriedades e 9 campos) só passou a rodar no jogo com a **v2.0.0** — a revalidação é a pendência **P-7.1**
(ver [`memory/sessions.md`](../memory/sessions.md)).

**Sobre os checkboxes das specs:** eles são **checklist de referência**, não registro de execução. A validação
foi feita **por feature**, não critério a critério, e está registrada na memória — caixas desmarcadas **não**
significam item não testado.

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
