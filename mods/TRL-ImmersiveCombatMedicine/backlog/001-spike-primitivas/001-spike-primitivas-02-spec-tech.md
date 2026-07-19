# 001 — Spike: primitivas vanilla de trauma — Spec Técnica (plano de investigação)

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-18
**Spec funcional:** [001-spike-primitivas-01-spec.md](./001-spike-primitivas-01-spec.md)

## Natureza do item

Spike de pesquisa — esta spec técnica é o **plano de investigação**: para cada pergunta P1–P10, onde procurar, o que provar e o formato da resposta. Não há stubs de código de produção (AC: diff docs-only). Âncoras abaixo **pré-verificadas na rodada A de review** (ilspycmd + fontes do repo) — pesquisadores partem delas, não de nomes inferidos.

## Fontes canônicas (hierarquia)

1. **Assembly real:** `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` via `ilspycmd -t <FQN>` — SEMPRE que o dump não tiver o tipo (102 namespaces vazios; `EFT.HealthSystem` é um deles).
2. **Dump navegável:** `references/eft-decompiled/Assembly-CSharp/` (+ grafos `references/graphs/`).
3. **Fika 2.3.4:** `references/fika-plugin/` (fonte) **+ `references/fika-headless/`** (headless — corner obrigatório).
4. **Fontes de mods NO REPO** (preferir a decompile): `mods/SAIN/original` (4.4.3 — **igual ao instalado**, verificado), `mods/ORBIT` (repo=1.2.1, **instalado=1.1.0 — MISMATCH**: todo achado do ORBIT re-verificado no `D:/SPT/BepInEx/plugins/ORBIT/ORBIT.dll`, que é a verdade de runtime; registrar o mismatch como risco no doc), `mods/Skills-Extended` (+ grafo), `mods/CustomClasses/modded` (nosso).
5. **DLLs instalados** (verificação final): `D:/SPT/BepInEx/plugins/SAIN/SAIN.dll`, `.../ORBIT/ORBIT.dll`, `.../DrakiaXYZ-BigBrain.dll` (raiz), `.../SkillsExtended/SkillsExtended.dll`, `SPTRecoilRework`/`FOVFix` DLLs.
6. **Referência de ABORDAGEM (era 3.11 — re-verificar tudo no 0.16.x):** `mods/SPT-Realism-Mod-Client` (+ grafo) e `mods/RealismMod` — implementam mancar/dor/painkiller/tremor gerenciados.
7. **Scratchpad:** `<scratchpad>/spike001/` (decompilações e protótipos — fora do repo).

## Plano por pergunta

### P1 — Mancar + pipeline de velocidade (consome: 003)
- **Âncoras verificadas:** `EFT.EPhysicalCondition` (flags: `OnPainkillers=1`, `LeftLegDamaged=2`, `RightLegDamaged=4`, `ProneDisabled=8`, `LeftArmDamaged=0x10`, `RightArmDamaged=0x20`, `Tremor=0x40`, `SprintDisabled=0x400`…) — **variação POR LADO nativa**; setter `MovementContext.SetPhysicalCondition` (MovementContext.cs:1578); wiring saúde→condições em Player.cs:28920-28957; **sync a observers via `ActorDataStruct.conditions`** (MovementContext.cs:4286).
- **Investigar:** consumo de cada flag (animação/velocidade — grep `PhysicalCondition|LegDamaged`); tabela de penalidades nativas com VALORES (containers: `BackendConfigSettingsClass`, `EFTHardSettings`, `GClass3019` — settings de efeitos via `GClass3008.GClass3019_0`); mapa de escritores de velocidade: vanilla + CustomClasses (`MaxSpeedPatch`/`SprintingSpeedPatch` — postfix em GETTERS, fonte `mods/CustomClasses/modded/Client/Patches/ClassMovementPatches.cs:79/100`; comentários explicam por que getter-postfix não acumula) + SkillsExtended (`MovementContextSetSpeedLimitPatch` — confirmado no DLL); **bots**: mancar/caps afetam locomoção SAIN-driven? o que o peer vê? — se não afetarem, recomendar mecanismo equivalente p/ bots.
- **Formato:** tabela de penalidades + veredito lado/nível (via EPhysicalCondition) + recomendação de composição N1/N2 humano E bot (ou fallback caps — D10).

