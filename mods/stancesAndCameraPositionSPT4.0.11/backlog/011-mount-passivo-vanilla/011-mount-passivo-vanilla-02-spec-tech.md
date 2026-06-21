# 011 — Mount passivo sobre o mount vanilla (ativo) · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [011-mount-passivo-vanilla-01-spec.md](011-mount-passivo-vanilla-01-spec.md)
**Criado:** 2026-06-21

> Fonte de verdade: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Tipos não presentes no dump local (`ProceduralWeaponAnimation`, `NewRecoilShotEffect`, `EftBattleUIScreen`) têm assinatura **validada em runtime no item 004** (compilavam + `[enable] OK` no `LogOutput.log` desta sessão); marcados como **[validado-004]**. Confirmar via dnSpy/ILSpy no Assembly real se alguma falhar no build.

## 1. Estratégia

O **mount ativo é 100% vanilla** — o mod não patcha o input nem o estado de mount do EFT (lição do 004: o `MountingInputPatch` suprimia `ECommand.WeaponMounting` e quebrava tudo). Implementamos **apenas o passivo**, em três blocos:

1. **Detecção** — Postfix em `Player.FirearmController.method_11` (o cálculo de oclusão de arma do EFT). Reaproveita o `origin`/`ln`/`weaponUp` **reais** que o EFT passa (correção sobre o 004, que inventava `ln`) e dispara 3 raycasts próprios (Top/Left/Right) — o EFT não expõe "superfície montável disponível" antes de montar (`GClass2667` é caixa-preta). Resultado grava `PassiveMountState` (encostado? direção?). **Por assinatura**, não por número (AP-03).
2. **Buffs** — Prefix em `NewRecoilShotEffect.AddRecoilForce` (recoil) e Postfix em `ProceduralWeaponAnimation.ProcessEffectors` (sway, via `Breath.Intensity`); a stamina reusa o `StanceStaminaRecoveryPatch` já existente. Todos aplicam o multiplicador **só** quando `PassiveMountState.IsBracing` **e não** montado no vanilla (`IsMountedState`), não prone, não sprint, e **só no jogador local** (AP-02).
3. **UI** — `MonoBehaviour` anexado ao **GameObject persistente do plugin** (padrão do `OxygenUI`; nunca `new GameObject` no boot — lição do 004) + Postfix em `EftBattleUIScreen.Show` para parentar o ícone ao HUD. Mostra `mounting.png`/`mountingleft.png`/`mountingright.png` conforme a direção.

