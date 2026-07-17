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
| 016 | Fork realism: transições por curvas + gate de aim-speed | Fork experimental `modded-realism/` que troca a mola sub-amortecida por transições determinísticas por curvas e adiciona gate de aim-speed (portados do Fontaine-StanceOverhaul, com permissão). Mira os bugs de overshoot Low Ready→mira (~5cm) e braço deformado na transição de ADS (P-11.2). Fases F0–F4 com gate humano por fase; pode virar canônico (GO/NO-GO na F4). | [016-transicao-realism-fork/](./016-transicao-realism-fork/) | 🟡 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Estado (2026-07-11)

**Backlog fechado** — 14 entregues, 1 cancelado, nada em aberto. Trabalho novo começa por um item novo
(`/add-backlog-item`).

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
