# Skills-Extended — Documentação Técnica

## Apresentação

**Skills-Extended** (`com.cj.SkillsExtended`, v2.2.2) é um mod híbrido SPT 4.0.2 / EFT 0.16.x que reativa skills dormentes do vanilla (Lockpicking, Prone Movement, Silent Ops) e adiciona 4 skills inteiramente novas (Usec Ar Systems, Bear Ak Systems, Usec Negotiations, Bear Raw Power), com um minigame customizado de arrombamento de fechaduras, uma extensa camada de configuração via JSON/Web UI, sincronização multiplayer via Fika, e um mecanismo pouco comum de **patching de IL em tempo de preload** (via `Mono.Cecil`) para estender fisicamente os tipos `EFT.SkillManager` e `EFT.EBuffId` do jogo antes mesmo do assembly do jogo ser carregado.

Esta documentação cobre a arquitetura **base** do mod a partir do código intocado em [`original/`](../original/). Correções pontuais aplicadas nesta sandbox vivem em [`modded/`](../modded/) e são citadas apenas onde relevante (ver seção "Diffs conhecidos vs. upstream" no [documento 01](01-visao-geral-e-arquitetura.md)).

Para a configuração do BepInEx ConfigurationManager (F12) do lado cliente, ver [`PROPRIEDADES.md`](../PROPRIEDADES.md) na raiz do mod — não duplicada aqui.

## Índice de documentos

| # | Documento | Conteúdo | Status |
|---|---|---|---|
| 01 | [Visão Geral e Arquitetura](01-visao-geral-e-arquitetura.md) | Os 6 projetos, ciclo de vida do plugin, o mecanismo do Prepatcher (IL patching via Mono.Cecil), soft-dependencies, diffs conhecidos vs. upstream | 🟢 Vivo |
| 02 | [Sistema Central de Skills e Buffs](02-sistema-central-de-skills-e-buffs.md) | `SkillManagerExt`, `SkillBuffs`, como cada skill registra seus buffs, o papel do `EBuffId` customizado, os patches de construtor que "plugam" skills novas no `SkillManager` vanilla | 🟢 Vivo |
| 03 | [Skills Físicas, Movimento e Combate](03-skills-fisicas-e-combate.md) | Endurance/Strength/Vitality/Health/Metabolism/StressResistance/Immunity, Weapon Skills (Usec/Bear), Silent Ops, velocidade em obstáculos (Strength), Prone Movement | 🟢 Vivo |
| 04 | [Medicina de Campo](04-medicina-de-campo.md) | Field Medicine (teto de skill cap de estimuladores) e First Aid (tempo de uso, custo de recursos, movimento elite) | 🟢 Vivo |
| 05 | [Lockpicking e Minigame](05-lockpicking-e-minigame.md) | Interação de contexto em portas, dificuldade por porta/mapa, máquina de estados do minigame, XP, quebra de fechadura, trilha de hacking (desativada) | 🟢 Vivo |
| 06 | [Servidor, Configuração e Web UI](06-servidor-configuracao-e-web-ui.md) | DI/`IOnLoad` do servidor, importação de dados, patches server-side, rotas HTTP, Web UI Blazor, `SkillsConfig.json`/`ServerConfig.json` | 🟢 Vivo |
| 07 | [Multiplayer (Fika)](07-multiplayer-fika.md) | `FikaSync` (plugin opcional), sincronização do minigame de lockpicking via pacotes de rede, dependências hard vs. soft | 🟢 Vivo |

## Mapa de código-fonte (`original/`)

### Prepatcher (BepInEx preloader)

- [`Prepatcher/Patcher.cs`](../original/Prepatcher/Patcher.cs) — patching de IL via Mono.Cecil (novos `EBuffId`, campo `SkillManagerExtended`)
- [`Prepatcher/MessageBoxHelper.cs`](../original/Prepatcher/MessageBoxHelper.cs) — diálogo de erro nativo se o plugin não estiver instalado

### Plugin (cliente BepInEx)

