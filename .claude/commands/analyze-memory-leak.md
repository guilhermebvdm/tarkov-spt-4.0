# /analyze-memory-leak

Auditoria **estática de memory leak** de um mod. Varre o código procurando retenção não liberada (estado de raid, subscriptions, static, objetos Unity, IDisposable, threads) e pressão de GC em hot path, classifica cada achado por **mecanismo** × **taxa de acúmulo**, e emite um relatório novo `MEMORY-LEAK-review-NN.md` (NN incremental) com sugestões acionáveis. **Auxiliar** — fora do ciclo linear de backlog (como `/review-mod-properties`); roda em qualquer mod a qualquer momento.

> **Skills obrigatórias:** carregar `spt-memory-leak-analysis` (taxonomia, taxa de acúmulo, greps, padrões preventivos, plano de confirmação), `csharp-mod-best-practices` (§1 memory ownership, §2 async/Unity), `spt-mod-best-practices` (§2 raid lifecycle, §3 memory/perf) e `graph-code-navigation` (achar teardown/callers via grafo). Consultar `memory-curation` §14 (passo de contexto de memória).

## Uso

```
/analyze-memory-leak <ref>
```

- `<ref>` — normalmente o **`<mod>`** (nome da pasta em `mods/`, ex.: `stancesAndCameraPositionSPT4.0.11`). Aceita também um **path de pasta/arquivo dentro de `modded/`** para escopo reduzido (ex.: auditar só `mods/<mod>/modded/Components/`). Validar que existe.

## Pré-condições

1. `mods/<mod>/modded/` existe e contém código-fonte (`.cs` client/server ou `.ts`/`.js` server legado).
2. (Recomendado) grafo do mod em `references/graphs/mods/<mod>/` — se ausente, seguir com Grep + leitura, e sugerir `/update-mod-graph <mod>` depois.

Se `mods/<mod>/` não existir, listar os mods disponíveis e parar.

## O que fazer

1. **Resolver `<ref>`** → `<mod>`, `<path-escopo>` (default = `modded/` inteiro).

2. **Classificar o mod** (skill §5.1): **client** (`BepInEx`/`Plugin.cs`/`[HarmonyPatch]`/`UnityEngine`) · **server** (`[Injectable]`, sem `UnityEngine`) · **combo**. Define quais mecanismos se aplicam (UNITY/HOT só client; SRV só server; LIFE/EVT/STAT/DISP/THRD ambos). Preencher `{{MOD_KIND}}`.

3. **Calcular `NN` da review.** Listar `mods/<mod>/MEMORY-LEAK-review-*.md`. Próximo NN = maior + 1, padded a 2 dígitos. Primeira = `01`.

4. **Ler contexto (memória — `memory-curation` §14):** topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências) + entradas que citem "leak", "memória", "OOM", "crash", "GC", "dispose", "cleanup", "teardown". Leak já **resolvido** não volta como achado; leak **pendente** conhecido é reforçado (cita a sessão). Emitir a linha `Memória consultada: ...`. Se não existir, registrar "sem memória prévia".

5. **Mapear a vida do mod** (skill §5.2, `graph-code-navigation`): localizar Awake, o raid-**start** hook, e — crucial — o raid-**end** hook (`GameWorld.OnDestroy` **e** `BaseLocalGame.Stop`; server: logout/raid-end). Registrar o ponto de teardown no Panorama, ou **"AUSENTE"** (já é achado LIFE / AP-01). Para mods grandes, delegar a varredura a um sub-agent read-only e consolidar.

6. **Varrer superfícies de risco** com os greps da skill §3 (um por mecanismo aplicável). Para **cada alocação** encontrada, procurar o **release pareado** e validar o escopo:
   - `+= X` (EVT) → existe `-= X`? Prova a ausência com o grep negativo.
   - `static` coleção/campo (STAT) → existe `.Clear()`/eviction no escopo certo?
   - `new GameObject`/`Instantiate`/`.material`/`AssetBundle`/`Texture` (UNITY) → `Destroy`/`Unload(true)`, ou parenteado a objeto que o EFT destrói?
   - `new CancellationTokenSource`/`StartCoroutine`/`new Timer`/`Stream` (DISP) → `Dispose`/`Cancel`/`StopCoroutine`?
   - `Task.Run`/`new Thread`/`async void` (THRD) → `CancellationToken` amarrado à raid?
   - patch em `Update`/`FixedUpdate`/AI-tick (HOT) → algum `new`/LINQ/`string.Format`/boxing dentro?
   - server singleton (SRV) → cache com eviction, ou só cresce? Dado imutável (não é leak) distinguido de cache mutável?
   - **Grafo aponta, leitura prova:** todo candidato é confirmado abrindo `arquivo.cs:linha`. Não reportar achado sem ler o par alocação↔release.

