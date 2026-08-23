# 089 — perf — Rodada 01 de otimização · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [089-perf-rodada-01-01-spec.md](089-perf-rodada-01-01-spec.md)
**Criado:** 2026-08-22

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: `references/eft-decompiled/Assembly-CSharp/`. ⚠️ **O dump é gitignored e NÃO existe neste worktree** — a auditoria e esta spec o leram do checkout principal `C:\Repos\spt\tarkov-spt-4.0\references\eft-decompiled\`. As referências abaixo são citadas como `arquivo.cs:linha` em texto, sem link. Para gerar o dump aqui: `bash scripts/decompile-eft.sh`.

## 1. Estratégia

Rodada de **não-regressão**: nenhum alvo Harmony novo, nenhum comportamento novo. As mudanças são de quatro naturezas:

1. **Eliminar trabalho desnecessário** — `AUD-01-01` (desistir cedo quando o Menu-Overhaul não está presente), `AUD-01-05` (não montar string de log que ninguém pediu), `AUD-01-07d`.
2. **Cache / reutilização** — `AUD-01-08` (cache de textura limitado por ícone), `AUD-01-04` (delegate compilado), `AUD-01-07c`.
3. **Reduzir custo unitário do gate** — `AUD-01-02` (id de enum em vez de comparação de string), `AUD-01-06`.
4. **Reduzir frequência** — `AUD-01-01` (poll a cada 3 frames).

`AUD-01-03` é o único **refactor estrutural**: reduzir o número de patches por alvo. Fica por último na ordem de implementação, porque é o de maior risco de regressão de balance.

> **Revisão técnica 01 (PA-01-07): `AUD-01-07b` foi DROPADO desta rodada.** Trocar o `yield return null` do `AdrenalineState.WatchWindow` por `WaitForSeconds` renderia ~30 µs por janela de 25 s e, em troca, atrasaria em até 50 ms a detecção do fechamento — justamente o que o watcher existe para evitar (`ForceReloadResync`) —, alocaria um objeto por iteração e divergiria sob `timeScale`. Cai na proibição da `spt-performance-analysis` §8 (micro-otimização de código frio). Registrado no relatório de auditoria como ❌ Rejeitado.

**Alternativas descartadas:**
- *`AUD-01-02` via dicionário `string→enum`:* um lookup de dicionário não é mais barato que comparar 12 caracteres. O ganho só existe se os **call-sites** passarem o enum. Descartado.
- *`AUD-01-08` via uma textura única por ícone, re-tingida in place:* seria o desenho ideal (zero alocação e zero risco de sprite destruído), mas **é inviável**: o mesmo `iconFile` precisa de **duas variantes vivas ao mesmo tempo** — o brasão com gradiente (`IconGradient` → `top ≠ bottom`, `ClassIdentityView.cs:134`) e a marca d'água chapada (`top == bottom`, `PerksPanelView.cs:242`), ambas visíveis juntas na aba CLASS. Descartado em favor de LRU por ícone.
- *`AUD-01-08` voltando ao `ClassIconGradient` (`BaseMeshEffect`, zero alocação por cor):* reabriria o bug que o 06-fix-02 fechou (o efeito falha em `Image` criada em runtime, caso do ícone do menu). Descartado.
- *`AUD-01-07a` (subscrever/desinscrever `GClass897.OnShoot` por classe):* mexer na subscrição arrisca deixar o perk morto numa raid. Depois do `AUD-01-02` o gate custa uma comparação de inteiros e o achado se dissolve. Descartado — resolvido por consequência.

## 2. Pontos de patch

**Nenhum alvo Harmony novo.** A tabela abaixo registra os alvos **existentes** que mudam de forma (consolidação do `AUD-01-03`); todos já estão em produção e foram reconfirmados no dump.

| Alvo (Assembly) | Hoje | Depois | Motivo |
|---|---|---|---|
| `EFT/Player.cs` → `Player.ApplyDamageInfo` | 4 patches (3 Prefix + 1 Postfix) | **1 Prefix + 1 Postfix** | Gate resolvido 1× por evento de dano |
| `EFT.Animations/ProceduralWeaponAnimation.cs` → `Shoot` | 4 Prefix c/ 3 `[HarmonyPriority]` + estático `StrBefore` | **2 Prefix** (`First` + `Last`) | Gate 4× → 2×; ordem interna vira sequência de statements. ⚠️ **PA-01-01:** NÃO consolidar em 1 — as prioridades de fronteira (`First`/`Last`) ordenam contra **mods externos**, não só contra os nossos |
| `EFT/Player.cs:13xxx` → `Player.FirearmController.SetAnimatorAndProceduralValues` | 2 Prefix + 2 Postfix (3 patches) | **1 Prefix + 1 Postfix** | `__state` único; elimina risco de duplo escalonamento de `BuffInfo.ReloadSpeed` |
| `EFT/Player.cs:12062` → `Player.FirearmController.TotalErgonomics` (getter) | 2 Postfix | **1 Postfix** | Gate 1× por leitura |

> ✅ **Verificado na review 02 (PA-02-06) — nenhuma outra fronteira de prioridade é movida.** `grep -rn "HarmonyPriority" modded/Client/` devolve prioridade explícita em exatamente quatro lugares: `RecoilFloorPatch.cs:41` (`First`), `RecoilFloorPatch.cs:68` (`Last`) e `WeaponMasteryPatches.cs:116` (`High`) — **todos em `PWA.Shoot`**, tratados pelo `PA-01-01` — e `ClassMedicPatches.cs:186` (`First`), que é em `ObservedMedsControllerClass.method_5`, **alvo fora desta rodada**. Logo: `ApplyDamageInfo`, `SetAnimatorAndProceduralValues` e `TotalErgonomics` **não têm prioridade explícita**, e consolidá-los não move nenhuma fronteira contra mods externos. ⚠️ Consolidar **não** é seguro por regra — foi seguro aqui porque isto foi conferido.

Referências de comportamento que sustentam a consolidação (todas relidas no dump):
- `MovementContext.cs:910` (`MaxSpeed`), `:912` (`SprintingSpeed`), `:4181`/`:2375`/`:2377`/`:2368` (as 3 leituras de `MaxSpeed` por frame de movimento) — justificam o `AUD-01-02`.
- `BotMover.cs:930`/`:985` → `Player.ChangeSpeed` → `MovementState.cs:248` — provam que o getter roda **para bots**, o que fixa o multiplicador de entidades.
- `Player.cs:12062` `TotalErgonomics => gclass849_1.Value` — valor lazy do EFT; o Postfix roda em **toda leitura da propriedade**, não só quando recalcula.
- `BaseSoundPlayer.cs:395` (`PlayClip`) — funil do `AUD-01-04`.
- `EFT/HideoutPlayer.cs` — tipo do `is HideoutPlayer` do `AUD-01-06`.

## 3. Novas propriedades F12 (BepInEx)

**Nenhuma `ConfigEntry` nova.** Toda a instrumentação temporária reusa o toggle existente `0 · General → Perk Diagnostics overlay` (`PerksConfig.DiagnosticsEnabled`, default `false`, marcado `advanced`), via a propriedade `PerkDiag.Enabled`.

Consequência: **`PROPRIEDADES.md` e `PROPERTIES.md` não mudam** nesta rodada. (Se a Fase 4 decidir manter algum bloco de instrumentação ligado por default, aí sim vira `ConfigEntry` própria e exige update — não é o plano.)

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/SkillMultipliers.cs` | MODIFICAR | `AUD-01-02`: enum `EClassId`, campo `_classId`, `LocalClassId`, `Parse`, overloads `IsLocalClass(EClassId)`/`IsClass(EClassId, EClassId)`; resolve o id em `Apply()` e zera em `Reset()` |
| `modded/Client/ClassIdentities.cs` | MODIFICAR | `AUD-01-02`: `Identity.ClassId` (resolvido no `Commit`), `ClassIdOf(Player)` espelhando `ClassNameEnOf` |
| `modded/Client/UI/ClassIconCache.cs` | MODIFICAR | `AUD-01-08`: quantização de cor na chave + LRU por ícone (cap 4) com destruição da entrada evicta + guard de mesmo-frame; INSTR-3 |
| `modded/Client/Patches/MenuClassIdentityPatch.cs` | MODIFICAR | `AUD-01-01`: bail sem Menu-Overhaul, cache do transform, poll a cada 3 frames, espera em tempo real; INSTR-1 |
| `modded/Client/Patches/SilentKnifePatch.cs` | MODIFICAR | `AUD-01-04`: acessor do emissor compilado 1× |
| `modded/Client/Patches/SkillsClassTabPatch.cs` | MODIFICAR | `AUD-01-05`: logs `[053-tab*]` gateados por `PerkDiag.Enabled` |
| `modded/Client/Patches/WeaponMasteryPatches.cs` | MODIFICAR | `AUD-01-06`: `is HideoutPlayer`; `AUD-01-03`: `WeaponMasteryRecoilPatch` e `WeaponMasteryErgoPatch` viram branches |
| `modded/Client/Patches/ClassWeaponPatches.cs` | MODIFICAR | `AUD-01-03`: consolidação de `Shoot`, `SetAnimatorAndProceduralValues` e `TotalErgonomics` |
| `modded/Client/Patches/RecoilFloorPatch.cs` | MODIFICAR | `AUD-01-03` · `PA-01-01`: os 2 patches viram os branches `ApplyFloor` chamados pelo `ShootApplyPatch` (`Priority.Last`); o estático **permanece**, renomeado para `ShootRecoilState.StrBefore` e gravado pelo `ShootCapturePatch` (`Priority.First`) |
| `modded/Client/Patches/BulwarkPatch.cs` | MODIFICAR | `AUD-01-03`: vira branch do Prefix consolidado de `ApplyDamageInfo` |
| `modded/Client/Patches/ClassCombatHealthPatches.cs` | MODIFICAR | `AUD-01-03`: `ExecutionMeleePatch` vira branch; `AUD-01-02` nos gates |
| `modded/Client/Patches/AdrenalineTriggerPatch.cs` | MODIFICAR | `AUD-01-03`: vira branch do Postfix consolidado |
| ~~`modded/Client/AdrenalineState.cs`~~ | **NÃO TOCAR** | `AUD-01-07b` **dropado** (PA-01-07) — o arquivo fica exatamente como está |
| `modded/Client/Patches/SkillPanelPatch.cs` | MODIFICAR | `AUD-01-07c`: cache do texto de tooltip chaveado por `(ESkillId, float, className)` + `ClearTooltipCache()` |
| `modded/Client/PerkDiagnostics.cs` | MODIFICAR | `AUD-01-07d`: cache dos grupos + `ClearGroupCache()`; INSTR-2 (contadores) |
| `modded/Client/Plugin.cs` | MODIFICAR | Registro dos patches (remoção dos `Enable()` que deixam de existir, adição de `ShootCapturePatch`/`ShootApplyPatch`) + `SyncPerfDump()` amarrado ao `SettingChanged` do diagnóstico (INSTR-2) |
| `modded/Client/Patches/ClassMovementPatches.cs`, `ClassSoundPatches.cs`, `ClassMedicPatches.cs`, `MedrosoPatch.cs`, `PackMulePatch.cs`, `QuickHandsPatch.cs`, `CalmSightsPatch.cs`, `StancesArmStaminaBridge.cs`, `CombatMedicSurgery.cs`, `CombatMedicAllyPerks.cs` | MODIFICAR | `AUD-01-02`: call-sites migrados de literal de string para `EClassId` |

