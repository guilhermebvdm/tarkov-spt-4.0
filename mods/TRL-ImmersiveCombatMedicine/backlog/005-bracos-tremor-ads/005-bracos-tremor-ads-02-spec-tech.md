# 005 — Braços: Tremor + cancelamento de ADS escalonado · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Em progresso (aguardando review)
**Spec funcional:** [005-bracos-tremor-ads-01-spec.md](005-bracos-tremor-ads-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) + decompiles do spike 001 (`scratchpad/spike001/` — ilspycmd no Assembly-CSharp.dll REAL; o dump tem 102 namespaces vazios, ausência não é evidência). Toda referência ao código do EFT cita `arquivo.cs:linha`. Segunda fonte canônica: [docs/trauma-primitives.md](../../docs/trauma-primitives.md) — **P2 (Tremor)**, **P9 (cancela-ADS/lockout)**, **P5 (voz prioritária)**, com os textos CORRIGIDOS da verificação e da Rodada 2. Terceira fonte: o contrato do motor 002 **implementado** ([TraumaEngine.cs](../../modded/Patches/Trauma/TraumaEngine.cs), [TraumaEngineState.cs](../../modded/Patches/Trauma/TraumaEngineState.cs)) e o padrão de consumidor do 003 ([TraumaLegsConsumer.cs](../../modded/Patches/Trauma/TraumaLegsConsumer.cs)) — assinaturas citadas por `arquivo:linha` do próprio mod.

## 1. Estratégia

**Segundo consumidor do motor 002 — evento-first, 2 patches Harmony novos (lockout de re-ADS + contorno visual do tremor), ZERO mudança no motor.**

1. **O motor JÁ publica as linhas de braços** — nenhuma mudança no 002: `TraumaLine.ArmsTremor/ArmsTremorAdsCancel4s/3s/2s` ([TraumaEngineState.cs:25-28](../../modded/Patches/Trauma/TraumaEngineState.cs)), resolvidas por `TraumaMatrixResolver.ResolveArms` ([TraumaMatrixResolver.cs:40-54](../../modded/Patches/Trauma/TraumaMatrixResolver.cs)) na avaliação ([TraumaEngine.cs:531](../../modded/Patches/Trauma/TraumaEngine.cs)) e na reconciliação (:638-642). Braços não têm one-shot do motor (o cancela-ADS é timer contínuo do CONSUMIDOR, não `OneShotPublished`). O analgésico rebaixa NO RESOLVER (com-analgésico: Z1/Q1/Z1+Q1→None; Z2/Q2/Z2+Q2→Tremor) — o consumidor só re-deriva efeito da linha publicada, exatamente o "tremor re-derivado do ESTADO" da funcional (comportamento 1).
2. **Consumidor `TraumaArmsConsumer`** (MonoBehaviour no GO do plugin — padrão 003): registra-se no `TraumaConsumerRegistry` para `Arms` (destrava o toast de 1ª ocorrência — decisão 20; [TraumaEngineState.cs:132](../../modded/Patches/Trauma/TraumaEngineState.cs)) e assina `TraumaEngine.SubscribeWithSnapshot` ([TraumaEngine.cs:72](../../modded/Patches/Trauma/TraumaEngine.cs) — replay `Establishing=true` cobre assinatura tardia, religar e spawn ferido SEM toast/voz). Dono-only (D16) herdado do motor; **bots EXCLUÍDOS de TODOS os efeitos** (comportamento 5 da funcional — supersede D9 pré-spike): transição de braço de bot vira só log `arms EXCLUDED (bot)`. Efeitos apenas no humano local (`IsYourPlayer`) → headless é no-op por construção (lá só há bots) **+ gate explícito `FikaBackendUtils.IsHeadless` no `IsActive()`** (P9 rec. (b): o consumidor nem arma hooks/Update no headless — evita trabalho morto e voz acidental do player-shell; fika FikaBackendUtils.cs:49; `Fika.Core` já é referência hard do csproj).
3. **Tremor gerenciado (`TraumaTremor`, primitiva estática — P2):** instância única via `AddEffect<Tremor>` por reflection cacheada com fail-fast no boot (`typeof(ActiveHealthController).GetNestedType("Tremor", NonPublic)` + `GetMethod("AddEffect")` não-ambíguo + `MakeGenericMethod` — prova runtime + protótipo compilado do P2). Aplicação: `addTremor.Invoke(p.ActiveHealthController, new object[]{ armPart, 0f, null, null, null, null })` — **`delayTime=0f` OBRIGATÓRIO** (Tremor.DefaultDelay=15 s do globals; [scratchpad/spike001/ActiveHealthController.cs:3119-3121](../../docs/trauma-primitives.md)) e `workTime=null` = ∞ (DefaultWorkTime=+Infinity — lifecycle estado-driven, SEM renovação; :383,462-466). `Player.ActiveHealthController` é propriedade pública ([Player.cs:25291](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25291)). **Âncora no braço comprometido** (LeftArm/RightArm), nunca Head — escapa do lookup `method_16<Tremor>(Head)` de stim negativo (:2789-2806) e o wiring do flag é part-agnostic ([Player.cs:28959-28962](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28959) seta `EPhysicalCondition.Tremor` p/ GInterface361; :29030-29033 só limpa com `FindActiveEffect<GInterface361>()==null`). **Idempotência:** re-aplicar só se `_owned == null || !_owned.Existing` (GInterface331 = AddEffect SEMPRE cria instância nova, sem merge — AHC:3514-3538, confirmado neste worktree). **Re-âncora pós-cura (PA-01-06):** com instância viva, se o braço-âncora não está mais comprometido (cura parcial/cirurgia — `RestoreBodyPart` NÃO remove o efeito, só sync: AHC:3891-3907/4573) e `PickArmAnchor` aponta o OUTRO braço, `Remove("re-anchor")` + re-`Apply` na parte nova — ícone da UI de saúde acompanha o membro comprometido e a âncora curada sai do alcance de `method_16<Tremor>(parte)` de terceiros; com ambos comprometidos, mantém a âncora existente (sem churn). **Remoção SEMPRE pela própria instância:** `_owned.ForceResidue()` (fade 0.2 s) — NUNCA `method_15/16` (FirstOrDefault pode comer o tremor do Pain ou de stim — AHC:3615-3634); remoção executa inclusive com `IsAlive=false` (downed do Fika revive com o MESMO AHC — lição CR-02 do 003 aplicada ao tremor); **mundo morto usa `Discard` (bookkeeping-only)** — raid-end/world-swap NUNCA chamam `ForceResidue` num AHC destruído (eventos/sync em objeto morto — PA-01-01; padrão do 003, TraumaLegsConsumer.cs:157-175). **Watchdog (re-estabelecimento):** handler em `EffectResidualEvent`/`EffectRemovedEvent` do AHC local; efeito que saiu é `ReferenceEquals(_owned)` com linha de braço ainda ativa → flag `_reestablishPending`, re-aplica no PRÓXIMO Update do consumidor (nunca re-entra AddEffect dentro do dispatch do evento de saúde) + log `tremor REESTABLISH`; `Remove` anula `Owned` ANTES do `ForceResidue` (o `EffectResidualEvent` dispara SÍNCRONO dentro dele — a remoção PRÓPRIA não latcha pending espúrio) e o re-apply tem piso de 0,5 s entre tentativas + `LogWarning` 1×/sessão após 3 re-applies na mesma linha (diagnóstico de removedor externo por tick — PA-01-05).
4. **Contorno da supressão visual do analgésico (P2 — mecanismo real):** o vanilla NÃO remove efeito nenhum sob analgésico — a supressão é um **gate visual** em `ProceduralWeaponAnimation.PhysicalConditionUpdated`: com `OnPainkillers` setado força `Breath.TremorOn=false` IGNORANDO o flag Tremor ([scratchpad/spike001/ProceduralWeaponAnimation.cs:1182-1186](../../docs/trauma-primitives.md) — relido neste worktree; senão `TremorOn=(full & Tremor)!=0`, :1189). Contorno = **postfix** nesse método re-assertando `__instance.Breath.TremorOn = true` quando a **NOSSA instância** existe (`_owned.Existing` = Added|Started, AHC:219-233 — cobre o gap Added→Started de até 1 tick do frame da aplicação, PA-01-03; Residued fica fora, fade não re-asserta — + `ReferenceEquals(__instance, ownedPlayer.ProceduralWeaponAnimation)`) — checar a instância, NUNCA o flag `EPhysicalCondition.Tremor` (o flag também fica true por ZombieInfection/stim, cujo gate sob analgésico é intencional). Complemento: write direto `Breath.TremorOn=true` logo após aplicar (o postfix só roda em MUDANÇA de condição; o campo é consumido por frame — BreathEffector.cs:74,182). **Coexistência sem intensificação dupla por construção:** o shake do BreathEffector é `flag = TremorOn || Fracture` (OR booleano, não soma — BreathEffector.cs:182); o tremor-por-dor vanilla (filho do Pain, morre com ele — AHC:2103-2114) segue o comportamento vanilla sob analgésico.
5. **Cancela-ADS escalonado — detecção por EVENTO (substitui o polling do legado):** assinar `player.HandsChangedEvent` ([Player.cs:25544](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25544), invocado em :31646) e, quando o controller é `Player.FirearmController`, `fc.OnAimingChanged` (evento público, :20037; invocado SÓ em mudança real de estado, :20146-20149). **Âncora do timer = `max(edge de mira, entrada na linha)`:** (a) mirar com linha 2-braços ativa → âncora no edge true; (b) linha ativada MID-ADS (dano mirando) → âncora na transição; (c) **mudança de linha mid-ADS → timer REINICIA com o N da nova linha** (comportamento 3 da funcional — sem cancelamento retroativo); (d) rebaixar p/ Tremor/None → timer DESCARTADO; (e) soltar a mira → reset. Deadline checado no `Update` do consumidor SÓ com timer armado (1 comparação de float/frame — precisão de 1 frame cobre ±0,25 s; zero polling de estado). **Cancel pelo funil vanilla (P9):** `if (p.HandsController is IFirearmHandsController f && p.ProceduralWeaponAnimation.IsAiming) f.SetAim(false)` — funil ÚNICO de mira ([Player.cs:13695-13743](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L13695)); o setter `IsAiming` faz o desmonte completo (HoldBreath, slowdown, animator, sensibilidade — :12136-12168) e o `FikaClientFirearmController.SetAim` emite `ToggleAimPacket(-1)` → espelho abaixa a arma NATIVAMENTE (fika FikaClientFirearmController.cs:216-227; ToggleAimPacket.cs:28-34; ObservedFirearmController.cs:148-161). O guard `IsAiming` já cobre o early-return de blindfire (:13711-13716); armas estacionárias/montadas usam o mesmo funil (:13721-13724); "mira" de `UsableItemController` fica FORA (funil separado, :21442-21461 — limitação registrada na funcional).
6. **Lockout de re-ADS (decisão 17 — P9):** prefix em `Player.FirearmController.SetAim(bool)` que retorna `false` quando `value==true` + lockout ativo + humano local — **bloqueia TODAS as rotas de re-entrada** porque todas convergem no funil: input/ToggleAim (:13701), troca de scope `SetAim(int)` (:13705-13709), re-aim automático de fast-slot (:10670-10673), restauração pós-overlap (:13067-13070). Lockout guardado por **ProfileId do jogador** (persiste à troca de arma — é do jogador, não da arma); `lockoutUntil = Time.time + cfg` setado no cancel. **Invisível ao peer:** o prefix impede a mudança de `IsAiming` e o flag `AimingInterruptedByOverlap` (limpo pelo nosso `SetAim(false)`, :13729; só re-setável com `IsAiming==true`, :13062) permanece false → o FikaClientFirearmController não emite pacote (correção da verificação do P9). **Voz de dor da tentativa — mecanismo prioritário do P5:** `p.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat, demand: true, importance: 100)` (fura o Busy do Speaker — PhraseSpeakerClass.cs:206-227; `Player.Say` com demand NÃO fura, :28799-28829), audível aos peers via PhrasePacket (fika FikaPlayer.cs:1093-1103). **Throttle: 1 voz por janela de lockout** (flag `voicePlayed` zerado a cada cancel — cobre modo hold sem spam, premissa P-005-A); **o flag só é consumido se `Speaker.Play` retornar não-null (TagBank)** — dois gates ENGOLEM a chamada mesmo com importance=100: Busy com outra frase importance ≥100 em curso (`100 <= Int_0` → skip, PhraseSpeakerClass.cs:206-210) e Blocker de GRUPO de fala (`SpeakerManager.FreeToSpeak` — SpeakerManager.cs:106-131 + GClass2291:50-59, sem bypass por demand/importance; squads coop alimentam o Blocker, inclusive o OnAgony do item 004); engolido → log `voice=skipped(busy|blocked)` e a PRÓXIMA tentativa da mesma janela re-tenta (custo = 1 Play por tentativa, cadência de input — PA-01-02) + **guard de blackout** (sem voz durante inconsciência — `TraumaState.BlackoutTimers`; o SilenceVoicePatch legado só cobre `Player.Say`, não `Speaker.Play` direto). Colateral cosmético aceito: `ToggleAim` executa `RemoveLeftHandItem`/`SetCompassState(false)` ANTES do SetAim bloqueado — bússola/mão esquerda podem recolher na tentativa (raro; reavaliar no playtest — corner da funcional).
7. **Aposentadoria do legado de braços (D10, comportamento 7):** sai a fadiga de mira 1 s do `MainLoopPatch` ([MovementPatches.cs:119-139](../../modded/Patches/Trauma/MovementPatches.cs) — polling `ProceduralWeaponAnimation.IsAiming` + `SetAim(false)` + voz "TryAim"), sai a voz "Arm" em hit de braço zerado do `HealthPatches` (:113-119 — paridade com a aposentadoria do 003, que removeu também a voz de hit de perna; o feedback de entrada agora é o toast de 1ª ocorrência + tremor visível) e saem `AimingFatigueTimers` + entrada no `ResetAll` do `TraumaState` (:22,:43). `Sistema de Braços` (config antigo) permanece bindado porém **INERTE** (tooltip atualizado; migração/remoção no item 010 — mesmo padrão do `Sistema de Pernas`). A migração mojibake existente do `MigrateOrphanedConfigKeys` (Plugin:238-263) permanece — escreve numa key inerte, inofensivo.
8. **Rename-at-delivery do toggle (padrão 003):** `"Arms Effects (item 005)"` → `"Arms Effects"`, nasce **ON** (master governa); a key órfã do placeholder é DELETADA em `MigrateOrphanedConfigKeys` SEM copiar o valor (o `false` de placeholder não é escolha do usuário — Plugin:265-287, lição CR-03-01).

