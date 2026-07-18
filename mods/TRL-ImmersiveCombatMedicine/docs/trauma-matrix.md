# Matriz de Trauma 2.0 — design canônico

> **Data:** 2026-07-18<br>
> **Status:** ✅ Aprovado<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [coop-heal-matrix.md](./coop-heal-matrix.md)<br>

---

Fonte de verdade do redesign do sistema de trauma (pernas/braços/estômago/desmaio) aprovado em 2026-07-18. Alimenta as specs dos itens 001–010 do [backlog](../backlog/mod-backlog.md).

## Matriz de efeitos

| Região | Condição | Sem analgésico | Com analgésico |
|---|---|---|---|
| Perna | Zerar 1 | Mancar N1 (p=100%) | Nada |
| Perna | Zerar 2 | Agachar involuntário (p=100%) + Mancar N2 (p=100%) | Mancar N1 (p=100%) |
| Perna | Quebrar 1 | Mancar N1 (p=100%) | Nada |
| Perna | Quebrar 2 | Cair (p=100%) + ciclo levantar 3s/15s | Mancar N1 (p=100%) |
| Perna | Zerar 1 + Quebrar 1 | Mancar N2 (p=100%) | Mancar N1 (p=100%) |
| Perna | Zerar 2 + Quebrar 2 | Cair (p=100%) + ciclo levantar 3s/15s | Mancar N2 (p=100%) |
| Estômago | Zerar | Agachar involuntário (p=75%) | Agachar involuntário (p=25%) |
| Tórax | Tiro que remove ≥50% da vida ATUAL | Desmaia (p=50%) | **Nada (imune)** |
| Cabeça | Tiro que remove ≥25% da vida ATUAL | Desmaia (p=50%) | Desmaia (p=25%) |
| Braço | Zerar 1 | Tremor | Nada |
| Braço | Zerar 2 | Cancela ADS após 4s + Tremor | Tremor |
| Braço | Quebrar 1 | Tremor | Nada |
| Braço | Quebrar 2 | Cancela ADS após 3s + Tremor | Tremor |
| Braço | Zerar 1 + Quebrar 1 | Tremor | Nada |
| Braço | Zerar 2 + Quebrar 2 | Cancela ADS após 2s + Tremor | Tremor |

## Decisões de design (sessão 2026-07-18)

1. **Estados vs eventos:** Zerar/Quebrar são estados **contínuos** — o efeito dura enquanto a condição durar e **reverte ao curar/operar**. O desmaio é **evento** (rolado no momento do tiro).
2. **Combos mistos** (ex.: Zerar 2 + Quebrar 1): aplica a **coluna mais severa** que casar.
3. **Braço — fratura pior que zerado é intencional** (Quebrar 2 → ADS 3s < Zerar 2 → ADS 4s): fratura dói mais ao sustentar mira.
4. **Mancar N1/N2:** usar os tipos de mancar **vanilla** do jogo — item de pesquisa: mapear quais existem (incl. animação do lado correto da perna ferida) e mapear N1/N2 aos nativos.
5. **Agachar involuntário:** ação one-shot — o player agacha sem querer, mas pode **ficar de pé em seguida** (não trava pose).
6. **Cair (pernas):** prone forçado; pode levantar **e andar por 3s**, depois cai automaticamente; nova tentativa só após **15s**. Tentar levantar durante o bloqueio → **som de dor** (avaliar simular a tentativa frustrada de levantar); ao liberar, levanta **lentamente** com som de dor mais leve (distinto do primeiro).
7. **Estômago:** a probabilidade re-rola **a cada vez** que o estômago chega a 0 (curou → zerou de novo → rola de novo).
8. **Desmaio percentual:** compara o dano do tiro com a vida ATUAL da parte **antes do tiro**.
9. **Tórax + analgésico = imunidade total a desmaio** (confirmado). Cabeça mantém p=25%.
10. **Duração aleatória do desmaio (min–max)** entra neste escopo (ponto único `RANGE-READY` já marcado no código).
11. **Bots** seguem as mesmas regras (onde a mecânica se aplicar).
12. **Analgésico** = qualquer efeito Painkiller **nativo** ativo (analgin, morfina, stims com painkiller, etc.).
13. **Configurabilidade:** todas as probabilidades e timers expostos no F12.

## Decisões da validação de backlog (sessão 2026-07-18, rodada 2)

14. **Expiração do analgésico = reavaliação IMEDIATA e completa** — a dor volta na hora: inclusive one-shots (cair/agachar) disparam no instante da expiração se a condição os exigir. Tomar analgésico de-escala na hora (counterplay).
15. **Piso de dano absoluto no desmaio percentual**: tórax ≥ **25** de dano E ≥50% da vida atual; cabeça ≥ **10** de dano E ≥25% da vida atual (pisos configuráveis). Sem agregação de pellets — comparação por hit.
16. **Bots no ciclo de queda — interferência CIRÚRGICA**: hoje a regra existente deixa a IA levantar após X s e ela **nunca mais cai** (limitação conhecida). Novo desenho: interferir no mínimo, **devolver o controle ao SAIN após cada interferência**; ao levantar, se a condição persistir (2 pernas quebradas), a reavaliação derruba de novo — o bot entra no mesmo ciclo cair→esperar X s→levantar. **X configurável** (separado do timer humano 15s/3s).
17. **Re-ADS pós-cancelamento: lockout curto** de 1–1,5 s (configurável) com som de dor — evita "solta e re-mira" anular a penalidade.

## Decisões da validação FMEA (sessão 2026-07-18, rodada 3)

