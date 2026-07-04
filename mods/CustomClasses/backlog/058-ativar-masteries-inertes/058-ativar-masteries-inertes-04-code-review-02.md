# 058 — Ativar masteries inertes · Code Review 02 (código pós-apply + regras de negócio)

**Mod:** CustomClasses
**Objeto:** estado pós-`656ca97` (fixes da rodada 1) — pedido explícito do usuário: code-review + review de regras de negócio.
**Data:** 2026-07-04

> Dois revisores adversariais independentes de contexto limpo: (A) código — foco nos fixes aplicados na rodada 1
> (nunca revisados) + interop com mods do install real; (B) regras de negócio — 7 dimensões quantitativas
> (economia de XP, paridade, stacking, percepção, coerência de roster, aceite).

## Resumo

> **Código (A):** 🔴 0 · 🟠 0 · 🟡 2 · 🟢 2 — **APROVADO** (os 7 fixes da rodada 1 corretos; nenhum inerte).
> **Regras de negócio (B):** 🟠 2 · 🟡 2 · 🟢 4 — **"jogáveis e coerentes"**, 2 ajustes pré-gate.
> **Aplicados nesta rodada: 4** (R2-01, RN-01, RN-05, RN-07) · **Registrados: 6** (R2-02/03/04, RN-02 no gate, RN-03 proposto ao usuário, RN-04 → item 060).

## Base fatual estabelecida pelos revisores (evidência decompilada)

- **`[HarmonyPriority(Priority.High)]` É HONRADO** ponta a ponta: `ModulePatch.GetPatchMethods` → `new HarmonyMethod(MethodInfo)` → `ImportMethod`/`GetFromMethod` lê o atributo → `PatchSorter.PriorityComparer` ordena descendente entre instâncias Harmony distintas. `ShootRecoilPatch` sem atributo = Normal(400) < High(600) → maestria SEMPRE antes, independente dos `Enable()`. CR-01-03 efetivo.
- Curva de XP real: 1 nível = 100 Current interno (clamp 5100 → nível máx 51); custo cru por nível = (n+1)×10 até o 9, depois 100. `CalculateExpOnFirstLevels` é função pura e o call-site novo reproduz o vanilla (nuance R2-03).
- Gate do hideout sem falso-positivo (`HideoutPlayer : LocalPlayer` é o MainPlayer no hideout; nenhum tipo de raid casa com "Hideout"); e o range **permite** underbarrel (`ReleaseShootingRangeInventory` → `ToggleLauncher`, HideoutPlayer.cs:576-578) — o gate não é dead code.
- Paridade dos efeitos EXATA com o vanilla: `WeaponSkillRecoilBonusPerLevel = 0.004f` (BackendConfigSettingsClass.cs:2168) e ergo 0.002 (SkillManager.cs:1941). No cap (51): recuo −20.4% / ergo +10.2% — a maestria só alcança o Bunker (−15% flat) no nível 38: **complementa, não ofusca**.
- Stacking saudável (classes mutuamente exclusivas): pior caso real = Fuzileiro+SMG51 na janela Adrenaline → recuo ×0.557; Tanque+LMG51 → ×0.677 recuo / ×1.267 ergo. Nenhum caso degenerado nos defaults.

## Achados e decisões

### Aplicados (mesma rodada)

| ID | Impacto | Título | Resolução |
|---|---|---|---|
| R2-01 | 🟡 | Gate do hideout matava também a perna de EFEITO do underbarrel (range mentiria sobre o in-raid) | Gate movido: bloqueia SÓ o XP; escala do `float_5` vale no range (como recuo/ergo das outras) |
| RN-01 | 🟠 | Default 0.1/disparo sub-tunado — paridade POR AÇÃO ≠ POR ESFORÇO (nível 5 = 75–250 raids, ₽0,7–3,2M em VOG; custo/XP ~100× o da SMG) | **Default 0.5** (nível 1 ≈ 20 disparos ≈ 1–3 raids; nível 51 segue aspiracional: 9.300 disparos). Tooltip + PROPRIEDADES atualizados |
| RN-05 | 🟢 | Maestria modded dá só recuo+ergo (vanilla tem também reload/swap/elite ×2) | Nota de escopo consciente no PROPRIEDADES |
| RN-07 | 🟢 | Corner "XP por acerto" da 01-spec obsoleto pós-V1d | Corner riscado/emendado na 01-spec |

