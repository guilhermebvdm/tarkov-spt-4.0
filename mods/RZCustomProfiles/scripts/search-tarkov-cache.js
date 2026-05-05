#!/usr/bin/env node
/**
 * search-tarkov-cache.js (one-shot — delete after use)
 *
 * Searches the local PVE items cache by name fragment.
 * No API call — pure local lookup.
 *
 * Usage:
 *   node scripts/search-tarkov-cache.js "IFAK"
 *   node scripts/search-tarkov-cache.js "PNV-10T"
 *   node scripts/search-tarkov-cache.js "Tri-Zip"
 */

'use strict';

const fs   = require('fs');
const path = require('path');

const CACHE = path.join(__dirname, 'cache', 'tarkov-pve-items.json');
const QUERY = (process.argv[2] || '').toLowerCase();

if (!QUERY) {
  console.error('Usage: node scripts/search-tarkov-cache.js "<name fragment>"');
  process.exit(1);
}

if (!fs.existsSync(CACHE)) {
  console.error(`Cache not found: ${CACHE}\nRun fetch-tarkov-prices.js first.`);
  process.exit(1);
}

const items = JSON.parse(fs.readFileSync(CACHE, 'utf8'));

const matches = items.filter((it) => {
  const n = (it.name      || '').toLowerCase();
  const s = (it.shortName || '').toLowerCase();
  return n.includes(QUERY) || s.includes(QUERY);
});

if (matches.length === 0) {
  console.log(`No matches for "${QUERY}".`);
  process.exit(0);
}

console.log(`${matches.length} matches for "${QUERY}":\n`);
console.log('| Name | Short | Tpl | avg24h | trader |');
console.log('|------|-------|-----|-------:|-------:|');
for (const it of matches) {
  const avg    = (it.avg24hPrice ?? 0).toLocaleString('pt-BR');
  const trader = (it.traderPrice ?? 0).toLocaleString('pt-BR');
  console.log(`| ${it.name} | ${it.shortName || ''} | \`${it.bsgId}\` | ${avg} | ${trader} |`);
}
