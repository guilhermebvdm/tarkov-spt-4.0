# Spec Técnica: Animação Orgânica ao Trocar Stances (Wiggle)

## 1. Contexto Técnico
O mod Realism resolveu a linearidade de animação adicionando um efeito "Wiggle" que altera os campos da animação procedural no momento exato em que uma Stance muda, voltando ao normal suavemente por meio de Lerp ao longo do tempo.

## 2. Implementação do "Wiggle"

### 2.1 Campos Alvo
Precisaremos manipular o estado da câmera e da arma através da classe `ProceduralWeaponAnimation`.
O Realism utiliza um método próprio `DoWiggleEffects(Player player, ProceduralWeaponAnimation pwa, Weapon weapon, Vector3 wiggleDir, bool isCover = false, float wiggleFactor = 1f, bool isADS = false, bool useGearSound = false)`.

### 2.2 Estrutura do Efeito
Nós precisaremos criar algo semelhante:
1. Detectar a mudança de postura (já capturamos na nossa classe `StanceController.ChangeStance`).
2. Adicionar impulsos angulares ou posicionais nas instâncias de `CameraRotationRecoilEffect` e `CameraPositionRecoilEffect` ou modificar os vetores de transição durante os primeiros segundos (através de uma rotina no `Update`).

**Exemplo Técnico:**
```csharp
public static void ApplyStanceWiggle(ProceduralWeaponAnimation pwa)
{
    if (!PluginConfig.EnableStanceWiggle.Value) return;

    // Simular o recuo (Wiggle)
    Vector3 wiggleRotation = new Vector3(-1f, 1.5f, 0f) * PluginConfig.StanceWiggleMultiplier.Value;
    
    // Injetar um leve recoil rotacional no momento da troca para dar o "tranco"
    // pwa.Shootingg.CurrentRecoilEffect.CameraRotationRecoilEffect...
    // Ou aplicar um Lerp na posição no nosso próprio StancePatches.
}
```

### 2.3 Configuração BepInEx
```csharp
public static ConfigEntry<bool> EnableStanceWiggle;
public static ConfigEntry<float> StanceWiggleMultiplier;

// Em PluginConfig.cs
EnableStanceWiggle = Config.Bind("9. Stance Animations", "Enable Stance Wiggle", true, "Adiciona movimento orgânico ao trocar de stance.");
StanceWiggleMultiplier = Config.Bind("9. Stance Animations", "Wiggle Multiplier", 1.0f, "Força da animação orgânica.");
```

## 3. Passo a Passo
- Adicionar a chamada de Wiggle no método que invoca a mudança de stance.
- Garantir que não "trave" a arma caso o jogador spame as teclas de postura.
