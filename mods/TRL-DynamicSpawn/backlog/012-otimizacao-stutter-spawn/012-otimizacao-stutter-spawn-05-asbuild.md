# 012 — As-Built & Plano de Validação em Raid Real

**Item:** 012-otimizacao-stutter-spawn  
**Versão Entregue:** `3.7.7`  
**Data:** 2026-09-15  
**Autor:** Antigravity  

---

## 1. Registro de Compilação e Artefatos Gerados

Compilação realizada em modo `Release` com isolamento estrito no workspace local (sem cópia para `D:/SPT`):

* **Client (.NET Framework 4.7.2):**
  - **Projeto:** `mods/TRL-DynamicSpawn/modded/Client/TRL-DynamicSpawn-Client.csproj`
  - **Assembly:** `mods/TRL-DynamicSpawn/modded/Client/bin/Release/TRL-DynamicSpawn.dll`
  - **Status:** Compilado com êxito (0 erros, 0 avisos).
* **Server (.NET 9.0):**
  - **Projeto:** `mods/TRL-DynamicSpawn/modded/Server/TRL-DynamicSpawn-Server.csproj`
  - **Assembly:** `mods/TRL-DynamicSpawn/modded/Server/bin/Release/TRL-DynamicSpawn-Server/TRL-DynamicSpawn-Server.dll`
  - **Status:** Compilado com êxito (0 erros).

---

## 2. Plano de Validação em Raid Real

### Metodologia de Teste

Para validar empiricamente as melhorias de desempenho e coesão tática em ambiente de jogo real, execute os três passos a seguir:

---

### Teste 1: Comparação de Frame Time em Spawn de Esquadrão Grande (4+ Bots)

* **Objetivo:** Comprovar a eliminação do congelamento de quadros (*stutter*) durante a instanciação de esquadrões completos de PMCs.
* **Ferramenta Recomendada:** CapFrameX, MSI Afterburner / RivaTuner (RTSS) com gráfico de Frame Time ativo, ou comando de console `fps 1`.
* **Cenário de Teste:**
  1. Configure a distribuição de facções para **"PMC War"** ou **"Warzone"** (para forçar grupos de 3 a 5 integrantes).
  2. Inicie uma raid no mapa **Customs** ou **Streets of Tarkov**.
  3. No menu F12 (`TRL-DynamicSpawn`), certifique-se de que `Enable Smooth Spawning` está marcado (delay padrão 1.5s).
  4. Monitore a curva de frame time quando uma onda de spawn disparar:
* **Critério de Sucesso:**
  - **Baseline (Antes):** Pico súbito de frame time atingindo **200ms a 350ms** contínuos (queda visual para 3-5 FPS durante a geração do squad).
  - **Versão 3.7.7 (Depois):** Spikes diluídos e contidos abaixo de **30ms–35ms** em cada pulso individual. A taxa de quadros deve se manter fluida sem congelamentos perceptíveis na visão do jogador.

---

### Teste 2: Confirmação Visual e Empírica do Pré-Carregamento (Pre-Warming) de Itens Raros

* **Objetivo:** Certificar que bots equipados com itens complexos (ex: capacetes pesados Altyn/Vulkan, miras térmicas FLIR/Reap-IR ou lanternas múltiplas) não causam I/O de disco no momento de renderizar o corpo.
* **Procedimento:**
  1. No menu F12 (`Debug Logs & Developer HUD`), ative **"Enable Debug Logs"**.
  2. Durante a raid, acompanhe o log no console (`~`) ou arquivo `BepInEx/LogOutput.log`.
  3. Localize as linhas correspondentes ao ciclo:
     ```text
     [TRL-DynamicSpawn][SPY] PRE-WARMING BUNDLES: Loading N prefab keys for pmcUSEC squad...
     ```
  4. Observe que a linha `SQUAD LEADER READY` e `SQUAD FOLLOWER SPAWNED` só aparecem **após** a conclusão do pre-warming.
* **Critério de Sucesso:**
  - Nenhum travamento de leitura de disco durante o surgimento físico do bot no mapa.

---

### Teste 3: Coesão Tática de Esquadrão e Integração com SAIN

* **Objetivo:** Verificar se o esquadrão de PMCs se comporta como uma equipe unificada (líder e guarda-costas avançando juntos).
* **Procedimento:**
  1. Aproxime-se da área de atuação do squad ou observe via free-cam/drone caso utilize ferramentas de depuração.
  2. Verifique o posicionamento dos integrantes:
     - Os membros devem nascer próximos uns dos outros (ancoragem adjacente < 20m).
     - Os seguidores devem adotar postura de cobertura e proteção ao redor do líder (`Protect`), avançando em formação e cobrindo ângulos.
  3. Se o SAIN estiver instalado, observe os avisos de rádio entre os bots e confirmação de compartilhamento de alvos (ao disparar contra um membro, os demais integrantes do squad recebem a chamada de socorro imediatamente).
* **Critério de Sucesso:**
  - Nenhum bot do esquadrão vaga desgovernado ou sozinho em direção oposta da zona.
