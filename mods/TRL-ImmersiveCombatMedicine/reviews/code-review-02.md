# TRL-ImmersiveCombatMedicine — Code Review 02 (delta da sessão autônoma)

> **Data:** 2026-07-12<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme (workflow 4 dimensões × verificação adversarial, 8 agentes)<br>
> **Referências:** [code-review-01.md](./code-review-01.md), [../docs/coop-heal-matrix.md](../docs/coop-heal-matrix.md)<br>

---

**Escopo:** SOMENTE o delta dos commits `8126d8ba..ac58098d` (ondas da sessão autônoma). Achados do código pré-existente estão no CR-01.

**Contadores:** 🟠 4 · 🟡 5 · 🟢 4 (sem bloqueadores) — **8 aplicados nesta mesma sessão**, restante deferido com justificativa.

## Achados

### CR-02-01 · C — Gap vs. spec · 🟠 Forte ✅

**TraumaFaintPacket não carrega duração — receptor fabrica timers com ConfigBlackoutDuration LOCAL; vira wake precoce + RestoreAggro com o player caído assim que CR-01-28 for corrigido**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:123` (dim: faint-sync) · **Veredito adversarial:** CONFIRMED

**Problema:** O pacote (TraumaFaintPacket.cs:13-14) só tem ProfileId+IsFainted; o receptor espelha BlackoutTimers/GraceTimers usando a ConfigBlackoutDuration LOCAL e Time.time da CHEGADA. Se host e client tiverem durações diferentes (config F5 é por processo, default 20s mas editável), os timers espelhados no host expiram em momento diferente do dono. HOJE isso é mascarado: fluxo completo verificado — todo consumo por expiry (MainLoopPatch CASO 2 + branch de grace, MovementPatches.cs:27/104) NUNCA roda para ObservedPlayer (CR-01-28: ObservedPlayer.LateUpdate não chama base), então no host os timers espelhados são peso morto removido SÓ pelo pacote IsFainted=false (linhas 131-133) e RestoreAggro roda no host só na chegada desse false (linha 141) — happy path funciona. Mas o comentário do próprio delta (linhas 119-120: 'Espelhar os timers para o MainLoopPatch/BotPatches deste processo tratarem o desmaia

**Sugestão:** Incluir a duração no pacote (ex.: float DurationSeconds — ou melhor, RemainingSeconds + GraceSeconds) e usar esses valores no espelho do receptor em vez da config local. Complementar com guarda de autoridade: só o processo dono do estado emite pacotes sobre o próprio ProfileId (no MainLoopPatch, condicionar SyncFaintStatus a __instance.IsYourPlayer), para que um futuro fix de CR-01-28 não transforme o host em emissor de false sobre players remotos. Atualizar o comentário das linhas 119-120 para 

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). Duração+grace viajam no pacote (config do DONO); espelho usa valores do pacote; guard de autoridade em SyncFaintStatus (IsYourPlayer||IsAI); comentário enganoso corrigido. Validação in-game pendente (P-2.9).

---

### CR-02-02 · C — Gap vs. spec · 🟠 Forte ✅

**Assimetria no sync de desmaio de BOT: true é broadcast (HealthPatches→SyncFaintStatus) mas o wake do bot remove estado local sem enviar false — clients ficam com espelho órfão e bot permanentemente mudo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:80` (dim: faint-sync) · **Veredito adversarial:** CONFIRMED

