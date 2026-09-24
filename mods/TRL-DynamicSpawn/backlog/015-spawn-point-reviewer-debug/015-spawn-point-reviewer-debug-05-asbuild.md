---
title: "Item 015 — Spawn Point Reviewer (Debug/Validação Visual 3D) — As-Built"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 015 — Spawn Point Reviewer (Debug/Validação Visual 3D) — As-Built

## 1. Resumo Executivo

O Item 015 implementou no mod **TRL-DynamicSpawn** uma ferramenta de desenvolvimento e depuração para validação visual, criação e ajuste interativo de todos os pontos de spawn (Nativos da BSG, importados do MOAR e pontos customizados do usuário) via câmera livre, mapa por mapa.

A ferramenta permite identificar e corrigir visualmente pontos de spawn enterrados abaixo do solo, sob rampas/estruturas ou em geometrias inválidas, criando e validando novos pontos com um **Holograma Móvel (Ghost Marker)** translúcido que muda dinamicamente de cor (Verde para válido, Cinza para inválido) e persistindo as correções em arquivos JSON por mapa para uso futuro no sistema de spawn dinâmico.

---

## 2. Componentes e Funcionalidades Implementadas

### 2.1. Marcador Visual 3D (`SpawnMarkerVisual.cs`)
- **Poste Vertical:** `LineRenderer` fino estendido de -12m a +12m relativo ao ponto. Renderiza com shader `Hidden/Internal-Colored` e propriedade `_ZTest = Always` (visível através de paredes, rochas e terrenos).
- **Plano Horizontal do Piso:** `LevelQuad` de 1x1m na cota Y exata da coordenada para verificação do nível do chão.
- **Rótulo Billboard:** `TextMeshPro` em world space orientado continuamente para a câmera ativa com o ID do ponto e indicadores de status (`[OK]`, `[X]`, `*MOVIDO*`, `[NOVO VÁLIDO]`, `[INVÁLIDO: motivo]`).
- **Trigger Collider:** `CapsuleCollider` vertical centrado no poste para permitir detecção por raycast de foco e seleção.

### 2.2. Esquema de Cores de Estado
- **Vermelho:** Ponto Nativo de IA (BSG) pendente de revisão.
- **Magenta:** Ponto customizado MOAR pendente de revisão.
- **Azul Escuro:** Ponto nativo de Infiltração de Player humano (`ESpawnCategoryMask.Player`), com contraste nítido para não confundir com o Verde de aprovado em visão macro aérea.
- **Verde Sólido:** Ponto aprovado e ativo para spawn.
- **Preto:** Ponto reprovado (inválido, no limbo ou obstruído).
- **Amarelo:** Ponto atualmente selecionado/travado para ajuste fino.
- **Ciano:** Ponto em mira (hover).
- 🟢 **Verde Translúcido (Holograma/Ghost):** Novo ponto em ajuste que atende a 100% dos requisitos de jogo.
- ⚪ **Cinza Translúcido (Holograma/Ghost):** Ponto em ajuste que NÃO atende aos requisitos (sem NavMesh, fora de nível ou longe demais de BotZone).

### 2.3. Motor de Validação em Tempo Real (`EvaluatePointCriteria`)
A cada frame em que um ponto é movido ou editado, três testes físicos e lógicos são realizados:
1. **NavMesh:** `NavMesh.SamplePosition` confirma se há malha azul de navegação em um raio de $\le 1.2\text{m}$.
2. **Solo Sólido:** Raycast vertical confirma colisão com terreno/asfalto e tolerância estrita de altura ($|\Delta Y| \le 0.40\text{m}$).
3. **BotZone Tethering:** Localiza a `BotZone` mais próxima e calcula a distância até o baricentro dos seus spawns nativos. Se a distância exceder $85\text{m}$, o ponto é marcado como inválido com o aviso de zona distante.

### 2.4. Fluxo de Criação, Ajuste e Avaliação de Pontos
- **Criação Rápida (`Insert`):** Cria instantaneamente um Ghost Marker no chão à frente da câmera livre e trava a seleção nele.
- **Ajuste Fino 3D das Coordenadas (Relativo à Câmera Livre):**
  - `NumPad 8 / NumPad 2`: Move para Frente / Trás em relação ao vetor de visão horizontal da câmera (±0.1m; com `Shift`: ±0.5m).
  - `NumPad 4 / NumPad 6`: Move para Esquerda / Direita em relação à visão horizontal da câmera (±0.1m; com `Shift`: ±0.5m).
  - `NumPad 9 / NumPad 3`: Sobe / Desce verticalmente no eixo Y (±0.1m; com `Shift`: ±0.5m).
  - `End`: Raycast vertical para baixo com snap imediato da cota Y na malha sólida de solo.
