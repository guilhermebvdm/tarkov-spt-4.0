# 002 — culling-loot-dinamico-ads

**Mod:** TRL-CoreSight  
**Status:** Em progresso  
**Criado:** 2026-09-15T20:45:00Z  

## Visão geral

Implementar um sistema de **Culling Dinâmico de Loose Loot** (itens soltos no mapa) com escalonamento por tamanho de inventário (`Grid Size`) e suspensão automática ao mirar com a arma (**ADS Bypass**), aliviando centenas de *Draw Calls* da GPU e ciclos de renderização da CPU em mapas densos (como Streets of Tarkov e Interchange) sem prejudicar a visibilidade do jogador ao usar lunetas telescópicas ou inspecionar o cenário à distância.

## Comportamento atual

1. Mapas como Streets of Tarkov, Interchange e Reserve contêm entre 1.500 e 4.000 itens de *loose loot* espalhados em prateleiras, mesas, chão e caixas abertas.
2. Cada pequeno item (parafuso de 3cm, bala de 9mm, pacote de fósforo, pilha) possui ao menos um `MeshRenderer`, materiais PBR com shaders complexos e um colisor de física.
3. O motor da Unity continua enviando dados de renderização e realizando testes de oclusão para milhares desses itens minúsculos a 40m–80m de distância, mesmo quando o jogador está correndo com a arma abaixada e o item ocupa menos de 1 pixel na tela.
4. Mods genéricos de desativação de loot cortam itens indistintamente por distância, fazendo armas grandes e mochilas de alto valor desaparecerem abruptamente de bancadas quando vistas de longe, gerando *pop-in* agressivo e quebrando a imersão de franco-atiradores.

## Comportamento desejado

1. **Classificação por Tamanho de Grid (Zero-Cost):** 
   - Utilizar as dimensões nativas do item (`Item.CalculateCellSize()`):
     - **Pequeno (1x1 ou 1x2):** Balas soltas, parafusos, porcas, chaves, moedas, pilhas, fósforos. Distância máxima de renderização configurável (padrão: 25m).
     - **Médio (2x2 ou 2x3):** Maletas médicas, kits de reparo, capacetes, comida/bebida volumosa, ferramentas. Distância máxima de renderização configurável (padrão: 45m).
     - **Grande (2x4 ou maior):** Armas longas, rifles de precisão, mochilas e coletes. **Sempre visíveis** (nunca ocultados à distância).
2. **ADS Bypass (Prioridade de Visada / Luneta):**
   - No momento em que o jogador mirar com a arma (`ProceduralWeaponAnimation.IsAiming == true`), o culling é temporariamente suspenso para todos os itens ou seu alcance é expandido para o horizonte visual, garantindo que o jogador veja 100% dos itens ao escanear com lunetas ou miras reflexivas.
   - Ao baixar a arma, o culling retorna gradualmente de forma amortizada.
3. **Integridade Física e Interação 100% Preservadas:**
   - O componente físico (`Collider` / `Rigidbody`) do [`LootItem`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs) **nunca** é desativado ou alterado, garantindo que o loot não caia através do piso ou gere instabilidade de física.
   - Apenas o `Renderer.enabled` é alternado.
   - A coleta, interação com tecla 'F' e detecção de proximidade permanecem idênticas ao jogo original.
4. **Isolamento de IA:**
   - Bots não utilizam os olhos da câmera para achar loot (leem diretamente de `GameWorld.LootItems` em memória), portanto o comportamento da IA não sofre qualquer impacto.

## Critérios de aceite

- [ ] Mod inicializa sem erros e registra o novo subsistema `LootCullingManager` anexado ao `GameWorld`.
- [ ] Itens pequenos (1x1 e 1x2) soltos no mapa a mais de 25m têm seus renderers desativados enquanto o jogador está navegando com a arma abaixada.
- [ ] Ao se aproximar a menos de 25m de um item pequeno, sua renderização é reativada instantaneamente sem stuttering.
- [ ] Armas longas (2x4+) e mochilas volumosas permanecem sempre visíveis em qualquer distância.
- [ ] Ao levantar a mira da arma (ADS), os itens dentro do cone de visada têm sua renderização reativada imediatamente, permitindo inspecionar bancadas com luneta.
- [ ] Ao baixar a arma (sair do ADS), os itens distantes voltam a ser ocultados de forma amortizada (sem pico de frame).
- [ ] Itens nunca caem no limbo ou atravessam superfícies (colisores físicos 100% preservados).
- [ ] Ao pegar um item que estava com renderer oculto ou visível, o inventário adiciona o item normalmente e a instância é removida do rastreador sem memory leak.
- [ ] Ao término da raid (`OnDestroy`), todos os renderers são restaurados e as listas internas liberadas.

## Corner cases

- [ ] **Loot dentro de Contêineres Fechados:** Apenas `LootItem` (itens soltos no mundo / *loose loot*) é gerenciado; itens guardados dentro de caixas, mochilas e jaquetas já não possuem renderers 3D ativos no mundo.
- [ ] **Loot Arremessado pelo Jogador:** Quando o jogador descarta uma arma ou mochila no chão (`GameWorld.ThrowItem`), o item é registrado dinamicamente no gerenciador de culling.
- [ ] **Troca Rápida de Miras (ADS Spammer):** Pressionar repetidamente o botão de mira não pode provocar alocação excessiva de memória (GC) nem travar corrotinas em loop.
- [ ] **Miras Noturnas e Térmicas (NVG / Thermal):** Em miras térmicas (onde itens podem ter assinaturas térmicas), a visibilidade deve obedecer à suspensão imediata de ADS.

## Fora de escopo

- Alteração nas probabilidades de spawn de loot do servidor SPT.
- Modificação de contêineres estáticos (`LootableContainer`) que já possuem oclusão própria da BSG.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Item criado e especificação funcional inicial definida com base no feedback de ADS e escalonamento de tamanho. |