**Problema:** O delta transformou SyncFaintStatus de local-only em broadcast (FikaBridge.cs:22). O gatilho de desmaio (HealthPatches.cs:76) não filtra IsAI e roda para bots no host/headless (FikaBot.ApplyDamageInfo chama base → patch dispara): todo bot que desmaia (cabeça ≥10 dmg ou tórax ≥35 e sobrevive — frequente) gera TraumaFaintPacket(true) para todos os clients, que espelham BlackoutTimers/FaintedPlayerIds do bot (BandAidNetworkHandler.cs:121-128). Mas o wake do bot (MainLoopPatch CASO 2, branch IsAI — MovementPatches.cs:72-84, incluindo o fix CR-01-19 nas linhas 80-81) remove FaintedPlayerIds/GraceTimers/BlackoutTimers SÓ localmente, sem SyncFaintStatus(false). Nenhum false é jamais enviado para bots → nos clients o espelho do bot fica órfão até o fim da raid. Efeito observável: SilenceVoicePatch (VoiceAndHealthUtils.cs:71) checa BlackoutTimers.ContainsKey e RODA para observed (ObservedPlayer.S

**Sugestão:** No branch IsAI do CASO 2, trocar as remoções cruas por FikaBridge.SyncFaintStatus(__instance, false) (que já faz UpdateFaintedList + broadcast) mantendo a remoção de GraceTimers; OU não sincronizar desmaio de bot (guard !player.IsAI em FikaBridge.SyncFaintStatus antes do SendTraumaFaintPacket), já que o aggro dos bots é tratado 100% no processo dono dos bots e o único consumo client-side é o mute de voz — escolher um lado e manter o par true/false simétrico.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). Wake do bot agora emite SyncFaintStatus(false) → peers limpam o espelho (bot não fica mudo). Validação in-game pendente (P-2.9).

---

### CR-02-03 · B — Bug latente · 🟠 Forte ✅

**Abort (distância e Mouse0) cancela o ActiveHealthController do médico, mas nunca o MedEffect nativo criado no PACIENTE — tratamento e consumo continuam após 'Abortado!'**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:868` (dim: redirect-anim) · **Veredito adversarial:** CONFIRMED

**Problema:** Quando o redirect criou MedEffect NATIVO no paciente (NativeMedEffectApplied=true — host curando bot, o caminho DoMedEffect de MedicHealPatch.cs:262-319), o efeito vive no ActiveHealthController do PACIENTE. O abort por distância (CR-01-11 → DeactivateMedicMode) e o cancel por Mouse0 (CancelHealInProgress) chamam CancelApplyingItem apenas no controller do MÉDICO — que está vazio, pois nada foi aplicado nele. O MedEffect do paciente continua rodando até o fim: cura HP, remove bleeds/fraturas no Residue e consome HpResource do item, tudo DEPOIS da notificação de aborto. O vanilla cancela no dono do efeito: ObservedMedsControllerClass.Remove() faz `MedsController_0._player.HealthController.CancelApplyingItem()` (Player.cs:19596-19600) — o comentário de CancelHealInProgress ('Equivalente ao vanilla Mouse0') não se sustenta no caso redirecionado.

**Sugestão:** Nos dois caminhos de cancel (DeactivateMedicMode branch _isHealingInProgress e CancelHealInProgress), ANTES de zerar MedicHealPatch.CurrentPatient: `if (MedicHealPatch.NativeMedEffectApplied) MedicHealPatch.CurrentPatient?.ActiveHealthController?.CancelApplyingItem();` (paciente local apenas; ObservedCoopPlayer não tem ActiveHC e nesse caso não há MedEffect nativo). Resetar NativeMedEffectApplied=false no cleanup para não vazar leitura stale.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). MedicHealPatch.CancelNativePatientEffect() chamado nos 5 caminhos de abort (distância, Mouse0, EmergencyDrop, CleanupHealState, ResetAllState) — MedEffect do paciente é cancelado; caminho de sucesso intocado. Validação in-game pendente (P-2.9).

---

### CR-02-04 · B — Bug latente · 🟠 Forte ✅

**Reparo de mojibake mudou os bytes da KEY de config — 'Sistema de Braços' recriada com default, valor do usuário perdido (regressão já materializada no cfg implantado)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:48` (dim: misc-delta) · **Veredito adversarial:** CONFIRMED

**Problema:** O CR-01-06 (wave 6, e7683f19) corrigiu o mojibake também dentro de um literal usado como IDENTIDADE de config: a key era 'Sistema de BraÃ§os' (bytes duplo-encodados) e virou 'Sistema de Braços'. BepInEx casa entrada do .cfg por section+key byte a byte — a key nova não existe no arquivo salvo, então Config.Bind cria entrada nova com o default (true) e a antiga vira órfã. Evidência real no ambiente implantado D:/SPT/BepInEx/config/com.trl.immersivecombatmedicine.cfg: linha 26 'Sistema de BraÃ§os = false' (escolha do usuário, agora órfã) e linha 33 'Sistema de Braços = true' (recriada com default). As demais keys/seções são ASCII e não mudaram; nomes de arquivo de imagem ('cabeca'/'torax'/'bracoe'... em BodyPartImagePrefix) e demais chaves ficaram intactos — esta é a única quebra de identidade.