Estado em classe estática `PassiveMountState` (sem MonoBehaviour para a detecção — o Postfix do `method_11` já roda no fluxo do EFT). Reset no `GameWorld.OnDestroy` (raid end) e cessão automática quando sem arma/montado.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`Player.cs:12814`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L12814) — `Player.FirearmController.method_11(Vector3 origin, float ln, ref bool overlapsWithPlayer, Vector3? weaponUp)` | **Postfix** | Hook de detecção: usa `origin`/`ln`/`weaponUp` reais + raycasts próprios → grava `PassiveMountState`. |
| `EFT.Animations.NewRecoilShotEffect.AddRecoilForce(ref float)` **[validado-004]** (caller em [`ShotEffector.cs:164`](../../../../references/eft-decompiled/Assembly-CSharp/ShotEffector.cs#L164)) | **Prefix** | Reduz recoil no passivo (`incomingForce *= recoilMult`). |
| `EFT.Animations.ProceduralWeaponAnimation.ProcessEffectors` **[validado-004]** | **Postfix** | Reduz sway no passivo (`__instance.Breath.Intensity *= swayMult`). |
| `EFT.UI.EftBattleUIScreen.Show(owner)` **[validado-004]** (usado em [`EftGamePlayerOwner.cs`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/EftGamePlayerOwner.cs)) | **Postfix** | Parenta o ícone ao HUD de batalha. |
| [`MovementContext.cs:1502`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1502) — `IsInMountedState` (leitura via `ProceduralWeaponAnimation.IsMountedState` **[validado-004]**) | leitura | Guard: passivo cede ao mount vanilla (sem somar). |
| `GameWorld.OnDestroy` (patch existente `GameWorldOnDestroyPatch`) | reuso | Reset de `PassiveMountState` no fim da raid. |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Weapon Mount (Passive)` | `Enable Passive Mount` | bool | `true` | — | — | Liga o apoio passivo: ao encostar a arma numa superfície (sem a tecla de mount) você ganha um benefício leve de estabilidade. Desligado = só o mount nativo do jogo. |
| `Weapon Mount (Passive)` | `Passive Recoil Multiplier` | float | `0.7` | 0.1 a 1.0 | — | Multiplicador de recuo enquanto apoiado (passivo). 0.7 = 30% menos recuo. Deve ser MAIOR que o do mount ativo (vanilla) — o passivo é mais fraco. |
| `Weapon Mount (Passive)` | `Passive Sway Multiplier` | float | `0.65` | 0.0 a 1.0 | — | Multiplicador de sway (respiração) enquanto apoiado. 0.65 = 35% menos sway. |
| `Weapon Mount (Passive)` | `Passive Stamina Save` | bool | `true` | — | — | Enquanto apoiado, pausa/reduz o drain de stamina de braço (como no mount nativo, porém só no passivo). |
| `Weapon Mount (Passive)` | `Show Mount Icon` | bool | `true` | — | — | Mostra o ícone direcional (esquerda/direita/baixo) no canto inferior direito quando o apoio passivo está ativo. |

> Atualizar `PROPRIEDADES.md` com estas 5 entradas no `/code-mod`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/PassiveMountState.cs` | CRIAR | Estado estático: `IsBracing`, `BracingDirection`, `Reset()`. Enum `EBracingDir { None, Top, Left, Right }`. |
| `modded/Patches/PassiveMountDetectPatch.cs` | CRIAR | Postfix em `method_11` (por assinatura) + raycasts (Top/Left/Right) com `origin`/`ln`/`weaponUp` reais. Gate `IsYourPlayer` + cooldown. |
| `modded/Patches/PassiveMountBuffPatches.cs` | CRIAR | Prefix `AddRecoilForce` + Postfix `ProcessEffectors` (sway). Guards: `IsBracing && !IsMountedState && !prone && !sprint && IsYourPlayer`. |
| `modded/PassiveMountUI.cs` | CRIAR | `MonoBehaviour` do ícone (no gameObject do plugin) + `BattleUIScreenPatch` (Postfix em `Show`). |
| `modded/Patches/StanceStaminaRecoveryPatch.cs` | MODIFICAR | Estender o guard para também poupar stamina quando `PassiveMountState.IsBracing` (além do `IsMountedState` já lido). |
| `modded/Plugin.cs` | MODIFICAR | 5 `ConfigEntry` novas; `SafeEnable` dos novos patches; `gameObject.AddComponent<PassiveMountUI>()`; reset via `GameWorldOnDestroyPatch`. |

## 5. Stubs de código

```csharp
// modded/PassiveMountState.cs
namespace CameraRotationMod
{
    public enum EBracingDir { None, Top, Left, Right }

    public static class PassiveMountState
    {
        public static bool IsBracing { get; private set; }
        public static EBracingDir Direction { get; private set; }

        public static void Set(bool bracing, EBracingDir dir)
        {
            IsBracing = bracing;
            Direction = bracing ? dir : EBracingDir.None;
        }
        public static void Reset() { IsBracing = false; Direction = EBracingDir.None; }
    }
}
```

```csharp
// modded/Patches/PassiveMountDetectPatch.cs
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    // Hook no cálculo de oclusão de arma do EFT — reaproveita origin/ln/weaponUp reais (correção do 004,
    // que inventava o ln). Faz raycasts próprios pois o EFT não expõe "superfície montável" antes de montar.
    public class PassiveMountDetectPatch : ModulePatch
    {
        private static FieldInfo _playerField; // FirearmController._player
        private static float _lastDetect;
        private const float Cooldown = 0.1f;
        // offsets validados (item 004 / RealismMod CollisionPatch), em espaço local do WeaponRootAnim:
        private static readonly Vector3 _down  = new Vector3(0f, 0f, -0.19f);
        private static readonly Vector3 _left  = new Vector3(0.143f, 0f, 0f);
        private static readonly Vector3 _right = new Vector3(-0.143f, 0f, 0f);

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            // resolver por assinatura (AP-03): method_11(Vector3, float, ref bool, Vector3?)
            return AccessTools.Method(typeof(Player.FirearmController), "method_11",
                new[] { typeof(Vector3), typeof(float), typeof(bool).MakeByRefType(), typeof(Vector3?) });
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance, float ln, Vector3? weaponUp)
        {
            if (!Plugin._EnablePassiveMount.Value) return;
            var player = (Player)_playerField.GetValue(__instance);
            if (player == null || !player.IsYourPlayer) return;           // AP-02: só local
            if (Time.time - _lastDetect <= Cooldown) return;
            _lastDetect = Time.time;

            // Cede ao vanilla: montado ou prone => sem passivo
            var pwa = player.ProceduralWeaponAnimation;                    // [validado-004]
            if (pwa == null || pwa.HandsContainer?.WeaponRootAnim == null) { PassiveMountState.Reset(); return; }
            if (pwa.IsMountedState || pwa.IsBipodUsed || player.IsInPronePose || player.IsSprintEnabled)
            { PassiveMountState.Reset(); return; }

            Transform w = pwa.HandsContainer.WeaponRootAnim;
            Vector3 up = weaponUp ?? w.TransformDirection(Vector3.up);
            float len = ln * 1.25f;
            int mask = EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS;   // ref: EFTHardSettings.cs:251

            if (Cast(EBracingDir.Top,   w.position + w.TransformDirection(_down),  up, len, mask)) return;
            if (Cast(EBracingDir.Left,  w.position + w.TransformDirection(_left),  up, len, mask)) return;
            if (Cast(EBracingDir.Right, w.position + w.TransformDirection(_right), up, len, mask)) return;
            PassiveMountState.Reset();
        }

        private static bool Cast(EBracingDir dir, Vector3 start, Vector3 up, float len, int mask)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (Physics.Linecast(start, start - up * len, out RaycastHit hit, mask, QueryTriggerInteraction.Ignore)
                && hit.collider.gameObject.layer != playerLayer)
            {
                PassiveMountState.Set(true, dir);
                return true;
            }
            return false;
        }
    }
}
```

```csharp
// modded/Patches/PassiveMountBuffPatches.cs
using EFT.Animations;                 // ProceduralWeaponAnimation [validado-004]
using EFT.Animations.NewRecoil;       // NewRecoilShotEffect [validado-004]
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace CameraRotationMod.Patches
{
    public class PassiveRecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(NewRecoilShotEffect), "AddRecoilForce");

        [PatchPrefix]
        private static void Prefix(ref float incomingForce)
        {
            if (PassiveMountState.IsBracing && Plugin._EnablePassiveMount.Value)
                incomingForce *= Plugin._PassiveRecoilMultiplier.Value;
        }
    }

    public class PassiveSwayPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(ProceduralWeaponAnimation),
                                  nameof(ProceduralWeaponAnimation.ProcessEffectors));

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (PassiveMountState.IsBracing && Plugin._EnablePassiveMount.Value && __instance.Breath != null)
                __instance.Breath.Intensity *= Plugin._PassiveSwayMultiplier.Value;
        }
    }
}
```
> Nota: `PassiveMountState` já é só-local (gravado sob `IsYourPlayer` no detect). Os patches de buff rodam para todos os players, mas só agem quando `IsBracing` (que só fica `true` para o seu player) — satisfaz AP-02. **TODO confirmar** no `/code-mod`: se `ProcessEffectors` roda para peers, adicionar gate explícito `__instance` == PWA do MainPlayer.

```csharp
// modded/PassiveMountUI.cs (resumo — espelha o MountingUI do 004, mas no gameObject do plugin)
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CameraRotationMod
{
    public class PassiveMountUI : MonoBehaviour
    {
        public GameObject ActiveUIScreen;
        // CreateGameObject(parent): cria Image, sprite por Direction; Update() pisca alpha quando IsBracing.
        // ref de sprites: Plugin.LoadedSprites["mounting(left|right).png"]
    }

    public class BattleUIScreenPatch : ModulePatch   // [validado-004]
    {
        protected override MethodBase GetTargetMethod()
            => typeof(EftBattleUIScreen).GetMethods(BindingFlags.Instance | BindingFlags.Public)
               .First(x => x.Name == "Show" && x.GetParameters().Length > 0
                           && x.GetParameters()[0].Name == "owner");

        [PatchPostfix]
        private static void Postfix(EftBattleUIScreen __instance) { /* anexa PassiveMountUI ao HUD */ }
    }
}
```

## 6. Fluxo de dados

```
[A] EFT processa a arma a cada frame
      → chama Player.FirearmController.method_11(origin, ln, weaponUp)   // Player.cs:12814
[B] Postfix PassiveMountDetectPatch (gate IsYourPlayer; cede se IsMountedState/prone/sprint)
      → 3 raycasts (WEAPON_OCCLUSION_LAYERS)  // EFTHardSettings.cs:251
      → PassiveMountState.Set(IsBracing, Direction)
[C1] NewRecoilShotEffect.AddRecoilForce  → Prefix: incomingForce *= recoilMult  (se IsBracing)
[C2] ProceduralWeaponAnimation.ProcessEffectors → Postfix: Breath.Intensity *= swayMult
[C3] StanceStaminaRecoveryPatch → poupa stamina de braço (se IsBracing)
[D] PassiveMountUI.Update → ícone left/right/down no HUD (se IsBracing && Show Mount Icon)
[E] GameWorld.OnDestroy → PassiveMountState.Reset()
```

## 7. Riscos e dependências

- **Patches existentes:** `StanceStaminaRecoveryPatch` (já lê `IsMountedState`) — estender com cuidado para não duplicar com o regen vanilla. `GameWorldOnDestroyPatch` — adicionar o reset.
- **Tipos fora do dump:** `ProceduralWeaponAnimation`/`NewRecoilShotEffect`/`EftBattleUIScreen` — validados no 004; se algum `GetTargetMethod` retornar null no build, confirmar assinatura no Assembly real.
- **`ProcessEffectors` roda para peers/bots** — confirmar no code-mod o gate (hoje protegido por `IsBracing` só-local; adicionar `__instance == MainPlayer.PWA` se necessário) — AP-02.
- **Sobreposição com mount vanilla:** os buffs cedem via `IsMountedState`; validar que não há janela de soma na transição passivo→ativo.
- **Ordem de init:** sprites já carregados no `Awake` (mantidos); `PassiveMountUI` anexado ao gameObject do plugin; `BattleUIScreenPatch` habilitado no `Awake`.

## 8. Checklist de implementação

- [ ] Criar `PassiveMountState.cs` (estado + enum + Reset).
- [ ] Criar `PassiveMountDetectPatch.cs` (Postfix `method_11` por assinatura + raycasts + guards/gate).
- [ ] Criar `PassiveMountBuffPatches.cs` (recoil + sway) com guards `IsBracing && !IsMountedState`.
- [ ] Estender `StanceStaminaRecoveryPatch` para poupar stamina quando `IsBracing` (atrás de `Passive Stamina Save`).
- [ ] Criar `PassiveMountUI.cs` + `BattleUIScreenPatch` (no gameObject do plugin).
- [ ] `Plugin.cs`: 5 ConfigEntry, `SafeEnable` dos patches, `AddComponent<PassiveMountUI>`, reset no `GameWorldOnDestroyPatch`.
- [ ] Atualizar `PROPRIEDADES.md` (5 entradas).
- [ ] Build (`dotnet build`); validar `[enable] OK` dos novos patches no log.
- [ ] Validar in-game: vanilla intacto; passivo (recoil/sway/stamina + ícone) ao encostar; cede ao montar; reset entre raids; Fika (só local).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop idempotentes | ✅ | Reset em `GameWorldOnDestroyPatch` (§2/§5); estado estático + UI no gameObject do plugin (sem leak de `new GameObject`). |
| 2 | Filtro MainPlayer/Fika (AP-02) | ✅ | Detect com `IsYourPlayer` (§5); buffs só agem sob `IsBracing` (só-local). TODO confirmar gate em `ProcessEffectors` (§7). |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados (AP-03) | ✅ | `method_11` resolvido por assinatura `(Vector3,float,ref bool,Vector3?)` (§5). |
| 4 | Mudança de estado via API canônica; side-effects (AP-04) | ✅ | Não muda estado do EFT — só lê (`IsMountedState`) e multiplica força/sway já existentes; ativo é 100% vanilla. |
| 5 | Estado entre raids coberto | ✅ | `PassiveMountState.Reset()` no raid end + cessão por `!IsMountedState`/sem arma (§5/§6). |
| 6 | Semântica/defaults/faixas das ConfigEntry (AP-05) | ✅ | §3 com defaults/faixas; estado neutro = passivo desligado → vanilla puro. |
| 7 | Reentry-guard em re-invocação (AP-07) | N/A | Nenhum patch re-invoca o método patcheado; sem recursão. |
| 8 | Flags/caches validados após troca de contexto (AP-08) | ✅ | `IsBracing` é recomputado a cada `method_11` (cooldown) e cede sem arma/montado; não persiste estado obsoleto entre armas. |

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Spec técnica criada via `/create-technical-spec` |
