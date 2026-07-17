# 001 — TarkovIRL General Code Review · Code Review 01

**Mod:** TarkovIRL-SPT4.0-beta
**Data:** 2026-07-17

> Análise crítica do código do mod TarkovIRL. Cada achado recebe um ID `CR-01-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 2 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🔴 Bloqueador | Vazamento de Estado Estático (Raid Leaks) | `[ ]` Pendente |
| CR-01-02 | D — Arquitetura | 🔴 Bloqueador | Acoplamento Direto com Interface Ofuscada (`GInterface381`) | `[ ]` Pendente |
| CR-01-03 | E — Legibilidade/manutenção | 🟠 Forte | Código Morto / Duplicado em `NewSwayController` e `SwayController` | `[ ]` Pendente |
| CR-01-04 | E — Legibilidade/manutenção | 🟡 Médio | Alocações desnecessárias na chamada de log do FixedUpdate | `[ ]` Pendente |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · D — Arquitetura · 🔴 Bloqueador

**Vazamento de Estado Estático (Raid Leaks)**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/FreeAimController.cs:8-13`](../../modded/FreeAimController.cs#L8-L13), [`mods/TarkovIRL-SPT4.0-beta/EfficiencyController.cs:37-47`](../../modded/EfficiencyController.cs#L37-L47)

**Problema:**
O mod usa classes estáticas e campos estáticos para gerenciar estados importantes durante uma raid:
* `FreeAimController.Offset` (desvio de mira atual).
* `EfficiencyController._injuryTimes` (dicionário de lesões e tempos).
* `NewSwayController._laggingSwayPoses` e `_laggingSwayRots` (arrays de histórico de sways).
* `ThrowController._isThrowing` e `_throwLerp` (estados de arremesso).

O mod não implementa um gancho de encerramento de raid (`OnDestroy` do `GameWorld` ou `BaseLocalGame.Stop`). Por isso, esses estados **não são resetados** quando o jogador sai ou morre em uma raid e inicia outra.

```csharp
// Exemplo em EfficiencyController.cs
private static Dictionary<Injury, float> _injuryTimes = new Dictionary<Injury, float>();
```

**Por que importa:**
Se o jogador extrair com a mira torta, com os braços tremendo ou sob o efeito de ferimentos e imediatamente iniciar outra raid, os ferimentos anteriores e o deslocamento de mira vão transbordar para a nova raid, iniciando o jogo com cálculos incorretos de eficiência física ou movimentação da arma instável.

**Sugestão:**
Implementar um método estático `Reset()` em cada um desses controladores e criar ganchos de limpeza de ciclo de vida (`OnDestroy` no `GameWorld` do Tarkov ou similar) no script principal `PrimeMover.cs` para invocar esses resets a cada fim/início de sessão.

---

### CR-01-02 · D — Arquitetura · 🔴 Bloqueador

**Acoplamento Direto com Interface Ofuscada (`GInterface381`)**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/EfficiencyController.cs:138-141`](../../modded/EfficiencyController.cs#L138-L141)

**Problema:**
O código realiza conversões diretas de tipo (*casts*) para interfaces ofuscadas da engine do Tarkov, tais como:
```csharp
ValueStruct hydration = ((GInterface381) player.HealthController).Hydration;
ValueStruct energy = ((GInterface381) player.HealthController).Energy;
```

**Por que importa:**
Nomes como `GInterface381` são gerados aleatoriamente pelo ofuscador da BSG durante a compilação do jogo. Qualquer pequena atualização de micro-patch ou hotfix do Tarkov reordenará essas interfaces, gerando uma `TypeLoadException` ou `InvalidCastException` instantânea, quebrando todo o mod no carregamento da raid.

**Sugestão:**
Acessar esses dados através do tipo concreto que implementa o `HealthController` do jogador ou buscar via reflexão dinâmica baseada em assinatura de propriedades (`Hydration` e `Energy`), o que garante a durabilidade e compatibilidade do mod ao longo dos updates do jogo.

---

### CR-01-03 · E — Legibilidade/manutenção · 🟠 Forte

**Código Morto / Duplicado em `NewSwayController` e `SwayController`**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/SwayController.cs`](../../modded/SwayController.cs)

**Problema:**
O mod possui dois controladores de sway diferentes: `SwayController.cs` e `NewSwayController.cs`. No entanto, todo o processamento dinâmico complexo e rotações estão sendo feitos em `NewSwayController.cs`. O `SwayController.cs` antigo apenas atualiza um `_addedSwayLerp` que não parece estar sendo integrado ativamente nos movimentos modernos da arma do mod.

**Por que importa:**
Isso incha o mod com arquivos redundantes, confunde os desenvolvedores sobre qual lógica de sway está ativa e mantém ganchos inúteis no `LateUpdate` rodando a cada frame.

**Sugestão:**
Remover ou consolidar completamente o `SwayController.cs` antigo, mantendo apenas o `NewSwayController.cs` ativado.

---

### CR-01-04 · E — Legibilidade/manutenção · 🟡 Médio

**Alocações desnecessárias na chamada de log do FixedUpdate**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/PrimeMover.cs:391`](../../modded/PrimeMover.cs#L391)

**Problema:**
A interpolação de strings dentro do método de logging ocorre independentemente de o arquivo de log estar sendo gravado ou não:
```csharp
TIRLUtils.LogError($"fixed deltaTime exceeds limits : {this.FixedDeltaTime} / 0.01666 -- movement updates skipped this frame");
```

**Por que importa:**
Em métodos executados repetidamente no ciclo de frames (`FixedUpdate`/`LateUpdate`), a concatenação/interpolação aloca lixo (Garbage Collection) desnecessário na heap, o que contribui para micro-engasgos visuais durante a jogabilidade intensa.

**Sugestão:**
Substituir a chamada interpolada por mensagens estáticas ou encapsular o log de forma que o formato de string só seja avaliado se a flag de log estiver ativa.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-17 | Code review 01 criada via `/code-review` |
