# TRL-ImmersiveCombatMedicine — Code Review 01 (ad-hoc)

> **Data:** 2026-07-12<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme (revisão gerada por workflow 6 dimensões × verificação adversarial, 29 agentes)<br>
> **Referências:** [memory/sessions.md](../memory/sessions.md)<br>

---

**⚠️ Artefato FORA do fluxo formal de backlog** — o mod não tem `01-spec`/`02-spec-tech` (nasceu da fusão Band-Aid+TrueTrauma 3.11). Formato e IDs seguem o padrão `/code-review`; para regularizar, criar spec retroativa via `/create-spec` e migrar este arquivo para `backlog/NNN-<slug>/`.

**Contadores:** 🔴 4 · 🟠 10 · 🟡 20 · 🟢 15 (49 achados após dedup de 6 dimensões; 2 obsoletos descartados)

**Detalhe completo** (evidência com excerpts, análise integral por dimensão): saída do workflow `wf_051c1142-b91` — journal em `~/.claude/projects/.../subagents/workflows/wf_051c1142-b91/journal.jsonl`.

## Índice

- CR-01-01 ✅ · 🔴 A — Curar BOT como médico CLIENTE é impossível: handshake nunca respondido e ApplyFullTreatment restrito
- CR-01-02 ✅ · 🔴 C — Desmaio não é sincronizado com o host — FikaPacketManager/TraumaFaintPacket do 3.11 não foi migrado 
- CR-01-03 · 🔴 A — Auto-close de 1.0m na BandAidUI mata o modo médico ativado pelo prompt nativo (2.5m) e aborta curas  ✅
- CR-01-04 ✅ · 🔴 A — Consumo do desfibrilador via reflection com assinatura errada — exceção certa dentro do Prefix quebr
- CR-01-05 ✅ · 🟠 B — csproj mistura NuGet UnityEngine.Modules 2019.4.39 (era SPT 3.11/Unity 2019) com DLLs 2022.3 do jogo
- CR-01-06 ✅ · 🟠 C — Mojibake UTF-8 duplo-encodado em 3 arquivos (86 ocorrências) — vaza para strings visíveis ao jogador
- CR-01-07 ✅ · 🟠 C — PROPRIEDADES.md não existe — 13 ConfigEntries (incluindo a seção nova '5. Debug') sem documentação o
- CR-01-08 ✅ · 🟠 B — Bridge EffectRemoved filtra a interface errada: GInterface350 em vez de GInterface376 (marcador real
- CR-01-09 ✅ · 🟠 B — Estado de desmaio vaza no fim da raid: menu com áudio mudo e prone forçado no primeiro frame da raid
- CR-01-10 ✅ · 🟠 B — Handshake pendente (_pendingHeal*) não é limpo em DeactivateMedicMode/ResetAllState → resposta tardi
- CR-01-11 ✅ · 🟠 B — Abort por distância >3.5m zera _isHealingInProgress ANTES de DeactivateMedicMode → StopCoroutine nun
- CR-01-12 ✅ · 🟠 B — RenderEcgTexture aloca ~50KB (Color32[12600]) por frame com a HUD médica aberta
- CR-01-13 ✅ · 🟠 B — Sem hook de fim de raid: AudioListener.volume fica ~0.05 no menu quando a raid termina durante black
- CR-01-14 ✅ · 🟠 B — PatchAll sem try/catch no Awake + TargetMethod que retorna null → Awake morre e o patching fica parc
- CR-01-15 ✅ · 🟡 C — ShoulderTapKey/ShoulderTapMode são configs mortas e a descrição de MedicInteractKey ficou obsoleta a
- CR-01-16 · 🟡 C — Três keybinds default na MESMA tecla F que também é a interação nativa do jogo — apertar F durante c
- CR-01-17 · 🟡 E — Três namespaces no mesmo assembly (Band_Aid ×11, TrueTrauma ×8, TRLImmersiveCombatMedicine ×5) + enu
- CR-01-18 · 🟡 E — FaintController é classe morta (nunca anexada) com a mesma lógica duplicada inline no Plugin.Update
- CR-01-19 ✅ · 🟡 B — Bot que desmaia entra em FaintedPlayerIds e nunca sai — fica permanentemente 'invisível' para os out
- CR-01-20 ✅ · 🟡 B — Dupla aplicação e duplo consumo quando o paciente é local (host→bot): DoMedEffect redirecionado + Ap
- CR-01-21 · 🟡 C — Peer com desfibrilador pode 'reviver' um jogador apenas desmaiado — estados divergem entre dono e pe
- CR-01-22 · 🟡 C — Fallback do ItemDatabase transforma qualquer item desconhecido em 'medkit de 50HP'
- CR-01-23 · 🟡 C — Consumo parcial de HpResource/Resource é mutação local sem transação de rede — server e peers não sa
- CR-01-24 · 🟡 C — HealRoutine ignora o resultado de SetInHands: se falhar, a cura 'telepática' completa sem item nas m
- CR-01-25 · 🟡 B — BotUpdateManualPatch aloca List<IPlayer> por bot por tick enquanto houver qualquer desmaiado
- CR-01-26 · 🟡 F — MainLoopPatch faz 4-8 consultas de saúde + ~6 lookups de dicionário por player/bot por frame, mesmo 
- CR-01-27 ✅ · 🟡 B — DoContusion(1f,1f) re-disparado TODO frame durante o blackout (vanilla usa por evento)
- CR-01-28 · 🟡 C — MainLoopPatch nunca roda para players remotos: ObservedPlayer.LateUpdate não chama base (AP-03)
- CR-01-29 · 🟡 F — BandAidUI.HasEffect faz MakeGenericMethod + Invoke + new object[] ~49×/250ms com a HUD aberta
- CR-01-30 · 🟡 B — MedicHealPatch._currentObservedMedsControllerClass sobrevive ao fim da raid — pina o meds controller
- CR-01-31 · 🟡 C — Pipeline médico roda no hideout sem guard `is HideoutPlayer` (obrigatório pela skill §2)
- CR-01-32 · 🟡 D — Teardown fragmentado em 3 mecanismos (prefix OnGameStarted, polling _lastGameWorld, OnRaidEnded enca
- CR-01-33 · 🟡 B — Corpos de patch sem try/catch — prefix de ApplyDamageInfo deref HealthController sem null-check e Ma
- CR-01-34 · 🟡 C — Alvos ofuscados resolvidos por nome literal (method_5/6/8/9, method_15, GInterface###) sem predicado
- CR-01-35 · 🟢 E — Campos mortos confirmados pelo build: _harmony (CS0169) e trio shoulder-tap (CS0414) no BandAidContr
- CR-01-36 · 🟢 E — using duplicado em BandAidUI.cs (CS0105)
- CR-01-37 · 🟢 E — csproj: <Reference mscorlib> gera MSB3245/MSB3243 todo build; LangVersion não pinado; AssemblyName d
- CR-01-38 · 🟢 E — 17 warnings Harmony003 em HealthPatches — falso positivo do analyzer em leituras de DamageInfoStruct
- CR-01-39 · 🟢 F — Mod não tem README.md, mod.json nem backlog/ — abaixo do padrão dos mods irmãos do repo
- CR-01-40 · 🟢 F — Git hygiene do acervo legado: 9 arquivos .cs.txt versionados em 'TrueTrauma - FINALIZADO/ARQUIVOS DO
- CR-01-41 · 🟢 E — Whitelist de frases 'sinal secreto de rede' no SilenceVoicePatch é vestígio do 3.11 — nada escuta On
- CR-01-42 · 🟢 F — ImageLoader.Load sem cache negativo: File.Exists (I/O de disco) repetido para sprites ausentes
- CR-01-43 · 🟢 F — CheckPressMode: shortcut.Modifiers.Any() aloca iteradores LINQ por frame nos modos médico/cura
- CR-01-44 · 🟢 F — MedicHealPatch.Prefix resolve AccessTools.Field/Method a cada chamada e loga Warning incondicional p
- CR-01-45 · 🟢 F — Plugin.Update escreve AudioListener.volume todo frame em raid, mesmo com intensidade ~0
- CR-01-46 · 🟢 F — BandAidUI.Awake roda I/O de disco + FindObjectsOfTypeAll + construção de canvas inteiro no boot do j
- CR-01-47 · 🟢 F — ImageLoader: cache estático de Texture2D/Sprite nunca liberado — ClearCache() existe mas não tem nen
- CR-01-48 · 🟢 E — Reflection não cacheada em MedicalLogic/BandAidNetworkHandler: GetMethods+MakeGenericMethod por cham
- CR-01-49 · 🟢 E — VoiceHelper.SafePlayVoice usa GetMethod("Say")+Invoke por chamada quando a chamada tipada direta é p

## Achados 🔴/🟠 (formato completo)

### CR-01-01 · A — Crítico · 🔴 Bloqueador

**Curar BOT como médico CLIENTE é impossível: handshake nunca respondido e ApplyFullTreatment restrito ao MainPlayer receptor**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:505` (dim: coop)

```csharp
// Só o paciente processa
if (mainPlayer == null || packet.PatientProfileId != mainPlayer.ProfileId) return;
```

**Problema:** No cenário prioritário (2 players + bots, médico é cliente): no client o bot é ObservedPlayer sem ActiveHealthController → ProcessHeal entra no caminho de handshake (BandAidController.cs:305-320). O HealCheck chega ao host, mas o handler exige `PatientProfileId == mainPlayer.ProfileId` — um bot nunca é MainPlayer de ninguém → ninguém responde → timeout de 3s ('Sem resposta do paciente', BandAidController.cs:214-222). Mesmo que a cura iniciasse, MedicalLogic.ApplyTreatment (cs:81-82) envia pacote FullTreatment com campos zerados, e o handler só aplica FullTreatment quando o RECEPTOR é o paciente (cs:148) — o host cai no branch específico com HealAmount=0 e flags false → no-op. Bônus: o item do médico é consumido/descartado ANTES (MedicalLogic.cs:78).

**Por que importa:** Quebra de coop direta: cliente não consegue curar bots companheiros (uso central do mod no servidor Fika Coop PVE) e ainda perde o item médico sem efeito algum. Solo-host mascara 100% (no host o bot tem ActiveHC e segue o caminho local).

**Ref vanilla:** `BandAidNetworkHandler.cs:148 (`if (packet.ApplyFullTreatment && packet.PatientProfileId == localPlayer.ProfileId)`) — segundo gate do mesmo gap`

**Sugestão:** No host, assumir autoridade sobre bots: (1) em OnHealCheckReceived, se o paciente não é MainPlayer de ninguém mas FindPatient devolve Player com ActiveHealthController local (bot), o host valida com MedicalLogic.CanUseItem e responde o HealCheck; (2) em OnBandAidHealPacketReceived, aplicar ApplyFullTreatmentLocally quando o paciente resolvido tem ActiveHealthController local, em vez de exigir PatientProfileId == localPlayer. Mover o consumo do item para depois da confirmação.

**Ajuste do verificador adversarial:** [A/blocker] Curar BOT como médico CLIENTE é impossível: handshake nunca respondido (BandAidNetworkHandler.cs:505 exige PatientProfileId == MainPlayer, e bot nunca é MainPlayer) → timeout de 3s (BandAidController.cs:218-227). Defeito latente adicional: mesmo se o handshake fosse respondido, o FullTreatment enviado por MedicalLogic.ApplyTreatment (cs:81-82, campos zerados) é no-op no host (BandAidNetworkHandler.cs:148 só aplica no próprio paciente), e o item seria consumido antes (cs:78). Ajuste de escopo: no fluxo ATUAL o item NÃO é perdido — o timeout limpa o estado pendente sem consumir; a pe

**Duplicatas consolidadas:** correctness: Cliente Fika não consegue curar bots: handshake exige que o paciente responda e 

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). Host/headless agora respondem o handshake E aplicam FullTreatment em nome de bots locais (TryAnswerForLocalBot/TryApplyFullTreatmentOnLocalBot). Validação in-game pendente.

---

### CR-01-02 · C — Gap vs. spec · 🔴 Bloqueador

**Desmaio não é sincronizado com o host — FikaPacketManager/TraumaFaintPacket do 3.11 não foi migrado (confirmado)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Fika/FikaBridge.cs:12` (dim: coop)

```csharp
public static void SyncFaintStatus(Player player, bool isFainted)
{
    if (player == null) return;

    // Atualiza localmente
    UpdateFaintedList(player.ProfileId, isFainted);
```

**Problema:** SyncFaintStatus só atualiza a lista LOCAL — nenhum pacote é enviado (o comentário em HealthPatches.cs:74 'Sincroniza via Fika' é falso). Como ObservedPlayer.ApplyDamageInfo não chama base, o host nunca computa o blackout de um cliente: FaintedPlayerIds/BlackoutTimers do cliente desmaiado existem só na máquina dele. Todos os BotPatches (IsEnemy/IsPlayerEnemy/AddEnemy/CalcGoal/UpdateManual) e o AggroHelper rodam no host com lista vazia → bots continuam mirando e atirando no desmaiado; NeutralizeAggro no client é no-op (bots observados não têm BotOwner). O ragdoll visual sincroniza por acaso (ToggleDowned → DownedSyncPacket do Fika), mascarando o gap. O mod 3.11 tinha exatamente esse fluxo em mods/TrueTrauma - FINALIZADO/FikaPacketManager.cs:71-118 (host recebe, popula timers, NeutralizeAggro, re-broadcast).

**Por que importa:** A mecânica central do trauma (bots ignoram o desmaiado + grace pós-acordar) não funciona para NENHUM jogador não-host. O desmaiado não morre (ToggleDowned seta DamageCoeff 0), mas bots esvaziam carregadores nele e re-engajam instantaneamente no wake — comportamento visivelmente quebrado em coop e invisível em teste solo-host.

**Ref vanilla:** `fika-plugin Fika.Core/Main/Players/ObservedPlayer.cs:570-577 (override de ApplyDamageInfo SEM base-call → o postfix de HealthPatches nunca dispara no host para peers)`

**Sugestão:** Migrar o TraumaFaintPacket do 3.11: registrar o pacote em BandAidNetworkHandler.CheckInit (mesma infra já usada pelos pacotes Band-Aid); no SyncFaintStatus enviar o pacote; no host, ao receber, popular BlackoutTimers/BlackoutStartTimes/GraceTimers/FaintedPlayerIds e chamar NeutralizeAggro/RestoreAggro (copiar OnTraumaFaintPacketReceivedOnServer do arquivo antigo, adaptando SendDataToAll → SendData/relay atual).

**Duplicatas consolidadas:** patches: Faint nunca chega ao host em coop: SyncFaintStatus é local-only e ObservedPlayer

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). TraumaFaintPacket migrado do 3.11 e ligado ao stack de rede do mod (CheckInit + relay + espelhamento de timers + NeutralizeAggro no dono dos bots). Validação in-game pendente.

---

### CR-01-03 · A — Crítico · 🔴 Bloqueador

**Auto-close de 1.0m na BandAidUI mata o modo médico ativado pelo prompt nativo (2.5m) e aborta curas a >1m**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:681` (dim: correctness)

```csharp
float dist = Vector3.Distance(mainPlayer.Position, _targetPlayer.Position);
if (dist > 1.0f)
{
    Logger.LogInfo($"Paciente distante ({dist:F1}m > 1.0m), fechando HUD.");
    HideUI();
    BandAidController.Instance.DeactivateMedicModeExternal();
```

**Problema:** O refactor de hoje estabeleceu regra única de 3.5m no BandAidController.Update (linhas 193-197: "se o prompt apareceu, examinar funciona e o modo não fecha sozinho no mesmo lugar"), e o prompt nativo aparece até ~2.5m (PLAYER_RAYCAST_DISTANCE, comentário do MedicPlayerRangePatch). Mas BandAidUI.Update mantém um auto-close antigo a 1.0m que chama DeactivateMedicModeExternal() a cada 0.25s. Ativar 'Examinar (Médico)' entre 1.0m e 2.5m abre o painel e ele fecha sozinho em até 0.25s; durante uma cura com o médico a >1m, DeactivateMedicMode roda o branch de cleanup (StopCoroutine + CancelApplyingItem) e aborta o tratamento no meio.

**Por que importa:** Anula na prática o refactor do prompt F entregue hoje (o painel só sobrevive colado no paciente) e aborta silenciosamente curas legítimas — também impede que o fluxo completo (HealRoutine→ApplyTreatment) rode a distâncias em que o prompt nativo funciona. É a checagem que efetivamente governa o modo, não a de 3.5m do controller.

**Sugestão:** Remover o bloco de auto-close da BandAidUI.Update (deixar a regra única de 3.5m do BandAidController.Update decidir o fechamento), ou no mínimo igualar o threshold a 3.5f lendo de uma constante compartilhada no controller. Se o objetivo era fechar só o HUD sem matar o modo, remover a chamada a DeactivateMedicModeExternal.

**Duplicatas consolidadas:** coop: Auto-close da UI a 1,0m mata o modo médico aberto pelo prompt nativo (que alcanç

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (hotfix na mesma sessão): `BandAidUI.cs` auto-close agora usa `MedicInteractDistance + 1f` — regra única com o controller. Verificado por forense no DLL implantado.

---

### CR-01-04 · A — Crítico · 🔴 Bloqueador

**Consumo do desfibrilador via reflection com assinatura errada — exceção certa dentro do Prefix quebra o revive do Fika**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/FikaRevivePatch.cs:87` (dim: coop)

```csharp
var method = inventoryController.GetType().GetMethod("TryRunNetworkTransaction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
if (method != null)
{
    method.Invoke(inventoryController, new object[] { discardResult.Value });
}
```

**Problema:** O método real tem 2 parâmetros (operationResult, callback=null). `MethodInfo.Invoke` NÃO preenche defaults → Invoke com array de 1 elemento lança TargetParameterCountException; além disso `discardResult.Value` não é o GStruct153 esperado (MedicalLogic.DiscardItemNetworked, no mesmo mod, passa `discardResult` inteiro na chamada tipada — cs:473). Nem ConsumeDefibrillator nem o Prefix têm try/catch → a exceção estoura dentro do callback de Plant do Fika (ReviveInteractable.RevivePlayer é o `_revivePlayerDelegate` — fika ReviveInteractable.cs:202) e o corpo original de RevivePlayer nunca executa.

**Por que importa:** Com defibrilador no inventário (o único cenário em que o gate do mod permite reviver), completar o revive lança exceção: o jogador caído fica preso em BeingRevived/ragdoll (ToggleRevive(false) e RemoveRagdoll nunca rodam) e o defib nem é consumido. Feature de revive do coop quebra exatamente quando usada 'certo'.

**Ref vanilla:** `EFT/Player.cs:952 (`public override Task<IResult> TryRunNetworkTransaction(GStruct153 operationResult, Callback callback = null)`) — 2 parâmetros`

**Sugestão:** Substituir a reflection por chamada tipada idêntica à de MedicalLogic.DiscardItemNetworked: `var r = InteractionsHandlerClass.Discard(defib, inventoryController); if (r.Succeeded) _ = inventoryController.TryRunNetworkTransaction(r);` e envolver o Prefix em try/catch com log. Considerar mover o consumo para um Postfix (só consumir se o revive de fato completou).

**Ajuste do verificador adversarial:** Impacto: manter no mínimo strong; defensável elevar a blocker (quebra determinística do revive coop no único cenário que o gate do mod permite — 100% repro). Correção de detalhe no cenário: em vez de 'o defib nem é consumido', o correto é 'o defib é removido localmente pelo Discard (que executa a operação na hora) mas a transação nunca é enviada à rede — desync de inventário com o host, além do jogador preso em BeingRevived'. Fix sugerido permanece: chamada tipada idêntica a MedicalLogic.DiscardItemNetworked (cs:470-473) + try/catch no Prefix; idealmente mover consumo para Postfix condicionado

**Duplicatas consolidadas:** correctness: FikaRevivePlayerPatch: Invoke de TryRunNetworkTransaction com 1 argumento (métod; patches: FikaRevivePlayerPatch: prefix sem try/catch com reflection por chamada — exceção

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). Consumo do desfibrilador por chamada tipada TryRunNetworkTransaction(discardResult) + prefix inteiro em try/catch (revive nunca morre por nossa conta). Validação in-game pendente.

---

### CR-01-05 · B — Bug latente · 🟠 Forte

**csproj mistura NuGet UnityEngine.Modules 2019.4.39 (era SPT 3.11/Unity 2019) com DLLs 2022.3 do jogo — vencedor decidido módulo a módulo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRL-ImmersiveCombatMedicine.csproj:19` (dim: conventions)

```csharp
<PackageReference Include="UnityEngine.Modules" Version="2019.4.39" IncludeAssets="compile" />
...
<Reference Include="UnityEngine.CoreModule">
  <HintPath>References\UnityEngine.CoreModule.dll</HintPath>
```

**Problema:** O log diagnóstico do ResolveAssemblyReferences (dotnet msbuild -t:ResolveAssemblyReferences -v:diag, rodado hoje) prova o split: os 4 módulos declarados como <Reference> local (UnityEngine, CoreModule, UI, UIModule) resolvem para References\ do jogo 2022.3 (ResolvedFrom={CandidateAssemblyFiles}); TODOS os demais módulos que o código usa — InputLegacyModule (Input.GetMouseButtonDown no BandAidController:178), AudioModule (AudioListener.volume no Plugin:164/232), PhysicsModule, ImageConversionModule (ImageLoader), TextRenderingModule (Font/Text do BandAidUI) — resolvem para C:\Users\guime\.nuget\packages\unityengine.modules\2019.4.39 (ResolvedFrom={HintPathFromItem}). O jogo roda Unity 2022.3; a superfície de compilação é metade 2019.4. O pacote 2019.4.39 é herança direta do csproj do Band-Aid 3.11 (SPT 3.x usava Unity 2019.4).

**Por que importa:** Dois riscos concretos: (1) API presente em 2019.4 mas removida/movida de módulo em 2022.3 compila limpo e explode em runtime com MissingMethodException/TypeLoadException — sem nenhuma proteção em compile-time; (2) tipo que migrou de módulo entre versões gera IL apontando para o assembly errado. O split é invisível (nenhum warning MSB3243 para UnityEngine, porque as identidades 0.0.0.0 são idênticas) e muda silenciosamente conforme se adiciona/remove <Reference> local. Também viola a skill csharp-mod-best-practices §9 (referências do jogo via References/ resolvidas pelo /compile-mod).

**Sugestão:** Remover a PackageReference UnityEngine.Modules do csproj e declarar explicitamente cada módulo usado como <Reference Private="false"> apontando para References\ (a pasta já contém InputLegacyModule, PhysicsModule, ImageConversionModule, TextRenderingModule, AnimationModule etc., populados pelo /compile-mod) — mesmo padrão do TRL-ImmersiveScopes.csproj deste repo. Recompilar e smoke-testar em raid depois da troca.

**Ajuste do verificador adversarial:** Impacto mantido em strong. Ajuste na sugestão de fix: além de remover a PackageReference UnityEngine.Modules 2019.4.39 e declarar cada módulo usado como <Reference Private="false"> apontando para References\, é preciso garantir que /compile-mod também popule UnityEngine.AudioModule.dll (usado por AudioListener.volume no Plugin:164/232) — esse módulo NÃO está na pasta References\ hoje (diferente de InputLegacyModule, PhysicsModule, ImageConversionModule e TextRenderingModule, que já estão). Padrão de referência: mods/TRL-ImmersiveScopes/TRL-ImmersiveScopes.csproj (na raiz do mod). Recompilar e 

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). Nuget UnityEngine.Modules 2019.4.39 e mscorlib removidos; módulos Unity 2022.3 reais referenciados de References\ (resolvidos de D:\SPT). Validação in-game pendente.

---

### CR-01-06 · C — Gap vs. spec · 🟠 Forte

**Mojibake UTF-8 duplo-encodado em 3 arquivos (86 ocorrências) — vaza para strings visíveis ao jogador (F12, notificações, UI do painel médico)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:48` (dim: conventions)

```csharp
ConfigLegsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Pernas", true, "Cair no chÃ£o ao perder as pernas.");
ConfigArmsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de BraÃ§os", true, "Perder a mira ao perder os braÃ§os.");
```

**Problema:** Três arquivos foram salvos com conteúdo duplo-encodado (UTF-8 lido como Latin-1 e re-salvo como UTF-8): TRLImmersiveCombatMedicinePlugin.cs (11 ocorrências de sequência \xc3\x83\xc2), BandAidController.cs (41), BandAidUI.cs (34). Os arquivos são UTF-8 VÁLIDO (sem risco de compilação — `file` confirma, build passa), mas os caracteres errados estão baked no fonte e vão verbatim para o binário. Não é só comentário: atinge strings de usuário — descrições de Config.Bind no F12 (Plugin.cs:48-59: 'chÃ£o', 'BraÃ§os', 'animaÃ§Ã£o'), notificação in-game (BandAidController.cs:327: '$"{stats.Name}: Sem ferimento compatÃ­vel."'), texto renderizado no painel médico (BandAidUI.cs:672: '_subtitleText.text = "INDISPONÃVEL"') e ~10 mensagens de log. Os demais 23 .cs do mod estão com UTF-8 correto (ex.: MedicalLogic.cs:116 'REMOÇÃO' íntegro).

**Por que importa:** Jogador vê 'Cair no chÃ£o', 'INDISPONÃVEL' e 'Sem ferimento compatÃ­vel' na tela — qualidade percebida do mod despenca; logs mojibake também dificultam grep de diagnóstico ('MÃ©dico morreu' não casa com 'Médico').

**Sugestão:** Corrigir o texto (não o encoding do arquivo — ele já é UTF-8): reconverter os 3 arquivos com iconv reverso (utf8→latin1 reinterpretado como utf8: `iconv -f utf-8 -t latin1 arquivo | sbcs-check` ou script equivalente em Python `bytes(txt,'latin1').decode('utf8')`), conferir diff manualmente e recompilar. Prioridade nas strings de usuário: Plugin.cs:48-59, BandAidController.cs:327, BandAidUI.cs:672.

**Ajuste do verificador adversarial:** Ajuste na SUGESTÃO de fix, não no impacto: os 3 arquivos são MISTOS — contêm mojibake (texto antigo) E UTF-8 correto lado a lado (Plugin.cs:68 "PRÓPRIO"/"destrói", BandAidController.cs:66 "INÍCIO", comentários [DEBUG-ICM] recentes com "após"/"diagnóstico" corretos). Um iconv reverso no arquivo INTEIRO corromperia as strings já corretas (um "Ó" correto vira byte 0xD3 isolado em latin1 → UTF-8 inválido na re-decodificação). Fix seguro: `python -m ftfy` (repara só as sequências quebradas, preserva o resto) ou substituição direcionada por regex das sequências mojibake (Ã£→ã, Ã§→ç, Ã­→í, Ã³→ó, Ã‡→Ç

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). 92 linhas duplo-encodadas reparadas (roundtrip cp1252→utf-8 por linha); zero mojibake restante. Validação in-game pendente.

---

### CR-01-07 · C — Gap vs. spec · 🟠 Forte

**PROPRIEDADES.md não existe — 13 ConfigEntries (incluindo a seção nova '5. Debug') sem documentação obrigatória**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:46` (dim: conventions)

```csharp
ConfigMasterEnabled = Config.Bind("1. Geral (Trauma)", "Ativar Mod", true, "Liga ou desliga todo o funcionamento do mod.");
ConfigBlackoutEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Desmaio", true, ...);
```

**Problema:** A raiz do mod tem só builds/, memory/ e modded/ — não há PROPRIEDADES.md. O mod expõe 13 ConfigEntries no F12: 6 de Trauma (seções 1-3), 6 de Keybinds (seção 4, Plugin.cs:54-59) e 1 de Debug ('5. Debug' → 'Invisivel para Bots', DebugBotInvisibility.cs:36-38). A regra do repo (skill repo-workflow-best-practices §7: 'Toda nova ConfigEntry exposta no F12 exige update em mods/<mod>/PROPRIEDADES.md') vale para todas; 11 outros mods do repo (AutoGym, CustomClasses, StanceSync, SPT-DynamicMaps...) têm o arquivo.

**Por que importa:** Sem single source das configs, ninguém sabe unidades, faixas e efeitos colaterais (ex.: 'Duracao do Desmaio' em segundos? interage com o GraceTimer +5s do HealthPatches:65?); e renomes futuros de seção/key — breaking change silencioso do BepInEx — passam sem changelog porque não há baseline documentada.

**Sugestão:** Criar mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md com a tabela padrão do repo (Nome EN, Tradução pt-BR, Tipo, Padrão, Faixa, Tooltip pt-BR) cobrindo as 13 entries, seguindo o modelo de AutoGym/PROPRIEDADES.md. Marcar 'Invisivel para Bots' explicitamente como DEBUG host-only. O /review-mod-properties pode validar depois.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). PROPRIEDADES.md criado (12 entries, 5 seções, ledger de removidas). Validação in-game pendente.

---

### CR-01-08 · B — Bug latente · 🟠 Forte

**Bridge EffectRemoved filtra a interface errada: GInterface350 em vez de GInterface376 (marcador real do med-effect)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:61` (dim: coop)

```csharp
private static void OnPatientEffectRemoved(IEffect effect)
{
    if (!(effect is GInterface350))
        return;
```

**Problema:** O method_8 vanilla — que o bridge replica — reage a `GInterface376` (efeito de medicação em progresso). O mod filtra `GInterface350` (interface diferente, usada em outro contexto de efeitos em Player.cs:28953/28991). Quando o MedEffect do paciente terminar, o bridge nunca notifica method_8 → a cadeia multi-membro e a finalização nativa da animação não rodam; pior, efeitos não relacionados que implementem GInterface350 disparariam o bridge indevidamente. Hoje o caminho está morto por causa do achado A1 (redirect nunca engata), mas vira o próximo bloqueador assim que A1 for corrigido.

**Por que importa:** Após corrigir o casing dos campos, curas com MedEffect real no paciente vão travar na transição de membro/finalização (dependendo só do fallback ForceFinishAnimation por timer, que usa UseTime fixo do ItemDatabase e não o tempo real da animação).

**Ref vanilla:** `EFT/Player.cs:19617 (method_8 real: `if (effect is GInterface376)`; Fika ObservedMedsController.cs:156 usa o mesmo GInterface376)`

**Sugestão:** Trocar para `GInterface376` (e documentar com `// ref: Player.cs:19617`); idealmente resolver a interface por predicado estável (a interface que MedsController usa em FindActiveEffect<GInterface376>, Player.cs:1369) em vez de número literal — AP-09.

**Ajuste do verificador adversarial:** [B/strong] Bridge EffectRemoved filtra a interface errada: GInterface350 (Berserk) em vez de GInterface376 (marcador real do med-effect). Local: mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:96 (OnPatientEffectRemoved). O method_8 vanilla que o bridge invoca reage a GInterface376 (Player.cs:19617); GInterface376 é o efeito de medicação (MedItem, UsingMeds — EffectsController.cs:483, Player.cs:28963), enquanto GInterface350 é o efeito Berserk (Player.cs:28991-28995, 29070) — o MedEffect não implementa GInterface350 (dispatch mutuamente exclusivo em Player.cs:28953). 

**Duplicatas consolidadas:** patches: Bridge do EffectRemovedEvent filtra GInterface350, mas o MedEffect real é GInter

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). Filtro do bridge trocado para GInterface376 (marcador real de MedEffect; Player.cs:19617). Validação in-game pendente.

---

### CR-01-09 · B — Bug latente · 🟠 Forte

**Estado de desmaio vaza no fim da raid: menu com áudio mudo e prone forçado no primeiro frame da raid seguinte (AP-01)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:168` (dim: coop)

```csharp
var gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
if (gameWorld == null || gameWorld.MainPlayer == null) return;
```

**Problema:** Se a raid termina durante um blackout (morte, extract, alt-F4), o Update do plugin passa a retornar na checagem de GameWorld ANTES das linhas que restauram `AudioListener.volume` (cs:231-232) → o volume fica ~0,05 no menu até a próxima raid. Além disso `TraumaState.IsFainted` (static) permanece true: OnRaidStartCleanup não o limpa, então no primeiro Update da raid seguinte o else (cs:219-228) executa `fikaPlayer.ToggleDowned(false)` (no-op com warning, guard do Fika) e força `MovementContext.IsInPronePose = true` no MainPlayer recém-spawnado. Não há hook de fim de raid (GameWorld.OnDestroy/BaseLocalGame.Stop), só o prefix de início.

**Por que importa:** Sintomas visíveis e confusos: menu inteiro quase mudo após morrer desmaiado, e spawn deitado sem motivo na raid seguinte — bugs 'fantasma' clássicos de estado estático entre raids.

**Ref vanilla:** `TRLImmersiveCombatMedicinePlugin.cs:129-141 — OnRaidStartCleanup limpa dicionários e EffectIntensity mas NÃO reseta TraumaState.IsFainted nem AudioListener.volume`

**Sugestão:** Adicionar patch idempotente em GameWorld.OnDestroy (e/ou BaseLocalGame.Stop) que chame um TraumaState.ResetAll(): limpar IsFainted, EffectIntensity=0, AudioListener.volume=1 e os dicionários; incluir IsFainted também no OnRaidStartCleanup como cinto-e-suspensório.

**Ajuste do verificador adversarial:** Mesmo achado e impacto [B/strong], com correções pontuais: checagem de GameWorld/MainPlayer em Plugin.cs:175-176 (não 168); restore de volume em Plugin.cs:239 (não 231-232); wake indevido pode ocorrer por DOIS caminhos — else Plugin.cs:224-236 (pós-cleanup) OU branch de dict stale Plugin.cs:207-221 durante o loading (ProfileId estável + Time.time monotônico fazem timeElapsed > duration), ambos com ToggleDowned(false) no-op-com-warning (FikaPlayer.cs:510-515) + IsInPronePose=true forçado. Remover "alt-F4" dos exemplos (encerra o processo, statics zeram); cenários válidos: MIA por timer, host Fi

**Duplicatas consolidadas:** lifecycle: TraumaState.IsFainted nunca é resetado entre raids → próxima raid começa com Tog; patches: OnRaidStartCleanup não limpa LegPenaltyTimers nem IsFainted — estado de trauma v

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). TraumaState.ResetAll() cobre todos os campos (incl. IsFainted/LegPenaltyTimers) e roda no início E no fim de raid (polling de GameWorld no controller). Validação in-game pendente.

---

### CR-01-10 · B — Bug latente · 🟠 Forte

**Handshake pendente (_pendingHeal*) não é limpo em DeactivateMedicMode/ResetAllState → resposta tardia inicia cura com modo fechado e vaza Player entre raids**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:87` (dim: correctness)

```csharp
// Verificar se temos um check pendente
if (_pendingHealTimeout < 0 || _pendingHealItem == null || _pendingHealPatient == null) return;
```

**Problema:** O timeout de 3s só é processado dentro do bloco `if (_isMedicModeActive && _targetPatient != null)` do Update (linhas 214-222), e nem DeactivateMedicMode (715-748) nem ResetAllState (761-781) limpam _pendingHealItem/_pendingHealStats/_pendingHealPatient/_pendingHealTimeout. Cenário: médico envia HealCheck a um paciente remoto, e antes da resposta o modo fecha (afastou >3.5m, apertou F, ou o auto-close de 1.0m da UI). Os campos pendentes ficam setados com _pendingHealTimeout ainda positivo → quando a resposta Approved chega (até 3s depois, rede lenta ou relay do host), o guard acima PASSA e OnHealCheckResponseHandler inicia HealRoutine com o modo médico desativado e o paciente a qualquer distância. Entre raids, ResetAllState não limpa → _pendingHealPatient segura o Player do raid anterior (pin do grafo do raid, AP-01) e uma resposta duplicada/atrasada iniciaria HealRoutine sobre um Player destruído.

**Por que importa:** Cura 'fantasma' a 10m+ com painel fechado (imobiliza o médico com UsingMeds, consome item, aplica tratamento remoto), e leak/uso de Player destruído cross-raid com exceções no HealRoutine.

**Sugestão:** Extrair um `ClearPendingHeal()` que zera os 5 campos e chamá-lo em DeactivateMedicMode, ResetAllState e no próprio handler; em OnHealCheckResponseHandler, revalidar antes de iniciar: `_isMedicModeActive && _targetPatient == _pendingHealPatient && Vector3.Distance(...) <= 3.5f`.

**Ajuste do verificador adversarial:** Impacto mantido em strong. Ajuste no cenário cross-raid: resposta duplicada/atrasada NÃO alcança o próximo raid (NetworkManager Fika é destruído entre raids — BandAidNetworkHandler.CheckInit M1, linhas 47-51 — e ReliableOrdered não duplica), então não há HealRoutine sobre Player destruído; o dano cross-raid é o pin de memória do Player/Item do raid anterior (AP-01) + notificação espúria de timeout na próxima ativação do modo médico. Em contrapartida, o dano in-raid é PIOR que o descrito: como o timeout só é processado com o modo ativo, o pending nunca expira após fechar o modo — a cura fantasm

**Duplicatas consolidadas:** coop: Handshake pendente sobrevive ao fechamento do modo médico — resposta atrasada in; lifecycle: Estado pendente do handshake (_pendingHeal*) não é limpo no ResetAllState → Play

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (junto com G-1/G-5 da coop-heal-matrix): timeout do handshake movido para o nível do Update (avaliado sempre), e `_pendingHeal*` limpo em `DeactivateMedicMode` e `ResetAllState`. Validação in-game pendente (protocolo da matriz coop).

---

### CR-01-11 · B — Bug latente · 🟠 Forte

**Abort por distância >3.5m zera _isHealingInProgress ANTES de DeactivateMedicMode → StopCoroutine nunca roda e o tratamento aplica mesmo após 'Abortado!'**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:199` (dim: correctness)

```csharp
if (_isHealingInProgress)
{
    _isHealingInProgress = false;
    _itemBeingUsed = null;
    MedicHealPatch.IsRedirectingHeal = false;
    MedicHealPatch.CurrentPatient = null;
    NotificationManagerClass.DisplayMessageNotification("Abortado!", ...);
}
DeactivateMedicMode();
```

**Problema:** Este caminho seta `_isHealingInProgress = false` e só DEPOIS chama DeactivateMedicMode() — cujo cleanup inteiro (StopCoroutine, CancelApplyingItem, ForceFinishAnimation, liberar UsingMeds) é gated por `if (_isHealingInProgress)` (linha 718). Resultado: nesse caminho o cleanup é código morto — a HealRoutine continua rodando, `MedicHealPatch.BandAidHealActive` fica true (bloqueando method_5 vanilla nesse meio-tempo), CleanupPatientSubscription não é chamado, e após WaitForSeconds(UseTime+2) a coroutine aplica MedicalLogic.ApplyTreatment no paciente a qualquer distância e mostra 'Tratamento Completo.' logo após o 'Abortado!'.

**Por que importa:** Cura à distância contradizendo o abort mostrado ao usuário. Hoje é latente porque o auto-close de 1.0m da BandAidUI (outro achado) intercepta antes e faz o cleanup correto — mas vira bug vivo assim que o threshold da UI for corrigido/removido, que é exatamente o fix recomendado.

**Sugestão:** Inverter a ordem: chamar DeactivateMedicMode() (que já faz StopCoroutine + cleanup completo quando _isHealingInProgress==true) e remover o reset manual das flags deste bloco, mantendo só a notificação 'Abortado!'.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). Abort por distância não zera mais _isHealingInProgress antes do DeactivateMedicMode — cleanup gated volta a executar. Validação in-game pendente.

---

### CR-01-12 · B — Bug latente · 🟠 Forte

**RenderEcgTexture aloca ~50KB (Color32[12600]) por frame com a HUD médica aberta**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:807` (dim: hotpath)

```csharp
private void RenderEcgTexture()
{
    Color32[] pixels = new Color32[ECG_WIDTH * ECG_HEIGHT];
```

**Problema:** UpdateEcg() roda todo frame enquanto o canvas está ativo (BandAidUI.Update:661) e chama RenderEcgTexture() (linha 787) praticamente todo frame (pixelsPerSec≈97 → steps>=1 a 60fps). Cada chamada aloca um Color32[360*35] novo (~50KB), redesenha 12.600 pixels em CPU (incl. loop de glow 33x33 com Sqrt) e faz SetPixels32+Apply (upload GPU).

**Por que importa:** ~3 MB/s de lixo GC gen0 durante toda cura/exame em raid → coletas frequentes = hitches perceptíveis exatamente no momento de tensão (curando aliado em coop). Viola checklist csharp §1/spt §3 (sem new por frame em hot path).

**Sugestão:** Alocar o buffer uma única vez em CreateEcg (campo _ecgPixels reutilizado; ClearEcgTexture já pode preenchê-lo) e reutilizar em RenderEcgTexture. Opcional: limitar o re-render a ~30Hz (acumular steps) e chamar Apply só quando houver mudança. Adicionar OnDestroy destruindo _ecgTexture.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). RenderEcgTexture reusa buffer Color32 (fim dos ~50KB/frame). Validação in-game pendente.

---

### CR-01-13 · B — Bug latente · 🟠 Forte

**Sem hook de fim de raid: AudioListener.volume fica ~0.05 no menu quando a raid termina durante blackout**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:169` (dim: lifecycle)

```csharp
var gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
if (gameWorld == null || gameWorld.MainPlayer == null) return;
```

**Problema:** A única via que restaura o volume é a linha 232 (`AudioListener.volume = Mathf.Lerp(1f, 0.05f, TraumaState.EffectIntensity)`), que só executa COM GameWorld+MainPlayer vivos. Não existe nenhum hook de fim de raid (GameWorld.OnDestroy / BaseLocalGame.Stop) que zere EffectIntensity e restaure o volume — o mod só patcheia o INÍCIO (OnGameStarted). AudioListener.volume é estático global do Unity e sobrevive ao unload da cena.

**Por que importa:** Cenário concreto: jogador desmaia (blackout, volume vai a ~0.05), morre sangrando enquanto desmaiado (DoT não passa pelo escudo do DamageTriggerPatch, que só bloqueia Bullet/Explosion/etc.) ou dá alt-F4 → tela de morte, menu e matchmaking inteiros ficam com áudio a 5% até a próxima raid começar (OnRaidStartCleanup zera EffectIntensity e o Lerp da linha 232 devolve 1.0). Regressão auditiva certa e reproduzível — AP-01 clássico.

**Ref vanilla:** `EFT/GameWorld.cs:2111`

**Sugestão:** Adicionar hook de fim de raid conforme skill spt-mod-best-practices §2: postfix em GameWorld.OnDestroy (EFT/GameWorld.cs:2111, virtual com corpo vazio — seguro) e/ou BaseLocalGame.Stop (BaseLocalGame.cs:1018), chamando um RaidEnd() idempotente que faça `TraumaState.EffectIntensity = 0f; AudioListener.volume = 1f;` além da limpeza dos dicionários (ver achado do OnRaidStartCleanup).

**Ajuste do verificador adversarial:** [B/strong] Sem hook de fim de raid: AudioListener.volume fica ~0.05 no menu quando a raid termina durante blackout. Cenário corrigido: jogador desmaia (volume→~0.05 via linha 232) e morre durante o blackout por tipo de dano NÃO bloqueado pelo escudo de HealthPatches.cs:21-25 (sangramento DoT, melee de bot, fogo — o escudo só cobre Bullet/Explosion/GrenadeFragment/Landmine/Sniper); a raid encerra antes do blackout de 20s expirar → GameWorld destruído → Update retorna cedo (linha 169) para sempre → menu/matchmaking com áudio a 5% até a próxima raid (OnRaidStartCleanup). Nota: alt-F4 NÃO reproduz

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). AudioListener.volume=1 restaurado no fim de raid (ResetAllState) e no início (OnRaidStartCleanup). Validação in-game pendente.

---

### CR-01-14 · B — Bug latente · 🟠 Forte

**PatchAll sem try/catch no Awake + TargetMethod que retorna null → Awake morre e o patching fica parcial e não-determinístico**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:86` (dim: patches)

```csharp
_harmony = new Harmony("com.trl.immersivecombatmedicine");
_harmony.PatchAll();
```

**Problema:** PatchAll não está protegido. Três classes de patch podem fazê-lo lançar: FikaRevivePatch.cs:19-21 retorna null quando o tipo Fika não resolve (a intenção era degradar graciosamente, mas TargetMethod null faz o Harmony lançar HarmonyException e ABORTAR o PatchAll inteiro); AnimCleanupPatch (MedicHealPatch.cs:332-338) retorna AccessTools.Method(...) sem null-guard; MedicActionsPatch.cs:19-22 usa .First() que lança InvalidOperationException se a assinatura de GetAvailableActions mudar num update do EFT. Quando qualquer uma falha, as classes ainda não processadas (ordem de GetTypes() é não-especificada) ficam sem patch, e o resto do Awake (patch manual de OnGameStarted, registro de pacotes Fika, handler de handshake — linhas 88-116) nunca executa, deixando o plugin meio-inicializado (componentes já adicionados ao GameObject).

**Por que importa:** Um update do EFT ou do Fika que renomeie um único alvo derruba o mod inteiro de forma opaca (BepInEx loga o erro do Awake, mas o jogador vê 'mod não funciona'), em vez de degradar só a feature afetada.

**Sugestão:** Substituir o PatchAll único por um loop de `new PatchClassProcessor(_harmony, type).Patch()` com try/catch+LogError por classe (ou no mínimo envolver PatchAll em try/catch e mover o registro Fika/handler para antes dele). Para os patches Fika, condicionar o processamento à presença do plugin (`Chainloader.PluginInfos.ContainsKey("com.fika.core")`) em vez de retornar null do TargetMethod. Adicionar null-guard no TargetMethod do AnimCleanupPatch.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). PatchAll substituído por PatchClassProcessor por classe com try/catch+log — falha isolada não aborta os demais patches. Validação in-game pendente.

