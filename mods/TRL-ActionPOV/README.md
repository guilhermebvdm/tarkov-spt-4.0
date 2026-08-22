# TRL-ActionPOV

Mod de cinemática e física de câmera/arma estilo **Bodycam** para SPT Tarkov 4.0 (Escape From Tarkov 0.16.9).

## Características
- **Kinetic Spring Engine:** Física contínua de amortecimento e inércia (*Spring-Damper*) para a arma e cabeça.
- **Shoulder Spherical Pivot:** Deslocamento esférico orgânico com pivô na cavidade do ombro direito $\vec{P}_{ombro} \approx (+0.18\text{m}, -0.16\text{m}, -0.12\text{m})$.
- **Proportional Input Split:** Divisão dinâmica do movimento do mouse entre rotação direta da visão e inércia da arma (sem travamento de câmera).
- **Organic Head Roll:** Inclinação suave da visão proporcional à velocidade angular real do mouse em graus/segundo.
- **ADS Quick Snap:** Recentralização instantânea e amortecida ao aproximar a arma dos olhos.
- **Menu F12 Intuitivo:** Calibração simplificada com apenas parâmetros fundamentais.

## Estrutura do Código
- `Core/EFTBindings.cs` — Cache de reflexão e ligação com objetos do Tarkov.
- `Core/KineticSpringEngine.cs` — Simulação vetorial de física e pivô.
- `Patches/ActionPOVPatches.cs` — 3 hooks Harmony com guards completos de segurança.
- `Plugin.cs` — Ponto de entrada BepInEx.