### P2 — Tremor (consome: 005)
- **Âncoras verificadas:** nested `Tremor` no ActiveHealthController real (~3117, GClass3008+GInterface361); **NÃO existe DoTremor** — aplicação vanilla: `AddEffect<Tremor>(EBodyPart.Head, delay, work, residue, strength, initCallback)` (~3514; caminho HandsTremor ~2794) e `method_1<Tremor>` via Pain (~2111) — ancorado em **Head**; remoção: `method_15<T>` (ForceRemove, ~3615) / `method_16<T>` (ForceResidue); rede: `NetworkHealthControllerAbstractClass`; sync barato: `EPhysicalCondition.Tremor` (Player.cs:28944) via ActorDataStruct.
- **Investigar:** lifecycle próprio (D11) com AddEffect (work/residue longos + renovação vs remoção explícita); interação com Pain/PainKiller (nosso tremor NÃO pode sumir com analgésico); visibilidade em bots/peers; headless-safe.
- **Formato:** sequência de chamadas recomendada + nota de sync + headless.

### P3 — Analgésico (consome: 002)
- **Âncoras verificadas:** interfaces no 0.16.x: `Pain=GInterface357`, `PainKiller=GInterface358`, `MedEffect=GInterface376`, `Stimulator=GInterface377`; `DoPainKiller()` existe (~3590); eventos `EffectStartedEvent`/`EffectAddedEvent`/`EffectRemovedEvent` (~3399-3407); cross-check barato: `EPhysicalCondition.OnPainkillers`.
- **Investigar:** predicado `IsUnderPainkiller` (item comum/morfina/stims — buffs `Stimulator` com painkiller embutido: onde o buff seta o efeito?); hook de EXPIRAÇÃO event-driven (decisão 14) via EffectRemovedEvent; funciona no AHC de BOTS (host/headless)?
- **Formato:** predicado + hook com assinaturas.

### P4 — Agachar/derrubar involuntário (consome: 003/004/006)
- **Âncoras:** `MovementContext.SetPoseLevel`, `IsInPronePose`, `Player.ToggleProne` (Player.cs:26054); guards em 3 eixos (rodada B): (a) vault = `EPlayerState.ClimbOver/ClimbUp/Vaulting*` via `MovementContext.CurrentState.Name` (MovementContext.cs:2141); (b) BTR = `Player.BtrState` (`EPlayerBtrState`, Player.cs:25413) + `OnBtrStateChanged` (25540); (c) escada/corda = PESQUISA ABERTA — nenhum tipo `*Ladder*` no Assembly-CSharp (verificado por listagem completa de 11.474 classes): investigar se escada interativa/corda existe no 0.16.x ou concluir N/A com evidência; **dano de queda — cadeia real:** `MovementContext.OnGrounded` += `Player.method_5` (Player.cs:27296) → `method_5(fallHeight, jumpHeight)` (25733) → `LandingAdjustments` (25739; gate `Inertia.FallThreshold`); dano de andar/sprint com perna zerada: `Player.InflictSelfDamage` (~31140, 2f/tick com `PhysicalConditionIs(LegDamaged)` — o dano da janela de 3 s do D18).
- **Investigar:** agachar one-shot sem lock (decisão 5); prone forçado limpo; guards; provar que pose forçada NÃO dispara OnGrounded com fallHeight relevante (D18); **timer de extração roda em prone forçado?** (evidência no código de extração ou teste pontual).
- **Formato:** chamadas exatas + guards + provas de não-dano/extração.

