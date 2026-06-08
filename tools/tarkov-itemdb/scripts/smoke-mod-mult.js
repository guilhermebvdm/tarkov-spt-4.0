#!/usr/bin/env node
/**
 * smoke-mod-mult.js — validate the `itemPriceMultiplier` lever for MOD items.
 *
 * The additive `itemPriceOverrideRouble` lever is IGNORED for mod items: a mod's
 * CustomItemService re-writes Prices[tpl] = fleaPriceRoubles AFTER ApplyFleaPriceOverrides,
 * wiping the override. Proven in-game (Thermaster/Citadel/Fanny all showed base, not target).
 *
 * `itemPriceMultiplier`, by contrast, is applied at OFFER-GENERATION time in
 * RagfairPriceService.GetDynamicOfferPriceForOffer (line ~341):
 *     price = GetFleaPriceForItem(tpl)      // = fleaPriceRoubles + handbook×M  (mod base)
 *     price *= itemPriceMultiplier[tpl]     // ← THE LEVER, applied AFTER the mod set the price
 *     price *= qualityModifier              // 1.0 at full quality
 *     price = AdjustUnreasonablePrice(...)  // ceiling (none for these items)
 *     price = RandomiseOfferPrice(0.8..1.2) // variance
 * So it SHOULD work for mod items. This test confirms it in-game.
 *
 * Design: 3 mod items, 3 DISTINCT multipliers (×2, ×3, ×4). If the lever works,
 * each lands on base×factor (±20% variance), and the ratios cross-validate
 * proportionality. If it does NOT work, all three stay at their base price.
 *
 * Modes: prep | revert | status
 * State: backs up ragfair.json once → ragfair.json.pre-modmult-backup. prep merges
 * the 3 test entries onto the EXISTING itemPriceMultiplier map (preserves the 2
 * vanilla entries). revert restores the backup. Idempotent.
 *
 * After prep: RESTART the SPT server so offers regenerate with the new prices.
 */
'use strict';

const fs     = require('fs');
const path   = require('path');
const crypto = require('crypto');

const SPT_ROOT = process.env.SPT_PATH || 'D:/SPT/SPT';
const SPT_DATA = fs.existsSync(path.join(SPT_ROOT, 'SPT_Data'))
  ? path.join(SPT_ROOT, 'SPT_Data') : SPT_ROOT;

const RAGFAIR_JSON = path.join(SPT_DATA, 'configs', 'ragfair.json');
const RAGFAIR_BAK  = path.join(SPT_DATA, 'configs', 'ragfair.json.pre-modmult-backup');
const CHECKS_DAT   = path.join(SPT_DATA, 'checks.dat');

// ── Test matrix ──────────────────────────────────────────────────────────────
// base = in-memory flea base for the mod item (fleaPriceRoubles + handbook×M),
// taken from the viewer's computed effectiveFleaPrice (ceiling=null for all → no clamp).
const MATRIX = [
  { tpl: '669c1a420c8342338269dd86', label: 'Vintage Thermaster Cooler', base: 239839, mult: 2.0, search: 'Thermaster',      mod: 'WTT-PackNStrap' },
  { tpl: '669c10fa06c00c483c58537a', label: 'Small Cash Box',            base: 134145, mult: 3.0, search: 'Small Cash Box',  mod: 'WTT-PackNStrap' },
  { tpl: '69f95efeb93f3b8fa8d3879f', label: 'Hacker Device',             base: 125000, mult: 4.0, search: 'Hacker Device',   mod: 'HackerServer'   },
];

// ── helpers ───────────────────────────────────────────────────────────────────
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

// ── commands ────────────────────────────────────────────────────────────────
function cmdPrep() {
  if (!fs.existsSync(RAGFAIR_BAK)) { fs.copyFileSync(RAGFAIR_JSON, RAGFAIR_BAK); console.log('→ Backup criado:', RAGFAIR_BAK); }

  const ragfair = readJson(RAGFAIR_JSON);
  const existing = (ragfair.dynamic && ragfair.dynamic.itemPriceMultiplier) || {};
  const map = { ...existing };
  for (const row of MATRIX) map[row.tpl] = row.mult;
  ragfair.dynamic.itemPriceMultiplier = map;
  writeJsonPreservingStyle(RAGFAIR_JSON, ragfair);

  const r = refreshChecksDat('configs/ragfair.json');
  console.log('→ ragfair.json escrito (' + Object.keys(existing).length + ' vanilla + ' + MATRIX.length + ' teste). checks.dat:', r.oldHash, '→', r.newHash, '(changed=' + r.changed + ')');
  console.log('→ REINICIE o servidor SPT pra regerar as ofertas.');
  console.log('');
  printTable();
}

function cmdRevert() {
  if (!fs.existsSync(RAGFAIR_BAK)) throw new Error('backup missing: ' + RAGFAIR_BAK);
  fs.copyFileSync(RAGFAIR_BAK, RAGFAIR_JSON);
  const r = refreshChecksDat('configs/ragfair.json');
  console.log('→ ragfair.json restaurado do backup. checks.dat:', r.oldHash, '→', r.newHash);
  console.log('→ REINICIE o servidor SPT pra voltar aos preços base.');
}

function cmdStatus() {
  const ragfair = readJson(RAGFAIR_JSON);
  const map = (ragfair.dynamic && ragfair.dynamic.itemPriceMultiplier) || {};
  const active = MATRIX.every(r => map[r.tpl] === r.mult);
  console.log('Test multipliers ativos?', active ? 'SIM' : 'NÃO (rode prep)');
  console.log('Backup existe?', fs.existsSync(RAGFAIR_BAK) ? 'SIM' : 'NÃO');
  console.log('Multiplier map atual:', JSON.stringify(map));
  console.log('');
  printTable();
}

function printTable() {
  console.log('item                         mod              base         ×mult   alvo (centro)   faixa esperada (0.8–1.2)        busca');
  console.log('───────────────────────────  ───────────────  ───────────  ─────   ─────────────   ──────────────────────────────  ──────────────────');
  for (const r of MATRIX) {
    const target = Math.round(r.base * r.mult);
    const lo = Math.round(target * 0.8), hi = Math.round(target * 1.2);
    console.log(
      `${r.label.padEnd(27)}  ${r.mod.padEnd(15)}  ${r.base.toLocaleString('en-US').padStart(11)}  ×${String(r.mult).padEnd(4)}  ${target.toLocaleString('en-US').padStart(13)}   ${(lo.toLocaleString('en-US')+' – '+hi.toLocaleString('en-US')).padEnd(30)}  "${r.search}"`
    );
  }
  console.log('');
  console.log('LER: se o lever FUNCIONA → cada item cai na "faixa esperada" (base×mult ±20%).');
  console.log('     se NÃO funciona     → todos ficam na base ±20% (Thermaster ~192k–288k, Cash Box ~107k–161k, Hacker ~100k–150k).');
  console.log('     as faixas não se sobrepõem, então a leitura é inequívoca.');
}

const mode = process.argv[2];
try {
  if (mode === 'prep')        cmdPrep();
  else if (mode === 'revert') cmdRevert();
  else if (mode === 'status') cmdStatus();
  else { console.log('Uso: node smoke-mod-mult.js <prep|revert|status>'); process.exit(1); }
} catch (e) {
  console.error('ERRO:', e.message);
  process.exit(1);
}