**Alternativas descartadas:** (a) remoção de tremor "por tipo" (`method_15/16`) — lookup FirstOrDefault acerta tremor do Pain/stim (P2, bloqueio central do D11); (b) renovação periódica do efeito — desnecessária, DefaultWorkTime=∞ (P2); (c) escrever o flag `EPhysicalCondition.Tremor` direto — o wiring vanilla recomputa do estado dos efeitos e o flag é compartilhado com zumbi/stim (AP-04); (d) polling de `IsAiming` por frame (o legado) — substituído por evento, decisão da funcional; (e) estender o prefix a `ToggleAim` para suprimir o colateral bússola/mão esquerda — superfície extra de patch por cosmético raro, rejeitado (limitação aceita na funcional); (f) postfix em `BreathEffector.Process` (estilo Realism) — ponto mais quente (por frame) e mod não está na load order; `PhysicalConditionUpdated` (só em mudança de condição) é o ponto mínimo; (g) tremor em bots — canal condicional de pontaria não medido e assimétrico por topologia (funcional 5; premissa p/ item 011).

## 2. Pontos de patch

**2 patches Harmony novos** + hooks C# do motor 002 (o "ponto de patch" real do consumidor):

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/Player.cs` — `Player.FirearmController.SetAim(bool)`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L13711) (público virtual, nested; resolvido por `AccessTools.Method(typeof(Player.FirearmController), "SetAim", new[]{typeof(bool)})` — mesmo alvo que RecoilRework/FOVFix resolvem em produção, IL provado no P9) | Prefix (NOVO) | Lockout de re-ADS: `value==true` + lockout ativo + humano local → `return false` (pula o original; sem pacote → invisível ao peer) + voz throttled. Overrides auditados (AP-03): `FikaClientFirearmController.SetAim` CHAMA `base.SetAim` (fika :216-227) → humano local Fika passa pelo corpo patchado; `BotFirearmController` (sealed : FikaClientFirearmController) e `AIFirearmController` vanilla descem até a base SEM override de SetAim → **guard `IsYourPlayer`/`!IsAI` OBRIGATÓRIO** (D9/comportamento 5); `ObservedFirearmController.SetObservedAim` seta o `IsAiming` sobrescrito SEM `base.SetAim` (fika :49-68,148-161) → espelhos nunca atravessam o prefix. Postfixes de RecoilRework/FOVFix rodam mesmo com o skip (semântica Harmony) e são idempotentes — D13 confirmado, sem ordenação. |
| `EFT.Animations.ProceduralWeaponAnimation.PhysicalConditionUpdated(EPhysicalCondition, EPhysicalCondition)` (público, instância — scratchpad/spike001/ProceduralWeaponAnimation.cs:1175-1192, relido neste worktree) | Postfix (NOVO) | Contorno da supressão visual do analgésico (P2 rec. (e)): o corpo vanilla força `Breath.TremorOn=false` sob `OnPainkillers` (:1182-1186); o postfix re-asserta `true` SÓ quando a NOSSA instância EXISTE (`Owned.Existing` = Added|Started — cobre o gap Added→Started, PA-01-03) E o `__instance` é o PWA do dono do tremor gerenciado (`ReferenceEquals`) — nunca pelo flag `EPhysicalCondition.Tremor`. Método não-virtual chamado direto pelo wiring do Player → sem superfície AP-03; PWA de bots/espelhos nunca casa o `ReferenceEquals` (tremor é só do humano local). Corpo = 1 write de bool (AP-07 seguro: nada re-dispara o método). |

| Hook C# (motor 002) | Assinatura | Uso |
|---|---|---|
| `TraumaEngine.SubscribeWithSnapshot` | `void (Action<TraumaTransition>)` — replay `Establishing=true` | Entrada/saída/rebaixamento das linhas de braço (From/To/Establishing/PainkillerActive) — [TraumaEngine.cs:72](../../modded/Patches/Trauma/TraumaEngine.cs) |
| `TraumaEngine.GetLine` | `TraumaLine (Player, TraumaRegion)` | Religar toggle mid-raid (estabelecer do snapshot) + re-validação do watchdog/re-establish — :48 |
| `TraumaEngine.IsOwnedHere` | `bool (Player)` (internal — mesmo assembly, dependência já registrada no 003) | Sweep do religar via `RegisteredPlayers` — :110 |
| `TraumaConsumerRegistry.Register` | `void (TraumaConsumerId, TraumaRegion[], Func<bool>)` | `ArmsEffects` cobre `Arms` — destrava toast (decisão 20) — [TraumaEngineState.cs:132](../../modded/Patches/Trauma/TraumaEngineState.cs) |

**APIs de aplicação (sem patch — chamadas diretas no dono humano):** `Player.ActiveHealthController` get ([Player.cs:25291](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25291)) · `AddEffect<Tremor>` via reflection (AHC:3514-3538; prova runtime P2) · `ActiveHealthController.GClass3008` nested PÚBLICO com `ForceResidue()/ForceRemove()/Existing/Active` (AHC:29,219-233,550-570,637-662) · `IHealthController.EffectResidualEvent/EffectRemovedEvent` (watchdog — mesmas assinaturas do motor) · `IHealthController.IsBodyPartDestroyed/IsBodyPartBroken` (escolha da âncora de braço) · `FindActiveEffect<GInterface361>()` (query sem reflection) · `Player.HandsChangedEvent` (:25544) · `Player.FirearmController.OnAimingChanged` (:20037) · `IFirearmHandsController.SetAim(bool)` (uso existente no mod — MovementPatches.cs:47) · `ProceduralWeaponAnimation.IsAiming` (uso existente — MovementPatches.cs:123) · `Breath.TremorOn` (campo público — BreathEffector.cs:74) · `Speaker.Play(EPhraseTrigger, ETagStatus, bool, int?)` — retorna `TagBank` null quando engolido (protótipo P5 compilado) · `FikaBackendUtils.IsHeadless` (gate headless — fika FikaBackendUtils.cs:49; P9 rec. (b)) · `AccessTools.Field(typeof(Player.FirearmController), "_player")` (mesmo acesso do FOVFix — P9).

## 3. Novas propriedades F12 (BepInEx)

Seção nova `8. Trauma 2.0 (Braços)` + 2 edições nas seções 6/2. `PROPRIEDADES.md` atualizado na entrega.

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `8. Trauma 2.0 (Braços)` | `ADS Cancel Seconds (Zeroed x2)` | float | `4` | 1 a 10 | — | Segundos de mira sustentada com 2 braços ZERADOS até o cancelamento do ADS. Soltar a mira reseta o timer. |
| `8. Trauma 2.0 (Braços)` | `ADS Cancel Seconds (Fractured x2)` | float | `3` | 1 a 10 | — | Segundos com 2 braços FRATURADOS até o cancelamento (fratura pior que zerado por design — decisão 3). |
| `8. Trauma 2.0 (Braços)` | `ADS Cancel Seconds (Zeroed + Fractured x2)` | float | `2` | 1 a 10 | — | Segundos com 2 braços zerados E 2 fraturados. Efetivo = min dos três timers — a linha mais severa nunca fica mais lenta que as outras (warn no log, 1x). |
| `8. Trauma 2.0 (Braços)` | `Re-ADS Lockout Seconds` | float | `1.5` | 1.0 a 1.5 | — | Bloqueio de re-mirar após o cancelamento (persiste à troca de arma). Tentativa durante o bloqueio dispara voz de dor (1 por janela). Faixa fixada pela decisão 17 (1–1,5 s). |
| `6. Trauma 2.0 (Consumidores)` | `Arms Effects` | bool | **`true`** (era `false` no placeholder) | — | — | Tremor contínuo + cancelamento de ADS escalonado (item 005). Governado pelo master Trauma 2.0; desligar mid-raid remove o tremor e cancela o lockout. |
| `2. Mecanicas (Trauma)` | `Sistema de Braços` | bool | `true` | — | — | (INERTE desde a v1.5.0 — substituído pelo Trauma 2.0 / Arms Effects. Remoção da key no item 010.) |

Estado neutro: toggle 005 off = zero efeito de braços do mod (só rastreamento/log do motor). Configs lidas por `.Value` a cada uso (sem cache). **Sanidade dos timers:** o efetivo da linha Z2+Q2 é `min(cfgZ2Q2, cfgZ2, cfgQ2)` — configuração invertida não deixa a linha mais severa (ranking do enum) mais lenta que as menos severas; warn 1×/sessão quando o clamp atua (padrão min(N2,N1) do 003). Z2 vs Q2 entre si é livre (decisão 3 é default, não invariante). **Faixa do lockout:** 1.0–1.5 s conforme decisão 17 + funcional (default 1,5 s) — ver abertura 2.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/Trauma/TraumaArmsConsumer.cs` | CRIAR | Componente no GO do plugin: registry + SubscribeWithSnapshot; mapa linha→efeito (toda linha de braço inclui tremor — decisão 3; AdsCancel-tier arma o timer); exclusão de bots com log; detecção de ADS por evento (HandsChangedEvent/OnAimingChanged com subscribe/unsubscribe simétrico); timer com âncora `max(edge, entrada na linha)` + reinício por mudança de linha; cancel + lockout + voz throttled (janela só consumida se o Play tocou — PA-01-02); watchdog/re-establish do tremor (piso 0,5 s + warn de conflito externo — PA-01-05); edges do toggle mid-raid; sweeps de world-null/world-swap via `Discard` (bookkeeping-only, sem tocar AHC morto — PA-01-01; lições CR-02 do 003). |
| `modded/Patches/Trauma/TraumaTremor.cs` | CRIAR | Primitiva do tremor gerenciado (P2): cache reflection fail-fast (nested `Tremor` + `AddEffect` genérico), `Apply` idempotente (delay=0, work=∞, âncora no braço comprometido, re-âncora pós-cura — PA-01-06, write `Breath.TremorOn=true` pós-apply), `Remove` pela PRÓPRIA instância (`ForceResidue`; campos anulados ANTES — PA-01-05), `Discard` bookkeeping-only p/ mundo morto (PA-01-01), estado `Owned/OwnedPlayer/OwnedAnchor` consumido pelo postfix do PWA. Degradação graciosa se a reflection falhar na RESOLUÇÃO ou em RUNTIME (try/catch no Apply/Remove — PA-01-04; premissa P-005-B). |
| `modded/Patches/Trauma/ArmsAimPatches.cs` | CRIAR | 2 patches (§2): prefix `SetAim(bool)` (lockout — guards IsYourPlayer/!IsAI; try/catch; skip sem pacote) + postfix `PhysicalConditionUpdated` (re-assert `Breath.TremorOn` da instância gerenciada sob analgésico). |
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | Binds §3 (seção 8 + rename do toggle p/ `Arms Effects` default ON + tooltip INERTE no `Sistema de Braços`); delete da key órfã `"Arms Effects (item 005)"` no `MigrateOrphanedConfigKeys` (sem copiar valor — padrão 003, Plugin:265-287); `AddComponent<TraumaArmsConsumer>()` após o motor; bump `1.5.0`. |
| `modded/Patches/Trauma/MovementPatches.cs` | MODIFICAR | **Aposentar fadiga de mira legada (D10):** remover o bloco "Braços Quebrados (Fadiga ao mirar)" (:119-139 — polling 1 s + SetAim(false) + voz "TryAim"). `MainLoopPatch` fica só desmaio/grace. |
| `modded/Patches/Trauma/HealthPatches.cs` | MODIFICAR | Remover SÓ o sub-bloco de braços do Postfix (:113-119 — voz "Arm" em hit de braço zerado; fronteira igual à do 003 com pernas). Desmaio e estômago INTACTOS (fronteiras 007/006). |
| `modded/Patches/Trauma/TraumaState.cs` | MODIFICAR | Remover `AimingFatigueTimers` (:22) + entrada no `ResetAll` (:43) — campo órfão após a aposentadoria. |
| `modded/Patches/Trauma/TraumaLocale.cs` | MODIFICAR | Calibrar (se preciso) os textos EN/PT `ArmsTremor`/`ArmsAdsCancel` — chaves JÁ existem desde o 002 ([TraumaLocale.cs:6,67-70](../../modded/Patches/Trauma/TraumaLocale.cs)); nenhuma chave nova. |
| `PROPRIEDADES.md` | MODIFICAR | Seção 8 nova; toggle `Arms Effects` ON; `Sistema de Braços` marcado inerte com nota de migração p/ 010 (gate de entrega). |