**Nenhum arquivo criado. Nenhum arquivo de `modded/Server/` tocado.**

## 5. Stubs de código

### 5.1 `AUD-01-02` — id de classe como enum (a peça central)

```csharp
// modded/Client/SkillMultipliers.cs
/// <summary>
///     ref: AUD-01-02 — id numérico da classe. A classe é imutável durante a raid; resolver 1× no fetch
///     transforma todo gate (42 call-sites, vários per-frame) numa comparação de inteiros.
///     ⚠️ Os nomes abaixo são os MESMOS que o server envia em `displayName.en` e que o F12 usa como chave de cor
///     (PerksConfig.BindClassColor). Fonte única: o Parse abaixo. Nome desconhecido → None + 1 aviso.
/// </summary>
// ⚠️ PA-01-06: visibilidade `internal` — casa com `ClassIdentities.Identity` (internal sealed) e evita
// expor um tipo novo na superfície pública que o ICM consome por reflexão.
internal enum EClassId
{
    None = 0, CombatMedic, Rifleman, Hunter, Stealth, Scavenger, Tank, Naked,
}

private static EClassId _classId;                       // resolvido em Apply(), zerado em Reset()
internal static EClassId LocalClassId => _classId;

private static bool _warnedUnknownClass;

// PA-03-06: `warnUnknown` existe para a checagem de boot (PA-02-03) não consumir o warn-once — ela já emite
// o próprio LogError, mais específico, e o warn-once fica reservado ao caminho de runtime (fetch de peer,
// troca de perfil), que é o cenário para o qual ele foi criado.
internal static EClassId Parse(string? nameEn, bool warnUnknown = true)
{
    if (string.IsNullOrEmpty(nameEn)) return EClassId.None;

    // OrdinalIgnoreCase preservado (mesma semântica do IsClass antigo) — roda 1× por fetch, não por frame.
    if (string.Equals(nameEn, "Combat Medic", StringComparison.OrdinalIgnoreCase)) return EClassId.CombatMedic;
    if (string.Equals(nameEn, "Rifleman",     StringComparison.OrdinalIgnoreCase)) return EClassId.Rifleman;
    if (string.Equals(nameEn, "Hunter",       StringComparison.OrdinalIgnoreCase)) return EClassId.Hunter;
    if (string.Equals(nameEn, "Stealth",      StringComparison.OrdinalIgnoreCase)) return EClassId.Stealth;
    if (string.Equals(nameEn, "Scavenger",    StringComparison.OrdinalIgnoreCase)) return EClassId.Scavenger;
    if (string.Equals(nameEn, "Tank",         StringComparison.OrdinalIgnoreCase)) return EClassId.Tank;
    if (string.Equals(nameEn, "Naked",        StringComparison.OrdinalIgnoreCase)) return EClassId.Naked;

    // Corner case da 01-spec: edition órfã, ou classe nova criada no editor web. Degrada para None
    // (nenhum perk dispara) com 1 aviso por sessão — NUNCA casa com a classe errada.
    if (warnUnknown && !_warnedUnknownClass)
    {
        _warnedUnknownClass = true;
        Plugin.Log?.LogWarning($"[CustomClasses] (AUD-01-02) classe desconhecida '{nameEn}' — perks desligados p/ ela.");
    }

    return EClassId.None;
}

/// <summary>
///     ref: PA-03-01 — inverso do <see cref="Parse"/>: id → nome EN. `switch` puro, sem dicionário.
///     Existe SÓ para o diagnóstico (o <c>PerkDiag.LogPeer</c> precisa do nome legível). Chamar apenas
///     de dentro de <c>if (PerkDiag.Enabled)</c> — nunca no caminho quente.
/// </summary>
internal static string? NameOf(EClassId id) => id switch
{
    EClassId.CombatMedic => "Combat Medic",
    EClassId.Rifleman    => "Rifleman",
    EClassId.Hunter      => "Hunter",
    EClassId.Stealth     => "Stealth",
    EClassId.Scavenger   => "Scavenger",
    EClassId.Tank        => "Tank",
    EClassId.Naked       => "Naked",
    _ => null,
};

/// <summary>ref: AUD-01-02 — o gate quente. Comparação de int; sem alocação, sem string.</summary>
public static bool IsLocalClass(EClassId id)
{
    // ⚠️ PA-02-08 — NÃO remover o EnsureLoaded por parecer redundante depois da migração. Ele é o fetch
    // PREGUIÇOSO para quando nenhum Prefetch rodou (menu, hideout, 1ª raid pós-restart do server). Com o
    // cache frio ele faz um GET HTTP SÍNCRONO — e é exatamente por isso que todo patch que roda para
    // bots/peers coloca o gate de INSTÂNCIA ANTES deste (ref: CalmSightsPatch.cs:51-53, achado CR-F5).
    EnsureLoaded();
    return id != EClassId.None && _classId == id;
}

/// <summary>ref: AUD-01-02 — versão para EMISSOR (peer Fika), espelhando o IsClass(string, string) antigo.</summary>
public static bool IsClass(EClassId classId, EClassId id) => id != EClassId.None && classId == id;
```

Em `Apply(Payload payload)`, logo após `_classNameEn` ser atribuído:

```csharp
_classId = Parse(_classNameEn);   // ref: AUD-01-02 — id resolvido junto com o nome (inclusive no Prefetch)
```

Em `Reset()`: `_classId = EClassId.None;`

**PA-02-03 — `Parse` não é a única fonte de verdade; é a segunda.** A primeira já existe e a rodada **não** a migra (corretamente — é caminho frio de render): `PerksConfig.BindClassColor(config, secao, "<nome>", "<hex>")`, chamada 7× (`PerksConfig.cs:313, 365, 421, 465, 548, 633, 638`), que popula `PerksConfig.ClassColors` — o dicionário que `ClassColorOverride.Resolve(classNameEn)` consulta **por string**. As duas listas precisam concordar, e nada obriga. Divergência é silenciosa: classe nova registrada só na cor ganha cor no F12 e **nenhum perk**; registrada só no `Parse`, ganha perks e a cor do F12 **nunca se aplica**. Checagem de boot no fim de `PerksConfig.Bind(config)` (caminho frio, 1×):

```csharp
// ref: PA-02-03 — Parse e ClassColors são as duas faces da MESMA lista de classes. Sem esta checagem, uma
// divergência não gera erro nenhum: só um comportamento pela metade que sobrevive meses.
foreach (var key in ClassColors.Keys)
{
    if (SkillMultipliers.Parse(key, warnUnknown: false) == SkillMultipliers.EClassId.None)   // PA-03-06
    {
        Plugin.Log?.LogError($"[CustomClasses] (PA-02-03) classe '{key}' tem cor no F12 mas não existe em EClassId — perks NÃO vão disparar p/ ela.");
    }
}

foreach (SkillMultipliers.EClassId id in Enum.GetValues(typeof(SkillMultipliers.EClassId)))
{
    if (id != SkillMultipliers.EClassId.None && !ClassColors.Keys.Any(k => SkillMultipliers.Parse(k, warnUnknown: false) == id))
    {
        Plugin.Log?.LogError($"[CustomClasses] (PA-02-03) EClassId.{id} não tem entrada em ClassColors — a cor do F12 nunca se aplica a ela.");
    }
}
```

> ⚠️ **Sem overload de compatibilidade.** Os overloads `IsLocalClass(string)` / `IsClass(string, string)` são **removidos**, não mantidos como wrapper. Motivo: um wrapper deixaria call-sites antigos passando despercebidos e anularia o ganho. Removê-los faz o **compilador** apontar todos os 42+ call-sites — o erro de compilação é a rede de segurança, e um nome digitado errado vira erro de build em vez de perk silenciosamente morto. É a razão pela qual esta mudança é **mais segura** que o estado atual, não menos.

