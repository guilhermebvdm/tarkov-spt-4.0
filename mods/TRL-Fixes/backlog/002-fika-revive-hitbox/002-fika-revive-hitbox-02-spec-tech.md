---
title: Fika Revive Hitbox Loss Fix — spec técnica
date: 2026-07-26
status: 🟢 Vivo
authors: [Claude, Guilherme]
---

# 002 — Spec técnica: hitbox perdida após revive

**Spec funcional:** [002-fika-revive-hitbox-01-spec.md](./002-fika-revive-hitbox-01-spec.md)

## 1. Prova da causa

### 1.1 A hitbox balística vive na layer `HitCollider`, não em `Player`

`Player.SetupHitColliders()` → `Player.method_92(...)` — `references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:29832-29862`:

```csharp
public virtual void SetupHitColliders() {
    _hitColliders = GetComponentsInChildren<BodyPartCollider>();
    _armorPlateColliders = GetComponentsInChildren<ArmorPlateCollider>(includeInactive: true);
    foreach (var c in _hitColliders)         method_92(c, includeChild: false);
    foreach (var c in _armorPlateColliders)  method_92(c, includeChild: true);
}

public void method_92(BodyPartCollider bodyPartCollider, bool includeChild) {
    int layer = LayerMask.NameToLayer("HitCollider");
    bodyPartCollider.SetUpPlayer(this);
    bodyPartCollider.PlayerProfileID = ProfileId;
    bodyPartCollider.gameObject.layer = layer;
    ...
}
```

### 1.2 A máscara de traçado de bala não inclui `Player`

`LayerMasksDataAbstractClass.cs:69-83` (alias 4.1: `EFT.Ballistics.BallisticsCalculatorConstants`):

```csharp
String_0 = { "Water", "Terrain", "HighPolyCollider", "TransparentCollider" };
String_1 = { "Deadbody" };
String_2 = { "HitCollider" };
HitMask  = smethod_0(String_0.Concat(String_1.Concat(String_2)).ToArray());
```

→ `HitMask` = Water | Terrain | HighPolyCollider | TransparentCollider | **Deadbody** | **HitCollider**. Sem `Player`.

Consumida no traçado real do projétil: `EftBulletClass.cs:819` — `method_16(prev, next, HitMask, HitMask, Func_0)`. Facas: `GClass2967.cs:66` e `Player.cs:18176`. Granada/trigger: `EFT.Interactive/DamageTrigger.cs:92` e `RocketLauncherConeBlastClass.cs:85,116` usam `LayerMaskClass.HitColliderMask` (= só `1 << HitCollider`, `LayerMaskClass.cs:114`).

### 1.3 O par é inseparável no vanilla

`Player.Init` — `EFT/Player.cs:28646-28647`:

```csharp
TransformHelperClass.SetLayersRecursively(base.gameObject, LayerMask.NameToLayer(layerName));
SetupHitColliders();                                     // linha IMEDIATAMENTE seguinte
```

e `:28687` → `RecalculateEquipmentParams();`

Este é o argumento central: **o EFT nunca atribui layer à hierarquia sem repromover as hitboxes na sequência.** Quando o vanilla precisa preservar exceções de layer, usa a sobrecarga com lista de ignore (`EFT.Interactive/Corpse.cs:255` → `SetLayersRecursively(gameObject, layer, "Shells")`).

### 1.4 O Fika quebra o par

`references/fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs`:

| Linha | Momento | Ação |
|---|---|---|
| `:80` | down (`Init`) | `SetLayersRecursively(gameObject, "Deadbody")` — hitboxes ficam em `Deadbody`, que **está** no `HitMask` → derrubado continua baleável (correto) |
| `RagdollClass.cs:132-136` | down | desativa todos os `PlayerBones.ArmorPlateColliders` (`SetActive(false)`) |
| `:132` | revive (`RemoveRagdoll`) | `SetLayersRecursively(gameObject, "Player")` — **sem `SetupHitColliders()` depois** |

`TransformHelperClass.SetLayersRecursively(GameObject, int)` (`TransformHelperClass.cs:600-608`) é a sobrecarga **sem** ignore: sobrescreve `layer` de todos os filhos, sem guardar o original.

