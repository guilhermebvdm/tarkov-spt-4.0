#!/usr/bin/env node
/**
 * class-matrix.mjs — fonte ÚNICA e reproduzível da MATRIZ skill×classe do redesign 6-classes.
 *
 * BASELINE (2026-06-22): re-sincronizado com os `.jsonc` editados no EDITOR WEB (decisão do usuário:
 * "o que editei no editor é a nova baseline"). Substitui a calibração da Fase 2. Cada classe declara:
 *   { s: ESkillId, m: XP-multiplier (1=neutro, >1 buff, <1 debuff), l: nível inicial (opcional, >0) }
 *
 * Custo/netMult (balance-model.md):
 *   custo(classe)   = Σ nível × peso
 *   netMult(classe) = Σ (m − 1) × peso
 * ⚠️ Pesos vêm de ./skill-weights.mjs. Várias skills NOVAS do editor (Memory, Health, RecoilControl,
 *    WeaponTreatment, UsecNegotiations, BearRawpower, TroubleShooting, Charisma, Intellect…) podem cair
 *    em UnmappedFallback (peso 1.00) → custo/netMult são PRELIMINARES até o skill-weights ganhar esses pesos.
 *    O cross-check vs "aprovado" foi removido (o baseline mudou) — os números abaixo SÃO o novo baseline.
 *
 * Rodar: node mods/CustomClasses/scripts/class-matrix.mjs
 */
'use strict';
import { resolveWeight, BUDGET_MIN, BUDGET_MAX, MAX_SKILLS_WITH_POINTS } from './skill-weights.mjs';

const r2 = n => Math.round(n * 100) / 100;

// Tiers (cosmético; skills sem tier saem como '?').
export const SPEED = {
  Perception: '🐇', Metabolism: '🐇', Attention: '🐇', Search: '🐇', Charisma: '🐇', Intellect: '🐇', Memory: '🐇',
  Assault: '🚶', Pistol: '🚶', SMG: '🚶', Shotgun: '🚶', DMR: '🚶', AimDrills: '🚶', MagDrills: '🚶',
  StressResistance: '🚶', Endurance: '🚶', CovertMovement: '🚶', RecoilControl: '🚶',
  Strength: '🐌', Sniper: '🐌', Vitality: '🐌', Health: '🐌', Immunity: '🐌', Melee: '🐌', Throwing: '🐌',
  Surgery: '🐌', FirstAid: '🐌', FieldMedicine: '🐌', LightVests: '🐌', HeavyVests: '🐌', WeaponTreatment: '🐌',
  TroubleShooting: '🐌', HideoutManagement: '🐌', Crafting: '🐌',
  Lockpicking: '🐌', SilentOps: '🐌', ProneMovement: '🐌', Shadowconnections: '🐌',
  UsecArsystems: '🐌', BearAksystems: '🐌', UsecNegotiations: '🐌', BearRawpower: '🐌',
};

