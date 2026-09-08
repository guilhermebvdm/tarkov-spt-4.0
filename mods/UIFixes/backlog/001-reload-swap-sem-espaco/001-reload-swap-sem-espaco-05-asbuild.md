# 001 — Swap de magazine no mesmo slot ao recarregar com R sem espaço · As-Built

**Mod:** UIFixes
**Spec funcional:** [001-reload-swap-sem-espaco-01-spec.md](001-reload-swap-sem-espaco-01-spec.md)
**Spec técnica:** [001-reload-swap-sem-espaco-02-spec-tech.md](001-reload-swap-sem-espaco-02-spec-tech.md)
**Última review técnica:** [001-reload-swap-sem-espaco-03-spec-tech-review-01.md](001-reload-swap-sem-espaco-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-06

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Resumo da correção

`SwapIfNoSpacePatch.Prefix` (`ReloadInPlacePatches.cs`) ganhou um fallback: quando a busca ampla existente (`GetPrioritizedGridsForUnloadedObject(false)`, que nunca considera a mochila) não encontra vaga para o carregador antigo, o código agora checa diretamente o espaço que o carregador novo acabou de deixar livre (`magAddress`) — cobrindo o caso relatado (colete/bolsos sem espaço, ou carregador novo vindo da mochila) sem alterar a prioridade do caso que já funcionava.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/UIFixes/modded/src/Patches/ReloadInPlacePatches.cs` | `SwapIfNoSpacePatch.Prefix` ganha fallback via `magAddress`/`GridItemAddress` quando a busca ampla retorna `null` |
| MODIFICADO | `mods/UIFixes/modded/Shared.props` | Bump `<Version>`: `5.3.21` → `5.3.22` |
| MODIFICADO | `mods/UIFixes/mod.json` | Bump `version`: `5.3.21` → `5.3.22` |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de lógica · 🟠 | Já resolvido na spec técnica antes do build (ordem invertida: fallback, não prioridade) — código implementado exatamente conforme o stub final |
| PA-01-02 | A — Gap · 🟢 | Documentação apenas — sem mudança de código necessária |

## Pendências para fechar o item (fora do escopo do `/code-mod`)

- [ ] `/compile-mod` (build local em `mods/UIFixes/builds/` — sem instalação automática no jogo, por preferência do usuário).
- [ ] Validação in-game dos cenários do checklist §8 da spec técnica (colete cheio com origem mochila/colete, sem regressão com espaço livre, double-R intocado, coop Host/Headless).
- [ ] `/code-review` deste item.

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada.

**Rodada 01 (2026-09-06):** 1 achado aplicado, 0 rejeitados.
- **CR-01-01** (🟢, Legibilidade): `ReloadInPlacePatches.cs:199-202` — comentário inline reescrito no formato canônico `// ref: CR-01-01 — ...`.

**Fix 01 (2026-09-08):** ver [001-reload-swap-sem-espaco-06-fix-01.md](001-reload-swap-sem-espaco-06-fix-01.md) — descoberto que o cenário "0% de espaço livre em qualquer lugar" nunca funcionava mesmo com o fallback CR-01-01, porque o motor nativo (`GClass2006.Run`) colide consigo mesmo ao mover o carregador antigo pra vaga que o novo ainda ocupa. Corrigido com `InteractionsHandlerClass.Swap` atômico (bypassando `ReloadMag` nativo nesse caso específico) + replicação manual de `RemoveLeftHandItem(3f)`/`ForceStopInteractions()` pra preservar a animação/tempo de "recarga penalizada". Validado in-game em Fika Host + Headless, sem `GClass1561`. Versão final: `5.3.26`.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-06 | Build concluído via `/code-mod` |
| 2026-09-06 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — ID: CR-01-01 |
| 2026-09-08 | Fix 01 aplicado — swap atômico pro caso "0% de espaço livre" + pacing de animação. Versão `5.3.24` → `5.3.26` |
