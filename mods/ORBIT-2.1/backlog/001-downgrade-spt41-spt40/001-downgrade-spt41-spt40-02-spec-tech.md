# 001 — Downgrade SPT 4.1 → 4.0 · Spec Técnica

**Mod:** ORBIT-2.1
**Spec funcional:** [001-downgrade-spt41-spt40-01-spec.md](001-downgrade-spt41-spt40-01-spec.md)
**Criado:** 2026-09-30

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

Esta spec foi escrita depois de um reconhecimento guiado pelo compilador numa cópia descartável do mod: as tabelas abaixo registram o que o compilador e o decompile 4.0 confirmaram, não uma previsão.

## 1. Estratégia

### 1.1 O que muda entre as versões

| Camada | SPT 4.1 (onde o ORBIT 2.1 foi escrito) | SPT 4.0.13 (alvo) | Medido em |
|---|---|---|---|
| Jogo | EFT 0.16.9.5.40743 | EFT 0.16.9.40087 | `README.md` de `SP-Tushonka/modules` · `D:\SPT\EscapeFromTarkov.exe` |
| `Assembly-CSharp` | deofuscado: nome real de tipo, namespace e membro (`BotMover._owner`, `EFT.InventoryLogic.Ammo`) | remap antigo: tipos planos (`AmmoItemClass`, `GClass45`), membros derivados do tipo (`BotOwner_0`, `method_10`) | commit `00e7ad8` do upstream · decompile local |
| Servidor | .NET 10, `IModMetadata`, `IOnLoad.OnLoadAsync(CancellationToken)`, ação de rota com 5 argumentos, `ISptLogger` em `SPTarkov.Common` | .NET 9, `AbstractModMetadata`, `IOnLoad.OnLoad()`, ação de rota com 4 argumentos, `ISptLogger` em `SPTarkov.Server.Core.Models.Utils` | [Server_40_to_41.md](../../../../wiki/spt/modding/SPT_41_Modding/Server_40_to_41.md) · `references/spt-source/` |
| Painel web | `IModBlazorMetadata` (cartão `HomePage`), MudBlazor mais novo | `IModWebMetadata` (marcador vazio), MudBlazor 8.13.0 | `references/spt-source/Libraries/SPTarkov.Server.Web/SPTWeb.cs:16` |
| Dependências do cliente | SAIN 4.5+, Fika para 4.1 | SAIN 4.8.0, Fika.Core 2.3.21, BigBrain 1.4.0, Waypoints 1.8.2 | versão de arquivo das DLLs em `D:\SPT\BepInEx\plugins` |

A build do jogo é diferente. O port só consegue provar equivalência de nomes e de assinaturas; diferença de comportamento dentro de um método do jogo que tenha o mesmo nome e a mesma assinatura nas duas builds só aparece em jogo.

### 1.2 Fontes do de-para, em ordem de confiança

1. **Commit `00e7ad8` do upstream ("Migrate to SPT 4.1.2"), lido ao contrário.** É o port 4.0 → 4.1 feito pelo autor em 30 arquivos; a mensagem do commit diz que os membros foram conferidos contra o decompile dos dois remaps. Cobre o código que existia na versão 1.3.0.
2. **Tabela oficial de nomes de tipo**, [Class_Name_Mappings.md](../../../../wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md), idêntica par a par a [consolidated-mappings.txt](../../../../docs/files-from-4.1/consolidated-mappings.txt) (5.961 pares). Cobre tipos, não membros.
3. **Decompile 4.0 local**, para os membros que o ORBIT passou a usar depois do commit `00e7ad8` (versões 2.0 e 2.1, 124 commits). Cada membro foi casado por tipo, assinatura e papel no corpo do método.
4. **Compilador contra as DLLs reais do 4.0** e **execução dos patches fora do jogo** (§8, checkpoints 1 e 2), que são o juiz de 1 a 3.

### 1.3 Forma do port: camada de compatibilidade + edições mínimas

O objetivo é manter o diff contra o upstream pequeno, para que uma versão futura do ORBIT possa ser portada reaplicando o mesmo conjunto de mudanças, e concentrar todo nome ofuscado em poucos arquivos (AP-03, AP-09).

