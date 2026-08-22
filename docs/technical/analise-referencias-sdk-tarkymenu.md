---
title: Análise de Referências Técnicas — EscapeFromTushonka-SDK e Tarky-Menu
date: 2026-08-18
status: 🟢 Vivo
authors: Antigravity + Guilherme
---

# Análise de Referências Técnicas: EscapeFromTushonka-SDK e Tarky-Menu

Análise comparativa e arquitetural dos repositórios **EscapeFromTushonka-SDK** e **Tarky-Menu** frente às diretrizes técnicas, padrões de engenharia e ciclo de desenvolvimento adotados no ecossistema `tarkov-spt-4.0`.

---

## 1. Sumário Executivo

| Pergunta-Chave | Veredito | Resumo |
|---|---|---|
| **Estamos usando boas práticas e a estrutura correta?** | **SIM (Exemplar)** | Nossa arquitetura é substancialmente mais avançada, segura e modular que os padrões comunitários convencionais. O harness previne ativamente problemas que afetam o ecossistema SPT (leaks de raid, corrupção de rede Fika, caminhos hardcoded, mutação cega de estado). |
| **Os 2 repositórios são úteis para construção de mods?** | **SIM (Utilidade Específica)** | **`EscapeFromTushonka-SDK`** é indispensável para mods focados em **Asset/Bundle 3D** (armas, vestimentas, mapas, VFX). **`Tarky-Menu`** funciona como um valioso **catálogo de snippets e receitas de runtime/sandbox** (spawns in-raid, pooling de bundle, manipulação de mundo e IA). |
| **Vale a pena rever nossa abordagem atual?** | **PARCIALMENTE (Evolução)** | Não mudar o fluxo de código/build (que é superior), mas **expandir** para cobrir: (1) pipeline de assets 3D via Unity Editor; (2) aproveitar receitas úteis de runtime para enriquecer nossas ferramentas de debug e testes locais. |

---

## 2. Anatomia dos Repositórios Analisados

### A. EscapeFromTushonka-SDK (`references/EscapeFromTushonka-SDK`)
- **Natureza:** Projeto Unity Editor (Unity `2022.3.43f1` LTS — versão idêntica à do EFT 0.16.x).
- **Conteúdo Principal:**
  - **Stubs de Engine/Assembly:** Mais de 500 classes e stubs do Tarkov (`WeaponPrefab`, `BotZone`, `PatrolPoint`, `DressItem`, `TOD_Sky`, `BetterAudio`, etc.), permitindo abrir prefabs e scenes do Tarkov dentro do Unity Editor sem erros de scripts ausentes (*missing scripts*).
  - **Editor Tooling & Scripts Comunitários:** Integrações de ferramentas consagradas da cena de modding:
    - `EFTCleaner.cs` (DrakiaXYZ): Limpeza de metadados e componentes residuais pós-AssetRipper.
    - `StaticDataEditor.cs` (Choccy): Configuração de static data para armas e animações.
    - Ferramentas de Vestimentas e Rigging (Groovey): `CustomRigLayoutEditor`, `GameReadyCharacterArmsCreator`, `GameReadyDressObjectCreator`, `GameReadyHeadClothesCreator`, `GameReadySkinnedObjectCreator`, `TagbankVoiceCreator`.
    - `Unity.AssetBundleBrowser`: Configuração visual e compilação de AssetBundles (`.bundle`).
- **Público / Casos de Uso:** Criação de conteúdo que envolve assets visuais/sonoros novos ou modificados:
  - Armas de fogo e armas brancas customizadas.
  - Modelos de personagens, coletes, rigs, mochilas e luvas em primeira pessoa.
  - Mapas/cenários customizados e efeitos de partículas (VFX).
  - Voice lines e pacotes de áudio (`TagBank`).

