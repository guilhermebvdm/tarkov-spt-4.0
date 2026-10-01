# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md](002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-08-24 (Sessão 2026-08-24) — nenhuma pendência aberta que afete este item. **Docs técnicos conferidos:** `spt-antipatterns.md`, `fika-packet-desync-prevention-plan.md` (ambos já citados corretamente na spec técnica §7/§9).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | Ponto de inserção de `DropHeadEquipment` fica dentro do loop por-transform, não depois dele | ✅ Resolvido em 2026-09-09 |
| PA-01-02 | A — Gap | 🟡 Importante | Efeito colateral de pular `DropItemDead` sobre `Corpse.SetItemInHandsLootedCallback` não investigado | ✅ Resolvido em 2026-09-09 |
| PA-01-03 | B — Edge Case | 🟡 Importante | Autoridade de rede do item 1 (arma) não tem precedente verificável no código, ao contrário do item 2 | ✅ Resolvido em 2026-09-09 |
| PA-01-04 | A — Gap | 🟢 Menor | §7 não cita a evidência que já existe no próprio mod confirmando o comportamento do gate para o item 2 | ✅ Resolvido em 2026-09-09 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-09

**`DropHeadEquipment` seria chamado dentro do loop por-transform, não uma vez por evento**

**Problema:** A spec técnica §5.2 instrui inserir a chamada `DropHeadEquipment(player)` "logo após o bloco existente de efeito sonoro de cabeça (contexto: KillPatch.cs:437-445)", dentro do mesmo `if ((int)bodyPartType == 0 ...)`. Conferindo o arquivo real ([KillPatch.cs:230-448](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L230-L448)), esse bloco (linhas 437-447) está **dentro** do `foreach (Transform val in array)` aberto na linha 230 e fechado na linha 448 — ou seja, roda **uma vez para cada transform** cujo nome contém a palavra-chave do osso (`affectedLimbs`, resolvido na linha 223-227 via `Utils.EnumerateHierarchyCore` com `.Contains(boneLower)`). Para a cabeça isso tipicamente casa **múltiplos** transforms na hierarquia (ex.: o osso raiz `Base HumanHead` e colliders/filhos cujo nome também contém "head"). Se `DropHeadEquipment` for inserido ali dentro, ele é chamado múltiplas vezes por um único evento de desmembramento de cabeça — não trava (o guard `ContainedItem != null` faz as chamadas extras virarem no-op), mas é logicamente errado e desperdiça `ThrowItem`/checagens a cada iteração, na contramão do check 4 da própria §9 (mudança de estado via API canônica "com side-effects mapeados" — aqui o side-effect não está mapeado corretamente).

**Por que importa:** Um implementador seguindo a spec ao pé da letra reproduz o bug. Funciona "por acidente" hoje (por causa do guard de null), mas é frágil: qualquer refatoração futura que troque a ordem das chamadas ou remova o guard reintroduz duplicação real (ex.: se um dia o guard for trocado por um contador/persistência que não tolera reentrada). Também é o tipo de erro que o `/code-review` provavelmente pegaria depois, custando uma rodada a mais.

**Sugestão:** Mover a chamada para **depois** do `foreach` fechar — o ponto correto é logo após a linha 448 (`}` que fecha o `foreach`), no mesmo nível do bloco já existente `if (player.IsYourPlayer && (int)bodyPartType == 0)` (linha 449), que é precisamente o precedente já usado neste método para "executar algo uma vez por evento de cabeça, fora do loop por-transform". Reescrever a §5.2 assim:

```csharp
// (fora do foreach, mesmo nível do bloco "if (player.IsYourPlayer && (int)bodyPartType == 0)" já existente — KillPatch.cs:449)
if ((int)bodyPartType == 0)
{
    DropHeadEquipment(player);
}
if (player.IsYourPlayer && (int)bodyPartType == 0)
{
    // ... bloco já existente, sem alteração ...
}
```

E manter o bloco de `bloodSFX` (linhas 437-447) exatamente onde está, sem tocar nele.

**Decisão:**
- `[x]` Aceitar sugestão
<!-- Resolução: chamada movida para fora do foreach na spec técnica §5.2 (nível do bloco "if player.IsYourPlayer && bodyPartType == 0"), citando KillPatch.cs:230/448/449. -->

---

### PA-01-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-09

**Efeito de pular `DropItemDead` sobre `Corpse.SetItemInHandsLootedCallback` não investigado**

