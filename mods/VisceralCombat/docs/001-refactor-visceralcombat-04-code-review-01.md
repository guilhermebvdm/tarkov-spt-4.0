# 001 — Refatoração e Otimização do Visceral Combat · Code Review 01

**Mod:** VisceralCombat  
**Roadmap de Refatoração:** [docs/refactor-roadmap.md](refactor-roadmap.md)  
**Data:** 2026-08-07  

> Análise crítica do código implementado em `mods/VisceralCombat/modded/`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar a tarefa.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🔴 Bloqueador | Callback assíncrono em `WaitSeconds` pode causar `NullReferenceException` pós-raid | ✅ Aplicado em 2026-08-07 |
| CR-01-02 | E — Desempenho/Manutenção | 🟠 Forte | Invocação de `SupportRigidbody` via Reflection a cada collider de ragdoll sem cache | ✅ Aplicado em 2026-08-07 |
| CR-01-03 | B — Bug Latente | 🟠 Forte | Vazamento de memória nativa por falta de `Destroy` em `AnimatorOverrideController` | ✅ Aplicado em 2026-08-07 |
| CR-01-04 | C — Gap vs. Spec | 🟡 Médio | `GoreObjectPool` criado mas não integrado ao `KillPatch` / `BleedPatch` | ✅ Aplicado em 2026-08-07 |
| CR-01-05 | F — Melhoria Opcional | 🟢 Menor | Uso da propriedade obsoleta `ParticleSystem.loop` | ✅ Aplicado em 2026-08-07 |

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

## Pontos

### CR-01-01 · Cat D — Arquitetura · 🔴 Bloqueador

**Callback assíncrono em `WaitSeconds` pode causar `NullReferenceException` pós-raid**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs:478-481`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L478-L481) e [`KillPatch.cs:564-571`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L564-L571)

**Problema:**
```csharp
GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, sleepDelay, (Action)delegate
{
    if (componentInChildren != null)
    {
        componentInChildren.mode = PuppetMaster.Mode.Kinematic;
        ((Behaviour)componentInChildren).enabled = false;
    }
});
```
O método `GClass855.WaitSeconds` é acoplado ao `StaticManager.Instance`, que é um singleton persistente mantido durante todo o ciclo de vida do jogo (inclusive no menu principal e no carregamento de raids). Se o jogador morrer e retornar ao menu ou encerrar a raid antes do tempo `sleepDelay` ou `num + 1f` expirar, o delegate agendado ainda será executado, tentando acessar instâncias de `PuppetMaster` ou `GameObject` já destruídos pela Unity.

**Por que importa:**
Gera erros de `NullReferenceException` e exceções silenciosas no console BepInEx ao sair da raid.

**Sugestão:**
Verificar se o objeto Unity não foi destruído e se o mundo do jogo ainda está ativo antes de executar as ações:
```csharp
GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, sleepDelay, (Action)delegate
{
    if (componentInChildren != null && ((Component)componentInChildren).gameObject != null && Singleton<GameWorld>.Instantiated)
    {
        componentInChildren.mode = PuppetMaster.Mode.Kinematic;
        ((Behaviour)componentInChildren).enabled = false;
    }
});
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:**
- **Status:** ✅ Aplicado em 2026-08-07
- **Aplicação:** Adicionadas checagens de `Singleton<GameWorld>.Instantiated` e validações de não-nulo em todos os callbacks diferidos de `KillPatch.cs` e `BleedPatch.cs`.

---

### CR-01-02 · Cat E — Desempenho/Manutenção · 🟠 Forte

**Invocação de `SupportRigidbody` via Reflection a cada collider de ragdoll sem cache**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs:204-205`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs#L204-L205)

**Problema:**
```csharp
MethodInfo supportMethod = typeof(Player).Assembly.GetTypes().FirstOrDefault(t => t.GetMethod("SupportRigidbody") != null)?.GetMethod("SupportRigidbody");
supportMethod?.Invoke(null, new object[] { val4, 0f, null });
```
A busca iterativa por todos os tipos de `Assembly-CSharp` (`GetTypes().FirstOrDefault(...)`) ocorre dentro do laço `for (int j = 0; j < rigidbodySpawner_.Length; j++)` para cada corpo rígido do ragdoll.

**Por que importa:**
Executar varredura de assemblies via Reflection em um loop de inicialização de ragdoll gera centenas de milissegundos de travamento de CPU e aloca megabytes de lixo na memória Garbage Collector (GC).

**Sugestão:**
Armazenar o `MethodInfo` em um campo estático privado pré-inicializado:
```csharp
private static readonly MethodInfo _supportRigidbodyMethod = typeof(Player).Assembly.GetTypes()
    .FirstOrDefault(t => t.GetMethod("SupportRigidbody") != null)?.GetMethod("SupportRigidbody");