| Categoria de diferença | Tratamento | Onde |
|---|---|---|
| A. Nome de tipo não genérico | `global using <nome 4.1> = <tipo 4.0>;` — o fonte mantém o nome 4.1 | `Orbit/Compat/Spt40TypeAliases.cs` (compilado também pelo `Orbit.Fika`) |
| B. Tipo genérico aberto, nome totalmente qualificado | edição no ponto de uso (apelido não cobre) | arquivos do upstream |
| C. Nome de membro acessado por sintaxe C# | edição no ponto de uso | arquivos do upstream |
| D. Nome de tipo comparado como texto em tempo de execução (`GetType().Name`) | tradução tipo 4.0 → nome 4.1 num único ponto de leitura | `Orbit/Compat/Spt40TypeNames.cs` + `NativeGhostPartisan.Layer` |
| E. Nome de membro passado como texto para reflexão/Harmony | tradução nome 4.1 → nome 4.0 no ponto que faz a busca | `Orbit/Compat/Spt40Members.cs` + `NativeGhostBodyPatches.Bind` |
| F. Parâmetro de patch que injeta campo por nome (`___campo`) | renomear o parâmetro | arquivos do upstream |
| G. API do servidor | arquivo de compatibilidade + edição dos 4 pontos de contrato | `Orbit.Server/Compat/Spt40ServerCompat.cs` |
| H. Projeto e referências | `Directory.Build.props` resolve a instalação pelo `.spt-path`; alvos de cópia do autor removidos | `modded/Directory.Build.props`, os três `.csproj` |

Alternativa descartada: substituir os nomes 4.1 por nomes 4.0 em todos os arquivos. Espalha `GClassNNNN` por 30 arquivos, aumenta o diff e troca nomes curtos e comuns (`Ammo`, `Vest`, `Money`) por busca textual, com risco de atingir identificadores do próprio mod.

As categorias D, E e F não geram erro de compilação. São elas que quebram em silêncio e são o alvo dos checkpoints 2 e 3.

## 2. Pontos de patch

O port não cria patch novo. Os alvos abaixo são os que mudam de nome no 4.0; os demais 39 patches `ModulePatch` resolvem pelo mesmo nome nas duas versões (lista completa na saída de `scripts/verify-port.sh`).

