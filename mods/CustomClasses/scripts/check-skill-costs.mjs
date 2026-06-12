#!/usr/bin/env node
/**
 * check-skill-costs.mjs — item 022 parity check (no deps).
 *
 * Reads every class file in modded/Server/config/classes/*.jsonc, computes the weighted
 * skill cost with the SAME table/formula as modded/Server/SkillWeights.cs + CostService.cs
 * (which is the 1:1 port of RZCustomProfiles SKILL_MULTS / weightedCost), and prints
 * class → cost, flagging the [28, 32] budget and the informative design rules.
 *
 * The weight tables below MUST stay identical to SkillWeights.cs (Explicit + Derived).
 * Run: node mods/CustomClasses/scripts/check-skill-costs.mjs
 */
'use strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const CLASSES_DIR = path.join(__dirname, '..', 'modded', 'Server', 'config', 'classes');

const BUDGET_MIN = 28;
const BUDGET_MAX = 32;
const MAX_SKILLS_WITH_POINTS = 6;
const SUGGESTED_LEVEL_CAP = 10;

// ── Weights — MUST mirror SkillWeights.Explicit (RZ SKILL_MULTS) + SkillWeights.Derived ──
const EXPLICIT = {
  // Ph — Physical
  Endurance: 1.00, Strength: 0.47, Vitality: 1.67, Health: 1.67,
  StressResistance: 0.88, Metabolism: 0.29, Immunity: 3.75,
  // M — Mental
  Perception: 0.88, Intellect: 0.68, Attention: 0.60, Charisma: 0.40, Memory: 0.50,
  // C — Combat
  Pistol: 3.00, Revolver: 0.71, Assault: 1.00, Shotgun: 2.50, Sniper: 1.50,
  DMR: 3.75, Throwing: 0.83, Melee: 1.88,
  RecoilControl: 1.00, AimDrills: 1.15, TroubleShooting: 2.50,
  // P — Practical
  Surgery: 1.25, CovertMovement: 0.94, Search: 0.43, MagDrills: 0.94,
  LightVests: 3.75, HeavyVests: 3.75, WeaponTreatment: 1.25,
  Crafting: 0.33, HideoutManagement: 0.39,
};

// Skills-Extended derived weights — MUST mirror SkillWeights.Derived (rationale in SkillWeights.cs)
const DERIVED = {
  FirstAid: 0.94,
  FieldMedicine: 1.88,
  UsecNegotiations: 2.50,
  BearRawpower: 2.50,
};

// Category per skill — mirror of SkillWeights.Categories for the live skills (coverage rule)
const CATEGORIES = {
  Endurance: 'Ph', Strength: 'Ph', Vitality: 'Ph', Health: 'Ph',
  StressResistance: 'Ph', Metabolism: 'Ph', Immunity: 'Ph',
  Perception: 'M', Intellect: 'M', Attention: 'M', Charisma: 'M', Memory: 'M',
  Pistol: 'C', Revolver: 'C', Assault: 'C', Shotgun: 'C', Sniper: 'C',
  DMR: 'C', Throwing: 'C', Melee: 'C', RecoilControl: 'C', AimDrills: 'C', TroubleShooting: 'C',
  Surgery: 'P', CovertMovement: 'P', Search: 'P', MagDrills: 'P',
  LightVests: 'P', HeavyVests: 'P', WeaponTreatment: 'P',
  Crafting: 'P', HideoutManagement: 'P',
  FirstAid: 'P', FieldMedicine: 'P', UsecNegotiations: 'S', BearRawpower: 'S',
};

// Category medians of EXPLICIT weights — fallback for categorized skills without a weight
// (must equal SkillWeights.ComputeCategoryMedians: Ph=1.00, M=0.60, C=1.50, P=0.94)
function categoryMedians() {
  const byCat = {};
  for (const [skill, w] of Object.entries(EXPLICIT)) {
    (byCat[CATEGORIES[skill]] ??= []).push(w);
  }
  const medians = {};
  for (const [cat, ws] of Object.entries(byCat)) {
    ws.sort((a, b) => a - b);
    const n = ws.length;
    medians[cat] = n % 2 === 1 ? ws[(n - 1) / 2] : Math.round(((ws[n / 2 - 1] + ws[n / 2]) / 2) * 100) / 100;
  }
  return medians;
}
const MEDIANS = categoryMedians();

/** Mirrors SkillWeights.ResolveWeight: explicit → derived → category median → 1.00 flagged. */
function resolveWeight(skill) {
  if (skill in EXPLICIT) return { weight: EXPLICIT[skill], origin: 'Explicit' };
  if (skill in DERIVED) return { weight: DERIVED[skill], origin: 'Derived' };
  const cat = CATEGORIES[skill];
  if (cat && cat in MEDIANS) return { weight: MEDIANS[cat], origin: 'CategoryFallback' };
  return { weight: 1.00, origin: 'UnmappedFallback' };
}

/** Strips // and /* *​/ comments outside of strings (good enough for our hand-written jsonc). */
function stripJsonc(text) {
  let out = '';
  let inString = false;
  let inLine = false;
  let inBlock = false;
  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    const next = text[i + 1];
    if (inLine) {
      if (c === '\n') { inLine = false; out += c; }
      continue;
    }
    if (inBlock) {
      if (c === '*' && next === '/') { inBlock = false; i++; }
      continue;
    }
    if (inString) {
      out += c;
      if (c === '\\') { out += next ?? ''; i++; }
      else if (c === '"') inString = false;
      continue;
    }
    if (c === '"') { inString = true; out += c; continue; }
    if (c === '/' && next === '/') { inLine = true; continue; }
    if (c === '/' && next === '*') { inBlock = true; i++; continue; }
    out += c;
  }
  return out;
}

// ── Main ─────────────────────────────────────────────────────────────────────

const files = fs.readdirSync(CLASSES_DIR)
  .filter(f => f.endsWith('.json') || f.endsWith('.jsonc'))
  .sort();

if (files.length === 0) {
  console.error(`No class files found in ${CLASSES_DIR}`);
  process.exit(1);
}

console.log(`check-skill-costs — ${files.length} class file(s), budget [${BUDGET_MIN}, ${BUDGET_MAX}]\n`);

let anyOutOfBudget = false;

for (const file of files) {
  const raw = fs.readFileSync(path.join(CLASSES_DIR, file), 'utf8');
  const def = JSON.parse(stripJsonc(raw));
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