```
E no loop utilizar apenas: `_supportRigidbodyMethod?.Invoke(null, new object[] { val4, 0f, null });`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:**
- **Status:** ✅ Aplicado em 2026-08-07
- **Aplicação:** Criado o campo estático `_supportRigidbodyMethod` em `RagdollClassPatch.cs` e reutilizado no loop de corpos rígidos do ragdoll.

---

### CR-01-03 · Cat B — Bug Latente · 🟠 Forte

**Vazamento de memória nativa por falta de `Destroy` em `AnimatorOverrideController`**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs:525-526`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L525-L526)

**Problema:**
```csharp
AnimatorOverrideController runtimeAnimatorController = new AnimatorOverrideController(p.BodyAnimatorCommon.runtimeAnimatorController);
p.BodyAnimatorCommon.runtimeAnimatorController = (RuntimeAnimatorController)(object)runtimeAnimatorController;
```
Uma nova instância de `AnimatorOverrideController` herda de `UnityEngine.Object`. Atribuir uma nova instância ao animator sobrescreve o ponteiro sem destruir o recurso C++ nativo anterior.

**Por que importa:**
A cada eliminação em raid, a instância nativa anterior continua alocada na memória C++ da Unity, gerando vazamento progressivo de RAM.

**Sugestão:**
Verificar se `p.BodyAnimatorCommon.runtimeAnimatorController` já é um `AnimatorOverrideController` e destruí-lo antes da reatribuição:
```csharp
if (p.BodyAnimatorCommon.runtimeAnimatorController is AnimatorOverrideController oldOverride)
{
    UnityEngine.Object.Destroy(oldOverride);
}
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:**
- **Status:** ✅ Aplicado em 2026-08-07
- **Aplicação:** Adicionada a destruição com `UnityEngine.Object.Destroy(oldOverride)` antes de atribuir o novo `AnimatorOverrideController` no `KillPatch.cs`.

---

### CR-01-04 · Cat C — Gap vs. Spec · 🟡 Médio

**`GoreObjectPool` criado mas não integrado ao `KillPatch` / `BleedPatch`**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/GoreObjectPool.cs) e [`KillPatch.cs:450-475`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L450-L475)

**Problema:**
O item 3.1 do roadmap previa a utilização de Object Pooling para os borrifos de sangue e gore. O arquivo `GoreObjectPool.cs` foi criado no projeto, mas `KillPatch.cs` e `BleedPatch.cs` continuam instanciando e destruindo borrifos com `Object.Instantiate` / `Object.Destroy`.

**Por que importa:**
Instanciações frequentes durante tiroteios intensos geram picos de Garbage Collector.

**Sugestão:**
Conectar as chamadas de `GoreObjectPool.Rent` e `GoreObjectPool.Return` dentro dos métodos `SpawnArterialSprays` e `HitEffect`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:**
- **Status:** ✅ Aplicado em 2026-08-07
- **Aplicação:** Integrado `GoreObjectPool.Instance.Spawn` e `GoreObjectPool.Instance.Recycle` nos métodos `SpawnArterialSprays` de `KillPatch.cs` e `HitEffect` / `BleedEffect` de `BleedPatch.cs`.

---

### CR-01-05 · Cat F — Melhoria Opcional · 🟢 Menor

**Uso da propriedade obsoleta `ParticleSystem.loop`**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs:469`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L469) e [`BleedPatch.cs:340`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs#L340)

**Problema:**
`val.loop = false;` gera o aviso de compilação `CS0618: ParticleSystem.loop é obsoleto`.

**Por que importa:**
Mantém o código livre de avisos de depreciação nas versões mais recentes da API Unity.

**Sugestão:**
Usar `main.loop = false;` aproveitando a variável `main` obtida em `val.main`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:**
- **Status:** ✅ Aplicado em 2026-08-07
- **Aplicação:** Substituído `val.loop = false` por `main.loop = false` em `KillPatch.cs` e `BleedPatch.cs`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-07 | Code review 01 criada via `/code-review` |
| 2026-08-07 | Todos os 5 achados resolvidos e aplicados via plano de implementação aprovado |
