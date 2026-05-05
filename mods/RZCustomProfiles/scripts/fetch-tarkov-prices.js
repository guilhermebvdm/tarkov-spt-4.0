#!/usr/bin/env node
/**
 * fetch-tarkov-prices.js (one-shot — delete after use)
 *
 * Fetches PVE flea prices for the ~30 anchor items used in
 * mods/RZCustomProfiles/backlog/001-custom-profiles.md.
 *
 * Strategy:
 *  - Single bulk call to /pve/items/all (rate-limited 5/min).
 *  - Cache the response locally so re-runs are cheap.
 *  - Match each anchor by Tpl. The API field that holds the BSG Tpl
 *    is unknown a priori — we probe several candidates (uid, bsgId,
 *    tpl, _id, id) and use the first that matches.
 *
 * Usage:
 *   set TARKOV_MARKET_API_KEY=<key>      (Windows cmd)
 *   $env:TARKOV_MARKET_API_KEY='<key>'   (PowerShell)
 *   export TARKOV_MARKET_API_KEY=<key>   (bash)
 *   node scripts/fetch-tarkov-prices.js
 *
 * Output: markdown table on stdout, ready to paste into the backlog.
 */

'use strict';

const fs    = require('fs');
const path  = require('path');
const https = require('https');

const API_KEY  = process.env.TARKOV_MARKET_API_KEY;
const API_URL  = 'https://api.tarkov-market.app/api/v1/pve/items/all';
const CACHE    = path.join(__dirname, 'cache', 'tarkov-pve-items.json');
const CACHE_H  = 1; // hours; re-fetch if older

// ── Anchor items ────────────────────────────────────────────────────────────

const ANCHORS = [
  // Meds
  ['Weapon repair kit',          '591094e086f7747caa7bb43e'],
  ['IFAK',                       '590c661e86f7741e566b646a'],
  ['Salewa first aid kit',       '544fb45d4bdc2dee738b4568'],
  ['Surv12 field surgical kit',  '5d02778e86f774203e7dedbe'],
  ['Grizzly medical kit',        '590c657e86f77412b013051d'],
  ['CALOK-B hemostatic',         '5e8488fa4f4af2001a5b9fe0'],
  ['CMS surgical kit',           '5d02797c86f774203f38e30a'],

  // Weapons
  ['AKM',                        '59d6088586f774275f37482f'],
  ['AKMS',                       '59ff346386f77477562ff5e2'],
  ['AKS-74U',                    '57dc2fa62459775949412633'],
  ['M4A1',                       '5447a9cd4bdc2dbd208b4567'],
  ['Mosin Infantry',             '5bfd297f0db834001a669119'],
  ['SV-98',                      '55801eed4bdc2d89578b4588'],
  ['TOZ-106',                    '5a38e6bac4a2826c6e06d79b'],

  // Optics & attachments
  ['PNV-10T NVG',                '5c066ef40db834001966a595'],
  ['PSO-1 scope',                '570fd6c2d2720bc6458b457f'],
  ['PBS-1 supressor',            '564caa3d4bdc2d17108b458e'],

  // Armor
  ['6B23-1 armor (classe 3)',    '5c0e5edb86f77461f55ed1f7'],

  // Backpacks
  ['Tri-Zip backpack',           '545cdb794bdc2d3a198b456a'],
  ['Pilgrim backpack',           '59e763f286f7742ee57895da'],
  ['MBSS backpack',              '5ab8ee7786f7742d8f33f0b9'],

  // Specialty
  ['Ghillie suit',               '5fd4c4fa16cac650092f5d27'],
  ['Lockpick',                   '6391fcf5744e45201147080f'],
  ['Toolset',                    '590c2e1186f77425357b6124'],
  ['Items case (small/THICC)',   '5d235bb686f77443f4331278'],
];

// Items where we don't yet know the Tpl — script will report blank for these
// so the user can fill them by hand from db.sp-tarkov.com.
const ANCHORS_PENDING = [
  'AK-74N',
  'Saiga 12',
  '6B2 armor (classe 2)',
  'LZSh helmet',
  'MICH 2000',
];

// ── HTTP helper ─────────────────────────────────────────────────────────────

