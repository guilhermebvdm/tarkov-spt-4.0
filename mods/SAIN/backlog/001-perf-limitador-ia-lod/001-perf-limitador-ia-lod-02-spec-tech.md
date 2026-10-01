# 001 — Otimização do Limitador de IA por Distância (AI Limiter + LOD) · Spec Técnica

**Mod:** SAIN (`modded-multithread`)
**Spec funcional:** [001-perf-limitador-ia-lod-01-spec.md](001-perf-limitador-ia-lod-01-spec.md)
**Criado:** 2026-09-19

> Este item não patcheia nenhum método do EFT — toda a mudança é interna ao SAIN (`SAINAILimit`, `BotComponent`, `AILimitSettings`), que já tem seus próprios pontos de entrada (`ManualUpdate`, chamado pelo `BotManagerComponent` do próprio SAIN). A seção "Pontos de patch" padrão não se aplica; a estratégia é descrita como mudança de fluxo de dados interno.

## 1. Estratégia

Três correções cirúrgicas, todas dentro de `mods/SAIN/modded-multithread/SAIN/`, sem novo ponto de entrada e sem tocar em nenhuma assinatura pública existente:

1. **`AUD-01-01` (corrigido na Review Técnica 01, `PA-01-01`)** — a duplicação é eliminada na direção **oposta** à proposta original. `SAINAILimit` mantém sua fonte de dado atual (`FindClosestHumanPlayer` via `PlayerComponent`/`OtherPlayersData`, alimentada pelo `DirectionDataJob`) **intocada** — trocar essa fonte mudaria comportamento sensorial real, não só eficiência (`PA-01-01`), e aumentaria a exposição às pendências `[P-2.1]`/`[P-2.2]` da memória do mod. Em vez disso, `BotComponent.GetMinDistanceToHumanPlayer()` (usado só para bucketing de tier do LOD, tolerante a menor precisão) passa a reusar `AILimit.ClosestPlayerDistanceSqr` — propriedade pública já existente em `SAINAILimit` (`BotComponent.AILimit` já expõe a instância, `BotComponent.cs:129`) — com fallback para o cálculo síncrono próprio quando esse valor não estiver disponível (`PA-01-02`).
   - ⚠️ **Armadilha de nome confirmada durante esta correção:** apesar do nome, `SAINAILimit.ClosestPlayerDistanceSqr` **não é squared** — é distância linear em metros (`SAINAILimit.cs:44-45`, alimentada por `PlayerDistanceData.Distance`, documentada como "Distance, in Meters" em `PlayerDistanceData.cs:46-49`, e comparada diretamente contra `AILimitRanges` em metros em `SAINAILimit.CheckDistances`). **Não aplicar `Mathf.Sqrt()`** ao consumir esse valor — diferente do cálculo antigo de `GetMinDistanceToHumanPlayer()`, que genuinamente precisava de `Mathf.Sqrt(closestSqrMag)` porque sua fonte (a outra sobrecarga de `FindClosestHumanPlayer`, baseada em `Vector3`) retorna magnitude ao quadrado de verdade. Nome enganoso é uma característica pré-existente do SAIN, não introduzida por este item — só documentado aqui para não ser reproduzido incorretamente na implementação.
2. **`AUD-01-02`** — `BotComponent` ganha um campo privado de estado (`_wasCloseToHuman`) e um dead-band assimétrico na transição perto/longe.
3. **`AUD-01-03`** — os 4 limiares do LOD migram de `const` privada em `BotComponent.cs` para campos públicos novos em `AILimitSettings.cs`, no mesmo padrão de atributos já usado por `AILimitRanges`.
4. **`AUD-01-04`** — atualização de `docs/modded-multithread/01-arquitetura-multithread-e-lod.md` (documentação, sem código).

Alternativa descartada: unificar os dois sistemas (`SAINAILimit` + LOD) numa única classe. Rejeitada por violar a restrição de compatibilidade — `SAINAILimit`/`AILimitSettings` são tipos públicos, possivelmente referenciados por outros mods; uma fusão exigiria remover/renomear tipos existentes. A correção escolhida mantém os dois sistemas como estão, só eliminando a redundância de dados entre eles.

## 2. Pontos de patch

