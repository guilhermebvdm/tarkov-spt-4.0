# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** snapshot de 2026-08-24 (Sessão 2026-08-24) — sem pendências abertas que afetem este item. **Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (sempre), `fika-packet-desync-prevention-plan.md` (mod declara `INetSerializable`), `spt4-items-inventory-hideout.md` não trazia nada além do já coberto pelo `EquipmentSlot` enum lido direto no Assembly.

---

## 0. Achado prévio que muda a leitura do "comportamento atual" da spec funcional

A spec funcional (01) descreve a arma como "presa ao corpo, sem drop automático". Investigação no Assembly mostra que isso é **impreciso**: o EFT vanilla **já** solta a arma da mão no momento da morte — só que não do jeito que o pedido do usuário quer.

- `Player.OnDead(EDamageType)` ([Player.cs:30539](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30539)) chama `Corpse = CreateCorpse()` ([Player.cs:30641](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30641)) e, se o jogador não morreu operando uma arma estacionária, agenda a corrotina `method_98()` ([Player.cs:30681-30692](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30681-L30692)), que roda **1 frame depois** e chama `DropItemDead(_handsController.Item, _handsController.ControllerGameObject)` ([Player.cs:30686](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30686)).
- `DropItemDead` ([Player.cs:26802-26855](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26802-L26855)) **não remove o item do inventário do cadáver** — ele cria um `Rigidbody` físico e chama `Corpse.Ragdoll.AttachWeapon(...)` ([Player.cs:26853](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26853)), um efeito puramente cosmético: a arma balança fisicamente presa à mão do cadáver, mas continua sendo o mesmo item, ainda dentro do loot do cadáver (não vira um item solto/independente no mundo).
- É essa mesma linha (`26848`) que o próprio EFT usa para decidir se o item é uma faca: `item is PistolItemClass || item is ThrowWeapItemClass || item.GetItemComponent<KnifeComponent>() != null` — usado aqui só para decidir a pose da mão, não para pular o drop.

**Conclusão:** o pedido do usuário ("a arma cai como se o personagem tivesse soltado", "slot fica vazio") não é о que o vanilla já faz — é o comportamento do **`ThrowItem`** que `ShootOffHelmetPatch.cs:41-44` já usa para o capacete (remove o item do slot e o materializa como item solto no mundo, via `RemoveOperationClass` — a operação canônica de inventário do EFT, ver §6). O ponto de patch correto para a arma, portanto, não é `CreateCorpsePatch`/`CreateBSGRagdollPatch` (que tratam da ragdoll física, não do item em mãos) — é o próprio `Player.DropItemDead`, que já é o hook vanilla dedicado a "o que fazer com o item em mãos quando o dono morre". Interceptá-lo evita correr em paralelo/depois da corrotina `method_98` e reaproveita exatamente o parâmetro (`item`) que ela já resolveu.

## 1. Estratégia

- **Item 1 (drop de arma na morte, exceto faca):** `Prefix` em `Player.DropItemDead(Item, GameObject)` ([Player.cs:26802](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26802)). Se o item não for faca e a feature estiver ligada, executa `ThrowItem` (mesmo padrão de `ShootOffHelmetPatch.cs:41-44`) e retorna `false` (pula o fling cosmético do vanilla, que se tornaria redundante/inconsistente com o item já removido do inventário). Faca: retorna `true` (comportamento vanilla preservado — não vira item solto).
- **Item 2 (capacete + óculos a 100% no desmembramento de cabeça):** todos os três gatilhos reais de desmembramento de cabeça já existentes no mod (`KillPatch.Postfix` caso `bodyPartType == 0`, e as duas estratégias de `LimbKillPatch.ProcessLimbKill` para corpos já mortos) convergem num único ponto: `KillPatch.DismemberLimb(...)` ([KillPatch.cs:204](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L204)). Não se cria um patch novo — adiciona-se uma chamada condicional (`bodyPartType == (EBodyPart)0`) dentro desse método já existente, no mesmo bloco que já trata efeitos exclusivos de cabeça (linha 437). Isso garante cobertura dos 3 gatilhos com uma única mudança, sem duplicar lógica.
- **Alternativas descartadas:**
  - Patchear `CreateCorpsePatch`/`CreateBSGRagdollPatch` para o drop de arma — descartado porque nenhum dos dois tem acesso direto ao item-em-mãos no momento certo; a evidência (§0) mostra que `DropItemDead` já é o hook vanilla dedicado a essa decisão.
  - Duplicar a chamada de drop de capacete/óculos em `KillPatch.Postfix` e nas duas estratégias de `LimbKillPatch.ProcessLimbKill` — descartado por violar DRY; todos os três já chamam `DismemberLimb`, então um único ponto cobre os três sem risco de divergência futura.

