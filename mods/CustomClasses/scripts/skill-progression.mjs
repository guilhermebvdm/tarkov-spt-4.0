#!/usr/bin/env node
/**
 * ⚫ HISTÓRICO (2026-06-21): NÃO é mais load-bearing. Decidiu-se que TODAS as signatures são
 *    perks FLAT (sem skill que escala) — ver docs/class-custom-perks.md. Este modelo de progressão
 *    (XP/freq/raids-até-nível) ficou obsoleto; mantido só como registro do raciocínio.
 *
 * skill-progression.mjs — modelo de progressão das skills custom (itens 048/049).
 * Só as skills que ESCALAM (🧪). Efeitos flat (head-start) viram perks 🔧 — fora deste modelo.
 *
 * Decompile 0.16.x (references/eft-decompiled/Assembly-CSharp/):
 *   SkillClass.cs:197  → `if (base.Level > 50) Unsubscribe()`  → MÁX = 51 (elite).
 *   AbstractSkillClass → Level = floor(Current/100); Current ∈ [0, 5100] → 100 de "Current" por nível (LINEAR).
 *   SkillClass.cs:236  → `if (Level < 9) val = CalculateExpOnFirstLevels(val)` → boost de níveis iniciais (ressalva).
 *   Nossas skills custom escrevem o Current direto (bypass da fadiga), então XP/ev é NOSSO lever de design.
 *
 * MODELO PER-EVENTO (1 evento = 1 XP/ev + 1 freq; eventos distintos somam):
 *   Current_por_raid(tier) = Σ_eventos( freq_evento[tier] × xp_evento )
 *   raids p/ nível L (tier) = (100·L) / Current_por_raid(tier)   ·   raids p/ elite = 5100 / Current_por_raid(normal)
 *
 * EFEITO (linear, normalizado pelo elite): fator(nv) = base + range·(nv/51) → alvo cheio em nv=51.
 * FREQ por evento {light/normal/heavy}. [grounded]=SkillActionClass/dado público · [reasoned]=estimado (validar in-game).
 *
 * Uso: node skill-progression.mjs
 */
'use strict';

const PER_LEVEL = 100, MAX = 51, CURRENT_MAX = PER_LEVEL * MAX;
const MILESTONES = [1, 10, 20, 30, 40, 51];
const pct = x => `${(x * 100).toFixed(x * 100 % 1 === 0 ? 0 : 1)}%`;
const ev = (name, freq, xp, basis) => ({ name, freq, xp, basis });

const SKILLS = [
  { name: 'Adrenaline', pt: 'Adrenalina', cls: 'Rifleman', cat: 'Special',
    events: [
      ev('deal damage to enemy (hit)', { light: 6, normal: 20, heavy: 50 }, 0.8, 'reasoned — hits dealt/raid; arms/renews the buff'),
      ev('take damage (under fire)',    { light: 2, normal: 8,  heavy: 20 }, 1.6, 'reasoned — hits taken/raid; arms/renews + DOUBLE xp vs dealing'),
    ],
    eff: nv => `dur ${9 + nv}s (cd 2min)` },

  { name: 'Iron Lungs', pt: 'Fôlego de Aço', cls: 'Hunter', cat: 'Special',
    events: [
      ev('aim/ADS (WeaponAimAction)',          { light: 20, normal: 60, heavy: 120 }, 0.280, 'reasoned — sniper ADS a lot/raid'),
      ev('hold breath (HoldBreathAction)',     { light: 5,  normal: 15, heavy: 30 },  1.130, 'reasoned — holds breath on some shots'),
    ],
    eff: nv => `breath ×${(1 + 1 * (nv / 51)).toFixed(2)} · sway/arm −${pct(0.5 * (nv / 51))}` },

  { name: 'Bulwark', pt: 'Couraça', cls: 'Tank', cat: 'Special',
    events: [ ev('damage taken and survived (=Vitality)', { light: 3, normal: 10, heavy: 25 }, 3.188, 'reasoned — hits taken/raid') ],
    eff: nv => `damage −${pct(0.25 * (nv / 51))}` },

  { name: 'Execution', pt: 'Execução', cls: 'Ghost', cat: 'Special',
    events: [ ev('melee strike (FistfightAction)', { light: 1, normal: 5, heavy: 15 }, 4.08, 'grounded — melee HIT (not kill), frequent') ],
    eff: nv => `melee ×${(1 + 19 * (nv / 51)).toFixed(1)} · speed +${pct(0.2 * (nv / 51))}` },
];

const curPerRaid = (s, tier) => s.events.reduce((a, e) => a + e.freq[tier] * e.xp, 0);
const raidsTo = (L, cpr) => (cpr <= 0 ? Infinity : (PER_LEVEL * L) / cpr);
const fmt = n => (n === Infinity ? '∞' : n < 10 ? n.toFixed(1) : Math.round(n).toString());

console.log(`\nPROGRESSÃO — 4 skills 🧪 (que escalam) · MÁX=${MAX} (elite, Current ${CURRENT_MAX}) · modelo per-evento`);
console.log(`[grounded]=ancorado em SkillAction/dado público · [reasoned]=estimado (validar in-game)\n`);

for (const s of SKILLS) {
  const cprN = curPerRaid(s, 'normal'), cprL = curPerRaid(s, 'light'), cprH = curPerRaid(s, 'heavy');
  console.log(`■ ${s.name} (${s.pt}) — ${s.cls} · cat=${s.cat} → elite em ~${fmt(raidsTo(51, cprN))} raids (normal)`);
  for (const e of s.events)
    console.log(`    • ${e.name} | freq L/N/H=${e.freq.light}/${e.freq.normal}/${e.freq.heavy} | XP/ev=${e.xp} | ${e.basis}`);
  console.log(`    Current/raid L/N/H = ${cprL.toFixed(1)}/${cprN.toFixed(1)}/${cprH.toFixed(1)}`);
  console.log(MILESTONES.map(L =>
    `      Lvl ${String(L).padStart(2)} → ${fmt(raidsTo(L, cprN)).padStart(4)} raids (L:${fmt(raidsTo(L, cprL))}/H:${fmt(raidsTo(L, cprH))}) → ${s.eff(L)}`).join('\n'));
  console.log('');
}
console.log(`(raids = cumulativo p/ alcançar o nível, jogo normal; L/H = casual/grinder. XP/ev de cada evento é lever no F12.)\n`);
