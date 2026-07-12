# 015 — Bloquear mount ativo em Stance 1/2/3

> **Status de validação:** 🟢 **Entregue e validado in-game** (2026-07-11).
> A validação foi **funcional, por feature** — não critério a critério. Os checkboxes deste documento
> são **checklist de referência**, não registro de execução: o fato de estarem desmarcados **não** significa
> que o item não foi testado. A evidência do teste vive em [`memory/sessions.md`](../../memory/sessions.md).<br>
> ⚠️ Essa validação rodou sobre a build **anterior** à reorganização do F12. A revalidação sobre a **v2.0.0**
> é a pendência **P-7.1** (ver a memória).

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Entregue
**Criado:** 2026-07-09

## Visão geral

Completa o corner-case do item 012 ("mount só vale em Stance 0 ou ADS"). O item 011 já bloqueia o mount **passivo** (buffs + ícones ao encostar) quando o jogador está em Stance 1/2/3; falta bloquear o mount **ativo** — o mount nativo do EFT, em que o jogador apoia a arma numa superfície (janela, muro, saco de areia) e ganha estabilização real. Após esta entrega, apoiar a arma numa superfície só será permitido em Stance 0 (Vanilla) ou enquanto estiver mirando (ADS).

## Comportamento atual

- O **mount ativo vanilla** do EFT pode ser acionado em **qualquer stance**, inclusive nas posturas customizadas Stance 1 (High Ready), Stance 2 (Low Ready) e Stance 3 (Custom).
- Resultado incoerente: o jogador pode estar com a arma numa pose "pronta/levantada" (High Ready) e, ao apontar para uma superfície, ainda ativar o mount — a arma "gruda" na superfície numa pose que não combina com a postura ativa.
- O item **011 (mount passivo)** já trata o passivo: em Stance 1/2/3 os ícones/buffs passivos não aparecem (só Stance 0/ADS). Mas o mount **ativo** (a estabilização real) continua livre — este item fecha essa lacuna.

## Comportamento desejado

- O mount ativo vanilla só é **permitido** quando o jogador está em **Stance 0** OU **mirando (ADS)** — independentemente da stance nesse último caso.
- Em **Stance 1/2/3 sem ADS**, a tentativa de montar a arma numa superfície **não ativa** o mount (nem o prompt/indicador de mount aparece, nem a estabilização é aplicada).
- O comportamento nativo em Stance 0 e em ADS permanece **100% intacto** — nada de novo drift, prompt sumido ou estabilização perdida nesses casos.

## Critérios de aceite

- [ ] Em **Stance 1/2/3 sem ADS**, apontar a arma para uma superfície montável **não** ativa o mount vanilla (sem prompt, sem estabilização).
- [ ] Em **Stance 0**, o mount vanilla funciona exatamente como no jogo base (prompt aparece, estabiliza).
- [ ] Em **qualquer stance COM ADS**, o mount vanilla funciona (o ADS libera o mount mesmo estando em Stance 1/2/3).
- [ ] Montado (Stance 0), o input de **trocar para Stance 1/2/3 é ignorado** — o jogador fica preso em Stance 0 com o mount mantido. <!-- revisado no code-review 01 (CR-01-01): o item 013 já força Stance 0 enquanto montado; adotado "travar em Stance 0" em vez de "desmontar" (decisão do usuário 2026-07-09). -->
- [ ] O bloqueio é uma **restrição de ativação** — não altera armas já estabilizadas por caminhos legítimos (Stance 0/ADS) nem quebra o fluxo nativo. Armas com **bipé** não são afetadas.
- [ ] **Fika/multiplayer:** o bloqueio é uma restrição de **ativação local** do MainPlayer — em Stance 1/2/3 o jogador simplesmente não monta, então não há estado de mount a sincronizar. O **desmontar automático** deve reusar o caminho de unmount vanilla (que já propaga via Fika como um desmontar normal). Peers/servidor intactos (AP-02). <!-- review: (tech-spec) confirmar que (a) suprimir a ativação não deixa o cliente inconsistente vs. servidor e (b) o desmontar forçado ecoa igual ao manual — testar como CLIENTE, não só host (solo=host mascara bugs de cliente — ver feedback_coop_multiplayer_sync) -->
- [ ] **Estado entre raids:** o bloqueio é reavaliado a cada frame pela stance/ADS atuais — sem estado persistente próprio; raid1 → exit → raid2 (e morte/MIA) começam limpos.

