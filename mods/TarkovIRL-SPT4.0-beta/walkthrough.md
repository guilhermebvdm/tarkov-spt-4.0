# Walkthrough — Ajustes Finais de Auto-Center (ADS vs Hipfire) e Cooldown de 500ms no Sway

Finalizamos e implementamos com sucesso as mecânicas de física refinadas para o Free Aim e Weapon Sway.

## Mudanças Realizadas

### [PrimeMover.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/PrimeMover.cs)
* **Novo Slider de Calibração ADS**: Adicionamos a configuração `Camera Auto-Center Sensitivity Compensation` (`FreeAimAutoCenterADSComp`), com padrão `0.35f` (configurável de `0.01f` a `2.0f`). Este slider permite compensar perfeitamente os multiplicadores de zoom e sensibilidade da mira telescópica do jogo.

### [FreeAimController.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/FreeAimController.cs)
* **Retorno da Rotação de Câmera no ADS**: A câmera volta a auto-centralizar na mira durante o ADS (trazendo a câmera até a mira).
* **Calibração Estática**: A fórmula no ADS agora divide a rotação pelo novo slider: `deltaRotation = deltaRotation + (vector2_5 / PrimeMover.FreeAimAutoCenterADSComp.Value)`.
  * **Como calibrar**: Se ao parar o mouse a mira "escorregar" para frente (mesmo sentido), aumente o slider. Se a mira "puxar de volta" (sentido oposto), diminua o slider. Ajuste até que a mira fique **100% fixa no mesmo spot do mundo** enquanto a câmera gira.

### [NewSwayController.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/NewSwayController.cs)
* **Timer do Sway ajustado para 500ms**: O tempo necessário de caminhada estritamente reta (W ou S) para liberar a centralização passiva do Sway foi estendido de 350ms para **500ms** (`0.5f` segundos), ideal para ziguezagues de diagonal.

---

## Verificação e Compilação

Executamos uma nova build do mod:
* **Comando**: `dotnet build` no diretório do mod.
* **Resultado**: Compilado com sucesso!
