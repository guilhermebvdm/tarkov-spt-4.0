# Spec Funcional — Item 008: Calibrador de Voz Interativo (Assistente de 3 Fases)

## 📌 Visão Geral

Implementar um **Assistente de Calibração de Voz Interativo (Voice Calibration Wizard)** em 3 fases no centro da tela. O assistente libera o mouse para interação do usuário e guia o jogador na leitura de 3 frases temáticas do Tarkov, capturando a energia sonora real (RMS) do microfone para calibrar automaticamente os limiares de **Sussurro (Nível 1)**, **Voz Normal (Nível 2)** e **Falar Alto (Nível 3)**.

---

## 🎯 Objetivos Principais

1. **Ajuste Personalizado ao Microfone:** Calibrar milimetricamente a sensibilidade de cada microfone individual sem depender de constantes estáticas no código.
2. **Interface Guiada & Imersiva (3 Fases):**
   - **Fase 1 (Sussurro):** *"Inimigo se aproximando, mantenha silêncio..."* (Mede RMS de sussurro $\rightarrow$ grava `WhisperThreshold`).
   - **Fase 2 (Voz Normal):** *"Contato visual a cem metros, cobrindo o setor norte."* (Mede RMS de voz normal $\rightarrow$ grava `NormalThreshold`).
   - **Fase 3 (Falar Alto):** *"Cuidado, granada no corredor! Recuar!"* (Mede RMS de fala alta $\rightarrow$ grava `LoudThreshold`).
3. **Controle de Entrada (Mouse & Teclado):** Liberação do cursor do mouse (`Cursor.lockState = CursorLockMode.None`, `Cursor.visible = true`) durante a calibração com trava de entrada do personagem para evitar tiros ou movimento acidental.
4. **Persistência BepInEx:** Salvar os valores calculados no arquivo de configurações `.cfg` do mod no BepInEx.

---

## 🛠️ Critérios de Aceite

- [ ] Tecla/Botão no F12 ("Abrir Assistente de Calibração de Voz") aciona o modal no centro da tela.
- [ ] O cursor do mouse fica livre e visível enquanto o HUD do calibrador estiver aberto.
- [ ] Botão "Segurar para Gravar" (ou barra de espaço) captura as amostras de áudio durante a leitura da frase.
- [ ] Barra visual VU meter em tempo real dentro do calibrador mostra o nível durante o teste.
- [ ] Ao terminar as 3 fases, exibe o resumo com botão "Salvar Calibração" ou "Refazer".
- [ ] Níveis salvos são aplicados instantaneamente ao `VoipProcessor.cs` e ao `InRaidVoipHUD.cs`.

---

## 📋 Etapas do Backlog de Construção

1. **Etapa 1 — Data Model & BepInEx Configs:**
   - Adicionar entradas `WhisperThreshold`, `NormalThreshold` e `LoudThreshold` no `VOIPPlugin.cs`.
   - Adicionar método de acionamento do Calibrador no `VoipController.cs`.

2. **Etapa 2 — UI Modal Centrado (`VoiceCalibrationHUD.cs`):**
   - Componente OnGUI / uGUI centralizado na tela (`width = 520px`, `height = 360px`) com tema escuro tático.
   - Gerenciador de estado do cursor do mouse (`UnlockCursor` / `LockCursor`).

3. **Etapa 3 — Máquina de Estados da Calibração:**
   - Estado `Intro` $\rightarrow$ `Step1_Whisper` $\rightarrow$ `Step2_Normal` $\rightarrow$ `Step3_Loud` $\rightarrow$ `Summary_Save`.
   - Amostragem de RMS médio e pico em cada passo durante 2 a 5 segundos de fala.

4. **Etapa 4 — Conexão com InRaidVoipHUD e Processador de Áudio:**
   - Atualizar a fórmula de preenchimento do `InRaidVoipHUD.cs` para usar os valores calibrados do jogador em vez das constantes estáticas.