## 5. Stubs de código

> Pré-código: assinaturas completas + corpo mínimo plausível. Cada referência tem `// ref:`. Assinaturas do EFT re-verificadas no decompile do assembly real (spike001) ou no dump; contrato do motor citado do código implementado.

```csharp
// modded/Patches/Trauma/TraumaTremor.cs
using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Primitiva do TREMOR GERENCIADO (item 005 — P2): no máximo 1 instância por processo
    /// (efeito só no humano local — bots excluídos, funcional 5). Remoção SEMPRE pela própria instância.</summary>
    internal static class TraumaTremor
    {
        // Reflection cacheada com FAIL-FAST (csharp-best-practices §3): resolver 1x; falha → tremor degrada
        // p/ no-op logado como ERROR e o resto do 005 (ADS-cancel/lockout) segue — premissa P-005-B.
        private static MethodInfo _addTremor;   // AddEffect<Tremor> fechado — ref: P2 prova runtime (GetMethod("AddEffect") não-ambíguo)
        private static bool _resolveTried, _resolveOk;

        /// <summary>Instância gerenciada + dono + âncora — consumidos pelo postfix do PWA (ArmsAimPatches)
        /// e pela re-âncora pós-cura (PA-01-06).</summary>
        internal static ActiveHealthController.GClass3008 Owned;  // nested PÚBLICO — ref: AHC:29,219-233 (P2 evid.)
        internal static Player OwnedPlayer;
        internal static EBodyPart OwnedAnchor;                    // parte da instância viva (re-âncora — PA-01-06)

        internal static bool EnsureResolved()
        {
            if (_resolveTried) return _resolveOk;
            _resolveTried = true;
            // Type tremor = typeof(ActiveHealthController).GetNestedType("Tremor", BindingFlags.NonPublic); // ref: AHC:3117-3122
            // _addTremor = typeof(ActiveHealthController).GetMethod("AddEffect").MakeGenericMethod(tremor);
            // _resolveOk = tremor != null && _addTremor != null; se falhou → LogError 1x ("[Trauma2] tremor reflection FAILED — tremor disabled")
            return _resolveOk;
        }

        /// <summary>Aplica se não há instância viva (Owned==null || !Owned.Existing — GInterface331 empilha, AHC:3514-3538).
        /// delayTime=0f OBRIGATÓRIO (DefaultDelay=15s do globals — AHC:3119); workTime=null=∞ (AHC:383,462-466).
        /// armAnchor = braço COMPROMETIDO (escapa do lookup method_16&lt;Tremor&gt;(Head) de stim — AHC:2789-2806).
        /// Instância viva com âncora DIVERGENTE e parte antiga CURADA → re-âncora (PA-01-06).</summary>
        internal static void Apply(Player p, EBodyPart armAnchor)
        {
            if (!EnsureResolved() || p == null) return;
            if (Owned != null && Owned.Existing && ReferenceEquals(OwnedPlayer, p))
            {
                // re-âncora (PA-01-06): cura parcial moveu o braço comprometido — RestoreBodyPart NÃO remove
                // o efeito (só sync — AHC:3891-3907/4573) e a idempotência deixaria a âncora num braço SAUDÁVEL
                // (ícone da UI de saúde errado + alcance de method_16<Tremor>(parte) de terceiros).
                // Com a parte antiga AINDA comprometida (ambos os braços), mantém — sem churn.
                // if (armAnchor == OwnedAnchor || IsBodyPartDestroyed(OwnedAnchor) || IsBodyPartBroken(OwnedAnchor)) return;
                // Remove("re-anchor"); // e segue p/ criar na parte nova (AddEffect força-remove o residued da mesma parte antes do Create)
            }
            // try {                                                                     // PA-01-04: Apply roda dentro do
            //     Owned = (ActiveHealthController.GClass3008)_addTremor.Invoke(         // dispatch DESPROTEGIDO do motor
            //         p.ActiveHealthController,                                         // (StateChanged?.Invoke — TraumaEngine.cs:565);
            //         new object[] { armAnchor, 0f, null, null, null, null });          // exceção escapando abortaria a consolidação
            //     OwnedPlayer = p; OwnedAnchor = armAnchor;                             // do frame p/ TODOS os consumidores
            // } catch (Exception ex) { LogError 1x ("[Trauma2] tremor APPLY FAILED — tremor disabled: " + ex);
            //     _resolveOk = false; Owned = null; OwnedPlayer = null; return; }       // degrada SÓ o tremor (P-005-B)
            // ref: Player.cs:25291 (ActiveHealthController — propriedade pública)
            // p.ProceduralWeaponAnimation.Breath.TremorOn = true; // edge de flag sem update — ref: P2 rec. (b); BreathEffector.cs:74
            // log: "[Trauma2] arms tremor ON <profileId> anchor=<armAnchor> pk=<IsUnderPainkiller>"
        }

        /// <summary>Remove a PRÓPRIA instância: ForceResidue() (fade 0.2s — AHC:550-570), NUNCA method_15/16
        /// (FirstOrDefault come tremor do Pain/stim — AHC:3615-3634). Roda inclusive com IsAlive=false
        /// (downed Fika revive com o MESMO AHC — lição CR-02 do 003). Usar em state-exit/rebaixamento p/ None/
        /// toggle-off/re-âncora — mundo morto usa Discard (PA-01-01). Membros gerenciados: seguro pós-destroy.</summary>
        internal static void Remove(string reason)
        {
            // var owned = Owned;
            // Owned = null; OwnedPlayer = null;   // anular ANTES do ForceResidue (PA-01-05): o EffectResidualEvent
            //                                     // dispara SÍNCRONO dentro dele — com Owned==null o watchdog falha o
            //                                     // ReferenceEquals e a remoção PRÓPRIA não latcha pending espúrio
            // try { if (owned != null && owned.Existing) owned.ForceResidue(); }
            // catch (Exception ex) { LogError 1x + _resolveOk = false; }                // PA-01-04 (degradação P-005-B)
            // log: "[Trauma2] arms tremor OFF <profileId> (<reason>)"
        }

        /// <summary>Descarte bookkeeping-only (raid-end/world-swap — PA-01-01): efeito/AHC morreram com o
        /// Player — ForceResidue aqui dispararia method_0 → eventos do AHC destruído + sync de rede
        /// (AHC:462-476; method_36 = SendNetworkSyncPacket, AHC:4573) no meio do teardown da sessão Fika.
        /// Padrão do 003 (TraumaLegsConsumer.cs:157-175: "só limpar bookkeeping").</summary>
        internal static void Discard(string reason)
        {
            Owned = null; OwnedPlayer = null;
            // log: "[Trauma2] arms tremor DISCARD (<reason>)"
        }

        /// <summary>Consulta do watchdog: o efeito que saiu é o NOSSO?</summary>
        internal static bool IsOurs(IEffect e) { return Owned != null && ReferenceEquals(e, Owned); }
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaArmsConsumer.cs
using System;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using Fika.Core.Main.Utils; // FikaBackendUtils.IsHeadless (gate P9 rec. (b) — PA-01-07)
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor de BRAÇOS (spec 005): tremor gerenciado + cancela-ADS escalonado + lockout.
    /// Evento-first (OnAimingChanged substitui o polling do legado). Efeitos SÓ no humano local
    /// (bots EXCLUÍDOS — funcional 5; headless no-op por construção). ZERO mudança no motor 002.</summary>
    public sealed class TraumaArmsConsumer : MonoBehaviour
    {
        private static TraumaArmsConsumer _instance;
        private static bool _timerClampWarned; // warn 1x do min() dos timers (§3)

        // ---- estado (humano local apenas — campos únicos, não dict) ----
        private TraumaLine _localLine;            // linha de braço APLICADA ao humano local
        private Player _localPlayer;              // referência p/ desfazer/re-estabelecer
        private Player.FirearmController _hookedFc;               // p/ -= simétrico de OnAimingChanged
        private Action<IHandsController> _handsHandler;           // p/ -= simétrico de HandsChangedEvent
        private Action<IEffect> _effectGoneHandler;               // watchdog (Residual/Removed) — p/ -= simétrico
        private bool _reestablishPending;         // remoção externa detectada → re-aplica no próximo Update
        private float _nextReestablishAt;         // piso de 0,5s entre re-applies do watchdog (PA-01-05)
        private int _reestablishCount;            // re-applies na linha CORRENTE (zera na mudança de linha)
        private static bool _reestablishStormWarned; // LogWarning 1x/sessão após 3 re-applies na mesma linha (PA-01-05)
        private float _aimAnchor = -1f;           // <0 = sem mira sustentada; senão Time.time da âncora
        private float _lockoutUntil;              // Time.time do fim do lockout (0 = inativo)
        private string _lockoutProfileId;         // dono do lockout (defesa contra stale entre raids)
        private bool _lockoutVoicePlayed;         // throttle: 1 voz por janela (funcional 4)
        private bool _wasActive;
        private GameWorld _trackedWorld;          // world-swap sem null (transit) — lição CR-02 do 003

        private static readonly TraumaRegion[] ArmsRegions = { TraumaRegion.Arms };

        private void Awake()
        {
            _instance = this;
            TraumaConsumerRegistry.Register(TraumaConsumerId.ArmsEffects, ArmsRegions, IsActive); // destrava toast (decisão 20)
            TraumaEngine.SubscribeWithSnapshot(OnTransition); // replay establishing — ref: TraumaEngine.cs:72
        }

        internal static bool IsActive()
        {
            return !FikaBackendUtils.IsHeadless // P9 rec. (b): headless nem arma hooks/Update (fika FikaBackendUtils.cs:49) — PA-01-07
                && TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerArmsEffects.Value;
        }

        /// <summary>Linhas que armam o timer de cancela-ADS (2 braços comprometidos).</summary>
        internal static bool IsAdsCancelTier(TraumaLine line)
        {
            return line == TraumaLine.ArmsTremorAdsCancel4s
                || line == TraumaLine.ArmsTremorAdsCancel3s
                || line == TraumaLine.ArmsTremorAdsCancel2s;
        }

        /// <summary>Timer efetivo por linha; Z2+Q2 = min dos três (linha mais severa nunca mais lenta — warn 1x).</summary>
        internal static float LineCancelSeconds(TraumaLine line) { /* §3 */ return 0f; }

        private void OnTransition(TraumaTransition t)
        {
            if (t.Region != TraumaRegion.Arms) return;
            if (!IsActive()) return; // toggle off = ignora (motor segue publicando — comportamento 9 do 002)
            Player p = t.Player;
            if (p is null) return;
            if (p.IsAI)
            {
                // Bots EXCLUÍDOS de tremor E cancela-ADS (funcional 5 — supersede D9 pré-spike). AC-8.
                if (t.To != TraumaLine.None)
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] arms EXCLUDED (bot) {p.ProfileId} line={t.To}");
                return;
            }
            if (!p.IsYourPlayer) return; // defesa extra — motor só publica donos; espelho nunca chega (D16)
            ApplyLine(p, t.To);          // establishing idem: aplica SEM toast/voz (toast é gate do motor)
        }

        private void ApplyLine(Player p, TraumaLine line)
        {
            if (line == TraumaLine.None) { TearDownLocal("state-exit"); return; } // worldDead=false → Remove (AHC VIVO)
            if (line != _localLine) _reestablishCount = 0;   // linha nova → zera a contagem do watchdog (PA-01-05)
            _localPlayer = p; _localLine = line;
            // 1. Tremor: TODA linha de braço inclui tremor (decisão 3); Apply re-ancora se a âncora foi curada (PA-01-06)
            TraumaTremor.Apply(p, PickArmAnchor(p));  // âncora = braço zerado/quebrado (IsBodyPartDestroyed/Broken)
            EnsureWatchdog(p);                        // Residual/Removed → IsOurs + GetLine!=None → _reestablishPending
            // 2. Timer de ADS: tier 2-braços arma detecção; senão desarma e DESCARTA timer (mudança de linha
            //    mid-ADS REINICIA no Update via re-âncora — comportamento 3 da funcional)
            if (IsAdsCancelTier(line)) { EnsureAimHooks(p); _aimAnchor = IsAimingNow(p) ? Time.time : -1f; } // linha mudou mid-ADS → reancora AGORA
            else { TearDownAimHooks(); _aimAnchor = -1f; }
        }

        private void OnAimingChanged(bool aiming)
        {
            _aimAnchor = aiming ? Time.time : -1f; // edge por EVENTO (Player.cs:20037; invocado só em mudança real :20146-20149)
        }

        private void OnHandsChanged(IHandsController c)
        {
            // troca de arma: re-subscrever no novo FirearmController; timer reseta (novo controller nasce sem mira);
            // lockout NÃO reseta (é do jogador — funcional/corner). c não-firearm (meds/granada) → desarmado até voltar.
        }

        /// <summary>Desfaz os efeitos locais + hooks/timer. worldDead=false (state-exit/toggle-off):
        /// `TraumaTremor.Remove` — ForceResidue no AHC VIVO (inclusive downed — CR-02). worldDead=true
        /// (raid-end/world-swap): `TraumaTremor.Discard` — bookkeeping-only, o AHC morreu com o Player e
        /// ForceResidue dispararia eventos/sync em objeto destruído (PA-01-01; padrão TraumaLegsConsumer.cs:157-175).
        /// Ambos: TearDownAimHooks + watchdog off + _aimAnchor=-1 + _localPlayer/_localLine/_reestablish* limpos.</summary>
        private void TearDownLocal(string reason, bool worldDead = false)
        {
            // if (worldDead) TraumaTremor.Discard(reason); else TraumaTremor.Remove(reason);
            // TearDownAimHooks(); watchdog -=; _aimAnchor = -1f; _localPlayer = null; _localLine = TraumaLine.None;
            // _reestablishPending = false; _reestablishCount = 0;
        }

        private void ExecuteCancel(Player p)
        {
            // guard IsAiming cobre blindfire (early-return vanilla Player.cs:13711-13716)
            // if (p.HandsController is IFirearmHandsController f && p.ProceduralWeaponAnimation.IsAiming) f.SetAim(false);
            //   → funil vanilla: teardown completo (setter IsAiming :12136-12168) + ToggleAimPacket(-1) ao peer (P9)
            // _lockoutUntil = Time.time + Mathf.Clamp(cfgLockout, 1f, 1.5f); _lockoutProfileId = p.ProfileId;
            // _lockoutVoicePlayed = false; _aimAnchor = -1f;
            // log: "[Trauma2] ads CANCEL <profileId> line=<line> n=<N:0.##> lockout=<L:0.##>"
        }

        /// <summary>Chamado pelo prefix de SetAim (ArmsAimPatches): true = BLOQUEAR o re-ADS.</summary>
        internal static bool TryBlockReAds(Player p)
        {
            TraumaArmsConsumer inst = _instance;
            if (inst == null || p is null) return false;
            if (!IsActive()) return false; // toggle off mid-raid → lockout morto (corner da funcional)
            if (inst._lockoutUntil <= 0f || Time.time >= inst._lockoutUntil) return false;
            if (!string.Equals(inst._lockoutProfileId, p.ProfileId, StringComparison.Ordinal)) return false;
            if (!inst._lockoutVoicePlayed)
            {
                // guard de blackout: !TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) → sem voz inconsciente (corner D3)
                // var bank = p.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat, demand: true, importance: 100);
                //   ref: P5 (fura Busy de importance<100 — PhraseSpeakerClass.cs:206-227; Say+demand NÃO fura :28799-28829); peers via PhrasePacket
                // inst._lockoutVoicePlayed = bank != null; // PA-01-02: só consome a janela se TOCOU — Play retorna null
                //   engolido por (a) Busy com frase importance>=100 em curso (100 <= Int_0 → skip, :206-210) e
                //   (b) Blocker de GRUPO (SpeakerManager.FreeToSpeak :106-131 + GClass2291:50-59 — squad coop, sem
                //   bypass por demand/importance); próxima tentativa da MESMA janela re-tenta (1 Play por tentativa,
                //   cadência de input — sem hot path)
                // log: bank != null → "[Trauma2] ads LOCKOUT BLOCK <profileId> remaining=<t:0.##> voice=true"
                //      bank == null → "[Trauma2] ads LOCKOUT BLOCK <profileId> remaining=<t:0.##> voice=skipped(busy|blocked)"
            }
            return true; // prefix skipa o original: IsAiming não muda, AimingInterruptedByOverlap false → sem pacote (P9 corrigido)
        }

        private void Update()
        {
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                // padrão N1: mundo morreu — efeito/hooks morreram com o Player; só limpar bookkeeping (AC-10);
                // worldDead=true → TraumaTremor.Discard, NUNCA ForceResidue em AHC morto (PA-01-01)
                TearDownLocal("raid-end", worldDead: true); ResetLockout(); _trackedWorld = null; _wasActive = IsActive(); return;
            }
            if (!ReferenceEquals(gw, _trackedWorld)) { TearDownLocal("world-swap", worldDead: true); ResetLockout(); _trackedWorld = gw; }

            bool active = IsActive();
            if (_wasActive && !active)
            {
                // desligar mid-raid: tremor removido (ForceResidue — inclusive downed, CR-02) + lockout cancelado + hooks off
                TearDownLocal("toggle-off"); ResetLockout();
            }
            else if (!_wasActive && active)
            {
                // religar: estabelecer do snapshot SEM toast/voz — GetLine no humano local via RegisteredPlayers+IsOwnedHere
                // linha AdsCancel-tier + já mirando → âncora = AGORA (sem cancelamento retroativo)
            }
            _wasActive = active;
            if (!active) return;

            if (_reestablishPending)
            {
                // GetLine ainda ativa → re-apply com PISO de 0,5s entre tentativas (Time.time >= _nextReestablishAt —
                // PA-01-05: sem limite, um removedor externo por tick geraria AddEffect+HealthSyncPacket+log POR FRAME)
                // + log REESTABLISH; _reestablishCount++ e > 3 na MESMA linha → LogWarning 1x/sessão
                // (_reestablishStormWarned — diagnóstico de conflito com mod externo); linha inativa → descarta pending.
            }

            // deadline do timer (SÓ com timer armado — 1 comparação de float; zero polling de estado)
            if (_aimAnchor >= 0f && _localPlayer != null && IsAdsCancelTier(_localLine)
                && Time.time - _aimAnchor >= LineCancelSeconds(_localLine))
            {
                ExecuteCancel(_localPlayer);
            }
        }
    }
}
```

