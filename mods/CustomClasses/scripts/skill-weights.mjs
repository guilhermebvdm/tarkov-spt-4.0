#!/usr/bin/env node
/**
 * skill-weights.mjs — fonte ÚNICA (lado JS) da tabela de pesos de skill + helpers.
 *
 * Espelha modded/Server/SkillWeights.cs (Explicit + Derived + Categories + medianas) — o port 1:1
 * do RZCustomProfiles SKILL_MULTS. Consumido por check-skill-costs.mjs (paridade de custo) e
 * class-balance-snapshot.mjs (baseline de balance) — evita 3ª cópia das tabelas.
 *
 * Ao mudar SkillWeights.cs, mudar AQUI (e só aqui no lado JS).
 */
'use strict';
import fs from 'node:fs';

export const BUDGET_MIN = 28;
export const BUDGET_MAX = 32;
export const MAX_SKILLS_WITH_POINTS = 6;
export const SUGGESTED_LEVEL_CAP = 10;

// ── Weights — MUST mirror SkillWeights.Explicit (RZ SKILL_MULTS) ──
export const EXPLICIT = {
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
export const DERIVED = {
  FirstAid: 0.94,
  FieldMedicine: 1.88,
  UsecNegotiations: 2.50,
  BearRawpower: 2.50,
};

// Category per skill — mirror of SkillWeights.Categories (coverage rule). Skills sem peso explícito
// resolvem para a MEDIANA da categoria (Ph=1.00, M=0.60, C=1.50, P=0.94). 'S' não tem mediana
// (sem skill explícita) → UsecNegotiations/BearRawpower resolvem via DERIVED.
//
// ⚠️ DELTA vs SkillWeights.cs: as 3 gems funcionais marcadas (★) NÃO existem no Categories do .cs
// ainda (caem em UnmappedFallback 1.00 lá). Adicionadas aqui na Fase 4 (derivar-por-categoria das
// gems). Replicar no SkillWeights.cs numa mudança coordenada de modded/Server (sessão paralela).
export const CATEGORIES = {
  Endurance: 'Ph', Strength: 'Ph', Vitality: 'Ph', Health: 'Ph',
  StressResistance: 'Ph', Metabolism: 'Ph', Immunity: 'Ph',
  Perception: 'M', Intellect: 'M', Attention: 'M', Charisma: 'M', Memory: 'M',
  Pistol: 'C', Revolver: 'C', Assault: 'C', Shotgun: 'C', Sniper: 'C',
  DMR: 'C', Throwing: 'C', Melee: 'C', RecoilControl: 'C', AimDrills: 'C', TroubleShooting: 'C',
  SMG: 'C', LMG: 'C', HMG: 'C', Launcher: 'C', AttachedLauncher: 'C',
  Surgery: 'P', CovertMovement: 'P', Search: 'P', MagDrills: 'P',
  LightVests: 'P', HeavyVests: 'P', WeaponTreatment: 'P',
  Crafting: 'P', HideoutManagement: 'P',
  FirstAid: 'P', FieldMedicine: 'P',
  Sniping: 'P', ProneMovement: 'P', WeaponModding: 'P', AdvancedModding: 'P', NightOps: 'P',
  SilentOps: 'P', Lockpicking: 'P',
  UsecNegotiations: 'S', BearRawpower: 'S',
  // ★ gems funcionais derivadas por categoria na Fase 4 (delta vs .cs — replicar lá):
  ShadowConnections: 'P', UsecArsystems: 'C', BearAksystems: 'C',
};

// Category medians of EXPLICIT weights — fallback for categorized skills without a weight
// (must equal SkillWeights.ComputeCategoryMedians: Ph=1.00, M=0.60, C=1.50, P=0.94)
function computeCategoryMedians() {
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
export const MEDIANS = computeCategoryMedians();

/** Mirrors SkillWeights.ResolveWeight: explicit → derived → category median → 1.00 flagged. */
export function resolveWeight(skill) {
  if (skill in EXPLICIT) return { weight: EXPLICIT[skill], origin: 'Explicit' };
  if (skill in DERIVED) return { weight: DERIVED[skill], origin: 'Derived' };
  const cat = CATEGORIES[skill];
  if (cat && cat in MEDIANS) return { weight: MEDIANS[cat], origin: 'CategoryFallback' };
  return { weight: 1.00, origin: 'UnmappedFallback' };
}

/** Strips // and block comments outside of strings (good enough for our hand-written jsonc). */
export function stripJsonc(text) {
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

/** Lê todos os .json/.jsonc de um diretório de classes → [{ file, def }] (ordenado por nome). */
export function readClasses(dir) {
  return fs.readdirSync(dir)
    .filter(f => f.endsWith('.json') || f.endsWith('.jsonc'))
    .sort()
    .map(file => ({ file, def: JSON.parse(stripJsonc(fs.readFileSync(`${dir}/${file}`, 'utf8'))) }));
}