### P5 — Levantar controlado + vozes (consome: 004)
- **Âncoras verificadas:** clamps de pose (o `CantStandUpPatch` atual como baseline); vozes: `Player.Say(EPhraseTrigger phrase, bool demand, float delay, ETagStatus mask, int probability, bool aggressive)` (Player.cs:28799) — **`demand=true` fura throttle** (candidato); throttle real: `PhraseSpeakerClass` (`Player.Speaker`, Player.cs:24347) e, em bots, `BotTalk` (prioridades `OnAgony=198f`, `OnBeingHurt=88f` — BotTalk.cs:115/126); Fika: `PhrasePacket` (fika-plugin .../SubPackets/PhrasePacket.cs).
- **Investigar:** ciclo 15s/3s com clamps; "levantar lento" (SetPoseLevel gradual vs transição animada); tentativa frustrada (pose bump 0→0.2→0 viável?); 2 sons distintos (forte=OnAgony?, leve=OnBeingHurt?) confiáveis em sequência curta (<20 s, demand); audível em peers (PhrasePacket); headless-safe p/ bots. Fallback: pipeline OGG do repo (lição de memória) SE vanilla não diferenciar.
- **Formato:** máquina de estados do ciclo + triggers recomendados + headless.

### P6 — SAIN/ORBIT/BigBrain (consome: 004/009)
- **Âncoras verificadas:** SAIN (fonte no repo): `SAIN.Patches.Movement.PlayerSetPosePatch`, `CrawlPatch`, `BotMoverManualUpdatePatch`/`BotMoverManualFixedUpdatePatch`, `SAIN.SAINComponent.Classes.Mover.SAINMoverClass`/`PoseClass`/`ProneClass`; BigBrain 1.4.0: `DrakiaXYZ.BigBrain.Brains.BrainManager` (+`CustomLayer`/`CustomLogic`, `LayerInfo`/`ExcludeLayerInfo`); ORBIT: `Orbit.Brain.OrbitBrainLayer` (**exemplo real de camada custom na load order** — modelo p/ "TraumaDowned"), `Orbit.Systems.MovementSystem`, `Orbit.Sain.*`; baseline do mod: `AggroHelper.PauseBot/UnpauseBot` + limitação atual "levanta e nunca mais cai" (diagnosticar causa-raiz como evidência).
- **Investigar:** (a) o que SAIN escreve em pose/mover por frame; (b) camada BigBrain "TraumaDowned" de alta prioridade: **sustenta o bot no chão por X s** sem thrash? bot atira/gira caído?; (c) contrato derrubar→devolver→re-decidir (decisão 16) com sequência de chamadas; (d) **agachar one-shot em bots** (dip de pose com devolução imediata — decisões 11/16, consome 006); (e) **UNTAR**: identificar o mod PROVEDOR dos bots UNTAR na instalação (server mods em `D:/SPT/SPT/user/mods` + BotTypes do SAIN), extrair brain name/WildSpawnType e provar cobertura pela camada/SAIN — só então concluir D15.
- **Formato:** recomendação ÚNICA de mecanismo (camada vs clamp) com prova + contrato de interferência.

### P7 — Hook de dano p/ desmaio percentual (consome: 007)
- **Âncoras verificadas:** ponto atual: postfix em `Player.ApplyDamageInfo` — **VIRTUAL** (Player.cs:30463) → **check AP-03 obrigatório** (overrides em ObservedPlayer/etc. via grafo — onde o dano de peer passa no DONO?); alternativas: `ActiveHealthController.ApplyDamage(EBodyPart, float, DamageInfoStruct)` (~3721) e **`ApplyDamageEvent`** (`event Action<EBodyPart, float, DamageInfoStruct>`, ~3411 — escuta SEM Harmony; verificar se expõe vida ATUAL pré-aplicação na ordem certa).
- **Investigar:** no hook escolhido: `(danoEfetivo, bodyPart, hpAtualPréTiro)` corretos p/ dano local E de peer; **granularidade por hit/pellet** (decisão 15: shotgun dispara N chamadas, não agregado?) nas duas origens.
- **Formato:** hook + campos + granularidade com prova de ordem.

### P8 — Idioma do jogo (consome: 002)
- **Âncoras verificadas:** `LocaleManagerClass`/`LocaleClass` (ilspycmd — fora do dump); timing: `SPT.Custom.Patches.LocaleManagerRaceConditionFixPatch` (spt-custom.dll) prova que locale tem race de init — ler pós-menu.
- **Investigar:** predicado `IsGamePortuguese()` estável + momento seguro.
- **Formato:** API + timing.

