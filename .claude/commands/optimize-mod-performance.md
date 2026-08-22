# /optimize-mod-performance

Pipeline de **engenharia de performance** sobre um mod: investigação → plano técnico → implementação → validação medida, com portões humanos entre as fases. **Auxiliar** — fora do ciclo linear de backlog, mas **não cria um processo paralelo**: a Fase 1 usa o [/audit-mod-code](audit-mod-code.md) `--perf` como motor de investigação, e as Fases 2–3 entram pelo **ciclo normal de backlog** com um perfil de spec de **não-regressão** (os critérios de aceite são o comportamento atual preservado + metas medíveis, não comportamento novo).

> **Skills obrigatórias:** `spt-performance-analysis` (modelo de custo, taxonomia, instrumentação, validação — a espinha dorsal), `spt-memory-leak-analysis` (quando houver achados ALLOC/retenção), `spt-mod-best-practices`, `csharp-mod-best-practices`, `graph-code-navigation`, `repo-workflow-best-practices` (Fases 2–3). Consultar `memory-curation` §14 (contexto de memória).

> **Perguntas-guia** (skill §0): *"o que este mod está fazendo mais vezes, por mais tempo, para mais entidades ou por mais ciclos de vida do que realmente precisa?"* e *"quando esse processamento deveria deixar de existir — e ele realmente deixa?"*

## Uso

```bash
/optimize-mod-performance <mod> [--fase N] [--escopo <subpasta>] [--relatorio NN]
```

- `<mod>` — nome da pasta em `mods/` (ex.: `TRL-DynamicSpawn`). Validar que existe; senão, listar os mods e parar.
- `--fase N` — força a fase (1–4). Sem a flag, **detectar onde o processo está**:
  - sem `relatorio-auditoria-codigo-*.md` com achados de performance pendentes → **Fase 1**;
  - relatório com Decisões marcadas e sem item de backlog correspondente → **Fase 2**;
  - item de backlog `perf-*` em andamento → apontar o **próximo command do ciclo** (spec-tech-review / code-mod / code-review / compile) e parar — a Fase 3 é o ciclo, não este command;
  - build entregue sem o Plano de validação executado → **Fase 4**.
- `--escopo <subpasta>` — repassado ao `/audit-mod-code --scope` na Fase 1.
- `--relatorio NN` — aponta um relatório de auditoria específico (default: o mais recente com achados de performance).

## Pré-condições

`mods/<mod>/` com código-fonte no diretório editável (`modded/`, ou raiz/`src/` em mod próprio). Grafo do mod recomendado (`references/graphs/mods/<mod>/`); ausente, seguir com Grep + leitura e sugerir `/update-mod-graph` depois.

## Fases

### Fase 1 — Investigação (read-only) · PORTÃO: decisões humanas

1. Invocar **`/audit-mod-code <mod> --perf [--scope <escopo>]`** — ele produz `mods/<mod>/docs/relatorio-auditoria-codigo-NN.md` com achados `AUD-NN-MM` classificados por severidade × **nível de evidência** (Forte / Suspeita / Melhoria preventiva), Panorama de execução, Configuração, Instrumentação proposta e Plano de validação.
2. **Suspeitas que bloqueiam a priorização** podem ser medidas antes da Fase 2 numa mini-rodada: implementar **só a instrumentação temporária** proposta (gated por config, marcada `// PERF-INSTR AUD-NN-MM` — skill §6), compilar via `/compile-mod`, medir in-game e **anotar o resultado no achado** (promovendo para Evidência forte ou descartando). Instrumentação não é otimização — não exige item de backlog, mas exige remoção/desligamento registrado na Fase 4.
3. **PARAR.** Reportar o resumo e pedir que o usuário marque a **Decisão** de cada achado no relatório. Nada de código de otimização antes disso.

### Fase 2 — Plano técnico · PORTÃO: `/review-technical-spec` sem 🔴

Agrupar os achados aceitos num **único item de backlog por rodada** (não um por achado — precedente do `/prepare-mod-for-publish`):

1. Criar o item via **`/add-backlog-item <mod> "perf: <resumo da rodada>"`**, interrompendo o `/create-spec` genérico — a spec funcional aqui tem perfil próprio.
2. **`01-spec.md` no perfil de não-regressão.** A diferença para uma feature nova: o contrato funcional **é o comportamento atual**. Critérios de aceite:
   - **Não-regressão:** lista explícita, por achado, do comportamento observável que deve permanecer idêntico (o que o jogador vê/sente não muda) — mais os critérios padrão do repo (Fika/coop, estado entre raids).
   - **Metas medíveis:** por achado, a métrica e o alvo (ex.: "chamadas/s de X caem de ~N para ≤M", "custo zera após despawn", "tamanho da coleção estável entre ondas") — vêm do Plano de validação do relatório.
   - **Exceção declarada:** achado aceito que propõe mudança **perceptível** (ajuste de feature, default de config que muda a experiência) entra como AC de mudança explícito, com o trade-off descrito — nunca como efeito colateral silencioso.
