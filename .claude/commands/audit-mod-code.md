# /audit-mod-code

Executa uma **auditoria técnica estática profunda, rigorosa e minuciosa** de todo o código-fonte de um mod (classes, métodos, estruturas de dados, algoritmos e patches Harmony). Valida a integridade e conformidade cruzando obrigatoriamente as referências em `references/` (`eft-decompiled` / `Assembly-CSharp`, `spt-source` e `fika-*`), caça vazamentos de memória (RAM leaks / GC pressure), avalia a necessidade de rotinas rodando em `Update()`, identifica funções órfãs/código morto e emite um relatório técnico acionável com classificação de severidade, alternativas de melhor lógica e código de correção.

> **Skills obrigatórias:** carregar `spt-mod-best-practices`, `csharp-mod-best-practices` e `graph-code-navigation` antes de auditar. Na Dimensão 3, carregar `spt-memory-leak-analysis`. **`spt-performance-analysis` é obrigatória no modo `--perf`** e recomendada sempre que a Dimensão 2 encontrar superfícies frequentes. Consultar `memory-curation` §14 (contexto de memória).

> **Só audita — não corrige.** O command produz achados com bloco de Decisão; a correção entra pelo ciclo normal (ver "Como um achado vira correção"). O relatório serve de Fase 1 (investigação) para o [/optimize-mod-performance](optimize-mod-performance.md).

---

## Uso

```bash
/audit-mod-code <mod-ou-caminho> [--scope <subpasta>] [--target-ref <all|eft|spt|fika>] [--strict] [--perf]
```

- `<mod-ou-caminho>` — Nome da pasta do mod em `mods/` (ex.: `ORBIT`, `VisceralCombat`, `TRL-ActionPOV`).
- `--scope <subpasta>` (Opcional) — Limita a auditoria a um submódulo específico (ex.: `--scope modded/Orbit/Looting`). Padrão: todo o código do mod — `modded/` quando existe, a raiz/`src/` em mod próprio (`.agents/conventions.md` § Estrutura Padrão de Mods), ou `original/` (read-only) quando o mod ainda não foi forkado para `modded/`.
- `--target-ref <all|eft|spt|fika>` (Opcional) — Foco das checagens cruzadas de referência (padrão: `all`).
- `--strict` (Opcional) — Ativa verificação com nível máximo de rigor contra tolerâncias de GC, polling em loops e suposições de singleplayer.
- `--perf` (Opcional) — **Modo performance** (ver seção "Modo `--perf`"): concentra a auditoria em custo de execução, aplicando a metodologia da skill `spt-performance-analysis` (modelo de custo, panorama de execução, configuração, instrumentação).

---

## Pré-condições

1. A pasta `mods/<NomeDoMod>/` deve existir com código-fonte (`.cs` client ou `.ts`/`.js` server).
2. As referências vendorizadas em `references/` devem estar acessíveis:
   - `references/eft-decompiled/` (e `types-index.json` para conferência de tipos/métodos do EFT).
   - `references/spt-source/` (servidor SPT 4.0).
   - `references/fika-plugin/` (`Fika.Core`), `references/fika-server/`, `references/fika-headless/` (suporte multiplayer coop).
3. Consultar [docs/technical/spt-antipatterns.md](../../docs/technical/spt-antipatterns.md) (AP-01 a AP-09).

---

## Contexto (antes das dimensões)

1. **Resolver o mod e a raiz de código** (`modded/`, raiz/`src/` em mod próprio, ou `original/` read-only). Se `mods/<mod>/` não existir, listar os mods disponíveis e parar.
2. **Memória do mod** (`memory-curation` §14): ler o topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências) + entradas sobre performance/leak/crash. Pendência 🔴 → alertar. Emitir a linha `Memória consultada: ...` no encerramento; sem arquivo, registrar "sem memória prévia".
3. **Relatórios anteriores:** `relatorio-auditoria-codigo-*.md` e `MEMORY-LEAK-review-*.md`. Achado `✅ Aplicado`/resolvido **não volta**; achado pendente conhecido é reforçado citando o ID original (`AUD-NN-MM` / `ML-NN-MM`), sem duplicar.
4. **Mod grande** (acima de ~2.000 linhas no escopo): delegar a varredura de cada dimensão a sub-agents **read-only** em paralelo e consolidar os achados aqui (mesmo padrão do `/prepare-mod-for-publish` fase 2).

---

## Modo `--perf` (auditoria de performance)

