# 005 — Toggle Mestre (F12) com Núcleo de Compatibilidade Sempre Ativo em Coop · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [005-toggle-mestre-compat-coop-02-spec-tech.md](005-toggle-mestre-compat-coop-02-spec-tech.md)
**Data:** 2026-09-20

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-09-20 (Sessão 13) — nenhuma pendência 🔴 que afete este item; `[P-10.2]`/`[P-10.3]` já tratadas corretamente na spec (ver §2 da spec técnica). **Docs técnicos conferidos:** `spt-antipatterns.md` (nenhuma violação de AP-NN encontrada além do já discutido abaixo).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🔴 Bloqueador | §2 não cobre todas as checagens de config das 6 categorias — pelo menos 7 pontos reais faltando, achados por grep exaustivo | ✅ Resolvido em 2026-09-20 |
| PA-01-02 | B — Edge Case | 🟡 Importante | Capacete/óculos podem cair sem o efeito visual de desmembramento correspondente (consequência do desacoplamento de `EnableDismemberment` em §1) — falta justificativa explícita de que isso é aceitável | ✅ Resolvido em 2026-09-20 |
| PA-01-03 | B — Edge Case | 🟢 Menor | Setup de colisão de camada física em `GameStartedPatch` (`BodyCollision`/`UseActiveRagdolls`, raid-start-only) não tem decisão explícita sobre entrar ou não na Camada 1 | ✅ Resolvido em 2026-09-20 |

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

### PA-01-01 · A — Gap · 🔴 Bloqueador · ✅ Resolvido em 2026-09-20

**§2 não cobre todas as checagens de config das 6 categorias — pelo menos 7 pontos reais faltando**

**Problema:** A tabela §2 da spec técnica lista 19 pontos de patch, mas um grep exaustivo (`grep -rn "VisceralEntry\.Instance\.<Propriedade>\.Value" mods/VisceralCombat/modded/`) rodado nesta revisão pra CADA uma das 13 propriedades booleanas das 6 categorias achou pelo menos **7 ocorrências reais não cobertas pela tabela**:

1. [`KillPatch.cs:319`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L319) — `UseActiveRagdolls.Value` (branch de ativação de ragdoll físico na morte).
2. [`KillPatch.cs:409`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L409) — `UseActiveRagdolls.Value` (segunda ocorrência, branch de desmembramento de membro).
3. [`KillPatch.cs:821`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L821) — `EnableBloodEffects.Value` (dentro de `SpawnOldVolumetricBlood`).
4. [`KillPatch.cs:968`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L968) — `EnableBloodEffects.Value` + `ArterySpray.Value` (dentro de `SpawnArterialSprays`).
5. [`VisceralCombat.Dismemberment.Patches/BleedPatch.cs:80`](../../modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs#L80) — `EnableArmorSparks.Value`. A spec (§2, linha de `BleedPatch.cs`) diz "3 ocorrências" — na verdade são **4** (`:38`, `:80`, `:142`, `:201`); a de `:80` ficou de fora da contagem.
6. [`VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs:582`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L582) — `EnableImpactBloodCloud.Value`. **Este arquivo não aparece em nenhum lugar da spec técnica** — nem na tabela §2, nem na lista de arquivos §4.
7. `KillPatch.cs` também usa `OnlyPlayersCanActiveRagdollEnemies.Value` (`:321`, `:412`) — não precisa de `IsCategoryActive` própria (já está `&&`-encadeada com `UseActiveRagdolls.Value` na mesma condição, então corrigir os itens 1-2 acima resolve esta transitivamente), mas vale citar explicitamente na spec pra deixar claro que foi considerada, não esquecida.

**Por que importa:** Se `/code-mod` seguir §2 ao pé da letra, o toggle mestre desligado **não impede** a ativação de ragdoll físico na morte (itens 1-2) nem os efeitos de sangue de `KillPatch.cs` (itens 3-4) nem as faíscas de armadura (item 5) nem a nuvem de sangue de impacto (item 6) — ou seja, 4 das 6 categorias prometidas na spec funcional ("Ragdolls \| Character Properties", "Blood") continuariam parcialmente ativas com o mestre desligado, quebrando o critério de aceite "nenhum efeito das 6 categorias confirmadas ocorre na tela desse jogador" e a promessa central de "comportamento equivalente a não ter o mod instalado".

**Sugestão:** Duas ações:
1. Adicionar os 6 pontos concretos (itens 1-6 acima) à tabela §2 e à lista de arquivos §4, com a mesma transformação já usada pros outros pontos de cada arquivo (`IsCategoryActive(UseActiveRagdolls)` nos itens 1-2; `IsCategoryActive(EnableBloodEffects)` nos itens 3-4; corrigir a contagem de `BleedPatch.cs` de "3" pra "4 ocorrências" incluindo `:80`; adicionar `RagdollHelperClass.cs:582` como novo arquivo tocado com `IsCategoryActive(EnableImpactBloodCloud)`).
2. **Adicionar um passo explícito ao checklist §8** (antes do primeiro item de edição): rodar `grep -rn "VisceralEntry\.Instance\.<Propriedade>\.Value" mods/VisceralCombat/modded/` pra CADA uma das ~13 propriedades booleanas de categoria (lista completa: `EnableDismemberment`, `EnableBloodEffects`, `UseActiveRagdolls`, `ShootHelmetOff`, `NeverDeleteShells`, `ItemForce`, `BodyCollision`, `DisableRagdollsAfterTime`, `OnlyPlayersCanActiveRagdollEnemies`, `EnableImpactBloodCloud`, `EnableArmorSparks`, `UseOldBloodDecal`, `ArterySpray`) e conferir CADA resultado contra a tabela §2 antes de considerar o levantamento completo — a enumeração manual (mesmo com um agente de pesquisa dedicado a 13 dos 15 arquivos originais) já errou duas vezes nesta mesma sessão (uma vez na spec funcional original, outra agora na spec técnica), então o passo de verificação precisa ser mecânico/repetível, não confiar em releitura humana.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — §2/§4/§8 da spec técnica expandidos com os 7 pontos (2 `UseActiveRagdolls` + 2 `EnableBloodEffects` em `KillPatch.cs`; `BleedPatch.cs` corrigido de 3 pra 4 ocorrências; `RagdollHelperClass.cs` adicionado como arquivo novo; `OnlyPlayersCanActiveRagdollEnemies`/`ArterySpray` documentados como cobertos transitivamente). Passo de verificação por grep adicionado como primeiro item do checklist §8.

---

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-20

**Capacete/óculos podem cair sem o efeito visual de desmembramento correspondente — falta justificativa explícita**

**Problema:** A spec técnica (§1, "Achado que resolve o corner case...") remove a checagem `EnableDismemberment.Value` de `ResolveAndDropHeadEquipment`, deixando a queda de capacete/óculos condicionada só a `DropHeadEquipmentOnDismemberment.Value`. Isso significa: com o toggle mestre desligado (e portanto `EnableDismemberment` efetivamente inativo via `IsCategoryActive`), um bot pode ter a cabeça "desmembrada" pra fins de lógica interna (`ResolveHeadOutcome` já roda incondicionalmente) e **perder o capacete/óculos**, mas o efeito visual de desmembramento (`KillPatch.Postfix` encolhendo a cabeça) **não roda** — o jogador vê uma cabeça visualmente intacta, mas o capacete simplesmente caiu no chão sem explicação visual aparente.

**Por que importa:** É um comportamento plausível de acontecer toda vez que o cenário "mestre desligado + toggle de capacete ligado + tiro de cabeça que rolaria desmembramento" ocorrer — não é um edge case raro. Pode parecer um bug pro jogador ("por que o capacete caiu sozinho?"), mesmo não sendo tecnicamente incorreto pra quem projetou a feature.

**Sugestão:** A spec provavelmente já tem uma boa resposta pra isso (o mecanismo `ShootOffHelmetPatch` já derruba capacete sem nenhum desmembramento visual, então "capacete cai sem cabeça mudar" já é um padrão aceito neste mod) — mas essa justificativa **não está escrita na spec técnica**, só existe implicitamente. Adicionar 2-3 frases ao final da seção "Achado que resolve o corner case..." em §1, citando `ShootOffHelmetPatch.cs` como precedente já existente de "queda de capacete sem efeito visual de cabeça", deixando explícito que esse comportamento já é aceito pelo mod hoje e não é uma inconsistência nova introduzida por este item.

**Decisão:**
- `[x]` Aceitar com modificação: justificativa mais forte que a sugerida — não é (só) precedente de UX (`ShootOffHelmetPatch`), é a mesma exigência de sincronização de dado entre peers do `CR-NET-LOCK-01`. Se a queda ficasse condicionada ao toggle de quem executa, o item ficaria removido pra quem tem a feature ativa mas ainda presente no inventário do cadáver pra quem não tem — o mesmo bug de "item em 2 lugares" que motivou toda a reformulação do item 002. Vale pra arma e capacete/óculos igualmente, não só capacete.

**Resolução:** §1 da spec técnica reescrita com essa justificativa (parágrafo "Por que a queda de arma E de capacete/óculos precisam ficar fora do toggle mestre"), citando explicitamente os dois itens (arma e capacete) e o precedente de `CR-NET-LOCK-01`.

---

### PA-01-03 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-20

**Setup de colisão de camada física em `GameStartedPatch` não tem decisão explícita sobre a Camada 1**

**Problema:** `VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs:48-67` configura `Physics.IgnoreLayerCollision` com base em `BodyCollision.Value` e `UseActiveRagdolls.Value` — mas isso roda **uma única vez**, no início da raid (`GameWorld.OnGameStarted`), não por evento recorrente como os demais patches da Camada 1. A spec técnica não decide explicitamente se esse bloco entra na Camada 1 (ganha `IsCategoryActive`) ou fica de fora (raciocínio: já que só é lido uma vez por raid, alternar o toggle mestre no meio da raid não teria efeito nele de qualquer forma — diferente da promessa de "troca em tempo real" que vale pro resto da Camada 1).

**Por que importa:** Sem uma decisão explícita, quem implementar pode (a) esquecer completamente este bloco (mesma classe de erro do `PA-01-01`), ou (b) aplicar o gate mecanicamente sem perceber que aqui ele só afeta o **próximo** início de raid, não a raid atual — potencialmente surpreendente se não documentado.

**Sugestão:** Adicionar uma frase a §1 ou §7 explicitando: como este setup só roda uma vez por `OnGameStarted`, ele também recebe `IsCategoryActive` nos 2 pontos (`:52`, `:62` — `UseActiveRagdolls`) e no `BodyCollision` (`:48`, se aplicável à mesma lógica), mas com a ressalva de que — ao contrário do resto da Camada 1 — alternar o toggle mestre em tempo real só afeta esse comportamento específico na **próxima raid**, não na atual. Isso é uma exceção pequena e aceitável à regra geral de "troca em tempo real" já que decorre da natureza do próprio hook do EFT (`OnGameStarted` só dispara uma vez), não de uma limitação evitável do design deste item.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — nova linha em §2/§4/§8 pro setup de colisão de `GameStartedPatch` (`:48`, `:52`, `:62`), com a ressalva explícita de que só afeta a próxima raid, não a atual.

---

## Status

✅ **Pronta para `/code-mod`** — os 3 pontos desta rodada foram resolvidos. Spec técnica atualizada com as correções de `PA-01-01` (7 pontos faltando em §2/§4/§8), `PA-01-02` (justificativa de §1 reescrita com a razão correta — sincronização de dado, não UX, e explicitamente estendida pra arma) e `PA-01-03` (setup de colisão de `GameStartedPatch` decidido).