**Problema:** `Player.OnDead` registra `Corpse.SetItemInHandsLootedCallback(ReleaseHand)` em [Player.cs:30660](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30660) **antes** de iniciar a corrotina `method_98()` que chama `DropItemDead`. A spec técnica (§5.1, §6) propõe que `WeaponDropOnDeathPatch` retorne `false` para armas não-faca, pulando inteiramente o corpo de `DropItemDead` — o que significa que `_garbage`, o `Rigidbody` físico e o `RigidbodySpawner.RemoveEvent` (Player.cs:26826-26854) **nunca são criados** para essa morte. A spec não verificou o que `Corpse.SetItemInHandsLootedCallback` realmente faz internamente (arquivo `Corpse.cs`, não lido nesta spec) nem se esse callback, registrado mas nunca dado o gatilho esperado (presumivelmente algo como "jogador lootou a arma pendurada do cadáver"), fica órfão de forma inofensiva ou se deixa algum estado do `Corpse`/`PlayerBones` pendente (ex.: pose de mão nunca resetada, já que o `HandPosers[1].Lerp2Target(...)` de `DropItemDead:26850` também é pulado).

**Por que importa:** Se `SetItemInHandsLootedCallback` espera ser eventualmente disparado (mesmo que não haja handler crítico), ou se o corpo do cadáver depende do `RigidbodySpawner`/`_garbage` criado por `DropItemDead` para algo além do fling cosmético (ex.: um "slot vazio" visual da mão precisa desse rigidbody para não ficar com a malha da mão fechada/quebrada), pular o método inteiro pode deixar o cadáver com uma pose de mão ou hierarquia visual inconsistente — um bug só visível in-game, não no código.

**Sugestão:** Antes do `/code-mod`, ler `Corpse.cs` (buscar `SetItemInHandsLootedCallback` e o campo/evento que ele dispara) para confirmar que não fica pendente de forma prejudicial quando `DropItemDead` é pulado. Se a leitura confirmar que é inofensivo (ex.: só limpa um callback opcional que nunca é obrigatório), documentar a citação `arquivo.cs:linha` na spec técnica §7 como evidência fechando este ponto. Se não for possível confirmar estaticamente, adicionar ao checklist de validação in-game (§8) um item explícito: "conferir visualmente a mão do cadáver após o drop da arma (sem pose quebrada/T-pose parcial)".

**Decisão:**
- `[x]` Aceitar sugestão
<!-- Resolução: lido Corpse.cs:287-302/247 — SetItemInHandsLootedCallback só dispara via RemoveLootItem quando alguém loota do CADÁVER o item "em mãos"; como o ThrowItem já removeu o item da árvore do cadáver, isso nunca é acionado, e o delegate nunca invocado (ReleaseHand) só faria um Destroy() null-safe sobre um _garbage que nunca chegou a ser criado. Sem leak, sem bug visual. Evidência incorporada na spec técnica §7. -->

---

### PA-01-03 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-09

**Autoridade de rede do item 1 (arma) não tem precedente verificável, ao contrário do item 2**

**Problema:** A spec técnica §7 trata o risco de autoridade de rede (gate `FikaBackendUtils.IsServer || IsSinglePlayer`) como uma única categoria de risco para os itens 1 e 2, mas eles não são equivalentes. Para o **item 2** (capacete/óculos), existe evidência direta no próprio mod de que o host sempre processa `DismemberLimb(..., isFromNetwork: true)` mesmo quando o evento foi detectado por um client (`VisceralEntry.cs:391-397` e `:421-423` — `OnDismembermentPacketServer` chama `OnDismembermentPacketClient` antes de relayar), confirmando que o gate `IsServer` cobre exatamente uma execução por evento, na máquina certa. Para o **item 1** (arma), **não existe pacote nem callback equivalente** — `WeaponDropOnDeathPatch` intercepta diretamente `Player.DropItemDead`, um método vanilla que roda por conta própria em cada peer via `Player.OnDead`/`method_98` (sem nenhum roteamento host→client controlado pelo mod). A spec não verificou (nem cita evidência) se `Player.OnDead` de fato executa em todos os peers para todo `Player` (incluindo o personagem humano de um **client remoto**, quando esse client morre) — presume isso por analogia com `KillPatch.Postfix`/`ApplyDamageInfo` (que sabidamente roda em todo peer, exigindo os gates já vistos em `KillPatch.cs:113,170`), mas não confirma que `OnDead`/`DropItemDead` seguem o mesmo padrão, nem se um `ThrowItem` disparado pelo **host** sobre o `PlayerInventoryController` do personagem de um **client remoto** de fato replica de volta para aquele client via o canal nativo de operação de inventário do EFT/Fika.