**Sugestão:** Migração one-time no Awake logo após o Bind: ler ConfigFile.OrphanedEntries (internal — via reflection) procurando a key antiga com bytes mojibake ("Sistema de BraÃ§os" na section '2. Mecanicas (Trauma)'), e se existir copiar o valor para ConfigArmsEnabled.Value e salvar. Alternativa mínima: corrigir manualmente o cfg das máquinas afetadas (local + produção) e registrar a quebra em PROPRIEDADES.md/release notes — o ledger de removidas já existe, mas hoje só cobre ShoulderTap e chama órfãs de 'in

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). MigrateOrphanedConfigKeys() no Awake: copia o valor da key órfã 'Sistema de BraÃ§os' (via OrphanedEntries/reflection) para a key corrigida e salva — vale para todas as máquinas. Validação in-game pendente (P-2.9).

---

### CR-02-05 · C — Gap vs. spec · 🟡 Médio

**Morte/disconnect durante o desmaio: ninguém envia IsFainted=false — espelhos órfãos no host até o fim da raid; reconnect do Fika deixa o player permanentemente invisível para bots**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:15` (dim: faint-sync)

**Problema:** O único emissor de IsFainted=false é o grace-expiry do MainLoopPatch no processo dono (MovementPatches.cs:107), atrás do gate IsAlive (linha 15). Se o dono morre desmaiado/em grace (bleed-out, queda — o shield de HealthPatches.cs:19-28 só bloqueia dano balístico), o false nunca sai: host e demais clients mantêm FaintedPlayerIds + BlackoutTimers + GraceTimers do morto até ResetAll no fim da raid (Plugin.Update do dono limpa BlackoutTimers/StartTimes locais nas linhas 223-224 mas não sincroniza nem remove FaintedPlayerIds). Efeitos verificados: (a) enquanto morto — benigno (bots não miram cadáver; vazamento de dicionário apenas); (b) revive por desfibrilador se AUTO-CURA: no primeiro LateUpdate vivo, o GraceTimers vencido do dono dispara o branch de grace → SyncFaintStatus(false) + RestoreAggro, limpando todos os processos (colateral: RestoreAggro chama AddPointToSearch na posição do recém

**Sugestão:** Dar ao host um mecanismo de expiração dos espelhos que não dependa do pacote false: (1) com a duração incluída no pacote (finding anterior), um watchdog no processo dono-de-bots remove entradas espelhadas após expiry+grace+margem (ex.: +10s); e/ou (2) assinar o evento de morte (Player.OnPlayerDead) e o disconnect de peer (FikaServer) para limpar FaintedPlayerIds/timers daquele ProfileId em todos os consumidores locais.

**Decisão:**
- [ ] Pendente — Deferido: órfãos são benignos até ResetAll; caso reconnect Fika vira item próprio.
- [ ] Aceitar sugestão
- [ ] Rejeitar (deferir): _________________

---

### CR-02-06 · B — Bug latente · 🟡 Médio ✅

**Notificação 'Você foi tratado por um aliado.' aparece para o host-player quando ele aplica FullTreatment em nome de um BOT**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:353` (dim: bot-authority)

**Problema:** ApplyFullTreatmentLocally foi escrito para o cenário 'EU sou o paciente' e termina exibindo a notificação de paciente. O delta (CR-01-01, commit 99927604) reaproveita a função em TryApplyFullTreatmentOnLocalBot, que roda no processo DONO do bot: no cenário host-player, toda cura de bot feita por um client médico faz o HOST ver o toast 'Você foi tratado por um aliado.' sem ter sido tratado. No headless a chamada é inócua (exception eventual é capturada pelo try/catch de TryApplyFullTreatmentOnLocalBot APÓS o tratamento já aplicado — sem quebra funcional, só warning no log). O caminho de cirurgia retorna antes da notificação, então o sintoma ocorre nos itens de efeito/HP.

