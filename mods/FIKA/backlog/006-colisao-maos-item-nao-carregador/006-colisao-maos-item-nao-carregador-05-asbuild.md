# 006 — Trava de mãos ao equipar arma/faca/granada após ação recente (item não-carregador) · As-Built

**Mod:** FIKA
**Spec funcional:** [006-colisao-maos-item-nao-carregador-01-spec.md](006-colisao-maos-item-nao-carregador-01-spec.md)
**Spec técnica:** [006-colisao-maos-item-nao-carregador-02-spec-tech.md](006-colisao-maos-item-nao-carregador-02-spec-tech.md)
**Última review técnica:** [006-colisao-maos-item-nao-carregador-03-spec-tech-review-02.md](006-colisao-maos-item-nao-carregador-03-spec-tech-review-02.md)
**Build inicial:** 2026-09-11

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Contexto — pausa e retomada

Este item ficou **pausado** na Sessão 6 (2026-09-10) aguardando validação empírica da causa raiz assumida (`PA-01-01`, review 01). Na Sessão 7 (2026-09-11), o usuário compilou um build Debug, reproduziu o bug em raid Headless real (municiamento contínuo de carregadores) e capturou o log exato do momento da falha — provando que a causa raiz real é diferente da assumida (`GEventArgs9`/`GEventArgs10`, não `GEventArgs17`). A spec técnica foi inteiramente reescrita a partir dessa evidência (ver review 01 `PA-01-01` e review 02) antes deste build.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs` | Fix 1a: `IsSelfReferentialHandsTransition` generalizada (removida restrição a `MagazineItemClass`). Fix 1b: novo método `IsRecentSelfHandsBookkeepingEvent` + bloco `item == geventArgs2.Item` de `CheckItemAction` (linhas **186-192** no código final, pós-build — a spec técnica citava `182-188`, faixa de antes do `/code-mod`, ver `CR-01-01`) passa a tolerar colisão recente contra `GEventArgs9`/`GEventArgs10` do mesmo jogador no mesmo item |
| CRIADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/HandsBookkeepingTimestampPatch.cs` | Fix 1b: Postfix em `TraderControllerClass.method_19(GEventArgs1)` (resolvido por predicado de assinatura + validação em runtime), correlaciona `(controller, item) → timestamp` pra `GEventArgs9`/`GEventArgs10` |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Registro explícito de `HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent`, protegido por `try/catch` (alvo obfuscado pode não resolver — `GetTargetMethod() == null` lança exceção, sem a proteção derrubaria o `Awake()` inteiro). Bump `FikaVersion`: `2.3.16` → `2.3.17` |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs` | Fix 2: novo campo `_unknownPacketCount` + método `TryReadAllPackets` (`catch (Exception)`, cobre hash desconhecido e payload malformado); `OnNetworkReceive` passa a chamar `TryReadAllPackets` em vez de `_packetProcessor.ReadAllPackets` direto |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs` | Fix 2: espelho exato de `FikaClient.cs`, tag de log `"[SERVER]"` |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs` | Fix 3: os 2 blocos `catch (Exception) { }` vazios (linhas ~63 e ~114) agora logam via `FikaGlobals.LogError`, mesmo padrão já usado no resto do arquivo |
| MODIFICADO | `mods/FIKA/mod.json` | Bump do componente `plugin`: `2.3.16` → `2.3.17` |

Implementação seguiu os stubs da spec técnica sem desvios. Build verificado com `dotnet build -c Release`: 0 erros, 1 warning pré-existente e não relacionado (conflito de versão do `System.Runtime.CompilerServices.Unsafe`). `node scripts/check-packet-hashes.js` confirmou 0 colisões de CRC-16 (os 4 avisos de duplicata reportados são pré-existentes, entre `Band-Aid` e `TRL-ImmersiveCombatMedicine`, não relacionados a este item). Build copiado para `mods/FIKA/builds/Fika.Core-260911-0541.dll` — **`Fika.Headless.dll` não foi recompilado nesta sessão** (nenhuma mudança em `Fika-Headless/`; se o Headless usar uma cópia própria de `Fika.Core.dll`, ela precisa ser atualizada manualmente antes da validação in-game).

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

Nenhum — todos os pontos das reviews 01 e 02 (`PA-01-01`, `PA-01-02`, `PA-02-01`, `PA-02-02`) já tinham sido resolvidos diretamente na spec técnica antes deste `/code-mod` (ver Histórico de `006-colisao-maos-item-nao-carregador-02-spec-tech.md`). A implementação seguiu os stubs já corrigidos, sem desvios que gerassem resolução adicional aqui.

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

### Rodada 01 (2026-09-11) — [006-colisao-maos-item-nao-carregador-04-code-review-01.md](006-colisao-maos-item-nao-carregador-04-code-review-01.md)

- **Aplicado:** `CR-01-01` — tabela "Arquivos alterados" acima atualizada com a faixa de linha final do bloco de Fix 1b (`186-192`), já que a spec técnica citava a faixa de antes do `/code-mod` (`182-188`).
- Rejeitados: nenhum. Pulados: nenhum.

## Validação in-game

**Cenário bloqueador — ✅ Confirmado em 2026-09-11**, pelo usuário, em raid Headless real, usando o build `Fika.Core-260911-0541.dll` (o mesmo copiado nesta sessão, sem recompilação adicional):

- Municiamento de carregador cancelado no meio (interrupção).
- Vários carregadores municiados em sequência, um após o outro.
- Carregadores esvaziados e reenchidos um por um.
- **Nenhuma trava de mãos em nenhum dos ciclos** — o sintoma original (`"Default Inventory is currently being modified"` ao equipar arma após `SPT-ContinuousLoadAmmo`) não se reproduziu mais.

Isso confirma que o Fix 1b (`HandsBookkeepingTimestampPatch` + tolerância no bloco genérico de `CheckItemAction`) resolve a causa raiz real identificada nesta sessão. A janela de graça `HandsBookkeepingGraceWindowSeconds` (2.5s, `TODO confirmar` — `P-7.2`) se mostrou suficiente nos testes realizados, mas continua não-calibrada por instrumentação — o `TODO` permanece.

**Validações não-bloqueadoras ainda pendentes** (não impedem considerar o item entregue, mas seguem como débito de validação — `P-7.1` rebaixada na memória do mod):
- Concorrência genuína entre dois jogadores pela mesma arma continua bloqueada (proteção estrutural, não testada em raid).
- Corner case cura→faca/granada/troca-de-arma (Fix 1a).
- Cenário que dispare especificamente `GEventArgs9` (só `GEventArgs10` foi empiricamente confirmado).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-11 | Build concluído via `/code-mod` |
| 2026-09-11 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — ID: CR-01-01 |
| 2026-09-11 | Validação in-game bloqueadora confirmada pelo usuário — cenário original do `ContinuousLoadAmmo` sem travamento em raid Headless real |