```csharp
// modded/Patches/Trauma/ArmsAimPatches.cs
using System.Reflection;
using EFT;
using EFT.Animations;
using HarmonyLib;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Lockout de re-ADS (decisão 17 — P9): prefix no funil ÚNICO de mira. Todas as rotas de
    /// re-entrada convergem aqui (input/ToggleAim :13701, scope :13705-13709, fast-slot :10670-10673,
    /// pós-overlap :13067-13070). Skip não emite pacote → invisível ao peer (P9 corrigido).</summary>
    [HarmonyPatch]
    internal static class SetAimLockoutPatch
    {
        // Alvo por assinatura (não GClassNNNN): mesmo lookup que RecoilRework/FOVFix resolvem em produção (IL — P9)
        private static readonly MethodBase Target =
            AccessTools.Method(typeof(Player.FirearmController), "SetAim", new[] { typeof(bool) });
        private static readonly FieldInfo PlayerField =
            AccessTools.Field(typeof(Player.FirearmController), "_player"); // cacheado (mesmo acesso do FOVFix — P9)

        static MethodBase TargetMethod() { return Target; }

        static bool Prefix(Player.FirearmController __instance, bool value)
        {
            try
            {
                if (!value) return true; // saída de mira NUNCA bloqueada (nosso cancel e o desmaio passam livres)
                var p = PlayerField.GetValue(__instance) as Player;
                // AP-03 auditado: FikaClientFirearmController chama base (humano local COBERTO); BotFirearmController/
                // AIFirearmController descem à base SEM override → guard obrigatório (bots excluídos — D9/funcional 5);
                // ObservedFirearmController seta IsAiming sobrescrito sem base.SetAim (espelhos nunca chegam)
                if (p == null || !p.IsYourPlayer || p.IsAI) return true;
                return !TraumaArmsConsumer.TryBlockReAds(p); // true=bloqueado → skip (postfixes RecoilRework/FOVFix ainda rodam — idempotentes, D13)
            }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] SetAimLockoutPatch: {ex}");
                return true; // nunca engolir o original por exceção nossa
            }
        }
    }

    /// <summary>Contorno da supressão VISUAL do analgésico (P2 rec. (e)): o corpo vanilla força
    /// Breath.TremorOn=false sob OnPainkillers (PWA:1182-1186) atingindo QUALQUER tremor, inclusive o nosso.
    /// Re-assert SÓ da instância gerenciada — nunca pelo flag EPhysicalCondition.Tremor (zumbi/stim têm gate
    /// intencional). Corpo = 1 write de bool; roda só em MUDANÇA de condição (fora do hot path por frame).</summary>
    [HarmonyPatch(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.PhysicalConditionUpdated))] // ref: PWA:1175 (público, instância)
    internal static class TremorVisualReassertPatch
    {
        static void Postfix(ProceduralWeaponAnimation __instance)
        {
            try
            {
                if (TraumaTremor.Owned == null || !TraumaTremor.Owned.Existing) return; // Existing = Added|Started (AHC:219-233)
                //   PA-01-03: gate por Active (só Started) deixava descoberto o gap Added→Started de até 1 tick —
                //   um PhysicalConditionUpdated nesse frame com OnPainkillers forçaria TremorOn=false sem re-assert,
                //   e o Started seguinte NÃO re-dispara o evento se o flag Tremor já era true (tremor-por-dor coexistindo
                //   — AC-2). Residued segue fora (fade não re-asserta — comportamento atual); delay=0 → Added dura <=1 tick.
                Player p = TraumaTremor.OwnedPlayer;
                if (p == null || !ReferenceEquals(__instance, p.ProceduralWeaponAnimation)) return; // PWA de bot/espelho nunca casa
                __instance.Breath.TremorOn = true; // ref: BreathEffector.cs:74 (campo público); shake = TremorOn || Fracture (OR — sem dupla intensidade)
            }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] TremorVisualReassertPatch: {ex}");
            }
        }
    }
}
```

