# 023 — Atraso na checagem de carregador (Magazine Check Delay) · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [023-atraso-checagem-carregador-01-spec.md](023-atraso-checagem-carregador-01-spec.md)
**Criado:** 2026-09-22

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

> ⚠️ **Esta spec é retroativa.** A feature já está implementada e em produção (build 2.25.x). O código
> abaixo (seção 5) é o código REAL já existente, não um stub proposto — esta spec documenta e valida o
> que já foi feito, e a seção 9 (conformidade) aponta os achados desta auditoria.

## 1. Estratégia

**Prefix** em **`EFT.UI.EftBattleUIScreen.ShowAmmoDetails(int, int, int, string, bool)`**
([EftBattleUIScreen.cs:91](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/EftBattleUIScreen.cs#L91)).
O Prefix suprime a exibição imediata do painel (`return false`) e agenda uma `Coroutine` que, após um
atraso configurável, re-invoca o método original (com uma flag de reentrada ligada, pra deixar passar
dessa vez).

**Por que este ponto e não a origem do evento nativo:** o painel de munição tem **duas origens
independentes** que convergem exatamente aqui:
1. **Checagem de carregador nativa** — `FirearmController.GClass2037.CheckAmmo()`
   ([Player.cs:5770](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L5770)) dispara
   o evento `Player.OnShowAmmoDetails`
   ([Player.cs:25504](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25504)), que
   `GamePlayerOwner` assina
   ([GamePlayerOwner.cs:358](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GamePlayerOwner.cs#L358))
   e encaminha via `method_8`
   ([GamePlayerOwner.cs:520-523](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GamePlayerOwner.cs#L520-L523))
   até `EftBattleUIScreen.ShowAmmoDetails`.
2. **Checagem de câmara (item 019 deste mesmo mod)** — `ChamberCheckAmmoPatch.cs:69/73` (código do mod,
   `modded-testchannel/Patches/ChamberCheckAmmoPatch.cs`) chama `screen.ShowAmmoDetails(...)` **direto**,
   sem passar pelo evento nativo (decisão de design do item 019: reusar o painel sem depender de qual
   `GamePlayerOwner` está inscrito — ver `019-checar-camara-ui-01-investigacao.md`).

Patchear em `EftBattleUIScreen.ShowAmmoDetails` cobre as DUAS origens com um único ponto — mas
**também significa que o atraso não distingue a fonte** (achado principal desta auditoria, ver §9 check 6
e §7).

**Alternativa descartada:** patchear `FirearmController.GClass2037.CheckAmmo()` e `Player.FirearmController.CheckChamber()`
separadamente exigiria dois patches e duplicaria a lógica de Coroutine/cancelamento — o ponto único já
escolhido é estrategicamente correto, só a **documentação da config** (nome/tooltip) não reflete isso.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [EftBattleUIScreen.cs:91](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/EftBattleUIScreen.cs#L91) `ShowAmmoDetails(int,int,int,string,bool)` | Prefix (skip condicional) | Ponto único onde checagem de carregador (evento nativo) e checagem de câmara (item 019, chamada direta) convergem — suprime a exibição imediata e agenda a versão atrasada. |

**Pontos de contexto (não patcheados, só documentam o fluxo — ver §6):**

| Local | Papel |
|---|---|
| [Player.cs:25504](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25504) — `event Action<int,int,int,string,bool> OnShowAmmoDetails` | Evento nativo disparado pela checagem de carregador. |
| [Player.cs:5770](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L5770) | `CheckAmmo()` nativo invoca o evento acima com os dados do carregador. |
| [GamePlayerOwner.cs:358](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GamePlayerOwner.cs#L358) | `player.OnShowAmmoDetails += owner.method_8;` — assinatura do handler que encaminha pro `EftBattleUIScreen`. É um evento **por-owner, local** (não há campo/mensagem de rede nesta cadeia) — evidência de que a feature é 100% client-side (ver §9 check 2 e critério Fika da spec funcional). |
| `ChamberCheckAmmoPatch.cs:69,73` (código do mod) | Chama `screen.ShowAmmoDetails(...)` direto pra checagem de câmara — confirma que o Prefix acima intercepta esta origem também. |

## 3. Novas propriedades F12 (BepInEx)

> Já implementadas em produção. Tooltip **corrigido** nesta spec (achado §9 check 6) — a versão em
> produção não menciona a checagem de câmara; a coluna abaixo é a redação **alvo** pro `/code-review`.

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) — alvo corrigido |
|---|---|---|---|---|---|---|
| `Weapon Inspection` | `Enable Magazine Check Delay` | bool | `true` | — | — | Quando ativado, checar o carregador **ou a câmara** na raid atrasa a exibição do HUD de munição por um tempo realista em vez de mostrar instantaneamente no frame 0. As duas checagens compartilham o mesmo painel e o mesmo atraso. |
| `Weapon Inspection` | `Magazine Check Delay Seconds` | float | `2.0` | 0.5 – 4.0 | — | Segundos de espera antes de exibir o HUD de situação do carregador **ou da câmara** durante a animação de checagem. |

**Nomes das props mantidos** (`Magazine Check Delay*`) — renomear seria breaking change (reseta o valor
salvo do usuário, ver `repo-workflow-best-practices` §7) sem ganho real; o tooltip corrigido já resolve
a ambiguidade sem quebrar config existente. `<!-- review: usuário pode preferir renomear mesmo assim
(ex.: "Ammo Check Delay") pra deixar o nome também correto, não só o tooltip — decisão de produto, não
técnica. -->`

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-testchannel/Patches/MagCheckDelayPatch.cs` | JÁ CRIADO | Prefix em `EftBattleUIScreen.ShowAmmoDetails`, Coroutine de atraso, reentry-guard, `OnRaidEnd()`. |
| `modded-testchannel/Plugin.cs` | JÁ MODIFICADO | 2 `ConfigEntry` (`_EnableMagCheckDelay`, `_MagCheckDelaySeconds`) + `SafeEnable("MagCheckDelayPatch", ...)`. **Pendente:** corrigir os 2 tooltips (§3). |
| `modded-testchannel/Patches/RaidLifecyclePatches.cs` | JÁ MODIFICADO | `GameWorldOnDestroyPatch.Postfix` chama `MagCheckDelayPatch.OnRaidEnd()` junto dos outros `Reset()`/`OnRaidEnd()`. |
| `PROPRIEDADES.md` | **PENDENTE** | Não documenta as 2 props novas — gap de processo (feature nunca passou por `/code-mod`, que atualizaria isso automaticamente). |

## 5. Código existente (não é stub — é o que já roda em produção)

```csharp
// modded-testchannel/Patches/MagCheckDelayPatch.cs (já existe)
using System;
using System.Collections;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    public class MagCheckDelayPatch : ModulePatch
    {
        private static Coroutine _activeRoutine;
        private static bool _isExecutingDelayedShow;

        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/EFT.UI/EftBattleUIScreen.cs:91
            return AccessTools.Method(typeof(EftBattleUIScreen), nameof(EftBattleUIScreen.ShowAmmoDetails), new[]
            {
                typeof(int), typeof(int), typeof(int), typeof(string), typeof(bool)
            });
        }

        [PatchPrefix]
        private static bool Prefix(EftBattleUIScreen __instance, int ammoCount, int maxAmmoCount,
            int mastering, string details, bool foldingMechanimWeapon)
        {
            try
            {
                if (_isExecutingDelayedShow) return true; // é a própria Coroutine re-chamando — deixa passar

                if (Plugin._EnableMagCheckDelay == null || !Plugin._EnableMagCheckDelay.Value) return true;

                var player = Singleton<GameWorld>.Instantiated ? Singleton<GameWorld>.Instance.MainPlayer : null;
                if (player == null || !player.IsYourPlayer || !player.FirstPersonPointOfView) return true;

                var fc = player.HandsController as Player.FirearmController;
                var weapon = fc?.Weapon;

                if (_activeRoutine != null && Plugin.Instance != null)
                {
                    Plugin.Instance.StopCoroutine(_activeRoutine); // cancela check anterior ainda pendente
                    _activeRoutine = null;
                }

                if (Plugin.Instance != null && __instance != null)
                {
                    _activeRoutine = Plugin.Instance.StartCoroutine(
                        DelayedShowRoutine(__instance, player, weapon, ammoCount, maxAmmoCount, mastering, details, foldingMechanimWeapon));
                }

                return false; // suprime a exibição instantânea no frame 0
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[MagCheckDelay] Erro no Prefix: {ex.Message}");
                return true;
            }
        }

        private static IEnumerator DelayedShowRoutine(EftBattleUIScreen screen, Player player, Weapon weapon,
            int ammoCount, int maxAmmoCount, int mastering, string details, bool foldingMechanimWeapon)
        {
            float delay = Mathf.Max(0.1f, Plugin._MagCheckDelaySeconds?.Value ?? 2.0f); // capturado 1x — mudar a config no meio não afeta este delay já em curso
            yield return new WaitForSeconds(delay);

            _activeRoutine = null;

            try
            {
                if (screen == null || !screen.gameObject.activeInHierarchy) yield break;
                if (player == null || !player.HealthController.IsAlive || !player.FirstPersonPointOfView) yield break;

                var fc = player.HandsController as Player.FirearmController;
                if (fc == null || fc.Weapon == null || fc.Weapon != weapon) yield break; // arma trocada — descarta

                _isExecutingDelayedShow = true;
                try { screen.ShowAmmoDetails(ammoCount, maxAmmoCount, mastering, details, foldingMechanimWeapon); }
                finally { _isExecutingDelayedShow = false; }
            }
            catch (Exception ex)
            {
                _isExecutingDelayedShow = false;
                Plugin.Logger.LogError($"[MagCheckDelay] Erro na exibição atrasada: {ex.Message}");
            }
        }

        public static void OnRaidEnd()
        {
            if (_activeRoutine != null && Plugin.Instance != null) Plugin.Instance.StopCoroutine(_activeRoutine);
            _activeRoutine = null;
            _isExecutingDelayedShow = false;
        }
    }
}
```

## 6. Fluxo de dados

```
[A1] Jogador aperta a tecla de check-carregador (vanilla)
      → FirearmController.GClass2037.CheckAmmo() (Player.cs:5770)
      → Player.OnShowAmmoDetails?.Invoke(...) (Player.cs:25504)
      → GamePlayerOwner.method_8 (GamePlayerOwner.cs:520-523, assinado em :358)
      → [D] EftBattleUIScreen.ShowAmmoDetails(...) (EftBattleUIScreen.cs:91)

[A2] Jogador aperta a tecla de check-câmara (item 019 deste mod)
      → Postfix de ChamberCheckAmmoPatch em Player.FirearmController.CheckChamber()
      → (gated por Plugin._ShowChamberAmmoOnCheck — se desligado, [A2] nunca dispara e o corner case
         correspondente da spec funcional já está trivialmente resolvido: sem chamada, sem painel)
      → screen.ShowAmmoDetails(...) DIRETO (ChamberCheckAmmoPatch.cs:69/73)
      → [D] EftBattleUIScreen.ShowAmmoDetails(...) (EftBattleUIScreen.cs:91)  ← MESMO PONTO que A1

[D] → [E] MagCheckDelayPatch.Prefix intercepta, retorna false (suprime), agenda DelayedShowRoutine
[E] → [F] Coroutine espera Plugin._MagCheckDelaySeconds (capturado no início, imutável durante a espera)
[F] → [G] Revalida: tela ativa, player vivo e em 1ª pessoa, MESMA arma ainda equipada
[G] → [H] _isExecutingDelayedShow=true; screen.ShowAmmoDetails(...) de novo (Prefix deixa passar) → AmmoCountPanel.Show(...)
```

Como A1 e A2 convergem no mesmo [D], o atraso configurado em `_MagCheckDelaySeconds` afeta as duas
checagens igualmente — daí o achado do §9 check 6.

## 7. Riscos e dependências

- **Acoplamento com o item 019** (`ChamberCheckAmmoPatch`): qualquer mudança futura em como o item 019
  dispara o painel (ex.: voltar a usar o evento `OnShowAmmoDetails` em vez de chamada direta) continua
  funcionando sem alteração aqui, pois o ponto patcheado (`EftBattleUIScreen.ShowAmmoDetails`) é o mesmo
  de qualquer forma — acoplamento é de **efeito colateral compartilhado**, não de dependência de código.
- **Risco de nome enganoso confirmado** (não hipótese): a config `Magazine Check Delay` afeta também o
  chamber-check. Baixo risco técnico (comportamento é até desejável — a checagem de câmara também tem
  animação de inspeção, o atraso faz sentido igual), mas risco real de confusão do usuário/suporte
  ("configurei só o carregador, por que a câmara também atrasou?").
- **Nenhum outro patch** deste mod (nem indício de outro mod na varredura de `SafeEnable`) toca
  `EftBattleUIScreen.ShowAmmoDetails` — sem risco de conflito de Harmony conhecido.
- **Dependência de `Plugin.Instance` como host de Coroutine**: padrão já usado em outros pontos deste mod
  (não é novidade introduzida por este item) — `Plugin` precisa continuar sendo um `MonoBehaviour`
  persistente pra `StartCoroutine`/`StopCoroutine` funcionarem entre frames.

## 8. Checklist de implementação

> Retroativo — os itens 1-6 já estão feitos (código em produção); os itens 7-9 são o que falta pra
> fechar esta auditoria.

- [x] Prefix em `EftBattleUIScreen.ShowAmmoDetails` com reentry-guard (`_isExecutingDelayedShow`).
- [x] Coroutine de atraso com cancelamento de check anterior pendente.
- [x] Revalidação de arma/vida/1ª-pessoa antes de exibir o painel atrasado.
- [x] `OnRaidEnd()` conectado em `RaidLifecyclePatches.cs`.
- [x] 2 `ConfigEntry` criadas e registradas.
- [x] Filtro `IsYourPlayer && FirstPersonPointOfView` (AP-02).
- [ ] Corrigir os 2 tooltips (`_EnableMagCheckDelay`, `_MagCheckDelaySeconds`) pra mencionar que a
      checagem de câmara (item 019) também é afetada — texto alvo na §3.
- [ ] Atualizar `PROPRIEDADES.md` com as 2 props novas (seção `Weapon Inspection`, que passa de 1 pra 3
      opções).
- [ ] Gerar `023-...-05-asbuild.md` retroativo, documentando que o código já existia antes desta auditoria,
      com uma seção "Validação pendente" cobrindo (PA-01-01): (1) checar carregador, confirmar atraso de
      ~2s; (2) checar câmara, confirmar que TAMBÉM atrasa; (3) checar duas vezes rápido, confirmar que só
      um painel aparece com o dado da segunda checagem; (4) trocar de arma durante o atraso, confirmar
      que nenhum painel aparece depois; (5) sair de raid com atraso pendente, confirmar que não vaza pra
      próxima raid.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | `MagCheckDelayPatch.OnRaidEnd()` para a Coroutine ativa e reseta as duas flags estáticas; conectado em `RaidLifecyclePatches.cs` (`GameWorldOnDestroyPatch.Postfix`). Idempotente: `_activeRoutine != null` guarda contra `StopCoroutine(null)`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `Prefix` checa `player.IsYourPlayer && player.FirstPersonPointOfView` antes de agendar qualquer coisa; `DelayedShowRoutine` reconfirma antes de exibir. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | `EftBattleUIScreen.ShowAmmoDetails` é método concreto, não-virtual, nome real (não ofuscado) — sem despacho virtual a auditar (confirmado lendo a declaração, `EftBattleUIScreen.cs:91`, sem `virtual`/`override`). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | O "re-disparo" usa a própria API pública (`screen.ShowAmmoDetails(...)`) — não escreve campos privados do painel/tela. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `OnRaidEnd()` cobre o fim normal de raid. Morte em raid (sem sair) é coberta pela revalidação `player.HealthController.IsAlive` dentro da própria Coroutine antes de exibir. Alt-F4: `Plugin.Instance` como `MonoBehaviour` tem sua Coroutine destruída pelo Unity junto do processo — sem vazamento entre processos (não sobrevive a fechar o jogo). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ❌→resolvido nesta spec | **Achado confirmado** (não hipótese — ver §1/§6): a config `Enable Magazine Check Delay`/`Magazine Check Delay Seconds` afeta TAMBÉM a checagem de câmara (item 019), mas o tooltip em produção não menciona isso. Resolvido nesta spec com o texto-alvo corrigido em §3 — pendente aplicar no código via `/code-review`+`/apply-code-review` (checklist §8). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` — AP-07 | ✅ | `_isExecutingDelayedShow` (bool estático) impede que a chamada de re-exibição, que invoca o MESMO método patcheado, reentre no Prefix e crie um loop — a Coroutine chama `screen.ShowAmmoDetails` de novo, mas o Prefix vê a flag ligada e deixa passar (`return true`) em vez de agendar outro atraso. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | `DelayedShowRoutine` guarda a referência da arma (`weapon`) no momento do check e reconfirma `fc.Weapon == weapon` antes de exibir — troca de arma durante o atraso descarta a exibição (`yield break`) em vez de mostrar dado desatualizado. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `EftBattleUIScreen.ShowAmmoDetails` confirmado em `EftBattleUIScreen.cs:91` nesta sessão (não só recon antigo do item 019). |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Feature não usa skill do EFT como alavanca. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Feature não declara pacote de rede. Evidência de que é dispensável: a cadeia nativa (`GamePlayerOwner.OnShowAmmoDetails`) é um evento **por-owner local**, sem campo/mensagem de rede em nenhum ponto do fluxo (§2, §6) — e o próprio `ChamberCheckAmmoPatch.cs:23` já documenta "UI LOCAL (sem sync Fika — só quem checa vê)" para o mesmo ponto de exibição. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-22 | Spec técnica criada via `/create-technical-spec` — retroativa, documentando código já em produção (build 2.25.x). Achado principal: config atrasa carregador E câmara, tooltip não menciona a segunda. |
| 2026-09-22 | Review técnica 01 — 0 bloqueadores, 2 pontos (PA-01-01, PA-01-02), ambos aceitos e aplicados inline (checklist §8 expandido; §6 fluxo `[A2]` esclarecido). |