### 5.2 `AUD-01-02` — lado do peer (`ClassIdentities`)

```csharp
// modded/Client/ClassIdentities.cs — na classe Identity
internal SkillMultipliers.EClassId ClassId;   // ref: AUD-01-02 · PA-01-06 (internal, não public) — resolvido 1× no Commit()

/// <summary>ref: AUD-01-02 — espelho de ClassNameEnOf devolvendo o id. Bots e vanilla → None.</summary>
internal static SkillMultipliers.EClassId ClassIdOf(EFT.Player? player)   // PA-01-06: internal
{
    if (player is null || player.IsAI) return SkillMultipliers.EClassId.None;   // ref: Player.cs:25135
    if (player.IsYourPlayer) return SkillMultipliers.LocalClassId;

    var nickname = player.Profile?.Nickname;   // HOT PATH — lookup cru, sem EnsureLoaded (mantido do B14)
    return nickname != null && ByNickname.TryGetValue(nickname, out var identity)
        ? identity.ClassId
        : SkillMultipliers.EClassId.None;
}
```

Em `TryFetch`, ao montar cada `Identity`: `ClassId = SkillMultipliers.Parse(p.ClassNameEn)`.

> 🔴 **PA-03-01 — `ClassIdOf` é o ÚNICO resolvedor no caminho quente. `ClassNameEnOf` é REMOVIDO.**
>
> `AiSoundPatch` e `SoundRadiusPatch` (as duas superfícies de maior frequência do mod — per-passo × N players+bots) hoje resolvem a classe **uma vez** e usam a string em dois papéis: alimentar os helpers **e** alimentar o `PerkDiag.LogPeer`. Migrar só os helpers para `EClassId` levaria à implementação natural de **manter `ClassNameEnOf` para o log e acrescentar `ClassIdOf` para o gate** — ou seja, **dois lookups de dicionário por passo**, dobrando o custo exatamente onde o `AUD-01-02` queria baratear. Nada quebraria, nenhum teste falharia, o overlay não mudaria: um achado de performance que piora a performance, em silêncio.
>
> Contrato obrigatório: **o nome só é resolvido dentro do gate de diagnóstico**, e via `NameOf(EClassId)` (switch puro), nunca via dicionário.

```csharp
// ClassSoundPatches.cs → AiSoundPatch.Prefix (forma canônica; SoundRadiusPatch.Postfix é simétrico)
var emitterId = ClassIdentities.ClassIdOf(p);          // ref: AUD-01-02 · PA-03-01 — ÚNICO lookup do hot path
if (emitterId == SkillMultipliers.EClassId.None)
{
    return;   // bot, vanilla ou desconhecido
}

var p0 = power;
power *= QuietStep.MultFor(emitterId);
power *= LoudOperator.MultFor(emitterId);

if (PerkDiag.Enabled)   // ⚠️ PA-03-01 — o NOME só existe aqui dentro. Nunca no caminho quente.
{
    if (p.IsYourPlayer)
    {
        PerkDiag.AiPowerBefore = p0;
        PerkDiag.AiPowerAfter = power;
    }
    else if (power != p0)
    {
        PerkDiag.LogPeer("AI hear power", p.Profile?.Nickname ?? "?",
                         SkillMultipliers.NameOf(emitterId) ?? "?", p0, power);
    }
}
```

**`ClassIdentities.ClassNameEnOf` é removido** (não deprecado). Com ele fora, o compilador garante que ninguém o reintroduza num hot path — a mesma lógica que fez o `PA-01-06` remover os overloads de string do `IsLocalClass`. Os quatro call-sites migram para `ClassIdOf`: `AiSoundPatch`, `SoundRadiusPatch`, `SainSoundPatch` e `SilentKnifePatch` (este último: `SkillMultipliers.IsClass(ClassNameEnOf(emitter), "Stealth")` → `ClassIdentities.ClassIdOf(emitter) == EClassId.Stealth`). `CombatMedicSurgery.Adjust` idem — e é caminho frio, mas migra junto por consistência.

Os helpers de som (`QuietStep.MultFor`, `LoudOperator.MultFor`, `SilentLooter.MultFor`) passam a receber `EClassId`; `CombatMedicSurgery.Adjust` troca `string.Equals(cls, "Combat Medic", …)` por `ClassIdentities.ClassIdOf(doctor) == EClassId.CombatMedic`.

> ⚠️ **PA-01-06 — fronteira pública intocável.** Só o **corpo** desses métodos muda. As quatro assinaturas abaixo são consumidas pelo **TRL-ImmersiveCombatMedicine por reflexão** — o compilador **não** protege, e a quebra apareceria in-game como "cirurgia de aliado sem o perk", sem nenhum erro no log:
> - `public static float CombatMedicSurgery.Adjust(Player? doctor, float penalty)`
> - `public static void CombatMedicSurgery.SetExternalHandling(bool value)`
> - `public static float CombatMedicAllyPerks.AllyHealTimeMult(bool isSurgery)`
> - `public static bool CombatMedicAllyPerks.AllyMobileSurgeon()`
>
> Nome, visibilidade, tipos de parâmetro e tipo de retorno: **byte a byte**. `EClassId` fica `internal` justamente para não vazar para essa superfície.

### 5.3 `AUD-01-08` — cache de textura com LRU por ícone

```csharp
// modded/Client/UI/ClassIconCache.cs
// ref: AUD-01-08 — o mesmo ícone precisa de DUAS variantes vivas (brasão com gradiente + marca d'água
// chapada), então não dá para manter 1 textura por ícone. Cap de 4 por ícone dá as 2 formas + 1 geração
// de folga durante uma troca de cor.
private const int MaxVariantsPerIcon = 4;
private const int ColorQuantum = 8;   // arredonda cada canal p/ múltiplo de 8 → ~32× menos chaves

private static readonly Dictionary<string, List<string>> VariantsByIcon = new(StringComparer.OrdinalIgnoreCase);
private static readonly Dictionary<string, int> CreatedFrame = new(StringComparer.Ordinal);

private static Color32 Quantize(Color c)
{
    // ⚠️ Exceção declarada na 01-spec: muda a cor renderizada em até 3/255 por canal (~1,2%).
    // ⚠️ PA-01-02: clampar DEPOIS de quantizar. v=1.0 → round(31.875)=32 → 32*8 = 256 → (byte)256 == 0
    //    (unchecked, o default do C#): o topo do gradiente de uma classe clara viraria PRETO.
    static byte Q(float v)
    {
        var q = Mathf.RoundToInt(Mathf.Clamp01(v) * 255f / ColorQuantum) * ColorQuantum;
        return (byte)Mathf.Min(q, 255);
    }

    return new Color32(Q(c.r), Q(c.g), Q(c.b), 255);
}

public static Sprite? GetTinted(string? iconFile, Color top, Color bottom)
{
    if (string.IsNullOrWhiteSpace(iconFile)) return null;

    var name = Path.GetFileName(iconFile);
    var qTop = Quantize(top);
    var qBottom = Quantize(bottom);
    var key = $"{name}|{qTop.r:X2}{qTop.g:X2}{qTop.b:X2}|{qBottom.r:X2}{qBottom.g:X2}{qBottom.b:X2}";
    if (TintedCache.TryGetValue(key, out var cached))
    {
        Touch(name, key);   // LRU: recém-usado vai para o fim da lista
        return cached;
    }

    var sprite = BuildTinted(name, (Color)qTop, (Color)qBottom);   // PA-01-09 — ver nota abaixo
    TintedCache[key] = sprite;
    CreatedFrame[key] = Time.frameCount;
    Touch(name, key);
    EvictIfNeeded(name);

    // PERF-INSTR AUD-01-08 — temporary, remove after validation
    if (PerkDiag.Enabled)
    {
        Plugin.Log?.LogInfo($"[CustomClasses][perf/AUD-01-08] tintedCache={TintedCache.Count} (~{TintedCache.Count * 256} KB) +{key}");
    }

    return sprite;
}

/// <summary>
///     ref: AUD-01-08 · PA-03-03 — LRU **de verdade**: usar uma variante a manda para o FIM da fila.
///     Sem o move-to-end isto degenera em FIFO, e aí o brasão em uso (redesenhado a cada `Show`) seria
///     evicto antes de uma variante velha e parada — exatamente o sprite que não pode morrer. É esta
///     função que decide qual textura é destruída; por isso ela é o mecanismo, não um detalhe.
/// </summary>
private static void Touch(string name, string key)
{
    if (!VariantsByIcon.TryGetValue(name, out var keys))
    {
        keys = new List<string>(MaxVariantsPerIcon + 1);
        VariantsByIcon[name] = keys;
    }

    var at = keys.IndexOf(key);   // O(n) com n <= 5 — irrelevante
    if (at >= 0)
    {
        keys.RemoveAt(at);        // já existia → tira da posição atual…
    }

    keys.Add(key);                // …e recoloca no fim (mais recente). EvictIfNeeded remove do INÍCIO.
}

private static void EvictIfNeeded(string name)
{
    if (!VariantsByIcon.TryGetValue(name, out var keys) || keys.Count <= MaxVariantsPerIcon) return;

    // Guard de mesmo-frame: nunca destruir algo criado NESTE frame — dentro de um frame todos os
    // consumidores do ClassColorsChanged (menu + aba CLASS) já se re-apontaram para o sprite novo.
    for (var i = 0; i < keys.Count && keys.Count > MaxVariantsPerIcon; )
    {
        var k = keys[i];
        if (CreatedFrame.TryGetValue(k, out var f) && f == Time.frameCount) { i++; continue; }

        if (TintedCache.TryGetValue(k, out var old)) DestroySprite(old);   // libera Texture2D + Sprite
        TintedCache.Remove(k);
        CreatedFrame.Remove(k);
        keys.RemoveAt(i);
    }
}
```