// ── MATRIZ (baseline editor 2026-06-22) ────────────────────────────────────────
export const MATRIX = {
  medico: {
    emoji: '🩺', pt: 'Médico', en: 'Combat Medic', file: 'medicoDeCombate.jsonc',
    entries: [
      { s: 'FirstAid', m: 2.5, l: 5 }, { s: 'FieldMedicine', m: 2, l: 5 }, { s: 'Surgery', m: 2, l: 4 },
      { s: 'HideoutManagement', m: 1.5, l: 5 }, { s: 'Crafting', m: 1.5 }, { s: 'Perception', m: 1.5, l: 5 },
      { s: 'Intellect', m: 1.5 }, { s: 'Memory', m: 2, l: 5 }, { s: 'Metabolism', m: 1.5 }, { s: 'Immunity', m: 2 },
      { s: 'Health', m: 2 }, { s: 'StressResistance', m: 1.5 }, { s: 'Melee', m: 2 }, { s: 'Lockpicking', m: 1.5 },
      { s: 'Endurance', m: 1.5, l: 3 }, { s: 'TroubleShooting', m: 0.85 },
      { s: 'Assault', m: 0.6 }, { s: 'AimDrills', m: 0.7 }, { s: 'CovertMovement', m: 0.7 },
      { s: 'Shotgun', m: 0.7 }, { s: 'Sniper', m: 0.7 }, { s: 'DMR', m: 0.7 },
    ],
  },
  fuzileiro: {
    emoji: '🔫', pt: 'Fuzileiro', en: 'Rifleman', file: 'fuzileiro.jsonc',
    entries: [
      { s: 'Assault', m: 2.5, l: 7 }, { s: 'UsecArsystems', m: 2, l: 3 }, { s: 'BearAksystems', m: 2, l: 3 },
      { s: 'AimDrills', m: 1.5, l: 5 }, { s: 'MagDrills', m: 1.5, l: 4 }, { s: 'Endurance', m: 1.75, l: 5 },
      { s: 'StressResistance', m: 1.5 }, { s: 'Pistol', m: 1.5 }, { s: 'Strength', m: 1.75 }, { s: 'Vitality', m: 1.75 },
      { s: 'Immunity', m: 1.5 }, { s: 'Shotgun', m: 1.5 }, { s: 'RecoilControl', m: 1.5 }, { s: 'TroubleShooting', m: 1.5 },
      { s: 'LightVests', m: 1.5 }, { s: 'HeavyVests', m: 1.5 }, { s: 'WeaponTreatment', m: 1.5 }, { s: 'FirstAid', m: 1.5 },
      { s: 'UsecNegotiations', m: 1.5 }, { s: 'BearRawpower', m: 1.5 }, { s: 'Charisma', m: 1.25 },
      { s: 'CovertMovement', m: 0.6 }, { s: 'Attention', m: 0.7 }, { s: 'Search', m: 0.8 }, { s: 'Sniper', m: 0.8 }, { s: 'DMR', m: 0.8 },
      { s: 'SilentOps', m: 0.65 }, { s: 'ProneMovement', m: 0.65 }, { s: 'Lockpicking', m: 0.65 }, { s: 'Shadowconnections', m: 0.65 },
      { s: 'FieldMedicine', m: 0.75 }, { s: 'Crafting', m: 0.8 }, { s: 'HideoutManagement', m: 0.8 }, { s: 'Intellect', m: 0.8 }, { s: 'Memory', m: 0.8 },
    ],
  },
  cacador: {
    emoji: '🎯', pt: 'Caçador', en: 'Hunter', file: 'cacador.jsonc',
    entries: [
      { s: 'Sniper', m: 2.5, l: 7 }, { s: 'DMR', m: 1.5, l: 2 }, { s: 'AimDrills', m: 1.5 }, { s: 'ProneMovement', m: 1.5, l: 3 },
      { s: 'Pistol', m: 1.5, l: 2 }, { s: 'Perception', m: 1.5, l: 2 }, { s: 'Metabolism', m: 1.5 }, { s: 'CovertMovement', m: 2, l: 3 },
      { s: 'Immunity', m: 1.5 }, { s: 'Endurance', m: 1.5 }, { s: 'TroubleShooting', m: 1.5 }, { s: 'Surgery', m: 1.5 },
      { s: 'Crafting', m: 2 }, { s: 'FirstAid', m: 1.5 }, { s: 'SilentOps', m: 1.75 }, { s: 'Lockpicking', m: 1.5 },
      { s: 'Health', m: 1.5 }, { s: 'HideoutManagement', m: 1.5 },
      { s: 'Assault', m: 0.6 }, { s: 'BearAksystems', m: 0.8 }, { s: 'UsecArsystems', m: 0.8 },
    ],
  },
  furtivo: {
    emoji: '👻', pt: 'Furtivo', en: 'Stealth', file: 'furtivo.jsonc',
    entries: [
      { s: 'SilentOps', m: 2.5, l: 5 }, { s: 'Pistol', m: 3, l: 1 }, { s: 'CovertMovement', m: 2, l: 5 }, { s: 'Perception', m: 2, l: 5 },
      { s: 'Melee', m: 1.5, l: 3 }, { s: 'LightVests', m: 1.3 }, { s: 'ProneMovement', m: 2, l: 3 }, { s: 'Lockpicking', m: 2, l: 3 },
      { s: 'Throwing', m: 2, l: 2 }, { s: 'Shadowconnections', m: 2 }, { s: 'Memory', m: 2 },
      { s: 'Assault', m: 0.6 }, { s: 'StressResistance', m: 0.7 }, { s: 'Shotgun', m: 0.7 },
      { s: 'BearRawpower', m: 0.75 }, { s: 'UsecNegotiations', m: 0.75 }, { s: 'Vitality', m: 0.8 }, { s: 'Metabolism', m: 0.75 },
    ],
  },
  saqueador: {
    emoji: '🎒', pt: 'Saqueador', en: 'Scavenger', file: 'saqueador.jsonc',
    entries: [
      { s: 'Lockpicking', m: 3, l: 5 }, { s: 'Strength', m: 2, l: 5 }, { s: 'Shadowconnections', m: 2.5 }, { s: 'Attention', m: 1.5, l: 5 },
      { s: 'Perception', m: 1.5, l: 5 }, { s: 'Search', m: 2, l: 5 }, { s: 'HideoutManagement', m: 2 }, { s: 'Intellect', m: 1.5 },
      { s: 'Charisma', m: 1.5 }, { s: 'Endurance', m: 1.5, l: 5 }, { s: 'Memory', m: 1.5 }, { s: 'MagDrills', m: 1.5 },
      { s: 'WeaponTreatment', m: 1.5 }, { s: 'Crafting', m: 2 }, { s: 'CovertMovement', m: 2, l: 5 }, { s: 'SilentOps', m: 2 },
      { s: 'ProneMovement', m: 1.75 }, { s: 'AimDrills', m: 1.5 }, { s: 'Throwing', m: 2, l: 5 },
      { s: 'Assault', m: 0.8 }, { s: 'StressResistance', m: 0.7 },
    ],
  },
  tanque: {
    emoji: '🛡️', pt: 'Tanque', en: 'Tank', file: 'tanque.jsonc',
    entries: [
      { s: 'StressResistance', m: 2 }, { s: 'HeavyVests', m: 2, l: 3 }, { s: 'Vitality', m: 2, l: 5 }, { s: 'Strength', m: 2.5, l: 5 },
      { s: 'Shotgun', m: 3, l: 5 }, { s: 'Throwing', m: 2, l: 1 }, { s: 'Melee', m: 2 }, { s: 'LightVests', m: 2 }, { s: 'Immunity', m: 2 },
      { s: 'Endurance', m: 2 }, { s: 'Charisma', m: 1.5 }, { s: 'BearRawpower', m: 1.75 },
      { s: 'Metabolism', m: 0.5 }, { s: 'CovertMovement', m: 0.5 }, { s: 'AimDrills', m: 0.7 }, { s: 'DMR', m: 0.7 },
      { s: 'Perception', m: 0.7 }, { s: 'Intellect', m: 0.7 }, { s: 'Attention', m: 0.7 }, { s: 'Memory', m: 0.8 }, { s: 'Sniper', m: 0.7 },
      { s: 'TroubleShooting', m: 0.7 }, { s: 'Search', m: 0.7 }, { s: 'Crafting', m: 0.8 }, { s: 'HideoutManagement', m: 0.8 },
      { s: 'FirstAid', m: 0.8 }, { s: 'FieldMedicine', m: 0.8 }, { s: 'SilentOps', m: 0.7 }, { s: 'ProneMovement', m: 0.7 },
      { s: 'Lockpicking', m: 0.65 }, { s: 'Shadowconnections', m: 0.7 }, { s: 'Health', m: 0.8 },
    ],
  },
};

