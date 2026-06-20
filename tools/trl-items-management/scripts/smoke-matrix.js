#!/usr/bin/env node
/**
 * smoke-matrix.js — richer override smoke test.
 *
 * Validates the source-derived flea formula:
 *     base[tpl] = (override ?? prices.json[tpl] ?? 0) + bonus[tpl]
 *     bonus[tpl] = max( handbook × M(tpl) , traderSell )      (clamp ON)
 *     M(tpl)     = ItemTplMultiplierOverride[tpl]
 *                | ItemTypeMultiplierOverride[baseclass]
 *                | 1.5   (+0.8 if hideout craft ingredient)
 *     offer      = base × quality × variance(0.8..1.2)
 *
 * Strategy: for each item pick a target flea price X and write
 *     override = X − bonus_predicted
 * so that (if the formula holds) offers center on X. Reading the offers
 * against X confirms the additive formula, the replace-not-sum behaviour,
 * the per-item/per-type multipliers, and the negative-override edge — all
 * at once. Mismatch → back-calculate the real bonus from observed_base.
 *
 * Modes: prep | revert | status
 *
 * State: resets itemPriceOverrideRouble to {vanilla defaults from backup}
 * + {test items} on every prep, so it is idempotent and leaves no residue
 * from the older 2-item action0 prep.
 */
'use strict';

const fs     = require('fs');
const path   = require('path');
const crypto = require('crypto');

const SPT_ROOT = process.env.SPT_PATH || 'D:/SPT/SPT';
const SPT_DATA = fs.existsSync(path.join(SPT_ROOT, 'SPT_Data'))
  ? path.join(SPT_ROOT, 'SPT_Data') : SPT_ROOT;

const RAGFAIR_JSON = path.join(SPT_DATA, 'configs', 'ragfair.json');
const RAGFAIR_BAK  = path.join(SPT_DATA, 'configs', 'ragfair.json.pre-action0-backup');
const CHECKS_DAT   = path.join(SPT_DATA, 'checks.dat');

// ── Test matrix ──────────────────────────────────────────────────────────────
// X is the intended flea price; `override` is written = X − bonus so offers
// should center on X. `predictBase` documents the expected in-memory base.
const MATRIX = [
  { id: 'A', tpl: '57347ca924597744596b4e71', label: 'GPU',                X: 500000,  bonus: 297000,   override: 203000,   search: 'graphic',                       scenario: 'non-craft ×1.5, prices.json present → replace-not-sum' },
  { id: 'B', tpl: '57347c5b245977448d35f6e1', label: 'Bolts',              X: 200000,  bonus: 25300,    override: 174700,   search: 'bolts',                         scenario: 'craft ×2.3, prices.json present → replace-not-sum' },
  { id: 'C', tpl: '666b11055a706400b717cfa5', label: 'Tripwire kit',       X: 300000,  bonus: 36963,    override: 263037,   search: 'tripwire',                      scenario: 'non-craft ×1.5, prices.json ABSENT → bonus from nothing' },
  { id: 'D', tpl: '6389c7750ef44505c87f5996', label: 'Microcontroller',    X: 600000,  bonus: 230000,   override: 370000,   search: 'microcontroll',                 scenario: 'craft ×2.3, prices.json ABSENT' },
  { id: 'E', tpl: '59fb016586f7746d0d4b423a', label: 'Money case',         X: 2000000, bonus: 597302,   override: 1402698,  search: 'money case',                    scenario: 'non-craft ×1.5, BIG prices.json (1.4M) → strong replace(2M) vs sum(3.4M) discriminator' },
  { id: 'F', tpl: '5c0530ee86f774697952d952', label: 'LEDX',               X: 400000,  bonus: null,     override: null,     search: 'ledx',                          scenario: 'NEGATIVE override (X=400k << bonus ~1.45M). non-craft ×1.5, prices.json present. center 400k=neg+replace OK | ~1.45M=neg clamped to 0 | ~1.62M=neg OK but summed w/prices.json' },
  { id: 'G', tpl: '5780cf7f2459777de4559322', label: 'Dorm 314 key',       X: 4000000, bonus: 2700000,  override: 1300000,  search: 'dorm room 314',                 scenario: 'ItemTplMultiplierOverride M=1.8 (NOT 1.5) — cond=uses, read full-uses offer' },
  { id: 'H', tpl: '5c1d0c5f86f7744bb2683cf0', label: 'Labs keycard Blue',  X: 3000000, bonus: 2250000,  override: 750000,   search: 'terragroup labs keycard (blue', scenario: 'ItemTypeMultiplierOverride (Keycard) M=2.5 — cond=uses, read full-uses offer' },
  { id: 'I', tpl: '6038b4b292ec1c3103795a0b', label: 'LBT Slick PC',       X: null,    bonus: null,     override: 250000,   search: 'slick plate carrier (coyote',   scenario: 'CLAMP diagnostic: traderSell >> handbook×mult. base = 250000 + max(13449, traderSell). Back-calc bonus from observed.' },
];

// F: Bundle of wires — craft. bonus computed live (handbook × 2.3). override = X − bonus.
function fillDynamic() {
  const hb = JSON.parse(fs.readFileSync(path.join(SPT_DATA, 'database/templates/handbook.json'), 'utf8'));
  const H = t => { const e = (hb.Items || []).find(i => i.Id === t); return e ? e.Price : null; };
  for (const row of MATRIX) {
    if (row.id === 'F') {
      const h = H(row.tpl);
      row.handbook = h;
      row.bonus = Math.round(h * 1.5); // LEDX is non-craft → ×1.5
      row.override = row.X - row.bonus; // negative by construction (X << bonus)
    }
  }
}