`Dispose()` passa a limpar também `VariantsByIcon` e `CreatedFrame`.

> ⚠️ **PA-01-09 — `BuildTinted` não existe hoje; é uma extração, não uma chamada a algo pronto.** O corpo (carregar PNG → `Texture2D` → `LoadImage` → `GetPixels32` → laço de tingimento → `SetPixels32` → `Apply(false)` → `Sprite.Create`) está **inline** em `GetTinted` (`ClassIconCache.cs:88-134`). Extrair para `private static Sprite? BuildTinted(string name, Color top, Color bottom)` **preservando integralmente**: (a) o `try/catch` com o `LogError`, (b) o `LogWarning` do arquivo ausente, e (c) o **`UnityEngine.Object.Destroy(tex)` do ramo em que `LoadImage` falha** (`:122`) — é ele que evita vazar uma textura quando o PNG está corrompido. `GetTinted` fica sendo apenas a camada de chave + LRU.

### 5.4 `AUD-01-01` — coroutine do menu

```csharp
// modded/Client/Patches/MenuClassIdentityPatch.cs
private static Transform? _cachedPmv;   // ref: AUD-01-01 — o == do Unity detecta a instância destruída

private static IEnumerator ApplyToMenu(MenuScreen menu)
{
    // ref: AUD-01-01 · PA-01-05 — sem o Menu-Overhaul o painel NUNCA existe, e o no-op custava 60 buscas
    // globais na cena + 90 frames de coroutine viva. IsPresent é O(1) (Chainloader.PluginInfos.ContainsKey).
    //
    // ⚠️ Este bail preserva o comportamento — cadeia PROVADA (PA-01-05):
    //   1. `MainMenuPlayerModelView` é criado e NOMEADO pelo Menu-Overhaul
    //      (mods/SPT-Menu-Overhaul/modded/Patches/PlayerProfileFeaturesPatch.cs:302).
    //   2. Sem o MO esse objeto não existe → `nick` fica null nas 60 iterações.
    //   3. O guard atual (`menu == null || nick == null || nickname vazio`) já faz `yield break` ANTES
    //      do FixTopGlow → nada abaixo daqui roda hoje sem o MO. Sair mais cedo não remove feature.
    //   (`Environment UI`/`Glow Canvas`/`TopGlowPve` são objetos do EFT que o MO apenas muta —
    //    MenuVisibilityController.cs:14-15 — mas isso é irrelevante: o caminho já era inalcançável.)
    if (!MenuOverhaulBridge.IsPresent) yield break;

    // PERF-INSTR AUD-01-01 — temporary, remove after validation
    var sw = PerkDiag.Enabled ? System.Diagnostics.Stopwatch.StartNew() : null;
    var finds = 0;

    TextMeshProUGUI? nick = null;
    for (var i = 0; i < 60 && nick == null; i++)
    {
        // ⚠️ PA-02-04 — o `==` do Unity cobre o objeto DESTRUÍDO, mas não o DESATIVADO. E `GameObject.Find`
        // só encontra ATIVOS: hoje, um painel velho desativado é ignorado automaticamente a cada frame.
        // Com cache, ele sequestraria a identidade do painel novo (escreveríamos num painel invisível).
        if (_cachedPmv == null || !_cachedPmv.gameObject.activeInHierarchy)
        {
            _cachedPmv = GameObject.Find("MainMenuPlayerModelView")?.transform;   // busca global — agora 1 a cada 3 frames
            finds++;
        }

        nick = _cachedPmv != null
            ? _cachedPmv.Find("BottomField/NicknameText")?.GetComponent<TextMeshProUGUI>()
            : null;

        if (nick == null)
        {
            yield return null; yield return null; yield return null;   // poll a cada 3 frames (mesma janela total)
            i += 2;
        }
    }

    // PERF-INSTR AUD-01-01 — temporary, remove after validation
    if (sw != null)
    {
        Plugin.Log?.LogInfo($"[CustomClasses][perf/AUD-01-01] menu apply: finds={finds} mo={MenuOverhaulBridge.IsPresent} ms={sw.Elapsed.TotalMilliseconds:F1}");
    }

    // … corpo atual inalterado (SetAccent / gradiente / ícone / linha de classe / ReapplyLayout) …

    // ref: AUD-01-01 — 90 frames fixos → espera em tempo real equivalente (não precisa de granularidade de frame).
    yield return new WaitForSecondsRealtime(1.5f);
    FixTopGlow(baseColor);
}
```

### 5.5 `AUD-01-04` — acessor do emissor compilado

```csharp
// modded/Client/Patches/SilentKnifePatch.cs
// ref: AUD-01-04 — molde do SainSoundPatch (ClassSoundPatches.cs:352-355), que já compila o getter
// "justamente p/ tirar o reflection do hot-path". Aqui era FieldInfo.GetValue + PropertyInfo.GetValue crus.
private static readonly Func<BaseSoundPlayer, object?>? EmitterOf = BuildEmitterAccessor();

private static Func<BaseSoundPlayer, object?>? BuildEmitterAccessor()
{
    try
    {
        var field = AccessTools.Field(typeof(BaseSoundPlayer), "playersBridge");
        var prop = field != null ? AccessTools.Property(field.FieldType, "iPlayer") : null;
        if (field == null || prop == null) return null;

        var p = Expression.Parameter(typeof(BaseSoundPlayer), "sp");
        var body = Expression.Convert(Expression.Property(Expression.Field(p, field), prop), typeof(object));
        return Expression.Lambda<Func<BaseSoundPlayer, object?>>(body, p).Compile();
    }
    catch (Exception ex)
    {
        Plugin.Log?.LogWarning($"[CustomClasses] (083/AUD-01-04) accessor do emissor não compilado — Morte Silenciosa inerte: {ex.Message}");
        return null;
    }
}
```

No Prefix: `if (PerksConfig.SilentKnifeEnabled?.Value != true || EmitterOf == null) return true;` … `if (EmitterOf(__instance) is not Player emitter) return true;`

### 5.6 `AUD-01-03` — consolidação de `ProceduralWeaponAnimation.Shoot` (o de maior risco)

> ⚠️ **PA-01-01 — consolidar 4 → 2, NÃO 4 → 1.** A versão anterior desta seção propunha um patch único de prioridade `Normal`. Errado: `[HarmonyPriority(Priority.First)]` (`RecoilFloorPatch.cs:41`) e `[HarmonyPriority(Priority.Last)]` (`RecoilFloorPatch.cs:68`) ordenam contra **os prefixos de outros mods** no mesmo método, não só contra os nossos. O usuário roda **RealRecoil**, que patcha recuo. Num patch único `Normal`: (a) a "captura do original" pegaria um `str` já multiplicado por um mod de prioridade mais alta, e (b) o piso B15 clamparia **antes** dos multiplicadores externos — o produto final passaria do piso em silêncio, e o overlay 052 **não pegaria** (ele só mede a nossa cadeia).
>
> O ganho real (4 gates → 2, ordem interna explícita, fim da coordenação por 3 prioridades) é preservado. O campo estático **fica** — dois patches distintos não compartilham `__state`.

```csharp
// modded/Client/Patches/ClassWeaponPatches.cs

/// <summary>ref: AUD-01-03 · PA-01-01 — estado de uma invocação de Shoot. Main thread (Shoot roda no update
/// do player) — sem concorrência. Substitui RecoilFloorCapturePatch.StrBefore, com o mesmo papel.</summary>
internal static class ShootRecoilState
{
    internal static float StrBefore = float.NaN;   // NaN = não é a arma do player local nesta invocação
}

/// <summary>
///     ref: AUD-01-03 · PA-01-01 — FRONTEIRA DE ENTRADA. Priority.First: captura o `str` ANTES de qualquer
///     multiplicador, inclusive os de OUTROS MODS (RealRecoil). Não mexe em nada — só observa.
/// </summary>
internal class ShootCapturePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));

    [HarmonyPriority(Priority.First)]
    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            ShootRecoilState.StrBefore = p != null && ReferenceEquals(__instance, p.ProceduralWeaponAnimation)
                ? str
                : float.NaN;
        }
        catch (Exception ex)
        {
            ShootRecoilState.StrBefore = float.NaN;
            Plugin.Log?.LogError($"[CustomClasses] recoil capture falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     ref: AUD-01-03 · PA-01-01 — FRONTEIRA DE SAÍDA. Priority.Last: roda depois de TODOS os multiplicadores,
///     nossos e de terceiros. Funde 3 patches num só (maestria 058 + perks 050 + piso B15), com a ordem interna
///     escrita em sequência em vez de emergir de [HarmonyPriority]:
///       (1) maestria  →  (2) perks  →  (3) piso  →  (4) diag.
///     Gate resolvido UMA vez (era 3×). O `str` original vem do ShootCapturePatch.
/// </summary>
internal class ShootApplyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));

    [HarmonyPriority(Priority.Last)]
    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        var str0 = ShootRecoilState.StrBefore;
        if (float.IsNaN(str0)) return;   // não era a arma do player local nesta invocação

        var p = Singleton<GameWorld>.Instance?.MainPlayer;
        if (p == null) return;           // GATE ÚNICO (era resolvido 3× aqui dentro) — o ganho do AUD-01-03

        // ⚠️ PA-02-01 — try/catch POR BRANCH, nunca um externo único. Consolidar o GATE não pode consolidar
        // a FALHA: hoje são 3 patches Harmony independentes, e um que lança NÃO impede os outros de rodar.
        // Com um catch externo, `ApplyMastery` lançando (ela toca p.Skills e skill.Level, que ficam nulos numa
        // troca de arma) pularia o PISO B15 — e o tiro sairia sem clamp nenhum.
        try { RecoilBranches.ApplyMastery(p, ref str); }  catch (Exception ex) { BranchFail("mastery", ex); }
        try { RecoilBranches.ApplyPerks(p, ref str); }    catch (Exception ex) { BranchFail("perks", ex); }
        try { RecoilBranches.ApplyFloor(str0, ref str); } catch (Exception ex) { BranchFail("floor", ex); }

        if (PerkDiag.Enabled)   // baseline = str ORIGINAL (contrato do CR 2026-07-11)
        {
            PerkDiag.RecoilBefore = str0;
            PerkDiag.RecoilAfter = str;
        }
    }
}

/// <summary>
///     ref: PA-02-01 — log de falha de branch com dedupe. Estes branches rodam em hot path (por tiro); o
///     padrão atual (LogError a cada ocorrência, em cada patch) inunda o console quando algo quebra numa
///     rajada. Uma linha por branch por sessão basta para diagnosticar.
/// </summary>
internal static class BranchFailLog
{
    private static readonly HashSet<string> Seen = new(StringComparer.Ordinal);

    internal static void Once(string branch, Exception ex)
    {
        if (!Seen.Add(branch)) return;
        Plugin.Log?.LogError($"[CustomClasses] branch '{branch}' falhou (log 1× por sessão): {ex.Message}");
    }
}
```

