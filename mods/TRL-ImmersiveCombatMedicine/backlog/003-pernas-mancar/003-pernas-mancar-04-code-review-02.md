# 003 — Pernas (mancar N1/N2 + agachar involuntário) · Code Review 02

> **Data:** 2026-07-19<br>
> **Status:** ✅ Aprovado (3 achados resolvidos em v1.4.1)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [003-pernas-mancar-02-spec-tech.md](./003-pernas-mancar-02-spec-tech.md)<br>

---

Rodada 2 sobre a v1.3.1 (commit `04855ff3`), por revisor adversarial de contexto limpo. Parte A: verificação de que os 5 fixes da rodada 1 estão corretos (não só presentes). Parte B: caça a regressões introduzidas pelos fixes + bugs novos, com foco em ordem de eventos, fake-null da Unity, contabilidade do cooldown one-shot, toggles mid-raid e coop/Fika.

Arquivos revisados: `TraumaLegsConsumer.cs`, `TraumaPose.cs`, `SpeedLimitPatches.cs`, `TRLImmersiveCombatMedicinePlugin.cs` (config 5/6/7 + `MigrateOrphanedConfigKeys` + Awake), com `TraumaEngine.cs`/`TraumaEngineState.cs` como referência de contrato. Evidências cruzadas no decompilado (`MovementContext.cs`) e no fonte do Fika (`FikaPlayer.cs`, `ClientHealthController.cs`, `ObservedMovementContext.cs`, `ObservedPlayer.cs`, `NoInertiaMovementContext.cs`, `ClientMovementContext.cs`).

## Verificação dos fixes da rodada 1

### CR-01-01 (rename-at-delivery do toggle) — ✅ fix CORRETO
`Legs Effects` nasce ON na seção "6. Trauma 2.0 (Consumidores)" (`TRLImmersiveCombatMedicinePlugin.cs:109`); `MigrateOrphanedConfigKeys` deleta a órfã `Legs Effects (item 003)` SEM copiar valor + `Config.Save` (`:270-287`). Sem risco de re-rodar clobberando (lição CR-03-01): o delete só acontece se a órfã existir em `OrphanedEntries`; após o primeiro Save ela some do .cfg e o bloco vira no-op — e o bloco **nunca escreve** em `ConfigConsumerLegsEffects.Value`, então escolha posterior do usuário via F12 é intocável por construção. `orphans.Remove` acontece fora do `foreach` (sem mutação durante iteração), e o segundo loop só inicia após o primeiro `Remove` completar.

### CR-01-02 (higiene do `_applied`) — ✅ fix CORRETO
- `RemoveCapGuarded` pula morto/destruído sem log (`TraumaLegsConsumer.cs:145-146`) — só bookkeeping. *(Mas a condição `!IsAlive` colide com o estado DOWNED do Fika — ver CR-02-01.)*
- Poda de `GetLine == None` (`:213-223`) coleta em `_sweepScratch` durante a iteração e só muta `_applied` depois — sem `InvalidOperationException`. Mesmo padrão no branch de toggle-off (`:180-184`).
- World-swap por `_trackedWorld`/`ReferenceEquals` (`:165-173`): caps do mundo anterior morrem com os `MovementContext`s dos players destruídos — limpar só bookkeeping é o correto, sem vazamento. A adoção do mundo novo roda **antes** do gate `active`, então acontece já no primeiro frame em que o singleton existe (telas de load/countdown), muitos frames antes de `OnGameStarted` armar o motor — a janela teórica "establishing publicado no mesmo frame da criação do mundo, antes do Update do consumidor" não tem cenário realista.
- Morte de player: o motor untracka via `OnPlayerDeadOrUnspawn` (`TraumaEngine.cs:368,379-384`) sem publicar transição → `GetLine` vira `None` → a poda remove a entrada e `RemoveCapGuarded` pula o cadáver sem log. Fechado como desenhado.

### CR-01-03 (BotCrouchDip NOOP pose baixa) — ✅ fix CORRETO
`TraumaPose.cs:185-192`: pose já baixa → `TryGetOneShotDeadline` + `ReportOneShotCanceled` + return, sem restore agendado. Matching por deadline confere: `OneShotPublished` é invocado sincronamente por `TryPublishOneShot` logo após stampar `_cooldownUntil` (`TraumaEngine.cs:595-597`), então o deadline lido no refund É o do publish. Double-refund impossível: `ReportOneShotCanceled` remove a entrada no primeiro refund (guard `Mathf.Approximately` — `TraumaEngine.cs:132-133`); um segundo cancel não encontra a key e no-opa. Também auditado o cruzamento com a fila de adiados: entrada deferida órfã que NOOPa depois de um refund direto encontra stamp divergente (ou ausente) e não devolve nada.

