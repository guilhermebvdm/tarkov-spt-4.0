---
title: Spec — Desmembramento de Perna em Bots Vivos (Prone, Agonia & Rastro de Sangue)
date: 2026-08-10
status: 🟡 Planejado
authors: [AI Assistant, USER]
---

# Spec 001 · Desmembramento de Perna em Bots Vivos (Prone, Agonia & Rastro de Sangue)

## 📌 Visão Geral
Esta funcionalidade visa adicionar um nível extremo de imersão ao combate: permitir que disparos de grosso calibre amputem a perna de um bot (AI) que sobreviva ao impacto inicial, forçando o bot a cair de bruços (*Prone*), entrar em animação de agonia, ficar permanentemente impedido de se levantar e rastejar pelo mapa deixando um rastro contínuo de sangue até a morte por sangramento.

---

## 🎯 Requisitos Funcionais

### 1. Escopo & Restrições
- **Apenas Bots (AI):** Aplicado estritamente a bots (Scavs, PMCs, Bosses). Jogadores humanos são totalmente ignorados para evitar travamentos de controle.
- **Específico para Pernas:** Válido exclusivamente para as partes corporais `EBodyPart.LeftLeg` (5) e `EBodyPart.RightLeg` (6).
- **Gatilho de Calibre:** Disparado apenas por calibres de alto impacto cadastrados com probabilidade ativa no `VD_Calibers.json` (ex: 12g, KS-23, 7.62x51, .338 Lapua, .277 Spear).

### 2. Fluxo de Animação & Comportamento
1. **Disparo & Amputação:** Ao receber um tiro de grosso calibre na perna e sobreviver, a perna é amputada via `KillPatch.DismemberLimb`.
2. **Queda Instantânea em Prone:** O bot é imediatamente forçado para a postura de bruços (`Player.IsInPronePose = true` / transição imediata no Animator).
3. **Execução da Agonia:** A animação de agonia de perna é iniciada no chão.
4. **Bloqueio de Postura (Lock em Prone):** Interceptar e bloquear qualquer comando da IA de bot que tente levantar o personagem (*Stand* ou *Crouch*). O bot permanece preso no estado *Prone*.

### 3. Sangramento Arterial & Rastro de Sangue
1. **Fluxo Contínuo de Sangue:** Iniciar um emissor de sangue continuo no coto da perna amputada (`limbSquirter` / `ArterialSpray`).
2. **Rastro de Poças no Chão (Bleed Trail):** Conforme o bot se move/rasteja em prone, instanciar decalques de poça de sangue em intervalos regulares na posição atual do bot, criando um rastro visível de sangue pelo chão.
3. **Morte por Exsanguição:** Aplicar dano por sangramento contínuo na saúde do bot até o decesso definitivo por perda de sangue.

---

## ⚙️ Especificação Técnica Detalhada (Arquitetura)

### Componentes Envolvidos:
- **`LimbKillPatch.cs` / `KillPatch.cs`:** Interceptação do dano na perna quando `player.HealthController.IsAlive == true`.
- **`LivingDismembermentController` (Novo Componente C#):** `MonoBehaviour` anexado ao bot vivo amputado para gerenciar o loop de rastejamento, bloqueio de postura, decalques de rastro de sangue e sangramento por segundo.
- **`GoreObjectPool`:** Reuso dos prefabs de sangue para manter alta performance de FPS durante o rastro no chão.

---

## ✅ Critérios de Aceite
1. Bot atingido por calibre pesado na perna perde o membro e cai instantaneamente em prone.
2. Bot não consegue se colocar em pé ou agachado sob nenhuma circunstância.
3. Rastro de sangue visível se forma no chão ao longo do trajeto de rastejamento do bot.
4. Bot morre naturalmente por sangramento após o tempo configurado.
5. Zero erros de console e sem vazamento de RAM ao descarregar a raid.