- [`Plugin/SkillsExtendedPlugin.cs`](../original/Plugin/SkillsExtendedPlugin.cs) — ponto de entrada, `Awake`/`Start`, checagem de versão
- [`Plugin/Config/ConfigManager.cs`](../original/Plugin/Config/ConfigManager.cs) · [`ConfigurationManagerAttributes.cs`](../original/Plugin/Config/ConfigurationManagerAttributes.cs)
- [`Plugin/Exceptions/Exceptions.cs`](../original/Plugin/Exceptions/Exceptions.cs)
- [`Plugin/GlobalUsings.cs`](../original/Plugin/GlobalUsings.cs) — aliases para tipos decompilados do EFT
- [`Plugin/Helpers/ConsoleCommands.cs`](../original/Plugin/Helpers/ConsoleCommands.cs) · [`CursorSettings.cs`](../original/Plugin/Helpers/CursorSettings.cs) · [`IdHelper.cs`](../original/Plugin/Helpers/IdHelper.cs) · [`ReflectionHelper.cs`](../original/Plugin/Helpers/ReflectionHelper.cs)
- [`Plugin/Models/BuffModel.cs`](../original/Plugin/Models/BuffModel.cs) · [`WeaponClasses.cs`](../original/Plugin/Models/WeaponClasses.cs)
- [`Plugin/Utils/GameUtils.cs`](../original/Plugin/Utils/GameUtils.cs) · [`MathUtils.cs`](../original/Plugin/Utils/MathUtils.cs) · [`SkillUtils.cs`](../original/Plugin/Utils/SkillUtils.cs)

**Core** (documento 02):
- [`Skills/Core/SkillManagerExt.cs`](../original/Plugin/Skills/Core/SkillManagerExt.cs) · [`SkillBuffs.cs`](../original/Plugin/Skills/Core/SkillBuffs.cs)
- [`Skills/Core/Patches/CreateSkillPatches.cs`](../original/Plugin/Skills/Core/Patches/CreateSkillPatches.cs) · [`SkillClassConstructorPatch.cs`](../original/Plugin/Skills/Core/Patches/SkillClassConstructorPatch.cs) · [`SkillClassOnTriggerPatch.cs`](../original/Plugin/Skills/Core/Patches/SkillClassOnTriggerPatch.cs) · [`SkillManagerConstructorPatch.cs`](../original/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs)

**Skills físicas e combate** (documento 03):
- [`Skills/SkillClasses/Physical/EnduranceSkill.cs`](../original/Plugin/Skills/SkillClasses/Physical/EnduranceSkill.cs) · [`HealthSkill.cs`](../original/Plugin/Skills/SkillClasses/Physical/HealthSkill.cs) · [`ImmunitySkill.cs`](../original/Plugin/Skills/SkillClasses/Physical/ImmunitySkill.cs) · [`MetabolismSkill.cs`](../original/Plugin/Skills/SkillClasses/Physical/MetabolismSkill.cs) · [`StrengthSkill.cs`](../original/Plugin/Skills/SkillClasses/Physical/StrengthSkill.cs) · [`StressResistanceSkill.cs`](../original/Plugin/Skills/SkillClasses/Physical/StressResistanceSkill.cs) · [`VitalitySkill.cs`](../original/Plugin/Skills/SkillClasses/Physical/VitalitySkill.cs)
- [`Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs`](../original/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs)
- [`Skills/SilentOps/Patches/DoorSoundPatch.cs`](../original/Plugin/Skills/SilentOps/Patches/DoorSoundPatch.cs) · [`GetBarterPricePatch.cs`](../original/Plugin/Skills/SilentOps/Patches/GetBarterPricePatch.cs) · [`MeleeSpeedPatch.cs`](../original/Plugin/Skills/SilentOps/Patches/MeleeSpeedPatch.cs)
- [`Skills/Strength/Patches/MovementContextSetSpeedLimitPatch.cs`](../original/Plugin/Skills/Strength/Patches/MovementContextSetSpeedLimitPatch.cs)
- [`Skills/ProneMovement/Patches/ProneMoveStatePatch.cs`](../original/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs)
- [`Skills/UI/Patches/BuffIconShowPatch.cs`](../original/Plugin/Skills/UI/Patches/BuffIconShowPatch.cs) · [`SkillIconShowPatch.cs`](../original/Plugin/Skills/UI/Patches/SkillIconShowPatch.cs) · [`SkillPanelDisablePatch.cs`](../original/Plugin/Skills/UI/Patches/SkillPanelDisablePatch.cs)
- [`Skills/Shared/Patches/OnEnemyKillPatch.cs`](../original/Plugin/Skills/Shared/Patches/OnEnemyKillPatch.cs) · [`OnGameStarted.cs`](../original/Plugin/Skills/Shared/Patches/OnGameStarted.cs)