| Patch do ORBIT | Alvo no 4.1 | Alvo no 4.0 | Evidência |
|---|---|---|---|
| `AirdropLandedPatch` | `ClientAirDrop.PlayLandingSound` | `AirdropLogicClass.method_0` | commit `00e7ad8` |
| `HardTeleportTracePatch`, `RescueInterceptPatch` | `BotMover.CastFromPos` | `BotMover.method_10(Vector3 posiblePos, bool withExtra)` | [BotMover.cs:756](../../../../references/eft-decompiled/Assembly-CSharp/BotMover.cs#L756) |
| `OrbitMoverHandoffFallbackPatch` | `BotMover.FindBetterPosition` | `BotMover.method_4(Vector3 castPoint)` | [BotMover.cs:631](../../../../references/eft-decompiled/Assembly-CSharp/BotMover.cs#L631), chamado em `SetPlayerToNavMesh` |
| `OrbitMoverMotionPatch` | `BotMoverImpostor.OnMotionApplied` | `GClass494.method_21(CollisionFlags flags, Vector3 deltaMove)` | [GClass494.cs:103](../../../../references/eft-decompiled/Assembly-CSharp/GClass494.cs#L103) |
| `NativePatrolArrivalDiagnosticPatch` | `PatrolMoveSimple.IsCome()` | `GClass510.method_0()` | [GClass510.cs:85](../../../../references/eft-decompiled/Assembly-CSharp/GClass510.cs#L85) |
| `OrbitTickPatch` | `AICoreController.Update` | `AICoreControllerClass.Update` | commit `00e7ad8` |
| Camadas `AssaultEnemyFar`, `Exfiltration`, `PtrlBirdEye`, `AvoidDanger` | tipo com nome real | `GClass45`, `GClass75`, `GClass79`, `GClass48` | literal de `Name()` em cada classe |
| `NativeGhostBodyPatches.Bind` (BotDoorOpener) | `TryPassCurrentDoor`, `RunEnteringDoorSequence`, `WaitForDoorOpen`, `InteractionWithDoor` | `method_2`, `method_7`, `method_0`, `method_8` | [BotDoorOpener.cs:455](../../../../references/eft-decompiled/Assembly-CSharp/BotDoorOpener.cs#L455), `:626`, `:428`, `:737` |
| `NativeGhostBodyPatches.Bind` (BotFirstAid) | `ApplyToSelf` | `BotFirstAidClass.method_3(int? varianAnim, Action callback)` | [BotFirstAidClass.cs:447](../../../../references/eft-decompiled/Assembly-CSharp/BotFirstAidClass.cs#L447) |
| `Orbit.Fika/DoorSyncBridge` | `FikaPlayer.ExecuteInteraction` / `StartInteraction` | `FikaPlayer.vmethod_1` / `vmethod_0` | `references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs:1361` e `:1350` |

`DormantBoarAvoidDangerBypassPatch`: no 4.0 `GClass49` (BoarAvoidDangerLayer) não sobrescreve `ShallUseNow`, então o alvo resolvido é `GClass48.ShallUseNow`, o mesmo de `DormantAvoidDangerBypassPatch`. Ver §7.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. As 4 entradas existentes estão em [PROPRIEDADES.md](../../PROPRIEDADES.md) e não mudam.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Directory.Build.props` | CRIAR | Resolve `SptRoot`, `FikaRef` e `SPT_DIR` a partir de `SPT_PATH` ou do `.spt-path` |
| `modded/Orbit/Compat/Spt40TypeAliases.cs` | CRIAR | 47 apelidos de tipo 4.1 → 4.0 |
| `modded/Orbit/Compat/Spt40TypeNames.cs` | CRIAR | 17 camadas de IA: tipo 4.0 → nome 4.1, para comparação de texto |
| `modded/Orbit/Compat/Spt40Members.cs` | CRIAR | Nomes 4.0 dos membros que o mod busca por texto |
| `modded/Orbit.Server/Compat/Spt40ServerCompat.cs` | CRIAR | Namespace do logger, ponte `ShowMessageBoxAsync` do MudBlazor 8.13 |
| `modded/Orbit/Orbit.csproj`, `Orbit.Fika/Orbit.Fika.csproj`, `Orbit.Server/Orbit.Server.csproj` | MODIFICAR | Sem caminho fixo do autor, sem cópia para `C:\Games\SPT-4.1`, servidor em `net9.0` |
| 30 arquivos em `modded/Orbit/` | MODIFICAR | Membros renomeados e genéricos (categorias B e C), cerca de 115 linhas |
| `modded/Orbit/Systems/NativeGhostPartisan.cs` | MODIFICAR | `Layer(bot)` passa pelo `Spt40TypeNames` (categoria D) |
| `modded/Orbit/Patches/NativeGhostBodyPatches.cs` | MODIFICAR | `Bind` resolve campo do dono e nomes de método pelo `Spt40Members` (categoria E) |
| `modded/Orbit/Patches/NativePatrolDiagnosticPatch.cs` | MODIFICAR | Alvo `method_0`; `___BotOwner_0` e `___Owner` (categoria F) |
| `modded/Orbit.Fika/DoorStateReceiver.cs`, `DoorSyncBridge.cs` | MODIFICAR | `WorldInteractiveDataPacketStruct`, `GlobalEventHandlerClass`, `vmethod_0/1` |
| `modded/Orbit.Server/ModMetadata.cs`, `Load/OrbitServerLoad.cs`, `Routers/*.cs` | MODIFICAR | Contrato do servidor 4.0 |
| `scripts/patch-dryrun/`, `scripts/check-type-name-literals.py`, `scripts/verify-port.sh` | CRIAR | Verificação fora do jogo (§8) |

`original/` não é tocado.

## 5. Tabelas de de-para

### 5.1 Tipos (categoria A)

Arquivo-fonte único: [Spt40TypeAliases.cs](../../modded/Orbit/Compat/Spt40TypeAliases.cs). Cada par vem da tabela oficial e foi conferido contra a declaração da classe no decompile; para camadas de IA, contra o literal de `Name()`.

### 5.2 Genéricos e nomes qualificados (categoria B)

| 4.1 | 4.0 |
|---|---|
| `AICoreAgent<T>` | `AICoreAgentClass<T>` |
| `AICoreStrategy<T>` | `AICoreStrategyAbstractClass<T>` |
| `AICoreActionResult<T, U>` | `AICoreActionResultStruct<T, U>` |
| `AICoreLayer<T>` | `AICoreLayerClass<T>` |
| `OperationResult<T>` | `GStruct154<T>` |
| `EFT.Ballistics.DamageInfo` | `DamageInfoStruct` |
| `EFT.HandBook.Handbook` | `HandbookClass` |
| `EFT.GlobalEvents.GlobalEventsController` | `GlobalEventHandlerClass` |
| `EFT.Ballistics.BallisticsCalculatorConstants` | `LayerMasksDataAbstractClass` |
| `WorldInteractiveObject.InteractiveObjectStatusInfo` | `WorldInteractiveObject.WorldInteractiveDataPacketStruct` |

### 5.3 Membros (categoria C)

| Tipo 4.0 | Membro 4.1 | Membro 4.0 | Fonte |
|---|---|---|---|
| `BotMover` | `_owner` | `BotOwner_0` | commit `00e7ad8` |
| `BotMover` | `_player` | `Player` | commit `00e7ad8` |
| `BotMover` | `_lastGoodCastPoint`, `_lastGoodCastPointTime` | `LastGoodCastPoint`, `LastGoodCastPointTime` | commit `00e7ad8` |
| `BotMover` | `_prevSuccessLinkedFrom`, `_prevPosLinkedTime` | `PrevSuccessLinkedFrom_1`, `PrevPosLinkedTime_1` | commit `00e7ad8` |
| `BotMover` | `_prevLinkPos`, `_positionOnWayCasted` | `PrevLinkPos`, `PositionOnWayCasted` | commit `00e7ad8` |
| `BotMover` | `CastFromPos` | `method_10` | commit `00e7ad8` |
| `BotMover` | `FindBetterPosition` | `method_4` | `BotMover.cs:631` |
| `BotMover` | `_dirCurPoint` | `DirCurPoint_1` | `BotMover.cs:40`, `:177` |
| `BotMover` | `_linkedToNavmeshInitially`, `_prevOffsetGoodCasted`, `_prevOffsetGoodCastedTime` | `LinkedToNavmeshInitially`, `PrevOffsetGoodCasted`, `PrevOffsetGoodCastedTime` | `BotMover.cs:82`, `:124`, `:127` |
| `GClass494` (BotMoverImpostor) | `OnMotionApplied`, `_isImpostorWorks`, `_lastFlags` | `method_21`, `Bool_0`, `CollisionFlags_0` | `GClass494.cs:103`, `:13`, `:16` |
| `BaseLogicLayerAbstractClass` | `_owner` | `BotOwner_0` | commit `00e7ad8` |
| `BaseBrain` | `_owner` | `Owner` | `BaseBrain.cs:10` |
| `BotDoorOpener` | `_owner`, `_currentDoorLink`, `_doBreach`, `_enteringDoorSequence` | `Owner`, `CurrentDoorLink`, `DoBreach`, `EnteringDoorSequence` | `BotDoorOpener.cs:250`, `:247`, `:274`, `:283` |
| `Player` | `ExecuteInteraction` | `vmethod_1` | commit `00e7ad8` |
| `AirdropLogicClass` | `PlayLandingSound`, `_syncObject` | `method_0`, `AirdropSynchronizableObject_0` | commit `00e7ad8` |
| `AICoreControllerClass` | `_enable` | `Bool_0` | commit `00e7ad8` |
| `InventoryController`, `TraderControllerClass` | `ActiveEvents` | `List_0` | commit `00e7ad8` |
| `BotSpawner` | `_allBotZones` | `AllBotZones` | commit `00e7ad8` |
| `BotBoss` | `SetPatrolMode` | `method_1` | `BotBoss.cs:312` (corpo: `BossLogic.SetPatrolMode()`) |
| `GClass124` (PartisanPlantingTargetManyLayer) | `_cachePoint`, `_cachePoints` | `CustomNavigationPoint_0`, `HashSet_0` | `GClass124.cs:38`, `:41` |
| `GClass442` (BossPartisan) | `_listOfPrewarms`, `_period` | `List_0`, `Gclass25_0` | `GClass442.cs:77`, `:32` |
| `GClass3411` (MoveResult) | `_item`, `_to` | `Item_0`, `ItemAddress_1` | `GClass3411.cs:12`, `:20` |
| `GClass3687` (OpticCameraManager) | `IsAnyOpticCameraRendering` | `Boolean_0` | `GClass3687.cs:90` |
| `PatrolDataFollower` | `followerAIBase` | `FollowerAIBase` | `PatrolDataFollower.cs:8` |
| `FikaPlayer`, `ObservedPlayer` | `ExecuteInteraction`, `StartInteraction` | `vmethod_1`, `vmethod_0` | `FikaPlayer.cs:1361`, `:1350` |

### 5.4 Nomes de tipo comparados como texto (categoria D)

`NativeGhostPartisan.Layer(bot)` devolve o nome da classe da camada de IA ativa, e sete arquivos do Ghost Mode comparam esse texto com nomes 4.1. No 4.0 `GetType().Name` devolve `GClassNNN`. As 17 camadas e o literal de `Name()` que confirma cada par estão em [Spt40TypeNames.cs](../../modded/Orbit/Compat/Spt40TypeNames.cs).

### 5.5 Membros buscados por texto (categorias E e F)

Arquivo-fonte: [Spt40Members.cs](../../modded/Orbit/Compat/Spt40Members.cs). O campo do dono do bot (`_owner` no 4.1) é `BotOwner_0` nas classes derivadas de `GClass429` e de `GClass177<T>`, e `Owner` em `BotDoorOpener` e em `PatrolPointChooserBasic`.

### 5.6 Servidor (categoria G)

| 4.1 | 4.0 | Evidência |
|---|---|---|
| `record X : IModMetadata, IModBlazorMetadata` | `record X : AbstractModMetadata, IModWebMetadata` com `override` e `IsBundleMod` | `references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Mod/AbstractModMetadata.cs` |
| `SptVersion "~4.1.0"` | `"~4.0.0"` | — |
| `OnLoadAsync(CancellationToken)` | `OnLoad()` | `references/spt-source/Libraries/SPTarkov.Server.Core/DI/IOnLoad.cs` |
| `OnLoadOrder.Preload + 10` | `OnLoadOrder.PreSptModLoader + 10` | `references/spt-source/Libraries/SPTarkov.Server.Core/DI/OnLoadOrder.cs` |
| `(url, info, sessionId, output, cancellationToken) =>` | `(url, info, sessionId, output) =>` | `references/spt-source/Libraries/SPTarkov.Server.Core/DI/Router.cs` |
| `using SPTarkov.Common.Models.Logging` | `global using SPTarkov.Server.Core.Models.Utils` + namespace vazio | compilação |
| `IDialogService.ShowMessageBoxAsync` | `ShowMessageBox` (MudBlazor 8.13.0), por método de extensão | compilação |
| `net10.0` | `net9.0` | `D:\SPT\SPT\SPT.Server.runtimeconfig.json` |

## 6. Fluxo de dados

O port não altera fluxo. O único caminho novo é o da tradução de nomes:

```
[BSG] camada ativa do bot (objeto GClass86 no 4.0)
   → NativeGhostPartisan.Layer(bot)                      mods/ORBIT-2.1/modded/Orbit/Systems/NativeGhostPartisan.cs
   → Spt40TypeNames.Of(objeto) → "FollowerPatrolLayer"   mods/ORBIT-2.1/modded/Orbit/Compat/Spt40TypeNames.cs
   → comparação com o texto 4.1 nos arquivos NativeGhost*  (sem edição)
```

```
NativeGhostBodyPatches.Bind(tipo, "TryPassCurrentDoor")
   → Spt40Members.Method(tipo, nome 4.1) → "method_2"
   → Spt40Members.OwnerField(tipo)       → campo BotOwner_0 ou Owner
   → harmony.Patch(método 4.0, prefixo)
```

## 7. Riscos e dependências

- **Build do jogo diferente.** Risco residual não verificável fora do jogo: método com mesmo nome e assinatura e corpo diferente. Mitigação: os transpilers do ORBIT falham de forma explícita quando o IL esperado não aparece, e o checkpoint 2 os executa sobre o IL real do 4.0.
- **`DormantBoarAvoidDangerBypassPatch`.** No 4.0 o alvo resolvido é o mesmo de `DormantAvoidDangerBypassPatch`; os dois prefixos fazem o mesmo teste e o resultado é igual, com uma chamada duplicada por decisão de camada. Decisão: manter como no upstream (comportamento correto, custo de uma comparação) e registrar no as-built.
- **SAIN 4.8.0 em vez de 4.5+.** A reflexão do ORBIT no SAIN já tenta dois caminhos de tipo; o checkpoint 2 mostra qual resolve. Os demais acessos por nome ao SAIN são cobertos pelo checkpoint 3.
- **Mods de facção.** As integrações (UNTAR, RUAF, Black Division, ISB, Combine Soldiers) usam reflexão em mods que não estão instalados na máquina de desenvolvimento ou cuja versão 4.0 difere. Sem o mod, a integração não liga. Com o mod em versão diferente, cada integração se desliga com aviso no log. Não verificável aqui.
- **Identificador de plugin duplicado.** ORBIT 1.2.1 e 2.1 têm o mesmo GUID `com.chazut.orbit`. A instalação precisa remover a pasta `BepInEx/plugins/ORBIT/` do 1.2.1.
- **Addon Fika e versões.** Host, headless e todos os clientes precisam do mesmo `ORBIT.dll` e do mesmo `Orbit.Fika.dll`.
- **Ordem de carga do servidor.** `PreSptModLoader + 10` roda antes da importação do banco; o `PresetService` só usa os próprios arquivos.

## 8. Checklist de implementação e checkpoints de revisão

- [ ] Reconhecimento em cópia descartável: compilar contra o 4.0 e classificar cada erro nas categorias de §1.3.
- [ ] Aplicar em `modded/` preservando fim de linha dos arquivos do upstream.
- [ ] **Checkpoint 1 — compila.** `/compile-mod ORBIT-2.1 --no-install`: três projetos, zero erros.
- [ ] **Checkpoint 2 — patches aplicam.** `scripts/verify-port.sh`: carrega `ORBIT.dll` e `Orbit.Fika.dll` com as DLLs reais do jogo, roda `Enable()` de todos os patches e confere alvo, ligação de parâmetros e transpilers. Critério: zero `FAIL`.
- [ ] **Checkpoint 3 — nomes em texto.** `scripts/check-type-name-literals.py` (todo literal igual a nome de tipo 4.1 tem linha no `Spt40TypeNames`) + auditoria independente de toda reflexão por texto contra as DLLs instaladas.
- [ ] **Checkpoint 4 — servidor.** Subir o servidor 4.0.13 numa cópia isolada só com o ORBIT; conferir carga, as 13 páginas e as 3 rotas.
- [ ] **Checkpoint 5 — revisão adversarial.** Três revisores independentes (patches e equivalência de membros; reflexão por texto; servidor) + `/code-review`.
- [ ] As-built, memória do mod, grafo, documentação do procedimento em `docs/technical/`.
- [ ] **Validação em jogo** (humana): critérios 4 a 8 da spec funcional.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | O port não cria nem altera hook de ciclo de vida; `OrbitInitPatch`/`OrbitDisposePatch` do upstream resolvem nos mesmos alvos (checkpoint 2) |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Nenhum patch novo; os do upstream filtram por bot (`BotRoster.IsOrbitActive`) |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | §2: alvos `method_N` citados com assinatura e linha; `Bind` do upstream já inclui overrides; overrides auditados no checkpoint 5 |
| 4 | Mudança de estado via API canônica do EFT — AP-04 | N/A | O port não introduz escrita de estado nova |
| 5 | Estado entre raids coberto | N/A | Sem estado novo; `Spt40TypeNames` e `Spt40Members` são tabelas imutáveis |
| 6 | Semântica/defaults de ConfigEntry — AP-05 | N/A | §3: nenhuma entrada nova |
| 7 | Re-invocação de método patcheado com reentry-guard — AP-07 | N/A | Nenhum patch novo |
| 8 | Flags/caches de intercept validados após troca de contexto — AP-08 | N/A | Nenhum cache novo |
| 9 | Patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | §2 e §5.3 citam `arquivo.cs:linha`; existência conferida também pelo compilador e pela execução do checkpoint 2 |
| 10 | Skill EFT usada como lever não-inerte — AP-10 | N/A | O port não usa skill do EFT |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Os dois pacotes do addon são do upstream e não mudam de formato; só os nomes de método do alvo do postfix mudam |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-30 | Spec técnica criada após reconhecimento guiado pelo compilador |
