# Workflow — Auditoria e otimização de performance de um mod

Processo completo de **engenharia de performance** sobre um mod deste repo: auditoria (investigação baseada em evidências) + otimização (plano → implementação → validação medida). Escrito para ser executado por **qualquer sessão de agente** sem contexto prévio — este documento é o mapa; os detalhes vivem nos commands e skills referenciados (aponta, não duplica).

**Pergunta que o processo responde:** *"existe alguma parte deste mod consumindo CPU, memória ou recursos com frequência, duração ou ciclo de vida maiores do que deveria — e quando esse processamento deveria parar, ele para?"*

## Peças do processo

| Peça | Papel |
|---|---|
| [/audit-mod-code](../../.claude/commands/audit-mod-code.md) `--perf` | Motor da investigação (Fase 1). Gera o relatório com achados `AUD-NN-MM` |
| [/optimize-mod-performance](../../.claude/commands/optimize-mod-performance.md) | Orquestrador das 4 fases e portões |
| Skill `spt-performance-analysis` | Metodologia: modelo de custo (frequência × entidades × duração × acúmulo), taxonomia de superfícies (FREQ/PATCH/ENT/LIFE/GROW/UNITY/ALLOC/LOG/IO/CFG), instrumentação, validação medida |
| Skill `spt-memory-leak-analysis` | Lado de retenção/alocação (mecanismo HOT, medição de RSS) |
| Ciclo de backlog ([WORKFLOW.md](../../WORKFLOW.md)) | Fases 2–3 rodam **dentro dele** — não existe fluxo paralelo de implementação |

## Visão geral

```
Fase 1  INVESTIGAÇÃO   /audit-mod-code <mod> --perf          (read-only)
        → mods/<mod>/docs/relatorio-auditoria-codigo-NN.md
        ⏸ PORTÃO HUMANO: usuário marca a Decisão de cada achado AUD-NN-MM

Fase 2  PLANO          /optimize-mod-performance <mod> --fase 2
        → 1 item de backlog "perf" por rodada, com:
          01-spec  = perfil de NÃO-REGRESSÃO (comportamento atual preservado + metas medíveis)
          02-spec-tech = plano por achado (problema/evidência/mecanismo/solução/risco/validação)
        ⏸ PORTÃO: /review-technical-spec até zerar 🔴 (é a aprovação do plano)

Fase 3  IMPLEMENTAÇÃO  ciclo normal do backlog
        /code-mod → /code-review → /apply-code-review → /compile-mod
        (código cita // ref: AUD-NN-MM; dizer sempre se a build é client, server ou ambos)

Fase 4  VALIDAÇÃO      medição antes/depois em cenário pareado + matriz de lifecycle
        → achados anotados ✅ com números · instrumentação removida/desligada
        → /update-memory <mod> · /update-mod-graph <mod>
```

## O que diferencia este processo de uma feature nova

O contrato funcional **é o comportamento atual**. A `01-spec` do item de backlog declara:

1. **Não-regressão** — lista explícita do que o jogador vê/sente que deve permanecer idêntico (+ critérios padrão Fika/estado entre raids);
2. **Metas medíveis** — por achado, métrica e alvo (ex.: "chamadas/s de X caem de ~N para ≤M", "custo zera após despawn", "coleção estável entre ondas");
3. **Exceção declarada** — mudança perceptível (ajuste de feature/config) só entra como critério de aceite explícito com trade-off descrito, nunca como efeito colateral silencioso.

## Papéis: sessão executora × usuário

| Quem | Decide/faz |
|---|---|
| **Sessão executora** | Roda as fases, produz artefatos, propõe achados/planos, implementa o que foi aprovado, mede |
| **Usuário (portões)** | Marca a **Decisão** de cada achado no relatório (fim da Fase 1) · aprova o plano (review técnica sem 🔴, fim da Fase 2) · valida in-game o que exige jogar |

A sessão executora **nunca** pula um portão: sem Decisões marcadas não há Fase 2; com 🔴 aberto na review técnica não há Fase 3.

## Regras de ouro (resumo — detalhes nos commands/skills)

- **Fase 1 é read-only.** Única exceção: instrumentação temporária para medir uma Suspeita (gated por config, marcada `// PERF-INSTR AUD-NN-MM`, removida na Fase 4).
- **Evidência sempre:** todo achado cita `arquivo.cs:linha` + os eixos de custo (classe de frequência × entidades × duração × acúmulo) + nível de evidência (**Forte / Suspeita / Melhoria preventiva**). Grafo aponta, leitura prova.
- **Maiores ofensores primeiro:** alto ganho × baixo risco. Melhoria preventiva não vira item de backlog sozinha.
- **Ordem de preferência de solução:** corrigir lifecycle → eliminar trabalho desnecessário → configuração → cache/reutilização → reduzir frequência → event-driven → refatoração grande (conceitual, não obrigatória).
- **Sucesso = medição** (AP-06): antes/depois em cenário pareado (mesmo mapa/condições) + matriz de lifecycle (despawn, múltiplas ondas, raid longa, raid1→exit→raid2, alt-F4/morte/MIA, headless quando aplicável). "Parece mais eficiente" não fecha achado.
- **Imutabilidade:** relatórios e reviews nunca são reescritos — achados ganham anotações (`✅ Aplicado` + números, ou `❌ sem ganho` + lição).
- **Não duplicar:** achado de retenção pura referencia `ML-NN-MM` do `/analyze-memory-leak`; achado já resolvido em rodada anterior não volta.

## Como iniciar (prompt para a sessão executora)

```
Rodar o processo de auditoria + otimização de performance no mod <MOD>, seguindo
.agents/workflows/performance-optimization.md (fonte de verdade do processo).

1. Comece pela Fase 1: /audit-mod-code <MOD> --perf  (read-only).
2. Ao terminar, reporte o resumo dos achados e PARE — vou marcar as Decisões no relatório.
3. Depois das minhas Decisões: /optimize-mod-performance <MOD> --fase 2 (item de backlog
   com spec de não-regressão + plano), e siga o ciclo normal até /compile-mod.
4. Feche com a Fase 4 (validação MEDIDA antes/depois) e /update-memory.

Não pule portões; não otimize nada sem Decisão marcada; não remova funcionalidade
sem trade-off declarado na spec.
```

## Artefatos produzidos (onde procurar o estado do processo)

| Artefato | Onde | Fase |
|---|---|---|
| Relatório de auditoria (achados `AUD-NN-MM` + Panorama + Configuração + Instrumentação + Plano de validação) | `mods/<mod>/docs/relatorio-auditoria-codigo-NN.md` | 1 |
| Item de backlog `perf` (spec de não-regressão + plano técnico + reviews + asbuild) | `mods/<mod>/backlog/NNN-perf-<slug>/` | 2–3 |
| Anotações de resolução com números medidos | no próprio relatório de auditoria | 4 |
| Lições e números antes/depois | `mods/<mod>/memory/sessions.md` (via `/update-memory`) | 4 |
