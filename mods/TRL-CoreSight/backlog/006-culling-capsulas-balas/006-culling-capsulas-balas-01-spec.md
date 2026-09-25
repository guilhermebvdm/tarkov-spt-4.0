# 006 — culling-capsulas-balas

**Mod:** TRL-CoreSight  
**Status:** Em progresso  
**Criado:** 2026-09-15T21:46:00Z  
**Atualizado:** 2026-09-15T22:00:00Z  

## Visão geral

Otimizar a CPU e a física da Unity durante tiroteios médios e distantes, **interceptando na raiz a instanciação e o spawn de cápsulas de bala vazias** ([`Player.FirearmController.StartSpawnShell`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L11123) e [`WeaponManagerClass.StartSpawnShell`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/WeaponManagerClass.cs#L595)) quando os disparos ocorrem além do raio de percepção visual do jogador (configurável, padrão: 25–30 metros), aproveitando a infraestrutura nativa do Tarkov ([`EFTHardSettings.FLYING_SHELLS_VISIBLE_DISTANCE`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFTHardSettings.cs#L451)).

## Problema e custo de CPU

1. Toda vez que uma arma dispara no Tarkov, a engine prepara o cartucho vazio na janela de ejeção (`shellport`).
2. Se a cápsula for liberada no mundo:
   - A Unity inicia uma corrotina (`StartCoroutine`);
   - Desvincula o transform do osso da arma (`parent = null`);
   - Registra o objeto no `GameWorld.SpawnShellInTheWorld`;
   - Ativa o componente físico [`Shell.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Shell.cs) na PhysX com forças e torques rotacionais;
   - Realiza detecção contínua de colisão com o solo no layer `LayerMaskClass.ShellsCollisionsMask` com múltiplos quiques;
   - Inicia um temporizador de auto-destruição (`StartAutoDestroyCountDown`).
3. Em tiroteios entre múltiplos bots a 70m–150m de distância, dezenas de corrotinas e corpos rígidos são colocados no mundo simultaneamente para simular o quique de cartuchos que o jogador sequer consegue enxergar, provocando quedas de frametime de CPU e micro-stutters.

## Comportamento desejado (Interceptação Precoce no Spawn)

Em vez de permitir a criação da cápsula no mundo para depois ter que desativar física ou esconder renderers, **o sistema intercepta a criação no frame zero**:

1. **Supressão no Nascimento (Zero Overhead):**
   - Interceptar a rotina de disparo/ejeção (`StartSpawnShell` ou controle do limiar `FLYING_SHELLS_VISIBLE_DISTANCE`):
     - **Disparo Próximo (< 25m ou Arma do Jogador):** Ejeção normal com 100% de física, quiques no piso e efeitos sonoros de metal/plástico.
     - **Disparo Distante (> 25m):** A corrotina de spawn **não é iniciada** (`StartCoroutine` ignorada).
   - **Ganhos Imediatos:**
     - **Zero cálculos na PhysX:** Nenhum teste de colisão ou quique de cartucho entra na simulação.
     - **Zero instanciação no mundo:** O `GameObject` não é desparentado nem inserido na lista do `GameWorld`.
     - **Zero Draw Calls de GPU:** Nenhuma malha 3D de cartucho é submetida ao pipeline de renderização.
     - **Zero risco de cápsulas flutuando:** Como a cápsula nunca é solta no mundo, é impossível haver cartuchos estáticos no ar.
2. **Ciclo da Câmara e Reciclagem no Pool:**
   - A BSG já gerencia a limpeza da janela de ejeção: no próximo disparo ou ciclo de ferrolho, o método `SetPatronInShellPort` detecta se há cartucho anterior retido e chama diretamente `AssetPoolObject.ReturnToPool`, reciclando a memória sem nenhum leak.
3. **Preservação Sonora e Balística:**
   - O áudio do tiro da arma, o estrondo distante, o som sônico do projétil voando e o impacto balístico no alvo continuam **100% inalterados**.
   - Apenas a ejeção visual do cartucho metálico vazio a longa distância é suprimida.

## Critérios de aceite

- [ ] Disparos efetuados pela arma do jogador local sempre mantêm a ejeção visual e física completa dos cartuchos.
- [ ] Disparos efetuados por bots a mais de 25 metros não executam `StartCoroutine` de spawn de cápsula no mundo.
- [ ] Zero cápsulas flutuando no ar ou travadas no cenário.
- [ ] Nenhum erro de pool de objetos (`PoolManagerClass` / `AssetPoolObject`) em tiroteios prolongados.
- [ ] Balística, dano e sons de tiros preservados com 100% de fidelidade.
- [ ] Redução mensurável no tempo de execução de física e corrotinas da Unity durante tiroteios intensos entre bots.

## Corner cases

- [ ] **Miras Telescópicas / Lunetas (PiP):** Mesmo mirando de longe no tiroteio, cartuchos a mais de 25m da arma continuam suprimidos para poupar draw calls enquanto a luneta renderiza o alvo.
- [ ] **Armas Estacionárias (Metralhadoras Pesadas / Utes):** Cápsulas gigantes de 12.7mm de metralhadoras fixas também obedecem ao filtro de distância.
- [ ] **Pane da Arma (Jam / Misfire):** Cartuchos ejetados manualmente ao sanar panes distantes (`SpawnShellAfterJam`) são igualmente interceptados.

## Fora de escopo

- Alteração na trajetória de balas disparadas contra alvos.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Item criado. |
| 2026-09-15 | Lógica aprimorada: substituída a desativação pós-spawn pela interceptação precoce no nascimento (`StartSpawnShell` / `FLYING_SHELLS_VISIBLE_DISTANCE`), eliminando alocação de corrotinas e física sem risco de cápsulas flutuantes. |