### CR-01-04 (refund no null MovementContext) — ✅ fix CORRETO
`TraumaPose.cs:79-86`: refund antes do return, inclusive para player DESTRUÍDO (fake-null: `p == null` true, `p is null` false → refund ainda roda; `ProfileId` é membro gerenciado, seguro).

### CR-01-05 (ResolveLadderType só "ladders") — ✅ fix CORRETO
`TraumaPose.cs:248`: match único por `"ladders"` no GUID; string de tipo assembly-qualified inalterada.

## Verificações adicionais (Parte B — limpas)

- **Coop/Fika (dono-only):** `ObservedMovementContext` sobrescreve `CanSprint` (`fika-plugin/.../ObservedMovementContext.cs:34`) e `ObservedPlayer.UpdateSpeedLimitByHealth` é no-op (`ObservedPlayer.cs:462-465`) — os dois postfixes de `SpeedLimitPatches.cs` nunca rodam em espelho. O player local do Fika usa `NoInertiaMovementContext : ClientMovementContext : MovementContext` **sem** override de `CanSprint`/`UpdateSpeedLimitByHealth` — o gate pega o local normalmente. Consumidor só recebe donos (motor D16); religar re-checa `IsOwnedHere` explicitamente (`TraumaLegsConsumer.cs:198`).
- **Sem compounding de cap:** `MovementContext.MaxSpeed` é curva de Strength (`MovementContext.cs:910`), independente de `SpeedLimits` — re-derivar `cap = pct × MaxSpeed` em N1→N2 não compõe com o cap anterior. `Remove+Add` só marcam dirty (`:1672-1679`, `:1790-1796`, `method_5 :1826-1829`) com recompute único (`:2553`).
- **Min(N1,N2):** clamp efetivo + warn 1×/sessão presentes (`TraumaLegsConsumer.cs:66-84`).
- **Toggle mid-raid:** OFF desfaz caps + `CancelAll` + `FlushBotRestores` (`:176-188`); ON re-estabelece do snapshot sem one-shot/toast (`:189-206`); saídas `EngineDisabled` ignoradas pelo `IsActive()` do handler são cobertas pelo branch OFF (o `IsActive` do consumidor é subconjunto do do motor). Estabelecimento duplicado motor+consumidor no religar é idempotente (Remove+Add, `_applied[p] = line`).
- **Cooldown one-shot:** todos os caminhos que não executam refundam (NOOP humano, NOOP bot, null-context humano, cancel de adiado por estado/pose, `CancelAll`), com matching por deadline — exceto os dois early-returns de bot do CR-02-02.
- **Downed do Fika sem transição espúria:** downed não dispara `OnDead` (`ClientHealthController.TryProcessDownedState` — restaura head/chest NoEvents e `ToggleDowned(true)`, `:164-172`); record do motor persiste; adiado pendente NOOPa via `IsInPronePose = true` do downed com refund correto.

## Achados novos