### B. Tarky-Menu (`references/Tarky-Menu`)
- **Natureza:** Mod client BepInEx em C# voltado para testes, sandbox e debug in-game.
- **Conteúdo Principal:**
  - **Comandos de Console In-Game:** Registrados via `ConsoleScreen.Processor.RegisterCommandGroup<ConsoleCommands>()`:
    - `SpawnNPC`: Geração de bots via `BotWaveDataClass` e `BotsController.BotSpawner.ActivateBotsByWave`, incluindo detecção dinâmica do singleton do **Fika** (`Fika.Core.Main.GameMode.IFikaGame`) via reflexão.
    - `SpawnItem`: Criação de itens dinâmicos com instanciação e carregamento assíncrono de bundles via `PoolManagerClass.LoadBundlesAndCreatePools` + arremesso no mundo via `GameWorld.ThrowItem`.
    - `ChangeTime` / `SetWeather`: Manipulação de data, hora e clima in-raid.
    - `extract`: Finalização segura de raid via `EndByExitTrigerScenario.GInterface146.StopSession`.
  - **Módulos de Gameplay / Utilities:**
    - `NPC_Controller`: Teleporte de bots por facção (USEC, BEAR, Scav, Boss) e teleporte para a mira (raycast a partir do `FirearmController.Fireport`). Sistema de chams visual alterando shaders dos skins para detecção.
    - `Health`: Godmode, Demigod (com troca de layer para `PlayerSpiritAura`), remoção de fall damage e desativação de metabolismo.
    - `InfiniteAmmo`: Harmony patch em `BallisticsCalculator.Shoot` reinserindo munição consumida no `MagazineItemClass` ou na câmara da arma.
    - `Noclip`: Desativação de colisões e movimentação livre de câmera/player.
    - `RecoilControlSystem` / `WeaponUtils`: Modificação de taxas de disparo, recuo e eliminação de superaquecimento/engasgo.

---

## 3. Comparativo Arquitetural: Nossa Abordagem vs. Referências

| Aspecto | Abordagem Atual (`tarkov-spt-4.0`) | Tarky-Menu | EscapeFromTushonka-SDK | Avaliação / Diagnóstico |
|---|---|---|---|---|
| **Formato de Projeto C#** | Modern SDK-style (`Microsoft.NET.Sdk`), .NET 9 (Server) e .NET Framework 4.7.2 (Client) via `Directory.Build.props`. | Legado MSBuild (.NET 4.7.2 tradicional sem SDK-style). | Projeto Unity (.sln gerado pelo Unity Editor). | **Nossa abordagem é superior:** SDK-style é mais limpo, moderno e livre de GUIDs redundantes. |
| **Resolução de Dependências** | **Zero Hardcode:** Resolução automatizada via `.spt-path` e `/compile-mod` populando `References/` dinamicamente. | **Hardcoded:** `<HintPath>X:\SPT\...</HintPath>` estático em 25 referências. | Referencia assemblies compilados no projeto Unity (`Assets/Plugins`). | **Nossa abordagem é superior:** Portabilidade total entre máquinas sem quebrar compilação. |
| **Build & Deploy** | CLI padronizado (`/compile-mod`), isolamento de build em `builds/`, gate semântico de versão (SemVer). | PostBuildEvent com `copy` fixo para `X:\SPT\...`. | Build manual de bundles via Unity AssetBundleBrowser. | **Nossa abordagem é superior:** Controle de versão real, rastreabilidade e sem dependência de caminhos absolutos. |
| **Tratamento de Ciclo de Vida** | **Rigoroso:** Hooks em `GameWorld.OnDestroy` e `BaseLocalGame.Stop`, teardown idempotente, prevenção ativa de leaks (AP-01). | Variáveis estáticas e loops em `Update()` verificando `Singleton<GameWorld>.Instantiated` sem limpeza explícita ao fim da raid. | N/A (Editor). | **Nossa abordagem é superior:** Previne retenção de memória e referências zumbis entre raids consecutivas. |
| **Filtro de Contexto / Fika** | Obrigatório filtrar `IsYourPlayer` (AP-02) e prevenção contra corrupção de rede (AP-11). | Filtra `x.IsYourPlayer` em teleportes e faz duck-typing em `IFikaGame` para bots. | N/A (Editor). | **Nossa abordagem é superior:** O Tarky-Menu possui boa compatibilidade pontual com Fika, mas sem as proteções de rede e estado que nosso repo exige. |
| **Engenharia Reversa & Decompile** | 8.683 tipos descompilados e indexados (`types-index.json`), grafos de chamada (MCP), mapas de deofuscação 4.1. | Dependência de inspecionar DLL compilada ou código empírico. | Stubs extraídos via AssetRipper para permitir compilação no Unity. | **Nossa abordagem é superior para código C#; o SDK é superior para assets 3D.** |
| **Criação de Assets 3D / Bundles** | Atualmente focado em lógica C# e JSON (ausência de pipeline de AssetBundles documentado). | N/A (Consome bundles existentes do jogo). | **Especialista:** Pipeline completo para importar 3D, configurar rig/cloth/armas e empacotar `.bundle`. | **O SDK preenche uma lacuna real** caso o repositório deseje construir mods com modelos 3D proprietários. |

---

## 4. O que Podemos Aproveitar e Integrar

