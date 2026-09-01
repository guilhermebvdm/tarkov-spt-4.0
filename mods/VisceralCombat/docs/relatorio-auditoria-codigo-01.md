---
title: "Relatório de Auditoria Técnica de Código — Visceral Combat (Review 01)"
date: 2026-08-22
status: 🟢 Vivo
authors:
  - Antigravity
---

# Relatório de Auditoria Técnica de Código — Visceral Combat (Review 01)

Este relatório consolida a **auditoria técnica estática profunda, rigorosa e minuciosa** realizada no código-fonte de [`mods/VisceralCombat/modded/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/), cobrindo as classes, métodos, patches Harmony, ciclo de vida de raid, impacto de garbage collection e compatibilidade cruzada com o **EFT 0.16.9**, **SPT 4.0.13** e **FIKA Coop**.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 2 | Assinatura inexistente no EFT 0.16.9 (`ShootOffHelmetPatch`) e NRE fatal em patch de cadáver (`CreateBSGRagdollPatch`) |
| 🟠 **Alto** | 2 | Duplicação descontrolada de patches a cada raid e freeze por varredura cega em `Object.FindObjectsOfType` |
| 🟡 **Médio** | 5 | Tripla corrotina `WatchShot` em polling por bala no ar, triplo callback por frame em scalers de membros, erro de LayerMask em granadas, config nula em shells e pooling inativo |
| 🔵 **Baixo** | 2 | Resíduos de descompilação ILSpy, Reflection em loop quente e patches/campos órfãos |
| 💡 **Otimização** | 1 | Migração de re-asserção contínua de prone lock em `Update()` para cadência controlada reativa |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🔴 Crítico | [`ShootOffHelmetPatch.cs:L16`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs#L16) | Assinatura Inexistente | Método-alvo `Player.ReceiveDamage` não existe no EFT 0.16.9 (patch inativo) |
| `AUD-01-02` | 🔴 Crítico | [`CreateBSGRagdollPatch.cs:L28`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs#L28) | NRE / Quebra de Estado | `GetComponent<Player>()` em GameObject de `Corpse` retorna `null`, gerando NRE fatal em `.PlayerBody` |
| `AUD-01-03` | 🟠 Alto | [`GameStartedPatch.cs:L53-62`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/GameStartedPatch.cs#L53-L62) | Duplicação de Patches | `KillPatch` e `KillClientPatch` re-habilitados a cada raid; ambos miram `Player.ApplyDamageInfo` |
| `AUD-01-04` | 🟠 Alto | [`GameStartedPatch.cs:L28-34`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs#L28-L34) | CPU Freeze / GC Pressure | `Object.FindObjectsOfType<GameObject>()` chamado 2x na cena inteira com resultado descartado |
| `AUD-01-05` | 🟡 Médio | [`LimbKillPatch.cs:L33`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L33), [`BodiesImpulsePatch.cs:L76`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L76), [`BleedPatch.cs:L38`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs#L38) | Polling / FPS Thief | Três corrotinas separadas de polling frame-a-frame (`while (!shot.IsShotFinished)`) por projétil no ar |
| `AUD-01-06` | 🟡 Médio | [`RagdollHelperClass.cs:L561-577`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L561-L577) | Overhead de Update | `DismemberedLimbScaler` roda `Update`, `OnAnimatorMove` e `LateUpdate` em dezenas de transforms |
| `AUD-01-07` | 🟡 Médio | [`ShellCasingPatch.cs:L15,L20`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combat.Patches/ShellCasingPatch.cs#L15-L20) | Config Nula / Memory Leak | `NeverDeleteShells` não vinculado via `Config.Bind()`, patch inativo e busca linear O(N) em `Queue` estática |
| `AUD-01-08` | 🟡 Médio | [`GrenadeItemsPatch.cs:L27`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GrenadeItemsPatch.cs#L27) | Bug de LayerMask | `LayerMask.NameToLayer("Default")` retorna `0` em vez de bitmask (`1 << 0`), ignorando colliders |
| `AUD-01-09` | 🟡 Médio | [`GoreObjectPool.cs:L7-107`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs#L7-L107) | GC Pressure / Pooling Inativo | Classe de pool criada mas nunca consumida por `KillPatch` ou `BleedPatch` (mantém `Instantiate`/`Destroy`) |
| `AUD-01-10` | 🔵 Baixo | [`RagdollClassPatch.cs:L23-159`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs#L23-L159) | ILSpy Residue / Reflection | Classe stub `_003CRagdollSleepHandler_003Ed__2` e `MethodInfo.Invoke` com `new object[]` em loop |
| `AUD-01-11` | 🔵 Baixo | [`PlayerDetonationPatch.cs:L15-71`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/PlayerDetonationPatch.cs#L15-L71) | Código Morto / Risco | Patch órfão não registrado contendo `Object.Destroy(player)` destrutivo; campos estáticos não lidos |
| `AUD-01-12` | 💡 Otimização | [`LivingDismembermentController.cs:L125`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/LivingDismembermentController.cs#L125) | Throttling / Dirty State | Re-asserção contínua de `BotLay.IsLay` no `Update()` pode ser movida para intervalo cadenciado de 0.5s |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Método-alvo inexistente no EFT 0.16.9 em `ShootOffHelmetPatch`
- **Severidade:** 🔴 Crítico
- **Localização no Mod:** [`ShootOffHelmetPatch.cs:L14-17`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs#L14-L17)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:L30463`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30463)
- **Causa Raiz:** O patch tenta interceptar `typeof(Player).GetMethod("ReceiveDamage")`. Este método foi removido/refatorado no motor do Escape From Tarkov nas versões recentes. No EFT 0.16.9, o pipeline unificado de dano ao jogador reside em `Player.ApplyDamageInfo(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, float absorbed)`. Como o método alvo não existe, `GetTargetMethod()` retorna `null` e o patch falha ao inicializar.
- **Impacto Técnico Real:** A funcionalidade de arrancar capacetes com tiros na cabeça nunca é disparada no jogo.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* Tentativa de patch em método inexistente.
  - *Abordagem Otimizada:* Mover a lógica de ejeção de capacete para um `Postfix` cirúrgico em `Player.ApplyDamageInfo`, verificando se `colliderType == EBodyPartColliderType.Head` ou `EBodyPartColliderType.Helmet` e se o dano atingiu o capacete.

