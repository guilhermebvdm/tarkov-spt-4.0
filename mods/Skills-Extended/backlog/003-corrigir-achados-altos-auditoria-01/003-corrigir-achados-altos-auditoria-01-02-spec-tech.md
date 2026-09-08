# 003 — Corrigir achados altos da auditoria 01 · Spec Técnica

**Mod:** Skills-Extended
**Spec funcional:** [003-corrigir-achados-altos-auditoria-01-01-spec.md](003-corrigir-achados-altos-auditoria-01-01-spec.md)
**Criado:** 2026-09-07

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** [mods/Skills-Extended/memory/sessions.md](../../memory/sessions.md) — Sessão 1 (snapshot 2026-09-07). Pendências que afetam este item: `P-1.1` (este próprio item, sem bloqueio adicional). Padrão sistêmico documentado na Sessão 1 ("`GameUtils.GetSkillManager()`/`GetPlayer()` sempre resolvem o MainPlayer") se repete em 4 dos 9 achados deste item (`AUD-01-07`, `AUD-01-08` parcial, `AUD-01-10`, e indiretamente `AUD-01-11`/`AUD-01-12` que usam o mesmo utilitário mas de forma já correta, pois são inerentemente ações do jogador local).

**Docs técnicos lidos (gatilho disparado):** [spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md) (obrigatório). Nenhum outro doc de `docs/technical/` foi disparado — este item não introduz `ConfigEntry`, não declara pacote FIKA novo, e não mexe em itens/inventário/hideout além do que já está coberto por `csharp-mod-best-practices`.

---

## 0. Nota metodológica — 2 dos 9 achados exigiram investigação mais profunda que o previsto

Durante a pesquisa no Assembly, dois achados (`AUD-01-07` e a metade "Recoil" de `AUD-01-08`) revelaram uma limitação arquitetural real do EFT que a auditoria original não tinha detectado (a auditoria propôs "aplicar via `Item.Attributes`/bônus por-instância" de forma genérica, sem confirmar se existe um ponto de extensão por-instância de fato). A investigação abaixo documenta exatamente o que foi confirmado e o que não tem solução limpa disponível — ver §1.1 e §1.2.

**Decisão do usuário (2026-09-07):** para `AUD-01-07`, investigar mais a fundo em vez de aceitar a regressão cosmética do tooltip — resolvido (ver §1.1, mecanismo `AddOrReplaceAttribute`/`Item.Replacements` confirmado no Assembly). Para `AUD-01-08`, aceitar a metade "Recoil" como está hoje (mutação de template preservada, documentada como débito técnico) — ver §1.2.

## 1. Estratégia

### 1.1 · `AUD-01-07` — Custo de item médico não deve mutar o template compartilhado

**Alvo:** `HealthEffectsComponent(Item, IHealthEffect)` (construtor), já patcheado hoje.

