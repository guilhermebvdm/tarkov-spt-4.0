# 082 — Medroso (tremor sob fogo) · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.9.0 → **0.10.1**

Drawback NOVO (Scavenger): mãos trêmulas (Tremor) quando sob fogo. Portado do mod `UnderFire-2.0.1`, gateado só p/ o Scav.

## Implementação
Tipos ofuscados **reconfirmados no decompile atual** (sub-agent): `GClass897`(BulletSoundsUtils)/`GClass898`(SonicInfo)/`GClass3008`(Effect), `GInterface361`/`GInterface331` — todos inalterados vs. o UnderFire antigo.

- `ScavengerTremor : ActiveHealthController.GClass3008, GInterface361, IEffect, GInterface331` — subclasse própria (o Tremor nativo é `protected`).
- `Medroso.Trigger()` → `hc.AddEffect<ScavengerTremor>(EBodyPart.Head, 0.1f, dur, 1.5f)`, gate perk-on + `IsLocalClass("Scavenger")` + cooldown.
- **2 gatilhos:** (1) levar tiro — Postfix `Player.ReceiveDamage` (bala/fragmento); (2) supressão/near-miss — handler estático em `GClass897.OnShoot` (geometria projetando a posição do player na reta do tiro).
- `Medroso.Init()` registra o hook 1× (idempotente); `Medroso.ResetRaid()` zera o cooldown no raid-start.
- **⚠️ UnderFire global DESATIVADO** na instalação (D:\SPT: os 2 toggles "Enable Adrenaline On Suppression/Hit" → false) — senão TODAS as classes ganhariam o efeito. (Config não versionada; se o launcher ressincronizar, reconferir.)

## Arquivos
| Ação | Path | Resumo |
|---|---|---|
| CRIA | `Patches/MedrosoPatch.cs` | `ScavengerTremor` + `Medroso` (trigger/init/reset) + `MedrosoDamagePatch` |
| MOD | `PerksConfig.cs` | +Medroso Enabled/Duration/Cooldown/SuppressDistance |
| MOD | `PerksCatalog.cs` | +grupo `medroso` (Nervous/Medroso); ByClass Scav +medroso |
| MOD | `Plugin.cs` | +`MedrosoDamagePatch().Enable()` + `Medroso.Init()` |
| MOD | `Patches/RaidPerksNotificationPatch.cs` | +`Medroso.ResetRaid()` no raid-start |
| CFG | `D:\SPT\...\com.rpmwpm.UnderFire.cfg` | UnderFire global desativado (2 toggles → false) |

## Code-review (sub-agent adversarial) — 0 bloqueadores
| ID | Sev | Achado | Resolução |
|---|---|---|---|
| CR#7 | 🟡 | Tremor EMPILHA se `cooldown < duration` (AddEffect não deduplica efeitos com GInterface331) | **Corrigido** — `_cooldownUntil = Time + Max(cooldown, duration)` |

**Verificado limpo:** sem vazamento 075 (Trigger só mira MainPlayer; dano gateia IsYourPlayer; near-miss só afeta local); hook estático idempotente + não dispara no menu; sem persistência entre raids (Time.time monotônico + ResetRaid + HC recriado por raid); hot path com early-out cedo; sem div/0 (guard denom≤0). Nota 🟢: scav-raid aplica (a classe é atributo de conta side-agnóstico — igual a todo perk `IsLocalClass`, gap mod-wide se indesejado).

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Build via g-autodev; port do UnderFire (tipos reconfirmados); code-review CR#7 corrigido; UnderFire global off; 0.10.1 |
