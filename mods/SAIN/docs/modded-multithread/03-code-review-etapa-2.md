---
title: SAIN — Code Review Etapa 2 (Buffers Nativos Persistentes / Zero-Alloc)
date: 2026-09-02
status: 🟢 Vivo
authors: [guilhermebvdm, Antigravity]
---

# SAIN — Code Review Etapa 2 · Buffers Nativos Persistentes (Zero-Alloc)

**Mod:** `SAIN (modded-multithread)`  
**Versão:** `4.6.0`  
**Referência:** Etapa 2 do [Plano de Implementação](../../../.gemini/antigravity-ide/brain/c2374fdc-b4d7-49ce-97f9-f53e9470d810/implementation_plan.md)  
**Documentação Base:** [01-arquitetura-multithread-e-lod.md](01-arquitetura-multithread-e-lod.md)  
**Data:** 2026-09-02  

> Análise crítica formal do código implementado na **Etapa 2: Buffers Nativos Persistentes (Zero-Alloc nos Jobs de Visão, Distâncias e Lugares)**.  
> Cada achado recebe um ID permanente `CR-02-MM` categorizado em 6 dimensões × 4 níveis de impacto.

---

## 📊 Resumo Executivo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 (1 resolvido) · 🟢 Menores: 2 (1 resolvido, 1 aceito como projeto) · ✅ Resolvidos: 2 · Total: 3

| ID | Categoria | Impacto | Título | Status |
|:---:|:---:|:---:|---|:---:|
| **CR-02-01** | D — Arquitetura | 🟡 Médio | `CalcEnemyPlaceJob.Dispose()` tenta descartar fatias nativas (`GetSubArray`) | `[x]` ✅ Aplicado em 2026-09-02 |
| **CR-02-02** | B — Bug latente | 🟢 Menor | `OtherPlayerDirectionData` aloca com `count = 0` se nenhum outro jogador existir | `[x]` ✅ Aplicado em 2026-09-02 |
| **CR-02-03** | F — Melhoria opcional | 🟢 Menor | Dimensionamento de buffers não encolhe após picos de bots em raids populosas | `[x]` ✅ Aceito / Padrão de Projeto |

---

## 🔍 Pontos Críticos e Análise Detalhada

### CR-02-01 · D — Arquitetura · 🟡 Médio

**`CalcEnemyPlaceJob.Dispose()` tenta descartar fatias nativas (`GetSubArray`)**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/EnemyPlaceRaycastJob.cs:38-64`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/EnemyPlaceRaycastJob.cs#L38-L64)

**Problema:**
No struct `CalcEnemyPlaceJob`, o método `Dispose()` permaneceu com chamadas diretas a `.Dispose()` nos campos:
```csharp
public void Dispose()
{
    if (PlacePositions.IsCreated) PlacePositions.Dispose();
    if (BotPositions.IsCreated) BotPositions.Dispose();
    if (EnemyPositions.IsCreated) EnemyPositions.Dispose();
    if (PlaceDistancesToBot.IsCreated) PlaceDistancesToBot.Dispose();
    if (PlaceDistancesToEnemy.IsCreated) PlaceDistancesToEnemy.Dispose();
}
```
Como esses campos agora recebem fatias sub-array (`GetSubArray`) gerenciadas pelos buffers persistentes de `EnemyPlaceRaycastJob`, qualquer invocação acidental de `CalcEnemyPlaceJob.Dispose()` lançará `InvalidOperationException: The NativeArray can not be Disposed because it was not allocated with a valid allocator.`.

**Por que importa:**
Embora tenhamos removido a chamada a `EnemyPlaceJob.Dispose()` dentro do loop principal, manter o método ativo no struct representa uma armadilha de código morto/legado que pode causar crash se reutilizado futuramente.

**Sugestão:**
Tornar o método `Dispose()` de `CalcEnemyPlaceJob` um no-op seguro com comentário explicativo:
```csharp
public void Dispose()
{
    // Memória nativa gerenciada exclusivamente pelos buffers persistentes de EnemyPlaceRaycastJob
}
```

**Decisão:**
- `[x]` ✅ Aceitar sugestão (Aplicado em 2026-09-02)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** `CalcEnemyPlaceJob.Dispose()` transformado em no-op seguro documentado, evitando descarte ilegal de fatias nativas.

---

### CR-02-02 · B — Bug latente · 🟢 Menor

**`OtherPlayerDirectionData` aloca com `count = 0` se nenhum outro jogador existir**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/DirectionDataJob.cs:34-41`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/DirectionDataJob.cs#L34-L41)