**Achado confirmado:** `HealthEffectsComponent.DamageEffects` ([HealthEffectsComponent.cs:34](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/HealthEffectsComponent.cs#L34)) é um repasse direto de `IHealthEffect.DamageEffects` ([IHealthEffect.cs:14](../../../../references/eft-decompiled/Assembly-CSharp/IHealthEffect.cs#L14)) — um `Dictionary<EDamageEffectType, GClass1443>` cujos valores (`GClass1443`, [GClass1443.cs:16-17](../../../../references/eft-decompiled/Assembly-CSharp/GClass1443.cs#L16-L17)) têm um campo `public int Cost` **público e mutável**. O mod hoje muta esse `Cost` diretamente no objeto compartilhado por `TemplateId`.

**Fix confirmado (elimina o compartilhamento):** ao invés de mutar `GClass1443.Cost` in-place, o Postfix constrói um **dicionário novo** (`Dictionary<EDamageEffectType, GClass1443>`) contendo **clones** dos `GClass1443` que precisam de ajuste (com `Cost` recalculado a partir do valor pristino do template, que agora nunca é tocado) e troca `__instance.IHealthEffect` (campo público, [HealthEffectsComponent.cs:26](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/HealthEffectsComponent.cs#L26)) por um wrapper `PerInstanceHealthEffect : IHealthEffect` que repassa tudo do template original, exceto `DamageEffects` (que retorna o dicionário clonado). Isso elimina o vazamento entre instâncias do mesmo `TemplateId` — o bug literal do achado.

**Benefício colateral:** como o template nunca é mais mutado, toda a maquinaria `OriginalCosts`/`InstanceIdsChangedAtLevel`/`ResetLevelChangedAt` (que existia só para "lembrar" o valor original depois de tê-lo sobrescrito) deixa de ser necessária — `template.DamageEffects[tipo].Cost` passa a ser **sempre** o valor pristino. Simplificação líquida de código.

**Risco de tooltip identificado e RESOLVIDO:** `Item.CreateAttributesFromDictionary(DamageEffects, ...)` ([Item.cs:930-948](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Item.cs#L930-L948)) roda **dentro do construtor original** (antes do nosso Postfix), e cria lambdas de UI (`StringValue = () => spec.GetStringValue()`) que capturam a referência ao objeto `GClass1443` **original e compartilhado** — trocar só `__instance.IHealthEffect` depois desse ponto não atualizaria essas lambdas já criadas, que continuariam presas ao objeto antigo.
**Fix (decisão do usuário: investigar em vez de aceitar a regressão):** o próprio `Item.cs` expõe o mecanismo para substituir uma attribute individual — `Item.AddOrReplaceAttribute(ItemAttributeClass, bool ignoreZero = false)` ([Item.cs:897-908](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Item.cs#L897-L908)) localiza a attribute existente por `Id` (`Attributes.FirstOrDefault(attr => attr.Id.Equals(itemAttribute.Id))`), remove e adiciona a nova. Combinado com `Item.Replacements` (`public static Dictionary<Enum, string>`, [Item.cs:360-390](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Item.cs#L360-L390) — confirma que `Fracture`, `LightBleeding` e `HeavyBleeding`, os 3 tipos que este patch ajusta, têm label de substituição), o Postfix pode **recriar** a attribute de cada tipo ajustado, apontando para o `GClass1443` clonado (com o `Cost` já reduzido) — replicando exatamente a mesma construção que `CreateAttributesFromDictionary<TEnum>(Dictionary<TEnum, GClass1443>, EItemAttributeDisplayType)` ([Item.cs:930-948](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Item.cs#L930-L948)) já faz internamente. Isso mantém o tooltip sincronizado com o custo real, sem precisar de Transpiler no construtor base — ver stub atualizado em §5.1.

**Nota sobre propriedade real do item:** esta correção resolve o compartilhamento entre instâncias do mesmo `TemplateId`, mas **não** adiciona um filtro de "dono real do item" — o ajuste de custo continua sendo calculado a partir de `GameUtils.GetSkillManager()` (sempre o MainPlayer) e aplicado a qualquer `HealthEffectsComponent` construído, seja de um item do jogador local, de um bot, ou de outro jogador Fika. Diferente de `AUD-01-08` (ergonomia, ver §1.2), aqui não existe um filtro de propriedade "de graça" reaproveitável do código já existente do mod, e localizar um caminho barato `Item → dono real` não foi possível dentro do orçamento desta sessão (`Item.Owner` existe mas é um `IItemOwner` de baixo nível sem referência direta a `Player`/`Profile` — ver [IItemOwner.cs](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/IItemOwner.cs)). Registrado como candidato a um achado próprio numa auditoria futura — é uma variante mais sutil do mesmo padrão sistêmico, não o bug literal deste achado (que é sobre compartilhamento de template, já resolvido acima).

### 1.2 · `AUD-01-08` — Ergonomia resolvida por-instância; Recoil permanece mutação de template (limitação confirmada)

**Ergonomia — fix completo e confirmado:** `Weapon.ErgonomicsTotal` ([Weapon.cs:773](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Weapon.cs#L773)) é uma property **por-instância** (`Template.Ergonomics * (1f + ErgonomicsDelta)`, onde `ErgonomicsDelta` soma o bônus dos mods equipados, [Weapon.cs:775](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Weapon.cs#L775)) — e é **confirmadamente** o valor lido pela ergonomia real de gameplay em [Player.cs:12845](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L12845) (`Item.ErgonomicsTotal * (1f + num + _player.ErgonomicsPenalty)`, onde a variável local chamada `Item` é o `Weapon` em mãos). Um Postfix em `Weapon.ErgonomicsTotal` que multiplica `__result` por `(1 + skillBuff)` — só para instâncias já rastreadas em `UsecWeaponInstanceIds`/`EasternWeaponInstanceIds` — reproduz **exatamente** a mesma fórmula matemática que a mutação de template atual (`orig.ergo * (1+skillBuff) * (1+modsDelta)` ≡ `Template.Ergonomics * (1+modsDelta) * (1+skillBuff)`, multiplicação é associativa), sem nunca escrever no `Template` compartilhado. Como `UsecWeaponInstanceIds`/`EasternWeaponInstanceIds` só são populados a partir de `profile.Inventory.AllRealPlayerItems` do **perfil local** ([UpdateWeaponsPatch.cs:54,61-62](../../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs#L54)), o filtro de propriedade "é arma do jogador local" já vem de graça — nenhum código novo de ownership é necessário.

**Recoil — limitação confirmada, sem fix disponível nesta rodada:** ao contrário de Ergonomics, **não existe** uma property por-instância equivalente para `RecoilForceUp` no Assembly. `Weapon.RecoilForceBack` ([Weapon.cs:578](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Weapon.cs#L578)) é um repasse direto de `Template.RecoilForceBack`, e `RecoilForceUp` **não tem property de instância alguma** — todo consumo encontrado no Assembly (tooltip em [Weapon.cs:1022-1024](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Weapon.cs#L1022-L1024), reação de acerto em 3ª pessoa em [Player.cs:26489](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26489)) lê `Template.RecoilForceUp`/`Template.RecoilForceBack` **diretamente**, e o Harmony não intercepta leitura de campo público (só métodos/properties). A propriedade `RecoilDelta`/`RecoilTotal` ([Weapon.cs:767,771](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Weapon.cs#L767-L771)) existe mas **nenhum consumidor real foi localizado** nesta sessão — parece ser calculada e nunca lida (possível código morto/vestigial, ou consumida por uma classe com nome ofuscado não localizada dentro do orçamento de busca).
**Conclusão:** a sugestão genérica da auditoria ("aplicar via `Item.Attributes`/bônus por-instância") **não tem caminho de implementação de baixo risco confirmado para Recoil** — o único mecanismo observado no jogo para este campo específico é mutação direta do `Template`. **Decisão proposta:** manter a mutação de `Template.RecoilForceUp`/`RecoilForceBack` exatamente como está hoje (comportamento inalterado), documentar a limitação inline com referência a este documento, e não fechar esta metade do achado como resolvida — registrar como débito técnico aceito (mesmo padrão de decisão do item 001 para `CR-01-02`), candidato a nova investigação (via profiling/logging em runtime, não só leitura estática) numa rodada futura.

### 1.3 · `AUD-01-09` — Gating do reprocessamento de armas por raid + remoção do `.Clear()` incondicional

**Alvo:** `MenuTaskBar.OnScreenChanged` (já patcheado hoje, [UpdateWeaponsPatch.cs:29](../../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs#L29)). `GameUtils.IsInRaid()` já existe e é usado em outros pontos do mod ([GameUtils.cs:15-18](../../modded/Plugin/Utils/GameUtils.cs#L15-L18)). Adicionar `if (!GameUtils.IsInRaid()) return;` no topo do Prefix, e **remover** as chamadas `UsecWeaponInstanceIds.Clear()`/`EasternWeaponInstanceIds.Clear()` do Prefix — o próprio corpo da coroutine já pula instâncias cujo nível não mudou (`if (UsecWeaponInstanceIds[item.Id] == skillManager.UsecArsystems.Level) continue;`, [UpdateWeaponsPatch.cs:94](../../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs#L94)), então o `.Clear()` só existia para forçar reprocessamento total a cada disparo — removê-lo faz o mecanismo de dirty-tracking já existente funcionar como pretendido.

**Resolução de `PA-01-01` (review 01):** gatear por `IsInRaid()` remove o comportamento colateral que existia antes desta correção — navegar telas no **hideout antes de entrar em raid** já disparava a coroutine, e como a mutação de template era compartilhada, o efeito ficava "pré-cozido" antes da raid começar. Com o gate + a migração de Ergonomics para `ErgonomicsTotalPatch` (que só aplica bônus a instâncias já registradas em `UsecWeaponInstanceIds`), o bônus só existe depois da coroutine rodar pelo menos uma vez **dentro** da raid — e não há garantia de que `MenuTaskBar.OnScreenChanged` dispare automaticamente no exato instante em que a raid começa. Fix: `UpdateWeaponsPatch` ganha `internal static void TriggerRaidStart()` (mesma lógica do Prefix, sem o gate — o caller já garante o contexto), chamado uma vez por `OnGameStartedPatch.Postfix` (§1.6/§5.7) — garante que o cache seja populado desde o primeiro frame da raid, independente de qualquer interação de tela.

### 1.4 · `AUD-01-10` — Cap de skill exibido resolvido pelo dono real via mapa de propriedade

**Achado confirmado:** `AbstractSkillClass` ([AbstractSkillClass.cs:8-159](../../../../references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs)) **não tem** nenhuma referência de volta ao `SkillManager`/`Player` dono — `SummaryLevel` ([AbstractSkillClass.cs:61](../../../../references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs#L61)) é `Mathf.Min((Buff > 0) ? 60 : 51, Level + Buff)`, calculado inteiramente a partir de campos do próprio `__instance`. O mod hoje substitui a constante `60` por um cap vindo de `GameUtils.GetSkillManager()` (MainPlayer), ignorando de quem é o `__instance` de fato.

**Fix confirmado:** o mod já tem `SkillManagerConstructorPatch` ([SkillManagerConstructorPatch.cs:11-32](../../modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs#L11-L32)), que roda no **Postfix do construtor de `SkillManager`** — ou seja, para **todo** `SkillManager` criado no processo (jogador local, bots, outros jogadores Fika observados localmente). No Postfix desse patch já existente, registrar `skillManager.FieldMedicine` (campo `public SkillClass FieldMedicine`, [SkillManager.cs:1583](../../../../references/eft-decompiled/Assembly-CSharp/EFT/SkillManager.cs#L1583); `SkillClass : AbstractSkillClass`, [SkillClass.cs:9](../../../../references/eft-decompiled/Assembly-CSharp/SkillClass.cs#L9)) num `ConditionalWeakTable<AbstractSkillClass, SkillManager>` estático. `AbstractSkillClassSummaryLevelPatch` passa a resolver o `SkillManager` do dono real via esse mapa, em vez de `GameUtils.GetSkillManager()`.
**Por que `ConditionalWeakTable` e não `Dictionary`:** um `Dictionary<AbstractSkillClass, SkillManager>` estático seguraria referência forte a **todo** `SkillManager` (e, por extensão, `Profile`/`Player`) já criado no processo, inclusive de bots mortos há muito tempo — leak equivalente ao próprio `AUD-01-14` deste item. `ConditionalWeakTable` mantém a entrada só enquanto a chave (`AbstractSkillClass`) ainda é referenciada por outro lugar (o próprio `SkillManager` dono) — quando o `SkillManager`/`Profile` de um bot é coletado pelo GC ao fim da raid, a entrada correspondente desaparece sozinha, sem precisar de um `.Clear()` manual em lugar nenhum.

### 1.5 · `AUD-01-11` — Null-guard em `GetBarterPricePatch`/`RequiredItemsCountPatch`

**Alvo:** ambos os patches já existentes hoje ([GetBarterPricePatch.cs:15-87](../../modded/Plugin/Skills/SilentOps/Patches/GetBarterPricePatch.cs)), alvo confirmado `TraderAssortmentControllerClass.GetBarterPrice` em [TraderAssortmentControllerClass.cs:557](../../../../references/eft-decompiled/Assembly-CSharp/TraderAssortmentControllerClass.cs#L557). Fix: capturar `GameUtils.GetSkillManager()` numa variável local **antes** do loop e retornar cedo se `null`, em vez de `GetSkillManager()!` sem checagem dentro do loop — mesmo padrão já correto usado em `HealthEffectComponentPatch` (mod, não Assembly).

### 1.6 · `AUD-01-12` — Reusar campo `Player` já validado em vez de nova chamada insegura

**Alvo:** `OnGameStartedPatch.ApplyMedicalXp` ([OnGameStarted.cs:97](../../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs#L97) — arquivo do mod, não Assembly). Fix: trocar `GameUtils.GetPlayer()!.Skills.FirstAid.IsEliteLevel` por `Player.Skills.FirstAid.IsEliteLevel` (campo estático `Player` da própria classe).

**Resolução de `PA-01-02` (review 01):** a troca acima sozinha não cobria o corner case obrigatório da spec funcional ("handler dispara numa janela de transição de cena... não pode falhar sem tratamento"), já que `Player!` (null-forgiving, sem checagem real) já era usado incondicionalmente na primeira linha do método, antes de qualquer branch. Fix completo: adicionar `if (Player == null) return;` no topo de `ApplyMedicalXp`, e usar `Player` (sem `!`) no resto do corpo — casa com `OnGameEndedPatch` (§1.8) zerando `Player` no fim da raid, tornando qualquer invocação tardia/residual do evento um no-op seguro.

### 1.7 · `AUD-01-13` — Remover 7 campos de reflection não utilizados (inclui os 2 com bug de copy-paste)

**Alvo:** `ReflectionHelper` ([ReflectionHelper.cs:10-34](../../modded/Plugin/Helpers/ReflectionHelper.cs#L10-L34) — arquivo do mod). Confirmado nesta sessão (Grep qualificado/não-qualificado nos 6 projetos do mod) que `BleedType`, `LightBleedType`, `HeavyBleedType`, `FractureType`, `PainType`, `MedEffectType`, `StimulatorType` não são lidos por nenhum código. Fix: remover os 7 campos e a lógica de resolução/checagem de `null` correspondente no construtor estático — não há necessidade de "corrigir os nomes" de campos que ninguém usa.

### 1.8 · `AUD-01-14` — Desinscrição de eventos de skill em `GameWorld.OnDestroy`

**Alvo novo:** `GameWorld.OnDestroy()`, `virtual`, [GameWorld.cs:2111](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111). Cadeia de overrides auditada por completo (AP-03): só `ClientGameWorld : GameWorld` existe como subclasse direta ([ClientGameWorld.cs:15](../../../../references/eft-decompiled/Assembly-CSharp/EFT/ClientGameWorld.cs#L15)), cujo `OnDestroy()` chama `base.OnDestroy()` ([ClientGameWorld.cs:219-222](../../../../references/eft-decompiled/Assembly-CSharp/EFT/ClientGameWorld.cs#L219-L222)). Dela derivam só duas classes concretas: `ClientLocalGameWorld` (raid solo — **não** sobrescreve `OnDestroy`, herda direto de `ClientGameWorld`) e `ClientNetworkGameWorld` (raid coop/Fika, inclusive headless — sobrescreve `OnDestroy` e chama `base.OnDestroy()`, [ClientNetworkGameWorld.cs:61-64](../../../../references/eft-decompiled/Assembly-CSharp/ClientNetworkGameWorld.cs#L61-L64)). **Os dois caminhos reais de raid (solo e coop) confirmadamente chegam em `GameWorld.OnDestroy()`** — achado fechado, sem débito técnico residual.
**Fix:** novo `ModulePatch` (`OnGameEndedPatch`, mesmo arquivo/namespace de `OnGameStartedPatch`) com Postfix em `GameWorld.OnDestroy` que desinscreve os 3 handlers (`EffectStartedEvent -= ApplyMedicalXp`, `OnMasteringExperienceChanged -= ApplyNatoRifleXp`, `OnMasteringExperienceChanged -= ApplyEasternRifleXp`), zera o campo estático `Player`, e chama `UpdateWeaponsPatch.ClearRaidState()` (novo método `internal static` que limpa `UsecOriginalWeaponValues`/`UsecWeaponInstanceIds`/`EasternOriginalWeaponValues`/`EasternWeaponInstanceIds` — consolida a limpeza de estado entre raids também para o achado `AUD-01-09`, já que os dicionários de "já processado nesta raid" não fazem mais sentido guardados de uma raid pra outra depois que o `.Clear()` do Prefix foi removido em §1.3).
**Requer:** `Player` (campo `[CanBeNull] private static Player Player;`, [OnGameStarted.cs:26](../../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs#L26)) muda de `private` para `internal` para o novo patch, no mesmo namespace, poder acessá-lo. `Action<MasterSkillClass> OnMasteringExperienceChanged` confirmado em [SkillManager.cs:1865](../../../../references/eft-decompiled/Assembly-CSharp/EFT/SkillManager.cs#L1865); `event Action<IEffect> EffectStartedEvent` confirmado em [ActiveHealthController.cs:3399](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3399).

### 1.9 · `AUD-01-15` — Limpar `DoorAttempts` no `OnGameStarted` do lado FIKA (inclusive headless)

**Alvo:** `FikaSync/Patches/OnGameStartedPatch.PatchPostfix` ([OnGameStartedPatch.cs:16-20](../../modded/FikaSync/Patches/OnGameStartedPatch.cs#L16-L20) — arquivo do mod), já roda para **client e headless** (sem guard de `IsFikaHeadless`, ao contrário do `OnGameStartedPatch` do lado `Plugin/`). Fix: adicionar `LockPickingHelpers.DoorAttempts.Clear();` no início do método, antes de `LockPickingFikaController.GetDoors()`. Requer `using SkillsExtended.Skills.LockPicking;` novo neste arquivo.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT.InventoryLogic/HealthEffectsComponent.cs:38`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/HealthEffectsComponent.cs#L38) (construtor, já patcheado) | Postfix (reescrito) | Troca `__instance.IHealthEffect` por wrapper per-instance em vez de mutar `Cost` compartilhado — `AUD-01-07` |
| [`EFT.InventoryLogic/Weapon.cs:773`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/Weapon.cs#L773) `get_ErgonomicsTotal` | Postfix (novo) | Aplica o bônus de ergonomia por-instância em vez de mutar `Template.Ergonomics` — `AUD-01-08` (metade Ergonomics) |
| `MenuTaskBar.OnScreenChanged` (já patcheado hoje) | Prefix (modificado) | Gate por `GameUtils.IsInRaid()` + remoção do `.Clear()` incondicional — `AUD-01-09` |
| `SkillManager` construtor (já patcheado hoje, `SkillManagerConstructorPatch`) | Postfix (estendido) | Registra `skillManager.FieldMedicine` num mapa fraco de propriedade — `AUD-01-10` |
| [`TraderAssortmentControllerClass.cs:557`](../../../../references/eft-decompiled/Assembly-CSharp/TraderAssortmentControllerClass.cs#L557) `GetBarterPrice` (já patcheado hoje) | Postfix (modificado) | Null-guard antes de usar o `SkillManager` — `AUD-01-11` |
| `RequiredItemsCount` (já patcheado hoje, alvo resolvido por reflection) | Postfix (modificado) | Mesmo null-guard — `AUD-01-11` |
| [`EFT/GameWorld.cs:2111`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111) `OnDestroy` | Postfix (novo) | Desinscreve eventos de skill + limpa caches de arma entre raids — `AUD-01-14` (+ suporte a `AUD-01-09`) |
| `GameWorld.OnGameStarted` (lado FikaSync, já patcheado hoje) | Postfix (modificado) | Limpa `DoorAttempts` inclusive no headless — `AUD-01-15` |

`AUD-01-12` e `AUD-01-13` são edições dentro de patches/classes já existentes do mod, sem novo ponto de patch no Assembly.

## 3. Novas propriedades F12 (BepInEx)

N/A — este item não introduz nenhuma `ConfigEntry` nova nem altera semântica de uma existente.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin/Skills/FirstAid/Patches/HealthEffectComponentPatch.cs` | MODIFICAR | Substitui mutação de `Cost` compartilhado por wrapper `IHealthEffect` per-instance; remove bookkeeping `OriginalCosts`/`InstanceIdsChangedAtLevel` (`AUD-01-07`) |
| `modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs` | MODIFICAR | Gate por raid + remoção do `.Clear()` (`AUD-01-09`); adiciona `internal static void ClearRaidState()` (suporte `AUD-01-14`); remove mutação de `Template.Ergonomics` (`AUD-01-08` parcial) |
| `modded/Plugin/Skills/WeaponSkills/Patches/ErgonomicsTotalPatch.cs` | CRIAR | Postfix em `Weapon.ErgonomicsTotal` aplicando o bônus de ergonomia por-instância (`AUD-01-08` parcial) |
| `modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs` | MODIFICAR | Adiciona `ConditionalWeakTable<AbstractSkillClass, SkillManager> SkillOwners` e o registro de `FieldMedicine` no Postfix (`AUD-01-10`) |
| `modded/Plugin/Skills/FieldMedicine/Patches/AbstractSkillClassSummaryLevelPatch.cs` | MODIFICAR | Resolve o `SkillManager` via `SkillManagerConstructorPatch.SkillOwners` em vez de `GameUtils.GetSkillManager()` (`AUD-01-10`) |
| `modded/Plugin/Skills/SilentOps/Patches/GetBarterPricePatch.cs` | MODIFICAR | Null-guard em `GetBarterPricePatch`/`RequiredItemsCountPatch` (`AUD-01-11`) |
| `modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs` | MODIFICAR | `Player` vira `internal`; `ApplyMedicalXp` reusa `Player` em vez de `GameUtils.GetPlayer()!` (`AUD-01-12`); adiciona classe `OnGameEndedPatch` no mesmo arquivo (`AUD-01-14`) |
| `modded/Plugin/Helpers/ReflectionHelper.cs` | MODIFICAR | Remove os 7 campos de reflection não utilizados e a lógica de resolução correspondente (`AUD-01-13`) |
| `modded/FikaSync/Patches/OnGameStartedPatch.cs` | MODIFICAR | Adiciona `LockPickingHelpers.DoorAttempts.Clear()` (`AUD-01-15`) |

## 5. Stubs de código

### 5.1 · `HealthEffectComponentPatch.cs` (reescrito) — `AUD-01-07`

```csharp
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.FirstAid.Patches;

// ref: EFT.InventoryLogic/IHealthEffect.cs:6-15 — GInterface392 (StimulatorBuffs) + membros de IHealthEffect
internal sealed class PerInstanceHealthEffect : IHealthEffect
{
    private readonly IHealthEffect _original;
    private readonly Dictionary<EDamageEffectType, GClass1443> _damageEffects;

    public PerInstanceHealthEffect(IHealthEffect original, Dictionary<EDamageEffectType, GClass1443> damageEffects)
    {
        _original = original;
        _damageEffects = damageEffects;
    }

    public float UseTime => _original.UseTime;
    public KeyValuePair<EBodyPart, float>[] BodyPartTimeMults => _original.BodyPartTimeMults;
    public Dictionary<EHealthFactorType, GClass1444> HealthEffects => _original.HealthEffects;
    public Dictionary<EDamageEffectType, GClass1443> DamageEffects => _damageEffects;
    public string StimulatorBuffs => _original.StimulatorBuffs;
}

public class HealthEffectComponentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: EFT.InventoryLogic/HealthEffectsComponent.cs:38
        return AccessTools.Constructor(typeof(HealthEffectsComponent), [typeof(Item), typeof(IHealthEffect)]);
    }

    [PatchPostfix]
    public static void PostFix(HealthEffectsComponent __instance, Item item, IHealthEffect template)
    {
        var skillData = SkillsExtendedPlugin.SkillData.FirstAid;
        if (!skillData.Enabled)
        {
            return;
        }

        var skillManager = GameUtils.GetSkillManager();
        if (skillManager == null)
        {
            return;
        }

        if (template.DamageEffects is null || item is not MedicalItemClass meds)
        {
            return;
        }

        // Why? -- I don't know, but leave it for now because something probably broke
        if (meds.TemplateId.LocalizedName().Contains("Name"))
        {
            return;
        }

        // ref: AUD-01-07 — clona em um dicionário novo em vez de mutar template.DamageEffects[x].Cost
        // (compartilhado por todos os itens do mesmo TemplateId, EFT.InventoryLogic/HealthEffectsComponent.cs:34).
        var adjusted = new Dictionary<EDamageEffectType, GClass1443>(template.DamageEffects);
        var changedTypes = new List<EDamageEffectType>(3);

        TryCloneWithAdjustedCost(adjusted, EDamageEffectType.Fracture, skillManager, changedTypes);
        TryCloneWithAdjustedCost(adjusted, EDamageEffectType.LightBleeding, skillManager, changedTypes);
        TryCloneWithAdjustedCost(adjusted, EDamageEffectType.HeavyBleeding, skillManager, changedTypes);

        if (changedTypes.Count == 0)
        {
            return;
        }

        __instance.IHealthEffect = new PerInstanceHealthEffect(template, adjusted);

        // ref: AUD-01-07 — o tooltip (Item.Attributes) foi montado no construtor base ANTES deste Postfix
        // rodar, com lambdas presas ao GClass1443 original e compartilhado (EFT.InventoryLogic/Item.cs:930-948).
        // Recriamos a attribute de cada tipo ajustado apontando pro clone, via o mesmo mecanismo que o EFT usa
        // (Item.AddOrReplaceAttribute + Item.Replacements, EFT.InventoryLogic/Item.cs:360-390,897-908) —
        // sem isso o tooltip continuaria mostrando o custo pristino, não o já reduzido pela skill.
        foreach (var type in changedTypes)
        {
            if (!Item.Replacements.TryGetValue(type, out var replacement))
            {
                continue;
            }

            var spec = adjusted[type];
            item.AddOrReplaceAttribute(new ItemAttributeClass(type)
            {
                Name = replacement,
                DisplayType = () => EItemAttributeDisplayType.Compact,
                StringValue = () => spec.GetStringValue(),
                FullStringValue = () => spec.GetFullStringValue(replacement)
            });
        }

#if DEBUG
        Logger.LogDebug($"[FirstAid] Per-instance cost applied to {meds.TemplateId.LocalizedName()}\n");
#endif
    }

    private static void TryCloneWithAdjustedCost(
        Dictionary<EDamageEffectType, GClass1443> adjusted,
        EDamageEffectType type,
        SkillManager skillManager,
        List<EDamageEffectType> changedTypes)
    {
        if (!adjusted.TryGetValue(type, out var original) || original is null || original.Cost <= 0)
        {
            return;
        }

        var newCost = original.Cost;
        skillManager.SkillManagerExtended.FirstAidResourceCostBuff.Apply(ref newCost);

        adjusted[type] = new GClass1443
        {
            Delay = original.Delay,
            Duration = original.Duration,
            FadeOut = original.FadeOut,
            Cost = newCost,
            HealthPenaltyMin = original.HealthPenaltyMin,
            HealthPenaltyMax = original.HealthPenaltyMax
        };

        changedTypes.Add(type);
    }
}
```

> O tooltip é ressincronizado explicitamente (loop `AddOrReplaceAttribute` acima) — não fica mais preso ao valor pristino. Ver §1.1 para a evidência completa do porquê isso era necessário.

### 5.2 · `ErgonomicsTotalPatch.cs` (novo) — `AUD-01-08` (metade Ergonomics)

```csharp
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.WeaponSkills.Patches;

internal class ErgonomicsTotalPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: EFT.InventoryLogic/Weapon.cs:773 — property por-instância, lida em Player.cs:12845
        return AccessTools.PropertyGetter(typeof(Weapon), nameof(Weapon.ErgonomicsTotal));
    }

    [PatchPostfix]
    private static void Postfix(Weapon __instance, ref float __result)
    {
        if (SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled &&
            UpdateWeaponsPatch.UsecWeaponInstanceIds.ContainsKey(__instance.Id))
        {
            var skillManager = GameUtils.GetSkillManager();
            if (skillManager != null)
            {
                __result *= 1f + skillManager.SkillManagerExtended.UsecArSystemsErgoBuff;
            }
            return;
        }

        if (SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled &&
            UpdateWeaponsPatch.EasternWeaponInstanceIds.ContainsKey(__instance.Id))
        {
            var skillManager = GameUtils.GetSkillManager();
            if (skillManager != null)
            {
                __result *= 1f + skillManager.SkillManagerExtended.BearAkSystemsErgoBuff;
            }
        }
    }
}
```

> Requer que `UsecWeaponInstanceIds`/`EasternWeaponInstanceIds` em `UpdateWeaponsPatch` passem de `private` para `internal static readonly` (visibilidade dentro do mesmo assembly, mesma pasta `Patches/`).

### 5.3 · `UpdateWeaponsPatch.cs` (diffs) — `AUD-01-08` (parcial), `AUD-01-09`, suporte a `AUD-01-14`

```csharp
internal class UpdateWeaponsPatch : ModulePatch
{
    internal static readonly Dictionary<string, OrigWeaponValues> UsecOriginalWeaponValues = [];
    internal static readonly Dictionary<string, int> UsecWeaponInstanceIds = [];

    internal static readonly Dictionary<string, OrigWeaponValues> EasternOriginalWeaponValues = [];
    internal static readonly Dictionary<string, int> EasternWeaponInstanceIds = [];

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuTaskBar), nameof(MenuTaskBar.OnScreenChanged));
    }

    [PatchPrefix]
    public static void Prefix(EEftScreenType eftScreenType)
    {
        // ref: AUD-01-09 — gate por raid; sem isso o processamento inteiro rodava em qualquer tela de menu,
        // inclusive fora de raid. `.Clear()` removido: o dirty-tracking por nível já existe no corpo da coroutine.
        if (!GameUtils.IsInRaid())
        {
            return;
        }

        if (SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled)
        {
            StaticManager.BeginCoroutine(UpdateUsecWeapons());
        }

        if (SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled)
        {
            StaticManager.BeginCoroutine(UpdateEasternWeapons());
        }
    }

    // ref: AUD-01-14 — chamado pelo OnGameEndedPatch (OnGameStarted.cs) no Postfix de GameWorld.OnDestroy.
    internal static void ClearRaidState()
    {
        UsecOriginalWeaponValues.Clear();
        UsecWeaponInstanceIds.Clear();
        EasternOriginalWeaponValues.Clear();
        EasternWeaponInstanceIds.Clear();
    }

    // ref: PA-01-01 (review 01) — chamado por OnGameStartedPatch.Postfix pra garantir que o bônus já
    // esteja ativo desde o início da raid, sem depender do jogador abrir uma tela primeiro.
    internal static void TriggerRaidStart()
    {
        if (SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled)
        {
            StaticManager.BeginCoroutine(UpdateUsecWeapons());
        }

        if (SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled)
        {
            StaticManager.BeginCoroutine(UpdateEasternWeapons());
        }
    }

    // UpdateUsecWeapons()/UpdateEasternWeapons(): remover as duas linhas
    //   weapon.Template.Ergonomics = ... ;
    // (mutação de template compartilhado — corrigida via ErgonomicsTotalPatch, §5.2).
    // As linhas de RecoilForceUp/RecoilForceBack permanecem inalteradas — ver §1.2 (limitação confirmada, sem
    // fix disponível: nenhuma property por-instância existe para esses dois campos no Assembly).
}
```

### 5.4 · `SkillManagerConstructorPatch.cs` (diff) — `AUD-01-10`

```csharp
using System.Runtime.CompilerServices;
// ...usings existentes...

internal class SkillManagerConstructorPatch : ModulePatch
{
    // ref: AUD-01-10 — mapa fraco (não impede GC do SkillManager/Profile dono ao fim da raid).
    internal static readonly ConditionalWeakTable<AbstractSkillClass, SkillManager> SkillOwners = new();

    // ...GetTargetMethod()/Prefix() existentes sem mudança...

    [PatchPostfix]
    public static void Postfix(SkillManager __instance, ref SkillClass[] ___DisplayList, ref SkillClass[] ___Skills)
    {
        InitializeNewSkills(__instance, ref ___Skills);
        ModifyDisplayList(__instance, ref ___DisplayList);
        LockSkills(__instance);

        // ref: EFT/SkillManager.cs:2530 — FieldMedicine já construído pelo ctor base neste ponto.
        if (!SkillOwners.TryGetValue(__instance.FieldMedicine, out _))
        {
            SkillOwners.Add(__instance.FieldMedicine, __instance);
        }
    }

    // ...métodos privados existentes sem mudança...
}
```

### 5.5 · `AbstractSkillClassSummaryLevelPatch.cs` (reescrito) — `AUD-01-10`

```csharp
using System.Reflection;
using HarmonyLib;
using SkillsExtended.Skills.Core.Patches;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SkillsExtended.Skills.FieldMedicine.Patches;

public class AbstractSkillClassSummaryLevelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: AbstractSkillClass.cs:61
        return AccessTools.PropertyGetter(typeof(AbstractSkillClass), nameof(AbstractSkillClass.SummaryLevel));
    }

    [PatchPrefix]
    public static bool Prefix(AbstractSkillClass __instance, ref int __result)
    {
        if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled)
        {
            return true;
        }

        // ref: AUD-01-10 — dono real via SkillManagerConstructorPatch.SkillOwners, não GameUtils.GetSkillManager()
        // (que sempre resolve o MainPlayer, independentemente de quem é o __instance de fato).
        if (!SkillManagerConstructorPatch.SkillOwners.TryGetValue(__instance, out var skillManager) || skillManager == null)
        {
            return true;
        }

        var newSkillCap = 60 * (1 + skillManager.SkillManagerExtended.FieldMedicineSkillCap);

        var level = __instance.Level;
        var buff = __instance.Buff;
        __result = Mathf.CeilToInt(Mathf.Min(buff > 0 ? newSkillCap : 51, level + buff));

        return false;
    }
}
```

### 5.6 · `GetBarterPricePatch.cs` (diffs) — `AUD-01-11`

```csharp
[PatchPostfix]
private static void Postfix(TraderAssortmentControllerClass __instance, ref TraderClass.GStruct300? __result, Item[] items)
{
    if (!SkillsExtendedPlugin.SkillData.SilentOps.Enabled || items.IsNullOrEmpty())
    {
        return;
    }

    // ref: AUD-01-11 — captura e checa null antes do loop, em vez de GetSkillManager()! dentro dele.
    var skillManager = GameUtils.GetSkillManager();
    if (skillManager == null)
    {
        return;
    }

    var scheme = __instance.GetSchemeForItem(items[0]);
    if (scheme is null)
    {
        return;
    }

    float price = 0;
    foreach (var item in items)
    {
        var barterScheme = __instance.GetSchemeForItem(item);
        if (barterScheme is null)
        {
            continue;
        }

        var num2 = Mathf.Ceil((float)barterScheme.Sum(TraderAssortmentControllerClass.Class2058.class2058_0.method_0));
        var bonus = 1f - skillManager.SkillManagerExtended.SilentOpsSilencerCostRedBuff;

        if (item is SilencerItemClass)
        {
            num2 *= bonus;
        }

        price += num2;
    }

    Selecteditem = __instance.SelectedItem;
    __result = new TraderClass.GStruct300(scheme[0][0]._tpl, (int)Mathf.Ceil(price));
}
```

```csharp
[PatchPostfix]
private static void Postfix(GClass2064 __instance, ref int __result)
{
    if (!SkillsExtendedPlugin.SkillData.SilentOps.Enabled || GetBarterPricePatch.Selecteditem is not SilencerItemClass)
    {
        return;
    }

    // ref: AUD-01-11 — mesmo guard.
    var skillManager = GameUtils.GetSkillManager();
    if (skillManager == null)
    {
        return;
    }

    var bonus = 1f - skillManager.SkillManagerExtended.SilentOpsSilencerCostRedBuff;
    __result = (int)Mathf.Ceil(__result * bonus);
}
```

### 5.7 · `OnGameStarted.cs` (diffs) — `AUD-01-12`, `AUD-01-14`

```csharp
internal class OnGameStartedPatch : ModulePatch
{
    // ...
    [CanBeNull] internal static Player Player; // ref: AUD-01-14 — era private; OnGameEndedPatch precisa acessar.

    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        // ...corpo existente sem mudança (guard headless, LockPickingHelpers.InitializeLockpickingForLocation,
        // atribuição de Player, as 3 subscriptions de evento, FixDoors)...

        // ref: PA-01-01 (review 01) — garante que o bônus de Ergonomia/Recuo já esteja ativo desde o
        // primeiro frame da raid, sem depender do jogador abrir uma tela de menu primeiro.
        UpdateWeaponsPatch.TriggerRaidStart();

#if DEBUG
        LogMissingDoors(__instance);
#endif
    }

    // ref: PA-01-03 (review 01) — internal (era private) para OnGameEndedPatch poder desinscrever.
    internal static void ApplyMedicalXp(IEffect effect)
    {
        // ref: PA-01-02 (review 01) — guard de nulidade explícito; Player! (null-forgiving sem checagem)
        // era usado incondicionalmente aqui antes, sem cobrir o corner case de janela de transição de cena
        // exigido pela spec funcional. Casa com OnGameEndedPatch zerando Player no fim da raid.
        if (Player == null)
        {
            return;
        }

        var skillMgrExt = Player.Skills.SkillManagerExtended;

        if (SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled && _stimType.IsInstanceOfType(effect) || _painKillerType.IsInstanceOfType(effect))
        {
            if (Player.Skills.FieldMedicine.IsEliteLevel)
            {
                return;
            }

            var xpGain = SkillsExtendedPlugin.SkillData.FieldMedicine.XpPerAction;
            Player.ExecuteSkill(() => skillMgrExt.FieldMedicineAction.Complete(xpGain));
            return;
        }

        if (SkillsExtendedPlugin.SkillData.FirstAid.Enabled && _medEffectType.IsInstanceOfType(effect))
        {
            // ref: AUD-01-12 — reusa o campo Player já validado em vez de GameUtils.GetPlayer()! (redundante e inseguro).
            if (Player.Skills.FirstAid.IsEliteLevel)
            {
                return;
            }

            var xpGain = SkillsExtendedPlugin.SkillData.FirstAid.XpPerAction;
            Player.ExecuteSkill(() => skillMgrExt.FirstAidAction.Complete(xpGain));
        }
    }

    // ref: PA-01-03 (review 01) — internal (eram private) para OnGameEndedPatch poder desinscrever.
    internal static void ApplyNatoRifleXp(MasterSkillClass skillClass) { /* ...corpo existente sem mudança... */ }
    internal static void ApplyEasternRifleXp(MasterSkillClass skillClass) { /* ...corpo existente sem mudança... */ }
}

// ref: AUD-01-14 — GameWorld.cs:2111 (OnDestroy, virtual; override em ClientGameWorld.cs:219-222 chama base).
internal class OnGameEndedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        // ref: PA-01-03 (review 01) — nomes reais, sem placeholder.
        if (OnGameStartedPatch.Player != null)
        {
            OnGameStartedPatch.Player.ActiveHealthController.EffectStartedEvent -= OnGameStartedPatch.ApplyMedicalXp;
            OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyNatoRifleXp;
            OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyEasternRifleXp;
            OnGameStartedPatch.Player = null;
        }

        UpdateWeaponsPatch.ClearRaidState();
    }
}
```

### 5.8 · `ReflectionHelper.cs` (reescrito) — `AUD-01-13`

```csharp
using System;
using HarmonyLib;
using SkillsExtended.Exceptions;

namespace SkillsExtended.Helpers;

public static class ReflectionHelper
{
    internal static Type OldMovementIdleState;

    public static void GetOldMovementTypes()
    {
        OldMovementIdleState = AccessTools.TypeByName("OldIdleState");

        if (OldMovementIdleState is null)
        {
            throw new SkillsExtendedException("Could not find OldIdleState or OldStationaryState");
        }
    }
}
```

> `AUD-01-13` — os 7 campos (`BleedType`, `LightBleedType`, `HeavyBleedType`, `FractureType`, `PainType`, `MedEffectType`, `StimulatorType`) e o construtor estático que os resolvia (incluindo o bug de copy-paste em `LightBleedType`/`StimulatorType`) foram removidos por não terem nenhum consumidor no mod (confirmado por Grep qualificado/não-qualificado nos 6 projetos).

### 5.9 · `FikaSync/Patches/OnGameStartedPatch.cs` (diff) — `AUD-01-15`

```csharp
using System.Reflection;
using EFT;
using HarmonyLib;
using SkillsExtended.Skills.LockPicking;
using SkillsExtendedFika.Controllers;
using SPT.Reflection.Patching;

namespace SkillsExtendedFika.Patches;

public class OnGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: EFT/GameWorld.cs:2584
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    public static void PatchPostfix(GameWorld __instance)
    {
        // ref: AUD-01-15 — sem isto, DoorAttempts nunca é limpo no headless (o lado Plugin/OnGameStarted.cs
        // que faz isso tem guard `IsFikaHeadless => return`, então nunca roda lá).
        LockPickingHelpers.DoorAttempts.Clear();
        LockPickingFikaController.GetDoors();
    }
}
```

## 6. Fluxo de dados

```
[AUD-01-07] Item médico usado → HealthEffectsComponent construído → Postfix clona DamageEffects
            → __instance.IHealthEffect trocado por PerInstanceHealthEffect → custo ajustado só nesta instância

[AUD-01-08 Ergo] Arma equipada/skill muda → UpdateUsecWeapons() marca item.Id em UsecWeaponInstanceIds
            → Weapon.ErgonomicsTotal (get) → ErgonomicsTotalPatch.Postfix aplica bônus só se Id rastreado
            → Player.cs:12845 lê ErgonomicsTotal já ajustado (gameplay real)

[AUD-01-09] Menu troca de tela → UpdateWeaponsPatch.Prefix → GameUtils.IsInRaid()? → só dispara coroutine em raid
            → coroutine pula instâncias já no nível atual (dirty-tracking existente, sem .Clear() forçado)

[PA-01-01]  GameWorld.OnGameStarted() → OnGameStartedPatch.Postfix → UpdateWeaponsPatch.TriggerRaidStart()
            → bônus de Ergonomia já ativo desde o 1º frame da raid, sem depender de MenuTaskBar.OnScreenChanged

[AUD-01-10] SkillManager (qualquer dono — MainPlayer, bot, peer) construído → SkillManagerConstructorPatch.Postfix
            → registra FieldMedicine em SkillOwners (ConditionalWeakTable)
            → AbstractSkillClass.SummaryLevel (get) → AbstractSkillClassSummaryLevelPatch.Prefix
            → resolve o SkillManager real via SkillOwners[__instance] → cap correto do dono real

[AUD-01-14] Raid termina → GameWorld.OnDestroy() → OnGameEndedPatch.Postfix
            → desinscreve os 3 eventos de skill do Player da raid que terminou → limpa UpdateWeaponsPatch (AUD-01-09)

[AUD-01-15] GameWorld.OnGameStarted() (client OU headless) → FikaSync/OnGameStartedPatch.Postfix
            → LockPickingHelpers.DoorAttempts.Clear() → sem acúmulo entre partidas no processo headless
```

## 7. Riscos e dependências

- **Patches existentes que este item modifica diretamente:** `HealthEffectComponentPatch`, `UpdateWeaponsPatch`, `SkillManagerConstructorPatch`, `AbstractSkillClassSummaryLevelPatch`, `GetBarterPricePatch`/`RequiredItemsCountPatch`, `OnGameStartedPatch` (ambos os lados, `Plugin/` e `FikaSync/`), `ReflectionHelper` — todos já existem em `mods/Skills-Extended/modded/`, nenhum conflito com mods externos identificado (todos os alvos são específicos do Skills-Extended ou classes nativas do EFT sem outro patch conhecido no repo tocando os mesmos métodos).
- **Dependência entre achados desta rodada:** `AUD-01-08` (Ergonomics) depende de `UsecWeaponInstanceIds`/`EasternWeaponInstanceIds` virarem `internal` (mudança feita para `AUD-01-09` também); `AUD-01-14` depende de `UpdateWeaponsPatch.ClearRaidState()` (novo método, criado para dar suporte a `AUD-01-09`/`AUD-01-14` juntos) e de `OnGameStartedPatch.Player` virar `internal`. Implementar em ordem: `AUD-01-09` → `AUD-01-08` → `AUD-01-14` evita retrabalho de visibilidade.
- **Item 001/002 (mesma auditoria):** este item não reabre nenhum arquivo tocado por 001/002, exceto indiretamente — nenhuma sobreposição de linha.
- **FIKA:** `AUD-01-15` toca `mods/Skills-Extended/modded/FikaSync/`, que depende de `Fika.Core` — comparar contra `mods/FIKA/modded/Fika-Plugin/Fika.Core/` (fork mantido por este repo) se qualquer dúvida sobre o ciclo de vida de `GameWorld.OnGameStarted` em coop surgir durante a implementação; não foi necessário nesta spec pois o ponto de patch já existe e funciona hoje (só adicionamos uma chamada a mais no corpo).
- **Ordem de inicialização:** `OnGameEndedPatch` (novo) precisa estar registrado no mesmo `PatchManager`/ciclo de ativação dos demais patches do Plugin — sem tratamento especial, mesmo padrão dos demais `ModulePatch` do mod.

## 8. Checklist de implementação

- [x] `HealthEffectComponentPatch.cs` — criar `PerInstanceHealthEffect`, reescrever o Postfix para clonar em vez de mutar, ressincronizar o tooltip via `AddOrReplaceAttribute`/`Item.Replacements`, remover `OriginalCosts`/`InstanceIdsChangedAtLevel`/`ResetLevelChangedAt` (`AUD-01-07`)
- [ ] Validar in-game que o tooltip do item médico mostra o custo já reduzido pela skill FirstAid após o fix (critério de aceite da spec funcional) (`AUD-01-07`) — **pendente de teste do usuário**
- [x] `UpdateWeaponsPatch.cs` — tornar os 4 dicionários `internal static readonly`; adicionar gate `GameUtils.IsInRaid()` no Prefix; remover os dois `.Clear()`; remover as 2 linhas de mutação de `weapon.Template.Ergonomics`; adicionar `ClearRaidState()`; adicionar `TriggerRaidStart()` (`AUD-01-08` parcial, `AUD-01-09`, suporte `AUD-01-14`, `PA-01-01`)
- [x] Criar `ErgonomicsTotalPatch.cs` (`AUD-01-08` parcial)
- [x] Documentar inline em `UpdateWeaponsPatch.cs` a limitação confirmada de Recoil (§1.2) — comentário citando este documento, não silenciar a decisão
- [x] `SkillManagerConstructorPatch.cs` — adicionar `SkillOwners` (`ConditionalWeakTable`) e o registro de `FieldMedicine` no Postfix (`AUD-01-10`)
- [x] Reescrever `AbstractSkillClassSummaryLevelPatch.cs` para consumir `SkillOwners` (`AUD-01-10`)
- [x] `GetBarterPricePatch.cs` — null-guard nos dois Postfix (`AUD-01-11`)
- [x] `OnGameStarted.cs` — `Player` e os 3 handlers (`ApplyMedicalXp`/`ApplyNatoRifleXp`/`ApplyEasternRifleXp`) viram `internal`; `ApplyMedicalXp` ganha `if (Player == null) return;` e reusa `Player` sem `!` (`AUD-01-12`, `PA-01-02`); `Postfix` chama `UpdateWeaponsPatch.TriggerRaidStart()` (`PA-01-01`); criar `OnGameEndedPatch` referenciando os nomes reais dos 3 handlers (`AUD-01-14`, `PA-01-03`)
- [x] `ReflectionHelper.cs` — remover os 7 campos e a resolução correspondente (`AUD-01-13`)
- [x] `FikaSync/Patches/OnGameStartedPatch.cs` — adicionar `LockPickingHelpers.DoorAttempts.Clear()` + `using` novo (`AUD-01-15`)
- [ ] Validar in-game que o bônus de Ergonomia/Recuo da arma já está ativo no primeiro frame da raid, sem precisar abrir nenhuma tela antes (`PA-01-01`) — **pendente de teste do usuário**
- [x] Grep por `.IHealthEffect ==`/`.IHealthEffect !=` fora de `HealthEffectComponentPatch.cs` antes de fechar `AUD-01-07` — confirmado sem ocorrências em `EFT.InventoryLogic`/`EFT.HealthSystem` (`PA-01-04`)
- [x] Verificação residual: buscar por outros overrides de `GameWorld.OnDestroy` além de `ClientGameWorld` — **confirmado fechado**, ver §1.8 (cadeia completa auditada: `ClientLocalGameWorld` não sobrescreve, `ClientNetworkGameWorld` chama `base.OnDestroy()`)
- [ ] Compilar Plugin + Server (Release) com 0 erros antes de considerar o item pronto para `/code-review` — **pendente de `/compile-mod` (fora do escopo do `/code-mod`)**

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Este item **adiciona** o stop hook que faltava: `OnGameEndedPatch` em `GameWorld.OnDestroy` (§1.8, §5.7) — antes deste item o mod só tinha start hooks para os sistemas tocados aqui |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `AUD-01-10` deixa de depender do MainPlayer (resolve o dono real via `SkillOwners`); `AUD-01-08` Ergonomics só aplica a instâncias já filtradas pelo perfil local (`UsecWeaponInstanceIds`, populado só a partir de `profile.Inventory.AllRealPlayerItems`, §1.2). `AUD-01-11`/`AUD-01-12` mantêm `GameUtils.GetSkillManager()`/`Player` propositalmente — são patches em telas/eventos que só disparam para o jogador local (tela de trader aberta localmente, handler de XP assinado só no `Player` local) |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `GameWorld.OnDestroy` é `virtual`; cadeia completa auditada — `ClientGameWorld` (única subclasse direta) chama `base.OnDestroy()`; suas 2 subclasses concretas (`ClientLocalGameWorld` — solo, não sobrescreve; `ClientNetworkGameWorld` — coop/Fika/headless, sobrescreve e chama `base.OnDestroy()`) cobrem os dois caminhos reais de raid. Sem override que quebre a cadeia |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Todas as mudanças usam campos/properties públicos já expostos pelo próprio EFT (`HealthEffectsComponent.IHealthEffect` é campo público settable; `Weapon.ErgonomicsTotal` é property patcheável via Harmony) — nenhum acesso a memória não-canônico introduzido |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `OnGameEndedPatch` (Postfix em `GameWorld.OnDestroy`, que roda independente do motivo de saída de raid) desinscreve os 3 eventos e limpa os caches de `UpdateWeaponsPatch`; `AUD-01-15` limpa `DoorAttempts` a cada `OnGameStarted` novo (idempotente, não depende do encerramento anterior ter rodado corretamente) |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Este item não introduz nem altera nenhuma `ConfigEntry` |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhum patch deste item invoca de volta o método que ele mesmo patcheia |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | `UsecWeaponInstanceIds`/`EasternWeaponInstanceIds` continuam validando o nível de skill atual por `item.Id` antes de pular reprocessamento (mecanismo já existente, preservado); `ErgonomicsTotalPatch` consulta esse mesmo cache a cada leitura, nunca assume um valor calculado antes da troca |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | Todas as refs a `arquivo.cs:linha` desta spec foram lidas nesta sessão diretamente do dump (não reaproveitadas cegamente do relatório de auditoria) — `HealthEffectsComponent.cs`, `Weapon.cs`, `AbstractSkillClass.cs`, `SkillClass.cs`, `SkillManager.cs`, `GameWorld.cs`, `ClientGameWorld.cs`, `ActiveHealthController.cs`, `TraderAssortmentControllerClass.cs`, `IHealthEffect.cs`, `GClass1443.cs`, `IItemOwner.cs`, `GInterface392.cs`, `Item.cs` |
| 10 | Skill EFT usada como lever confirmada **não-inerte** (`SkillsSettings` ≠ `[]` no `globals.json`); se inerte, efeito entregue por patch direto — AP-10 | N/A | Os buffs aqui são todos do próprio `SkillManagerExtended` (mecânica custom do mod), não uma skill nativa do EFT usada como lever |
| 11 | Pacote FIKA próprio: envelope de comprimento + só `TryGet*` + flag `Valid`, campos resetados no `Deserialize`, envio só na main thread, registro por instância/evento (nunca `bool`), zero `UnregisterPacket`, airbag com throttle em todo callback — AP-11 | N/A | `AUD-01-15` só adiciona uma chamada a um método já existente (`DoorAttempts.Clear()`) dentro de um patch FIKA já registrado — nenhum pacote `INetSerializable` novo é criado ou modificado |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-07 | Decisão do usuário sobre os 2 pontos complexos: `AUD-01-07` investigado mais a fundo (tooltip ressincronizado via `AddOrReplaceAttribute`, sem regressão); `AUD-01-08` metade Recoil aceita como débito técnico (mutação de template preservada) |
| 2026-09-07 | `/review-technical-spec` rodada 01 — 4 achados (`PA-01-01` a `PA-01-04`), todos aceitos e aplicados: `TriggerRaidStart()` em `UpdateWeaponsPatch` chamado por `OnGameStartedPatch.Postfix`; guard de nulidade em `ApplyMedicalXp`; `OnGameEndedPatch` corrigido para referenciar os nomes reais dos handlers; verificação de `IHealthEffect` por referência adicionada ao checklist |
