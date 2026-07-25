# Análise de Funcionalidades do Mod (TarkovIRL)

Este documento analisa detalhadamente todas as funcionalidades do mod **TarkovIRL**, mapeando seus comportamentos, impacto de desempenho (frequência de execução), dependências internas (acoplamento com outros scripts) e externas (outros mods), servindo de base para decidirmos o que manter e o que remover.

---

## 🛠️ Motor Central de Dados (Infraestrutura)
Estas classes não são "funcionalidades visíveis" para o jogador, mas servem de motor de cálculo e coleta de dados para quase todos os efeitos.

### 1. Player Motion Controller (`PlayerMotionController.cs`)
* **Descrição:** Rastreia e calcula a velocidade do jogador, posição do frame anterior, inclinação (lean), stamina dos braços, direção do movimento e os deltas de rotação da câmera (`RotationDelta`, `HorizontalRotationDelta`, `VerticalRotationDelta`, `RawHorizontalSpeed`).
* **Frequência de Execução:** `LateUpdate` (a cada frame).
* **Impacto de Desempenho:** Baixo (apenas cálculos matemáticos simples de vetores e ângulos).
* **Dependências Internas:** Lido por `FreeAimController`, `NewSwayController`, `ParallaxController`, `DirectionalSwayController`.
* **Dependências Externas:** Lê propriedades do `Player` do Tarkov (`EFT.Player`).
* **Veredito:** **Obrigatório/Infraestrutura** (não pode ser removido).

### 2. Efficiency Controller (`EfficiencyController.cs`)
* **Descrição:** Calcula um valor de "Eficiência" (`EfficiencyModifier` entre `0.0` e `1.0` e seu inverso) com base no peso da arma, ergonomia, stamina geral, stamina dos braços, ferimentos e efeitos de adrenalina. Quanto menor a eficiência, maiores/mais instáveis são os efeitos visuais.
* **Frequência de Execução:** `Update` (a cada frame).
* **Impacto de Desempenho:** Baixo.
* **Dependências Internas:** Altamente acoplado. Usado por `NewSwayController`, `ParallaxController`, `DirectionalSwayController`, `WeaponSelectionController`, `FootstepController`.
* **Dependências Externas:** Integrado opcionalmente com o mod **UnderFire** (para ler efeito de adrenalina).
* **Veredito:** **Obrigatório** se mantivermos qualquer efeito que varie de intensidade com base no cansaço/peso da arma.

---

## 🎮 Funcionalidades e Efeitos (Interface BepInEx)

Abaixo está o detalhamento de cada funcionalidade visível que pode ser ativada ou desativada no menu F12:

| Funcionalidade BepInEx | Classe/Script Responsável | Descrição Detalhada | Impacto no Desempenho / Frequência | Dependências Internas | Dependências Externas | Recomendação de Remoção |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Enable Mod** (`EnableMod`) | `PrimeMover.cs` | Toggle mestre do mod. | Baixo | Controla a ativação de todos os patches. | Nenhuma. | **Manter** |
| **Enable weapon deadzone** (`IsWeaponDeadzone`) | `NewDeadzoneController.cs` | Adiciona um peso/atraso mecânico de zona morta para a arma. | Baixo (`LateUpdate`) | Nenhuma. | Nenhuma. | **Remover** (Confirmado pelo usuário que será descartado). |
| **Enable custom weapon sway** (`IsWeaponSway`) | `NewSwayController.cs`, `SwayController.cs` | Substitui o balanço de arma padrão do Tarkov por um balanço customizado que lidera o ponto de mira em vez de atrasar. | Médio (`LateUpdate`) | `PlayerMotionController`, `WeaponController`, `EfficiencyController`, `RealismWrapper` | `EFT.Player` | **Manter** (Essência do mod). |
| **Enable breathing effect** (`IsBreathingEffect`) | `HandBreathController.cs`, `HandShakeController.cs` | Adiciona oscilação visual à arma dependendo da stamina atual. | Baixo (`LateUpdate`) | `PlayerMotionController`, `EfficiencyController` | `EFT.Player` | **Manter** (Adiciona imersão). |
| **Enable stance-dependent weapon position** (`IsPoseEffect`) | `HandPoseController.cs`, `StanceController.cs` | Aproxima a arma do peito do personagem ao agachar ou mudar de postura. | Baixo (`LateUpdate`) | `StanceController`, `PlayerMotionController` | `EFT.Player` | **Candidato a Remoção** (Pode ser integrado de forma nativa ou removido para simplificar a física de animação). |
| **Enable stance transition effect** (`IsPoseChangeEffect`) | `HandPoseController.cs` | Cria um solavanco/mergulho visual na mira quando o jogador muda de postura (ex: de pé para agachado). | Baixo (`LateUpdate`) | `StanceController`, `TarkovIRLCurves` | `EFT.Player` | **Candidato a Remoção** (Mera perfumaria visual). |
| **Enable extra arm stam shake** (`IsArmShakeEffect`) | `HandShakeController.cs` | Aumenta o tremor dos braços conforme a stamina do braço esgota. | Baixo (`LateUpdate`) | `PlayerMotionController`, `TarkovIRLCurves` | `EFT.Player` | **Manter** (Mecânica de gameplay importante). |
| **Enable small visual effects** (`IsSmallMovementsEffect`) | `HandMovWithRotController.cs` | Detalhes cosméticos: puxa a arma levemente para o peito ao girar a câmera rápido, abaixa armas sem coronha ao correr, efeito alternativo de inclinação. | Baixo (`LateUpdate`) | `PlayerMotionController`, `WeaponController` | `EFT.Player` | **Candidato a Remoção** (São micro-detalhes que incham o código de animação). |
| **Enable footstep effect** (`IsFootstepEffect`) | `FootstepController.cs`, `Patch_PlayStepSound.cs` | Faz a arma balançar mais ritmicamente com o som dos passos do personagem. | Baixo (`LateUpdate` + Patch de áudio) | `PlayerMotionController`, `Patch_PlayStepSound` | `EFT.Player` | **Candidato a Remoção** (O Tarkov já possui balanço de passos nativo). |
| **Enable aiming misalignment feature** (`IsParallaxEffect`) | `ParallaxController.cs`, `ParallaxAdsController.cs` | Rotaciona a arma nas mãos do jogador ao girar a câmera rápido, desalinhando a alça e massa de mira (Parallax). | Médio (`LateUpdate`) | `PlayerMotionController`, `EfficiencyController`, `RealismWrapper` | `EFT.Player` | **Manter** (Funcionalidade chave para o realismo físico da mira). |
| **Enable Shot Parallax** (`EnableShotParallax`) | `ParallaxController.cs` | Desalinha as miras temporariamente após efetuar um disparo devido ao recuo físico. | Baixo | `ParallaxController` | `Patch_OnShot` | **Manter** (Funciona junto com o Parallax acima). |
| **Enable directional sway feature** (`IsDirectionalSway`) | `DirectionalSwayController.cs` | Balanço adicional na arma induzido pela movimentação física do jogador (andar para frente, trás ou laterais). | Baixo (`LateUpdate`) | `PlayerMotionController`, `WeaponController`, `EfficiencyController` | `EFT.Player` | **Candidato a Remoção** (Pode ser fundido ao Sway principal ou removido). |
| **Enable ADS head tilt** (`IsHeadTiltADS`) | `Patch_SetHeadRotation.cs` | Adiciona uma inclinação sutil na cabeça ao mirar com armas que possuem coronha. | Baixo (Patch) | `PlayerMotionController`, `WeaponController` | `EFT.Player` | **Candidato a Remoção** (Puramente cosmético). |
| **Enable Enhanced Weapon Transitions** (`IsWeaponTrans`) | `WeaponSelectionController.cs` | Customiza a animação/tempo de transição das armas (coldre, bandoleira, etc). | Baixo (`Update`) | `EfficiencyController`, `WeaponController` | `EFT.Player` | **Candidato a Remoção** (Interfere muito com a lógica de animação original do Tarkov). |
| **Enable True Free Aim** (`EnableFreeAim` / `EnableFreeAimADS`) | `FreeAimController.cs`, `Patch_PlayerRotate.cs` | Faz com que a arma se mova livremente na tela (dentro de uma caixa invisível) antes da câmera do jogador girar. | Médio (Patch no Input) | `PlayerMotionController`, `PrimeMover` | `EFT.Player` (Prefix no método `Rotate`) | **Manter** (Junto com o Sway, é o coração do mod). |

---

## 🔌 Dependências Externas (Outros Mods)
O mod possui pontos de integração opcionais via reflexão/checagem de DLL com os seguintes projetos:

1. **UnderFire (Adrenalina)**:
   * **Onde:** `RealismWrapper.cs` e `UnderFireSoftWrapper.cs`.
   * **Como funciona:** Se o mod `com.rpmwpm.UnderFire` estiver carregado na pasta do BepInEx, o TarkovIRL lê se o estado de Adrenalina está ativo (`UnderFire.Plugin.isAdrenalineActive`) para alterar os multiplicadores de Eficiência física do jogador.
   * **Impacto:** Se o mod não estiver instalado, a integração é ignorada de forma segura (`IsUnderFireLoaded` retorna `false`).

2. **Realism Mod (Equilíbrio de Armas)**:
   * **Onde:** Referenciado conceitualmente em `RealismWrapper.cs` (por exemplo, `RealismWrapper.WeaponBalanceMulti` retorna `1f` atualmente, mas indica que no passado havia dependência de dados ou velocidade de recarga).

---

## 📈 Análise de Desempenho e Gargalos
O mod é relativamente leve por não rodar cálculos de renderização pesados (como Raycasts complexos ou renderizações 3D extras). No entanto, o inchaço ocorre no acúmulo de **Lerps matemáticos** rodando no `LateUpdate` a cada frame para suavizar animações de ossos dos braços (`ProceduralWeaponAnimation` do Tarkov).

Se quisermos enxugar o mod tanto em **desempenho** (menos código executando a cada frame no CPU) quanto em **complexidade de bugs**, a remoção de funcionalidades secundárias de animação cosmética (como `IsWeaponTrans`, `IsPoseChangeEffect`, `IsHeadTiltADS`, `IsSmallMovementsEffect` e `IsFootstepEffect`) trará uma enorme simplificação do código.
