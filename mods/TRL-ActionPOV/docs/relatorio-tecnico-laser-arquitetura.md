---
title: Dossiê Técnico de Arquitetura — Laser Tático vs Balística vs Colimadores no EFT SPT 4.0
date: 2026-08-20
status: 🟢 Vivo
authors: Antigravity + Guilherme + Gemini
---

# Dossiê Técnico de Arquitetura: Laser Tático vs Balística vs Colimadores no EFT SPT 4.0

---

## 1. Sumário Executivo e Diferenciação Fundamental

Durante a investigação minuciosa no código-fonte descompilado do Escape From Tarkov (`Assembly-CSharp`), identificamos a distinção estrutural entre dois sistemas ópticos que frequentemente geram confusão de nomenclatura:

1. **Colimadores e Retículos de Lente (`CollimatorSight.cs` / `ScopePrefabCache.cs`):**
   - Controla o ponto/retículo holográfico desenhado **dentro da lente da mira** (ex: EOTech, Holosun Red Dot, PK-06, etc.).
   - Utiliza a classe `CollimatorSight` com o método `LookAt(point, worldUp)` controlado por `ProceduralWeaponAnimation._adjustCollimatorsToTrajectory`.
   - O ponto da lente se move na tela para compensar a parábola balística da distância de zeramento (zeroing).

2. **Dispositivos Táticos e Feixes Laser (`LaserBeam.cs` / `TacticalComboVisualController.cs`):**
   - Controla os lasers montados nos trilhos do guarda-mão (ex: *NcSTAR Tactical Blue Laser*, *AN/PEQ-15*, *Holosun LS321*, *Zenit Klesh*, *DBAL-PL*).
   - O script `LaserBeam` é um componente monobehaviour anexado ao prefab do acessório tático.
   - Emite um feixe volumétrico 3D (`mesh_1`) a partir da lente do acessório (`transform.position`) e projeta uma malha de ponto vermelho/azul (`mesh_0`) na parede calculada por `Physics.Raycast` no ponto `hitInfo.point`.

---

## 2. Anatomia Detalhada do `LaserBeam.cs`

O código descompilado de `LaserBeam.cs` revela o pipeline exato de renderização:

```csharp
public class LaserBeam : MonoBehaviour
{
    public float RayStart = 0.1f;
    public float MaxDistance = 100f;
    public Material BeamMaterial;     // Shader do feixe volumétrico
    public Material PointMaterial;    // Shader da bolinha na parede
    public LayerMask Mask;            // Camada de colisão ("HitCollider", paredes, corpos)

    private Mesh mesh_0;              // Quad 2D da bolinha (ponto na parede)
    private Mesh mesh_1;              // Cilindro/Pirâmide cônica do feixe de luz
    private Light light_0;            // SpotLight dinâmica de iluminação pontual

    public void LateUpdate()
    {
        Vector3 forward = base.transform.forward;
        if (Physics.Raycast(base.transform.position + forward * RayStart, forward, out var hitInfo, MaxDistance, Mask))
        {
            // 1. Atualização das propriedades materiais com base na distância
            float value = Mathf.Lerp(PointSizeClose, PointSizeFar, hitInfo.distance / MaxDistance);
            float num = (1f - hitInfo.distance / MaxDistance) * IntensityFactor;
            
            // 2. Posicionamento da luz pontual do impacto
            Vector3 vector = hitInfo.point + (hitInfo.normal - forward).normalized * SurfaceOffsetForLight;
            light_0.transform.SetPositionAndRotation(vector, Quaternion.Lerp(Quaternion.LookRotation(hitInfo.point - vector, Vector3.up), base.transform.rotation, 0.25f));
            
            // 3. Desenho da bolinha na parede (Ponto de Impacto)
            Vector3 normal = hitInfo.normal;
            Graphics.DrawMesh(mesh_0, hitInfo.point, Quaternion.LookRotation(normal), PointMaterial, LayerMask.NameToLayer("Default"), null, 0, materialPropertyBlock_1);
        }

        // 4. Desenho do feixe volumétrico da lente até o alvo
        Graphics.DrawMesh(mesh_1, base.transform.position, base.transform.rotation, BeamMaterial, LayerMask.NameToLayer("Default"), null, 0, materialPropertyBlock_0);
    }
}
```

