# DiscordRaidMap — Code Review · 02

**Mod:** DiscordRaidMap · **Tipo:** client (host-only via `HostCheck`)
**Escopo:** estado **v1.1.2** (após CODE-review-01 aplicado + MEMORY-LEAK-review-02). Segunda passada de correção/qualidade.
**Data:** 2026-07-21
**Skills:** `spt-mod-best-practices`, `csharp-mod-best-practices`, `spt-memory-leak-analysis`, `repo-workflow-best-practices`.

> Cada achado tem ID `CR-02-MM`. Reviews anteriores: [CODE-review-01.md](CODE-review-01.md) (8 achados, todos aplicados), [MEMORY-LEAK-review-02.md](MEMORY-LEAK-review-02.md) (sem leak acionável).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 2 · 🟢 Menores: 2 · Total: 4

## Panorama — verificado BOM

- ✅ **Threading correto:** `CollectSnapshot` (que lê API Unity — `player.Position`, `player.Rotation`, `airdrop.transform.position`, `GetSynchronizableObjects`) roda na **main thread** (dentro de `Update`); só o render `System.Drawing` vai para `Task.Run`, operando **apenas sobre dados puros** (`RaidSnapshot`/`Color32[]`/`Mathf`). Nenhuma API Unity é tocada fora da main thread ([RaidBroadcaster.cs:46-72](modded/RaidMap/RaidBroadcaster.cs#L46)).
- ✅ **Todos os achados de leak fechados** (ver MEMORY-LEAK-review-02): OOM driver eliminado, teardown idempotente, sem retenção estática.
- ✅ **Dependência do Fika** resolvida por reflection com degradação graciosa (soft-dep).
- ✅ **Nullability defensiva** em `CollectSnapshot`/`AddExtracts`/`AddPlayers` (checa `referencePlayer`/`MainPlayer`/controllers nulos).

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-02-01 | B | 🟡 | Filtro de airdrop por `IsActive` pode esconder airdrop **já pousado** | ✅ Resolvido (feature removida, v1.1.3) |
| CR-02-02 | C | 🟡 | `PROPRIEDADES.md` desatualizado — faltam os 3 configs de *Image Output* | Pendente |
| CR-02-03 | E | 🟢 | `README.md` provavelmente desatualizado (JPEG/downscale/coleta no intervalo) | Pendente |
| CR-02-04 | F | 🟢 | `.csproj` sem `<Version>` (só o `BepInPlugin` versiona) | Pendente |

---

## Achados

### CR-02-01 · B — Bug latente · 🟡 Médio

**Filtro de airdrop por `IsActive` pode esconder airdrop já pousado**

**Local:** [`modded/RaidMap/RaidStateCollector.cs:134-145`](modded/RaidMap/RaidStateCollector.cs#L134) (`AddAirdrops`, filtro `!IsInited || !IsActive`)

**Problema:** o fix do CR-01-01 filtra por `IsInited && IsActive` para evitar marcadores fantasma de objetos pooled. Mas isso levanta uma dúvida: um airdrop **já pousado** (a caixa vira container lootável) ainda tem `IsActive == true`? Se o `IsActive` virar `false` após o pouso, o marcador **desaparece** — regressão sobre o comportamento antigo (que mostrava o airdrop permanentemente, uma vez visto). O gate `IsInited` sozinho já exclui objetos pooled (não inicializados), e `SyncObjectProcessorClass.RemoveNonActiveAndStaticObjects` ([SyncObjectProcessorClass.cs:146](../../references/eft-decompiled/Assembly-CSharp/SyncObjectProcessorClass.cs#L146)) já poda objetos removidos das listas — então `GetSynchronizableObjects()` não devolve airdrops "mortos".

**Por que importa:** comportamento visível — o airdrop pousado é justamente o que interessa mostrar no mapa. Não dá para decidir sem observar o `IsActive` de um airdrop pousado in-game.

**Sugestão:**
1. **Verificar in-game:** deixar um airdrop pousar e ver se o marcador some.
2. Se sumir, **relaxar o filtro para só `IsInited`** (mantém pooled fora e preserva pousados):
   ```csharp
   if (syncObject is not AirdropSynchronizableObject airdrop || airdrop == null || !airdrop.IsInited) continue;
   ```
   (Alternativa mais robusta: acumular a posição do airdrop uma vez visto ativo — como fazemos com os mortos — já que airdrop não se move.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (relaxar p/ `IsInited` após confirmar)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (manter `IsActive`): _________________

---

### CR-02-02 · C — Gap vs. convenção · 🟡 Médio

**`PROPRIEDADES.md` desatualizado — faltam os 3 configs novos**

**Local:** [`PROPRIEDADES.md`](PROPRIEDADES.md) vs [`modded/Settings.cs:62-84`](modded/Settings.cs#L62) (seção *Image Output*)

**Problema:** a v1.1.0 adicionou 3 `ConfigEntry` novos (`Max Image Size`, `Image Format`, `Jpeg Quality`) na seção *Image Output*, mas o `PROPRIEDADES.md` não foi regenerado. Convenção do repo (`repo-workflow-best-practices §7`, `AGENTS.md`): todo `ConfigEntry` novo atualiza o `PROPRIEDADES.md` (fonte = `Config.Bind` no código).

**Por que importa:** documentação das opções F12 fica defasada — divergência código × doc.

**Sugestão:** regenerar `PROPRIEDADES.md` a partir de `Settings.cs` (ou rodar `/review-mod-properties` que reconcilia). Inclui a nova seção *Image Output* com defaults/faixas/tooltips.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (deferir): _________________

---

### CR-02-03 · E — Legibilidade/doc · 🟢 Menor

**`README.md` provavelmente desatualizado**

**Local:** [`modded/README.md`](modded/README.md) / [`README.md`](README.md)

**Problema:** o comportamento mudou (envio em **JPEG** por default, imagem **downscaled**, coleta **100% no intervalo**, host-only). Se o README descreve PNG/resolução total/coleta por evento, está defasado.

**Sugestão:** revisar o README quando conveniente. Não bloqueia.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (deferir): _________________

---

### CR-02-04 · F — Melhoria · 🟢 Menor

**`.csproj` sem `<Version>`**

**Local:** [`modded/DiscordRaidMap.csproj`](modded/DiscordRaidMap.csproj)

**Problema:** a versão vive só no `BepInPlugin` (que é o que o F12 mostra — ok). O `.csproj` não tem `<Version>`, então o gate do `/compile-mod` não tem uma segunda fonte para conferir. Funciona, mas adicionar `<Version>1.1.2</Version>` sincroniza as fontes.

**Sugestão:** opcional — adicionar `<Version>` ao `.csproj` e manter em sincronia com o `BepInPlugin`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (deferir): _________________

---

## Verificação in-game (recomendada)

1. **CR-02-01:** airdrop pousado continua com marcador? (decide o filtro).
2. Marcadores de morte de inimigo/boss aparecem e persistem (CR-01-02).
3. RSS estável em 20 min / Customs; `LogOutput.log` sem `OutOfMemory` nem erro de GDI+ (ML-02-01).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-21 | Code review 02 criada (estado v1.1.2, pós-CODE-review-01 + MEMORY-LEAK-review-02) |