### P9 — Cancela-ADS + lockout (consome: 005) *(adicionado na rodada A — era bloqueador)*
- **Âncoras:** código atual (polling `ProceduralWeaponAnimation.IsAiming` + `IFirearmHandsController.SetAim(false)` — MovementPatches.cs:123-137) como baseline a substituir; D13: `SPTRecoilRework` e `Fontaine-FOVFix` patcheiam o set de aim vanilla (localizar o FQN real — `Player.SetPlayerAiming`? via ilspycmd + decompile dos 2 DLLs).
- **Investigar:** (a) detecção de mira + tempo sustentado; (b) caminho VANILLA de cancelar ADS (evidência arquivo:linha); (c) **como BLOQUEAR re-entrada em ADS por 1–1,5 s** (decisão 17) + voz de dor na tentativa (reusa P5); (d) mapa de escritores (RecoilRework/FOVFix — ordem de patch); (e) exclusão de bots (D9) e o que o peer vê.
- **Formato:** mecanismo completo cancelar+lockout com compat mapeada.

### P10 — Observação de estado de saúde (consome: 002) *(adicionado na rodada A — era forte)*
- **Âncoras:** `EffectStartedEvent`/`EffectAddedEvent`/`EffectRemovedEvent` (P3); query de Fracture ativo por membro (`FindActiveEffect<T>`/`HasEffect` — baseline do mod em BandAidNetworkHandler); restauração de parte zerada por cirurgia (`ApplySurgeryFromNetwork` do mod + caminho vanilla Surv12/CMS); remoção REMOTA via cura coop (o próprio `RemoveEffectNative` do mod — D17).
- **Investigar:** (a) query Quebrar/Zerar por `EBodyPart` (humano e bot); (b) evento de efeito adicionado/removido local + ponto equivalente p/ cura remota (rede do mod/Fika); (c) detecção de restauração de parte zerada; (d) recomendação **evento vs polling ≤4 Hz** por transição (D19 × decisão 14 — o que dá para ter por evento?).
- **Formato:** tabela transição→mecanismo (evento/polling) com assinaturas.

## Execução

Workflow dinâmico: 10 pesquisadores paralelos (1 por P), retorno estruturado com evidência `arquivo:linha`; verificação adversarial POR ACHADO (céticos re-derivam a evidência) = rodada 1. Consolidação em `docs/trauma-primitives.md` (1 seção por P: resposta, evidências, recomendação, limitações, **comportamento no headless**, item consumidor, risco residual; provas por protótipo registram o trecho mínimo compilado + resultado observado — auditável sem o protótipo). Rodada 2 adversarial sobre o DOC final (equivale aos 2 code-reviews).

## Critérios de saída

- Doc responde P1–P10 com evidência verificada (2 rodadas adversariais).
- Cada primitiva declara comportamento no **headless** (seguro/no-op/exige guard — fika-headless quando aplicável).
- Cada recomendação nomeia o item consumidor e o risco residual; correções de premissa da matriz (ex.: redação do D12; assertiva do D13 sobre SetPlayerAiming — hipótese até o P9 provar) listadas para retrofit.
- Nenhuma mudança fora de `docs/` e `backlog/`.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Spec técnica (plano de investigação) criada via `/create-technical-spec` |
| 2026-07-18 | Review rodada B (revisor fresco): rodada A confirmada (round_a_ok); guards do P4 desdobrados em 3 eixos (vault=EPlayerState; BTR=Player.BtrState; escada/corda=pesquisa aberta — sem tipo *Ladder* no assembly); D13 anotado como hipótese p/ retrofit |
| 2026-07-18 | Review rodada A (2 revisores adversariais): +P9 (cancela-ADS/lockout — bloqueador) e +P10 (observação de estado — forte); âncoras corrigidas com verificação no assembly (EPhysicalCondition como primitiva real do mancar POR LADO com sync via ActorDataStruct; AddEffect<Tremor> em vez de DoTremor; cadeia real de fall damage; interfaces Pain=357/PainKiller=358/Stimulator=377; BrainManager é do BigBrain, OrbitBrainLayer como exemplo; PhraseSpeakerClass/BotTalk como throttle real; LocaleManagerClass; fontes SAIN/ORBIT/SkillsExtended/Realism NO REPO + mismatch ORBIT 1.1.0×1.2.1; fika-headless nas fontes) |