```csharp
// modded/TRLImmersiveCombatMedicinePlugin.cs — ADIÇÕES (trechos; resto intocado)
// Campos: ConfigArmsAdsCancelZ2Seconds / ConfigArmsAdsCancelQ2Seconds / ConfigArmsAdsCancelZ2Q2Seconds /
//         ConfigArmsReAdsLockoutSeconds (ConfigEntry<float>)
// Binds §3 na seção "8. Trauma 2.0 (Braços)"; rename:
ConfigConsumerArmsEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Arms Effects", true,
    "Tremor contínuo + cancelamento de ADS escalonado (item 005). Governado pelo master Trauma 2.0; desligar mid-raid remove o tremor e cancela o lockout.");
ConfigArmsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Braços", true,
    "(INERTE desde a v1.5.0 — substituído pelo Trauma 2.0 / Arms Effects. Remoção da key no item 010.)");
// MigrateOrphanedConfigKeys: mesmo bloco do "Legs Effects (item 003)" (Plugin:265-287) replicado p/ a key
// "Arms Effects (item 005)" — DELETAR sem copiar (o false de placeholder não é escolha do usuário) + Config.Save().
// Awake: gameObject.AddComponent<TraumaArmsConsumer>(); // DEPOIS de TraumaEngine (replay vazio inofensivo — padrão 003)
// [BepInPlugin(..., "1.5.0")]
```