Responde à pergunta: **"o que este mod está fazendo mais vezes, por mais tempo, para mais entidades ou por mais ciclos de vida do que realmente precisa — e quando esse processamento deveria parar, ele para?"** Metodologia completa na skill `spt-performance-analysis` (obrigatória neste modo); aqui só o que muda em relação à auditoria padrão:

1. **Escopo das dimensões:** aprofundar as Dimensões **2** (Update vs. reativa), **3** (GC pressure — só a parte de churn/alocação; retenção pura é território do `/analyze-memory-leak`) e **6** (threading), e **acrescentar** as superfícies da skill §2: FREQ · PATCH · ENT · LIFE · GROW · UNITY · ALLOC · LOG · IO · CFG. Dimensões 1/4/5 rodam em modo leve (só o que sustenta um achado de custo).
2. **Panorama de execução obrigatório** (skill §5.2): antes dos achados, tabela com toda superfície periódica/frequente do mod — superfície → classe de frequência → multiplicador de entidades → gate de contexto → quem para/quando. Patch Harmony entra com a **frequência estimada do alvo** (skill §3 — grafo aponta callers, leitura prova).
3. **Auditoria de configuração** (skill §2 CFG): mapear as chaves de `Config.Bind`/`*.json`/`*.cfg` que alimentam frequência, raio, quantidade, logging e limpeza — default atual × default proposto. É a alavanca de ganho sem mudar arquitetura.
4. **Cada achado carrega os eixos do modelo de custo** (skill §1): classe de frequência × entidades × duração × acúmulo, com o custo unitário que ajusta a severidade — e um **nível de evidência**:
   - **Evidência forte** — frequência, multiplicador e mecanismo comprovados por leitura/medição.
   - **Suspeita** — padrão preocupante com eixo não provado; **entra com proposta de instrumentação** (skill §6), não com refactor.
   - **Melhoria preventiva** — boa prática que não explica problema atual; entra **agregada e curta**. O objetivo do modo é achar os maiores ofensores primeiro, não produzir 80 sugestões cosméticas.
5. **Seções extras no relatório:** `## Panorama de execução`, `## Configuração`, `## Instrumentação proposta` (para as Suspeitas) e `## Plano de validação` (métrica, cenário pareado e critério por achado — skill §7).
6. **Não duplicar memória:** achado de retenção descoberto no caminho é registrado por referência ao mecanismo da `spt-memory-leak-analysis` (ou ao `ML-NN-MM` existente), mantendo aqui só o ângulo de custo de execução.

---

## Como um achado vira correção

O relatório **não é consumível pelo `/apply-code-review`** (esse command exige artefatos de item de backlog). Caminhos válidos:

| Tipo | Como corrigir |
|---|---|
| Código (qualquer dimensão) | Agrupar os achados aceitos num **item de backlog** (`/add-backlog-item`) e seguir o ciclo normal — um item por rodada de auditoria, não um por achado. No modo `--perf`, o [/optimize-mod-performance](optimize-mod-performance.md) automatiza essa ponte (Fase 2), com spec funcional de **não-regressão** |
| Só configuração (CFG) | Ajuste direto + registro da mudança no próprio relatório (✅ Aplicado) e em `PROPRIEDADES.md` quando for `ConfigEntry` |
| Opções do F12 (UX) | Pelo `/review-mod-properties`, dono desse escopo |

Rastreabilidade: correção aplicada em código cita o achado no comentário inline — `// ref: AUD-NN-MM` (mesmo esquema de PA/CR em `repo-workflow-best-practices` §4).

---

## As 6 Dimensões Críticas de Auditoria

```mermaid
graph TD
    subgraph Dimensoes_de_Auditoria [As 6 Dimensões de Auditoria Técnica]
        D1["1. Validação Cruzada em references/<br>Assinaturas EFT 0.16.9, rotas SPT 4.0, sincronização FIKA"]
        D2["2. Auditoria de Update() vs Lógica Otimizada<br>Polling frame-a-frame vs Eventos nativos / Patches / Throttling"]
        D3["3. Vazamentos de Memória & GC Pressure<br>Retenção raid-a-raid, alocações em hot paths, eventos sem -= "]
        D4["4. Funções Órfãs & Código Morto<br>Callers = 0, patches inativos, catches vazios"]
        D5["5. Antipadrões do SPT (AP-01..AP-09)<br>Singletons inseguros, polling, reflection sem cache"]
        D6["6. Threading, Concorrência & Unity<br>Acesso à main thread, Tasks sem CancellationToken"]
    end
```

---

### Dimensão 1: Validação Cruzada contra `references/`

Para cada classe, método chamado e patch Harmony do mod, cruzar obrigatoriamente com as referências canônicas:

