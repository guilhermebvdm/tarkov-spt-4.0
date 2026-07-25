# 010 — Migração de configs + release · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [010-migracao-release-01-spec.md](010-migracao-release-01-spec.md)
**Criado:** 2026-07-25

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.
>
> **Nota de natureza do item:** diferente da maioria dos itens do overhaul Trauma 2.0, este item **não introduz nenhum patch Harmony novo** sobre o Assembly do EFT — é limpeza/consolidação do código já escrito do mod (`modded/`) + um script de shell novo. A fonte primária aqui é o código do mod (🥈 na hierarquia de evidência); o Assembly só é citado onde algo do jogo precisa ser confirmado (§1, uma única referência: `EBodyPart.cs`, usada para validar o conjunto fechado de membros indexado pelas novas tabelas i18n).

## 1. Estratégia

Não há "patch alvo" — o item é dividido em 4 blocos de trabalho puramente dentro de `modded/`:

- **Bloco A (config cleanup):** remoção de 3 `ConfigEntry<bool>` inertes + remoção de todas as sondas `[DEBUG-ICM]` (logs + campos dedicados). Risco principal identificado nesta spec (não estava na spec funcional): uma das 3 remoções **quebra a compilação** se feita "ingenuamente" — ver achado crítico abaixo.
- **Bloco B (docs + config):** `PROPRIEDADES.md` consolidado + `Medic Interact Distance` default `5f` → `3.5f`.
- **Bloco C (i18n EN/PT):** nova classe `MedicLocale` (mesmo padrão de `Trauma/TraumaLocale.cs`, fora do namespace `Trauma`) + migração dos ~25 pontos de texto inventariados na spec funcional + mudança de wire format do `DenyReason` (string → enum) no handshake de rede.
- **Bloco D (release):** script `mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh`, adaptado do precedente `tools/trl-items-management/scripts/package-release.sh` para um mod client-only.

**Achado crítico desta spec (corrige a spec funcional):** a spec funcional afirma que a remoção das 3 keys legadas é "SEM tocar `MigrateOrphanedConfigKeys()`". Isso está **incompleto**: o primeiro bloco de `MigrateOrphanedConfigKeys()` (`TRLImmersiveCombatMedicinePlugin.cs:339-375`, a migração histórica do mojibake "Sistema de Braços", CR-02) **escreve diretamente em `ConfigArmsEnabled.Value`** (`:371`). Removendo o campo sem remover esse bloco espec��fico, o projeto **não compila**. Confirmado por grep exaustivo (`grep -rn "ConfigArmsEnabled\|ConfigLegsEnabled\|ConfigStomachEnabled" modded/`): `ConfigLegsEnabled` e `ConfigStomachEnabled` não têm nenhum outro uso além de declaração+`Config.Bind` (remoção realmente simples para essas duas); só `ConfigArmsEnabled` tem o uso extra na migração do mojibake. Resolução adotada nesta spec (§5 Bloco A, stub 1): remover **também** esse bloco específico (linhas 339-375) — não por precisar de uma migração nova, mas porque (a) ele já cumpriu seu papel one-time em toda instalação real desde 2026-07-12 (`PROPRIEDADES.md` tabela "Renomeadas") e (b) seu alvo de escrita deixa de existir. Os outros 5 blocos de `MigrateOrphanedConfigKeys()` (placeholders `Legs/Fall/Arms/Stomach/Blackout Effects (item NNN)` + duração do desmaio) só chamam `orphans.Remove(...)` sem escrever em nenhum dos 3 campos removidos — confirmados intocados. **Recomenda-se que `/review-technical-spec` confirme esta leitura antes do `/code-mod`**, já que contradiz uma afirmação explícita da spec funcional revisada.

## 2. Pontos de patch

N/A — este item não adiciona nem modifica nenhum ponto de patch Harmony. O único arquivo de patch tocado é `Patches/Medical/MedicActionsPatch.cs`, e a mudança nele é puramente a remoção da sonda de log `[DEBUG-ICM]` do `Prefix` existente (§5 Bloco A) — a lógica do patch (interceptar `GetActionsClass.GetAvailableActions` para `MedicInteractable`) não muda.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma `ConfigEntry` **nova** é introduzida. Uma existente muda de valor/tooltip (Bloco B) e 3 são removidas (Bloco A):

| Seção | Nome (EN) | Ação | Antes | Depois |
|---|---|---|---|---|
| `4. Keybinds (Medic)` | `Medic Interact Distance` | ALTERAR default+tooltip | `5f`, "Valor alto para testes; reduzir no pacote final." | `3.5f`, sem a nota de teste |
| `2. Mecanicas (Trauma)` | `Sistema de Pernas` | REMOVER | `ConfigLegsEnabled`, `true` | — |
| `2. Mecanicas (Trauma)` | `Sistema de Braços` | REMOVER | `ConfigArmsEnabled`, `true` | — |
| `2. Mecanicas (Trauma)` | `Sistema de Estomago` | REMOVER | `ConfigStomachEnabled`, `true` | — |

Faixa (`AcceptableValueRange<float>(1f, 15f)`) de `Medic Interact Distance` **não muda** — `3.5f` já está dentro dela, sem necessidade de ajustar o range.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | Bloco A: remove 3 `ConfigEntry`+`Config.Bind`, remove o bloco mojibake de `MigrateOrphanedConfigKeys()` (:339-375), remove sondas `[DEBUG-ICM]` (campos `_debugHost`/`_debugCtrl`/`_debugNextBeat` + heartbeat do `Update()` + linhas do `Awake()`). Bloco B: `Medic Interact Distance` default+tooltip. |
| `modded/Patches/Medical/BandAidController.cs` | MODIFICAR | Bloco A: remove sondas `[DEBUG-ICM]` (`Awake`/`OnEnable`/`OnDisable`/`OnDestroy`/`Update`/`EnsureMedicInteractables`) preservando o `try/catch` funcional de `CheckInit()`. Bloco C: migra ~13 notificações + `DenyReason` para `MedicLocale`/enum. |
| `modded/Patches/Medical/MedicActionsPatch.cs` | MODIFICAR | Bloco A: remove sonda `[DEBUG-ICM]` (campo `_dbgNextLog` + log no `Prefix`), preserva a lógica de interceptação. |
| `modded/Patches/Medical/BandAidNetworkHandler.cs` | MODIFICAR | Bloco C: migra ~5 notificações + gera `DenyReasonId` (enum) no lugar da string `denyReason` nos dois pontos de resposta do handshake (`OnHealCheckReceived`, `TryAnswerForLocalBot`). |
| `modded/Patches/Medical/BandAidHealCheckPacket.cs` | MODIFICAR | Bloco C: `BandAidHealCheckResponsePacket.DenyReason` (`string`) → `DenyReasonId` (`MedicDenyReasonId`, serializado como `byte`) — **wire format muda**. |
| `modded/Patches/Medical/TourniquetManager.cs` | MODIFICAR | Bloco C: migra 6 notificações + `GetBodyPartName` vira wrapper de `MedicLocale.BodyPartLong`. |
| `modded/Patches/Medical/MedicInteractable.cs` | MODIFICAR | Bloco C: migra os 2 rótulos do ActionPanel (`Name = "..."` → `MedicLocale.Get(...)`). |
| `modded/Patches/Medical/BandAidUI.cs` | MODIFICAR | Bloco C: migra título, footer (fixo+dinâmico), rótulos de membro (`PartLabelPt`→`MedicLocale.BodyPartShort`) e `"INDISPONÍVEL"`; **requer mover a atribuição do título e dos rótulos de membro de `BuildUI()` (chamado 1x no `Awake`) para `ShowUI()`** (chamado toda vez que o exame abre) — ver achado de design abaixo. |
| `modded/Patches/Medical/MedicLocale.cs` | CRIAR | Bloco C: classe nova de i18n (enum `MedicTextId`, enum `MedicDenyReasonId`, tabelas EN/PT, `Get`, `BodyPartShort`/`BodyPartLong`, `PressModeVerb`). |
| `PROPRIEDADES.md` | MODIFICAR | Bloco B: remove 3 linhas da Seção 2, adiciona 3 linhas em "Removidas", atualiza `Medic Interact Distance`, atualiza a frase da Seção 5, adiciona entrada no Histórico. |
| `mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` | CRIAR | Bloco D: script de empacotamento client-only. |

**Achado de design (Bloco C, HUD):** `BandAidUI.BuildUI()` (chamado uma única vez, dentro de `Awake()` — antes de qualquer raid, na mesma janela de risco documentada em `TraumaLocale` como "race de boot") constrói o título (`:310`) e os 7 rótulos de membro (`CreateLimbBlock`, `:348-354`, texto armazenado em `LimbUI.NameText`) **uma vez só**. Se a tradução for aplicada apenas na string literal passada nesse momento, o texto fica **congelado no idioma detectado no boot** — nunca mais reavaliado, violando a regra "nunca cachear o idioma, ler no momento de exibir" (mesma regra do `TraumaLocale.IsGamePortuguese()`, reforçada no corner case "idioma trocado mid-raid" da spec funcional). `ShowUI(Player target)` (`:643`) já roda toda vez que o médico abre o examinador — é o ponto correto para (re)aplicar o idioma corrente. Fix: `ShowUI()` passa a também setar `_titleText.text` e, em loop sobre `_limbViews`, `NameText.text` de cada membro, além do que já faz para o footer. O footer (`:660`) já é recomputado dentro de `ShowUI()`, então sua migração é direta (sem esse problema). O texto inicial passado em `BuildUI()` (`:310`, `:348-354`, `:366-369`) fica inconsequente (nunca visível — `_canvasObj.SetActive(false)` roda logo em seguida e todo `ShowUI()` sobrescreve antes do canvas aparecer) — mantido como literal EN por clareza, sem necessidade de chamar `MedicLocale` ali.

## 5. Stubs de código

### Bloco A — Config cleanup + remoção de sondas [DEBUG-ICM]