**Por que importa:** Se a suposição estiver errada — por exemplo, se o `InventoryController` de um client remoto não for mutável a partir do host da forma que o gate assume, ou se `DropItemDead` só roda localmente na máquina "dona" daquele `Player` (e não em todo peer) — o resultado poderia ser: a arma nunca cai quando um **client** morre (porque o host nunca vê o `OnDead` daquele client rodar do jeito esperado), ou cai só localmente sem replicar (o oposto do problema que o gate tenta evitar). Esse é justamente o cenário que a spec funcional marca como critério de aceite obrigatório ("Fika/multiplayer... quem observa de outra máquina vê o mesmo item").

**Por que importa (2):** Esse ponto já é reconhecido *parcialmente* pela própria spec técnica no checklist §8 ("client morre" está na lista de validação coop), mas o item de checklist não é suficiente sozinho — se a suposição arquitetural estiver errada, a validação in-game vai mostrar isso tarde (depois do código escrito), quando o certo é registrar a incerteza como uma decisão explícita de risco aceito, não como um item de checklist genérico entre vários.

**Sugestão:** Duas opções, ambas aceitáveis — a spec deveria escolher uma explicitamente antes do `/code-mod`:
1. **Investigar antes de codar:** ler `references/fika-plugin/Fika.Core/Main/Players/ClientPlayer.cs` (ou equivalente) para confirmar se `OnDead`/`DropItemDead` roda em toda máquina para todo `Player`, e se a mutação de inventário via `ThrowItem` feita pelo host sobre um `PlayerInventoryController` remoto de fato usa o mesmo canal de replicação que outras operações de inventário (drop de item do inventário, por exemplo) já usam com sucesso neste jogo — documentar a citação encontrada na spec técnica §0/§7.
2. **Aceitar o risco e validar empiricamente:** manter a spec como está, mas mover a validação "client morre com arma em mãos, host observa" do checklist genérico (§8) para o topo da lista, marcada explicitamente como o teste que **decide** se o gate atual é a implementação certa ou se precisa de um pacote próprio de sincronização (nesse caso, o item voltaria para `/create-technical-spec` com uma abordagem revisada).

**Decisão:**
- `[x]` Aceitar sugestão (opção 1 — investigado)
<!-- Resolução: lido Player_OnDead_Patch.cs (FIKA transpila LocalPlayer.OnDead → Player.OnDead direto) e ObservedPlayer.cs:1133-1170 (ObservedPlayer.OnDead chama base.OnDead) — confirma que OnDead/DropItemDead roda por peer, tanto para o LocalPlayer quanto para todo ObservedPlayer observado, mesmo padrão do item 2. Não prova 100% a propagação de volta ao client dono via ThrowItem no host, então a validação coop (opção 2) permanece como fechamento empírico, reordenada na spec técnica §8 como teste decisivo (client morre, host observa) em vez de item de checklist genérico. Evidência incorporada na spec técnica §7. -->

---

### PA-01-04 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-09

**§7 não cita a evidência que já existe confirmando o gate do item 2**

**Problema:** Como levantado em PA-01-03, `VisceralEntry.cs:391-397` e `:421-423` já demonstram que o host processa `DismemberLimb(..., isFromNetwork: true)` para eventos originados em qualquer client, o que é exatamente a evidência que falta para reduzir a incerteza do 🔴 central de §7 **no caso do item 2** (não do item 1, que continua incerto — ver PA-01-03). A spec técnica atual trata os dois itens com o mesmo grau de incerteza ("nenhuma auditoria anterior... testou especificamente"), quando na verdade o item 2 tem mais respaldo do que o texto atual sugere.

**Por que importa:** Sub-representar a evidência já disponível faz o risco parecer mais uniforme/grave do que é, podendo levar a validação in-game redundante no item 2 quando o esforço deveria se concentrar no item 1 (PA-01-03).

**Sugestão:** Adicionar ao final do primeiro parágrafo de §7 a citação: "Para o item 2, `VisceralEntry.cs:391-397`/`:421-423` confirma que o host sempre executa `DismemberLimb(isFromNetwork: true)` para eventos originados em qualquer peer antes de relayar — o gate `IsServer` está architecturally correto para este caso. O mesmo não está confirmado para o item 1 (ver PA-01-03)." Isso também permite reduzir o item de checklist §8 correspondente ao item 2 de "obrigatório" para "confirmação", mantendo o foco da validação coop no item 1.
<!-- Resolução: citação incorporada na spec técnica §7 (subseção "Item 2 — confirmado"); checklist §8 reordenado priorizando o teste do item 1. -->

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

## Status

✅ **Pronta para `/code-mod`** — os 4 pontos desta rodada foram resolvidos (3 por investigação adicional no Assembly/FIKA, 1 por correção direta na spec). O risco residual de PA-01-03 (autoridade de rede do item 1) não é mais um bloqueador de código, mas permanece como **decisivo** no checklist de validação coop da spec técnica §8 — se a validação empírica falhar, o item retorna para `/create-technical-spec`.
