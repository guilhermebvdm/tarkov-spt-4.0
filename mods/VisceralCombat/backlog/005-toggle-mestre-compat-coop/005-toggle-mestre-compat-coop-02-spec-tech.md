# 005 — Toggle Mestre (F12) com Núcleo de Compatibilidade Sempre Ativo em Coop · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [005-toggle-mestre-compat-coop-01-spec.md](005-toggle-mestre-compat-coop-01-spec.md)
**Criado:** 2026-09-20

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** snapshot de 2026-09-20 (Sessão 13), `mods/VisceralCombat/memory/sessions.md`. **Pendências que afetam este item:** `[P-10.2]` (🟡 — ver §1, resolvida pela mesma investigação que já embasa a decisão de gate em `CreateBSGRagdollPatch`/`RagdollClassPatch`), `[P-10.3]` (🟡 — mecanismo de `DropCorpseHeadEquipment` desativado, não afeta este item diretamente, mas a mesma classe `LimbKillPatch.cs` é tocada aqui). Nenhuma pendência 🔴. **Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (sempre — AP-01, AP-02, AP-05 relevantes), `fika-packet-desync-prevention-plan.md` (não disparado — nenhum pacote novo).

Esta feature é **quase inteiramente código do mod** (tier 🥈/🥉 da hierarquia de evidência) — não introduz nenhum hook novo no Assembly do EFT. O trabalho é: (a) uma `ConfigEntry<bool>` nova + um helper de composição em `VisceralEntry.cs`, e (b) inserir uma checagem dessa nova config em ~19 patches Harmony já existentes do próprio mod. Onde há referência ao Assembly, é só pra confirmar o comportamento de fallback de `CreateBSGRagdollPatch`/`RagdollClassPatch` (já investigado nesta sessão, ver `mods/VisceralCombat/memory/sessions.md` Sessão 11).

## 1. Estratégia

**Uma `ConfigEntry<bool> VisceralCombatEnabled`** nova, categoria `"General"`, default `true`, ligada em `VisceralEntry.Awake()` **antes** de qualquer outra `Config.Bind` (linha de referência: logo antes da atual primeira bind, `VisceralEntry.cs:182`). Ela é lida em tempo de execução (não só no boot) por cada patch relevante — nunca cacheada — porque o critério de aceite da spec funcional exige que alternar no F12 durante uma raid tenha efeito imediato (ver §7 sobre a decisão de não bloquear a troca em tempo real).

Pra evitar duplicar `VisceralCombatEnabled.Value && Xxx.Value` em toda parte, um **helper de instância** novo em `VisceralEntry.cs`:

```csharp
public bool IsCategoryActive(ConfigEntry<bool> categoryToggle) => VisceralCombatEnabled.Value && categoryToggle.Value;
```

Auditoria completa dos 23 patches registrados em `Awake()` (`VisceralEntry.cs:249-256, 286-300`) — feita nesta sessão, ver detalhamento em §2 — resultou em **3 grupos**, mapeando 1:1 com as 3 camadas já decididas na spec funcional:

- **Camada 3 (sempre ativo, nunca gated):** `WeaponDropOnDeathSkipVanillaFlingPatch` (núcleo anti-fantasma, já não depende de nenhuma config — nada a mudar) + 3 patches de segurança contra crash (`PlaySoundBankPatch`, `PlayStepSoundPatch`, `DefaultPlayPatch` — evitam `NullReferenceException` tocando som em player morto/caído; não produzem nenhum efeito visual, e desligá-los reintroduziria os NREs que eles existem pra prevenir) + as duas `GameStartedPatch` (limpeza de estado estático entre raids — precisa rodar sempre, senão dicionários como `PendingHeadOutcome`/`_evaluatedLivingVolleys` vazam entre raids independente do toggle).
- **Camada 2 (toggle próprio, fora do mestre):** `DeathInventoryDropPatch` — já tem os dois toggles próprios (`DropWeaponOnDeath`, `DropHeadEquipmentOnDismemberment`), nenhuma mudança de gate necessária ali. **Mudança real necessária:** remover o acoplamento com `EnableDismemberment.Value` que hoje existe em `ResolveAndDropHeadEquipment` (ver achado abaixo) — a arma (`DropHandsWeapon`) já nunca teve esse acoplamento, então já está correta como está, mas pelo MESMO motivo (ver "Por que isso precisa ser assim" abaixo).
- **Camada 1 (atrás do mestre):** os ~19 patches restantes, cada um recebendo `IsCategoryActive(...)` (se já tem uma config de categoria) ou `VisceralCombatEnabled.Value` puro (se nunca teve gate nenhum) — ver tabela completa em §2.