1. **EFT Decompiled (`references/eft-decompiled/` / `Assembly-CSharp`):**
   - **Assinaturas de Patches:** Verificar se a classe-alvo (`[HarmonyPatch(typeof(X), nameof(X.Method))]`) e seus tipos de argumentos existem no EFT `0.16.9` / SPT `4.0.13`.
   - **Tipos Obfuscados:** Consultar `references/eft-decompiled/types-index.json` para confirmar equivalências (`GClass...` vs nomes de domínio).
   - **Comportamento Original:** Inspecionar o corpo do método descompilado para garantir que um `Prefix` que retorna `false` não quebre lógicas internas vitais do EFT (ex.: registro de eventos de áudio, cálculos de balística, chamadas de animação).
2. **Servidor SPT (`references/spt-source/`):**
   - Verificar se rotas HTTP/WebSocket, handlers de pacotes e chamadas ao banco de dados (`tables.templates.items`, `locales`, etc.) seguem a arquitetura do SPT 4.0.
3. **FIKA Coop (`references/fika-plugin/` - `Fika.Core`, `fika-server/`):**
   - **Detecção de Suposições "Singleplayer-Only":**
     - O código usa `Camera.main` sem checar se a câmera ativa pertence ao jogador local ou a um espectador?
     - O código assume `Singleton<GameWorld>.Instance.MainPlayer` sem iterar sobre outros jogadores remotos no coop?
     - Ações que alteram estado de inventário ou mundo emitem pacotes de sincronização (`Fika.Core.Networking`) ou quebram a sincronia com outros jogadores?

---

### Dimensão 2: Auditoria de Necessidade de `Update()` vs Arquitetura Reativa Otimizada

> **Regra de Ouro:** A auditoria deve avaliar criteriosamente se uma lógica executada a cada frame em `Update()`, `LateUpdate()` ou `FixedUpdate()` é estritamente necessária nessa frequência ou se pode ser substituída por uma abordagem mais inteligente e otimizada **sem perder a funcionalidade proposta**.

```mermaid
flowchart TD
    DetectUpdate[Detectada lógica rodando em Update / LateUpdate] --> CheckNature{Qual a natureza da lógica?}
    
    CheckNature -- Checagem de Estado / Mudança de Condição --> AltEvents[1. Eventos Nativos do EFT ou Patches de Transição]
    CheckNature -- Varredura / Cálculos Pesados Periódicos --> AltThrottle[2. Throttling / TimePacing / Coroutines]
    CheckNature -- Consulta a Dados / Raycasts Condicionais --> AltDirty[3. Dirty Flags / Caching / Lazy Evaluation]
    CheckNature -- Efeito Cosmético / Físico Contínuo --> AltGating[4. Gating de Distância / LOD / Contexto de Raid]
```

#### Matriz de Soluções Otimizadas:

| Abordagem Atual em `Update()` | Diagnóstico da Auditoria | Solução Alternativa Recomendada | Ganho Técnico |
|---|---|---|---|
| **Polling de Mudança de Estado**<br>(Ex.: Checar a cada frame se o player trocou de arma, se mirou, se a vida mudou, se abriu porta) | Ineficiente. Gasta 60 a 144 execuções/segundo verificando uma variável que raramente muda. | **Arquitetura Orientada a Eventos / Harmony Postfix:**<br>Substituir por inscrição em eventos nativos (`Player.OnHandsControllerChanged`, `HealthController.ApplyDamage`, etc.) ou aplicar um patch cirúrgico no método que altera o estado. | Redução de 99% de overhead de CPU. Zero chamadas no frame. |
| **Cálculo Periódico sem Urgência de Frame**<br>(Ex.: Checar bots próximos, escanear corpos, recalcular rotas, verificar inventário) | Desperdício de ciclos de renderização com lógica que não afeta a interpolação visual do frame. | **Cadência Controlada (Throttling / TimePacing):**<br>Executar em intervalos controlados (ex.: a cada `0.2s`, `0.5s` via `TimePacing` ou Coroutine `WaitForSeconds`), mantendo a exata percepção para o jogador. | Queda de 90% a 95% no consumo de CPU da rotina. |
| **Consultas e Raycasts Repetitivos**<br>(Ex.: Raycasts de visibilidade, varredura de contêineres, transformações geométricas) | Execução redundante de operações caras de física e busca. | **Dirty Flags & Caching:**<br>Armazenar o último resultado em cache e só recalcular quando um gatilho relevante for ativado (`_isDirty = true`). | Elimina raycasts e chamadas desnecessárias no NavMesh/Física. |
| **Lógica Ativa Fora de Contexto**<br>(Ex.: Rodando nos menus, em tela de carregamento ou para bots distantes a 400m) | Falta de controle de ciclo de vida e LOD. | **Gating Condicional & LOD:**<br>Desativar o script quando fora de raid (`GameWorld == null`), quando o player estiver morto ou usar distância de corte (*Degraded Tickrate*). | Previne NREs em transições e economiza CPU com entidades distantes. |

