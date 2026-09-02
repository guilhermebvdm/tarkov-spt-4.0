---
title: SAIN — Roadmap do Addon de Imersão Militar Tática (MilSim)
date: 2026-09-02
status: 🟢 Vivo
authors: [guilhermebvdm, Antigravity]
---

# 🗺️ SAIN — Roadmap do Addon de Imersão Militar Tática (MilSim)

Este documento define a visão, a arquitetura e as entregas do futuro **Addon de IA Militar Tática (MilSim)**, projetado para operar em conjunto com o SAIN como um mod independente no BepInEx, garantindo fácil manutenção, isolamento de código e sobrevida entre atualizações do SPT.

---

## 🏛️ Decisão de Arquitetura: Modelo de Addon Desacoplado

```mermaid
graph TD
    A["Escape From Tarkov (0.16.9 / SPT 4.0)"] --> B["BigBrain (xyz.drakia.bigbrain)"]
    B --> C["SAIN (Motor Base: Visão, Cobertura, Raycasts, Áudio)"]
    C --> D["Addon MilSim (Mod Separado)"]
    
    D -.->|Injeta Camadas de Ação com Alta Prioridade| B
    D -.->|Consulta Estado: BotComponent, Squad, DoorHandler| C
```

* **Manutenibilidade:** O SAIN permanece como base limpa e estável. As mecânicas táticas avançadas residem em seu próprio mod.
* **Injeção Não-Invasiva:** Utilização do `BigBrain` para registrar *Brain Layers* de alta prioridade (`BreachAndClearLayer`, `BoundingOverwatchLayer`) que assumem o bot apenas durante os eventos táticos coordenados.
* **Prevenção de Conflitos Internos:** Bloqueio de reações espúrias do SAIN (como fuga em pânico de granadas arremessadas pelo próprio bot ou gatilhos falsos de *unstuck* durante a espera em cobertura).

---

## 🛡️ Pilares de Desenvolvimento do Addon

### 1. Invasão Coordenada de Cômodos (*Breach & Clear*)
- [ ] **Identificação de Cômodos por Portas (`Door`):** Utilização de objetos interativos de porta como delimitador topológico entre corredores e aposentos fechados.
- [ ] **Posicionamento de Umbral (*Stack / Threshold Position*):** Posicionamento do bot na lateral externa da porta, protegido da "linha fatal" do vão.
- [ ] **Lançamento Preciso no Vão da Porta:** Cálculo de vetor balístico mirando no centro geométrico da abertura da porta com trajetória plana (ângulo de 5° a 15°).
- [ ] **Priorização de Granadas:**
  - *Flashbang (Zarya / Stun):* Prioritária para invasão rápida de quartos e salas com confirmação ou suspeita de alvo próximo.
  - *Fragmentação (Frag):* Utilizada para neutralizar alvos entrincheirados em cobertura dura.
- [ ] **Máquina de Estado de Espera (*Hold for Detonation*):** O bot mantém a posição em cobertura aguardando o evento de explosão (`OnGrenadeExplosive`), suprimindo temporariamente decisões de fuga ou busca desordenada.
- [ ] **Invasão Imediata (*Dynamic Push*):** Disparo de avanço agressivo para dentro do cômodo imediatamente após a detonação, aproveitando a janela de atordoamento/desorientação do inimigo.

### 2. Tática de Esquadrão Real (*Bounding Overwatch / Fogo e Movimento*)
- [ ] **Avanço Coordenado (*Leapfrog / Bounding*):** Enquanto 1 ou mais bots do esquadrão mantêm supressão contínua no setor inimigo, outros membros avançam para a próxima linha de cobertura.
- [ ] **Espaçamento Tático de Esquadrão:** Manutenção de distâncias táticas de dispersão para prevenir baixas múltiplas por granadas ou rajadas em linha.
- [ ] **Cobertura de Setores e Retaguarda:** Alocação de membros sem linha de tiro para vigiar flancos expostos e retaguarda.

### 3. Reação Humana ao Fogo e Supressão Pesada
- [ ] **Prioridade de Sobrevivência:** Transição imediata para cobertura sólida ou postura deitada (*prone*) ao sofrer fogo concentrado, evitando trocas de tiro expostas.
- [ ] **Choque de Supressão (*Suppression Flinch*):** Penalidade temporária na velocidade de aquisição de mira e dispersão de disparos ao receber tiros rasantes.
- [ ] **Disciplina de Reexposição:** Redução drástica na frequência com que o bot repete o mesmo ângulo de observação (*re-peeking*) após ser alvejado.

---

## 🔒 Diretrizes de Preservação e Integração
- **Comunicação / VOIP do Jogador:** O subsistema de voz, gritos e detecção de áudio do jogador (`PlayerComponent` / `EnemyTalkClass`) é preservado integralmente, garantindo total compatibilidade com sistemas de VOIP customizados.
- **Parametrização Flexível:** Parâmetros de visão, reflexos e tempos de reação continuarão configuráveis via presets e interface in-game (F6).
