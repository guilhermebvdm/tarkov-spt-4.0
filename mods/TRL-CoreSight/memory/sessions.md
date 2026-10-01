# Memory — TRL-CoreSight

Memória cronológica de sessões de trabalho (timestamps em GMT-3). Cada entrada resume as alterações efetuadas, decisões de arquitetura e estado atual. Atualizada ao fim de cada sessão de trabalho.

---

## Estado Atual (Snapshot ao Fim da Sessão — 2026-09-15)

**Evolução do Mod TRL-CoreSight (`v0.3.0`)**:
- **Identity**: `TRL-CoreSight` (Client BepInEx DLL: `TRL-CoreSight.dll`). Compatível com SPT 4.0.13 e EFT 0.16.9 / FIKA.
- **Subsistemas Integrados**:
  1. **Interior Occlusion Watcher**: Culling dinâmico de distância de sombras solares (`shadowDistance`) em ambientes internos.
  2. **Bot Performance Limiter**: Culling de animações e transformações de ossos de bots distantes/ocluídos (`AnimatorCullingMode.CullUpdateTransforms`).
  3. **Dynamic Camera LOD Bias**: Ajuste de qualidade geométrica (LOD) dinâmico entre hipfire, ADS normal e luneta telescópica.
  4. **Declutter & Mesh Culling Inteligente (`v0.2.0`)**: Varredura *time-sliced* Zero-GC para remoção de detritos cosméticos (papéis, lixo, cartuchos gastos, comida cenográfica, cacos, poças e decalques), com proteção estrita atualizada para EFT 0.16.9 (Tripwires, Airdrops/BTR, missões, armas fixas, extrações) e whitelist por mapa com Labyrinth e Lab desativados por padrão.
  5. **Smart PiP Optimizer (`v0.3.0`)**: Otimizador inteligente de Picture-in-Picture que substitui abordagens destrutivas (como o *Dynamic-External-Resolution*):
     - Detecção precisa de lunetas telescópicas com zoom via `ProceduralWeaponAnimation.SightNBone.IsOptic`.
     - Corte suave de sombras externas para 35m durante o ADS de luneta (`PiPShadowDistance`), economizando dezenas de draw calls de sombra.
     - Redução suave de LOD Bias externo (`PiPExternalLODBias` = 0.75) via lerp `Mathf.MoveTowards`.
     - Supressão de luz volumétrica duplicada na câmera da luneta (`OpticVolumetricsPatch` no `OpticComponentUpdater.LateUpdate`).
     - **100% compatível com DLSS/FSR**, sem telas pretas e sem memory thrashing.
- **Build Status**: Compilação `v0.3.0` isolada com 0 erros e 0 avisos em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (46.5 KB).

---

## Sessão 2026-09-15

### Decisões de Arquitetura
1. **Nome do Mod**: Escolhido `TRL-CoreSight` para unificar *Core* (CPU / Bots / Multithreading) e *Sight* (Visão / Sombras / LOD / Culling).
2. **Setup do Workflow**: Adoção estrita da estrutura canônica de backlog com `001-fundacao-e-culling-arquitetura`.
3. **Isolamento de Compilação**: Todos os builds serão gerados exclusivamente dentro de `mods/TRL-CoreSight/builds/` ou `modded/bin/Release/`, cumprindo as regras locais do repositório.
4. **Mapeamento da Stack de IA e Motor**: Analisados `spt-bigbrain`, `SAIN (modded-multithread)` e `ORBIT`. O CoreSight atuará no ponto cego do motor Unity (`Animator.cullingMode` e `QualitySettings.shadowDistance`), sem sobrescrever o throttling tático do SAIN nem o roteamento de células do ORBIT.
5. **Download e Análise do Simple-Declutter (`somtam.simple.declutter`)**: Download concluído e código-fonte inspecionado. Incorporada a taxonomia de proteção (`isBadThing`) e o dicionário de detritos.
6. **Implementação do Declutter no CoreSight (`v0.2.0`)**: 
   - Criado `DeclutterManager.cs` com varredura fatiada no tempo (*time-sliced*, 250 objetos/frame) sem congelamento na entrada da raid.
   - Adicionada blindagem estrita para componentes novos do EFT 0.16.x (`TripwireVisual`, `SynchronizableObject`, `ExfiltrationPoint`, `StationaryWeapon`, `PlaceItemTrigger`, `BorderZone`, `TrapSyncable`).
   - Mapa `Labyrinth` protegido e desativado por padrão no F12 para preservar a mecânica procedural do evento.
   - Atualizado `ModConfig.cs`, `PerformanceManager.cs`, `TRL-CoreSight.csproj` e `PROPRIEDADES.md`.
7. **Análise do SPT-Dynamic-External-Resolution e Criação do Smart PiP Optimizer (`v0.3.0`)**:
   - Diagnosticada a falha fatal do DER: troca de resolução e modo DLSS em tempo de execução destrói a `RenderTexture` e desliga o DLSS via `CameraClass.SetAntiAliasing`, causando 1-3 quadros pretos a cada ADS.
   - Implementada alternativa não-destrutiva no CoreSight que atua em `shadowDistance`, `lodBias` e desativação de `VolumetricLightRenderer` duplicado na `BaseOpticCamera`.
   - Bump para v0.3.0 e compilação limpa com 0 erros.

## Sessão 2026-09-19 — Abertura do Item 010 (Afinidade de CPU, Topologia e Fixação de Threads)
- **Diagnóstico**: O verdadeiro gargalo de desempenho em SPT com IA/SAIN é CPU-bound e latência de cache L3 / inter-CCX (Ryzen) ou E-Cores (Intel). Otimizações puramente cosméticas de GPU são neutras em processadores potentes como Ryzen 3800X.
- **Arquitetura**: Proposta de orquestrador universal de afinidade via Win32 `GetLogicalProcessorInformationEx`, `SetProcessAffinityMask` e `SetThreadAffinityMask` para isolar P-Cores, cortar SMT secundário e fixar threads principais no bloco de menor latência.
- **Backlog**: Criado item `010-afinidade-cpu-topologia-threads` com spec funcional `010-afinidade-cpu-topologia-threads-01-spec.md`.