**Stub 1 — `TRLImmersiveCombatMedicinePlugin.cs`: remoção dos 3 configs + achado crítico do mojibake.**

```csharp
// ANTES (linhas 26-28, declaração):
public static ConfigEntry<bool> ConfigMasterEnabled;
public static ConfigEntry<bool> ConfigLegsEnabled;      // REMOVER
public static ConfigEntry<bool> ConfigArmsEnabled;      // REMOVER
public static ConfigEntry<bool> ConfigStomachEnabled;   // REMOVER
public static ConfigEntry<bool> ConfigBlackoutEnabled;

// DEPOIS:
public static ConfigEntry<bool> ConfigMasterEnabled;
public static ConfigEntry<bool> ConfigBlackoutEnabled;
```

```csharp
// ANTES (linhas 95-101, Awake — Config.Bind):
ConfigBlackoutEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Desmaio", true, "Ativa o desmaio ao receber muito dano massivo.");
// ref: spec 003 §3 — legado de pernas aposentado (D10); key mantida p/ não órfanar o .cfg (remoção no item 010)
ConfigLegsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Pernas", true, "(INERTE desde a v1.3.0 ...)");
// ref: spec 005 §1.7 — legado de braços aposentado (D10); key mantida p/ não órfanar o .cfg (remoção no item 010)
ConfigArmsEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Braços", true, "(INERTE desde a v1.6.0 ...)");
// ref: spec 006 §1.9 — legado de estômago aposentado (D10); key mantida p/ não órfanar o .cfg (remoção no item 010)
ConfigStomachEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Estomago", true, "(INERTE desde a v1.7.0 ...)");

// DEPOIS (item 010 — as 3 keys legadas eram vestígio puro do sistema pré-Trauma-2.0; nenhum
// patch lê .Value delas fora da migração histórica do mojibake removida no stub abaixo):
ConfigBlackoutEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Desmaio", true, "Ativa o desmaio ao receber muito dano massivo.");
```

```csharp
// ACHADO CRÍTICO — MigrateOrphanedConfigKeys() (TRLImmersiveCombatMedicinePlugin.cs:339-375):
// este bloco ESCREVE em ConfigArmsEnabled.Value (:371) — remover o campo sem remover o bloco
// não compila. O bloco é a migração one-time do mojibake "Sistema de BraÃ§os" (CR-02,
// 2026-07-12) — já consumida em toda instalação real (PROPRIEDADES.md tabela Renomeadas).
// Remover o bloco INTEIRO (não só a linha 371):

// ANTES:
/// <summary>
/// ref: CR-02 — copia o valor da key antiga com bytes quebrados
/// ("Sistema de BraÃ§os") para a key corrigida, uma única vez.
/// OrphanedEntries é internal no BepInEx → reflection.
/// </summary>
private void MigrateOrphanedConfigKeys()
{
    try
    {
        var orphansProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        if (!(orphansProp?.GetValue(Config) is System.Collections.IDictionary orphans)) return;

        string oldKey = "Sistema de BraÃ§os"; // mojibake literal da key antiga
        object orphanDef = null;
        bool oldValue = false;
        foreach (System.Collections.DictionaryEntry entry in orphans)
        {
            var def = entry.Key;
            string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
            string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
            if (section == "2. Mecanicas (Trauma)" && key == oldKey &&
                bool.TryParse(entry.Value as string, out oldValue))
            {
                orphanDef = def;
                break;
            }
        }
        if (orphanDef != null)
        {
            ConfigArmsEnabled.Value = oldValue;   // <-- alvo removido no Bloco A
            orphans.Remove(orphanDef);
            Config.Save();
            ModLogger.LogWarning($"[Config] Valor órfão migrado (one-time): 'Sistema de Braços' = {oldValue}; key antiga removida do .cfg.");
        }

        // ref: item 008 — MIGRAÇÃO POR CÓPIA (bloco seguinte — MANTIDO intocado) ...
        object legacyDurationDef = null;
        // ... (resto do método continua igual)

// DEPOIS — método começa direto no bloco do item 008 (mantido), o bloco do mojibake some por
// inteiro (comentário do <summary> também atualizado — não descreve mais a migração inicial):
/// <summary>
/// Migrações one-time de keys órfãs (renomeações/remoções ao longo dos itens 003-008).
/// OrphanedEntries é internal no BepInEx → reflection. A migração do mojibake "Sistema de
/// Braços" (CR-02, 2026-07-12) foi removida no item 010 junto com o campo ConfigArmsEnabled —
/// já tinha cumprido seu papel one-time em toda instalação real.
/// </summary>
private void MigrateOrphanedConfigKeys()
{
    try
    {
        var orphansProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        if (!(orphansProp?.GetValue(Config) is System.Collections.IDictionary orphans)) return;

        // ref: item 008 — MIGRAÇÃO POR CÓPIA ...
        object legacyDurationDef = null;
        // ... (resto do método continua IDÊNTICO — 5 blocos restantes, todos só chamam
        // orphans.Remove(...) sem escrever em nenhum campo removido; confirmado por grep)
    }
    catch (Exception ex)
    {
        ModLogger.LogWarning($"MigrateOrphanedConfigKeys: {ex.Message}");
    }
}
```

```csharp
// Sondas [DEBUG-ICM] do Plugin.cs — campos (linhas 546-549) e uso no Awake (linhas 270-273) e
// heartbeat do Update() (linhas 552-561) removidos por inteiro (nenhum outro call site — grep
// confirmado):

// ANTES (Awake, ~linha 256-273):
// Componentes no GameObject do PRÓPRIO plugin (BepInEx manager): o boot do
// EFT destrói GameObjects órfãos criados durante o chainloader (provado por
// [DEBUG-ICM] OnDestroy logo após "Chainloader startup complete") — o manager
// do BepInEx sobrevive a sessão inteira. DontDestroyOnLoad NÃO protege de
// destruição explícita.
gameObject.AddComponent<BandAidUI>();
gameObject.AddComponent<BandAidController>();
// ... (demais AddComponent inalterados)
TraumaBotFall.RegisterLayer();

// [DEBUG-ICM] sondas de lifecycle — remover após diagnóstico do prompt F
_debugHost = gameObject;
_debugCtrl = gameObject.GetComponent<BandAidController>();
ModLogger.LogWarning($"[DEBUG-ICM] componentes no plugin GO | active={gameObject.activeInHierarchy} | ctrl!=null={_debugCtrl != null} | ctrl.enabled={(_debugCtrl != null ? _debugCtrl.enabled.ToString() : "n/a")}");

// DEPOIS:
// Componentes no GameObject do PRÓPRIO plugin (BepInEx manager): o boot do EFT destrói
// GameObjects órfãos criados durante o chainloader (achado da Sessão 2, diagnóstico do
// prompt F) — o manager do BepInEx sobrevive à sessão inteira. DontDestroyOnLoad NÃO
// protege de destruição explícita.
gameObject.AddComponent<BandAidUI>();
gameObject.AddComponent<BandAidController>();
// ... (demais AddComponent inalterados)
TraumaBotFall.RegisterLayer();
```

```csharp
// ANTES (campos + Update(), ~linhas 546-561):
// [DEBUG-ICM] heartbeat — remover após diagnóstico do prompt F
private static GameObject _debugHost;
private static BandAidController _debugCtrl;
private float _debugNextBeat = 0f;

private void Update()
{
    // PA-01-07 (review técnica 01): bloco funcional que PRECEDE a sonda no método real — registro
    // de pacotes Fika, caminho distinto de BandAidController.Update()→CheckInit() (guards
    // diferentes). Preservado sem alteração — só citado aqui para não ser apagado por acidente
    // ao aplicar o diff.
    if (IsFikaInstalled)
    {
        Band_Aid.BandAidNetworkHandler.EnsurePacketsRegistered();
    }

    // [DEBUG-ICM] roda ANTES de qualquer early-return: Plugin.Update comprovadamente vive em raid
    if (Time.time >= _debugNextBeat)
    {
        _debugNextBeat = Time.time + 10f;
        var gw = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
        string host = _debugHost == null ? "DESTRUÍDO" : (_debugHost.activeInHierarchy ? "ativo" : "INATIVO");
        string ctrl = _debugCtrl == null ? "DESTRUÍDO" : (_debugCtrl.enabled ? "enabled" : "DISABLED");
        ModLogger.LogWarning($"[DEBUG-ICM] beat | host={host} | ctrl={ctrl} | world={(gw != null)} | mainPlayer={(gw?.MainPlayer != null)}");
    }

    // Lógica unificada de Update aqui
    if (!ConfigMasterEnabled.Value || !ConfigBlackoutEnabled.Value)
    { /* ... inalterado ... */ }

// DEPOIS (campos removidos; bloco IsFikaInstalled preservado INALTERADO no topo — PA-01-07 —
// Update() segue direto para a lógica real):
private void Update()
{
    if (IsFikaInstalled)
    {
        Band_Aid.BandAidNetworkHandler.EnsurePacketsRegistered();
    }

    // Lógica unificada de Update aqui
    if (!ConfigMasterEnabled.Value || !ConfigBlackoutEnabled.Value)
    { /* ... inalterado ... */ }
```

**Stub 2 — `BandAidController.cs`: sondas de lifecycle (Awake/OnEnable/OnDisable/OnDestroy) e Update().**

```csharp
// ANTES (linhas 58-76):
private void Awake()
{
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[DEBUG-ICM] Controller.Awake INÍCIO");
    Instance = this;
    BandAidNetworkHandler.OnHealCheckResponse += OnHealCheckResponseHandler;
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[DEBUG-ICM] Controller.Awake FIM (handler registrado; prompt via ActionPanel nativo)");
}

// [DEBUG-ICM] sondas de lifecycle — remover após diagnóstico do prompt F
private void OnEnable()  { TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[DEBUG-ICM] Controller.OnEnable"); }
private void OnDisable() { TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[DEBUG-ICM] Controller.OnDisable"); }

private void OnDestroy()
{
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[DEBUG-ICM] Controller.OnDestroy");
    BandAidNetworkHandler.OnHealCheckResponse -= OnHealCheckResponseHandler;
}

// DEPOIS (OnEnable/OnDisable existiam SÓ para o log — removidos por inteiro; OnDestroy mantém
// o unsubscribe real, só perde o log):
private void Awake()
{
    Instance = this;
    BandAidNetworkHandler.OnHealCheckResponse += OnHealCheckResponseHandler;
}

private void OnDestroy()
{
    BandAidNetworkHandler.OnHealCheckResponse -= OnHealCheckResponseHandler;
}
```

