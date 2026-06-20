#!/usr/bin/env node
/**
 * extract-item-data.js
 *
 * Lê dados do trl-items-management (que tem partes gitignored em cache/) e emite
 * scripts/item-data.json (versionado) com apenas o subset que build-profile-jsons
 * precisa: stackMax + width + height + name por TPL referenciado nas recipes.
 *
 * Roda quando o trl-items-management é regenerado (raro — só quando EFT atualiza).
 *
 * Fonte: tools/trl-items-management/cache/spt-raw.json (stackMaxSize, dims) +
 *        tools/trl-items-management/data/items.json (fallback/dims se necessário).
 *
 * Saída: scripts/item-data.json (~5-10 KB, só os TPLs usados em recipes).
 */

'use strict';
const fs   = require('fs');
const path = require('path');

const MOD_ROOT  = path.resolve(__dirname, '..');
const REPO_ROOT = path.resolve(MOD_ROOT, '../..');

const SPT_RAW_PATH = path.join(REPO_ROOT, 'tools/trl-items-management/cache/spt-raw.json');
const ANCHOR_PATH  = path.join(MOD_ROOT, 'backlog/anchor-items.json');

if (!fs.existsSync(SPT_RAW_PATH)) {
  console.error(`ERRO: ${SPT_RAW_PATH} não existe. Rode tools/trl-items-management/scripts/load-spt.js antes.`);
  process.exit(1);
}

const SPT_RAW = JSON.parse(fs.readFileSync(SPT_RAW_PATH, 'utf8'));
const ANCHOR  = JSON.parse(fs.readFileSync(ANCHOR_PATH, 'utf8'));

// Coletar TPLs únicos de todos os IDs simbólicos do anchor
const tpls = new Set();
for (const [id, r] of Object.entries(ANCHOR)) {
  if (r && r.bsgId) tpls.add(r.bsgId);
}

// Extrair subset
const out = {};
const missing = [];
for (const tpl of tpls) {
  const it = SPT_RAW.items && SPT_RAW.items[tpl];
  if (!it) {
    missing.push(tpl);
    continue;
  }
  out[tpl] = {
    name: it.name || '',
    stackMax: typeof it.stackMaxSize === 'number' ? it.stackMaxSize : 1,
    width: typeof it.width === 'number' ? it.width : 1,
    height: typeof it.height === 'number' ? it.height : 1,
  };
}

const OUTPUT = path.join(__dirname, 'item-data.json');
fs.writeFileSync(OUTPUT, JSON.stringify(out, null, 2) + '\n', { encoding: 'utf8' });

console.log(`✓ ${OUTPUT}`);
console.log(`  TPLs extraídos: ${Object.keys(out).length}`);
if (missing.length > 0) {
  console.warn(`  ⚠️  TPLs não encontrados no spt-raw (${missing.length}):`, missing.slice(0, 5).join(', '));
}