## 2. Pontos de patch

| Alvo (Assembly/mod) | Tipo | Motivo |
|---|---|---|
| [`EFT/Player.cs:26802`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26802) `DropItemDead(Item, GameObject)` | Prefix (novo) | Interceptar o hook vanilla de "o que fazer com o item em mãos ao morrer"; substituir o fling cosmético por `ThrowItem` real quando não for faca |
| [`KillPatch.cs:204`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L204) `DismemberLimb(...)` (método já existente do mod) | Edição inline (não é patch Harmony — é código próprio do mod) | Ponto único de convergência dos 3 gatilhos de desmembramento de cabeça; adicionar drop de capacete+óculos a 100% quando `bodyPartType == Head` |

Nenhum novo alvo Harmony obfuscado/virtual é introduzido (AP-03 não se aplica — `DropItemDead` é `public void` concreto na classe base `Player`, sem overrides a auditar).

## 3. Novas propriedades F12 (BepInEx)

Seguindo a convenção do mod de expor cada comportamento como toggle individual (`ShootHelmetOff`, `EnableDismemberment`, etc. — `VisceralEntry.cs`), adicionar dois novos toggles, ambos default `true` (preserva o comportamento desejado por padrão; usuário pode desligar se preferir o vanilla):

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Ragdolls \| Character Properties` | `Drop Weapon On Death` | bool | `true` | — | — | Solta a arma em mãos (exceto faca) como item avulso ao morrer, em vez de deixá-la presa ao cadáver. |
| `Dismemberment` | `Drop Headwear/Eyewear On Head Dismemberment` | bool | `true` | — | — | Derruba capacete e óculos com 100% de chance quando a cabeça é efetivamente desmembrada (evento distinto da chance configurável de "Helmet Knock Off Chance", que continua funcionando como antes). |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs` | CRIAR | Prefix em `Player.DropItemDead` — dropa a arma (exceto faca) como item solto no chão via `ThrowItem`, gated por `FikaBackendUtils.IsServer \|\| IsSinglePlayer` |
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | MODIFICAR | `DismemberLimb(...)`: adicionar chamada a `DropHeadEquipment(player)` quando `bodyPartType == (EBodyPart)0`; novo método privado `DropHeadEquipment` |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | MODIFICAR | Duas novas `ConfigEntry<bool>` (§3) + `((ModulePatch)new WeaponDropOnDeathPatch()).Enable();` junto dos demais `.Enable()` de patches de Ragdolls (perto da linha 282/287) |
| `mods/VisceralCombat/PROPRIEDADES.md` | MODIFICAR | Documentar as duas novas `ConfigEntry` (§3) nas seções `Ragdolls \| Character Properties` e `Dismemberment` |

## 5. Stubs de código

### 5.1 `WeaponDropOnDeathPatch.cs` (novo)

