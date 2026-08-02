# 001 — Cache persistente do manifesto · Spec técnica

**Mod:** TarkovRedLine.Server
**Criado:** 2026-08-02
**Spec funcional:** [001-cache-persistente-do-manifesto-01-spec.md](./001-cache-persistente-do-manifesto-01-spec.md)

> Fonte primária = o próprio código do servidor (`Server/TarkovRedLine.Server/`); a versão TS legada (`TarkovRedLine-ServerMod/`) é 🥈 de paridade. Não há Assembly EFT/Harmony/F12 (é infra HTTP do servidor). **Não há projeto de testes do servidor** — a verificação automatizável é `dotnet build`; o comportamento (CA-001.1..6) é validado in-game (gate humano).

## 1. Estratégia

Hoje `GenerateManifestAsync` ([ModUpdater.cs:430](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L430)) escaneia o `mods_repo`, hasheia cada arquivo, monta o `manifestObj` e publica em três estáticos — `_fileMapCache`/`_manifestCache`/`_manifestHash` ([:637-639](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L637)) — que só vivem em memória e nascem vazios a cada boot.

A mudança, **toda no `ModUpdaterController` + um disparo no boot**, sem tocar o AutoSync:

1. **Extrair `GenerateManifestCore`** (o corpo atual, sem o gate) e transformar `GenerateManifestAsync` num wrapper que segura o gate `_manifestGenerating` e chama o Core. Isso permite reusar o Core a partir do novo caminho de boot **sem** dupla-aquisição do gate.
2. **No fim do Core, capturar a impressão leve e persistir** em disco (`manifest-cache{StateSuffix}.json`): `formatVersion`, `hash`, `fingerprint`, o `manifest` inteiro, e o `fileMap` (relativizado ao `mods_repo`). Escrita atômica (temp + `File.Move` overwrite). A impressão é acumulada **no mesmo loop** que já enumera os arquivos (mesmos `FileInfo` → consistente com o que foi hasheado, CC-2), e cobre **todos** os arquivos do `mods_repo` (inclusive os 2 JSON de definição, que o loop pula só para o manifesto).
3. **`EnsureManifestReady()` (novo, público, com gate próprio):** se o cache em memória já existe, retorna; senão tenta **carregar do disco** — válido se `formatVersion` bate **e** a impressão recomputada do `mods_repo` bate com a salva → popula os três estáticos e serve o **hash gravado** (não recomputa, CC-9); se não bate/ausente/ilegível → chama o Core (regera + persiste).
4. **Disparo proativo no boot:** o `static TarkovRedLineModMetadata()` ([Plugin.cs:23](../../TarkovRedLine.Server/Plugin.cs#L23)) passa a disparar `Task.Run(ModUpdaterController.EnsureManifestReady)` — fire-and-forget, **não bloqueia** o startup. Assim, quando o primeiro player loga, o cache já está quente (do disco, na maioria dos boots).
5. **`/refresh` e o lazy dos endpoints** seguem chamando `GenerateManifestAsync` → o Core agora persiste, então a próxima subida carrega a versão nova sem regerar.

> **Dois fatos que ancoram o desenho:** (a) o `manifestObj` embute `generatedAt = DateTime.UtcNow` ([:613](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L613)) — é o **único** campo não-determinístico, então o hash muda a cada geração mesmo sem mudança de conteúdo; a persistência **congela** esse `generatedAt` e serve o mesmo hash entre boots (é isso que estabiliza o hash — intencional, ver R-7). (b) Hoje o `skipFileScan` do launcher está **desligado** ([ProfileViewModel.cs:481](../../../../launcher/Launcher4.0-v2/project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L481)) — o launcher SEMPRE faz scan e SEMPRE busca `/manifest` (retry 5×3s). Logo o ganho concreto deste item é **eliminar a espera do retry-loop** (o `/manifest` responde na hora por já estar quente), **não** fazer o launcher pular o scan — esse é um benefício **latente** que volta se o `skipFileScan` for reativado.

## 2. Pontos de extensão

| Ponto | Local | Papel |
|---|---|---|
| `GenerateManifestAsync` (gate) | [ModUpdater.cs:430-434,646-649](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L430) | Vira wrapper: segura o gate, chama `GenerateManifestCore`. |
| corpo scan+hash+swap | [ModUpdater.cs:436-645](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L436) | Extraído para `GenerateManifestCore` (sem gate); ganha captura de impressão + `PersistManifest` no fim. |
| loop de `allFiles` | [ModUpdater.cs:471-483](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L471) | Acumula a impressão (count/sizeSum/maxMtime/paths) **antes** do `continue` dos metadados. |
| publish dos estáticos | [ModUpdater.cs:637-639](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L637) | Depois do swap, chama `PersistManifest(manifestObj, hash, fingerprint, fileMap)`. |
| `Refresh` | [ModUpdater.cs:198-203](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L198) | Inalterado (já chama `GenerateManifestAsync`, que agora persiste). |
| `GetManifestHash`/`GetManifest` | [ModUpdater.cs:134-153](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L134) | Inalterados; o boot proativo reduz a janela de 503. |
| `static TarkovRedLineModMetadata()` | [Plugin.cs:23-27](../../TarkovRedLine.Server/Plugin.cs#L23) | Adiciona `Task.Run(ModUpdaterController.EnsureManifestReady)`. |
| `GetUpdaterBasePath` | [ModUpdater.cs:28-48](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L28) | Reusado para o caminho do arquivo persistido. |
| padrão de persistência (`StateSuffix`, load/save) | [PlayerIpsManager.cs:13,60-79](../../TarkovRedLine.Server/Controllers/PlayerIpsManager.cs#L13) | Modelo do path + gravação. |

## 3. Propriedades F12 · Harmony

`N/A` — servidor HTTP; sem Assembly EFT, Harmony ou ConfigEntry.

## 4. Arquivos

### MODIFICAR

| Arquivo | Mudança |
|---|---|
| `Controllers/ModUpdater.cs` | Extrair `GenerateManifestCore`; `EnsureManifestReady`; `PersistManifest`/`TryLoadPersisted`; `ComputeFingerprint` + acúmulo no loop; classe privada `PersistedManifest`/`Fingerprint`; const do path persistido. |
| `Plugin.cs` | `static ctor` dispara `Task.Run(ModUpdaterController.EnsureManifestReady)`. |

### CRIAR

Nenhum arquivo novo — modelos persistidos são classes privadas dentro do `ModUpdater.cs`. (Sem projeto de testes no servidor; verificação = build + gate in-game.)

## 5. Stubs

### 5.1 Modelo persistido + caminho

```csharp
// ModUpdater.cs — dentro do ModUpdaterController
private const int PersistedFormatVersion = 1;

private static string GetManifestCachePath() =>
    Path.Combine(GetUpdaterBasePath(), $"manifest-cache{ModRouting.StateSuffix}.json");

private sealed class Fingerprint
{
    public int count { get; set; }
    public long sizeSum { get; set; }
    public long maxMtimeUtcTicks { get; set; }
    public string pathsDigest { get; set; } = "";  // MD5 da lista ORDENADA de caminhos relativos
}

private sealed class PersistedManifest
{
    public int formatVersion { get; set; }
    public string hash { get; set; } = "";
    public Fingerprint fingerprint { get; set; } = new();
    public JsonElement manifest { get; set; }                 // o manifestObj serializado
    public Dictionary<string, string> fileMap { get; set; } = new(); // chaveLógica -> relativo ao mods_repo
}
```

### 5.2 Impressão leve (acumulada no loop existente)

```csharp
// No começo de GenerateManifestCore, antes do foreach:
int fpCount = 0; long fpSizeSum = 0; long fpMaxTicks = 0;
var fpPaths = new List<string>();

// DENTRO do foreach (var file in allFiles), no TOPO — ANTES do continue dos metadados,
// para a impressão cobrir TODO o mods_repo (editar as definições também deve invalidar):
var fi = new FileInfo(file);
fpCount++;
fpSizeSum += fi.Length;
long ticks = fi.LastWriteTimeUtc.Ticks;
if (ticks > fpMaxTicks) fpMaxTicks = ticks;
fpPaths.Add(relPath);   // relPath já é calculado logo acima

// Depois do foreach, monta a impressão (paths ordenados → digest barato, pega rename/add/remove — CC-8):
fpPaths.Sort(StringComparer.Ordinal);
string pathsDigest;
using (var md5p = MD5.Create())
    pathsDigest = BitConverter.ToString(md5p.ComputeHash(
        System.Text.Encoding.UTF8.GetBytes(string.Join("\n", fpPaths)))).Replace("-", "").ToLowerInvariant();
var fingerprint = new Fingerprint { count = fpCount, sizeSum = fpSizeSum, maxMtimeUtcTicks = fpMaxTicks, pathsDigest = pathsDigest };
```

> Nota: `FileInfo` faz só `stat` (não abre o arquivo). Reaproveitar o `var fi` do topo do loop na linha do `size` do manifesto — trocar `new FileInfo(file).Length` ([:484](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L484)) por `fi.Length` — uma `FileInfo` por arquivo.

### 5.3 Persistir (atômico) — no fim de `GenerateManifestCore`, após o swap

```csharp
// ...
_fileMapCache = fileMap;
_manifestCache = manifestObj;
_manifestHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

PersistManifest(manifestObj, _manifestHash, fingerprint, fileMap, modsPath); // best-effort (CC-7)

// ---

private static void PersistManifest(object manifestObj, string hash, Fingerprint fp,
    Dictionary<string, string> fileMap, string modsPath)
{
    try
    {
        // fileMap guardado RELATIVO ao mods_repo (robusto a mudança de pasta — CC do fileMap).
        var relMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in fileMap)
            relMap[kv.Key] = Path.GetRelativePath(modsPath, kv.Value).Replace("\\", "/");

        var payload = new PersistedManifest
        {
            formatVersion = PersistedFormatVersion,
            hash = hash,
            fingerprint = fp,
            manifest = JsonSerializer.SerializeToElement(manifestObj),
            fileMap = relMap,
        };

        string path = GetManifestCachePath();
        string tmp = path + ".tmp";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        System.IO.File.WriteAllText(tmp, JsonSerializer.Serialize(payload));
        System.IO.File.Move(tmp, path, overwrite: true); // troca atômica (CC-4)
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ModUpdater] persist manifest falhou (best-effort): {ex.Message}");
    }
}
```

### 5.4 Carregar no boot + gate

```csharp
// Não-async (o corpo é síncrono/CPU-bound) → elimina o CS1998; `_ = GenerateManifestAsync()`
// nos endpoints segue funcionando. GenerateManifestCore RETÉM o try/catch interno (:641-645,
// o log "Critical error…") — nunca propaga (PA-01-04).
private static Task GenerateManifestAsync()
{
    if (Interlocked.CompareExchange(ref _manifestGenerating, 1, 0) != 0) return Task.CompletedTask;
    try { GenerateManifestCore(); }
    finally { Interlocked.Exchange(ref _manifestGenerating, 0); }
    return Task.CompletedTask;
}

/// <summary>Item 001: chamado proativamente no boot (Plugin static ctor). Carrega do disco se
/// a impressão bate; senão regera. Gate próprio — não roda concorrente com uma geração em voo.
/// GenerateManifestCore e TryLoadPersisted têm try/catch internos, mas o CALLER (Plugin static ctor)
/// ainda envolve num try/catch por robustez (PA-01-04).</summary>
public static void EnsureManifestReady()
{
    if (_manifestCache != null) return;
    if (Interlocked.CompareExchange(ref _manifestGenerating, 1, 0) != 0) return; // já cuidando
    try
    {
        if (TryLoadPersisted()) return; // carregou do disco (log dentro)
        GenerateManifestCore();          // regera + persiste
    }
    finally { Interlocked.Exchange(ref _manifestGenerating, 0); }
}

private static bool TryLoadPersisted()
{
    try
    {
        string path = GetManifestCachePath();
        if (!System.IO.File.Exists(path)) return false;

        var p = JsonSerializer.Deserialize<PersistedManifest>(System.IO.File.ReadAllText(path));
        if (p == null || p.formatVersion != PersistedFormatVersion) return false; // CC-5

        var modsPath = GetModsRepoPath();
        var current = ComputeFingerprint(modsPath);
        if (!FingerprintEquals(current, p.fingerprint))
        {
            Console.WriteLine("[ModUpdater] impressão do mods_repo mudou — regerando manifesto.");
            return false;
        }

        // Reconstrói o fileMap absoluto a partir dos relativos.
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in p.fileMap)
            map[kv.Key] = Path.GetFullPath(Path.Combine(modsPath, kv.Value.Replace("/", Path.DirectorySeparatorChar.ToString())));

        _fileMapCache = map;
        _manifestCache = p.manifest;   // JsonElement — o Ok(...) serializa igual
        _manifestHash = p.hash;        // CC-9: serve o hash ORIGINAL, não recomputa
        Console.WriteLine("[ModUpdater] manifesto carregado do disco (sem regerar).");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ModUpdater] cache persistido ilegível ({ex.Message}) — regerando.");
        return false;
    }
}

private static Fingerprint ComputeFingerprint(string modsPath)
{
    if (!Directory.Exists(modsPath)) return new Fingerprint { pathsDigest = "" }; // CC-10
    int count = 0; long sizeSum = 0; long maxTicks = 0; var paths = new List<string>();
    foreach (var file in Directory.GetFiles(modsPath, "*.*", SearchOption.AllDirectories))
    {
        var fi = new FileInfo(file);
        count++; sizeSum += fi.Length;
        long t = fi.LastWriteTimeUtc.Ticks; if (t > maxTicks) maxTicks = t;
        paths.Add(Path.GetRelativePath(modsPath, file).Replace("\\", "/"));
    }
    paths.Sort(StringComparer.Ordinal);
    using var md5 = MD5.Create();
    string digest = BitConverter.ToString(md5.ComputeHash(
        System.Text.Encoding.UTF8.GetBytes(string.Join("\n", paths)))).Replace("-", "").ToLowerInvariant();
    return new Fingerprint { count = count, sizeSum = sizeSum, maxMtimeUtcTicks = maxTicks, pathsDigest = digest };
}

private static bool FingerprintEquals(Fingerprint a, Fingerprint b) =>
    a.count == b.count && a.sizeSum == b.sizeSum && a.maxMtimeUtcTicks == b.maxMtimeUtcTicks
    && string.Equals(a.pathsDigest, b.pathsDigest, StringComparison.Ordinal);
```

### 5.5 Disparo no boot (Plugin.cs)

```csharp
static TarkovRedLineModMetadata()
{
    Patches.FikaProfilePatch.Enable();
    // Item 001: prepara o manifesto em background no boot (do disco se a impressão bate; senão gera).
    // Fire-and-forget — não bloqueia o startup. O try/catch garante que QUALQUER falha (inclusive fora
    // do Core, ex.: GetManifestCachePath) seja LOGADA, nunca uma unobserved task exception (PA-01-04).
    System.Threading.Tasks.Task.Run(() =>
    {
        try { Controllers.ModUpdaterController.EnsureManifestReady(); }
        catch (Exception ex) { Console.WriteLine($"[ModUpdater] boot warmup falhou: {ex.Message}"); }
    });
}
```

## 6. Fluxo de dados

```
BOOT do mod → static TarkovRedLineModMetadata() → Task.Run(EnsureManifestReady)   [Plugin.cs:23]
   EnsureManifestReady (gate)
     ├─ _manifestCache != null → return
     ├─ TryLoadPersisted: lê manifest-cache.json; formatVersion + ComputeFingerprint(mods_repo) batem?
     │     SIM → popula _fileMapCache/_manifestCache/_manifestHash do disco (hash original) → PRONTO, zero espera
     │     NÃO → ↓
     └─ GenerateManifestCore: scan+hash → swap dos estáticos → PersistManifest (atômico)

PLAYER loga → GET manifest-hash / manifest [ModUpdater.cs:134-153]
   _manifestHash/_manifestCache já quentes → 200 na hora (sem 503, sem countdown de 30s)

PUBLICAÇÃO → /refresh OU boot com mods_repo alterado
   fingerprint diferente → GenerateManifestCore regera + PersistManifest → próximo boot carrega o novo
```

## 7. Riscos e dependências

- **R-1 (dupla-gate):** `EnsureManifestReady` e `GenerateManifestAsync` seguram o **mesmo** gate; por isso o corpo foi extraído para `GenerateManifestCore` (sem gate) — cada entrada adquire o gate **uma** vez. Um request lazy durante o boot que encontre o gate ocupado retorna 503 e o cliente re-tenta (comportamento atual, só que a janela é curta).
- **R-2 (o invariante NÃO é byte-a-byte — CC-9):** o launcher usa `/manifest-hash` como **token opaco** (compara com `manifest_hash.txt`, [ProfileViewModel.cs:465-489](../../../../launcher/Launcher4.0-v2/project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L465); não recomputa) e consome `/manifest` como **dados parseados** (`manifest["files"]`). Já hoje o hash (`Serialize` default, [:629](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L629)) não bate byte-a-byte com os bytes servidos (`Ok` usa opções web). O que precisa valer é **preservação semântica**: servir o `p.hash` congelado como token, e o `p.manifest` (JsonElement) preserva o array `files` com os mesmos `path`/`hash`/`size`. É o que o round-trip garante. **Nunca** recomputar o hash do manifesto recarregado.
- **R-3 (fileMap relativizado):** guardar o valor do `fileMap` relativo ao `mods_repo` cobre os paths lógicos que não derivam trivialmente da chave (ex.: `config-optional-ref/` → arquivo físico sob `config-optional/`, D-18): o valor persistido é o relativo do arquivo FÍSICO, reconstruído com `Path.Combine(modsPath, rel)`.
- **R-4 (impressão consistente — CC-2):** a impressão gravada é a computada no **mesmo** loop que hasheou (mesmos `FileInfo`); no `TryLoadPersisted` ela é **recomputada** do disco atual e comparada. Se o `mods_repo` mudou entre a gravação e o boot, a recomputação difere → regera (correto).
- **R-5 (arquivo persistido cresce):** o manifesto pode ter milhares de entradas; o JSON persistido é da ordem de MB. Aceitável (é lido uma vez no boot). Escrita atômica evita corrupção.
- **R-6 (StateSuffix redundante mas seguro):** `GetUpdaterBasePath` já separa homolog/prod pela pasta (`Launcher-Updater` vs `-Homolog`); o `StateSuffix` no nome é redundante mas mantém o padrão e blinda contra base compartilhada.
- **R-7 (`generatedAt` é o eixo — PA-01-01):** `generatedAt = DateTime.UtcNow` ([:613](../../TarkovRedLine.Server/Controllers/ModUpdater.cs#L613)) é o único campo não-determinístico → o hash vivo muda a cada geração. A persistência **congela** esse `generatedAt` e serve o mesmo hash entre boots — **intencional**, é o que estabiliza o hash (e elimina um re-download espúrio do manifesto a cada boot). Consequência: o `generatedAt` servido passa a ser o da geração original, não o do boot. Não "consertar" isso sem entender que quebra a estabilidade.
- **R-8 (stale sobrevive ao reinício — PA-01-03):** se a impressão tiver um false-negative (edição in-place preservando count+size+mtime+paths — estreito, edições normais mexem no mtime), o manifesto stale que antes sumia no próximo boot agora **sobrevive** aos reinícios. Escotilha: `/refresh` (regera + persiste). Documentado no CC-1 da spec funcional. Não relido conteúdo no boot de propósito (reintroduziria o custo que este item elimina).

## 8. Checklist de implementação

1. `ModUpdater.cs`: classes privadas `Fingerprint`/`PersistedManifest` + `PersistedFormatVersion` + `GetManifestCachePath`.
2. Extrair `GenerateManifestCore` (corpo atual sem gate); `GenerateManifestAsync` vira wrapper com gate.
3. No `Core`: acumular a impressão no loop de `allFiles` (antes do `continue`); montar `fingerprint` após o loop; `PersistManifest` após o swap.
4. `EnsureManifestReady` + `TryLoadPersisted` + `ComputeFingerprint` + `FingerprintEquals`.
5. `Plugin.cs`: `Task.Run(ModUpdaterController.EnsureManifestReady)` no static ctor.
6. `dotnet build TarkovRedLine.Server.csproj -c Release` verde (0 erros; warnings nullable pré-existentes toleráveis).
7. `/code-review`. Depois: **gate humano** — subir o `SPT.Server` real e confirmar: (a) 2º boot sem mudança loga "carregado do disco" e o `/manifest`/`/manifest-hash` respondem **na hora** no 1º login (sem os retries de 5s do launcher → sem "preparing the list") — CA-001.1; (b) tocar um arquivo do `mods_repo` e reiniciar loga "regerando" e muda o hash — CA-001.2. Como o `skipFileScan` do launcher está `false` (PA-01-05), a validação é sobre o `/manifest` responder **instantâneo**, não sobre o launcher pular o scan.

## 9. Conformidade com skills (auto-checklist)

| Item | Status | Evidência |
|---|---|---|
| Lifecycle de raid / GameWorld | N/A | Servidor HTTP; nada roda em raid (spec §estado-entre-raids). |
| Harmony patch shape | N/A | Sem Harmony no escopo (o mod já tem FikaProfilePatch, não tocado). |
| Memory leak / estado raid-scoped | N/A | Estáticos já existentes; a persistência não acumula em memória. |
| Coop/Fika paridade | ✅ | O hash servido é idêntico ao da geração (CC-9, R-2) → nenhum cliente diverge; a mudança é só de timing (spec §Fika). |
| Thread-safety | ✅ | Gate atômico único por entrada (`_manifestGenerating`, R-1); swaps de referência atômicos (mantidos do código atual). |
| Atomicidade / integridade de FS | ✅ | Persistência atômica temp+`File.Move` (§5.3, CC-4); leitura tolerante a arquivo ilegível/versão (§5.4, CC-5); best-effort com log (CC-7). |
| Determinismo do hash | ✅ | Serve o hash ORIGINAL do disco, não recomputa (CC-9, R-2). |
| Detecção de mudança | ✅ | Impressão inclui paths (rename/add/remove — CC-8) além de count/size/mtime (§5.2). |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-02 | Guilherme | Spec técnica via `/create-technical-spec`. Abordagem: extrair `GenerateManifestCore` + persistir (atômico) + `EnsureManifestReady` no boot (static ctor), impressão leve com digest de paths, `fileMap` relativizado, servir hash original. 8 pontos de extensão, 5 stubs. Sem projeto de testes no servidor → verificação = build + gate in-game. |
| 2026-08-02 | Guilherme | `/review-technical-spec` review 01 (sub-agent adversarial + verificação no launcher). 0 🔴 · 5 🟡 · 2 🟢, todos aplicados: reconhecido o `generatedAt` como eixo do hash instável (R-7); invariante real do hash = token opaco, não byte-a-byte (R-2 reescrito); stale sobrevive ao reinício → `/refresh` como escotilha (R-8); boot warmup envolto em try/catch (PA-01-04); `skipFileScan=false` recalibra a validação in-game (§8); wrapper não-async (CS1998); nota FileInfo corrigida. |
