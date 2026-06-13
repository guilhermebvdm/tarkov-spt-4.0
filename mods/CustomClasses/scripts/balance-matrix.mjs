#!/usr/bin/env node
/**
 * balance-matrix.mjs — matriz skills × classes (espelha o heatmap do editor /customclasses/skills),
 * focada no épico de balance. Cada célula mostra "<nível inicial> ×<multiplicador>" quando existem.
 *
 * Linhas = skills (agrupadas por categoria Ph/M/C/P/SE, só as usadas por alguma classe).
 * Colunas = classes (displayName.pt, abreviado). Rodapé = netMult por classe.
 * Sem deps. Run: node mods/CustomClasses/scripts/balance-matrix.mjs
 */
'use strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { CATEGORIES, resolveWeight, readClasses } from './skill-weights.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const CLASSES_DIR = path.join(__dirname, '..', 'modded', 'Server', 'config', 'classes');
const r2 = n => Math.round(n * 100) / 100;

const CAT_ORDER = ['Ph', 'M', 'C', 'P', 'S'];
const CAT_LABEL = { Ph: 'Físicas', M: 'Mentais', C: 'Combate', P: 'Práticas', S: 'SE / outras' };

const classes = readClasses(CLASSES_DIR).map(({ def, file }) => {
  const label = (def.displayName && def.displayName.pt) || def.name || file;
  return { label, skills: def.skills ?? {}, mults: def.skillMultipliers ?? {} };
});

// Abreviação de coluna (≤4 chars) a partir do label pt.
const abbr = l => l.replace(/Operador\s+/i, '').replace(/\s+de\s+Operações/i, '').slice(0, 4);

// Conjunto de skills usadas (nível>0 OU multiplicador).
const used = new Set();
for (const c of classes) {
  for (const [s, lvl] of Object.entries(c.skills)) if (lvl > 0) used.add(s);
  for (const s of Object.keys(c.mults)) used.add(s);
}

// Ordena skills por categoria.
const byCat = {};
for (const s of used) (byCat[CATEGORIES[s] ?? 'S'] ??= []).push(s);
for (const cat of Object.keys(byCat)) byCat[cat].sort();

const cols = classes;
const cell = (c, s) => {
  const lvl = c.skills[s] > 0 ? c.skills[s] : null;
  const m = c.mults[s];
  const mTxt = m != null ? `×${m}` : '';
  const lTxt = lvl != null ? `${lvl}` : '';
  if (lTxt && mTxt) return `${lTxt} ${mTxt}`;
  return lTxt || mTxt || '';
};

// netMult por classe.
const net = c => {
  let n = 0;
  for (const [s, f] of Object.entries(c.mults)) n += (f - 1) * resolveWeight(s).weight;
  return r2(n);
};

// ── Render markdown ──
const head = ['Skill', 'peso', ...cols.map(c => abbr(c.label))];
const sep = head.map(() => '---');
const lines = [`| ${head.join(' | ')} |`, `| ${sep.join(' | ')} |`];

for (const cat of CAT_ORDER) {
  if (!byCat[cat]?.length) continue;
  lines.push(`| **${CAT_LABEL[cat]}** | | ${cols.map(() => '').join(' | ')} |`);
  for (const s of byCat[cat]) {
    const w = resolveWeight(s).weight.toFixed(2);
    const row = cols.map(c => cell(c, s));
    lines.push(`| ${s} | ${w} | ${row.join(' | ')} |`);
  }
}
lines.push(`| **netMult** | | ${cols.map(c => { const n = net(c); return Object.keys(c.mults).length ? (n >= 0 ? '+' : '') + n.toFixed(2) : '—'; }).join(' | ')} |`);

console.log(lines.join('\n'));
console.log('\nLegenda: célula = `<nível inicial> ×<multiplicador de XP>`. Vazio = skill não usada. netMult = Σ (fator−1)×peso (meta ~+6, Médico = padrão).');
console.log('Colunas:', cols.map(c => `${abbr(c.label)}=${c.label}`).join(' · '));
