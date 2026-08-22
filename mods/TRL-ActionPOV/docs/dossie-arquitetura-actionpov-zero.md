---
title: ActionPOV — Dossiê Técnico de Arquitetura do Zero
date: 2026-08-18
status: 🟢 Vivo
authors: Antigravity + Gemini + Guilherme
---

# ActionPOV — Dossiê Técnico para Construção do Zero
> Guia de engenharia reversa, hooks essenciais, matemática de pivô e guards do EFT para a nova arquitetura limpa (Kinetic Spring Engine).

---

## 1. Mapeamento de Pivô de Ombro / Arma (Space & Anchoring)

### Hierarquia de Câmera e Mãos no EFT
- O `ProceduralWeaponAnimation` (PWA) controla a hierarquia de animação procedural das armas.
- `__instance.HandsContainer.WeaponRoot` é o `Transform` raiz onde a arma e os braços em primeira pessoa estão ancorados.
- O `HandsContainer` é filho direto do rig da câmera dos olhos do operador (`Eye Space`).
- Por padrão, o `WeaponRoot.localRotation` gira em torno do ponto pivô local do modelo da arma (normalmente próximo ao punho/receiver da arma).

### Ponto de Ancoragem do Ombro Direito (Shoulder Anchor)
Em relação ao centro focal da visão do operador (olhos do jogador no EFT), a cavidade do ombro direito situa-se aproximadamente em:
$$\vec{P}_{ombro} \approx (+0.18\text{m}, -0.16\text{m}, -0.12\text{m})$$

### Equação do Pivô Orgânico (Cone Esférico)
Para que a arma não "deslize em um plano 2D" e gire naturalmente a partir do apoio da coronha no ombro, o deslocamento compensatório $\Delta \vec{P}$ ao aplicar uma rotação de mola $\Delta R$ é:
$$\Delta \vec{P} = \Delta R \cdot (\vec{P}_{arma} - \vec{P}_{ombro}) - (\vec{P}_{arma} - \vec{P}_{ombro})$$

Isso garante que:
- Ao mirar para a direita, a coronha permanece ancorada no ombro direito enquanto o cano da arma descreve um arco esférico natural.
- Ao mirar para cima/baixo, os braços acompanham a elevação sem desacoplar do corpo.

---

## 2. Hooks Mínimos e Indispensáveis (Apenas 3 Patches)

Para o novo mod do zero, **apenas 3 hooks nativos** são necessários:

| # | Método Alvo | Tipo | Propósito |
|---|---|---|---|
| **1** | `EFT.Player.Rotate(ref Vector2 deltaRotation, bool ignoreClamp)` | `Prefix` | **Interceptação e Split de Input:** Divide o movimento do mouse em rotação imediata da câmera (ex: 20–30%) e aceleração da mola cinética da arma/visão (70–80%). |
| **2** | `EFT.Animations.ProceduralWeaponAnimation.SetHeadRotation(Vector3 headRot)` | `Prefix (return false)` | **Cinética de Cabeça / Head Roll:** Assume o controle da inclinação e lag da visão, escrevendo em `player.HeadRotation` e no campo privado `_headRotationVec`. |
| **3** | `EFT.Animations.ProceduralWeaponAnimation.CalculateCameraPosition()` | `Postfix` | **Mãos e Arma:** Aplica a translação esférica e a rotação da mola física diretamente em `__instance.HandsContainer.WeaponRoot`. |

*(Nota: Um hook futuro em `ProceduralWeaponAnimation.OnShot` ou `Player.FirearmController.Shot` cuidará do Camera Punch / Weapon Kick de tiro).*

---

## 3. Eventos de Estado (Guards de Segurança do EFT)

| Estado | Propriedade Nativa | Comportamento Requerido |
|---|---|---|
| **Jogador Local** | `player.IsYourPlayer` | Obrigatório: ignora bots, NPCs e outros clientes (100% compatível com Fika/coop). |
| **Mira Ativa (ADS)** | `__instance.IsAiming` ou `player.HandsController.IsAiming` | Recua a amplitude da deadzone e centraliza a mola na alça de mira. |
| **Corrida (Sprint)** | `player.MovementContext.IsSprintEnabled` ou `CurrentState.Name == EPlayerState.Sprint` | Desativa o free-aim, mantendo a arma na animação de corrida. |
| **Inventário / Bancada** | `player.MovementContext.CurrentState.Name == EPlayerState.Stationary` | Bypassa todos os patches (`return true`), permitindo controle livre do cursor. |
| **Troca de Ombro** | `player.MovementContext.LeftStance` / `IsLeftShoulder` | Fade multiplicador $(1 \to 0)$ para não quebrar a transição de ombro esquerdo. |
| **Arma Apoiada** | `__instance.IsMountedState` | Trava o deslocamento livre da arma no bipé/superfície. |
| **Tiro às Cegas** | `player.MovementContext.CurrentState.AnimatorStateHash == -1271366218 \|\| 1276948056` | Suprime offsets durante o blindfire. |
| **Cura / Uso de Item** | `player.HandsController is Player.UsableItemController` | Evita aplicar offsets de arma enquanto usa itens médicos/cirurgia. |

---

## 4. Estrutura de Reflection Limpa (`EFTBindings`)

```csharp
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Animations;
using HarmonyLib;
using UnityEngine;

namespace ActionPOV.Core
{
    public static class EFTBindings
    {
        // Resolução estática uma única vez no boot
        public static readonly FieldInfo FC_PlayerField = 
            AccessTools.Field(typeof(Player.FirearmController), "_player");
            
        public static readonly FieldInfo PWA_FCField = 
            AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            
        public static readonly FieldInfo PWA_HeadRotVecField = 
            AccessTools.Field(typeof(ProceduralWeaponAnimation), "_headRotationVec");

        // Cache O(1) de instâncias Player associadas a cada ProceduralWeaponAnimation
        private static readonly ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache = new();

        public static Player GetPlayer(ProceduralWeaponAnimation pwa)
        {
            if (pwa == null) return null;
            if (!_playerCache.TryGetValue(pwa, out var player))
            {
                var fc = PWA_FCField?.GetValue(pwa) as Player.FirearmController;
                if (fc != null)
                {
                    player = FC_PlayerField?.GetValue(fc) as Player;
                    if (player != null)
                    {
                        _playerCache.Add(pwa, player);
                    }
                }
            }
            return player;
        }

        public static void SetHeadRotationVec(ProceduralWeaponAnimation pwa, Vector3 rot)
        {
            PWA_HeadRotVecField?.SetValue(pwa, rot);
        }
    }
}
```
