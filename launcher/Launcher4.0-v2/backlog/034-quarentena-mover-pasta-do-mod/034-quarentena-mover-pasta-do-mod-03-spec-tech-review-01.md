# 034 — Quarentena move a pasta do mod inteira · Review Técnica 01

**Mod:** Launcher4.0-v2
**Spec técnica revisada:** [034-quarentena-mover-pasta-do-mod-02-spec-tech.md](034-quarentena-mover-pasta-do-mod-02-spec-tech.md)
**Data:** 2026-08-02

> Análise crítica adversarial da spec técnica (revisor humano + sub-agent independente de contexto limpo). Todos os membros referenciados foram confirmados no código real (nenhum método fantasma). Aplicada em modo autônomo (`/g-autodev`) — decisões com default defensável tomadas e registradas.

## Resumo

> 🔴 Bloqueadores: 2 · 🟡 Importantes: 1 · 🟢 Menores: 3 · ✅ Resolvidos: 6 · Total: 6

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 | `FileMustStay` ignora a regra do arquivo → arrasta config preservada/forçada | ✅ Resolvido |
| PA-01-02 | B — Edge Case | 🟡 | Grupo de origem mista + `kv.Value[0]` → destino arbitrário / 2ª entrada por casing | ✅ Resolvido |
| PA-01-03 | C — Erro de Lógica | 🔴 | `MoveDirectoryMerge` muta árvore durante enumeração lazy + delete recursivo → perda de dados | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟢 | Local da faxina ambíguo (§2 × §5.4) + comportamento sob cancelamento não travado | ✅ Resolvido |
| PA-01-05 | A — Gap | 🟢 | Rótulo humano do relatório não ajustado; contador mistura arquivos+pastas | ✅ Resolvido |
| PA-01-06 | B — Edge Case | 🟢 | `EmptyDirCleanupRoots` recebe roots redundantes/inexistentes | ✅ Resolvido |

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador ✅ Resolvido em 2026-08-02

**`FileMustStay` ignora a `SyncFolderRule` do arquivo — arrasta arquivos de canais de preservação/deleção para a quarentena**

**Problema:** `FileMustStay` (spec §5.2) checa coop-safe/ignored/excluded/protected/-disabled/manifest, mas **não** checa a regra resolvida do arquivo. O `ScanExtras` real pula explicitamente todo arquivo cuja regra é `PreserveDivergent`/`MirrorReference`/`ForceToConfig`/`OptionalConfigToConfig` ([SyncPlanner.cs:522-533](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L522)) — esses nunca recebem `MoveToDisabled` e não entram no set `moving`. Um arquivo desses fisicamente sob `plugins/X/` (servidor mapeando, via `folderRules`, um subprefixo `preserve-divergent`/`force`) retorna `false` em `FileMustStay` → a pasta é consolidada e o arquivo é **arrastado para a quarentena**, quebrando a garantia do canal.

**Por que importa:** perda de garantia real — uma config `preserve-divergent` customizada pelo jogador viraria quarentenada; um arquivo `mirror-delete` que deveria ser deletado seria ressuscitado sob `-disabled`.

**Sugestão / Resolução (aceita):** adicionar em `FileMustStay`, antes do return final: `if (_resolver.Resolve(norm, out _) != SyncFolderRule.MirrorMoveDisabled) return true;` — espelha o skip do `ScanExtras` e fecha o buraco para todas as regras não-quarentena de uma vez. Aplicado na §5.2.

**Decisão:** `[x]` Aceitar sugestão

---

### PA-01-02 · B — Edge Case · 🟡 Importante ✅ Resolvido em 2026-08-02

**Grupo de pasta com origens mistas + `kv.Value[0]` → namespace de destino arbitrário; casing divergente → 2ª entrada agregada**

**Problema:** `QuarantineDisabledOptionalMods` (origem `Optional` → `plugins-disabled/optional/X/…`) roda antes de `ScanExtras` (origem `MirrorExtra` → `plugins-disabled/X/…`) ([SyncPlanner.cs:464](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L464) vs `466`). Uma pasta com um arquivo catalogado-desligado **e** um extra gera duas `MoveToDisabled` de namespaces diferentes no mesmo grupo; `var sample = kv.Value[0]` (spec §5.2) escolhe o destino por ordem arbitrária. Além disso, agrupar pela string original-case faz o **mesmo folder físico com casing divergente** (manifesto × disco) cair em duas chaves `Ordinal` distintas → duas `MoveDirToDisabled` → 2ª entrada `moved-to-disabled`, violando CA-034.7.

**Por que importa:** viola o isolamento de namespace D-14 e a garantia "1 entrada agregada"; pode causar overwrite de homônimo no merge que o per-file não causaria.

