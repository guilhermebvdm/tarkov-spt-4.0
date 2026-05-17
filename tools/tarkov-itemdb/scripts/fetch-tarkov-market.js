#!/usr/bin/env node
/**
 * fetch-tarkov-market.js — pulls full PVE item dump from tarkov-market.com.
 *
 * Requires TARKOV_MARKET_API_KEY env var. Rate limit: 5 req/min, so cache 6h.
 *
 * Usage:  node scripts/fetch-tarkov-market.js [--force]
 * Output: cache/tarkov-market-raw.json
 */
'use strict';

const fs    = require('fs');
const path  = require('path');
const https = require('https');

const API_KEY = process.env.TARKOV_MARKET_API_KEY;
const API_URL = 'https://api.tarkov-market.app/api/v1/pve/items/all';
const CACHE   = path.join(__dirname, '..', 'cache', 'tarkov-market-raw.json');
const CACHE_H = 6;
const FORCE   = process.argv.includes('--force');

function get(url, headers) {
  return new Promise((resolve, reject) => {
    https.get(url, { headers }, (res) => {
      if (res.statusCode !== 200) return reject(new Error(`HTTP ${res.statusCode}: ${url}`));
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end',  () => {
        try { resolve(JSON.parse(Buffer.concat(chunks).toString('utf8'))); }
        catch (e) { reject(e); }
      });
    }).on('error', reject);
  });
}

function cacheFresh() {
  if (FORCE || !fs.existsSync(CACHE)) return false;
  const ageH = (Date.now() - fs.statSync(CACHE).mtimeMs) / 3.6e6;
  return ageH < CACHE_H;
}

async function main() {
  if (cacheFresh()) {
    const ageH = (Date.now() - fs.statSync(CACHE).mtimeMs) / 3.6e6;
    console.error(`Cache fresh (${ageH.toFixed(2)}h) — skipping fetch. Use --force to re-fetch.`);
    return;
  }
  if (!API_KEY) {
    console.error('ERROR: TARKOV_MARKET_API_KEY env var not set.');
    process.exit(1);
  }
  console.error(`GET ${API_URL} ...`);
  const t0 = Date.now();
  const items = await get(API_URL, { 'x-api-key': API_KEY });
  if (!Array.isArray(items)) {
    console.error('Unexpected response shape:');
    console.error(JSON.stringify(items, null, 2).slice(0, 500));
    process.exit(1);
  }
  console.error(`Fetched in ${((Date.now()-t0)/1000).toFixed(1)}s — items: ${items.length}`);
  fs.mkdirSync(path.dirname(CACHE), { recursive: true });
  fs.writeFileSync(CACHE, JSON.stringify(items, null, 2));
  const sizeMB = (fs.statSync(CACHE).size / 1048576).toFixed(2);
  console.error(`Wrote ${CACHE} (${sizeMB} MB)`);
}

main().catch(e => { console.error('Fatal:', e.message); process.exit(1); });
