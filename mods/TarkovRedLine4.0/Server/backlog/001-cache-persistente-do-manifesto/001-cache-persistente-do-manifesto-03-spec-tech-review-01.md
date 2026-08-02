# 001 — Cache persistente do manifesto · Review Técnica 01

**Mod:** TarkovRedLine.Server
**Spec técnica revisada:** [001-cache-persistente-do-manifesto-02-spec-tech.md](001-cache-persistente-do-manifesto-02-spec-tech.md)
**Data:** 2026-08-02

> Review adversarial por sub-agent independente (contexto limpo), cruzando a spec com o código real do servidor **e** com o consumidor real no launcher. **Nenhum bloqueador** — o desenho funciona; os achados são de raciocínio/robustez da spec. Aplicado em modo `/g-autodev`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 5 · 🟢 Menores: 2 · ✅ Resolvidos: 7 · Total: 7

## Índice

| ID | Cat · Impacto | Título | Status |
|---|---|---|---|
| PA-01-01 | A · 🟡 | `generatedAt` (o eixo de tudo) não é reconhecido na spec | ✅ Resolvido |
| PA-01-02 | C · 🟡 | Garantia "byte-a-byte" do hash é falsa → invariante real é token opaco | ✅ Resolvido |
| PA-01-03 | B · 🟡 | Persistência faz um manifesto stale SOBREVIVER a reinícios | ✅ Resolvido |
| PA-01-04 | A · 🟡 | Exceção no boot pode virar unobserved/silenciosa | ✅ Resolvido |
| PA-01-05 | A · 🟡 | `skipFileScan` hard-desligado recalibra o que o item entrega | ✅ Resolvido |
| PA-01-06 | E · 🟢 | Nota "reusar FileInfo" contradiz o stub | ✅ Resolvido |
| PA-01-07 | E · 🟢 | `async` sem `await` (CS1998) na redação | ✅ Resolvido |

---

## Pontos

### PA-01-01 · A — Gap · 🟡 ✅ Resolvido

**O `generatedAt` é o motivo real de o hash mudar todo boot — e a spec não o menciona**

**Problema:** o `manifestObj` embute `generatedAt = DateTime.UtcNow.ToString("O")` ([ModUpdater.cs:613](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L613)), e o hash é o MD5 de `JsonSerializer.Serialize(manifestObj)` ([:629,:632](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L629)) — que inclui esse campo. É o **único** campo não-determinístico; logo o hash muda a cada geração mesmo sem mudança de conteúdo. A persistência serve o `hash`/`manifest` **congelados** (com o `generatedAt` da geração original), e como a impressão leve não inclui `generatedAt`, o hash fica estável entre boots — o que é o efeito desejado. Mas a spec nunca cita isso; um mantenedor futuro que "conserte" o `generatedAt` reintroduz a instabilidade sem perceber.

**Resolução:** adicionado risco explícito (§7 R-7) e nota na §1 — `generatedAt` é o campo não-determinístico, a persistência o congela de propósito (é o habilitador da estabilidade), e a consequência semântica (o `generatedAt` servido passa a ser o da geração original, não o do boot).

### PA-01-02 · C — Erro de Lógica (na justificativa) · 🟡 ✅ Resolvido

**A spec afirma preservação "byte-a-byte" do manifesto — e isso é falso (embora inofensivo)**

**Problema:** R-2/§9 tratavam `Ok(_manifestCache)` como byte-a-byte com o que gera o hash. Não é: o hash usa `JsonSerializer.Serialize` com opções **default** ([:629](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L629)), enquanto o MVC serializa `/manifest` com opções **web** (camelCase) — já hoje divergem. O launcher **não recomputa** o hash: usa `/manifest-hash` como **token opaco** (compara com `manifest_hash.txt`, [ProfileViewModel.cs:465-489](../../../../launcher/Launcher4.0-v2/project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L465)) e consome `/manifest` como **dados parseados** (`manifest["files"].ToObject<...>`).

**Resolução:** R-2 reescrito para enunciar o invariante real — token opaco + dados parseados; o round-trip via `JsonElement` preserva a semântica (mesmo array `files`, mesmos `path`/`hash`/`size`), que é o que basta. Removida a alegação byte-a-byte.

### PA-01-03 · B — Edge Case · 🟡 ✅ Resolvido