function httpGet(url, headers) {
  return new Promise((resolve, reject) => {
    https.get(url, { headers }, (res) => {
      if (res.statusCode !== 200) {
        return reject(new Error(`HTTP ${res.statusCode}: ${url}`));
      }
      const chunks = [];
      res.on('data', (c) => chunks.push(c));
      res.on('end',  () => {
        try   { resolve(JSON.parse(Buffer.concat(chunks).toString('utf8'))); }
        catch (e) { reject(e); }
      });
    }).on('error', reject);
  });
}

// ── Cache ───────────────────────────────────────────────────────────────────

function readCache() {
  if (!fs.existsSync(CACHE)) return null;
  const ageH = (Date.now() - fs.statSync(CACHE).mtimeMs) / 3.6e6;
  if (ageH > CACHE_H) return null;
  console.error(`Using cache (age: ${ageH.toFixed(2)}h)`);
  return JSON.parse(fs.readFileSync(CACHE, 'utf8'));
}

function writeCache(items) {
  fs.mkdirSync(path.dirname(CACHE), { recursive: true });
  fs.writeFileSync(CACHE, JSON.stringify(items, null, 2));
  console.error(`Cached ${items.length} items → ${CACHE}`);
}

// ── Tpl probe ───────────────────────────────────────────────────────────────

// The API field holding the BSG Tpl is not documented. Probe candidates on
// the first known-good anchor (Roubles, AKM, etc.) until we find a hit.
function findTplField(items, knownTpl) {
  const candidates = ['uid', 'bsgId', 'tpl', '_id', 'id', 'shortName'];
  for (const field of candidates) {
    const hit = items.find((it) => it[field] === knownTpl);
    if (hit) {
      console.error(`Tpl field detected: "${field}"`);
      return field;
    }
  }
  return null;
}

// ── Main ────────────────────────────────────────────────────────────────────

async function main() {
  if (!API_KEY) {
    console.error('ERROR: TARKOV_MARKET_API_KEY env var not set.');
    process.exit(1);
  }

  let items = readCache();
  if (!items) {
    console.error(`Fetching ${API_URL} ...`);
    items = await httpGet(API_URL, { 'x-api-key': API_KEY });
    if (!Array.isArray(items)) {
      console.error('Unexpected API response shape:');
      console.error(JSON.stringify(items, null, 2).slice(0, 500));
      process.exit(1);
    }
    writeCache(items);
  }

  console.error(`Loaded ${items.length} items from PVE flea.`);

  // Probe field name once (use AKM Tpl as known reference).
  const tplField = findTplField(items, '59d6088586f774275f37482f');
  if (!tplField) {
    console.error('Could not detect Tpl field. Inspecting first item:');
    console.error(JSON.stringify(items[0], null, 2));
    process.exit(1);
  }

  const byTpl = new Map();
  for (const it of items) byTpl.set(it[tplField], it);

  // ── Render markdown table ────────────────────────────────────────────────

  const rows = [];
  rows.push('| Item | Tpl | avg24h (₽) | trader (₽) | Δ24h |');
  rows.push('|------|-----|-----------:|-----------:|-----:|');

  for (const [name, tpl] of ANCHORS) {
    const it = byTpl.get(tpl);
    if (!it) {
      rows.push(`| ${name} | \`${tpl}\` | **NOT FOUND** | — | — |`);
      continue;
    }
    const avg     = (it.avg24hPrice ?? 0).toLocaleString('pt-BR');
    const trader  = (it.traderPrice ?? 0).toLocaleString('pt-BR');
    const diff    = (it.diff24h ?? 0).toFixed(1);
    rows.push(`| ${it.name || name} | \`${tpl}\` | ${avg} | ${trader} | ${diff}% |`);
  }

  for (const name of ANCHORS_PENDING) {
    rows.push(`| ${name} | *(pendente)* | ? | ? | ? |`);
  }

  rows.push('');
  rows.push('> Preços via [tarkov-market.com](https://tarkov-market.com) (PVE, avg 24h). Snapshot único, atualizar conforme necessário.');

  console.log(rows.join('\n'));
}

main().catch((e) => {
  console.error('Fatal:', e.message);
  process.exit(1);
});