---

## Achados 🟡/🟢 (formato compacto)

### CR-01-15 · C — Gap vs. spec · 🟡 Médio

**ShoulderTapKey/ShoulderTapMode são configs mortas e a descrição de MedicInteractKey ficou obsoleta após a refatoração para o pipeline nativo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:56` (dim: conventions)

**Problema:** Após a refatoração de hoje, o shoulder tap dispara exclusivamente pela ação 'Tocar no ombro' do ActionPanel nativo (MedicInteractable.cs:47-51 → SendShoulderTapExternal). As entries ShoulderTapKey/ShoulderTapMode continuam bindadas no F12 mas nunca são lidas: os wrappers _shoulderTapKey/_shoulderTapMode (BandAidController.cs:44,47) têm apenas a declaração (0 leituras — confirmado por grep), e o build acusa CS0414 nos timers correspondentes. Já MedicInteractKey ainda é usada, porém SÓ para FECHAR

**Sugestão:** Remover os binds ShoulderTapKey/ShoulderTapMode (ou reconectá-los se a intenção era manter atalho direto além do painel) e reescrever a descrição de MedicInteractKey/Mode para 'fecha o modo médico aberto pelo painel de interação'. Registrar no PROPRIEDADES.md novo. Atenção: remover entries de Config.Bind não quebra configs salvas (BepInEx só ignora), mas documente no changelog.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). ShoulderTap Key/Mode removidas + tooltip do MedicInteractKey atualizado; registradas no PROPRIEDADES.md. Validação in-game pendente.

---

### CR-01-16 · C — Gap vs. spec · 🟡 Médio

**Três keybinds default na MESMA tecla F que também é a interação nativa do jogo — apertar F durante cura dropa o item médico**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:54` (dim: conventions)

