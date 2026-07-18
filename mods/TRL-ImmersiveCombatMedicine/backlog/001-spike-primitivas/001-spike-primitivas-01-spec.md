# 001 — Spike: primitivas vanilla de trauma

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Em progresso
**Criado:** 2026-07-18

## Visão geral

Pesquisa técnica que de-risca o escopo Trauma 2.0 ([docs/trauma-matrix.md](../../docs/trauma-matrix.md)) antes de qualquer código de produção. Investiga, com prova no jogo decompilado e nos mods de terceiros da load order, as primitivas que a matriz exige: mancar por nível/lado, tremor, detecção de analgésico, agachar/derrubar involuntário, controle de levantar, vozes de dor, pontos seguros de interferência na IA, cancelamento de ADS com lockout, observação de estado de saúde e idioma do cliente. O deliverable é um documento de referência que responde cada pergunta com evidência (arquivo:linha) e recomendação de implementação — o item 002 (motor de estados) e os itens 003–007 dependem dele.

## Comportamento atual

O mod hoje implementa versões simples das mecânicas que serão substituídas: queda binária ao zerar 2 pernas com punição por levantar (fratura 30%/15 de dano — decisão 21 manda aposentar), fadiga de mira de 1 s com 2 braços zerados, "sem ar" no estômago e desmaio por dano fixo. Nenhuma dessas usa mancar vanilla, tremor gerenciado, detecção de analgésico ou interferência coordenada com SAIN/ORBIT — as primitivas necessárias nunca foram mapeadas no assembly do EFT 0.16.x.

## Comportamento desejado

Existir um documento `docs/trauma-primitives.md` (novo) que responda, com evidência decompilada e teste in-game pontual quando necessário, as dez perguntas de pesquisa abaixo — cada uma com: API/campo/efeito exato (arquivo:linha do assembly ou do mod de terceiro), limitações encontradas, e recomendação de uso (ou fallback) para os itens 002–007. Nenhum código de produção é entregue neste item (protótipos descartáveis são permitidos para provar uma API).

**Perguntas de pesquisa (P1–P10):**

- **P1 — Mancar:** quais mecânicas vanilla de "mancar" existem (animação, penalidade de velocidade, pose)? Há variação por LADO da perna ferida e por intensidade (base do N1/N2)? Qual o inventário completo das penalidades nativas de perna (fratura/zerada) que a decisão 18 manda calibrar por baixo? **Mapa dos escritores do pipeline de velocidade** — vanilla + CustomClasses + Skills Extended (D12): quem escreve, onde, em que ordem. **Bots:** o mancar vanilla/caps de velocidade afetam a locomoção SAIN-driven e o que o peer vê? — se não, recomendar mecanismo equivalente p/ bots (item 003 inclui bots). Fallback obrigatório (D10): se não houver mancar por lado/nível, recomendar composição via caps de velocidade.
- **P2 — Tremor:** como aplicar/renovar/remover o efeito Tremor nativo com lifecycle próprio (D11), sem depender do tremor-por-dor que o analgésico apaga? O efeito é visível em bots e em peers (Fika)?
- **P3 — Analgésico:** como detectar "efeito Painkiller nativo ativo" (decisão 12) de forma robusta (item comum, morfina, stims), incluindo o instante de EXPIRAÇÃO (decisão 14 exige reavaliação imediata na expiração)? A mesma detecção funciona para BOTS (avaliados no host/headless)?
- **P4 — Agachar/derrubar involuntário:** APIs para forçar agachar one-shot (sem travar pose — decisão 5) e prone forçado; guards de contexto viáveis (escada/corda/BTR/vault — D7); confirmar que quedas forçadas não geram dano de queda (D18) e que o **timer de extração roda com o player em prone forçado**.
- **P5 — Controle de levantar:** como bloquear o levantar durante o lockout do ciclo (15 s), permitir janela de 3 s, e produzir "levantar lento"; é possível simular a tentativa frustrada de levantar (decisão 6)? Vozes de dor: quais linhas vanilla servem para os dois sons (tentativa bloqueada = forte; liberação = leve) e como disparar por facção/voz do personagem, audível pelos peers. **Fallback:** se as vozes vanilla não diferenciarem os 2 sons, recomendar áudio customizado pelo pipeline já provado no repo (carregar OGG no `OnGameStarted` — lição de memória de áudio em mod client) e marcar a decisão para o usuário.
- **P6 — SAIN/ORBIT (D14):** mapa dos pontos seguros de interferência para derrubar/levantar bots — mover/pose/camadas BigBrain no SAIN 4.4.3 e ORBIT 1.1.0 (com prova no código deles em references/ ou decompilado); contrato "interferir → devolver controle → camada re-decide" (decisão 16); **avaliar explicitamente uma camada BigBrain customizada** (DrakiaXYZ-BigBrain está na load order) como mecanismo de "TraumaDowned" em vez de brigar com o mover do SAIN — incluindo **como sustentar o bot no chão por X s sem thrash** e se o bot atira/gira caído (decisão 16); **agachar one-shot em bots** (dip de pose com devolução imediata — decisões 11/16, consome o 006); diagnosticar a causa-raiz da limitação atual "levanta e nunca mais cai" como evidência; **identificar o mod provedor dos bots UNTAR** (brain/WildSpawnType) antes de concluir que não exigem tratamento distinto (D15).
- **P7 — Gatilho de dano p/ desmaio percentual:** onde interceptar o dano efetivo pós-armadura com a vida atual pré-tiro disponível (D5), no mesmo ponto (ou equivalente) do ApplyDamageInfo atual — validando que funciona para dano vindo de peers (Fika) no dono; **confirmar a granularidade do ponto de interceptação** (1 chamada por pellet/hit, não agregado por disparo — decisão 15) nas duas origens.
- **P8 — Idioma do jogo (i18n):** como detectar o idioma ativo do cliente para a decisão 22 (textos EN padrão + tradução PT quando o jogo estiver em português) — API/setting exato e momento seguro de leitura.
- **P9 — Cancela-ADS + lockout (decisões 3/17, D13, D9):** detecção do estado de mira e tempo sustentado; caminho VANILLA para cancelar ADS (o polling atual IsAiming+SetAim(false) é baseline a substituir); como **bloquear a re-entrada em ADS por 1–1,5 s** com voz de dor na tentativa; mapa dos escritores (SPTRecoilRework e Fontaine-FOVFix patcheiam o set de aim — ordem); exclusão de bots (D9) e o que o peer vê.
- **P10 — Observação de estado de saúde (decisão 1, D17, D19):** query de Zerar/Quebrar por membro (humano e bot); evento de efeito adicionado/removido local E o ponto equivalente para cura REMOTA (rede do mod); detecção de restauração de parte zerada por cirurgia; recomendação evento vs polling ≤4 Hz por transição (a expiração do analgésico — decisão 14 — exige evento).

