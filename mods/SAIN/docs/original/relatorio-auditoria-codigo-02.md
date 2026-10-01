---
title: "Relatório de Auditoria Técnica de Código — SAIN (Parte 2: Sensores, Visão e Audição)"
date: 2026-08-31
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SAIN (Parte 2: Sensores, Visão e Audição)

Auditoria estática e comportamental profunda focada nos subsistemas de **Percepção Visual**, **Audição Espacial / Balística Acústica**, **Detecção de Fogo Amigo**, **Ofuscamento por Lanternas/Lasers (Dazzle)** e **Patches de Sensores**.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Falhas catastróficas imediatas |
| 🟠 **Alto** | 2 | Vazamento de evento acumulativo em `HearingInputClass` (zombie bots em impactos de bala) e falso positivo de som de Reload para consumíveis/ações |
| 🟡 **Médio** | 2 | Alocação contínua de Heap (`SphereCastAll`) em checagens de friendly fire e cálculos redundantes de raiz quadrada em `FlashLightDazzleClass` |
| 🔵 **Baixo** | 1 | Risco de `NullReferenceException` em `player.HandsController.Item` durante trocas de itens |
| 💡 **Otimização** | 2 | Implementação de `Physics.SphereCastNonAlloc` e cache local de vetores normalizados |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-02-01` | 🟠 Alto | [`HearingInputClass.cs:L52`](../modded/SAIN/Classes/Bot/Sense/Hearing/HearingInputClass.cs#L52) | Event Leak / Zombie Invocation | `BotHearing.BulletImpact` é inscrito no `Init()` mas nunca cancelado (`Dispose` ausente), acumulando instâncias de bots mortos que continuam processando impactos. |
| `AUD-02-02` | 🟠 Alto | [`SAINSoundTypeHandler.cs:L44-L47`](../modded/SAIN/Helpers/SAINSoundTypeHandler.cs#L44-L47) | Lógica Comportamental | `else` incondicional classifica qualquer som que não seja granada/cura como `SAINSoundType.Reload`, fazendo bots rusharem quando o player come, bebe ou usa itens. |
| `AUD-02-03` | 🟡 Médio | [`SAINFriendlyFireClass.cs:L114-L118`](../modded/SAIN/Classes/Bot/Sense/SAINFriendlyFireClass.cs#L114-L118) | GC Pressure / Heap Allocation | `Physics.SphereCastAll` aloca um novo array `RaycastHit[]` no Heap a cada checagem de linha de tiro em combate. |
| `AUD-02-04` | 🟡 Médio | [`FlashLightDazzleClass.cs:L50-L57`](../modded/SAIN/Classes/Bot/Sense/FlashLightDazzleClass.cs#L50-L57) | Desempenho Matemático | Cálculo duplo de raiz quadrada (`.normalized` e `.magnitude` isolados) em raycasts frequentes de lanternas e lasers. |
| `AUD-02-05` | 🔵 Baixo | [`SAINSoundTypeHandler.cs:L16`](../modded/SAIN/Helpers/SAINSoundTypeHandler.cs#L16) | NRE Defensivo | Acesso a `player.HandsController.Item` sem verificação de nulo no controller de mãos. |

---

## 3. Detalhamento dos Achados

### AUD-02-01 · Inscrição Permanente de `BulletImpact` em Bots Despawnados
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`HearingInputClass.cs:L50-L54`](../modded/SAIN/Classes/Bot/Sense/Hearing/HearingInputClass.cs#L50-L54)
- **Referência Cruzada:** [`SAINHearingSensorClass.cs:L45`](../modded/SAIN/Classes/Bot/Sense/Hearing/SAINHearingSensorClass.cs#L45)
- **Causa Raiz:** No método `Init()`, a classe inscreve manipuladores de eventos globais:
  `PlayerComponent.OnBulletFlyBy += OnBulletFlyBy;`
  `BotManagerComponent.Instance.BotHearing.BulletImpact += bulletImpacted;`
  No entanto, `HearingInputClass` não sobrescreve o método `Dispose()` para desinscrever os eventos.
- **Impacto Técnico Real:** Toda vez que um bot morre ou é reciclado, o `BotHearing.BulletImpact` singleton continua mantendo uma referência viva para aquele `HearingInputClass`, impedindo que o Garbage Collector libere a memória e invocando `bulletImpacted` em dezenas de instâncias inativas a cada impacto de bala na raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Implementar o método `Dispose()` em `HearingInputClass` realizando o unsubscribe explícito dos eventos.
  - *Código Refatorado:*

```csharp
public override void Dispose()
{
    if (PlayerComponent != null)
    {
        PlayerComponent.OnBulletFlyBy -= OnBulletFlyBy;
    }
    if (BotManagerComponent.Instance?.BotHearing != null)
    {
        BotManagerComponent.Instance.BotHearing.BulletImpact -= bulletImpacted;
    }
    base.Dispose();
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

### AUD-02-02 · Falso Positivo de Som de Recarga (`SAINSoundType.Reload`) para Sons Genéricos
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`SAINSoundTypeHandler.cs:L44-L47`](../modded/SAIN/Helpers/SAINSoundTypeHandler.cs#L44-L47)
- **Causa Raiz:** Na triagem de eventos de som capturados de `BaseSoundPlayer.SoundEventHandler`, caso o item ativo não seja `ThrowWeapItemClass` nem `MedsItemClass`, o código cai em um bloco `else` fixo que atribui:
  `soundType = SAINSoundType.Reload;`
  `soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Reload;`
- **Impacto Técnico Real:** Ações como consumir comida/bebida, verificar miras, inspecionar câmara, alterar modo de tiro ou interagir com itens que não sejam granadas/meds são notificadas como som de "Reload" para toda a IA em raio de até 20–30 metros. Os bots interpretam que o jogador está indefeso recarregando e ativam manobras imediatas de rush tático (`RushEnemy` / `OpportunityReload`).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Validar a string do `sound` ou o tipo de animação em execução antes de presumir `Reload`. Se o som não corresponder a um padrão conhecido de recarga (ex.: `sound.Contains("mag") || sound.Contains("reload") || sound == "Chamber"`), classificar como som genérico de equipamento (`SAINSoundType.GearSound`) ou ignorar.
  - *Código Refatorado:*

```csharp
else if (sound.Contains("Reload") || sound.Contains("Mag") || sound.Contains("ammo"))
{
    soundType = SAINSoundType.Reload;
    soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Reload;
}
else
{
    soundType = SAINSoundType.GearSound;
    soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_AimingandGearRattle;
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

### AUD-02-03 · Alocação de Array no Heap em `SAINFriendlyFireClass`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`SAINFriendlyFireClass.cs:L80`](../modded/SAIN/Classes/Bot/Sense/SAINFriendlyFireClass.cs#L80), [`L114-L118`](../modded/SAIN/Classes/Bot/Sense/SAINFriendlyFireClass.cs#L114-L118)
- **Causa Raiz:** O método `SphereCastAll` é chamado toda vez que um bot avalia uma intenção de disparo e invoca internamente `Physics.SphereCastAll(...)`, que aloca um novo `RaycastHit[]` na memória gerenciada a cada chamada.
- **Impacto Técnico Real:** Em combates com múltiplos bots de esquadrão mirando e disparando, centenas de arrays são criados por segundo, alimentando a fragmentação do Garbage Collector do Unity.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Utilizar um buffer fixo estático com `Physics.SphereCastNonAlloc`.
  - *Código Refatorado:*

```csharp
private static readonly RaycastHit[] _friendlyFireHitBuffer = new RaycastHit[16];

public static FriendlyFireStatus CheckFriendlyFire(
    Vector3 weaponFirePort,
    float distance,
    Vector3 weaponPointDirection,
    BotComponent bot
)
{
    const float sphereCastRadius = 0.2f;
    int count = Physics.SphereCastNonAlloc(weaponFirePort, sphereCastRadius, weaponPointDirection, _friendlyFireHitBuffer, distance, LayerMaskClass.PlayerMask);
    if (count == 0) return FriendlyFireStatus.None;

    for (int i = 0; i < count; i++)
    {
        var hit = _friendlyFireHitBuffer[i];
        if (hit.collider == null) continue;

        Player player = GameWorldComponent.Instance?.GameWorld?.GetPlayerByCollider(hit.collider);
        if (player == null || player.ProfileId == bot.ProfileId) continue;

        if (!bot.EnemyController.IsPlayerAnEnemy(player.ProfileId))
        {
            return FriendlyFireStatus.FriendlyBlock;
        }
    }
    return FriendlyFireStatus.Clear;
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 2)`

---

### AUD-02-04 · Cálculo Redundante de Magnitude em `FlashLightDazzleClass`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`FlashLightDazzleClass.cs:L50-L57`](../modded/SAIN/Classes/Bot/Sense/FlashLightDazzleClass.cs#L50-L57), [`L80-L87`](../modded/SAIN/Classes/Bot/Sense/FlashLightDazzleClass.cs#L80-L87)
- **Causa Raiz:** A expressão `(botPos - weaponRoot).normalized` e `(botPos - weaponRoot).magnitude` é executada consecutivamente dentro dos métodos de checagem de lanterna e laser, calculando a raiz quadrada da distância duas vezes separadas no frame.
- **Impacto Técnico Real:** Desperdício de ciclos de CPU de ponto flutuante em checagens sensoriais repetidas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Reutilizar a variável de vetor e calcular a magnitude uma única vez.
  - *Código Refatorado:*

```csharp
Vector3 toBot = botPos - weaponRoot;
float rayDist = toBot.magnitude;
if (rayDist > 0.01f && !Physics.Raycast(weaponRoot, toBot / rayDist, rayDist, LayerMaskClass.HighPolyWithTerrainMask))
{
    float gainSight = 1.33f;
    float dazzlemodifier = dist < MaxDazzleRange ? GetDazzleModifier(enemy) : 1f;
    ApplyDazzle(dazzlemodifier, gainSight);
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-02-05 · Ausência de Navegação Segura em `HandsController.Item`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`SAINSoundTypeHandler.cs:L16`](../modded/SAIN/Helpers/SAINSoundTypeHandler.cs#L16)
- **Causa Raiz:** Leitura direta de `player.HandsController.Item` sem verificação de nulo em `HandsController`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Utilizar `player.HandsController?.Item`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 1)`

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata do Dispose de Áudio (AUD-02-01):** Adicionar `Dispose()` a `HearingInputClass` para eliminar o leak de bots mortos no `BulletImpact`.
2. **Correção de Discriminação Sonora (AUD-02-02):** Refinar `SAINSoundTypeHandler` para não tratar qualquer uso de item como recarga de arma.
3. **Zero-Allocation em Friendly Fire (AUD-02-03):** Migrar para `Physics.SphereCastNonAlloc` no `SAINFriendlyFireClass`.
4. **Otimização Numérica (AUD-02-04):** Simplificar os vetores em `FlashLightDazzleClass`.
