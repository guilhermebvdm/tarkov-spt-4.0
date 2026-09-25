# 001 — fundacao-e-culling-arquitetura · As-Built

**Mod:** TRL-CoreSight  
**Spec funcional:** [001-fundacao-e-culling-arquitetura-01-spec.md](001-fundacao-e-culling-arquitetura-01-spec.md)  
**Spec técnica:** [001-fundacao-e-culling-arquitetura-02-spec-tech.md](001-fundacao-e-culling-arquitetura-02-spec-tech.md)  
**Review técnica:** [001-fundacao-e-culling-arquitetura-03-spec-tech-review-01.md](001-fundacao-e-culling-arquitetura-03-spec-tech-review-01.md)  
**Code review:** [001-fundacao-e-culling-arquitetura-04-code-review-01.md](001-fundacao-e-culling-arquitetura-04-code-review-01.md)  
**Versão entregue:** `0.3.1`  
**Data do build:** 2026-09-15  

> Documentação **pós-implementação**. Reflete o estado real do código compilado e entregue em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`. Quando o conteúdo aqui divergir da spec técnica, este documento prevalece (a spec técnica é o planejamento inicial; o as-built é o produto real construído).

---

## 1. Contexto e Escopo Construído

O item 001 estabeleceu a espinha dorsal de otimização de renderização e CPU do `TRL-CoreSight`. Além do culling dinâmico de sombras em interiores e do throttling de animação esquelética de bots ocluídos planejados originalmente, a arquitetura foi expandida para incorporar o **Declutter** (limpeza assíncrona de detritos inúteis de mapas) e o **Smart PiP** (desativação de luz volumétrica redundante em lunetas telescópicas com redução de LOD periférico).

O binário foi compilado em modo Release, mantido isolado dentro do repositório em conformidade com as diretrizes do workspace:
- **Binário gerado:** `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`
- **Tamanho:** 47.104 bytes
- **Compilação:** 0 erros, 0 avisos.

---

## 2. Inventário de Arquivos do Mod

| Ação | Arquivo | Resumo da Entrega |
|---|---|---|
| CRIADO | [`modded/Plugin.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Plugin.cs) | Entrypoint BepInEx (`com.trl.coresight` v0.3.0), inicialização da configuração e registro de patches Harmony. |
| CRIADO | [`modded/Configuration/ModConfig.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Configuration/ModConfig.cs) | Centralização das propriedades F12 com seções categorizadas, limites `AcceptableValueRange` e tags para ConfigurationManager. |
| CRIADO | [`modded/Core/PerformanceManager.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/PerformanceManager.cs) | Orquestrador MonoBehaviour principal anexado ao `GameWorld`. Coordena o ciclo de vida, interpolação de `lodBias` e HUD de debug. |
| CRIADO | [`modded/Core/InteriorOcclusionWatcher.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/InteriorOcclusionWatcher.cs) | Detector de ambiente fechado via raycasts verticais/cardeais (0.5s) e transição suave de `shadowDistance` (80m/s). |
| CRIADO | [`modded/Core/BotPerformanceLimiter.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/BotPerformanceLimiter.cs) | Otimizador de esqueleto de bots: culling de frustum e distância (>75m) chaveando `Animator.cullingMode` com zero alocação de GC. |
| CRIADO | [`modded/Core/DeclutterManager.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/DeclutterManager.cs) | Limpeza cooperativa amortizada (250 objetos/frame) com proteção a 22+ componentes vitais e restauração ao fim da raid. |
| CRIADO | [`modded/Patches/RaidLifecyclePatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Patches/RaidLifecyclePatches.cs) | Patches Harmony em `GameWorld.OnGameStarted` e `GameWorld.OnDestroy` para inicialização e teardown limpo. |
| CRIADO | [`modded/Patches/OpticVolumetricsPatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Patches/OpticVolumetricsPatch.cs) | Patch Harmony em `OpticComponentUpdater.LateUpdate` para desativar a luz volumétrica secundária duplicada na luneta. |
| CRIADO | [`modded/TRL-CoreSight.csproj`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/TRL-CoreSight.csproj) | Projeto C# configurado com referências automáticas relativas à instalação do SPT. |

---

## 3. Critérios de Aceite vs. Estado Real

| Critério de Aceite (Spec Funcional) | Status no Código | Evidência / Validação |
|---|---|---|
| Inicialização sem erros no SPT 4.0.13 via BepInEx | ✅ Atendido | Compilado contra Assembly EFT 0.16.9, carregamento via `Plugin.Awake()`. |
| Redução suave de `shadowDistance` em quartos fechados sem pop-in | ✅ Atendido | Interpolação via `Mathf.MoveTowards` a 80m/s em `InteriorOcclusionWatcher.cs:78`. |
| Restauração para `ExteriorShadowDistance` ao ar livre | ✅ Atendido | Transição automática ao falhar nos testes de raio cardeais em `InteriorOcclusionWatcher.cs:64`. |
| Redução de CPU nos bots ocluídos sem congelar hitboxes | ✅ Atendido | Manipulação restrita de `animator.cullingMode` para `CullUpdateTransforms`. Colisores e físicas permanecem 100% intactos. |
| Registro de tiro através de paredes penetráveis | ✅ Atendido | `BodyPartCollider` e `CharacterController` nunca são desativados. |
| Elevação de `lodBias` no ADS e retorno ao base | ✅ Atendido | `HandleDynamicLOD` em `PerformanceManager.cs:63-92` com interpolação de 4.0/s. |
| Compatibilidade Multiplayer Fika (cálculo client-side) | ✅ Atendido | Nenhuma mutação em estado sincronizado de rede; operações puramente locais de câmera e renderização. |
| Zero memory leak / teardown completo entre raids | ✅ Atendido | `PerformanceManager.Cleanup()` chamado pelo `GameWorld.OnDestroy` restaura valores e destrói instâncias. |

*Nota:* Todos os critérios acima estão implementados e verificados estaticamente; o status geral permanece **🔵 Em andamento** aguardando a rodada de validação empírica em raid real pelo usuário.

---

## 4. Propriedades F12 Configurações (BepInEx)

| Seção | Nome | Tipo | Padrão | Faixa | Efeito |
|---|---|---|---|---|---|
| 1. Geral | `ModEnabled` | bool | `true` | - | Chave mestre liga/desliga de todas as otimizações |
| 1. Geral | `DebugMode` | bool | `false` | - | Exibe HUD de telemetria verde no canto da tela em raid |
| 2. Sombras | `EnableInteriorShadowCulling` | bool | `true` | - | Ativa redução de sombras sob tetos/paredes |
| 2. Sombras | `InteriorShadowDistance` | float | `25.0` | 10.0 a 50.0 | Distância máxima de sombra em áreas fechadas |
| 2. Sombras | `ExteriorShadowDistance` | float | `100.0` | 50.0 a 200.0 | Distância de sombra restaurada ao ar livre |
| 3. Bots | `EnableBotAnimationLOD` | bool | `true` | - | Ativa culling de Mecanim de bots fora de visão |
| 3. Bots | `BotOcclusionCheckInterval` | float | `0.2` | 0.05 a 1.0 | Frequência de atualização da visibilidade dos bots |
| 4. LOD | `EnableDynamicLODBias` | bool | `true` | - | Ajusta qualidade geométrica dinamicamente |
| 4. LOD | `BaseLODBias` | float | `1.0` | 0.5 a 2.0 | LOD padrão durante caminhada/corrida |
| 4. LOD | `AimLODBias` | float | `2.0` | 1.5 a 4.0 | LOD aprimorado ao mirar (ADS) |
| 5. Declutter | `EnableDeclutter` | bool | `true` | - | Remoção de lixo estático e decalques inúteis |
| 6. Smart PiP | `EnableSmartPiP` | bool | `true` | - | Otimização avançada de mira telescópica |
| 6. Smart PiP | `StripOpticVolumetrics` | bool | `true` | - | Desativa luz volumétrica duplicada na luneta |

---

## 5. Mudanças Posteriores (Rodada 01 de Code Review)

A rodada 01 de revisão de código ([001-fundacao-e-culling-arquitetura-04-code-review-01.md](001-fundacao-e-culling-arquitetura-04-code-review-01.md)) levantou 3 pontos de refinamento que foram corrigidos e validados diretamente no build `0.3.1`:

- **`CR-01-01` (Aplicado):** Blindagem da obtenção da câmera ativa no `BotPerformanceLimiter.cs`. Agora implementa `GetActiveCamera()` com resolução dinâmica via `CameraClass.Instance.Camera` (padrão EFT) e fallback para `Camera.main`, revalidando caso a câmera seja alterada em runtime ou modo espectador do FIKA.
- **`CR-01-02` (Aplicado):** No `InteriorOcclusionWatcher.cs`, caso o jogador seja eliminado dentro de um edifício (`!IsAlive`), `Restore()` é invocado imediatamente no mesmo frame, garantindo que as sombras não permaneçam travadas em 25m na tela pós-morte.
- **`CR-01-03` (Aplicado):** Remoção do campo inerte `_mainCamera` no `InteriorOcclusionWatcher.cs`.

---

## 6. Histórico de Versões e Builds

| Data | Versão | Acontecimento |
|---|---|---|
| 2026-09-15 | `0.1.0` | Arquitetura base, ciclo de raid, sombras de interior e culling de bots. |
| 2026-09-15 | `0.2.0` | Adição do `DeclutterManager` (remoção segura de detritos cosméticos). |
| 2026-09-15 | `0.3.0` | Adição do `Smart PiP` e `OpticVolumetricsPatch` (otimização de lunetas). Binário `TRL-CoreSight.dll` gerado. |
| 2026-09-15 | `0.3.1` | Conclusão das revisões técnicas, aplicação de CR-01-01/02/03 e novo build limpo de 47.104 bytes. |