## Critérios de aceite

- [ ] Cada pergunta P1–P10 respondida em `docs/trauma-primitives.md` com evidência `arquivo:linha` (assembly real via ilspycmd quando o dump estiver incompleto — lição da memória: 102 namespaces vazios geram falso-negativo) e recomendação explícita de implementação para o item consumidor (002–007).
- [ ] O diff do item contém APENAS documentação — nenhuma mudança em `modded/` (protótipos descartáveis não entram no repo).
- [ ] P1 entrega o inventário das penalidades vanilla de perna com valores (base da calibração N1/N2 da decisão 18) e veredito sobre mancar por lado (decisão 4 da matriz) — com fallback recomendado se negativo.
- [ ] P6 entrega o contrato de interferência bot (sequência exata de chamadas para derrubar/devolver controle) validado contra o código real do SAIN 4.4.3 e ORBIT presentes na load order.
- [ ] Toda API recomendada foi provada compilável/invocável (protótipo descartável ou assinatura confirmada no assembly) — nenhuma recomendação "por inferência de nome". Provas por protótipo registram no doc o trecho mínimo compilado/invocado e o resultado observado (auditável sem o protótipo no repo).
- [ ] **Fika/multiplayer:** para cada primitiva, o doc registra ONDE ela roda (dono/espelho) e se o efeito é visível aos peers via sync nativo (pose/velocidade/voz) — insumo do D16; `N/A` não se aplica (o mod é coop-first).
- [ ] **Estado entre raids:** `N/A: spike de pesquisa sem código de produção — persistência entre raids é responsabilidade do item 002 (motor), que consumirá as recomendações deste doc.`

## Corner cases

- [ ] Dump decompilado incompleto: APIs "inexistentes" no references/eft-decompiled devem ser re-verificadas no assembly real (`ilspycmd`) antes de um veredito negativo (já custou 2 perks dados como impossíveis em outro mod).
- [ ] Mancar/pose de players REMOTOS: primitivas que funcionam no dono podem não replicar no ObservedPlayer (overrides sem base — lição AP-03/CR-01-28); o doc deve testar/prever a visão do peer.
- [ ] Analgésico de fontes não-óbvias: stims compostos (ex.: Obdolbos), buffs de comida ou efeitos de mods (CustomClasses) que embutem Painkiller — a detecção recomendada deve capturá-los ou declarar limitação.
- [ ] Conflito de escrita em pose/velocidade com SAIN (bots), CustomClasses/SkillsExtended (humanos — D12), **Visceral Combat (pose/hit-reaction — P4) e BringBackConcussion (contusão — P2/P7)** quando o domínio sobrepor: o doc identifica QUEM mais escreve nos mesmos campos e em que ordem de patch.
- [ ] Vozes de dor sob cooldown/limites vanilla (sistema de voz tem throttle): confirmar que os dois sons do ciclo (decisão 6) disparam de forma confiável em sequência curta.
- [ ] Headless: primitivas chamadas pelo dono do BOT rodam num processo sem renderização — APIs de voz/pose/efeito recomendadas precisam ser seguras (ou no-op documentado) no headless.

## Fora de escopo

- [x] Qualquer código de produção (patches, motor, configs) — itens 002+.
- [x] Balanceamento numérico de N1/N2 e probabilidades — item 003+ com base no inventário do P1.
- [x] Sons customizados (assets próprios) — o spike só avalia as linhas de voz vanilla.

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — matriz canônica (22 decisões + defaults D1–D20)
- `references/eft-decompiled/` + `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` (ilspycmd)
- `references/fika-plugin/` (2.3.4) — sync de pose/voz; SAIN 4.4.3 e ORBIT (decompilar dos DLLs da instalação se não vendorados)
- `memory/sessions.md` — lições: dump incompleto, AP-03/observed, autoridade dono-only

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Item criado via backlog Trauma 2.0; spec funcional criada via `/create-spec` |
| 2026-07-18 | Revisão rodada A (adversarial #2): +P9 cancela-ADS/lockout (bloqueador), +P10 observação de estado (forte), granularidade por pellet no P7, bots no P1, hold/agachar-bot/UNTAR no P6, extração prone no P4, prova auditável, corners Visceral Combat/BringBackConcussion, status alinhado |
| 2026-07-18 | Revisão `/review-spec` — 5 gaps + 2 corner cases corrigidos (P8 idioma i18n; mapa do pipeline de velocidade no P1; detecção de analgésico p/ bots no P3; fallback de áudio custom no P5; camada BigBrain como alternativa no P6; AC docs-only; corner headless) |