**Sugestão:** Parametrizar a notificação: ApplyFullTreatmentLocally(Player patient, BandAidHealPacket packet, bool notifyPatient) — TryApplyFullTreatmentOnLocalBot chama com notifyPatient:false (ou condicionar a `patient == Singleton<GameWorld>.Instance.MainPlayer`); no caminho bot, trocar por log info ('FullTreatment aplicado no bot X em nome do médico Y').

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). Toast condicionado a patient==MainPlayer; caminho bot loga em vez de notificar. Validação in-game pendente (P-2.9).

---

### CR-02-07 · B — Bug latente · 🟡 Médio

**Bridge GInterface376 (CR-01-08) continua efetivamente morto para a família MedKit: MedKitStartDelay(2s) + medUseTime + atraso de levantar o kit excede o WaitForSeconds(UseTime+2) do HealRoutine**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:574` (dim: redirect-anim)

**Problema:** A vida do MedEffect nativo = StartDelay + UseTimeFor(part)/fator, contada a partir do method_5 — que só roda após a animação de levantar o kit (δ ≈ 0.5-1.5s depois do início do WaitForSeconds). Para MedKitItemClass o StartDelay é MedKitStartDelay=2s (globals.json) e medUseTime da Salewa/IFAK=3s: remoção do efeito ≈ T0+δ+5s, enquanto o HealRoutine acorda em T0+5s exato. O coroutine acorda ANTES, chama CleanupPatientSubscription (linha 601) e dessubscreve OnPatientEffectRemoved — o bridge → method_8 nunca dispara para medkits (o caso central do fix Salewa). Só dispara para MedicalItemClass (bandagem/splint/CMS, StartDelay=0) ou com o speedup de ÷1.2. Verifiquei as duas ordens de corrida: ambas idempotentes (Bool_0 em method_9, Player.cs:19644-19646; AnimCleanupPatch early-return quando !IsRedirectingHeal) — sem crash, e o MedEffect residual completa a cura sozinho no paciente. Além disso s

**Sugestão:** Dirigir a finalização pelo evento em vez do timer: no HealRoutine, após o WaitForSeconds, se NativeMedEffectApplied e o efeito ainda existir no paciente, aguardar (com timeout) o OnPatientEffectRemoved antes de ForceFinishAnimation — ou simplesmente somar o StartDelay real (2s p/ MedKitItemClass, via config ou constante documentada) à margem. No mínimo, logar quando o wake-up ocorre com a subscription ainda ativa, para a validação in-game do CR-01-08 medir se o bridge disparou.

**Decisão:**
- [ ] Pendente — Deferido: análise de timing (MedKitStartDelay+useTime vs UseTime+2s) — comportamento atual coberto pelo timer; observar no teste in-game.
- [ ] Aceitar sugestão
- [ ] Rejeitar (deferir): _________________

---

### CR-02-08 · B — Bug latente · 🟡 Médio ✅

**Ownership guard falha ABERTO: se ResolveOperationOwner retornar null durante um redirect, o prefix devolve o method_5 original ao MÉDICO — redirect e bloqueio G5 desligam silenciosamente**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:175` (dim: redirect-anim)

**Problema:** Os 3 campos da operação têm cache por tipo com fallback rename-proof (EnsureFieldCache/ResolveField), mas `_player` (Player.cs:17698, `protected internal Player _player` em ItemHandsController) é resolvido por nome literal, por chamada, sem fallback por tipo e com catch-all que retorna null. Se um update do EFT renomear o campo (ou GetValue lançar), ResolveOperationOwner→null faz isDoctorOperation=false e o prefix retorna true (linhas 196-197) ANTES dos checks IsRedirectingHeal/BandAidHealActive: (a) durante um redirect ativo, o method_5 ORIGINAL roda no médico — DoMedEffect no próprio médico (self-heal indevido + consumo) ou null→FailedToApply (animação cancela); como NativeMedEffectApplied fica false, o HealRoutine ainda aplica MedicalLogic.ApplyTreatment no paciente ao acordar → dupla aplicação/consumo; (b) o bloqueio G5 (BandAidHealActive) deixa de proteger _currentObservedMedsContro