```csharp
// ANTES (linhas 129-150 e 169-174 — dentro de Update()):
// [DEBUG-ICM] flags log-once — remover após diagnóstico do prompt F
private bool _dbgUpdateAlive = false;
private bool _dbgInRaid = false;

private void Update()
{
    // [DEBUG-ICM]
    if (!_dbgUpdateAlive)
    {
        _dbgUpdateAlive = true;
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[DEBUG-ICM] Controller.Update PRIMEIRO frame");
    }

    try { BandAidNetworkHandler.CheckInit(); }
    catch (Exception ex)
    {
        // [DEBUG-ICM] CheckInit era chamado sem guarda ANTES do scan — uma exceção aqui mataria o Update todo frame
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[DEBUG-ICM] CheckInit exception: {ex}");
    }

    if (Singleton<GameWorld>.Instance == null || Singleton<GameWorld>.Instance.MainPlayer == null)
    { /* ... */ }

    if (_lastGameWorld != Singleton<GameWorld>.Instance)
    { /* ... */ }

    // [DEBUG-ICM]
    if (!_dbgInRaid)
    {
        _dbgInRaid = true;
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[DEBUG-ICM] Controller.Update EM RAID (GameWorld+MainPlayer ok) — sweep de MedicInteractable ativo");
    }
    // ... resto inalterado

// DEPOIS (campos removidos; try/catch de CheckInit() PRESERVADO — só o texto do log muda,
// sem a tag e sem [DEBUG-ICM], continua um LogError de produção legítimo):
private void Update()
{
    // O registro de pacotes deve ocorrer independentemente de haver um jogador local.
    // Em servidores dedicados (Headless), o MainPlayer é null. Se pularmos, os pacotes nunca são registrados.
    try { BandAidNetworkHandler.CheckInit(); }
    catch (Exception ex)
    {
        // Guard: uma exceção aqui mataria o Update inteiro a cada frame.
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"CheckInit exception: {ex}");
    }

    if (Singleton<GameWorld>.Instance == null || Singleton<GameWorld>.Instance.MainPlayer == null)
    { /* ... inalterado ... */ }

    if (_lastGameWorld != Singleton<GameWorld>.Instance)
    { /* ... inalterado ... */ }

    // ... resto inalterado (sem os blocos de log-once)
```

```csharp
// ANTES (EnsureMedicInteractables, linhas 765-786) — achado do corner case CS0219: `attached` só
// existe para o log removido; NENHUM outro uso (não retornado, não lido fora do log) — decisão
// desta spec é remover o acumulador JUNTO com o log (não só o log), evitando qualquer risco de
// warning/dead-store:
private void EnsureMedicInteractables()
{
    if (Time.time < _nextInteractableSweep) return;
    _nextInteractableSweep = Time.time + 2f;

    var gameWorld = Singleton<GameWorld>.Instance;
    var mainPlayer = gameWorld.MainPlayer;
    var players = gameWorld.AllAlivePlayersList;
    int attached = 0;
    for (int i = 0; i < players.Count; i++)
    {
        Player p = players[i];
        if (p == null || p == mainPlayer) continue;
        if (p.HealthController == null || !p.HealthController.IsAlive) continue;
        if (MedicInteractable.Ensure(p)) attached++;
    }

    // [DEBUG-ICM] remover após diagnóstico
    if (attached > 0)
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
            $"[DEBUG-ICM] sweep: +{attached} MedicInteractable (vivos na lista: {players.Count})");
}

// DEPOIS (acumulador removido por inteiro — Ensure() já é idempotente e faz seu próprio
// trabalho; nada além do log consumia a contagem):
private void EnsureMedicInteractables()
{
    if (Time.time < _nextInteractableSweep) return;
    _nextInteractableSweep = Time.time + 2f;

    var gameWorld = Singleton<GameWorld>.Instance;
    var mainPlayer = gameWorld.MainPlayer;
    var players = gameWorld.AllAlivePlayersList;
    for (int i = 0; i < players.Count; i++)
    {
        Player p = players[i];
        if (p == null || p == mainPlayer) continue;
        if (p.HealthController == null || !p.HealthController.IsAlive) continue;
        MedicInteractable.Ensure(p);
    }
}
```

**Stub 2b — `TRLImmersiveCombatMedicinePlugin.cs`: handler morto/duplicado (PA-01-04).**

```csharp
// ANTES (:325 assinatura do subscribe, :333-336 o handler morto):
BandAidNetworkHandler.OnHealCheckResponse += OnHealCheckResponseHandler;
...
private void OnHealCheckResponseHandler(BandAidHealCheckResponsePacket response)
{
    // O tratamento disso ficará na classe dedicada ou adaptaremos o código de BandAidPlugin aqui.
}

// DEPOIS — removido por inteiro (subscribe + método): corpo sempre vazio, nunca preenchido; o
// tratamento real já existe em BandAidController.OnHealCheckResponseHandler (subscrito
// separadamente no Awake() do controller, migrado no Stub 5 acima). Achado PA-01-04: segundo
// assinante inerte do mesmo evento, código morto do mesmo domínio que este item já limpa.
```

**Stub 3 — `MedicActionsPatch.cs`: sonda de log rate-limited.**

```csharp
// ANTES:
[HarmonyPatch]
public static class MedicActionsPatch
{
    public static MethodBase TargetMethod() { /* ... inalterado ... */ }

    // [DEBUG-ICM] última chamada logada — remover após diagnóstico
    private static float _dbgNextLog = 0f;

    [HarmonyPrefix]
    public static bool Prefix(GamePlayerOwner owner, GInterface177 interactive, ref ActionsReturnClass __result)
    {
        // [DEBUG-ICM] o que o pipeline nativo está entregando ao painel (1 log/2s)
        if (interactive != null && UnityEngine.Time.time >= _dbgNextLog)
        {
            _dbgNextLog = UnityEngine.Time.time + 2f;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                $"[DEBUG-ICM] GetAvailableActions: interactive={interactive.GetType().Name}");
        }

        if (interactive is MedicInteractable medic)
        {
            __result = medic.GetActions(owner);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                $"[DEBUG-ICM] MedicInteractable interceptado → ações={__result?.Actions?.Count ?? 0}");
            return false;
        }
        return true;
    }
}

// DEPOIS (campo + os 2 logs removidos; lógica de interceptação idêntica):
[HarmonyPatch]
public static class MedicActionsPatch
{
    public static MethodBase TargetMethod() { /* ... inalterado ... */ }

    [HarmonyPrefix]
    public static bool Prefix(GamePlayerOwner owner, GInterface177 interactive, ref ActionsReturnClass __result)
    {
        if (interactive is MedicInteractable medic)
        {
            __result = medic.GetActions(owner);
            return false;
        }
        return true;
    }
}
```

### Bloco B — `Medic Interact Distance` (distância final)

```csharp
// ANTES (TRLImmersiveCombatMedicinePlugin.cs:120-125):
// Regra ÚNICA de distância: o prompt e o acionamento usam este valor (o
// controller dirige o ActionPanel nativo por scan próprio — os caps do
// vanilla, 1,3m/2,5m, não se aplicam). Reduzir ao empacotar para o server.
MedicInteractDistance = Config.Bind("4. Keybinds (Medic)", "Medic Interact Distance", 5f,
    new ConfigDescription("Distancia (m) do prompt E do acionamento do modo medico (mesma regra). Valor alto para testes; reduzir no pacote final.",
        new AcceptableValueRange<float>(1f, 15f)));

// DEPOIS (item 010 — decisão resolvida na spec funcional: 3,5 m, levemente acima do vanilla
// ~2,5 m para tolerar a barra de progresso da cura sem reentrar range a cada leve movimento):
// Regra ÚNICA de distância: o prompt e o acionamento usam este valor (o
// controller dirige o ActionPanel nativo por scan próprio — os caps do
// vanilla, 1,3m/2,5m, não se aplicam).
MedicInteractDistance = Config.Bind("4. Keybinds (Medic)", "Medic Interact Distance", 3.5f,
    new ConfigDescription("Distancia (m) do prompt E do acionamento do modo medico (mesma regra).",
        new AcceptableValueRange<float>(1f, 15f)));
```

**Verificação do corner case "distância duplicada em caminho paralelo":** grep (`grep -rn "MedicInteractDistance" modded/`) confirma que os 2 outros usos (`BandAidController.cs:219,846` e `BandAidUI.cs:797`) **sempre leem `.Value` ao vivo** — nenhum caminho cacheia ou hardcoda `5f`/`6f` separadamente. Mudar o default no `Config.Bind` é suficiente; nenhum outro arquivo precisa de edição para este sub-item.

