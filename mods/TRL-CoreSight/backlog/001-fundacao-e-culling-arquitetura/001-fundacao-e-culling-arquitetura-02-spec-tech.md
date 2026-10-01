# 001 — fundacao-e-culling-arquitetura · Spec Técnica

**Mod:** TRL-CoreSight  
**Spec funcional:** [001-fundacao-e-culling-arquitetura-01-spec.md](001-fundacao-e-culling-arquitetura-01-spec.md)  
**Criado:** 2026-09-15T00:21:00Z  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`.

---

## 1. Estratégia

1. **Ciclo de Vida de Raid**:
   - Registrar início de raid no `GameWorld.OnGameStarted` ([`GameWorld.cs:720`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L720)) para anexar o `PerformanceManager` (MonoBehaviour).
   - Registrar término no `GameWorld.OnDestroy` ([`GameWorld.cs:810`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L810)) para limpeza completa de instâncias e coleta de lixo preventiva (evitando vazamentos e acúmulo de estado entre raids - AP-01).

2. **Detecção de Interiores e Sombras**:
   - Utilizar o `EnvironmentController` nativo do EFT ([`EnvironmentController.cs:120`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EnvironmentController.cs#L120)) ou amostragem vertical com `Physics.Raycast(headPos, Vector3.up, out hit, 15f, LayerMaskClass.HighPolyWithTerrainMask)` a cada 0.5s.
   - Interpolar suavemente o `QualitySettings.shadowDistance` entre `ExteriorShadowDistance` (100m) e `InteriorShadowDistance` (25m) usando `Mathf.MoveTowards`.

3. **Animation LOD em Bots Ocluídos**:
   - Interagir com a lista de bots registrados no `BotSpawner.TrackedBots` ou `GameWorld.AllAlivePlayersList`.
   - A cada 200ms (throttled):
     - Se o bot estiver vivo e não for o `MainPlayer`:
     - Testar frustum da câmera: `GeometryUtility.TestPlanesAABB(cameraPlanes, bot.CharacterController.bounds)`.
     - Se estiver fora do frustum ou a mais de 80m com oclusão total:
       - Configurar `bot.PlayerBones.BodyAnimator.cullingMode = AnimatorCullingMode.CullUpdateTransforms`.
     - Se entrar na visão ou combate:
       - Restaurar para `AnimatorCullingMode.AlwaysAnimate`.
   - **Nota vital de integridade física**: NUNCA desativar o `CharacterController` ou os `BodyPartCollider` do bot ([`BodyPartCollider.cs:10`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/BodyPartCollider.cs#L10)), preservando balística e registro de acertos.

---

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`GameWorld.OnGameStarted`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L720) | Postfix | Iniciar o `PerformanceManager` e anexar observers na raid ativa. |
| [`GameWorld.OnDestroy`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L810) | Prefix | Destruir corrotinas, limpar referências estáticas e restaurar configurações de renderização originais. |

---

## 3. Novas propriedades F12 (BepInEx)

Ver detalhes completos em [`PROPRIEDADES.md`](../../PROPRIEDADES.md).
- `ModEnabled` (bool, default: true)
- `EnableInteriorShadowCulling` (bool, default: true)
- `InteriorShadowDistance` (float, default: 25.0f, range: 10.0f a 50.0f)
- `ExteriorShadowDistance` (float, default: 100.0f, range: 50.0f a 200.0f)
- `EnableBotAnimationLOD` (bool, default: true)
- `BotOcclusionCheckInterval` (float, default: 0.2f, range: 0.05f a 1.0f, Avançado)
- `EnableDynamicLODBias` (bool, default: true)

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin.cs` | CRIAR | Entrypoint BepInEx e inicialização do Harmony. |
| `modded/Configuration/ModConfig.cs` | CRIAR | Definição das configurações BepInEx F12. |
| `modded/Core/PerformanceManager.cs` | CRIAR | Orquestrador MonoBehaviour gerenciador do loop de frame. |
| `modded/Core/InteriorOcclusionWatcher.cs` | CRIAR | Controlador de interpolação da distância de sombra. |
| `modded/Core/BotPerformanceLimiter.cs` | CRIAR | Gerenciador de LOD e visibilidade de bots. |
| `modded/Patches/RaidLifecyclePatches.cs` | CRIAR | Hooks no `GameWorld` para start e cleanup de raid. |

---

## 5. Checklist de implementação

- [x] Criar estrutura de pastas do mod em `mods/TRL-CoreSight`.
- [x] Criar `mod.json`, `README.md`, `PROPRIEDADES.md` e `memory/sessions.md`.
- [x] Criar `TRL-CoreSight.csproj` e código fonte compilável em `modded/`.
- [x] Implementar `RaidLifecyclePatches` com teardown determinístico.
- [x] Implementar `InteriorOcclusionWatcher` e `BotPerformanceLimiter`.
- [x] Compilar o projeto em `modded/bin/Release/` e gerar pacote em `builds/`.

---

## 6. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Patches em `GameWorld.OnGameStarted` e `GameWorld.OnDestroy` com teardown idempotente. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `BotPerformanceLimiter` ignora explicitamente `player.IsYourPlayer` e aplica apenas em bots/IA. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | Patches aplicados em métodos públicos não ofuscados de alto nível (`GameWorld`). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Manipulação direta do `QualitySettings` da Unity e do `cullingMode` do `Animator`. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `PerformanceManager.Cleanup()` chamado no `OnDestroy` reseta `QualitySettings.shadowDistance` e limpa listas. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | Todas as propriedades com `AcceptableValueRange` no `ModConfig.cs`. |
| 7 | Re-invocação de método patcheado tem reentry-guard (sem recursão infinita) — AP-07 | N/A | Nenhum método patcheado é re-invocado. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Não há substituição de transições de armas ou animações do jogador. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Conferido em `GameWorld.cs:720` e `GameWorld.cs:810`. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | O mod não utiliza skills do RPG como lever. |
| 11 | Pacote FIKA próprio: envelope de comprimento — AP-11 | N/A | Mod puramente client-side; não registra novos pacotes de rede no FIKA. |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Spec técnica criada e conformidade com APs aprovada |