---

### Dimensão 3: Vazamentos de Memória (RAM Leaks) e Pressão de GC

Varre sistematicamente o código procurando padrões de retenção não liberada e alocações de Heap:

1. **Retenção de Ciclo de Vida de Raid (Raid Lifecycle Leaks):**
   - **Eventos:** Toda inscrição com `+=` (`SettingChanged`, `OnGameStarted`, `OnDoorStateChanged`, eventos de inventário) possui o correspondente `-=` no teardown (`OnDestroy` / `OnGameEnded`)?
   - **Campos Estáticos:** Coleções estáticas (`static List<...>`, `static Dictionary<...>`) acumulam instâncias de `BotOwner`, `Player`, `GameObject`, `Item` ou `Transform` sem serem limpas com `.Clear()` ao final da partida?
   - **Coroutines & Tasks:** Há coroutines iniciadas sem `StopCoroutine()` ou `Task.Run`/Tasks assíncronas sem `CancellationToken` amarrado ao ciclo de vida da raid?
2. **Alocações em Loops Quentes (Hot Paths):**
   - Inspecione métodos chamados com alta frequência (`Update()`, ticks de IA, callbacks de física):
     - Há `new List<...>()`, `new Dictionary<...>()` ou `new Vector3[]` sendo instanciados no corpo do método em vez de reutilizar buffers/coleções estáticas recicladas?
     - Há uso de **LINQ** (`.Where()`, `.Select()`, `.ToList()`, `.FirstOrDefault()`) em hot paths? (LINQ aloca iteradores e closures no Heap, gerando pressão massiva no Garbage Collector).
     - Há interpolação contínua de strings ou `string.Format` em métodos executados a cada frame?
     - Há *boxing* desnecessário de structs (`Vector3`, enums, `int`) passados como `object`?
3. **Recursos Nativos e Unity:**
   - Instanciação dinâmica de materiais (`renderer.material` cria uma cópia nova no Heap a cada chamada — usar `renderer.sharedMaterial` ou `MaterialPropertyBlock`).
   - Texturas, Meshes dinâmicas e `AssetBundle` sem o devido `Destroy()` ou `Unload(true)`.

---

### Dimensão 4: Funções Órfãs, Código Morto e Lógicas Incompletas

1. **Métodos e Campos Órfãos:** Métodos privados/internos sem nenhum chamador no grafo do projeto.
2. **Patches Inativos/Mortos:** Patches Harmony cujo método-alvo não é mais invocado pelo fluxo do EFT 0.16.9.
3. **Tratamento de Exceção Silencioso:** Blocos `try { ... } catch { }` vazios que engolem exceções sem log, mascarando falhas críticas de estado ou `NullReferenceException`.
4. **Variáveis e Branches Mortos:** Cálculos caros cujos resultados são descartados sem uso, ou condições `if (false)` / flags estáticas inalcançáveis.

---

### Dimensão 5: Conformidade com Antipadrões do SPT

Verificação sistemática contra a base [docs/technical/spt-antipatterns.md](../../docs/technical/spt-antipatterns.md):

- **AP-01:** Falta de teardown entre raids.
- **AP-02:** Acesso não defensivo a Singletons (`Singleton<GameWorld>.Instance` sem validação de nulo).
- **AP-03:** Polling manual em `Update()` para checar estados que possuem eventos nativos do EFT.
- **AP-04:** Uso de Reflection em hot paths sem cache prévio de `MethodInfo` / `FieldInfo` / `PropertyInfo`.
- **AP-05:** Caminhos absolutos hardcoded.
- **AP-07:** Incompatibilidade entre padrões arquiteturais do SPT 3.x e SPT 4.0.

---

### Dimensão 6: Threading e Segurança de Execução

- **Main Thread Safety:** Garantir que nenhuma thread secundária (`Task.Run`, `Thread`, `BackgroundWorker`) tente instanciar `UnityEngine.Object`, chamar `GameObject.GetComponent` ou acessar APIs do Unity sem despachar para a main thread.

---

## Estrutura do Relatório de Auditoria

O relatório gerado deve ser salvo em:
`mods/<NomeDoMod>/docs/relatorio-auditoria-codigo-NN.md` (onde `NN` é incremental, ex.: `01`).

