# 058 — Ativar masteries inertes · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [058-ativar-masteries-inertes-02-spec-tech.md](058-ativar-masteries-inertes-02-spec-tech.md)
**Data:** 2026-07-03

> Review por agente adversarial de contexto limpo (verificou refs no decompile curado, no assembly real via
> ilspycmd, no `Fika.Core.dll` e no DB do server). Decisões em modo autônomo, registradas por ponto.
> **As resoluções serão aplicadas à spec técnica numa única rodada junto com os resultados do protocolo V
> estendido** (o code-mod segue bloqueado pelo gate V de qualquer forma).

## Resumo

> 🔴 Bloqueadores: 0 (1 resolvido via protocolo estendido) · 🟡 Importantes: 6 (todos aceitos) · 🟢 Menores: 4 · Total: 11

## Índice

| ID | Cat | Impacto | Título | Decisão |
|---|---|---|---|---|
| PA-01-01 | C | 🔴 | "Launcher inequivocamente morta" é FALSO — underbarrel roteia pra Launcher via `IsInstanceOfType`; V1 não testa lançadores | ✅ Protocolo V estendido (abaixo) + premissa corrigida na spec |
| PA-01-02 | C | 🟡 | `SetCurrent` direto ignora o `OnTriggerPatch` (multiplicadores por classe) e fadiga anti-farm | ✅ Aplicar `SkillMultipliers.TryGet` no patch; desvio de fadiga documentado como decisão |
| PA-01-03 | C | 🟡 | Stub sem `silent: true` → `LevelChanged()` por acerto (spam de UI/notificação 014) | ✅ `SetCurrent(v, silent: true)` |
| PA-01-04 | C | 🟡 | Detecção por `WeapClass`/`HeavyWeapon.IsHeavy(Weapon)` não vê underbarrel (`LauncherItemClass` NÃO é `Weapon`) | ✅ `SkillFor(Item)` por TIPO C# (`SmgItemClass`/`MachineGunItemClass`/`GrenadeLauncherItemClass`+`RocketLauncherItemClass`=standalone/`LauncherItemClass`=underbarrel) |
| PA-01-05 | B | 🟡 | HMG×LMG tem discriminante plausível não avaliado (única HMG machinegun = NSV estacionária) | ✅ Matriz V4: HMG = estacionária (templateId NSV/AGS ou mounted-state); LMG = portátil |
| PA-01-06 | A | 🟡 | Matriz resultado→decisão V1–V4 incompleta (sem plano B do V3; sem ramo negativo do V4) | ✅ Tabela incógnita→resultado(±)→decisão entra na spec |
| PA-01-07 | A | 🟡 | "XP per hit" indefinido, mas derivável: paridade vanilla = 0.1/acerto (`WeaponShotAction 0.1 × ProgressRate 1`, globals) | ✅ Default `0.1`, faixa 0–1 |
| PA-01-08 | B | 🟢 | Morte/alt-F4 fora do V2 | ✅ V2 estendido com morte |
| PA-01-09 | B | 🟢 | Friendly-fire credita XP no Fika (`ObservedPlayer.ManageAggressor` sem gate de GroupId) | ✅ Corner documentado (paridade com skills funcionais no Fika) |
| PA-01-10 | B | 🟢 | Ordem Harmony com `ShootRecoilPatch` afeta o baseline do PerkDiag (052) | ✅ Fixar `HarmonyPriority`/documentar |
| PA-01-11 | C | 🟢 | Refs com drift (PerksCatalog :149→:152 · `Shoot(float str = 1f)` não `ref` · HeavyWeapon :202-221) | ✅ Corrigir na spec |

## Evidências-chave do revisor (colar na spec na rodada de aplicação)

- **PA-01-01:** `WeaponSkillClass` inscreve `WeaponShotAction.Where(weaponType.IsInstanceOfType)` (match por
  HERANÇA, não Type exato); `SkillManager.cs:2038` — `Launcher = new WeaponSkillClass(..., typeof(LauncherItemClass), ...)`
  e `LauncherItemClass` é o tipo REAL do underbarrel (`FirearmController.UnderbarrelWeapon`, Player.cs:5896/6510).
  Só HMG (`typeof(int)`) e AttachedLauncher (`typeof(float)`) são sentinelas mortas. BSG ligou underbarrel→Launcher;
  a spec mapeava underbarrel→AttachedLauncher. Splash: `DamageInfoStruct.Weapon` pode ser a MUNIÇÃO → registrar
  qual `Item` chega em `ExecuteShotSkill` por caso.
- **Fika coop PASSOU (evidência que a spec não tinha):** `FikaPlayer : LocalPlayer` não sobrescreve
  `ExecuteShotSkill`/`ManageAggressor`; `ObservedPlayer.ApplyShot` roda shooter-side → gate
  `ReferenceEquals(__instance, MainPlayer)` vale em host E cliente. `HideoutPlayer.cs:653` override VAZIO →
  sem XP no shooting range (paridade vanilla).
- **Verificações limpas:** `Player.cs:29934/29992` · `SkillManager.cs:1306-1320/2229` · `AbstractSkillClass.cs:58/100/115`
  · `SkillClass.cs:228` · `GlobalSkillsSettings` sem slots p/ as 5 · globals `[]` → veredito "server não pega" confirmado
  · stubs Harmony no molde 050 · sem conflito de alvo com patches existentes.

## Protocolo V ESTENDIDO (substitui o V1–V4 da 01-spec; resultado alimenta a rodada de aplicação)

- **V1a** SMG: ~10 acertos em bot → barra de SMG mexeu?
- **V1b** LMG: idem.
- **V1c** GL **standalone** (FN40GL/MSGL): acerto DIRETO em bot e SPLASH perto de bot → barra de **Launcher** mexeu em cada caso?
- **V1d** **Underbarrel** (GP-25/M203 acoplado): direto e splash → alguma barra mexeu (Launcher?)?
- **V2** Persistência: após qualquer XP acima, **extract** → reabrir Skills (persistiu?) e repetir com **morte**.
- **V4** (observação) há NSV/AGS estacionária no mapa? disparo montado credita algo?

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Review 01 criada (agente adversarial) — 11 pontos; protocolo V estendido publicado; aplicação na spec agendada p/ a rodada pós-resultados V |
