# 058 — Ativar masteries inertes · As-Built

**Mod:** CustomClasses
**Spec funcional:** [058-ativar-masteries-inertes-01-spec.md](058-ativar-masteries-inertes-01-spec.md)
**Spec técnica:** [058-ativar-masteries-inertes-02-spec-tech.md](058-ativar-masteries-inertes-02-spec-tech.md) (§10 = redesenho pós-V, manda sobre §1–§8)
**Última review técnica:** [058-ativar-masteries-inertes-03-spec-tech-review-01.md](058-ativar-masteries-inertes-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-04

> Documentação **pós-implementação**. Quando diverge da spec, este documento ganha.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Client/WeaponMastery.cs` | Mapa arma-em-mãos → skill de maestria (`smg`→SMG, `machinegun`→LMG, `grenadeLauncher`→Launcher). |
| CRIADO | `mods/CustomClasses/modded/Client/Patches/WeaponMasteryPatches.cs` | 3 patches: `UnderbarrelMasteryXpPatch` (Postfix em `FirearmController.method_57` — XP por DISPARO × fator de classe, `SetCurrent(silent)` + escala do coice `float_5` por nível), `WeaponMasteryRecoilPatch` (Prefix em `PWA.Shoot`, `str × (1 − rec·lvl)`, clamp 0.5), `WeaponMasteryErgoPatch` (Postfix em `TotalErgonomics`, `× (1 + ergo·lvl)`). |
| MODIFICADO | `mods/CustomClasses/modded/Client/PerksConfig.cs` | Seção F12 `Weapon Mastery` (Enabled / XP per shot 0.1 / Recoil 0.004 / Ergo 0.002 por nível). |
| MODIFICADO | `mods/CustomClasses/modded/Client/Plugin.cs` | `Enable()` dos 3 patches. |
| MODIFICADO | `mods/CustomClasses/PROPRIEDADES.md` | Seção `Weapon Mastery`. |

## Decisões do build (vs. spec original — §10 aplicado)

| Tema | Decisão |
| --- | --- |
| Anti-XP-duplo | SMG/LMG/Launcher sobem VANILLA (validação in-game 2026-07-04) → **nenhum XP do mod** nelas; XP só pro underbarrel (AttachedLauncher). |
| Ponto do XP | `ExecuteShotSkill` NÃO recebe o underbarrel (Item do hit da explosão = munição) → Postfix em `method_57(LauncherItemClass, AmmoItemClass)` (Player.cs:14231), 1×/disparo. |
| **HMG: DEFERIDA** | Única HMG real (NSV) é estacionária de mapa (outro controller); sem arma portável pra validar. Nem XP nem efeito; candidata a item futuro com gate mounted. |
| Efeito por nível | Vale pras 4 alcançáveis (SMG/LMG/Launcher via arma em mãos; underbarrel via coice `float_5`) — inclusive as que sobem vanilla (o nível delas é decorativo: slots de buff vazios no jogo). |
| Fator de classe | XP do underbarrel multiplica pelo `SkillMultipliers` da classe (consistência com o `OnTriggerPatch`); fadiga anti-farm vanilla NÃO se aplica (decisão registrada — PA-01-02). |

## PA-NN-MM resolvidos durante o build

| ID | Resumo da resolução |
| --- | --- |
| PA-01-01 🔴 | Protocolo V estendido RODADO (perfil zerado); premissa corrigida (§10); anti-XP-duplo implementado. |
| PA-01-02/03 🟡 | Fator de classe aplicado no XP; `SetCurrent(v, true)` (silent). |
| PA-01-04 🟡 | Detecção pela ARMA EM MÃOS (weapClass) + método dedicado do underbarrel — independe do tipo do hit. |
| PA-01-05 🟡 | HMG deferida (discriminante estacionário anotado p/ item futuro). |
| PA-01-06/07 🟡 | Matriz resultado→decisão na §10; defaults fixados (0.1 / 0.004 / 0.002). |
| PA-01-08/09/10/11 🟢 | Morte = risco baixo (nativo) + V3 mod-side no checklist; friendly-fire N/A (por-disparo); prioridade Harmony desnecessária (multiplicadores comutam; PerkDiag lê baseline no próprio Prefix do 050); refs corrigidas. |

## Validação (gate humano — checklist)

- [x] Compile client+server 0 erros (2026-07-04); DLL instalada.
- [ ] Disparar GP-25 → barra "Underbarrel Launchers" sobe na tela SKILLS (UI ao vivo).
- [ ] **V3 mod-side:** extract → o XP do underbarrel PERSISTIU? (se zerar → precisa persistência server; reabre discussão de escopo)
- [ ] Efeito: overlay 052 mostra `Recoil str` menor com nível ≥1 de SMG/LMG (comparar com nível 0).
- [ ] Sem XP duplo: SMG/LMG/GL continuam subindo SÓ pelo vanilla (taxa igual à medida em 2026-07-04).
- [ ] Fika: rodar como CLIENTE; bots atirando underbarrel NÃO creditam XP no seu perfil.
- [ ] **RN-02 (rodada 2):** ao sacar SMG/LMG/GL, procurar no LogOutput a linha `"<tipo> has no buffs"` (SkillManager.cs:1900). Logou = vanilla sem efeito por nível → Perna 2 limpa; NÃO logou = vanilla JÁ aplica −0.4%/nível → isentar SMG/LMG/Launcher da Perna 2 (fix de 3 linhas no WeaponMastery.SkillForHeld).
- [ ] **R2-02 (rodada 2):** validar o efeito de RECUO com SPTRecoilRework OFF (o RealRecoil dirige a câmera fora do funil `str` — com ele ON o efeito da maestria/perks só atua no recuo procedural).

## Mudanças posteriores

### 2026-07-04 — Rodada 01 ([04-code-review-01](058-ativar-masteries-inertes-04-code-review-01.md))

- **Aplicados (7/7):** CR-01-01 (`CalculateExpOnFirstLevels` em Level<9 — paridade real dos primeiros níveis) ·
  CR-01-02 (`EnsureLoaded` antes do TryGet) · CR-01-03 (`HarmonyPriority.High` + ordem documentada no Plugin.cs;
  caveat: com maestria >0 o "Before" do overlay 052 inclui a maestria) · CR-01-04 (no-op no hideout — range não
  dá XP no vanilla) · CR-01-05 (piso 0.5 no excesso do float_5) · CR-01-06 (fator de classe respeita
  `EnableSkillMultipliers`) · CR-01-07 (clamp de fator negativo).
- **Arquivos:** `WeaponMasteryPatches.cs`, `Plugin.cs`. Recompile 0/0; DLL instalada.

### 2026-07-04 — Rodada 02 ([04-code-review-02](058-ativar-masteries-inertes-04-code-review-02.md): código pós-apply + regras de negócio)

- **Aplicados (4):** R2-01 (gate do hideout bloqueia SÓ o XP — efeito por nível vale no range) · RN-01 (**default XP 0.5**/disparo — paridade de ESFORÇO; 0.1 deixava nível 5 a 75–250 raids) · RN-05 (nota de paridade parcial no PROPRIEDADES) · RN-07 (corner obsoleto riscado na 01-spec).
- **Registrados:** RN-02 + R2-02 no checklist do gate (acima) · RN-03 proposto ao usuário (multiplicadores de maestria nos .jsonc de classe) · RN-04 → item 060 · R2-03/04, RN-06/08 anotados.
- **Fato-chave:** `HarmonyPriority` confirmado HONRADO pelo pipeline SPT (cadeia decompilada) — CR-01-03 efetivo.
- **Arquivos:** `WeaponMasteryPatches.cs`, `PerksConfig.cs`, `PROPRIEDADES.md`, `01-spec`, `mod-backlog.md` (060).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-04 | Build concluído via `/code-mod` (autônomo /g-autodev; compile 0/0) |
