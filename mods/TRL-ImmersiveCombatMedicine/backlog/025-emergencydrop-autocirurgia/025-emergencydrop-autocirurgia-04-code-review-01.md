# 025 — EmergencyDrop na cirurgia própria (self-heal) · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [025-emergencydrop-autocirurgia-01-spec.md](025-emergencydrop-autocirurgia-01-spec.md)
**Spec técnica:** [025-emergencydrop-autocirurgia-02-spec-tech.md](025-emergencydrop-autocirurgia-02-spec-tech.md)
**Asbuild:** [025-emergencydrop-autocirurgia-05-asbuild.md](025-emergencydrop-autocirurgia-05-asbuild.md)
**Data:** 2026-09-12

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

Memória consultada: `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`, snapshot de 2026-09-12 (Sessão 13, escrita nesta mesma sessão). Nenhuma pendência 🔴 afeta este item. Pendência 🟡 [P-13.2] (validação in-game) já cobre o caminho feliz — nenhum dos achados abaixo é bloqueador pra ela.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F — Melhoria opcional | 🟡 Médio | `ItemDatabase.GetStats` recalculado todo frame enquanto QUALQUER item médico está nas mãos, não só cirurgia | ✅ Aplicado em 2026-09-12 |
| CR-01-02 | D — Arquitetura | 🟢 Menor | Gate de auto-cirurgia depende de `_isHealingInProgress` ficar sempre sincronizado com `MedicHealPatch.BandAidHealActive`, sem referenciar o segundo diretamente | ✅ Aplicado em 2026-09-12 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa um critério de aceite, corner case ou AC de verificação manual da spec funcional/técnica.
- **D — Arquitetura** — viola padrões do repo, duplica código existente, abuso de reflection, leak de estado entre raids.
- **E — Legibilidade/manutenção** — nomes ruins, falta de comentário onde o "porquê" é obscuro, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade de vida, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · F — Melhoria opcional · 🟡 Médio · ✅ Aplicado em 2026-09-12

**`ItemDatabase.GetStats` recalculado todo frame enquanto QUALQUER item médico está nas mãos, não só cirurgia**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs:180-183`](../../modded-V4/Patches/Medical/BandAidController.cs#L180-L183)

**Problema:**
```csharp
var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
bool selfSurgeryInProgress = !_isHealingInProgress
    && mainPlayer.HandsController is Player.MedsController selfMeds
    && (ItemDatabase.GetStats(selfMeds.Item.TemplateId.ToString())?.IsSurgery ?? false);
```
Essa lógica roda em **todo `Update()`**, sem cache. Enquanto o jogador segura QUALQUER item médico (não só CMS/Surv12 — bandagem, tala, torniquete, medkit comum contam igual, já que o `is Player.MedsController` casa com todos eles), esse trecho recalcula `ItemDatabase.GetStats(...)` a cada frame, incluindo `TemplateId.ToString()` (`Item.cs:503`, `MongoID` → `string`) todo frame. O padrão já estabelecido no mesmo arquivo pra cura de aliado (`_currentHealIsSurgery`) computa isso **uma única vez**, no início de `HealRoutine`, e reusa o valor cacheado em todo `Update()` seguinte — este trecho novo não segue o mesmo padrão.

**Por que importa:** Curar-se com bandagem/tala é rápido (poucos segundos) mas frequente — em raids com trauma pesado, o jogador pode passar boa parte do tempo com algum item médico nas mãos. Recalcular uma consulta de dicionário + conversão de tipo todo frame nesse cenário é exatamente o tipo de alocação/trabalho redundante em `Update()` que este mesmo mod já identificou e corrigiu antes (ex.: `TourniquetManager.Update()`, alocação de `List<EBodyPart>` por frame, corrigida com buffer estático reusável). Não é um bug funcional — é uma reintrodução do mesmo padrão de ineficiência que o mod já tem disciplina de evitar.

**Sugestão:** Cachear o resultado por referência de item, recalculando só quando o item nas mãos muda — mesmo espírito do buffer estático do `TourniquetManager`:
```csharp
// campos da classe
private Item _lastSelfMedsItemChecked = null;
private bool _lastSelfMedsIsSurgery = false;