### O que controla a "Bolinha na Parede" (`mesh_0`):
- A bolinha é um **Quad 2D** (`mesh_0`) renderizado na posição exata `hitInfo.point` retornada pelo `Physics.Raycast`.
- A orientação da bolinha é `Quaternion.LookRotation(normal)`, ou seja, ela se "cola" perfeitamente paralela à superfície da parede.
- O raio do `Physics.Raycast` parte de `base.transform.position + forward * RayStart` e viaja no vetor `forward`.

---

## 3. A Mecânica do Tiro e Balística Real (`Player.FirearmController`)

Quando o jogador dispara uma arma no EFT, o método `Player.FirearmController.method_58()` (ou `CreateShot`) executa o seguinte cálculo balístico:

```csharp
Transform original = CurrentFireport.Original;
Vector3 position = CurrentFireport.position;
Vector3 direction = (func_0() ? _player.LookDirection : WeaponDirection);
Vector3 position2 = (func_0() ? _player.AIData.BotOwner.LookSensor.ShootStartPos : position);

// Ajuste crítico de escala de FOV e compensação de tórax (Ribcage)
AdjustShotVectors(ref position2, ref direction);

// Disparo do projétil físico na trajetória consolidada
InitiateShot(weapon, ammo, position2, shotDirection.normalized, position, chamberIndex, weapon.MalfState.LastShotOverheat);
```

### Onde:
- `WeaponDirection`: É definido como `CurrentFireport.Original.TransformDirection(_player.LocalShotDirection)`.
- `AdjustShotVectors`: Aplica a compensação `RibcageScaleCurrent` e o recuo do osso de tórax da hierarquia de mãos (`HandsHierarchy.Self`).

---

## 4. Diagnóstico: Por Que o Laser Ficava Torto no ActionPOV?

1. **A Causa do Desvio:**
   - No jogo vanilla, a câmera e a arma estão travadas no centro da tela. O `transform.forward` do acessório aponta para a frente da câmera.
   - No **ActionPOV**, introduzimos o Free Aim e o Stock Slide, rotacionando a arma fisicamente no espaço 3D (`HandsContainer.WeaponRootAnim`).
   - Quando a arma gira para a direita, a bala sai na direção `WeaponDirection` ajustada por `AdjustShotVectors`.
   - No entanto, o `LaserBeam` nativo lia `base.transform.forward`, que no ciclo de frames do Unity é afetado pela matriz de animação e pela rotação de 3ª pessoa, apontando para um vetor divergente do cano real.

2. **O Erro da Versão 1.3.6 (Laser saindo do cano):**
   - Na versão 1.3.6, para tentar corrigir a posição, alteramos a origem para usar `HandsContainer.Fireport.position`.
   - Como o `Fireport` é o osso na boca do cano (de onde a bala sai), o feixe do laser passou a ser desenhado saindo da ponta do cano em vez da lente do acessório lateral.

---

## 5. A Fórmula Matemática de Alinhamento Perfeito

Para que o laser funcione de forma 100% realista e alinhada ao tiro:

1. **Origem do Feixe (`startPos`):**
   - Deve ser **`__instance.transform.position`** (a posição física real do laser no guarda-mão da arma).
2. **Direção do Feixe e do Raycast (`forward`):**
   - Deve ser a **direção balística consolidada do tiro**:
     ```csharp
     Vector3 forward = firearmController.WeaponDirection;
     Vector3 shotOrigin = firearmController.FireportPosition;
     firearmController.AdjustShotVectors(ref shotOrigin, ref forward);
     ```
3. **Resultado:**
   - O feixe sai do acessório lateral (`PEQ-15` / `NcSTAR`) no guarda-mão.
   - O feixe viaja paralelo ao cano na direção do tiro.
   - A bolinha (`mesh_0`) na parede atinge exatamente o ponto onde o projétil atinge (com o decalque de 2 cm do offset físico do trilho, fiel à física militar real).
