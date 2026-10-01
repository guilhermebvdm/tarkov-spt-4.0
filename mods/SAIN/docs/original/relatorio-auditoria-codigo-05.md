---
title: "Relatório de Auditoria Técnica de Código — SAIN (Parte 5: Combate, Mira, Recoil e Disparo)"
date: 2026-08-31
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SAIN (Parte 5: Combate, Mira, Recoil e Disparo)

Auditoria estática e comportamental profunda focada nos subsistemas de **Balística e Mira Preditiva (`AimClass`)**, **Fórmula de Recoil e Decaimento Angular (`Recoil.cs`)**, **Controle de Cadência e Rajada (`Firerate.cs`, `BotShootPatch`)**, **Supressão Balística (`SAINBotSuppressClass`)** e **Gestão de Granadas (`BotGrenadeManager`, `GrenadePatches`)**.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Falhas catastróficas imediatas |
| 🟠 **Alto** | 2 | Bug matemático em `Recoil.cs` (anulação imediata do recuo em disparos com ângulo negativo) e snap instantâneo de mira em troca de alvos múltiplos |
| 🟡 **Médio** | 1 | Acesso não-defensivo a singleton em patches de granada (`SetGrenadePatch`, `ResetGrenadePatch`) |
| 🔵 **Baixo** | 2 | Avaliação repetida de alocação de depuração em `CalculateAim` e método órfão/catch vazio em `BotLightController` |
| 💡 **Otimização** | 2 | Uso de `Mathf.Abs` para decaimento bidirecional de recuo e unificação com `SAINEnableClass.GetSAIN` |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-05-01` | 🟠 Alto | [`Recoil.cs:L58`](../modded/SAIN/Classes/Bot/WeaponFunction/Recoil.cs#L58) | Lógica / Balística | Condição de término de recuo sem `Mathf.Abs` zera instantaneamente o recuo de 50% dos tiros (todos os que geram ângulo negativo). |
| `AUD-05-02` | 🟠 Alto | [`AimClass.cs:L79-L83`](../modded/SAIN/Classes/Bot/WeaponFunction/AimClass.cs#L79-L83) | Comportamento / Snap de Mira | `smoother.Snap(aimPoint)` é chamado em qualquer troca de inimigo, fazendo a mira do bot teleportar sem suavização inercial. |
| `AUD-05-03` | 🟡 Médio | [`GrenadePatches.cs:L23`](../modded/SAIN/Patches/Shoot/GrenadePatches.cs#L23), [`L42`](../modded/SAIN/Patches/Shoot/GrenadePatches.cs#L42) | AP-02 Singleton Inseguro | Chamada direta `BotManagerComponent.Instance.GetSAIN` sem validação de nulo em eventos de granada. |
| `AUD-05-04` | 🔵 Baixo | [`AimDataPatches.cs:L224-L230`](../modded/SAIN/Patches/Shoot/AimDataPatches.cs#L224-L230) | Micro-alocação | Construção de `StringBuilder` para cálculo de mira em produção sob checagens de flag de debug. |
| `AUD-05-05` | 🔵 Baixo | [`BotLightController.cs:L56-L59`](../modded/SAIN/Classes/Bot/WeaponFunction/BotLightController.cs#L56-L59), [`L68`](../modded/SAIN/Classes/Bot/WeaponFunction/BotLightController.cs#L68) | AP-08 Catch Vazio / Órfão | `setLight` possui `catch { }` sem registro e `ToggleLaser` é método órfão vazio. |

---

## 3. Detalhamento dos Achados

### AUD-05-01 · Anulação Instantânea de Recuo em Disparos de Ângulo Negativo
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`Recoil.cs:L55-L64`](../modded/SAIN/Classes/Bot/WeaponFunction/Recoil.cs#L55-L64), [`Recoil.cs:L102-L103`](../modded/SAIN/Classes/Bot/WeaponFunction/Recoil.cs#L102-L103)
- **Causa Raiz:** O método `calculateRecoil` gera os ângulos de deslocamento horizontal e vertical multiplicados por um sinal aleatório `randomSign()` ($\pm 1$).
  No método `CalcRecoilDecay()`, a condição para considerar o recuo finalizado e zerar os valores foi codificada como:
  ```csharp
  if (_currentRecoilHorizAngle <= 0.001f && _currentRecoilVertAngle < 0.001f)
  {
      _recoilFinished = true;
      _currentRecoilHorizAngle = 0f;
      _currentRecoilVertAngle = 0f;
  }
  ```
  Caso o tiro tenha gerado ângulos negativos (por exemplo, $-2.4^\circ$ horizontal e $-1.5^\circ$ vertical), a expressão avalia como `true` logo no **primeiro frame de decaimento** (pois valores negativos são menores que $0.001$).
- **Impacto Técnico Real:** Metade de todos os tiros disparados pelos bots na partida sofrem cancelamento instantâneo do recuo no frame seguinte, transformando rajadas automáticas e tiros repetidos em disparos com precisão laser artificial.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Avaliar o valor absoluto dos ângulos (`Mathf.Abs`).
  - *Código Refatorado:*

```csharp
public void CalcRecoilDecay()
{
    if (!_recoilFinished)
    {
        float decayTime = Time.fixedDeltaTime * _recoilDecayCoef;
        _currentRecoilHorizAngle = Mathf.LerpAngle(0, _currentRecoilHorizAngle, 1f - decayTime);
        _currentRecoilVertAngle = Mathf.LerpAngle(0, _currentRecoilVertAngle, 1f - decayTime);

        // CORREÇÃO: Usar valor absoluto para checagem de proximidade de zero
        if (Mathf.Abs(_currentRecoilHorizAngle) <= 0.001f && Mathf.Abs(_currentRecoilVertAngle) <= 0.001f)
        {
            _recoilFinished = true;
            _currentRecoilHorizAngle = 0f;
            _currentRecoilVertAngle = 0f;
        }
    }
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

### AUD-05-02 · Teleporte de Ponto de Mira em Troca Rápida de Inimigos
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`AimClass.cs:L79-L84`](../modded/SAIN/Classes/Bot/WeaponFunction/AimClass.cs#L79-L84)
- **Causa Raiz:** No método `AimAtTarget`, ao detectar um novo alvo (`enemy != _lastAimEnemy`), o código executa `smoother.Snap(aimPoint); _lastAimEnemy = enemy;`.
- **Impacto Técnico Real:** Quando um bot está mirando ativamente em um oponente e a decisão de combate altera o foco para outro inimigo próximo, o ponto de mira salta instantaneamente (teleporte em 0 ms) para a cabeça/corpo do novo alvo, contornando a transição inercial de visada que deveria simular o tempo de reação e deslocamento físico da arma.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Executar `smoother.Snap()` apenas se o bot estava sem alvo anterior (`AimStatus == AimStatus.NoTarget`). Caso já estivesse engajado em combate mirando, permitir que o `PositionSmoother` faça a transição fluida com velocidade máxima ajustada.
  - *Código Refatorado:*

```csharp
if (AimStatus == AimStatus.NoTarget)
{
    smoother.Snap(aimPoint);
}
_lastAimEnemy = enemy;
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 2)`

---

### AUD-05-03 · Acesso Não Defensivo a Singleton em `GrenadePatches`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`GrenadePatches.cs:L23`](../modded/SAIN/Patches/Shoot/GrenadePatches.cs#L23), [`GrenadePatches.cs:L42`](../modded/SAIN/Patches/Shoot/GrenadePatches.cs#L42)
- **Causa Raiz:** `SetGrenadePatch` e `ResetGrenadePatch` chamam `BotManagerComponent.Instance.GetSAIN(__instance.BotOwner_0, out var botComponent)` diretamente sem checar se `BotManagerComponent.Instance` é não-nulo.
- **Impacto Técnico Real:** Disparo de `NullReferenceException` se um evento de inicialização de granada ocorrer durante a transição de cena.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Padronizar o uso de `SAINEnableClass.GetSAIN(__instance.BotOwner_0.ProfileId, out BotComponent bot)`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-05-04 · Alocação de `StringBuilder` sob Hot Path de Mira
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`AimDataPatches.cs:L224-L230`](../modded/SAIN/Patches/Shoot/AimDataPatches.cs#L224-L230)
- **Causa Raiz:** Invocação frequente de checagem condicional com instanciação de `StringBuilder` para log de tempo de mira.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Encapsular a lógica de log dentro de `#if DEBUG` ou criar método estático condicional.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-05-05 · Bloco Catch Vazio e Método Órfão em `BotLightController`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`BotLightController.cs:L56-L59`](../modded/SAIN/Classes/Bot/WeaponFunction/BotLightController.cs#L56-L59), [`BotLightController.cs:L68`](../modded/SAIN/Classes/Bot/WeaponFunction/BotLightController.cs#L68)
- **Causa Raiz:** O método `setLight` engole todas as exceções sem registrar no log (`catch { // eft code go burr }`), violando o AP-08. Adicionalmente, `public void ToggleLaser(bool value) { }` é uma função órfã vazia sem implementação.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir o catch cego por tratamento defensivo ou log de depuração condicional.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata do Recoil (AUD-05-01):** Adicionar `Mathf.Abs` na checagem de decaimento de recuo no `Recoil.cs`.
2. **Suavização de Transição de Alvos (AUD-05-02):** Limitar o `smoother.Snap` exclusivamente para estados em que o bot não estava previamente mirando.
3. **Defensiva em Patches de Granada (AUD-05-03):** Migrar para `SAINEnableClass.GetSAIN`.
