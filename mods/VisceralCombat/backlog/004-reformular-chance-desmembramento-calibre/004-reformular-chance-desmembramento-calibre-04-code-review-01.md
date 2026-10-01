# 004 — Reformular Chance de Desmembramento por Calibre e Parte do Corpo · Code Review 01

**Mod:** VisceralCombat
**Spec funcional:** [004-reformular-chance-desmembramento-calibre-01-spec.md](004-reformular-chance-desmembramento-calibre-01-spec.md)
**Spec técnica:** [004-reformular-chance-desmembramento-calibre-02-spec-tech.md](004-reformular-chance-desmembramento-calibre-02-spec-tech.md)
**Data:** 2026-09-10

**Memória consultada:** snapshot de 2026-09-10 (Sessão 8) — sem pendências que afetem este item diretamente.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟠 Forte | Acumulador de momento pode contar o mesmo pellet duas vezes (`KillPatch.Postfix` + `LimbKillPatch.ProcessLimbKill` processando o mesmo hit em corpo morto) | Pendente |

## Achados durante a implementação (aplicados, não são pontos de review — registrados aqui por transparência)

Três correções de compilação/lógica feitas durante o `/code-mod`, além do previsto na spec técnica:
1. `AmmoTemplate.Caliber` mantém o prefixo `"Caliber"`, mas `AmmoItemClass.Caliber` (usado em `LimbKillPatch.cs`) já vem sem ele (`AmmoItemClass.cs:32`) — **bug pré-existente real**: o código antigo de `LimbKillPatch.cs` (ramo "Dead corpses branch") tentava as duas formas mas nunca batia com as chaves `"Caliber..."` da tabela, então sempre caía no default `0.5f`. Corrigido normalizando (`KillPatch.NormalizeCaliber`) tanto na leitura do JSON quanto nos dois pontos de consumo.
2. `AmmoItemClass.Name` (herdado de `Item.cs:392`) é a chave de localização, não o nome interno — `LimbKillPatch.cs` precisa de `ammo.AmmoTemplate.Name` para bater com as chaves de `dismember_exceptions`.
3. `player.PlayerBody.SkeletonRootJoint` é `Diz.Skinning.Skeleton`, não `Transform` — `BurstHead` corrigido pra usar `player.PlayerBones.Head.Original` (bone real da cabeça) para os efeitos de sangue, mantendo `SkeletonRootJoint` só para `Skin.Init()`.
4. `OnDismembermentPacketClient` (`VisceralEntry.cs`) sempre chamava `DismemberLimb` — corrigido pra despachar pra `BurstHead` quando `packet.bone == "head_burst"`, senão um peer remoto encolheria a cabeça real por engano ao receber o efeito "estourar".

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria opcional**

## Impacto

- 🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos

### CR-01-01 · B — Bug latente · 🟠 Forte

**Acumulador de momento pode contar o mesmo pellet duas vezes em corpos já mortos**

**Local:** [`KillPatch.cs` (`Postfix`, casos 3-6 do switch)](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs) e [`LimbKillPatch.cs` (`ProcessLimbKill`, "Dead corpses branch")](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs)

**Problema:** `KillPatch.Postfix` (patch em `Player.ApplyDamageInfo`) processa braço/perna diretamente no próprio switch (casos 3-6) sempre que `!IsAlive` — incluindo hits **pós-morte**, não só o hit fatal. `LimbKillPatch.ProcessLimbKill` (patch em `BallisticsCalculator.Shoot`, via `VisceralShotProcessor`) **também** processa o mesmo tipo de hit pós-morte, de forma independente, pela sua "Dead corpses branch". Não há evidência de que essas duas entradas sejam mutuamente exclusivas para o mesmo pellet atingindo um corpo já morto — ambas parecem rodar para o mesmo evento físico, com detecção de parte do corpo por caminhos diferentes (`bodyPartType` do `ApplyDamageInfo` vs. detecção por nome de osso do ragdoll).

No código **anterior** a este item, isso já podia acontecer (as duas chamavam `DismemberLimb` para o mesmo hit), mas era inofensivo — `DismemberLimb` já tem uma guarda de idempotência (`if (val.localScale == RagdollHelperClass.limbSize) continue;`) que torna a segunda chamada um no-op seguro. **O novo acumulador de momento (`AccumulateMultiProjectileMomentum`) não tem essa mesma guarda** — cada chamada simplesmente soma o momento do pellet, sem checar se aquela parte do corpo já foi processada. Se as duas entradas realmente processam o mesmo pellet, o momento é contado em dobro, inflando artificialmente a chance calculada pro calibre 12/20/23x75/M576 em corpos já mortos (não afeta o hit fatal em si, nem o desmembramento de perna em vivos, que usa sua própria lógica separada de 30% fixo).

**Por que importa:** Se confirmado, calibres multi-projétil ficariam desmembrando partes do corpo em corpos já mortos com chance mais alta do que a curva calibrada prevê — não é um crash nem uma regressão visível óbvia (o resultado ainda "faz sentido" visualmente, só a probabilidade fica errada), o que torna difícil de perceber sem comparação cuidadosa em jogo.

**Sugestão:** Adicionar a mesma guarda de idempotência que `DismemberLimb` já usa, checando **antes** de acumular: se a parte do corpo alvo já está no estado desmembrado (mesmo teste de escala que `DismemberLimb` usa), pular a acumulação inteira (retornar a chance atual sem somar, ou simplesmente não chamar `ResolveDismemberChance` de novo). Alternativa mais simples de implementar: mover a checagem pra dentro de `AccumulateMultiProjectileMomentum` — receber o `Transform`/`Player` e checar se aquele membro já tem `DismemberedLimbScaler` antes de somar. Precisa de acesso ao `player`/à parte física já resolvida, o que exige repassar mais contexto pro método do que ele tem hoje (só recebe `playerId`, não o `Player` inteiro) — ajuste de assinatura necessário.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-10 | Code review 01 criada via `/code-review` |