```csharp
using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class WeaponDropOnDeathPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT/Player.cs:26802 — public void DropItemDead(Item item, GameObject prefab)
        return typeof(Player).GetMethod(
            "DropItemDead",
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new Type[] { typeof(Item), typeof(GameObject) },
            null);
    }

    [PatchPrefix]
    private static bool Prefix(Player __instance, Item item)
    {
        try
        {
            if (VisceralEntry.Instance == null || !VisceralEntry.Instance.DropWeaponOnDeath.Value) return true;
            if (item == null) return true;

            // **Correção pós-compilação (AP-09):** o teste originalmente proposto aqui era
            // `item.GetItemComponent<KnifeComponent>() != null` (mesmo teste que Player.cs:26848
            // usa para decidir a pose de mão da faca). O `dotnet build` real falhou — CS0311/CS0012:
            // `IItemComponent` vive num assembly (`ItemComponent.Types`) não referenciado por
            // `VisceralCombat.csproj`. Substituído por checagem de tipo concreto, sem mudar o
            // resultado (mesma exceção: faca nunca vira item solto no chão).
            // ref: Assembly-CSharp/KnifeItemClass.cs:7 — `public class KnifeItemClass : Item`
            if (item is KnifeItemClass) return true;

            // Só o host (ou singleplayer) executa a remoção real do inventário. A replicação
            // para os demais peers acontece pelo canal nativo de operação de inventário do
            // EFT/Fika (RemoveOperationClass) — mesmo padrão de gate já usado neste mod em
            // VisceralCombat.Combined.Patches.KillPatch.cs:113 e :170 para outras ações que
            // não podem rodar em duplicidade em cada peer. Ver spec-tech §7 (risco a validar
            // in-raid coop antes de fechar o item).
            if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return true;

            // ref: mods/VisceralCombat/.../ShootOffHelmetPatch.cs:41-44 — mesmo padrão de drop já
            // usado pelo mod para o capacete.
            if (__instance.InventoryController is TraderControllerClass controller)
            {
                controller.ThrowItem(item, false, null);
            }

            return false; // pula o fling cosmético do vanilla (Player.cs:26853 AttachWeapon) —
                           // a arma já foi removida do inventário e virou item solto no mundo
        }
        catch (Exception ex)
        {
            QuickLogger.Log(ELogType.Error, $"[WeaponDropOnDeathPatch] {ex}");
            return true; // falha seguro: deixa o vanilla rodar em vez de arriscar corpo inconsistente
        }
    }
}
```

### 5.2 `KillPatch.cs` — edição em `DismemberLimb` (não é um patch Harmony novo, é código do próprio mod)

**Correção pós-review (`PA-01-01`, 🔴):** o bloco de efeito sonoro de cabeça ([KillPatch.cs:437-447](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L437-L447)) está **dentro** do `foreach (Transform val in array)` aberto em [KillPatch.cs:230](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L230) e fechado em [:448](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L448) — roda uma vez por transform cujo nome casa a palavra-chave do osso (tipicamente mais de um para "head"). Inserir `DropHeadEquipment` ali dentro o chamaria múltiplas vezes por evento (inofensivo pelo guard de `ContainedItem != null`, mas logicamente errado). O ponto correto é **depois** do `foreach` fechar, no mesmo nível do bloco já existente `if (player.IsYourPlayer && (int)bodyPartType == 0)` ([KillPatch.cs:449](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L449)), que já é o precedente do método para "uma vez por evento de cabeça, fora do loop":

```csharp
// (fora do foreach — mesmo nível do bloco "if (player.IsYourPlayer && (int)bodyPartType == 0)"
// já existente em KillPatch.cs:449; NÃO dentro do foreach de affectedLimbs)
if ((int)bodyPartType == 0)
{
    DropHeadEquipment(player);
}
if (player.IsYourPlayer && (int)bodyPartType == 0)
{
    // ... bloco já existente, sem alteração ...
}
```

O bloco de `bloodSFX` (linhas 437-447, dentro do `foreach`) permanece exatamente onde está, sem alteração.

Novo método privado, mesma classe `KillPatch`:

```csharp
/// <summary>
/// Derruba capacete e óculos com 100% de chance quando a cabeça é efetivamente
/// desmembrada. Gatilho distinto de VisceralCombat.Ragdolls.Patches.ShootOffHelmetPatch
/// (chance configurável em QUALQUER hit de cabeça, só bots) — este dispara só no evento
/// real de desmembramento, para bots E jogadores, e coexiste sem alterar aquele.
/// </summary>
private static void DropHeadEquipment(Player player)
{
    if (VisceralEntry.Instance == null || !VisceralEntry.Instance.DropHeadEquipmentOnDismemberment.Value) return;
    if (player == null) return;

    // Mesmo gate de autoridade usado em WeaponDropOnDeathPatch (spec-tech §7) — só o host
    // (ou singleplayer) executa a remoção real; replicação via operação nativa de inventário.
    if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return;

    if (!(player.InventoryController is TraderControllerClass controller)) return;

    // ref: VisceralCombat.Ragdolls.Patches.ShootOffHelmetPatch.cs:40-44 — mesmo padrão de drop
    Slot helmetSlot = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Headwear);
    if (helmetSlot?.ContainedItem != null)
    {
        controller.ThrowItem(helmetSlot.ContainedItem, false, null);
    }

    Slot eyewearSlot = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Eyewear);
    if (eyewearSlot?.ContainedItem != null)
    {
        controller.ThrowItem(eyewearSlot.ContainedItem, false, null);
    }
}
```

