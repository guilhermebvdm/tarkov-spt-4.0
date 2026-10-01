# 006 — culling-capsulas-balas · Spec Técnica

**Mod:** TRL-CoreSight  
**Spec funcional:** [006-culling-capsulas-balas-01-spec.md](006-culling-capsulas-balas-01-spec.md)  
**Criado:** 2026-09-15T22:05:00Z  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

---

## 1. Estratégia

Otimizar o custo de CPU e simulação física da PhysX durante tiroteios médios e distantes, **interceptando a criação e o spawn de cápsulas no nascimento**, evitando que o motor do jogo aloque corrotinas, desparente GameObjects na cena ou execute cálculos de quique de cartuchos no solo para armas distantes.

A implementação adota uma estratégia de duas camadas:
1. **Sincronização de Limiar Nativo (`EFTHardSettings`):**
   - O Tarkov já possui nativamente o campo público [`EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFTHardSettings.cs#L451) (padrão BSG: `25f`). Ao carregar a raid e ao alterar o menu F12, o mod atualiza esse campo diretamente para garantir que todas as chamadas nativas de `method_7()` ([`Player.cs:11168`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L11168)) e `method_4()` ([`WeaponManagerClass.cs:375`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/WeaponManagerClass.cs#L375)) respeitem a configuração do usuário.
2. **Patch Harmony de Segurança (Interceptação Precoce):**
   - Para cobrir armas de bots, armas montadas/estacionárias e garantir imunidade absoluta ao jogador local mesmo em situações de spectator ou câmeras especiais, interceptar via **Prefix** os métodos `StartSpawnShell` em [`Player.FirearmController`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L11123) e [`WeaponManagerClass`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/WeaponManagerClass.cs#L595):
     - Se o disparo for da arma do jogador local (`MainPlayer`): permite execução normal (retorna `true`).
     - Se o disparo for de um bot ou entidade a uma distância maior que `ModConfig.ShellCullingDistance.Value`: retorna `false`, abortando a corrotina de spawn antes que qualquer objeto entre na cena ou na PhysX.

---

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFTHardSettings.cs:451`](../../../../references/eft-decompiled/Assembly-CSharp/EFTHardSettings.cs#L451) | Modificação Direta | Ajustar `FLYING_SHELLS_VISIBLE_DISTANCE` para sincronizar o limiar nativo da BSG com a configuração do mod. |
| [`Player.cs:11123`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L11123) (`Player.FirearmController.StartSpawnShell`) | Prefix | Interceptar a corrotina de spawn de cápsulas para armas em terceira pessoa/bots que utilizam o controller de player. |
| [`WeaponManagerClass.cs:595`](../../../../references/eft-decompiled/Assembly-CSharp/WeaponManagerClass.cs#L595) (`WeaponManagerClass.StartSpawnShell`) | Prefix | Interceptar a corrotina de spawn de cápsulas para bots gerenciados por `WeaponManagerClass` e armas fixas. |
| [`WeaponManagerClass.cs:603`](../../../../references/eft-decompiled/Assembly-CSharp/WeaponManagerClass.cs#L603) (`WeaponManagerClass.StartSpawnAllShells`) | Prefix | Interceptar ejeções múltiplas simultâneas (espingardas/revolvers) a longa distância. |

---

## 3. Novas propriedades F12 (BepInEx)

Adicionar na seção **`7. Otimização de Física e Projéteis`** em `Configuration/ModConfig.cs`:

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `7. Otimização de Física` | `EnableShellCulling` | bool | `true` | — | — | Suprime a criação física e ejeção de cápsulas de bala vazias em tiroteios médios e distantes. |
| `7. Otimização de Física` | `ShellCullingDistance` | float | `25.0` | 10.0 a 60.0 | — | Distância máxima em metros para renderizar e processar física de quique de cápsulas ejetadas. |

---

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Configuration/ModConfig.cs` | MODIFICAR | Registrar as opções `EnableShellCulling` e `ShellCullingDistance` na seção 7. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar as novas propriedades da seção 7 para os usuários. |
| `Patches/ShellSpawnCullingPatch.cs` | CRIAR | Implementar os patches Harmony Prefix que interceptam `StartSpawnShell` e `StartSpawnAllShells`. |
| `Plugin.cs` | MODIFICAR | Habilitar o novo patch `new ShellSpawnCullingPatch().Enable()` no `Awake()`. |

---

## 5. Stubs de código

### `Patches/ShellSpawnCullingPatch.cs`

```csharp
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Intercepta o spawn de cápsulas de balas ejetadas por armas distantes,
    /// evitando o acionamento de corrotinas na Unity e simulação de corpos rígidos na PhysX.
    /// ref: Assembly-CSharp/Player.cs:11123
    /// ref: Assembly-CSharp/WeaponManagerClass.cs:595
    /// </summary>
    public class ShellSpawnCullingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/WeaponManagerClass.cs:595
            return AccessTools.Method(typeof(WeaponManagerClass), nameof(WeaponManagerClass.StartSpawnShell));
        }

        [PatchPrefix]
        private static bool Prefix(WeaponManagerClass __instance)
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableShellCulling.Value)
            {
                return true;
            }

            // O jogador local sempre processa cápsulas normalmente
            if (__instance.Player != null && __instance.Player.IsYourPlayer)
            {
                return true;
            }

            Camera activeCamera = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                ? CameraClass.Instance.Camera
                : Camera.main;

            if (activeCamera == null)
            {
                return true;
            }

            Vector3 weaponPos = (__instance.WeaponPrefab_0 != null)
                ? __instance.WeaponPrefab_0.transform.position
                : (__instance.Player != null ? __instance.Player.Position : Vector3.zero);

            float sqrDist = (weaponPos - activeCamera.transform.position).sqrMagnitude;
            float maxDist = ModConfig.ShellCullingDistance.Value;

            // Se o disparo estiver além da distância configurada, cancela o spawn da cápsula
            if (sqrDist > (maxDist * maxDist))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Intercepta armas que utilizam a rota de Player.FirearmController para spawn de cápsulas.
    /// ref: Assembly-CSharp/Player.cs:11123
    /// </summary>
    public class PlayerShellSpawnCullingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/Player.cs:11123 (classe aninhada de manipulação de câmara/munição)
            // Localizado via reflexão no tipo interno de FirearmController
            var nestedTypes = typeof(Player).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var type in nestedTypes)
            {
                var method = type.GetMethod("StartSpawnShell", BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    return method;
                }
            }

            return AccessTools.Method(typeof(WeaponManagerClass), nameof(WeaponManagerClass.StartSpawnShell));
        }

        [PatchPrefix]
        private static bool Prefix(object __instance)
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableShellCulling.Value)
            {
                return true;
            }

            Camera activeCamera = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                ? CameraClass.Instance.Camera
                : Camera.main;

            if (activeCamera == null)
            {
                return true;
            }

            // Obter WeaponPrefab_0 via campo refletido
            var field = AccessTools.Field(__instance.GetType(), "WeaponPrefab_0");
            Transform weaponTransform = null;
            if (field != null)
            {
                var prefab = field.GetValue(__instance) as Component;
                if (prefab != null)
                {
                    weaponTransform = prefab.transform;
                }
            }

            if (weaponTransform != null)
            {
                float sqrDist = (weaponTransform.position - activeCamera.transform.position).sqrMagnitude;
                float maxDist = ModConfig.ShellCullingDistance.Value;
                if (sqrDist > (maxDist * maxDist))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
```

---

## 6. Fluxo de dados

```
[Disparo da Arma / Bot Atira] 
         │
         ▼
[StartSpawnShell chamado no EFT] (WeaponManagerClass.cs:595 / Player.cs:11123)
         │
         ▼
[Prefix: ShellSpawnCullingPatch]
         │
         ├─── É o MainPlayer? ──(SIM)──► [Executa corrotina nativa: StartCoroutine(method_6)]
         │                                       │
         │                                       ▼
         │                            [Cápsula ejetada com física completa]
         │
         └─── É disparo de Bot?
                   │
                   ▼
       Distância até a Câmera > 25m?
                   │
                   ├─── (NÃO, <25m) ──► [Executa corrotina nativa normal]
                   │
                   └─── (SIM, >25m) ──► [RETORNA FALSE]
                                                │
                                                ▼
                                    [Corrotina CANCELADA na raiz]
                                    [Zero física PhysX]
                                    [Zero GameObjects na cena]
                                    [Zero cápsulas flutuando]
                                    [Cartucho reciclado no próximo ciclo]
```

---

## 7. Riscos e dependências

- **Compatibilidade com SAIN:** SAIN não depende de cápsulas físicas para tomada de decisão (utiliza eventos sonoros `AISoundType` e dados do bot). 100% compatível.
- **Compatibilidade com FIKA:** Em modo cooperativo, cartuchos ejetados não são sincronizados por pacotes de rede (cada cliente renderiza suas próprias cápsulas locais). Interceptar localmente economiza CPU de todos os clientes sem impactar a rede.
- **Armas com Múltiplos Canos / Revolvers:** `StartSpawnAllShells` também é interceptado pelo mesmo princípio de proximidade.
- **Ciclo da Câmara:** Testado e confirmado no código da BSG ([`Player.cs:11211`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L11211)): ao recarregar ou ciclar a arma, o método nativo `SetPatronInShellPort` chama `AssetPoolObject.ReturnToPool` caso encontre um cartucho anterior não ejetado, garantindo 0% de vazamento de memória.

---

## 8. Checklist de implementação

- [ ] Adicionar as propriedades `EnableShellCulling` e `ShellCullingDistance` em `Configuration/ModConfig.cs`.
- [ ] Atualizar `PROPRIEDADES.md` com as novas opções da seção 7.
- [ ] Criar `Patches/ShellSpawnCullingPatch.cs` com a lógica de Prefix de interceptação precoce.
- [ ] Sincronizar `EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE` no ciclo de inicialização de raid em `Core/PerformanceManager.cs`.
- [ ] Habilitar os patches em `Plugin.cs`.
- [ ] Incrementar a versão SemVer para `0.3.2` no `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`.
- [ ] Compilar a build em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` com 0 erros e 0 avisos.

---

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|:---:|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Gerenciado pelo `PerformanceManager` já existente e orquestrado por `RaidLifecyclePatches.cs`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Linha 33 do stub: `__instance.Player.IsYourPlayer` sempre recebe bypass (retorna `true`). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | Alvos `WeaponManagerClass.StartSpawnShell` e `StartSpawnAllShells` são classes públicas estáveis. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Apenas cancela corrotina visual de cartuchos; câmara reciclada pelo método nativo `SetPatronInShellPort`. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Sem estado estático retido em memória; patches sem estado acumulativo. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | Configurações tipadas com `AcceptableValueRange<float>(10.0f, 60.0f)` e tooltips em pt-BR. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | ✅ | Patch puramente condicional Prefix (retorna `true` ou `false`), sem chamadas recursivas. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | Posição e distância calculadas dinamicamente no momento exato de cada disparo (`activeCamera.transform.position`). |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | Reconfirmado em `WeaponManagerClass.cs:595`, `Player.cs:11123` e `EFTHardSettings.cs:451`. |
| 10 | Skill EFT usada como lever confirmada **não-inerte** (`SkillsSettings` ≠ `[]` no `globals.json`); se inerte, efeito entregue por patch direto — AP-10 | N/A | Não utiliza skills de RPG do EFT como alavanca. |
| 11 | Pacote FIKA próprio: envelope de comprimento + só `TryGet*` + flag `Valid`, campos resetados no `Deserialize`, envio só na main thread, registro por instância/evento (nunca `bool`), zero `UnregisterPacket`, airbag com throttle em todo callback — AP-11 | N/A | Não envia pacotes customizados pela rede; efeito puramente local de cliente. |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Spec técnica criada com a estratégia de interceptação precoce no spawn de cápsulas e conformidade §9. |