## 6. Fluxo de dados

```
[dano/cura/fratura/analgésico no DONO]
        ▼
[motor 002: eventos AHC → linha de braços (TraumaMatrixResolver.ResolveArms — já publica desde o 002)]
        │ StateChanged (sem one-shot p/ braços)
        ▼
[TraumaArmsConsumer.OnTransition]
        ├── bot → log "arms EXCLUDED (bot)" e NADA (funcional 5)
        ├── To=None → TraumaTremor.Remove + desarma timer/hooks
        ▼
[ApplyLine: TODA linha → TraumaTremor.Apply (AddEffect<Tremor> reflection, delay=0, work=∞, âncora no braço)]
        │ wiring vanilla: EffectStarted → EPhysicalCondition.Tremor (Player.cs:28959-28962) → PWA → Breath.TremorOn
        │ sob analgésico: gate visual (PWA:1182-1186) → POSTFIX re-asserta TremorOn (só a NOSSA instância)
        │ remoção externa: EffectResidual/Removed + ReferenceEquals(_owned) + GetLine ativa → re-apply (watchdog)
        │ peer: efeito sinca como DADO (HealthSyncPacket) mas ObservedPlayer descarta o visual (fika ObservedPlayer.cs:737-751) — tremor é first-person (AC-9)
        ▼
[linha AdsCancel-tier → hooks de evento: HandsChangedEvent (:25544) + fc.OnAimingChanged (:20037)]
        │ âncora = max(edge de mira, entrada/mudança de linha); soltar = reset; linha nova = reinicia com N novo
        ▼
[Update: Time.time - âncora ≥ N (4/3/2 cfg) → ExecuteCancel]
        │ SetAim(false) no funil vanilla → teardown completo (:12136-12168) + ToggleAimPacket(-1) → peer VÊ a arma abaixar
        ▼
[lockout 1–1,5 s por ProfileId (persiste troca de arma)]
        │ re-ADS por QUALQUER rota → prefix SetAim(true) → skip (sem pacote — peer não vê) + voz OnAgony
        │   via Speaker.Play(importance:100) — 1 voz/janela (consumida SÓ se o Play tocou; Busy≥100/Blocker de
        │   grupo engolem → log voice=skipped e a próxima tentativa re-tenta); sem voz em blackout; peers via PhrasePacket
        ▼
[lockout expira → ciclo recomeça]
```