N/A — nenhum patch Harmony novo ou alterado. Todas as mudanças são em métodos já existentes do próprio SAIN, chamados pelo ciclo de tick interno do mod (`BotComponent.ManualUpdate`, já coberto pelo lifecycle existente de criação/destruição por bot/raid).

## 3. Novas propriedades F12/F6 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `AILimitSettings` (mesma seção de `AILimitRanges`) | `LODCloseDistance` | float | `50` | 20–100 | — | Distância (m) abaixo da qual o bot recebe atualização de IA em taxa máxima. |
| `AILimitSettings` | `LODMidDistance` | float | `150` | 50–300 | — | Distância (m) abaixo da qual o bot recebe atualização em taxa média (~25Hz); acima, taxa mínima (~8Hz). |
| `AILimitSettings` | `LODCloseDistanceMargin` | float | `10` | 0–30 | Sim | Margem de segurança (m) para sair da faixa "perto" — evita oscilação de taxa de atualização para bots parados perto do limiar. |
| `AILimitSettings` | `LODMidIntervalSeconds` | float | `0.04` | 0.02–0.1 | Sim | Intervalo (s) entre atualizações de IA para bots na faixa média de distância (~25Hz no padrão). |
| `AILimitSettings` | `LODFarIntervalSeconds` | float | `0.12` | 0.05–0.3 | Sim | Intervalo (s) entre atualizações de IA para bots na faixa distante (~8Hz no padrão). |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-multithread/SAIN/Preset/GlobalSettings/Categories/General/AILimitSettings.cs` | MODIFICAR | Adiciona os 5 campos públicos novos da seção 3 (só adição — `AILimitRanges` e demais membros existentes intocados) |
| `modded-multithread/SAIN/Components/BotComponent.cs` | MODIFICAR | Remove as 4 `const` privadas de LOD; lê os valores de `AILimitSettings`; adiciona `_wasCloseToHuman` (privado) e aplica o dead-band; `GetMinDistanceToHumanPlayer` passa a ler `AILimit.ClosestPlayerDistanceSqr` primeiro, com fallback pro cálculo próprio (`PA-01-01`/`PA-01-02`) |
| `modded-multithread/SAIN/Classes/Bot/SAINAILimit.cs` | **NÃO TOCADO** | Revertido em relação à primeira versão desta spec (`PA-01-01`) — fonte de dado sensorial permanece exatamente como está hoje |
| `docs/modded-multithread/01-arquitetura-multithread-e-lod.md` | MODIFICAR | Nova seção documentando a coexistência `SAINAILimit` (sensorial) vs. LOD (tick-rate) |

## 5. Stubs de código

```csharp
// AILimitSettings.cs — campos novos (adição, nada removido/renomeado)
[Name("LOD Close Distance"), Description("Distância (m) para atualização de IA em taxa máxima.")]
[MinMax(20f, 100f, 1f)]
public float LODCloseDistance = 50f;

[Name("LOD Mid Distance"), Description("Distância (m) para atualização de IA em taxa média.")]
[MinMax(50f, 300f, 1f)]
public float LODMidDistance = 150f;

[Name("LOD Close Distance Margin"), Description("Margem de segurança (m) para evitar oscilação de tier."), IsAdvanced]
[MinMax(0f, 30f, 1f)]
public float LODCloseDistanceMargin = 10f;

[Name("LOD Mid Interval"), Description("Intervalo (s) de atualização para bots na faixa média."), IsAdvanced]
[MinMax(0.02f, 0.1f, 100f)]
public float LODMidIntervalSeconds = 0.04f;