**Problema:** MedicInteractKey (Hold), EmergencyDropKey (Press) e ShoulderTapKey (DoubleTap, morta) usam F como default — a mesma tecla do prompt de interação nativo que a refatoração de hoje passou a usar para ABRIR o modo médico (ActionPanel). Cenário concreto: com _isHealingInProgress=true, qualquer toque em F (jogador tentando abrir porta pra recuar, lootear, ou por hábito) cai no CheckPressMode de EmergencyDropKey/Press (BandAidController.cs:170-175) e executa EmergencyDrop() → item médico vai pro chão n

**Sugestão:** Trocar o default de EmergencyDropKey para tecla sem colisão (ex.: X ou G) e revisar se MedicInteractKey(Hold-F) para fechar ainda faz sentido agora que a abertura é nativa — talvez fechar por distância (já existe, linha 197) + ESC seja suficiente. Se manter F, adicionar guarda que ignora o press que veio do ActionPanel (ex.: cooldown de 0.5s após ActivateMedicModeExternal).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-17 · E — Legibilidade · 🟡 Médio

**Três namespaces no mesmo assembly (Band_Aid ×11, TrueTrauma ×8, TRLImmersiveCombatMedicine ×5) + enum no namespace global**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:6` (dim: conventions)

**Problema:** A fusão dos dois mods 3.11 preservou os namespaces originais: 11 arquivos em Band_Aid (todo Patches/Medical + Helpers/ImageLoader + FikaRevivePatch), 8 em TrueTrauma (Patches/Trauma + Fika/FikaBridge + Helpers/AggroHelper), 5 no namespace do plugin (RootNamespace do csproj). Pior: EBandAidPressMode.cs:1 declara o enum no namespace GLOBAL ('public enum EBandAidPressMode { Press, Hold, DoubleTap }' sem namespace) — é por isso que os três namespaces o enxergam sem using. A organização física (Patch

**Sugestão:** Consolidar em TRLImmersiveCombatMedicine.{Medical,Trauma,Helpers,Fika,Debugging} num commit mecânico (find/replace de namespace + usings; Harmony PatchAll não depende de namespace). Mover EBandAidPressMode para TRLImmersiveCombatMedicine. Fazer isso ANTES de crescer o mod — o custo só aumenta.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-18 · E — Legibilidade · 🟡 Médio

**FaintController é classe morta (nunca anexada) com a mesma lógica duplicada inline no Plugin.Update**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/FaintController.cs:10` (dim: conventions)

