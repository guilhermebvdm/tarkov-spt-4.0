# 015 — Revisão Técnica Crítica: Spawn Point Reviewer & Editor

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento  
**Criado:** 2026-09-15T09:42:00-03:00  
**Ref:** `015-spawn-point-reviewer-debug`  

---

## 1. Avaliação Crítica da Abordagem

### 1.1. Complexidade e Risco de Regressão em Gameplay
* **Ponto Forte:** A ferramenta é 100% isolada e protegida pela trava booleana `Settings.enableSpawnPointReviewer.Value`. Se o desenvolvedor não ativar a opção no F12, o `SpawnPointReviewerManager` nem sequer inicia sua rotina, garantindo 0 alocações e 0 impacto na taxa de quadros de jogadores normais.
* **Atenção:** Devemos garantir que o componente não interfira com colisões físicas do jogador real ou dos bots. Por essa razão, todos os `CapsuleCollider`s de seleção dos postes devem ser configurados como `isTrigger = true` e registrados em uma layer sem contato físico com o `CharacterController` do jogador (ex.: ignorar colisão ou layer de UI/Editor).

### 1.2. Detecção com Câmera Livre (FreeCam)
* **Trade-off:** Em Tarkov, mods de câmera livre (como FreeCam / Flycam do SPT ou BepInEx) criam ou desacoplam a câmera principal da cena.
* **Mitigação:** Em vez de confiar exclusivamente em `Camera.main` (que pode ser desativada ou perder a tag `MainCamera` durante voo livre), o código deve implementar um helper que busca `Camera.main ?? Camera.current ?? Object.FindObjectsOfType<Camera>().FirstOrDefault(c => c.enabled)`. Isso garante que o raycast funcione perfeitamente com qualquer ferramenta de câmera livre utilizada pelo desenvolvedor.

### 1.3. Reutilização de Materiais e Performance Gráfica
* **Risco:** Criar um `Material` novo para cada poste e cada quad em mapas com centenas de spawns (ex.: Streets ou Woods com > 200 pontos) alocaria centenas de materiais na VRAM e impediria o batching dinâmico da Unity.
* **Mitigação:** Implementar um pool estático de 5 materiais compartilhados (um para cada estado: Vermelho, Magenta, Verde, Preto e Amarelo de seleção). Todos os postes e quads compartilham esses mesmos 5 materiais, reduzindo o overhead gráfico a praticamente zero.

---

## 2. Parecer

A spec técnica do Item 015 atende a todos os requisitos solicitados com alto rigor técnico e isolamento. Aprovada para implementação em código.