- **Controle de Status e Avaliação (Teclado Numérico):**
  - **`NumPad 1` (Aprovar):** Marca o ponto como Aprovado (Verde sólido) e persiste no JSON. Se for Ghost válido, ativa-o permanentemente.
  - **`NumPad 0` (Reprovar):** Marca o ponto como Reprovado (Preto sólido) e persiste no JSON. Se for Ghost, descarta o holograma.
  - **`NumPad ,` (Resetar Status):** Redefine o status de revisão de volta para `Pending` mantendo intactas as coordenadas 3D ajustadas.
  - **`Backspace` (Resetar Ponto):** Restaura as coordenadas físicas de volta à posição original de fábrica e redefine para `Pending`.
  - **`Ctrl + Shift + R` (Reset Geral do Mapa):** Restaura todos os pontos do mapa de volta ao estado original de fábrica.
  - **`Ctrl + U` (Elevar Vanilla):** Aplica elevação de segurança em lote (+0.15m) a todos os pontos nativos do mapa.
  - **`NumPad 7` (Ciclar Categoria):** Alterna a categoria do ponto entre: `PMC`, `SCAV`, `SNIPER`, `BOSS`, `ROGUE`, `RAIDER`, `PLAYER`.

### 2.5. HUD On-Screen (`OnGUI`)
- Exibe painel semitransparente com dados do ponto ativo, coordenadas, distância da câmera, checklist de requisitos com tags ricas em cores (`[✓] NavMesh`, `[✓] Solo`, `[✓] Zona`), feedback momentâneo de ações executadas, contagem de Players (`Total: N (Players: X)`) e legenda completa de atalhos e cores.

---

## 3. Configurações BepInEx F12 (`Debug - Spawn Point Reviewer`)

| Configuração | Padrão | Descrição |
|---|---|---|
| `Enable Spawn Point Reviewer` | `false` | Master toggle para ativar/desativar o reviewer e os marcadores 3D. |
| `Create Custom Point Key` | `Insert` | Cria um holograma móvel (ghost) no chão à frente da câmera livre. |
| `Delete Point Key` | `Delete` | Exclui definitivamente da cena e do JSON o ponto customizado selecionado (ou reprova ponto nativo). |
| `Select / Lock Target Key` | `KeypadEnter` | Travar/destravar foco no ponto de spawn selecionado. |
| `Approve Point Key` | `Keypad1` | Confirmar Ghost válido ou aprovar ponto existente e salvar no JSON. |
| `Reject Point Key` | `Keypad0` | Cancelar Ghost ou marcar ponto existente como Reprovado e salvar no JSON. |
| `Reset Status Key` | `KeypadPeriod` | Redefinir status de revisão do ponto de volta para Pendente sem perder coordenadas ajustadas. |
| `Snap to Ground Key` | `End` | Snap instantâneo da cota Y ao nível do solo via raycast. |
| `Copy Point Info Key` | `Ctrl + C` | Copia ID e BotZone do ponto selecionado para o clipboard do Windows. |
| `Reset Selected Point Key` | `Backspace` | Restaura o ponto selecionado de volta à coordenada original e status pendente. |
| `Reset Whole Map Key` | `Ctrl + Shift + R` | Restaura todos os pontos do mapa atual para as coordenadas originais e limpa ghosts. |
| `Elevate Vanilla Spawns Key` | `Ctrl + U` | Aplica elevação de segurança em lote (+0.15m na cota Y) para todos os spawns Vanilla. |
| `Cycle Spawn Type Key` | `NumPad 7` | Alterna a categoria do ponto focado: PMC → SCAV → SNIPER → BOSS → ROGUE → RAIDER → PLAYER. |

---

## 4. Estado da Versão

- **Versão do Mod:** `3.7.24` (bump SemVer aplicado em Client e Server).
- **Melhorias Aplicadas (v3.7.24):**
  - **Exclusão Definitiva de Pontos (`Delete` / `Del`):**
    - Pressionar `Delete` em um ponto customizado criado pelo usuário (`CUSTOM_...`) ou fantasma (`Ghost`) remove o ponto permanentemente da memória da cena e do arquivo JSON do mapa.
    - Se pressionado em um ponto Nativo da BSG ou MOAR, o sistema marca o ponto como **Reprovado** (`Rejected`) no JSON sem quebrar a integridade dos arquivos originais.
  - **HUD Atualizado:** Atalho `[Del]: Deletar Ponto` incluído na legenda do rodapé.
  - **Compilação:** 0 erros no Client e no Server.
- **Status do Item:** 🔵 Em andamento (funcionalidades ativas prontas para testes em raid).
