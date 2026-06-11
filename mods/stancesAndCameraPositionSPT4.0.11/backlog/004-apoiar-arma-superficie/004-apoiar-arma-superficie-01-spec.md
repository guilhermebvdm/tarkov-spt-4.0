# Spec: Apoiar Arma em Superfícies (Weapon Mounting)

## 1. Visão Geral
Adicionar a capacidade do jogador apoiar (montar) sua arma em superfícies próximas, como barricadas, muros, caixas e janelas. Isso deve proporcionar uma vantagem tática, sacrificando mobilidade por precisão e estabilidade.

## 2. Requisitos Funcionais

### 2.1 Mecânica de Ativação
- A montagem da arma será ativada via uma tecla dedicada configurável via F12 (BepInEx) ou utilizando a keybind nativa de montagem (se aplicável, interceptando a ação).
- O jogador precisa estar fisicamente próximo a uma superfície válida (detectada através de raycasts partindo da arma ou da câmera).
- O sistema verificará:
  - Montagem frontal (apoiado em cima de um muro/caixa).
  - Montagem lateral (escorado na quina de uma parede, esquerda ou direita).

### 2.2 Benefícios e HUD da Montagem
Enquanto a arma estiver montada:
- **Indicador no HUD:** Adicionar um ícone/imagem na interface do jogador que demonstre quando é possível apoiar a arma e, quando montada, indique em qual direção (frente, direita, esquerda). Vamos utilizar a mesma lógica visual do Realism.
- **Redução de Recuo (Recoil):** O recuo visual e mecânico da arma deve ser significativamente reduzido. O multiplicador de redução deve ser configurável no F12 (padrão ~0.5).
- **Redução de Balanço (Sway):** O balanço (sway) natural da mira deve ser quase eliminado. Multiplicador configurável no F12.
- **Estamina de Braço:** A estamina de braço não deve drenar enquanto a arma estiver apoiada. Se possível, deve até regenerar lentamente como se a arma estivesse abaixada.

### 2.3 Restrições e Interrupções
A montagem será cancelada automaticamente se:
- O jogador se mover além de um pequeno limite angular (deadzone) permitido para mirar.
- O jogador se distanciar da superfície de apoio.
- O jogador mudar de postura (agachar, levantar) de forma drástica.
- A arma não estiver em estado utilizável (ex: inspecionando arma ou trocando de arma).

### 2.4 Configurações BepInEx (F12)
- `Enable Weapon Mounting`: Toggle global para a funcionalidade (Bool, Padrão: true).
- `Mounting Recoil Multiplier`: Multiplicador aplicado ao recuo quando apoiado (Float, 0.1 a 1.0, Padrão: 0.5).
- `Mounting Sway Multiplier`: Multiplicador aplicado ao sway quando apoiado (Float, 0.1 a 1.0, Padrão: 0.2).
- (Opcional) Tecla de ativação, caso não usemos a nativa.

## 3. Critérios de Aceite
- [ ] O jogador se aproxima de um muro na altura do peito, pressiona o botão de montar, e a arma "trava" no local.
- [ ] Ao atirar, o recuo é perceptivelmente menor em relação a atirar solto.
- [ ] A estamina de mira para de descer durante o apoio.
- [ ] Mover as teclas WASD (sair da posição) solta a arma da superfície e cancela o bônus imediatamente.
- [ ] Os valores do F12 são lidos e aplicados corretamente na lógica.

## 4. Corner Cases
- **Superfícies Inclinadas:** Garantir que o raycast lide razoavelmente bem com rampas ou objetos esféricos, não montando de forma bizarra.
- **Armas muito curtas:** Pistolas não devem ter o mesmo benefício de rifles, ou podem nem ser suportadas para montagem lateral. A avaliar.
- **Desync:** Cuidado com o estado de montagem ficando "preso" como true se o jogador for teletransportado ou morrer no processo. Garantir limpeza do estado no `OnDestroy` ou equivalente.