// ── helpers (shared shape with action0-override-smoke-test.js) ────────────────
function readJson(p) { return JSON.parse(fs.readFileSync(p, 'utf8').replace(/^﻿/, '')); }

function detectIndent(rawText) {
  const m = rawText.match(/^([\t ]+)"/m);
  if (!m) return '\t';
  return m[1][0] === '\t' ? '\t' : m[1].length;
}

function writeJsonPreservingStyle(p, obj) {
  const original = fs.readFileSync(p, 'utf8').replace(/^﻿/, '');
  const indent = detectIndent(original);
  const trailingNewline = original.endsWith('\n');
  let serialized = JSON.stringify(obj, null, indent);
  if (trailingNewline) serialized += '\n';
  fs.writeFileSync(p, serialized, 'utf8');
}

function md5OfFile(p) { return crypto.createHash('md5').update(fs.readFileSync(p)).digest('hex').toUpperCase(); }

function refreshChecksDat(relPath) {
  if (!fs.existsSync(CHECKS_DAT)) throw new Error('checks.dat not found at ' + CHECKS_DAT);
  const raw = fs.readFileSync(CHECKS_DAT, 'utf8');
  const manifest = JSON.parse(Buffer.from(raw, 'base64').toString('utf8'));
  const entry = manifest.find(x => x.Path === relPath);
  if (!entry) throw new Error(`checks.dat has no entry for ${relPath}`);
  const newHash = md5OfFile(path.join(SPT_DATA, relPath));
  const oldHash = entry.Hash;
  entry.Hash = newHash;
  const reencoded = Buffer.from(JSON.stringify(manifest, null, 2), 'utf8').toString('base64') + '\n';
  fs.writeFileSync(CHECKS_DAT, reencoded, 'utf8');
  return { oldHash, newHash, changed: oldHash !== newHash };
}

function vanillaOverrides() {
  // The pre-action0 backup holds the pristine 5 SPT-default overrides.
  if (!fs.existsSync(RAGFAIR_BAK)) throw new Error('vanilla backup missing: ' + RAGFAIR_BAK);
  const bak = readJson(RAGFAIR_BAK);
  return (bak.dynamic && bak.dynamic.itemPriceOverrideRouble) || {};
}

// ── commands ─────────────────────────────────────────────────────────────────
function cmdPrep() {
  fillDynamic();
  if (!fs.existsSync(RAGFAIR_BAK)) { fs.copyFileSync(RAGFAIR_JSON, RAGFAIR_BAK); console.log('→ Backup criado:', RAGFAIR_BAK); }

  const vanilla = vanillaOverrides();
  const ragfair = readJson(RAGFAIR_JSON);
  const map = { ...vanilla };
  for (const row of MATRIX) map[row.tpl] = row.override;
  ragfair.dynamic.itemPriceOverrideRouble = map;
  writeJsonPreservingStyle(RAGFAIR_JSON, ragfair);

  const r = refreshChecksDat('configs/ragfair.json');
  console.log('→ ragfair.json escrito (5 vanilla + ' + MATRIX.length + ' teste). checks.dat:', r.oldHash, '→', r.newHash, '(changed=' + r.changed + ')');
  console.log('');
  printTable();
}

function cmdRevert() {
  if (!fs.existsSync(RAGFAIR_BAK)) throw new Error('backup missing: ' + RAGFAIR_BAK);
  fs.copyFileSync(RAGFAIR_BAK, RAGFAIR_JSON);
  const r = refreshChecksDat('configs/ragfair.json');
  console.log('→ ragfair.json restaurado do backup. checks.dat:', r.oldHash, '→', r.newHash);
}

function cmdStatus() {
  fillDynamic();
  const ragfair = readJson(RAGFAIR_JSON);
  const map = (ragfair.dynamic && ragfair.dynamic.itemPriceOverrideRouble) || {};
  const active = MATRIX.every(r => map[r.tpl] === r.override);
  console.log('Test overrides ativos?', active ? 'SIM' : 'NÃO (rode prep)');
  console.log('Backup existe?', fs.existsSync(RAGFAIR_BAK) ? 'SIM' : 'NÃO');
  console.log('');
  printTable();
}

function printTable() {
  console.log('id  item                 alvo X        override        bonus     cenário / como ler');
  console.log('──  ───────────────────  ───────────   ────────────    ────────  ─────────────────────────────────────────────');
  for (const r of MATRIX) {
    const x = r.X == null ? '(diag)' : r.X.toLocaleString('en-US');
    const lo = r.X == null ? '' : Math.round(r.X * 0.8).toLocaleString('en-US');
    const hi = r.X == null ? '' : Math.round(r.X * 1.2).toLocaleString('en-US');
    console.log(
      `${r.id}   ${r.label.padEnd(19)}  ${x.padStart(11)}   ${String(r.override).padStart(12)}    ${String(r.bonus).padStart(8)}  ${r.scenario}`
    );
    if (r.X != null) console.log(`    busca: "${r.search}"  →  esperado: ofertas entre ${lo} e ${hi} (centro ${x})`);
    else             console.log(`    busca: "${r.search}"  →  reportar centro observado; back-calc bonus = centro − ${r.override}`);
  }
}

const mode = process.argv[2];
try {
  if (mode === 'prep')        cmdPrep();
  else if (mode === 'revert') cmdRevert();
  else if (mode === 'status') cmdStatus();
  else { console.log('Uso: node smoke-matrix.js <prep|revert|status>'); process.exit(1); }
} catch (e) {
  console.error('ERRO:', e.message);
  process.exit(1);
}
