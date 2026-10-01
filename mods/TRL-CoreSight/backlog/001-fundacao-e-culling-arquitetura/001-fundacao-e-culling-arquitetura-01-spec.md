# 001 — fundacao-e-culling-arquitetura

**Mod:** TRL-CoreSight  
**Status:** Em progresso  
**Criado:** 2026-09-15T00:21:00Z  

## Visão geral

Estruturar a base do mod `TRL-CoreSight`, incluindo orquestração do ciclo de vida de raid, gerenciador de oclusão de sombras em interiores e otimização de taxa de animação de bots ocluídos, aliviando simultaneamente o gargalo de CPU (draw calls e Mecanim) e de GPU (shadow cascades).

## Comportamento atual

1. Ao entrar em ambientes fechados (quartos de dormitório, salas de resorts, prédios de Streets), a Unity continua calculando a projeção de sombras da luz solar (`TOD_Sky` / Directional Light) com alcance de 100m a 150m, gerando 4 passadas de cascata sobre árvores, prédios e terrenos externos que estão totalmente invisíveis para o jogador.
2. Todos os bots vivos continuam processando atualizações completas de esqueleto (`Animator`) todo frame, mesmo a mais de 100 metros e atrás de múltiplas paredes sólidas, sobrecarregando a CPU com dezenas de matrizes de transformação de ossos.
3. Não há ajuste dinâmico de `QualitySettings.lodBias` integrado à postura e estado de mira da arma, mantendo o nível de detalhe rígido mesmo quando o jogador não precisa de fidelidade periférica.

## Comportamento desejado

1. **Detecção de Interior e Oclusão de Sombra**: Identificar quando o jogador está sob teto e paredes fechadas; reduzir suavemente `QualitySettings.shadowDistance` para 20m–30m, restaurando para 100m ao ar livre.
2. **Animation LOD em Bots Ocluídos**: Quando um bot estiver fora do cone de visão (Frustum) ou ocluído atrás de paredes sem linha de visão, definir seu `Animator.cullingMode` para `CullUpdateTransforms` e/ou diminuir o tick rate de cálculo de ossos.
3. **Preservação Balística e Tática**: Garantir que as colisões de hitboxes de cabeça, tórax e membros ([`BodyPartCollider`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/BodyPartCollider.cs)) e o movimento físico no NavMesh ([`BotMover`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/BotMover.cs)) continuem 100% íntegros.
4. **LOD Bias Dinâmico**: Diminuir o `lodBias` para 1.0 em corrida/movimento interno e elevar para 2.0 apenas ao mirar (ADS).

## Critérios de aceite

- [ ] Mod inicializa sem erros via BepInEx no SPT 4.0.13.
- [ ] Ao entrar em um quarto fechado, `QualitySettings.shadowDistance` cai para o valor configurado (`InteriorShadowDistance`) sem transições bruscas que causem pop-in visual perceptível.
- [ ] Ao sair para o exterior, a distância de sombra é restaurada para `ExteriorShadowDistance`.
- [ ] Bots ocluídos têm o custo de animação na CPU reduzido sem que suas hitboxes congelem ou desalinhem no espaço.
- [ ] Se o jogador atirar através de uma parede penetrável em um bot ocluído, o tiro registra dano normalmente.
- [ ] Ao mirar (ADS), o `lodBias` sobe para o valor configurado (`AimLODBias`) e retorna ao base ao baixar a arma.
- [ ] **Fika/multiplayer:** A renderização e culling ocorrem 100% no cliente local; cada jogador na raid calcula sua própria oclusão independentemente, sem desync de posições ou estados de bots.
- [ ] **Estado entre raids:** Todas as listas de monitoramento, referências a GameObjects e caches estáticos são desalocados no evento de fim de raid (`OnRaidEnd`), garantindo zero memory leak (OOM).

## Corner cases

- [ ] **Olhar através de Janelas em Interiores**: Se o jogador estiver dentro de uma sala mas olhando pela janela para a rua, a redução de sombras não deve apagar prédios adjacentes visíveis.
- [ ] **Miras Térmicas e Noturnas (NVG/Thermal)**: Em modos térmicos ou ópticas de longo alcance, a redução de LOD de bots deve ser suspensa para garantir silhuetas térmicas precisas.
- [ ] **Morte do Jogador ou Saída Rápida (Alt+F4)**: O watcher deve interromper imediatamente corrotinas e threads de monitoramento sem lançar `NullReferenceException`.
- [ ] **Bosses e Guardas com IA Personalizada**: Garantir que animações de combate corpo-a-corpo ou rituais de cultistas não sofram atraso caso o jogador entre subitamente na sala.

## Fora de escopo

- Modificações no spawner de bots ou nas tabelas de loot (responsabilidade de outros mods como `TRL-DynamicSpawn`).
- Re-baking estático dos pacotes de oclusão do Perfect Culling da BSG nos arquivos de mapa.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Item criado e especificação funcional inicial definida |