// dentro de Update(), no lugar do trecho atual
bool selfSurgeryInProgress = false;
if (!_isHealingInProgress && mainPlayer.HandsController is Player.MedsController selfMeds)
{
    if (!ReferenceEquals(selfMeds.Item, _lastSelfMedsItemChecked))
    {
        _lastSelfMedsItemChecked = selfMeds.Item;
        _lastSelfMedsIsSurgery = ItemDatabase.GetStats(selfMeds.Item.TemplateId.ToString())?.IsSurgery ?? false;
    }
    selfSurgeryInProgress = _lastSelfMedsIsSurgery;
}
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada com uma pequena adaptação: os campos de cache (`_lastSelfMedsItemChecked`/`_lastSelfMedsIsSurgery`) foram declarados junto de `_currentHealIsSurgery` (topo da classe) em vez de soltos, e resetados em `ResetAllState()` pra não segurar referência de `Item` de uma raid anterior.

**Aplicação:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs`](../../modded-V4/Patches/Medical/BandAidController.cs) — `Update()` agora só chama `ItemDatabase.GetStats` quando o item nas mãos muda (`ReferenceEquals`); reset dos campos de cache adicionado em `ResetAllState()`. `dotnet build -c Release`: 0 Erros, 0 Warnings.

---

### CR-01-02 · D — Arquitetura · 🟢 Menor · ✅ Aplicado em 2026-09-12

**Gate de auto-cirurgia depende de `_isHealingInProgress` ficar sempre sincronizado com `MedicHealPatch.BandAidHealActive`, sem referenciar o segundo diretamente**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs:181`](../../modded-V4/Patches/Medical/BandAidController.cs#L181) vs. [`MedicHealPatch.cs:340-347`](../../modded-V4/Patches/Medical/MedicHealPatch.cs#L340-L347)

**Problema:** O isolamento aliado×self (spec funcional, corner case validado pelo usuário) depende de DOIS flags booleanos independentes, em duas classes diferentes, nunca se referenciando: `BandAidController._isHealingInProgress` (usado no gate novo, linha 181) e `MedicHealPatch.BandAidHealActive` (usado no guard G5 que bloqueia o self-heal nativo do médico durante redirect, `MedicHealPatch.cs:342`). Confirmei por leitura completa do arquivo que hoje os dois são sempre setados/resetados em conjunto (diretamente, ou indiretamente via `ForceFinishAnimation()`, que zera `BandAidHealActive` internamente) — não há bug ativo agora.

**Por que importa:** É uma dependência implícita, não documentada em nenhum dos dois pontos: se um cleanup futuro (ex.: um novo corner case do item de morte simultânea, já citado na spec funcional) resetar `_isHealingInProgress` sem passar por um caminho que também zera `BandAidHealActive` (ou vice-versa), o gate de auto-cirurgia (`Update()`) e o guard G5 nativo (`MedicHealPatch.cs`) passariam a discordar sobre se uma cura de aliado está ativa — reabrindo exatamente o cenário de double-heal que o guard G5 foi desenhado pra evitar, silenciosamente, sem nenhum teste unitário ou assert acusando a dessincronia.

**Sugestão:** Adicionar um comentário no gate novo (`BandAidController.cs:181`) apontando explicitamente essa dependência: `// invariante: _isHealingInProgress deve espelhar MedicHealPatch.BandAidHealActive (guard G5, MedicHealPatch.cs:342) — qualquer novo caminho de cleanup precisa zerar os dois`. Alternativa mais robusta (fora do escopo mínimo deste achado, registrar como ideia): usar `MedicHealPatch.BandAidHealActive` diretamente no gate de `Update()` em vez de `_isHealingInProgress`, eliminando a duplicação de estado.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Comentário adicionado exatamente no gate de `Update()`, apontando a invariante entre `_isHealingInProgress` e `MedicHealPatch.BandAidHealActive` (guard G5). A alternativa mais robusta (usar `BandAidHealActive` diretamente) ficou registrada como ideia, não aplicada nesta rodada.

**Aplicação:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs:174-180`](../../modded-V4/Patches/Medical/BandAidController.cs#L174-L180) — comentário do bloco `EMERGENCY DROP` expandido.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Code review 01 criada via `/code-review`. 0 🔴, 1 🟡 (`GetStats` sem cache por frame), 1 🟢 (dependência implícita entre `_isHealingInProgress` e `BandAidHealActive`). |
| 2026-09-12 | Aplicação automática de 2 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02. |
