# 001 — Downgrade SPT 4.1 → 4.0 · Spec Técnica

**Mod:** ORBIT-2.1
**Spec funcional:** [001-downgrade-spt41-spt40-01-spec.md](001-downgrade-spt41-spt40-01-spec.md)
**Criado:** 2026-09-30

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

Esta spec foi escrita depois de um reconhecimento guiado pelo compilador numa cópia descartável do mod e revisada pela [review 01](001-downgrade-spt41-spt40-03-spec-tech-review-01.md): as tabelas registram o que o compilador, o decompile 4.0 e a execução fora do jogo confirmaram. Docs técnicos lidos pelo gatilho: [spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md), [spt4-vs-spt41-gclass-deobfuscation.md](../../../../docs/technical/spt4-vs-spt41-gclass-deobfuscation.md), [fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md) (o addon declara `INetSerializable`).

## 1. Estratégia

### 1.1 O que muda entre as versões

| Camada | SPT 4.1 (onde o ORBIT 2.1 foi escrito) | SPT 4.0.13 (alvo) | Medido em |
|---|---|---|---|
| Jogo | EFT 0.16.9.5.40743 | EFT 0.16.9.40087 | `README.md` de `SP-Tushonka/modules` · `D:\SPT\EscapeFromTarkov.exe` |
| `Assembly-CSharp` | deofuscado: nome real de tipo, namespace e membro (`BotMover._owner`, `EFT.InventoryLogic.Ammo`) | remap antigo: tipos planos (`AmmoItemClass`, `GClass45`), membros derivados do tipo (`BotOwner_0`, `method_10`) | commit `00e7ad8` do upstream · decompile local |
| Servidor | .NET 10, `IModMetadata`, `IOnLoad.OnLoadAsync(CancellationToken)`, ação de rota com 5 argumentos, `ISptLogger` em `SPTarkov.Common` | .NET 9, `AbstractModMetadata`, `IOnLoad.OnLoad()`, ação de rota com 4 argumentos, `ISptLogger` em `SPTarkov.Server.Core.Models.Utils` | [Server_40_to_41.md](../../../../wiki/spt/modding/SPT_41_Modding/Server_40_to_41.md) · `references/spt-source/` |
| Painel web | `IModBlazorMetadata` (cartão `HomePage`), MudBlazor mais novo | `IModWebMetadata` (marcador vazio), MudBlazor 8.13.0; o documento do hospedeiro não carrega o MudBlazor | `references/spt-source/Libraries/SPTarkov.Server.Web/SPTWeb.cs:16`, `Components/App.razor` |
| Dependências do cliente | SAIN 4.5+, Fika para 4.1 | SAIN 4.8.0, Fika.Core 2.3.21, BigBrain 1.4.0, Waypoints 1.8.2 | versão de arquivo das DLLs em `D:\SPT\BepInEx\plugins` |

A build do jogo é diferente. O port prova equivalência de nomes e de assinaturas; diferença de comportamento dentro de um método do jogo com o mesmo nome e a mesma assinatura só aparece em jogo.

A fonte do Fika vendorizada em `references/fika-plugin` é a 2.3.8; o instalado é o 2.3.21. As linhas de Fika citadas aqui são da 2.3.8; o que vale para o instalado foi conferido contra a DLL (checkpoint 2 e decompile com `ilspycmd`).

### 1.2 Fontes do de-para, em ordem de confiança

1. **Commit `00e7ad8` do upstream ("Migrate to SPT 4.1.2"), lido ao contrário.** É o port 4.0 → 4.1 feito pelo autor em 30 arquivos. Cobre o código que existia na versão 1.3.0. Dá o par de nomes nos dois lados.
2. **Tabela oficial de nomes de tipo**, [Class_Name_Mappings.md](../../../../wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md), idêntica par a par a [consolidated-mappings.txt](../../../../docs/files-from-4.1/consolidated-mappings.txt) (5.961 pares). Cobre tipos, não membros.
3. **Decompile 4.0 local**, para os membros que o ORBIT passou a usar depois do commit `00e7ad8` (versões 2.0 e 2.1). Cada membro foi casado por tipo, assinatura e papel no corpo. **Só prova o lado 4.0**: não há assembly 4.1 na máquina.
4. **Compilador contra as DLLs reais do 4.0** e **execução dos patches fora do jogo** (§8). Verificam que o nome existe e que o patch liga; não verificam que o membro escolhido tem o mesmo papel do membro 4.1.

### 1.3 Forma do port: camada de compatibilidade + edições mínimas

O objetivo é manter o diff contra o upstream pequeno, para que uma versão futura do ORBIT possa ser portada reaplicando o mesmo conjunto de mudanças, e concentrar todo nome ofuscado em poucos arquivos (AP-03, AP-09).