Exemplo AC-4/AC-5 (zerar 2 braços): motor publica `Arms: ArmsTremor -> ArmsTremorAdsCancel4s reason=Damage` → tremor persiste (mesma instância — idempotente) + timer armado; jogador mira 4 s → `ads CANCEL ... n=4.00 lockout=1.50`; segura a mira (hold) → prefix bloqueia re-aim com 1 voz; 1,5 s depois re-mira livre. Aos ~3 s de mira quebra o 2º braço (vira Z2+Q2): transição re-ancora o timer com N=2 → cancela ~2 s DEPOIS da mudança. Analgésico com timer correndo: motor rebaixa p/ `ArmsTremor` (`PainkillerGained`) → timer descartado, tremor VISÍVEL sob analgésico (postfix), mira livre.

## 7. Riscos e dependências

- **Patches existentes:** `MainLoopPatch` perde só o bloco de braços (desmaio/grace intactos); `HealthPatches` perde só a voz "Arm" (desmaio/estômago intactos — fronteiras 007/006); `FreezeCommandPatch`/`FreezeAxesPatch` intocados (o guard de blackout da voz é aditivo). Nenhuma mudança no motor 002 — primeiro consumidor a entregar sem tocar o engine.
- **Compatibilidade (D13 confirmado):** SPTRecoilRework e FOVFix são POSTFIX-only no MESMO `SetAim(bool)` (cosmético de animator; idempotentes; rodam mesmo com o skip do prefix) — sem ordenação; AC de 3 ciclos valida FOV/zoom não-preso. Realism não está na load order (o estilo de patch no BreathEffector é só referência). BringBackConcussion só usa DoContusion — zero sobreposição com tremor (P2 evid.). Caso "lockout ativo + postfix RecoilRework/FOVFix" já anotado p/ a suíte D20 (009).
- **Ordem de inicialização:** consumidor criado DEPOIS do motor no Awake (replay do `SubscribeWithSnapshot` vazio e inofensivo — padrão 003); patches Harmony aplicados no processamento por classe existente do plugin.
- **Voz:** `importance:100` pode interromper fala em andamento (risco invertido do P5 — aceito); distinção audível do OnAgony por voz é asset-level (validação in-game; fallback OGG do repo é o plano B do P5, herdado). Chamada ENGOLIDA (Busy≥100/Blocker de grupo) não consome a janela do throttle (PA-01-02 — ver §1.6).
- **Headless (P9 rec. (b) — PA-01-07):** gate `FikaBackendUtils.IsHeadless` no `IsActive()` (1 linha; `Fika.Core` já é referência hard do csproj — fika FikaBackendUtils.cs:49). Sem o gate o shell do headless já não receberia transição Arms com `IsYourPlayer` (não toma dano), mas o consumidor assinaria eventos e rodaria Update à toa — o gate elimina o trabalho morto e qualquer voz acidental, conforme a recomendação do P9 (desvio anterior era omissão, agora registrado).
- **Custo quente:** prefix de SetAim roda por chamada de mira (não por frame — cadência de input); postfix do PWA roda só em mudança de `EPhysicalCondition`; deadline do timer = 1 comparação float/frame quando armado. Zero LINQ/alloc nos três caminhos.

### Aberturas explícitas para os reviewers

1. **Escopo da aposentadoria legada inclui a voz "Arm" do `HealthPatches` (:113-119):** a funcional pede a remoção da FADIGA; a voz de hit de braço é legado irmão pendurado no mesmo `ConfigArmsEnabled` (que vira INERTE). Manter a voz com key inerte = código morto; removê-la = paridade exata com o 003 (que removeu a voz de hit de perna junto). Proposta: remover. Confirmar.
2. **Faixa do slider de lockout 1.0–1.5 s (default 1.5):** segue a decisão 17 + funcional aprovada ("faixa 1–1,5 s configurável"). Faixa mais larga (ex.: 0.5–3.0 s) daria mais liberdade de tuning mas contraria a funcional validada (progressão/variação de lockout foi rejeitada lá). Proposta: manter 1.0–1.5. Confirmar.
3. **Cadência do input em modo HOLD (premissa P-005-A, inferência marcada do P9):** o handler de input de raid não aparece no dump (namespace vazio) — não está provado se segurar o botão re-dispara `SetAim(true)` por frame ou 1×. O bloqueio é correto nos dois casos (idempotente) e o throttle de 1 voz/janela cobre o spam; smoke test hold E toggle é gate do AC-6. Registrar como premissa p/ o item 011.
4. **Falha da reflection do tremor (premissa P-005-B):** nomes ofuscados (`Tremor` nested, `GClass3008`, `GInterface361`) mudam por versão do EFT. Fail-fast no 1º uso com `LogError`; tremor degrada para no-op e o cancela-ADS/lockout (sem reflection ofuscada) SEGUE funcionando. Alternativa (desligar o consumidor inteiro) pune o que ainda funciona. Confirmar a degradação parcial. **Cobertura estendida a RUNTIME (PA-01-04):** `TargetInvocationException` com resolução OK (internals do AHC mudaram/estado inesperado) subiria pelo dispatch DESPROTEGIDO do motor (`StateChanged?.Invoke` — TraumaEngine.cs:565; idem :312) e abortaria a consolidação do frame p/ TODOS os consumidores — por isso `TraumaTremor.Apply`/`Remove` têm try/catch próprio: no catch, `LogError` 1× + `_resolveOk = false` (mesma degradação parcial da sessão).
5. **Postfix do PWA não validado in-game** (spike P2 foi docs+protótipo compile-only): o teste visual "tremor visível sob analgésico" é o PRIMEIRO smoke test do checklist (lição de memória: escrita SPT exige validação no jogo).
6. **Âncora do efeito no braço:** proposta = braço COMPROMETIDO (Left se zerado/quebrado, senão Right). O wiring do flag é part-agnostic, então a escolha só afeta ícone da UI de saúde e o escape dos lookups vanilla de Head. Confirmar (alternativa: sempre LeftArm, mais simples e igualmente segura). **Drift pós-cura resolvido por RE-ÂNCORA (PA-01-06):** cura parcial/cirurgia não remove o efeito (`RestoreBodyPart` só sync — AHC:3891-3907/4573) e a idempotência sozinha deixaria a âncora num braço SAUDÁVEL; com instância viva, `PickArmAnchor != OwnedAnchor` E parte antiga não mais comprometida → `Remove("re-anchor")`+`Apply` na parte nova (o AddEffect força-remove o residued da mesma parte antes do Create; wiring part-agnostic → sem gap visual perceptível); ambos comprometidos → mantém (sem churn).