[Name("LOD Far Interval"), Description("Intervalo (s) de atualização para bots na faixa distante."), IsAdvanced]
[MinMax(0.05f, 0.3f, 100f)]
public float LODFarIntervalSeconds = 0.12f;
```

```csharp
// BotComponent.cs — GetMinDistanceToHumanPlayer() reusa o dado do SAINAILimit em vez de
// varrer de novo (ref: AUD-01-01, direção corrigida por PA-01-01). SAINAILimit.cs NÃO é
// alterado por este item — sua fonte de dado permanece a mesma de hoje.
private float GetMinDistanceToHumanPlayer(float currentTime)
{
    if (_nextDistCheckTime > currentTime)
    {
        return _cachedDistToHuman;
    }
    _nextDistCheckTime = currentTime + 0.5f;

    // ref: AUD-01-01 / PA-01-01 — AILimit.ClosestPlayerDistanceSqr, apesar do nome, é
    // distância LINEAR em metros (ver nota de armadilha na Seção 1). Não aplicar Sqrt.
    float aiLimitDist = AILimit.ClosestPlayerDistanceSqr;
    if (aiLimitDist >= 0f)
    {
        _cachedDistToHuman = aiLimitDist;
        DistanceToClosestHuman = aiLimitDist;
        return aiLimitDist;
    }

    // ref: AUD-01-02 / PA-01-02 — fallback para o cálculo síncrono próprio quando
    // SAINAILimit não tem valor disponível (ex.: Bot.EnemyController.ActiveHumanEnemy
    // ativo agora mesmo, SAINAILimit.cs:29-33, seta ClosestPlayerDistanceSqr = -1f).
    // Nesse cenário o bot já entra em Tier 0 por inCombat/isUnderFire de qualquer forma
    // (BotComponent.cs:267), então este fallback é rede de segurança, não caminho comum.
    var tracker = GameWorldComponent.Instance?.PlayerTracker;
    if (tracker != null && tracker.FindClosestHumanPlayer(out float closestSqrMag, Position, out _) != null)
    {
        float dist = Mathf.Sqrt(closestSqrMag); // esta sobrecarga SIM retorna magnitude ao quadrado
        _cachedDistToHuman = dist;
        DistanceToClosestHuman = dist;
        return dist;
    }

    _cachedDistToHuman = float.MaxValue;
    DistanceToClosestHuman = float.MaxValue;
    return float.MaxValue;
}
```

```csharp
// BotComponent.cs — dead-band na transição perto/longe (ref: AUD-01-02)
// Substitui: bool isCloseToHuman = distToHuman <= LOD_CLOSE_DIST || (...)
var lod = GlobalSettingsClass.Instance.General.AILimit; // ref: AUD-01-03, fonte dos limiares
float closeDist = lod.LODCloseDistance;
float closeMargin = lod.LODCloseDistanceMargin;

bool isCloseToHuman;
if (_wasCloseToHuman)
{
    isCloseToHuman = distToHuman <= (closeDist + closeMargin);
}
else
{
    isCloseToHuman = distToHuman <= closeDist;
}
isCloseToHuman |= (currentTime < 5f && distToHuman == float.MaxValue); // preserva CR-01-01
_wasCloseToHuman = isCloseToHuman;
```

`SAINAILimit.cs` não é modificado por este item (revertido em relação à primeira versão desta spec — ver `PA-01-01`).

## 6. Fluxo de dados

```
[A] Grupo _tickWhenActiveClasses (inclui SAINAILimit), throttlado por CurrentLodTier
    └──> SAINAILimit.CheckAILimit() — fonte de dado INALTERADA: FindClosestHumanPlayer via
         PlayerComponent/OtherPlayersData (alimentado pelo DirectionDataJob)
         ├──> CheckDistances() — thresholds de AILimitRanges inalterados, degradação sensorial
         └──> AILimit.ClosestPlayerDistanceSqr (público, já existia — nome enganoso, é linear)

[B] BotComponent.ManualUpdate() (tick do próprio SAIN, ~toda frame ativa)
    └──> GetMinDistanceToHumanPlayer() (a cada 0.5s, cacheada)
         ├──> 1ª tentativa: lê AILimit.ClosestPlayerDistanceSqr (AUD-01-01/PA-01-01) — sem nova busca
         ├──> fallback: PlayerSpawnTracker.FindClosestHumanPlayer(Vector3) próprio, só se [A] não tiver valor (PA-01-02)
         └──> alimenta a decisão de CurrentLodTier (com dead-band, AUD-01-02)