**O mesmo padrão vale para os outros três alvos consolidados.** Em `SetAnimatorAndProceduralValues` a consequência de um catch externo é ainda mais concreta: se o branch de Adrenalina lançar **depois** de escalar `BuffInfo.ReloadSpeed` e **antes** de gravar o `__state`, o Postfix não restaura e o campo fica sujo **pela raid inteira**. Hoje cada patch tem o seu par Prefix/Postfix e a falha é contida ao próprio par — a consolidação só é aceitável preservando isso.

**Onde `RecoilBranches` mora (PA-02-07):** classe estática **única**, declarada em `ClassWeaponPatches.cs`, imediatamente acima de `ShootCapturePatch`/`ShootApplyPatch`. Os três métodos são **movidos** para lá, cada um carregando um comentário de procedência (`// ref: origem WeaponMasteryPatches.cs:118-145` etc.). Espalhá-los pelos arquivos de origem anularia o objetivo declarado do `AUD-01-03` — ter a ordem de composição legível num lugar só.

**Destino do `RecoilFloorPatch.cs` (PA-03-07):** o arquivo é **deletado**. A versão anterior desta spec mandava mantê-lo "só com o XMLdoc histórico do B15" — um `.cs` sem tipo nenhum é resíduo, não preservação: o compilador o inclui e ignora, e quem abrir vai lê-lo como refatoração incompleta. O contexto de balance do B15 (a justificativa numérica do piso, com os casos do Anexo C) **é preservado movendo o XMLdoc para cima de `RecoilBranches.ApplyFloor`** — que é onde alguém investigando o piso vai efetivamente procurar. O histórico do arquivo fica no git; o rastro da decisão, nesta spec e no `05-asbuild`.

Os corpos de `ApplyMastery` / `ApplyPerks` / `ApplyFloor` são **movidos sem alteração de fórmula** dos patches atuais (`WeaponMasteryPatches.cs:118-145`, `ClassWeaponPatches.cs:26-69`, `RecoilFloorPatch.cs:70-100`), menos o gate de instância (agora feito uma vez no `ShootApplyPatch`). O `float.IsNaN` **continua** existindo — subiu para o topo do `ShootApplyPatch`, que é onde ele sempre pertenceu.

⚠️ **A checagem `PerksConfig.RecoilFloorEnabled` continua DENTRO de `ApplyFloor`, e a escrita do `PerkDiag` continua FORA dela** — é o contrato do code-review de 2026-07-11 (`RecoilFloorPatch.cs:91-95`): com o piso desligado o overlay ainda tem de mostrar o valor real, senão volta a mentir.

Mesma forma para os outros três alvos:
- **`ApplyDamageInfo`** → `ClassDamagePatch` com **1 Prefix** (ordem: registrar hit de combate → Couraça → Execution melee) e **1 Postfix** (gatilho da Adrenalina). O Prefix mantém `ref DamageInfoStruct`; cada branch conserva o seu próprio gate específico (Couraça exige `__instance == MainPlayer`; Execution exige `damageInfo.Player.iPlayer.ProfileId == mp.ProfileId`), mas `Singleton<GameWorld>.Instance?.MainPlayer` é resolvido **uma vez** e passado adiante.
- **`SetAnimatorAndProceduralValues`** → `FirearmSyncPatch` com **1 Prefix (out float __state)** + **1 Postfix**. Os branches de Adrenalina (Fuzileiro) e escopeta (Tanque) são mutuamente exclusivos e o Prefix aplica **no máximo um** deles. O Postfix restaura e, em seguida, roda o branch do reset de saque (`HolsterDrawResetPatch`).

  > 🔴 **PA-03-02 — o `__state` é capturado INCONDICIONALMENTE e ANTES de qualquer branch.** O `try/catch` por branch do `PA-02-01` **contém** a exceção mas **não desfaz a escrita**: um branch que lance depois de mutar `BuffInfo.ReloadSpeed` e antes de gravar o `__state` deixaria o campo escalado **pela raid inteira** (recarga permanentemente acelerada), com um único erro no log. A ordem tem de ser estrutural, não convenção:

  ```csharp
  [PatchPrefix]
  private static void Prefix(Player.FirearmController __instance, out float __state)
  {
      __state = float.NaN;

      var buff = __instance.BuffInfo;
      if (buff == null) return;
      if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController)) return;

      // ⚠️ PA-03-02 — captura ANTES de qualquer branch, e sem depender de nenhum deles ter rodado.
      // Nenhum branch grava __state; o Postfix restaura sempre que não for NaN (restaurar o mesmo valor
      // é no-op inofensivo, e é mais barato que rastrear "mudei ou não").
      __state = buff.ReloadSpeed;

      try { ReloadBranches.Adrenaline(buff); }          catch (Exception ex) { BranchFailLog.Once("reload/adren", ex); }
      try { ReloadBranches.Shotgun(__instance, buff); } catch (Exception ex) { BranchFailLog.Once("reload/shotgun", ex); }
  }
  ```
- **`TotalErgonomics`** → **1 Postfix** com gate único, chamando o branch do Bunker (Tanque + arma pesada) e o da maestria.

### 5.7 Demais achados (curtos)

```csharp
// AUD-01-05 — SkillsClassTabPatch.cs:435
if (!_loggedTabImages && PerkDiag.Enabled)   // ref: AUD-01-05 — molde do DumpNativeTexts (:463)

// AUD-01-06 — WeaponMasteryPatches.cs:57
var inHideout = p is HideoutPlayer;   // ref: AUD-01-06 — forma canônica (spt-mod-best-practices §2)

// AUD-01-07b — DROPADO (PA-01-07). AdrenalineState.WatchWindow fica EXATAMENTE como está.

// AUD-01-07c — SkillPanelPatch: cache do texto de tooltip
// ⚠️ PA-01-03: a chave PRECISA incluir o className. TooltipText(float factor, string? className)
//    (MultiplierFormat.cs:55) depende dele, e ClassName muda em dois cenários reais: troca de perfil sem
//    reiniciar o cliente (Reset + refetch) e troca de idioma do EFT (SkillMultipliers.cs:27-30).
private static readonly Dictionary<(ESkillId, float, string?), string> TooltipCache = new();   // ref: AUD-01-07c · PA-01-03

// AUD-01-07d — PerkDiagnostics: grupos cacheados por classe.
// Seguro: PerkGroup/PerkLine do Library são SINGLETONS e PerkLine.Multiplier resolve `Live?.Invoke()`
// a cada acesso (PerksCatalog.cs:39) → cachear o ARRAY não congela os valores; o F12 continua vivo.
private static SkillMultipliers.EClassId _cachedGroupsFor = SkillMultipliers.EClassId.None;
private static PerksCatalog.PerkGroup[]? _cachedGroups;   // ref: AUD-01-07d
```

**Invalidação dos dois caches acima (PA-01-03):** `SkillMultipliers.Apply()` e `SkillMultipliers.Reset()` — os dois únicos pontos que trocam a classe/idioma-resolvido — passam a chamar `SkillPanelPatch.ClearTooltipCache()` e `PerkDiagnostics.ClearGroupCache()`. Ambos são caminhos **frios** (1× por fetch), então o custo da invalidação é irrelevante e a correção é garantida na fonte.

### 5.8 INSTR-2 — censo periódico (substitui o dump de raid-end, que não existe)

```csharp
// modded/Client/PerkDiagnostics.cs
// PERF-INSTR AUD-01-02/03 — temporary, remove after validation
internal static class PerfCount
{
    internal static long MoveSpeedCalls, MoveSpeedPassed, StepAiCalls, StepAiPassed,
                         RolloffCalls, RolloffPassed, DamageCalls, DamageGates, ShootCalls, ShootGates;
    internal static void Reset() { MoveSpeedCalls = MoveSpeedPassed = StepAiCalls = StepAiPassed
                                 = RolloffCalls = RolloffPassed = DamageCalls = DamageGates
                                 = ShootCalls = ShootGates = 0; }
}
```

Corrotina hospedada no `Plugin` (o mod **não tem hook de raid-end** — ver RV-06 da revisão; por isso o dump é por tempo).