**Medicina de campo** (documento 04):
- [`Skills/FieldMedicine/Patches/AbstractSkillClassSummaryLevelPatch.cs`](../original/Plugin/Skills/FieldMedicine/Patches/AbstractSkillClassSummaryLevelPatch.cs) · [`PersonalBuffPatch.cs`](../original/Plugin/Skills/FieldMedicine/Patches/PersonalBuffPatch.cs) · [`PersonalBuffStringPatches.cs`](../original/Plugin/Skills/FieldMedicine/Patches/PersonalBuffStringPatches.cs) · [`StimulatorApplyBuffPatch.cs`](../original/Plugin/Skills/FieldMedicine/Patches/StimulatorApplyBuffPatch.cs)
- [`Skills/FirstAid/Patches/CanWalkPatch.cs`](../original/Plugin/Skills/FirstAid/Patches/CanWalkPatch.cs) · [`HealthEffectComponentPatch.cs`](../original/Plugin/Skills/FirstAid/Patches/HealthEffectComponentPatch.cs) · [`HealthEffectUseTimePatch.cs`](../original/Plugin/Skills/FirstAid/Patches/HealthEffectUseTimePatch.cs)

**Lockpicking** (documento 05):
- [`Skills/LockPicking/LockPickingGame.cs`](../original/Plugin/Skills/LockPicking/LockPickingGame.cs) · [`LockPickingHelpers.cs`](../original/Plugin/Skills/LockPicking/LockPickingHelpers.cs) · [`LockPickingEvents.cs`](../original/Plugin/Skills/LockPicking/LockPickingEvents.cs) · [`WorldInteractionUtils.cs`](../original/Plugin/Skills/LockPicking/WorldInteractionUtils.cs)
- [`Skills/LockPicking/Actions/Actions.cs`](../original/Plugin/Skills/LockPicking/Actions/Actions.cs) · [`HackingActionHandler.cs`](../original/Plugin/Skills/LockPicking/Actions/HackingActionHandler.cs) · [`InspectActionHandler.cs`](../original/Plugin/Skills/LockPicking/Actions/InspectActionHandler.cs) · [`LockPickActionHandler.cs`](../original/Plugin/Skills/LockPicking/Actions/LockPickActionHandler.cs)
- [`Skills/LockPicking/Patches/DoorActionPatch.cs`](../original/Plugin/Skills/LockPicking/Patches/DoorActionPatch.cs) · [`KeyCardDoorActionPatch.cs`](../original/Plugin/Skills/LockPicking/Patches/KeyCardDoorActionPatch.cs) (desativado, `[IgnoreAutoPatch]`)

### Common (biblioteca compartilhada)

- [`Common/SkillsExtendedInfo.cs`](../original/Common/SkillsExtendedInfo.cs) — constantes de versão e flags runtime
- [`Common/Config/SkillsConfig.cs`](../original/Common/Config/SkillsConfig.cs) · [`KeysData.cs`](../original/Common/Config/KeysData.cs)
- [`Common/Config/Skills/*.cs`](../original/Common/Config/Skills/) — um modelo `*Data` por skill (14 arquivos)
- [`Common/Extensions/MathExtensions.cs`](../original/Common/Extensions/MathExtensions.cs)
- [`Common/LockPicking/LockPickingEventData.cs`](../original/Common/LockPicking/LockPickingEventData.cs)

