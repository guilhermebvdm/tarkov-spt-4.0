# DiscordRaidMap — Memória de Sessões

Mod client **host-only** (roda no host/headless via `HostCheck`) que espelha a raid num mapa enviado ao Discord por webhook. Importado do upstream (`com.fiodor.discordraidmap`). Trabalho em `modded/`; `original/` = upstream pristino.

## Estado atual (fim da Sessão 1)

- **Versão de trabalho:** v1.1.3. Build limpo (0/0), deploy local + zip de teste `modded/Releases/DiscordRaidMap-v1.1.3-test.zip`.
- **OOM do headless:** contribuinte identificado e corrigido — **não** era leak clássico (teardown já era limpo); era **churn de LOH** no render de mapa na CPU (System.Drawing), ~60 MB/render em Customs a cada 5 s → crescimento de working set. Corrigido por downscale (1280) + reuso de buffers.
- **Perf vs oficial (v1.0.0):** churn ~60 MB/render → ~0; buffer de render ~10× menor; upload ~15–40× menor (downscale + JPEG); 2 patches Harmony removidos (1 era per-tick); `CanBroadcast` 1×/raid.
- **Coleta:** 100% no intervalo (sem trabalho per-tick/per-evento). Threading correto: API Unity só na main thread; render (dados puros) em `Task.Run`.
- **Feature de airdrop:** removida (escopo + ambiguidade `IsActive` vs. pousado).
- **Reviews:** MEMORY-LEAK 01/02 + CODE 01/02 — **todos os achados aplicados**. Sem leak acionável (review 02).
- **Dívida remanescente:** GDI+ (`System.Drawing`) roda em thread de fundo — único risco nativo; fix definitivo = atlas de glifos (render 100% gerenciado).

## Pendências abertas

- **[P-1.1] 🔴 Validação in-game (o gate — compila ≠ funciona, AP-06):** RSS **estável** em 20 min/Customs (teste decisivo do OOM), marcadores de morte aparecem e persistem, JPEG sem fundo preto, `LogOutput.log` sem `OutOfMemory` nem erro de GDI+. (aberta 2026-07-25)
- **[P-1.2] 🟡 Dívida GDI+ (ML-02-01):** `System.Drawing` em thread de fundo (Mono). Fix = atlas de glifos pré-renderizado compondo em `Color32[]` → elimina a dependência de GDI+ e o único uso é o texto de nomes. (aberta 2026-07-25)
- **[P-1.3] 🟡 `PROPRIEDADES.md` defasado (CR-02-02):** faltam os 3 configs da seção *Image Output*; regenerar (ou `/review-mod-properties`). (aberta 2026-07-25)
- **[P-1.4] 🟡 Distribuição:** sync do launcher reverte build local (Dev Mod off) — o DLL sumiu do deploy nesta sessão. Versão de teste precisa chegar ao **headless + todos os peers** (mod set casado). (aberta 2026-07-25)
- **[P-1.5] 🟢 Docs/hygiene menores:** `README.md` defasado (JPEG/downscale/coleta no intervalo — CR-02-03); `.csproj` sem `<Version>` (CR-02-04). (aberta 2026-07-25)

---

## 2026-07-25 17:05 (GMT-3) — Sessão 1: investigação do OOM do headless + refactor de performance do DiscordRaidMap

**Tema central:** headless reiniciava por `OutOfMemory` (1ª raid, ~100 mods, 32 GB). DiscordRaidMap apareceu por último no log; auditado, corrigido e transformado em versão de teste muito mais performática.

