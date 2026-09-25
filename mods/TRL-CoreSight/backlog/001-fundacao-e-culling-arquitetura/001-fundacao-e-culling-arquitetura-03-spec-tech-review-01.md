# 001 — fundacao-e-culling-arquitetura · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [001-fundacao-e-culling-arquitetura-02-spec-tech.md](001-fundacao-e-culling-arquitetura-02-spec-tech.md)  
**Data:** 2026-09-15  

> Análise crítica e independente da spec técnica contra o código real implementado. Cada ponto recebe um ID `PA-01-MM` permanente. Bloqueadores 🔴 devem ser mitigados antes de consolidação do item.

---

## Memória consultada

- `mods/TRL-CoreSight/memory/sessions.md` (Sessões 1 a 3).
- `docs/technical/spt-antipatterns.md` (AP-01 a AP-11):
  - **AP-01:** Teardown de ciclo de vida de raid determinístico.
  - **AP-02:** Filtragem estrita de `MainPlayer` vs bots remotos/locais.
  - **AP-04:** Alteração de estado de renderização da Unity e preservação de colisores/hitboxes.
  - **AP-09:** Validação de símbolos no Assembly descompilado do EFT 0.16.9.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 2 · Total: 4

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Cases | 🟡 Importante | Risco de `Camera.main` obsoleto ou nulo no `BotPerformanceLimiter` e campo órfão no `InteriorOcclusionWatcher` | 🟡 Aberto (Recomendada correção no código) |
| PA-01-02 | C — Concorrência e Conflito | 🟡 Importante | Mutação global de `QualitySettings.shadowDistance` e `lodBias` com risco de sobreposição por mods gráficos externos | 🟡 Aberto (Documentado e mitigável via F12) |
| PA-01-03 | B — Edge Cases | 🟢 Menor | Ausência de restauração imediata de sombras ao ocorrer a morte do jogador (`!IsAlive`) antes do fim de raid | 🟢 Menor |
| PA-01-04 | A — Gaps de Especificação | 🟢 Menor | Expansão de escopo não documentada na spec técnica inicial (Declutter e Smart PiP incorporados no mod) | 🟢 Menor (Formalizado no As-Built) |

---

## Categorias

- **A — Gaps de Especificação:** informações ausentes que causam divergência entre spec e código.
- **B — Edge Cases:** cenários dinâmicos de jogo não cobertos (troca de câmeras, morte, spectator).
- **C — Erros de Lógica / Concorrência:** premissas globais que podem conflitar com motor ou outros mods.

---

## Impacto

- 🔴 **Bloqueador** — impede funcionamento básico, crash ou perda de integridade estrutural.
- 🟡 **Importante** — comportamento anômalo em cenários comuns (modo observador, mods visuais).
- 🟢 **Menor** — inconsistência sem quebra funcional, oportunidade de refatoração ou alinhamento documental.

---

## Análise Detalhada dos Pontos

### PA-01-01 · B — Edge Cases · 🟡 Importante

**Risco de `Camera.main` obsoleto ou nulo no `BotPerformanceLimiter` e campo órfão no `InteriorOcclusionWatcher`**