**Problema:**
```csharp
int count = OtherPlayers.Count;
if (!OtherPlayerDirectionData.IsCreated || OtherPlayerDirectionData.Length != count)
{
    if (OtherPlayerDirectionData.IsCreated)
    {
        OtherPlayerDirectionData.Dispose();
    }
    OtherPlayerDirectionData = new NativeArray<PlayerDirectionData>(count, Allocator.Persistent);
}
```
Se `OtherPlayers.Count == 0` (ex: jogador sozinho no mapa no instante inicial antes dos spawns de bots), o código cria um `new NativeArray<PlayerDirectionData>(0, Allocator.Persistent)`. Logo em seguida, quando o primeiro bot spawnar (`count = 1`), ele descarta o array de tamanho 0 e cria outro de tamanho 1.

**Por que importa:**
Gera uma alocação/descarte desnecessária no primeiro frame em que o jogador está isolado.

**Sugestão:**
Garantir capacidade mínima ou não alocar se `count <= 0`:
```csharp
int count = OtherPlayers.Count;
if (count > 0 && (!OtherPlayerDirectionData.IsCreated || OtherPlayerDirectionData.Length != count))
{
    if (OtherPlayerDirectionData.IsCreated)
    {
        OtherPlayerDirectionData.Dispose();
    }
    OtherPlayerDirectionData = new NativeArray<PlayerDirectionData>(count, Allocator.Persistent);
}
```

**Decisão:**
- `[x]` ✅ Aceitar sugestão (Aplicado em 2026-09-02)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Adicionada guarda `if (count > 0 && ...)` e limpeza defensiva caso a contagem caia para 0.

---

### CR-02-03 · F — Melhoria opcional · 🟢 Menor

**Dimensionamento de buffers não encolhe após picos de bots em raids populosas**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/VisionRaycastJob.cs:138`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/VisionRaycastJob.cs#L138)

**Problema:**
O método `EnsureCapacity` apenas cresce (`_bufferCapacity = Mathf.Max(required + 64, 128)`), nunca encolhendo os buffers persistentes caso o número de bots ativos diminua significativamente.

**Por que importa:**
Em raids de Streets com 35 bots ativos, a capacidade máxima alocada atinge ~1.500 posições de raycasts. A pegada de memória nativa total para essa quantidade é de aproximadamente ~140 KB (quilobytes), o que é trivial em computadores modernos e não justifica a complexidade de rotinas de *shrink*.

**Sugestão:**
Manter como está. A retenção de ~140 KB durante a partida inteira é muito mais eficiente do que realocar frequentemente quando o número de bots oscila.

**Decisão:**
- `[x]` Rejeitar / Aceitar como comportamento desejado (140 KB é insignificante e evita fragmentação).

---

## 🏆 Conclusão do Review

* **Bloqueadores 🔴:** **0** — A implementação da Etapa 2 eliminou de forma limpa todas as alocações transitórias de memória nativa nos 3 subsistemas avaliados.
* **Segurança de Threads:** Todos os buffers persistentes contam com chamadas de `.Complete()` antes do `Dispose()`, eliminando riscos de crash por acesso concorrente na saída da raid.
* **Recomendação:** A aplicação de **CR-02-01** (sanitização de `CalcEnemyPlaceJob.Dispose`) e **CR-02-02** (guarda para `count <= 0`) fechará com excelência a Etapa 2.