**Por que a queda de arma E de capacete/óculos precisam ficar fora do toggle mestre — não é só estética, é a MESMA classe de bug que `CR-NET-LOCK-01` já resolveu (decisão do usuário confirmada na revisão técnica 01, `PA-01-02`):** as duas são decididas e executadas por quem for **autoritativo** pra aquela morte específica (o host, pra bots; o próprio jogador, pra sua própria morte — mesmo gate de `DeathInventoryDropPatch.cs:75`, compartilhado pelas duas) — e essa execução já é uma mutação real de inventário (`ThrowItem`), replicada pra todo o raid via o canal nativo de operação do EFT/Fika (`CR-NET-LOCK-01`). Se essa remoção ficasse condicionada ao toggle mestre de **quem está executando** — por exemplo, um jogador com desmembramento ativo mata/desmembra um bot que ele mesmo hospeda, mas o toggle relevante dele (efetivamente "desligado" nesse instante por causa do mestre) impedisse o `ThrowItem` de rodar — o item ficaria **corretamente removido só pra quem tem a feature ativa, mas continuaria presente no inventário do cadáver pra quem não tem** (outro jogador do mesmo raid, olhando o mesmo cadáver) — exatamente o padrão "item em 2 lugares (chão + inventário do morto)" que motivou a reformulação inteira do item 002. Isso vale igual pra arma (`DropWeaponOnDeath`) e pra capacete/óculos (`DropHeadEquipmentOnDismemberment`) — nenhum dos dois pode ficar atrás de `EnableDismemberment` nem do mestre, e é por isso que a Camada 2 existe como uma camada própria, não uma exceção pontual só do capacete. Não é uma escolha de UX (embora o mod já tenha um precedente parecido em `ShootOffHelmetPatch`, que derruba capacete sem nenhum efeito visual de cabeça) — é uma exigência de consistência de dado entre peers.

