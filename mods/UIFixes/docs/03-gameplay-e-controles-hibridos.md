---
title: "UIFixes — Gameplay e Controles Híbridos"
date: 2026-09-04
status: 🟢 Vivo
authors: Antigravity
---

# UIFixes — Gameplay e Controles Híbridos

O UIFixes aprimora a fluidez das ações do jogador durante a raid, corrigindo limitações históricas do sistema de comandos do Escape From Tarkov.

---

## 1. Controles Híbridos (Toggle vs. Hold)

No jogo base, as ações geralmente são configuradas como puramente alternadas (*Toggle*) ou seguradas (*Hold*). O UIFixes implementa suporte híbrido inteligente para:
- Mira óptica e ADS (`ToggleOrHoldAim`)
- Tecla de interação (`ToggleOrHoldInteract`)
- Disparo de lanternas e lasers táticos (`ToggleOrHoldTactical`)
- Corrida / Sprint (`ToggleOrHoldSprint`)
- Óculos de visão noturna / térmicos (`ToggleOrHoldGoggles`)

### Algoritmo de Decisão:
- **Toque rápido (< 250ms):** Alterna o estado da ação (*Toggle*).
- **Segurar (> 250ms):** Mantém a ação ativa enquanto a tecla estiver pressionada e desliga ao soltar (*Hold*).

---

## 2. Modificação de Armas e Placas Equipadas

No Tarkov nativo, o jogador é forçado a retirar armas ou coletes do personagem para alterar peças simples:
- **`ModifyEquippedWeapons`:** Permite instalar e retirar miras ópticas, trilhos e dispositivos táticos em armas equipadas no personagem fora de raid.
- **`ModifyEquippedPlates`:** Permite inserir ou remover placas balísticas corporais diretamente na tela de equipamentos.
- **`ModifyRaidWeapons`:** Controla se modificações de armas de campo (somente acessórios ou componentes vitais) são autorizadas durante a raid.

---

## 3. Enfileiramento de Entradas (`QueueHeldInputs`)

Evita perdas de comando quando o jogador pressiona uma tecla de ação enquanto uma animação prioritária (ex.: recarga, bandagem ou transição de postura) ainda está sendo concluída. A entrada é retida em buffer e executada no primeiro quadro livre.