### Formato Obrigatório do Relatório:

```markdown
---
title: "Relatório de Auditoria Técnica de Código — NomeDoMod (Review NN)"
date: YYYY-MM-DD
status: 🟢 Vivo
authors: {Autor}
---

# Relatório de Auditoria Técnica de Código — NomeDoMod (Review NN)

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | N | Crashes, corrupção de save, memory leak massivo descontrolado |
| 🟠 **Alto** | N | Leaks entre raids, patches quebrados contra EFT 0.16.9, quebra de coop FIKA |
| 🟡 **Médio** | N | Polling desnecessário em Update, pressão de GC, reflection sem cache |
| 🔵 **Baixo** | N | Melhorias de tipagem, manutenibilidade, comentários desatualizados |
| 💡 **Otimização** | N | Propostas de arquitetura reativa, pooling e eliminação de polling |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-NN-01` | 🔴 Crítico | `Caminho/Arquivo.cs:L45` | Memory Leak | Evento sem unsubscribe retém GameWorld entre raids |
| `AUD-NN-02` | 🟡 Médio | `Caminho/Laser.cs:L80` | Polling em Update | Checagem de mira rodando a 144 FPS em vez de evento |
| `AUD-NN-03` | 🟡 Médio | `Caminho/Outro.cs:L120` | GC Pressure | `new List` e LINQ dentro de `Update()` a cada frame |

---

## 3. Detalhamento dos Achados

### AUD-NN-01 · [Título Resumido do Problema]
- **Severidade:** 🔴 Crítico / 🟠 Alto / 🟡 Médio / 🔵 Baixo / 💡 Otimização
- **Evidência:** Forte / Suspeita / Melhoria preventiva *(obrigatório no modo `--perf`; nas Suspeitas, apontar a instrumentação proposta que fecha o eixo em aberto)*
- **Execução** *(modo `--perf`)*: [classe de frequência × multiplicador de entidades × duração × acúmulo — os eixos da skill `spt-performance-analysis` §1. Ex.: "per-frame × N bots (~30) × raid inteira, custo cresce com a lista de cadáveres"]
- **Localização no Mod:** [Arquivo.cs:L45](../modded/Caminho/Arquivo.cs#L45)
- **Referência Cruzada:** [Assembly-CSharp/ClasseAlvo.cs:L120](../../../references/eft-decompiled/Assembly-CSharp/ClasseAlvo.cs#L120)
- **Causa Raiz:** [Explicação técnica detalhada do porquê o código atual falha ou gera problema.]
- **Impacto Técnico Real:** [Ex.: Vazamento de 80MB por raid no Fika Headless / Queda de 12 FPS por micro-travamentos de GC.]
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* [Ex.: Polling contínuo no Update a 144 FPS]
  - *Abordagem Otimizada:* [Ex.: Inscrição no evento `Player.OnAimChanged` ou Throttling de 0.2s]
  - *Código Refatorado:*

```csharp
// Código refatorado demonstrando a solução viável e otimizada
```

- **Como validar** *(modo `--perf`)*: [métrica + cenário pareado + critério — skill §7. Ex.: "contador de chamadas/s antes/depois no mesmo mapa; critério: cai de ~140/s para ≤5/s e zera após despawn"]
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

<!-- Após corrigir (via ciclo de backlog ou ajuste de config): marcar a opção, trocar o título para ✅ Aplicado em YYYY-MM-DD e adicionar **Resolução:** + **Aplicação:** (arquivo.cs:linha ou chave de config) -->

---

> **Modo `--perf`:** o relatório ganha adicionalmente as seções `## Panorama de execução` (tabela superfície → frequência → entidades → gate → quem para/quando + frequência estimada de cada alvo Harmony), `## Configuração` (chave → default atual → default proposto → onde entra no código), `## Instrumentação proposta` (para as Suspeitas — padrões da skill §6, gated por config e marcada `// PERF-INSTR AUD-NN-MM`) e `## Plano de validação` (checklist por achado). Achados 💡 de nível "Melhoria preventiva" entram agrupados numa única subseção compacta.

---

## 4. Plano de Ação e Recomendações

1. Priorizar a resolução imediata dos itens 🔴 e 🟠.
2. Migrar rotinas de polling identificadas em 🟡 para arquitetura de eventos nativos ou cadência controlada.
3. Aplicar refatorações de zero-alloc e GC pooling.
```

---

## Validação Automática

Após gerar o relatório, valide os cabeçalhos executando:
```bash
bash .agents/hooks/validate-doc-header.sh mods/<NomeDoMod>/docs/relatorio-auditoria-codigo-NN.md
```