**Decisões-chave:**
- **Causa (contribuinte), não leak clássico:** o render do mapa na CPU (`Renderer`, System.Drawing) aloca 2 buffers grandes no LOH por render (canvas 30 MB + encode 30 MB em Customs), a cada 5 s, o headless todo. O GC incremental do headless não compacta LOH → working set cresce. O teardown do mod já era correto (sem leak de subscription). Ref: `MEMORY-LEAK-review-01.md` ML-01-01.
- **Fix de memória:** downscale único do fundo p/ lado máx. 1280 (config) + reuso de `_canvas`/`_encodeBuffer`/`_encodeBitmap`/`Font`. Ref: `modded/RaidMap/Renderer.cs`.
- **Fix de dados enviados:** JPEG (qualidade config) no lugar de PNG + downscale → upload ~15–40× menor. Discord re-envia a imagem inteira a cada edição (não há update parcial), então encolher a imagem é a única alavanca. Ref: `Settings.cs`, `DiscordWebhookClient.cs`.
- **Coleta 100% no intervalo:** removidos `PlayerOnDeadPatch` e `AirdropLandedPatch` (este patcheava `AirdropLogicClass.method_3`, chamado de `ManualUpdate` = **per-tick**). Mortes vêm da varredura de corpos que já rodava no intervalo; airdrops (depois) do scan do mundo. Ref: `RaidPatches.cs`, `RaidStateCollector.cs`.
- **Airdrop removido:** escopo + ambiguidade do gate `IsActive`. Ref: commit `6f5960b0`.
- **Versão de teste:** v1.1.3, `modded/Releases/DiscordRaidMap-v1.1.3-test.zip`. Gate `/code-review` satisfeito (2 rodadas).

**Lições / hipóteses descartadas:**
- ❌ **"Leak acumula entre raids"** — refutada pelo usuário: crash na **1ª raid** com restart-a-cada-3 ⇒ é consumo **intra-raid** (per-frame/per-event), não acúmulo per-raid. Promovido à skill `spt-memory-leak-analysis` §1.1.
- ❌ **"O mod no último frame do log de OOM é o culpado"** — refutada: com ~100 mods o OOM é **agregado**; o último a alocar costuma ser a **vítima**. Promovido à skill §1.2.
- ❌ **"JPEG resolve o OOM"** — não: o custo de memória é o `Color32[]` **descomprimido** (existe antes de virar JPEG). Só downscale + reuso resolvem; JPEG é só rede.
- ❌ **Bug que eu introduzi (CR-01-01):** `AddAirdrops` iterava `GetSynchronizableObjects()` sem filtrar → marcador fantasma (as listas contêm objetos pooled/uninited). Pego na minha própria review; depois eliminado com a remoção do airdrop.

**Atividade cronológica:**
1. Criadas skill `spt-memory-leak-analysis` + command `/analyze-memory-leak` + template (trabalho meta-repo — ver Notas).
2. `/analyze-memory-leak` no mod → `MEMORY-LEAK-review-01.md` (ML-01-01 suspeito nº1).
3. v1.1.0: downscale + reuso + cache Font + JPEG.
4. v1.1.1: coleta 100% no intervalo, 2 patches removidos.
5. `/code-review` → `CODE-review-01.md` (8 achados; 1 bug meu).
6. Todos os 8 aplicados → v1.1.2 (review anotada, append-only).
7. Re-varredura: `MEMORY-LEAK-review-02.md` (sem leak acionável) + `CODE-review-02.md` (4 achados).
8. Airdrop removido → v1.1.3.
9. Avaliação de perf/funcionalidade + zip de teste versionado.
10. `/update-mod-graph` (198/316 → 192/304) + esta memória.

**Pendências abertas nesta sessão:** P-1.1 (🔴 validação in-game), P-1.2 (🟡 GDI+), P-1.3 (🟡 PROPRIEDADES.md), P-1.4 (🟡 distribuição/launcher), P-1.5 (🟢 docs).

**Notas relevantes (não-mod, sem destino dedicado — `git log` é a verdade):**
- Criados nesta sessão a skill `.claude/skills/spt-memory-leak-analysis/`, o command `/analyze-memory-leak` e o template `memory-leak-review.md.tmpl`, registrados em `WORKFLOW.md`/`resources.md`. As lições de OOM acima já vivem na skill (§1.1/§1.2/§8) — a memória do mod aponta, não duplica.

**Cross-refs:**
- Aciona [P-1.1] como bloqueador de "entregue" — nenhum fix está validado até o teste in-game.