3. **`02-spec-tech.md` = o plano de otimização.** Para cada achado aceito, referenciando o `AUD-NN-MM`: problema · evidência (`arquivo.cs:linha` + eixos de custo) · mecanismo provável de impacto · solução proposta · risco · impacto esperado · como validar · risco de regressão funcional. Ordem de preferência de solução (conceitual, não obrigatória): **corrigir lifecycle → eliminar trabalho desnecessário → configuração → cache/reutilização → reduzir frequência → event-driven → refatoração grande** (a última só quando as anteriores não bastam).
4. Rodar **`/review-technical-spec`** até zerar 🔴 — **este é o checkpoint de aprovação do plano.** Achado que a review derrubar volta anotado no relatório de auditoria (sem apagar o original).

### Fase 3 — Implementação (o ciclo normal, sem atalho)

**`/code-mod`** → **`/code-review`** → **`/apply-code-review`** → **`/compile-mod`**. Nada específico de performance muda o fluxo; o que muda é o que se confere:

- Rastreabilidade tripla: código otimizado cita `// ref: AUD-NN-MM` (além de PA/CR quando aplicável — `repo-workflow-best-practices` §4).
- O `/code-review` valida também contra o checklist da skill `spt-performance-analysis` — em particular que o fix **não trocou custo por bug de lifecycle** e que a não-regressão da 01-spec tem como ser verificada.
- Ao compilar/deployar, **dizer explicitamente se a mudança é client, server ou ambos** (ciclos de reinício diferentes).
- Achado só de configuração (CFG) sem código pode ser aplicado direto nesta fase, registrado no relatório (✅ Aplicado) e em `PROPRIEDADES.md` quando for `ConfigEntry`.

### Fase 4 — Validação medida · fecha o loop

"Parece mais eficiente" não fecha nada (AP-06). Executar o **Plano de validação** do relatório/spec:

1. **Antes/depois em cenário pareado** (skill §7): mesma medição, mesmo mapa/condições — contadores e Stopwatch da instrumentação, census de instâncias/coroutines/handlers, tamanho de coleções, volume de log, RSS quando ALLOC/retenção, FPS/frametime externo como confirmação.
2. **Matriz de lifecycle:** morte/despawn (o trabalho parou?), múltiplas ondas (2ª custa como a 1ª?), raid longa (custo estável?), raid1→exit→raid2 e alt-F4/morte/MIA (nova raid não herda custo), headless quando o mod roda nele.
3. **Não-regressão:** conferir in-game cada AC da 01-spec.
4. **Encerrar:** anotar cada achado no relatório (✅ Aplicado + números medidos, ou ❌ sem ganho — reverter e registrar a lição); **remover ou desligar por default** a instrumentação temporária (grep `PERF-INSTR` deve voltar limpo ou só com blocos gated documentados em `PROPRIEDADES.md`); sugerir `/update-memory <mod>` (lições, números de antes/depois, hipóteses descartadas) e `/update-mod-graph <mod>`.

## Reporte por fase (formato)

```text
✓ /optimize-mod-performance <mod> — Fase N concluída
  Fase 1: relatório NN · achados: 🔴 N · 🟠 N · 🟡 N · 💡 N (Forte: N · Suspeita: N · Preventiva: N)
          → marque as Decisões no relatório e rode /optimize-mod-performance <mod> --fase 2
  Fase 2: item NNN-perf-<slug> criado · plano com N achados · review técnica: [pendente/zerada]
          → próximo: /review-technical-spec (até zerar 🔴), depois /code-mod
  Fase 3: [delegado ao ciclo — reportar o command executado e client/server/ambos]
  Fase 4: validados N/N achados · [métricas antes→depois] · instrumentação: removida/desligada
          → /update-memory e /update-mod-graph sugeridos
```

## Regras

- **Fase 1 é read-only** (exceto a mini-rodada de instrumentação, que é explícita, gated e marcada). Otimização sem Decisão humana não entra.
- **Um item de backlog por rodada de otimização** — os achados aceitos andam juntos; frentes independentes demais para um item só = rodadas separadas.
- **Não-regressão é o contrato:** a 01-spec declara o comportamento atual como critério de aceite; mudança perceptível só como AC explícito com trade-off. Não "otimizar" removendo funcionalidade em silêncio.
- **Priorizar os maiores ofensores:** alto ganho × baixo risco primeiro; Melhoria preventiva não vira item de backlog sozinha (agrega numa rodada futura ou fica como dívida anotada).
- **Não duplicar achados** de `MEMORY-LEAK-review-*` (referenciar `ML-NN-MM`) nem reabrir ✅ Aplicado de auditorias anteriores.
- **Relatórios e reviews são imutáveis** — achados ganham anotações de resolução, nunca reescrita.
- **Sucesso = medição** (skill §7; AP-06): sem números antes/depois, o achado não fecha — no máximo "aplicado, aguardando validação".
- Versão alvo: SPT 4.0+ / EFT 0.16.x / Fika (headless incluso).