`using Fika.Core.Main.Utils;` já não está em `KillPatch.cs` — **conferir**: o arquivo já importa `Fika.Core.Networking` e `Fika.Core.Main.Players`/`Fika.Core.Main.Utils` (linha 10-13, ver cabeçalho atual do arquivo) — `FikaBackendUtils` já é usado em `KillPatch.cs:113,170`, então nenhum `using` novo é necessário.

### 5.3 `VisceralEntry.cs` — novas `ConfigEntry` + registro do patch

```csharp
// Junto das demais ConfigEntry de "Ragdolls | Character Properties" (perto de VisceralEntry.cs:270-273)
DropWeaponOnDeath = ((BaseUnityPlugin)this).Config.Bind<bool>(
    "Ragdolls | Character Properties",
    "Drop Weapon On Death",
    true,
    "Solta a arma em maos (exceto faca) como item avulso ao morrer, em vez de deixa-la presa ao cadaver.");

// Junto de EnableDismemberment (perto de VisceralEntry.cs:178-185)
DropHeadEquipmentOnDismemberment = ((BaseUnityPlugin)this).Config.Bind<bool>(
    "Dismemberment",
    "Drop Headwear/Eyewear On Head Dismemberment",
    true,
    "Derruba capacete e oculos com 100% de chance quando a cabeca e efetivamente desmembrada.");

// Junto dos demais .Enable() de patches de Ragdolls (perto de VisceralEntry.cs:282-291)
((ModulePatch)new WeaponDropOnDeathPatch()).Enable();
```

E as duas novas propriedades públicas junto das demais `ConfigEntry<bool>` (perto de `VisceralEntry.cs:133`):

```csharp
public ConfigEntry<bool> DropWeaponOnDeath { get; set; }
public ConfigEntry<bool> DropHeadEquipmentOnDismemberment { get; set; }
```

## 6. Fluxo de dados

**Item 1 — drop de arma:**
```
[A] Player.OnDead (Player.cs:30539) → CreateCorpse (:30641) → corrotina method_98 (:30681, 1 frame depois)
  → [B] DropItemDead(item, prefab) (:30802) ← WeaponDropOnDeathPatch.Prefix intercepta aqui
    → não é faca? é host/singleplayer? → [C] TraderControllerClass.ThrowItem (TraderControllerClass.cs:1762)
      → InteractionsHandlerClass.Discard + RemoveOperationClass.vmethod_1 (:1764-1772) — operação
        canônica de inventário do EFT → [D] item removido do slot, materializado como loot solto no
        mundo; replicação para outros peers Fika via canal nativo de operação de inventário (não um
        pacote custom do VisceralCombat)
    → é faca, ou não é host? → Prefix retorna true → vanilla DropItemDead roda normalmente (fling
      cosmético, item permanece no cadáver)
```

**Item 2 — drop de capacete/óculos:**
```
[A] Tiro de cabeça fatal (KillPatch.Postfix, ApplyDamageInfo) OU tiro pós-morte em cabeça
    (LimbKillPatch.ProcessLimbKill, estratégias A/B) → ambos convergem em
  [B] KillPatch.DismemberLimb(..., bodyPartType=Head, ...) (KillPatch.cs:204)
    → bloco "if bodyPartType == 0" (:437) → [C] DropHeadEquipment(player) (novo)
      → é host/singleplayer? → GetSlot(Headwear) / GetSlot(Eyewear) → [D] ThrowItem por item
        equipado presente → mesma operação canônica do item 1 → replicação nativa Fika
```

## 7. Riscos e dependências

