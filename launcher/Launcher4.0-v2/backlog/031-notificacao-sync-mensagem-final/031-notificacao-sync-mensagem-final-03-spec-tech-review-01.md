# 031 — Notificações de sync · Review Técnica 01

**Mod:** Launcher4.0-v2 · **Data:** 2026-08-02 · **Spec:** [02-spec-tech](031-notificacao-sync-mensagem-final-02-spec-tech.md)

> Review adversarial por sub-agent independente (as 2 specs 031/032 contra o código real). Aplicado em `/g-autodev`.

## Resumo
> 🔴 0 · 🟡 2 · 🟢 3 · ✅ Resolvidos: 5

| ID | Impacto | Título | Status |
|---|---|---|---|
| PA-01-01 | 🟡 | `IsSyncRunning` não existe no `ModUpdateViewModel` → binding literal falha silencioso | ✅ Resolvido |
| PA-01-02 | 🟢 | Guard `_syncRunId` é redundante (FIFO+gate já garantem a ordem) — a causa real do print não é a corrida | ✅ Resolvido |
| PA-01-03 | 🟡 | Fechamento da barra precisa ser garantido no `finally` (erro/cancelamento) | ✅ Resolvido |
| PA-01-04 | 🟢 | `BuildSummary`: segmento `Errors` inalcançável no Profile (ramo `Errors>0` vem antes) | ✅ Resolvido |
| PA-01-05 | 🟡 | i18n: teste de parity precisa `AllowNullValues`; aposentar `update_completed` nos 4 lugares juntos | ✅ Resolvido |

## PA-01-01 · 🟡 ✅ — `IsSyncRunning` só existe no Profile
A §5.5 mandava a barra do ModUpdateView sob `IsVisible="{Binding IsSyncRunning}"`, mas o `ModUpdateViewModel` não tem essa prop (tem `IsBusy = IsChecking || IsUpdating`). Binding a prop inexistente → Avalonia usa `false` → barra some pra sempre. **Resolução:** §5.5 — ProfileView usa `IsSyncRunning`, ModUpdateView usa `IsBusy`; não copiar literal.

## PA-01-02 · 🟢 ✅ — Guard `_syncRunId` é cinto-e-suspensório
`Progress<T>` posta no mesmo `SynchronizationContext` em FIFO; a continuação (que faz `BuildSummary`) só é postada quando o `ExecuteAsync` retorna → todo report já foi drenado antes do resumo. O guard **nunca dispara**. Não é bug (inócuo, protege futuro `ConfigureAwait(false)`), mas o **fechamento real** vem da separação barra×status. ⚠️ A causa do print (barra presa em "Downloading") **não foi reproduzida** — provavelmente exceção antes da msg final (hip. 2), não a corrida. **Resolução:** R-2 reescrito; guard mantido como defensivo; causa confirmada só no gate in-game (`/g-diagnose`).

## PA-01-03 · 🟡 ✅ — Fechar no `finally`
Para a barra sumir mesmo em erro/cancelamento (não só no sucesso), `IsSyncRunning=false` vai no `finally` do run. **Resolução:** §5.5 (CR-01-05).

## PA-01-04 · 🟢 ✅ — `Errors` redundante no Profile
No Profile e no ModUpdate o ramo `result.Errors>0` (`update_completed_with_errors`) vem **antes** do ramo que chama `BuildSummary`, então o segmento `sync_seg_errors` é morto nesses fluxos. Mantido como **redundância defensiva** (BuildSummary é um helper geral; o `if (n>0)` não faz mal). Documentado.

## PA-01-05 · 🟡 ✅ — i18n parity
(a) O teste de parity precisa desserializar com `AllowNullValues: true` (senão `LoadClassWithoutSaving` devolve o objeto **inteiro** null ao 1º campo faltante e o teste não aponta a chave). (b) `update_completed` só é usada em `ModUpdateViewModel:339` (nenhum `.axaml`) — aposentar nos **4 lugares juntos** (property, GenerateDefaultLocale, 2 JSON). (c) `GenerateDefaultLocale` já é subconjunto incompleto — a garantia real são os 2 JSON completos + o teste. **Resolução:** §8 e R-1 atualizados.

## Confirmados corretos
`SyncProgress`+`Kind` (2 call-sites 4-arg, param opcional seguro); campos do `SyncResult` existem; `SyncMessages` em `SPT.Launcher`→`Base` compila (sem ciclo); `IsUpdateVisible` já não desliga no sucesso hoje (é o defeito), auditoria 453/520/653 completa.

## Histórico
| Data | Evento |
|---|---|
| 2026-08-02 | Review 01 (sub-agent adversarial). 0 🔴 · 2 🟡 · 3 🟢, todos aplicados na spec técnica no mesmo passo. |