## 8. Checklist de implementação

- [ ] `TraumaTremor.cs`: cache reflection fail-fast + `Apply` idempotente (delay=0/work=∞/âncora braço/re-âncora pós-cura PA-01-06/write TremorOn) + `Remove` por instância própria (campos anulados ANTES do ForceResidue — PA-01-05) + `Discard` bookkeeping-only p/ mundo morto (PA-01-01) + try/catch de runtime no Apply/Remove (degradação P-005-B — PA-01-04) + `IsOurs`.
- [ ] `ArmsAimPatches.cs`: prefix `SetAim(bool)` (guards IsYourPlayer/!IsAI; try/catch; `TryBlockReAds`) + postfix `PhysicalConditionUpdated` (re-assert gated por ReferenceEquals).
- [ ] `TraumaArmsConsumer.cs`: registry + SubscribeWithSnapshot + gate `IsHeadless` no `IsActive()` (PA-01-07) + exclusão de bots (log) + mapa linha→efeito + watchdog/re-establish (piso 0,5 s + warn de conflito externo — PA-01-05) + hooks de evento de ADS (subscribe/unsubscribe simétrico em HandsChanged) + timer (âncora max(edge, linha); reinício por mudança de linha; descarte fora de tier) + `ExecuteCancel` + lockout/voz throttled com guard de blackout (janela só consumida se o Play tocou — PA-01-02) + edges do toggle + sweeps world-null/world-swap com `Discard` (PA-01-01).
- [ ] Plugin: configs §3 (seção 8; rename `Arms Effects` ON; tooltip INERTE no `Sistema de Braços`); delete da key órfã `"Arms Effects (item 005)"` no `MigrateOrphanedConfigKeys`; `AddComponent<TraumaArmsConsumer>()`; bump `1.5.0`.
- [ ] Aposentadoria legada: `MovementPatches` (bloco de fadiga :119-139), `HealthPatches` (voz "Arm" :113-119 — se abertura 1 confirmada), `TraumaState` (`AimingFatigueTimers` + ResetAll).
- [ ] `PROPRIEDADES.md` + regenerar grafo do mod (`/update-mod-graph`) no commit da entrega.
- [ ] Smoke test 1 (gate — abertura 5): 2 braços zerados + analgésico → estado `ArmsTremor` e tremor VISÍVEL (postfix funcionando); tremor-por-dor vanilla segue suprimido (AC-2).
- [ ] Smoke test (mapeia ACs por grep): AC-1 (Z1 liga tremor; analgésico REMOVE — estado None; expirar re-aplica; curar remove ≤1 s); AC-3 (ciclos ferir→analgésico→expirar→curar com 1 instância — logs `tremor ON/OFF` pareados; remoção externa → `REESTABLISH`); AC-4 (Z2: cancel em 4 s ±0,25; soltar/re-mirar reseta; Z2+Q2 em 2 s ±0,25); AC-5 (mudança de linha mid-ADS reinicia; analgésico mid-timer descarta e libera mira); AC-6 (lockout bloqueia TODAS as rotas — input, scope, fast-slot; 1 voz/janela; voz ENGOLIDA por Busy/Blocker não queima a janela — log `voice=skipped(busy|blocked)` e a próxima tentativa toca (PA-01-02, cenário squad); hold E toggle sem spam nem furo; pós-lockout ciclo recomeça); AC-7 (legado inerte com `Sistema de Braços` ON no cfg antigo; 3 ciclos com RecoilRework+FOVFix sem FOV/zoom preso); AC-8 (bot ferido → log `EXCLUDED`, sem tremor/cancel); AC-9 (coop: peer não vê tremor; VÊ a arma abaixar no cancel; tentativas bloqueadas invisíveis; voz audível); AC-10 (reset entre raids; spawn ferido = establishing sem toast/voz).
- [ ] Smoke test extra (corners): cancelamento no meio de rajada (hip-fire funcional); troca de arma durante lockout (persiste); desmaio durante ADS/lockout (timer reseta, lockout expira, SEM voz, tremor re-estabelecido no wake); desligar/religar toggle mid-raid (tremor some/volta sem toast); cura parcial do braço-âncora com linha ainda ativa (re-âncora — ícone da UI de saúde migra p/ o braço comprometido, PA-01-06); scope PiP (suíte D20); tentativa no lockout com bússola na mão (colateral aceito — observar).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop idempotentes — AP-01 | ✅ | Consumidor herda o lifecycle do motor (transições/establishing); §5 `Update`: world-null E world-swap limpam tremor/timer/lockout/hooks via `Discard` bookkeeping-only (NUNCA `ForceResidue` em AHC morto — PA-01-01); o efeito vive no AHC que morre com o Player; lockout guarda ProfileId (stale entre raids inócuo e zerado no sweep). Handlers de evento com -= simétrico (fc/hands/watchdog guardados em campo). |
| 2 | Filtro MainPlayer/Fika em todo ponto que reage a player — AP-02 | ✅ | Efeitos só via transições do motor (só donos — D16) + guards `IsAI`/`IsYourPlayer` no consumidor E no prefix; bots explicitamente excluídos com log (funcional 5); headless no-op por construção (sem humano local) + gate `FikaBackendUtils.IsHeadless` no `IsActive()` (P9 rec. (b) — PA-01-07). |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados — AP-03 | ✅ | `SetAim(bool)` resolvido por `AccessTools.Method` com assinatura (mesmo lookup dos 2 mods em produção — IL P9); overrides auditados: FikaClient chama base (coberto), Bot/AI descem à base (guard exclui), Observed não chama base (imune). PWA postfix: método público não-virtual chamado direto — sem override possível. Reflection do Tremor: nested por NOME estável ("Tremor") + fail-fast, nunca GClassNNNN hardcoded (o cast usa `ActiveHealthController.GClass3008` público — risco de renumeração registrado na abertura 4 com degradação). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Tremor via `AddEffect` (state machine + eventos + sync nativos); remoção via `ForceResidue` da instância (mesmo funil de expiração — AHC:550-570); cancel via `SetAim(false)` (funil único — teardown completo + pacote Fika); voz via `Speaker.Play` (pipeline de frases + PhrasePacket). ÚNICO write direto: `Breath.TremorOn` (campo público consumido por frame) — bypass DOCUMENTADO do gate visual, exigido pela matriz (células com-analgésico→Tremor); side-effect é só o shake procedural (OR com Fracture). |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte/MIA | ✅ | §5 Update world-null/world-swap: bookkeeping zerado sem tocar objetos destruídos (membros gerenciados); motor reseta separadamente (AC8 do 002); nada estático sobrevive além de `_timerClampWarned`/cache de reflection (intencionais, por sessão). |
| 6 | ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: defaults/faixas/tooltips; estado neutro (toggle off) = zero efeito; min() dos timers com warn 1×; `Sistema de Braços` documentado INERTE (migração 010); rename-at-delivery com delete da key órfã. |
| 7 | Reentry-guard em método patcheado re-invocado — AP-07 | ✅ | `ExecuteCancel` chama `SetAim(false)` → prefix early-return em `!value` (nunca bloqueia nem recursa); `TryBlockReAds` não chama SetAim; postfix do PWA só escreve bool (não re-dispara `PhysicalConditionUpdated`); watchdog NÃO re-aplica dentro do dispatch do evento de saúde (flag + próximo Update) — sem reentrância no state machine do AHC. O handler `OnTransition` também não deixa exceção escapar p/ o dispatch DESPROTEGIDO do motor (`StateChanged?.Invoke` — TraumaEngine.cs:565): `TraumaTremor.Apply/Remove` têm try/catch próprio com degradação P-005-B (PA-01-04). |
| 8 | Flags/caches validados contra o contexto atual — AP-08 | ✅ | `_localLine` re-derivada por transição; re-establish re-valida `GetLine` NA execução; `_hookedFc` comparado/re-subscrito a cada `HandsChangedEvent` (lição stances CR-01-02: nada age sobre controller trocado); lockout valida ProfileId; `_aimAnchor` reancorado em toda mudança de linha/controller. |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec` (baseada no motor 002 implementado + consumidor 003 entregue + trauma-primitives P2/P9/P5 corrigidos; âncoras AddEffect/PWA relidas no decompile do spike 001 neste worktree; zero mudança no motor) |
| 2026-07-19 | Review técnica rodada 1 aplicada — 8 achados (0 bloqueadores) |
