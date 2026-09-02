---
title: SAIN — Code Review Etapa 1 (LOD de IA Adaptativo com Despertar Instantâneo)
date: 2026-09-02
status: 🟢 Vivo
authors: [guilhermebvdm, Antigravity]
---

# SAIN — Code Review Etapa 1 · LOD de IA Adaptativo

**Mod:** `SAIN (modded-multithread)`  
**Versão:** `4.6.0`  
**Referência:** Etapa 1 do [Plano de Implementação](../../../.gemini/antigravity-ide/brain/c2374fdc-b4d7-49ce-97f9-f53e9470d810/implementation_plan.md)  
**Documentação Base:** [01-arquitetura-multithread-e-lod.md](01-arquitetura-multithread-e-lod.md)  
**Data:** 2026-09-02  

> Análise crítica do código implementado na **Etapa 1: Sistema de LOD de IA Adaptativo com Despertar Instantâneo**.  
> Cada achado recebe um ID permanente `CR-01-MM` categorizado em 6 dimensões × 4 níveis de impacto.

---

## 📊 Resumo Executivo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 (1 resolvido) · 🟡 Médios: 1 (1 resolvido) · 🟢 Menores: 2 (1 resolvido, 1 futuro) · ✅ Resolvidos: 3 · Total: 4

| ID | Categoria | Impacto | Título | Status |
|:---:|:---:|:---:|---|:---:|
| **CR-01-01** | B — Bug latente | 🟠 Forte | Fallback `float.MaxValue` força bots no Tier 0 permanentemente se nenhum humano estiver vivo | `[x]` ✅ Aplicado em 2026-09-02 |
| **CR-01-02** | D — Arquitetura | 🟡 Médio | Duplicação de busca de humano em vez de reusar `PlayerSpawnTracker.FindClosestHumanPlayer` | `[x]` ✅ Aplicado em 2026-09-02 |
| **CR-01-03** | B — Bug latente | 🟢 Menor | `TimeLastShot` inicia em `0f`, ativando `isRecentlyShot` nos primeiros 10s de qualquer raid | `[x]` ✅ Aplicado em 2026-09-02 |
| **CR-01-04** | F — Melhoria opcional | 🟢 Menor | Constantes de distância e intervalos de corte poderiam ser configuráveis no menu F6 | `[ ]` Deferido (Fase de UI) |

---

## 🔍 Pontos Críticos e Análise Detalhada

### CR-01-01 · B — Bug latente · 🟠 Forte

**Fallback `float.MaxValue` força bots no Tier 0 permanentemente se nenhum humano estiver vivo**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs:257`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs#L257)

**Problema:**
```csharp
float distToHuman = GetMinDistanceToHumanPlayer(currentTime);
bool isCloseToHuman = distToHuman <= LOD_CLOSE_DIST || distToHuman == float.MaxValue;
```
Se todos os jogadores humanos da partida morrerem (ex.: o jogador morre e fica na tela pós-morte, câmera livre de espectador no Fika, ou em testes de raids bot-vs-bot), `GetMinDistanceToHumanPlayer` retorna `float.MaxValue`. A condição `distToHuman == float.MaxValue` força `isCloseToHuman = true` indefinidamente para todos os bots do mapa, forçando-os permanentemente no Tier 0 (Full 60+ Hz).

**Por que importa:**
Anula completamente a economia de CPU no pós-morte do jogador ou em modos de espectador coop, mantendo a carga da CPU em 100% como se o jogador estivesse a menos de 50 metros de todos os bots vivos ao mesmo tempo.

**Sugestão:**
Restringir o fallback de `float.MaxValue` aos primeiros segundos de inicialização da partida (`currentTime < 5f`). Após esse tempo, se nenhum humano for encontrado, `isCloseToHuman` deve ser `false`:
```diff
- bool isCloseToHuman = distToHuman <= LOD_CLOSE_DIST || distToHuman == float.MaxValue;
+ bool isCloseToHuman = distToHuman <= LOD_CLOSE_DIST || (currentTime < 5f && distToHuman == float.MaxValue);
```

**Decisão:**
- `[x]` ✅ Aceitar sugestão (Aplicado em 2026-09-02)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Restringido o fallback de `float.MaxValue` para os primeiros 5s de partida (`currentTime < 5f`). Após esse tempo, ausência de humano avalia como `isCloseToHuman = false`, liberando o estrangulamento de CPU caso o jogador morra.

---

### CR-01-02 · D — Arquitetura · 🟡 Médio

**Duplicação de busca de humano em vez de reusar `PlayerSpawnTracker.FindClosestHumanPlayer`**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs:228-251`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs#L228-L251)

