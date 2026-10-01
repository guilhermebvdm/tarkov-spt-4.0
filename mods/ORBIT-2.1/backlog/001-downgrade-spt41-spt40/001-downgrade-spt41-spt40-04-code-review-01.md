# 001 — Downgrade SPT 4.1 → 4.0 · Code Review 01

**Mod:** ORBIT-2.1
**Spec funcional:** [001-downgrade-spt41-spt40-01-spec.md](001-downgrade-spt41-spt40-01-spec.md)
**Spec técnica:** [001-downgrade-spt41-spt40-02-spec-tech.md](001-downgrade-spt41-spt40-02-spec-tech.md)
**Asbuild:** [001-downgrade-spt41-spt40-05-asbuild.md](001-downgrade-spt41-spt40-05-asbuild.md)
**Data:** 2026-09-30

> Análise crítica do código do port. Consolida quatro frentes independentes: a execução dos patches fora do jogo (`scripts/verify-port.sh`), e três revisores de contexto limpo, que não escreveram o código — reflexão por texto, patches Harmony e equivalência de membros, e servidor. Cada achado tem ID `CR-01-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 9 · ⏭️ Aceitos sem mudança: 5 · Total: 14

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | A — Crítico | 🔴 | Ghost Mode de bots nativos se desligava sozinho: campo `_owner` e 5 métodos buscados pelo nome 4.1 | ✅ Aplicado |
| CR-01-02 | A — Crítico | 🔴 | Painel web abria morto e sem estilo: o hospedeiro 4.0 não carrega o MudBlazor | ✅ Aplicado |
| CR-01-03 | B — Bug latente | 🔴 | 17 camadas de IA comparadas pelo nome de classe 4.1 | ✅ Aplicado |
| CR-01-04 | B — Bug latente | 🟠 | `_lootingNow` não existe no 4.0: bot nativo com saque real nunca dormia | ✅ Aplicado |
| CR-01-05 | B — Bug latente | 🟠 | ZIP de preset com caminho `SPT_Runtime/user/mods/ORBIT/` | ✅ Aplicado |
| CR-01-06 | B — Bug latente | 🟡 | `_shallStartInteract`: três scripts de ponto de reserva mantinham o bot acordado | ✅ Aplicado |
| CR-01-07 | B — Bug latente | 🟡 | `NativePatrolArrivalDiagnosticPatch`: alvo `IsCome()` inexistente e campo `____owner` | ✅ Aplicado |
| CR-01-08 | B — Bug latente | 🟡 | `NativeGlukharChoiceDiagnosticPatch`: campo `____owner` inexistente | ✅ Aplicado |
| CR-01-09 | E — Manutenção | 🟢 | Três campos de diagnóstico de patrulha lidos pelo nome 4.1 | ✅ Aplicado |
| CR-01-10 | F — Opcional | 🟢 | `DormantBoarAvoidDangerBypassPatch` cai no mesmo método do patch irmão | ⏭️ Aceito |
| CR-01-11 | E — Manutenção | 🟢 | Diagnóstico de granada do SAIN fica inerte (classe não existe no SAIN 4.8.0) | ⏭️ Aceito |
| CR-01-12 | B — Bug latente | 🟡 | Corpo de jogador humano remoto não gera ponto de corpo no host (Fika) | ⏭️ Aceito — igual no upstream |
| CR-01-13 | F — Opcional | 🟢 | `POST /orbit/zones/native-floors` sem corpo lança exceção | ⏭️ Aceito |
| CR-01-14 | F — Opcional | 🟢 | Link "Server home" leva à página de agradecimento; não há cartão de mod no 4.0 | ⏭️ Aceito |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · A — Crítico · 🔴 · ✅ Aplicado em 2026-09-30

**Ghost Mode de bots nativos se desligava sozinho**

**Local:** [`modded/Orbit/Patches/NativeGhostBodyPatches.cs:59-84`](../../modded/Orbit/Patches/NativeGhostBodyPatches.cs#L59)

**Problema:** `Bind` buscava o campo `"_owner"` e os métodos `"TryPassCurrentDoor"`, `"RunEnteringDoorSequence"`, `"WaitForDoorOpen"`, `"InteractionWithDoor"` e `"ApplyToSelf"` por texto. São nomes do 4.1. No 4.0 o campo é `BotOwner_0` (`GClass429.cs:8`, `GClass177-1.cs:8`) ou `Owner` (`BotDoorOpener.cs:250`) e os métodos são `method_2`, `method_7`, `method_0`, `method_8` (`BotDoorOpener.cs`) e `method_3` (`BotFirstAidClass.cs:447`).

**Por que importa:** a primeira chamada lançava `MissingFieldException`, `Enable` desfazia todos os patches e `Ready` ficava `false`. `NativeGhostSystem.CanSleep` recusava então todo bot nativo. O log dizia `NATIVE GHOST: body guards unavailable, native sleep disabled`. O compilador não acusa.

**Como foi achado:** execução dos patches fora do jogo; confirmado de forma independente pelos revisores de reflexão (N1–N3) e de patches (M-01).

**Resolução:** `Compat/Spt40Members.cs` com `OwnerField(type)` e `Method(type, nome 4.1)`; `Bind` consulta os dois. Os cinco nomes 4.0 foram escolhidos por dois leitores independentes do decompile, que chegaram ao mesmo par.

**Verificação:** `verify-port.sh` → `NativeGhostBodyPatches Ready, 33 Harmony.Patch calls bound`, log `body guards ready (32 entry points)`. Mutação M2 (trocar `method_2` por `method_99`) faz a verificação falhar.

**Ressalva:** os cinco pares de método não têm prova do lado 4.1 (não há assembly 4.1 na máquina); a equivalência vem do corpo do método 4.0.

---

### CR-01-02 · A — Crítico · 🔴 · ✅ Aplicado em 2026-09-30

**Painel web abria morto e sem estilo**

**Local:** [`modded/Orbit.Server/Web/Shared/MainLayout.razor:10-27`](../../modded/Orbit.Server/Web/Shared/MainLayout.razor#L10)

**Problema:** o documento raiz do hospedeiro 4.0 (`references/spt-source/Libraries/SPTarkov.Server.Web/Components/App.razor`) só carrega `blazor.web.js`. No 4.0 cada layout traz o CSS e o JS do MudBlazor (`Components/Layout/BaseMudBlazorLayout.razor:3-6,15`). O layout do ORBIT não trazia nenhum.

**Por que importa:** as 13 páginas respondiam 200 e renderizavam no servidor, mas no navegador o circuito caía no primeiro render interativo com `Could not find 'mudElementRef.getBoundingClientRect'`. Nenhum clique chegava ao servidor e a página ficava sem estilo. Compilar e obter HTTP 200 não mostravam o defeito.

**Como foi achado:** teste no navegador contra o servidor 4.0.13 real; achado em paralelo pelo revisor do servidor (A1).

**Resolução:** `<HeadContent>` com a folha de estilo e a fonte, e `<script>` do MudBlazor, no `MainLayout.razor`.

**Verificação:** no navegador, contra o `SPT.Server` 4.0.13: navegação pelo menu, switch "Vanilla scavs" abre a caixa "Disable ORBIT on these bots?" (que passa pela ponte `ShowMessageBoxAsync`), Cancel devolve o switch, alterar "Squad rally" + Save mostra "Custom saved" e `/orbit/config` passa a devolver `squad_rally: false`. `server-smoke-test.ps1` confere em cada página a presença dos dois arquivos.

---

### CR-01-03 · B — Bug latente · 🔴 · ✅ Aplicado em 2026-09-30

**17 camadas de IA comparadas pelo nome de classe 4.1**

**Local:** [`modded/Orbit/Systems/NativeGhostPartisan.cs:9`](../../modded/Orbit/Systems/NativeGhostPartisan.cs#L9) e os sete arquivos que comparam o resultado.

**Problema:** `Layer(bot)` devolvia `CurLayerInfo.GetType().Name`, comparado com `"FollowerPatrolLayer"`, `"PatrolAssaultLayer"` e outros 15 nomes 4.1. No 4.0 o nome em tempo de execução é `GClass86`, `GClass133` etc.

**Por que importa:** toda decisão do Ghost Mode para bots nativos baseada na camada ativa (patrulha, cobertura, marksman, Zryachiy, Partisan, Black Division) seria falsa, em silêncio.

**Resolução:** `Compat/Spt40TypeNames.cs` mapeia o tipo 4.0 para o nome 4.1; `Layer(bot)` passa por ele. Os literais nos sete arquivos ficam como no upstream.

**Verificação:** `check-type-name-literals.py` → 21 literais, 17 cobertos e 4 ignorados com motivo. Mutação M4 (remover a linha de `FollowerPatrolLayer`) faz o script falhar. Os 17 pares foram conferidos contra o literal de `Name()` de cada classe e por dois revisores.

---

### CR-01-04 · B — Bug latente · 🟠 · ✅ Aplicado em 2026-09-30

**`_lootingNow` não existe no 4.0**

**Local:** [`modded/Orbit/Systems/NativeGhostLoot.cs:18`](../../modded/Orbit/Systems/NativeGhostLoot.cs#L18)

**Problema:** o leitor do campo ficava `null`; `BodyReason` devolvia `"native-loot-operation"` para todo bot com `USE_REAL_LOOTING` (padrão `true`, `BotGlobalPatrolSettings.cs:192`).

**Resolução:** `Spt40Members.Field` resolve `LootingNow` (`PatrolLootPointsData.cs:135`).

**Verificação:** seção 3b do `verify-port.sh` (8 leitores resolvidos, 0 nulos). Mutação M1 faz a verificação falhar.

---

### CR-01-05 · B — Bug latente · 🟠 · ✅ Aplicado em 2026-09-30

**ZIP de preset com caminho do 4.1**

**Local:** [`modded/Orbit.Server/Presets/PresetArchive.cs:46`](../../modded/Orbit.Server/Presets/PresetArchive.cs#L46), `Web/Pages/Presets.razor:66`, `Web/Shared/PresetScopeDialog.razor:32`

**Problema:** o ZIP exportado e os textos do painel usavam `SPT_Runtime/user/mods/ORBIT/addon/`. No 4.0 a pasta do servidor é `SPT/` e o mod lê addons da pasta em que foi instalado.

**Resolução:** `Spt40Paths.AddonFolder` = `SPT/user/mods/<pasta do mod>/addon`, usado nos três pontos.

**Verificação:** página de presets no navegador mostra `Each folder in SPT/user/mods/ORBIT-2.1/addon becomes one preset.` A exportação do ZIP em si não foi executada depois da correção.

---

### CR-01-06 · B — Bug latente · 🟡 · ✅ Aplicado em 2026-09-30

**`_shallStartInteract` em três scripts de ponto de reserva**

**Local:** [`modded/Orbit/Systems/NativeGhostPatrol.cs:21`](../../modded/Orbit/Systems/NativeGhostPatrol.cs#L21)

**Problema:** no 4.0 o campo é privado, ainda ofuscado, e não é o mesmo nos três tipos: `DropItemReservWay.bool_1`, `DropItemAndHealReservWay.bool_0`, `UseSurgeKitReservWay.bool_0`.

**Resolução:** três linhas em `Spt40Members.Fields`, cada uma com a linha onde o campo é ligado e desligado.

**Verificação:** seção 3b do `verify-port.sh`.

---

### CR-01-07 · B — Bug latente · 🟡 · ✅ Aplicado em 2026-09-30

**`NativePatrolArrivalDiagnosticPatch`**

**Local:** [`modded/Orbit/Patches/NativePatrolDiagnosticPatch.cs:12-22`](../../modded/Orbit/Patches/NativePatrolDiagnosticPatch.cs#L12)

**Problema:** alvo `IsCome()` sem parâmetros não existe no 4.0 (é `GClass510.method_0`, `GClass510.cs:85`); o parâmetro `____owner` injeta um campo `_owner` que no 4.0 se chama `BotOwner_0`.

**Resolução:** alvo `method_0`, parâmetro `___BotOwner_0`.

**Verificação:** `verify-port.sh` → OK. Mutação M3 (voltar a `____owner`) falha com `No such field defined in class GClass510: _owner`.

---

### CR-01-08 · B — Bug latente · 🟡 · ✅ Aplicado em 2026-09-30

**`NativeGlukharChoiceDiagnosticPatch`**

**Local:** [`modded/Orbit/Patches/NativePatrolDiagnosticPatch.cs:24-33`](../../modded/Orbit/Patches/NativePatrolDiagnosticPatch.cs#L24)

**Resolução:** parâmetro `___Owner` (`PatrolPointChooserBasic.cs:85`).

---

### CR-01-09 · E — Manutenção · 🟢 · ✅ Aplicado em 2026-09-30

**Campos de diagnóstico de patrulha**

**Local:** [`modded/Orbit/Systems/NativePatrolDiagnostics.cs:32-34`](../../modded/Orbit/Systems/NativePatrolDiagnostics.cs#L32)

**Resolução:** `_comeTime` → `ComeTime`, `_reservChoosedTime` → `ReservChoosedTime`, `_nextChangeWay` → `NextChangeWay`, via `Spt40Members.Field`. Só afetava o texto do log.

---

### CR-01-10 · F — Opcional · 🟢 · ⏭️ Aceito

**`DormantBoarAvoidDangerBypassPatch` cai no mesmo método do patch irmão**

**Local:** [`modded/Orbit/Patches/DormantDangerLayerBypassPatch.cs:48-60`](../../modded/Orbit/Patches/DormantDangerLayerBypassPatch.cs#L48)

**Problema:** no 4.0 `GClass49` (BoarAvoidDangerLayer) não declara `ShallUseNow`; o alvo resolvido é `GClass48.ShallUseNow` (`GClass48.cs:88`), que já tem o prefixo de `DormantAvoidDangerBypassPatch`.

**Decisão:** manter como no upstream. Os dois prefixos fazem o mesmo teste, que só lê `BotOwner_0` da classe base; o resultado é igual e o custo é uma consulta a mais por decisão de camada. Três revisores chegaram à mesma conclusão. Remover o patch divergiria do upstream sem ganho observável.

---

### CR-01-11 · E — Manutenção · 🟢 · ⏭️ Aceito

**Diagnóstico de granada do SAIN fica inerte**

**Local:** [`modded/Orbit/Systems/NativeAwakeGrenadeDiagnostics.cs:17`](../../modded/Orbit/Systems/NativeAwakeGrenadeDiagnostics.cs#L17)

**Problema:** `SAIN.Layers.Combat.Solo.AvoidGrenadeAction` não existe no SAIN 4.8.0 instalado; a comparação nunca é verdadeira e o log `AWAKE GRENADE` nunca sai.

**Decisão:** aceitar. É só diagnóstico, depende de uma classe da linha 4.1 do SAIN e não há equivalente no 4.8.0. Nenhum efeito em comportamento.

---

### CR-01-12 · B — Bug latente · 🟡 · ⏭️ Aceito — igual no upstream

**Corpo de jogador humano remoto não gera ponto de corpo no host**

**Local:** [`modded/Orbit/Patches/CorpseRegistrationPatch.cs:21`](../../modded/Orbit/Patches/CorpseRegistrationPatch.cs#L21)

**Problema:** o postfix é em `Player.CreateCorpse`. No Fika, `ObservedPlayer.CreateCorpse` não chama a base. No host, um jogador humano remoto é `ObservedPlayer`: o corpo dele não vira ponto de saque para os bots nem credita o esquadrão.

**Decisão:** não é efeito do port. O `ObservedPlayer.CreateCorpse` do Fika atual (branch `main`, linha 4.1) tem o mesmo corpo do Fika 4.0. Registrado como pendência de coop na memória do mod; não foi observado em raid.

---

### CR-01-13 · F — Opcional · 🟢 · ⏭️ Aceito

**`POST /orbit/zones/native-floors` sem corpo lança `InvalidCastException`**

**Decisão:** o cliente sempre envia corpo (`Orbit/Systems/WaypointNativeZoneFloors.cs:49-57`). Com corpo a rota devolve `{}` (conferido no `server-smoke-test.ps1`).

---

### CR-01-14 · F — Opcional · 🟢 · ⏭️ Aceito

**Sem cartão de mod no 4.0**

**Decisão:** `IModWebMetadata` do 4.0 é um marcador vazio; não há seção de links de mods. O painel é aberto por `https://<servidor>/orbit` ou pelo botão "Open web config UI" do F12. O link "Server home" do painel leva à página inicial do servidor.