## Corner cases

- [ ] **Já montado e troca de stance (REVISTO no CR-01-01):** o item 013 já força Stance 0 enquanto montado e engole o input de stance → o jogador **fica preso em Stance 0** montado (não desmonta, não troca). Decisão do usuário (2026-07-09): manter esse comportamento ("travar") em vez de "desmontar". O código de desmontar do 015 foi removido (era morto).
- [ ] **Solta o ADS estando em Stance 1/2/3 com mount ativo:** se o ADS era o que liberava o mount, soltar o ADS em Stance 1/2/3 deve desmontar. Verificar que a transição é suave (sem flicker/estabilização presa). É o mesmo mecanismo do desmontar-ao-entrar-em-stance acima.
- [ ] **Interação stance × ADS (ambiguidade a resolver):** quando o jogador mira (ADS) estando em Stance 1/2/3, a pose de stance é suspensa (a arma vai para a pose de mira) ou coexiste? Isso define o gate: se o ADS já suspende a stance, "Stance 0 OU ADS" vira simplesmente "não está numa pose de stance ativa". <!-- review: (tech-spec) esclarecer a relação stance↔ADS no mod atual — determina como o gate é escrito e se o desmontar dispara ao mirar/desmirar -->
- [ ] **Spam de troca de stance montado (race):** trocar rapidamente Stance 0 ↔ 1 ↔ 0 com mount ativo — o desmontar deve ser **idempotente** (não empilhar/travar), e voltar a Stance 0 permite montar de novo. Sem estado preso entre as trocas.
- [ ] **Bipé (bipod) (DECIDIDO):** é **exceção** — não bloqueado. O bipé é estado físico da arma (deployado na superfície), não input de mount de superfície; continua funcionando em qualquer stance. (Decisão do usuário, 2026-07-09.)
- [ ] **Prone:** deitado, o jogo já estabiliza a arma; confirmar que o bloqueio não interfere no comportamento de prone (que é um estado próprio, não Stance 1/2/3).
- [ ] **Interação com o 011 (passivo) e o 013 (arma montada/stationary):** o passivo já é bloqueado em 1/2/3; o 013 força Stance 0 ao entrar em arma montada (turret). Garantir que os três não conflitam — o 015 trata só o mount ativo de **superfície**.

## Fora de escopo

- [ ] Mudar o mount **passivo** (item 011) ou a lógica de **arma montada/stationary** (item 013) — este item toca só o mount ativo de superfície.
- [ ] Alterar os buffs/estabilização do mount em si (magnitude de recoil/sway) — só a **condição de ativação**.

## Referências

- [012 — Controlador central de stamina](../012-controlador-central-stamina/012-controlador-central-stamina-01-spec.md) (corner-case "mount só vale em Stance 0 ou ADS"; este item é a fase 2)
- [011 — Mount passivo sobre o vanilla](../011-mount-passivo-vanilla/011-mount-passivo-vanilla-01-spec.md) (sistema irmão; já bloqueia o passivo)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-09 | Item criado via `/add-backlog-item` |
| 2026-07-09 | Decisões de design do usuário: desmontar automaticamente ao entrar em Stance 1/2/3; bipé é exceção (não bloqueado). |
| 2026-07-09 | Revisão `/review-spec` — critério Fika reforçado (coop-sync, testar como cliente); +2 corner cases (stance×ADS, spam de troca montado); 2 `<!-- review -->` técnicos para a tech-spec resolver. |