**Sugestão / Resolução (aceita):** (a) agrupar pela **chave normalizada** (`SyncPathUtil.Normalize`), resolvendo o casing do RelativePath via `ResolveOnDiskCasing` (CC-7); (b) derivar o destino de pasta de **cada** ação do grupo e só consolidar se todas convergem para o mesmo destino (`Distinct().Count() == 1`) — origens mistas caem para per-file (conservador; a faxina limpa a casca). Aplicado na §5.2.

**Decisão:** `[x]` Aceitar sugestão (default defensável — per-file conservador quando misturado)

---

### PA-01-03 · C — Erro de Lógica · 🔴 Bloqueador ✅ Resolvido em 2026-08-02

**`MoveDirectoryMerge` (ramo CC-4) move durante enumeração lazy + `Directory.Delete(recursive:true)` → perda de dados**

**Problema:** spec §5.3 (ramo merge, destino já existe) faz `File.Move` de cada arquivo **durante** um `Directory.EnumerateFiles(source, AllDirectories)` lazy e depois `Directory.Delete(source, recursive:true)`. Mutar a árvore enumerada é indefinido no .NET: um arquivo pode ser pulado pelo enumerador, e o `Delete(recursive:true)` no fim **apaga** esse arquivo pulado — perda silenciosa dentro de uma quarentena que a spec afirma não-destrutiva (contradiz CC-8 e o ✅ "não-destrutivo" da §9).

**Por que importa:** perda de dados silenciosa no 2º desligamento do mesmo mod (destino de quarentena já existe).

**Sugestão / Resolução (aceita):** materializar `.ToList()` antes do loop de move; e trocar o delete cego por remoção **guardada** — só apagar `source` se `!Directory.EnumerateFiles(source, AllDirectories).Any()` (nenhum arquivo restante). Aplicado na §5.3.

**Decisão:** `[x]` Aceitar sugestão

---

### PA-01-04 · A — Gap · 🟢 Menor ✅ Resolvido em 2026-08-02

**Local da faxina ambíguo entre §2 e §5.4; comportamento sob cancelamento não travado**

**Problema:** a §2 dizia "antes do `finally` [:346]" e a §5.4 "no fim do `try`, depois do foreach" — pontos ligeiramente diferentes; a spec não travava se a faxina roda quando o sync é **cancelado**.

**Por que importa:** ambiguidade de implementação. (Comportamento correto: dentro do `try`, após o foreach → pulada no cancelamento, o que é seguro: ação cancelada deixa o arquivo na origem, então a pasta não fica vazia.)

**Sugestão / Resolução (aceita):** fixar "**dentro do `try`, após o `foreach`**; não roda em cancelamento (estado parcial)". Alinhado §2 e §5.4.

**Decisão:** `[x]` Aceitar sugestão

---

### PA-01-05 · A — Gap · 🟢 Menor ✅ Resolvido em 2026-08-02

**Rótulo humano do relatório não é ajustado; `MovedToDisabled` passa a misturar arquivos e pastas**

**Problema:** R-6 promete "alinhar o texto humano", mas nenhum stub altera `SyncReport` — a entrada da pasta reusa `"moved-to-disabled"` (texto orientado a arquivo). E `result.MovedToDisabled++` conta a pasta como 1, então o `Summary` subconta arquivos.

**Por que importa:** consistência do relatório; menor porque o refino do texto do relatório é do **item 031** (correlato).

**Sugestão / Resolução (aceita):** manter o label `moved-to-disabled` (1 entrada agregada = decisão CA-034.7) e **documentar** que o refino do texto humano ("mod X movido para a quarentena") + a distinção de contadores pasta×arquivo pertencem ao item 031. Sem novo label no 034. Ajustado R-6.

**Decisão:** `[x]` Aceitar sugestão (refino de texto deferido ao 031)

---

### PA-01-06 · B — Edge Case · 🟢 Menor ✅ Resolvido em 2026-08-02

**`EmptyDirCleanupRoots` recebe roots redundantes/inexistentes (`plugins` + `bepinex/plugins`)**

**Problema:** `MirrorPrefixes` inclui `plugins`/`patchers` **e** `bepinex/plugins`/`bepinex/patchers` ([SyncRuleResolver.cs:46-49](../../project/SPT.Launcher.Base/Sync/SyncRuleResolver.cs#L46)) → a faxina adiciona 4 roots; num install real 2 não existem (sem dano — o engine faz `Directory.Exists → continue`, mas é ruído).

**Sugestão / Resolução (aceita):** ao popular, filtrar por `Directory.Exists` do caminho resolvido + dedup por `Contains`. Aplicado na §5.2 (bloco de população).

**Decisão:** `[x]` Aceitar sugestão

---

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Review 01 via `/review-technical-spec` (revisor + sub-agent adversarial independente). 2 🔴 + 1 🟡 + 3 🟢, todos aceitos e aplicados na spec técnica no mesmo passo (modo `/g-autodev`). Nenhum método fantasma. |
