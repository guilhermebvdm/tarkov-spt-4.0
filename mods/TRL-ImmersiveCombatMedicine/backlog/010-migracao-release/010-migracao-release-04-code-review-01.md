# 010 — Migração de configs + release · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [010-migracao-release-01-spec.md](010-migracao-release-01-spec.md)
**Spec técnica:** [010-migracao-release-02-spec-tech.md](010-migracao-release-02-spec-tech.md)
**Asbuild:** [010-migracao-release-05-asbuild.md](010-migracao-release-05-asbuild.md)
**Data:** 2026-07-25

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** snapshot de 2026-07-25 (Sessão 5, topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`) · pendências que afetam este item: nenhuma bloqueadora — a única pendência aberta relevante ao mod é [P-4.4] (validação in-game do overhaul completo, geral e pré-existente, não específica do item 010). A Sessão 5 registra o item 010 como "iniciado" após o 009; este code-review é a continuação natural.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F — Melhoria opcional | 🟢 Menor | `BandAidUI.ShowTreatment` mantém 2 textos PT hardcoded fora do escopo declarado da migração i18n | ✅ Aplicado |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Confirmações (evidência, não achados — releitura independente do código real)

### Prioridade 1 — mudança de wire format Fika (`DenyReason` string → `DenyReasonId` enum/byte)

Confirmado via `git diff` (working tree vs. HEAD, arquivo por arquivo — não apenas leitura do stub) que **todos** os pontos que tocavam `.DenyReason` foram migrados, sem resíduo:

- **`BandAidHealCheckPacket.cs`** — `BandAidHealCheckResponsePacket.DenyReason` (`string`) virou `DenyReasonId` (`internal TRLImmersiveCombatMedicine.MedicDenyReasonId`, `:49`). `Serialize`/`Deserialize` simétricos: `writer.Put((byte)DenyReasonId)` (`:60`) / `DenyReasonId = (MedicDenyReasonId)reader.GetByte()` (`:70`) — mesmo slot de wire, só o tipo muda (string→byte).
- **`BandAidNetworkHandler.cs`** — os 2 pontos de escrita migrados: `OnHealCheckReceived` (`denyReasonId` declarado `:713`, response construído `:732-740` com `DenyReasonId = denyReasonId` em `:738`) e `TryAnswerForLocalBot` (`:878`, response em `:888-896` com `DenyReasonId` em `:894`). O 3º ponto — leitura de diagnóstico em `OnHealCheckResponseReceived:932` — usa `packet.DenyReasonId` (era o achado PA-01-03 da review técnica 01; confirmado presente, não regrediu).
- **`BandAidController.cs`** — `OnHealCheckResponseHandler:114` chama `MedicLocale.GetDenyReasonText(response.DenyReasonId, response.ItemTemplateId)` antes de exibir a notificação — ponto de EXIBIÇÃO correto (tradução no médico, não no paciente).
- **Grep exaustivo** (`grep -rn "\.DenyReason\b" modded`) retorna **zero** ocorrências do campo antigo em qualquer arquivo; `grep -rn "DenyReasonId" modded` retorna exatamente os 5 pontos acima (struct + 2 escritas + 1 leitura de log + 1 exibição) — nenhum ponto órfão, nenhuma leitura esquecida.

**Sobre o desvio CS0052 documentado pelo `/code-mod`:** confirmado correto e suficiente. `internal TRLImmersiveCombatMedicine.MedicDenyReasonId DenyReasonId;` num struct `public` é válido em C# — CS0052 só dispara quando o **campo** é `public`/`protected` com tipo menos acessível; um campo `internal` não tem essa restrição. `Band_Aid` (dono do struct) e `TRLImmersiveCombatMedicine` (dono do enum e dos 3 consumidores do campo) são a mesma assembly (mesmo `.csproj`, confirmado em `TRL-ImmersiveCombatMedicine.csproj`), então `internal` não bloqueia nenhum dos pontos de leitura/escrita reais. `Serialize`/`Deserialize` acessam o campo de dentro do próprio struct — acessibilidade é irrelevante ali. Build isolado (ver abaixo) confirma 0 erros.

### Prioridade 2 — `MedicLocale.cs`

- `enum MedicTextId` tem 27 valores (`Aborted=0` … `DenyNoCompatibleWound=26`); `EnTexts`/`PtTexts` têm exatamente 27 entradas cada, na MESMA ordem do enum — mapeamento 1:1 confirmado por contagem manual dos dois arrays contra a lista do enum.
- Placeholders conferidos contra os call sites migrados: `{0}`/`{1}` batem em todos os casos com argumento (`CheckingItem`, `ShoulderTapSent`, `TreatmentCompleteWithPart`, `HudFooterDynamic` com 2 args, `TourniquetRemoved` com 2 args — `GetBodyPartName`+`duration.ToString("F0")`, etc.) — confirmado via `git diff` linha a linha de `BandAidController.cs`, `BandAidNetworkHandler.cs`, `TourniquetManager.cs`, `MedicInteractable.cs`, `BandAidUI.cs`.
- Ícones ⚠ (`TourniquetNecrosisWarning`), ☠ (`TourniquetDestroyed`) e ✈ (`ShoulderTapReceived`) — identificados como regressão em PA-01-06/PA-02-01 das 2 rodadas de review técnica — **presentes** em EN e PT no código final (`MedicLocale.cs:78-79,68`).
- `GetDenyReasonText` reusa `ItemTemplateId` já existente no pacote (nenhum campo novo) e resolve `stats?.Name` localmente no médico via `Band_Aid.ItemDatabase.GetStats` — `using Band_Aid;` presente (PA-01-01).

### Prioridade 3 — regressão nos 12 arquivos modificados

`git diff` de cada um dos 12 arquivos (não apenas leitura do estado final) confirma que a lógica de negócio é idêntica — a única mudança de comportamento observável é textual (PT hardcoded → `MedicLocale.Get(...)`), de config (default/tooltip) ou remoção de sondas de log/campos mortos:

- `BandAidController.cs` (74 linhas no diff): Awake/OnDestroy perderam só logs; `OnEnable`/`OnDisable` removidos por inteiro (existiam só para log); `Update()` perdeu 2 blocos log-once + o texto da tag no `catch` (guard preservado); `EnsureMedicInteractables` perdeu o acumulador `attached` por inteiro (sem `CS0219` — confirmado por build). Nenhuma ramificação de negócio tocada.
- `BandAidHealCheckPacket.cs`, `BandAidNetworkHandler.cs`: só o campo/tipo do deny reason (ver Prioridade 1); toda a lógica de relay host/client/bot-local, guards de identidade e `CanUseItem` idênticos.
- `BandAidUI.cs` (38 linhas): título/rótulos de membro passaram de fixados em `BuildUI()` (Awake) para reaplicados em `ShowUI()` — mudança de COMPORTAMENTO intencional e correta (fix do achado de design AP-08 da spec técnica, não uma regressão); `PartLabel` virou wrapper de uma linha; `ShowTreatment` (ver CR-01-01 abaixo) não foi tocado.
- `MedicActionsPatch.cs` (13 linhas, só remoções): campo + 2 logs de diagnóstico removidos; `Prefix` retorna exatamente o mesmo resultado para os mesmos casos.
- `MedicInteractable.cs` (4 linhas): só os 2 literais de `Name` viraram `MedicLocale.Get(...)`.
- `TourniquetManager.cs` (27 linhas): 6 notificações + `GetBodyPartName` viraram wrapper; `duration.ToString("F0")` preservado idêntico ao `{duration:F0}` original (conferido via diff, não achado — não há regressão de casas decimais).
- `HealthPatches.cs` (2 linhas): comentário histórico atualizado para citar a key removida por nome, não o campo C# inexistente — sem efeito de compilação/runtime.
- `TRLImmersiveCombatMedicinePlugin.cs` (103 linhas): 3 `ConfigEntry`+`Config.Bind` removidos; bloco do mojibake em `MigrateOrphanedConfigKeys()` removido por inteiro (método recompila, 5 blocos restantes — `Duracao do Desmaio` cópia + 4 renames Legs/Fall/Arms/Stomach/Blackout — intocados); handler morto `OnHealCheckResponseHandler` + subscribe + `OnDestroy()` do plugin removidos por inteiro (sem substituto necessário); sondas `[DEBUG-ICM]` (campos + heartbeat + logs de Awake) removidas, bloco `IsFikaInstalled`/`EnsurePacketsRegistered()` preservado no topo do `Update()` (PA-01-07).
- `csproj`, `PROPRIEDADES.md`, `mod-backlog.md`: só versão/documentação.

### Prioridade 4 — Bloco A (config cleanup)

- `MigrateOrphanedConfigKeys()` compila (confirmado por build isolado abaixo) — o método hoje tem 6 blocos (não os "5" mencionados na review técnica 01, que subcontou: `legacyDurationDef` + `legacyLegsDef` + `legacyFallDef` + `legacyArmsDef` + `legacyStomachDef` + `legacyBlackoutDef` = 6, cobrindo os itens 008/003/004/005/006/007) — todos usam variáveis locais próprias, nenhum toca campo removido. Essa subcontagem já estava na review técnica (artefato imutável, não revisado aqui) e não afeta o código real, que está correto.
- Grep (`grep -rn "\[DEBUG-ICM\]" modded`) e (`grep -rn "ConfigLegsEnabled\|ConfigArmsEnabled\|ConfigStomachEnabled" modded`) confirmam: zero sondas `[DEBUG-ICM]` em todo `modded/`; zero declarações dos 3 campos removidos (a única ocorrência restante de `ConfigArmsEnabled` é um comentário histórico em `TRLImmersiveCombatMedicinePlugin.cs:323`, não código executável).

### Prioridade 5 — Bloco D (`package-release.sh`)

- `|| true` presente na linha `VER=...` (`:28`), com o comentário explicando o motivo (PA-02-03) — confirmado.
- Script coerente: usa `compile-mod.sh` (não duplica lógica de build), resolve `SPT_PATH` na mesma ordem do `compile-mod.sh`, avisa sobre working tree sujo antes de empacotar, zip via `Compress-Archive` (sem depender de binário externo no Git Bash).

### Verificação de compilação independente

Rodei `bash .agents/scripts/compile-mod.sh TRL-ImmersiveCombatMedicine --allow-same-version` e, para confirmar warnings, um `dotnet build --no-incremental` isolado: **0 erros**, **10 warnings `Harmony003`** — todos em `Patches/Trauma/HealthPatches.cs:37-63` (pré-existentes, fora do escopo deste item, não novos). Confirma a claim do asbuild.

### `PROPRIEDADES.md` e `mod-backlog.md`

Refletem exatamente o F12 pós-implementação: Seção 2 sem as 3 keys legadas, tabela "Removidas" com as 3 entradas novas, `Medic Interact Distance` = `3.5` sem menção a "testes", Seção 5 com a frase atualizada. `mod-backlog.md` já mostra o item 010 como 🟢.

---

## Pontos

### CR-01-01 · F — Melhoria opcional · 🟢 Menor

**`BandAidUI.ShowTreatment` mantém 2 textos PT hardcoded fora do escopo declarado da migração i18n**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:716-718`](../../modded/Patches/Medical/BandAidUI.cs#L716)

**Problema:** os únicos textos player-facing do fluxo de cura Band-Aid que continuam 100% em português hardcoded, sem fallback EN, são:

```csharp
_treatmentText.text = string.IsNullOrEmpty(_treatmentItemName)
    ? $"► TRATANDO: {label}"
    : $"► {_treatmentItemName.ToUpper()} → {label}";
```

Este ponto **não é um gap não detectado** — o próprio asbuild (`010-migracao-release-05-asbuild.md`, "Notas de execução") documenta que `ShowTreatment` foi conferido contra a tabela de rastreio §5 da spec técnica e o inventário da spec funcional, confirmando por grep que não está em nenhum dos dois antes de decidir não tocar. É uma exclusão de escopo deliberada e correta, não um erro do `/code-mod`.

**Por que importa:** é o único resíduo PT-only na cadeia de UI que este item migrou (HUD título/rodapé/rótulos de membro, ActionPanel, notificações de cura/torniquete/handshake — todos agora EN/PT). Um jogador com o jogo em inglês, mid-treatment, vê `"► TRATANDO: HEAD"` em vez de algo como `"► TREATING: HEAD"`. Não quebra nada, mas é uma inconsistência de polimento no mesmo sistema que o item acabou de deixar bilíngue.

**Sugestão:** opcional — se um polimento futuro do i18n for desejado, adicionar 2 `MedicTextId` novos (ex.: `TreatingLabel`/`TreatingLabelWithItem`) espelhando o padrão de `TreatmentCompleteWithPart`, e trocar as 2 linhas de `ShowTreatment` por `MedicLocale.Get(...)`. Não recomendo abrir um item de backlog só para isso — fica como nota para a próxima vez que `BandAidUI.cs` for tocado por outro motivo.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** aplicado diretamente (diretiva do usuário: aplicar todos os achados de review por padrão) — `MedicTextId.TreatingLabel`/`TreatingLabelWithItem` adicionados a `MedicLocale.cs` (índices 27/28, EN+PT), `BandAidUI.ShowTreatment` migrado para `MedicLocale.Get(...)`. Recompilado: 0 erros, mesmos 10 warnings pré-existentes.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-25 | Code review 01 criada via `/code-review`. 0🔴/0🟠/0🟡/1🟢. Item pronto para fechar sem `/apply-code-review` — o único achado é opcional e não altera o comportamento entregue. |
