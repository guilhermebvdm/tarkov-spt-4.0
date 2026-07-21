# DiscordRaidMap — Code Review · 01

**Mod:** DiscordRaidMap · **Tipo:** client (BepInEx/Harmony; host-only via `HostCheck`)
**Escopo:** mudanças implementadas nas v1.1.0 (leak/size fixes) + v1.1.1 (coleta 100% no intervalo). Diff `original/ → modded/`.
**Data:** 2026-07-21
**Skills:** `spt-mod-best-practices`, `csharp-mod-best-practices`, `spt-memory-leak-analysis`.

> Review **fora do ciclo de backlog** (mod importado, não tem 01-spec/02-spec-tech/05-asbuild). Avalia o código que **eu** implementei. Cada achado tem ID `CR-01-MM` permanente. Referência de leak: [MEMORY-LEAK-review-01.md](MEMORY-LEAK-review-01.md).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 2 · 🟢 Menores: 5 · Total: 8 · ✅ Resolvidos: 8 (v1.1.2)

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria opcional**

## Impacto

- 🔴 **Bloqueador** — fix obrigatório · 🟠 **Forte** — recomendado · 🟡 **Médio** — caso a caso · 🟢 **Menor** — opcional.

## Panorama — o que ficou BOM (verificado)

- ✅ **Teardown correto, sem leak entre raids.** `Plugin.OnDestroy` pareia todos os `-=` ([Plugin.cs:46-58](modded/Plugin.cs#L46)); o `RaidStateCollector` agora **não assina nada** (deixou de depender de eventos). `StopBroadcaster` no `OnRaidEnd` é idempotente (null-check).
- ✅ **Removida a static `AirdropLandedPatch.Airdrops`** — eliminou uma retenção estática que sobrevivia entre raids (melhoria STAT sobre o upstream).
- ✅ **Churn de LOH eliminado (o driver do OOM):** downscale único do fundo ([Renderer.cs:135](modded/RaidMap/Renderer.cs#L135)) + reuso de `_canvas`/`_encodeBuffer` ([Renderer.cs:158](modded/RaidMap/Renderer.cs#L158)). `Font`/measure-`Graphics` cacheados (churn de handles GDI removido).
- ✅ **2 patches Harmony removidos** (um deles per-tick); zero trabalho entre intervalos.
- ✅ **Lock do renderer serializa render vs. Dispose/Replace corretamente; `Renderer.Dispose` é idempotente** (sem double-dispose no caminho de `Stop` + finally do upload).

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | B | 🟠 | Scan de airdrop não filtra sync objects pooled/não-inicializados → marcador fantasma | ✅ Aplicado |
| CR-01-02 | B | 🟡 | Marcadores de morte re-derivados por intervalo somem se o corpo despawna (evento removido os tornava permanentes) | ✅ Aplicado |
| CR-01-03 | B | 🟡 | JPEG sem canal alpha: mapa com transparência achata (provável fundo preto) | ✅ Aplicado |
| CR-01-04 | F | 🟢 | `GetJpegEncoder()` enumera encoders a cada render — cachear | ✅ Aplicado |
| CR-01-05 | F | 🟢 | `Bitmap`/`MemoryStream` de encode alocados por render (agora pequenos) | ✅ Aplicado |
| CR-01-06 | D | 🟢 | `DiscordWebhookClient` não dá `Dispose` no `HttpClient` (pré-existente) | ✅ Aplicado |
| CR-01-07 | B | 🟢 | `_headlessReferencePlayer` cacheado e nunca revalidado (pré-existente) | ✅ Aplicado |
| CR-01-08 | E | 🟢 | Tooltips dos novos configs só em inglês (convenção do repo é bilíngue) | ✅ Aplicado |

---

## Achados

### CR-01-01 · B — Bug latente · 🟠 Forte

**Scan de airdrop não filtra objetos pooled/não-inicializados → marcador fantasma**

**Local:** [`modded/RaidMap/RaidStateCollector.cs:130-152`](modded/RaidMap/RaidStateCollector.cs#L130) (`AddAirdrops`)

**Problema:** `AddAirdrops` itera `processor.GetSynchronizableObjects()` e desenha um marcador para **todo** `AirdropSynchronizableObject`. Mas o decompile mostra que `GetSynchronizableObjects()` faz `yield` de **todos** os itens de `List_0`/`List_1` sem filtrar ([SyncObjectProcessorClass.cs:81-91](../../references/eft-decompiled/Assembly-CSharp/SyncObjectProcessorClass.cs#L81)); e `WriteSyncObjects` só usa os que têm `IsInited` (`:107`) chamando depois `RemoveNonActiveAndStaticObjects` (`:114`) — provando que objetos **não-inicializados/inativos** (pooled) coexistem nessas listas. Um airdrop pooled/inativo (posição de origem ou estado pré-drop) vira um **marcador fantasma** no mapa. O patch original nunca mostrava esses (só adicionava quando `method_3` tickava num airdrop ativo).

**Por que importa:** regressão de comportamento visível — marcadores de airdrop errados/fantasma, exatamente o tipo de coisa que a coleta por evento evitava. Introduzido pela minha refatoração v1.1.1.

**Sugestão:** filtrar por estado ativo/inicializado:
```csharp
if (syncObject is not AirdropSynchronizableObject airdrop
    || airdrop == null
    || !airdrop.IsInited            // sync object realmente inicializado (idem WriteSyncObjects)
    || !airdrop.isActiveAndEnabled) // GameObject ativo na cena
{
    continue;
}
```
Confirmar in-game que só airdrops reais aparecem. (Se `IsInited` não for acessível, `isActiveAndEnabled` + checagem de `Type == SynchronizableObjectType.AirDrop` já reduz o risco.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**Marcadores de morte re-derivados por intervalo somem se o corpo despawna**

**Local:** [`modded/RaidMap/RaidStateCollector.cs:70-79`](modded/RaidMap/RaidStateCollector.cs#L70) (clear + `RefreshKilledPlayers`) · [`:190-205`](modded/RaidMap/RaidStateCollector.cs#L190)

**Problema:** as listas de mortos/mortes são limpas e reconstruídas a cada snapshot pela varredura de corpos (`AllPlayersEverExisted` + campo `Corpse` + `LastAggressor`). Isso remove a acumulação persistente que o evento `OnDead` (removido) fazia. Consequência: se um **corpo despawna** ou o `LastAggressor` for limpo entre um snapshot e o próximo, o marcador **desaparece** (antes era permanente até o fim da raid).

**Por que importa:** mudança de comportamento. **Não** é regressão do mecanismo de atribuição — o `RefreshKilledPlayers` do upstream já lia `LastAggressor` no scan (o evento era o caminho redundante). O que muda é a **permanência**. Aceitável dentro do trade-off de latência já combinado, mas precisa de confirmação de que corpos/atribuição persistem o suficiente numa raid real.

**Sugestão:** verificar in-game (raid com kills PMC) que os marcadores de inimigo/boss morto aparecem e persistem. Se a persistência importar, reintroduzir uma acumulação leve **sem** patch (manter um `HashSet<ProfileId>` de mortos já vistos, alimentado pelo próprio scan do intervalo) — mantém a coleta no intervalo e a permanência, sem voltar o patch per-evento.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · B — Bug latente · 🟡 Médio

**JPEG sem canal alpha: mapa com transparência achata**

**Local:** [`modded/RaidMap/Renderer.cs:456-505`](modded/RaidMap/Renderer.cs#L456) (`EncodeImage`, ramo JPEG)

**Problema:** JPEG não tem alpha. O canvas final é opaco no caminho dos markers (`Blend` força `a=255`), mas os pixels **de fundo** vêm de `CopyBackground`/`GetCanvas` com o alpha original do PNG do mapa. Se algum PNG de mapa tiver regiões transparentes, o JPEG as achata (provavelmente contra preto), gerando artefato visual. Os mapas atuais parecem opacos (risco baixo), mas é uma suposição não garantida.

**Por que importa:** só afeta o formato JPEG (novo default) e só se algum mapa tiver transparência. Baixo risco, mas silencioso.

**Sugestão:** garantir fundo opaco ao carregar (setar `a=255` no `LoadPng` do background, ou documentar a suposição "mapas são opacos"). O caminho PNG não é afetado.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-04 · F — Melhoria · 🟢 Menor

**`GetJpegEncoder()` enumera os encoders a cada render**

**Local:** [`modded/RaidMap/Renderer.cs:507-517`](modded/RaidMap/Renderer.cs#L507)

**Problema:** `ImageCodecInfo.GetImageEncoders()` aloca um array e é chamado a cada render (a cada intervalo). Trivial, mas desnecessário.

**Sugestão:** resolver uma vez num `static readonly ImageCodecInfo JpegEncoder = ...` (com fallback null tratado como hoje).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar: _________________

---

### CR-01-05 · F — Melhoria · 🟢 Menor

**`Bitmap`/`MemoryStream` de encode alocados por render**

**Local:** [`modded/RaidMap/Renderer.cs:458-505`](modded/RaidMap/Renderer.cs#L458)

**Problema:** o `new Bitmap(width,height)` (GDI, ~3,3 MB no tamanho reduzido) e o `MemoryStream` são alocados a cada render, apesar do `_encodeBuffer` reusado. Como o mapa é fixo na raid, poderiam ser reusados. Impacto pequeno agora (10× menor que antes), a cada 15 s.

**Sugestão:** opcional — reusar o `Bitmap` de encode por dimensão de mapa. Deferir se não valer a complexidade.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (aceitar como dívida): _________________

---

### CR-01-06 · D — Arquitetura · 🟢 Menor (pré-existente)

**`DiscordWebhookClient` não dá `Dispose` no `HttpClient`**

**Local:** [`modded/RaidMap/DiscordWebhookClient.cs:16`](modded/RaidMap/DiscordWebhookClient.cs#L16) (`_http = new()`)

**Problema:** o `HttpClient` é criado por raid e nunca disposto (a classe não é `IDisposable`). O `SocketsHttpHandler` interno só é liberado no finalizador. Numa sessão de headless com muitas raids, acumula até o GC. Pré-existente (não introduzido por mim).

**Sugestão:** tornar `DiscordWebhookClient : IDisposable` e dispô-lo no `RaidBroadcaster.Stop`. Baixa prioridade.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (aceitar como dívida): _________________

---

### CR-01-07 · B — Bug latente · 🟢 Menor (pré-existente)

**`_headlessReferencePlayer` cacheado e nunca revalidado**

**Local:** [`modded/RaidMap/RaidStateCollector.cs:275-296`](modded/RaidMap/RaidStateCollector.cs#L275) (`GetReferencePlayer`)

**Problema:** no headless, o "reference player" (usado para filtrar extracts por `Side` e para o tracking de aliados) é cacheado no primeiro peer Fika e retornado sem revalidar. Se esse peer desconectar/morrer, o filtro fica preso a um jogador stale. Pré-existente (não tocado por este trabalho).

**Sugestão:** revalidar o cache (se `!IsAlive` ou saiu do mundo, re-resolver). Deferir — pré-existente e fora do escopo desta rodada.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (aceitar como dívida): _________________

---

### CR-01-08 · E — Legibilidade · 🟢 Menor

**Tooltips dos novos configs só em inglês**

**Local:** [`modded/Settings.cs:68-84`](modded/Settings.cs#L68) (seção *Image Output*)

**Problema:** a convenção do repo (skill `/review-mod-properties`) é tooltip bilíngue (`"<EN>\n\n<PT>"`). Os novos configs são só-EN — **mas** os tooltips já existentes do mod também são só-EN, então isto **combina com o estilo local**. Nota de consistência, não defeito.

**Sugestão:** deixar para uma passada de `/review-mod-properties` que padronize o mod inteiro (não só os novos). Não bloquear.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (aceitar como dívida): _________________

---

## Dívida conhecida (não é achado novo — vem do MEMORY-LEAK-review-01)

- **ML-01-03** (deferido): render em `System.Drawing`/GDI+ roda numa thread de background (`Task.Run`). Serializado pelo `_rendererLock`, mas GDI+ cross-thread em Mono é frágil. Substituir o texto por atlas de glifos eliminaria a dependência de GDI+.

## Verificação in-game recomendada (antes de fechar)

1. **CR-01-01:** raid com airdrop — confirmar que só o airdrop real aparece (sem fantasma na origem).
2. **CR-01-02:** raid com kills PMC — confirmar que marcadores de inimigo/boss morto aparecem e persistem.
3. **CR-01-03:** conferir visualmente o JPEG dos mapas (sem fundo preto onde deveria ter mapa).
4. **Leak (o objetivo):** RSS estável ao longo de 20 min em Customs (MEMORY-LEAK-review-01 §Plano).

## Resolução — todos aplicados (v1.1.2, 2026-07-21)

| ID | O que foi feito | Arquivo |
|---|---|---|
| CR-01-01 | `AddAirdrops` filtra `IsInited && IsActive` (usa os campos de estado do próprio `SynchronizableObject`) — sem marcador fantasma | `modded/RaidMap/RaidStateCollector.cs` |
| CR-01-02 | Removido o `Clear()` das listas de mortos; acumulam com dedup por `Contains` e são limpas no `Dispose` — permanência restaurada, ainda 100% no intervalo | `modded/RaidMap/RaidStateCollector.cs` |
| CR-01-03 | `LoadBackground` carrega o fundo com `forceOpaque: true` (alpha=255) — JPEG nunca achata contra preto; markers preservam alpha | `modded/RaidMap/Renderer.cs` |
| CR-01-04 | Encoder JPEG resolvido 1× em `static readonly JpegEncoder` (era enumerado por render) | `modded/RaidMap/Renderer.cs` |
| CR-01-05 | `_encodeBitmap` reusado entre renders (recriado só se o tamanho muda), disposto no `Dispose` | `modded/RaidMap/Renderer.cs` |
| CR-01-06 | `HttpClient` agora é `static readonly Http` compartilhado (plugin-scope) — sem handler vazando por raid | `modded/RaidMap/DiscordWebhookClient.cs` |
| CR-01-07 | `GetReferencePlayer` revalida o cache: só retorna `_headlessReferencePlayer` se `HealthController.IsAlive`, senão re-resolve | `modded/RaidMap/RaidStateCollector.cs` |
| CR-01-08 | Tooltips dos 3 configs novos agora bilíngues (`EN\n\nPT`) | `modded/Settings.cs` |

Verificação in-game (§"Verificação in-game recomendada") continua pendente: airdrop sem fantasma, marcadores de morte persistem, JPEG sem fundo preto, e RSS estável.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-21 | Code review 01 criada (revisão das mudanças v1.1.0 + v1.1.1) |
| 2026-07-21 | Todos os 8 achados aplicados (v1.1.2) |