⚠️ **PA-01-10:** a corrotina **não** é um `while (true)` iniciado no `Awake`. Ela é iniciada/parada pelo `SettingChanged` do próprio toggle de diagnóstico e o laço tem condição de saída — com o default (`Perk Diagnostics = false`) **a corrotina nem existe**, inclusive no headless. No `Awake`: `PerksConfig.DiagnosticsEnabled.SettingChanged += (_, _) => SyncPerfDump();` mais uma chamada inicial a `SyncPerfDump()`, que faz `StartCoroutine`/`StopCoroutine` conforme o valor.

```csharp
private IEnumerator PerfDumpLoop()   // PERF-INSTR AUD-01-02/03 — temporary, remove after validation
{
    while (PerksConfig.DiagnosticsEnabled?.Value == true)   // PA-01-10: condição de saída real
    {
        yield return new WaitForSeconds(60f);
        if (!PerkDiag.Enabled || !Singleton<GameWorld>.Instantiated) continue;

        Log?.LogInfo($"[CustomClasses][perf] moveSpeed={PerfCount.MoveSpeedCalls}/{PerfCount.MoveSpeedPassed} "
                   + $"stepAI={PerfCount.StepAiCalls}/{PerfCount.StepAiPassed} "
                   + $"rolloff={PerfCount.RolloffCalls}/{PerfCount.RolloffPassed} "
                   + $"damage={PerfCount.DamageCalls} (gates={PerfCount.DamageGates}) "
                   + $"shoot={PerfCount.ShootCalls} (gates={PerfCount.ShootGates})");
        PerfCount.Reset();
    }
}
```

`DamageGates`/`ShootGates` são o que prova a meta **4 → 2** do `AUD-01-03` (corrigida pelo `PA-01-01`): contam execuções de gate por evento.

**Onde incrementar cada contador (PA-02-05).** A posição não é detalhe: o critério de aceite da 01-spec cobra que *"a fração que passa do gate permanece ~1/N"*. Se `Calls` e `Passed` forem ambos incrementados **depois** do gate, a razão dá sempre 1 e o critério não mede nada.

| Contador | Arquivo / método | Posição exata |
|---|---|---|
| `MoveSpeedCalls` | `ClassMovementPatches.cs` → `ClassMoveSpeed.Apply` | **1ª linha do `try`**, antes de resolver `MainPlayer` |
| `MoveSpeedPassed` | idem | logo após o `ReferenceEquals(ctx, p.MovementContext)` passar |
| `StepAiCalls` | `ClassSoundPatches.cs` → `AiSoundPatch.Prefix` | após o descarte `type != AISoundType.step` (só passo interessa à métrica) |
| `StepAiPassed` | idem | após `emitterClass is null` (ou seja, depois do gate `IsAI`) |
| `RolloffCalls` | `ClassSoundPatches.cs` → `SoundRadiusPatch.Postfix` | 1ª linha do `try` |
| `RolloffPassed` | idem | após `emitterClass is null` |
| `DamageCalls` | patch consolidado de `ApplyDamageInfo` (Prefix) | 1× por invocação, antes de qualquer gate |
| `DamageGates` | idem + o Postfix consolidado | **1× por resolução de gate** — hoje daria 4/evento, depois 2 |
| `ShootCalls` | `ShootCapturePatch.Prefix` | 1ª linha |
| `ShootGates` | `ShootCapturePatch` + `ShootApplyPatch` | 1× em cada patch que resolve gate — hoje daria 4/tiro, depois 2 |

⚠️ Os incrementos ficam dentro de `if (PerkDiag.Enabled)`. Ligar o diagnóstico **no meio** de uma janela de 60 s produz um primeiro dump com amostra parcial: **descartar o primeiro dump, usar do segundo em diante**. Registrar no `05-asbuild` para ninguém ler o primeiro número como medição.

### 5.9 Diff exato do bloco de registro no `Plugin.Awake` (PA-03-05)

O risco aqui é **assimétrico**, e é por isso que esta seção existe:

- **Remover** um `Enable()` de classe que deixou de existir → **erro de compilação**. O compilador protege.
- **Esquecer de adicionar** o `Enable()` de um patch consolidado → **compila perfeitamente**, e o alvo inteiro fica sem patch. Se faltar `ShootApplyPatch().Enable()`, o jogo perde **de uma vez** maestria de recuo (058), Shaky Hands, Adrenalina-recuo, Bunker **e** o piso B15 — e o único sintoma é "o recuo parece diferente". Com a linha de base desconhecida do `PA-01-04`, ninguém consegue afirmar que é regressão.

```
REMOVER (as classes deixam de existir — o compilador acusa cada uma):
  new ShootRecoilPatch().Enable()          new RecoilFloorCapturePatch().Enable()
  new RecoilFloorApplyPatch().Enable()     new WeaponMasteryRecoilPatch().Enable()
  new WeaponMasteryErgoPatch().Enable()    new HeavyWeaponErgoPatch().Enable()
  new BulwarkPatch().Enable()              new ExecutionMeleePatch().Enable()
  new AdrenalineTriggerPatch().Enable()    new LocalHitTypePatch().Enable()
  new ReloadSpeedPatch().Enable()          new ShotgunReloadPatch().Enable()
  new HolsterDrawResetPatch().Enable()

ACRESCENTAR (⚠️ NADA acusa se faltar — conferir um a um):
  new ShootCapturePatch().Enable()   // PWA.Shoot        · Priority.First (captura o str original)
  new ShootApplyPatch().Enable()     // PWA.Shoot        · Priority.Last  (maestria → perks → piso → diag)
  new ClassDamagePatch().Enable()    // ApplyDamageInfo  · Prefix (hit de combate → Couraça → Execution) + Postfix (Adrenalina)
  new FirearmSyncPatch().Enable()    // SetAnimatorAndProceduralValues · Prefix (__state + Adrenalina/escopeta) + Postfix (restaura + reset de saque)
  new TotalErgoPatch().Enable()      // TotalErgonomics  · Postfix (Bunker + maestria)
```

⚠️ Preservar a posição relativa dos `Enable()` restantes: a ordem de registro **não** define mais a ordem de execução em `Shoot` (isso agora é `[HarmonyPriority]` explícito + sequência no corpo), mas os comentários existentes no `Plugin.cs:123-124` e `:163-167` descrevem a ordem antiga e precisam ser reescritos para não mentir.

**AC de fumaça (três leituras num frame provam que os cinco estão registrados):** com `Perk Diagnostics` ligado, o overlay 052 mostra (a) `Recoil str` mudando ao atirar → `ShootApplyPatch` vivo; (b) `Ergo (weapon)` refletindo o Bunker com arma pesada em mãos → `TotalErgoPatch` vivo; (c) `Malfunction%` preenchido → o resto da cadeia de arma intacta. Para `ClassDamagePatch` e `FirearmSyncPatch`: levar um tiro como Tanque de colete pesado (dano reduzido) e recarregar uma escopeta tubular (mais rápida).

## 6. Fluxo de dados

```
[boot] Plugin.Awake → Config.Bind → Enable() dos patches (4 a menos que hoje)
                                  → StartCoroutine(PerfDumpLoop)                    [INSTR-2]

[fetch] rota /customclasses/skill-multipliers → SkillMultipliers.Apply
          → _classNameEn = "…"  →  _classId = Parse(_classNameEn)                   [AUD-01-02]
        rota /customclasses/class-identities  → ClassIdentities.Commit
          → Identity.ClassId = Parse(p.ClassNameEn)                                  [AUD-01-02]

[raid, por frame de movimento]
  MovementContext.MaxSpeed (:910) ← UpdateCharacterControllerSpeedLimit (:4181), ×3 leituras
    → MaxSpeedPatch.Postfix → ClassMoveSpeed.Apply
        → ReferenceEquals(ctx, MainPlayer.MovementContext)        (gate de instância, inalterado)
        → _classId == EClassId.Tank                               (era string.Equals OrdinalIgnoreCase)

[raid, por tiro]
  PWA.Shoot → ShootRecoilPatch.Prefix (ÚNICO)                                        [AUD-01-03]
    → gate 1× → str0 = str → maestria → perks → piso B15 → PerkDiag

[menu, ao abrir / ao mexer no picker]
  MenuScreen.Show / ClassColorsChanged → RefreshColors → ApplyToMenu
    → MenuOverhaulBridge.IsPresent? não → yield break                                [AUD-01-01]
    → sim → GameObject.Find a cada 3 frames (cacheado) → ApplyClassIcon
              → ClassIconCache.GetTinted(cor quantizada) → LRU cap 4 por ícone       [AUD-01-08]
```

## 7. Riscos e dependências