### 4.1. Do `EscapeFromTushonka-SDK` (Pipeline de Assets 3D)
1. **Padronização para Criação de Conteúdo 3D:**
   - Se algum mod do workspace necessitar de modelos 3D customizados (ex.: novas miras, novos carregadores, itens cosméticos, roupas personalizadas ou armaduras), o `EscapeFromTushonka-SDK` é a **ferramenta canônica recomendada**.
   - As ferramentas do Groovey (`GameReadyDressObjectCreator`, `GameReadyCharacterArmsCreator`) automatizam o trabalho manual de peso de vértices e alinhamento de bones do Tarkov.
2. **Exportação de AssetBundles Compatíveis:**
   - Uso da versão correta da engine (`Unity 2022.3.43f1`), garantindo que shaders (como `EFT/Standard`, `PBR/Standard`) e dependências binárias serializem sem quebrar no cliente EFT 0.16.x.

### 4.2. Do `Tarky-Menu` (Snippets e Ferramentas de Teste/Debug)
1. **Spawn In-Raid de Itens com Pooling de Bundles:**
   - O snippet de `ConsoleCommands.SpawnItem` demonstra como criar um item via `ItemFactoryClass`, aguardar o pool assíncrono (`PoolManagerClass.LoadBundlesAndCreatePools`) e injetar via `GameWorld.ThrowItem` sem crash de missing assets.
2. **Invocação Dinâmica de Waves de Bots com Suporte FIKA:**
   - O padrão utilizado em `ConsoleCommands.SpawnNPC` para resolver `Fika.Core.Main.GameMode.IFikaGame` em runtime via reflexão sem gerar dependência dura de compilação quando o Fika não estiver presente.
3. **Mecanismo de Noclip e Raycasting do Cano da Arma:**
   - O cálculo do ponto de impacto baseado em `FirearmController.Fireport.position` e `FirearmController.WeaponDirection` utilizando as máscaras de colisão do Tarkov (`1082202128`).
4. **Manipulador de Materiais/Shaders em Runtime:**
   - A iteração sobre `LoddedSkin._lods` e `SkinnedMeshRenderer` em `NPC_Controller` para aplicação de shaders alternativos em tempo real.

---

## 5. Antipatterns Identificados nas Referências (O que NÃO Reproduzir)

Embora funcionais para prototipagem rápida, os repositórios trazem práticas que violam os padrões do nosso repo:

1. **Caminhos de Build Hardcoded (`X:\SPT\`):**
   - No `Tarky-Menu.csproj`, todos os `<HintPath>` e `PostBuildEvent` apontam para uma partição fixa do autor original.
   - *Regra do Repo:* Sempre usar o padrão de `.spt-path` e compilação isolada via `Directory.Build.props` / `/compile-mod`.
2. **Mutação Contínua no `Update()` Sem Guards:**
   - Métodos como `godMod()`, `NoRecoil()` e `Stamina()` no `Tarky-Menu` reescrevem multiplicadores e propriedades a cada frame, consumindo CPU desnecessária e impedindo o disparo de side-effects do jogo.
   - *Regra do Repo:* Seguir o princípio de **API Canônica** (AP-04) e patchear pontos de transição via Harmony em vez de pollings pesados no `Update()`.
3. **Ausência de Teardown de Estado no Fim de Raid (AP-01):**
   - Variáveis estáticas de controle e listas persistem na memória após a extração, com potencial de causar memory leaks ou comportamentos fantasmas em raids subsequentes.
4. **Uso de LINQ e Alocações em Hot Paths (C# Best Practices §1):**
   - Chamadas a `Where()`, `FirstOrDefault()` e `FindObjectsOfType<GameObject>()` disparadas por hotkeys ou frames geram tráfego excessivo de Garbage Collection no Mono da Unity.

---

## 6. Conclusão e Recomendações

1. **Validação da Estrutura Atual:**
   - Nossa documentação técnica (`docs/technical/`), diretrizes de antipatterns (`spt-antipatterns.md`), regras de C# (`csharp-mod-best-practices`) e automações (`.claude/commands/`) estão **perfeitamente alinhadas com as melhores práticas de engenharia de software e modding de SPT 4.0**. Não há necessidade de reformulação estrutural do que foi construído.

2. **Ações Práticas Recomendadas:**
   - **Manter os 2 repositórios em `references/`** como fontes de consulta técnica:
     - `references/EscapeFromTushonka-SDK/`: Referência para modding de modelos 3D, skins, armas e AssetBundles.
     - `references/Tarky-Menu/`: Catálogo de snippets práticos de manipulação de engine, bots, itens e física do EFT.
   - **Documentar uma trilha de Asset Modding:** Criar oportunamente um doc técnico (ex.: `docs/technical/spt4-unity-asset-creation.md`) descrevendo o fluxo de integração entre Unity 2022.3.43 (Tushonka SDK) e os plugins BepInEx do repositório.

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-08-22 | Guilherme | chore(CustomClasses): regen graph (v0.16.7) |