**Sugestão:** 1) Cachear o FieldInfo de `_player` uma vez (junto do EnsureFieldCache) com fallback por tipo (campo único do tipo Player em ItemHandsController); 2) quando operationOwner==null E (IsRedirectingHeal || BandAidHealActive), logar LogError uma vez ('ownership guard degradado — campos renomeados?') e tratar como operação do médico (fail-closed) em vez de devolver ao original; 3) documentar a decisão no comentário do guard.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). Fail-CLOSED durante redirect: owner não-resolvido com IsRedirecting/BandAidHealActive → bloqueia (return false) com LogError. Validação in-game pendente (P-2.9).

---

### CR-02-09 · B — Bug latente · 🟡 Médio ✅

**Mojibake residual em 3 strings visíveis ao jogador — contradiz o 'zero mojibake left' do commit wave 6**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:605` (dim: misc-delta)

**Problema:** O roundtrip cp1252→utf-8 por linha (CR-01-06) deixou passar literais duplo-encodados que chegam ao jogador: (1) BandAidUI.cs:605 — o marcador de membro destruído renderiza 'âœ•' em vez de '✕' (U+2715); (2) BandAidUI.cs:672 — subtítulo da HUD 'INDISPONÃVEL' em vez de 'INDISPONÍVEL' (o segundo byte 0x8D é caractere de controle invisível); (3) BandAidController.cs:438 — notificação do shoulder tap '$"Toque no ombro â†' {nick}"' em vez de '→' (alcançável pelo painel nativo via MedicInteractable.ShoulderTap). Além dessas, sobraram vários comentários com 'â†''/'TÃTULO' (cosmético de código). Verificado por bytes com cat -v nos arquivos atuais.

**Sugestão:** Repassar o detector nos 3 arquivos e corrigir os 3 literais ('✕', 'INDISPONÍVEL', '→'); esses casos escaparam porque são duplo-encoding de caracteres não-latinos (✕, →) ou com byte de controle (Í→0x8D), que o roundtrip por linha classificou como 'mixed-encoding — manter intacta'.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). 3 literais visíveis corrigidos (✕, INDISPONÍVEL, →) + setas/comentários; verificação byte-level = zero real. Validação in-game pendente (P-2.9).

---

### CR-02-10 · B — Bug latente · 🟢 Menor

**OnPatientEffectRemoved não confere a identidade do efeito: qualquer GInterface376 removido no HC do paciente dispara method_8 e finaliza a animação do médico cedo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:99` (dim: redirect-anim)

**Problema:** A subscription é no EffectRemovedEvent do HealthController inteiro do paciente e o filtro é só por interface. Se o paciente (bot no host, ou host-player sendo curado) tiver OUTRO MedEffect próprio removido durante a janela do redirect (bot que se automedicou instantes antes, efeito de comida/bebida GInterface376 expirando), o bridge invoca method_8 na operação do médico com um efeito alheio → método_7/method_4 no médico saudável → method_9 → animação do médico finaliza prematuramente (o postfix do AnimCleanupPatch zera IsRedirectingHeal/CurrentPatient). O redirect real continua no paciente e completa sozinho (NativeMedEffectApplied já é true → sem dupla aplicação), então o dano é cosmético/timing, mas o gatilho é espúrio. O vanilla tem a mesma forma, porém no PRÓPRIO HC, onde só existe um MedEffect ativo por vez.

**Sugestão:** Guardar o IEffect retornado por DoMedEffect (campo estático _currentPatientEffect ao lado de _subscribedPatientHc) e em OnPatientEffectRemoved exigir ReferenceEquals(effect, _currentPatientEffect) além do type-check; limpar no CleanupPatientSubscription.

**Decisão:**
- [ ] Pendente — Deferido (minor).
- [ ] Aceitar sugestão
- [ ] Rejeitar (deferir): _________________

---

### CR-02-11 · B — Bug latente · 🟢 Menor