---

## Cobertura das revisões

| Frente | O que conferiu | Resultado |
|---|---|---|
| Execução dos patches fora do jogo | 43 `ModulePatch` + 3 conjuntos manuais + addon Fika: alvo, ligação de parâmetros, transpilers sobre o IL real | 90 chamadas `Harmony.Patch`, 0 falhas |
| Revisor de reflexão por texto | Todo acesso por nome em `Orbit/` e `Orbit.Fika/` contra o jogo 4.0, SAIN 4.8.0, Fika 2.3.21, BigBrain 1.4.0, Waypoints 1.8.2, MoreBotsAPI 2.0.1, UNTAR 3.1.0 | 14 NÃO (todos tratados acima), demais SIM |
| Revisor de patches e membros | 43 patches + 60 pares de renomeação, com programa próprio | 0 renomeação errada |
| Revisor do servidor | Carga, ordem, rotas, páginas, 35 componentes MudBlazor, JS, disco | 1 bloqueador e 1 importante (tratados), 4 menores |

**Fora do alcance de todas as frentes:** comportamento em raid; o lado 4.1 dos membros (não há assembly 4.1); integrações com mods ausentes da máquina (RUAF, Black Division, ISB, Combine Soldiers, RoguesVRaiders, InterchangeRework, mapas do Manimal, Fika headless).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-30 | Code review 01 criada, consolidando a execução fora do jogo e três revisores independentes; achados aplicados no mesmo dia |