- **Contexto no Código:**
  - Em [`BotPerformanceLimiter.cs:21`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/BotPerformanceLimiter.cs#L21), a câmera é obtida no início via `_mainCamera = Camera.main;` e utilizada em [`CalculateFrustumPlanes(_mainCamera, _cameraPlanes)`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/BotPerformanceLimiter.cs#L54).
  - Em [`InteriorOcclusionWatcher.cs:33`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/InteriorOcclusionWatcher.cs#L33), `_mainCamera = Camera.main;` é capturado mas nunca é referenciado no resto da classe (o cálculo usa `Physics.Raycast` a partir de `_mainPlayer.Position`).
- **Problema:**
  - O acesso estático `Camera.main` do Unity executa internamente uma busca por tag (`GameObject.FindGameObjectWithTag("MainCamera")`). No Escape From Tarkov, a câmera do jogador é gerenciada pelo singleton [`CameraClass.Instance.Camera`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/CameraClass.cs#L197).
  - Durante transições de raid, ativação de câmeras livres (FreeCam de mods de teste ou debugging) ou quando o jogador morre e o FIKA entra no modo espectador/observador (`SpectatorCamera`), a instância de `_mainCamera` pode:
    1. Ser desativada ou destruída, gerando `MissingReferenceException` ao tentar calcular os planos de frustum.
    2. Apontar para uma câmera inativa, fazendo com que todos os bots fiquem permanentemente marcados como fora de frustum ou gerando cálculos incorretos.
    3. Ficar nula, congelando a execução de `ProcessBots()` na linha 32 (`_mainCamera == null`).
- **Recomendação de Correção:**
  - No `BotPerformanceLimiter.cs`, atualizar dinamicamente a câmera ativa caso seja nula ou inativa:
    ```csharp
    Camera activeCam = (CameraClass.Exist && CameraClass.Instance.Camera != null) 
        ? CameraClass.Instance.Camera 
        : Camera.main;
    ```
  - No `InteriorOcclusionWatcher.cs`, remover o campo privado `_mainCamera` que está inerte (dead code).

---

### PA-01-02 · C — Concorrência e Conflito · 🟡 Importante

**Mutação global de `QualitySettings.shadowDistance` e `lodBias` com risco de sobreposição por mods gráficos externos**

- **Contexto no Código:**
  - [`InteriorOcclusionWatcher.cs:79`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/InteriorOcclusionWatcher.cs#L79): atualiza `QualitySettings.shadowDistance = _currentShadowDistance` continuamente durante o `OnUpdate()`.
  - [`PerformanceManager.cs:90`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/PerformanceManager.cs#L90): atualiza `QualitySettings.lodBias = _currentLodBias`.
- **Problema:**
  - As propriedades de `UnityEngine.QualitySettings` são estáticas e afetam toda a pipeline do processo do jogo.
  - Mods populares como **AmandsGraphics**, **Fontaine's FOV/Optics** ou **LOD modifers** também manipulam essas mesmas variáveis globais (por exemplo, ajustando a distância de sombras dinamicamente de acordo com horário do dia ou clima de neblina).
  - Consequências identificadas:
    1. **Frame Fighting (Flicker):** Se o AmandsGraphics escrever um valor no `LateUpdate` e o CoreSight interpolar no `Update`, a cada frame o motor renderizará com uma distância diferente, provocando cintilação visível nas sombras de árvores e prédios.
    2. **Sobrescrita do Estado Original:** Ao salvar `_originalShadowDistance` no `Initialize()`, o CoreSight presume que esse valor permanecerá constante durante toda a raid. Se outro mod reconfigurar as sombras no meio da partida (ex.: ao anoitecer), quando o CoreSight chamar `Restore()` no `OnDestroy()`, ele forçará o retorno ao valor capturado no minuto zero da raid.
- **Recomendação de Mitigação:**
  - Garantir que `ExteriorShadowDistance` tenha uma opção configurável via menu F12 para "Ignorar em Exteriores / Deixar Original", permitindo que em áreas abertas o CoreSight não sobrescreva a distância de sombra definida por mods gráficos terceiros.
  - No `ModConfig.cs`, documentar explicitamente na descrição do BepInEx a compatibilidade com mods de pós-processamento.

---

### PA-01-03 · B — Edge Cases · 🟢 Menor

**Ausência de restauração imediata de sombras ao ocorrer a morte do jogador (`!IsAlive`) antes do término da raid**

- **Contexto no Código:**
  - Em [`InteriorOcclusionWatcher.cs:50-53`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/InteriorOcclusionWatcher.cs#L50-L53):
    ```csharp
    if (_mainPlayer == null || !_mainPlayer.HealthController.IsAlive)
    {
        return;
    }
    ```
- **Problema:**
  - Se o jogador for eliminado dentro de um quarto (onde a distância de sombra estava reduzida a 25m), a verificação aborta prematuramente sem restaurar `QualitySettings.shadowDistance = _originalShadowDistance`.
  - O jogo transiciona para a câmera de morte lenta e tela pós-raid com sombras truncadas em 25 metros. A restauração só ocorre de fato quando `GameWorld.OnDestroy` é disparado durante o descarregamento da cena.
- **Recomendação:**
  - Detectar a transição de morte (quando `IsAlive` passa de `true` para `false`) e disparar `Restore()` imediatamente no próprio frame.

---

### PA-01-04 · A — Gaps de Especificação · 🟢 Menor

**Expansão de escopo não documentada na spec técnica inicial (Declutter e Smart PiP)**

- **Contexto:**
  - A spec técnica [`001-fundacao-e-culling-arquitetura-02-spec-tech.md`](001-fundacao-e-culling-arquitetura-02-spec-tech.md) planejou a v0.1.0 (sombras de interiores e culling de Mecanim de bots).
  - Durante o ciclo de implementação, o mod evoluiu diretamente para a v0.3.0, incorporando o `DeclutterManager.cs` (culling de detritos cosméticos) e o `OpticVolumetricsPatch.cs` (otimização de PiP/luneta).
- **Impacto:**
  - Não há quebra funcional no código; pelo contrário, o ganho de desempenho foi expandido. Contudo, há um descompasso documental formal entre a spec técnica do item 001 e os arquivos entregues.
- **Resolução:**
  - Alinhar formalmente no documento [001-fundacao-e-culling-arquitetura-05-asbuild.md](001-fundacao-e-culling-arquitetura-05-asbuild.md) todas as classes que foram agregadas à arquitetura base do mod.

---

## Conclusão da Review

A especificação técnica e a base arquitetural implementada demonstram **excelente maturidade e conformidade com os antipadrões do SPT 4.0** (zero alocações por frame, interpolação suave via `MoveTowards`, preservação estrita de hitboxes e colliders físicos, e cleanup centralizado via lifecycle do `GameWorld`).

Os riscos identificados (PA-01-01 a PA-01-03) são contornáveis e pontuais, não impedindo o avanço para a fase de testes e code-review.