- **🟡 Risco central — autoridade de rede, parcialmente de-riscado por evidência do próprio FIKA (pós-review 01).** A escolha de gate (`FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer`) segue o precedente já usado no próprio `KillPatch.cs:113,170` para evitar que uma ação de mutação de mundo rode em duplicidade em cada peer de um raid coop.
  - **Item 2 (capacete/óculos) — confirmado.** `VisceralEntry.cs:391-397` e `:421-423` mostram que `OnDismembermentPacketServer` chama `OnDismembermentPacketClient` (que executa `KillPatch.DismemberLimb(..., isFromNetwork: true)`) **antes** de relayar aos demais clients — ou seja, o host **sempre** processa localmente qualquer evento de desmembramento, não importa qual peer o detectou primeiro. O gate `IsServer` garante exatamente uma execução de `DropHeadEquipment`, na máquina certa.
  - **Item 1 (arma) — de-riscado, mas sem confirmação por pacote próprio.** Diferente do item 2, `WeaponDropOnDeathPatch` intercepta `Player.DropItemDead` (vanilla), sem nenhum pacote/roteamento controlado pelo mod. Evidência encontrada no FIKA que sustenta a suposição "`OnDead`/`DropItemDead` roda em todo peer, para todo Player":
    - [`Player_OnDead_Patch.cs`](../../../../references/fika-plugin/Fika.Core/Main/Patches/PlayerPatches/Player_OnDead_Patch.cs) — o FIKA já transpila `LocalPlayer.OnDead` para chamar `Player.OnDead` diretamente (pulando um handler de dogtag do BSG "mal implementado"), confirmando que `Player.OnDead` roda para o **LocalPlayer** (o personagem que a própria máquina possui/controla).
    - [`ObservedPlayer.cs:1133-1170`](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1133-L1170) — `ObservedPlayer.OnDead` (override) chama `base.OnDead(damageType)` na linha 1170, ou seja, quando qualquer peer **observa** outro player/bot (via `ObservedPlayer`, a representação usada para tudo que não é o personagem local daquela máquina), `Player.OnDead`/`DropItemDead` **também** roda ali, localmente, na máquina do observador.
    - Isso confirma a premissa arquitetural (mesmo padrão do item 2: o método roda por peer, independentemente de quem "é dono"), mas **não prova** que o `ThrowItem` do host sobre o `InventoryController` do `ObservedPlayer` que representa um client remoto de fato propaga de volta a esse client pelo canal nativo de replicação de inventário — isso fica para a validação empírica.
  - **Fechamento:** manter o gate como especificado (§5.1, §5.2) e tratar a validação coop (§8) como a etapa que **decide** se a suposição se sustenta — não apenas uma confirmação de rotina. Se a validação mostrar que a arma não aparece corretamente para o client observador quando o próprio client morre, o item volta para `/create-technical-spec` com uma abordagem revisada (ex.: pacote próprio de sincronização, seguindo o guia canônico de `docs/technical/fika-packet-desync-prevention-plan.md`, já que aí sim se justificaria criar um `INetSerializable` novo).