**Problema:** Grep no mod inteiro: zero referências a FaintController fora do próprio arquivo — nenhum AddComponent<FaintController>, nenhum atributo Harmony. É código copiado do TrueTrauma 3.11 que nunca foi ligado. A lógica dele (ToggleDowned/BlackoutTimers/IsInPronePose) existe duplicada e DIVERGENTE em TRLImmersiveCombatMedicinePlugin.Update:174-229 (a versão viva usa BlackoutStartTimes+ConfigBlackoutDuration; a morta usa BlackoutTimers como expiry). Há mais restos da fusão: blocos comentados de Tournique

**Sugestão:** Deletar FaintController.cs (git preserva o histórico; o original segue em mods/TrueTrauma - FINALIZADO/). Aproveitar para remover os blocos comentados de BandAidNetworkHandler.cs:220-230 e BandAidUI.cs:1005 ou convertê-los em issue/backlog item se a feature (torniquete via rede) ainda é desejada.

**Duplicatas consolidadas:** coop: FaintController e TourniquetManager são dead code — nunca são AddComponent em lu; hotpath: FaintController e TourniquetManager são MonoBehaviours órfãos: nunca há AddCompo; lifecycle: FaintController e TourniquetManager são MonoBehaviours nunca anexados a GameObj

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-19 · B — Bug latente · 🟡 Médio

