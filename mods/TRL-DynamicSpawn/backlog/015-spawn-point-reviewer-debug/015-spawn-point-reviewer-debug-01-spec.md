# 015 — spawn-point-reviewer-debug

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento  
**Criado:** 2026-09-15T09:40:00-03:00  

> **Perfil desta spec:** Ferramenta de Desenvolvimento / Debug. Validação visual, seleção com trava de mira, ajuste fino de coordenadas 3D (eixos X, Y, Z e snap no chão) e persistência de histórico de revisão para todos os pontos de spawn (nativos da BSG e importados do MOAR) via câmera livre.

---

## 1. Visão Geral

Para permitir a validação precisa e auditoria mapa por mapa de todos os pontos de spawn antes de integrar ativamente o MOAR ao mod, é necessária uma ferramenta interna de desenvolvimento que permita:
1. Visualizar com clareza a localização tridimensional exata de cada ponto de spawn (nativos das `BotZone`s e pontos do MOAR `PmcSpawns` / `ScavSpawns`).
2. Identificar visualmente anomalias: pontos enterrados no chão, flutuando no ar, sob caçambas, dentro de paredes ou em geometrias inválidas.
3. Permitir a seleção e trava de alvo de um ponto específico enquanto o desenvolvedor voa com a câmera livre ao redor dele.
4. Possibilitar o ajuste fino das coordenadas espaciais (X, Y, Z e snap ao solo) diretamente no jogo.
5. Salvar o veredito (Aprovado ou Reprovado) e eventuais coordenadas corrigidas em arquivos JSON por mapa dentro da pasta do mod, gerando uma base de dados limpa que será consumida automaticamente em itens futuros.

---

## 2. Comportamento Desejado

### 2.1. Marcador Visual 3D por Ponto
- **Poste Vertical:** LineRenderer fino esticado ±12 metros na vertical com shader `Hidden/Internal-Colored` e `_ZTest = Always`, visível de longa distância e através de qualquer malha ou parede.
- **Plano de Nível:** Quad horizontal 1x1m renderizado na cota Y exata da coordenada para confirmar o alinhamento com o solo.
- **Rótulo Billboard (TextMeshPro):** Texto sempre voltado para a câmera livre, indicando o identificador do ponto:
  - Nativos: `{ZoneName}/{SpawnId}`
  - MOAR PMC: `MOAR_PMC_#{Index}`
  - MOAR Scav: `MOAR_SCAV_#{Index}`

### 2.2. Esquema de Cores por Estado
- **Vermelho (`Color.red`):** Ponto nativo pendente de revisão.
- **Magenta (`Color.magenta`):** Ponto MOAR pendente de revisão.
- **Verde (`Color.green`):** Ponto revisado e Aprovado.
- **Preto (`Color.black`):** Ponto revisado e Reprovado.
- **Destaque Amarelo / Ciano:** Ponto mirado (Hover) ou selecionado com trava (Selected).

### 2.3. Seleção e Trava de Alvo (Lock Mode)
- Ao apontar a câmera livre na direção de um poste a até 100m, o ponto entra em estado `Hover`.
- Pressionar a tecla de seleção (`KeypadEnter` configurável) **TRAVA** o ponto. Uma vez travado, a câmera livre pode voar para qualquer ângulo sem desmarcar o ponto.
- Pressionar a tecla de seleção novamente desseleciona o ponto.

### 2.4. Ajuste Fino 3D de Coordenadas
Com um ponto travado/selecionado:
- **Setas Esquerda / Direita:** Move a coordenada no eixo X (± 0.10m, ou ± 0.50m com Shift).
- **Setas Cima / Baixo:** Move a coordenada no eixo Z (± 0.10m, ou ± 0.50m com Shift).
- **Page Up / Page Down:** Move a coordenada verticalmente no eixo Y (± 0.10m).
- **Tecla [End]:** Executa snap automático no chão via Raycast vertical, posicionando a coordenada milimetricamente na superfície sólida.

### 2.5. Aprovação, Reprovação e Persistência
- **Tecla [KeypadPlus]:** Marca o ponto como Aprovado e salva no JSON.
- **Tecla [KeypadMinus]:** Marca o ponto como Reprovado e salva no JSON.
- Salva imediatamente em `<ModDir>/SpawnReviews/<mapName>.json` preservando a posição original e a posição corrigida (`adjustedPosition`).

---

## 3. Critérios de Aceite

### Não-Regressão
- [ ] **NR-1:** A ferramenta só executa quando `EnableSpawnPointReviewer` for ativado no menu F12 (desligado por padrão).
- [ ] **NR-2:** Em raids normais com a ferramenta desligada, nenhum objeto, mesh ou HUD é instanciado na memória (0 impacto de CPU/GPU).
- [ ] **NR-3:** Nenhum arquivo é criado ou modificado fora da pasta do mod.

### Metas Mensuráveis
- [ ] **AC-M1:** 100% dos pontos nativos e MOAR do mapa atual são gerados com postes e textos legíveis.
- [ ] **AC-M2:** Pressionar as teclas de aprovação/reprovação reflete a cor verde/preta no mesmo frame.
- [ ] **AC-M3:** Fechar e reabrir o mapa carrega as cores verde/preto salvas no JSON sem perda de dados.
- [ ] **AC-M4:** O ajuste fino pelas setas e PgUp/PgDn translada o marcador visual e salva a nova coordenada com precisão.