| Cat. | Diferença | O compilador acusa? | Tratamento | Onde |
|---|---|---|---|---|
| A | Nome de tipo não genérico | sim | `global using <nome 4.1> = <tipo 4.0>;` — o fonte mantém o nome 4.1 | `Orbit/Compat/Spt40TypeAliases.cs` (compilado também pelo `Orbit.Fika`) |
| B | Tipo genérico aberto, nome totalmente qualificado | sim | edição no ponto de uso | arquivos do upstream |
| C | Membro acessado por sintaxe C# | sim | edição no ponto de uso | arquivos do upstream |
| D | Nome de tipo comparado como texto (`GetType().Name`) | **não** | tradução tipo 4.0 → nome 4.1 no ponto que lê o nome | `Orbit/Compat/Spt40TypeNames.cs` + `NativeGhostPartisan.Layer` |
| E | Método ou campo passado como texto a reflexão/Harmony | **não** | tradução nome 4.1 → nome 4.0 no ponto que faz a busca | `Orbit/Compat/Spt40Members.cs` |
| F | Parâmetro de patch que injeta campo por nome (`___campo`) | **não** | renomear o parâmetro | `NativePatrolDiagnosticPatch.cs` |
| G | API do servidor | sim | arquivo de compatibilidade + contrato de metadados, carga e rotas | `Orbit.Server/Compat/Spt40ServerCompat.cs` |
| H | Hospedeiro web: quem carrega CSS e JS do MudBlazor | **não** (responde HTTP 200) | o layout do mod carrega os dois | `Orbit.Server/Web/Shared/MainLayout.razor` |
| I | Caminho de instalação escrito em texto (`SPT_Runtime/`, pasta `ORBIT`) | **não** | derivar da localização da DLL | `Spt40Paths` em `Spt40ServerCompat.cs` |
| J | Projeto e referências | sim | `Directory.Build.props` resolve a instalação pelo `.spt-path`; alvos de cópia do autor removidos | `modded/Directory.Build.props`, os três `.csproj` |

Conferido e sem mudança entre as versões (texto que não é nome de tipo nem de membro do remap): os 12 nomes de cérebro passados ao BigBrain (`PMC`, `PmcUsec`, ...; cada `ShortName()` devolve o literal), a camada `"LootPatrol"` (`GClass117.cs:610`), os 13 nomes de `BotLogicDecision` comparados como texto (`BotLogicDecision.cs`), o limite `9000` de ação customizada do BigBrain e o nome `"SAIN : Combat Layer"` que o SAIN 4.8.0 monta.

Alternativa descartada: substituir os nomes 4.1 por nomes 4.0 em todos os arquivos. Espalha `GClassNNNN` por 30 arquivos, aumenta o diff e troca nomes curtos e comuns (`Ammo`, `Vest`, `Money`) por busca textual, com risco de atingir identificadores do próprio mod.

## 2. Pontos de patch

O port não cria patch novo. O plugin tem 43 classes `ModulePatch` e três conjuntos de patches manuais; no 4.0 ficam 89 chamadas `Harmony.Patch` sobre 80 métodos. A tabela lista os alvos cujo nome muda; os demais resolvem pelo mesmo nome (lista completa na saída de `scripts/verify-port.sh`).