**Bot que desmaia entra em FaintedPlayerIds e nunca sai — fica permanentemente 'invisível' para os outros bots**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:86` (dim: coop)

**Problema:** A entrada em FaintedPlayerIds acontece para qualquer player que sofra trauma de tórax/cabeça (bots desmaiam por design), mas a ÚNICA remoção está no branch `!__instance.IsAI` do grace timer. O caminho de wake do bot (MovementPatches.cs:59-67) remove BlackoutTimers/StartTimes mas não FaintedPlayerIds nem GraceTimers. Resultado: todo bot que desmaiou uma vez permanece na lista até o fim da raid.

**Sugestão:** No branch de wake do bot (MovementPatches.cs:63-67), adicionar `TraumaState.FaintedPlayerIds.Remove(id)` e `GraceTimers.Remove(id)` (bots não precisam de grace); ou gate o SyncFaintStatus(true) em HealthPatches para `!__instance.IsAI` se o escudo de visão for intencional só para humanos.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). Wake de bot remove FaintedPlayerIds e GraceTimers. Validação in-game pendente.

---

### CR-01-20 · B — Bug latente · 🟡 Médio

**Dupla aplicação e duplo consumo quando o paciente é local (host→bot): DoMedEffect redirecionado + ApplyTreatment incondicional**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:590` (dim: coop)

