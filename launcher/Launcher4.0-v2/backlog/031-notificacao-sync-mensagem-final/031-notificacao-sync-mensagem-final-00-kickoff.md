# 031 — Notificação de sync mente e fica pendurada como mensagem final · Kickoff

**Launcher:** Launcher4.0-v2 (v2.7.3) · **Data:** 2026-07-28 · **Origem:** relato do usuário com print (2026-07-28) · **Deps:** 007 (motor de sync), 030 (canais novos)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Sintoma relatado

Print da tela logada (launcher em produção, locale EN): a barra de update mostra

```
Downloading: BepInEx/plugins/TRL-Fixes.dll (1/1)
```

como **última mensagem exibida**, e o processo não conclui visualmente — a barra fica pendurada nesse texto.

O que estava realmente acontecendo: `TRL-Fixes.dll` **não está mais no manifesto do servidor**, então a regra de espelho de `plugins/` está **movendo o arquivo para `plugins-disabled/`** (quarentena, `SyncActionKind.MoveToDisabled`). Nada estava sendo baixado.

## Dois defeitos distintos no mesmo print

### D-031.1 — A mensagem mente sobre a ação

O texto de progresso do apply é **um só** para todas as ações de I/O:

- [ProfileViewModel.cs:650-654](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L650-L654) — `applyProgress` formata sempre `LocalizationProvider.Instance.update_downloading`.
- [LocalizationProvider.cs:221](../../project/SPT.Launcher.Base/Helpers/LocalizationProvider.cs#L221) — `update_downloading = "Baixando: {0} ({1}/{2})"` (EN: `"Downloading: …"`).
- [SyncEngine.cs:85](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L85) — o único `progress?.Report(...)` reporta a fase genérica `"applying"` **antes** do `switch`, sem dizer qual `SyncActionKind` é.

Consequência: `MoveToDisabled` (quarentena), `DeleteExtra` (remoção), `SeedCopy`, `ForceCopy` e `OptionalConfigCopy` **todos** aparecem como "Baixando". Para o jogador, um arquivo sendo *removido* do cliente aparece como se estivesse sendo *baixado* — pior ainda quando o nome é um mod TRL, que soa como instalação em curso.

O `SyncProgress` já carrega a fase como string ([SyncPlan.cs:7-9](../../project/SPT.Launcher.Base/Sync/SyncPlan.cs#L7-L9)) — dá para propagar o `Kind` sem quebrar o contrato.

### D-031.2 — Fica como mensagem FINAL (não fecha o ciclo)

Pelo código, ao terminar o apply sem erro o texto deveria ser substituído por `update_completed_success` ([ProfileViewModel.cs:682-686](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L682-L686)). No print isso **não aconteceu**. Hipóteses a investigar (nenhuma provada ainda):

1. **Corrida de `Progress<T>`** — o callback de progresso é postado no dispatcher (assíncrono) enquanto a atribuição final é síncrona na continuação do `await`; um `Report` em voo pode pousar **depois** do texto final e sobrescrevê-lo. Vale para os dois `Progress<SyncProgress>` ([ProfileViewModel.cs:629](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L629) e [:650](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L650)) e para o `Dispatcher.UIThread.Post` do medidor de taxa ([:790](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L790)).
2. **Exceção/travamento dentro do `MoveWithOverwrite`** ([SyncEngine.cs:426-435](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L426-L435)) ou no `finally` (`_baseline.Save()` / `SyncReport.Write`) — a atribuição final nunca roda.
3. **A barra nunca some.** `IsUpdateVisible = true` é setado em [ProfileViewModel.cs:454](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L454) e **nunca volta a `false`** no caminho de sucesso (só em [:449](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L449), antes de começar). Mesmo com o texto certo, a barra fica pendurada na tela indefinidamente — não há estado de "concluído e sumiu".

### D-031.3 — O link "X file(s) were updated, see details" do run ANTERIOR não é limpo

**Pedido do usuário (2026-07-28):** ao clicar em "Verificar arquivos" (`VERIFY FILES`), o texto `"{0} file(s) were updated, see details"` deve ser **limpo**.

Hoje ele persiste: o link é `LastUpdateText` / `HasLastUpdate` ([ProfileView.axaml:222-225](../../project/SPT.Launcher/Views/ProfileView.axaml#L222-L225)), preenchido por `SetLastUpdate` ([ProfileViewModel.cs:854-858](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L854-L858)) em dois momentos:

1. no carregamento da tela, lendo o `last-update.json` do run **anterior** ([:846](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L846));
2. no fim do sync, com o resultado do run atual ([:669](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L669)).

Nenhum dos caminhos de **início** de verificação o limpa — nem `ForceCheckForUpdates` (o comando dos botões `UpdateModsCommand`/`VerifyFilesCommand`, [:378-396](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L378-L396)), nem o bloco de reset do começo do run, que já zera texto de status, progresso e taxa mas **não** o `LastUpdateText` ([:453-459](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L453-L459)).

Efeito: durante toda a nova verificação o jogador vê o placar do run passado, e o link abre um `last-update.json` que ainda descreve o run anterior — informação obsoleta apresentada como se fosse do run em curso. É a mesma classe de defeito de D-031.2 (estado de sync que não fecha o ciclo).

**Observação adjacente (não é requisito, decidir na spec):** `SetLastUpdate` recebe **só** `result.Updated` e faz `HasLastUpdate = updatedCount > 0`. No cenário do próprio print — único evento foi mover `TRL-Fixes.dll` para `plugins-disabled` — `Updated == 0`, então o link **some** justamente no run em que houve mudança relevante no cliente. Se o link é a porta de entrada do relatório, o critério de visibilidade provavelmente deveria olhar o total de ações (incluindo `MovedToDisabled`/`Deleted`/`Forced`), não só downloads.

## Direções para a spec

- **Mensagem por tipo de ação**: propagar o `SyncActionKind` no `SyncProgress` e ter string própria por ação (baixando / removendo / movendo para desabilitados / restaurando config / aplicando config). Strings novas no `LocalizationProvider` (PT + EN).
- **Vocabulário do jogador**: "movendo para desabilitados" é jargão interno. Decidir a frase que explica *por que* (`mod removido do servidor — arquivado em plugins-disabled`).
- **Fechar o ciclo**: garantir que a mensagem final sempre vence a última de progresso (ex.: sequenciar a atribuição final no dispatcher, ou versionar/ignorar reports após o fim do run) **e** definir o que acontece com a barra depois — some após N segundos, vira linha de resumo estática, ou fica com o link do relatório.
- **Reset de estado no início do run** (D-031.3): um ponto único que zere `UpdateStatusText`, `UpdateProgress`, taxa **e** `LastUpdateText`/`HasLastUpdate` — hoje o reset existe mas é incompleto ([:453-459](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L453-L459)). Cobrir os dois gatilhos (botão manual e auto-check do login) e o caso de sync abortado logo no começo (manifesto falhou → o texto antigo deve continuar limpo, não ressuscitar).
- **Corner cases**: run cancelado, run com erro, plano só com ações não-I/O (preservados), sync disparado por `PendingApply` do item 030 logo após a tela "Mods e Configs".
- Reproduzir primeiro (`/g-diagnose`): remover um plugin do manifesto do servidor e rodar "Verificar arquivos" com apenas essa ação no plano, confirmando qual das 3 hipóteses de D-031.2 é a real antes de escrever a solução.

## Evidência de apoio

- Print anexado ao relato (2026-07-28) — barra de update da `ProfileView`, locale EN, `(1/1)`.
- Relatório do run: `SPT\user\launcher\last-update.json` — a linha de `TRL-Fixes.dll` deve estar com `action: "moved-to-disabled"`, confirmando que foi quarentena e não download.