**A persistência faz um manifesto stale sobreviver a reinícios (antes sumia no próximo boot)**

**Problema:** hoje cada boot regera → hashes por-arquivo sempre frescos. Com persistência, se a impressão **não detectar** uma mudança (edição in-place preservando count+sizeSum+maxMtime+pathsDigest — ex.: patch de poucos bytes com mtime restaurado), o servidor serve manifesto stale **entre reinícios** até um `/refresh`. Conecta ao modo de falha da memória `reference_launcher_manifest_stale_phantom_sync`.

**Resolução:** CC-1 ampliado e §7 (R-8) reforçam: a impressão pode ter false-negative (estreito na prática — edições normais mexem no mtime), e `/refresh` é a escotilha (regera+persiste). Registrado que o stale, antes efêmero, agora sobrevive ao reinício.

### PA-01-04 · A — Gap · 🟡 ✅ Resolvido

**Exceção no boot pode virar unobserved task exception (falha silenciosa)**

**Problema:** `Task.Run(EnsureManifestReady)` no static ctor; `EnsureManifestReady` só tem `try/finally` (sem `catch`). Depende de o `GenerateManifestCore` reter o `try/catch` interno atual ([:436-645](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L436)). Se um implementador mover o try/catch para o wrapper, o Core lança para dentro do `EnsureManifestReady` → exceção escapa → unobserved (não derruba o boot em .NET moderno, mas some o log "Critical error" — diagnóstico difícil).

**Resolução:** fixado na spec: (a) `GenerateManifestCore` **retém** o `try/catch` de :641-645; **e** (b) o disparo no boot é envolvido — `Task.Run(() => { try { EnsureManifestReady(); } catch (Exception ex) { Console.WriteLine(...); } })` — pra qualquer falha (inclusive fora do Core, ex.: `GetManifestCachePath`) ser logada, nunca silenciosa. Stubs §5.4/§5.5 atualizados.

### PA-01-05 · A — Gap · 🟡 ✅ Resolvido

**`skipFileScan` está hard-desligado no launcher — o ganho real é o pré-aquecimento, não "pular o scan"**

**Problema:** [ProfileViewModel.cs:481](../../../../launcher/Launcher4.0-v2/project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L481) — `bool skipFileScan = false; // Desabilitado a pedido`. O launcher SEMPRE faz scan completo e SEMPRE busca `/manifest` (retry até 5×3s, :494-504). Então o ganho concreto deste item é **eliminar a espera do retry-loop** (o `/manifest` responde na hora porque já está quente), **não** "o hash congelado faz o launcher pular o scan" — esse benefício é **latente** (só volta se `skipFileScan` virar `true`).

**Resolução:** §1 e o passo de validação in-game (§8) ajustados — a validação mira em `/manifest` **instantâneo** no 1º login pós-boot (sem os retries) e no log "carregado do disco"; o "pular scan" fica registrado como benefício latente. Sem falsa expectativa.

### PA-01-06 · E — Legibilidade · 🟢 ✅ Resolvido

**Nota "reusar o FileInfo (:484)" contradiz o stub, que cria um FileInfo próprio**

**Resolução:** nota do §5.2 corrigida — `FileInfo` é só `stat` (não abre o arquivo), custo desprezível; o stub reaproveita o mesmo `fi` para a linha do `size` do manifesto (uma `FileInfo` por arquivo), eliminando a contradição.

### PA-01-07 · E — Legibilidade · 🟢 ✅ Resolvido

**`async Task` sem `await` (CS1998)**

**Problema:** o wrapper `GenerateManifestAsync` fica `async` sem `await` → CS1998. Já é assim hoje (o método atual não tem `await`), então não é regressão.

**Resolução:** stub §5.4 troca o wrapper para não-`async` (`private static Task GenerateManifestAsync() { …; return Task.CompletedTask; }`) — elimina o CS1998 e mantém `_ = GenerateManifestAsync()` funcionando nos endpoints.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Review 01 via `/review-technical-spec` (sub-agent adversarial + verificação no launcher consumidor). 0 🔴 · 5 🟡 · 2 🟢, todos aplicados na spec técnica no mesmo passo (`/g-autodev`). Achados-chave: `generatedAt` como eixo do hash instável; invariante real (token opaco); `skipFileScan` desligado recalibra o entregue. |