- **✅ Resolvido (pós-review 01, `PA-01-02`) — efeito de pular `DropItemDead` sobre `Corpse.SetItemInHandsLootedCallback`.** Lido [`Corpse.cs:287-302`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/Corpse.cs#L287-L302): `SetItemInHandsLootedCallback` só guarda o delegate em `_itemInHandsLooted`, invocado exclusivamente por `RemoveLootItem` quando `ItemInHands.Value == args.Item` (ou seja, quando alguém loota do cadáver especificamente o item que está "em mãos"). `Corpse.ItemInHands` é atribuído em `CreateCorpse<T>` ([`Corpse.cs:247`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/Corpse.cs#L247)), **antes** e **independentemente** de `DropItemDead` rodar — então pular `DropItemDead` não impede esse campo de ser setado. O que se perde ao pular `DropItemDead` é só a criação do `_garbage`/`Rigidbody`/`RigidbodySpawner` do fling cosmético ([Player.cs:26826-26854](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26826-L26854)) — como a arma já foi removida da árvore de inventário do cadáver pelo `ThrowItem`, ninguém jamais vai "lootar do cadáver" aquele item específico (ele já não está lá), então `RemoveLootItem`/`_itemInHandsLooted` nunca disparam para ele — mas isso é inofensivo: o callback fica um delegate não-invocado dentro de um `Corpse` raid-scoped (sem leak entre raids), e `ReleaseHand()` (o delegate em si) só faria `_garbage?.Destroy()`, que já é `null`-safe e não tinha nada para destruir de qualquer forma, já que `_garbage` nunca foi criado neste fluxo.
- **Achado adicional (fora do escopo de código deste item, mas do escopo da auditoria pedida):** os pacotes próprios do mod (`DismembermentPacket`, `LivingDismembermentPacket`, `RagdollSyncPacket` — `VisceralCombat.Dismemberment.Classes.Packets`/`VisceralCombat.Ragdolls.Classes.Packets`) **não seguem** o guia canônico (`docs/technical/fika-packet-desync-prevention-plan.md`): serializam com `writer.Put`/`reader.Get*` crus (sem envelope de comprimento, sem `TryGet*`, sem flag `Valid`) — ver `DismembermentPacket.cs:21-42`, `LivingDismembermentPacket.cs:16-34`, `RagdollSyncPacket.cs:13-26`. O mod **não aparece** no inventário auditado de `fika-packet-desync-prevention-plan.md` §6 (auditoria de 2026-07-26, 6 mods) — está desatualizado, o VisceralCombat nunca foi incluído. Isso é dívida técnica pré-existente, não introduzida por este item (nenhum pacote novo é criado aqui — item 1 e item 2 não precisam de pacote próprio, replicam pela operação nativa de inventário). Registrar como candidato a item de backlog separado (auditoria/fix dos pacotes existentes, categoria AP-11), fora do escopo de código deste item 002.
- **Patches existentes que tocam a mesma área:** `ShootOffHelmetPatch.cs` (não alterado — mecanismo de chance por hit continua intacto, ver spec funcional item "Comportamento desejado" #2); `CreateCorpsePatch.cs`/`CreateBSGRagdollPatch.cs` (não tocados — investigação em §0 mostrou que não são o hook certo para o item 1).
- **Ordem de inicialização:** `WeaponDropOnDeathPatch` deve ser `.Enable()`-ado no `Awake()` do plugin junto dos demais patches de Ragdolls — sem dependência de ordem específica (não compete por nenhum recurso compartilhado com outros patches do mod).
- **Compatibilidade com outros mods:** nenhuma interação conhecida — `DropItemDead` é um método vanilla sem outros patches deste repo registrados nele (conferido via grep em `mods/*/modded/`).

## 8. Checklist de implementação

- [x] Criar `WeaponDropOnDeathPatch.cs` (§5.1) em `VisceralCombat.Ragdolls.Patches`.
- [x] Adicionar `DropHeadEquipment(Player)` e a chamada condicional em `KillPatch.DismemberLimb` (§5.2).
- [x] Adicionar as duas `ConfigEntry<bool>` + registrar `WeaponDropOnDeathPatch` no `Awake()` de `VisceralEntry.cs` (§5.3).
- [x] Atualizar `PROPRIEDADES.md` com as duas novas propriedades (seções `Ragdolls | Character Properties` e `Dismemberment`).
- [x] Compilar (`dotnet build ... -c Release -o mods/VisceralCombat/builds/...` — **sem** instalar automaticamente no jogo, por restrição explícita do usuário). Build succeeded em 2026-09-09 (1 correção pós-compilação, ver §5.1/§9 check 9 — AP-09).
- [ ] Validar solo: bot morre com arma em mãos → arma vira item solto; bot morre com faca em mãos → faca permanece; cabeça é desmembrada (pós-morte e na morte) → capacete + óculos caem a 100%; hit de cabeça sem desmembrar → capacete continua caindo só pela chance configurável antiga, sem duplicar.
- [ ] **Validar coop (2 máquinas, host + client) — obrigatório antes de fechar o item (§7):** ordem sugerida, do teste mais decisivo pro de confirmação —
  1. **Decisivo (item 1, arma):** **client morre** (o próprio personagem do client, não um bot) enquanto o host observa — confirma ou refuta a suposição de §7 de que o `ThrowItem` do host sobre o `ObservedPlayer` do client remoto replica de volta corretamente. Se falhar, o item retorna para `/create-technical-spec`.
  2. Host mata bot / client mata bot — arma aparece igual pros dois lados.
  3. Host morre com arma em mãos — confirmação de rotina.
  4. Cabeça de bot/player é desmembrada com host e client no raid — capacete/óculos aparecem iguais pros dois lados (confirmação de rotina — item 2 já tem respaldo arquitetural em §7).
  5. Nenhum item duplicado, nenhum item fantasma local-only em nenhum dos casos acima.
- [ ] Registrar resultado da validação coop na memória do mod (`mods/VisceralCombat/memory/sessions.md`) antes de considerar o item 🟢 entregue.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Feature não aloca estado estático raid-scoped nem assina eventos de longa duração; `ThrowItem` é uma operação pontual sem estado a limpar entre raids (nenhuma lista/flag estática nova) |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `WeaponDropOnDeathPatch` e `DropHeadEquipment` reagem à morte/desmembramento de **qualquer** player (bot ou humano) por design explícito da spec funcional (item 1 e 2 se aplicam a ambos) — não há MainPlayer a filtrar; o filtro relevante aqui é de **autoridade de rede** (`FikaBackendUtils.IsServer \|\| IsSinglePlayer`), tratado no check 11 e em §7 |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | `Player.DropItemDead` é `public void` concreto (não virtual/abstract) na classe base `Player.cs:26802`; `KillPatch.DismemberLimb` é código próprio do mod, não um alvo Harmony |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `ThrowItem`→`RemoveOperationClass`→`vmethod_1` (TraderControllerClass.cs:1762-1772) é a operação canônica de remoção de item, mesma usada por `ShootOffHelmetPatch.cs:41-44`; side-effects documentados em §6 (fluxo de dados) |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | Nenhum estado persiste além do evento pontual da morte/desmembramento dentro da raid atual (spec funcional, critério "Estado entre raids") |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Ambas as novas `ConfigEntry<bool>` têm default `true`, sem faixa numérica a ambiguar, tooltip explícito distinguindo do mecanismo de chance já existente (§3) |
| 7 | Reentry-guard em re-invocação de método patcheado — AP-07 | N/A | O Prefix não invoca `DropItemDead` de volta; quando deixa o vanilla rodar, apenas retorna `true` (não chama o método manualmente) |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Nenhum cache de contexto (arma/operação) é mantido entre chamadas; cada invocação do Prefix resolve o `item` recebido por parâmetro, sem estado stale possível |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `DropItemDead` (Player.cs:26802), `OnDead`/`method_98` (Player.cs:30539/30681), `TraderControllerClass.ThrowItem` (TraderControllerClass.cs:1762), `EquipmentSlot` enum (EquipmentSlot.cs), `KnifeItemClass : Item` (KnifeItemClass.cs:7) — todos lidos diretamente no dump local, não apenas citados por recon. **Caso real do AP-09 durante este item:** a 1ª tentativa (`Item.GetItemComponent<KnifeComponent>()`, também presente no dump em Item.cs:749) **compilou-se contra um assembly não referenciado** (`ItemComponent.Types`) e só falhou no `dotnet build` real, não na leitura do `.cs` — corrigido para `item is KnifeItemClass` (ver §5.1) |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Feature não usa/buffa nenhuma skill do EFT |
| 11 | Pacote FIKA próprio: envelope, `TryGet*`, `Valid`, registro por instância, zero `UnregisterPacket` — AP-11 | N/A (para este item) — ⚠️ ver §7 | Item 1 e 2 **não criam pacote novo** (replicam pela operação nativa de inventário, não por `INetSerializable` próprio). Os pacotes **já existentes** do mod (`DismembermentPacket`, `LivingDismembermentPacket`, `RagdollSyncPacket`) são **não conformes** ao guia — achado documentado em §7 como dívida técnica pré-existente, fora do escopo de código deste item, candidato a item de backlog separado |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | `/review-technical-spec` rodada 01: 1 🔴 + 2 🟡 + 1 🟢. Aplicadas correções: PA-01-01 (ponto de inserção movido para fora do `foreach`), PA-01-02 (resolvido por leitura de `Corpse.cs`), PA-01-03 (de-riscado por evidência do FIKA — `Player_OnDead_Patch.cs`/`ObservedPlayer.cs`; validação coop reordenada para priorizar o teste decisivo), PA-01-04 (citação adicionada) |
| 2026-09-09 | `/code-mod`: implementado + compilado (`dotnet build`, Release, saída em `mods/VisceralCombat/builds/002-.../`). 1 correção pós-compilação (AP-09): detecção de faca trocada de `Item.GetItemComponent<KnifeComponent>()` (assembly `ItemComponent.Types` não referenciado) para `item is KnifeItemClass` |
