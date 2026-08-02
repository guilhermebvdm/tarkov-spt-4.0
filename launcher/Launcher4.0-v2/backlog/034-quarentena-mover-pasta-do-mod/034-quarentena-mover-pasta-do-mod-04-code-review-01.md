# 034 — Quarentena move a pasta do mod inteira · Code Review 01

**Mod:** Launcher4.0-v2
**Data:** 2026-08-02
**Escopo:** implementação em `SPT.Launcher.Base/Sync/` (SyncPlanner, SyncEngine, SyncAction, SyncPlan) + testes.

> Review adversarial por dois sub-agents independentes de contexto limpo (lente de correção · lente de regressão+cobertura). Aplicado em modo `/g-autodev` — achados corrigidos e re-testados no mesmo passo. Suíte: **271/271 verdes**.

## Resumo

> 🔴 Bloqueadores: 2 · 🟡 Importantes: 4 · 🟢 Menores: 3 · ✅ Resolvidos: 9 · Total: 9

## Índice

| ID | Cat · Impacto | Título | Status |
|---|---|---|---|
| CR-01-01 | A · 🔴 | TOCTOU: consolidação varre download futuro de mandatório ausente → loop infinito | ✅ Resolvido |
| CR-01-02 | C · 🔴 | CA-034.7 (relatório 1 entrada agregada) sem teste | ✅ Resolvido |
| CR-01-03 | C · 🟡 | CC-10 (subpastas aninhadas) sem teste — `DeriveFolderDisabledTarget` depth>1 | ✅ Resolvido |
| CR-01-04 | C · 🟡 | CC-6 (faxina nunca desce em `-disabled`) sem teste | ✅ Resolvido |
| CR-01-05 | C · 🟡 | CC-7 (casing preservado) sem teste | ✅ Resolvido |
| CR-01-06 | B · 🟡 | Regressão de cobertura: colisão per-file (R3.3) perdeu teste dedicado | ✅ Resolvido |
| CR-01-07 | D · 🟢 | Faxina aborta o root inteiro numa falha de um subdir | ✅ Resolvido |
| CR-01-08 | E · 🟢 | CA-034.4 não assertava `MoveCount` (per-file vs zero-ação) | ✅ Resolvido |
| CR-01-09 | F · 🟢 | Faxina roda todo sync (CC-2) + symlink cíclico | ✅ Aceito/documentado |

---

## Pontos

### CR-01-01 · A — Bug latente · 🔴 Bloqueador ✅ Resolvido

**TOCTOU: a consolidação varre para a quarentena o download futuro de um arquivo mandatório ausente no disco**

**Problema:** `FileMustStay` só era chamado sobre os arquivos que o `Directory.EnumerateFiles` enxerga **no disco no momento do plano**. Um arquivo que é entrada de manifesto que FICA (mandatório ou optional ligado) mas está **ausente no disco** (apagado, instalação parcial, download anterior falhou) não era enumerado → a checagem que bloquearia a consolidação nunca rodava para ele → a pasta consolidava. Na execução, o `Download` desse arquivo roda **antes** do `MoveDirToDisabled` (ordem do plano) e o grava na pasta; o move de pasta então o leva para `-disabled/`. Pior: o loop de remoção de baseline do `MoveDirToDisabled` apaga o baseline do arquivo recém-baixado → no próximo sync ele está "missing" de novo → **loop infinito, silencioso, e a variante aninhada derruba um mod LIGADO**.

**Por que importa:** perda de garantia + loop infinito de sync; um mod mandatório nunca fica instalado.

**Resolução:** `ConsolidateFolderMoves` passou a receber `manifestFiles` e computar `blockedFolders` — pastas de 1º nível que contêm **alguma** entrada de manifesto que fica (`!optional || IsOptionalModEnabled`), independentemente da presença no disco. Grupo cuja pasta está em `blockedFolders` não consolida (cai per-file). Teste: `Folder_with_missing_mandatory_file_does_not_consolidate`.

### CR-01-02 · C — Gap vs spec · 🔴 Bloqueador ✅ Resolvido

**CA-034.7 (relatório = 1 entrada agregada) não tinha teste**

