#!/usr/bin/env node
/**
 * check-skill-costs.mjs — item 022 parity check (no deps).
 *
 * Reads every class file in modded/Server/config/classes/*.jsonc, computes the weighted
 * skill cost with the SAME table/formula as modded/Server/SkillWeights.cs + CostService.cs
 * (the 1:1 port of RZCustomProfiles SKILL_MULTS / weightedCost), and prints class → cost,
 * flagging the [28, 32] budget and the informative design rules.
 *
 * The weight tables live in ./skill-weights.mjs (single JS source, mirrors SkillWeights.cs).
 * Run: node mods/CustomClasses/scripts/check-skill-costs.mjs
 */
'use strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
  BUDGET_MIN, BUDGET_MAX, MAX_SKILLS_WITH_POINTS, SUGGESTED_LEVEL_CAP,
  CATEGORIES, resolveWeight, readClasses,
} from './skill-weights.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const CLASSES_DIR = path.join(__dirname, '..', 'modded', 'Server', 'config', 'classes');

const classes = readClasses(CLASSES_DIR);
if (classes.length === 0) {
  console.error(`No class files found in ${CLASSES_DIR}`);
  process.exit(1);
}

console.log(`check-skill-costs — ${classes.length} class file(s), budget [${BUDGET_MIN}, ${BUDGET_MAX}]\n`);

let anyOutOfBudget = false;

for (const { file, def } of classes) {
  const skills = def.skills ?? {};
  const lines = [];
  const covered = new Set();
  let total = 0;
  let withPoints = 0;
  const notes = [];

  for (const [skill, level] of Object.entries(skills)) {
    const { weight, origin } = resolveWeight(skill);
    const cost = level * weight;
    total += cost;
    if (level > 0) {
      withPoints++;
      if (CATEGORIES[skill]) covered.add(CATEGORIES[skill]);
    }
    if (level > SUGGESTED_LEVEL_CAP) notes.push(`${skill}=${level} > cap ${SUGGESTED_LEVEL_CAP}`);
    if (origin !== 'Explicit') notes.push(`${skill}: ${origin} weight ${weight.toFixed(2)}`);
    lines.push(`    ${skill.padEnd(18)} lvl ${String(level).padStart(2)} × ${weight.toFixed(2)} = ${cost.toFixed(2)}`);
  }

  total = Math.round(total * 100) / 100;
  const inBudget = total >= BUDGET_MIN && total <= BUDGET_MAX;
  const isEmpty = withPoints === 0;
  if (!inBudget && !isEmpty) anyOutOfBudget = true;

  const status = isEmpty ? 'OK (no skills — intentional)' : inBudget ? 'OK' : 'OUT OF BUDGET';
  console.log(`${(def.name ?? file).padEnd(24)} cost ${total.toFixed(2).padStart(6)}  ${status}`);
  for (const l of lines) console.log(l);

  if (!isEmpty) {
    const missing = ['Ph', 'M', 'C', 'P'].filter(c => !covered.has(c));
    if (missing.length > 0) notes.push(`categories without coverage: ${missing.join(', ')}`);
    if (withPoints > MAX_SKILLS_WITH_POINTS) notes.push(`${withPoints} skills > max ${MAX_SKILLS_WITH_POINTS}`);
  }
  for (const n of notes) console.log(`    note: ${n}`);
  console.log('');
}

console.log(anyOutOfBudget
  ? 'RESULT: at least one class outside the [28, 32] budget — review above.'
  : 'RESULT: all classes with skills are inside the [28, 32] budget (parity with the RZ formula).');
process.exit(anyOutOfBudget ? 1 : 0);
