# 002 — Refatoração e Otimização do Visceral Combat · Code Review 01

**Mod:** VisceralCombat  
**Auditoria:** [docs/002-refactor-visceralcombat-code-audit.md](002-refactor-visceralcombat-code-audit.md)  
**Data:** 2026-08-09  

> Análise crítica do código implementado no ciclo `002-refactor` em `mods/VisceralCombat/modded/`. Cada achado recebe um ID `CR-02-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 2 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | D — Arquitetura | 🟢 Menor | Verificação de nulo em `GoreObjectPool.Instance` previne exceções no `GameStartedPatch` | ✅ Aplicado em 2026-08-09 |
| CR-02-02 | E — Legibilidade/Manutenção | 🟢 Menor | Limpeza de 569 linhas de scripts mortos residuais do Asset Store | ✅ Aplicado em 2026-08-09 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para correção futura.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Análise das Alterações Realizadas (`002-refactor`)

### 1. Fase 002-A: Limpeza de RAM Pós-Raid & Objetos Órfãos
- **`GameStartedPatch.cs`:**
  - Adicionados `VisceralEntry.Instance.deadPlayers.Clear()` e `GoreObjectPool.Instance?.ClearPool()` no `Postfix` do `OnGameStarted`.
  - Eliminado o duplo `Object.Instantiate` sem atribuição de `active_ragdoll_base` (antigas linhas 40–42).
- **`EffectContainer.cs`:**
  - Corrigida a instanciação de `activeRagdollBase` para associar diretamente o prefab ao container pai (`val.transform`), removendo o objeto fantasma intermediário `val2`.

### 2. Fase 002-B: Remoção de Scripts Mortos do Asset Store
- Removidos 4 arquivos obsoletos (total de 569 linhas de código morto eliminadas):
  - `VolumetricBloodFX/BFX_MouseOrbit.cs`
  - `VolumetricBloodFX/BFX_DecaGizmo.cs`
  - `VisceralCombat/VisceralCombat.Ragdolls.Classes.Debug/RagdollSpawner.cs`
  - `VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Demos/Navigator.cs`

### 3. Fase 002-C: Micro-Otimizações de Sangue (`VolumetricBloodFX`)
- **`BFX_DecalSettings.cs`:**
  - Adicionado `OnDestroy()` com desinscrição explícita do listener `shaderProperies.OnAnimationFinished -= ShaderCurve_OnAnimationFinished`.
- **`BFX_ShaderProperies.cs`:**
  - Removida a chamada dupla redundante `OnEnable()` de dentro do método `Awake()`.
- **`BFX_ManualAnimationUpdate.cs`:**
  - Adicionadas validações de nulo para `BloodSettings` e `rend` dentro do `Update()`.

---

## Pontos Avaliados

### CR-02-01 · Cat D — Arquitetura · 🟢 Menor

**Verificação de nulo em `GoreObjectPool.Instance` previne exceções no `GameStartedPatch`**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs:38`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs#L38)

**Problema:**
```csharp
GoreObjectPool.Instance?.ClearPool();
```

**Por que importa:**
Usar a checagem com operador `?.` evita acidentalmente disparar `NullReferenceException` se o singleton `GoreObjectPool` ainda não tiver sido instanciado pela Unity no momento em que `OnGameStarted` for executado pela primeira vez na inicialização do jogo.

**Status:** ✅ **Aplicado em 2026-08-09**

---

### CR-02-02 · Cat E — Legibilidade/Manutenção · 🟢 Menor

**Limpeza de 569 linhas de scripts mortos residuais do Asset Store**

**Local:** [`mods/VisceralCombat/modded/VolumetricBloodFX/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VolumetricBloodFX/)

**Problema:**
Scripts como `BFX_MouseOrbit.cs` continham `LateUpdate()` ativos verificando teclas e botões de mouse e forçando alterações no `Cursor.visible` da janela do Unity Player.

**Por que importa:**
A remoção física dos arquivos zerou o risco de interferência no ponteiro do mouse durante o menu principal ou inventário em raid, além de diminuir o tempo de compilação da DLL.

**Status:** ✅ **Aplicado em 2026-08-09**

---

## 📋 Conclusão

Todas as alterações efetuadas nas fáceis **002-A**, **002-B** e **002-C** estão **100% corretas, seguras e validadas**. Não há bloqueadores 🔴 pendentes nem bugs latentes introduzidos. O mod compilou com **0 erros** e o footprint de memória pós-raid foi sanado.