**Achado que resolve o corner case da spec funcional ("capacete/óculos independente do mestre depende de Dismemberment Enabled também ficar independente"):** confirmado em [`DeathInventoryDropPatch.cs:156`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs#L156) — `ResolveAndDropHeadEquipment` hoje checa `!VisceralEntry.Instance.EnableDismemberment.Value` antes de derrubar o equipamento, um acoplamento que `DropHandsWeapon` (arma) nunca teve. Como `EnableDismemberment` vira `IsCategoryActive(EnableDismemberment)` (atrás do mestre) em todo resto do mod, deixar essa linha como está faria o capacete/óculos parar de cair sempre que o mestre estiver desligado — exatamente o problema descrito acima. **Resolução:** remover essa linha inteiramente. A rolagem de `ResolveHeadOutcome` (decisão de arranca/estoura/nenhum) já roda incondicionalmente hoje (não depende de `EnableDismemberment` — só o efeito visual em `KillPatch.Postfix` depende); a queda de equipamento passa a depender só do próprio toggle `DropHeadEquipmentOnDismemberment.Value`, igual à arma. O efeito visual (`DismemberLimb` encolhendo a cabeça) continua gated por `IsCategoryActive(EnableDismemberment)` em `KillPatch.Postfix` — só a **queda de item** se desacopla do **efeito visual**.

**Achado sobre `CreateBSGRagdollPatch`/`RagdollClassPatch` (confirma o corner case já registrado na spec funcional, ligado a `[P-10.2]`):** os dois retornam `bool` de um `[PatchPrefix]` e **substituem o método original inteiro** (`Corpse.method_16`/`RagdollClass.Start()`) incondicionalmente — [`CreateBSGRagdollPatch.cs:27`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs#L27) e [`RagdollClassPatch.cs:23`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs#L23). Não existe hoje nenhum caminho de fallback pro ragdoll vanilla. Pra "desligar o mestre" realmente significar "ragdoll volta a ser o vanilla" (comportamento desejado da spec funcional), o `Prefix` de cada um precisa de um `return true;` **logo no topo**, antes de qualquer outra lógica — diferente do padrão `return;`/early-return usado no resto do mod, porque aqui `true` significa "deixa o método original do EFT rodar" (mesma semântica de qualquer `[PatchPrefix]` que retorna `bool`).

**Alternativas descartadas:**
- Gate central em `VisceralEntry.Update()`/`OnGUI` desligando os patches via `Harmony.Unpatch` dinamicamente — descartado: reflection cara pra rodar todo frame, e o comportamento de "voltar pro vanilla instantaneamente" com `return true;` já cobre o caso sem custo de runtime extra.
- Um único patch "master gate" no topo da cadeia de eventos (ex.: interceptar `GameWorld.OnGameStarted` e desativar tudo dali) — descartado: não cobriria a troca em tempo real durante a raid (critério de aceite explícito da spec funcional), já que os efeitos individuais (som, sangue, ragdoll) disparam de pontos de entrada diferentes do Assembly, não de um único hook central.

## 2. Pontos de patch

Nenhum ponto de patch NOVO no Assembly — todos os 23 patches já existem e já têm seu alvo Harmony resolvido (auditado nesta sessão, ver Referências). A tabela abaixo mapeia cada um à mudança necessária.

| Arquivo (mod) | Camada | Mudança | Linha de inserção |
|---|---|---|---|
| `VisceralCombat/VisceralEntry.cs` | — | Nova `ConfigEntry<bool> VisceralCombatEnabled` + `IsCategoryActive(...)` helper | Bind antes de `:182`; propriedade + helper perto de `:103` |
| `VisceralCombat.Combined.Patches/KillPatch.cs` | 1 — Dismemberment | `EnableDismemberment.Value` → `IsCategoryActive(EnableDismemberment)` | [`:358`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L358) |
| `VisceralCombat.Combined.Patches/KillPatch.cs` — **achado na revisão técnica 01 (`PA-01-01`)** | 1 — Ragdoll Character Properties | `UseActiveRagdolls.Value` → `IsCategoryActive(UseActiveRagdolls)` (2 ocorrências — `OnlyPlayersCanActiveRagdollEnemies.Value` fica como está, já `&&`-encadeada na mesma condição, coberta transitivamente) | [`:319`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L319), [`:409`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L409) |
| `VisceralCombat.Combined.Patches/KillPatch.cs` — **achado na revisão técnica 01 (`PA-01-01`)** | 1 — Blood | `EnableBloodEffects.Value` → `IsCategoryActive(EnableBloodEffects)` (2 ocorrências, dentro de `SpawnOldVolumetricBlood` e `SpawnArterialSprays` — `ArterySpray.Value` na linha `:968` fica como está, já `&&`-encadeada) | [`:821`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L821), [`:968`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L968) |
| `VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | 1 — Dismemberment | `EnableDismemberment.Value` → `IsCategoryActive(EnableDismemberment)` | [`:110`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L110) |
| `VisceralCombat.Dismemberment.Patches/ProneLockPatch.cs` (classe `ProneLockPatch`) | 1 — Dismemberment | Nova checagem `IsCategoryActive(EnableDismemberment)` (hoje sem gate nenhum) | `:22` (topo do Prefix) |
| `VisceralCombat.Dismemberment.Patches/ProneLockPatch.cs` (classe `ProneMoverDoPronePatch`, mesmo arquivo) | 1 — Dismemberment | Idem acima | `:48` (topo do Prefix) |
| `VisceralCombat.Dismemberment.Patches/BleedPatch.cs` | 1 — Blood | `EnableBloodEffects.Value` → `IsCategoryActive(EnableBloodEffects)` (**4 ocorrências, corrigido na revisão técnica 01 de "3" pra "4" — `PA-01-01`**) | `:38`, `:80` (`EnableArmorSparks.Value` junto, mesma troca), `:142`, `:201` |
| `VisceralCombat.Dismemberment.Patches/GameStartedPatch.cs` | 1 — Blood (só o bloco de decal) | `EnableBloodEffects.Value` → `IsCategoryActive(EnableBloodEffects)` | `:37` |
| `VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs` — **arquivo novo, achado na revisão técnica 01 (`PA-01-01`)** | 1 — Blood | `EnableImpactBloodCloud.Value` → `IsCategoryActive(EnableImpactBloodCloud)` | [`:582`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L582) |
| `VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | 1 — Ragdoll Physical Properties | Nova checagem `VisceralCombatEnabled.Value` no topo de `ProcessImpulse` | [`:57`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L57) — **TODO confirmar:** linha exata do primeiro statement dentro do método (não lida nesta sessão em detalhe, só o assinatura) |
| `VisceralCombat.Ragdolls.Patches/CreateCorpsePatch.cs` | 1 — Ragdoll Physical Properties | Nova checagem `VisceralCombatEnabled.Value` (hoje sem gate nenhum) | `:32` (topo do Postfix) |
| `VisceralCombat.Ragdolls.Patches/GrenadeDeadBodiesPatch.cs` | 1 — Ragdoll Physical Properties | Nova checagem `VisceralCombatEnabled.Value` (hoje só checa `VisceralEntry.Instance == null`) | `:20` |
| `VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs` | 1 — Character Properties | `ShootHelmetOff.Value` → `IsCategoryActive(ShootHelmetOff)` | [`:29`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs#L29) |
| `VisceralCombat.Ragdolls.Patches/PlayerInitPatch.cs` | 1 — Character Properties | Nova checagem no topo do Postfix (antes do `ContinueWith` ser agendado, não só dentro dele) | `:19` |
| `VisceralCombat.Ragdolls.Patches/AttachWeaponPatch.cs` | 1 — Character Properties | Nova checagem `VisceralCombatEnabled.Value` (hoje sem gate nenhum) | `:18` |
| `VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs` | 1 — Character Properties (caso especial) | `if (!VisceralEntry.Instance.VisceralCombatEnabled.Value) return true;` no topo — fallback pro vanilla, não um early-return comum | [`:27`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs#L27) |
| `VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs` | 1 — Character Properties (caso especial) | Idem acima | [`:23`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs#L23) |
| `VisceralCombat.Combat.Patches/ShellCasingPatch.cs` | 1 — Combat \| Visuals | `NeverDeleteShells.Value` → `IsCategoryActive(NeverDeleteShells)` | [`:28`](../../modded/VisceralCombat/VisceralCombat.Combat.Patches/ShellCasingPatch.cs#L28) |
| `VisceralCombat.Ragdolls.Patches/GrenadeItemsPatch.cs` | 1 — Physics \| Item Physical Properties | `ItemForce.Value` → `IsCategoryActive(ItemForce)` | `:21` |
| `VisceralCombat.Ragdolls.Patches/PhysicalItemsPatch.cs` | 1 — Physics \| Item Physical Properties | `ItemForce.Value` → `IsCategoryActive(ItemForce)` | [`:23`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PhysicalItemsPatch.cs#L23) |
| `VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs` | 2 (independente) | Remover a checagem `!VisceralEntry.Instance.EnableDismemberment.Value` de `ResolveAndDropHeadEquipment` (ver achado em §1) | [`:156`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs#L156) — linha a **remover**, não a modificar |
| `VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs` (classe `WeaponDropOnDeathSkipVanillaFlingPatch`) | 3 (sempre ativo) | Nenhuma mudança — já não depende de config nenhuma | — |
| `VisceralCombat.Combined.Patches/PlaySoundBankPatch.cs`, `PlayStepSoundPatch.cs`, `DefaultPlayPatch.cs` | 3 (sempre ativo) | Nenhuma mudança — são proteção contra crash em player morto/caído, não efeito visual | — |
| `VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs`, `VisceralCombat.Dismemberment.Patches/GameStartedPatch.cs` | 3 (sempre ativo — limpeza de raid) | Nenhuma mudança na limpeza em si (só o bloco de decal de `EnableBloodEffects` já listado acima como Camada 1) | — |
| `VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` — setup de colisão de camada física | 1 — Ragdoll Physical Properties / Character Properties (caso especial, ver nota) | `BodyCollision.Value` → `IsCategoryActive(BodyCollision)`; `UseActiveRagdolls.Value` → `IsCategoryActive(UseActiveRagdolls)` (2 ocorrências) | `:48`, `:52`, `:62` — **nota:** roda uma única vez por `GameWorld.OnGameStarted`, não por evento recorrente. Alternar o toggle mestre em tempo real durante a raid **não afeta essa configuração de colisão até a próxima raid começar** — exceção pequena e aceitável à regra geral de "troca em tempo real" da Camada 1, decorrente do próprio hook do EFT (dispara uma vez só), não de uma limitação evitável do design |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `General` | `Visceral Combat Enabled` | bool | `true` | — | — | Liga/desliga todas as features visuais e de gameplay pesadas do mod (desmembramento, sangue, ragdoll customizado, efeitos de item físico) de uma vez. A queda de arma e a queda de capacete/óculos continuam funcionando mesmo desligado — cada uma com o próprio toggle — pra sincronizar corretamente com outros jogadores do raid que estejam com o mod ativo. |

> Posicionamento visual (aparecer no topo da lista do F12): a categoria `"General"` não é alfabeticamente a primeira frente a `Blood`/`Combat`/`Dismemberment`/`Physics`/`Ragdolls`. Se o BepInEx ConfigurationManager ordenar categorias alfabeticamente por padrão neste ambiente, avaliar `ConfigurationManagerAttributes.Order` (já usado em `VisceralEntry.cs` para outras entradas, ex. `EnableDismemberment` com `Order = 3`) durante o `/code-mod` e confirmar visualmente em jogo — detalhe cosmético, não bloqueia a lógica.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | MODIFICAR | Nova `ConfigEntry<bool> VisceralCombatEnabled` + propriedade + helper `IsCategoryActive(...)` + bump de versão |
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | MODIFICAR | `IsCategoryActive(EnableDismemberment)` em `:358`; `IsCategoryActive(UseActiveRagdolls)` em `:319`/`:409`; `IsCategoryActive(EnableBloodEffects)` em `:821`/`:968` (achados `PA-01-01`) |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | MODIFICAR | `IsCategoryActive(EnableDismemberment)` em `:110` |
| `modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/ProneLockPatch.cs` | MODIFICAR | 2 classes no mesmo arquivo, cada uma ganha o gate (`:22` e `:48`) |
| `modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs` | MODIFICAR | `IsCategoryActive(EnableBloodEffects)` em 4 pontos (`:38`, `:142`, `:201`) + `IsCategoryActive(EnableArmorSparks)` em `:80` (achado `PA-01-01`, contagem corrigida de 3 pra 4) |
| `modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/GameStartedPatch.cs` | MODIFICAR | `IsCategoryActive(EnableBloodEffects)` no bloco de decal |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs` | MODIFICAR | `IsCategoryActive(EnableImpactBloodCloud)` em `:582` (arquivo novo, achado `PA-01-01`) |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | MODIFICAR | Gate no topo de `ProcessImpulse` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateCorpsePatch.cs` | MODIFICAR | Gate novo no topo do Postfix |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GrenadeDeadBodiesPatch.cs` | MODIFICAR | Gate novo logo após o `VisceralEntry.Instance == null` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs` | MODIFICAR | `IsCategoryActive(ShootHelmetOff)` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerInitPatch.cs` | MODIFICAR | Gate no topo do Postfix, antes do `ContinueWith` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/AttachWeaponPatch.cs` | MODIFICAR | Gate novo no topo do Postfix |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` (setup de colisão) | MODIFICAR | `IsCategoryActive(BodyCollision)` em `:48`, `IsCategoryActive(UseActiveRagdolls)` em `:52`/`:62` (achado `PA-01-03` — só afeta a próxima raid, não a atual) |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs` | MODIFICAR | `return true;` de fallback no topo do Prefix |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs` | MODIFICAR | `return true;` de fallback no topo do Prefix |
| `modded/VisceralCombat/VisceralCombat.Combat.Patches/ShellCasingPatch.cs` | MODIFICAR | `IsCategoryActive(NeverDeleteShells)` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GrenadeItemsPatch.cs` | MODIFICAR | `IsCategoryActive(ItemForce)` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PhysicalItemsPatch.cs` | MODIFICAR | `IsCategoryActive(ItemForce)` |
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs` | MODIFICAR | Remover acoplamento com `EnableDismemberment` em `ResolveAndDropHeadEquipment` |
| `mods/VisceralCombat/PROPRIEDADES.md` | MODIFICAR | Documentar `Visceral Combat Enabled` (nova seção `General`, no topo do arquivo) |

## 5. Stubs de código

### 5.1 `VisceralEntry.cs` — nova config + helper

```csharp
// Propriedade nova, junto das demais ConfigEntry<bool> (perto de VisceralEntry.cs:103)
public ConfigEntry<bool> VisceralCombatEnabled { get; set; }

// Helper de composição — reduz "VisceralCombatEnabled.Value && Xxx.Value" repetido em
// ~15 patches pra uma única chamada. Lido em tempo de execução a cada invocação do patch
// (nunca cacheado), pra alternar no F12 valer imediatamente (critério de aceite da spec
// funcional — troca em tempo real durante raid).
public bool IsCategoryActive(ConfigEntry<bool> categoryToggle)
{
    return VisceralCombatEnabled != null && VisceralCombatEnabled.Value
        && categoryToggle != null && categoryToggle.Value;
}
```

```csharp
// Awake() — bind ANTES de qualquer outra Config.Bind (antes da atual primeira, EnableDismemberment,
// VisceralEntry.cs:182), pra ficar semanticamente "primeiro" mesmo que a ordem visual no F12
// dependa de ConfigurationManagerAttributes.Order (ver §3).
VisceralCombatEnabled = ((BaseUnityPlugin)this).Config.Bind<bool>(
    "General",
    "Visceral Combat Enabled",
    true,
    "Liga/desliga todas as features visuais e de gameplay pesadas do mod (desmembramento, sangue, ragdoll customizado, efeitos de item fisico) de uma vez. A queda de arma e a queda de capacete/oculos continuam funcionando mesmo desligado, pra sincronizar corretamente com outros jogadores do raid que estejam com o mod ativo.");
```

### 5.2 `CreateBSGRagdollPatch.cs` / `RagdollClassPatch.cs` — fallback pro vanilla

```csharp
// ref: Assembly-CSharp/EFT.Interactive/Corpse.cs:225 (Corpse.method_16) — este Prefix substitui
// o método inteiro (sempre retorna false hoje). Pra "desligar o mestre = ragdoll vanilla",
// precisa de um return true ANTES de qualquer outra lógica, não um early-return comum — aqui
// "true" tem semântica invertida (deixa o EFT original rodar), diferente de todo resto do mod.
[PatchPrefix]
private static bool Prefix(Corpse __instance, bool forceStill = false)
{
    if (VisceralEntry.Instance != null && !VisceralEntry.Instance.VisceralCombatEnabled.Value)
    {
        return true; // toggle mestre desligado — usa o ragdoll nativo do EFT
    }

    PlayerBody playerBody = _playerBodyField?.GetValue(__instance) as PlayerBody ?? __instance.GetComponentInChildren<PlayerBody>();
    // ... resto do método inalterado (já existente) ...
}
```

O mesmo padrão (`if (VisceralEntry.Instance != null && !VisceralEntry.Instance.VisceralCombatEnabled.Value) return true;` como a primeira linha do `Prefix`) se aplica idêntico em `RagdollClassPatch.cs:23`.

### 5.3 `DeathInventoryDropPatch.cs` — desacoplar queda de capacete/óculos de `EnableDismemberment`

```csharp
// ANTES (DeathInventoryDropPatch.cs:152-157):
if (outcome == KillPatch.HeadDismemberOutcome.None) return;
if (!VisceralEntry.Instance.EnableDismemberment.Value) return;   // <- REMOVER esta linha
if (!VisceralEntry.Instance.DropHeadEquipmentOnDismemberment.Value) return;

// DEPOIS:
if (outcome == KillPatch.HeadDismemberOutcome.None) return;
if (!VisceralEntry.Instance.DropHeadEquipmentOnDismemberment.Value) return;
```

### 5.4 Padrão mecânico aplicado nos demais patches "Camada 1" já gated

```csharp
// ANTES (ex.: KillPatch.cs:358):
if (!VisceralEntry.Instance.EnableDismemberment.Value)
{
    return;
}

// DEPOIS:
if (!VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.EnableDismemberment))
{
    return;
}
```

Mesmo padrão pra `EnableBloodEffects` (`BleedPatch.cs`, `GameStartedPatch.cs`), `ShootHelmetOff` (`ShootOffHelmetPatch.cs`), `NeverDeleteShells` (`ShellCasingPatch.cs`), `ItemForce` (`GrenadeItemsPatch.cs`, `PhysicalItemsPatch.cs`) — trocar `VisceralEntry.Instance.Xxx.Value` isolado por `VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.Xxx)` no `if` existente, sem mudar a estrutura do resto do método.

### 5.5 Padrão mecânico pros patches "Camada 1" sem gate nenhum hoje

```csharp
// Inserir como a PRIMEIRA linha do corpo do Prefix/Postfix (ex.: ProneLockPatch.cs:22,
// CreateCorpsePatch.cs:32, GrenadeDeadBodiesPatch.cs:20 — após o "VisceralEntry.Instance == null"
// já existente lá —, AttachWeaponPatch.cs:18, PlayerInitPatch.cs:19, BodiesImpulsePatch.cs
// topo de ProcessImpulse):
if (VisceralEntry.Instance == null || !VisceralEntry.Instance.VisceralCombatEnabled.Value) return;
```

## 6. Fluxo de dados

```
[A] Jogador marca/desmarca "Visceral Combat Enabled" no F12 (BepInEx ConfigurationManager,
    ConfigEntry<bool>.Value muda em memória imediatamente) OU o launcher TRL Red Line escreve o
    mesmo valor no arquivo de config do BepInEx ANTES do jogo abrir (jogo lê o arquivo uma vez em
    Config.Bind, dentro de Awake — VisceralEntry.cs:177)
  → [B] próxima vez que qualquer um dos ~19 patches da Camada 1 dispara (morte de bot, tiro,
    passo, explosão, etc.) — cada um lê VisceralCombatEnabled.Value em tempo real via
    IsCategoryActive(...) ou a checagem direta (§5.5)
    → [C] se desligado: patch retorna cedo (ou, no caso de CreateBSGRagdollPatch/RagdollClassPatch,
      retorna true e deixa o método original do EFT rodar) — nenhum efeito da Camada 1 acontece
    → [C'] se ligado: patch continua normalmente, comportamento idêntico ao que já existe hoje
  → [D] independente de [A], DeathInventoryDropPatch (Camada 2) e
    WeaponDropOnDeathSkipVanillaFlingPatch (Camada 3) continuam rodando pelos próprios critérios
    — a arma e o capacete/óculos caem (se os toggles próprios estiverem ligados) e o núcleo
    anti-fantasma sempre suprime a animação vanilla redundante, garantindo que outro jogador do
    raid com o mod ativo nunca veja duplicata/fantasma independente do estado de [A] neste cliente
```

## 7. Riscos e dependências

- **Troca em tempo real durante raid (decisão da spec funcional):** como cada patch lê `.Value` a cada invocação (nunca cacheado), alternar o toggle no F12 no meio de uma raid tem efeito **imediato** no próximo evento (próximo tiro, próxima morte) — sem exigir reiniciar a raid. Isso satisfaz a preferência do usuário registrada na spec funcional §"Corner cases" sem precisar da alternativa de bloqueio ("só na próxima raid"), porque nenhum patch mantém estado de "operação em andamento" que sobreviva além de uma única invocação — o único caso levantado como risco (ragdoll customizado abandonado a meio caminho) não se aplica aqui porque `CreateBSGRagdollPatch`/`RagdollClassPatch` só decidem vanilla-vs-custom **na criação** do ragdoll (não há um ragdoll "em andamento" que troque de comportamento no meio do processo — uma vez criado como customizado, ele termina como customizado; a checagem só afeta o PRÓXIMO corpo a ser criado).
- **`IsCategoryActive` como ponto único de composição:** centraliza a lógica "mestre E categoria" num só lugar — se o comportamento precisar mudar no futuro (ex.: adicionar uma terceira camada de composição), só esse método muda, não os ~15 call sites.
- **Patches com `TODO confirmar`:** `BodiesImpulsePatch.cs` — a linha exata do primeiro statement dentro de `ProcessImpulse` (`:57` é só a assinatura) não foi lida em detalhe nesta sessão (delegada a um subagente de pesquisa que reportou os números de linha dos usos de config **dentro** do método, não a primeira linha executável). Confirmar durante `/code-mod` antes de inserir o gate.
- **Patches deliberadamente fora de escopo (Camada 3):** `PlaySoundBankPatch`/`PlayStepSoundPatch`/`DefaultPlayPatch` existem especificamente para **prevenir** `NullReferenceException` ao tocar som em players mortos/caídos (checam `HealthController.IsAlive`/`IsPlayerDowned`, não produzem nenhum efeito visual próprio) — gateá-los pelo toggle mestre reintroduziria os crashes que eles existem para evitar. Confirmado por leitura direta nesta sessão (via subagente).
- **Achado de `[P-10.2]` fica resolvido tecnicamente por este item:** a pendência registrada na memória ("patches de ragdoll sem gate de config nenhum hoje") é exatamente o que `CreateBSGRagdollPatch.cs:27`/`RagdollClassPatch.cs:23` resolvem aqui — não precisa de item de backlog separado.
- **Sem dependência de pacote FIKA novo:** toda a feature é local a cada cliente (cada jogador lê sua própria config) — nenhuma sincronização de rede é necessária além do que já existe (o núcleo anti-fantasma da Camada 3, que não muda neste item).
- **Compatibilidade com outros mods:** nenhuma interação conhecida — `ConfigEntry<bool>` nova não colide com nenhuma seção/chave existente (`"General"` não é usada por nenhuma `ConfigEntry` atual do mod, confirmado via grep em `VisceralEntry.cs`).

## 8. Checklist de implementação

- [x] **Verificação de completude (achado `PA-01-01`, obrigatório antes de codar):** rodar `grep -rn "VisceralEntry\.Instance\.<Propriedade>\.Value" mods/VisceralCombat/modded/` pra CADA uma das 13 propriedades booleanas de categoria (`EnableDismemberment`, `EnableBloodEffects`, `UseActiveRagdolls`, `ShootHelmetOff`, `NeverDeleteShells`, `ItemForce`, `BodyCollision`, `DisableRagdollsAfterTime`, `OnlyPlayersCanActiveRagdollEnemies`, `EnableImpactBloodCloud`, `EnableArmorSparks`, `UseOldBloodDecal`, `ArterySpray`) e conferir CADA resultado contra a tabela §2 antes de começar a editar — a tabela já foi corrigida uma vez nesta revisão por não ter feito essa varredura completa logo de início; não repetir o erro. **Refeito ao final do build:** grep zerou pras 9 propriedades com gate direto; `OnlyPlayersCanActiveRagdollEnemies` (`KillPatch.cs:321,413`) e `UseOldBloodDecal` (`:860`) ficaram raw mas já herdam o gate por estarem dentro do `&&`/bloco de um check já convertido (`UseActiveRagdolls`/`EnableBloodEffects`) — não precisam de gate próprio. `ArterySpray` idem (combinado com `EnableBloodEffects` nas duas ocorrências). Achado extra fora da tabela original: `PlayerInitPatch.cs:23` também usava `UseActiveRagdolls.Value` raw (não estava na tabela §2) — convertido para `IsCategoryActive` durante o build, além do gate novo de topo exigido pela linha da tabela.
- [x] `VisceralEntry.cs`: adicionar `VisceralCombatEnabled` (propriedade + bind) e `IsCategoryActive(...)` (§5.1).
- [x] `KillPatch.cs:358`, `LimbKillPatch.cs:110`: trocar `EnableDismemberment.Value` por `IsCategoryActive(EnableDismemberment)` (§5.4).
- [x] `KillPatch.cs:319`, `:409`: trocar `UseActiveRagdolls.Value` por `IsCategoryActive(UseActiveRagdolls)` (achado `PA-01-01`).
- [x] `KillPatch.cs:821`, `:968`: trocar `EnableBloodEffects.Value` por `IsCategoryActive(EnableBloodEffects)` (achado `PA-01-01`).
- [x] `ProneLockPatch.cs` (2 classes, `:22` e `:48`): inserir gate novo (§5.5).
- [x] `BleedPatch.cs` (4 ocorrências: `:38`, `:80` `EnableArmorSparks`, `:142`, `:201`), `GameStartedPatch.cs` (bloco de decal): trocar por `IsCategoryActive(...)` (§5.4, contagem corrigida `PA-01-01`).
- [x] `RagdollHelperClass.cs:582`: trocar `EnableImpactBloodCloud.Value` por `IsCategoryActive(EnableImpactBloodCloud)` (achado `PA-01-01`, arquivo novo).
- [x] `GameStartedPatch.cs` (setup de colisão, `:48`, `:52`, `:62`): trocar `BodyCollision.Value`/`UseActiveRagdolls.Value` por `IsCategoryActive(...)` (achado `PA-01-03` — lembrar que só afeta a próxima raid, não a atual).
- [x] `BodiesImpulsePatch.cs`: confirmar linha exata do topo de `ProcessImpulse` (TODO da §7) e inserir gate (§5.5). **Resolvido:** `:57` é só a assinatura; a lógica de item-solto (`ItemForce`, já com config próprio) foi convertida para `IsCategoryActive`, e um gate novo raw (`VisceralCombatEnabled.Value`) foi inserido antes do bloco de impulso/wake de cadáver (que não tinha config nenhum).
- [x] `CreateCorpsePatch.cs:32`, `GrenadeDeadBodiesPatch.cs:20`, `AttachWeaponPatch.cs:18`, `PlayerInitPatch.cs:19`: inserir gate novo (§5.5).
- [x] `ShootOffHelmetPatch.cs:29`: trocar `ShootHelmetOff.Value` por `IsCategoryActive(ShootHelmetOff)` (§5.4).
- [x] `CreateBSGRagdollPatch.cs:27`, `RagdollClassPatch.cs:23`: inserir fallback `return true;` (§5.2).
- [x] `ShellCasingPatch.cs:28`: trocar `NeverDeleteShells.Value` por `IsCategoryActive(NeverDeleteShells)` (§5.4).
- [x] `GrenadeItemsPatch.cs:21`, `PhysicalItemsPatch.cs:23`: trocar `ItemForce.Value` por `IsCategoryActive(ItemForce)` (§5.4).
- [x] `DeathInventoryDropPatch.cs:156`: remover a linha `if (!VisceralEntry.Instance.EnableDismemberment.Value) return;` (§5.3).
- [x] `PROPRIEDADES.md`: nova seção `General` no topo, documentando `Visceral Combat Enabled`; adicionar 3 frases explicando por que "Helmet Knock Off Chance" (item 002, `CR-04-01`) e a exceção arma/capacete deste item existem por motivo de sincronização, não UX.
- [x] Compilar via `.agents/scripts/compile-mod.sh VisceralCombat` — build OK, 0 erros. **Nota:** o script instalou automaticamente em `E:/Tarkov Red Line/BepInEx/plugins/VisceralCombat` (comportamento padrão do script, ver aviso no relatório final do `/code-mod`).
- [ ] Validar em jogo: toggle desligado → nenhum efeito das 6 categorias, mas arma/capacete/óculos continuam caindo; toggle ligado → comportamento idêntico ao atual (regressão zero); alternar em raid tem efeito imediato no próximo evento.
- [ ] Validar em coop: host com toggle desligado observando outro jogador com toggle ligado (e vice-versa) — sem fantasma/duplicata de item, divergência visual esperada nos outros efeitos.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Feature não aloca estado estático raid-scoped novo; `VisceralCombatEnabled` é uma config persistente (não raid-scoped), lida em tempo real sem cache. Os patches de limpeza de raid existentes (`GameStartedPatch` × 2) não mudam de comportamento — continuam sempre ativos (Camada 3), como já documentado em §1 |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Este item não adiciona nenhum novo ponto de reação a ação de player — só insere uma checagem de config adicional em patches já existentes, cujo filtro de autoridade/Fika (quando aplicável) já está implementado e não é tocado aqui |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum alvo Harmony novo é introduzido — todos os 19 patches tocados já existem e já tiveram seus alvos resolvidos em specs técnicas anteriores (itens 001-004) |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | Este item não muda NENHUM estado do jogo/EFT — só controla se código do mod (já existente) roda ou não, via leitura de `ConfigEntry<bool>.Value` |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `VisceralCombatEnabled` é uma `ConfigEntry<bool>` persistente via BepInEx — mesmo mecanismo de qualquer outra config do mod, sobrevive a qualquer forma de saída de raid sem reset. Não há novo estado raid-scoped a limpar |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | `VisceralCombatEnabled`: bool, default `true` (preserva comportamento atual pra quem já usa o mod), sem faixa numérica a ambiguar, tooltip explícito sobre a exceção de arma/capacete (§3) |
| 7 | Reentry-guard em re-invocação de método patcheado — AP-07 | N/A | Nenhum patch novo invoca de volta o método que ele mesmo patcheia; `IsCategoryActive` é um helper puro sem recursão |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | `IsCategoryActive` sempre lê `.Value` fresco a cada chamada — não há cache a invalidar |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Todos os 19 arquivos/linhas citados em §2 foram lidos diretamente nesta sessão (13 pelo agente de pesquisa despachado, 6 já lidos anteriormente pela própria sessão principal — `KillPatch.cs`, `LimbKillPatch.cs`, `ShootOffHelmetPatch.cs`, `CreateBSGRagdollPatch.cs`, `RagdollClassPatch.cs`, `DeathInventoryDropPatch.cs`), não apenas citados por recon. Único ponto sem linha exata confirmada (`BodiesImpulsePatch.cs`, topo de `ProcessImpulse`) está marcado explicitamente como `TODO confirmar` em §2/§7, não inventado |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Feature não usa/buffa nenhuma skill do EFT |
| 11 | Pacote FIKA próprio: envelope, `TryGet*`, `Valid`, registro por instância, zero `UnregisterPacket` — AP-11 | N/A | Este item não introduz nenhum pacote `INetSerializable` novo — cada cliente lê sua própria config local, sem replicação de rede nova |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Spec técnica criada via `/create-technical-spec`. Auditoria completa dos 23 patches registrados em `VisceralEntry.Awake()` (parte via subagente de pesquisa) classificou cada um nas 3 camadas já decididas na spec funcional, e resolveu tecnicamente os 2 corner cases mais importantes dela: o acoplamento capacete/óculos↔`EnableDismemberment` (removido) e a falta de gate em `CreateBSGRagdollPatch`/`RagdollClassPatch` (novo fallback `return true;`, também fecha `[P-10.2]`). |
| 2026-09-20 | `/review-technical-spec` rodada 01: 1 🔴 + 1 🟡 + 1 🟢. Aplicados todos os 3: `PA-01-01` (grep exaustivo achou 7 pontos faltando em §2/§4/§8 — `KillPatch.cs` tinha `UseActiveRagdolls`/`EnableBloodEffects` não cobertos, `BleedPatch.cs` contado errado, `RagdollHelperClass.cs` nunca citado — checklist ganhou passo de verificação obrigatório por grep); `PA-01-02` (justificativa de §1 reescrita — decisão do usuário: a exceção arma/capacete é sobre sincronização de dado entre peers, igual ao `CR-NET-LOCK-01`, não sobre precedente de UX — reforçada explicitamente pro caso da arma também, não só capacete); `PA-01-03` (setup de colisão de `GameStartedPatch` entra na Camada 1, com a ressalva de só valer a partir da próxima raid). |