`PROPRIEDADES.md` (Bloco B, sem stub C#): Seção 2 perde as 3 linhas (`Sistema de Pernas`/`Braços`/`Estomago`); tabela "Removidas" ganha 3 entradas (mesmo padrão da entrada Shoulder Tap — key, data, motivo "legado Trauma 2.0 aposentado (item NNN), sem leitura funcional fora da key mojibake já migrada"); Seção 4 atualiza a linha de `Medic Interact Distance` (padrão `3.5` / faixa `1–15` / tooltip sem "para testes"); Seção 5 (Trauma 2.0 Motor) atualiza a frase "migração dos textos antigos é o item 010" para refletir a entrega; Histórico de Alterações ganha a linha do item 010.

### Bloco C — i18n EN/PT

**Stub 4 — `MedicLocale.cs` (classe nova, completa).**

```csharp
// modded/Patches/Medical/MedicLocale.cs
using System.Collections.Generic;
using Band_Aid; // PA-01-01 (review técnica 01): ItemDatabase vive em Band_Aid — sem este using,
                 // GetDenyReasonText não compila (CS0103).
using EFT;

namespace TRLImmersiveCombatMedicine
{
    /// <summary>Motivo de recusa do handshake de cura (Band-Aid) — trafega como ID pela rede,
    /// NUNCA como texto (a tradução acontece no médico, que exibe — ver MedicLocale.GetDenyReasonText).</summary>
    internal enum MedicDenyReasonId : byte
    {
        None = 0,
        UnknownItem = 1,        // ItemDatabase não tinha stats para o TemplateId
        NoCompatibleWound = 2,  // MedicalLogic.CanUseItem() reprovou (usa ItemTemplateId do próprio pacote p/ nome do item)
    }

    /// <summary>Chaves de texto dos sistemas legados (Band-Aid/torniquete/ActionPanel/HUD médico) migrados
    /// no item 010 (decisão 22). Textos com placeholder usam string.Format sobre o template já traduzido.</summary>
    internal enum MedicTextId
    {
        Aborted = 0,
        NoPatientResponseTimeout = 1,
        CheckingItem = 2,               // {0} = nome do item
        NoCompatibleWoundLocal = 3,     // {0} = nome do item (paciente local: bot/self)
        ShoulderTapSent = 4,            // {0} = nickname do alvo
        ItemDropped = 5,
        ApplyingItem = 6,               // {0} = nome do item
        TreatmentCompleteWithPart = 7,  // {0} = rótulo curto do membro
        TreatmentComplete = 8,
        ItemLostDuringTreatment = 9,
        TreatmentCancelled = 10,
        MedicExamining = 11,            // {0} = nickname do paciente
        TreatedByAlly = 12,
        ShoulderTapReceived = 13,       // {0} = nickname do remetente
        ActionExamine = 14,
        ActionShoulderTap = 15,
        HudTitle = 16,
        HudFooterDynamic = 17,          // {0} = verbo do modo (Press/Hold/DoubleTap), {1} = tecla
        HudUnavailable = 18,
        TourniquetAlreadyApplied = 19,  // {0} = rótulo longo do membro
        TourniquetApplied = 20,         // {0} = rótulo longo do membro
        TourniquetNotFound = 21,        // {0} = rótulo longo do membro
        TourniquetRemoved = 22,         // {0} = rótulo longo do membro, {1} = duração (s)
        TourniquetNecrosisWarning = 23, // {0} = rótulo longo do membro
        TourniquetDestroyed = 24,       // {0} = rótulo longo do membro
        DenyUnknownItem = 25,
        DenyNoCompatibleWound = 26,     // {0} = nome do item
    }

    internal static class MedicLocale
    {
        // Indexados por MedicTextId. EN é o default/fallback; PT vazio → EN (mesmo contrato do TraumaLocale).
        private static readonly string[] EnTexts =
        {
            /* Aborted                  */ "Aborted!",
            /* NoPatientResponseTimeout */ "No response from patient (timeout).",
            /* CheckingItem             */ "Checking {0}...",
            /* NoCompatibleWoundLocal   */ "{0}: no compatible wound.",
            /* ShoulderTapSent          */ "Shoulder tap \u2192 {0}",
            /* ItemDropped              */ "Item dropped!",
            /* ApplyingItem             */ "Applying {0}...",
            /* TreatmentCompleteWithPart*/ "Treatment complete ({0}).",
            /* TreatmentComplete        */ "Treatment complete.",
            /* ItemLostDuringTreatment  */ "Item lost during treatment.",
            /* TreatmentCancelled       */ "Treatment cancelled.",
            /* MedicExamining           */ "MEDIC: {0}",
            /* TreatedByAlly            */ "You were treated by an ally.",
            /* ShoulderTapReceived      */ "✈ You received a shoulder tap from {0}", // PA-02-01: ícone ✈ preservado
            /* ActionExamine            */ "Examine (Medic)",
            /* ActionShoulderTap        */ "Shoulder tap",
            /* HudTitle                 */ "OPERATOR STATUS",
            /* HudFooterDynamic         */ "Use your hotkeys to heal\n[{0} {1}] Close Examiner",
            /* HudUnavailable           */ "UNAVAILABLE",
            /* TourniquetAlreadyApplied */ "Tourniquet already applied: {0}",
            /* TourniquetApplied        */ "Tourniquet applied: {0}. Remove after bleeding stops!",
            /* TourniquetNotFound       */ "No tourniquet on: {0}",
            /* TourniquetRemoved        */ "Tourniquet removed: {0} ({1}s). Item returned.",
            /* TourniquetNecrosisWarning*/ "⚠ Tourniquet on {0}: necrosis risk! Remove now!", // PA-01-06: ícone ⚠ preservado
            /* TourniquetDestroyed      */ "☠ {0} destroyed by tourniquet necrosis!",          // PA-01-06: ícone ☠ preservado
            /* DenyUnknownItem          */ "Unknown item.",
            /* DenyNoCompatibleWound    */ "{0}: no compatible wound.",
        };

        private static readonly string[] PtTexts =
        {
            /* Aborted                  */ "Abortado!",
            /* NoPatientResponseTimeout */ "Sem resposta do paciente (timeout).",
            /* CheckingItem             */ "Verificando {0}...",
            /* NoCompatibleWoundLocal   */ "{0}: Sem ferimento compatível.",
            /* ShoulderTapSent          */ "Toque no ombro \u2192 {0}",
            /* ItemDropped              */ "Item dropado!",
            /* ApplyingItem             */ "Aplicando {0}...",
            /* TreatmentCompleteWithPart*/ "Tratamento Completo ({0}).",
            /* TreatmentComplete        */ "Tratamento Completo.",
            /* ItemLostDuringTreatment  */ "Item perdido durante tratamento.",
            /* TreatmentCancelled       */ "Tratamento cancelado.",
            /* MedicExamining           */ "MÉDICO: {0}",
            /* TreatedByAlly            */ "Você foi tratado por um aliado.",
            /* ShoulderTapReceived      */ "✈ Você recebeu um toque no ombro de {0}", // PA-02-01
            /* ActionExamine            */ "Examinar (Médico)",
            /* ActionShoulderTap        */ "Tocar no ombro",
            /* HudTitle                 */ "SITUAÇÃO DO OPERADOR",
            /* HudFooterDynamic         */ "Utilize as suas teclas de atalhos para curar\n[{0} {1}] Fechar Examinador",
            /* HudUnavailable           */ "INDISPONÍVEL",
            /* TourniquetAlreadyApplied */ "Torniquete já aplicado: {0}",
            /* TourniquetApplied        */ "Torniquete aplicado: {0}. Remova após parar o sangramento!",
            /* TourniquetNotFound       */ "Nenhum torniquete em: {0}",
            /* TourniquetRemoved        */ "Torniquete removido: {0} ({1}s). Item devolvido.",
            /* TourniquetNecrosisWarning*/ "⚠ Torniquete em {0}: risco de necrose! Remova agora!", // PA-01-06
            /* TourniquetDestroyed      */ "☠ {0} destruído por necrose do torniquete!",           // PA-01-06
            /* DenyUnknownItem          */ "Item desconhecido.",
            /* DenyNoCompatibleWound    */ "{0}: Sem ferimento compatível.",
        };

        // === Rótulos de membro — DUAS granularidades já existiam no código pré-migração:
        // BandAidUI usava rótulos CURTOS ("CABEÇA"), TourniquetManager usava rótulos LONGOS
        // ("Cabeça"). Preservados como dois resolvers para não alterar a UX existente.
        private static readonly Dictionary<EBodyPart, string> ShortEn = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "HEAD" }, { EBodyPart.Chest, "CHEST" }, { EBodyPart.Stomach, "STOMACH" },
            { EBodyPart.LeftArm, "L. ARM" }, { EBodyPart.RightArm, "R. ARM" },
            { EBodyPart.LeftLeg, "L. LEG" }, { EBodyPart.RightLeg, "R. LEG" },
        };
        private static readonly Dictionary<EBodyPart, string> ShortPt = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "CABEÇA" }, { EBodyPart.Chest, "TÓRAX" }, { EBodyPart.Stomach, "ESTÔMAGO" },
            { EBodyPart.LeftArm, "BRAÇO ESQ." }, { EBodyPart.RightArm, "BRAÇO DIR." },
            { EBodyPart.LeftLeg, "PERNA ESQ." }, { EBodyPart.RightLeg, "PERNA DIR." },
        };
        // ref: Assembly-CSharp/EBodyPart.cs:1-11 — 8 valores (Head..Common). PA-02-05 (review
        // técnica 02, corrige comentário anterior factualmente errado): Common CHEGA via
        // BandAidUI.ShowTreatment (membro-alvo ainda não resolvido, ex.: _expectedTreatmentPart
        // default em BandAidController.cs:375, ou catch silencioso em MedicHealPatch.cs) —
        // resolvido pelo fallback "..." abaixo, preservando o comportamento atual de PartLabel.
        // TourniquetManager nunca passa Common (só opera sobre membros com torniquete ativo).
        private static readonly Dictionary<EBodyPart, string> LongEn = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "Head" }, { EBodyPart.Chest, "Chest" }, { EBodyPart.Stomach, "Stomach" },
            { EBodyPart.LeftArm, "Left Arm" }, { EBodyPart.RightArm, "Right Arm" },
            { EBodyPart.LeftLeg, "Left Leg" }, { EBodyPart.RightLeg, "Right Leg" },
        };
        private static readonly Dictionary<EBodyPart, string> LongPt = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "Cabeça" }, { EBodyPart.Chest, "Tórax" }, { EBodyPart.Stomach, "Estômago" },
            { EBodyPart.LeftArm, "Braço Esquerdo" }, { EBodyPart.RightArm, "Braço Direito" },
            { EBodyPart.LeftLeg, "Perna Esquerda" }, { EBodyPart.RightLeg, "Perna Direita" },
        };

        private static readonly string[] PressVerbEn = { "Press", "Hold", "Double-tap" };
        private static readonly string[] PressVerbPt = { "Pressione", "Segure", "Duplo" };

        /// <summary>Reusa TraumaLocale.IsGamePortuguese() (internal, mesma assembly) — SEM duplicar a
        /// leitura de LocaleManagerClass (regra explícita do item 010).</summary>
        private static bool IsPt() => TRLImmersiveCombatMedicine.Trauma.TraumaLocale.IsGamePortuguese();

        internal static string Get(MedicTextId id, params object[] args)
        {
            int i = (int)id;
            if (i < 0 || i >= EnTexts.Length) return string.Empty;
            string template = EnTexts[i];
            if (IsPt())
            {
                string pt = i < PtTexts.Length ? PtTexts[i] : null;
                if (!string.IsNullOrEmpty(pt)) template = pt;
            }
            return (args == null || args.Length == 0) ? template : string.Format(template, args);
        }

        internal static string BodyPartShort(EBodyPart part)
        {
            var dict = IsPt() ? ShortPt : ShortEn;
            return dict.TryGetValue(part, out var l) ? l : "...";
        }

        internal static string BodyPartLong(EBodyPart part)
        {
            var dict = IsPt() ? LongPt : LongEn;
            return dict.TryGetValue(part, out var l) ? l : part.ToString();
        }

        internal static string PressModeVerb(EBandAidPressMode mode)
        {
            int idx = mode == EBandAidPressMode.Hold ? 1 : (mode == EBandAidPressMode.DoubleTap ? 2 : 0);
            return IsPt() ? PressVerbPt[idx] : PressVerbEn[idx];
        }

        /// <summary>Resolve o texto de recusa do handshake NO PONTO DE EXIBIÇÃO (médico) — o pacote
        /// carrega só o ID + o ItemTemplateId (já existia no pacote, reusado para o nome do item).</summary>
        internal static string GetDenyReasonText(MedicDenyReasonId reasonId, string itemTemplateId)
        {
            switch (reasonId)
            {
                case MedicDenyReasonId.UnknownItem:
                    return Get(MedicTextId.DenyUnknownItem);
                case MedicDenyReasonId.NoCompatibleWound:
                    var stats = ItemDatabase.GetStats(itemTemplateId);
                    return Get(MedicTextId.DenyNoCompatibleWound, stats?.Name ?? "?");
                default:
                    return string.Empty;
            }
        }
    }
}
```

**Stub 5 — mudança de wire format do handshake (`DenyReason` → `DenyReasonId`), o corner case mais sensível.**

**PA-01-02 (review técnica 01):** `BandAidNetworkHandler.cs` está em `namespace Band_Aid` — adicionar `using TRLImmersiveCombatMedicine;` ao topo do arquivo (mesmo padrão que `BandAidController.cs` já usa para importar tipos de outro namespace) antes de referenciar `MedicDenyReasonId` nos dois pontos de escrita (`OnHealCheckReceived`, `TryAnswerForLocalBot`). Sem isso, CS0103 nos dois métodos.

```csharp
// modded/Patches/Medical/BandAidHealCheckPacket.cs — ANTES:
public struct BandAidHealCheckResponsePacket : INetSerializable
{
    public string DoctorProfileId;
    public string PatientProfileId;
    public string ItemTemplateId;
    public bool Approved;
    public string DenyReason; // Motivo da recusa (ex: "Sem sangramento")
    public byte ExpectedBodyPart;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(DoctorProfileId);
        writer.Put(PatientProfileId);
        writer.Put(ItemTemplateId);
        writer.Put(Approved);
        writer.Put(DenyReason ?? "");
        writer.Put(ExpectedBodyPart);
    }

    public void Deserialize(NetDataReader reader)
    {
        DoctorProfileId = reader.GetString();
        PatientProfileId = reader.GetString();
        ItemTemplateId = reader.GetString();
        Approved = reader.GetBool();
        DenyReason = reader.GetString();
        ExpectedBodyPart = reader.GetByte();
    }
}

// DEPOIS — WIRE FORMAT MUDA (string → byte no mesmo slot): a spec funcional já criou o corner
// case ciente disso ("se algum pacote hoje trafega string finalizada, isso é mudança de wire
// format e precisa ser marcado explicitamente"). Igual a mudanças de pacote anteriores do mod
// (CR-05, CR-02) — TODOS os peers Fika precisam rodar a MESMA build após este item.
public struct BandAidHealCheckResponsePacket : INetSerializable
{
    public string DoctorProfileId;
    public string PatientProfileId;
    public string ItemTemplateId;   // reusado por MedicLocale.GetDenyReasonText p/ resolver o
                                     // nome do item localmente no médico — nenhum campo NOVO
    public bool Approved;
    public TRLImmersiveCombatMedicine.MedicDenyReasonId DenyReasonId; // ref: item 010 — tradução
                                     // acontece no médico (exibidor), nunca serializada como texto
    public byte ExpectedBodyPart;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(DoctorProfileId);
        writer.Put(PatientProfileId);
        writer.Put(ItemTemplateId);
        writer.Put(Approved);
        writer.Put((byte)DenyReasonId);
        writer.Put(ExpectedBodyPart);
    }

    public void Deserialize(NetDataReader reader)
    {
        DoctorProfileId = reader.GetString();
        PatientProfileId = reader.GetString();
        ItemTemplateId = reader.GetString();
        Approved = reader.GetBool();
        DenyReasonId = (TRLImmersiveCombatMedicine.MedicDenyReasonId)reader.GetByte();
        ExpectedBodyPart = reader.GetByte();
    }
}
```

```csharp
// BandAidNetworkHandler.cs — OnHealCheckReceived (PA-01-05: método começa em :676, denyReason
// declarado em :711, usado :709-716 — linhas corrigidas da citação original "~679-687"; PA-02-02:
// faixa ampliada para :676-738 — a construção do `response` que efetivamente escreve o campo
// (real :730-738, DenyReason em :736) ficava fora da faixa original, assimétrico com a citação
// já corrigida de TryAnswerForLocalBot :857-894), lado que GERA a resposta.
// ANTES:
var stats = ItemDatabase.GetStats(packet.ItemTemplateId);
bool approved = false;
string denyReason = "Item desconhecido.";

if (stats != null)
{
    approved = MedicalLogic.CanUseItem(mainPlayer, stats);
    denyReason = approved ? "" : $"{stats.Name}: Sem ferimento compatível.";
}
// ... response.DenyReason = denyReason;

// DEPOIS (mesma lógica de decisão, só troca a saída — NADA de texto é montado aqui; o texto só
// existe quando o MÉDICO chama MedicLocale.GetDenyReasonText):
var stats = ItemDatabase.GetStats(packet.ItemTemplateId);
bool approved = false;
var denyReasonId = MedicDenyReasonId.UnknownItem;

if (stats != null)
{
    approved = MedicalLogic.CanUseItem(mainPlayer, stats);
    denyReasonId = approved ? MedicDenyReasonId.None : MedicDenyReasonId.NoCompatibleWound;
}
// ...
var response = new BandAidHealCheckResponsePacket
{
    DoctorProfileId = packet.DoctorProfileId,
    PatientProfileId = packet.PatientProfileId,
    ItemTemplateId = packet.ItemTemplateId,
    Approved = approved,
    DenyReasonId = denyReasonId,
    ExpectedBodyPart = (byte)expectedPart
};
// (mesma mudança se aplica em TryAnswerForLocalBot — PA-01-05: assinatura em :857, construção
// do response em :886-894, linhas corrigidas da citação original "~833" — resposta em nome de bot local)
```

```csharp
// PA-01-03 (review técnica 01): terceiro ponto de leitura de DenyReason, não mapeado pela
// primeira versão desta spec — BandAidNetworkHandler.OnHealCheckResponseReceived:930 (handler
// REAL registrado via currentManager.RegisterPacket<BandAidHealCheckResponsePacket>(...), :63 —
// distinto de BandAidController.OnHealCheckResponseHandler, que só roda depois via o evento
// OnHealCheckResponse disparado em :933). Sem este fix, CS1061 após o rename do campo.
// ANTES (:930):
Logger.LogInfo($"HealCheck Response recebido | Approved: {packet.Approved} | Reason: {packet.DenyReason}");
// DEPOIS:
Logger.LogInfo($"HealCheck Response recebido | Approved: {packet.Approved} | Reason: {packet.DenyReasonId}");
```

```csharp
// BandAidController.cs — OnHealCheckResponseHandler (~linhas 116-121), lado que EXIBE.
// ANTES:
else
{
    NotificationManagerClass.DisplayMessageNotification(
        response.DenyReason, ENotificationDurationType.Default, ENotificationIconType.Alert);
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Handshake negado: {response.DenyReason}");
}

// DEPOIS (tradução acontece AQUI, no médico — cada peer vê no PRÓPRIO idioma, independente do
// idioma de quem gerou a recusa; satisfaz o critério de aceite Fika/multiplayer da spec funcional):
else
{
    string denyText = MedicLocale.GetDenyReasonText(response.DenyReasonId, response.ItemTemplateId);
    NotificationManagerClass.DisplayMessageNotification(
        denyText, ENotificationDurationType.Default, ENotificationIconType.Alert);
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Handshake negado: {denyText}");
}
```

**Stub 6 — 3 exemplos representativos de migração local (fora do handshake).**

```csharp
// BandAidController.cs:227 — notificação simples, sem variável:
// ANTES:
NotificationManagerClass.DisplayMessageNotification("Abortado!", ENotificationDurationType.Default, ENotificationIconType.Alert);
// DEPOIS:
NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.Aborted), ENotificationDurationType.Default, ENotificationIconType.Alert);
```

```csharp
// BandAidController.cs:364-365 (ProcessHeal, paciente LOCAL — bot/self) — notificação com {item}:
// ANTES:
NotificationManagerClass.DisplayMessageNotification(
    $"{stats.Name}: Sem ferimento compatível.", ENotificationDurationType.Default, ENotificationIconType.Alert);
// DEPOIS:
NotificationManagerClass.DisplayMessageNotification(
    MedicLocale.Get(MedicTextId.NoCompatibleWoundLocal, stats.Name), ENotificationDurationType.Default, ENotificationIconType.Alert);
```

```csharp
// TourniquetManager.cs:210-223 — GetBodyPartName vira wrapper fino (preserva a assinatura usada
// pelos outros 6 call sites do arquivo, migração mínima):
// ANTES:
private string GetBodyPartName(EBodyPart part)
{
    switch (part)
    {
        case EBodyPart.Head: return "Cabeça";
        case EBodyPart.Chest: return "Tórax";
        case EBodyPart.Stomach: return "Estômago";
        case EBodyPart.LeftArm: return "Braço Esquerdo";
        case EBodyPart.RightArm: return "Braço Direito";
        case EBodyPart.LeftLeg: return "Perna Esquerda";
        case EBodyPart.RightLeg: return "Perna Direita";
        default: return part.ToString();
    }
}
// DEPOIS:
private string GetBodyPartName(EBodyPart part) => TRLImmersiveCombatMedicine.MedicLocale.BodyPartLong(part);

// E o call site (:90-92) passa a usar string.Format sobre o template traduzido:
// ANTES:
NotificationManagerClass.DisplayMessageNotification(
    $"Torniquete aplicado: {GetBodyPartName(bodyPart)}. Remova após parar o sangramento!",
    ENotificationDurationType.Long, ENotificationIconType.Quest);
// DEPOIS:
NotificationManagerClass.DisplayMessageNotification(
    TRLImmersiveCombatMedicine.MedicLocale.Get(TRLImmersiveCombatMedicine.MedicTextId.TourniquetApplied, GetBodyPartName(bodyPart)),
    ENotificationDurationType.Long, ENotificationIconType.Quest);
```

```csharp
// MedicInteractable.cs:42-51 — rótulos do ActionPanel nativo (lidos a cada refresh do painel,
// não é hot-path de frame — IsGamePortuguese() ao vivo é seguro aqui):
// ANTES:
actions.Actions.Add(new ActionsTypesClass { Action = Examine, Name = "Examinar (Médico)" });
actions.Actions.Add(new ActionsTypesClass { Action = ShoulderTap, Name = "Tocar no ombro" });
// DEPOIS:
actions.Actions.Add(new ActionsTypesClass { Action = Examine, Name = MedicLocale.Get(MedicTextId.ActionExamine) });
actions.Actions.Add(new ActionsTypesClass { Action = ShoulderTap, Name = MedicLocale.Get(MedicTextId.ActionShoulderTap) });
```

```csharp
// BandAidUI.cs — ShowUI(Player target) (:643-665): footer já era recomputado aqui (migração
// direta) + NOVO: título e rótulos de membro passam a ser reaplicados aqui também (achado de
// design da §4 — BuildUI() só roda 1x no Awake, ShowUI() é o ponto correto de "exibir").
// ANTES (trecho do footer dinâmico, :649-661):
if (_footerText != null)
{
    var shortcut = TRLImmersiveCombatMedicinePlugin.MedicInteractKey.Value;
    var mode = TRLImmersiveCombatMedicinePlugin.MedicInteractMode.Value;
    string verbo = mode == EBandAidPressMode.Hold ? "Segure" : (mode == EBandAidPressMode.DoubleTap ? "Duplo" : "Pressione");
    string keyLabel = shortcut.MainKey.ToString();
    foreach (var m in shortcut.Modifiers) keyLabel = m + "+" + keyLabel;
    _footerText.text = $"Utilize as suas teclas de atalhos para curar\n[{verbo} {keyLabel}] Fechar Examinador";
}

// DEPOIS:
public void ShowUI(Player target)
{
    _targetPlayer = target;
    CacheSprites();
    if (_canvasObj != null)
    {
        // Título e rótulos de membro: BuildUI() só roda 1x (Awake, antes de qualquer raid) —
        // reaplicar aqui garante leitura do idioma NO MOMENTO DE EXIBIR (nunca cacheado).
        if (_titleText != null) _titleText.text = TRLImmersiveCombatMedicine.MedicLocale.Get(TRLImmersiveCombatMedicine.MedicTextId.HudTitle);
        foreach (var kvp in _limbViews)
            if (kvp.Value.NameText != null)
                kvp.Value.NameText.text = TRLImmersiveCombatMedicine.MedicLocale.BodyPartShort(kvp.Key);

        if (_footerText != null)
        {
            var shortcut = TRLImmersiveCombatMedicinePlugin.MedicInteractKey.Value;
            var mode = TRLImmersiveCombatMedicinePlugin.MedicInteractMode.Value;
            string verbo = TRLImmersiveCombatMedicine.MedicLocale.PressModeVerb(mode);
            string keyLabel = shortcut.MainKey.ToString();
            foreach (var m in shortcut.Modifiers) keyLabel = m + "+" + keyLabel;
            _footerText.text = TRLImmersiveCombatMedicine.MedicLocale.Get(TRLImmersiveCombatMedicine.MedicTextId.HudFooterDynamic, verbo, keyLabel);
        }
        _canvasObj.SetActive(true);
        _lastUpdateTime = 0f;
    }
}
```

```csharp
// BandAidUI.cs — PartLabel(EBodyPart) vira wrapper fino (mantém a API pública usada por
// BandAidController.cs:657, sem propagar a mudança para outros call sites):
// ANTES:
private static readonly Dictionary<EBodyPart, string> PartLabelPt = new Dictionary<EBodyPart, string> { /* ... */ };
public static string PartLabel(EBodyPart part) => PartLabelPt.TryGetValue(part, out var l) ? l : "...";
// DEPOIS:
public static string PartLabel(EBodyPart part) => TRLImmersiveCombatMedicine.MedicLocale.BodyPartShort(part);
```

```csharp
// BandAidUI.cs:788 — "INDISPONÍVEL" (dentro de Update(), reavaliado a cada UPDATE_INTERVAL=0.25s
// enquanto o HUD está visível — já satisfaz "ler no momento de exibir" sem mudança estrutural):
// ANTES:
if (hc == null || profile == null)
{ _subtitleText.text = "INDISPONÍVEL"; return; }
// DEPOIS:
if (hc == null || profile == null)
{ _subtitleText.text = TRLImmersiveCombatMedicine.MedicLocale.Get(TRLImmersiveCombatMedicine.MedicTextId.HudUnavailable); return; }
```

**Tabela de rastreio — pontos restantes migrados mecanicamente (mesmo padrão dos exemplos acima; nenhum exige mudança estrutural além da troca literal→`MedicLocale.Get`):**

| Arquivo:linha | `MedicTextId` | Placeholder |
|---|---|---|
| `BandAidController.cs:244-245` | `NoPatientResponseTimeout` | — |
| `BandAidController.cs:356-357` | `CheckingItem` | `{0}`=nome do item |
| `BandAidController.cs:445-447` | `ShoulderTapSent` | `{0}`=nickname |
| `BandAidController.cs:539` | `ItemDropped` | — |
| `BandAidController.cs:576` | `ApplyingItem` | `{0}`=nome do item (`.Localized()` nativo, fora de escopo) |
| `BandAidController.cs:658` | `TreatmentCompleteWithPart` | `{0}`=`BandAidUI.PartLabel(...)` (já migrado via wrapper) |
| `BandAidController.cs:671` | `TreatmentComplete` | — |
| `BandAidController.cs:676` | `ItemLostDuringTreatment` | — |
| `BandAidController.cs:755` | `TreatmentCancelled` | — |
| `BandAidController.cs:906` | `MedicExamining` | `{0}`=nickname |
| `BandAidNetworkHandler.cs:399-400` | `TreatedByAlly` | — |
| `BandAidNetworkHandler.cs:614-615` | `ShoulderTapReceived` | `{0}`=nickname |
| `TourniquetManager.cs:67-69` | `TourniquetAlreadyApplied` | `{0}`=membro (long) |
| `TourniquetManager.cs:105-107` | `TourniquetNotFound` | `{0}`=membro (long) |
| `TourniquetManager.cs:116-118` | `TourniquetRemoved` | `{0}`=membro, `{1}`=duração |
| `TourniquetManager.cs:173-175` | `TourniquetNecrosisWarning` | `{0}`=membro (long) |
| `TourniquetManager.cs:181-183` | `TourniquetDestroyed` | `{0}`=membro (long) |

### Bloco D — Script de release

**Stub 7 — `mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` (completo).**

```bash
#!/usr/bin/env bash
# package-release.sh — gera o zip de release do TRL-ImmersiveCombatMedicine (mod CLIENT-ONLY).
#
# Produz dist/trl-icm-release-v<versão>.zip contendo (espelhando a estrutura real de pastas do
# SPT, relativa à raiz do jogo):
#   BepInEx/plugins/TRL-ImmersiveCombatMedicine/   (DLL+pdb do mod client, já instalado em D:\SPT)
#
# Diferente do precedente tools/trl-items-management/scripts/package-release.sh (mod HÍBRIDO,
# client+server+pipeline Node): este mod não tem componente server nem pipeline externo — o
# bundle tem UM único diretório. Reaproveita o compile-mod.sh (build+install) em vez de duplicar
# essa lógica; a versão é lida do <Version> do único csproj do mod (fonte única — sem
# BepInPlugin/csproj a sincronizar entre 2 projetos, diferente do precedente).
#
# Uso:   bash mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh [OUTDIR]
#   OUTDIR padrão: <repo>/dist
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
MOD_NAME="TRL-ImmersiveCombatMedicine"
ASSEMBLY="TRLImmersiveCombatMedicine"
MOD="$ROOT/mods/$MOD_NAME"
OUTDIR="${1:-$ROOT/dist}"

# /c/Repos/... -> c:/Repos/... (powershell aceita barra normal)
winpath() { echo "$1" | sed 's|^/\([a-z]\)/|\1:/|'; }

VER="$(grep -oE '<Version>[0-9][0-9.]*</Version>' "$MOD/modded/$MOD_NAME.csproj" | grep -oE '[0-9][0-9.]*' | head -1 || true)"
# PA-02-03 (review técnica 02): `|| true` evita que `set -e`+`pipefail` aborte esta linha
# SILENCIOSAMENTE antes do guard abaixo rodar, caso o formato do <Version> mude no futuro
# (o precedente tools/trl-items-management/scripts/package-release.sh:35 tem o mesmo bug —
# não herdado aqui por ser script novo).
[[ -n "$VER" ]] || { echo "ERRO: não achei <Version> no csproj do mod"; exit 1; }
echo "→ versão (csproj): $VER"

# avisa se há mudanças não commitadas no mod (o bundle reflete o working tree, não HEAD) —
# mesmo guard do precedente: evita releasar um build sem commit rastreável.
if ! git -C "$ROOT" diff --quiet -- "$MOD" || ! git -C "$ROOT" diff --cached --quiet -- "$MOD"; then
  echo "  AVISO: há mudanças não commitadas no mod — o bundle reflete o working tree atual, não HEAD."
fi

# 1) build + instala no SPT local — fonte de verdade do conteúdo do bundle (evita duplicar a
#    lógica de filtro de DLL própria que o compile-mod.sh já resolve).
echo "→ compilando e instalando localmente (compile-mod.sh)..."
bash "$ROOT/.agents/scripts/compile-mod.sh" "$MOD_NAME" >/dev/null

# resolve o mesmo SPT_PATH que o compile-mod.sh usou (env > .spt-path > default D:/SPT).
SPT_INSTALL="${SPT_PATH:-}"
if [[ -z "$SPT_INSTALL" && -f "$ROOT/.spt-path" ]]; then
  SPT_INSTALL="$(grep -m1 '^SPT_PATH=' "$ROOT/.spt-path" | cut -d= -f2- | tr -d '\r')"
fi
SPT_INSTALL="${SPT_INSTALL:-D:/SPT}"

CLIENT_SRC="$SPT_INSTALL/BepInEx/plugins/$MOD_NAME"
[[ -f "$CLIENT_SRC/$ASSEMBLY.dll" ]] || { echo "ERRO: build/instalação local falhou (ausente: $CLIENT_SRC/$ASSEMBLY.dll)"; exit 1; }

# 2) staging do bundle — espelha <GameRoot>\BepInEx\plugins\TRL-ImmersiveCombatMedicine\
STAGE="$OUTDIR/.stage-v$VER"
BUN="$STAGE/trl-icm-release-v$VER"
rm -rf "$STAGE"
mkdir -p "$BUN/BepInEx/plugins/$MOD_NAME"

# DLL+pdb apenas (sem config/user-data). Nota: BepInEx/plugins/<mod>/Silhueta/ (assets PNG
# opcionais carregados em runtime por ImageLoader.Init, ver Helpers/ImageLoader.cs) NÃO é
# rastreado no repo e fica FORA deste bundle por decisão explícita — é asset local do usuário,
# não artefato do mod; a spec funcional deste item só cobre "o DLL do client".
cp -f "$CLIENT_SRC/$ASSEMBLY.dll" "$BUN/BepInEx/plugins/$MOD_NAME/"
[[ -f "$CLIENT_SRC/$ASSEMBLY.pdb" ]] && cp -f "$CLIENT_SRC/$ASSEMBLY.pdb" "$BUN/BepInEx/plugins/$MOD_NAME/"

# 3) zip (Compress-Archive — sem depender de binário zip no Git Bash)
mkdir -p "$OUTDIR"
OUT="$OUTDIR/trl-icm-release-v$VER.zip"
rm -f "$OUT"
powershell.exe -NoProfile -Command "Compress-Archive -Path '$(winpath "$BUN")' -DestinationPath '$(winpath "$OUT")' -Force" >/dev/null
rm -rf "$STAGE"

SIZE="$(du -h "$OUT" | cut -f1)"
echo "✓ bundle: $OUT ($SIZE)"
echo "  conteúdo: trl-icm-release-v$VER/BepInEx/plugins/$MOD_NAME/{$ASSEMBLY.dll,$ASSEMBLY.pdb}"
echo "  instalação: extraia e mescle BepInEx/plugins/ na raiz do jogo (client-only — sem componente server)."
```

## 6. Fluxo de dados

**Bloco C — handshake de cura remota (o único fluxo com mudança estrutural deste item):**

```
[A] Médico (BandAidController.ProcessHeal, patient REMOTO)
     → BandAidNetworkHandler.SendHealCheck(doctor, patient, itemTemplateId)   [inalterado]
     → rede: BandAidHealCheckPacket { ..., ItemTemplateId }                  [inalterado]
[B] Paciente (BandAidNetworkHandler.OnHealCheckReceived / TryAnswerForLocalBot)
     → MedicalLogic.CanUseItem(...)  → approved / MedicDenyReasonId          [MUDA: string→enum]
     → rede: BandAidHealCheckResponsePacket { Approved, DenyReasonId, ItemTemplateId, ExpectedBodyPart }
       (wire format muda: DenyReason string → DenyReasonId byte — ref: item 010, todos os peers
       precisam da MESMA build, mesmo padrão de aviso do CR-05/CR-02)
[C] Médico (BandAidController.OnHealCheckResponseHandler)
     → MedicLocale.GetDenyReasonText(response.DenyReasonId, response.ItemTemplateId)  [NOVO]
        → ItemDatabase.GetStats(itemTemplateId) [resolve nome do item LOCALMENTE — sem campo novo]
        → MedicLocale.Get(DenyNoCompatibleWound | DenyUnknownItem, ...) → EN ou PT conforme
          TraumaLocale.IsGamePortuguese() do PRÓPRIO cliente do médico
     → NotificationManagerClass.DisplayMessageNotification(denyText, ...)   [texto só existe aqui]
```

Cada peer resolve o texto no seu próprio idioma no passo [C] — nenhuma string trafega pela rede, satisfazendo o critério de aceite Fika/multiplayer da spec funcional (torniquete/cura aplicado por peer PT aparece em EN para peer com jogo em inglês, e vice-versa).

**Blocos A/B/D:** sem fluxo de dados — mudanças estáticas de código/config/script, sem runtime novo.

## 7. Riscos e dependências

- **Risco principal (novo, achado nesta spec): compatibilidade de build entre peers Fika.** A mudança de `DenyReason` (string) para `DenyReasonId` (byte) no `BandAidHealCheckResponsePacket` é uma mudança de wire format — **todas as máquinas de uma sessão coop precisam rodar a build pós-item-010** (mesmo padrão já documentado nas mudanças de pacote do CR-02/CR-05 deste mod). Registrar isso na entrega (asbuild + PROPRIEDADES/changelog) para não pegar o usuário de surpresa numa sessão mista.
- **Achado crítico do Bloco A** (já detalhado em §1): remover `ConfigArmsEnabled` sem remover o bloco de migração do mojibake (`TRLImmersiveCombatMedicinePlugin.cs:339-375`) não compila. Checklist §8 cobre a ordem correta.
- **Patches existentes tocados:** `MedicActionsPatch.cs` (só remoção de log, lógica intacta). Nenhum patch novo é registrado; nenhum patch existente muda de alvo.
- **Compatibilidade com outros mods:** nenhuma — este item não toca nenhum ponto de integração externo (Fika/BigBrain/CustomClasses inalterados).
- **Comentário stale (não-bloqueador):** `Patches/Trauma/HealthPatches.cs:139` menciona `ConfigArmsEnabled` num comentário histórico (documentação de uma remoção PASSADA, não código executável) — fica levemente desatualizado após este item, mas não quebra nada. Cleanup opcional no checklist.
- **Ordem de inicialização:** inalterada — `MedicLocale` é `static` sem estado (sem `Awake`/inicialização própria), pode ser referenciada de qualquer ponto do lifecycle sem risco de ordem.
- **`dist/` e `mod-backlog.md`:** pasta `dist/` ainda não existe no repo (confirmado) — o script a cria. Status do item 010 no `mod-backlog.md` está 🟡 (em progresso); `/code-mod` deve promover para 🟢 ao concluir os 4 blocos.

## 8. Checklist de implementação

- [x] **Bloco A.1** — Remover `ConfigLegsEnabled`/`ConfigArmsEnabled`/`ConfigStomachEnabled` (declaração `:26-28` + `Config.Bind` `:97,99,101`).
- [x] **Bloco A.2 (ordem importa — fazer JUNTO com A.1, não depois)** — Remover o bloco do mojibake "Sistema de Braços" em `MigrateOrphanedConfigKeys()` (`:339-375`, string `oldKey`/`orphanDef`/`oldValue` até o `if (orphanDef != null) { ... }` que escreve `ConfigArmsEnabled.Value`). Confirmar que o método ainda compila e que os 5 blocos restantes (placeholders 003-007 + duração do desmaio) permanecem intocados.
- [x] **Bloco A.3** — Remover sondas `[DEBUG-ICM]` de `TRLImmersiveCombatMedicinePlugin.cs`: campos `_debugHost`/`_debugCtrl`/`_debugNextBeat`, uso no `Awake()` (`:270-273`), heartbeat do `Update()` (`:552-561`).
- [x] **Bloco A.4** — Remover sondas `[DEBUG-ICM]` de `BandAidController.cs`: `OnEnable`/`OnDisable` (remover os 2 métodos por inteiro), logs de `Awake`/`OnDestroy` (manter a lógica, só o log some), campos `_dbgUpdateAlive`/`_dbgInRaid` + os 2 blocos log-once do `Update()`, log do `try/catch` de `CheckInit()` (manter o `try/catch`, trocar só o texto), acumulador `attached` + log em `EnsureMedicInteractables` (remover o acumulador INTEIRO, não só o log).
- [x] **Bloco A.5** — Remover sonda `[DEBUG-ICM]` de `MedicActionsPatch.cs` (campo `_dbgNextLog` + os 2 logs do `Prefix`).
- [x] **Bloco A.6 (opcional, cosmético)** — Atualizar o comentário de `HealthPatches.cs:139` que cita `ConfigArmsEnabled` (key removida) para não referenciar um campo inexistente.
- [x] **Bloco A.7 (PA-01-04)** — Remover o handler morto `OnHealCheckResponseHandler` de `TRLImmersiveCombatMedicinePlugin.cs` (assinatura `:325`, corpo `:333-336`) — segundo assinante inerte do mesmo evento, corpo sempre vazio.
- [x] **Bloco B.1** — `Medic Interact Distance`: default `5f`→`3.5f`, tooltip sem "para testes"/"reduzir no pacote final".
- [x] **Bloco B.2** — `PROPRIEDADES.md`: remover 3 linhas da Seção 2, adicionar 3 linhas em "Removidas", atualizar `Medic Interact Distance` (Seção 4), atualizar frase da Seção 5, adicionar linha no Histórico de Alterações.
- [x] **Bloco C.1 (ordem importa — PA-02-04: C.2/C.2b/C.3 dependem deste passo já existir, referenciam `MedicDenyReasonId`/`MedicLocale.GetDenyReasonText`)** — Criar `modded/Patches/Medical/MedicLocale.cs` (stub 4 completo, com `using Band_Aid;` — PA-01-01).
- [x] **Bloco C.2** — Adicionar `using TRLImmersiveCombatMedicine;` ao topo de `BandAidNetworkHandler.cs` (PA-01-02). Migrar `BandAidHealCheckResponsePacket` (`DenyReason`→`DenyReasonId`) e os 2 pontos que a preenchem (`OnHealCheckReceived` :676-738 — PA-02-02, `TryAnswerForLocalBot` :857-894 — linhas corrigidas, PA-01-05).
- [x] **Bloco C.2b (PA-01-03)** — Atualizar o log de diagnóstico em `OnHealCheckResponseReceived:930` (`packet.DenyReason` → `packet.DenyReasonId`) — terceiro ponto de leitura do campo, não coberto pelos 2 pontos de escrita nem pelo ponto de exibição.
- [x] **Bloco C.3** — Migrar `BandAidController.OnHealCheckResponseHandler` para `MedicLocale.GetDenyReasonText`.
- [x] **Bloco C.4** — Migrar as demais ~12 notificações de `BandAidController.cs` (tabela de rastreio §5).
- [x] **Bloco C.5** — Migrar as 2 notificações de `BandAidNetworkHandler.cs` fora do handshake (`TreatedByAlly`, `ShoulderTapReceived`).
- [x] **Bloco C.6** — Migrar `TourniquetManager.cs` (`GetBodyPartName` → wrapper + 6 notificações).
- [x] **Bloco C.7** — Migrar `MedicInteractable.cs` (2 rótulos do ActionPanel).
- [x] **Bloco C.8** — Migrar `BandAidUI.cs`: mover atribuição de título + rótulos de membro de `BuildUI()` para `ShowUI()`, migrar footer dinâmico (incl. `PressModeVerb`), migrar `PartLabel` (wrapper), migrar `"INDISPONÍVEL"`.
- [x] **Bloco D.1 (PA-02-04: executar — não só criar — depois de A-C completos; o script builda o estado ATUAL do mod via `compile-mod.sh`, um zip gerado antes disso reflete um build parcial)** — Criar `mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` (stub 7 completo). Invocado sempre via `bash script.sh` (PA-02-06: sem necessidade de `chmod +x`).
- [x] **Verificação final** — `bash .agents/scripts/compile-mod.sh TRL-ImmersiveCombatMedicine` compila 0 erros/0 warnings novos (checar especialmente CS0219/CS0414 nos pontos tocados pelo Bloco A); `grep -c "\[DEBUG-ICM\]" modded/**/*.cs` retorna 0; `bash mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` gera `dist/trl-icm-release-v<versão>.zip`.
- [x] Atualizar `mod-backlog.md` (item 010: 🟡 → 🟢).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Item não introduz nenhum patch/estado raid-scoped novo; Bloco A só REMOVE campos estáticos que não seguravam referência a `Player`/`Profile` (só `GameObject`/`BandAidController`/`float`), reduzindo superfície de leak, não aumentando. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | A mudança de wire format (`DenyReason`→`DenyReasonId`, §5 stub 5) preserva 100% da lógica de relay Host/Client/bot-local existente em `OnHealCheckReceived`/`TryAnswerForLocalBot` (`BandAidNetworkHandler.cs`) — só o CONTEÚDO do campo de recusa muda, nenhuma ramificação de owner/relay é tocada (confirmado por diff mental linha a linha nos stubs §5). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Nenhum alvo virtual/ofuscado novo é patcheado; `MedicActionsPatch.cs` só perde uma sonda de log, `TargetMethod()` não muda. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | Item não muta estado de jogo (HP, movimento, animação); é limpeza de config/log + i18n + script de empacotamento. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `MedicLocale` é `static` sem nenhum campo mutável (só tabelas `readonly`) — nada para resetar entre raids. `MedicDenyReasonId` é um valor transitório do pacote, não persiste. Bloco A remove estado estático (`_debugHost`/`_debugCtrl`/`_dbgUpdateAlive`/etc.) sem substituí-lo por nada novo — superfície de estado entre raids estritamente menor após o item. |
| 6 | Semântica/defaults/faixas de cada `ConfigEntry` sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | `Medic Interact Distance`: novo default `3.5f` permanece dentro da faixa existente `1f–15f` (§5 Bloco B) — sem ambiguidade nem necessidade de ajustar range. As 3 `ConfigEntry` removidas não tinham nenhum consumidor de `.Value` restante (confirmado por grep, §1) — remoção não deixa nenhum caminho de código órfão dependendo de um estado "neutro" que deixou de existir. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhum método patcheado é re-invocado por este item. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | Achado de design documentado em §4/§5 stub 6: título e rótulos de membro do HUD (`BandAidUI`) eram fixados 1x em `BuildUI()` (`Awake`, antes de qualquer raid) — corrigido para serem revalidados contra o idioma CORRENTE em `ShowUI()` (toda vez que o examinador abre), evitando texto congelado no idioma detectado no boot após uma troca de idioma mid-raid (mesmo padrão de risco de AP-08: cache não revalidado após troca de contexto). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-25 | Spec técnica criada via `/create-technical-spec`. Achado crítico: bloco de migração do mojibake "Sistema de Braços" em `MigrateOrphanedConfigKeys()` escreve em `ConfigArmsEnabled.Value` — removê-lo junto da key é obrigatório, não opcional (corrige afirmação da spec funcional). Achado de design: título/rótulos de membro do HUD precisam ser revalidados em `ShowUI()`, não fixados em `BuildUI()` (Awake), para respeitar "idioma trocado mid-raid". Wire format do handshake de cura muda (`DenyReason` string → `DenyReasonId` enum/byte) — reusa `ItemTemplateId` já existente no pacote em vez de adicionar campo novo. |
| 2026-07-25 | Review técnica 01 aplicada — 7 achados (3🔴+3🟡+1🟢): `using Band_Aid;`/`using TRLImmersiveCombatMedicine;` faltantes (2 bloqueadores de compilação), terceiro ponto de leitura do `DenyReason` não mapeado (`OnHealCheckResponseReceived:930`), handler morto/duplicado removido (Bloco A.7), citações de linha corrigidas em `BandAidNetworkHandler.cs`, ícones ⚠/☠ preservados nos templates de necrose, bloco `IsFikaInstalled` explicitado no stub do `Update()`. Ver [010-migracao-release-03-spec-tech-review-01.md](010-migracao-release-03-spec-tech-review-01.md). |
| 2026-07-25 | Review técnica 02 aplicada — 6 achados (0🔴+1🟡+5🟢), 0 bloqueadores: as 7 correções da rodada 1 confirmadas sem regressão; ícone ✈ do `ShoulderTapReceived` preservado (3º caso da mesma classe de regressão de PA-01-06); citação de `OnHealCheckReceived` ampliada para `:676-738` (simetria com `TryAnswerForLocalBot`); robustez de shell (`|| true`) no `VER=` do script de release; dependências de ordem explicitadas no checklist (C.1→C.2/C.2b/C.3, D.1 após A-C); comentário sobre `EBodyPart.Common` corrigido; instrução supérflua de `chmod` removida. Ver [010-migracao-release-03-spec-tech-review-02.md](010-migracao-release-03-spec-tech-review-02.md). Spec técnica pronta para `/code-mod`. |
| 2026-07-25 | `/code-mod` executado — checklist §8 (21 itens) 100% `[x]`. **Desvio da spec encontrado durante a implementação (não pego pelas 2 rodadas de review técnica):** o stub 5 declara `public MedicDenyReasonId DenyReasonId;` em `BandAidHealCheckResponsePacket` (struct `public`), mas `MedicDenyReasonId` é `internal` (stub 4) — campo público de tipo interno em struct público é **CS0052** ("Inconsistent accessibility"), confirmado por build isolado (`dotnet build` num repro mínimo). Fix aplicado: o campo virou `internal` (não `public`) — preserva o mesmo acesso de fato (Band_Aid/TRLImmersiveCombatMedicine são a mesma assembly) sem expor o enum fora do mod e sem precisar tornar `MedicDenyReasonId` público. Documentado inline em `BandAidHealCheckPacket.cs`. Build final: 0 erros, 10 warnings `Harmony003` pré-existentes (mesmo baseline de antes do item). Ver [010-migracao-release-05-asbuild.md](010-migracao-release-05-asbuild.md). |
