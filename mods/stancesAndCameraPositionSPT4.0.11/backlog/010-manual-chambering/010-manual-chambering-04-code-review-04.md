# 010 — Manual Chambering, Dry Rack & Manual Pump · Code Review 04

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [010-manual-chambering-01-spec.md](010-manual-chambering-01-spec.md)
**Spec técnica:** [010-manual-chambering-02-spec-tech.md](010-manual-chambering-02-spec-tech.md)
**Asbuild / Correção prévia:** [010-manual-chambering-06-fix-01.md](010-manual-chambering-06-fix-01.md)
**Data:** 2026-09-04

> Análise crítica do código implementado em `modded-testchannel/` (v2.19.1) para suporte a Dry Rack, Dry Fire Feedback, inibição de Empty Reload, Manual Bolt Action, Manual Pump Action e limpeza de resíduo 3D na câmara. Cada achado recebe um ID `CR-04-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| **CR-04-01** | B — Bug latente | 🔴 Bloqueador | `ManualBoltActionPatch` e correlatos não verificavam `_EnableManualPumpAction` | ✅ Aplicado |
| **CR-04-02** | B — Bug latente | 🟠 Forte | `DryFireFeedbackPatch` com retorno `void` não suprimia cliques secos repetidos | ✅ Aplicado |
| **CR-04-03** | B — Bug latente | 🟡 Médio | `CleanResidualChamberModel` atuava apenas no índice 0 | ✅ Aplicado |
| **CR-04-04** | D — Arquitetura | 🟡 Médio | Supressão de `BoltCatch` sem verificar `MustBoltBeOpennedForExternalReload` | ✅ Aplicado |
| **CR-04-05** | E — Legibilidade | 🟢 Menor | Ausência de alias semântico para `Player.FirearmController.GClass2037` no cabeçalho | ✅ Aplicado |

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

## Pontos

### CR-04-01 · Cat B — Bug latente · 🔴 Bloqueador (✅ Aplicado em 2026-09-04)

**`ManualBoltActionPatch` e correlatos não verificam `_EnableManualPumpAction`, quebrando pump isolado**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:630, 669, 701`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L630)

**Problema:**
As classes de patch que suprimem a repetição automática pós-tiro (`ManualBoltActionPatch.Prefix`, `ManualBoltActionStartPatch.Postfix` e `ManualBoltActionNetPatch.Prefix`) verificavam exclusivamente `_EnableManualBoltAction.Value`. Se o jogador habilitasse no F12 apenas `Enable Manual Pump Action = true`, as guardas abortavam e a escopeta pump continuava ciclando automaticamente ao soltar o mouse.

**Por que importa:**
Quebrava a independência funcional entre as opções do menu F12, impedindo o uso exclusivo de pump manual com snipers automáticas.

**Sugestão:**
Checar a flag correspondente ao tipo de arma em mãos (`IsPumpActionShotgun(fc.Weapon) ? _EnableManualPumpAction : _EnableManualBoltAction`).

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Aplicado em `ManualChamberingPatches.cs` (linhas 625, 665, 700). As três classes de patch agora identificam se a arma em mãos é pump action ou bolt action e consultam a respectiva configuração do F12.

---

### CR-04-02 · Cat B — Bug latente · 🟠 Forte (✅ Aplicado em 2026-09-04)