```

Antes desta mudança, [A] e [B] faziam buscas independentes e equivalentes em cadências diferentes. Depois, [B] consome o resultado que [A] já calcula, com fallback só para o caso raro em que [A] ainda não rodou (`PA-01-02`). A direção foi escolhida (em vez do inverso) para não alterar a fonte de dado sensorial de `SAINAILimit`, que é o comportamento que a spec funcional promete preservar (`PA-01-01`).

## 7. Riscos e dependências

- **Patches existentes:** nenhum patch Harmony toca essas classes — risco de conflito é zero nesse eixo.
- **Compatibilidade com outros mods:** `SAINAILimit`, `AILimitSettings`, `BotComponent` são tipos públicos que outros mods (Questing Bots, Looting Bots, pontes FIKA) podem referenciar via reflection ou dependência direta. Mitigação: checklist §9 item 4 e revisão de diff completo antes do `/code-review` — só adição de membros, zero remoção/rename.
- **Ordem de inicialização:** `AILimitSettings` já é carregado no boot do preset (`GlobalSettingsClass`), antes de qualquer `BotComponent` existir — os novos campos seguem o mesmo ciclo, sem risco de leitura antes da inicialização.

## 8. Checklist de implementação

- [x] Adicionar os 5 campos novos em `AILimitSettings.cs` (seção 5) — usando o atributo real `[Advanced]` (não `[IsAdvanced]` como o stub original sugeria; corrigido durante a implementação)
- [x] Remover as 4 `const` privadas de `BotComponent.cs:195-198`, substituir leituras por `GlobalSettingsClass.Instance.General.AILimit.*`
- [x] Adicionar `_wasCloseToHuman` e aplicar o dead-band em `BotComponent.cs:272`
- [x] Alterar `GetMinDistanceToHumanPlayer()` para ler `AILimit.ClosestPlayerDistanceSqr` primeiro (sem `Sqrt` — é linear apesar do nome), com fallback pro cálculo síncrono existente
- [x] **Não tocar em `SAINAILimit.cs`** (revertido por `PA-01-01`) — confirmado, arquivo não modificado
- [x] Adicionar comentários `// ref: AUD-01-01` / `AUD-01-02` / `AUD-01-03` nos pontos alterados
- [x] Atualizar `docs/modded-multithread/01-arquitetura-multithread-e-lod.md` com a seção de coexistência dos dois sistemas (`AUD-01-04`)
- [x] Confirmar via diff completo que nenhum membro `public`/`protected` foi renomeado/removido — só adição (5 campos em `AILimitSettings`, 1 campo privado em `BotComponent`)

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Não introduz novo hook de raid; `BotComponent` já é criado/destruído por bot/raid pelo ciclo existente do SAIN, inalterado por este item. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Verificado nesta investigação: `PlayerSpawnTracker.FindClosestHumanPlayer` filtra por `PlayerComponent.IsAI` (via `BotOwner`), nunca por `MainPlayer`/`IsYourPlayer` — correto para host, convidado Fika e Headless. Este item não altera esse mecanismo, só reduz quantas vezes ele é chamado. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum patch sobre método virtual/ofuscado do EFT. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | Estado alterado é interno ao SAIN (próprios campos), não estado do EFT. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `_wasCloseToHuman` é campo de instância de `BotComponent`, recriado por bot a cada raid — mesmo ciclo de vida que os demais campos já existentes (`_cachedDistToHuman`, etc.), sem persistência entre raids. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Defaults de `LODCloseDistance`/`LODMidDistance`/`LODMidIntervalSeconds`/`LODFarIntervalSeconds` preservam exatamente os valores hoje hardcoded (50/150/0.04/0.12). `LODCloseDistanceMargin` (10) é comportamento **novo** — hoje equivale a margem 0 — mudança intencional coberta pelo critério de aceite de `AUD-01-02` na spec funcional, não é regressão silenciosa (`PA-01-03`). |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Não há patch nem recursão nova. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | `_wasCloseToHuman` é revalidado a cada checagem de 0.5s contra a distância atual — não é um cache que sobrevive a uma troca de contexto sem revalidação. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | N/A | Sem patch-point do EFT envolvido. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Não cria pacote de rede. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-19 | Spec técnica criada via `/optimize-mod-performance SAIN --fase 2`, a partir de `AUD-01-01` a `AUD-01-04` |
| 2026-09-19 | Revisada conforme [Review Técnica 01](001-perf-limitador-ia-lod-03-spec-tech-review-01.md): direção de `AUD-01-01` invertida (`PA-01-01`), fallback de `-1f` documentado (`PA-01-02`), §9 item 6 corrigido (`PA-01-03`), stubs de `[MinMax]` ajustados (`PA-01-04`) |