**Problema:**
`GetMinDistanceToHumanPlayer` itera manualmente sobre `PlayerComponent?.OtherPlayersData?.DataList` checando `other.DistanceData.Distance`:
```csharp
var otherPlayers = PlayerComponent?.OtherPlayersData?.DataList;
if (otherPlayers != null)
{
    for (int i = 0; i < otherPlayers.Count; i++)
    {
        var other = otherPlayers[i];
        var otherPlayer = other?.OtherPlayerComponent?.Player;
        if (otherPlayer != null && !otherPlayer.IsAI && other.DistanceData != null)
        {
            float dist = other.DistanceData.Distance;
            if (dist < minDistance)
...
```
O repositório já possui um método nativo, otimizado e centralizado em [`PlayerSpawnTracker.cs:36`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/PlayerManager/Players/PlayerSpawnTracker.cs#L36):
`FindClosestHumanPlayer(out float closestPlayerSqrMag, Vector3 targetPosition, out Player player)`.

**Por que importa:**
`other.DistanceData.Distance` depende de o job `DirectionDataJob` ter rodado e populado o cache. Se o bot acabou de spawnar ou o job estiver defasado, o valor pode estar zerado ou incorreto. Além disso, `FindClosestHumanPlayer` compara distâncias ao quadrado (`sqrMagnitude`), dispensando chamadas desnecessárias a `Mathf.Sqrt`.

**Sugestão:**
Refatorar `GetMinDistanceToHumanPlayer` para utilizar `PlayerSpawnTracker.FindClosestHumanPlayer`:
```csharp
private float GetMinDistanceToHumanPlayer(float currentTime)
{
    if (_nextDistCheckTime > currentTime)
    {
        return _cachedDistToHuman;
    }
    _nextDistCheckTime = currentTime + 0.5f;

    var tracker = GameWorldComponent.Instance?.PlayerTracker;
    if (tracker != null && tracker.FindClosestHumanPlayer(out float sqrMag, Position, out _) != null)
    {
        float dist = Mathf.Sqrt(sqrMag);
        _cachedDistToHuman = dist;
        DistanceToClosestHuman = dist;
        return dist;
    }

    _cachedDistToHuman = float.MaxValue;
    DistanceToClosestHuman = float.MaxValue;
    return float.MaxValue;
}
```

**Decisão:**
- `[x]` ✅ Aceitar sugestão (Aplicado em 2026-09-02)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** `GetMinDistanceToHumanPlayer` refatorado para consultar `GameWorldComponent.Instance.PlayerTracker.FindClosestHumanPlayer` com busca direta zero-alloc.

---

### CR-01-03 · B — Bug latente · 🟢 Menor

**`TimeLastShot` inicia em `0f`, ativando `isRecentlyShot` nos primeiros 10s de qualquer raid**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs:256`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs#L256)

**Problema:**
```csharp
bool isRecentlyShot = Medical != null && Medical.TimeSinceShot < 10f;
```
Em `SAINBotMedicalClass.cs:240`, a propriedade `public float TimeLastShot { get; private set; }` inicia com o valor default `0f`. Nos primeiros 10 segundos da raid (`Time.time < 10f`), `TimeSinceShot` (`Time.time - 0f`) resulta em valores menores que 10f, classificando qualquer bot como recém-alvejado antes mesmo do primeiro disparo.

**Por que importa:**
Embora inofensivo nos primeiros instantes da partida (os bots ainda estão terminando de carregar waypoints), conceitualmente um bot que nunca foi alvejado não deve ter a flag de dano recente ativa.

**Sugestão:**
Exigir que `Medical.TimeLastShot > 0f` para validar se o bot realmente tomou dano:
```diff
- bool isRecentlyShot = Medical != null && Medical.TimeSinceShot < 10f;
+ bool isRecentlyShot = Medical != null && Medical.TimeLastShot > 0f && Medical.TimeSinceShot < 10f;
```

**Decisão:**
- `[x]` ✅ Aceitar sugestão (Aplicado em 2026-09-02)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Adicionada a validação `Medical.TimeLastShot > 0f` antes de calcular `TimeSinceShot < 10f`.

---

### CR-01-04 · F — Melhoria opcional · 🟢 Menor

**Constantes de corte e intervalos poderiam ser configuráveis via menu F6**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs:195-199`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs#L195-L199)

**Problema:**
Os thresholds `LOD_CLOSE_DIST` (50m), `LOD_MID_DIST` (150m), `LOD_MID_INTERVAL` (0.04s) e `LOD_FAR_INTERVAL` (0.12s) estão definidos como constantes privadas no código.

**Por que importa:**
Usuários com CPUs topo de linha (7800X3D / 9800X3D) podem querer estender o alcance pleno do Tier 0 para 100m, enquanto jogadores com CPUs mais modestas (4 a 6 núcleos) poderiam preferir reduzir para 35m e aumentar a economia.

**Sugestão:**
Em uma fase posterior de refinamento da UI, mapear essas propriedades em uma nova categoria no menu gráfico in-game (F6), mantendo os valores atuais como padrões de fábrica balanceados.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 🏆 Conclusão do Review

* **Bloqueadores 🔴:** **0** — A implementação da Etapa 1 está estável, segura e compila perfeitamente sem quebras de API.
* **Recomendações:** A aplicação dos achados **CR-01-01** (fallback pós-morte), **CR-01-02** (reuso de `PlayerSpawnTracker`) e **CR-01-03** (`TimeLastShot > 0`) tornará o sistema ainda mais robusto e elegante antes de iniciarmos a Etapa 2.