### CR-02-01 · B — Bug · 🟠 Forte
**Local:** `TraumaLegsConsumer.cs:146` (`RemoveCapGuarded`, guard `!IsAlive`) × `TraumaLegsConsumer.cs:176-188` (branch de toggle-off).
**Problema:** o guard de morto do CR-01-02 trata `HealthController.IsAlive == false` como "cadáver — MovementContext morre com o Player". No Fika 2.3.4 isso é falso: jogador DOWNED tem `IsAlive == false` **reversível** — o revive seta `ActiveHealthController.IsAlive = true` de volta (`fika-plugin/.../FikaPlayer.cs:557`) no MESMO Player/MovementContext, sem corpse e sem `OnDead`. Cenário: jogador local com cap N2 aplicado leva down (dano pesado de perna correlaciona com down); alguém desliga `Legs Effects` (ou o master) mid-raid enquanto ele está caído; o branch OFF limpa `_applied` e chama `RemoveCapGuarded`, que **pula** o jogador por `!IsAlive` — o cap `(ESpeedLimit)1000` fica no dict `SpeedLimits`; aliado revive; o jogador anda a 55% do baseline pelo resto da raid **com a feature desligada**, sem bookkeeping que permita desfazer (religar não re-remove: se as pernas foram curadas o `GetLine` é `None` e o estabelecimento pula o player; a transição de saída publicada durante o OFF foi ignorada pelo `IsActive()` em `OnTransition:89`).
**Por que importa:** quebra o contrato anunciado na própria descrição da config ("desligar mid-raid desfaz caps") com consequência permanente e invisível — exatamente o tipo de gap de coop-sync que o servidor (Fika Coop PVE com revive/defib em uso) mascara em teste solo. Janela estreita, mas o estado downed dura minutos e não tem auto-correção.
**Sugestão:** em `RemoveCapGuarded`, **sempre** executar `RemoveStateSpeedLimit(TraumaCause)` quando `p != null && p.MovementContext != null` (remover de dict é inócuo em cadáver); condicionar só o `UpdateSpeedLimitByHealth()` e o log "cap OFF" a `IsAlive` (o ruído de recompute em cadáver — motivação original do guard — continua evitado).

### CR-02-02 · B — Bug latente · 🟢 Menor
**Local:** `TraumaPose.cs:181-182` (`BotCrouchDip`, early-returns de `MovementContext == null` e `BotOwner == null`).
**Problema:** os dois early-returns de bot saem SEM refundar o cooldown do publish — inconsistente com o padrão estabelecido pelo CR-01-04 no caminho humano (`:79-86`) e pelo próprio NOOP de bot (`:185-192`). Cenário: one-shot publicado para bot dono cujo `AIData.BotOwner` ainda não vinculou (janela de spawn) → dip não acontece e o anti-thrash segura o próximo agachar legítimo por 3-5s.
**Por que importa:** one-shot consumido sem efeito; mesma classe do CR-01-04 (que foi 🟢). Consequência limitada a bots e a janelas curtas.
**Sugestão:** replicar o refund (`TryGetOneShotDeadline` + `ReportOneShotCanceled`) antes dos dois returns.

### CR-02-03 · D — Arquitetura · 🟢 Menor
**Local:** `TraumaPose.cs:113-125` (`Defer`, hit de dedup por `(player, kind)`).
**Problema:** no hit de dedup a entrada atualiza `PublishDeadline` e `RequiredLine` mas **não** `Region`. Hoje inalcançável (só pernas publica `InvoluntaryCrouch`), mas o próprio arquivo declara a primitiva compartilhada com o item 006 (estômago), que publicará o MESMO kind: um adiado de pernas + re-publish do estômago gravaria `RequiredLine = GetLine(p, Stomach)` numa entrada com `Region = Legs` — a re-validação do `PumpDeferred` (`GetLine(p, e.Region) != e.RequiredLine`) cancelaria/refundaria o one-shot errado.
**Por que importa:** armadilha pronta para a entrega do 006; custo de fix hoje é uma linha.
**Sugestão:** atualizar `e.Region = region` no hit de dedup (ou trocar a chave de dedup para `(player, kind, region)` na entrega do 006 — decisão registrada aqui para a spec do 006).

## Veredito

v1.3.1 sólida nos 5 fixes da rodada 1 e nos focos adversariais de contrato (cooldown, fake-null, dono-only, min N1/N2). Pendências: **CR-02-01 (🟠)** exige correção antes do próximo release; CR-02-02/03 (🟢) podem ir juntas ou ficar registradas para o 006.

## Resolução

Os 3 achados aplicados conforme sugerido, em **v1.4.1** (build 0 erros, implantada em D:/SPT):
- ✅ CR-02-01 — `RemoveCapGuarded` sempre remove o cap do dict quando `MovementContext` existe; só recompute + log gateados em `IsAlive` (downed do Fika coberto).
- ✅ CR-02-02 — refund do cooldown nos 2 early-returns do `BotCrouchDip`.
- ✅ CR-02-03 — `e.Region = region` no hit de dedup do `Defer` (armadilha do 006 desarmada; a spec do 006 ainda pode optar pela chave `(player, kind, region)`).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Rodada 2: 5 fixes da rodada 1 verificados ✅ corretos; 3 achados novos (1× 🟠 downed/revive Fika, 2× 🟢). |