```csharp
protected override MethodBase GetTargetMethod()
{
    return typeof(Player).GetMethod(
        "ApplyDamageInfo", 
        BindingFlags.Instance | BindingFlags.Public, 
        null, 
        new Type[] { typeof(DamageInfoStruct), typeof(EBodyPart), typeof(EBodyPartColliderType), typeof(float) }, 
        null
    );
}

[PatchPostfix]
private static void Postfix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType)
{
    if (!VisceralEntry.Instance.ShootHelmetOff.Value || !__instance.IsAI || bodyPartType != EBodyPart.Head)
        return;

    if (colliderType == EBodyPartColliderType.Head || colliderType == EBodyPartColliderType.Helmet)
    {
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll <= VisceralEntry.Instance.HelmetShootOffChance.Value)
        {
            Slot slot = __instance.Inventory?.Equipment?.GetSlot(EquipmentSlot.Headwear);
            if (slot?.ContainedItem != null && __instance.InventoryController is TraderControllerClass controller)
            {
                controller.ThrowItem(slot.ContainedItem, false, null);
            }
        }
    }
}
```

---

### AUD-01-02 · `NullReferenceException` iminente em `CreateBSGRagdollPatch`
- **Severidade:** 🔴 Crítico
- **Localização no Mod:** [`CreateBSGRagdollPatch.cs:L19,L28`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs#L19-L28)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/EFT.Interactive/Corpse.cs:L225-233`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.Interactive/Corpse.cs#L225-L233)
- **Causa Raiz:** O patch intercepta `Corpse.method_16` e executa:
  `PlayerBody playerBody = ((Component)__instance).gameObject.GetComponent<Player>().PlayerBody;`
  Em entidades `Corpse` instanciadas como corpos mortos estáticos ou no ciclo de loot (`CreateStillCorpse`), o GameObject possui apenas `PlayerPoolObject` e `Corpse`, **não** possuindo o componente `Player`. A chamada `GetComponent<Player>()` retorna `null`, gerando NRE imediato ao acessar `.PlayerBody`.
- **Impacto Técnico Real:** Travamento ao instanciar ragdolls em cadáveres pré-existentes no mapa ou após a morte.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* Busca insegura de `Player` em GameObject de `Corpse` e Reflection sem cache de campos privados.
  - *Abordagem Otimizada:* Obter `PlayerBody` via `__instance.GetComponentInChildren<PlayerBody>()` e cachear os `FieldInfo` de `Corpse` estaticamente.

```csharp
private static readonly FieldInfo _spawnersField = typeof(Corpse).GetField("rigidbodySpawner_0", BindingFlags.Instance | BindingFlags.NonPublic);
private static readonly FieldInfo _jointsField = typeof(Corpse).GetField("characterJointSpawner_0", BindingFlags.Instance | BindingFlags.NonPublic);
private static readonly FieldInfo _sleepListField = typeof(Corpse).GetField("list_0", BindingFlags.Instance | BindingFlags.NonPublic);
private static readonly FieldInfo _velocityField = typeof(Corpse).GetField("vector3_1", BindingFlags.Instance | BindingFlags.NonPublic);

[PatchPrefix]
private static bool Prefix(Corpse __instance, bool forceStill = false)
{
    if (!__instance.HasRagdoll) return false;

    PlayerBody playerBody = __instance.GetComponentInChildren<PlayerBody>();
    if (playerBody == null) return true; // fallback seguro para o método original

    var spawners = (RigidbodySpawner[])_spawnersField?.GetValue(__instance);
    var joints = (CharacterJointSpawner[])_jointsField?.GetValue(__instance);
    var sleepList = (List<PlayerRigidbodySleepHierarchy>)_sleepListField?.GetValue(__instance);
    var velocity = (Vector3)(_velocityField?.GetValue(__instance) ?? Vector3.zero);

    __instance.Ragdoll = new RagdollClass(
        spawners, joints, sleepList, velocity,
        EFTHardSettings.Instance.CorpseMaxDepenetrationVelocity,
        __instance.CollisionDetectionMode,
        __instance,
        __instance.CheckCorpseIsStill,
        playerBody,
        playerBody.IsVisible,
        __instance.OnRigidbodyStopped,
        keepRigidbody: false,
        !forceStill && !EFTHardSettings.Instance.DEBUG_CORPSE_PHYSICS
    );

    __instance.OnRigidbodyStarted();
    __instance.method_19();
    return false;
}
```

---

### AUD-01-03 · Duplicação e re-habilitação redundante de `KillPatch` e `KillClientPatch`
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`GameStartedPatch.cs (Dismemberment):L53-62`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/GameStartedPatch.cs#L53-L62) e [`KillClientPatch.cs:L19`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillClientPatch.cs#L19)
- **Referência Cruzada:** [`VisceralEntry.cs:L245`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L245)
- **Causa Raiz:** `KillPatch` já é habilitado na inicialização (`Awake()`) do plugin. Em cada início de partida (`GameWorld.OnGameStarted`), o patch de inicialização executa `new KillPatch().Enable()` e `new KillClientPatch().Enable()`. Como ambos os patches visam exatamente o mesmo método (`Player.ApplyDamageInfo`) e ambos invocam `KillPatch.Postfix`, cada tiro recebido por um jogador aciona a rotina de desmembramento e ragdoll 2 ou mais vezes consecutivas.
- **Impacto Técnico Real:** Execução duplicada de cálculos de balística, múltiplos pacotes de sincronização de rede enviados no FIKA e desmembramento duplo.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* `KillClientPatch` atua como um wrapper redundante que chama `KillPatch.Postfix`.
  - *Abordagem Otimizada:* Eliminar `KillClientPatch` e remover as chamadas de `Enable()` repetidas dentro de `GameStartedPatch.cs`. Manter a habilitação única no `Awake()` de `VisceralEntry`.

---

### AUD-01-04 · Congelamento de CPU e Pressão de GC por varredura cega em `GameStartedPatch`
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`GameStartedPatch.cs (Ragdolls):L26-34`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs#L26-L34)
- **Causa Raiz:** No carregamento da partida, o código executa:
  ```csharp
  TarkovApplication obj = (TarkovApplication)Singleton<ClientApplication<ISession>>.Instance;
  RaidSettings val = (RaidSettings)typeof(TarkovApplication).GetField("_raidSettings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
  IEnumerable<GameObject> enumerable = from go in Object.FindObjectsOfType<GameObject>() where go.layer == LayerMask.NameToLayer("Grass") select go;
  IEnumerable<GameObject> enumerable2 = from go in Object.FindObjectsOfType<GameObject>() where go.layer == LayerMask.NameToLayer("Foliage") select go;
  GameObject val2 = GameObject.Find("TerrainsAI");
  ```
  Nenhuma dessas variáveis (`obj`, `val`, `enumerable`, `enumerable2`, `val2`) é utilizada posteriormente no método. `FindObjectsOfType<GameObject>()` varre dezenas de milhares de GameObjects da cena de mapas pesados como Streets of Tarkov, alocando coleções LINQ volumosas no Heap.
- **Impacto Técnico Real:** Micro-congelamento (stutter/freeze) de até vários segundos ao spawnar no raid e acúmulo de lixo no GC.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* Varredura de cena inteira e Reflection descartadas.
  - *Abordagem Otimizada:* Excluir completamente as linhas 26 a 34 de `VisceralCombat.Ragdolls.Patches.GameStartedPatch.cs`.

---

### AUD-01-05 · Tripla corrotina `WatchShot` em polling frame-a-frame por projétil no ar
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`LimbKillPatch.cs:L33`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L33), [`BodiesImpulsePatch.cs:L76`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L76), [`BleedPatch.cs:L38`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs#L38)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/EFT.Ballistics/BallisticsCalculator.cs:L227-231`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.Ballistics/BallisticsCalculator.cs#L227-L231)
- **Causa Raiz:** Cada disparo cria 3 instâncias separadas da corrotina `WatchShot` em `StaticManager.Instance` executando `while (!shot.IsShotFinished) { timeout -= Time.deltaTime; yield return null; }`. Em tiroteios com armas automáticas ou disparos de escopeta (onde cada balote/bagaço gera fragmentos), dezenas de corrotinas ativas disputam o loop de atualização da Main Thread da Unity.
- **Impacto Técnico Real:** Queda severa de quadros (FPS Thief) durante tiroteios intensos e tiroteios simultâneos de IA.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* 3 corrotinas paralelas no ar para cada bala disparada.
  - *Abordagem Otimizada:* Unificar o processamento de projétil em um único manipulador centralizado ou despachar os efeitos (`ProcessImpulse`, `ProcessLimbKill`, `ProcessWatchShot`) em uma única corrotina unificada ou hook reativo quando o projétil atinge o alvo (`shot.HasAchievedTarget`).

```csharp
// Exemplo de Unificação: Uma única corrotina gerencia Impulso, Desmembramento e Efeitos de Sangue
private static IEnumerator WatchBulletUnified(EftBulletClass shot)
{
    if (shot == null) yield break;

    float timeout = 3.0f;
    while (!shot.IsShotFinished && timeout > 0f)
    {
        timeout -= Time.deltaTime;
        yield return null;
    }

    if (shot != null && shot.IsShotFinished && shot.HitCollider != null)
    {
        BodiesImpulsePatch.ProcessImpulse(shot);
        LimbKillPatch.ProcessLimbKill(shot);
        BleedPatch.ProcessWatchShot(shot);
    }
}
```

---

### AUD-01-06 · Triplo callback por frame em múltiplos transforms em `DismemberedLimbScaler`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`RagdollHelperClass.cs:L561-577`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L561-L577)
- **Causa Raiz:** O componente `DismemberedLimbScaler` é anexado a cada osso e transform filho de um membro decepado (15 a 30 transforms por membro). Cada instância implementa simultaneamente `Update()`, `OnAnimatorMove()` e `LateUpdate()`, reatribuindo `transform.localScale = RagdollHelperClass.limbSize;` três vezes por frame por osso.
- **Impacto Técnico Real:** Centenas de chamadas desnecessárias de bridge C# -> C++ no Unity Engine a cada frame da renderização.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* 3 métodos de ciclo de vida ativos a cada frame por osso.
  - *Abordagem Otimizada:* Manter apenas `LateUpdate()`, que executa imediatamente após a avaliação das animações e dos solvers de IK/PuppetMaster.

```csharp
public class DismemberedLimbScaler : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.localScale = RagdollHelperClass.limbSize;
    }
}
```

---

### AUD-01-07 · `NeverDeleteShells` nulo e não ativado em `ShellCasingPatch`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`ShellCasingPatch.cs:L15,L20`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combat.Patches/ShellCasingPatch.cs#L15-L20) e [`VisceralEntry.cs:L171`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L171)
- **Causa Raiz:** A propriedade `NeverDeleteShells` foi declarada em `VisceralEntry`, porém nunca foi registrada no BepInEx via `Config.Bind()`, permanecendo como `null`. Além disso, `ShellCasingPatch` não é habilitado no `Awake()`. Se ativado, `VisceralEntry.Instance.NeverDeleteShells.Value` disparará `NullReferenceException`. Adicionalmente, a fila `ActiveCasings` executa `Contains()` (busca linear O(N) a cada frame) e nunca é limpa ao fim do raid.
- **Impacto Técnico Real:** Funcionalidade inoperante e risco de NRE/Memory Leak.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Otimizada:* Vincular a propriedade no `VisceralEntry.Awake()`, substituir `Queue` + `Contains` por um `HashSet` para checagem O(1) e adicionar limpeza em `GameStartedPatch`.

---

### AUD-01-08 · Bug de LayerMask em `GrenadeItemsPatch`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`GrenadeItemsPatch.cs:L27`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GrenadeItemsPatch.cs#L27)
- **Causa Raiz:** `Physics.SphereCastAll` espera uma máscara de bits (`int layerMask`), mas o código passa `LayerMask.NameToLayer("Default")`, que retorna o índice da camada (`0`). Uma máscara com valor `0` instrui o PhysX a colidir com nenhuma camada.
- **Impacto Técnico Real:** Granadas não aplicam impulso físico a itens de loot caídos no chão.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* `LayerMask.NameToLayer("Default")` (retorna `0`).
  - *Abordagem Otimizada:* `1 << LayerMask.NameToLayer("Default")` ou `LayerMask.GetMask("Default")`.

---

### AUD-01-09 · `GoreObjectPool` não utilizado pelo pipeline de sangue
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`GoreObjectPool.cs:L7-107`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs#L7-L107)
- **Causa Raiz:** O pool de objetos foi criado e possui rotinas de limpeza no `GameStartedPatch`, mas `KillPatch.cs` e `BleedPatch.cs` continuam instanciando e destruindo prefabs de partículas de sangue (`Instantiate` / `Destroy`) continuamente a cada tiro.
- **Impacto Técnico Real:** Alocações contínuas de novos GameObjects e ParticleSystems no Heap, causando picos periódicos de Garbage Collection em combates prolongados.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Conectar as chamadas de spawn de sangue e gore caps aos métodos `GoreObjectPool.Instance.Spawn()` e `Recycle()`.

---

### AUD-01-10 · Resíduos de descompilação ILSpy e Reflection sem cache em `RagdollClassPatch`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`RagdollClassPatch.cs:L19-20, L23-159, L210`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs#L19-L210)
- **Causa Raiz:** Presença da classe de máquina de estados interna `_003CRagdollSleepHandler_003Ed__2` gerada por descompilador. No loop de configuração dos rigidbodies (linha 210), `_supportRigidbodyMethod?.Invoke(null, new object[] { val4, 0f, null })` aloca um novo array `object[]` a cada iteração para cada osso do jogador.
- **Impacto Técnico Real:** Poluição do código e pressão desnecessária no coletor de lixo.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Reescrever a corrotina com `IEnumerator` C# idiomático e reutilizar buffer para invocação de reflection.

---

### AUD-01-11 · Patch órfão `PlayerDetonationPatch` e campos mortos
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`PlayerDetonationPatch.cs:L15-71`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/PlayerDetonationPatch.cs#L15-L71) e [`CreateCorpsePatch.cs:L15`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateCorpsePatch.cs#L15)
- **Causa Raiz:** `PlayerDetonationPatch` nunca é ativado. Seu método `Postfix` contém uma chamada `Object.Destroy((Object)(object)componentInParentRecursive)` que, caso executada, destruiria a entidade `Player` do Tarkov, quebrando toda a lógica de sessão e HUD. Em `CreateCorpsePatch.cs`, `TargetBones` é um array estático que nunca é lido.
- **Impacto Técnico Real:** Código morto e risco de regressão caso seja ativado acidentalmente.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Remover o arquivo `PlayerDetonationPatch.cs` e limpar os campos órfãos.

---

### AUD-01-12 · Cadência de polling de `ForceProneLock` em `LivingDismembermentController`
- **Severidade:** 💡 Otimização
- **Localização no Mod:** [`LivingDismembermentController.cs:L125`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/LivingDismembermentController.cs#L125)
- **Causa Raiz:** `ForceProneLock()` executa a cada frame (`Update()`), atribuindo `_botOwner.BotLay.NextPosibleGetUp = Time.time + 999999f;` e verificando `MovementContext.IsInPronePose`. Como `ProneLockPatch.cs` e `ProneMoverDoPronePatch.cs` já bloqueiam de forma reativa qualquer tentativa do bot de levantar (`IsLay = false` ou `DoProne(false)`), a execução em todos os frames é redundante.
- **Impacto Técnico Real:** Execução desnecessária de verificações de estado a 60-144 FPS.
- **Alternativa de Melhor Lógica / Proposta de Correção:** Executar `ForceProneLock()` em um intervalo throttled de `0.5s` junto ao ciclo de sangramento.

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata de Bloqueadores (🔴 e 🟠):**
   - Corrigir `ShootOffHelmetPatch` migrando o alvo para `Player.ApplyDamageInfo`.
   - Corrigir `CreateBSGRagdollPatch` substituindo `GetComponent<Player>()` por `GetComponentInChildren<PlayerBody>()`.
   - Eliminar `KillClientPatch` e remover a re-habilitação de patches em `GameStartedPatch.cs`.
   - Remover as varreduras `FindObjectsOfType` inúteis em `Ragdolls/GameStartedPatch.cs`.

2. **Otimização de Desempenho e FPS (🟡):**
   - Unificar a tripla corrotina `WatchShot` em um único despachante reativo.
   - Reduzir `DismemberedLimbScaler` para responder unicamente no `LateUpdate()`.
   - Corrigir a bitmask de `GrenadeItemsPatch` (`1 << LayerMask.NameToLayer("Default")`).
   - Conectar o `GoreObjectPool` ao spawn de partículas e gore caps.

3. **Limpeza e Manutenibilidade (🔵 e 💡):**
   - Excluir o patch órfão `PlayerDetonationPatch.cs`.
   - Limpar classes geradas pelo ILSpy em `RagdollClassPatch.cs`.
   - Cadenciar a verificação de `ForceProneLock()` em `LivingDismembermentController`.