### Registrados (gate / follow-up / decisão do usuário)

| ID | Impacto | Título | Destino |
|---|---|---|---|
| RN-02 | 🟠 | **Risco de buff vanilla DUPLICADO em SMG/LMG/Launcher** — a premissa "nível decorativo" veio do recon que o V1 derrubou pro XP; o efeito vanilla usa o MESMO match por tipo (`GetWeaponInfo` → `WeaponBuffs[weapon.GetType()]`, SkillManager.cs:1886). Se o vanilla JÁ aplica −0.4%/nível nelas, a Perna 2 DOBRA (−0.8%/nível = 2× a régua da Assault) | **Checklist do gate** (asbuild): sacar SMG/LMG/GL e procurar o log `"<tipo> has no buffs"` (SkillManager.cs:1900). Logou → limpo; NÃO logou → isentar as 3 vanilla da Perna 2. O overlay 052 NÃO detecta (buff vanilla entra via GClass2250, não via PWA.Shoot) |
| R2-02 | 🟡 | **SPTRecoilRework** (instalado) dirige o recuo de CÂMERA fora do funil `str` (Postfix que ignora o parâmetro) → o efeito de recuo da maestria (e dos perks do 050!) só atua no recuo procedural | **Asbuild/gate:** validar recuo com RealRecoil OFF, ou aceitar efeito parcial. Limitação de interop documentada |
| RN-03 | 🟡 | Matriz de classe sem multiplicador pras maestrias — Tanque upa LMG na MESMA velocidade que o Médico (contradiz identidade-por-velocidade do class-design) | **Proposto ao usuário** (dado de classe = curadoria sua, via editor web): `tanque.jsonc` += `"LMG": 2, "Launcher": 2, "AttachedLauncher": 2` (na linha do `"Shotgun": 3` existente); Fuzileiro += `"SMG": 1.5`; Furtivo += `"SMG": 2`. Rota é lida ao vivo — vale pra perfis existentes |
| RN-04 | 🟡 | Zero transparência in-game do efeito por nível (quadradinhos vazios ensinam "skill não faz nada") | **Item 060 criado no backlog** (card "Weapon Mastery" na aba CLASS ou badge via SkillPanelPatch) |
| R2-03 | 🟢 | Ordem fator×amplificação difere do vanilla só no tiro que cruza fronteira de nível (~1 tiro/nível) | Anotado; sem ação |
| R2-04 | 🟢 | 1º disparo da sessão pode pagar o GET síncrono do EnsureLoaded (na prática outro call-site sempre roda antes) | Anotado; sem ação |
| RN-06 | 🟢 | XP do mod pula a fadiga vanilla (`UseEffectiveness`) | Decisão já documentada (PA-01-02); custo da VOG é o freio natural |
| RN-08 | 🟢 | Ergo do Tanque no cap encosta em ×1.267 (teto informal 1.30); F12 ≥0.003 ultrapassa | Anotado (escolha explícita do usuário) |

## Aceite da 01-spec (mapa do revisor B)

XP nas categorias: **parcial por redesenho documentado** (underbarrel por disparo; 3 via vanilla; HMG deferida) ·
Efeito por nível: ✓ paridade exata (+ risco RN-02 no gate) · Persistência: vanilla ✓ / mod-side no gate (V3) ·
Coexistência com Bunker: ✓ quantificada · Sem XP duplo: ✓ (confirmação de taxa no gate) · Fika: ✓ gates nos 3
patches (teste como cliente no gate) · Estado entre raids: = persistência.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Rodada 2 (2 revisores paralelos: código + regras de negócio) — 4 aplicados, 6 registrados; recompile 0/0 |
