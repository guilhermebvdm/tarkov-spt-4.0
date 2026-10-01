---
title: "Relatório de Auditoria Técnica de Código — SAIN (Parte 4: Cobertura, Movimentação, Steering e Portas)"
date: 2026-08-31
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SAIN (Parte 4: Cobertura, Movimentação, Steering e Portas)

Auditoria estática e comportamental profunda focada no **Scanner Volumétrico de Coberturas (`CoverFinderComponent`, `CoverAnalyzer`)**, **Navegação e Movimentação Tática (`SAINMoverClass`, `LeanClass`, `ProneClass`)**, **Priorização de Visada e Rotação (`SAINSteeringClass`)**, **Manipulação de Portas (`DoorOpener`)** e **Patches de Movimento**.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Falhas catastróficas imediatas |
| 🟠 **Alto** | 3 | Bug matemático na fórmula de projeção $t$ em `CoverAnalyzer`, comparação assimétrica de magnitude vs magnitude ao quadrado e bug dimensional de raio de extração em `ExtractAction` |
| 🟡 **Médio** | 2 | Risco de NRE ao restaurar colisão de portas nulas (`DoorOpener.Clear`) e Reflection em patch de pose de movimento (`PlayerSetPosePatch`) |
| 🔵 **Baixo** | 1 | Uso desnecessário de LINQ (`.Contains`) em loop frequente de lean |
| 💡 **Otimização** | 2 | Correção da projeção ortogonal escalar e leitura direta de campo `MovementContext.IsBot` |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-04-01` | 🟠 Alto | [`CoverAnalyzer.cs:L201`](../modded/SAIN/Classes/Coverfinder/CoverAnalyzer.cs#L201) | Matemática / Vetorial | Fórmula de projeção $t$ em `IsPositionNearLineSegment` divide por $\|lineDir\|$ duas vezes ($L^3$), reduzindo $t$ por um fator de $1/L$ e quebrando a detecção de inimigos na rota de cobertura. |
| `AUD-04-02` | 🟠 Alto | [`CoverAnalyzer.cs:L197`](../modded/SAIN/Classes/Coverfinder/CoverAnalyzer.cs#L197) | Lógica / Comparação | Segmentos degenerados comparam `magnitude` linear com `maxDistanceSqr` (distância ao quadrado), causando discrepância dimensional. |
| `AUD-04-03` | 🟡 Médio | [`DoorOpener.cs:L107`](../modded/SAIN/Classes/Bot/Doors/DoorOpener.cs#L107) | NRE Defensivo | `MovementContext.IgnoreInteractionCollision` recebe `ActiveDoor.Door?.Collider` que pode ser `null` no reset de interação. |
| `AUD-04-04` | 🟡 Médio | [`MovementPatches.cs:L66-L76`](../modded/SAIN/Patches/MovementPatches.cs#L66-L76) | AP-04 Reflection Desnecessário | `PlayerSetPosePatch` usa Reflection em `_player` para verificar `IsAI`, quando `MovementContext.IsBot` está publicamente acessível. |
| `AUD-04-05` | 🔵 Baixo | [`LeanClass.cs:L118`](../modded/SAIN/Classes/Bot/Mover/LeanClass.cs#L118) | Micro-alocação / LINQ | `DontLean.Contains(CurrentDecision)` invoca `Enumerable.Contains` a cada 50ms para avaliar 3 valores constantes. |
| `AUD-04-06` | 🟠 Alto | [`ExtractAction.cs:L65`](../modded/SAIN/Layers/Extract/ExtractAction.cs#L65), [`L173-L177`](../modded/SAIN/Layers/Extract/ExtractAction.cs#L173-L177) | Lógica / Matemática | `shouldStartExtract` compara `sqrMagnitude` ($d^2$) diretamente contra `MinDistanceToStartExtract` linear (6m), abortando extração se o bot estiver acima de 3.46m e exigindo aproximação de 2.44m. |

---

## 3. Detalhamento dos Achados

### AUD-04-01 · Bug na Projeção Escalar de Ponto sobre Segmento em `CoverAnalyzer`
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`CoverAnalyzer.cs:L201-L208`](../modded/SAIN/Classes/Coverfinder/CoverAnalyzer.cs#L201-L208)
- **Causa Raiz:** A projeção escalar de um ponto $P$ sobre um segmento de reta de $A$ a $B$ com vetor diretor $V = B - A$ e comprimento ao quadrado $L^2 = \|V\|^2$ é dada formalmente por:
  $$t = \frac{(P - A) \cdot V}{\|V\|^2} = \frac{\text{Vector3.Dot}(P - A, V)}{\text{lineDir.sqrMagnitude}}$$
  No código atual:
  ```csharp
  Vector3 lineDir = end - start;
  float lineLength = lineDir.sqrMagnitude; // lineLength é ||V||^2
  float t = Vector3.Dot(position - start, lineDir.normalized) / lineLength; // lineDir.normalized divide por ||V||
  ```
  O produto escalar usa `lineDir.normalized` (que já divide por $\|V\|$) e em seguida divide por `lineLength` ($\|V\|^2$). O resultado é que $t$ é dividido por $\|V\|^3$.
- **Impacto Técnico Real:** Para um segmento de 10 metros ($\|V\| = 10, \|V\|^2 = 100$), o fator $t$ resultante é 10 vezes menor do que deveria ser. Como consequência, a posição calculada $A + t \cdot V$ sempre fica colada no início do segmento ($A$). O método `checkPathToEnemy` falha em identificar quando o caminho para a cobertura passa perto de um inimigo, fazendo bots escolherem coberturas que exigem correr na frente da linha de fogo adversária.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Utilizar o vetor `lineDir` não-normalizado no produto escalar ou dividir apenas pela magnitude.
  - *Código Refatorado:*

```csharp
public static bool IsPositionNearLineSegment(Vector3 position, Vector3 start, Vector3 end, float maxDistanceSqr)
{
    Vector3 lineDir = end - start;
    float lineLengthSqr = lineDir.sqrMagnitude;
    if (lineLengthSqr < 0.01f)
    {
        return (position - start).sqrMagnitude <= maxDistanceSqr;
    }

    // Projeção ortogonal canônica sobre o segmento: t = ((P - A) . V) / ||V||^2
    float t = Vector3.Dot(position - start, lineDir) / lineLengthSqr;
    t = Mathf.Clamp01(t);

    Vector3 closestPoint = start + t * lineDir;
    float distSqr = (position - closestPoint).sqrMagnitude;

    return distSqr <= maxDistanceSqr;
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

### AUD-04-02 · Comparação Dimensional Incompatível em Segmentos Curtos
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`CoverAnalyzer.cs:L195-L198`](../modded/SAIN/Classes/Coverfinder/CoverAnalyzer.cs#L195-L198)
- **Causa Raiz:** Quando `lineLength < 0.01f`, a linha 197 executa:
  `return (position - start).magnitude <= maxDistanceSqr;`
- **Impacto Técnico Real:** O parâmetro recebido é `maxDistanceSqr` (distância ao quadrado), mas o lado esquerdo calcula a magnitude linear simples (`magnitude`). Para tolerâncias como $0.5$, a checagem exige $\text{dist} \le 0.5$ em vez de $\text{dist} \le \sqrt{0.5} \approx 0.707$, rejeitando incorretamente trajetórias válidas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir por `(position - start).sqrMagnitude <= maxDistanceSqr`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

### AUD-04-03 · Risco de Exceção Nula em Restauração de Colisão de Portas
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`DoorOpener.cs:L107`](../modded/SAIN/Classes/Bot/Doors/DoorOpener.cs#L107)
- **Causa Raiz:** `Bot.Player.MovementContext.IgnoreInteractionCollision(ActiveDoor.Door?.Collider, false);`
  Se a porta não tiver colisor ou se `ActiveDoor.Door` for nulo no momento da limpeza de estado, a função do EFT recebe `null`, podendo disparar `NullReferenceException`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar verificação de colisor antes de invocar a remoção de colisão ignorada:

```csharp
private void Clear()
{
    var collider = ActiveDoor.Door?.Collider;
    if (collider != null)
    {
        Bot.Player.MovementContext.IgnoreInteractionCollision(collider, false);
    }
    Interacting = false;
    _interactionDoorIndex = 0;
    ActiveDoor = new();
    _doorInteractionEndTime = 0;
    InteractionType = EInteractionType.Open;
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-04-04 · Reflection Redundante em `PlayerSetPosePatch`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`MovementPatches.cs:L66-L76`](../modded/SAIN/Patches/MovementPatches.cs#L66-L76)
- **Causa Raiz:** O patch utiliza `FieldInfo.GetValue` sobre o campo protegido `_player` para descobrir se o jogador é IA (`player.IsAI`), quando a classe `MovementContext` expõe a propriedade pública `public bool IsBot`.
- **Impacto Técnico Real:** Sobrecarga de Reflection em transições frequentes de corrida e agachamento.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Ler diretamente `__instance.MovementContext.IsBot`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-04-05 · Uso de LINQ em Hot Loop de Verificação de Lean
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`LeanClass.cs:L118`](../modded/SAIN/Classes/Bot/Mover/LeanClass.cs#L118)
- **Causa Raiz:** `DontLean.Contains(CurrentDecision)` utiliza o método de extensão LINQ `System.Linq.Enumerable.Contains`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Utilizar padrão C# moderno: `if (CurrentDecision is ECombatDecision.Retreat or ECombatDecision.RunAway or ECombatDecision.MeleeAttack)`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-04-06 · Discrepância Dimensional no Raio de Extração de Bots
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`ExtractAction.cs:L65`](../modded/SAIN/Layers/Extract/ExtractAction.cs#L65), [`ExtractAction.cs:L171-L188`](../modded/SAIN/Layers/Extract/ExtractAction.cs#L171-L188)
- **Causa Raiz:** No método `Update`, a distância ao ponto de extração é calculada ao quadrado:
  `float distance = (point - BotOwner.Position).sqrMagnitude;`
  Em seguida, `shouldStartExtract(distance)` avalia:
  ```csharp
  if (distance > MinDistanceToStartExtract * 2) // MinDistanceToStartExtract = 6f -> compara com 12f
      ExtractStarted = false;
  if (distance < MinDistanceToStartExtract)     // compara com 6f
      ExtractStarted = true;
  ```
- **Impacto Técnico Real:** Como `distance` é $d^2$, para iniciar a extração o bot precisa estar a menos de $\sqrt{6} \approx 2.44\text{ metros}$. Se o bot estiver a $3.5\text{ metros}$ ($3.5^2 = 12.25$), a condição `distance > 12f` cancela imediatamente o estado `ExtractStarted`, abortando a extração. Como várias zonas de extração de EFT possuem raios entre $3\text{m}$ e $5\text{m}$, bots entram em loop oscilatório ligando e desligando o timer de extração sem conseguir concluir a saída da raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Comparar com os valores ao quadrado dos limites ou usar distância linear `Vector3.Distance`.
  - *Código Refatorado:*

```csharp
private static readonly float _minExtractDistSqr = MinDistanceToStartExtract * MinDistanceToStartExtract; // 36f
private static readonly float _maxExtractDistSqr = (MinDistanceToStartExtract * 2) * (MinDistanceToStartExtract * 2); // 144f

private bool shouldStartExtract(float distanceSqr)
{
    if (distanceSqr > _maxExtractDistSqr)
    {
        ExtractStarted = false;
    }
    else if (distanceSqr < _minExtractDistSqr)
    {
        ExtractStarted = true;
    }

    return ExtractStarted;
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata da Projeção de Cobertura (AUD-04-01 & AUD-04-02):** Corrigir a fórmula matemática em `CoverAnalyzer.IsPositionNearLineSegment` para garantir que o cálculo de desvio de rota em relação ao inimigo funcione corretamente.
2. **Defensiva em Portas (AUD-04-03):** Garantir que colisores nulos não sejam repassados para `IgnoreInteractionCollision`.
3. **Eliminação de Reflection (AUD-04-04):** Usar `MovementContext.IsBot` diretamente.
