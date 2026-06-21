#!/usr/bin/env node
/**
 * class-matrix.mjs — fonte ÚNICA e reproduzível da MATRIZ skill×classe do redesign 6-classes.
 *
 * Materializa a "MATRIZ FINAL CALIBRADA" (aprovada na Fase 2) que antes vivia só no handoff
 * (.handoffs/handoff-2026-06-20-customclasses-class-redesign.md) e em scripts node inline — para
 * que o design não dependa de memória. Cada classe declara suas skills com:
 *   { s: ESkillId, m: XP-multiplier (1=neutro, >1 buff, <1 debuff), l: nível inicial (opcional) }
 *
 * Reusa a tabela de peso ÚNICA de ./skill-weights.mjs (espelha SkillWeights.cs). As fórmulas de
 * custo/netMult são as do balance-model.md:
 *   custo(classe)   = Σ nível × peso            (alvo 28–32)
 *   netMult(classe) = Σ (m − 1) × peso          (poder líquido de progressão; meta topo ~+6, base ~+4)
 *
 * Rodar: node mods/CustomClasses/scripts/class-matrix.mjs
 * Saída: por classe (custo, netMult, breakdown) + resumo + cross-check contra os valores APROVADOS
 * embutidos abaixo (APPROVED). Divergência custo>32 ou net≠aprovado = FLAG (pontas soltas Fase 3/4).
 *
 * Pesos das gems: derivados por CATEGORIA na Fase 4 (skill-weights.mjs) — Lockpicking/SilentOps/
 * ProneMovement/ShadowConnections → P (0.94); UsecArsystems/BearAksystems → C (1.50). SMG e
 * AttachedLauncher são INERTES no globals ("[]" → sem XP/efeito) e foram REMOVIDOS da matriz.
 * Delta a replicar no SkillWeights.cs: categorizar ShadowConnections/UsecArsystems/BearAksystems
 * (faltam lá) — mudança coordenada de modded/Server (sessão paralela).
 */
'use strict';
import { resolveWeight, BUDGET_MIN, BUDGET_MAX, MAX_SKILLS_WITH_POINTS } from './skill-weights.mjs';

const r2 = n => Math.round(n * 100) / 100;

// ── Tiers de velocidade de skill (handoff §"Tiers de velocidade") ──────────────
// 🐇 sobe-fácil · 🚶 média · 🐌 grind. Calibração: buff forte vale em 🐌; debuff só morde em 🐇/🚶.
export const SPEED = {
  // 🐇 sobe-fácil
  Perception: '🐇', Metabolism: '🐇', Attention: '🐇', Search: '🐇', Charisma: '🐇', Intellect: '🐇',
  // 🚶 média
  Assault: '🚶', Pistol: '🚶', SMG: '🚶', Shotgun: '🚶', DMR: '🚶', AimDrills: '🚶', MagDrills: '🚶',
  StressResistance: '🚶', Endurance: '🚶', CovertMovement: '🚶',
  // 🐌 grind
  Strength: '🐌', Sniper: '🐌', Vitality: '🐌', Health: '🐌', Immunity: '🐌', Melee: '🐌', Throwing: '🐌',
  Surgery: '🐌', FirstAid: '🐌', FieldMedicine: '🐌', LightVests: '🐌', HeavyVests: '🐌',
  TroubleShooting: '🐌', HideoutManagement: '🐌', Crafting: '🐌',
  Lockpicking: '🐌', SilentOps: '🐌', ProneMovement: '🐌', ShadowConnections: '🐌',
  UsecArsystems: '🐌', BearAksystems: '🐌', AttachedLauncher: '🐌',
};

// Nota de grafia: os IDs seguem o enum SkillTypes (server) — Lockpicking (não "LockPicking"),
// UsecArsystems, BearAksystems, ShadowConnections, SilentOps, ProneMovement, AttachedLauncher, SMG.

