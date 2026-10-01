# 008 — Fix 03 · Corpo recém-morto perto do jogador era "ejetado"/desaparecia (múltiplos ossos empurrados na mesma checagem)

**Mod:** VisceralCombat
**Item raiz:** [008-atropelar-acorda-corpos-itens-01-spec.md](008-atropelar-acorda-corpos-itens-01-spec.md)
**Asbuild:** [008-atropelar-acorda-corpos-itens-05-asbuild.md](008-atropelar-acorda-corpos-itens-05-asbuild.md)
**Criado:** 2026-09-21
**Disparado por:** feedback in-raid do usuário — empurrão funcionando corretamente pra corpos já assentados e itens no chão, mas bots morrendo perto do jogador "ejetavam" instantaneamente e desapareciam sem direção visível

## Contexto

Após o fix 02 (checagem periódica por proximidade), o usuário confirmou que o empurrão funcionava em corpos já assentados e itens largados, mas reportou algo diferente: bots que morriam perto dele eram lançados com força absurda no instante da morte, desaparecendo do mapa. Corpos já no chão há mais tempo e itens não tinham esse problema.

## Causa raiz

`CheckOnce()` ([PlayerContactPushPatch.cs](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs)) busca todos os colisores num raio de 0.6m ao redor do jogador (`Physics.OverlapSphereNonAlloc`) e aplica o empurrão em **cada Rigidbody encontrado, independentemente**. Um corpo recém-morto tem vários ossos de ragdoll (braço, antebraço, perna, tronco, etc.), cada um seu próprio `Rigidbody`/`Collider` — e, no instante da morte, todos esses ossos ainda estão bem próximos entre si (o corpo ainda não caiu/se espalhou no chão). Se o jogador estiver perto o suficiente, **vários ossos do MESMO corpo caem dentro do raio de detecção na mesma checagem**, e cada um recebe um empurrão independente — múltiplos "chutes" simultâneos em pontos diferentes do mesmo corpo, com os `Joint`s do ragdoll tentando reconciliar forças contraditórias entre si, resultando num lançamento descontrolado. Um corpo já assentado no chão não sofre disso porque os ossos já estão espalhados (poucos caem dentro de um raio de 0.6m de cada vez); um item largado normalmente tem só 1 `Rigidbody`, então também não é afetado.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs` | Adicionado `_processedCorpseRoots` (`HashSet<Transform>`, reaproveitado a cada checagem via `.Clear()`, mesmo padrão do buffer de colliders) — antes de empurrar um osso de corpo, verifica se a raiz (`col.transform.root`) daquele corpo já foi processada nesta mesma checagem; se sim, pula. Garante no máximo 1 empurrão por corpo por checagem, independente de quantos ossos dele estejam no raio. Itens largados não são afetados por essa dedupe (não têm o mesmo problema). |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (build 3.12.7)
- [ ] **In-raid:** matar/deixar um bot morrer perto do jogador em movimento — corpo reage de forma controlada (um empurrão, não um lançamento) — pendente de novo teste do usuário
- [ ] **Fika/multiplayer:** sem regressão com outros players — ou `N/A: <razão>`
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Fix criado e aplicado — build 3.12.7 compilada, 0 erros. Aguardando validação em raid do usuário. |
