# 015 — Spec Técnica: Spawn Point Reviewer & Editor (Debug Tool)

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento  
**Criado:** 2026-09-15T09:41:00-03:00  
**Ref:** `015-spawn-point-reviewer-debug`  

---

## 1. Arquitetura de Componentes

### 1.1. `SpawnPointReviewerManager : MonoBehaviour`
Responsável pelo ciclo de vida, gerenciamento de estado e persistência:
- **`StartReviewSession(string mapName)`:**
  - Carrega o JSON de reviews prévios em `<AssemblyDir>/SpawnReviews/<mapName>.json`.
  - Coleta todos os `BotZone` e seus `ISpawnPoint`s na cena via `LocationScene.GetAllObjects<BotZone>()` ou `Singleton<GameWorld>.Instance.AllBotZones`.
  - Coleta as listas de pontos do MOAR via `DynamicSpawnManager.PmcSpawns[mapName]` e `DynamicSpawnManager.ScavSpawns[mapName]`.
  - Instancia um `SpawnMarkerVisual` para cada ponto.
- **`Update()`:**
  - Processa raycast da câmera ativa para detecção de foco (`HoveredMarker`).
  - Escuta as teclas F12 configuradas:
    - Seleção/Trava (`KeypadEnter`)
    - Movimentação 3D (`LeftArrow`, `RightArrow`, `UpArrow`, `DownArrow`, `PageUp`, `PageDown`)
    - Snap ao solo (`End`)
    - Aprovação / Reprovação (`KeypadPlus`, `KeypadMinus`)
- **`OnGUI()`:**
  - Renderiza painel HUD discreto no topo da tela com as informações do ponto mirado/selecionado e guia de teclas.
- **`StopReviewSession()`:**
  - Destrói todos os GameObjects instanciados, limpa listas e descarrega materiais da memória.

### 1.2. `SpawnMarkerVisual : MonoBehaviour`
Representação 3D de cada ponto no mundo:
- **Poste:** `LineRenderer` com material configurado com shader `Hidden/Internal-Colored` e `_ZTest = Always`.
- **Quad de Nível:** `PrimitiveType.Quad` rotacionado em X (90°), posicionado em `pos.y + 0.02f`.
- **Rótulo:** `TextMeshPro` no espaço do mundo (`pos.y + 1.2f`). Em `LateUpdate()`, alinha a rotação diretamente com a câmera ativa (`transform.rotation = cam.transform.rotation`).
- **Colisor:** `CapsuleCollider` trigger cobrindo toda a extensão vertical do poste (raio 0.35m, altura 24m) para interceptação facilitada pelo raycast da câmera livre.
- **Métodos de Atualização:**
  - `SetPosition(Vector3 newPos)`: atualiza dinamicamente as posições do poste, do quad, do texto e do colisor.
  - `SetStatus(SpawnReviewStatus newStatus)`: atualiza as cores dos materiais e o texto para refletir Verde, Preto, Vermelho ou Magenta.
  - `SetHighlight(bool isHovered, bool isSelected)`: ajusta intensidade ou cor do texto para indicar seleção ativa.

### 1.3. Modelo de Persistência (`SpawnReviewData`)
```csharp
public class MapReviewFile
{
    public string Map { get; set; }
    public string UpdatedAt { get; set; }
    public Dictionary<string, SpawnPointReviewEntry> Reviews { get; set; } = new();
}

public class SpawnPointReviewEntry
{
    public string Id { get; set; }
    public string Source { get; set; } // "Native", "MOAR_PMC", "MOAR_SCAV"
    public string Zone { get; set; }
    public string Status { get; set; } // "Approved", "Rejected", "Pending"
    public Vector3Model OriginalPosition { get; set; }
    public Vector3Model AdjustedPosition { get; set; }
    public bool WasMoved { get; set; }
    public string ReviewedAt { get; set; }
}
```

---

## 2. Configurações F12 (`Settings.cs`)

Adicionados sob a categoria `[Debug] Spawn Point Reviewer`:
- `enableSpawnPointReviewer` (bool, default: `false`)
- `reviewerSelectKey` (KeyboardShortcut, default: `KeypadEnter`)
- `reviewerApproveKey` (KeyboardShortcut, default: `KeypadPlus`)
- `reviewerRejectKey` (KeyboardShortcut, default: `KeypadMinus`)
- `reviewerSnapKey` (KeyboardShortcut, default: `End`)

---

## 3. Gestão de Memória e Shaders

- Os materiais criados em runtime (`new Material(Shader.Find("Hidden/Internal-Colored"))`) são reutilizados via paleta estática de cores (um material para cada cor de estado: Vermelho, Magenta, Verde, Preto, Destaque Amarelo), evitando instanciar centenas de cópias de materiais na memória.
- Todos os objetos são filhos de um único GameObject raiz `TRL_SpawnPointReviewer_Root`, destruído integralmente no `OnRaidEnd`.