| Risco | Probabilidade | Mitigação |
|---|---|---|
| **`AUD-01-03` altera a ordem de composição do recuo** e muda o balance | Média | A ordem interna vira sequência explícita de statements, copiada 1:1 das prioridades atuais. AC de não-regressão exige `Recoil str` idêntico no overlay 052 nos 2 piores casos do Anexo C. **É o item a reverter primeiro se algo sair errado.** |
| **`AUD-01-03` quebra a composição com mods externos de recuo (RealRecoil)** | **Era Alta — mitigada** | **PA-01-01:** a consolidação é **4 → 2**, preservando `Priority.First` (captura antes de terceiros) e `Priority.Last` (piso depois de terceiros). Consolidar em 1 patch `Normal` teria movido as duas fronteiras. Validação: comparar `Recoil str` no overlay 052 **com o RealRecoil ativo** — é a única forma de flagrar, porque o overlay sozinho só mede a nossa cadeia |
| **`AUD-01-02` quebra a integração com o ICM** (assinatura pública consumida por reflexão) | Baixa — mitigada | **PA-01-06:** as 4 assinaturas (`CombatMedicSurgery.Adjust`/`SetExternalHandling`, `CombatMedicAllyPerks.AllyHealTimeMult`/`AllyMobileSurgeon`) são **intocáveis**; só o corpo muda. `EClassId` fica `internal` para não vazar. O compilador **não** protege aqui — a conferência é manual (checklist §8) e o AC de cirurgia de aliado é o teste in-game |
| **`AUD-01-07c` serve tooltip da classe errada** após troca de perfil/idioma | Baixa — mitigada | **PA-01-03:** `className` entra na chave **e** o cache é limpo em `SkillMultipliers.Apply()`/`Reset()` |
| **`AUD-01-08`: canal de cor estoura o byte** e inverte a cor | Baixa — mitigada | **PA-01-02:** clamp `Mathf.Min(q, 255)` depois da quantização + teste visual obrigatório numa classe clara |
| **Consolidação funde o isolamento de FALHA: um branch que lança pula os irmãos** (o pior caso é o piso B15 não rodar) | **Era Alta — mitigada** | **PA-02-01:** `try/catch` **por branch** nos quatro alvos consolidados, nunca um externo único, + `BranchFailLog.Once` para não inundar o console num hot path. Item de checklist próprio |
| **Bump de versão incompleto** faz o gate "confirmar a versão no log de boot" falhar sempre | Média — mitigada | **PA-02-02:** a versão vive em **4** arquivos; os dois que aparecem em log (`Plugin.cs:13` `BepInPlugin`, `CustomClassesMetadata.cs:19`) não estavam listados. Teste que pega os quatro: `grep -rn '0\.16\.8' modded/` vazio |
| **`Parse` e `ClassColors` divergem em silêncio** (classe com cor e sem perk, ou o inverso) | Média — mitigada | **PA-02-03:** checagem de boot bidirecional no fim de `PerksConfig.Bind` |
| **`_cachedPmv` fixa um painel desativado** e a identidade some do menu de forma intermitente | Baixa — mitigada | **PA-02-04:** `activeInHierarchy` no check do cache (`GameObject.Find` só acha ativos — hoje o caso se auto-corrige) |
| **`AUD-01-03` em `SetAnimatorAndProceduralValues`:** duplo escalonamento de `BuffInfo.ReloadSpeed` | Baixa | O Prefix consolidado aplica **no máximo um** branch (Fuzileiro e Tanque são classes exclusivas) e salva o original **uma vez** no `__state` único. Hoje há 2 `__state` independentes — o risco existe **hoje** e a consolidação o elimina. |
| **`AUD-01-02`: classe nova do editor web não reconhecida** | Média | `Parse` degrada para `None` com 1 aviso; nenhum perk dispara (fail-safe, nunca casa errado). Corner case explícito na 01-spec. |
| **`AUD-01-02`: call-site esquecido** | **Nula** | Os overloads de string são **removidos** → erro de compilação em cada call-site. O compilador é a rede. |
| **`AUD-01-08`: sprite destruído ainda referenciado** por uma `Image` → quadrado branco | Baixa | Cap 4 (2 formas em uso + 1 geração de folga) **e** guard de mesmo-frame. AC de não-regressão cobre as 4 superfícies do ícone. |
| **`AUD-01-08`: quantização visível** | Baixa | ≤3/255 por canal. Declarada como exceção na 01-spec; reversível isolada (a parte LRU sozinha já resolve o crescimento). |
| **`AUD-01-01`: `IsPresent` falso-negativo** (Menu-Overhaul carregando depois) | Baixa | `Chainloader.PluginInfos` está completo quando `MenuScreen.Show` roda (o menu só existe depois do chainload). O `MenuOverhaulBridge` já depende dessa premissa desde o item 015. |
| **`AUD-01-07b`: `WaitForSeconds` usa tempo escalado** | Baixa | A janela da Adrenalina é in-raid (timeScale = 1). Mantido `WaitForSeconds` por simetria com o resto do arquivo; o `while (IsActive)` re-avalia e corrige qualquer deriva. |

**Patches existentes que podem conflitar:** todos os que esta rodada consolida — a mudança é interna ao mod. **Nenhum patch externo** compartilha esses alvos (`ShootRecoilPatch` coexiste hoje com o RealRecoil só via ordem de prioridade do Harmony, que é preservada: o patch consolidado continua em `Priority.Normal` por default).

**Ordem de inicialização:** a remoção dos `Enable()` dos patches consolidados tem de ser **exata** — um `Enable()` órfão de uma classe removida é erro de compilação; um patch consolidado esquecido no `Plugin.Awake` duplicaria o efeito.

**Ordem de implementação recomendada** (cada passo compilável e revisável isolado):
1. `AUD-01-05`, `AUD-01-06`, `AUD-01-07c/d` — triviais, sem risco. (`AUD-01-07b` foi dropado — PA-01-07.)
2. `AUD-01-04` — localizado.
3. `AUD-01-08` — localizado, com AC visual próprio.
4. `AUD-01-01` — localizado, com AC visual próprio.
5. `AUD-01-02` — amplo mas mecânico; o compilador guia.
6. `AUD-01-03` — por último, um alvo de cada vez, `Shoot` sendo o mais delicado.

## 8. Checklist de implementação

- [ ] `AUD-01-05`: gate `PerkDiag.Enabled` nos logs `[053-tabicon]`/`[053-tabtext]`
- [ ] `AUD-01-06`: `p is HideoutPlayer` em `WeaponMasteryPatches.cs:57`
- [ ] `AUD-01-07c`: cache de tooltip em `SkillPanelPatch`, chave **com `className`** (PA-01-03) + `ClearTooltipCache()`
- [ ] `AUD-01-07d`: cache de grupos em `PerkDiagnostics` + `ClearGroupCache()`
- [ ] `PA-01-03`: chamar os dois `Clear*Cache()` em `SkillMultipliers.Apply()` **e** `Reset()`
- [ ] `AUD-01-04`: `EmitterOf` compilado em `SilentKnifePatch`
- [ ] `AUD-01-09`: extrair `BuildTinted` preservando `try/catch`, `LogWarning` e o `Destroy(tex)` do ramo falho
- [ ] `AUD-01-08`: `Quantize` **com clamp `Mathf.Min(q, 255)`** (PA-01-02) + `VariantsByIcon` + `EvictIfNeeded` + guard de frame + `Dispose` atualizado
- [ ] `AUD-01-01`: bail por `IsPresent` **com a cadeia do PA-01-05 registrada em comentário**, `_cachedPmv`, poll 3-em-3, `WaitForSecondsRealtime`
- [ ] `AUD-01-02`: `EClassId` (**`internal`** — PA-01-06) + `Parse` + `_classId` + `LocalClassId`; **remover** os overloads de string
- [ ] `AUD-01-02`: `Identity.ClassId` (`internal`) + `ClassIdOf`; migrar helpers de som e `CombatMedicSurgery`
- [ ] `AUD-01-02`: migrar **todos** os call-sites restantes (o build aponta)
- [ ] `PA-01-06`: conferir que `CombatMedicSurgery.Adjust`, `CombatMedicSurgery.SetExternalHandling`, `CombatMedicAllyPerks.AllyHealTimeMult` e `CombatMedicAllyPerks.AllyMobileSurgeon` mantêm **assinatura byte a byte** (nome, visibilidade, parâmetros, retorno)
- [ ] `AUD-01-03`: consolidar `TotalErgonomics` (2→1)
- [ ] `AUD-01-03`: consolidar `SetAnimatorAndProceduralValues` (3→2)
- [ ] `AUD-01-03`: consolidar `ApplyDamageInfo` (4→2)
- [ ] `AUD-01-03` · `PA-01-01`: consolidar `Shoot` **4→2** (`ShootCapturePatch` `Priority.First` + `ShootApplyPatch` `Priority.Last`), **mantendo** o estático como `ShootRecoilState.StrBefore`
- [ ] `PA-02-01`: **nenhum alvo consolidado tem `try/catch` externo único** — cada branch é isolado, e `BranchFailLog.Once` evita flood no hot path
- [ ] `PA-02-07` · `PA-03-07`: `RecoilBranches` declarada **uma vez** em `ClassWeaponPatches.cs`, com comentários de procedência; o XMLdoc do B15 migra para cima de `ApplyFloor` e **`RecoilFloorPatch.cs` é deletado**
- [ ] `PA-03-01`: `ClassIdentities.ClassNameEnOf` **removido**; `SkillMultipliers.NameOf(EClassId)` criado e chamado **só** de dentro de `if (PerkDiag.Enabled)`. Verificação: `grep -rn 'ClassNameEnOf' modded/Client/` volta **vazio**
- [ ] `PA-03-02`: em `SetAnimatorAndProceduralValues`, o `__state` é capturado **incondicionalmente antes** dos branches; **nenhum branch grava `__state`**
- [ ] `PA-03-03`: `Touch()` implementada com **move-to-end** (LRU real, não FIFO); `EvictIfNeeded` remove do **início** da lista
- [ ] `PA-03-05`: conferir os **5** `Enable()` novos um a um contra o diff da §5.9 — nenhum erro de compilação avisa se faltar
- [ ] `PA-03-06`: `Parse(nameEn, warnUnknown = true)`; a checagem de boot chama com `warnUnknown: false`
- [ ] `PA-02-03`: checagem bidirecional `Parse` ↔ `ClassColors` no fim de `PerksConfig.Bind`
- [ ] `PA-02-08`: comentário anti-remoção no `EnsureLoaded()` do `IsLocalClass`; **conferir que nenhum patch passou a chamar `IsLocalClass` ANTES do seu gate de instância** durante a migração dos 42 call-sites (é onde a ordem se inverte por descuido — CR-F5)
- [ ] `Plugin.cs`: ajustar os `Enable()` e adicionar `SyncPerfDump()` no `SettingChanged` do diagnóstico (PA-01-10)
- [ ] INSTR-1/2/3 nos pontos previstos, todos `// PERF-INSTR` e gated; contadores nas posições da tabela do §5.8 (PA-02-05)
- [ ] `PA-01-08` · `PA-02-02`: bumpar `0.16.8` → `0.16.9` em **quatro** arquivos — `Client/CustomClasses.Client.csproj:9`, **`Client/Plugin.cs:13` (`BepInPlugin` — é o que sai no log do BepInEx)**, `Server/CustomClasses.Server.csproj:10` e **`Server/CustomClassesMetadata.cs:19` (é o que sai no log do SPT.Server)**. Teste que pega os quatro: `grep -rn '0\.16\.8' modded/` volta vazio
- [ ] Build client 0 erros; conferir que nenhum warning novo apareceu
- [ ] `05-asbuild.md` com o que mudou por achado, o que ficou de fora (`AUD-01-07b`), a **lista de classes de patch removidas/criadas** (PA-02-09) e a nota do primeiro dump parcial (PA-02-05)