Grep no `fika-plugin`: `SetupHitColliders` → **0 ocorrências**. `RecalculateEquipmentParams` → só `FikaPlayer.cs:579` (reação a troca de item contido) e um transpiler de armadura.

### 1.5 Alcance: por observador, e nos dois caminhos

`RemoveRagdoll()` tem dois call sites, e **todo** observador passa por um deles:

- `ReviveInteractable.cs:240` — dentro de `RevivePlayer`, no cliente de **quem executou** o revive (que depois faz `ClearReviveInteractable()`, por isso o `ToggleDowned(false)` que chega depois retorna cedo em `ObservedPlayer.cs:1319-1325`).
- `ObservedPlayer.cs:1327` — dentro de `ToggleDowned(false)`, nos **demais** peers, ao receber `RevivedPlayerPacket`.

`ObservedPlayer` é `sealed class ObservedPlayer : FikaPlayer`, e `FikaPlayer : LocalPlayer : Player` — nenhum deles sobrescreve `SetupHitColliders`, então a chamada de `Player` serve.

## 2. Mudança

Postfix em `ReviveInteractable.RemoveRagdoll`, chamando o par que o Fika omitiu:

```csharp
observedPlayer.SetupHitColliders();          // repromove BodyPartCollider p/ "HitCollider"
observedPlayer.RecalculateEquipmentParams(); // → PlayerBones.SetArmorPlateCollidersState(mask)
```

Decisões:

- **Alvo `RemoveRagdoll`, não `RevivePlayer`** — `RemoveRagdoll` é o método que causa o dano e cobre **os dois** call sites de §1.5 com um patch só. Patchar `RevivePlayer` deixaria os peers não-revivedores quebrados.
- **Postfix, não Prefix** — tem de rodar **depois** do `SetLayersRecursively` da linha 132, senão é sobrescrito.
- **Acesso 100% por reflection** (`AccessTools.TypeByName` + `AccessTools.Field`) — `ReviveInteractable` é `internal sealed`, e assim o `TRL-Fixes` não passa a depender do assembly do Fika em tempo de build. Mesmo padrão que o ICM já usa nos seus dois patches de revive.
- **Sem guard de headless.** O Fika já faz early-return em headless tanto no `Init` (`:56-61`) quanto no `RemoveRagdoll` (`:116-119`), então o Postfix roda sobre um corpo que nunca teve a layer mexida. `SetupHitColliders` é idempotente (recoleta e reatribui) e não toca em câmera, render nem animator — ver o corpo em §1.1. Um guard por reflection em `FikaBackendUtils.IsHeadless` adicionaria uma superfície de quebra sem resolver risco real.
- **`RecalculateEquipmentParams` incluído** apesar de ser efeito secundário: é a única forma de reativar as placas (`PlayerBones.SetArmorPlateCollidersState`, `Player.cs:30250-30253`), e é o mesmo par que o `Player.Init` usa.

## 3. Hipótese refutada durante esta spec

A dúvida legítima do relato — *por que os bots pareciam acertar e um jogador não?* — tinha como hipótese de reconciliação que qualquer troca de equipamento restauraria a hitbox, deixando só uma janela curta quebrada.

**Refutada:** `RecalculateEquipmentParams` chama `PlayerBones.SetArmorPlateCollidersState`, mas **não** chama `SetupHitColliders` nem `method_92`. Uma troca de equipamento reativa as placas e deixa os `BodyPartCollider` na layer errada. Não existe caminho de auto-recuperação: sem este fix, a hitbox balística fica quebrada **até o fim da raid**.

Portanto a divergência bot × jogador **continua sem explicação no código**, e a verificação em jogo tem de medir as duas fontes de dano separadamente (§Verificação da spec funcional). O fix é correto de qualquer forma — restaura a invariante do §1.3 — mas o item só fecha quando a divergência for explicada ou deixar de se reproduzir.

## 4. Verificação

Cenário **C2** do roteiro happy-flow do ICM. Log esperado no boot: `TRL-Fixes: Hook no ReviveInteractable.RemoveRagdoll aplicado com sucesso!` — sem ele, o patch não pegou e qualquer conclusão do teste é inválida.