**Bridge method_8 pode re-entrar em method_5 (cadeia multi-membro dirigida pelas feridas do MÉDICO) e disparar DoMedEffects extras no paciente fora da janela única do HealRoutine**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:109` (dim: redirect-anim)

**Problema:** Com ContinuousHealMode habilitado (setting do jogo), a Queue_0 da operação é construída de BodyPartsPriority do MÉDICO (Player.cs:32119-32126). Quando o bridge invoca method_8, o dequeue usa `MedsController_0._player.HealthController.CanApplyItem` — saúde do MÉDICO — e, se o médico tiver partes feridas restantes e o item tiver HpResource, method_8 chama method_5 de novo → o prefix intercepta de novo (IsRedirectingHeal ainda true) → SEGUNDO DoMedEffect no paciente, e assim por diante. O HealRoutine modela UMA aplicação (UseTime+2s): ao acordar, dessubscreve e força method_9 no meio da cadeia, deixando o último MedEffect completando em background no paciente. Resultado: nº de aplicações no paciente ditado pelas feridas do médico, consumo de HpResource acima do modelado, notificação 'Tratamento Completo' com efeito ainda rodando. Disparável hoje principalmente com itens de StartDelay=0 (ban

**Sugestão:** No prefix, quando a chamada vier da re-entrada do bridge (IsRedirectingHeal true e _currentObservedMedsControllerClass == __instance já setado), decidir explicitamente: ou permitir a cadeia e estender o modelo (re-armar timer/subscription por membro), ou cortá-la (esvaziar a Queue_0 via reflection na primeira interceptação — equivalente ao ClearQueue vanilla) para garantir exatamente 1 aplicação por interação, que é o contrato do handshake coop.

**Decisão:**
- [ ] Pendente — Deferido (minor) — observar logs no teste.
- [ ] Aceitar sugestão
- [ ] Rejeitar (deferir): _________________

---

### CR-02-12 · B — Bug latente · 🟢 Menor ✅

**Loop por classe usa Assembly.GetTypes() direto — perde a tolerância a ReflectionTypeLoadException que o PatchAll tinha**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:92` (dim: misc-delta)

**Problema:** O fix CR-01-14 trocou _harmony.PatchAll() pelo loop por classe com try/catch INTERNO — mas a fonte do foreach é Assembly.GetTypes(), fora de qualquer try. PatchAll usa AccessTools.GetTypesFromAssembly, que captura ReflectionTypeLoadException e devolve os tipos que carregaram (ex.Types.Where(t => t != null)). Se qualquer tipo do assembly falhar em carregar (ex.: Fika.Core ausente — TraumaFaintPacket implementa INetSerializable do Fika — ou API-break de update do EFT/Fika numa assinatura), GetTypes() lança, a exceção escapa do foreach e mata o resto do Awake (nenhum patch aplicado, patch de cleanup e handshake nunca registrados).

**Sugestão:** Trocar a fonte por AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()) (HarmonyLib já está referenciado) — mantém o loop por classe e recupera a tolerância a tipos não-carregáveis.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado em 2026-07-12 (mesma sessão). Trocado por AccessTools.GetTypesFromAssembly (tolera ReflectionTypeLoadException). Validação in-game pendente (P-2.9).

---

### CR-02-13 · F — Melhoria · 🟢 Menor

**HasEffect refaz MakeGenericMethod + Invoke boxeado a cada consulta — os 7 tipos agora são constantes de compile-time e o método fechado poderia ser cacheado 1x**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:280` (dim: misc-delta)

**Problema:** Com a HUD aberta, UpdateEffects roda a 4Hz (UPDATE_INTERVAL=0.25f, linha 655) para 7 membros × 7 efeitos = ~196 chamadas/s de HasEffect, cada uma refazendo MakeGenericMethod (aloca MethodInfo wrapper) + Invoke com object[] novo e boxing do EBodyPart. Antes do G-2 os tipos vinham de GetNestedType em runtime; agora são typeof(GInterfaceNNN) fixos — os 7 MethodInfo fechados podem ser construídos uma única vez em CacheTypes ao lado do mapa de tipos.

**Sugestão:** Em CacheTypes, substituir os 7 campos Type por um Dictionary<Type, MethodInfo> (ou 7 campos MethodInfo) com _findEffectMethod.MakeGenericMethod(typeof(GInterfaceNNN)) pré-fechado; HasEffect passa a só Invoke. Opcionalmente reutilizar um object[1] estático para o argumento.

**Decisão:**
- [ ] Pendente — Deferido (minor/perf).
- [ ] Aceitar sugestão
- [ ] Rejeitar (deferir): _________________

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-12 | Guilherme | Criação (review de delta) + aplicação imediata dos 8 achados acionáveis. |