#!/usr/bin/env node
/**
 * gen-anchor-table.js (one-shot — delete after use)
 * Reads anchor-items.json and prints a compact markdown table sorted by avg24h desc.
 */
'use strict';
const fs = require('fs');
const path = require('path');

const J = JSON.parse(fs.readFileSync(
  path.join(__dirname, '..', 'mods/RZCustomProfiles/backlog/anchor-items.json'),
  'utf8'
));

const rows = Object.values(J)
  .map((r) => ({
    id:     r.id,
    name:   r.name,
    short:  r.shortName,
    tpl:    r.bsgId,
    avg:    r.avg24hPrice || 0,
    trader: r.traderPrice || 0,
  }))
  .sort((a, b) => b.avg - a.avg);

const fmt = (n) => n ? n.toLocaleString('pt-BR') : '—';

console.log('| ID | Nome | Short | Tpl | avg24h ₽ | trader ₽ |');
console.log('|----|------|-------|-----|---------:|---------:|');
for (const r of rows) {
  console.log(`| \`${r.id}\` | ${r.name} | ${r.short} | \`${r.tpl}\` | ${fmt(r.avg)} | ${fmt(r.trader)} |`);
}