// ── MATRIZ FINAL CALIBRADA (aprovada Fase 2) ───────────────────────────────────
// s=ESkillId, m=XP-mult, l=nível inicial (ausente = só multiplicador, sem pontos iniciais).
export const MATRIX = {
  medico: {
    emoji: '🩺', pt: 'Médico', en: 'Medic', file: 'medicoDeCombate.jsonc',
    entries: [
      { s: 'FirstAid', m: 2.5, l: 5 }, { s: 'FieldMedicine', m: 2, l: 5 }, { s: 'Surgery', m: 2, l: 4 },
      { s: 'Vitality', m: 2, l: 4 }, { s: 'HideoutManagement', m: 1.5, l: 6 }, { s: 'Crafting', m: 1.5 },
      { s: 'Immunity', m: 1.2, l: 1 },
      { s: 'Assault', m: 0.6 }, { s: 'AimDrills', m: 0.7 }, { s: 'CovertMovement', m: 0.7 }, { s: 'Perception', m: 0.8 },
    ],
  },
  fuzileiro: {
    emoji: '🔫', pt: 'Fuzileiro', en: 'Rifleman', file: 'fuzileiro.jsonc',
    entries: [
      { s: 'Assault', m: 2.5, l: 7 }, { s: 'UsecArsystems', m: 2, l: 3 }, { s: 'BearAksystems', m: 2, l: 3 },
      { s: 'AimDrills', m: 1.5, l: 5 }, { s: 'MagDrills', m: 1.5, l: 4 }, { s: 'Endurance', m: 1.5, l: 5 },
      { s: 'StressResistance', m: 1.3 }, { s: 'Pistol', m: 1.2 },
      { s: 'CovertMovement', m: 0.6 }, { s: 'Attention', m: 0.7 }, { s: 'Search', m: 0.8 },
    ],
  },
  cacador: {
    emoji: '🎯', pt: 'Caçador', en: 'Hunter', file: 'cacador.jsonc',
    entries: [
      { s: 'Sniper', m: 2.5, l: 7 }, { s: 'DMR', m: 1.5, l: 2 }, { s: 'AimDrills', m: 1.5 },
      { s: 'ProneMovement', m: 1.5, l: 3 }, { s: 'Pistol', m: 1.3, l: 2 }, { s: 'Perception', m: 1.3, l: 2 },
      { s: 'Metabolism', m: 1.3 }, { s: 'CovertMovement', m: 1.2, l: 3 },
      { s: 'Assault', m: 0.6 },
    ],
  },
  fantasma: {
    emoji: '👻', pt: 'Fantasma', en: 'Ghost', file: 'fantasma.jsonc',
    entries: [
      { s: 'SilentOps', m: 2.5, l: 6 }, { s: 'CovertMovement', m: 1.5, l: 6 },
      { s: 'Perception', m: 1.5, l: 5 }, { s: 'Pistol', m: 1.8, l: 2 }, { s: 'Melee', m: 1.5, l: 3 },
      { s: 'LightVests', m: 1.3 }, { s: 'ProneMovement', m: 1.5 }, { s: 'Lockpicking', m: 1.3, l: 3 },
      { s: 'Assault', m: 0.6 }, { s: 'StressResistance', m: 0.7 }, { s: 'Shotgun', m: 0.7 },
    ],
  },
  saqueador: {
    emoji: '🎒', pt: 'Saqueador', en: 'Looter', file: 'saqueador.jsonc',
    entries: [
      { s: 'Lockpicking', m: 3, l: 8 }, { s: 'ShadowConnections', m: 2.5, l: 6 }, { s: 'Strength', m: 3, l: 7 },
      { s: 'Attention', m: 1.3, l: 8 }, { s: 'Perception', m: 1.3, l: 5 }, { s: 'Search', m: 1.3, l: 6 },
      { s: 'HideoutManagement', m: 1.2 }, { s: 'Intellect', m: 1.2 }, { s: 'Charisma', m: 1.2 },
      { s: 'Assault', m: 0.6 }, { s: 'AimDrills', m: 0.7 }, { s: 'StressResistance', m: 0.7 },
    ],
  },
  tanque: {
    emoji: '🛡️', pt: 'Tanque', en: 'Tank', file: 'tanque.jsonc',
    entries: [
      { s: 'StressResistance', m: 2 }, { s: 'HeavyVests', m: 1.5, l: 3 }, { s: 'Health', m: 1.5, l: 4 },
      { s: 'Vitality', m: 1.5, l: 4 }, { s: 'Strength', m: 1.5, l: 5 }, { s: 'Shotgun', m: 1.5, l: 1 },
      { s: 'Throwing', m: 1.5, l: 1 }, { s: 'Melee', m: 1.5 },
      { s: 'Metabolism', m: 0.5 }, { s: 'CovertMovement', m: 0.5 }, { s: 'AimDrills', m: 0.7 },
      { s: 'Pistol', m: 0.7 }, { s: 'DMR', m: 0.7 },
    ],
  },
};

