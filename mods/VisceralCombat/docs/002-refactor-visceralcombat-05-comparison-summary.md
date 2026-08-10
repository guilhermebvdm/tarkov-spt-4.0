# Relatório Geral de Refatoração: VisceralCombat (Original vs. Modded)

Este relatório resume **todas** as intervenções técnicas, correções de bugs, otimizações de memória/CPU e recursos adicionados durante o processo completo de refatoração da pasta `modded` em relação ao código-fonte original.

---

## 🚀 1. Otimização de Performance & Vazamento de Memória (RAM / CPU)

* **Eliminação de Loops de Busca de RAM:** No mod original, vários patches executavam `GetComponentInChildren`, `FindObjectsOfType` e varreduras de transformadas repetidamente dentro de corrotinas ativas e métodos por frame (`Update`). As referências agora são cacheadas e reutilizadas no momento do evento.
* **Destruição Limpa de Instâncias Transientes:** Efeitos de partículas de sangue, decalques e áudios que ficavam órfãos na memória agora possuem tempos de vida limitados e descargas limpas (`Destroy`).
* **Prevenção de Leaks em Dicionários Estáticos:** Limpeza garantida em coleções estáticas (`deadPlayers`, `dismemberedPlayers`), evitando que jogadores e bots descarregados do mapa permaneçam retidos no Garbage Collector (GC).
* **Eliminação de Lag Spikes no PhysX:** A redução da escala de ossos (`0.001f`) no original mantinha os componentes `Joint` ativos, forçando o PhysX a calcular distâncias de âncoras multiplicadas por 1000x e causando quedas bruscas de FPS. Agora os `Joint` são destruídos instantaneamente (`Object.Destroy(j)`).

---

## 🎛️ 2. Restauração de Configurações & Menu F12 (BepInEx Binds)

* **Vinculação de Controles Mortos:** Na versão original, sliders e botões no menu F12 (como multiplicadores de impulso em cadáveres, forças por calibre, intensidade de desmembramento e efeitos de sangue) eram variáveis desconectadas ou ignoradas nos patches. Todos os parâmetros foram mapeados com `ConfigEntry`.
* **Mapeamento de Calibres Canônicos (`VD_Calibers.json`):** Ajustada a leitura de calibres balísticos do EFT (tratando prefixos `Caliber...`), garantindo que espingardas, fuzis e pistolas apliquem as probabilidades reais configuradas no JSON.

---

## 🎭 3. Animações, Agonia & Máquina de Estados (PuppetMaster & Animator)

* **Sincronismo no Frame 0 da Agonia:** Corrigida a transição de peso da camada de agonia (`Layer 18`). No original, levava 1 segundo para a camada atingir peso 1.0f, criando um "salto visual" ou atraso na animação. A injeção em `dismemberedPlayers` no início do `DeathSetup` aplica peso `1.0f` instantaneamente.
* **Interrupção Suave de Agonia por Tiro (`InterruptAgony`):** Criado sistema dedicado que desativa molas/pins e desliga o `BodyAnimatorCommon`. Quando um bot em agonia leva um tiro adicional, ele desaba naturalmente por gravidade a partir da pose atual no chão, eliminando o reset repentino em T-pose.
* **Agonia com Desmembramento de Braço/Perna:** O mod original desativava a agonia ou quebrava a malha ao desmembrar um membro. A versão refatorada permite que braços e pernas sejam amputados enquanto o resto do corpo executa a animação de agonia.
* **Regra Canônica da Cabeça:** Decapitações de cabeça ignoram a agonia e enviam o bot direto para o ragdoll limpo, evitando distorções no pescoço/crânio.

---

## 🦴 4. Deformações & Eliminação do "Gigantismo"

* **Componente `DismemberedLimbScaler` (Tripla Proteção):** Criado um `MonoBehaviour` anexado aos ossos amputados que força a escala `0.001f` nos eventos `Update()`, `OnAnimatorMove()` e `LateUpdate()`, impedindo que o `Animator` do Unity reescreva a escala `1.0f`.
* **Desconexão Total do Solver PuppetMaster (`isDisconnected = true`):** Músculos dos membros amputados recebem `isDisconnected = true` e multiplicadores zerados (`mappingWeightMlp = 0`). Isso faz com que a fase de `Map` do PuppetMaster ignore completamente os ossos amputados, eliminando distorções de estiramento da malha.
* **Resolução de Keywords de Músculos:** Corrigida a colisão de nomes (por exemplo, a keyword `"rarm"` no mod original fazia match falso com `humanl`**`upperarm`**, desligando os músculos do braço errado).

---

## 💀 5. Balística & Comportamento em Cadáveres (Pós-Morte)

* **Desmembramento Pós-Morte em Corpos no Chão:** No mod original, tiros em cadáveres mortos não desmembravam nada porque o EFT desativa o evento `Player.ApplyDamageInfo`. O `LimbKillPatch` agora intercepta tiros em mortos via `BallisticsCalculator.Shoot` (com `includeInactive: true`) e executa a amputação de braços e pernas no cadáver.
* **Proteção de Tiros na Cabeça de Mortos:** Tiros na cabeça de cadáveres não desativam mais o objeto do `PuppetMaster` nem encolhem o nó da pele, impedindo que o corpo do bot desapareça do chão e garantindo uma reação física natural ao disparo.
* **Cap de Impulso Físico (25%):** Ajustada a força aplicada em cadáveres mortos para evitar que corpos sejam arremessados descontroladamente pelo cenário.

---

## 📊 Tabela de Resumo Técnico

| Recurso / Área | Mod Original | Modded (Refatorado) |
| :--- | :--- | :--- |
| **Vazamento de Memória (RAM)** | Alocações por frame no `Update` e corrotinas sem dispose. | Referências salvas, limpezas estáticas e destruição de instâncias órfãs. |
| **Binds do Menu F12** | Sliders mortos e valores ignorados. | 100% dos parâmetros conectados com `ConfigEntry`. |
| **FPS / PhysX** | Lag spikes e explosões por juntas `Joint` escaladas. | `Joint` destruídos instantaneamente e alocações eliminadas. |
| **Animação de Agonia** | Piscar visual de 1s e reset em T-pose ao levar tiros. | Transição instantânea (frame 0) e queda suave no chão (`InterruptAgony`). |
| **Gigantismo (Membros Inflando)** | Flashes constantes por reescrita do Animator e solver `Map`. | `DismemberedLimbScaler` + `isDisconnected = true` zerando o solver. |
| **Desmembramento Pós-Morte** | Inexistente em cadáveres mortos no chão. | Habilitado para braços e pernas via balística balizada. |
| **Tiro na Cabeça de Mortos** | O corpo inteiro sumia do mapa. | Corpo protegido, visível e reagindo à física. |
