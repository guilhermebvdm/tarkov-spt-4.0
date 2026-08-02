# 001 — Cache persistente do manifesto · Code Review 01

**Mod:** TarkovRedLine.Server
**Data:** 2026-08-02
**Escopo:** `Controllers/ModUpdater.cs` (refactor `GenerateManifestAsync`→wrapper + `GenerateManifestCore`, e os novos `EnsureManifestReady`/`TryLoadPersisted`/`ComputeFingerprint`/`FingerprintEquals`/`PersistManifest`/`Md5Hex`/`GetManifestCachePath` + classes `Fingerprint`/`PersistedManifest`), `Plugin.cs` (disparo no boot).

> Review adversarial por sub-agent independente de contexto limpo, sobre o código implementado (não a spec), cruzando com o comportamento do HEAD. Build: **0 erros, 0 warnings novos, 0 CS1998**. Aplicado em modo `/g-autodev`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Fortes: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Cat · Impacto | Título | Status |
|---|---|---|---|
| CR-01-01 | B · 🟢 | Cache adulterado com fingerprint válido mas `manifest` ausente serviria Undefined | ✅ Resolvido |

O revisor fechou as 8 categorias auditadas com prova concreta — registradas aqui para rastreabilidade:

- **Impressão consistente geração×boot** ✅ — a captura no loop de `GenerateManifestCore` e o `ComputeFingerprint` são idênticos campo a campo (mesma enumeração, relPath, sort, digest, count/sizeSum/maxTicks); a captura está **antes** do `continue` dos metadados → cobre todo o `mods_repo`, igual ao `ComputeFingerprint`. Não há falso-match.
- **Cache fora do scan** ✅ — `manifest-cache.json` (+ `.tmp`) vive em `GetUpdaterBasePath()`, irmão de `mods_repo`; `Directory.GetFiles(mods_repo, …)` nunca o vê → sem loop de regeração.
- **fileMap round-trip (incl. D-18)** ✅ — persistido relativo ao `mods_repo`, reconstruído com `Path.Combine`; as entradas `config-optional-ref` resolvem para o arquivo físico correto; nenhum valor vira `..` (todos vêm do scan sob `modsPath`).
- **Gate balanceado** ✅ — wrapper e `EnsureManifestReady` adquirem o gate uma vez e liberam no `finally`; `GenerateManifestCore` sem gate; sem dupla-aquisição, sem deadlock; `if (_manifestCache != null)` fora do gate é benigno.
- **JsonElement sobrevive ao Deserialize** ✅ — vem de um `JsonDocument` standalone não-disposto; `Ok(_manifestCache)` serializa igual ao caminho vivo (camelCase verbatim, `files[]` preservado).
- **Hash/generatedAt congelados** ✅ — serve `p.hash` sem recomputar; `generatedAt` verbatim (token opaco no launcher).
- **Sem regressão** ✅ — `/refresh`, endpoints lazy e ordem map→manifesto→hash preservados; `_ = GenerateManifestAsync()` ainda dispara o trabalho (o método original já era `async` sem `await`, síncrono).

---

## Pontos

### CR-01-01 · B — Bug latente (robustez) · 🟢 Menor ✅ Resolvido

**Cache adulterado à mão com `manifest` ausente/null mas fingerprint que ainda bate serviria um `JsonElement` Undefined**

**Problema:** se alguém corromper manualmente o `manifest-cache.json` deixando `"manifest"` ausente/`null` mas com um `fingerprint` que ainda casa com o `mods_repo`, `TryLoadPersisted` popularia `_manifestCache` com um `JsonElement` `Undefined`/`Null` — e o erro só apareceria depois, na serialização de `GetManifest`, fora do `try` do load. Não é alcançável pelo próprio writer (que sempre grava um `manifest` válido via `SerializeToElement`, escrita atômica), só por adulteração externa deliberada.

**Por que importa:** robustez — um cache corrompido deve degradar para regeração, não servir conteúdo inválido.

**Resolução:** guard em `TryLoadPersisted` após a checagem de `formatVersion` — `if (p.manifest.ValueKind != JsonValueKind.Object) return false;` → regera em vez de servir Undefined. Build verde.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Code review 01 (sub-agent adversarial, 8 categorias auditadas com prova). 0 🔴 · 0 🟡 · 1 🟢, aplicado (guard de manifest corrompido). Implementação fiel à spec; build 0 erros / 0 warnings novos. Pendente: gate humano (subir o SPT.Server real). |
