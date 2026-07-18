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
