# 003 — culling-sombras-luzes-locais

**Mod:** TRL-CoreSight  
**Status:** Em progresso  
**Criado:** 2026-09-15T21:00:00Z  
**Atualizado:** 2026-09-15T22:25:00Z  

## Visão geral

Otimizar o pipeline de iluminação secundária em ambientes fechados e corredores escuros (Interchange, Dorms, Health Resort, The Lab), gerenciando dinamicamente a projeção de sombras em tempo real de fontes de luz pontuais (*Point Lights*) e refletores (*Spotlights*) secundárias ou de baixa intensidade, mantendo 100% da iluminação visual e da cor do ambiente sem pagar o custo exorbitante de desenho de sombras geométricas (cubemaps de 6 passadas por lâmpada) na GPU, com blindagem ativa contra vazamento de luz (*light leaking*) através de paredes finas.

## Comportamento atual

1. Centenas de lâmpadas de teto fluorescentes, luzes de emergência vermelhas e luminárias secundárias em corredores internos estão configuradas nativamente com projeção de sombras em tempo real (`Light.shadows = LightShadows.Hard` ou `LightShadows.Soft`).
2. Uma única lâmpada do tipo *Point Light* com projeção de sombra exige que a GPU renderize toda a geometria do quarto ou corredor **6 vezes adicionais** (uma passada para cada face do cubemap de sombra).
3. Em áreas como o subsolo do Interchange ou corredores do Resort em Shoreline, dezenas dessas lâmpadas atuam simultaneamente, provocando quedas severas de taxa de quadros e saturação de draw calls na GPU, mesmo que as sombras projetadas por essas lâmpadas fracas sejam quase imperceptíveis para o jogador.

## Comportamento desejado

1. **Varredura e Classificação de Luzes Locais:**
   - Ao carregar o mapa (`OnGameStarted`), identificar todas as fontes de luz (`UnityEngine.Light`) da cena que sejam do tipo `LightType.Point` ou `LightType.Spot`.
   - **Exclusão Estrita:** Ignorar completamente a luz direcional do sol (`LightType.Directional` / `TOD_Sky`), lanternas táticas de armas/capacetes (`TacticalComboVisualController`), luzes de focinho de armas (`MuzzleJet`) e luzes vinculadas a jogadores/bots (`Player`).
2. **Modos Configuráveis com Prevenção de Light Leaking:**
   - O jogador pode escolher entre modos de otimização no menu F12:
     - **`DisableWeakShadowsOnly` (Padrão):** Desativa sombras (`LightShadows.None`) apenas para luzes com raio curto ($\le 6$m) E intensidade fraca ($\le 1.2$), onde a atenuação de distância da luz impede qualquer vazamento visual perceptível em paredes vizinhas.
     - **`DowngradeSoftToHard`:** Converte sombras suaves (`Soft`) de todas as luzes pontuais/spot secundárias para sombras duras (`Hard`). Elimina o custo de amostragem PCF multifocal da GPU e reduz em ~50% a carga de sombreamento com **zero risco** de vazamento de luz através de paredes.
     - **`DisableAllSecondaryShadows`:** Desliga sombras de todas as point/spot lights estáticas do mapa (máximo ganho de FPS para GPUs de entrada).
3. **Preservação Visual e Jogabilidade:**
   - A cor, a intensidade, o alcance e os efeitos de cintilação (`FlickeringLights`) permanecem intactos.
   - O ambiente não fica escuro nem distorcido.
4. **Restauração Segura:**
   - Cachear o estado original (`LightShadows`) de cada luz modificada para restaurar integralmente no `Cleanup()` ao final da partida ou ao desativar no F12.

## Critérios de aceite

- [ ] Mod cataloga lâmpadas pontuais e refletores locais no início da raid em segundo plano sem travar o carregamento do jogo.
- [ ] Lâmpadas secundárias fracas deixam de projetar sombras em tempo real, gerando redução consistente de frametime de GPU em interiores densos.
- [ ] O modo padrão impede vazamento visível de luz através de paredes de quartos adjacentes (sem quebrar a escuridão tática do Tarkov).
- [ ] Lanternas montadas em armas e capacetes de jogadores e bots continuam projetando sombras normalmente.
- [ ] Todas as opções e limiares são ajustáveis em tempo real via menu F12 (BepInEx) e refletem imediatamente no jogo.
- [ ] Ao término da raid, o estado original de todas as luzes é restaurado deterministamente sem vazamento de memória.

## Corner cases

- [ ] **Luzes com Animação (FlickeringLights):** Devem manter sua oscilação periódica de intensidade sem interferência.
- [ ] **Luzes Dinâmicas de Raid:** Explosões, chamas e sinalizadores não devem ter suas configurações alteradas.
- [ ] **Troca de Configuração em Raid:** Ligar/desligar no F12 no meio da partida deve aplicar ou restaurar o estado original de imediato.

## Fora de escopo

- Alteração na luz solar global (`TOD_Sky`), que já é gerenciada pelo Item 001.
- Modificação de shaders de post-processing do EFT.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Item criado e especificação funcional inicial definida. |
| 2026-09-15 | Adicionadas mitigações de light leaking e modos configuráveis (SoftToHard, WeakOnly). |
