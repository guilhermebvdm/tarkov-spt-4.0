# 001 — Sincronização Fika em Terceira Pessoa · Code Review 01

**Mod:** TRL-FikaSync-ClimbableLadders / Climbable Ladders  
**Data:** 2026-08-16T01:45:00Z  

> Análise crítica do código implementado para a sincronização multijogador em rede e animação em terceira pessoa do mod Climbable Ladders no Fika Coop.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟠 Forte | Acúmulo de instâncias de `MainPlayerLadderTracker` em `_trackers` | ✅ Resolvido |
| CR-01-02 | B — Bug latente | 🟠 Forte | `ProceduralLadderBody.Dispose` não reseta pesos de IK dos braços e HandPosers | ✅ Resolvido |
| CR-01-03 | B — Bug latente | 🟡 Médio | `Ladder.Awake` usa `registry.Add` suscetível a colisão de chave em recarregamento | ✅ Resolvido |
| CR-01-04 | B — Bug latente | 🟡 Médio | Ausência de null-check em `CameraContainer` e `HandPosers` em `ProceduralLadderBody` | ✅ Resolvido |
| CR-01-05 | E — Manutenção | 🟢 Menor | Ocultação defensiva explícita de arma em `ObservedPlayerLadderController` | ✅ Resolvido |

---

## Pontos

### CR-01-01 · D — Arquitetura · 🟠 Forte

**Acúmulo de instâncias de `MainPlayerLadderTracker` em `_trackers`**