// Alvos APROVADOS + recalibrados na Fase 4 (pesos de gem por categoria + remoção dos levers inertes
// SMG/AttachedLauncher). Cross-check da transcrição contra estes valores.
const APPROVED = {
  medico: { cost: 31.87, net: 6.12 }, fuzileiro: { cost: 30.51, net: 6.27 }, cacador: { cost: 31.4, net: 6.21 },
  fantasma: { cost: 30.14, net: 6.12 }, saqueador: { cost: 28.23, net: 4.09 }, tanque: { cost: 30.29, net: 4.28 },
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
  return { cost: r2(cost), net: r2(net), withPoints, rows, unmapped };
}

const summary = [];
for (const [key, def] of Object.entries(MATRIX)) {
  const ev = evalClass(def);
  const appr = APPROVED[key];
  const netOk = appr && Math.abs(ev.net - appr.net) <= 0.02;
  const costOk = appr && Math.abs(ev.cost - appr.cost) <= 0.1;
  const flags = [];
  if (ev.cost > BUDGET_MAX) flags.push(`custo ${ev.cost} > ${BUDGET_MAX}`);
  if (ev.cost < BUDGET_MIN) flags.push(`custo ${ev.cost} < ${BUDGET_MIN}`);
  if (ev.withPoints > MAX_SKILLS_WITH_POINTS) flags.push(`${ev.withPoints} skills c/ pontos > ${MAX_SKILLS_WITH_POINTS}`);
  if (appr && !netOk) flags.push(`net ${ev.net} ≠ aprovado ${appr.net}`);
  if (appr && !costOk) flags.push(`custo ${ev.cost} ≠ aprovado ${appr.cost}`);

  summary.push({ key, def, ev, appr, flags });

  console.log(`\n=== ${def.emoji} ${def.pt} (${def.en}) ===  custo=${ev.cost}  netMult=${ev.net}  (${ev.withPoints} skills c/ pontos)`);
  for (const m of ev.rows.sort((a, b) => b.value - a.value)) {
    const lv = m.l ? `Lv${m.l}` : '  —';
    const tag = m.origin === 'UnmappedFallback' ? '  (unmapped w=1.00)' : (m.origin !== 'Explicit' ? `  (${m.origin})` : '');
    console.log(`   ${m.tier} ${m.s.padEnd(18)} ×${m.m.toFixed(2)}  ${lv.padStart(5)}  peso ${m.weight.toFixed(2)}  valor ${m.value >= 0 ? '+' : ''}${m.value.toFixed(2)}${tag}`);
  }
  if (ev.unmapped.length) console.log(`   gems/unmapped (peso 1.00, ponta solta #2): ${[...new Set(ev.unmapped)].join(', ')}`);
  for (const f of flags) console.log(`   ⚠ FLAG: ${f}`);
}

// ── Resumo + cross-check ─────────────────────────────────────────────────────────
console.log(`\n\n===== RESUMO (matriz vs aprovado) =====`);
console.log(`${'classe'.padEnd(14)} ${'custo'.padStart(6)} ${'~aprov'.padStart(7)} ${'netMult'.padStart(8)} ${'~aprov'.padStart(7)}  flags`);
for (const c of summary) {
  console.log(
    `${(c.def.emoji + ' ' + c.def.pt).padEnd(14)} ${c.ev.cost.toFixed(2).padStart(6)} ${(c.appr?.cost ?? '—').toString().padStart(7)} ` +
    `${('+' + c.ev.net.toFixed(2)).padStart(8)} ${('+' + (c.appr?.net ?? '—')).padStart(7)}  ${c.flags.length ? '⚠ ' + c.flags.join(' | ') : 'ok'}`
  );
}
const allOk = summary.every(c => c.appr && Math.abs(c.ev.net - c.appr.net) <= 0.02);
console.log(`\nCross-check net vs aprovado: ${allOk ? '✅ todos batem (transcrição fiel)' : '❌ divergência — revisar transcrição'}`);
console.log(`Topo (Méd/Fuz/Caç/Fan) ~+6 · base (Saq/Tan) ~+4 — base compensada por signatures 🔧/🧪 fora do netMult.`);
