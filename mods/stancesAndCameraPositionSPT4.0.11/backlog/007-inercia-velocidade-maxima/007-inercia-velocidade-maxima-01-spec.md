# Spec Funcional: Item 007 - Inércia e Velocidade Máxima

## Objetivo
O mod precisa ajustar o peso/inércia global do jogador, bem como diminuir a velocidade máxima de movimentação (caminhar e correr) para aumentar a cadência e simular um movimento mais cadenciado (semelhante a propostas de outros mods voltados ao realismo). Tudo precisa ser ajustável no F12 via BepInEx.

## Critérios de Aceite
1. **Multiplicador de Inércia (F12)**:
   - Um slider/multiplicador ajustável que afetará a inércia base calculada pela engine do jogo (ex: `EFTHardSettings.Instance.Inertia` ou o parâmetro individual de inércia por arma/peso).
   - Valor padrão inicial: +20% mais inércia.
2. **Multiplicador de Velocidade Máxima (F12)**:
   - Dois sliders separados (um para caminhada `Walk` e outro para corrida `Sprint`), ou um global que escala o atributo máximo da física do boneco.
   - Valor padrão: -15% na velocidade base de caminhar.
3. **Turn Penalty Ajustável (F12)**:
   - Modificar a restrição de velocidade de rotação da câmera (Turn Penalty) ao movimentar o mouse. Aumentar a penalidade baseada no peso ou usar um fator estático customizável no F12.
4. **Aplicação Dinâmica**:
   - As mudanças de inércia e velocidade de movimentação devem ser aplicadas sempre que a Raid iniciar (`OnGameStarted`) para garantir que os multiplicadores sobrescrevam qualquer configuração nativa ou padrão lida pelo `BackendConfigSettingsClass`.

## Corner Cases (Casos Extremos)
- **Multiplicadores Zerados ou Muito Baixos**: Se o multiplicador for colocado muito baixo, a inércia pode desaparecer, causando comportamento arcade. O slider F12 precisará ter *ranges* definidos com bom senso (ex: min 0.1x, max 3.0x).
- **Conflito com Realism Mod**: Como dito nas anotações: *"O Realism Mod não altera as velocidades nativas de transição de Pose (Crouch) e Tilt (Lean), apenas a inércia/peso"*. Se o jogador usar outro mod que ajusta Inércia (ex: SPT Realism), este mod poderá sobrescrever a Inércia do outro se aplicado muito tarde. Podemos colocar uma ordem lógica, ou avisar o usuário para gerir no Load Order/F12.
- **Transições Suaves**: A modificação na inércia não deve fazer o boneco travar; as curvas do SPT deverão continuar aplicando interpolação de forma suave.
