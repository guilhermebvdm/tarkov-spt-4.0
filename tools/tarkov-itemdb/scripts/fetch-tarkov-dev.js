#!/usr/bin/env node
/**
 * fetch-tarkov-dev.js — pulls every item from tarkov.dev for PVE + regular
 * and caches the raw GraphQL response.
 *
 * No API key needed. Single GraphQL POST. Re-runs use cache if < CACHE_H hours.
 *
 * Usage:  node scripts/fetch-tarkov-dev.js [--force]
 * Output: cache/tarkov-dev-raw.json
 */
'use strict';

const fs    = require('fs');
const path  = require('path');
const https = require('https');

const ENDPOINT = 'https://api.tarkov.dev/graphql';
const CACHE    = path.join(__dirname, '..', 'cache', 'tarkov-dev-raw.json');
const CACHE_H  = 6;
const FORCE    = process.argv.includes('--force');

// Mode-invariant fields (name, dims, images, category, types) are fetched from
// PVE only. Mode-variant fields (prices, buyFor, sellFor) are fetched from both.
// Explicit limit forces complete dump (default may paginate).
const QUERY = `
query Dump {
  pve: items(gameMode: pve, limit: 100000) {
    id name shortName normalizedName description wikiLink basePrice
    weight width height
    iconLink gridImageLink image512pxLink
    types
    category   { id name normalizedName parent { id name } }
    categories { id name normalizedName }
    avg24hPrice lastLowPrice low24hPrice high24hPrice changeLast48hPercent updated
    buyFor  { vendor { name normalizedName } price priceRUB currency }
    sellFor { vendor { name normalizedName } price priceRUB currency }
  }
  regular: items(gameMode: regular, limit: 100000) {
    id
    avg24hPrice lastLowPrice low24hPrice high24hPrice changeLast48hPercent updated
    buyFor  { vendor { name normalizedName } price priceRUB currency }
    sellFor { vendor { name normalizedName } price priceRUB currency }
  }
  categories: itemCategories {
    id name normalizedName parent { id }
  }
}`;

function post(url, body) {
  return new Promise((resolve, reject) => {
    const data = Buffer.from(JSON.stringify(body));
    const u = new URL(url);
    const req = https.request({
      method: 'POST', hostname: u.hostname, path: u.pathname,
      headers: { 'Content-Type': 'application/json', 'Content-Length': data.length }
    }, (res) => {
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end', () => {
        if (res.statusCode !== 200) return reject(new Error(`HTTP ${res.statusCode}`));
        try { resolve(JSON.parse(Buffer.concat(chunks).toString('utf8'))); }
        catch (e) { reject(e); }
      });
    });
    req.on('error', reject);
    req.write(data);
    req.end();
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
  console.error(`POST ${ENDPOINT} ...`);
  const t0 = Date.now();
  const resp = await post(ENDPOINT, { query: QUERY });
  if (resp.errors) {
    console.error('GraphQL errors:', JSON.stringify(resp.errors, null, 2));
    process.exit(1);
  }
  const { pve = [], regular = [], categories = [] } = resp.data || {};
  console.error(`Fetched in ${((Date.now()-t0)/1000).toFixed(1)}s — pve: ${pve.length}, regular: ${regular.length}, categories: ${categories.length}`);
  fs.mkdirSync(path.dirname(CACHE), { recursive: true });
  fs.writeFileSync(CACHE, JSON.stringify(resp.data, null, 2));
  const sizeMB = (fs.statSync(CACHE).size / 1048576).toFixed(2);
  console.error(`Wrote ${CACHE} (${sizeMB} MB)`);
}

main().catch(e => { console.error('Fatal:', e.message); process.exit(1); });