**Local:** [`mods/TRL-FikaSync-ClimbableLadders/modded/Networking/LadderNetworkHandler.cs:22-65`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-FikaSync-ClimbableLadders/modded/Networking/LadderNetworkHandler.cs#L22-L65)

**Problema:**
```csharp
private readonly List<MainPlayerLadderTracker> _trackers = new List<MainPlayerLadderTracker>();
...
private void OnPlayerLadderControllerSpawned(PlayerLadderController controller)
{
    if (controller == null)
        return;

    _trackers.Add(new MainPlayerLadderTracker(controller));
}
```
No `Climbable Ladders`, um novo `PlayerLadderController` é adicionado ao `Player` cada vez que ele inicia a subida em uma escada e é destruído via `Destroy(this)` ao sair. O evento estático `OnPlayerLadderControllerInit` dispara a cada entrada, fazendo com que instâncias antigas de `MainPlayerLadderTracker` se acumulassem na lista `_trackers` indefinidamente durante a sessão.

**Por que importa:**
Vazamento gradual de memória retendo referências de controladores destruídos ao longo de múltiplos raids.

**Sugestão:**
Fazer o `MainPlayerLadderTracker` desinscrever-se e fornecer um callback de remoção/auto-limpeza, com proteção por lock na lista `_trackers`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aplicado em 2026-08-16. `MainPlayerLadderTracker` agora recebe `Action<MainPlayerLadderTracker> onDisposed` e se auto-descarta no `Controller_OnProceduralBodyDestroy`, removendo-se com segurança por `lock (_trackers)`.

---

### CR-01-02 · B — Bug latente · 🟠 Forte

**`ProceduralLadderBody.Dispose` não reseta pesos de IK dos braços e HandPosers**

**Local:** [`mods/Climbable Ladders/modded/ladders.bep/ProceduralLadderBody.cs:215-235`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/Climbable%20Ladders/modded/ladders.bep/ProceduralLadderBody.cs#L215-L235)

**Problema:**
```csharp
public void Dispose()
{
    proceduralLegLeft.Dispose();
    proceduralLegRight.Dispose();
}
```
Durante o ciclo ativo, `proceduralArmLeft`/`proceduralArmRight` aplicam `IKPositionWeight = 1f` e `IKRotationWeight = 1f`, e os `HandPosers` recebem `weight = 1f`. Ao chamar `Dispose()`, esses pesos não eram zerados, podendo deixar resíduos de IK ou poses congeladas nas mãos de jogadores remotos (`ObservedPlayer`) ao sair da escada.

**Por que importa:**
Braços ou dedos do boneco podiam travar na pose do último degrau em vez de retornar suavemente para a pose da arma/idle.

**Sugestão:**
No `Dispose()` de `ProceduralLadderBody`, zerar os pesos `IKPositionWeight = 0f`, `IKRotationWeight = 0f` dos braços e `weight = 0f` dos `HandPosers`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aplicado em 2026-08-16. Implementado reset explícito de pesos de `LimbIK` dos braços e `weight = 0f` em `HandPosers` no `Dispose()` de `ProceduralLadderBody.cs`.

---

### CR-01-03 · B — Bug latente · 🟡 Médio

**`Ladder.Awake` usa `registry.Add` suscetível a colisão de chave em recarregamento**

**Local:** [`mods/Climbable Ladders/modded/ladders.shared/Ladder.cs:28-36`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/Climbable%20Ladders/modded/ladders.shared/Ladder.cs#L28-L36)

**Problema:**
```csharp
void Awake()
{
    registry.Add(NetId, this);
}
```
Se cenas fossem carregadas aditivamente com escadas que compartilham o mesmo nome ou em recarregamento de mapa/transição de sub-cenas, `registry.Add` lançava `ArgumentException: An item with the same key has already been added`.

**Por que importa:**
Interrompe a inicialização do MonoBehaviour `Ladder`, quebrando a interatividade da escada na raid.

**Sugestão:**
Usar indexador direto `registry[NetId] = this;`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aplicado em 2026-08-16. Atualizado `Ladder.Awake` para `registry[NetId] = this;`.

---

### CR-01-04 · B — Bug latente · 🟡 Médio

**Ausência de null-check em `CameraContainer` e `HandPosers` em `ProceduralLadderBody`**

**Local:** [`mods/Climbable Ladders/modded/ladders.bep/ProceduralLadderBody.cs:41, 150-155, 195`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/Climbable%20Ladders/modded/ladders.bep/ProceduralLadderBody.cs#L41)

**Problema:**
Acesso direto a `player.CameraContainer.transform` e `player.HandPosers[0]/[1]` sem verificação de nulidade. Em `ObservedPlayer` ou bots com modelos simplificados/zumbis, `CameraContainer` pode ser nulo ou o array `HandPosers` pode não conter 2 elementos, gerando `NullReferenceException` ou `IndexOutOfRangeException` que cancelava o loop de `Update`.

**Por que importa:**
Crash silencioso no frame da animação em terceira pessoa quando um boneco com esqueleto simplificado usa escada.

**Sugestão:**
Adicionar checagens:
`enterCamContainerRotation = player.CameraContainer != null ? player.CameraContainer.transform.rotation : Quaternion.identity;`
e proteger a rotação da câmera e acesso aos posers com verificações de nulidade e tamanho de array.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aplicado em 2026-08-16. Adicionados null-checks defensivos em `CameraContainer` e no array `HandPosers` em `InitHands`, `Update` e `UpdateGrip`.

---

### CR-01-05 · E — Manutenção · 🟢 Menor

**Ocultação defensiva explícita de arma em `ObservedPlayerLadderController`**

**Local:** [`mods/TRL-FikaSync-ClimbableLadders/modded/Controllers/ObservedPlayerLadderController.cs:25-70`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-FikaSync-ClimbableLadders/modded/Controllers/ObservedPlayerLadderController.cs#L25-L70)

**Problema:**
Se o pacote `LadderStatePacket.Enter` chegasse antes da sincronização do estado de mãos vazias do jogador remoto, a arma podia ficar visível flutuando durante os primeiros frames da escalada.

**Por que importa:**
Pequeno glitch cosmético transitório nos primeiros frames da subida.

**Sugestão:**
Chamar `player.HideWeapon()` em `Init()` e `player.RevealWeapon()` em `OnDestroy()`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aplicado em 2026-08-16. Chamadas `player.HideWeapon()` e `player.RevealWeapon()` adicionadas em `Init()` e `OnDestroy()` do `ObservedPlayerLadderController`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-16T01:45:00Z | Code review 01 criada via `/code-review` |
| 2026-08-16T01:50:00Z | Todas as 5 correções (CR-01-01 a CR-01-05) aplicadas e verificadas |