7. **Descartar falsos positivos** (skill §7): baseline do EFT/Unity/Fika, dado imutável carregado 1× no boot, objeto parenteado ao que o EFT destrói, cache com limite intencional, Harmony patch global de propósito. **Não** sugerir `GC.Collect`/`Resources.UnloadUnusedAssets`/RAM cleaner (o headless bane cleaners e isso causa hitch).

8. **Classificar cada achado:** **mecanismo** (§3: LIFE/EVT/STAT/UNITY/DISP/THRD/HOT/SRV) × **taxa de acúmulo** (§4: per-frame/per-raid/per-event/per-boot) → **severidade** (ajustada pelo que é retido e pela certeza). Quando fizer sentido, casar a sugestão com um **padrão preventivo** da skill §8 (RaidSession/disposable bag, weak-event, pooling, zero-alloc, parentear, DI lifetime).

9. **Renderizar `.agents/templates/memory-leak-review.md.tmpl`** preenchendo `{{MOD}}`, `{{MOD_KIND}}`, `{{SCOPE}}`, `{{CREATED_AT}}`, `{{REVIEW_NN}}`, o **Panorama** (tipo/superfícies de vida, ponto de teardown, tabela alocação→release, leaks conhecidos) e cada achado no formato `ML-NN-MM`.

10. **Adicionar achados** no formato do template. Cada um cita **onde nasce** (`arquivo.cs:linha` da alocação) e **onde deveria morrer** (o release esperado, ou "não existe" com o grep negativo que prova). **Toda sugestão é acionável** (onde adicionar o `-=`/`Destroy`/`Dispose`, ou o padrão §8 a adotar).

11. **Preencher o `## Plano de confirmação`** priorizando os achados 🔴/🟠 (matriz raid1→exit→raid2, raid longa, teardown por alt-F4/morte/MIA, headless real, heap snapshot opcional — skill §6).

12. **Atualizar índice e contadores** no topo.

13. **Reportar:**
    ```text
    ✓ Análise de memory leak NN criada: <path>
      Mod: <mod> (<client/server/combo>) · Escopo: <modded/ ou path>
      Memória consultada: snapshot de YYYY-MM-DD (Sessão N) · pendências que afetam: [...] / nenhuma
      Superfícies varridas: N · com release pareado: N · sem release (achado): N
      🔴 N · 🟠 N · 🟡 N · 🟢 N
    Leitura do resultado:
      🔴/🟠 = candidatos a leak que acumulam no headless (per-frame/per-raid). Priorizar no plano de confirmação.
    Próximo passo:
      Marque "Aceitar sugestão" nos achados a corrigir.
      Correção entra pelo ciclo normal: /code-mod (se item de backlog) ou fix manual + .agents/templates/fix.md.tmpl (06-fix-NN).
      Confirme in-game pela seção "Plano de confirmação" antes de considerar resolvido (análise estática só levanta hipótese — AP-06).
    ```

## Mecanismos × taxa × impacto

Ver a taxonomia completa na skill `spt-memory-leak-analysis` (§3 mecanismos, §4 taxa de acúmulo). Códigos de mecanismo: **LIFE · EVT · STAT · UNITY · DISP · THRD · HOT · SRV**. Taxa: **per-frame/tick · per-raid · per-event · per-boot**. Escala de impacto no template.

## Regras

- **Foco no headless:** a severidade é dominada pela **taxa de acúmulo**, não pelo tamanho da alocação. Um leak **per-raid** (🟠) é a causa clássica do OOM do headless (acumula raid a raid por horas); um **per-frame** (🔴) mata em minutos. Todo achado nomeia a taxa e explica o efeito no headless (skill §1/§4).
- **Cada achado cita evidência:** o `arquivo.cs:linha` da alocação **e** a prova da ausência do release (grep negativo, ou leitura do teardown que não cobre aquele objeto). Análise sem evidência não vai.
- **Grafo aponta, leitura prova** (`graph-code-navigation`): o grep/grafo localiza; a confirmação é sempre a leitura do `arquivo.cs:linha`.
- **Não duplicar as skills:** as regras de memória vivem em `csharp-mod-best-practices` §1 e `spt-mod-best-practices` §2/§3. O relatório **referencia** (AP-01/AP-07, item de checklist), não reescreve.
- **Não sugerir mitigação que o ambiente já faz** (skill §2/§7): sem `GC.Collect`/`UnloadUnusedAssets`/RAM cleaner no mod. A correção é **liberar o que o mod alocou**.
- **Reviews são artefatos imutáveis** — cada execução cria um arquivo novo; achados ganham só anotações de resolução depois (✅ Aplicado). Pontos já resolvidos não voltam.
- **Confirmação é in-game** (skill §6; AP-06): a estática entrega hipóteses priorizadas + plano de medição. "Sem leak" só depois de medir RSS entre raids / raid longa / teardown sujo — idealmente no headless real.
- Versão alvo: SPT 4.0+ / EFT 0.16.x / Fika (headless).