### Server (mod server-side SPT + Web UI)

- [`Server/Metadata.cs`](../original/Server/Metadata.cs) — `SeModMetadata`
- [`Server/Core/ConfigController.cs`](../original/Server/Core/ConfigController.cs) · [`DatabaseImporter.cs`](../original/Server/Core/DatabaseImporter.cs) · [`SkillLevelAdjuster.cs`](../original/Server/Core/SkillLevelAdjuster.cs) · [`SkillsExtendedPatch.cs`](../original/Server/Core/SkillsExtendedPatch.cs) · [`SkillsStaticRouter.cs`](../original/Server/Core/SkillsStaticRouter.cs) · [`UpdateChecker.cs`](../original/Server/Core/UpdateChecker.cs)
- [`Server/Models/ReleaseNotes.cs`](../original/Server/Models/ReleaseNotes.cs) · [`ServerConfig.cs`](../original/Server/Models/ServerConfig.cs)
- [`Server/Patches/CultistProductionPatch.cs`](../original/Server/Patches/CultistProductionPatch.cs) · [`GeneratePlayerScavPatch.cs`](../original/Server/Patches/GeneratePlayerScavPatch.cs) · [`GetTraderAssortPatch.cs`](../original/Server/Patches/GetTraderAssortPatch.cs) · [`QuestExperienceRewardPatch.cs`](../original/Server/Patches/QuestExperienceRewardPatch.cs) · [`QuestMoneyRewardPatch.cs`](../original/Server/Patches/QuestMoneyRewardPatch.cs) · [`ScavCooldownTimerPatch.cs`](../original/Server/Patches/ScavCooldownTimerPatch.cs)
- [`Server/Utils/SkillUtil.cs`](../original/Server/Utils/SkillUtil.cs)
- [`Server/Resources/Configs/SkillsConfig.json`](../original/Server/Resources/Configs/), `ServerConfig.json`
- [`Server/Web/`](../original/Server/Web/) — páginas Blazor (`.razor`) da Web UI de configuração

### FikaSync (plugin multiplayer opcional)

- [`FikaSync/FikaSyncPlugin.cs`](../original/FikaSync/FikaSyncPlugin.cs) · [`VersionChecker.cs`](../original/FikaSync/VersionChecker.cs) · [`ConfigManagerAttributes.cs`](../original/FikaSync/ConfigManagerAttributes.cs)
- [`FikaSync/Controllers/LockPickingFikaController.cs`](../original/FikaSync/Controllers/LockPickingFikaController.cs)
- [`FikaSync/Packets/LockPickingSyncPacket.cs`](../original/FikaSync/Packets/LockPickingSyncPacket.cs)
- [`FikaSync/Patches/OnGameStartedPatch.cs`](../original/FikaSync/Patches/OnGameStartedPatch.cs)

## Pendências / itens não confirmados

- O binding exato de persistência acionado pelos botões de salvar nas páginas Blazor da Web UI (`Server/Web/Pages/SkillConfigs/*.razor`) não foi confirmado a partir do código C# lido — presumivelmente chama `ConfigController.SaveSkillsConfig`/`SaveServerConfig`, mas o `.razor` em si não foi analisado linha a linha (fora do escopo de inspeção `.cs` desta rodada).
- `Plugin/Helpers/ConsoleCommands.cs`, `CursorSettings.cs` e `IdHelper.cs`, e `Plugin/Utils/MathUtils.cs` foram mapeados na árvore de arquivos mas não tiveram o conteúdo detalhado nos documentos temáticos (uso pontual/utilitário, sem impacto arquitetural relevante para os temas cobertos).
- `Server/Models/ReleaseNotes.cs` foi mapeado mas não detalhado (modelo de dados de apoio ao `UpdateChecker`, provavelmente as notas de release exibidas ao usuário).