**Gates de validação que a implementação precisa deixar preparados** (executados na Fase 4):

- [ ] `PA-01-04`: **raid de baseline na DLL ATUAL antes de instalar a nova** — percorrer a matriz de perks anotando o que funciona hoje, para distinguir regressão de defeito pré-existente (P-10.1 / P-16.1)
- [ ] `PA-01-02`: teste visual do brasão numa classe **clara** (Saqueador `#c4ad45`) — o canal R do topo do gradiente passa de 251
- [ ] `PA-01-01`: comparar `Recoil str` no overlay 052 **com o RealRecoil ativo**, não só isolado
- [ ] `PA-01-06`: cirurgia de aliado via ICM em coop
- [ ] `PA-01-08`: confirmar no log de boot que a DLL carregada é a `0.16.9` (o launcher já reverteu build antes — `feedback_server_launcher_sync_builds`)
- [ ] `PA-01-10`: após a validação, `grep -rn 'PERF-INSTR' modded/Client/` tem de voltar **vazio** (a corrotina inteira é um dos blocos a remover, não só as linhas de log)
- [ ] `PA-02-09`: **`/update-mod-graph CustomClasses`** — a rodada remove ~13 classes de patch (`RecoilFloorCapturePatch`, `RecoilFloorApplyPatch`, `WeaponMasteryRecoilPatch`, `WeaponMasteryErgoPatch`, `ShootRecoilPatch`, `HeavyWeaponErgoPatch`, `BulwarkPatch`, `ExecutionMeleePatch`, `AdrenalineTriggerPatch`, `LocalHitTypePatch`, `ReloadSpeedPatch`, `ShotgunReloadPatch`, `HolsterDrawResetPatch`) e cria 4+ (`ShootCapturePatch`, `ShootApplyPatch`, `RecoilBranches`, os consolidados de `ApplyDamageInfo`/`SetAnimatorAndProceduralValues`/`TotalErgonomics`), além de trocar a assinatura de `IsLocalClass`/`IsClass` em 42 call-sites. Grafo desatualizado é pior que nenhum — a próxima sessão que perguntar "quem chama `IsLocalClass`" recebe 42 respostas que não existem mais

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | **Nada muda aqui.** O mod não tem hook de raid-end (registrado no relatório 01, RV-06) e reseta tudo no raid-start (`RaidPerksNotificationPatch.cs:36-40`). Esta rodada **não** introduz estado novo com escopo de raid. A única corrotina nova (`PerfDumpLoop`, §5.8) é de escopo de **plugin** (vive o processo, como o resto), com guard `Singleton<GameWorld>.Instantiated` no corpo — padrão prescrito em `spt-mod-best-practices` §2 ("patches são globais; o gate vai no corpo"). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Todos os gates de instância (regra 075) são **preservados literalmente**. Na consolidação do `AUD-01-03` o gate passa a ser resolvido 1× e **compartilhado**, não afrouxado: os branches que precisam de gate adicional (Couraça = `__instance == MainPlayer`; Execution = `damageInfo.Player.iPlayer.ProfileId == mp.ProfileId`) mantêm o seu (§5.6). AC de não-regressão B verifica que a fração que passa do gate continua ~1/N. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | **Nenhum alvo novo.** Os existentes já resolvem por assinatura (`AccessTools.Method(typeof(X), "nome", new[]{tipos})`). `GClass2175.method_1` continua no seu `try/catch` próprio no `Plugin.cs:133`. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Nenhuma mudança de estado nova. O único ponto que escreve estado do EFT e é tocado é `BuffInfo.ReloadSpeed` (`AUD-01-03`), cujo salvar/restaurar via `__state` é **preservado** — e passa de 2 pares independentes para 1, o que reduz o risco de deixar o campo sujo. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA | ✅ | Nenhum estado novo de raid. Os estáticos que a rodada **remove** (`RecoilFloorCapturePatch.StrBefore`) diminuem a superfície. Os que ficam (`_cachedPmv` do `AUD-01-01`, caches do `AUD-01-08`/`07c`/`07d`) são de **menu/UI**, não de raid, e o `==` do Unity trata a instância destruída. AC de não-regressão A cobre a matriz. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ **N/A parcial** | **Nenhuma `ConfigEntry` nova** (§3). A instrumentação reusa `Perk Diagnostics` (default `false`, semântica já documentada em `PROPRIEDADES.md`). Nenhum default de gameplay muda. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | ✅ | `AdrenalineState.ForceReloadResync` chama `fc.SetAnimatorAndProceduralValues()` — um método **patcheado por este mod**. Isso já é assim hoje e **não é recursão**: o Prefix consolidado só escala `BuffInfo.ReloadSpeed` e não re-invoca o alvo. A consolidação **melhora** o caso: hoje essa chamada atravessa 3 pares de patch, depois atravessa 1. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | ✅ | `HolsterDrawSpeedPatch.BoostedDraw` (flag de intercept) tem a semântica preservada e continua sendo zerada no raid-start. Os caches novos são todos chaveados pelo contexto que os invalida: `AUD-01-07c` por `(ESkillId, fator)`, `AUD-01-07d` por `EClassId`, `AUD-01-08` pela cor quantizada, `_cachedPmv` pelo `==` do Unity. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Todos os alvos são **pré-existentes** e foram reabertos no dump durante a auditoria e a revisão (as 30 citações foram reconferidas — §4 da revisão 01). ⚠️ O dump **não existe neste worktree**; foi lido de `C:\Repos\spt\tarkov-spt-4.0\references\eft-decompiled\`. `HideoutPlayer` (`AUD-01-06`) confirmado em `EFT/HideoutPlayer.cs`. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | **N/A** | Esta rodada não usa skill do EFT como lever. Os levers de skill existentes (`SkillManager.CarryingWeightRelativeModifier`, `IsSearchDouble`, `AttachedLauncher`) mantêm patch e semântica inalterados. |
| 11 | Pacote FIKA próprio (`INetSerializable`) | **N/A** | O CustomClasses **não declara pacote FIKA**. A coordenação coop é feita pela rota HTTP `/customclasses/class-identities` (item 057), que não muda nesta rodada. |

## Histórico

| Data | Evento |
|---|---|
| 2026-08-22 | Spec técnica criada (`/optimize-mod-performance` Fase 2, perfil de não-regressão) |
| 2026-08-23 | Revisão técnica 03 aplicada — 7 pontos aceitos. Principais: `ClassIdOf` como **único** resolvedor do hot path, `ClassNameEnOf` **removido** e `NameOf(EClassId)` criado para o diagnóstico (PA-03-01 — a migração ingênua deixaria **dois** lookups por passo); `__state` capturado **incondicionalmente antes** dos branches em `SetAnimatorAndProceduralValues` (PA-03-02 — o try/catch por branch contém a exceção mas não desfaz a escrita); `Touch()` definida com move-to-end (PA-03-03); §5.9 nova com o diff exato dos `Enable()` (PA-03-05); `Parse` ganha `warnUnknown` (PA-03-06); `RecoilFloorPatch.cs` deletado com o XMLdoc do B15 migrado (PA-03-07). |
| 2026-08-23 | Revisão técnica 02 aplicada — 9 pontos aceitos. Principais: `try/catch` **por branch** nos 4 alvos consolidados (PA-02-01 — um catch externo faria um branch que lança pular o piso B15); bump de versão em **4** arquivos, não 2, incluindo os dois que aparecem em log (PA-02-02); checagem de boot `Parse` ↔ `ClassColors` (PA-02-03); `activeInHierarchy` no cache do painel (PA-02-04); tabela de posição dos contadores (PA-02-05); evidência das prioridades registrada (PA-02-06); `RecoilBranches` com casa definida (PA-02-07); comentário anti-remoção no `EnsureLoaded` (PA-02-08); `/update-mod-graph` no gate de Fase 4 (PA-02-09). |
| 2026-08-23 | Revisão técnica 01 aplicada — 10 pontos aceitos. Principais: `Shoot` consolida **4→2** e não 4→1, preservando `Priority.First`/`Last` contra mods externos (PA-01-01); clamp no `Quantize` (PA-01-02); `className` na chave do cache de tooltip (PA-01-03); fronteira pública do ICM declarada intocável e `EClassId` `internal` (PA-01-06); **`AUD-01-07b` dropado** (PA-01-07); bump de SemVer, extração explícita do `BuildTinted` e `PerfDumpLoop` com condição de saída (PA-01-08/09/10). |
