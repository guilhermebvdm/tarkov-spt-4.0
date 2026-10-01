# 022 — Médico ganha XP ao curar aliado · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [022-medico-xp-cura-aliado-01-spec.md](022-medico-xp-cura-aliado-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha` reconfirmada nesta sessão (AP-09).

## 0. Memória e docs consultados

**Memória consultada:** snapshot de 2026-09-05 (Sessão 10) · pendências que afetam: nenhuma diretamente (P-9.1 🔴 é validação in-game da build v1.13.5, não deste item — não bloqueia, mas o build atual do workspace já está em v1.13.6).

**Docs técnicos lidos (gatilho disparado):**
- `docs/technical/spt-antipatterns.md` — sempre (AP-01…AP-11, ver §9).
- `docs/technical/fika-packet-desync-prevention-plan.md` — o item estende um pacote `INetSerializable` existente (Caminho B envia XP pela rede FIKA).
- `docs/technical/spt4-vs-spt41-gclass-deobfuscation.md` — o item cita `GClass2266`, `GClass2268`, `GInterface326` (aliases resolvidos abaixo).

## 1. Estratégia

O mecanismo vanilla de XP de cura **não é o que a investigação prévia assumia**. Reconfirmado nesta sessão:

- `GClass2266` (alias 4.1 não-verificado: `EFT.HealthStatisticsManager`) é a classe base do "tracker" de estatísticas por-jogador. `method_6(IEffect effect)` ([GClass2266.cs:302-315](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L302)) é o consumidor único de XP de cura: extrai `effect.HealExperience` via a interface `GInterface326` (alias: `IExperienceHealthEffect`, [GInterface326.cs](../../../../references/eft-decompiled/Assembly-CSharp/GInterface326.cs) — `int HealExperience { get; }`) e, se `> 0`, chama `ExperienceGained(num)` + `Profile_0.EftStats.SessionCounters.AddInt(num, SessionCounterTypesAbstractClass.ExpHeal)` + `ShowStatNotification(...)`.
- **O evento que dispara `method_6` é `IHealthController.HealerDoneEvent`** ([IHealthController.cs:64](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/IHealthController.cs#L64), `event Action<IEffect>`), assinado em `GClass2266.BeginStatisticsSession()` ([GClass2266.cs:117](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L117): `IHealthController_0.HealerDoneEvent += method_6;`) — **não** `EffectRemovedEvent` como a investigação prévia supunha (esse dispara `method_10`, que só conta contadores de "item usado", [GClass2266.cs:357-381](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L357)).
- `HealerDoneEvent` é disparado dentro de `MedEffect.Residue()` ([ActiveHealthController.cs:1919,1924](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L1919)) — uma vez por efeito de dano REALMENTE resolvido (`HeavyBleeding`/`LightBleeding`/`Fracture`/`Intoxication`/`Pain`) naquele tick de tratamento, em `base.HealthController` = o `ActiveHealthController` **dono do efeito** (ou seja, do PACIENTE, nunca do médico).
- `IHealthController_0` em `GClass2266.Init(Profile, IHealthController)` ([GClass2266.cs:87-92](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L87)) é sempre o **próprio** `HealthController` do jogador a quem aquele tracker pertence — `LocationStatisticsCollectorAbstractClass.Init(Player player)` ([LocationStatisticsCollectorAbstractClass.cs:117-121](../../../../references/eft-decompiled/Assembly-CSharp/LocationStatisticsCollectorAbstractClass.cs#L117)) faz `Init(Player_0.Profile, Player_0.HealthController)`. **Conclusão:** o auto-cura vanilla só funciona porque paciente == médico (mesma pessoa, mesmo `HealthController`, mesmo tracker). Curar OUTRA pessoa nunca creditaria o médico no fluxo nativo — creditaria, na melhor das hipóteses, o PACIENTE (se o paciente tivesse um tracker "de verdade" ligado).
- **E o paciente não tem um tracker "de verdade" na maioria dos casos do ICM.** `Player.StatisticsManager` ([Player.cs:25111](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25111)) recebe `new GClass2268()` (`LocationStatisticsCollectorAbstractClass` concreto, alias `EFT.LocalStatisticManager`) **apenas** para o jogador local com `isYourPlayer:true, aiControl:false` ([BaseLocalGame-1.cs:743](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BaseLocalGame-1.cs#L743)). Bots locais (`LocalGame.cs:339`, `aiControl:true`) e peers Fika observados neste cliente (`ClientPlayer.cs:2891`) recebem `new GClass2265()` (`EFT.DumbStatisticsManager`, `IStatisticsManager` sem lógica de XP). Logo: **nenhum "aliado" tratado pelo ICM (bot local ou peer remoto observado) tem um tracker que faça algo com `HealerDoneEvent`** — não há risco de o vanilla "vazar" XP pro paciente ao religarmos esse fio; e não há XP nenhum fluindo hoje, exatamente como a spec funcional descreve.
- **Estratégia adotada — replicar o valor vanilla chamando o MESMO consumidor (`GClass2266.method_6`), no HealthController de quem deve ser creditado (o médico), reagindo ao mesmo evento que o vanilla usaria (`HealerDoneEvent`) no HealthController de quem tem o efeito (o paciente).** Isto satisfaz a exigência da spec funcional ("mesma quantidade que o vanilla", sem valor novo) porque é literalmente o mesmo código, e cobre a variação por tipo/severidade automaticamente (`HealExperience` já é lido do efeito real, cujos valores em `globals.json` são: `LightBleeding=25`, `HeavyBleeding=40`, `Fracture=30`, `Intoxication=150`, `Pain=20` — reconfirmados em [globals.json:30222-30366](../../../../references/spt-source/Libraries/SPTarkov.Server.Assets/SPT_Data/database/globals.json#L30222); **portanto XP VARIA por tipo/severidade de ferimento**, confirmando a suspeita da spec funcional).
- Nenhum patch novo em método do EFT é necessário — é uma **assinatura/desassinatura de evento adicional** (Caminho A) e uma **extensão de payload de pacote de rede já existente** (Caminho B). Zero Harmony patch novo.
### 1.1 Mecanismo 2 — XP por HP restaurado (`ExpForHeal`) — escopo expandido em `/review-technical-spec` PA-01-01

Restaurar HP puro (com ou sem remover um efeito) usa um mecanismo vanilla **diferente e independente** do §1 principal — confirmado nesta sessão, incluído no escopo por decisão do usuário (PA-01-01):

- `GClass2266.OnHealthChanged(EBodyPart bodyPart, float diff, DamageInfoStruct damageInfo)` ([GClass2266.cs:169-189](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L169)) é assinado em `IHealthController.HealthChangedEvent` ([IHealthController.cs:44](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/IHealthController.cs#L44) — **linha diferente da 64 usada pelo `HealerDoneEvent`**; confirmar no code-mod que não são confundidos). Quando `diff > 0` (HP restaurado) e `IsRestoreHealsByHealing(damageInfo)` é `true` (sempre `true` — nenhuma subclasse usada em raid a sobrescreve, `GClass2266.cs:191-194`), acumula `Float_0 += diff` e calcula `num = GClass1720_0.Heal.ExpForHeal * diff` para `ExperienceGained(num)` (no-op — ver nota abaixo) e `ShowHealingNotification(num)`.
- **O valor REALMENTE persistido no contador de sessão não é creditado no `OnHealthChanged`** — é diferido: `Float_0` acumula ao longo de TODA a sessão/raid e só é convertido para `int` e gravado em `Profile_0.EftStats.SessionCounters.AddInt(value, ExpHeal)` dentro de `method_1()` ([GClass2266.cs:256-264](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L256)), chamado por `ConsumeExperience()` → `method_0()`, ao FIM da sessão de estatísticas (fim de raid). **Isto importa para a correção da implementação:** truncar `(int)(ExpForHeal * diff)` a cada tick pequeno de cura (ex.: MedKit curando poucos HP por segundo) perderia quase todo o XP por arredondamento repetido para zero — o vanilla evita isso acumulando em `float` primeiro e truncando **uma única vez**. O ICM precisa replicar esse acúmulo (por operação de cura, não precisa ser por raid inteira) — ver §5.2/§5.4.
- `ExperienceGained(float)` é `public virtual void ExperienceGained(float experience) { }` — **vazio na base** ([GClass2266.cs:196-198](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L196)) e **não sobrescrito** por `GClass2268`/`LocationStatisticsCollectorAbstractClass` (a classe real usada em raid) — confirmado por grep, zero overrides. Chamá-lo é inofensivo/fiel ao vanilla, mas não faz nada observável; o crédito real é sempre via `SessionCounters.AddInt`/`AddFloat` explícito.
- Fórmula: `ExpForHeal` vem de `BackendConfigSettingsClass.Experience.Heal.ExpForHeal` ([BackendConfigSettingsClass.cs:2258](../../../../references/eft-decompiled/Assembly-CSharp/BackendConfigSettingsClass.cs#L2258) → `.Heal` em [:642](../../../../references/eft-decompiled/Assembly-CSharp/BackendConfigSettingsClass.cs#L642) → `.ExpForHeal` em [:531](../../../../references/eft-decompiled/Assembly-CSharp/BackendConfigSettingsClass.cs#L531)), deserializado do campo `expForHeal` do servidor ([globals.json:36324](../../../../references/spt-source/Libraries/SPTarkov.Server.Assets/SPT_Data/database/globals.json#L36324) = `1` no SPT padrão — **ler em runtime via `Singleton<BackendConfigSettingsClass>.Instance`, nunca hardcode**, pois é config de servidor).
- **Onde `HealthChangedEvent` dispara:** `ActiveHealthController.ChangeHealth(EBodyPart bodyPart, float value, DamageInfoStruct damageInfo)` ([ActiveHealthController.cs:3933-3954](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3933), evento na [:3949](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3949)) — **método público**, já chamado diretamente pelo próprio `BandAidNetworkHandler.cs` no Caminho B (`:424,526`, já existente) e internamente pelo `MedEffect` do Caminho A a cada tick de cura. Dispara em `base.HealthController` = a mesma controller cujo `ChangeHealth` foi chamado (a do PACIENTE), pelo mesmo motivo do §1 (paciente não tem tracker real — sem risco de vazar XP pra ele).
- **Restaurar membro destruído (`RestoreBodyPart`/`FullRestoreBodyPart`) NÃO passa por `ChangeHealth`** ([ActiveHealthController.cs:3912-3921](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3912) — sem `HealthChangedEvent`), então cirurgia continua fora do escopo de XP também para este 2º mecanismo, coerente com a conclusão do parágrafo anterior sobre `HealExperience`.
- **Restauração de membro destruído via cirurgia não gera `HealExperience` nem no vanilla.** No `Residue()`, o branch `EDamageEffectType.DestroyedPart` seta `flag=true` mas `gClass2` permanece `null`; o `HealerDoneEvent?.Invoke(gClass2)` subsequente é invocado com `null`, e `method_6` trata `null is GInterface326` como falso — nenhuma XP é gerada. Confirma que o path de cirurgia (`ApplyFullTreatmentLocally`, ramo `stats.IsSurgery`) fica **fora do escopo de crédito de XP**, coerente com o vanilla.

## 2. Pontos de patch

Nenhum patch Harmony novo. Pontos de EVENTO/API do Assembly usados (não patcheados — apenas consumidos, como o mod já faz para `EffectRemovedEvent`):

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`IHealthController.cs:64`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/IHealthController.cs#L64) `event Action<IEffect> HealerDoneEvent` | Consumo de evento (subscribe/unsubscribe) | Dispara 1x por efeito curável de fato resolvido — ponto exato onde o vanilla creditaria XP ao dono do efeito. |
| [`GClass2266.cs:302-315`](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L302) `method_6(IEffect effect)` (público) | Chamada direta (reuso de API existente) | Reusa 100% da lógica vanilla de crédito (valor, contador de sessão, notificação) — nenhum valor novo é inventado. |
| [`GInterface326.cs`](../../../../references/eft-decompiled/Assembly-CSharp/GInterface326.cs) `int HealExperience { get; }` | Leitura de propriedade | Fonte do valor por tipo/severidade de efeito (25/30/40/150/20 — ver §1). |
| [`Player.cs:25111`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25111) `IStatisticsManager StatisticsManager { get; set; }` | Leitura de propriedade + cast | Acesso ao tracker do médico (sempre `GClass2268` real, pois médico == `MainPlayer` local — confirmado em `/review-spec`, `MedicHealPatch.cs:253-254,546-547`). |
| [`IHealthController.cs:50`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/IHealthController.cs#L50) `event Action<EBodyPart, float, DamageInfoStruct> HealthChangedEvent` | Consumo de evento (subscribe/unsubscribe) | Mecanismo 2 (§1.1) — dispara a cada `ChangeHealth` com `diff > 0` no paciente; fonte do XP de HP restaurado. Desassinado no mesmo ciclo de vida já existente (junto de `EffectRemovedEvent`/`HealerDoneEvent`). |
| [`ActiveHealthController.cs:3933-3954`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3933) `ChangeHealth(EBodyPart, float, DamageInfoStruct)` (público) | Leitura/contexto — já chamado pelo mod | Caminho B já chama isso; o valor `heal`/`value` já está disponível localmente para o cálculo de XP sem precisar de evento novo. |
| [`BackendConfigSettingsClass.cs:2258,642,531`](../../../../references/eft-decompiled/Assembly-CSharp/BackendConfigSettingsClass.cs#L2258) `Experience.Heal.ExpForHeal` (público) | Leitura de propriedade | Multiplicador de XP por HP restaurado, lido do servidor (`globals.json:36324`), nunca hardcoded. |

## 3. Novas propriedades F12 (BepInEx)

N/A — a spec funcional não pede toggle; o comportamento passa a espelhar o vanilla incondicionalmente (mesma regra que já vale para o auto-cura). Nenhuma `ConfigEntry` nova.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-V3(review)/Helpers/HealXpCredit.cs` | CRIAR | Helper único `CreditHealXp(Player doctor, int amount)` — as 3 linhas de crédito (`ExperienceGained`+`AddInt(ExpHeal)`+`ShowStatNotification`), compartilhado pelos Caminhos A e B, para os dois mecanismos (efeito e HP). Fecha PA-01-05. |
| `modded-V3(review)/Patches/Medical/MedicHealPatch.cs` | MODIFICAR | Caminho A: assina `HealerDoneEvent` (XP de efeito) e `HealthChangedEvent` (XP de HP, com acumulador por operação) do paciente, junto com `EffectRemovedEvent` já existente (mesmo par subscribe/cleanup); handlers chamam `HealXpCredit.CreditHealXp`. |
| `modded-V3(review)/Patches/Medical/BandAidTreatmentReportPacket.cs` | MODIFICAR (rename V2→V3) | Caminho B: novo campo `XpAwarded` (soma dos dois mecanismos) no report paciente→médico. Mudança de layout exige renomear o tipo (AP-11/`fika-packet-desync-prevention-plan.md`). |
| `modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs` | MODIFICAR | Caminho B: computa `xpAwarded` em `ApplyFullTreatmentLocally` — soma de `HealExperience` dos efeitos (lido 1x via `FindEffectForRead`, reaproveitado da checagem de existência — fecha PA-01-02/04) **+** `ExpForHeal × heal` do HP restaurado nesta aplicação; popula o novo campo em `SendTreatmentReport`; credita o médico em `OnTreatmentReportReceived` via `HealXpCredit.CreditHealXp`; registra `BandAidTreatmentReportPacketV3` + stub de descarte para `V2` (peer desatualizado). |
| `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md` | — | Não modificar — nenhuma `ConfigEntry` nova (§3). |

## 5. Stubs de código

### 5.1 Helper compartilhado — `Helpers/HealXpCredit.cs` (NOVO — fecha PA-01-05)

```csharp
// modded-V3(review)/Helpers/HealXpCredit.cs
using EFT;
using EFT.HealthSystem;

namespace TRLImmersiveCombatMedicine.Helpers
{
    /// <summary>
    /// Credita XP de cura ao médico replicando as 3 chamadas que o vanilla usa em auto-cura
    /// (ref: Assembly-CSharp/GClass2266.cs:309-313, dentro de method_6) — único ponto de
    /// crédito usado pelos dois mecanismos (HealExperience por efeito, ExpForHeal por HP)
    /// e pelos dois caminhos (A local, B rede). Nunca inventa valor — só recebe o `amount`
    /// já calculado a partir de dados 100% vanilla (globals.json via BackendConfigSettingsClass).
    /// </summary>
    internal static class HealXpCredit
    {
        public static void CreditHealXp(Player doctor, int amount)
        {
            if (doctor == null || amount <= 0) return;

            // GClass2268 (LocalStatisticManager) é o único IStatisticsManager com lógica real;
            // bots/peers observados usam GClass2265 (Dumb) — cast falha (null), não credita ninguém.
            // ref: Assembly-CSharp/EFT/BaseLocalGame-1.cs:743 (isYourPlayer:true → new GClass2268())
            if (doctor.StatisticsManager is GClass2266 statsManager)
            {
                // ref: Assembly-CSharp/GClass2266.cs:196-198 — ExperienceGained é virtual vazio na
                // base e NÃO sobrescrito por GClass2268 (grep confirmado); chamada é fiel ao vanilla
                // mesmo sendo no-op na prática — mantém paridade se um dia for sobrescrito.
                statsManager.ExperienceGained(amount);
                doctor.Profile.EftStats.SessionCounters.AddInt(amount, SessionCounterTypesAbstractClass.ExpHeal);
                statsManager.ShowStatNotification(LocalizationKey.StatsTreatment, LocalizationKey.StatsHealed, amount);
            }
        }
    }
}
```

### 5.2 Caminho A — `MedicHealPatch.cs`

```csharp
// modded-V3(review)/Patches/Medical/MedicHealPatch.cs
// Adição ao bloco de campos estáticos existente (perto de _subscribedPatientHc):
// Acumula XP de HP (mecanismo 2, §1.1) em FLOAT durante toda a operação de cura atual —
// truncar por tick perderia XP em curas graduais pequenas (ver §1.1). Resetado ao assinar,
// consumido (truncado 1x) ao desassinar em CleanupPatientSubscription.
private static float _pendingHealthXp = 0f;

// Novo handler — espelha OnPatientEffectRemoved (mesmo arquivo, ~linha 106), mas reage ao
// evento que de fato credita XP de EFEITO no vanilla (mecanismo 1).
// ref: Assembly-CSharp/EFT.HealthSystem/IHealthController.cs:64 (HealerDoneEvent)
// ref: Assembly-CSharp/GInterface326.cs (IExperienceHealthEffect.HealExperience)
private static void OnPatientHealerDone(IEffect effect)
{
    try
    {
        int amount = (effect as GInterface326)?.HealExperience ?? 0;
        if (amount <= 0) return;
        var doctor = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
        TRLImmersiveCombatMedicine.Helpers.HealXpCredit.CreditHealXp(doctor, amount);
    }
    catch (Exception ex)
    {
        Logger.LogWarning($"OnPatientHealerDone: {ex.Message}");
    }
}

// Novo handler — mecanismo 2 (§1.1): acumula, NÃO credita ainda (credita 1x no cleanup).
// ref: Assembly-CSharp/EFT.HealthSystem/IHealthController.cs:50 (HealthChangedEvent)
// ref: Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:3933-3954 (ChangeHealth)
private static void OnPatientHealthChanged(EBodyPart bodyPart, float diff, DamageInfoStruct damageInfo)
{
    if (diff <= 0f) return; // diff negativo = dano, não cura — não conta
    try
    {
        // ref: Assembly-CSharp/BackendConfigSettingsClass.cs:2258,642,531 (Experience.Heal.ExpForHeal)
        float expForHeal = Comfort.Common.Singleton<BackendConfigSettingsClass>.Instance.Experience.Heal.ExpForHeal;
        _pendingHealthXp += expForHeal * diff;
    }
    catch (Exception ex)
    {
        Logger.LogWarning($"OnPatientHealthChanged: {ex.Message}");
    }
}
```

No `Prefix` (bloco que já assina `EffectRemovedEvent`, `MedicHealPatch.cs:412-416`):

```csharp
// Bridge: subscrever EffectRemovedEvent do paciente
CleanupPatientSubscription();
_currentObservedMedsControllerClass = __instance;
_subscribedPatientHc = CurrentPatient.HealthController;
_subscribedPatientHc.EffectRemovedEvent += OnPatientEffectRemoved;
_subscribedPatientHc.HealerDoneEvent += OnPatientHealerDone;     // NOVO — XP de efeito (022, mecanismo 1)
_subscribedPatientHc.HealthChangedEvent += OnPatientHealthChanged; // NOVO — XP de HP (022, mecanismo 2)
_pendingHealthXp = 0f; // reset — nova operação de cura começa do zero
```

Em `CleanupPatientSubscription()` (`MedicHealPatch.cs:194-202`) — este é o ÚNICO ponto de saída do par subscribe/cleanup (chamado tanto no início de um novo redirect quanto no fim natural do bridge, `OnPatientEffectRemoved:116`), por isso é o ponto certo para o truncamento único do XP de HP acumulado:

```csharp
public static void CleanupPatientSubscription()
{
    if (_subscribedPatientHc != null)
    {
        _subscribedPatientHc.EffectRemovedEvent -= OnPatientEffectRemoved;
        _subscribedPatientHc.HealerDoneEvent -= OnPatientHealerDone;         // NOVO — par da assinatura acima
        _subscribedPatientHc.HealthChangedEvent -= OnPatientHealthChanged;   // NOVO — par da assinatura acima
        _subscribedPatientHc = null;
    }

    // Trunca UMA VEZ o acumulado da operação que está terminando (mesmo padrão do
    // vanilla method_1, GClass2266.cs:256-264 — acumula em float, trunca 1x no fim da sessão).
    if (_pendingHealthXp > 0f)
    {
        int flushed = (int)_pendingHealthXp;
        if (flushed > 0)
        {
            var doctor = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
            TRLImmersiveCombatMedicine.Helpers.HealXpCredit.CreditHealXp(doctor, flushed);
        }
        _pendingHealthXp = 0f;
    }
}
```

> **Nota de granularidade (documentar, não é bug):** o vanilla acumula `Float_0` por RAID inteira antes de truncar (`ConsumeExperience` só roda no fim da sessão). Aqui o acumulador é escopado por OPERAÇÃO DE CURA (do subscribe ao cleanup), não pela raid inteira — perda de precisão só nos decimais residuais entre uma cura e a próxima (no máximo <1 XP por operação), nunca por tick. Mesma granularidade adotada no Caminho B (§5.4) — comportamento simétrico entre os dois caminhos.

### 5.3 Caminho B — pacote (`BandAidTreatmentReportPacket.cs`)

```csharp
// modded-V3(review)/Patches/Medical/BandAidTreatmentReportPacket.cs
// Layout mudou (campo novo) → renomear o tipo (AP-11 / fika-packet-desync-prevention-plan.md).
// ref: docs/technical/fika-packet-desync-prevention-plan.md:50 ("mudança de formato exige renomear o tipo")
namespace TRLImmersiveCombatMedicine.Medical
{
    public struct BandAidTreatmentReportPacketV3 : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;
        public byte BodyPart;
        public float HealedAmount;
        public float CostAmount;
        public int XpAwarded;      // NOVO (022) — soma de IEffect.HealExperience dos efeitos resolvidos nesta aplicação

        internal bool Valid;

        public void Serialize(NetDataWriter writer)
        {
            var inner = PacketEnvelope.Open();
            inner.Put(DoctorProfileId ?? string.Empty);
            inner.Put(PatientProfileId ?? string.Empty);
            inner.Put(ItemTemplateId ?? string.Empty);
            inner.Put(BodyPart);
            inner.Put(HealedAmount);
            inner.Put(CostAmount);
            inner.Put(XpAwarded);
            PacketEnvelope.Close(writer, inner);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = string.Empty;
            PatientProfileId = string.Empty;
            ItemTemplateId = string.Empty;
            BodyPart = 0;
            HealedAmount = 0f;
            CostAmount = 0f;
            XpAwarded = 0;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;
            if (!PacketEnvelope.TryReadString(inner, out DoctorProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out PatientProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out ItemTemplateId)) return;
            if (!inner.TryGetByte(out BodyPart)) return;
            if (!inner.TryGetFloat(out HealedAmount)) return;
            if (!inner.TryGetFloat(out CostAmount)) return;
            if (!inner.TryGetInt(out XpAwarded)) return;

            Valid = true;
        }
    }

    /// <summary>Stub do nome antigo — só descarta o payload de peer desatualizado (AP-11).</summary>
    public struct BandAidTreatmentReportPacketV2Stub : INetSerializable
    {
        public void Serialize(NetDataWriter writer) { }
        public void Deserialize(NetDataReader reader)
        {
            if (PacketEnvelope.TryOpen(reader, out var inner)) { /* descarta */ }
        }
    }
}
```

### 5.4 Caminho B — cálculo e crédito (`BandAidNetworkHandler.cs`)

> Fecha PA-01-02/PA-01-04: `FindEffectForRead` substitui `HasEffect` (o bool vira "tem instância" e a instância já serve pra ler `HealExperience` — uma única chamada de reflection por efeito, não duas) e usa os tipos "de leitura" (`_heavyBleedType` etc.), não os "de remoção".

```csharp
// modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs
// Substitui HasEffect nos usos de leitura+XP (linhas 499-501 atuais). HasEffect (linhas
// 601-616 atuais) pode continuar existindo para outros callers que só querem o bool.
// ref: Assembly-CSharp/GInterface326.cs (IExperienceHealthEffect.HealExperience)
private static IEffect FindEffectForRead(ActiveHealthController activeHc, EBodyPart bodyPart, Type effectType)
{
    if (effectType == null) return null;
    try
    {
        var findMethod = typeof(ActiveHealthController).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == "FindActiveEffect" && m.IsGenericMethod && m.GetParameters().Length == 1);
        return findMethod?.MakeGenericMethod(effectType).Invoke(activeHc, new object[] { bodyPart }) as IEffect;
    }
    catch { return null; }
}

// Em ApplyFullTreatmentLocally — substitui os hadHeavy/hadLight/hadFracture atuais
// (linhas 499-501) por instâncias, usando os tipos DE LEITURA (_heavyBleedType etc.,
// já existentes no arquivo — não os "concretos" de remoção):
var heavyEffect = FindEffectForRead(activeHc, target, _heavyBleedType);
var lightEffect = FindEffectForRead(activeHc, target, _lightBleedType);
var fractureEffect = FindEffectForRead(activeHc, target, _fractureType);
float effectCost = 0f;
int xpAwarded = 0;

// Mecanismo 1 (HealExperience por efeito) — acumula ANTES de remover, credita só se remover:
if ((stats.StopsHeavyBleed || stats.StopsAllBleeds) && heavyEffect != null &&
    RemoveEffectNative(hc, target, _heavyBleedConcreteType, "HeavyBleeding"))
{
    effectCost += stats.HeavyBleedCost;
    xpAwarded += (heavyEffect as GInterface326)?.HealExperience ?? 0;
}
if ((stats.StopsLightBleed || stats.StopsAllBleeds) && lightEffect != null &&
    RemoveEffectNative(hc, target, _lightBleedConcreteType, "LightBleeding"))
{
    effectCost += stats.LightBleedCost;
    xpAwarded += (lightEffect as GInterface326)?.HealExperience ?? 0;
}
if (stats.FixesFracture && fractureEffect != null &&
    RemoveEffectNative(hc, target, _fractureConcreteType, "Fracture"))
{
    effectCost += stats.FractureCost;
    xpAwarded += (fractureEffect as GInterface326)?.HealExperience ?? 0;
}

// (bloco de HP existente, linhas ~516-530, sem mudança até o `if (heal > 0)`) —
// Mecanismo 2 (ExpForHeal por HP) — trunca 1x por aplicação de tratamento em rede
// (mesma granularidade "por operação" do Caminho A, ver nota de §5.2; não é por-raid
// como o vanilla, mas evita perder XP em incrementos pequenos truncados por tick).
// ref: Assembly-CSharp/BackendConfigSettingsClass.cs:2258,642,531 (Experience.Heal.ExpForHeal)
if (heal > 0)
{
    activeHc.ChangeHealth(target, heal, default(DamageInfoStruct));
    healedTotal = heal;
    float expForHeal = Comfort.Common.Singleton<BackendConfigSettingsClass>.Instance.Experience.Heal.ExpForHeal;
    xpAwarded += (int)(expForHeal * heal);
    Logger.LogInfo($"HP +{heal:F1} em {target} pelo paciente (via rede | Disp:{availableForHp:F1}).");
}

// SendTreatmentReport ganha o novo parâmetro:
SendTreatmentReport(packet, target, healedTotal, totalCost, xpAwarded);
```

```csharp
// SendTreatmentReport (linha 881) — assinatura estendida:
private static void SendTreatmentReport(BandAidHealPacketV2 source, EBodyPart part, float healed, float cost, int xpAwarded)
{
    EnsurePacketsRegistered();
    if (_lastRegisteredNetworkManager == null) return;
    var report = new BandAidTreatmentReportPacketV3
    {
        DoctorProfileId = source.DoctorProfileId,
        PatientProfileId = source.PatientProfileId,
        ItemTemplateId = source.ItemTemplateId,
        BodyPart = (byte)part,
        HealedAmount = healed,
        CostAmount = cost,
        XpAwarded = xpAwarded
    };
    SendPacket(ref report, "TreatmentReport");
}

// OnTreatmentReportReceived (linha 897) — crédito do médico local via helper compartilhado
// (fecha PA-01-05 — mesmo ponto de crédito do Caminho A, TRLImmersiveCombatMedicine.Helpers.HealXpCredit):
if (packet.XpAwarded > 0)
{
    TRLImmersiveCombatMedicine.Helpers.HealXpCredit.CreditHealXp(mainPlayer, packet.XpAwarded);
}
```

## 6. Fluxo de dados

**Caminho A (paciente local, ex. bot) — dois mecanismos em paralelo, mesma janela de assinatura:**
```
[A] médico interage p/ curar aliado (MedicHealPatch.Prefix, já existente)
  → [B] patientHc.DoMedEffect(...) cria MedEffect no paciente (MedicHealPatch.cs:343)
  → [C] ICM assina patientHc.HealerDoneEvent + HealthChangedEvent (NOVO, ao lado de
       EffectRemovedEvent, :416) e zera _pendingHealthXp

  Mecanismo 1 (efeito):                       Mecanismo 2 (HP):
  → [D1] MedEffect.Residue() resolve 1         → [D2] MedEffect chama ChangeHealth a cada
       efeito por vez (ActiveHealthController        tick de cura (ActiveHealthController.cs:3933)
       .cs:1863-1927) → HealerDoneEvent.Invoke      → HealthChangedEvent.Invoke(parte,diff,...)
       (efeito) (:1919/:1924)                       (:3949) se diff>0
  → [E1] OnPatientHealerDone(effect) lê             → [E2] OnPatientHealthChanged acumula
       effect.HealExperience (GInterface326)             expForHeal*diff em _pendingHealthXp
       e chama HealXpCredit.CreditHealXp                 (NÃO credita ainda)
       IMEDIATAMENTE (mesma granularidade do vanilla,
       1 efeito = 1 crédito imediato)

  → [F] Tratamento termina → OnPatientEffectRemoved (GInterface376) → CleanupPatientSubscription()
       → desassina os 3 eventos e TRUNCA _pendingHealthXp 1x → HealXpCredit.CreditHealXp(flushed)
  → [G] HealXpCredit: doctor.StatisticsManager as GClass2266 → ExperienceGained (no-op) +
       SessionCounters.AddInt(ExpHeal) + ShowStatNotification ("Tratamento: Curado +N")
```

**Caminho B (paciente remoto/rede) — os dois mecanismos calculados no mesmo lugar, 1 report:**
```
[A] médico envia BandAidHealPacketV2 → [B] ApplyFullTreatmentLocally roda no cliente
    que simula o paciente (o próprio paciente humano, ou o host simulando um bot)
  → [C] FindEffectForRead(effectType) ANTES de cada RemoveEffectNative — 1 chamada de
       reflection por efeito, reaproveitada tanto pra saber "existe?" quanto pra ler
       HealExperience (GInterface326) — soma em xpAwarded (mecanismo 1)
  → [D] RemoveEffectNative remove o efeito de fato (comportamento já existente)
  → [E] activeHc.ChangeHealth(target, heal, ...) (já existente) → soma
       (int)(ExpForHeal × heal) em xpAwarded (mecanismo 2, truncado 1x por aplicação)
  → [F] SendTreatmentReport empacota xpAwarded (soma dos 2 mecanismos) em
       BandAidTreatmentReportPacketV3 (renomeado)
  → [G] pacote viaja pela rede FIKA até o cliente do MÉDICO
  → [H] OnTreatmentReportReceived (no cliente do médico) chama
       HealXpCredit.CreditHealXp(mainPlayer, packet.XpAwarded) — mesmo helper do Caminho A
```

## 7. Riscos e dependências

- **Mudança de wire format (Caminho B).** `BandAidTreatmentReportPacketV2` ganha um campo → **precisa** renomear para `V3` (a hash de roteamento do FIKA deriva do NOME do tipo, não da versão do mod — `docs/technical/fika-packet-desync-prevention-plan.md:50`). Exige: (1) bump de versão do mod, (2) nota de release **lockstep** (host e todos os clients atualizam juntos — mesmo padrão já registrado em memória, Sessão 6, para a mudança de wire format da 1.11.0), (3) registrar `RegisterPacket<BandAidTreatmentReportPacketV2Stub>` só para descartar payload de peer desatualizado sem matar a fila de eventos do frame (AP-11).
- **`node scripts/check-packet-hashes.js` obrigatório** após o rename — 16 bits de hash, colisão silenciosa é possível (regra do §9 da skill `spt-mod-best-practices`).
- **Patches existentes que tocam os mesmos pontos:** `MedicHealPatch.cs` já assina/desassina `EffectRemovedEvent` no mesmo par subscribe/cleanup (`:416`/`:198`) — as novas assinaturas de `HealerDoneEvent`/`HealthChangedEvent` seguem o MESMO ciclo de vida, sem hook de raid novo (não há lifecycle novo a cobrir: a assinatura nasce e morre dentro de uma única operação de cura, não atravessa raids).
- **`ApplyFullTreatmentLocally` também é usado no ramo de cirurgia e no ramo de bot local hospedado pelo host (`:947` "aplica o FullTreatment em nome de um BOT local").** `FindEffectForRead`/o cálculo de `xpAwarded` só correm no ramo de efeitos+HP (bleeding/fracture/MedKit), nunca no ramo de cirurgia — consistente com a conclusão do §1/§1.1 de que cirurgia não gera XP nem no vanilla (nem por efeito, nem por HP — `RestoreBodyPart` não passa por `ChangeHealth`).
- **Granularidade de truncamento do XP de HP (mecanismo 2) é "por operação de cura", não "por raid" como o vanilla.** Documentado como escolha deliberada em §5.2/§5.4 — perda de precisão limitada a <1 XP por operação, nunca por tick. Se o usuário quiser paridade estrita com o vanilla (acumulador por raid inteira), é um redesign maior (accumulator persistente por médico, flush em `EndStatisticsSession`) — não incluído aqui por não valer a complexidade para <1 XP de diferença.
- **Compatibilidade com outros mods:** nenhuma — o crédito de XP é 100% local ao ICM (não há Harmony patch em método do EFT, só consumo de evento/API pública já exposta pelo próprio jogo).
- **Fika/coop:** o médico só pode ser o `MainPlayer` LOCAL de quem inicia a cura (`MedicHealPatch.cs:253-254,546-547`, reconfirmado). Em ambos os caminhos, o crédito de XP acontece SEMPRE no processo/cliente do médico, nunca é replicado para outro peer — não há duplicação host↔client possível porque cada cliente só credita o PRÓPRIO `MainPlayer.StatisticsManager`.

## 8. Checklist de implementação

- [x] Criar `Helpers/HealXpCredit.cs` com `CreditHealXp(Player doctor, int amount)` (§5.1).
- [x] Caminho A: adicionar assinatura/desassinatura de `HealerDoneEvent` E `HealthChangedEvent` em `MedicHealPatch.cs` (par com `EffectRemovedEvent`, §5.2).
- [x] Caminho A: implementar `OnPatientHealerDone(IEffect effect)` (lê `HealExperience`, credita imediatamente via `HealXpCredit`) e `OnPatientHealthChanged(...)` (acumula em `_pendingHealthXp`, NÃO credita).
- [x] Caminho A: truncar/flush `_pendingHealthXp` uma única vez dentro de `CleanupPatientSubscription()`, antes de zerá-lo.
- [x] Caminho B: renomear `BandAidTreatmentReportPacketV2` → `BandAidTreatmentReportPacketV3` com campo `XpAwarded`; registrar stub de descarte para o nome antigo.
- [x] Caminho B: substituir `HasEffect` (nos 3 usos de leitura+XP) por `FindEffectForRead(...)`; acumular `xpAwarded` do mecanismo 1 (efeito) só quando `RemoveEffectNative` retornar `true`.
- [x] Caminho B: acumular `xpAwarded` do mecanismo 2 (HP) logo após `activeHc.ChangeHealth(...)`, usando `Singleton<BackendConfigSettingsClass>.Instance.Experience.Heal.ExpForHeal`.
- [x] Caminho B: estender `SendTreatmentReport` com o parâmetro `xpAwarded` (soma dos 2 mecanismos) e popular o novo campo do pacote.
- [x] Caminho B: em `OnTreatmentReportReceived`, chamar `HealXpCredit.CreditHealXp(mainPlayer, packet.XpAwarded)` quando `packet.XpAwarded > 0`.
- [x] Atualizar todos os `RegisterPacket<BandAidTreatmentReportPacketV2>` → `V3` (+ stub `V2`) em `EnsurePacketsRegistered`/`OnNetworkManagerCreated`.
- [x] Rodar `node scripts/check-packet-hashes.js` após o rename — ✓ nenhuma colisão CRC-16.
- [x] Bump de versão SemVer (`TRL-ImmersiveCombatMedicine.csproj` + `TRLImmersiveCombatMedicinePlugin.cs`) — `1.13.6` → `1.14.0`.
- [x] `dotnet build ... -c Release -o builds/...` (sem instalar no jogo — restrição do usuário) — Build succeeded, 0 Warning(s), 0 Error(s).
- [ ] Validar in-game (usuário, fora deste command): (a) cura de aliado local (bot) credita XP de efeito ao médico igual ao self-heal do mesmo efeito; (b) MedKit sem nenhum efeito ativo credita só XP de HP; (c) idem para cura de peer remoto via rede; (d) self-heal não duplica; (e) paciente não ganha XP (nem efeito nem HP); (f) kit resolvendo múltiplos efeitos + HP soma corretamente; (g) cura gradual longa (MedKit) não perde XP de HP por truncamento por tick.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | As assinaturas de `HealerDoneEvent`/`HealthChangedEvent` (Caminho A) nascem/morrem dentro de uma única operação de cura, no MESMO par subscribe/cleanup já existente para `EffectRemovedEvent` (`MedicHealPatch.cs:416`/`:198`) — não introduzem estado que atravesse raids; `_pendingHealthXp` é sempre zerado no mesmo cleanup que o desassina (§5.2). Caminho B não guarda estado nenhum (cálculo local por chamada). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Médico é sempre `Singleton<GameWorld>.Instance.MainPlayer` do cliente que credita (`MedicHealPatch.cs:253-254,546-547`; §7 "Fika/coop"). `HealXpCredit.CreditHealXp` (§5.1) só age se `doctor.StatisticsManager is GClass2266` — falha silenciosamente para qualquer `IStatisticsManager` que não seja o real (`GClass2265` dos bots/peers observados) — não há caminho de creditar a pessoa errada. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Não há patch em método virtual/abstract do EFT — só consumo de evento (`HealerDoneEvent`, `HealthChangedEvent`) e leitura de propriedades/campos públicos concretos. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `HealXpCredit.CreditHealXp` (§5.1) replica as 3 chamadas exatas que `GClass2266.method_6` usaria (`ExperienceGained`+`AddInt(ExpHeal)`+`ShowStatNotification`) — side-effects mapeados e idênticos ao auto-cura vanilla, único ponto de escrita para os 2 mecanismos × 2 caminhos (fecha PA-01-05). Nenhum campo interno é escrito fora dele. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver spec funcional §"Estado entre raids": XP é 100% persistência vanilla (`Profile.EftStats`), sem estado novo introduzido por este item. `_pendingHealthXp` não sobrevive além de uma operação de cura (flush no cleanup) — nada para limpar em morte/MIA/alt-F4 além do que `CleanupPatientSubscription` já cobre. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Não há patch Harmony novo nem invocação recursiva do método patcheado. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | ✅ | `_subscribedPatientHc`/`_pendingHealthXp` seguem o MESMO ciclo de vida já auditado (reset no subscribe, flush+zero no cleanup, `MedicHealPatch.cs:412-416`/`:194-202`) — nenhum cache novo sobrevive a troca de arma/operação/paciente. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump; "não existe" conferido no `types-index.json` — AP-09 | ✅ | Toda linha citada nesta spec foi lida nesta sessão em `references/eft-decompiled/Assembly-CSharp/` e `references/spt-source/` (não copiada da investigação prévia sem reconfirmar) — ver §1 (correção do mecanismo, `HealerDoneEvent` não `EffectRemovedEvent`) e §1.1 (mecanismo 2, `HealthChangedEvent`/`ExpForHeal`, adicionado por decisão do usuário em PA-01-01). |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT como lever de efeito; usa API de estatísticas/XP diretamente. |
| 11 | Pacote FIKA próprio conforme envelope/TryGet/Valid/rename — AP-11 | ✅ | Stub V3 mantém envelope de comprimento (`PacketEnvelope.Open/Close`, `TryOpen`) e só usa `TryGet*` (§5.3); rename V2→V3 por mudança de layout + stub de descarte do nome antigo planejados (§7/§8), seguindo `fika-packet-desync-prevention-plan.md`. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Revisão 01 (5 pontos): PA-01-01 expandiu o escopo (mecanismo 2, `ExpForHeal`/HP); PA-01-02/03/04/05 aplicados (helper `HealXpCredit` compartilhado, `FindEffectForRead` unificado, campo morto removido, tipos "de leitura" corrigidos). Todos ✅ Resolvido. |
| 2026-09-09 | Revisão 02 (1 ponto, 🟢): PA-02-01 (`OnPatientHealthChanged` não filtra `EBodyPart`) aceito como comportamento fiel ao vanilla, sem mudança de código; corner case adicionado à spec funcional. 0 🔴 — pronto para `/code-mod`. |
