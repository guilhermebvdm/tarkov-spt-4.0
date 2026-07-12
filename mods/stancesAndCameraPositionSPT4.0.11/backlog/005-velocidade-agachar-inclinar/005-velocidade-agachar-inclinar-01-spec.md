# Spec: Aumentar Velocidade de Agachar e Inclinar (Crouch & Lean Speed)

> **Status de validação:** 🟢 **Entregue e validado in-game** (2026-07-11).
> A validação foi **funcional, por feature** — não critério a critério. Os checkboxes deste documento
> são **checklist de referência**, não registro de execução: o fato de estarem desmarcados **não** significa
> que o item não foi testado. A evidência do teste vive em [`memory/sessions.md`](../../memory/sessions.md).<br>
> ⚠️ Essa validação rodou sobre a build **anterior** à reorganização do F12. A revalidação sobre a **v2.0.0**
> é a pendência **P-7.1** (ver a memória).

## 1. Visão Geral
Modificar a velocidade na qual o jogador transita entre as posições em pé, agachado e deitado (prone), além da velocidade de inclinação lateral (lean esquerda/direita). Isso torna a movimentação tática mais fluída e responsiva.

## 2. Requisitos Funcionais

### 2.1 Modificadores de Velocidade
- Aplicar multiplicadores diretos na velocidade das animações e transições de estado do personagem (`Player.MovementContext`).
- Inclinar (Lean) mais rapidamente (Q e E).
- Agachar (C e scroll) e Deitar (X) mais rapidamente.

### 2.2 Configurações BepInEx (F12)
- `Enable Faster Transitions`: Toggle global para a funcionalidade (Bool).
- `Crouch/Prone Speed Multiplier`: Multiplicador para a velocidade de agachar/deitar (Float, padrão: 1.5 - 50% mais rápido).
- `Lean Speed Multiplier`: Multiplicador para a velocidade de inclinar (Float, padrão: 1.5).

## 3. Critérios de Aceite
- [ ] O jogador transita de em pé para deitado visivelmente mais rápido do que no jogo base, baseado no multiplicador do F12.
- [ ] O lean atinge a angulação máxima mais rapidamente.
- [ ] Alterar os valores no F12 em tempo real reflete na velocidade instantaneamente.