18. **Mancar N1/N2 = TOTAL experienciado**: a matriz define a experiência final — calibrar o nosso efeito considerando a penalidade vanilla por baixo (adicionar só o delta até N1/N2).
19. **Anti-thrash**: o mesmo one-shot involuntário (cair/agachar) não re-dispara em menos de 3–5 s (configurável) — corta o loop do spam de analgésico sem mudar as regras.
20. **Feedback**: diegético (sons de dor) + **toast discreto na primeira ocorrência** de cada estado.
21. **Injeção legacy APOSENTADA**: a regra atual de 30% fratura / 15 dano ao levantar com 2 pernas zeradas (MovementPatches.cs:154-186, herdada do TrueTrauma 3.11) sai — a matriz é **puramente reativa** (fratura só vem de combate/vanilla; Zerar 2 nunca escala sozinho ao Cair).
22. **i18n dos textos**: todo texto exibido ao jogador (toasts, notificações de estado) em **inglês padrão**, com **tradução PT** aplicada quando o idioma do jogo for português. Textos existentes do mod (hoje em PT fixo) migram no item 010.

## Defaults de spec (validação de backlog — aplicar como critérios de aceite)

- **D1. Ranking de severidade** para combos mistos: `Cair+ciclo > Agachar+N2 > Mancar N2 > Mancar N1 > Nada`.
- **D2. Regiões coexistem**; conflito de POSE resolve pelo mais severo (prone > agachar).
- **D3. Desmaio tem precedência** — pausa o ciclo de queda; retoma no wake.
- **D4. Zerar+Quebrar no mesmo membro** conta como Z1+Q1 (contagem por condição).
- **D5. Dano do desmaio = efetivo aplicado** (pós-armadura) vs vida atual pré-tiro.
- **D6. Tipos de dano do desmaio** mantidos (Bullet/Explosion/Sniper/Landmine/GrenadeFragment).
- **D7. Guards de contexto**: agachar/cair não disparam em escada/corda/BTR/vault (adiam).
- **D8. Roll do estômago** usa o analgésico do instante da zerada (sem re-roll por mudança posterior).
- **D9. Bots**: tremor aplicado (cosmético); cancela-ADS não se aplica a bots.
- **D11. Tremor com dono próprio**: o mod gerencia o lifecycle do efeito Tremor (aplicar/renovar/remover) — sem depender do tremor-por-dor vanilla (que o analgésico apaga).
- **D12. Pipeline de velocidade**: composição multiplicativa; teste de compat com CustomClasses (patches reais: MaxSpeedPatch/SprintingSpeedPatch — postfix nos getters de MovementContext; corrigido na rodada A do item 001) e Skills Extended (MovementContextSetSpeedLimitPatch).
- **D13. Cancela-ADS pelo caminho vanilla** + teste de compat com SPTRecoilRework e Fontaine-FOVFix (ambos patcheiam SetPlayerAiming).
- **D14. Spike 001 mapeia SAIN 4.4.3 + ORBIT**: pontos seguros de interferência (mover/pose/camadas BigBrain) com prova decompilada; pausar/retomar sempre deixa a camada RE-DECIDIR (nunca matar camada).
- **D15. UNTAR** segue as mesmas regras de trauma (são bots).
- **D16. Autoridade**: o motor avalia SÓ no dono (humano local; bots no host/headless) — mesmo padrão do desmaio (lição CR-01-28); espelhos nunca aplicam efeito.
- **D17. Revert por cura REMOTA**: estado reverte também quando o efeito é removido via rede (cura do médico coop), não só por cura própria.
- **D18. Cair forçado sem dano de queda**; extração funciona prone; transit/fim de raid reseta o ciclo; a janela de 3 s andando com perna zerada TOMA o dano vanilla de andar (aceito como realismo).
- **D19. Observabilidade/perf**: todo roll logado (dano, vida, p, resultado) p/ balanceamento; motor por evento + polling <=4 Hz.
- **D20. Suíte de compat do 009**: SAIN, ORBIT, UNTAR, CustomClasses (Tank), SPTRecoilRework, Fontaine-FOVFix, BringBackConcussion, Visceral Combat.
- **D10. Substituição incremental**: cada item desliga o sistema antigo na SUA entrega; migração de configs do usuário no 010. Spike 001 entrega fallback (caps de velocidade) se não houver mancar vanilla por lado/nível.

## Substituições vs sistema atual

| Sistema atual (v1.1.1) | Destino |
|---|---|
| Sistema de Pernas (cair ao perder pernas + bot 90s) | Substituído pelos itens 003/004 |
| Sistema de Braços (fadiga de mira 1s) | Substituído pelo item 005 |
| Sistema de Estomago (sem ar em tiro ≥35) | Substituído pelo item 006 |
| Desmaio por dano fixo (tórax ≥35 / cabeça ≥10) | Substituído pelo item 007 |
| Duração fixa do desmaio | Substituída pelo item 008 (min–max) |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-18 | Guilherme | Criação — matriz aprovada + 13 decisões da sessão de requisitos. |
| 2026-07-18 | Guilherme | Validação de backlog: decisões 14–17 (analgésico reavalia na hora; pisos 25/10 no desmaio; bots cirúrgico+SAIN com X configurável; lockout de re-ADS) + defaults D1–D10. |
| 2026-07-18 | Guilherme | Rodada A do item 001 corrigiu a redação do D12 (patches reais do CustomClasses verificados no fonte). |
| 2026-07-18 | Guilherme | Validação FMEA (rodada 3): decisões 18–22 (N1/N2=total; anti-thrash; feedback diegético+toast; injeção legacy aposentada; i18n EN default + PT) + defaults D11–D20. |