**Problema:** Quando o redirect funciona (pós-fix do casing) e o paciente tem ActiveHealthController (bot no host), a cura acontece DUAS vezes: (1) o Prefix aplica o MedEffect nativo no paciente — que remove efeito, cura HP e consome HpResource do item pelo caminho vanilla; (2) o HealRoutine, após o UseTime, SEMPRE chama MedicalLogic.ApplyTreatment, que remove efeitos de novo (no-op), aplica ChangeHealth de novo e chama ConsumeSafe decrementando HpResource mais uma vez. O comentário em MedicalLogic.cs:401-403

**Sugestão:** Definir fonte única de verdade: quando o Prefix conseguir DoMedEffect no paciente (result != null), setar uma flag (ex.: MedicHealPatch.NativeEffectApplied) e o HealRoutine pular ApplyTreatment/consumo; manter ApplyTreatment apenas para os caminhos sem MedEffect (paciente remoto e DoMedEffect null).

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ CR-01-20 resolvido fora do fluxo automatizado (fix do Salewa, commit 25be6540: flag NativeMedEffectApplied evita dupla aplicação/consumo) — fechado na rodada 03.

---

### CR-01-21 · C — Gap vs. spec · 🟡 Médio

**Peer com desfibrilador pode 'reviver' um jogador apenas desmaiado — estados divergem entre dono e peers**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:188` (dim: coop)

**Problema:** O desmaio reusa o sistema downed do Fika: ToggleDowned(true) faz broadcast do DownedSyncPacket e os peers criam o ReviveInteractable (prompt de revive). Um peer com defib pode completar o revive → RevivedPlayerPacket força ToggleDowned(false) no desmaiado NO MEIO do blackout, enquanto BlackoutTimers/IsFainted continuam ativos no dono: input segue congelado (InputPatches), tela/áudio de desmaio continuam, e o defib do peer é gasto à toa; ao expirar o timer, o mod chama ToggleDowned(false) de novo

**Sugestão:** Duas opções: (a) suprimir o prompt de revive para desmaios do mod — com o packet de faint do achado 3 os peers sabem que o downed é 'faint' e o FikaReviveGetActionsPatch pode remover todas as ações; ou (b) tratar revive como wake antecipado: no Update, detectar ClientHealthController.Downed==false com BlackoutTimers ativo e limpar timers/IsFainted imediatamente.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-22 · C — Gap vs. spec · 🟡 Médio

**Fallback do ItemDatabase transforma qualquer item desconhecido em 'medkit de 50HP'**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Helpers/ItemDatabase.cs:76` (dim: coop)

**Problema:** A base cobre só 17 itens hardcoded. Qualquer outro item vinculado a slot rápido (morfina, Golden Star, Vaseline, analgésicos, meds de outros mods) passa por CanUseItem como se curasse 50 HP e, em coop, o paciente remoto aplica esses stats fabricados em si mesmo via ApplyFullTreatmentLocally (BandAidNetworkHandler.cs:194 usa ItemDatabase.GetStats do lado do paciente).

**Sugestão:** Retornar null no GetStats para item desconhecido e negar o uso com notificação ('item não suportado'); médio prazo, derivar os stats do template real (MedKitComponent.MaxHpResource, HealthEffectsComponent) em vez de tabela manual.

**Duplicatas consolidadas:** correctness: ProcessHeal aceita qualquer item bound como med: fallback 'Unknown' do ItemDa

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-23 · C — Gap vs. spec · 🟡 Médio

**Consumo parcial de HpResource/Resource é mutação local sem transação de rede — server e peers não sabem**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicalLogic.cs:421` (dim: coop)

**Problema:** Só o consumo TOTAL usa TryRunNetworkTransaction (discard). O decremento parcial altera o MedKitComponent apenas na memória do client do médico — o comentário do próprio arquivo (cs:396-403) descreve por que estado local divergente do server causa rejeição de operações ('slot fantasma'), mas o caminho parcial cria exatamente essa divergência: server SPT e peers continuam vendo o kit cheio.

**Sugestão:** Usar a operação de rede que o vanilla usa para consumo de recurso de med (grep no Assembly pelo op de MedKitComponent/ResourceComponent e rodá-la via TryRunNetworkTransaction), ou incluir o novo HpResource no BandAidHealPacket e aplicar a mesma mutação em todas as pontas (host/peers) para manter consistência até o fim da raid.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-24 · C — Gap vs. spec · 🟡 Médio

**HealRoutine ignora o resultado de SetInHands: se falhar, a cura 'telepática' completa sem item nas mãos nem animação**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:533` (dim: correctness)

