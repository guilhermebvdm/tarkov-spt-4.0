# 004 — Reformular Chance de Desmembramento por Calibre e Parte do Corpo · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [004-reformular-chance-desmembramento-calibre-01-spec.md](004-reformular-chance-desmembramento-calibre-01-spec.md)
**Criado:** 2026-09-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

**Memória consultada:** snapshot de 2026-09-10 (Sessão 8) — pendências [P-7.1]🔴/[P-7.2]🟡/[P-7.3]🟡 (item 002) e [P-8.1]🟡 (item 003) não afetam este item. **Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (sempre). `fika-packet-desync-prevention-plan.md` — não cria pacote novo, mas o §7 discute o padrão de convergência entre peers que já existe (`isFromNetwork`), reaproveitado sem alteração.

**Confirmação visual do usuário (assets de cabeça):** `Head_3` = cabeça arrancada (sem cabeça, coto de pescoço). `Head_1`/`Head_2` = cabeça estourada/partida (cabeça permanece, malha danificada). Isso resolve o corner case da spec funcional sobre o asset — o efeito de "estourar" **entra no escopo** deste item.

## 0. Como o calibre 12/20/23x75 realmente dispara múltiplos projéteis (evidência que baseia o mecanismo A)

[`BallisticsCalculator.cs:161-168`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Ballistics/BallisticsCalculator.cs#L161-L168):

```csharp
int projectileCount = ammo.ProjectileCount;
int fireIndex = int_0++;
for (int i = 0; i < projectileCount; i++)
{
    preallocatedShots.Add(CreateShot(ammo, origin, (direction * 100f + ammo.buckshotDispersion * ...).normalized, fireIndex, playerProfileID, weapon, speedFactor, i * 2));
}
```

Confirma: **todos os pellets de um disparo de espingarda compartilham o mesmo `fireIndex`**, atribuído uma vez antes do loop, e cada pellet vira sua própria instância de `EftBulletClass` ([`EftBulletClass.cs:144,492`](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L144)) criada no mesmo frame. Isso é a base do mecanismo A: agrupar por `(player.Id, shot.FireIndex, bodyPartType)` e somar o momento de cada pellet que chega, exatamente o mesmo padrão de agrupamento que `LimbKillPatch.cs:17-22` (`_evaluatedLivingVolleys`) já usa pra evitar re-rolar por pellet — só que em vez de "deduplicar", este mecanismo **acumula**.

**Por que não precisa esperar "todos os pellets chegarem":** como o momento só pode aumentar (nunca diminui) à medida que mais pellets do mesmo `FireIndex`/parte do corpo chegam, dá pra checar o threshold a cada pellet processado e agir assim que ele for cruzado — sem precisar coordenar "quando o disparo terminou". Pellets que chegam depois de já ter desmembrado aquela parte simplesmente não fazem nada (guarda já existente no `DismemberLimb`, linha 232: `if (val.localScale == RagdollHelperClass.limbSize) continue;`).

## 1. Estratégia

- **Reestruturar `KillPatch.calibers` de `Dictionary<string, float>` pra um tipo com 4 campos** (Braço, Perna, Cabeça-arranca, Cabeça-estourar), carregado de um `VD_Calibers.json` reformulado.
- **Três mecanismos de resolução de chance, nesta ordem de prioridade** (implementados como um único método `ResolveDismemberChance`):
  1. **C — exceção por munição:** `Dictionary<string, DismemberChances> caliberExceptions`, chave = `ammo.Name` (ex.: `"patron_12gauge_grizzly40"`) — checado primeiro.
  2. **A — multi-projétil dinâmico:** se `ammo.ProjectileCount > 1` e não achou em C, usa o acumulador de momento por `(player, FireIndex, bodyPart)`.
  3. **B — tabela por calibre:** fallback pra `ammo.ProjectileCount == 1` sem exceção, usa `caliber` (igual hoje, só que por parte do corpo).
  4. **Fallback final:** calibre/munição não encontrado em nenhum dos três → **0** (nunca um default alto — corrige o `0.5f` implícito de hoje, ver spec funcional corner case).
- **Cabeça vira duas decisões independentes**, não uma: primeiro rola "arranca" (se vencer, chama `DismemberLimb` existente com `capAsset` fixo `"Head_3"`); se não venceu, rola "estourar" (se vencer, chama um método **novo e mais leve**, `BurstHead`, que não encolhe a cabeça real nem mexe em rigidbody/collider/joint — só sobrepõe um dos props `Head_1`/`Head_2` e aciona sangue/drop de equipamento).
- **Alternativas descartadas:**
  - Fazer o mecanismo A esperar `shot.IsShotFinished` de todos os pellets antes de decidir (via coroutine coordenadora) — descartado por complexidade desnecessária; o acumulador monotônico resolve sem coordenação (ver §0).
  - Guardar a chance de "estourar"/"arranca" como uma probabilidade condicional (ex.: "estourar dado que não arrancou") em vez de duas rolagens independentes — descartado por ficar mais difícil de calibrar manualmente na planilha; duas rolagens independentes (arranca primeiro, estourar como fallback) é mais direto de explicar e ajustar.

## 2. Pontos de patch

| Alvo (mod) | Tipo | Motivo |
|---|---|---|
| [`KillPatch.cs:88-92`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L88-L92) | Edição inline | Troca leitura de `calibers.TryGetValue` por `ResolveDismemberChance` (novo método) |
| [`KillPatch.cs:156-159`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L156-L159) (caso `bodyPartType == 0`, morte por tiro de cabeça) | Edição inline | Duas rolagens (arranca/estourar) em vez de sempre desmembrar com cap aleatório |
| [`LimbKillPatch.cs:191-198`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L191-L198) (estratégia B, cabeça pós-morte) | Edição inline | Mesma reestruturação de duas rolagens |
| [`LimbKillPatch.cs:233-250`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L233-L250) (ramo "Dead corpses branch", calcula `chance` por calibre e chama `DismemberLimb` pra qualquer parte do corpo, cabeça incluída) | Edição inline | Interceptar `dismemberPart.Value == (EBodyPart)0` **antes** do cálculo genérico de `chance` — cabeça passa a usar `ResolveHeadOutcome`/`BurstHead`; braço/perna continuam nesse bloco só trocando a fonte da `chance` pra `ResolveDismemberChance` |

**Nota de linha (pós item 003):** as linhas de `LimbKillPatch.cs` citadas acima já refletem o arquivo atual — a guarda de Boss/escolta do item 003 (`LimbKillPatch.cs:66-79`) desloca tudo que vem depois em +9 linhas em relação ao estado do arquivo antes daquele item. Reconferido nesta spec.
| `GameStartedPatch.cs:32` (contexto — bloco de `.Clear()` de estado raid-scoped) | Edição inline | Adicionar `.Clear()` do novo acumulador de momento multi-projétil |
| `VisceralEntry.cs` `ParseDismembermentJson` ([linha 493-538](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L493-L538)) | Edição inline | Parsear o novo schema aninhado (por parte do corpo) em vez do `Dictionary<string,float>` plano |

Nenhum novo alvo Harmony — todas as mudanças são em código já patcheado pelo mod.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. Os valores continuam vindo de `VD_Calibers.json` (dado externo, não `ConfigEntry`), mantendo a mesma filosofia atual do arquivo (calibração de dados de jogo, não preferência de usuário no F12).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | MODIFICAR | Novo tipo `DismemberChances`, novo método `ResolveDismemberChance`, novo acumulador de momento, novo método `BurstHead`, reestruturação das 2 rolagens de cabeça, troca do `calibers` de `float` pra `DismemberChances` |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | MODIFICAR | Reestruturação equivalente no ramo pós-morte (estratégias A/B) |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` | MODIFICAR | Limpar o acumulador de momento no início de cada raid |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | MODIFICAR | `ParseDismembermentJson` lê o novo schema aninhado |
| `mods/VisceralCombat/assets/ssh/VD_Calibers.json` | MODIFICAR | Novo schema: `dismember_calibers` aninhado por parte do corpo, novo bloco `dismember_exceptions` (mecanismo C), novo bloco `dismember_multiprojectile_curve` (mecanismo A) |

## 5. Novo schema de `VD_Calibers.json` (mecanismos B e C)

```json
{
  "dismember_calibers": {
    "Caliber556x45NATO": { "arm": 0, "leg": 0, "head_off": 0, "head_burst": 0.5 },
    "Caliber127x99":     { "arm": 1, "leg": 1, "head_off": 1, "head_burst": 1 }
  },
  "dismember_exceptions": {
    "patron_12gauge_grizzly40": { "arm": 1, "leg": 1, "head_off": 1, "head_burst": 1 },
    "patron_23x75_barricade":   { "arm": 1, "leg": 1, "head_off": 1, "head_burst": 1 },
    "patron_23x75_star":        { "arm": 0, "leg": 0, "head_off": 0, "head_burst": 0 }
  },
  "dismember_multiprojectile_curve": {
    "momentum_min_ns": 3.0,
    "momentum_max_ns": 15.0,
    "head_burst_multiplier": 1.5
  }
}
```

- `dismember_calibers`/`dismember_exceptions`: mapeados pra `Dictionary<string, DismemberChances>`, chave = `Caliber` (bruto, com prefixo — igual hoje) pra calibers, `ammo.Name` (nome interno) pra exceções.
- `dismember_multiprojectile_curve`: **um único** par min/max em N·s (não por parte do corpo) — abaixo de `momentum_min_ns` a chance é 0, acima de `momentum_max_ns` a chance é 1, interpolação linear no meio. Aplicado igualmente às 4 partes do corpo (a diferenciação por parte já existe no critério de escolha do alvo do tiro, não precisa duplicar aqui). `head_burst_multiplier` é o fator que deriva a chance de "estourar" a partir da chance de "arranca" quando a munição é multi-projétil (ver §6.1, `ResolveHeadOutcome`). **Correção pós-review (`PA-01-03`):** os 3 valores são lidos do JSON (via `ParseDismembermentJson`), **não** mais constantes hardcoded no C# — permite recalibrar sem recompilar, mesmo padrão do resto da tabela.

## 6. Stubs de código

### 6.1 `KillPatch.cs` — novo tipo e método de resolução

```csharp
internal struct DismemberChances
{
    public float Arm;
    public float Leg;
    public float HeadOff;
    public float HeadBurst;
}

// Trocam os Dictionary<string, float> calibers antigos:
public static Dictionary<string, DismemberChances> calibers = new Dictionary<string, DismemberChances>();
public static Dictionary<string, DismemberChances> caliberExceptions = new Dictionary<string, DismemberChances>();

// ref: PA-01-03 — lidos de VD_Calibers.json ("dismember_multiprojectile_curve") via
// ParseDismembermentJson, não hardcoded; valores abaixo são só o default de fallback
// caso a chave não exista no JSON (evita NullReference/0 se o usuário esquecer o bloco).
public static float MultiProjectileMomentumMin = 3.0f;
public static float MultiProjectileMomentumMax = 15.0f;
public static float HeadBurstMultiplier = 1.5f;

// Acumulador do mecanismo A — chave: (player.Id << 40) | ((long)FireIndex << 8) | (byte)bodyPartType
// ref: BallisticsCalculator.cs:163-167 — todos os pellets do mesmo disparo compartilham FireIndex
private static readonly Dictionary<long, float> _multiProjectileMomentum = new Dictionary<long, float>();

public static void ClearMultiProjectileMomentum()
{
    _multiProjectileMomentum.Clear();
}

private static long MakeMomentumKey(int playerId, int fireIndex, EBodyPart bodyPart)
{
    return ((long)playerId << 40) | ((long)fireIndex << 8) | (byte)bodyPart;
}

internal enum DismemberOutcome { None, HeadOff, HeadBurst }

/// <summary>
/// Soma o momento (N.s) de um pellet no acumulador de (player, disparo, parte do corpo) e
/// retorna o total acumulado até agora. Chamar EXATAMENTE UMA VEZ por (pellet, parte do
/// corpo) — nunca duas, senão o mesmo pellet conta duas vezes (ver ResolveHeadOutcome, que
/// evita isso lendo o acumulado uma única vez e derivando as duas chances de cabeça dele).
/// </summary>
private static float AccumulateMultiProjectileMomentum(AmmoItemClass ammo, int playerId, int fireIndex, EBodyPart bodyPart)
{
    float massKg = (ammo.BulletMassGram > 0f) ? (ammo.BulletMassGram / 1000f) : 0f;
    float speed = (ammo.InitialSpeed > 0f) ? ammo.InitialSpeed : 0f;
    float pelletMomentum = massKg * speed;

    long key = MakeMomentumKey(playerId, fireIndex, bodyPart);
    float accumulated = _multiProjectileMomentum.TryGetValue(key, out float existing) ? existing + pelletMomentum : pelletMomentum;
    _multiProjectileMomentum[key] = accumulated;

    if (_multiProjectileMomentum.Count > 2000) _multiProjectileMomentum.Clear(); // guarda de segurança, mesmo padrão de _evaluatedLivingVolleys

    return accumulated;
}

private static float MomentumToChance(float accumulated)
{
    if (accumulated <= MultiProjectileMomentumMin) return 0f;
    if (accumulated >= MultiProjectileMomentumMax) return 1f;
    return (accumulated - MultiProjectileMomentumMin) / (MultiProjectileMomentumMax - MultiProjectileMomentumMin);
}

/// <summary>
/// Resolve a chance de desmembrar braço/perna pra uma munição/tiro específico, seguindo a
/// precedência: (C) exceção por munição -> (A) multi-projétil dinâmico -> (B) tabela por
/// calibre -> 0 (nunca um default alto). NÃO usar para cabeça — ver ResolveHeadOutcome.
/// </summary>
internal static float ResolveDismemberChance(AmmoItemClass ammo, EBodyPart bodyPart, int playerId, int fireIndex)
{
    if (ammo == null) return 0f;

    // (C) Exceção por munição — checada primeiro, nome exato do template.
    // ref: KillPatch.cs (IsHeavyCaliberNoAgony já usa ammo.Name com sucesso, mesmo padrão aqui)
    if (!string.IsNullOrEmpty(ammo.Name) && caliberExceptions.TryGetValue(ammo.Name, out DismemberChances exc))
    {
        return SelectByBodyPart(exc, bodyPart);
    }

    // (A) Multi-projétil dinâmico — soma o momento dos pellets do mesmo disparo/parte do corpo.
    if (ammo.ProjectileCount > 1)
    {
        float accumulated = AccumulateMultiProjectileMomentum(ammo, playerId, fireIndex, bodyPart);
        return MomentumToChance(accumulated);
    }

    // (B) Tabela por calibre — fallback pra projétil único sem exceção.
    string caliber = ammo.Caliber;
    if (!string.IsNullOrEmpty(caliber) && calibers.TryGetValue(caliber, out DismemberChances cal))
    {
        return SelectByBodyPart(cal, bodyPart);
    }

    return 0f; // fallback final: nunca um default alto (corrige o 0.5f implícito de hoje)
}

private static float SelectByBodyPart(DismemberChances c, EBodyPart bodyPart) => (int)bodyPart switch
{
    3 or 4 => c.Arm,
    5 or 6 => c.Leg,
    _ => c.HeadOff, // Head (0) tratado à parte por DismemberOutcome — ver ResolveHeadOutcome
};

/// <summary>
/// Cabeça é uma decisão de 2 rolagens independentes: primeiro "arranca" (Head_3, remoção
/// completa via DismemberLimb), se não vencer tenta "estourar" (Head_1/2, via BurstHead,
/// sem remover a cabeça real). No caso multi-projétil, o momento do pellet é somado UMA
/// ÚNICA VEZ (AccumulateMultiProjectileMomentum) e as duas chances derivam da mesma leitura
/// — nunca chama o acumulador duas vezes para o mesmo pellet.
/// </summary>
internal static DismemberOutcome ResolveHeadOutcome(AmmoItemClass ammo, int playerId, int fireIndex)
{
    if (ammo == null) return DismemberOutcome.None;

    float offChance;
    float burstChance;

    if (!string.IsNullOrEmpty(ammo.Name) && caliberExceptions.TryGetValue(ammo.Name, out DismemberChances exc))
    {
        offChance = exc.HeadOff;
        burstChance = exc.HeadBurst;
    }
    else if (ammo.ProjectileCount > 1)
    {
        float accumulated = AccumulateMultiProjectileMomentum(ammo, playerId, fireIndex, EBodyPart.Head); // UMA chamada só
        offChance = MomentumToChance(accumulated);
        burstChance = Mathf.Min(1f, offChance * HeadBurstMultiplier); // estourar satura mais cedo que arrancar — ver Riscos §8
    }
    else if (!string.IsNullOrEmpty(ammo.Caliber) && calibers.TryGetValue(ammo.Caliber, out DismemberChances cal))
    {
        offChance = cal.HeadOff;
        burstChance = cal.HeadBurst;
    }
    else
    {
        return DismemberOutcome.None;
    }

    if (UnityEngine.Random.value <= offChance) return DismemberOutcome.HeadOff;
    if (UnityEngine.Random.value <= burstChance) return DismemberOutcome.HeadBurst;
    return DismemberOutcome.None;
}
```

### 6.1.1 `VisceralEntry.cs` — `ParseDismembermentJson` lê o bloco da curva (correção `PA-01-03`)

Adicionar ao método existente ([`VisceralEntry.cs:493-538`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L493-L538)), no mesmo `foreach (Dictionary<string, object> item in list)`:

```csharp
if (item.ContainsKey("dismember_multiprojectile_curve"))
{
    Dictionary<string, float> curve = JsonConvert.DeserializeObject<Dictionary<string, float>>(item["dismember_multiprojectile_curve"].ToString());
    if (curve.TryGetValue("momentum_min_ns", out float min)) KillPatch.MultiProjectileMomentumMin = min;
    if (curve.TryGetValue("momentum_max_ns", out float max)) KillPatch.MultiProjectileMomentumMax = max;
    if (curve.TryGetValue("head_burst_multiplier", out float mult)) KillPatch.HeadBurstMultiplier = mult;
}
```

Segue exatamente o mesmo padrão dos blocos `dismember_calibers`/`bleed_calibers` já existentes no método — só populando 3 campos escalares em vez de um dicionário inteiro. Os `TryGetValue` preservam os defaults declarados em `KillPatch.cs` (§6.1) se a chave não existir no JSON.

### 6.2 `KillPatch.cs` — `BurstHead` (novo, efeito "estourar")

```csharp
/// <summary>
/// Cabeça "estourada": a cabeça real NÃO é escondida/encolhida (ao contrário de
/// DismemberLimb) — só sobrepõe um dos props Head_1/Head_2 e aciona sangue/drop de
/// equipamento, mantendo a cabeça funcionalmente intacta na cena.
/// </summary>
private static void BurstHead(Player player, Vector3 direction, bool isFromNetwork = false)
{
    if (player == null) return;

    if (!isFromNetwork)
    {
        VisceralCombat.Combined.Classes.VisceralNetworkUtils.SendDismemberment(player, direction, EBodyPart.Head, "head_burst", $"Head_{UnityEngine.Random.Range(1, 3)}", Array.Empty<string>());
    }

    // TODO confirmar: ponto de ancoragem do prop Head_1/2 (mesma referência de
    // player.PlayerBody.SkeletonRootJoint usada em DismemberLimb:396-398) — não faz o
    // scaling/rigidbody/collider/joint que DismemberLimb faz, só posiciona e aplica skin.
    GameObject capPrefab = VisceralEntry.Instance?.effectContainer?.goreCaps?
        .FirstOrDefault(cap => cap != null && (cap.name == "Head_1" || cap.name == "Head_2"));
    if (capPrefab == null) return;

    GameObject instance = Object.Instantiate(capPrefab);
    Skin skin = instance.GetComponentInChildren<Skin>();
    if (skin != null)
    {
        skin.Init(player.PlayerBody.SkeletonRootJoint);
        ((AbstractSkin)skin).ApplySkin();
    }

    // Reusa os mesmos efeitos de sangue já usados no desmembramento de cabeça existente.
    Transform headTransform = player.PlayerBody.SkeletonRootJoint; // TODO confirmar: bone exato da cabeça, não a raiz do skeleton
    SpawnOldVolumetricBlood(headTransform, direction, 1f);
    SpawnArterialSprays(player, headTransform, direction, "head");

    DropHeadEquipment(player); // ref: item 002 — "estourar" também derruba capacete/óculos
}
```

**`TODO confirmar` explícito (regra AP-09):** o ponto exato de ancoragem visual do prop `Head_1`/`Head_2` (`SkeletonRootJoint` é um palpite baseado no padrão de `DismemberLimb:396-398`, que usa o mesmo campo pra ancorar os caps de membro — mas cabeça pode precisar de um bone mais específico, tipo `PlayerBones.Head`, pra não sobrepor errado quando a cabeça real continua lá). Resolver isso **visualmente em jogo** durante o `/code-mod`, não só por leitura de código — é exatamente o tipo de coisa que só aparece testando.

### 6.3 `KillPatch.cs` — reestruturação do caso de cabeça em `Postfix`

Contexto: [`KillPatch.cs:140-160`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L140-L160), dentro do `switch ((int)bodyPartType)`:

```csharp
case 0:
    DismemberOutcome headOutcome = ResolveHeadOutcome(currentAmmoTemplate as AmmoItemClass, __instance.Id, damageInfo.FireIndex);
    if (headOutcome == DismemberOutcome.HeadOff)
    {
        DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, "Head_3", Array.Empty<string>(), out affectedLimbs);
    }
    else if (headOutcome == DismemberOutcome.HeadBurst)
    {
        BurstHead(__instance, damageInfo.Direction);
    }
    break;