| Patch do ORBIT | Alvo no 4.1 | Alvo no 4.0 | Evidência | Base do par |
|---|---|---|---|---|
| `AirdropLandedPatch` | `ClientAirDrop.PlayLandingSound` | `AirdropLogicClass.method_0` | commit `00e7ad8` | autor |
| `HardTeleportTracePatch`, `RescueInterceptPatch` | `BotMover.CastFromPos` | `BotMover.method_10(Vector3 posiblePos, bool withExtra)` | [BotMover.cs:756](../../../../references/eft-decompiled/Assembly-CSharp/BotMover.cs#L756) | autor |
| `OrbitTickPatch` | `AICoreController.Update` | `AICoreControllerClass.Update` | commit `00e7ad8` | autor |
| Camadas `AssaultEnemyFar`, `Exfiltration`, `PtrlBirdEye`, `AvoidDanger` | tipo com nome real | `GClass45`, `GClass75`, `GClass79`, `GClass48` | literal de `Name()` em cada classe | tabela oficial |
| `OrbitMoverHandoffFallbackPatch` | `BotMover.FindBetterPosition` | `BotMover.method_4(Vector3 castPoint)` | [BotMover.cs:631](../../../../references/eft-decompiled/Assembly-CSharp/BotMover.cs#L631); único `Vector3 → Vector3` da classe, chamado só em `SetPlayerToNavMesh` (`:583`) | inferido do 4.0 |
| `OrbitMoverMotionPatch` | `BotMoverImpostor.OnMotionApplied` | `GClass494.method_21(CollisionFlags flags, Vector3 deltaMove)` | [GClass494.cs:103](../../../../references/eft-decompiled/Assembly-CSharp/GClass494.cs#L103); inscrito em `MovementContext.OnMotionApplied` (`GClass494.cs:48`) | inferido do 4.0 |
| `NativePatrolArrivalDiagnosticPatch` | `PatrolMoveSimple.IsCome()` | `GClass510.method_0()` | [GClass510.cs:85](../../../../references/eft-decompiled/Assembly-CSharp/GClass510.cs#L85) | inferido do 4.0 |
| `NativeGhostBodyPatches.Bind` (BotDoorOpener) | `TryPassCurrentDoor`, `RunEnteringDoorSequence`, `WaitForDoorOpen`, `InteractionWithDoor` | `method_2`, `method_7`, `method_0`, `method_8` | [BotDoorOpener.cs:455](../../../../references/eft-decompiled/Assembly-CSharp/BotDoorOpener.cs#L455), `:626`, `:428`, `:737` | **inferido pelo corpo do método 4.0** |
| `NativeGhostBodyPatches.Bind` (BotFirstAid) | `ApplyToSelf` | `BotFirstAidClass.method_3(int? varianAnim, Action callback)` | [BotFirstAidClass.cs:447](../../../../references/eft-decompiled/Assembly-CSharp/BotFirstAidClass.cs#L447) | **inferido pelo corpo do método 4.0** |
| `Orbit.Fika/DoorSyncBridge` | `FikaPlayer.ExecuteInteraction` / `StartInteraction` | `FikaPlayer.vmethod_1` / `vmethod_0` | `references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs:1361` e `:1350` (2.3.8); nomes de parâmetro conferidos na DLL 2.3.21 | autor (para `Player`) |

"Inferido" significa que o par não aparece no commit do autor nem em tabela: o membro 4.0 foi escolhido lendo o código 4.0, sem o lado 4.1 para comparar. Dois leitores independentes chegaram aos mesmos cinco métodos de `Bind`. Ver §7.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. As 4 entradas existentes estão em [PROPRIEDADES.md](../../PROPRIEDADES.md) e não mudam.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Directory.Build.props` | CRIAR | Resolve `SptRoot`, `FikaRef` e `SPT_DIR` a partir de `SPT_PATH` ou do `.spt-path` |
| `modded/Orbit/Compat/Spt40TypeAliases.cs` | CRIAR | 47 apelidos de tipo 4.1 → 4.0 |
| `modded/Orbit/Compat/Spt40TypeNames.cs` | CRIAR | 17 camadas de IA: tipo 4.0 → nome 4.1, para comparação de texto |
| `modded/Orbit/Compat/Spt40Members.cs` | CRIAR | Nomes 4.0 de 5 métodos e 7 campos buscados por texto, e do campo do dono do bot |
| `modded/Orbit.Server/Compat/Spt40ServerCompat.cs` | CRIAR | Namespace do logger, ponte `ShowMessageBoxAsync` do MudBlazor 8.13, `Spt40Paths.AddonFolder` |
| `modded/Orbit/Orbit.csproj`, `Orbit.Fika/Orbit.Fika.csproj`, `Orbit.Server/Orbit.Server.csproj` | MODIFICAR | Sem caminho fixo do autor, sem cópia para `C:\Games\SPT-4.1`, servidor em `net9.0` |
| 30 arquivos em `modded/Orbit/` | MODIFICAR | Membros renomeados e genéricos (categorias B e C) |
| `modded/Orbit/Plugin.cs` | MODIFICAR | `HandbookClass`; `OrbitBuild` no log; `DormantBoarAvoidDangerBypassPatch` só liga quando `Applies` |
| `modded/Orbit/Systems/NativeGhostPartisan.cs` | MODIFICAR | `Layer(bot)` passa pelo `Spt40TypeNames` (categoria D) |
| `modded/Orbit/Patches/NativeGhostBodyPatches.cs` | MODIFICAR | `Bind` resolve campo do dono e nomes de método pelo `Spt40Members` (categoria E) |
| `modded/Orbit/Systems/NativeGhostLoot.cs`, `NativeGhostPatrol.cs`, `NativePatrolDiagnostics.cs` | MODIFICAR | Campos lidos por reflexão passam por `Spt40Members.Field` (categoria E) |
| `modded/Orbit/Patches/NativePatrolDiagnosticPatch.cs` | MODIFICAR | Alvo `method_0`; `___BotOwner_0` e `___Owner` (categoria F) |
| `modded/Orbit/Patches/DormantDangerLayerBypassPatch.cs` | MODIFICAR | Propriedade `Applies` no patch do Boar |
| `modded/Orbit.Fika/DoorStateReceiver.cs`, `DoorSyncBridge.cs` | MODIFICAR | `WorldInteractiveDataPacketStruct`, `GlobalEventHandlerClass`, `vmethod_0/1` |
| `modded/Orbit.Fika/OrbitFikaPlugin.cs` | MODIFICAR | Recebimento do pacote de combate fantasma dentro de captura de exceção |
| `modded/Orbit.Server/ModMetadata.cs`, `Load/OrbitServerLoad.cs`, `Routers/*.cs` | MODIFICAR | Contrato do servidor 4.0 (categoria G) |
| `modded/Orbit.Server/Web/Shared/MainLayout.razor` | MODIFICAR | Carrega CSS, fonte e JS do MudBlazor (categoria H) |
| `modded/Orbit.Server/Presets/PresetArchive.cs`, `Web/Pages/Presets.razor`, `Web/Shared/PresetScopeDialog.razor` | MODIFICAR | Caminho de addon do 4.0 (categoria I) |
| `scripts/patch-dryrun/`, `scripts/check-type-name-literals.py`, `scripts/verify-port.sh`, `scripts/server-smoke-test.ps1`, `scripts/install-to-spt.sh` | CRIAR | Verificação fora do jogo e instalação (§8) |

`original/` não é tocado. Tamanho medido: `git diff --shortstat b1bedb0b -- mods/ORBIT-2.1/modded`.

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

A coluna "Linha que mostra o papel" cita onde o código 4.0 usa o membro do jeito que o nome 4.1 descreve, quando o nome 4.0 é só tipo e ordinal.

| Tipo 4.0 | Membro 4.1 | Membro 4.0 | Declaração | Linha que mostra o papel | Base do par |
|---|---|---|---|---|---|
| `BotMover` | `_owner` | `BotOwner_0` | `GClass429.cs:8` | — | autor |
| `BotMover` | `_player` | `Player` | `BotMover.cs:25` | `:249` | autor |
| `BotMover` | `_lastGoodCastPoint`, `_lastGoodCastPointTime` | `LastGoodCastPoint`, `LastGoodCastPointTime` | `BotMover.cs:103`, `:106` | `:564`, `:558` | autor |
| `BotMover` | `_prevSuccessLinkedFrom`, `_prevPosLinkedTime` | `PrevSuccessLinkedFrom_1`, `PrevPosLinkedTime_1` | `BotMover.cs:85`, `:88` | `:626`, `:607` | autor |
| `BotMover` | `_prevLinkPos`, `_positionOnWayCasted` | `PrevLinkPos`, `PositionOnWayCasted` | `BotMover.cs:97`, `:112` | `:575`, `:713` | autor |
| `BotMover` | `CastFromPos` | `method_10` | `BotMover.cs:756` | parâmetro `posiblePos` | autor |
| `BotMover` | `FindBetterPosition` | `method_4` | `BotMover.cs:631` | `:583` | inferido |
| `BotMover` | `_dirCurPoint` | `DirCurPoint_1` | `BotMover.cs:40` | `:177` | inferido |
| `BotMover` | `_linkedToNavmeshInitially`, `_prevOffsetGoodCasted`, `_prevOffsetGoodCastedTime` | `LinkedToNavmeshInitially`, `PrevOffsetGoodCasted`, `PrevOffsetGoodCastedTime` | `BotMover.cs:82`, `:124`, `:127` | `:625`, `:726`, `:721` | inferido |
| `GClass494` (BotMoverImpostor) | `OnMotionApplied`, `_isImpostorWorks`, `_lastFlags` | `method_21`, `Bool_0`, `CollisionFlags_0` | `GClass494.cs:103`, `:13`, `:16` | `:48`, `:24` (`IsImpostorWorks => Bool_0`), `:26` | inferido |
| `BaseLogicLayerAbstractClass` | `_owner` | `BotOwner_0` | `BaseLogicLayerAbstractClass.cs:9` | — | autor |
| `BaseBrain` | `_owner` | `Owner` | `BaseBrain.cs:10` | — | inferido |
| `BotDoorOpener` | `_owner`, `_currentDoorLink`, `_doBreach`, `_enteringDoorSequence` | `Owner`, `CurrentDoorLink`, `DoBreach`, `EnteringDoorSequence` | `BotDoorOpener.cs:250`, `:247`, `:274`, `:283` | `:472` (o texto `"_currentDoorLink == null"` mostra o nome original), `:460`, `:632` | inferido |
| `Player` | `ExecuteInteraction` | `vmethod_1` | `EFT/Player.cs:26082` | — | autor |
| `AirdropLogicClass` | `PlayLandingSound`, `_syncObject` | `method_0`, `AirdropSynchronizableObject_0` | `AirdropLogicClass.cs:259`, `:40` | — | autor |
| `AICoreControllerClass` | `_enable` | `Bool_0` | `AICoreControllerClass.cs:8` | único `bool` | autor |
| `InventoryController`, `TraderControllerClass` | `ActiveEvents` | `List_0` | `TraderControllerClass.cs:362` | `:465-470` | autor |
| `BotSpawner` | `_allBotZones` | `AllBotZones` | `EFT/BotSpawner.cs:125` | — | autor |
| `BotBoss` | `SetPatrolMode` | `method_1` | `BotBoss.cs:312` | corpo: `BossLogic.SetPatrolMode()` | inferido |
| `GClass124` (PartisanPlantingTargetManyLayer) | `_cachePoint`, `_cachePoints` | `CustomNavigationPoint_0`, `HashSet_0` | `GClass124.cs:38`, `:41` | `:47`, `:89` | inferido |
| `GClass442` (BossPartisan) | `_listOfPrewarms`, `_period` | `List_0`, `Gclass25_0` | `GClass442.cs:77`, `:32` | `:316`, `:87` | inferido |
| `GClass3411` (MoveResult) | `_item`, `_to` | `Item_0`, `ItemAddress_1` | `GClass3411.cs:12`, `:20` | `:87`, `:89` | inferido |
| `GClass3687` (OpticCameraManager) | `IsAnyOpticCameraRendering` | `Boolean_0` | `GClass3687.cs:90` | única propriedade `bool` de leitura; corpo `CurrentOpticSight != null` | inferido |
| `PatrolDataFollower` | `followerAIBase` | `FollowerAIBase` | `PatrolDataFollower.cs:8` | — | inferido |
| `FikaPlayer`, `ObservedPlayer` | `ExecuteInteraction`, `StartInteraction` | `vmethod_1`, `vmethod_0` | `FikaPlayer.cs:1361`, `:1350` (2.3.8) | — | autor (para `Player`) |

### 5.4 Nomes de tipo comparados como texto (categoria D)

`NativeGhostPartisan.Layer(bot)` devolve o nome da classe da camada de IA ativa, e sete arquivos do Ghost Mode comparam esse texto com nomes 4.1. No 4.0 `GetType().Name` devolve `GClassNNN`. As 17 camadas e o literal de `Name()` que confirma cada par estão em [Spt40TypeNames.cs](../../modded/Orbit/Compat/Spt40TypeNames.cs).

### 5.5 Membros buscados por texto (categorias E e F)

Arquivo-fonte: [Spt40Members.cs](../../modded/Orbit/Compat/Spt40Members.cs).

**Campo do dono do bot** (`_owner` no 4.1): `BotOwner_0` nas classes derivadas de `GClass429` e de `GClass177<T>`; `Owner` em `BotDoorOpener` e em `PatrolPointChooserBasic`.

**Métodos** (usados por `NativeGhostBodyPatches.Bind`): os cinco de §2.

**Campos lidos por reflexão:**

| Tipo 4.0 | Campo 4.1 | Campo 4.0 | Evidência | Quem lê | Efeito de errar |
|---|---|---|---|---|---|
| `PatrolLootPointsData` | `_lootingNow` | `LootingNow` | `PatrolLootPointsData.cs:135` | `NativeGhostLoot.cs:18` | todo bot nativo com saque real fica acordado |
| `DropItemReservWay` | `_shallStartInteract` | `bool_1` | `DropItemReservWay.cs:11`; ligado em `:65`, desligado em `:23` | `NativeGhostPatrol.cs:21` | bot nesse ponto de reserva fica acordado |
| `DropItemAndHealReservWay` | `_shallStartInteract` | `bool_0` | `DropItemAndHealReservWay.cs:11`; `:78`, `:29` | idem | idem |
| `UseSurgeKitReservWay` | `_shallStartInteract` | `bool_0` | `UseSurgeKitReservWay.cs:14`; `:88`, `:32` | idem | idem |
| `PatrollingData` | `_comeTime` | `ComeTime` | `PatrollingData.cs:34` | `NativePatrolDiagnostics.cs:32` | só texto de log |
| `PatrollingData` | `_reservChoosedTime` | `ReservChoosedTime` | `PatrollingData.cs:37` | `NativePatrolDiagnostics.cs:33` | só texto de log |
| `PatrolPointChooserBasic` | `_nextChangeWay` | `NextChangeWay` | `PatrolPointChooserBasic.cs:88` | `NativePatrolDiagnostics.cs:34` | só texto de log |

**Parâmetros de patch (categoria F):** `____owner` → `___BotOwner_0` (`GClass429.cs:8`) e `___Owner` (`PatrolPointChooserBasic.cs:85`). `____player` não muda: `MovementContext._player` existe no 4.0 (`EFT/MovementContext.cs:157`).

### 5.6 Servidor (categorias G, H e I)

| 4.1 | 4.0 | Evidência |
|---|---|---|
| `record X : IModMetadata, IModBlazorMetadata` | `record X : AbstractModMetadata, IModWebMetadata` com `override` e `IsBundleMod` | `references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Mod/AbstractModMetadata.cs` |
| `SptVersion "~4.1.0"` | `"~4.0.0"` | log de carga do servidor 4.0.13 |
| `OnLoadAsync(CancellationToken)` | `OnLoad()` | `references/spt-source/Libraries/SPTarkov.Server.Core/DI/IOnLoad.cs` |
| `OnLoadOrder.Preload + 10` | `OnLoadOrder.PreSptModLoader + 10` | `references/spt-source/Libraries/SPTarkov.Server.Core/DI/OnLoadOrder.cs` |
| `(url, info, sessionId, output, cancellationToken) =>` | `(url, info, sessionId, output) =>` | `references/spt-source/Libraries/SPTarkov.Server.Core/DI/Router.cs` |
| `using SPTarkov.Common.Models.Logging` | `global using SPTarkov.Server.Core.Models.Utils` + namespace vazio | compilação |
| `IDialogService.ShowMessageBoxAsync` | `ShowMessageBox` (MudBlazor 8.13.0), por método de extensão | compilação; caixa aberta no navegador |
| `net10.0` | `net9.0` | `D:\SPT\SPT\SPT.Server.runtimeconfig.json` |
| CSS e JS do MudBlazor vêm do hospedeiro (inferido: não há fonte do 4.1 no repositório) | cada layout carrega os seus | `Components/App.razor:1-16` só carrega `blazor.web.js`; `Components/Layout/BaseMudBlazorLayout.razor:5`, `:15` |
| `SPT_Runtime/user/mods/ORBIT/addon/` | `SPT/user/mods/<pasta do mod>/addon/` | o mod lê addons de `<pasta da DLL>/addon` (`Presets/PresetService.cs:29`) |
| Cartão do mod na página do servidor (`HomePage`) | não existe | `IModBlazorMetadata.cs:22` (`IModWebMetadata` é vazio) |

## 6. Fluxo de dados

O port não altera fluxo. Os caminhos novos são os da tradução de nomes:

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

Diagramas de sequência: `.diagramas.md` na raiz do repositório, seção "ORBIT 2.1 no SPT 4.0".

## 7. Riscos e dependências

### 7.1 Build do jogo diferente

Risco residual não verificável fora do jogo: método com mesmo nome e assinatura e corpo diferente. O que mitiga, com o alcance real:

- Os transpilers falham de forma explícita quando o IL esperado não aparece, e o checkpoint 2 os executa sobre o IL real. Isso cobre **4 dos 80 métodos** com patch (`DropItemAndHealReservWay.ManualUpdate`, `UseSurgeKitReservWay.ManualUpdate`, `PatrollingData.Unpause`, `GClass247.UpdateNodeByBrain`).
- `NativeGhostBodyPatches.Bind` busca por nome e inclui as subclasses. No 4.0 ele liga **32 pontos de entrada**. Não há o número do 4.1 para comparar: um método que exista no 4.1 com um nome que `Bind` não lista, ou um método a mais sob o mesmo nome, não é detectado.
- Para os outros 76 métodos, a única prova é nome, assinatura e nomes de parâmetro.

### 7.2 Cinco métodos sem prova do lado 4.1

`BotDoorOpener` `method_2`, `method_7`, `method_0`, `method_8` e `BotFirstAidClass.method_3` foram escolhidos pelo corpo do método 4.0. Os mais críticos:

- `method_2` (`TryPassCurrentDoor`): se errado, um bot nativo dormindo entra no estado `NearDoor` e manda o corpo andar até a porta, ou deixa de fazê-lo acordado.
- `method_3` (`ApplyToSelf`): se errado, um bot nativo dormindo põe o remédio na mão, ou a guarda bloqueia outra operação.
- `method_0` (`WaitForDoorOpen`) é o menos certo dos quatro de porta.

Os quatro de porta recebem a mesma guarda (`VoidPrefix`/`BoolPrefix`, que só distinguem `ManualUpdate` dos demais), então trocar dois entre si não muda o comportamento; o que importa é o conjunto. Validação em jogo: com Ghost Mode ligado, observar bots nativos atravessando portas e se curando ao acordar; conferir no log a linha `body guards ready (32 entry points)`.

### 7.3 `DormantBoarAvoidDangerBypassPatch`

No 4.0 `GClass49` (BoarAvoidDangerLayer) não sobrescreve `ShallUseNow` (`GClass49.cs:9-76`). O alvo que o patch resolve é `GClass48.ShallUseNow` (`GClass48.cs:88`), que `DormantAvoidDangerBypassPatch` já cobre, incluindo as instâncias `GClass49`. Aplicar os dois teria três efeitos: dois prefixos idênticos no mesmo método; o HarmonyX roda todos os prefixos e junta os retornos com `And`, então o resultado é o mesmo; e o segundo prefixo receberia instâncias `GClass48` num parâmetro declarado como `GClass49` (o HarmonyX passa `__instance` sem conferir tipo), o que só não falha enquanto o prefixo não tocar membro declarado em `GClass49`.

**Decisão:** o patch ganhou a propriedade `Applies` (verdadeira só quando `BoarAvoidDangerLayer` declara `ShallUseNow`) e `Plugin.cs` só o liga quando ela é verdadeira. No 4.0 ele não liga; no 4.1 o comportamento do upstream fica igual. O `verify-port.sh` lista o patch como `N/A`.

### 7.4 Overrides dos alvos virtuais (AP-03)

| Alvo do patch | Override | O override chama a base? | Consequência e decisão |
|---|---|---|---|
| `GClass45/75/79.ShallUseNow` | nenhuma subclasse | — | — |
| `GClass48.ShallUseNow` | `GClass49` não sobrescreve | — | §7.3 |
| `BotMover.GoToByWay` (`BotMover.cs:378`) | `GClass493.cs:30` (mover do BTR), corpo vazio | não | o patch não roda para o BTR; sem efeito prático |
| `Player.CreateCorpse()` (`EFT/Player.cs:30694`) | `FikaPlayer`, `ObservedPlayer` (Fika) | `FikaPlayer` chama só no servidor; `ObservedPlayer` não chama | no host, o corpo de um jogador humano remoto não vira ponto de corpo nem credita o esquadrão. **Igual no upstream** (o `ObservedPlayer.CreateCorpse` do Fika atual tem o mesmo corpo); fora do escopo do port; pendência P-1.5 |
| `Player.OnItemAddedOrRemoved` (`EFT/Player.cs:29212`) | `ObservedPlayer`, corpo vazio | não | item pego por jogador remoto não remove o ponto de loot por este patch; `LootItemKilledPatch` cobre o caso (comentário no fonte do upstream) |
| `Player.InitVaultingComponent` (`EFT/Player.cs:28727`) | `HideoutPlayer.cs:665`, `NarratePlayer.cs:70`, corpos vazios | não | irrelevante em raid |
| `MovementContext.UpdateGroundCollision` (`EFT/MovementContext.cs:2621`) | `ObservedMovementContext` (Fika) | não | só afeta jogador observado, que não é bot dormindo |
| `GameWorld.Dispose` (`EFT/GameWorld.cs:2115`) | `ClientGameWorld.cs:324`, `FikaHostGameWorld`, `FikaClientGameWorld` | os três chamam (`:326`, `:111`, `:123`) | o gancho de fim de raid roda nos três |
| `LootItem.Kill` (`LootItem.cs:536`) | `Corpse.cs:278` | chama (`:284`) | — |

### 7.5 Pacotes do addon Fika (AP-11)

O addon declara `OrbitDoorPacket` e `OrbitGhostFightPacket`. `node scripts/check-packet-hashes.js`: hashes 38924 e 43571, sem colisão entre os 80 tipos de pacote do repositório (os do Fika lidos da fonte 2.3.8).

Desvios contra o checklist de [fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md) §7:

| Item do checklist | `OrbitDoorPacket` | `OrbitGhostFightPacket` | Decisão |
|---|---|---|---|
| Envelope de comprimento | não tem | não tem | herdado do upstream; item 002 |
| Só `TryGet*` | usa `Get*` (`OrbitDoorPacket.cs:34-45`) | usa `Get*` (`OrbitGhostFightPacket.cs:50-70`) | herdado; item 002 |
| Flag `Valid` | não tem | não tem | herdado; item 002 |
| Zero `UnregisterPacket` | chama em `DoorSyncBridge.cs:125` | não chama | herdado; item 002 |
| Callback com captura total | tem (`DoorSyncBridge.cs:160-164`) | não tinha | **corrigido no port** (`OnGhostFightPacketSafe`) |
| Leitura independente do fim do leitor | — | decide se há lista de atiradores por `AvailableBytes` (`:59`) | herdado; só é exercido com host de versão anterior à 2.1 |
| Registro por instância do gerenciador | por evento do Fika | por evento do Fika | conforme |

Mudar o formato dos pacotes exige renomear o tipo e todos os clientes na mesma versão; ficou para o item de backlog 002. Risco enquanto isso: host e clientes com versões diferentes do addon leem o pacote de combate fantasma de forma diferente; o `OrbitDoorPacket` tem campo de protocolo e é ignorado quando não bate.

### 7.6 Mods opcionais

| Mod | Instalado em `D:\SPT` | Verificado fora do jogo |
|---|---|---|
| SAIN 4.8.0 | sim | reflexão de personalidade resolve pelo caminho de reserva (`SAIN.Models.Preset.Personalities.EPersonality, SAIN`); os dois patches em tipos do SAIN ligam |
| MoreBotsAPI 2.0.1 | sim | 11 membros do `BotHuntManager` ligam |
| UNTAR 3.1.0 | sim | os 5 membros do adaptador de checkpoint ligam |
| RUAF Come Home, ISB, Black Division, Combine Soldiers, RoguesVRaiders, InterchangeRework, mapas do Manimal, Fika headless | não | não verificável: sem o mod a integração fica inerte; com o mod em versão diferente ela se desliga com aviso no log |

O diagnóstico `AWAKE GRENADE` depende de `SAIN.Layers.Combat.Solo.AvoidGrenadeAction`, que não existe no SAIN 4.8.0: fica inerte, sem efeito em comportamento.

### 7.7 Outros

- **Identificador de plugin duplicado.** ORBIT 1.2.1 e 2.1 têm o mesmo GUID `com.chazut.orbit`. `scripts/install-to-spt.sh` move a pasta do 1.2.1 para fora de `plugins`.
- **Identificação da build.** A versão do plugin continua `2.1.0`, a do upstream de origem (o addon e o painel a usam). O log passa a dizer `ORBIT 2.1.0 (SPT 4.0 port)`; a versão do assembly traz o número de build (`2.1.0.NNNNN`).
- **Addon Fika e versões.** Host, headless e todos os clientes precisam do mesmo `ORBIT.dll` e do mesmo `Orbit.Fika.dll`.
- **Ordem de carga do servidor.** `PreSptModLoader + 10` roda antes da importação do banco; o `PresetService` só usa os próprios arquivos.
- **Funcionalidade do 4.1 que não tem como ser portada:** o cartão do mod na página do servidor. O painel abre por `https://<servidor>/orbit` e pelo botão do F12.
- **Validador de mods do 4.0** rejeita a pasta do mod se houver `*.js` ou `*.ts` em qualquer nível. O ORBIT só grava `.json`.

## 8. Checklist de implementação e checkpoints de revisão

- [x] Reconhecimento em cópia descartável: compilar contra o 4.0 e classificar cada erro nas categorias de §1.3.
- [x] Aplicar em `modded/` preservando fim de linha dos arquivos do upstream.
- [x] **Checkpoint 1 — compila.**
- [x] **Checkpoint 2 — patches ligam.**
- [x] **Checkpoint 3 — nomes em texto.**
- [x] **Checkpoint 4 — servidor.**
- [x] **Checkpoint 5 — revisão adversarial.**
- [x] As-built, memória do mod, grafo, documentação do procedimento em `docs/technical/`.
- [ ] **Validação em jogo** (humana).

| Checkpoint | Como roda | O que prova | O que não prova |
|---|---|---|---|
| 1 | `/compile-mod ORBIT-2.1 --no-install` | tipos e membros acessados por sintaxe C# existem no 4.0 com assinatura compatível | nada sobre texto nem comportamento |
| 2 | `scripts/verify-port.sh` parte 1 (`PatchDryRun`) | alvo de cada patch resolve nesta instalação; parâmetros ligam pelas regras de `HarmonyManipulator.EmitCallParameter` (nome, `___campo`, `__result`); cada transpiler roda sobre o IL real; 8 + 3 leitores por reflexão em campos estáticos não são nulos; reflexão do SAIN, do MoreBotsAPI e do UNTAR liga; qualquer aviso ou erro que o mod grave no próprio log durante a execução conta como falha | o desvio em si (o programa substitui `Harmony.Patch`, não aplica); o tipo de um `___campo`; compatibilidade exata de `__instance`; os acessores do Fika em `GhostSpectatorPlayers`, que precisam do `Chainloader` do BepInEx (conferidos por leitura contra a DLL 2.3.21); buscas feitas só em raid; mods não instalados |
| 3 | `verify-port.sh` partes 2 e 3 + auditoria por leitura | todo literal igual a um nome de tipo 4.1 tem linha no `Spt40TypeNames`; nenhuma linha de código monta caminho do 4.1. Auditoria: um revisor independente listou todo acesso por nome em `Orbit/` e `Orbit.Fika/` (incluindo os 16 helpers de reflexão do mod) e conferiu cada um contra o jogo 4.0 e as DLLs instaladas; 14 não existiam e foram tratados | nome montado em tempo de execução; o script não lê nomes de membro |
| 4 | `scripts/server-smoke-test.ps1` + navegador | o `SPT.Server` 4.0.13 carrega o mod; 13 páginas respondem com os arquivos do MudBlazor; 3 rotas respondem; log sem erro. No navegador (uma vez, em 2026-09-30): navegação, caixa de confirmação, Save refletido em `/orbit/config`, texto do caminho de addons | o script não clica; a chamada pelo cliente do jogo; importação e exportação de preset |
| 5 | quatro revisores de contexto limpo + defeitos introduzidos de propósito | 60 pares de renomeação relidos, 0 errado; cada tipo de verificação falha quando a correção é revertida | — |

**Critérios da spec funcional → onde são verificados**

| Critério / corner case | Checkpoint | Estado |
|---|---|---|
| 1. Compilar os três projetos | 1 | verificado |
| 2. Servidor carrega e painel abre sem erro | 4 | verificado (script + navegador) |
| 3. Rotas devolvem JSON | 4 | verificado |
| 4. Log do jogo com as linhas de prontidão | validação em jogo; o checkpoint 2 mostra as três linhas `ready` fora do jogo | **pendente** |
| 5. Raid com esquadrões e Ghost Mode | validação em jogo | **pendente** |
| 6. Mudar opção no painel e ver na raid seguinte | 4 cobre até o servidor devolver o valor; o resto em jogo | **parcial** |
| 7. Fika | validação em jogo; checkpoint 2 cobre a ligação dos 4 postfixes | **pendente** |
| 8. Estado entre raids | validação em jogo | **pendente** |
| Patch que não acha alvo não falha em silêncio | 2 (todo alvo resolve) + `EnableSafe` do upstream | verificado fora do jogo |
| Nome de classe comparado como texto | 3 | verificado |
| Mods opcionais em versão 4.0 | 2 para SAIN, MoreBotsAPI, UNTAR; §7.6 para os ausentes | parcial |
| Servidor sem o mod ou fora do ar | validação em jogo (`ServerConfig.Fetch` cai nos padrões) | **pendente** |
| 1.2.1 e 2.1 juntos | `install-to-spt.sh`, testado contra árvore falsa | verificado fora do jogo |
| Pasta de instalação `ORBIT-2.1` | 4 (o mod grava ao lado da DLL) e categoria I | verificado para o servidor; cliente em jogo |
| Headless | validação em jogo | **pendente** |

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ herdado | O port não altera ganchos. Único gancho de fim de raid do mod: `GameWorld.Dispose` (`modded/Orbit/Patches/Lifecycle.cs:103-108`), com corpo protegido contra exceção e dupla chamada. Os três overrides chamam a base (§7.4). O AP-01 recomenda também `GameWorld.OnDestroy` e `BaseLocalGame.Stop`; o upstream não os usa. Os caminhos de morte e de fechar o jogo ficam para o critério 8 |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ herdado, com alcance declarado | Cinco patches reagem a ação de qualquer jogador, por desenho: `CorpseRegistrationPatch` (todo `Player` cujo `CreateCorpse` chegue à base; §7.4), `InventoryChangePatch` (todo `Player` não observado), `DoorUnlockTracePatch` (só log), `LootItemKilledPatch` (todo item que sai do mundo), e os 4 postfixes de `DoorSyncBridge` (`FikaPlayer` e `ObservedPlayer`, com ramos por host e cliente em `DoorSyncBridge.cs:259-279`). Os patches de movimento e de camada filtram por bot do ORBIT (`BotRoster.IsOrbitActive`) |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | §2 (alvos `method_N` com assinatura e linha); §7.4 (tabela de overrides, com decisão por linha) |
| 4 | Mudança de estado via API canônica do EFT — AP-04 | N/A | O port não introduz escrita de estado nova |
| 5 | Estado entre raids coberto | N/A para o port; pendente em jogo | As tabelas de compatibilidade são imutáveis. O estado do mod é o do upstream; critério 8 pendente |
| 6 | Semântica/defaults de ConfigEntry — AP-05 | N/A | §3: nenhuma entrada nova |
| 7 | Re-invocação de método patcheado com reentry-guard — AP-07 | N/A | Nenhum patch novo |
| 8 | Flags/caches de intercept validados após troca de contexto — AP-08 | N/A | Nenhum cache novo |
| 9 | Patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | §2 e §5.3 citam `arquivo.cs:linha`; existência conferida pelo compilador e pelo checkpoint 2. O decompile corresponde à DLL instalada (mesmo sha256 de `.provenance.json`) |
| 10 | Skill EFT usada como lever não-inerte — AP-10 | N/A | O port não usa skill do EFT |
| 11 | Pacote FIKA próprio — AP-11 | ⚠️ não conforme, herdado | §7.5: sem colisão de hash; quatro desvios de formato herdados do upstream e registrados no item 002; a captura de exceção do callback foi corrigida no port |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-30 | Spec técnica criada após reconhecimento guiado pelo compilador |
| 2026-09-30 | Revisada com os 20 pontos da review 01 (PA-01-01 a PA-01-20) |