**Problema:** O overload usado é Player.SetInHands(Item, Callback<IHandsController>) (decompilado Player.cs:31845), que pode falhar assincronamente (CheckAction falha, troca de arma em andamento, mãos ocupadas) — o callback vazio descarta `result.Failed` e nem verifica se o controller resultante é um MedsController. A coroutine segue cegamente: WaitForSeconds(UseTime+2) e ApplyTreatment aplica o tratamento e consome o item mesmo que o item nunca tenha chegado às mãos (nenhum method_5, nenhuma animação, item p

**Sugestão:** No callback, verificar `result.Failed || !(result.Value is Player.MedsController)` e nesse caso chamar CleanupHealState(patient) + liberar UsingMeds + notificação de falha (um bool capturado pela coroutine, checado logo após o SetInHands com 1-2 frames de espera, resolve o timing).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-25 · B — Bug latente · 🟡 Médio

**BotUpdateManualPatch aloca List<IPlayer> por bot por tick enquanto houver qualquer desmaiado**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/BotPatches.cs:139` (dim: hotpath)

**Problema:** BotOwner.UpdateManual roda todo frame por bot ativo (BotOwner.cs:1021). O postfix aloca uma List<IPlayer> nova por bot por frame sempre que FaintedPlayerIds.Count > 0 — ou seja, durante toda a janela desmaio+grace (~25s), mesmo quando nenhum inimigo do grupo é o desmaiado (caso comum).

**Sugestão:** Alocação lazy: declarar `List<IPlayer> enemiesToRemove = null;` e só instanciar dentro do if quando achar match; ou usar uma lista `static` reutilizável com Clear() (IA roda na main thread). Alternativa mais barata: como BotsGroup.Enemies é por GRUPO (não por bot), mover essa limpeza para o momento do SyncFaintStatus(true) em vez de reconciliar por tick.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-26 · F — Melhoria · 🟡 Médio

**MainLoopPatch faz 4-8 consultas de saúde + ~6 lookups de dicionário por player/bot por frame, mesmo sem nenhum trauma ativo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:103` (dim: hotpath)

**Problema:** O postfix em Player.LateUpdate roda para cada player/bot todo frame no estado normal (configs default ligadas): braços = IsPartDestroyed×2 (linha 103; cada um = GetBodyPartHealth + IsBodyPartBroken, VoiceAndHealthUtils.cs:16-17), pernas humanos = ×2 (linhas 125-126), bots = GetBodyPartHealth×2 (linhas 172-173); mais pares ContainsKey+indexador em BlackoutTimers/GraceTimers/AimingFatigueTimers/LegPenaltyTimers/BotLegsBrokenStartTimes (lookup duplo com chave string).

**Sugestão:** Throttle por player: guardar `nextCheckTime` num Dictionary<string,float> (ou campo em componente) e só reavaliar membros destruídos a cada 0.2-0.5s — trauma de membro não precisa de resolução de frame; manter por-frame apenas o que é enforcement visual (pose durante blackout). Trocar todos os pares ContainsKey+[id] por TryGetValue (corta lookups pela metade).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-27 · B — Bug latente · 🟡 Médio

**DoContusion(1f,1f) re-disparado TODO frame durante o blackout (vanilla usa por evento)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:42` (dim: hotpath)

**Problema:** Dentro do CASO 1 do MainLoopPatch (ainda desmaiado), DoContusion é chamado por frame por player desmaiado durante toda a duração (default 20s = ~1200 chamadas). No vanilla, DoContusion é disparado por EVENTO (ex.: dano de granada, GClass2085.cs:261), não por frame — cada chamada passa pelo pipeline de efeitos do ActiveHealthController (criação/renovação de efeito + notificações).

**Sugestão:** Aplicar DoContusion 1× ao entrar no blackout (junto do DoStun em HealthPatches.cs:69) e renová-lo por intervalo (ex.: a cada 2s com timestamp), ou usar duração = tempo restante do blackout numa única chamada. Remover a chamada per-frame.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (sessão autônoma /g-autodev). DoContusion renovado a cada 2s (ContusionRenewTimers) em vez de por frame. Validação in-game pendente.

---

### CR-01-28 · C — Gap vs. spec · 🟡 Médio

**MainLoopPatch nunca roda para players remotos: ObservedPlayer.LateUpdate não chama base (AP-03)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:9` (dim: hotpath)

**Problema:** O patch está na base virtual Player.LateUpdate (Player.cs:26361). No Fika 2.3.4, ObservedPlayer.LateUpdate (ObservedPlayer.cs:1602-1616) sobrescreve SEM chamar base.LateUpdate() — o IL patcheado nunca executa para jogadores remotos observados. Todo o enforcement do CASO 1 (pose prone, stamina, arma baixada) e a limpeza de timers nunca rodam para peers no host/cliente local; só rodam para o próprio jogador e bots.

**Sugestão:** Auditar os overrides de LateUpdate (grafo/decompile) e cobrir o caminho observed: ou patchear também ObservedPlayer.LateUpdate, ou mover a lógica de estado para um tick central próprio (ex.: no Update do plugin iterando GameWorld.AllAlivePlayersList com throttle) que não dependa de virtual dispatch.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-29 · F — Melhoria · 🟡 Médio

**BandAidUI.HasEffect faz MakeGenericMethod + Invoke + new object[] ~49×/250ms com a HUD aberta**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:281` (dim: hotpath)

**Problema:** UpdateEffects testa 6 tipos de efeito × 7 membros (42 chamadas) + UpdateSilhouetteImage testa fratura ×7 a cada UPDATE_INTERVAL (0.25s) — ~200 chamadas/s de MakeGenericMethod (que NÃO é cacheado; refaz binding genérico e aloca) + Invoke com array e boxing do enum a cada chamada.

**Sugestão:** Em CacheTypes(), materializar um Dictionary<Type, MethodInfo> com os 7 _findEffectMethod.MakeGenericMethod(tipo) já resolvidos (uma vez), e reutilizar um object[1] por chamada (ou cachear os 7 valores boxed de EBodyPart). Melhor ainda: compilar delegates via Delegate.CreateDelegate para cada tipo fechado.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-30 · B — Bug latente · 🟡 Médio

**MedicHealPatch._currentObservedMedsControllerClass sobrevive ao fim da raid — pina o meds controller/Player da raid morta**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:91` (dim: lifecycle)

**Problema:** O campo estático `_currentObservedMedsControllerClass` (setado no Prefix, linhas 164/215/237) só é zerado por `ForceFinishAnimation()` (linha 112). ResetAllState do BandAidController chama apenas `CleanupPatientSubscription()`, que preserva o campo de propósito, e NÃO chama ForceFinishAnimation. Se a raid acaba no meio de uma cura (coroutine parada pelo StopCoroutine do ResetAllState antes do ForceFinishAnimation pós-WaitForSeconds), o campo mantém a instância do ObservedMedsControllerClass — qu

**Sugestão:** Criar `MedicHealPatch.ResetStatics()` que faça `_currentObservedMedsControllerClass = null; BandAidHealActive = false; IsRedirectingHeal = false; CurrentPatient = null; CleanupPatientSubscription();` SEM invocar method_9, e chamá-lo no ResetAllState (substituindo as 4 linhas atuais) e no futuro hook de raid-end.

**Duplicatas consolidadas:** patches: _currentObservedMedsControllerClass estático não é limpo no ResetAllState — pina; coop: _currentObservedMedsControllerClass (static) não é limpo no reset de raid — refe

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-31 · C — Gap vs. spec · 🟡 Médio

**Pipeline médico roda no hideout sem guard `is HideoutPlayer` (obrigatório pela skill §2)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:164` (dim: lifecycle)

**Problema:** O único guard do Update é `Singleton<GameWorld>.Instance == null || MainPlayer == null` (linha 137). O hideout TEM GameWorld (HideoutGameWorld : ClientLocalGameWorld) com MainPlayer = HideoutPlayer, então o sweep de MedicInteractable (a cada 2s sobre AllAlivePlayersList), o Tick do DebugBotInvisibility, o CheckPressMode das hotkeys e a máquina de blackout do Plugin.Update rodam no hideout. Também marca `_lastGameWorld` com o mundo do hideout e dispara ResetAllState em cada entrada/saída de hideo

**Sugestão:** Após o null-check da linha 137, adicionar `if (Singleton<GameWorld>.Instance.MainPlayer is EFT.HideoutPlayer) return;` (mantendo o bloco de detecção de mudança de _lastGameWorld antes do return, para o reset continuar funcionando). Aplicar o mesmo guard no Update do plugin (TRLImmersiveCombatMedicinePlugin.cs:169).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-32 · D — Arquitetura · 🟡 Médio

**Teardown fragmentado em 3 mecanismos (prefix OnGameStarted, polling _lastGameWorld, OnRaidEnded encadeado) sem hook canônico de fim de raid**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:129` (dim: lifecycle)

**Problema:** A limpeza está dividida por dono do estado, não por evento: TraumaState só limpa no INÍCIO da raid seguinte (prefix em OnGameStarted); flags BandAid/MedicHealPatch/DebugBotInvisibility limpam via polling `_lastGameWorld != Singleton<GameWorld>.Instance` no Update (BandAidController.cs:139-153). Há redundância (ResetAllState também dispara no início da raid, logo após o prefix) e lacuna (nenhum dos dois roda no FIM — todo o estado fica sujo durante menu/tela de morte). O polling funciona porque o

**Sugestão:** Consolidar num `RaidSession.End()` idempotente (guard `bool _ended`) chamado por: postfix em GameWorld.OnDestroy (GameWorld.cs:2111) + postfix em BaseLocalGame.Stop (BaseLocalGame.cs:1018), mantendo o polling do Update apenas como cinto-e-suspensório. End() agrupa: limpeza completa do TraumaState (incl. IsFainted/LegPenaltyTimers), AudioListener.volume=1, MedicHealPatch.ResetStatics(), _pendingHea

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-33 · B — Bug latente · 🟡 Médio

**Corpos de patch sem try/catch — prefix de ApplyDamageInfo deref HealthController sem null-check e MainLoopPatch roda per-frame desprotegido**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs:16` (dim: patches)

**Problema:** Violação do checklist C# item 6 / skill SPT §5 (todo corpo de patch em try/catch): DamageTriggerPatch (prefix e postfix), MainLoopPatch (MovementPatches.cs:12, postfix em Player.LateUpdate — per-frame por player), SilenceVoicePatch, os 7 BotPatches, MedicActionsPatch.Prefix, MedicPlayerRangePatch.Postfix, AnimCleanupPatch.Postfix e os dois FikaRevivePatch não têm try/catch. No prefix acima, `__instance.HealthController.IsAlive` lança NRE se HealthController for null (frames de spawn/despawn) — e

**Sugestão:** Envolver os corpos em try/catch { LogError } garantindo `return true` no catch dos prefixes (nunca engolir o original por exceção). Prioridade: DamageTriggerPatch.Prefix (trocar também para `__instance?.HealthController?.IsAlive != true`), MainLoopPatch (per-frame) e AnimCleanupPatch (mexe em estado do redirect).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-34 · C — Gap vs. spec · 🟡 Médio

**Alvos ofuscados resolvidos por nome literal (method_5/6/8/9, method_15, GInterface###) sem predicado de assinatura nem assert de boot**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:85` (dim: patches)

**Problema:** method_N e GInterface### são numerações de ofuscação que mudam entre builds do EFT (AP-03 agravante; skill SPT §1 'resolve por assinatura/predicado estável, nunca hardcode'). Ocorrências: method_5/8/9 aqui, method_6 no Prefix (linhas 215/266/308), method_15 em MedicalLogic.cs:240 e BandAidNetworkHandler.cs:359, GInterface350/376 na bridge. O fix dos fields (EnsureFieldCache com fallback por TIPO) mostrou o padrão certo — mas os métodos continuam por nome literal, e _method8Cached/_method9Cached 

**Sugestão:** Criar um resolvedor por assinatura no mesmo helper dos fields: method_5 = único método void sem params que o corpo referencia DoMedEffect (ou: resolver os 3 por aridade+tipo dentre os method_N da classe: method_8 = único (IEffect)→void, method_9 = único void()→void que subscreve/dessubscreve EffectRemovedEvent, method_6 = void() restante); method_15 = único método genérico de 1 type-param (EBodyPa

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-35 · E — Legibilidade · 🟢 Menor

**Campos mortos confirmados pelo build: _harmony (CS0169) e trio shoulder-tap (CS0414) no BandAidController**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:39` (dim: conventions)

**Problema:** dotnet build de hoje confirma: CS0169 em BandAidController._harmony:39 ('nunca é usado' — o Harmony real vive no Plugin.cs:85, este é resto do BandAidPlugin 3.11 que patchava por conta própria) e CS0414 em _shoulderHoldTimer:54, _shoulderHoldTriggered:55, _shoulderLastTapTime:61 (atribuídos no declarador, nunca lidos — o caminho de input do shoulder tap morreu na refatoração para o painel nativo).

**Sugestão:** Deletar as 4 declarações (linhas 39, 54, 55, 61). Se o achado das configs mortas de shoulder tap for aceito, remover junto os wrappers _shoulderTapKey/_shoulderTapMode (linhas 44 e 47) na mesma passada.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-36 · E — Legibilidade · 🟢 Menor

**using duplicado em BandAidUI.cs (CS0105)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:3` (dim: conventions)

**Problema:** Linhas 2 e 3 importam TRLImmersiveCombatMedicine duas vezes — warning CS0105 confirmado no build de hoje. Provável sobra de merge da fusão dos mods.

**Sugestão:** Deletar a linha 3. Se o achado de consolidação de namespaces for aplicado, esses usings somem naturalmente.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-37 · E — Legibilidade · 🟢 Menor

**csproj: <Reference mscorlib> gera MSB3245/MSB3243 todo build; LangVersion não pinado; AssemblyName difere do nome do projeto**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRL-ImmersiveCombatMedicine.csproj:23` (dim: conventions)

**Problema:** Três itens menores confirmados no build: (1) '<Reference Include="mscorlib"/>' sem HintPath é irresolvível em netstandard2.1 → MSB3245 ('Não foi possível localizar o assembly mscorlib') + MSB3243 (conflito resolvido 'arbitrariamente') a cada build; num target netstandard o mscorlib vem do próprio TFM, a linha é inútil. (2) LangVersion ausente — a skill csharp-mod-best-practices §9 pede pin explícito (C# 9-11 para Unity Mono); hoje compila com o default do SDK 10 instalado, que aceita sintaxe que

**Sugestão:** Remover a linha do mscorlib; adicionar <LangVersion>10</LangVersion> (ou 'latest' como o TRL-ImmersiveScopes, se aceitar o risco documentado); manter AssemblyName sem hífen (BepInEx prefere) mas documentar a diferença no futuro README.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-38 · E — Legibilidade · 🟢 Menor

**17 warnings Harmony003 em HealthPatches — falso positivo do analyzer em leituras de DamageInfoStruct, mas ruído permanente no build**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs:13` (dim: conventions)

**Problema:** O build emite 17× 'Harmony003: Harmony non-ref patch parameter damageInfo.DamageType modified. This assignment have no effect' (linhas 21-25, 43-47, 56-57, 86-90). Verifiquei cada linha flagada: TODAS são leituras/comparações (==, >=), nenhuma atribuição — o BepInEx.Analyzers dispara errado em member-access de parâmetro struct sem ref. Não há bug funcional (ler cópia do struct é seguro), mas são 17 dos 22 warnings do build.

**Sugestão:** Trocar a assinatura para 'ref DamageInfoStruct damageInfo' nos dois patches (prefix e postfix — leitura continua idêntica, Harmony aceita ref em ambos) ou suprimir pontualmente com #pragma warning disable Harmony003 + comentário explicando o falso positivo. Preferir o ref: zero supressão e protege contra atribuição futura.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-39 · F — Melhoria · 🟢 Menor

**Mod não tem README.md, mod.json nem backlog/ — abaixo do padrão dos mods irmãos do repo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRL-ImmersiveCombatMedicine.csproj:1` (dim: conventions)

**Problema:** A raiz de mods/TRL-ImmersiveCombatMedicine/ tem apenas builds/, memory/ e modded/. Mods comparáveis do repo (AutoGym, CustomClasses) têm README.md, mod.json, backlog/ com mod-backlog.md, e PROPRIEDADES.md (este último já é achado próprio, strong). Sem backlog/mod-backlog.md, o ciclo /create-spec→/code-review do repo-workflow não tem onde ancorar artefatos — os achados deste review (04-code-review-NN.md) não têm pasta NNN-slug para viver.

**Sugestão:** Criar o esqueleto mínimo: README.md (o que o mod faz — fusão Band-Aid+TrueTrauma, features Medical/Trauma, dependência Fika 2.3.4), backlog/mod-backlog.md vazio no formato padrão, e mod.json se o padrão do repo o exigir para client mods (conferir com AutoGym). Pode ser o mesmo commit do PROPRIEDADES.md.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-40 · F — Melhoria · 🟢 Menor

**Git hygiene do acervo legado: 9 arquivos .cs.txt versionados em 'TrueTrauma - FINALIZADO/ARQUIVOS DO MOD/' divergindo dos .cs da raiz**

**Local:** `mods/TrueTrauma - FINALIZADO/ARQUIVOS DO MOD/TrueTraumaPlugin.cs.txt:1` (dim: conventions)

**Problema:** git ls-files confirma 9 fontes C# com extensão .txt trackeados em 'mods/TrueTrauma - FINALIZADO/ARQUIVOS DO MOD/' (AggroHelper, BotPatches, FikaBridge, HealthPatches, InputPatches, MovementPatches, TraumaState, TrueTraumaPlugin, VoiceAndHealthUtils). diff mostra que .cs.txt ≠ .cs da raiz do mesmo mod (ex.: TraumaState) — ou seja, DUAS versões de origem do TrueTrauma no repo sem indicação de qual foi a base da fusão. Está fora de modded/, então não afeta build — mas é o acervo que a investigação 

**Sugestão:** Decidir qual snapshot é canônico, deletar o outro (ou renomear a pasta para deixar explícito, ex.: 'ARQUIVOS DO MOD' → '_snapshot-pre-fusao/' com uma linha de README dizendo a data/origem). Baixa prioridade — não tocar enquanto a investigação paralela estiver usando esses arquivos.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-41 · E — Legibilidade · 🟢 Menor

**Whitelist de frases 'sinal secreto de rede' no SilenceVoicePatch é vestígio do 3.11 — nada escuta OnYourOwn/OnBreath**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Helpers/VoiceAndHealthUtils.cs:75` (dim: coop)

**Problema:** No mod 3.11 essas frases eram usadas como canal de sinalização de faint entre clientes; no código atual não existe nenhum listener (o FikaPacketManager não foi migrado — ver achado do faint sync). O comentário descreve um mecanismo inexistente e a exceção deixa o desmaiado emitir OnBreath (a voz de 'TryAim' do VoiceHelper usa OnBreath) durante o blackout, furando a mudez.

**Sugestão:** Remover a whitelist (bloquear tudo durante blackout) ou atualizar o comentário; se o packet de faint do achado 3 for migrado, não há razão para canal por voz.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-42 · F — Melhoria · 🟢 Menor

**ImageLoader.Load sem cache negativo: File.Exists (I/O de disco) repetido para sprites ausentes**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Helpers/ImageLoader.cs:48` (dim: hotpath)

**Problema:** Só hits entram no _cache. UpdateSilhouetteImage (BandAidUI.cs:983/987) chama ImageLoader.Load(prefix + "_fratura"/sufixo) por membro a cada 0.25s com a HUD aberta; se o PNG do variant não existir no deploy, cada chamada vira concat de string + Path.Combine + File.Exists no disco, 4×/s por membro afetado.

**Sugestão:** Cachear misses: após o File.Exists falhar, gravar `_cache[imageName] = null` e retornar — o TryGetValue passa a devolver o null cacheado sem tocar disco. (O contrato atual já retorna null para ausente, então nada mais muda.)

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-43 · F — Melhoria · 🟢 Menor

**CheckPressMode: shortcut.Modifiers.Any() aloca iteradores LINQ por frame nos modos médico/cura**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:355` (dim: hotpath)

**Problema:** KeyboardShortcut.Modifiers do BepInEx é implementado como AllKeys.Skip(1) (IEnumerable) — cada chamada de .Any() aloca 2 iteradores. CheckPressMode roda por frame enquanto _isHealingInProgress (emergency drop, linha 170) ou _isMedicModeActive (linha 186), e a configuração de modifiers não muda no meio do frame.

**Sugestão:** Pré-computar o bool por ConfigEntry: campo static bool atualizado no handler SettingChanged de cada keybind (padrão já usado em DebugBotInvisibility.Init), e CheckPressMode recebe o bool pronto.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-44 · F — Melhoria · 🟢 Menor

**MedicHealPatch.Prefix resolve AccessTools.Field/Method a cada chamada e loga Warning incondicional para method_5 de QUALQUER player**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:136` (dim: hotpath)

**Problema:** O Prefix em ObservedMedsControllerClass.method_5 dispara para uso de meds de qualquer observed player em coop. Linha 120 emite LogWarning com interpolação SEMPRE (sem tag [DEBUG-ICM], sem gate); dentro do caminho redirect, AccessTools.Field/Method (medsController_0, _player, queue_0, float_0, firearmsAnimator_0, SetUseTimeMultiplier, method_6) são re-resolvidos por chamada, apesar de _method8Cached/_method9Cached mostrarem que o padrão de cache já existe no arquivo.

**Sugestão:** Resolver todos os FieldInfo/MethodInfo em TargetMethod() (mesmo lugar onde _method8Cached é preenchido) e guardar em static readonly; rebaixar o log da linha 120 para Debug gateado por config ou taguear [DEBUG-ICM] com throttle.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-45 · F — Melhoria · 🟢 Menor

**Plugin.Update escreve AudioListener.volume todo frame em raid, mesmo com intensidade ~0**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:232` (dim: hotpath)

**Problema:** O caminho desabilitado (linha 164) tem guard `if (AudioListener.volume != 1f)`, mas o caminho em-raid seta a propriedade nativa incondicionalmente todo frame; o Lerp exponencial nunca chega a exatamente 0, então o mod fica escrevendo ~1.0 para sempre.

**Sugestão:** Snap: `if (targetIntensity == 0f && TraumaState.EffectIntensity < 0.01f) TraumaState.EffectIntensity = 0f;` e só escrever AudioListener.volume quando EffectIntensity > 0 (restaurando 1f uma única vez na transição para 0, com flag).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-46 · F — Melhoria · 🟢 Menor

**BandAidUI.Awake roda I/O de disco + FindObjectsOfTypeAll + construção de canvas inteiro no boot do jogo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:160` (dim: hotpath)

**Problema:** O componente é adicionado no Awake do plugin (Plugin.cs:73), então isso executa no chainloader do BepInEx: Resources.FindObjectsOfTypeAll<Font> (varre todos os assets carregados), até 7 File.Exists + ReadAllBytes/decodificação de PNGs (LoadCustomImages→CreateSilhouetteImages via ImageLoader) e criação de ~80 GameObjects de UI — antes de existir raid.

**Sugestão:** Lazy-init: mover LoadFont/LoadCustomImages/CreateCanvas para o primeiro ShowUI (guard `_built`), mantendo no Awake só Instance/CacheTypes. Estrutura já favorece isso porque ShowUI é o único ponto de entrada do canvas.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-47 · F — Melhoria · 🟢 Menor

**ImageLoader: cache estático de Texture2D/Sprite nunca liberado — ClearCache() existe mas não tem nenhum caller**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Helpers/ImageLoader.cs:97` (dim: lifecycle)

**Problema:** As ~30 PNGs da silhueta (7 partes × 5 cores + fratura + ícones) viram Texture2D não-comprimidas (RGBA32) que ficam no cache estático a sessão inteira; `ClearCache()` foi escrito para liberar mas nenhum código o chama. A skill spt-mod-best-practices §3 pede Destroy/Unload de texturas no raid-end ou no teardown do plugin.

**Sugestão:** Decisão explícita: (a) tratar como cache de sessão intencional — remover ClearCache() ou documentar no summary do método por que não é chamado; ou (b) chamar ImageLoader.ClearCache() no OnDestroy do plugin (TRLImmersiveCombatMedicinePlugin.cs:119-122). Não chamar por raid — o custo de reload por raid supera o ganho.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-48 · E — Legibilidade · 🟢 Menor

**Reflection não cacheada em MedicalLogic/BandAidNetworkHandler: GetMethods+MakeGenericMethod por chamada dentro de loops de 7 body parts**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicalLogic.cs:294` (dim: patches)

**Problema:** HasEffect re-escaneia os métodos de IHealthController e refaz MakeGenericMethod a CADA chamada, e é chamado em loop de 7 body parts × até 3 tipos de efeito por validação (GetSmartTarget/CanUseItem — este último roda no handshake de rede e a cada tecla de slot). Mesmo padrão em RemoveEffect (method_15, linha 240) e em BandAidNetworkHandler.HasEffect/RemoveEffectNative (linhas 312/359). O checklist C# item 4 exige MethodInfo cacheado em static readonly — e o próprio codebase já tem o padrão corret

**Sugestão:** Extrair um único helper estático (ex.: EffectReflectionCache) com _findEffectMethod e _method15 resolvidos 1x + Dictionary<Type,MethodInfo> para os MakeGenericMethod por tipo de efeito, e consumi-lo de MedicalLogic, BandAidNetworkHandler e BandAidUI.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

### CR-01-49 · E — Legibilidade · 🟢 Menor

**VoiceHelper.SafePlayVoice usa GetMethod("Say")+Invoke por chamada quando a chamada tipada direta é possível**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Helpers/VoiceAndHealthUtils.cs:32` (dim: patches)

**Problema:** Player.Say é público e o assembly já é referenciado em compile-time — o próprio arquivo patcheia `[HarmonyPatch(typeof(Player), "Say")]` 30 linhas abaixo. A reflection aqui é herança do mod 3.11: GetMethod por chamada (não cacheado), boxing dos 6 argumentos, e o `0` int dependendo da coerção do binder para ETagStatus. O catch{} vazio esconde qualquer falha (voz simplesmente não toca).

**Sugestão:** Trocar por chamada direta `player.Say(trigger, true, 0f, (ETagStatus)0, 100, false);` mantendo só o Enum.TryParse do nome da phrase (com log em falha em vez de catch vazio).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir): _________________

---

## Descartados na verificação adversarial

- ~~[blocker/A] MedicHealPatch busca campos com casing errado (medsController_0 vs MedsController_0) — redirect nunca engata e~~ — REFUTADO. Achado obsoleto — já corrigido no commit 25be6540 (2026-07-12): MedicHealPatch.cs:48-68 resolve MedsController_0/Queue_0/Float_0 com PascalCase + fallback camelCase + resolução por tipo, exatamente como o achado sugeria.
- ~~[strong/B] AggroHelper.NeutralizeAggro varre o mundo inteiro TODO FRAME durante blackout e grace period~~ — REFUTADO. [B/minor] NeutralizeAggro é chamada todo frame (MovementPatches.cs:53/96) mas o early-out em AggroHelper.cs:36-43 torna o custo steady-state desprezível (checks baratos por bot, sem alocação); o caminho pesado (GetCompon

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-12 | Guilherme | Criação (workflow 6 dimensões, 29 agentes, verificação adversarial). CR-01-03 aplicado como hotfix na mesma sessão. |