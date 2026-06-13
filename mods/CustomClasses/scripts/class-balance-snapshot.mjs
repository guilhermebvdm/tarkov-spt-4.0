#!/usr/bin/env node
/**
 * class-balance-snapshot.mjs — baseline de balanceamento (épico 039+). Sem deps.
 *
 * Para cada classe (modded/Server/config/classes/*.jsonc): consolida o BUDGET DE CUSTO (skills
 * iniciais, Σ nível×peso, alvo 28–32) e o BUDGET DE MULTIPLICADOR (buffs/debuffs de XP,
 * netMult = Σ (fator−1)×peso). Aplica a "regra anti-furo": um debuff numa skill que a classe não
 * treina (não-inicial e fora das categorias que ela treina) é "grátis" → mostra netMult com e sem
 * esses debuffs (netStrict). Usa a tabela de peso ÚNICA de ./skill-weights.mjs (espelha SkillWeights.cs).
 *
 * Peso = raridade de aquisição (NÃO poder de combate) — o snapshot mede INVESTIMENTO, não poder real.
 * Loadout ₽ fica fora (contexto, não calculado aqui). Run: node mods/CustomClasses/scripts/class-balance-snapshot.mjs
 */
'use strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { BUDGET_MIN, BUDGET_MAX, CATEGORIES, resolveWeight, readClasses } from './skill-weights.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const CLASSES_DIR = path.join(__dirname, '..', 'modded', 'Server', 'config', 'classes');
const r2 = n => Math.round(n * 100) / 100;

const classes = readClasses(CLASSES_DIR);
if (classes.length === 0) {
  console.error(`No class files found in ${CLASSES_DIR}`);
  process.exit(1);
}

const summary = [];

for (const { file, def } of classes) {
  const name = def.name ?? file;
  const skills = def.skills ?? {};
  const mults = def.skillMultipliers ?? {};
  const hasSkills = Object.values(skills).some(l => l > 0);
  const hasMults = Object.keys(mults).length > 0;

  // Classe sem skills nem multiplicadores (ex.: Peladão noBaseline) → isenta do balance.
  if (!hasSkills && !hasMults) {
    summary.push({ name, exempt: true });
    console.log(`\n=== ${name} ===  (ISENTA — sem skills/multiplicadores)`);
    continue;
  }

  // Budget de CUSTO.
  let skillCost = 0;
  for (const [s, lvl] of Object.entries(skills)) skillCost += lvl * resolveWeight(s).weight;
  skillCost = r2(skillCost);

  // "Plausível de treinar" (heurística da baseline): categorias das skills iniciais (lvl>0) + das skills buffadas.
  const plausibleCats = new Set();
  for (const [s, lvl] of Object.entries(skills)) if (lvl > 0 && CATEGORIES[s]) plausibleCats.add(CATEGORIES[s]);
  for (const [s, f] of Object.entries(mults)) if (f > 1 && CATEGORIES[s]) plausibleCats.add(CATEGORIES[s]);
  const isPlausible = s => (skills[s] > 0) || (mults[s] > 1) || plausibleCats.has(CATEGORIES[s]);

  // Budget de MULTIPLICADOR.
  let netAll = 0, netStrict = 0, buffs = 0, debuffs = 0;
  const rows = [], freeDebuffs = [];
  for (const [s, f] of Object.entries(mults)) {
    const { weight, origin } = resolveWeight(s);
    const value = (f - 1) * weight;
    const kind = f > 1 ? 'buff' : f < 1 ? 'debuff' : 'flat';
    if (f > 1) buffs++; else if (f < 1) debuffs++;
    netAll += value;
    const plausible = isPlausible(s);
    if (plausible) netStrict += value;
    else if (f < 1) freeDebuffs.push(s);   // debuff numa skill que a classe não treina → "grátis"
    rows.push({ s, f, weight, origin, value: r2(value), kind, plausible });
  }
  netAll = r2(netAll); netStrict = r2(netStrict);

  const flags = [];
  if (hasSkills && (skillCost < BUDGET_MIN || skillCost > BUDGET_MAX)) flags.push(`custo ${skillCost} fora de [${BUDGET_MIN},${BUDGET_MAX}]`);
  if (freeDebuffs.length) flags.push(`debuff grátis?: ${freeDebuffs.join(', ')}`);

  summary.push({ name, skillCost, netAll, netStrict, buffs, debuffs, free: freeDebuffs.length, flags });

  console.log(`\n=== ${name} ===  custo=${skillCost}  netMult=${netAll}  netMult(plausível)=${netStrict}  (${buffs} buff / ${debuffs} debuff)`);
  for (const m of rows.sort((a, b) => b.value - a.value)) {
    const tag = m.kind === 'debuff' && !m.plausible ? '  ⚠ grátis?' : (m.origin !== 'Explicit' ? `  (${m.origin})` : '');
    console.log(`   ${m.s.padEnd(18)} ×${m.f.toFixed(2)}  peso ${m.weight.toFixed(2)}  valor ${m.value >= 0 ? '+' : ''}${m.value.toFixed(2)}${tag}`);
  }
  for (const f of flags) console.log(`   FLAG: ${f}`);
}

// ── Resumo ───────────────────────────────────────────────────────────────────
console.log(`\n\n===== BASELINE (resumo) =====`);
console.log(`${'classe'.padEnd(22)} ${'custo'.padStart(6)} ${'netMult'.padStart(8)} ${'net(plaus)'.padStart(11)}  b/d   flags`);
for (const c of summary) {
  if (c.exempt) { console.log(`${c.name.padEnd(22)} ${'—'.padStart(6)} ${'isenta'.padStart(8)}`); continue; }
  const bd = `${c.buffs}/${c.debuffs}`;
  console.log(`${c.name.padEnd(22)} ${c.skillCost.toFixed(2).padStart(6)} ${c.netAll >= 0 ? '+' : ''}${c.netAll.toFixed(2).padStart(7)} ${c.netStrict >= 0 ? '+' : ''}${c.netStrict.toFixed(2).padStart(10)}  ${bd.padEnd(5)} ${c.flags.length ? '⚠ ' + c.flags.join(' | ') : ''}`);
}

const nets = summary.filter(c => !c.exempt).map(c => c.netStrict);
if (nets.length) {
  const min = Math.min(...nets), max = Math.max(...nets);
  const avg = r2(nets.reduce((a, b) => a + b, 0) / nets.length);
  console.log(`\nnetMult(plausível): min ${min.toFixed(2)} · max ${max.toFixed(2)} · média ${avg.toFixed(2)} · amplitude ${r2(max - min).toFixed(2)}`);
  console.log(`(use a amplitude/dispersão para DIMENSIONAR a meta de |net| — não para adotá-la como tolerância.)`);
}
