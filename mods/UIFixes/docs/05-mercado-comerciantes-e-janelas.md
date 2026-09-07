---
title: "UIFixes — Mercado, Comerciantes e Janelas de Inspeção"
date: 2026-09-04
status: 🟢 Vivo
authors: Antigravity
---

# UIFixes — Mercado, Comerciantes e Janelas de Inspeção

O UIFixes aprimora substancialmente as telas fora de raid, trazendo agilidade na comercialização, entrega de tarefas e visualização tridimensional de equipamentos.

---

## 1. Otimizações no Flea Market e Comerciantes

Implementado nos patches de Flea e Trading ([`FixFleaPatches.cs`](../original/src/Patches/FixFleaPatches.cs) e [`AddOfferPatches.cs`](../original/src/Patches/AddOfferPatches.cs)):
- **Foco Automático de Preço:** Ao criar uma nova oferta no mercado, o campo de preço é automaticamente focado e selecionado para digitação imediata.
- **Histórico de Pesquisa:** Memoriza o texto digitado anteriormente na barra de busca e permite navegar com as setas do teclado.
- **Atalhos Rápidos de Filtro:**
  - `F`: Filtra ofertas do item no mercado (*Filter by Item*).
  - `L`: Busca itens associados (*Linked Search*).
  - `R`: Busca transações requeridas (*Required Search*).

---

## 2. Automação de Entrega de Missões (`AutofillQuestTurnIns`)

Elimina a necessidade de clicar repetidamente nos botões de entrega de itens de tarefa:
- Ao abrir a tela de tarefas de um comerciante, o sistema seleciona automaticamente os itens correspondentes presentes no inventário do jogador, emulando o botão `AUTO` nativo de forma instantânea.

---

## 3. Câmera e Inspeção 3D de Armas e Personagem

Implementado em [`WeaponZoomPatches.cs`](../original/src/Patches/WeaponZoomPatches.cs) e [`PlayerModelViewPatches.cs`](../original/src/Patches/PlayerModelViewPatches.cs):
- **Zoom com Roda do Mouse:** Permite aproximar e afastar a visão tridimensional de armas e componentes durante a inspeção.
- **Pan com Botão do Meio:** Clicar e arrastar com o botão do meio move a arma pelo espaço da tela, facilitando examinar canos longos e miras traseiras.
- **Inspeção de Personagem:** Habilita rotação fluida, pan vertical e zoom no modelo do PMC no menu principal.