**Problema:** nenhum teste inspecionava `result.Entries`/`last-update.json`. `Assert.Equal(1, plan.MoveDirCount)` prova 1 ação no plano, não 1 entrada no relatório (geração separada, `SyncEngine`); um bug que emitisse N entradas passaria.

**Resolução:** `Folder_move_emits_single_aggregated_report_entry_and_cleanup_is_silent` — mod-pasta de 3 arquivos → exatamente 1 entry `moved-to-disabled` com `path` = a pasta; e a faxina não gera nenhuma entrada (`GhostMod` ausente das entries).

### CR-01-03 · C — Gap vs spec · 🟡 Importante ✅ Resolvido

**CC-10 (subpastas aninhadas) sem teste — `DeriveFolderDisabledTarget` em depth>1 sem exercício**

**Resolução:** `Nested_folder_mod_moves_whole_tree_without_orphan` — mod-pasta com arquivos em depth 1, 3 e 4 (todos convergindo para o mesmo alvo de pasta), assert do destino aninhado preservado e origem sem órfã.

### CR-01-04 · C — Gap vs spec · 🟡 Importante ✅ Resolvido

**CC-6 (faxina nunca desce em `-disabled`) sem teste**

**Resolução:** `Cleanup_skips_disabled_segment_folders_under_root` — pasta vazia `plugins/mod-disabled/EmptyInside` sobrevive à faxina (guard `EndsWith("-disabled")`).

### CR-01-05 · C — Gap vs spec · 🟡 Importante ✅ Resolvido

**CC-7 (casing on-disk preservado) sem teste**

**Resolução:** `Divergent_casing_still_consolidates_and_removes_source` — disco `PiP-Disabler`, manifesto `pip-disabler` → agrupa numa chave só (`MoveDirCount == 1`), origem removida sem duplicata.

### CR-01-06 · B — Bug latente (cobertura) · 🟡 Importante ✅ Resolvido

**Colisão per-file (R3.3) perdeu teste dedicado**

**Problema:** os dois testes de engine que exercitavam a colisão no destino via `MoveWithOverwrite` (per-file) agora passam por `MoveDirToDisabled`/merge (pastas de 1 arquivo consolidam). O caminho `MoveWithOverwrite` com colisão ficou sem cobertura dedicada.

**Resolução:** `PerFile_move_overwrites_colliding_target_in_disabled` (pasta compartilhada → per-file → colisão no `-disabled` → o recém-movido vence). Comentários dos dois testes de engine realinhados para apontar o novo caminho e o teste que re-fixa o R3.3 per-file.

### CR-01-07 · D — Arquitetura · 🟢 Menor ✅ Resolvido

**A faxina abortava a limpeza do root inteiro numa falha de um subdiretório**

**Resolução:** `RemoveEmptyDirsBottomUp` envolve o `Directory.Delete` num `try/catch` por-diretório — pasta em uso/sem permissão deixa a casca, sem abortar o resto nem o sync.

### CR-01-08 · E — Legibilidade · 🟢 Menor ✅ Resolvido

**CA-034.4 não assertava `MoveCount`** — adicionado `Assert.Equal(1, plan.MoveCount)` (prova que caiu para per-file, não zero-ação).

### CR-01-09 · F — Opcional · 🟢 Aceito/documentado

**Faxina roda em todo sync + symlink cíclico**

A faxina apaga qualquer pasta vazia sob `plugins/`/`patchers/` em todo sync fora do Dev Mode — um mod ligado que dependa de uma pasta vazia (ex.: `Logs/`) a perderia. Risco **explicitamente aceito** na spec funcional (CC-2). Symlink/junction cíclico sob `plugins/` causaria `StackOverflowException` não-capturável, mas o domínio SPT não distribui isso — nota defensiva, sem ação. Ambos ficam como gate de validação in-game.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Code review 01 (2 sub-agents adversariais). 2 🔴 + 4 🟡 + 3 🟢. Os 6 acionáveis corrigidos e re-testados no mesmo passo (`/g-autodev`); 1 🟢 aceito/documentado. Suíte 271/271 verde. |