**`DryFireFeedbackPatch` com retorno `void` não suprime cliques secos repetidos (gatilho inerte)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:917-949`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L917-L949)

**Problema:**
O patch `DryFireFeedbackPatch.Prefix` foi implementado com retorno `void`. Embora fizesse `fc.Weapon.Armed = false`, ele não interrompia a execução de `SetTriggerPressed`, permitindo que cliques adicionais continuassem acionando `FirearmController_0.DryShot()`.

**Por que importa:**
A simulação de "gatilho morto" ficava ineficaz, soando cliques repetidos do percussor caindo no vazio a cada clique do mouse.

**Sugestão:**
Alterar o Prefix para retornar `bool` e retornar `false` quando `!fc.Weapon.Armed`.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Assinatura alterada para `bool Prefix(...)`. Se a câmara estiver vazia e `fc.Weapon.Armed == false`, o método original vanilla é suprimido (`return false`), garantindo que apenas o primeiro clique desmonte o percussor com feedback sonoro e cliques subsequentes fiquem inertes até novo ciclo de ferrolho.

---

### CR-04-03 · Cat B — Bug latente · 🟡 Médio (✅ Aplicado em 2026-09-04)

**`CleanResidualChamberModel` atua apenas no índice 0 e não limpa armas multi-barrel**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:58-85`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L58-L85)

**Problema:**
`CleanResidualChamberModel` invocava `wm.DestroyPatronInWeapon(0)` e processava apenas `wm.Transform_0[0]`. Em espingardas de canos múltiplos (ex: MP-43 de cano duplo) com câmaras vazias, a câmara secundária não era limpa.

**Por que importa:**
Poderia deixar resíduo 3D de cartucho na segunda câmara em armas multi-barrel.

**Sugestão:**
Utilizar `wm.DestroyAllPatronsInWeapon()` e iterar por todos os `Transform` de `wm.Transform_0`.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** `CleanResidualChamberModel` agora chama `wm.DestroyAllPatronsInWeapon()` e itera em laço por todos os índices de `wm.Transform_0`, limpando GameObjects filhos com `AssetPoolObject.ReturnToPool` defensivamente para qualquer número de câmaras.

---

### CR-04-04 · Cat D — Arquitetura · 🟡 Médio (✅ Aplicado em 2026-09-04)

**Supressão de `BoltCatch` sem verificar `MustBoltBeOpennedForExternalReload` em armas com ferrolho aberto**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:495, 525, 578`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L495)

**Problema:**
Em `StartReloadResetPatch` e `ReloadMagInsertedPatch`, o código forçava `FirearmsAnimator_0.SetBoltCatch(false)` incondicionalmente em recargas com câmara vazia, o que poderia forçar o fechamento do ferrolho antes da hora em armas do tipo *Open Bolt*.

**Por que importa:**
Risco de glitch visual de pose em armas com trava de ferrolho obrigatória aberta.

**Sugestão:**
Condicionar `SetBoltCatch(false)` a `!fc.Weapon.MustBoltBeOpennedForExternalReload`.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Adicionada guarda `if (!fc.Weapon.MustBoltBeOpennedForExternalReload)` antes de forçar `SetBoltCatch(false)` em `StartReloadResetPatch` (Prefix e Postfix) e em `ReloadMagInsertedPatch`.

---

### CR-04-05 · Cat E — Legibilidade · 🟢 Menor (✅ Aplicado em 2026-09-04)

**Ausência de alias semântico para `Player.FirearmController.GClass2037` no cabeçalho**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:18-26, 839, 917`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L18-L26)

**Problema:**
Reflection inline utilizava diretamente o nome ofuscado `typeof(Player.FirearmController.GClass2037)` sem alias semântico correspondente no topo do arquivo.

**Por que importa:**
Alinhamento com as diretrizes de Readiness 4.1 e legibilidade do repositório.

**Sugestão:**
Adicionar `using IdleWeaponOpClass = EFT.Player.FirearmController.GClass2037;`.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Adicionado `using IdleWeaponOpClass = EFT.Player.FirearmController.GClass2037;` no bloco de mapeamento semântico do EFT 0.16.9 e atualizados os patches `ChamberCheckModelCleanupPatch` e `DryFireFeedbackPatch`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-04 | Code review 04 criada via `/code-review` analisando implementação em `modded-testchannel/` (v2.19.0). |
| 2026-09-04 | Todos os 5 achados (CR-04-01 a CR-04-05) aplicados e validados no código C#; build Release compilado limpo (0 erros, 0 avisos) com bump SemVer para v2.19.1. |