// ── Cálculo ────────────────────────────────────────────────────────────────────
function evalClass(def) {
  let cost = 0, net = 0, withPoints = 0;
  const rows = [], unmapped = [];
  for (const e of def.entries) {
    const { weight, origin } = resolveWeight(e.s);
    const value = (e.m - 1) * weight;
    net += value;
    if (e.l) { cost += e.l * weight; withPoints++; }
    if (origin === 'UnmappedFallback') unmapped.push(e.s);
    rows.push({ ...e, weight, origin, value: r2(value), tier: SPEED[e.s] ?? '?' });
  }
  return { cost: r2(cost), net: r2(net), withPoints, rows, unmapped: [...new Set(unmapped)] };
}

const summary = [];
for (const [key, def] of Object.entries(MATRIX)) {
  const ev = evalClass(def);
  const flags = [];
  if (ev.cost > BUDGET_MAX) flags.push(`custo ${ev.cost} > ${BUDGET_MAX}`);
  if (ev.withPoints > MAX_SKILLS_WITH_POINTS) flags.push(`${ev.withPoints} skills c/ pontos > ${MAX_SKILLS_WITH_POINTS}`);
  if (ev.unmapped.length) flags.push(`${ev.unmapped.length} skill(s) sem peso (UnmappedFallback=1.00): ${ev.unmapped.join(', ')}`);
  summary.push({ key, def, ev, flags });

  console.log(`\n=== ${def.emoji} ${def.pt} (${def.en}) ===  custo=${ev.cost}  netMult=${ev.net}  (${ev.withPoints} skills c/ pontos)`);
  for (const m of ev.rows.sort((a, b) => b.value - a.value)) {
    const lv = m.l ? `Lv${m.l}` : '  —';
    const tag = m.origin === 'UnmappedFallback' ? '  (unmapped w=1.00)' : (m.origin !== 'Explicit' ? `  (${m.origin})` : '');
    console.log(`   ${m.tier} ${m.s.padEnd(18)} ×${m.m.toFixed(2)}  ${lv.padStart(5)}  peso ${m.weight.toFixed(2)}  valor ${m.value >= 0 ? '+' : ''}${m.value.toFixed(2)}${tag}`);
  }
  for (const f of flags) console.log(`   ⚠ FLAG: ${f}`);
}

console.log(`\n\n===== RESUMO (baseline editor 2026-06-22) =====`);
console.log(`${'classe'.padEnd(14)} ${'custo'.padStart(7)} ${'netMult'.padStart(8)}  flags`);
for (const c of summary) {
  console.log(
    `${(c.def.emoji + ' ' + c.def.pt).padEnd(14)} ${c.ev.cost.toFixed(2).padStart(7)} ${('+' + c.ev.net.toFixed(2)).padStart(8)}  ${c.flags.length ? '⚠ ' + c.flags.join(' | ') : 'ok'}`
  );
}
console.log(`\n⚠️ Sem baseline "aprovado" — estes números SÃO o novo baseline (editor). Custo/netMult preliminares enquanto houver skills sem peso (ver flags) — adicionar pesos no skill-weights.mjs p/ números finais.`);
