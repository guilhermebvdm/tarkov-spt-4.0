# 013 — Desfibrilador: consumo trava o inventário · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [013-desfibrilador-consumo-inventario-01-spec.md](013-desfibrilador-consumo-inventario-01-spec.md)
**Spec técnica:** [013-desfibrilador-consumo-inventario-02-spec-tech.md](013-desfibrilador-consumo-inventario-02-spec-tech.md)
**Data:** 2026-07-26

> Análise crítica do código implementado. Cada achado recebe um ID `CR-01-MM` permanente.

**Desvio de gate declarado:** este item não passou por `/review-technical-spec` nem gerou `05-asbuild.md`. Decisão consciente: fix pontual, causa-raiz provada no Assembly antes de escrever código, e o padrão de correção já validado em teste de 2 PCs no próprio mod. Os arquivos tocados estão na §3 da spec técnica e conferem com o diff.

**Memória consultada:** snapshot de 2026-07-25 (Sessão 5) · pendências que afetam: **[P-4.4]** (nada do overhaul validado in-game) — este item nasce justamente do 1º teste que atacou essa pendência. A memória registra em P-2.14/P-2.15 exatamente o modo de falha que este item corrige, no sistema de cura; a lição não havia sido propagada para o revive.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Aplicados: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟠 Forte | Plugin não declara SoftDependency do Fika, mas resolve tipos do Fika por nome | ✅ Aplicado |
| CR-01-02 | E — Legibilidade/manutenção | 🟡 Médio | Reflection do `ReviveInteractable` refeita a cada revive em vez de cacheada | ✅ Aplicado |
| CR-01-03 | B — Bug latente | 🟡 Médio | `FikaReviveGetActionsPatch.Postfix` sem try/catch derruba o prompt de interação inteiro | ✅ Aplicado |

---

### CR-01-01 · D — Arquitetura · 🟠 Forte

**Plugin não declara SoftDependency do Fika, mas resolve tipos do Fika por nome**

**Local:** [`modded/TRLImmersiveCombatMedicinePlugin.cs:16`](../../modded/TRLImmersiveCombatMedicinePlugin.cs#L16)

**Problema:** o plugin declara `[BepInDependency("xyz.drakia.bigbrain", SoftDependency)]` mas **não** declara nada sobre o Fika — apesar de `FikaRevivePatch` e `BandAidNetworkHandler` resolverem tipos do Fika via `AccessTools.TypeByName("Fika.Core.Main.Components.ReviveInteractable")`. Sem a dependência declarada, a ordem de carga do BepInEx é indeterminada.

**Por que importa:** se este plugin subir antes do Fika, `TypeByName` devolve `null`, `TargetMethod()` devolve `null` e os patches de revive são dispensados. O gate de desfibrilador e o consumo simplesmente não existiriam, **em silêncio**. Funcionou até hoje por acidente de ordenação, não por garantia. Outros mods do repo já fazem isso certo (DiscordRaidMap, MOAR-Client declaram `com.fika.core`).

**Sugestão:** adicionar `[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** aplicado. GUID confirmado em `references/fika-plugin/Fika.Core/FikaPlugin.cs:40`.

---

### CR-01-02 · E — Legibilidade/manutenção · 🟡 Médio

**Reflection do `ReviveInteractable` refeita a cada revive em vez de cacheada**

**Local:** [`modded/Patches/Trauma/FikaRevivePatch.cs`](../../modded/Patches/Trauma/FikaRevivePatch.cs) — `Prefix` e os dois `TargetMethod()`

**Problema:** o `Prefix` chamava `AccessTools.TypeByName(...)` + `AccessTools.Field(...)` a cada execução, e os dois `TargetMethod()` repetiam o mesmo `TypeByName` com a string literal duplicada. O fix deste item **piorou** o quadro ao adicionar um terceiro lookup (`_observedPlayer`) no mesmo caminho.

**Por que importa:** `csharp-mod-best-practices` §3 e item 4 do checklist mandam cachear todo `MethodInfo`/`FieldInfo` em `static readonly`. Não é hot path (1× por revive), então o custo é irrelevante — o que importa é a string do FQN duplicada em 3 lugares: uma renomeação no Fika passaria a falhar em pontos diferentes, em momentos diferentes, com sintomas diferentes.

**Sugestão:** extrair um `FikaReviveReflection` estático com `Type`, `LocalPlayerField` e `ObservedPlayerField` resolvidos uma vez, e apontar os dois `TargetMethod()` e o `Prefix` para ele.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** aplicado. Uma única const de FQN, três lookups resolvidos em inicialização estática, com guarda de `Type != null` para não lançar quando o Fika está ausente.

---

### CR-01-03 · B — Bug latente · 🟡 Médio

**`FikaReviveGetActionsPatch.Postfix` sem try/catch derruba o prompt de interação inteiro**

**Local:** [`modded/Patches/Trauma/FikaRevivePatch.cs`](../../modded/Patches/Trauma/FikaRevivePatch.cs) — `FikaReviveGetActionsPatch.Postfix`

**Problema:** o corpo não tinha try/catch, contrariando o item 6 do checklist de `csharp-mod-best-practices`. `HasDefibrillator` chama `Profile.Inventory.GetAllItemByTemplate` — inventário lido num frame de transição pode lançar.

**Por que importa:** a exceção propagaria para `ReviveInteractable.GetActions` do Fika, que constrói a lista de ações do prompt. O jogador perderia o prompt **inteiro** (inclusive "Search"), não só a ação de revive — e o sintoma apareceria como "não consigo interagir com o corpo", longe da causa. O patch irmão (`FikaRevivePlayerPatch`) já tinha essa proteção desde o CR-01-04 do review antigo; este ficou sem.

**Sugestão:** envolver em try/catch com log, e aproveitar para checar `owner == null`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** aplicado, com log em nível de erro nomeando a consequência (as ações seguem sem o gate). Verificado na fonte do Fika que `Name = "Search"` é literal hardcoded (`ReviveInteractable.cs:176`), não localizado — a comparação por nome é estável em qualquer idioma, então **não** foi levantada como achado; documentada em comentário para não ser "corrigida" por engano depois.

---

## Não-achados (verificados e descartados)

Registrados para não voltarem como falso positivo numa próxima rodada:

- **`owner` null em `GetActions`** — o próprio Fika dereferencia `owner.Player` em `ReviveInteractable.cs:163`, antes do nosso postfix; se fosse null quebraria lá primeiro. O check foi adicionado como defensivo junto do try/catch, mas não é um bug real.
- **`items.First()` com LINQ** — não é hot path (1× por revive) e o checklist restringe LINQ a patches per-frame/per-tick.
- **Item empilhável** — `Discard` removeria o stack inteiro em vez de 1 unidade, enquanto o vanilla usaria `SplitToNowhere` (`GClass3017.cs:7-35`). O desfibrilador não empilha, e tratar aqui divergiria do caminho da cura, que tem a mesma característica. Registrado na §4 da spec técnica como consciente.
- **`ObservedPlayer` sobrescrevendo `SetupHitColliders`** (AP-03) — verificado: nenhum tipo na cadeia `ObservedPlayer → FikaPlayer → LocalPlayer → Player` sobrescreve. Relevante para o item irmão no `TRL-Fixes`.

## Verificação pendente

Nenhum achado bloqueia o fechamento. O item só é confirmado pelo cenário **C1** do [roteiro happy-flow](../../docs/happy-flow-test-plan.md), nos dois papéis (reanimador host e reanimador client), porque o defeito original só aparece em jogo.