```

**Confirmado (era risco 🟡, fechado):** `DamageInfoStruct.cs:31` — `public int FireIndex;`, populado em `DamageInfoStruct.cs:109` (`FireIndex = shot.FireIndex;`) diretamente a partir do `EftBulletClass` que causou o dano. `damageInfo.FireIndex` está disponível em `KillPatch.Postfix` sem nenhum ajuste — o mecanismo A funciona igual nos dois pontos (`KillPatch.Postfix` e `LimbKillPatch.ProcessLimbKill`), usando a mesma chave de agrupamento em ambos.

## 7. Fluxo de dados

```
[A] Tiro atinge braço/perna/cabeça de um bot/player
  → [B] KillPatch.Postfix (morte) ou LimbKillPatch.ProcessLimbKill (pós-morte/vivo em perna)
    → [C] ResolveDismemberChance(ammo, bodyPart, playerId, fireIndex)
      → (C) caliberExceptions por ammo.Name?  → retorna direto
      → (A) ProjectileCount > 1?  → acumula momento em _multiProjectileMomentum, interpola 0-1
      → (B) calibers por ammo.Caliber?  → retorna direto
      → nenhum? → 0
    → [D] Random.value <= chance? → DismemberLimb (braço/perna/Head_3) ou BurstHead (cabeça)
      → [E] DismemberLimb: pipeline existente (scale, rigidbody, collider, joint, gore cap,
            sangue) — inalterado, só o valor de entrada (chance) muda de fonte
      → [E'] BurstHead (só cabeça, novo): sem scale/rigidbody/collider/joint — só cap +
            sangue + DropHeadEquipment (item 002)
```

## 8. Riscos e dependências

- **✅ Resolvido no design — contagem dupla de momento na rolagem de cabeça.** A primeira versão do stub chamava uma função que soma-e-retorna o momento duas vezes (uma por `isBurst`), contando o mesmo pellet duas vezes. Corrigido separando `AccumulateMultiProjectileMomentum` (soma, chamada uma vez) de `MomentumToChance` (leitura pura, sem side-effect) — `ResolveHeadOutcome` agora soma uma única vez e deriva as duas chances (`offChance`/`burstChance`) da mesma leitura. **Ainda em aberto, esse sim precisa de calibração:** o multiplicador `1.5x` usado pra "estourar satura mais cedo que arrancar" é um palpite de proporção, não um valor testado — validar em jogo se essa proporção entre as duas chances parece certa, ou se o usuário prefere valores independentes (ex.: uma segunda curva momentum→chance só pra "estourar", em vez de derivar por multiplicador da curva de "arranca").
- **✅ Resolvido — `KillPatch.Postfix` tem `FireIndex` disponível.** `DamageInfoStruct.cs:31/109` confirma `damageInfo.FireIndex`, populado a partir do `EftBulletClass` do tiro. O mecanismo A funciona igual em `KillPatch.Postfix` e `LimbKillPatch.ProcessLimbKill`, sem degradação.
- **🟡 Threshold de momento do mecanismo A (`momentum_min_ns`/`momentum_max_ns`) é placeholder.** Os valores `3.0`/`15.0` em §5 foram escolhidos só pra deixar o stub compilável e coerente com a faixa de momento já vista na planilha de referência (calibre 12 buckshot isolado ~1-1.4 N·s por pellet, várias pellets somadas facilmente cruzam 15) — **precisam da calibração do usuário** antes de fechar o item (mesmo processo dos valores da tabela B).
- **Nenhum pacote de rede novo.** O fluxo de rede (`SendDismemberment`/`isFromNetwork`) é reaproveitado sem alteração — mesma arquitetura de hoje, mesmo padrão de convergência entre peers (ver spec funcional, corner case sobre consistência entre peers).
- **Compatibilidade com item 002:** `BurstHead` chama `DropHeadEquipment` (método já existente do item 002) — nenhuma duplicação, reuso direto.
- **Compatibilidade com item 003:** o bloqueio de Boss/escolta vivos (`LimbKillPatch.cs:66-79`) roda **antes** de qualquer chamada a `ResolveDismemberChance` — nenhuma interação, guardas independentes.
- **🟠 Achado no `/code-mod` (`CR-01-01`, ver `04-code-review-01.md`) — possível dupla contagem de momento entre `KillPatch.Postfix` e `LimbKillPatch.ProcessLimbKill`.** As duas entradas processam hits em corpos já mortos por caminhos independentes (`ApplyDamageInfo` vs. `BallisticsCalculator.Shoot`); se ambas processarem o mesmo pellet fisicamente, o acumulador de momento soma em dobro. No código anterior isso era inofensivo pra `DismemberLimb` (guarda de idempotência por escala), mas o acumulador novo não tem essa guarda. Impacto limitado (chance um pouco mais alta que a curva calibrada prevê, não crash) — requer validação em jogo antes de fechar como não-problema, ou um ajuste de idempotência se confirmado.

## 9. Checklist de implementação

- [x] Criar `DismemberChances` (struct), `ResolveDismemberChance`, `ResolveHeadOutcome`, `BurstHead`, acumulador de momento + `ClearMultiProjectileMomentum` em `KillPatch.cs`.
- [x] Atualizar `ParseDismembermentJson` (`VisceralEntry.cs`) pro novo schema aninhado (§5).
- [x] Reformular `VD_Calibers.json` com os valores da planilha de referência (29 calibres) + exceções (mecanismo C: `Barrikada`/`Zvezda` do 23x75) + thresholds do mecanismo A (placeholder, calibração do usuário ainda pendente).
- [x] Adicionar `.Clear()` do acumulador em `GameStartedPatch.cs`.
- [x] Editar os 2 pontos de cabeça (`KillPatch.cs` caso 0, `LimbKillPatch.cs` "Dead corpses branch") pra usar `ResolveHeadOutcome` em vez do desmembramento incondicional atual.
- [x] Editar os pontos de braço/perna (`KillPatch.cs`, `LimbKillPatch.cs`) pra usar `ResolveDismemberChance` em vez do `calibers.TryGetValue` plano.
- [x] Corrigido durante o `/code-mod` (não previsto na spec técnica original): `OnDismembermentPacketClient` despachando "head_burst" pra `BurstHead`, `AmmoItemClass.Caliber` sem prefixo vs `AmmoTemplate.Caliber` com prefixo, `AmmoItemClass.Name` vs `AmmoItemClass.AmmoTemplate.Name`, `SkeletonRootJoint` não é `Transform`.
- [x] Confirmar disponibilidade do `FireIndex` em `KillPatch.Postfix` — resolvido via `damageInfo.FireIndex` (`DamageInfoStruct.cs:31/109`), sem gap.
- [x] Compilar (`dotnet build ... -c Release -o mods/VisceralCombat/builds/...` — sem instalar no jogo).
- [ ] Validar em jogo: cabeça "estoura" visualmente diferente de "arranca" (confirma o mapeamento `Head_3` vs `Head_1/2`); calibre 12 buckshot acumula corretamente (vários pellets no mesmo membro desmembram mais que um só); calibre fraco (5.56/9mm) nunca desmembra braço/perna; `.50 BMG` desmembra com alta chance incluindo cabeça; bloqueio de Boss (item 003) continua funcionando; drop de capacete/óculos (item 002) dispara tanto em "arranca" quanto em "estourar".

## 10. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Novo acumulador `_multiProjectileMomentum` limpo em `GameStartedPatch.cs` (mesmo padrão de `_evaluatedLivingVolleys`, já existente e testado) |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Mesma lógica de hoje (reage a qualquer player/bot por design) — nenhuma mudança de escopo de quem é afetado |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum novo alvo Harmony; edições são em código já patcheado do mod |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `BurstHead` reusa `Skin.Init`/`ApplySkin` (mesma API de `DismemberLimb`) e `DropHeadEquipment`/`SpawnOldVolumetricBlood`/`SpawnArterialSprays` (métodos já existentes do mod, não reimplementados) |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Acumulador limpo no início de cada raid (`GameStartedPatch.cs`); guarda de segurança adicional se ultrapassar 2000 entradas (mesmo padrão de `_evaluatedLivingVolleys`) |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova; valores vêm de JSON externo, calibração é responsabilidade do usuário (documentada como risco pendente em §8, não ambiguidade de código) |
| 7 | Reentry-guard em re-invocação de método patcheado — AP-07 | N/A | Nenhuma re-invocação de método patcheado |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | ✅ | Chave do acumulador (`playerId + fireIndex + bodyPart`) é resolvida fresca a cada chamada, sem cache de contexto entre disparos diferentes |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `BallisticsCalculator.cs:163-167` (fireIndex compartilhado), `EftBulletClass.cs:144,492` (FireIndex), `ammo.Name`/`ProjectileCount`/`BulletMassGram`/`InitialSpeed` (já usados e compilando em `IsHeavyCaliberNoAgony`) — todos lidos/confirmados. 2 `TODO confirmar` explícitos deixados nos stubs (§6.2, §6.3) para pontos que só se resolvem em `/code-mod`/teste in-game, não por leitura estática |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT |
| 11 | Pacote FIKA próprio: envelope, `TryGet*`, `Valid`, registro por instância, zero `UnregisterPacket` — AP-11 | N/A | Nenhum pacote novo; reusa `SendDismemberment` existente sem alteração de formato |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-10 | `/review-technical-spec` rodada 01: 2 pontos já resolvidos durante a redação (contagem dupla de momento, `FireIndex` em `KillPatch.Postfix`), 1 🟡 aplicado (thresholds/multiplicador movidos pro JSON, `PA-01-03`), 1 🟢 aceito como `TODO confirmar` visual (`PA-01-04`). Citações de linha do `LimbKillPatch.cs` reconferidas contra o arquivo pós-item-003. |
