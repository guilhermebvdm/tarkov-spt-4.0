# Protocolo de Teste Coop/Bots — Trauma 2.0

> **Data:** 2026-07-20<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [trauma-behavior-matrix.md](./trauma-behavior-matrix.md), [coop-heal-matrix.md](./coop-heal-matrix.md)<br>

---

Roteiro de validação manual para o item 009 (Bloco B — hardening coop/bots). Nenhum cenário aqui foi executado ainda; este documento entrega o roteiro pronto, não a validação em si (mesma natureza da pendência P-4.4 registrada na memória do mod: nenhum item do overhaul Trauma 2.0 foi validado in-game de fato). Complementa o plano de teste geral de [trauma-behavior-matrix.md §5](./trauma-behavior-matrix.md) (44 cenários, a maioria executável solo) com os cenários que exigem especificamente bots com IA de terceiros ativa (B1) ou um segundo humano (B2).

## B1 — Smoke test SAIN/ORBIT do re-derrubar de bot

**Executável em raid SOLO** (host com bots) — não exige 2º PC. Confirma que a camada BigBrain do mod (prioridade 90, item 004) e as camadas de decisão de SAIN/ORBIT convivem sem travar o bot num estado inconsistente.

**Pré-condição:** SAIN e ORBIT instalados e ativos (confirmar em `docs/trauma-compat-suite.md` que ambos estão presentes na instalação).

**Roteiro:**

1. Entrar em raid solo com bots ativos (SAIN e/ou ORBIT controlando a IA).
2. Localizar ou causar (tiro controlado) a quebra das 2 pernas de um bot, sem matá-lo.
3. Observar: o bot deve cair (prone forçado) e ficar pelo menos X segundos (config `Bot Fall Hold Seconds`, default 15s) sem atirar nem se locomover — log `[Trauma2]` deve confirmar o hold.
4. Após X segundos, observar a IA (SAIN/ORBIT) tentando levantar o bot.
5. Se a fratura das pernas ainda persistir (não curada), o bot deve ser **re-derrubado** — voltando ao passo 3 (novo hold de X segundos) — não deve ficar "preso de pé" tentando andar com a condição ativa, nem preso deitado indefinidamente sem a IA tentar de novo.
6. Repetir por pelo menos 2 ciclos completos (cair → hold → tentar levantar → re-cair) para confirmar que o ciclo não trava numa das duas pontas.
7. Curar as pernas do bot (ex. com um tiro certeiro que o mate não serve — precisa ser um cenário onde a cura acontece, ou aceitar como corner não coberto se não houver forma de curar bot deliberadamente) — OU aceitar que este passo é observação de longo prazo (o bot eventualmente morre ou o jogador avança).
8. Repetir o roteiro com CustomClasses-Tank ativo num personagem/bot Tank especificamente, se possível — para exercitar a prova de mecanismo (§`docs/trauma-compat-suite.md`) num cenário real, não só a composição de velocidade em teoria.

**Critério de sucesso:** nenhum bot fica travado (de pé andando com a condição ativa, ou deitado permanentemente sem a IA reagir); log confirma os ciclos de hold/re-derrubar sem erro/exception.

## B2 — Protocolo de teste 2 PCs (coop Fika)

**Exige 2 PCs** (ou 1 PC + 1 headless, para os cenários que não dependem de percepção humana do 2º peer). Reaproveita o esqueleto de `docs/coop-heal-matrix.md` (ordem de custo crescente).

**Pré-condição:** mod na MESMA BUILD em todas as máquinas (lição já registrada em `coop-heal-matrix.md` — wire-format de pacotes exige isso).

### Cenários (ordem de custo crescente)

1. **Mancar (003) visível ao peer** — P1 zera/quebra uma perna; P2 observa: manqueira (N1/N2) deve ser visível na animação de P1 pelo sync nativo de pose do Fika, sem qualquer lag perceptível além do já esperado da rede.
2. **Ciclo de queda (004) visível ao peer** — P1 quebra as 2 pernas sem analgésico; P2 observa a queda (prone forçado), a janela de 3s de pé, e o re-cair automático — tudo sincronizado corretamente na tela de P2.
3. **Tremor (005) — CONFIRMAR que o peer NÃO vê** (é limitação aceita por design, não bug): P1 com 2 braços comprometidos deve sentir o tremor na própria mira; P2 não deve ver nenhum indicativo visual do tremor em P1 (efeito é só-primeira-pessoa). Cancelamento de ADS (a arma abaixando) DEVE ser visível a P2 (efeito nativo do jogo, não o tremor em si).
4. **Agachar do estômago (006) visível ao peer** — P1 zera o estômago e tem sucesso no roll; P2 observa o agachar involuntário de P1.
5. **Desmaio percentual (007) e duração aleatória (008)** — P1 sofre um tiro que atinge o gatilho de desmaio; P2 observa P1 desmaiando, e a duração (variável, min-max configurado) deve ser a MESMA percebida por P1 e P2 (sincronizada via pacote).
6. **Vozes de dor audíveis ao peer** — P1 sofre uma queda forçada (004) ou tem uma tentativa de re-ADS bloqueada (005); P2 deve ouvir a voz de dor de P1 (`OnAgony`) pela voz nativa do jogo.
7. **Bots do host coerentes ao client** — com P1 como host e bots ativos, P2 (client) deve ver os bots mancando/caindo/agachando corretamente, sem "pular" para um estado diferente do que P1 (host) vê.
8. **Toggle OFF/ON mid-raid, replicado nos 2 PCs** — desligar um consumidor (ex. "Fall Cycle") no meio da raid em P1: os efeitos ativos de P1 somem na tela de P1 E de P2 simultaneamente (sem residual em nenhum dos dois lados).
9. **Teste negativo de deploy** — 1 raid com versões DIFERENTES do mod entre os peers (ou o mod ausente num deles): confirmar que o comportamento é gracioso (sem exception que descarte outros pacotes de rede do frame, mesma lição do `coop-heal-matrix.md` "Requisito de deploy") — não é esperado que a coop funcione corretamente nesse cenário, só que falhe de forma contida.

**Critério de sucesso:** todos os 9 cenários executados sem exception nova, com o comportamento observável batendo com o descrito (inclusive as 2 limitações aceitas — tremor não visível ao peer, dor pode ser engolida por colisão de voz em casos raros).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-20 | Guilherme (com Claude) | Criação — protocolo B1 (smoke solo SAIN/ORBIT) + B2 (2 PCs), item 009 do backlog. Roteiro entregue, execução pendente (mesma natureza de P-4.4). |
