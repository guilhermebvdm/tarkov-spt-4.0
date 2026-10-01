# 004 — Reformular Chance de Desmembramento por Calibre e Parte do Corpo · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [004-reformular-chance-desmembramento-calibre-02-spec-tech.md](004-reformular-chance-desmembramento-calibre-02-spec-tech.md)
**Data:** 2026-09-10

**Memória consultada:** snapshot de 2026-09-10 (Sessão 8) — pendências de outros itens não afetam este.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | — | Contagem dupla de momento na rolagem de cabeça | ✅ Já resolvido na spec (§6.1 refatorado antes desta review) |
| PA-01-02 | C — Erro de Lógica | — | `damageInfo.FireIndex` presumido sem confirmar | ✅ Já resolvido na spec (`DamageInfoStruct.cs:31/109` confirmado) |
| PA-01-03 | A — Gap | 🟡 Importante | Curva de momento e multiplicador "estourar" são placeholders sem calibração | ✅ Aplicado em 2026-09-10 (movido pro JSON) |
| PA-01-04 | B — Edge Case | 🟢 Menor | `BurstHead` ancora no `SkeletonRootJoint` sem confirmar se é o bone certo pra cabeça | ✅ Aceito em 2026-09-10 (`TODO confirmar` mantido, resolve em `/code-mod`) |

## Verificação de citações de linha (pós itens 002/003, arquivos já editados nesta sessão)

Conferido contra o estado atual dos arquivos (não o estado de quando os itens 002/003 começaram):
- `KillPatch.cs:88` (`dismemberChance = 0.5f`) e `:156-157` (`case 0:`) — **inalterados**, batem com a spec.
- `LimbKillPatch.cs` — **corrigido durante a própria redação**: a guarda de Boss/escolta (item 003, linhas 66-79) desloca tudo depois em +9 linhas. A spec já reflete os números corretos (`:191-198` estratégia B, `:233-250` ramo de corpo morto) — citado aqui só pra registrar que a verificação foi feita.

## Pontos

### PA-01-01 · C — Erro de Lógica

**Contagem dupla de momento na rolagem de cabeça** — já corrigido na própria spec técnica (§6.1) antes desta review ser escrita: a primeira versão do stub chamava uma função soma-e-retorna duas vezes (uma por `isBurst`), contando o mesmo pellet duas vezes no acumulador. Corrigido separando `AccumulateMultiProjectileMomentum` (side-effect, uma chamada) de `MomentumToChance` (pura). Nenhuma ação adicional necessária.

### PA-01-02 · C — Erro de Lógica

**`damageInfo.FireIndex` presumido sem confirmar** — já corrigido: confirmado no Assembly (`DamageInfoStruct.cs:31,109`) que o campo existe e é populado a partir do `EftBulletClass` do tiro. Nenhuma ação adicional necessária.

---

### PA-01-03 · A — Gap · 🟡 Importante

**Curva de momento (mecanismo A) e multiplicador de "estourar" são valores de exemplo, não calibrados**

**Problema:** `_multiProjectileMomentumMin = 3.0f`, `_multiProjectileMomentumMax = 15.0f` (§5/§6.1) e o multiplicador `1.5x` de `ResolveHeadOutcome` pra derivar `burstChance` a partir de `offChance` são escolhas de exemplo do autor da spec, não valores calibrados pelo usuário. A spec já documenta isso como risco em aberto (§8), mas reforço aqui como achado formal porque afeta diretamente se o mecanismo A "sente" parecido com os valores manuais da tabela B pra calibres únicos.

**Por que importa:** Se os thresholds ficarem descalibrados, o calibre 12/20/23x75 (mecanismo A) pode desmembrar bem mais ou bem menos que o pretendido, mesmo com a lógica de código 100% correta — é um problema de dado, não de implementação, mas só aparece testando em jogo.

**Sugestão:** Antes do `/code-mod` fechar como "pronto", rodar uma validação específica: comparar o resultado do mecanismo A pra calibre 12 buckshot contra a expectativa intuitiva do usuário (ex.: "3 chumbinhos no braço de perto deviam quase sempre arrancar") e ajustar `_multiProjectileMomentumMin/Max` no código (ou promover pra dentro do JSON, se o usuário quiser recalibrar sem recompilar — considerar mover esses 2 valores pro `VD_Calibers.json` em vez de constantes hardcoded, já que são exatamente do mesmo tipo de dado calibrável que o resto da tabela).

**Decisão:**
- `[x]` Aceitar sugestão (com modificação: mover os thresholds pro JSON em vez de constante)
<!-- Resolução: aplicado no stub — ver §5/§6.1 atualizados após esta review. -->

---

### PA-01-04 · B — Edge Case · 🟢 Menor

**Ponto de ancoragem do prop `Head_1`/`Head_2` em `BurstHead` não confirmado**

**Problema:** O stub usa `player.PlayerBody.SkeletonRootJoint` como âncora (mesmo campo usado por `DismemberLimb` pra ancorar caps de membro), mas cabeça pode precisar de um bone mais específico (`PlayerBones.Head`, por exemplo) pra não sobrepor errado quando a cabeça real continua na cena (diferente do caso de membro, onde a cabeça real É removida, então qualquer ancoragem aproximada funciona visualmente).

**Por que importa:** Se o prop `Head_1`/`Head_2` aparecer deslocado da cabeça real, o efeito "estourar" fica visualmente quebrado (ex.: sangue flutuando ao lado da cabeça em vez de sobre ela).

**Sugestão:** Já marcado como `TODO confirmar` explícito no stub (§6.2) — resolver visualmente em jogo durante `/code-mod`, testando `PlayerBones.Head` como alternativa se `SkeletonRootJoint` não alinhar bem. Não bloqueia o código de compilar nem a lógica de chance — é ajuste de posicionamento visual.

**Decisão:**
- `[x]` Aceitar sugestão (manter `TODO confirmar` no código, resolver em `/code-mod` com teste visual)

---

## Status

✅ **Pronta para `/code-mod`**, com 1 ajuste aplicado nesta rodada (thresholds do mecanismo A movidos pro JSON) e 1 `TODO confirmar` visual que só se resolve testando em jogo (já sinalizado explicitamente no código, não é um bloqueador de compilação). Falta ainda a tabela B final do usuário e a lista de exceções (mecanismo C) antes de gerar o `VD_Calibers.json` de verdade — o código funciona com qualquer conteúdo de tabela, então isso não bloqueia a implementação da lógica, só o preenchimento de dado final.
