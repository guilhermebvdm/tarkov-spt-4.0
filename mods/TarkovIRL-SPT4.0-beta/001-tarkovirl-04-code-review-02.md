# 001 — TarkovIRL General Code Review · Code Review 02

**Mod:** TarkovIRL-SPT4.0-beta
**Data:** 2026-07-17

> Análise crítica do código do mod TarkovIRL. Cada achado recebe um ID `CR-02-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | E — Legibilidade/manutenção | 🟠 Forte | Comparação de Enum com Número Mágico `21` (`EPlayerState`) | `[ ]` Pendente |
| CR-02-02 | D — Arquitetura | 🟡 Médio | Falha de Verificação de Nulo em Propriedades Refletidas (`Hydration`/`Energy`) | `[ ]` Pendente |
| CR-02-03 | A — Crítico | 🔴 Bloqueador | Risco de Divisão por Zero (`NaN` / `Infinity`) ao definir Configurações como `0` | `[ ]` Pendente |

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

### CR-02-01 · E — Legibilidade/manutenção · 🟠 Forte

**Comparação de Enum com Número Mágico `21` (`EPlayerState`)**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/Patch_UpdateSwayFactors.cs:39`](../../modded/Patch_UpdateSwayFactors.cs#L39), [`mods/TarkovIRL-SPT4.0-beta/Patch_SetHeadRotation.cs:50`](../../modded/Patch_SetHeadRotation.cs#L50), [`mods/TarkovIRL-SPT4.0-beta/Patch_Look.cs:19`](../../modded/Patch_Look.cs#L19)

**Problema:**
O código realiza a comparação do estado de movimentação atual convertendo o enum `CurrentState.Name` para `int` e comparando com o número mágico `21`:
```csharp
(int)player.MovementContext.CurrentState.Name == 21
```

**Por que importa:**
`CurrentState.Name` é do tipo `EPlayerState`. Se o Tarkov sofrer atualizações e a BSG alterar a ordem dos enums ou inserir novas posturas (como ocorreu no patch de Halloween com posturas de Zumbi), o valor numérico `21` mudará de significado (ex: passará de `Stationary` para outra coisa), quebrando o fluxo de detecção do mod de forma silenciosa.

**Sugestão:**
Substituir a comparação numérica diretamente pela comparação com o tipo de enum correspondente:
```csharp
player.MovementContext.CurrentState.Name == EPlayerState.Stationary
```
(ou o enum planejado que representa o valor 21).

---

### CR-02-02 · D — Arquitetura · 🟡 Médio

**Falha de Verificação de Nulo em Propriedades Refletidas (`Hydration`/`Energy`)**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/EfficiencyController.cs:142-145`](../../modded/EfficiencyController.cs#L142-L145)

**Problema:**
Ao ler as propriedades `Hydration` e `Energy` por reflexão para evitar o cast ofuscado do `GInterface381`, o código assume que `_hydrationProp` e `_energyProp` sempre retornarão valores válidos (não-nulos):
```csharp
ValueStruct hydration = (ValueStruct) _hydrationProp.GetValue(player.HealthController);
```

**Por que importa:**
Caso alguma futura atualização remova ou altere o nome dessas propriedades na classe concreta, `GetProperty("Hydration")` retornará `null`. A tentativa de chamar `GetValue` em um objeto nulo lançará uma `NullReferenceException` que travará a execução do `UpdateEfficiency` todo frame, quebrando a HUD e o mod por completo.

**Sugestão:**
Adicionar verificações defensivas de nulo. Caso as propriedades não sejam encontradas, o mod deve usar valores seguros padrão (ex: eficiência máxima com `hydration.Normalized = 1f` e `energy.Normalized = 1f`) em vez de estourar um erro de exceção.

---

### CR-02-03 · A — Crítico · 🔴 Bloqueador

**Risco de Divisão por Zero (`NaN` / `Infinity`) ao definir Configurações como `0`**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/FreeAimController.cs:53-60`](../../modded/FreeAimController.cs#L53-L60), [`mods/TarkovIRL-SPT4.0-beta/WeaponController.cs:66`](../../modded/WeaponController.cs#L66), [`mods/TarkovIRL-SPT4.0-beta/EfficiencyController.cs:214`](../../modded/EfficiencyController.cs#L214)

**Problema:**
Existem múltiplos locais no mod onde ocorre divisão por variáveis de configuração que aceitam o valor `0.0f` no BepInEx:
1. **FreeAimController**: `deltaRotation = (deltaRotation - (vector2_4 / currentSensitivity))` — se a sensibilidade do FreeAim ou o multiplicador master for `0`, `currentSensitivity` será `0`, provocando divisão por zero.
2. **WeaponController**: `return getInverse ? 1f / num : num` — se a configuração `MinimumWeaponSway` for `0` (limite inferior permitido no F12) e a arma for muito leve, `num` pode ser `0`.
3. **EfficiencyController**: `public static float EfficiencyModifierInverse => 1f / EfficiencyController._efficiencyLerp` — se a eficiência começar em `0` ou for reduzida a `0`.

**Por que importa:**
Divisão por zero em floats no C# resulta em `Infinity` ou `NaN` (Not a Number). Quando estes valores são injetados em propriedades físicas ou vetores de rotação da câmera da Unity (como `__instance.HeadRotation` ou `deltaRotation`), eles quebram a câmera do jogador, congelando a tela, distorcendo o HUD ou provocando tela preta instantânea.

**Sugestão:**
Aplicar travas matemáticas (`Mathf.Max(valor, 0.0001f)`) antes de realizar a divisão para garantir que o divisor nunca seja zero, independentemente de qual valor o jogador digite no menu F12.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-17 | Code review 02 criada via `/code-review` |
| 2026-07-17 | Adicionado achado CR-02-03 (Divisão por zero) |
